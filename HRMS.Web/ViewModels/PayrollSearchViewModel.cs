using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class PayrollSearchViewModel
    {
        [Display(Name = "بحث باسم الموظف")]
        public string? EmployeeName { get; set; }

        [Display(Name = "شهر")]
        public int Month { get; set; } = DateTime.Today.Month;

        [Display(Name = "سنة")]
        public int Year { get; set; } = DateTime.Today.Year;
    }
}
