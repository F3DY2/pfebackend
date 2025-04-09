using EmailService;
using Microsoft.AspNetCore.Identity;
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
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public ExpenseService(AppDbContext context, INotificationService notificationService, IEmailSender emailSender, UserManager<User> userManager)
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
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name,
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
                CategoryId = expense.CategoryId,
                CategoryName = expense.Category?.Name,
                Date = expense.Date,
                Amount = expense.Amount,
                UserId = expense.UserId
            };
        }
        public async Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(string userId)
        {
            var expenses = await _context.Expenses
                                          .Include(b => b.Category)
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
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
                Date = e.Date,
                Amount = e.Amount,
                UserId = e.UserId
            }).ToList();
        }

        public async Task<(bool, string)> UpdateExpenseAsync(int id, ExpenseDto expenseDto)
        {
            if (id != expenseDto.Id)
                return (false, "Expense ID mismatch.");

            // Get the existing expense with tracking
            var expense = await _context.Expenses
                .AsTracking() // Important for detecting changes
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
                return (false, $"Expense with ID {id} not found.");

            // Store original values for comparison
            var originalAmount = expense.Amount;
            var originalDate = expense.Date;
            var originalUserId = expense.UserId;
            var originalCategoryId = expense.CategoryId;

            // Update the expense with new values
            expense.Name = expenseDto.Name;
            expense.CategoryId = expenseDto.CategoryId;
            expense.Date = expenseDto.Date;
            expense.Amount = expenseDto.Amount;
            expense.UserId = expenseDto.UserId;

            try
            {
                // Case 1: Nothing changed about amount or date
                if (originalAmount == expenseDto.Amount && originalDate == expenseDto.Date)
                {
                    await _context.SaveChangesAsync();
                    return (true, "Expense updated successfully (no financial impact).");
                }

                // Case 2: Only amount changed (same date)
                if (originalDate == expenseDto.Date)
                {
                    var amountDifference = originalAmount - expenseDto.Amount;
                    await _context.SaveChangesAsync();

                    // Adjust savings by the difference
                    await AdjustSavingsForUpdate(
                        expenseDto.UserId,
                        expenseDto.Date,
                        amountDifference,
                        originalCategoryId,
                        expenseDto.CategoryId);

                    return (true, "Expense amount updated successfully.");
                }

                // Case 3: Date changed (amount may or may not have changed)
                // First remove the old amount from old period
                await AdjustSavingsForUpdate(
                    originalUserId,
                    originalDate,
                    originalAmount, // This will add the amount back (reverse)
                    originalCategoryId,
                    originalCategoryId);

                // Then add the new amount to new period
                await AdjustSavingsForUpdate(
                    expenseDto.UserId,
                    expenseDto.Date,
                    -expenseDto.Amount, // This will subtract the new amount
                    originalCategoryId,
                    expenseDto.CategoryId);

                await _context.SaveChangesAsync();
                return (true, "Expense date and amount updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (_context.Expenses.Any(e => e.Id == id),
                       "Concurrency issue occurred while updating.");
            }
        }

        private async Task AdjustSavingsForUpdate(
            string userId,
            DateTime date,
            float amountDifference,
            int originalCategoryId,
            int newCategoryId)
        {
            // Find all active budget periods for this user that include the date
            var affectedPeriods = await _context.BudgetPeriods
                .Where(bp => bp.UserId == userId &&
                            bp.StartDate <= date &&
                            bp.EndDate >= date)
                .ToListAsync();

            foreach (var period in affectedPeriods)
            {
                // Adjust savings by the difference
                period.Savings += amountDifference;

                // Ensure savings doesn't go negative
                if (period.Savings < 0)
                {
                    period.Savings = 0;
                }

                // If category changed, we might need to adjust budget tracking
                if (originalCategoryId != newCategoryId)
                {
                    await _notificationService.SendCategoryNotification(
                        userId,
                        $"Expense category changed from {originalCategoryId} to {newCategoryId}",
                        NotificationType.CategoryChange,
                        newCategoryId,
                        (await _context.Categories.FindAsync(newCategoryId))?.Name);
                }
            }

            await _context.SaveChangesAsync();
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

            // 3. Recalculate savings for all affected budget periods
            await RecalculateSavings(expenseDto.UserId, expenseDto.Date, expenseDto.Amount);

            // 4. Check budget limits and send alerts
            await CheckBudgetLimitsAndNotify(expenseDto);

            return (true, "Expense created successfully.", expenseDto);
        }

        private async Task<(bool isValid, string error)> ValidateExpenseCreation(ExpenseDto dto)
        {
            if (dto == null) return (false, "Expense data is required");
            if (string.IsNullOrEmpty(dto.UserId)) return (false, "User ID is required");
            if (dto.Amount < 0) return (false, "Amount must be zero or greater than zero");
            if (!await UserExists(dto.UserId)) return (false, "User not found");
            return (true, null);
        }

        private async Task<Expense> CreateAndSaveExpense(ExpenseDto dto)
        {
            var expense = new Expense
            {
                Name = dto.Name,
                CategoryId = dto.CategoryId,
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
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            var activeBudgets = await GetActiveBudgetsForUser(dto.UserId, dto.CategoryId, dto.Date);

            foreach (var budget in activeBudgets)
            {
                if (budget?.BudgetPeriod == null) continue;

                float totalExpenses = await CalculateTotalExpenses(
                    dto.UserId,
                    dto.CategoryId,
                    budget.BudgetPeriod.StartDate,
                    budget.BudgetPeriod.EndDate);

                await HandleBudgetAlerts(
                    dto.UserId,
                    category.Id,
                    category.Name,
                    totalExpenses,
                    budget.LimitValue,
                    budget.AlertValue,
                    dto.Date,
                    budget.BudgetPeriod);
            }
        }

        private async Task<List<Budget>> GetActiveBudgetsForUser(string userId, int categoryId, DateTime expenseDate)
        {
            return await _context.Budgets
                .Include(b => b.BudgetPeriod)
                .Include(b => b.Category)
                .Where(b => b.BudgetPeriod != null &&
                          b.BudgetPeriod.UserId == userId &&
                          b.BudgetPeriod.StartDate <= expenseDate &&
                          b.BudgetPeriod.EndDate >= expenseDate &&
                          b.CategoryId == categoryId)
                .ToListAsync();
        }

        private async Task<float> CalculateTotalExpenses(
            string userId,
            int categoryId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.UserId == userId &&
                          e.CategoryId == categoryId &&
                          e.Date >= startDate &&
                          e.Date <= endDate)
                .SumAsync(e => e.Amount);
        }

        private async Task HandleBudgetAlerts(
            string userId,
            int categoryId,
            string categoryName,
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
                string message = $"Budget exceeded for {categoryName}! Limit: {limitValue}, Current: {totalExpenses}";
                await _notificationService.SendCategoryNotification(
                    userId,
                    message,
                    NotificationType.BudgetAlert,
                    categoryId,
                    categoryName);
                await SendBudgetEmail(user.Email, "Budget Limit Exceeded", message);
            }
            else if (totalExpenses > alertValue)
            {
                string message = $"Approaching budget limit for {categoryName}! Limit: {limitValue}, Current: {totalExpenses}";
                await _notificationService.SendCategoryNotification(
                    userId,
                    message,
                    NotificationType.BudgetWarning,
                    categoryId,
                    categoryName);
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
        private async Task RecalculateSavings(string userId, DateTime expenseDate, float expenseAmount)
        {
            // Find all active budget periods for this user that include the expense date
            var activePeriods = await _context.BudgetPeriods
                .Where(bp => bp.UserId == userId &&
                            bp.StartDate <= expenseDate &&
                            bp.EndDate >= expenseDate)
                .ToListAsync();

            foreach (var period in activePeriods)
            {
                // Subtract the expense amount from savings
                period.Savings -= expenseAmount;

                // Ensure savings doesn't go negative (optional - you might want to handle this differently)
                if (period.Savings < 0)
                {
                    period.Savings = 0;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteExpenseAsync(int id)
        {
            Expense expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;

            // Get the amount before deleting
            var amount = expense.Amount;
            var userId = expense.UserId;
            var date = expense.Date;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            // Add back the amount to savings in affected budget periods
            await AdjustSavingsOnDeletion(userId, date, amount);

            return true;
        }
        private async Task AdjustSavingsOnDeletion(string userId, DateTime expenseDate, float expenseAmount)
        {
            // Find all active budget periods for this user that included the expense date
            var affectedPeriods = await _context.BudgetPeriods
                .Where(bp => bp.UserId == userId &&
                            bp.StartDate <= expenseDate &&
                            bp.EndDate >= expenseDate)
                .ToListAsync();

            foreach (var period in affectedPeriods)
            {
                // Add back the expense amount to savings
                period.Savings += expenseAmount;

                // Don't let savings exceed income (optional)
                if (period.Savings > period.Income)
                {
                    period.Savings = period.Income;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> UserExists(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }
    }
}
