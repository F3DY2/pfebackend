using pfebackend.Attributes;
using pfebackend.DTOs;
using pfebackend.Models;
using System.ComponentModel.DataAnnotations;

public class BudgetDto
{
    public int? Id { get; set; }
    [Required]
    public Category Category { get; set; }
    [Required]
    public float LimitValue { get; set; }
    [Required]
    public float AlertValue { get; set; }
    [Required]
    public int BudgetPeriodId { get; set; }

}
