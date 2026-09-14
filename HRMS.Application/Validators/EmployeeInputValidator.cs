using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Validators
{
    public class EmployeeInputValidator : AbstractValidator<EmployeeInput>
    {
        private static readonly DateTime CompanyFoundationDate = new(2008, 1, 1);
        private const int MinimumAge = 20;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionService _encryptionService;

        public EmployeeInputValidator(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
        {
            // Stop executing further rules for a property once one rule for that property fails
            RuleLevelCascadeMode = CascadeMode.Stop;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;

            RuleFor(x => x.FullName).NotEmpty().WithMessage("هذا الحقل مطلوب");
            RuleFor(x => x.Address).NotEmpty().WithMessage("هذا الحقل مطلوب");
            RuleFor(x => x.Nationality).NotEmpty().WithMessage("هذا الحقل مطلوب");

            RuleFor(x => x.PhoneNumber)
     .NotEmpty().WithMessage("هذا الحقل مطلوب")
     .Matches(@"^01[0125]\d{8}$").WithMessage("رقم التليفون غير صحيح، يجب ان يبدأ بـ 010 أو 011 أو 012 أو 015 ويتكون من 11 رقم")
         .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("هذا الحقل مطلوب")
                .CustomAsync(ValidateNationalIdAsync)
                    .When(x => !string.IsNullOrWhiteSpace(x.NationalId));

            RuleFor(x => x.BirthDate)
                .NotEqual(default(DateTime)).WithMessage("هذا الحقل مطلوب")
                .Must(d => d.Date <= DateTime.Today).WithMessage("تاريخ الميلاد غير صحيح");

            RuleFor(x => x.ContractDate)
                .NotEqual(default(DateTime)).WithMessage("هذا الحقل مطلوب")
                .Must(d => d.Date >= CompanyFoundationDate).WithMessage($"تاريخ التعاقد لا يمكن ان يكون قبل {CompanyFoundationDate:yyyy/MM/dd}")
                .Must(d => d.Date <= DateTime.Today).WithMessage("تاريخ التعاقد غير صحيح");

            RuleFor(x => x.ContractDate)
                .Must((input, _) => CalculateAge(input.BirthDate, input.ContractDate) >= MinimumAge)
                .WithMessage($"عمر الموظف وقت التعاقد لا يمكن ان يقل عن {MinimumAge} سنة")
                .When(x => x.BirthDate != default && x.ContractDate != default);

            RuleFor(x => x.Salary).GreaterThan(0).WithMessage("الراتب يجب ان يكون رقم صحيح اكبر من صفر");

            RuleFor(x => x.AttendanceTime).NotEqual(default(TimeSpan)).WithMessage("هذا الحقل مطلوب");

            RuleFor(x => x.DepartureTime)
                .NotEqual(default(TimeSpan)).WithMessage("هذا الحقل مطلوب")
                .Must((input, departure) => departure > input.AttendanceTime)
                    .WithMessage("موعد الانصراف يجب ان يكون بعد موعد الحضور")
                    .When(x => x.AttendanceTime != default);
        }

        private async Task ValidateNationalIdAsync(string nationalId, ValidationContext<EmployeeInput> context, CancellationToken ct)
        {
            if (!EgyptianNationalIdHelper.TryDecodeBirthDate(nationalId, out _, out var formatError))
            {
                context.AddFailure(formatError!);
                return;
            }

            var excludeId = context.RootContextData.TryGetValue("ExcludeEmployeeId", out var v) ? v as int? : null;

            var exists = await _unitOfWork.Employees.NationalIdExistsAsync(_encryptionService.Encrypt(nationalId.Trim()), excludeId);
            if (exists)
                context.AddFailure("هذا الرقم القومي مستخدم بالفعل لموظف اخر");
        }

        private static int CalculateAge(DateTime birthDate, DateTime contractDate)
        {
            var age = contractDate.Year - birthDate.Year;
            if (birthDate.Date > contractDate.AddYears(-age)) age--;
            return age;
        }
    }
}
