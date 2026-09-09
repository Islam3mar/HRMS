using System;
using System.Collections.Generic;
using System.Text;
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

        // تاريخ تأسيس الشركة - نفس التاريخ المستخدم فى EmployeeService (قاعدة رقم 6 هناك)
        // عدّله هنا لو اتغير هناك عشان يفضلوا متطابقين
        private static readonly DateTime CompanyFoundationDate = new(2005, 6, 6);

        public int MinimumAllowedYear => CompanyFoundationDate.Year;

        public PayrollService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();

            // قاعدة 1: البحث باسم موظف غير موجود
            if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
            {
                employees = employees
                    .Where(e => e.FullName.Contains(filter.EmployeeName.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (employees.Count == 0)
                {
                    result.SearchError = "لا يوجد موظف بهذا الاسم، من فضلك ادخل اسم موظف صالح";
                    return result;
                }
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

            var attendanceFilter = new AttendanceSearchFilter
            {
                FromDate = monthStart,
                ToDate = monthEnd,
                Page = 1,
                PageSize = int.MaxValue
            };
            var records = (await _unitOfWork.AttendanceRecords.SearchAllAsync(attendanceFilter))
                .Where(r => r.EmployeeId == employeeId)
                .ToList();

            return BuildRow(employee, records, settings, holidays, monthStart, monthEnd, month, year);
        }

        public async Task<PayrollRowDto?> ApproveAsync(int employeeId, int month, int year)
        {
            // لو اتعمله اعتماد قبل كده، رجّع نفس السجل المحفوظ ومتعملش سجل جديد فوقه
            var existing = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(employeeId, month, year);
            if (existing != null) return MapFromRecord(existing);

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
                NetSalary = live.NetSalary,
                ApprovedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.PayrollRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            // بعد الحفظ، رجّع القيم بعد اضافة بيانات الموظف (اسم/قسم) للعرض
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
            var result = new PayrollEditResult();

            if (input.TotalOvertimeAmount < 0)
                result.TotalOvertimeError = "من فضلك ادخل قيمة صحيحة اكبر من او تساوى صفر";

            if (input.TotalDeductionAmount < 0)
                result.TotalDeductionError = "من فضلك ادخل قيمة صحيحة اكبر من او تساوى صفر";

            if (input.NetSalary <= 0)   // ← غيّرت من < 0 لـ <= 0
                result.NetSalaryError = "الصافى لا يمكن ان يكون صفر او اقل، من فضلك ادخل قيمة صحيحة";

            if (result.HasErrors) return result;

            var record = await _unitOfWork.PayrollRecords.GetByEmployeeAndMonthAsync(input.EmployeeId, input.Month, input.Year);
            if (record == null)
            {
                result.NotFoundError = "لا يوجد راتب معتمد لهذا الموظف عن هذا الشهر، من فضلك اعتمد الراتب اولاً";
                return result;
            }

            // نسجّل القيم القديمة قبل ما نعدّل عليها
            result.PreviousTotalOvertimeAmount = record.TotalOvertimeAmount;
            result.PreviousTotalDeductionAmount = record.TotalDeductionAmount;
            result.PreviousNetSalary = record.NetSalary;

            record.TotalOvertimeAmount = input.TotalOvertimeAmount;
            record.TotalDeductionAmount = input.TotalDeductionAmount;
            record.NetSalary = input.NetSalary;
            record.UpdatedAt = DateTime.Now;

            _unitOfWork.PayrollRecords.Update(record);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Row = MapFromRecord(record);
            return result;
        }

        // ---------- Helpers ----------
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
            NetSalary = record.NetSalary,
            Month = record.Month,
            Year = record.Year,
            IsApproved = true,
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
            var additionRate = settings?.AdditionRatePerHour ?? 0m;
            var deductionRate = settings?.DeductionRatePerHour ?? 0m;
            var weeklyHoliday1 = settings?.WeeklyHoliday1;
            var weeklyHoliday2 = settings?.WeeklyHoliday2;

            // احسب عدد ايام الشغل الفعلية فى الشهر (من غير الاجازات الاسبوعية و الرسمية)
            // لو الشهر الحالى لسه ماخلصش، نوقف عند النهاردة (مينفعش نحسب غياب فى ايام لسه ما جاتش)
            var effectiveEnd = monthEnd;
            var today = DateTime.Today;
            if (year == today.Year && month == today.Month)
                effectiveEnd = today;
            else if (monthStart > today)
                effectiveEnd = monthStart.AddDays(-1); // شهر فى المستقبل بالكامل: صفر ايام شغل

            var workingDays = 0;
            for (var day = monthStart; day <= effectiveEnd; day = day.AddDays(1))
            {
                if (weeklyHoliday1.HasValue && day.DayOfWeek == weeklyHoliday1.Value) continue;
                if (weeklyHoliday2.HasValue && day.DayOfWeek == weeklyHoliday2.Value) continue;
                if (holidays.Contains(day.Date)) continue;
                workingDays++;
            }

            var attendanceDays = employeeRecords.Select(r => r.Date.Date).Distinct().Count();
            var absenceDays = Math.Max(0, workingDays - attendanceDays);

            decimal overtimeHours = 0;
            decimal deductionHours = 0;

            foreach (var record in employeeRecords)
            {
                if (record.CheckInTime > employee.AttendanceTime)
                    deductionHours += (decimal)(record.CheckInTime - employee.AttendanceTime).TotalHours;

                if (record.CheckOutTime > employee.DepartureTime)
                    overtimeHours += (decimal)(record.CheckOutTime - employee.DepartureTime).TotalHours;
            }

            var totalOvertimeAmount = Math.Round(overtimeHours * additionRate, 2);
            var totalDeductionAmount = Math.Round(deductionHours * deductionRate, 2);
            var netSalary = employee.Salary + totalOvertimeAmount - totalDeductionAmount;

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
                TotalOvertimeAmount = totalOvertimeAmount,
                TotalDeductionAmount = totalDeductionAmount,
                NetSalary = netSalary,
                Month = month,
                Year = year,
                IsApproved = false,
                ApprovedAt = null
            };
        }
    }
}
