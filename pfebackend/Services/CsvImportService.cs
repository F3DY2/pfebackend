using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Mappings;

namespace pfebackend.Services
{
    public class CsvImportService : ICsvImportService
    {
        public async Task<List<ExpenseDto>> ImportExpensesFromCsvAsync(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.Configuration.HeaderValidated = null;
                csv.Context.Configuration.MissingFieldFound = null;

                csv.Context.RegisterClassMap<ExpenseDtoMap>();

                var expenses = csv.GetRecords<ExpenseDto>().ToList();
                return expenses;
            }
        }
    }
}