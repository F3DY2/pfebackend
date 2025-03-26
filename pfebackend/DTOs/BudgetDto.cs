using pfebackend.Attributes;
using pfebackend.DTOs;
using System.ComponentModel.DataAnnotations;

public class BudgetDto
{
    public int? Id { get; set; }

    [Required]
    public Category Category { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Amount must be Positive.")]
    public float LimitValue { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Amount must be Positive.")]
    public float AlertValue { get; set; }

    [Required]
    [StartDate]
    public DateOnly StartDate { get; set; }

    [Required]
    [EndDateAfterStartDate] 
    public DateOnly EndDate { get; set; }

    [Required]
    public string UserId { get; set; }
}
