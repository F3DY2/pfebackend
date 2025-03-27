using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pfebackend.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Category Category { get; set; }
        [Required]
        public float LimitValue { get; set; }
        [Required]
        public float AlertValue { get; set; }
        [Required]
        public int BudgetPeriodId { get; set; }

        [ForeignKey("BudgetPeriodId")]
        public BudgetPeriod BudgetPeriod { get; set; }

    }
}
