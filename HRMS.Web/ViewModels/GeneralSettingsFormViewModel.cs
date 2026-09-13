using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class GeneralSettingsFormViewModel
    {
        [Display(Name = "نسبة الاضافة (%)")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0.01, 500, ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        public decimal AdditionRatePercentage { get; set; }

        [Display(Name = "نسبة الخصم (%)")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0.01, 300, ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        public decimal DeductionRatePercentage { get; set; }

        [Display(Name = "يوم الاجازة الرسمى 1")]
        public DayOfWeek WeeklyHoliday1 { get; set; } = DayOfWeek.Friday;

        [Display(Name = "يوم الاجازة الرسمى 2")]
        public DayOfWeek WeeklyHoliday2 { get; set; } = DayOfWeek.Saturday;

        public List<SelectListItem> DayOptions { get; set; } = new();
        public bool IsExisting { get; set; }
    }
}
