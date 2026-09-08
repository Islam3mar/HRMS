using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class DepartmentFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "اسم القسم")]
        [Required(ErrorMessage = "من فضلك ادخل اسم القسم")]
        public string Name { get; set; } = string.Empty;
    }
}
