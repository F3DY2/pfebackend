using pfebackend.DTOs;

namespace pfebackend.Interfaces
{
    public interface IPredictedMonthlyExpenseService
    {
        Task<PredictedMonthlyExpenseDto> CreatePredictedExpense(float income, string userId);
        Task<PredictedMonthlyExpenseDto> UpdatePredictedExpense(int id, float newIncome, string userId);
        Task<bool> DeletePredictedExpense(int id);
        Task<PredictedMonthlyExpenseDto> GetPredictedExpense(int id);
        Task<float> GetPredictionFromAI(float income, string userId);
    }
}
