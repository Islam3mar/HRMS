using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class AttendanceFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "اسم الموظف")]
        [Required(ErrorMessage = "من فضلك ادخل اسم موظف صالح")]
        public int EmployeeId { get; set; }

        [Display(Name = "التاريخ")]
        [Required(ErrorMessage = "من فضلك ادخل تاريخ صحيح")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "وقت الحضور")]
        [Required(ErrorMessage = "من فضلك ادخل وقت الحضور")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan CheckInTime { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "وقت الانصراف")]
        [Required(ErrorMessage = "من فضلك ادخل وقت الانصراف")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan CheckOutTime { get; set; } = new TimeSpan(17, 0, 0);

        public List<SelectListItem> EmployeeOptions { get; set; } = new();
    }
}
