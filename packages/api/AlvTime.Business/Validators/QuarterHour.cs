using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using System.ComponentModel.DataAnnotations;

namespace AlvTime.Business.Validators
{
    public class QuarterHour : ValidationAttribute
    {
        private static string GetErrorMessage() =>
            "Value must be a multiple of a quarter hour (0.25)";

        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            var request = validationContext.ObjectInstance;

            if (request is GenericHourEntry entry && entry.Hours % 0.25M != 0 ||
                request is CreateTimeEntryDto entry2 && entry2.Value% 0.25M != 0)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
    }
}
