using pfebackend.Models;
using System.ComponentModel.DataAnnotations;

namespace pfebackend.DTOs
{
    public class BudgetPeriodDto
    {   
        public int? Id { get; set; }

        [Required]
        public BudgetPeriodType Period { get; set; }

        [Required]
        public float Income { get; set; }

        [Required]
        public float Savings { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
