using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace pfebackend.Models
{
    public class PredictedMonthlyExpense
    {
        public int Id { get; set; }
        public float PredictedExpense { get; set; }
        public int BudgetPeriodId { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public User User { get; set; }
        

        [ForeignKey("BudgetPeriodId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public BudgetPeriod BudgetPeriod { get; set; }
    }
}