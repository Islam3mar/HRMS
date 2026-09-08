using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class AttendanceSearchViewModel
    {
        [Display(Name = "بحث باسم الموظف او القسم")]
        public string? EmployeeName { get; set; }

        [Display(Name = "من")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "الى")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
    }
}
