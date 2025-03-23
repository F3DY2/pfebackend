using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDto>> GetAllBudget();
        Task<BudgetDto> GetBudgetById(int id);
        Task<IEnumerable<BudgetDto>> GetUserBudgetsByUserId(string userId);
        Task<bool> UpdateBudget(int id, BudgetDto budgetDto);
        Task<BudgetDto> CreateBudget(BudgetDto budgetDto);
        Task<bool> RemoveBudget(int id);
        bool BudgetExists(int id);
    }
}
