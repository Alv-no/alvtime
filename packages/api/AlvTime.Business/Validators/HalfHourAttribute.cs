using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using System.ComponentModel.DataAnnotations;

namespace AlvTime.Business.Validators
{
    public class HalfHourAttribute : ValidationAttribute
    {
        public string GetErrorMessage() =>
            $"Value must be a multiple of a half hour (0.5)";

        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            var request = validationContext.ObjectInstance;

            if (request is GenericHourEntry entry && entry.Hours % 0.5M != 0 ||
                request is CreateTimeEntryDto entry2 && entry2.Value% 0.5M != 0 ||
                request is HourEntryRequest entry3 && entry3.Value% 0.5M != 0)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
    }
}
