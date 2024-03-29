﻿using System.ComponentModel.DataAnnotations;
using AlvTime.Business.TimeRegistration;
using ValidationException = FluentValidation.ValidationException;

namespace AlvTime.Business.Validators;

public class QuarterHour : ValidationAttribute
{
    protected override ValidationResult IsValid(object value,
        ValidationContext validationContext)
    {
        var request = validationContext.ObjectInstance;

        if (request is CreateTimeEntryDto entry2 && entry2.Value % 0.25M != 0)
            throw new ValidationException("Ført time må gå opp i kvarter");

        return ValidationResult.Success;
    }
}