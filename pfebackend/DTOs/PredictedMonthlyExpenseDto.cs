using Microsoft.EntityFrameworkCore;
using pfebackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace pfebackend.DTOs
{
    public class PredictedMonthlyExpenseDto
    {
        public int Id { get; set; }
        public float PredictedExpense { get; set; }
        public int BudgetPeriodId { get; set; }
        public string UserId { get; set; }
    }
}
