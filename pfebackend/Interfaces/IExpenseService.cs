using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IExpenseService
    {   
        Task<IEnumerable<ExpenseDto>> GetExpensesAsync();
        Task<ExpenseDto?> GetExpenseByIdAsync(int id);
        Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(string userId);
        Task<bool> UpdateExpenseAsync(int id, ExpenseDto expenseDto);
        Task<ExpenseDto> CreateExpenseAsync(ExpenseDto expenseDto);
        Task<bool> DeleteExpenseAsync(int id);
        Task<bool> UserExists(string userId);
    }
}
