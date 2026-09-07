using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class UserFormViewModel
    {
        [Display(Name = "الاسم بالكامل")]
        [Required(ErrorMessage = "من فضلك ادخل الاسم بالكامل")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "اسم المستخدم")]
        [Required(ErrorMessage = "من فضلك ادخل اسم مستخدم صالح")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "البريد الالكتروني")]
        [Required(ErrorMessage = "من فضلك ادخل بريد الكتروني صالح")]
        [EmailAddress(ErrorMessage = "من فضلك ادخل بريد الكتروني صالح")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "الباسورد")]
        [Required(ErrorMessage = "من فضلك ادخل كلمة مرور صالحة")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "الصلاحيات")]
        [Required(ErrorMessage = "من فضلك اختر الصلاحيات")]
        public int RoleId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();
    }
}
