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
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IGeneralSettingsService _settingsService;

        public SettingsController(IGeneralSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [PermissionAuthorize(SystemPage.GeneralSettings, PermissionAction.View)]
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSettingsAsync();

            var model = settings == null
                ? new GeneralSettingsFormViewModel { DayOptions = GetDayOptions(), IsExisting = false }
                : new GeneralSettingsFormViewModel
                {
                    AdditionRatePercentage = settings.AdditionRatePercentage,
                    DeductionRatePercentage = settings.DeductionRatePercentage,
                    WeeklyHoliday1 = settings.WeeklyHoliday1,
                    WeeklyHoliday2 = settings.WeeklyHoliday2,
                    DayOptions = GetDayOptions(),
                    IsExisting = true
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(SystemPage.GeneralSettings, PermissionAction.Edit)]
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
                AdditionRatePercentage = model.AdditionRatePercentage,
                DeductionRatePercentage = model.DeductionRatePercentage,
                WeeklyHoliday1 = model.WeeklyHoliday1,
                WeeklyHoliday2 = model.WeeklyHoliday2
            };

            var result = await _settingsService.SaveSettingsAsync(input);

            if (!result.Success)
            {
                if (result.AdditionRateError != null) ModelState.AddModelError(nameof(model.AdditionRatePercentage), result.AdditionRateError);
                if (result.DeductionRateError != null) ModelState.AddModelError(nameof(model.DeductionRatePercentage), result.DeductionRateError);
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
