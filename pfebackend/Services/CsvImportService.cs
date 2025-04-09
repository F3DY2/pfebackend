using CsvHelper;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Mappings;
using System.Globalization;
using System.Text;
using System.Text.Json;

public class CsvImportService : ICsvImportService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context; 

    public CsvImportService(HttpClient httpClient, AppDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<List<ExpenseDto>> ImportExpensesFromCsvAsync(string filePath)
    {
        var expenses = new List<ExpenseDto>();

        // First load all categories from database to memory
        var categories = await _context.Categories.ToListAsync();

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.Configuration.HeaderValidated = null;
            csv.Context.Configuration.MissingFieldFound = null;

            csv.Context.RegisterClassMap<ExpenseDtoMap>();

            var records = csv.GetRecords<ExpenseDto>();

            foreach (var record in records)
            {
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
                var category = categories.FirstOrDefault(c =>
                    string.Equals(c.Name, record.CategoryName, StringComparison.OrdinalIgnoreCase));

                if (category == null)
                {
                    // If category doesn't exist, default to "Others"
                    category = categories.FirstOrDefault(c =>
                        string.Equals(c.Name, "Others", StringComparison.OrdinalIgnoreCase));
                }

                record.CategoryId = category.Id;
                expenses.Add(record);
            }
        }

        return expenses;
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