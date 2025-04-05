//using System.Globalization;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using CsvHelper;
//using CsvHelper.Configuration;
//using pfebackend.DTOs;
//using pfebackend.Interfaces;
//using pfebackend.Mappings;
//using pfebackend.Models;

//namespace pfebackend.Services
//{
//    public class CsvImportService : ICsvImportService
//    {
//        private readonly HttpClient _httpClient;

//        public CsvImportService(HttpClient httpClient)
//        {
//            _httpClient = httpClient;
//        }

//        public async Task<List<ExpenseDto>> ImportExpensesFromCsvAsync(string filePath)
//        {
//            var expenses = new List<ExpenseDto>();

//            using (var reader = new StreamReader(filePath))
//            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
//            {
//                csv.Context.Configuration.HeaderValidated = null;
//                csv.Context.Configuration.MissingFieldFound = null;

//                csv.Context.RegisterClassMap<ExpenseDtoMap>();

//                var records = csv.GetRecords<ExpenseDto>();

//                foreach (var record in records)
//                {
//                    if (record.Amount < 0)
//                    {
//                        record.Amount = 0;
//                    }

//                    record.Name ??= "Missing Product Name";
//                    record.Date = record.Date == default ? DateTime.Now : record.Date;
//                    record.Amount = record.Amount > 0 ? record.Amount : 0;
//                    if (record.Category == Category.Others)
//                    {
//                        record.Category = await PredictCategoryAsync(record.Name);
//                    }

//                    expenses.Add(record);
//                }
//            }

//            return expenses;
//        }

//        private async Task<Category> PredictCategoryAsync(string name)
//        {
//            try
//            {
//                var requestBody = new { product = name };
//                var json = JsonSerializer.Serialize(requestBody);
//                var content = new StringContent(json, Encoding.UTF8, "application/json");

//                var response = await _httpClient.PostAsync("http://localhost:5000/predict", content);

//                if (!response.IsSuccessStatusCode)
//                {
//                    return Category.Others;
//                }

//                var result = await response.Content.ReadAsStringAsync();
//                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(result);

//                if (jsonResponse != null && jsonResponse.TryGetValue("predicted_category", out var categoryString))
//                {
//                    if (Enum.TryParse<Category>(categoryString, true, out var predictedCategory))
//                    {
//                        return predictedCategory;
//                    }
//                }
//            }
//            catch{}

//            return Category.Others;
//        }
//    }
//}
