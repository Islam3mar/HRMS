using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class RoleFormViewModel
    {
        [Display(Name = "اسم المجموعة")]
        public string Name { get; set; } = string.Empty;

        public List<PermissionRowViewModel> Permissions { get; set; } = new();
    }

}
