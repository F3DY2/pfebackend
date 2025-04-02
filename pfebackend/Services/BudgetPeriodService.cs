using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class BudgetPeriodService: IBudgetPeriodService 
    {
        private readonly AppDbContext _context;

        public BudgetPeriodService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<BudgetPeriodDto>> GetBudgetPeriodsAsync()
        {
            return await _context.BudgetPeriods
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
                        Category = b.Category,
                        LimitValue = b.LimitValue,
                        AlertValue = b.AlertValue,
                        BudgetPeriodId = b.BudgetPeriodId
                    }).ToList()
                })
                .ToListAsync();
        }



        public async Task<BudgetPeriodDto> GetBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods.FindAsync(id);

            BudgetPeriodDto budgetPeriodDto = new BudgetPeriodDto
            {
                Id = budgetPeriod.Id,
                Period = budgetPeriod.Period,
                Income = budgetPeriod.Income,
                Savings = budgetPeriod.Savings,
                StartDate = budgetPeriod.StartDate,
                EndDate = budgetPeriod.EndDate,
                UserId = budgetPeriod.UserId
            };
            if (budgetPeriod == null)
            {
                return null;
            }

            return budgetPeriodDto;
        }


        public async Task<IEnumerable<BudgetPeriodDto>> GetBudgetPeriodsByUserIdAsync(string userId)
        {
            var budgetPeriods = await _context.BudgetPeriods
                                              .Where(bp => bp.UserId == userId)
                                              .Include(bp => bp.Budgets) // Eager loading budgets
                                              .ToListAsync();

            if (budgetPeriods == null || !budgetPeriods.Any())
            {
                return Enumerable.Empty<BudgetPeriodDto>();
            }

            return budgetPeriods.Select(bp => new BudgetPeriodDto
            {
                Id = bp.Id,
                Period = bp.Period,
                Income = bp.Income,
                Savings = bp.Savings,
                StartDate = bp.StartDate,
                EndDate = bp.EndDate,
                UserId = bp.UserId,
                Budgets = bp.Budgets?.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Category = b.Category,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                }).ToList()
            }).ToList();
        }

        public async Task<bool> PutBudgetPeriodAsync(int id, BudgetPeriodDto budgetPeriodDto)
        {
            BudgetPeriod budgetPeriods = await _context.BudgetPeriods.FindAsync(id);
            if (budgetPeriods == null)
                return false;

            budgetPeriods.Period = budgetPeriodDto.Period;
            budgetPeriods.Income = budgetPeriodDto.Income;
            budgetPeriods.Savings = budgetPeriodDto.Savings;
            budgetPeriods.StartDate = budgetPeriodDto.StartDate;
            budgetPeriods.EndDate = budgetPeriodDto.EndDate;
            budgetPeriods.UserId = budgetPeriodDto.UserId;

            _context.Entry(budgetPeriods).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                budgetPeriodDto.Id = budgetPeriods.Id;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetPeriodExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public async Task<(bool, BudgetPeriodDto)> PostBudgetPeriodAsync(BudgetPeriodDto budgetPeriodDto)
        {
            if (budgetPeriodDto == null) { return (false, null); }
            BudgetPeriod budgetPeriod = new BudgetPeriod
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
            budgetPeriodDto.Id = budgetPeriod.Id;
            if (budgetPeriodDto.Budgets != null && budgetPeriodDto.Budgets.Any())
            {
                var budgets = budgetPeriodDto.Budgets.Select(b => new Budget
                {
                    Category = b.Category,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = budgetPeriod.Id
                }).ToList();

                _context.Budgets.AddRange(budgets);
                await _context.SaveChangesAsync();

                budgetPeriodDto.Budgets = budgets.Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Category = b.Category,
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                }).ToList();
            }

            return (true, budgetPeriodDto);
        }



        public async Task<bool> DeleteBudgetPeriodAsync(int id)
        {
            var budgetPeriod = await _context.BudgetPeriods.FindAsync(id);
            if (budgetPeriod == null)
            {
                return false;
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
