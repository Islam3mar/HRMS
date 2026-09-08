using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class OfficialHolidayFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "الاسم")]
        [Required(ErrorMessage = "من فضلك ادخل اسم الاجازة")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "التاريخ")]
        [Required(ErrorMessage = "من فضلك ادخل تاريخ الاجازة")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
