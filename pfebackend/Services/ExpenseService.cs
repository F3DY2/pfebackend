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
            // 1. Validate input
            var validation = await ValidateExpenseCreation(expenseDto);
            if (!validation.isValid)
                return (false, validation.error, null);

            // 2. Create and save expense
            var expense = await CreateAndSaveExpense(expenseDto);
            expenseDto.Id = expense.Id;

            // 3. Check budget limits and send alerts
            await CheckBudgetLimitsAndNotify(expenseDto);

            return (true, "Expense created successfully.", expenseDto);
        }

        private async Task<(bool isValid, string error)> ValidateExpenseCreation(ExpenseDto dto)
        {
            if (dto == null) return (false, "Expense data is required");
            if (string.IsNullOrEmpty(dto.UserId)) return (false, "User ID is required");
            if (dto.Amount <= 0) return (false, "Amount must be greater than zero");
            if (!await UserExists(dto.UserId)) return (false, "User not found");
            return (true, null);
        }

        private async Task<Expense> CreateAndSaveExpense(ExpenseDto dto)
        {
            var expense = new Expense
            {
                Name = dto.Name,
                Category = (Category)dto.Category,
                Date = dto.Date,
                Amount = dto.Amount,
                UserId = dto.UserId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        private async Task CheckBudgetLimitsAndNotify(ExpenseDto dto)
        {
            var activeBudgets = await GetActiveBudgetsForUser(dto.UserId, (Category)dto.Category,dto.Date);

            foreach (var budget in activeBudgets)
            {
                if (budget?.BudgetPeriod == null) continue;

                float totalExpenses = await CalculateTotalExpenses(
                    dto.UserId,
                    (Category)dto.Category,
                    budget.BudgetPeriod.StartDate,
                    budget.BudgetPeriod.EndDate);

                await HandleBudgetAlerts(
                    dto.UserId,
                    (Category)dto.Category,
                    totalExpenses,
                    budget.LimitValue,
                    budget.AlertValue,
                    dto.Date,
                    budget.BudgetPeriod);
            }
        }

        private async Task<List<Budget>> GetActiveBudgetsForUser(string userId, Category category, DateTime expenseDate)
        {
            return await _context.Budgets
                .Include(b => b.BudgetPeriod)
                .Where(b => b.BudgetPeriod != null &&
                          b.BudgetPeriod.UserId == userId &&
                          b.BudgetPeriod.StartDate <= expenseDate &&
                          b.BudgetPeriod.EndDate >= expenseDate &&
                          b.Category == category)
                .ToListAsync();
        }

        private async Task<float> CalculateTotalExpenses(
            string userId,
            Category category,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId &&
                          e.Category == category &&
                          e.Date >= startDate &&
                          e.Date <= endDate)
                .SumAsync(e => e.Amount);
        }

        private async Task HandleBudgetAlerts(
            string userId,
            Category category,
            float totalExpenses,
            float limitValue,
            float alertValue,
            DateTime expenseDate,
            BudgetPeriod budgetPeriod)
        {
            if (expenseDate < budgetPeriod.StartDate || expenseDate > budgetPeriod.EndDate)
            {
                return;
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user?.Email == null) return;

            if (totalExpenses > limitValue)
            {
                string message = $"Budget exceeded for {category}! Limit: {limitValue}, Current: {totalExpenses}";
                await _notificationService.SendCategoryNotification(
                    userId,
                    message,
                    NotificationType.BudgetAlert,
                    category);
                await SendBudgetEmail(user.Email, "Budget Limit Exceeded", message);
            }
            else if (totalExpenses > alertValue)
            {
                string message = $"Approaching budget limit for {category}! Limit: {limitValue}, Current: {totalExpenses}";
                await _notificationService.SendCategoryNotification(
                    userId,
                    message,
                    NotificationType.BudgetWarning,
                    category);
                await SendBudgetEmail(user.Email, "Budget Warning", message);
            }
        }

        private async Task SendBudgetEmail(string email, string subject, string content)
        {
            var message = new Message(
                new[] { email },
                subject,
                content);
                _emailSender.SendEmail(message);
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
