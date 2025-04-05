using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly AppDbContext _context;

        public BudgetService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync()
        {
            return await _context.Budgets
                .Include(b => b.Category) 
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,  
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                })
                .ToListAsync();
        }

        public async Task<BudgetDto> GetBudgetAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)  
                .FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
            {
                return null;
            }

            return new BudgetDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId, 
                LimitValue = budget.LimitValue,
                AlertValue = budget.AlertValue,
                BudgetPeriodId = budget.BudgetPeriodId
            };
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsByUserIdAsync(string userId)
        {
            return await _context.Budgets
                .Include(b => b.BudgetPeriod)
                .Include(b => b.Category)  
                .Where(b => b.BudgetPeriod.UserId == userId)
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,  
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    BudgetPeriodId = b.BudgetPeriodId
                })
                .ToListAsync();
        }

        public async Task<bool> PutBudgetAsync(int id, BudgetDto budgetDto)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return false;
            }

            budget.CategoryId = budgetDto.CategoryId;  
            budget.LimitValue = budgetDto.LimitValue;
            budget.AlertValue = budgetDto.AlertValue;
            budget.BudgetPeriodId = budgetDto.BudgetPeriodId;

            _context.Entry(budget).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BudgetExists(id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<(bool, BudgetDto)> PostBudgetAsync(BudgetDto budgetDto)
        {
            if (budgetDto == null)
            {
                return (false, null);
            }

            
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == budgetDto.CategoryId);
            if (!categoryExists)
            {
                return (false, null);
            }

            var budget = new Budget
            {
                CategoryId = budgetDto.CategoryId,  
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