using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class BudgetService: IBudgetService
    {
        private readonly AppDbContext _context;
        public BudgetService(AppDbContext context)
        {
            _context = context;
            
        }

        public  async Task<IEnumerable<BudgetDto>> GetBudgetsAsync()
        {
            return await _context.Budgets
                       .Select(bp => new BudgetDto
                       {
                           Id= bp.Id,
                           Category = bp.Category,
                           LimitValue = bp.LimitValue,
                           AlertValue = bp.AlertValue,
                           BudgetPeriodId = bp.BudgetPeriodId
                       })
                       .ToListAsync();
        }

        public async Task<BudgetDto> GetBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);


            if (budget == null)
            {
                return null;
            }
            BudgetDto budgetDto = new BudgetDto
            {
                Id = budget.Id,
                Category = budget.Category,
                LimitValue = budget.LimitValue,
                AlertValue = budget.AlertValue,
                BudgetPeriodId = budget.BudgetPeriodId
            };

            return budgetDto;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsByUserIdAsync(string userId)
        {
            var budgets = await _context.Budgets
                                        .Where(b => b.BudgetPeriod.UserId == userId)
                                        .ToListAsync();

            if (budgets == null || !budgets.Any())
            {
                return Enumerable.Empty<BudgetDto>();
            }

            return budgets.Select(b => new BudgetDto
            {
                Id = b.Id,
                Category = b.Category,
                LimitValue = b.LimitValue,
                AlertValue = b.AlertValue,
                BudgetPeriodId = b.BudgetPeriodId
            }).ToList();
        }

        public async Task<bool> PutBudgetAsync(int id, BudgetDto budgetDto)
        {
            Budget budget = await _context.Budgets.FindAsync(id);

            if (budget == null)
            {
                return false;
            }
            budget.Category = (Models.Category)budgetDto.Category;
            budget.LimitValue = budgetDto.LimitValue;
            budget.AlertValue = budgetDto.AlertValue;
            budget.BudgetPeriodId = budgetDto.BudgetPeriodId;

            _context.Entry(budget).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetExists(id))
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

        public async Task<(bool, BudgetDto)> PostBudgetAsync(BudgetDto budgetDto)
        {
            if (budgetDto == null)
            {
                return (false,null);
            }
            Budget budget = new Budget
            {
                Category = (Models.Category)budgetDto.Category,
                LimitValue = budgetDto.LimitValue,
                AlertValue = budgetDto.AlertValue,
                BudgetPeriodId = budgetDto.BudgetPeriodId
            };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            budgetDto.Id = budget.Id;

            return (true, budgetDto);
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return false;
            }

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }
    }
}
