using CsvHelper;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Mappings;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class CsvImportService : ICsvImportService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly int _maxDegreeOfParallelism;

    public CsvImportService(HttpClient httpClient, AppDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
        // Set the degree of parallelism based on available processors
        // Usually a good practice to leave some cores available for other processes
        _maxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1);
    }

    public async Task<List<ExpenseDto>> ImportExpensesFromCsvAsync(string filePath)
    {
        // First load all categories from database to memory (this remains sequential)
        var categories = await _context.Categories.ToListAsync();

        // Use a thread-safe collection for parallel processing
        var concurrentExpenses = new ConcurrentBag<ExpenseDto>();

        // Read all records from the CSV file first
        List<ExpenseDto> rawRecords;
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.Configuration.HeaderValidated = null;
            csv.Context.Configuration.MissingFieldFound = null;
            csv.Context.RegisterClassMap<ExpenseDtoMap>();
            rawRecords = csv.GetRecords<ExpenseDto>().ToList();
        }

        // Process records in parallel
        await Parallel.ForEachAsync(
            rawRecords,
            new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
            async (record, cancellationToken) =>
            {
                // Validate and normalize the record
                if (record.Amount < 0)
                {
                    record.Amount = 0;
                }
                record.Name ??= "Missing Product Name";
                record.Date = record.Date == default ? DateTime.Now : record.Date;
                record.Amount = record.Amount > 0 ? record.Amount : 0;

                // Handle category
                if (string.IsNullOrWhiteSpace(record.CategoryName))
                {
                    record.CategoryName = "Others";
                }

                // If category is "Others", try to predict
                if (record.CategoryName.Equals("Others", StringComparison.OrdinalIgnoreCase))
                {
                    record.CategoryName = await PredictCategoryAsync(record.Name);
                }

                // Find category in memory (case insensitive)
                // Using local search to avoid thread safety issues with the DbContext
                var category = categories.FirstOrDefault(c =>
                    string.Equals(c.Name, record.CategoryName, StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    // If category doesn't exist, default to "Others"
                    category = categories.FirstOrDefault(c =>
                        string.Equals(c.Name, "Others", StringComparison.OrdinalIgnoreCase));
                }

                record.CategoryId = category.Id;
                concurrentExpenses.Add(record);
            });

        // Convert back to a regular list and return
        return concurrentExpenses.ToList();
    }

    private async Task<string> PredictCategoryAsync(string name)
    {
        try
        {
            var requestBody = new { product = name };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://localhost:5000/predict", content);
            if (!response.IsSuccessStatusCode)
            {
                return "Others";
            }
            var result = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            if (jsonResponse != null && jsonResponse.TryGetValue("predicted_category", out var categoryString))
            {
                // Return the predicted category as-is (will be matched case-insensitively later)
                return categoryString;
            }
        }
        catch { }
        return "Others";
    }
}