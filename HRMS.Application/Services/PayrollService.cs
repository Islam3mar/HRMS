using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    // تقرير رواتب الموظفين (المطلوب الثامن)
    // + اعتماد/تجميد الراتب كسجل ثابت (PayrollRecord) عشان لو اتعدلت بيانات الحضور
    // او الراتب الاساسى بعد كده، الراتب اللى اتعمله اعتماد او طباعة يفضل زي ما هو
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<PayrollManualEditInput> _manualEditValidator;

        // تاريخ تأسيس الشركة - نفس التاريخ المستخدم فى EmployeeService (قاعدة رقم 6 هناك)
        // عدّله هنا لو اتغير هناك عشان يفضلوا متطابقين
        private static readonly DateTime CompanyFoundationDate = new(2008, 1, 1);

        public int MinimumAllowedYear => CompanyFoundationDate.Year;

        public PayrollService(IUnitOfWork unitOfWork, IValidator<PayrollManualEditInput> manualEditValidator)
        {
            _unitOfWork = unitOfWork;
            _manualEditValidator = manualEditValidator;
        }

        public async Task<PayrollReportResult> GetReportAsync(PayrollSearchFilter filter)
        {
            var result = new PayrollReportResult();

            // قاعدة 2: منع اختيار سنة قبل تأسيس الشركة
            if (filter.Year < MinimumAllowedYear)
            {
                result.YearError = $"لا يمكن اختيار سنة اقل من سنة تأسيس الشركة ({MinimumAllowedYear})";
                return result;
            }

            if (filter.Year > DateTime.Today.Year)
            {
                result.YearError = "السنة المختارة غير صحيحة";
                return result;
            }

            // قاعدة 1: البحث باسم موظف غير موجود
            // ملحوظة: الفلترة بالاسم بقت بتتم فى الداتابيز نفسها (SQL LIKE) عن طريق
            // SearchByNameAsync (اللى بتستخدم EmployeesByNameSpecification) بدل ما كنا
            // بنجيب كل الموظفين GetAllAsync() ونفلترهم هنا فى الـ C# Memory
            List<Employee> employees;

            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
            {
                employees = (await _unitOfWork.Employees.SearchByNameAsync(filter.EmployeeName.Trim())).ToList();

                if (employees.Count == 0)
                {
                    result.SearchError = "لا يوجد موظف بهذا الاسم، من فضلك ادخل اسم موظف صالح";
                    return result;
                }
            }
            else
            {
                employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            }

            // السجلات المعتمدة (المحفوظة) بالفعل عن الشهر/السنة دول - بنجيبها مرة واحدة لكل الموظفين
            var approvedRecords = (await _unitOfWork.PayrollRecords.GetByMonthAsync(filter.Month, filter.Year))
                .ToDictionary(p => p.EmployeeId);

            var settings = await _unitOfWork.GeneralSettings.GetSingleAsync();
            var holidays = (await _unitOfWork.OfficialHolidays.GetAllOrderedByDateAsync())
                .Select(h => h.Date.Date)
                .ToHashSet();

            var (monthStart, monthEnd) = GetMonthRange(filter.Month, filter.Year);

            var attendanceFilter = new AttendanceSearchFilter
            {
                FromDate = monthStart,
                ToDate = monthEnd,
                Page = 1,
                PageSize = int.MaxValue
            };
            var allRecordsInMonth = (await _unitOfWork.AttendanceRecords.SearchAllAsync(attendanceFilter)).ToList();

            foreach (var employee in employees.OrderBy(e => e.FullName))
            {
                // لو الراتب معتمد ومحفوظ بالفعل، اعرض نفس القيم المحفوظة زي ما هى ومتحسبش تانى
                if (approvedRecords.TryGetValue(employee.Id, out var approved))
                {
                    result.Rows.Add(MapFromRecord(approved));
                    continue;
                }

                var employeeRecords = allRecordsInMonth.Where(r => r.EmployeeId == employee.Id).ToList();
                result.Rows.Add(BuildRow(employee, employeeRecords, settings, holidays, monthStart, monthEnd, filter.Month, filter.Year));
            }

            return result;
        }

        public async Task<PayrollRowDto?> GetEmployeePayrollAsync(int employeeId, int month, int year)
        {
            // لو فيه سجل معتمد محفوظ بالفعل، رجّعه زي ما هو من غير اى حساب جديد
            var approved = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(employeeId, month, year);
            if (approved != null) return MapFromRecord(approved);

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null) return null;

            var settings = await _unitOfWork.GeneralSettings.GetSingleAsync();
            var holidays = (await _unitOfWork.OfficialHolidays.GetAllOrderedByDateAsync())
                .Select(h => h.Date.Date)
                .ToHashSet();

            var (monthStart, monthEnd) = GetMonthRange(month, year);

            // بنضيف EmployeeId فى الفلتر نفسه عشان السجلات المطلوبة بس هى اللي تتجاب من
            // الداتابيز (SQL WHERE EmployeeId = ...)، بدل ما كنا بنجيب سجلات كل الموظفين
            // فى الشهر ده ونفلترها هنا لموظف واحد بس فى الـ Memory
            var attendanceFilter = new AttendanceSearchFilter
            {
                EmployeeId = employeeId,
                FromDate = monthStart,
                ToDate = monthEnd,
                Page = 1,
                PageSize = int.MaxValue
            };
            var records = (await _unitOfWork.AttendanceRecords.SearchAllAsync(attendanceFilter)).ToList();

            return BuildRow(employee, records, settings, holidays, monthStart, monthEnd, month, year);
        }

        public async Task<PayrollRowDto?> ApproveAsync(int employeeId, int month, int year)
        {
            var existing = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(employeeId, month, year);
            if (existing != null) return MapFromRecord(existing);

            // لا يمكن اعتماد راتب لشهر مستقبلي
            var today = DateTime.Today;
            if (year > today.Year || (year == today.Year && month > today.Month))
                throw new InvalidOperationException("لا يمكن اعتماد راتب لشهر مستقبلي");

            // الشهر الحالي لا يمكن اعتماده أو طباعته لأن البيانات قد لا تكون نهائية
            if (year == today.Year && month == today.Month)
                throw new InvalidOperationException("لا يمكن اعتماد او طباعة راتب هذا الشهر لانه لم ينته بعد، من فضلك انتظر حتى نهاية الشهر");

            var live = await GetEmployeePayrollAsync(employeeId, month, year);
            if (live == null) return null;

            var record = new PayrollRecord
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                BaseSalary = live.BaseSalary,
                AttendanceDaysCount = live.AttendanceDaysCount,
                AbsenceDaysCount = live.AbsenceDaysCount,
                OvertimeHours = live.OvertimeHours,
                DeductionHours = live.DeductionHours,
                TotalOvertimeAmount = live.TotalOvertimeAmount,
                TotalDeductionAmount = live.TotalDeductionAmount,
                AbsenceDeductionAmount = live.AbsenceDeductionAmount,
                NetSalary = live.NetSalary,
                HourlyRate = live.HourlyRate,
                ApprovedAt = DateTime.Now
            };

            await _unitOfWork.PayrollRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            live.IsApproved = true;
            live.ApprovedAt = record.ApprovedAt;
            if (employee != null)
            {
                live.EmployeeName = employee.FullName;
                live.DepartmentName = employee.Department?.Name ?? "-";
            }

            return live;
        }
        public async Task<PayrollRowDto?> GetForEditAsync(int employeeId, int month, int year)
        {
            // نتأكد إن الراتب معتمد ومحفوظ الأول (لو مش معتمد، بيتعمله اعتماد لحظياً)
            // وده بيمنع إنك تعدّل قيمة لسه ماتحفظتش كـ Snapshot
            return await ApproveAsync(employeeId, month, year);
        }

        public async Task<PayrollEditResult> EditApprovedAsync(PayrollManualEditInput input)
        {
            var result = await ValidateManualEditAsync(input);
            if (result.HasErrors) return result;

            var record = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(input.EmployeeId, input.Month, input.Year);
            if (record == null)
            {
                result.NotFoundError = "لا يوجد راتب معتمد لهذا الموظف عن هذا الشهر، من فضلك اعتمد الراتب اولاً";
                return result;
            }

            if (record.HourlyRate <= 0)
            {
                record.HourlyRate = await RecalculateStandardHourlyRateAsync(input.EmployeeId);
            }

            var hourlyRate = record.HourlyRate;

            // Treat AdditionRatePercentage as the full percent value (e.g., 135 means 135% of hourly rate)
            var overtimeHourRate = hourlyRate * (input.AdditionRatePercentage / 100m);
            var deductionHourRate = hourlyRate * (input.DeductionRatePercentage / 100m);

            var newTotalOvertimeAmount = Math.Round(record.OvertimeHours * overtimeHourRate, 2);
            var newTotalDeductionAmount = Math.Round(record.DeductionHours * deductionHourRate, 2);
            var newNetSalary = record.BaseSalary - record.AbsenceDeductionAmount + newTotalOvertimeAmount - newTotalDeductionAmount;

            if (newNetSalary <= 0)
            {
                result.NetSalaryError = "الصافى الناتج من النسب دي صفر او اقل، من فضلك ادخل نسب مختلفة";
                return result;
            }

            result.PreviousTotalOvertimeAmount = record.TotalOvertimeAmount;
            result.PreviousTotalDeductionAmount = record.TotalDeductionAmount;
            result.PreviousNetSalary = record.NetSalary;

            record.TotalOvertimeAmount = newTotalOvertimeAmount;
            record.TotalDeductionAmount = newTotalDeductionAmount;
            record.NetSalary = newNetSalary;

            _unitOfWork.PayrollRecords.Update(record);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Row = MapFromRecord(record);
            return result;
        }


        public async Task<(PayrollRowDto? Row, decimal HourlyRate)> GetEditContextAsync(int employeeId, int month, int year)
        {
            var row = await GetForEditAsync(employeeId, month, year);
            if (row == null) return (null, 0m);

            if (row.HourlyRate <= 0)
            {
                var record = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(employeeId, month, year);
                if (record != null)
                {
                    record.HourlyRate = await RecalculateStandardHourlyRateAsync(employeeId);
                    _unitOfWork.PayrollRecords.Update(record);
                    await _unitOfWork.SaveChangesAsync();
                    row.HourlyRate = record.HourlyRate;
                }
            }

            return (row, row.HourlyRate);
        }
        // ---------- Helpers ----------


        private async Task<PayrollEditResult> ValidateManualEditAsync(PayrollManualEditInput input)
        {
            var result = new PayrollEditResult();

            var validation = await _manualEditValidator.ValidateAsync(input);
            if (validation.IsValid) return result;

            foreach (var failure in validation.Errors)
            {
                switch (failure.PropertyName)
                {
                    case nameof(PayrollManualEditInput.AdditionRatePercentage): result.AdditionRateError = failure.ErrorMessage; break;
                    case nameof(PayrollManualEditInput.DeductionRatePercentage): result.DeductionRateError = failure.ErrorMessage; break;
                }
            }

            return result;
        }

        private static (DateTime start, DateTime end) GetMonthRange(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }

        private static PayrollRowDto MapFromRecord(PayrollRecord record) => new()
        {
            EmployeeId = record.EmployeeId,
            EmployeeName = record.Employee?.FullName ?? string.Empty,
            DepartmentName = record.Employee?.Department?.Name ?? "-",
            BaseSalary = record.BaseSalary,
            AttendanceDaysCount = record.AttendanceDaysCount,
            AbsenceDaysCount = record.AbsenceDaysCount,
            OvertimeHours = record.OvertimeHours,
            DeductionHours = record.DeductionHours,
            TotalOvertimeAmount = record.TotalOvertimeAmount,
            TotalDeductionAmount = record.TotalDeductionAmount,
            AbsenceDeductionAmount = record.AbsenceDeductionAmount,
            NetSalary = record.NetSalary,
            Month = record.Month,
            Year = record.Year,
            IsApproved = true,
            HourlyRate = record.HourlyRate,
            ApprovedAt = record.ApprovedAt
        };

        private static PayrollRowDto BuildRow(
       Employee employee,
       List<AttendanceRecord> employeeRecords,
       GeneralSettings? settings,
       HashSet<DateTime> holidays,
       DateTime monthStart,
       DateTime monthEnd,
       int month,
       int year)
        {
            var additionPercentage = settings?.AdditionRatePercentage ?? 0m;
            var deductionPercentage = settings?.DeductionRatePercentage ?? 0m;
            var weeklyHoliday1 = settings?.WeeklyHoliday1;
            var weeklyHoliday2 = settings?.WeeklyHoliday2;

            // احسب عدد ايام الشغل الفعلية اللى "عدت" فى الشهر (من غير الاجازات) - لحساب الغياب بس
            var effectiveEnd = monthEnd;
            var today = DateTime.Today;
            if (year == today.Year && month == today.Month)
                effectiveEnd = today;
            else if (monthStart > today)
                effectiveEnd = monthStart.AddDays(-1); // شهر فى المستقبل بالكامل: صفر ايام شغل

            var effectiveStart = monthStart > employee.ContractDate.Date ? monthStart : employee.ContractDate.Date;
            var elapsedWorkingDays = CountWorkingDays(effectiveStart, effectiveEnd, weeklyHoliday1, weeklyHoliday2, holidays);

            var attendanceDays = employeeRecords.Select(r => r.Date.Date).Distinct().Count();
            var absenceDays = Math.Max(0, elapsedWorkingDays - attendanceDays);

            decimal overtimeHours = 0;
            decimal deductionHours = 0;

            foreach (var record in employeeRecords)
            {
                if (record.CheckInTime > employee.AttendanceTime)
                    deductionHours += (decimal)(record.CheckInTime - employee.AttendanceTime).TotalHours;

                if (record.CheckOutTime > employee.DepartureTime)
                    overtimeHours += (decimal)(record.CheckOutTime - employee.DepartureTime).TotalHours;
            }

            // الراتب بيتحسب على 30 يوم ثابتين كل شهر (مش على ايام الشغل الفعلية فقط)
            // لان ايام الاجازات الاسبوعية والرسمية مدفوعة الاجر برضو ومبتتخصمش من الموظف
            const decimal DaysInMonthForPayroll = 30m;
            var dailyRate = employee.Salary / DaysInMonthForPayroll;

            var dailyWorkHours = (decimal)(employee.DepartureTime - employee.AttendanceTime).TotalHours;
            if (dailyWorkHours <= 0) dailyWorkHours = 8; // احتياطى لو موظف بيانات مواعيده غلط

            var hourlyRate = dailyRate / dailyWorkHours;

            // Treat AdditionRatePercentage as the full percent value (e.g., 135 means 135% of hourly rate)
            var overtimeHourRate = hourlyRate * (additionPercentage / 100m);
            var deductionHourRate = hourlyRate * (deductionPercentage / 100m);

            var totalOvertimeAmount = Math.Round(overtimeHours * overtimeHourRate, 2);
            var totalDeductionAmount = Math.Round(deductionHours * deductionHourRate, 2);

            // خصم ايام الغياب الفعلية (يوم غياب = يوم شغل مفروض يحضره ومحضرش، غير الاجازات) بسعر اليوم الثابت
            var absenceDeductionAmount = Math.Round(absenceDays * dailyRate, 2);

            var netSalary = employee.Salary - absenceDeductionAmount + totalOvertimeAmount - totalDeductionAmount;

            return new PayrollRowDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                DepartmentName = employee.Department?.Name ?? "-",
                BaseSalary = employee.Salary,
                AttendanceDaysCount = attendanceDays,
                AbsenceDaysCount = absenceDays,
                OvertimeHours = Math.Round(overtimeHours, 2),
                DeductionHours = Math.Round(deductionHours, 2),
                HourlyRate = Math.Round(hourlyRate, 2),
                TotalOvertimeAmount = totalOvertimeAmount,
                TotalDeductionAmount = totalDeductionAmount,
                AbsenceDeductionAmount = absenceDeductionAmount,
                NetSalary = netSalary,
                Month = month,
                Year = year,
                IsApproved = false,
                ApprovedAt = null
            };
        }
        // ميثود مساعدة جديدة - بتحسب عدد ايام الشغل بين تاريخين مع استبعاد الاجازات الاسبوعية والرسمية
        private static int CountWorkingDays(DateTime start, DateTime end, DayOfWeek? holiday1, DayOfWeek? holiday2, HashSet<DateTime> officialHolidays)
        {
            if (start > end) return 0;

            var count = 0;
            for (var day = start; day <= end; day = day.AddDays(1))
            {
                if (holiday1.HasValue && day.DayOfWeek == holiday1.Value) continue;
                if (holiday2.HasValue && day.DayOfWeek == holiday2.Value) continue;
                if (officialHolidays.Contains(day.Date)) continue;
                count++;
            }
            return count;
        }
        private async Task<decimal> RecalculateStandardHourlyRateAsync(int employeeId)
        {
            var schedule = await _unitOfWork.Employees.GetScheduleAsync(employeeId);
            if (schedule == null) return 0m;

            const decimal DaysInMonthForPayroll = 30m;
            var dailyRate = schedule.Salary / DaysInMonthForPayroll;

            var dailyWorkHours = (decimal)(schedule.DepartureTime - schedule.AttendanceTime).TotalHours;
            if (dailyWorkHours <= 0) dailyWorkHours = 8;

            return Math.Round(dailyRate / dailyWorkHours, 2);
        }
    }
}