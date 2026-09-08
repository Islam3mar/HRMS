using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class GeneralSettingsFormViewModel
    {
        [Display(Name = "الاضافة")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0.01, double.MaxValue, ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        public decimal AdditionRatePerHour { get; set; }

        [Display(Name = "الخصم")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0.01, double.MaxValue, ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        public decimal DeductionRatePerHour { get; set; }

        [Display(Name = "يوم الاجازة الرسمى 1")]
        public DayOfWeek WeeklyHoliday1 { get; set; } = DayOfWeek.Friday;

        [Display(Name = "يوم الاجازة الرسمى 2")]
        public DayOfWeek WeeklyHoliday2 { get; set; } = DayOfWeek.Saturday;

        public List<SelectListItem> DayOptions { get; set; } = new();

        // بيحدد لو السطر ده اتحفظ قبل كده -> الحقول بتفتح بس بعد الضغط على "تعديل"
        public bool IsExisting { get; set; }
    }
}
