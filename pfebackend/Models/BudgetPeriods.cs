using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using pfebackend.Models;

namespace pfebackend.Models
{
    public enum BudgetPeriodType
    {
        Weekly,
        Monthly
    }

    public class BudgetPeriod
    {
        [Key]
        public int Id { get; set; }

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
        [ForeignKey("UserId")]

        public User User { get; set; }
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}