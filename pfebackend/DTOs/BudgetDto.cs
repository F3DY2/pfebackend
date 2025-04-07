using pfebackend.Attributes;
using pfebackend.DTOs;
using pfebackend.Models;
using System.ComponentModel.DataAnnotations;

public class BudgetDto
{
    public int? Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }
    [Required]
    public float LimitValue { get; set; }

    [Required]
    public float AlertValue { get; set; }

    public float? PercentageSpent { get; set; }
    [Required]
    public int BudgetPeriodId { get; set; }
}