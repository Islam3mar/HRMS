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
    public class SettingsController : Controller
    {
        private readonly IGeneralSettingsService _settingsService;

        public SettingsController(IGeneralSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();

            var model = settings == null
                ? new GeneralSettingsFormViewModel { DayOptions = GetDayOptions(), IsExisting = false }
                : new GeneralSettingsFormViewModel
                {
                    AdditionRatePerHour = settings.AdditionRatePerHour,
                    DeductionRatePerHour = settings.DeductionRatePerHour,
                    WeeklyHoliday1 = settings.WeeklyHoliday1,
                    WeeklyHoliday2 = settings.WeeklyHoliday2,
                    DayOptions = GetDayOptions(),
                    IsExisting = true
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(GeneralSettingsFormViewModel model)
        {
            var wasExisting = await _settingsService.GetSettingsAsync() != null;

            if (!ModelState.IsValid)
            {
                model.DayOptions = GetDayOptions();
                model.IsExisting = wasExisting;
                return View(nameof(Index), model);
            }

            var input = new GeneralSettingsInput
            {
                AdditionRatePerHour = model.AdditionRatePerHour,
                DeductionRatePerHour = model.DeductionRatePerHour,
                WeeklyHoliday1 = model.WeeklyHoliday1,
                WeeklyHoliday2 = model.WeeklyHoliday2
            };

            var result = await _settingsService.SaveSettingsAsync(input);

            if (!result.Success)
            {
                if (result.AdditionRateError != null) ModelState.AddModelError(nameof(model.AdditionRatePerHour), result.AdditionRateError);
                if (result.DeductionRateError != null) ModelState.AddModelError(nameof(model.DeductionRatePerHour), result.DeductionRateError);
                if (result.WeeklyHoliday1Error != null) ModelState.AddModelError(nameof(model.WeeklyHoliday1), result.WeeklyHoliday1Error);
                if (result.WeeklyHoliday2Error != null) ModelState.AddModelError(nameof(model.WeeklyHoliday2), result.WeeklyHoliday2Error);

                model.DayOptions = GetDayOptions();
                model.IsExisting = wasExisting;
                return View(nameof(Index), model);
            }

            TempData["SuccessMessage"] = "تم الحفظ بنجاح";
            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> GetDayOptions() =>
            Enum.GetValues<DayOfWeek>().Select(d => new SelectListItem
            {
                Value = ((int)d).ToString(),
                Text = d.ToArabicName()
            }).ToList();
    }
}
