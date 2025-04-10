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
        // Get all expenses for this user within the period date range
        List<Expense> expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date >= budgetPeriod.StartDate &&
                        e.Date <= budgetPeriod.EndDate)
            .ToListAsync();

        // Get all budgets for this period
        List<Budget> budgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.BudgetPeriodId == budgetPeriod.Id)
            .ToListAsync();

        // Create a list of BudgetDto objects with calculated spent amounts
        List<BudgetDto> budgetDtos = budgets.Select(b =>
        {
            var categoryExpenses = expenses
                .Where(e => e.CategoryId == b.CategoryId)
                .Sum(e => e.Amount);

            float percentageSpent = b.LimitValue > 0 ?
                (categoryExpenses / b.LimitValue * 100) : 0;

            return new BudgetDto
            {
                Id = b.Id,
                LimitValue = b.LimitValue,
                AlertValue = b.AlertValue,
                BudgetPeriodId = b.BudgetPeriodId,
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.Name ?? string.Empty,
                PercentageSpent = percentageSpent
            };
        }).ToList();

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

        // Get first 8 expenses (most recent)
        List<ExpenseDto> recentExpenses = expenses
            .OrderByDescending(e => e.Date)
            .Take(5)
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
        List<ExpenseSumDto> dailyExpensesSum = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId)
            .GroupBy(e => new { Date = e.Date.Date, CategoryName = e.Category.Name })
            .Select(g => new ExpenseSumDto
            {
                CategoryName = g.Key.CategoryName,
                Date = g.Key.Date,
                Amount = g.Sum(e => e.Amount),
            })
            .ToListAsync();

        List<DailyExpensesDto> expensesByDate = await _context.Expenses
            .Include(e => e.Category)
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailyExpensesDto
            {
                Date = g.Key,
                TotalExpensesForDate = g.Sum(e => e.Amount),
                Categories = g.GroupBy(e => new { e.CategoryId, e.Category.Name })
                             .Select(cg => new CategoryExpenseDto
                             {
                                 CategoryId = cg.Key.CategoryId,
                                 CategoryName = cg.Key.Name,
                                 TotalAmount = cg.Sum(e => e.Amount),
                                
                             })
                             .OrderByDescending(c => c.TotalAmount)
                             .ToList()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();
        return new DashboardDataDto
        {
            TotalBudget = totalBudget,
            TotalExpenses = totalExpenses,
            BudgetLeft = budgetLeft,
            ExpensesByDate = expensesByDate,
            ExpensesByCategory = expensesByCategory,
            RecentExpenses = recentExpenses,
            DailyExpensesSum= dailyExpensesSum,
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