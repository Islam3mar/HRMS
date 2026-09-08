using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;   // جديد

        public EmployeesController(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }

        public async Task<IActionResult> Create() => View(await BuildEmptyForm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillOptions(model);
                return View(model);
            }

            var result = await _employeeService.CreateEmployeeAsync(MapToInput(model));

            if (!result.Success)
            {
                AddErrorsToModelState(result);
                await FillOptions(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "تم اضافة الموظف بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();

            var model = new EmployeeFormViewModel
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Address = employee.Address,
                PhoneNumber = employee.PhoneNumber,
                Gender = employee.Gender,
                Nationality = employee.Nationality,
                BirthDate = employee.BirthDate,
                NationalId = employee.NationalId,
                ContractDate = employee.ContractDate,
                Salary = employee.Salary,
                AttendanceTime = employee.AttendanceTime,
                DepartureTime = employee.DepartureTime,
                DepartmentId = employee.DepartmentId
            };

            await FillOptions(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillOptions(model);
                return View(model);
            }

            var result = await _employeeService.UpdateEmployeeAsync(id, MapToInput(model));

            if (!result.Success)
            {
                AddErrorsToModelState(result);
                await FillOptions(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات الموظف بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _employeeService.DeleteEmployeeAsync(id);

            TempData["SuccessMessage"] = deleted ? "تم حذف الموظف بنجاح" : "الموظف غير موجود";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------
        private async Task<EmployeeFormViewModel> BuildEmptyForm()
        {
            var model = new EmployeeFormViewModel();
            await FillOptions(model);
            return model;
        }

        private async Task FillOptions(EmployeeFormViewModel model)
        {
            model.GenderOptions = GetGenderOptions();
            model.DepartmentOptions = await GetDepartmentOptionsAsync();
        }

        private static List<SelectListItem> GetGenderOptions() =>
            Enum.GetValues<Gender>().Select(g => new SelectListItem
            {
                Value = ((int)g).ToString(),
                Text = g.ToArabicName()
            }).ToList();

        private async Task<List<SelectListItem>> GetDepartmentOptionsAsync()
        {
            var departments = await _departmentService.GetAllAsync();
            return departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();
        }

        private static EmployeeInput MapToInput(EmployeeFormViewModel model) => new()
        {
            FullName = model.FullName,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            Gender = model.Gender,
            Nationality = model.Nationality,
            BirthDate = model.BirthDate,
            NationalId = model.NationalId,
            ContractDate = model.ContractDate,
            Salary = model.Salary,
            AttendanceTime = model.AttendanceTime,
            DepartureTime = model.DepartureTime,
            DepartmentId = model.DepartmentId
        };

        private void AddErrorsToModelState(EmployeeResult result)
        {
            if (result.FullNameError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.FullName), result.FullNameError);
            if (result.AddressError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.Address), result.AddressError);
            if (result.PhoneNumberError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.PhoneNumber), result.PhoneNumberError);
            if (result.GenderError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.Gender), result.GenderError);
            if (result.NationalityError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.Nationality), result.NationalityError);
            if (result.BirthDateError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.BirthDate), result.BirthDateError);
            if (result.NationalIdError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.NationalId), result.NationalIdError);
            if (result.ContractDateError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.ContractDate), result.ContractDateError);
            if (result.SalaryError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.Salary), result.SalaryError);
            if (result.AttendanceTimeError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.AttendanceTime), result.AttendanceTimeError);
            if (result.DepartureTimeError != null) ModelState.AddModelError(nameof(EmployeeFormViewModel.DepartureTime), result.DepartureTimeError);
        }
    }
}
