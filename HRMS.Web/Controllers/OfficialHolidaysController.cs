using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    [Authorize]
    public class OfficialHolidaysController : Controller
    {
        private readonly IOfficialHolidayService _holidayService;

        public OfficialHolidaysController(IOfficialHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new OfficialHolidaysIndexViewModel
            {
                Holidays = await _holidayService.GetAllAsync(),
                Form = new OfficialHolidayFormViewModel()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(OfficialHolidayFormViewModel form)
        {
            if (!ModelState.IsValid)
                return await ReturnIndexWithErrors(form);

            var input = new OfficialHolidayInput { Name = form.Name, Date = form.Date };

            var result = form.Id == 0
                ? await _holidayService.CreateAsync(input)
                : await _holidayService.UpdateAsync(form.Id, input);

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError(nameof(form.Name), result.NameError);
                if (result.DateError != null) ModelState.AddModelError(nameof(form.Date), result.DateError);
                return await ReturnIndexWithErrors(form);
            }

            TempData["SuccessMessage"] = form.Id == 0 ? "تم اضافة الاجازة بنجاح" : "تم تعديل الاجازة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _holidayService.DeleteAsync(id);
            TempData["SuccessMessage"] = deleted ? "تم حذف الاجازة بنجاح" : "الاجازة غير موجودة";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> ReturnIndexWithErrors(OfficialHolidayFormViewModel form)
        {
            var model = new OfficialHolidaysIndexViewModel
            {
                Holidays = await _holidayService.GetAllAsync(),
                Form = form
            };
            return View(nameof(Index), model);
        }
    }
}
