using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDataDto> GetDashboardDataAsync(string userId)
    {
        // Get the current active budget period (most recent one that includes today)
        DateTime currentDate = DateTime.Today;
        BudgetPeriod? budgetPeriod = await _context.BudgetPeriods
            .Where(bp => bp.UserId == userId &&
                         bp.StartDate <= currentDate &&
                         bp.EndDate >= currentDate)
            .OrderByDescending(bp => bp.EndDate)
            .FirstOrDefaultAsync();

        if (budgetPeriod == null)
        {
            // If no current period, get the most recent one (either future or past)
            budgetPeriod = await _context.BudgetPeriods
                .Where(bp => bp.UserId == userId)
                .OrderByDescending(bp => bp.EndDate)
                .FirstOrDefaultAsync();

            if (budgetPeriod == null)
            {
                return new DashboardDataDto
                {
                    Message = "No budget periods found for this user"
                };
            }
        }

        // Get all budgets for this period
        List<Budget> budgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.BudgetPeriodId == budgetPeriod.Id)
            .ToListAsync();
        List<BudgetDto> budgetDtos = budgets.Select(b => new BudgetDto
        {
            Id = b.Id,
            LimitValue = b.LimitValue,
            AlertValue = b.AlertValue,
            BudgetPeriodId = b.BudgetPeriodId,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? string.Empty
        }).ToList();

        // Get all expenses for this user within the period date range
        List<Expense> expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date >= budgetPeriod.StartDate &&
                        e.Date <= budgetPeriod.EndDate)
            .ToListAsync();

        // Calculate totals
        float totalBudget = budgetPeriod.Income;
        float totalExpenses = expenses.Sum(e => e.Amount);
        float budgetLeft = totalBudget - totalExpenses;

        // Group expenses by category
        List<CategoryExpenseDto> expensesByCategory = expenses
            .GroupBy(e => new { e.CategoryId, e.Category.Name })
            .Select(g => new CategoryExpenseDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalAmount = g.Sum(e => e.Amount),
                Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount) / totalExpenses * 100) : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        // Get first 5 expenses (most recent)
        List<ExpenseDto> recentExpenses = expenses
            .OrderByDescending(e => e.Date)
            .Take(10)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Name = e.Name,
                Date = e.Date,
                Amount = e.Amount,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name
            })
            .ToList();

        return new DashboardDataDto
        {
            TotalBudget = totalBudget,
            TotalExpenses = totalExpenses,
            BudgetLeft = budgetLeft,
            ExpensesByCategory = expensesByCategory,
            RecentExpenses = recentExpenses,
            BudgetPeriod = new BudgetPeriodDto
            {
                Id = budgetPeriod.Id,
                Period = budgetPeriod.Period,
                Income = budgetPeriod.Income,
                Savings = budgetPeriod.Savings,
                StartDate = budgetPeriod.StartDate,
                EndDate = budgetPeriod.EndDate,
                Budgets = budgetDtos,
            },
            Message = budgetPeriod.EndDate < currentDate ?
                "Showing data for previous budget period" :
                budgetPeriod.StartDate > currentDate ?
                "Showing data for upcoming budget period" :
                "Showing data for current budget period"
        };
    }
}