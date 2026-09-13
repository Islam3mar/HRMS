using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.DTOs;

namespace HRMS.Application.Validators
{
    public class GeneralSettingsInputValidator : AbstractValidator<GeneralSettingsInput>
    {
        public GeneralSettingsInputValidator()
        {
            RuleFor(x => x.AdditionRatePercentage).GreaterThan(0).WithMessage("من فضلك ادخل بيانات الحقل");
            RuleFor(x => x.DeductionRatePercentage).GreaterThan(0).WithMessage("من فضلك ادخل بيانات الحقل");

            RuleFor(x => x.WeeklyHoliday2)
                .Must((input, holiday2) => holiday2 != input.WeeklyHoliday1)
                .WithMessage("لا يمكن اختيار نفس يوم الاجازة مرتين");
        }
    }
}
