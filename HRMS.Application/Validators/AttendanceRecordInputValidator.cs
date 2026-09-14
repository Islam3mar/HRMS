using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.DTOs;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Validators
{
    public class AttendanceRecordInputValidator : AbstractValidator<AttendanceRecordInput>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceRecordInputValidator(IUnitOfWork unitOfWork)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            _unitOfWork = unitOfWork;

            RuleFor(x => x.EmployeeId)
                .MustAsync(async (id, ct) => id > 0 && await _unitOfWork.Employees.GetByIdAsync(id) != null)
                .WithMessage("من فضلك ادخل اسم موظف صالح");

            RuleFor(x => x.Date)
                .NotEqual(default(DateTime)).WithMessage("من فضلك ادخل تاريخ صحيح")
                .Must(d => d.Date >= DateTime.Today).WithMessage("لا يمكن إضافة أو تعديل سجل ليوم ماضٍ، المسموح: اليوم أو المستقبل");

            RuleFor(x => x.CheckInTime).NotEqual(default(TimeSpan)).WithMessage("من فضلك ادخل وقت الحضور");

            RuleFor(x => x.CheckOutTime)
                .NotEqual(default(TimeSpan)).WithMessage("من فضلك ادخل وقت الانصراف")
                .Must((input, checkOut) => input.CheckInTime == default || checkOut > input.CheckInTime)
                    .WithMessage("وقت الانصراف يجب ان يكون بعد وقت الحضور");

            RuleFor(x => x).CustomAsync(ValidateNoDuplicateAsync);
        }

      

        private async Task ValidateNoDuplicateAsync(AttendanceRecordInput input, ValidationContext<AttendanceRecordInput> context, CancellationToken ct)
        {
            if (input.EmployeeId <= 0 || input.Date == default) return;

            var excludeId = context.RootContextData.TryGetValue("ExcludeRecordId", out var v) ? v as int? : null;
            if (await _unitOfWork.AttendanceRecords.RecordExistsAsync(input.EmployeeId, input.Date, excludeId))
                context.AddFailure(nameof(AttendanceRecordInput.Date), "يوجد سجل حضور وانصراف مسجل بالفعل لهذا الموظف فى نفس هذا اليوم");
        }
    }
}
