using System.ComponentModel.DataAnnotations;
using AlvTimeWebApi.Requests;

namespace AlvTimeWebApi.Validators
{
    public class QuarterHour : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            var request = validationContext.ObjectInstance;

            if (request is PayoutRequest pr && pr.Hours % 0.25M != 0)
                throw new ValidationException("Bestilling må gå opp i kvarter");

            return ValidationResult.Success;
        }
    }
}
