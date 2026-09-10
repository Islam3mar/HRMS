using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
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
            var row = await _payrollService.ApproveAsync(employeeId, month, year);
            if (row == null)
                return NotFound();

            ViewBag.MonthName = ArabicMonthNames[month - 1];
            return View(row);
        }


        // اعتماد الراتب من غير طباعة (زرار منفصل فى الجدول)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int employeeId, int month, int year, string? employeeName)
        {
            var row = await _payrollService.ApproveAsync(employeeId, month, year);
            TempData["SuccessMessage"] = row != null
                ? $"تم اعتماد راتب {row.EmployeeName} بنجاح"
                : "الموظف غير موجود";

            return RedirectToAction(nameof(Index), new { Month = month, Year = year, EmployeeName = employeeName });
        }

        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.Edit)]
        public async Task<IActionResult> Edit(int employeeId, int month, int year)
        {
            var row = await _payrollService.GetForEditAsync(employeeId, month, year);
            if (row == null) return NotFound();

            var model = new PayrollEditViewModel
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
                TotalOvertimeAmount = row.TotalOvertimeAmount,
                TotalDeductionAmount = row.TotalDeductionAmount,
                NetSalary = row.NetSalary
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.PayrollReport, PermissionAction.Edit)]
        public async Task<IActionResult> Edit(PayrollEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var input = new PayrollManualEditInput
            {
                EmployeeId = model.EmployeeId,
                Month = model.Month,
                Year = model.Year,
                TotalOvertimeAmount = model.TotalOvertimeAmount,
                TotalDeductionAmount = model.TotalDeductionAmount,
                NetSalary = model.NetSalary
            };

            var result = await _payrollService.EditApprovedAsync(input);

            if (!result.Success)
            {
                if (result.TotalOvertimeError != null) ModelState.AddModelError(nameof(model.TotalOvertimeAmount), result.TotalOvertimeError);
                if (result.TotalDeductionError != null) ModelState.AddModelError(nameof(model.TotalDeductionAmount), result.TotalDeductionError);
                if (result.NetSalaryError != null) ModelState.AddModelError(nameof(model.NetSalary), result.NetSalaryError);
                if (result.NotFoundError != null) ModelState.AddModelError(string.Empty, result.NotFoundError);
                return View(model);
            }

            TempData["SuccessMessage"] = BuildEditSummaryMessage(result);
            return RedirectToAction(nameof(Index), new { Month = model.Month, Year = model.Year });
        }


        // ---------- Helpers ----------
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
