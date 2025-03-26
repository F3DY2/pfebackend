using System;
using System.ComponentModel.DataAnnotations;

namespace pfebackend.Attributes
{
    public class StartDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateOnly startDate && startDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "Start date must be today or later.");
        }
    }
}
