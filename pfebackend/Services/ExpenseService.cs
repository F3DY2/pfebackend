using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Hubs;
using pfebackend.Interfaces;
using pfebackend.Models;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;


namespace pfebackend.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public ExpenseService(AppDbContext context,INotificationService notificationService, IEmailSender emailSender, UserManager<User> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ExpenseDto>> GetExpensesAsync()
        {
            return await _context.Expenses
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Category = e.Category,
                    Date = e.Date,
                    Amount = e.Amount,
                    UserId = e.UserId,
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
                Category = expense.Category,
                Date = expense.Date,
                Amount = expense.Amount,
                UserId = expense.UserId
            };
        }
        public async Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(string userId)
        {
            var expenses = await _context.Expenses
                                          .Where(e => e.UserId == userId)
                                          .ToListAsync();

            if (expenses == null || !expenses.Any())
            {
                return Enumerable.Empty<ExpenseDto>(); 
            }

            return expenses.Select(e => new ExpenseDto
            {
                Id = e.Id,
                Name = e.Name,
                Category = e.Category,
                Date = e.Date,
                Amount = e.Amount,
                UserId = e.UserId
            }).ToList();
        }


        public async Task<(bool, string)> UpdateExpenseAsync(int id, ExpenseDto expenseDto)
        {
            if (id != expenseDto.Id)
                return (false, "Expense ID mismatch.");

            Expense expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return (false, $"Expense with ID {id} not found.");

            expense.Name = expenseDto.Name;
            expense.Category = (Models.Category)expenseDto.Category;
            expense.Date = expenseDto.Date;
            expense.Amount = expenseDto.Amount;
            expense.UserId = expenseDto.UserId;

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return (true, "Expense updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (_context.Expenses.Any(e => e.Id == id), "Concurrency issue occurred while updating.");
            }
        }

        public async Task<(bool, string, ExpenseDto)> CreateExpenseAsync(ExpenseDto expenseDto)
        {
            try
            {
                // 1. Validate input
                if (expenseDto == null)
                    return (false, "Expense data is required", null);

                if (string.IsNullOrEmpty(expenseDto.UserId))
                    return (false, "User ID is required", null);

                if (expenseDto.Amount <= 0)
                    return (false, "Amount must be greater than zero", null);

                // 2. Verify user exists and get user email
                var user = await _context.Users
                    .Where(u => u.Id == expenseDto.UserId)
                    .Select(u => new { u.Email, u.UserName })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return (false, "User not found", null);

                // 3. Create expense entity
                var expense = new Expense
                {
                    Name = expenseDto.Name,
                    Category = (Models.Category)expenseDto.Category,
                    Date = expenseDto.Date,
                    Amount = expenseDto.Amount,
                    UserId = expenseDto.UserId
                };

                // 4. Save expense first
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                expenseDto.Id = expense.Id;

                // 5. Check budget limits
                var activeBudgets = await _context.Budgets
                    .Include(b => b.BudgetPeriod)
                    .Where(b => b.BudgetPeriod != null &&
                              b.BudgetPeriod.UserId == expenseDto.UserId &&
                              b.BudgetPeriod.StartDate <= DateTime.Now &&
                              b.BudgetPeriod.EndDate >= DateTime.Now &&
                              b.Category == (Models.Category)expenseDto.Category)
                    .ToListAsync();

                foreach (var budget in activeBudgets)
                {
                    if (budget?.BudgetPeriod == null) continue;

                    try
                    {
                        float totalExpenses = await _context.Expenses
                            .Where(e => e.UserId == expenseDto.UserId &&
                                      e.Category == (Models.Category)expenseDto.Category &&
                                      e.Date >= budget.BudgetPeriod.StartDate &&
                                      e.Date <= budget.BudgetPeriod.EndDate)
                            .SumAsync(e => e.Amount);

                        // Use the notification service
                        await _notificationService.SendBudgetNotification(
                            expenseDto.UserId,
                            (Models.Category)expenseDto.Category,
                            totalExpenses,
                            budget.LimitValue,
                            budget.AlertValue);

                        // Create email message with current amount
                        string categoryName = Enum.GetName(typeof(Models.Category), expenseDto.Category);
                        string emailSubject = "Budget Alert Notification";
                        string emailContent = $"Approaching budget limit for {categoryName}! " +
                                            $"Limit: {budget.LimitValue}, Current: {totalExpenses}";

                        Message message = new Message(
                            new[] { user.Email }, // Utilisez l'email récupéré
                            emailSubject,
                            emailContent);

                        _emailSender.SendEmail(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking budget {budget.Id}: {ex.Message}");
                    }
                }

                return (true, "Expense created successfully.", expenseDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateExpenseAsync: {ex}");
                return (false, "An error occurred while creating the expense", null);
            }
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
