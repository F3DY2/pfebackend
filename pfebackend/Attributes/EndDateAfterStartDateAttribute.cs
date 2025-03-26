using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace pfebackend.Attributes
{
    public class EndDateAfterStartDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var obj = validationContext.ObjectInstance;
            var type = obj.GetType();
            var startDateProperty = type.GetProperty("StartDate");

            if (startDateProperty == null)
            {
                return new ValidationResult("Start date property not found.");
            }

            var startDateValue = startDateProperty.GetValue(obj);
            if (startDateValue is DateOnly startDate && value is DateOnly endDate)
            {
                if (endDate > startDate)
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult("End date must be after the start date.");
            }

            return new ValidationResult("Invalid date values.");
        }
    }
}
