using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Domain.Enums;
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
            // بتوقف باقي الشروط لنفس الحقل أول ما شرط يفشل
            RuleLevelCascadeMode = CascadeMode.Stop;
            _unitOfWork = unitOfWork;
            _encryptionService = encryptionService;

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("هذا الحقل مطلوب")
                .MustAsync(BeUniqueFullNameAsync).WithMessage("هذا الاسم مستخدم بالفعل لموظف اخر");

            RuleFor(x => x.Address).NotEmpty().WithMessage("هذا الحقل مطلوب");
            RuleFor(x => x.Nationality).NotEmpty().WithMessage("هذا الحقل مطلوب");


            // ---------- Phone Number Validation ----------
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("هذا الحقل مطلوب")
                .Length(11).WithMessage("رقم التليفون يجب ان يتكون من 11 رقم بالظبط")
                .Matches(@"^\d{11}$").WithMessage("رقم التليفون يجب ان يتكون من ارقام فقط")
                .Must(HaveValidEgyptianPrefix)
                    .WithMessage($"رقم التليفون يجب ان يبدأ بأحد البادئات التالية: {string.Join(", ", EgyptianMobilePrefixes.All)}");

            // ---------- National ID Validation ----------
            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("هذا الحقل مطلوب")
                .Length(14).WithMessage("الرقم القومي يجب ان يتكون من 14 رقم بالظبط")
                .Matches(@"^\d{14}$").WithMessage("الرقم القومي يجب ان يتكون من ارقام فقط")
                .Must(HaveValidCenturyDigit).WithMessage("الرقم القومي غير صحيح (خانة القرن يجب ان تكون 2 او 3)")
                .Must(HaveValidBirthDateSegment).WithMessage("الرقم القومي غير صحيح (تاريخ الميلاد المستخرج منه غير صالح)")
                .Must(HaveValidGovernorateSegment).WithMessage("الرقم القومي غير صحيح (كود المحافظة غير معروف)")
                .Must(MatchEmployeeBirthDate).WithMessage("تاريخ الميلاد المدخل لا يطابق تاريخ الميلاد المستخرج من الرقم القومي")
                .MustAsync(BeUniqueNationalIdAsync).WithMessage("هذا الرقم القومي مستخدم بالفعل لموظف اخر");


            // ---------- Date Validation ----------
            RuleFor(x => x.BirthDate)
                .NotEqual(default(DateTime)).WithMessage("هذا الحقل مطلوب")
                .Must(d => d.Date <= DateTime.Today).WithMessage("تاريخ الميلاد غير صحيح");


            // ---------- Contract Date Validation ----------
            RuleFor(x => x.ContractDate)
                .NotEqual(default(DateTime)).WithMessage("هذا الحقل مطلوب")
                .Must(d => d.Date >= CompanyFoundationDate).WithMessage($"تاريخ التعاقد لا يمكن ان يكون قبل {CompanyFoundationDate:yyyy/MM/dd}")
                .Must(d => d.Date <= DateTime.Today).WithMessage("تاريخ التعاقد غير صحيح");

            // شرط عبر حقلين (BirthDate + ContractDate) - الحارس اتحط جوه الـ Must نفسه بدل .When() منفصلة
            RuleFor(x => x.ContractDate)
                .Must((input, _) =>
                    input.BirthDate == default || input.ContractDate == default ||
                    CalculateAge(input.BirthDate, input.ContractDate) >= MinimumAge)
                .WithMessage($"عمر الموظف وقت التعاقد لا يمكن ان يقل عن {MinimumAge} سنة");

            RuleFor(x => x.Salary).GreaterThan(0).WithMessage("الراتب يجب ان يكون رقم صحيح اكبر من صفر");

            RuleFor(x => x.AttendanceTime).NotEqual(default(TimeSpan)).WithMessage("هذا الحقل مطلوب");

            RuleFor(x => x.DepartureTime)
                .NotEqual(default(TimeSpan)).WithMessage("هذا الحقل مطلوب")
                .Must((input, departure) => input.AttendanceTime == default || departure > input.AttendanceTime)
                    .WithMessage("موعد الانصراف يجب ان يكون بعد موعد الحضور");
        }

        // ---------- Helpers ----------

        private static bool HaveValidEgyptianPrefix(string phone) =>
            EgyptianMobilePrefixes.All.Contains(phone.Substring(0, 3));

        private static bool HaveValidCenturyDigit(string nationalId) =>
            nationalId[0] == '2' || nationalId[0] == '3';

        private static bool HaveValidBirthDateSegment(string nationalId) =>
            EgyptianNationalIdHelper.TryDecodeBirthDate(nationalId, out _, out _);

        private static bool HaveValidGovernorateSegment(string nationalId) =>
            EgyptianNationalIdHelper.TryDecodeGovernorate(nationalId, out _, out _);

        private static bool MatchEmployeeBirthDate(EmployeeInput input, string nationalId)
        {
            if (!EgyptianNationalIdHelper.TryDecodeBirthDate(nationalId, out var decoded, out _))
                return false; // الشرط اللي فات فشل بالفعل، مش هنكرر نفس الخطأ هنا

            return decoded.Date == input.BirthDate.Date;
        }

        private async Task<bool> BeUniqueFullNameAsync(
            EmployeeInput input, string fullName, ValidationContext<EmployeeInput> context, CancellationToken ct)
        {
            var excludeId = context.RootContextData.TryGetValue("ExcludeEmployeeId", out var v) ? v as int? : null;
            var exists = await _unitOfWork.Employees.FullNameExistsAsync(fullName.Trim(), excludeId);
            return !exists;
        }

        private async Task<bool> BeUniqueNationalIdAsync(
            EmployeeInput input, string nationalId, ValidationContext<EmployeeInput> context, CancellationToken ct)
        {
            var excludeId = context.RootContextData.TryGetValue("ExcludeEmployeeId", out var v) ? v as int? : null;
            var exists = await _unitOfWork.Employees.NationalIdExistsAsync(_encryptionService.Encrypt(nationalId.Trim()), excludeId);
            return !exists;
        }

        private static int CalculateAge(DateTime birthDate, DateTime contractDate)
        {
            var age = contractDate.Year - birthDate.Year;
            if (birthDate.Date > contractDate.AddYears(-age)) age--;
            return age;
        }
    }
}
