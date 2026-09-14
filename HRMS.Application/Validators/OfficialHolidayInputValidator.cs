using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.DTOs;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Validators
{
    public class OfficialHolidayInputValidator : AbstractValidator<OfficialHolidayInput>
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfficialHolidayInputValidator(IUnitOfWork unitOfWork)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Name).NotEmpty().WithMessage("من فضلك ادخل اسم الاجازة");

            RuleFor(x => x.Date)
                .NotEqual(default(DateTime)).WithMessage("من فضلك ادخل تاريخ الاجازة")
                .CustomAsync(ValidateUniqueDateAsync);
        }

        private async Task ValidateUniqueDateAsync(DateTime date, ValidationContext<OfficialHolidayInput> context, CancellationToken ct)
        {
            var excludeId = context.RootContextData.TryGetValue("ExcludeHolidayId", out var v) ? v as int? : null;
            if (await _unitOfWork.OfficialHolidays.DateExistsAsync(date, excludeId))
                context.AddFailure("يوجد اجازة رسمية مسجلة بنفس هذا التاريخ من قبل");
        }
    }
}
