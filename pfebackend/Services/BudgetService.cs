using System.Linq;
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
                    LimitValue = b.LimitValue,
                    AlertValue = b.AlertValue,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
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
                LimitValue = budget.LimitValue,
                AlertValue = budget.AlertValue,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
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
                LimitValue = b.LimitValue,
                AlertValue = b.AlertValue,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
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
            budget.LimitValue = (float)budgetDto.LimitValue;
            budget.AlertValue = (float)budgetDto.AlertValue;
            budget.StartDate = budgetDto.StartDate;
            budget.EndDate = budgetDto.EndDate;
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
                LimitValue = budgetDto.LimitValue,
                AlertValue = budgetDto.AlertValue,
                StartDate = budgetDto.StartDate,
                EndDate = budgetDto.EndDate,
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
        public async Task<bool> CheckBudgetOverlap(BudgetDto budgetDto, int? excludeBudgetId = null)
        {
            bool isOverlap = await _context.Budgets
                .Where(b => b.UserId == budgetDto.UserId)
                .Where(b => b.Category == (Models.Category)budgetDto.Category)
                .Where(b => !excludeBudgetId.HasValue || b.Id != excludeBudgetId.Value) 
                .AnyAsync(b => (b.StartDate <= budgetDto.EndDate && b.EndDate >= budgetDto.StartDate) ||
                               (b.StartDate >= budgetDto.StartDate && b.EndDate <= budgetDto.EndDate));

            return isOverlap;
        }

    }
}
