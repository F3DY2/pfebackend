using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pfebackend.Data;
using pfebackend.DTOs;
using pfebackend.Interfaces;
using pfebackend.Models;

namespace pfebackend.Services
{
    public class PredictedMonthlyExpenseService : IPredictedMonthlyExpenseService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly UserManager<User> _userManager;

        public PredictedMonthlyExpenseService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            UserManager<User> userManager)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("PredictionApi");
            _userManager = userManager;
        }

        public async Task<PredictedMonthlyExpenseDto> CreatePredictedExpense(float income, string userId)
        {
            var predictedValue = await GetPredictionFromAI(income, userId);

            var expense = new PredictedMonthlyExpense
            {
                PredictedExpense = predictedValue,
                UserId = userId


            };

            _context.PredictedMonthlyExpenses.Add(expense);
            await _context.SaveChangesAsync();

            return new PredictedMonthlyExpenseDto
            {
                PredictedExpense = expense.PredictedExpense,
                BudgetPeriodId = expense.BudgetPeriodId,
                UserId = expense.UserId
            };
        }

        public async Task<PredictedMonthlyExpenseDto> UpdatePredictedExpense(int id, float newIncome, string userId)
        {
            var expense = await _context.PredictedMonthlyExpenses.FindAsync(id);
            if (expense == null)
                throw new KeyNotFoundException("Predicted expense not found");

            expense.PredictedExpense = await GetPredictionFromAI(newIncome, userId);

            await _context.SaveChangesAsync();

            return new PredictedMonthlyExpenseDto
            {
                PredictedExpense = expense.PredictedExpense,
                BudgetPeriodId = expense.BudgetPeriodId,
                UserId = expense.UserId
            };
        }

        public async Task<bool> DeletePredictedExpense(int id)
        {
            var expense = await _context.PredictedMonthlyExpenses.FindAsync(id);
            if (expense == null) return false;

            _context.PredictedMonthlyExpenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PredictedMonthlyExpenseDto> GetPredictedExpense(int id)
        {
            var expense = await _context.PredictedMonthlyExpenses.FindAsync(id);
            if (expense == null) return null;

            return new PredictedMonthlyExpenseDto
            {
                PredictedExpense = expense.PredictedExpense,
                BudgetPeriodId = expense.BudgetPeriodId,
                UserId = expense.UserId
            };
        }

        public async Task<float> GetPredictionFromAI(float income, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Validate required prediction parameters
            if (string.IsNullOrEmpty(user.AgriculturalHouseHoldIndicator) ||
                !user.TotalNumberOfFamilyMembers.HasValue ||
                !user.TotalNumberOfFamilyMembersEmployed.HasValue)
            {
                throw new InvalidOperationException(
                    "User profile missing required prediction parameters: " +
                    "AgriculturalHouseHoldIndicator, TotalNumberOfFamilyMembers, " +
                    "and TotalNumberOfFamilyMembersEmployed must be set");
            }

            // Prepare API request
            int agriIndicator;
            if (!int.TryParse(user.AgriculturalHouseHoldIndicator, out agriIndicator))
            {
                throw new InvalidOperationException("Invalid AgriculturalHouseHoldIndicator value");
            }
            var fullUrl = $"http://localhost:5000/predict_expense/{income}/{agriIndicator}/" +
                          $"{user.TotalNumberOfFamilyMembers.Value}/" +
                          $"{user.TotalNumberOfFamilyMembersEmployed.Value}";

            var response = await _httpClient.GetAsync(fullUrl);


            // Handle response
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (!root.TryGetProperty("expense_prediction", out JsonElement predictionArray) ||
                predictionArray.ValueKind != JsonValueKind.Array ||
                predictionArray.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Invalid prediction format from AI service");
            }

            float prediction = predictionArray[0].GetSingle();
            prediction = (float)Math.Round(prediction, 2); 

            return prediction;
        }


        public async Task<PredictedMonthlyExpenseDto> updatePredictedExpenseWhenUserDetailsChanged(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be empty");

            DateTime currentDate = DateTime.Today;
            var budgetPeriod = await _context.BudgetPeriods
                .Include(bp => bp.PredictedExpense)
                .Where(bp => bp.UserId == userId)
                .OrderByDescending(bp => bp.EndDate)
                .FirstOrDefaultAsync();

            if (budgetPeriod == null)
            {
                return new PredictedMonthlyExpenseDto
                {

                };
            }

            var predictedExpense = await _context.PredictedMonthlyExpenses
                .FirstOrDefaultAsync(pe => pe.BudgetPeriodId == budgetPeriod.Id);

            if (predictedExpense == null)
            {
                // Option 1: Create new predicted expense if none exists
                predictedExpense = new PredictedMonthlyExpense
                {
                    BudgetPeriodId = budgetPeriod.Id,
                    UserId = userId
                };
                _context.PredictedMonthlyExpenses.Add(predictedExpense);
            }

            predictedExpense.PredictedExpense = await GetPredictionFromAI(budgetPeriod.Income, userId);
            await _context.SaveChangesAsync();

            return new PredictedMonthlyExpenseDto
            {
                PredictedExpense = predictedExpense.PredictedExpense,
                BudgetPeriodId = predictedExpense.BudgetPeriodId,
                UserId = predictedExpense.UserId
            };
        }




    }
}
