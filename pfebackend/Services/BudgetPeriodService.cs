using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class BudgetPeriodService : IBudgetPeriodService
    {
        private readonly AppDbContext _context;
        private readonly IPredictedMonthlyExpenseService _expenseService;
        public BudgetPeriodService(AppDbContext context,
        IPredictedMonthlyExpenseService expenseService)
        {
            _context = context;
            _expenseService = expenseService;
        }

        public async Task<List<BudgetPeriodDto>> GetBudgetPeriodsAsync()
        {
            return await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                    .ThenInclude(b => b.Category)
                .Include(bp => bp.PredictedExpense)
                .Select(bp => new BudgetPeriodDto
                {
                    Id = bp.Id,
                    Period = bp.Period,
                    Income = bp.Income,
                    Savings = bp.Savings,
                    StartDate = bp.StartDate,
                    EndDate = bp.EndDate,
                    UserId = bp.UserId,
                    PredictedExpense = bp.PredictedExpense.PredictedExpense,
                    Budgets = bp.Budgets.Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        CategoryId = b.CategoryId,  
                        CategoryName = b.Category.Name,
                        LimitValue = b.LimitValue,
                        AlertValue = b.AlertValue,
                        BudgetPeriodId = b.BudgetPeriodId
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<BudgetPeriodDto> GetBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                    .ThenInclude(b => b.Category)
                .Include(bp => bp.PredictedExpense)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (budgetPeriod == null)
            {
                return null;
            }

            return new BudgetPeriodDto
            {
                Id = budgetPeriod.Id,
                Period = budgetPeriod.Period,
                Income = budgetPeriod.Income,
                Savings = budgetPeriod.Savings,
                StartDate = budgetPeriod.StartDate,
                EndDate = budgetPeriod.EndDate,
                UserId = budgetPeriod.UserId,
                PredictedExpense = budgetPeriod.PredictedExpense?.PredictedExpense,
                Budgets = budgetPeriod.Budgets?.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,  // Changed from Category to CategoryId
                    CategoryName = b.Category.Name,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                }).ToList()
            };
        }

        public async Task<IEnumerable<BudgetPeriodDto>> GetBudgetPeriodsByUserIdAsync(string userId)
        {
            return await _context.BudgetPeriods
                .Where(bp => bp.UserId == userId)
                .Include(bp => bp.Budgets)
                    .ThenInclude(b => b.Category)
                .Include(bp => bp.PredictedExpense)
                .Select(bp => new BudgetPeriodDto
                {
                    Id = bp.Id,
                    Period = bp.Period,
                    Income = bp.Income,
                    Savings = bp.Savings,
                    StartDate = bp.StartDate,
                    EndDate = bp.EndDate,
                    UserId = bp.UserId,
                    PredictedExpense = bp.PredictedExpense.PredictedExpense,
                    Budgets = bp.Budgets.Select(b => new BudgetDto
                    {
                        Id = b.Id,
                        CategoryId = b.CategoryId,  // Changed from Category to CategoryId
                        CategoryName = b.Category.Name,
                        LimitValue = b.LimitValue,
                        AlertValue = b.AlertValue,
                        BudgetPeriodId = b.BudgetPeriodId
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<(bool, BudgetPeriodDto)> PutBudgetPeriodAsync(int id, BudgetPeriodDto budgetPeriodDto)
        {
            if (budgetPeriodDto == null || id != budgetPeriodDto.Id)
                return (false, null);

            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                .Include(bp => bp.PredictedExpense)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (budgetPeriod == null)
                return (false, null);

            // ✅ Save the original income BEFORE updating
            float originalIncome = budgetPeriod.Income;

            // ✅ Now update main properties
            budgetPeriod.Period = budgetPeriodDto.Period;
            budgetPeriod.Income = budgetPeriodDto.Income;
            budgetPeriod.Savings = budgetPeriodDto.Savings;
            budgetPeriod.StartDate = budgetPeriodDto.StartDate;
            budgetPeriod.EndDate = budgetPeriodDto.EndDate;
            budgetPeriod.UserId = budgetPeriodDto.UserId;

            // ✅ Only trigger AI prediction if the income actually changed
            if (originalIncome != budgetPeriodDto.Income)
            {
                try
                {
                    if (budgetPeriod.PredictedExpense == null)
                    {
                        // ✅ Changed this from UpdatePredictedExpense to CreatePredictedExpense
                        var newExpense = await _expenseService.UpdatePredictedExpense(
                            budgetPeriod.Id,
                            budgetPeriodDto.Income,
                            budgetPeriodDto.UserId);

                        budgetPeriod.PredictedExpense = new PredictedMonthlyExpense
                        {
                            PredictedExpense = newExpense.PredictedExpense,
                            BudgetPeriodId = budgetPeriod.Id,
                            UserId = newExpense.UserId
                        };
                    }
                    else
                    {
                        // ✅ This calls the AI to update prediction
                        var updatedExpense = await _expenseService.UpdatePredictedExpense(
                            budgetPeriod.PredictedExpense.Id,
                            budgetPeriodDto.Income,
                            budgetPeriodDto.UserId);

                        // ✅ Ensure updated value is stored locally
                        budgetPeriod.PredictedExpense.PredictedExpense = updatedExpense.PredictedExpense;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Prediction update failed: {ex.Message}");
                }
            }

            // ✅ Update budgets if provided
            if (budgetPeriodDto.Budgets != null)
            {
                foreach (var budgetDto in budgetPeriodDto.Budgets)
                {
                    var existingBudget = budgetPeriod.Budgets
                        .FirstOrDefault(b => b.Id == budgetDto.Id);

                    if (existingBudget != null)
                    {
                        existingBudget.CategoryId = budgetDto.CategoryId;
                        existingBudget.LimitValue = budgetDto.LimitValue;
                        existingBudget.AlertValue = budgetDto.AlertValue;
                    }
                    else if (budgetDto.Id == 0) // New budget
                    {
                        _context.Budgets.Add(new Budget
                        {
                            CategoryId = budgetDto.CategoryId,
                            LimitValue = budgetDto.LimitValue,
                            AlertValue = budgetDto.AlertValue,
                            BudgetPeriodId = id
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                // ✅ Return updated predicted value in the DTO
                budgetPeriodDto.PredictedExpense = budgetPeriod.PredictedExpense?.PredictedExpense;

                return (true, budgetPeriodDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetPeriodExists(id))
                    return (false, null);
                throw;
            }
        }



        public async Task<(bool, string, BudgetPeriodDto)> PostBudgetPeriodAsync(BudgetPeriodDto budgetPeriodDto)
        {
            if (budgetPeriodDto == null)
                return (false, "Budget period data cannot be null", null);

            // Validate date range (end date must be after start date)
            if (budgetPeriodDto.StartDate >= budgetPeriodDto.EndDate)
                return (false, "End date must be after start date", null);

            // Check if this is a first-time user
            bool isFirstPeriod = !await _context.BudgetPeriods
                .AnyAsync(bp => bp.UserId == budgetPeriodDto.UserId);

            if (isFirstPeriod)
            {
                // For first period, enforce max 29 days backdating
                DateTime maxAllowedStartDate = DateTime.Now.AddMonths(-1);
                if (budgetPeriodDto.StartDate < maxAllowedStartDate)
                {
                    return (false,
                           $"For your first budget period, the start date cannot be before {maxAllowedStartDate:yyyy-MM-dd}",
                           null);
                }
            }
            else
            {
                // Existing users - enforce chronological order
                var mostRecentPeriod = await _context.BudgetPeriods
                    .Where(bp => bp.UserId == budgetPeriodDto.UserId)
                    .OrderByDescending(bp => bp.StartDate)
                    .FirstOrDefaultAsync();

                if (mostRecentPeriod != null && budgetPeriodDto.StartDate <= mostRecentPeriod.EndDate)
                {
                    return (false,
                           $"New budget period must start after {mostRecentPeriod.EndDate:yyyy-MM-dd}",
                           null);
                }
            }

            // Create and save the new budget period
            var budgetPeriod = new BudgetPeriod
            {
                Period = budgetPeriodDto.Period,
                Income = budgetPeriodDto.Income,
                Savings = budgetPeriodDto.Income,
                StartDate = budgetPeriodDto.StartDate,
                EndDate = budgetPeriodDto.EndDate,
                UserId = budgetPeriodDto.UserId
            };

            _context.BudgetPeriods.Add(budgetPeriod);
            await _context.SaveChangesAsync();

            try
            {
                // Create predicted expense with the actual budgetPeriod.Id
                var predictedExpense = new PredictedMonthlyExpense
                {
                    PredictedExpense = await _expenseService.GetPredictionFromAI(
                        budgetPeriodDto.Income,
                        budgetPeriodDto.UserId),
                    BudgetPeriodId = budgetPeriod.Id, // Use the actual ID
                    UserId = budgetPeriodDto.UserId
                };

                _context.PredictedMonthlyExpenses.Add(predictedExpense);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Prediction failed: {ex.Message}");
            }
            // Create budgets if provided
            if (budgetPeriodDto.Budgets != null && budgetPeriodDto.Budgets.Any())
            {
                var budgets = budgetPeriodDto.Budgets.Select(b => new Budget
                {
                    CategoryId = b.CategoryId,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = budgetPeriod.Id
                }).ToList();

                _context.Budgets.AddRange(budgets);
                await _context.SaveChangesAsync();

                budgetPeriodDto.Budgets = budgets.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                }).ToList();
            }

            budgetPeriodDto.Id = budgetPeriod.Id;
            return (true, "Budget period created successfully", budgetPeriodDto);
        }

        public async Task<bool> DeleteBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                .Include(bp => bp.PredictedExpense)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (budgetPeriod == null)
                return false;

            // Remove associated budgets first
            if (budgetPeriod.Budgets.Any())
            {
                _context.Budgets.RemoveRange(budgetPeriod.Budgets);
            }
            // Remove predicted expense if exists
            if (budgetPeriod.PredictedExpense != null)
            {
                _context.PredictedMonthlyExpenses.Remove(budgetPeriod.PredictedExpense);
            }

            _context.BudgetPeriods.Remove(budgetPeriod);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool BudgetPeriodExists(int id)
        {
            return _context.BudgetPeriods.Any(e => e.Id == id);
        }
    }
}