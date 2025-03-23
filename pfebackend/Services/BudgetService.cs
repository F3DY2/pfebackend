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
        public async Task<IEnumerable<BudgetDto>> GetAllBudget()
        {
            return await _context.Budgets
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Category = (DTOs.Category)b.Category,
                    limitValue = b.limitValue,
                    alertValue = b.alertValue,
                    UserId = b.UserId
                })
                .ToListAsync();
        }
        public async Task<BudgetDto> GetBudgetById(int id)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
            {
                return null;
            }

            return new BudgetDto
            {
                Id = budget.Id,
                Category = (DTOs.Category)budget.Category,
                limitValue = budget.limitValue,
                alertValue = budget.alertValue,
                UserId = budget.UserId
            };
        }
        public async Task<IEnumerable<BudgetDto>> GetUserBudgetsByUserId(string userId)
        {
            var budgets = await _context.Budgets
                                        .Where(b => b.UserId == userId)
                                        .ToListAsync();

            if (budgets == null)
            {
                return null;
            }

            return budgets.Select(b => new BudgetDto
            {
                Id = b.Id,
                Category = (DTOs.Category)b.Category,
                limitValue = b.limitValue,
                alertValue = b.alertValue,
                UserId = b.UserId
            }).ToList();
        }
        public async Task<bool> UpdateBudget(int id, BudgetDto budgetDto)
        {
            if (id != budgetDto.Id)
            {
                return false;
            }

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                return false;
            }

            budget.Category = (Models.Category)budgetDto.Category;
            budget.limitValue = (float)budgetDto.limitValue;
            budget.alertValue = (float)budgetDto.alertValue;
            budget.UserId = budgetDto.UserId;

            _context.Entry(budget).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            return true;
        }
        public async Task<BudgetDto> CreateBudget(BudgetDto budgetDto)
        {
            var budget = new Budget
            {
                Category = (Models.Category)budgetDto.Category,
                limitValue = budgetDto.limitValue,
                alertValue = budgetDto.alertValue,
                UserId = budgetDto.UserId
            };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            budgetDto.Id = budget.Id;

            return budgetDto;
        }
        public async Task<bool> RemoveBudget(int id)
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
        public bool BudgetExists(int id)
        {
            return _context.Budgets.Any(e => e.Id == id);
        }

    }
}
