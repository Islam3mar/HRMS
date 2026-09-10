using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Enums;
using HRMS.Web.Authorization;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [PermissionAuthorize(SystemPage.Departments, PermissionAction.View)]
        public async Task<IActionResult> Index()
        {
            var model = new DepartmentsIndexViewModel
            {
                Departments = await _departmentService.GetAllAsync(),
                Form = new DepartmentFormViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.Departments, PermissionAction.Edit)]
        public async Task<IActionResult> Save(DepartmentFormViewModel form)
        {
            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(form);

            var input = new DepartmentInput { Name = form.Name };

            var result = form.Id == 0
                ? await _departmentService.CreateAsync(input)
                : await _departmentService.UpdateAsync(form.Id, input);

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError("Form.Name", result.NameError);
                return await ReturnIndexWithErrors(form);
            }

            TempData["SuccessMessage"] = form.Id == 0 ? "تم اضافة القسم بنجاح" : "تم تعديل القسم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.Departments, PermissionAction.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _departmentService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "تم حذف القسم بنجاح" : error;
            return RedirectToAction(nameof(Index));
        }


        private async Task<IActionResult> ReturnIndexWithErrors(DepartmentFormViewModel form)
        {
            var model = new DepartmentsIndexViewModel
            {
                Departments = await _departmentService.GetAllAsync(),
                Form = form
            };
            return View(nameof(Index), model);
        }
    }
}
