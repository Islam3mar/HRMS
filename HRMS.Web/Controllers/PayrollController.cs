using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Web.Authorization;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Controllers
{
    // تقرير رواتب الموظفين (المطلوب الثامن)
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IEmployeeService _employeeService;

        private static readonly string[] ArabicMonthNames =
        {
            "يناير", "فبراير", "مارس", "ابريل", "مايو", "يونيو",
            "يوليو", "اغسطس", "سبتمبر", "اكتوبر", "نوفمبر", "ديسمبر"
        };

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
            
        }

        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.View)]
        public async Task<IActionResult> Index(PayrollSearchViewModel search)
        {
            if (search.Month is < 1 or > 12)
                search.Month = DateTime.Today.Month;

            if (search.Year <= 0)
                search.Year = DateTime.Today.Year;

            var filter = new PayrollSearchFilter
            {
                EmployeeName = search.EmployeeName,
                Month = search.Month,
                Year = search.Year
            };

            var report = await _payrollService.GetReportAsync(filter);

            var model = new PayrollIndexViewModel
            {
                Rows = report.Rows,
                Search = search,
                MonthOptions = BuildMonthOptions(search.Month),
                YearOptions = BuildYearOptions(search.Year),
                SearchError = report.SearchError,
                YearError = report.YearError
            };

            return View(model);
        }

        // الطباعة نفسها بتعتبر اعتماد: اول مرة يتطبع فيها راتب شهر معين لموظف،
        // بيتحفظ كـ Snapshot ثابت ومتتأثرش قيمته بعد كده حتى لو اتعدلت بيانات الحضور
        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.View)]
        public async Task<IActionResult> Print(int employeeId, int month, int year)
        {
            try
            {
                var row = await _payrollService.ApproveAsync(employeeId, month, year);
                if (row == null) return NotFound();

                ViewBag.MonthName = ArabicMonthNames[month - 1];
                return View(row);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index), new { Month = month, Year = year });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int employeeId, int month, int year, string? employeeName)
        {
            try
            {
                var row = await _payrollService.ApproveAsync(employeeId, month, year);
                TempData["SuccessMessage"] = row != null
                    ? $"تم اعتماد راتب {row.EmployeeName} بنجاح"
                    : "الموظف غير موجود";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { Month = month, Year = year, EmployeeName = employeeName });
        }

        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.Edit)]
        public async Task<IActionResult> Edit(int employeeId, int month, int year)
        {
            try
            {
                var model = await BuildEditViewModelAsync(employeeId, month, year);
                if (model == null) return NotFound();

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index), new { Month = month, Year = year });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.Edit)]
        public async Task<IActionResult> Edit(PayrollEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillReadOnlyDisplayFieldsAsync(model);
                return View(model);
            }

            var input = new PayrollManualEditInput
            {
                EmployeeId = model.EmployeeId,
                Month = model.Month,
                Year = model.Year,
                AdditionRatePercentage = model.AdditionRatePercentage,
                DeductionRatePercentage = model.DeductionRatePercentage
            };

            var result = await _payrollService.EditApprovedAsync(input);

            if (!result.Success)
            {
                if (result.AdditionRateError != null) ModelState.AddModelError(nameof(model.AdditionRatePercentage), result.AdditionRateError);
                if (result.DeductionRateError != null) ModelState.AddModelError(nameof(model.DeductionRatePercentage), result.DeductionRateError);
                if (result.NetSalaryError != null) ModelState.AddModelError(string.Empty, result.NetSalaryError);
                if (result.NotFoundError != null) ModelState.AddModelError(string.Empty, result.NotFoundError);

                await FillReadOnlyDisplayFieldsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = BuildEditSummaryMessage(result);
            return RedirectToAction(nameof(Index), new { Month = model.Month, Year = model.Year });
        }

        // ---------- Helpers ----------
        private async Task<PayrollEditViewModel?> BuildEditViewModelAsync(int employeeId, int month, int year)
        {
            var (row, hourlyRate) = await _payrollService.GetEditContextAsync(employeeId, month, year);
            if (row == null) return null;

            return new PayrollEditViewModel
            {
                EmployeeId = row.EmployeeId,
                Month = row.Month,
                Year = row.Year,
                EmployeeName = row.EmployeeName,
                DepartmentName = row.DepartmentName,
                BaseSalary = row.BaseSalary,
                AttendanceDaysCount = row.AttendanceDaysCount,
                AbsenceDaysCount = row.AbsenceDaysCount,
                OvertimeHours = row.OvertimeHours,
                DeductionHours = row.DeductionHours,
                AbsenceDeductionAmount = row.AbsenceDeductionAmount,
                HourlyRate = hourlyRate,
                CurrentNetSalary = row.NetSalary,
                AdditionRatePercentage = 0,
                DeductionRatePercentage = 0
            };
        }

        private async Task FillReadOnlyDisplayFieldsAsync(PayrollEditViewModel model)
        {
            var (row, hourlyRate) = await _payrollService.GetEditContextAsync(model.EmployeeId, model.Month, model.Year);
            if (row == null) return;

            model.EmployeeName = row.EmployeeName;
            model.DepartmentName = row.DepartmentName;
            model.BaseSalary = row.BaseSalary;
            model.AttendanceDaysCount = row.AttendanceDaysCount;
            model.AbsenceDaysCount = row.AbsenceDaysCount;
            model.OvertimeHours = row.OvertimeHours;
            model.DeductionHours = row.DeductionHours;
            model.HourlyRate = hourlyRate;
            model.CurrentNetSalary = row.NetSalary;
            model.AbsenceDeductionAmount = row.AbsenceDeductionAmount;
        }
        // نفس المعادلة بالظبط المستخدمة فى PayrollService (BuildRow / EditApprovedAsync) - للعرض والمعاينة بس


        private List<SelectListItem> BuildMonthOptions(int selectedMonth)
        {
            return Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = ArabicMonthNames[m - 1],
                    Selected = m == selectedMonth
                })
                .ToList();
        }

        private List<SelectListItem> BuildYearOptions(int selectedYear)
        {
            var minYear = _payrollService.MinimumAllowedYear;
            var maxYear = DateTime.Today.Year;

            var years = Enumerable.Range(minYear, Math.Max(1, maxYear - minYear + 1))
                .Reverse();

            return years
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == selectedYear
                })
                .ToList();
        }

        private static string BuildEditSummaryMessage(PayrollEditResult result)
        {
            var row = result.Row!;
            var changes = new List<string>();

            if (result.PreviousTotalOvertimeAmount != row.TotalOvertimeAmount)
                changes.Add($"اجمالى الاضافى: {result.PreviousTotalOvertimeAmount:N2} ← {row.TotalOvertimeAmount:N2}");

            if (result.PreviousTotalDeductionAmount != row.TotalDeductionAmount)
                changes.Add($"اجمالى الخصم: {result.PreviousTotalDeductionAmount:N2} ← {row.TotalDeductionAmount:N2}");

            if (result.PreviousNetSalary != row.NetSalary)
                changes.Add($"الصافى: {result.PreviousNetSalary:N2} ← {row.NetSalary:N2}");

            var changesText = changes.Count > 0
                ? string.Join(" | ", changes)
                : "لم تحدث أي تغييرات فعلية";

            return $"تم تعديل راتب {row.EmployeeName} عن شهر {row.Month}/{row.Year} — {changesText}";
        }
    }
}