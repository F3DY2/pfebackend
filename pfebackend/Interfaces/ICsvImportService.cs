using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface ICsvImportService
    {
        Task<List<ExpenseDto>> ImportExpensesFromCsvAsync(string filePath);
    }
}