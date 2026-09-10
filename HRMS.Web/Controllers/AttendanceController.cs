using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Enums;
using HRMS.Web.Authorization;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceRecordService _attendanceService;
        private readonly IEmployeeService _employeeService;

        private const int PageSize = 10;

        public AttendanceController(IAttendanceRecordService attendanceService, IEmployeeService employeeService)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
        }

        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.View)]
        public async Task<IActionResult> Index(AttendanceSearchViewModel search)
        {
            var model = await BuildIndexViewModel(search);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.Edit)]
        public async Task<IActionResult> Save(AttendanceFormViewModel form, AttendanceSearchViewModel search)
        {
            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(form, search);

            var input = new AttendanceRecordInput
            {
                EmployeeId = form.EmployeeId,
                Date = form.Date,
                CheckInTime = form.CheckInTime,
                CheckOutTime = form.CheckOutTime
            };

            var result = form.Id == 0
                ? await _attendanceService.CreateAsync(input)
                : await _attendanceService.UpdateAsync(form.Id, input);

            if (!result.Success)
            {
                if (result.EmployeeIdError != null) ModelState.AddModelError("Form.EmployeeId", result.EmployeeIdError);
                if (result.DateError != null) ModelState.AddModelError("Form.Date", result.DateError);
                if (result.CheckInTimeError != null) ModelState.AddModelError("Form.CheckInTime", result.CheckInTimeError);
                if (result.CheckOutTimeError != null) ModelState.AddModelError("Form.CheckOutTime", result.CheckOutTimeError);
                return await ReturnIndexWithErrors(form, search);
            }

            TempData["SuccessMessage"] = form.Id == 0 ? "تم اضافة سجل الحضور بنجاح" : "تم تعديل سجل الحضور بنجاح";
            return RedirectToIndexWithSearch(search);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.Delete)]
        public async Task<IActionResult> Delete(int id, AttendanceSearchViewModel search)
        {
            var deleted = await _attendanceService.DeleteAsync(id);
            TempData["SuccessMessage"] = deleted ? "تم حذف السجل بنجاح" : "السجل غير موجود";
            return RedirectToIndexWithSearch(search);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.Add)]
        public async Task<IActionResult> Import(IFormFile? excelFile, AttendanceSearchViewModel search)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "من فضلك اختر ملف اكسيل صالح";
                return RedirectToIndexWithSearch(search);
            }

            using var stream = excelFile.OpenReadStream();
            var result = await _attendanceService.ImportFromExcelAsync(stream);

            TempData["SuccessMessage"] = $"تم استيراد {result.SuccessCount} سجل بنجاح، وفشل {result.FailedCount} سجل";
            if (result.Errors.Count > 0)
                TempData["ImportErrors"] = string.Join(" | ", result.Errors.Take(10));

            return RedirectToIndexWithSearch(search);
        }

        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.View)]
        public async Task<IActionResult> ExportExcel(AttendanceSearchViewModel search)
        {
            var filter = BuildFilter(search, forExportOrPrint: true);
            var bytes = await _attendanceService.ExportToExcelAsync(filter);

            var fileName = $"تقرير_الحضور_والانصراف_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [PermissionAuthorize(SystemPage.AttendanceReport, PermissionAction.View)]
        public async Task<IActionResult> Print(AttendanceSearchViewModel search)
        {
            var filter = BuildFilter(search, forExportOrPrint: true);
            var records = await _attendanceService.SearchAllAsync(filter);
            return View(records);
        }

        // ---------- Helpers ----------
        private async Task<AttendanceIndexViewModel> BuildIndexViewModel(AttendanceSearchViewModel search)
        {
            var filter = BuildFilter(search, forExportOrPrint: false);
            var paged = await _attendanceService.SearchAsync(filter);

            return new AttendanceIndexViewModel
            {
                Records = paged.Items,
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Search = search,
                Form = new AttendanceFormViewModel { EmployeeOptions = await GetEmployeeOptionsAsync() },
                EmployeeOptions = await GetEmployeeOptionsAsync()
            };
        }

        private static AttendanceSearchFilter BuildFilter(AttendanceSearchViewModel search, bool forExportOrPrint) => new()
        {
            EmployeeName = search.EmployeeName,
            FromDate = search.FromDate,
            ToDate = search.ToDate,
            Page = search.Page < 1 ? 1 : search.Page,
            PageSize = forExportOrPrint ? int.MaxValue : PageSize
        };

        private async Task<List<SelectListItem>> GetEmployeeOptionsAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return employees.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.FullName
            }).ToList();
        }

        private async Task<IActionResult> ReturnIndexWithErrors(AttendanceFormViewModel form, AttendanceSearchViewModel search)
        {
            var model = await BuildIndexViewModel(search);
            form.EmployeeOptions = model.EmployeeOptions;
            model.Form = form;
            // tell the view to open the modal so user sees validation errors inside it
            ViewBag.ShowAttendanceModal = true;
            return View(nameof(Index), model);
        }

        private IActionResult RedirectToIndexWithSearch(AttendanceSearchViewModel search) =>
            RedirectToAction(nameof(Index), new
            {
                search.EmployeeName,
                search.FromDate,
                search.ToDate,
                search.Page
            });
    }
}
