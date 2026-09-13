using FluentValidation;
using HRMS.Application.DTOs;

namespace HRMS.Application.Validators
{
    public class PayrollManualEditInputValidator : AbstractValidator<PayrollManualEditInput>
    {
        public PayrollManualEditInputValidator()
        {
            RuleFor(x => x.AdditionRatePercentage)
                .GreaterThanOrEqualTo(0).WithMessage("من فضلك ادخل نسبة اضافة صحيحة اكبر من او تساوى صفر");

            RuleFor(x => x.DeductionRatePercentage)
                .GreaterThanOrEqualTo(0).WithMessage("من فضلك ادخل نسبة خصم صحيحة اكبر من او تساوى صفر");
        }
    }
}