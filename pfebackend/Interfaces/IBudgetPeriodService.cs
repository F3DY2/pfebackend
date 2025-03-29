using Microsoft.AspNetCore.Mvc;
using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IBudgetPeriodService
    {

        Task<List<BudgetPeriodDto>> GetBudgetPeriodsAsync();
        Task<BudgetPeriodDto> GetBudgetPeriodAsync(int id);
        Task<IEnumerable<BudgetPeriodDto>> GetBudgetPeriodsByUserIdAsync(string userId);
        Task<bool> PutBudgetPeriodAsync(int id, BudgetPeriodDto budgetPeriodDto);
        Task<(bool, BudgetPeriodDto)> PostBudgetPeriodAsync(BudgetPeriodDto budgetPeriodDto);

        Task<bool> DeleteBudgetPeriodAsync(int id);
    }
}
