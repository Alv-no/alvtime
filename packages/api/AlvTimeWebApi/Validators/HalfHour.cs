using System.ComponentModel.DataAnnotations;
using AlvTimeWebApi.Requests;

namespace AlvTimeWebApi.Validators;

public class HalfHour : ValidationAttribute
{
    protected override ValidationResult IsValid(object value,
        ValidationContext validationContext)
    {
        var request = validationContext.ObjectInstance;

        if (request is PayoutRequest pr && pr.Hours % 0.5M != 0)
        {
            throw new ValidationException("Bestilling må gå opp i halvtimer");
        }

        return ValidationResult.Success;
    }
}