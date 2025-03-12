using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;

        public ExpenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync()
        {
            return await _context.Expenses
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Category = (DTOs.Category)e.Category,
                    Date = e.Date,
                    Amount = e.Amount,
                    UserId = e.UserId
                })
                .ToListAsync();
        }

        public async Task<ExpenseDto?> GetExpenseByIdAsync(int id)
        {
            Expense expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return null;

            return new ExpenseDto
            {
                Id = expense.Id,
                Name = expense.Name,
                Category = (DTOs.Category)expense.Category,
                Date = expense.Date,
                Amount = expense.Amount,
                UserId = expense.UserId
            };
        }

        public async Task<bool> UpdateExpenseAsync(int id, ExpenseDto expenseDto)
        {
            if (id != expenseDto.Id) return false;

            Expense expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;

            expense.Name = expenseDto.Name;
            expense.Category = (Models.Category)expenseDto.Category;
            expense.Date = expenseDto.Date;
            expense.Amount = expenseDto.Amount;
            expense.UserId = expenseDto.UserId;

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return _context.Expenses.Any(e => e.Id == id);
            }
        }

        public async Task<ExpenseDto> CreateExpenseAsync(ExpenseDto expenseDto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == expenseDto.UserId);
            if (!userExists)
            {
                return null;
            }

            if (expenseDto.Amount <= 0)
            {
                return null;
            }
            Expense expense = new Expense
            {
                Name = expenseDto.Name,
                Category = (Models.Category)expenseDto.Category,
                Date = expenseDto.Date,
                Amount = expenseDto.Amount,
                UserId = expenseDto.UserId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            expenseDto.Id = expense.Id;
            return expenseDto;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            Expense expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UserExists(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
