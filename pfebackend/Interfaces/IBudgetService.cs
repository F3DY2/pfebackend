using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetBudgetsAsync();
        Task<BudgetDto> GetBudgetAsync(int id);
        Task<IEnumerable<BudgetDto>> GetBudgetsByUserIdAsync(string userId);
        Task<bool> PutBudgetAsync(int id, BudgetDto budgetDto);
        Task<(bool, BudgetDto)> PostBudgetAsync(BudgetDto budgetDto);
        Task<bool> DeleteBudgetAsync(int id);
    }
}
