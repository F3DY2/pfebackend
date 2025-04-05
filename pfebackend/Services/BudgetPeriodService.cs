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

        public BudgetPeriodService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BudgetPeriodDto>> GetBudgetPeriodsAsync()
        {
            return await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                    .ThenInclude(b => b.Category)
                .Select(bp => new BudgetPeriodDto
                {
                    Id = bp.Id,
                    Period = bp.Period,
                    Income = bp.Income,
                    Savings = bp.Savings,
                    StartDate = bp.StartDate,
                    EndDate = bp.EndDate,
                    UserId = bp.UserId,
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

        public async Task<BudgetPeriodDto> GetBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                    .ThenInclude(b => b.Category)
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
                .Select(bp => new BudgetPeriodDto
                {
                    Id = bp.Id,
                    Period = bp.Period,
                    Income = bp.Income,
                    Savings = bp.Savings,
                    StartDate = bp.StartDate,
                    EndDate = bp.EndDate,
                    UserId = bp.UserId,
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
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (budgetPeriod == null)
                return (false, null);

            // Update main properties
            budgetPeriod.Period = budgetPeriodDto.Period;
            budgetPeriod.Income = budgetPeriodDto.Income;
            budgetPeriod.Savings = budgetPeriodDto.Savings;
            budgetPeriod.StartDate = budgetPeriodDto.StartDate;
            budgetPeriod.EndDate = budgetPeriodDto.EndDate;
            budgetPeriod.UserId = budgetPeriodDto.UserId;

            // Update budgets if provided
            if (budgetPeriodDto.Budgets != null)
            {
                foreach (var budgetDto in budgetPeriodDto.Budgets)
                {
                    var existingBudget = budgetPeriod.Budgets
                        .FirstOrDefault(b => b.Id == budgetDto.Id);

                    if (existingBudget != null)
                    {
                        existingBudget.CategoryId = budgetDto.CategoryId;  // Changed from Category to CategoryId
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
                return (true, budgetPeriodDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetPeriodExists(id))
                    return (false, null);
                throw;
            }
        }

        public async Task<(bool, BudgetPeriodDto)> PostBudgetPeriodAsync(BudgetPeriodDto budgetPeriodDto)
        {
            if (budgetPeriodDto == null)
                return (false, null);

            // Create main budget period
            var budgetPeriod = new BudgetPeriod
            {
                Period = budgetPeriodDto.Period,
                Income = budgetPeriodDto.Income,
                Savings = budgetPeriodDto.Savings,
                StartDate = budgetPeriodDto.StartDate,
                EndDate = budgetPeriodDto.EndDate,
                UserId = budgetPeriodDto.UserId
            };

            _context.BudgetPeriods.Add(budgetPeriod);
            await _context.SaveChangesAsync();

            // Create budgets if provided
            if (budgetPeriodDto.Budgets != null && budgetPeriodDto.Budgets.Any())
            {
                var budgets = budgetPeriodDto.Budgets.Select(b => new Budget
                {
                    CategoryId = b.CategoryId,  // Changed from Category to CategoryId
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = budgetPeriod.Id
                }).ToList();

                _context.Budgets.AddRange(budgets);
                await _context.SaveChangesAsync();

                budgetPeriodDto.Budgets = budgets.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,  // Changed from Category to CategoryId
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                }).ToList();
            }

            budgetPeriodDto.Id = budgetPeriod.Id;
            return (true, budgetPeriodDto);
        }

        public async Task<bool> DeleteBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.Budgets)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (budgetPeriod == null)
                return false;

            // Remove associated budgets first
            if (budgetPeriod.Budgets.Any())
            {
                _context.Budgets.RemoveRange(budgetPeriod.Budgets);
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