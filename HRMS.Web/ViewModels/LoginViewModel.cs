using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "من فضلك ادخل اسم مستخدم صالح")]
        [Display(Name = "اسم المستخدم أو البريد الإلكتروني")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "من فضلك ادخل كلمة مرور صالحة")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
