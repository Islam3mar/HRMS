using System.ComponentModel.DataAnnotations;
using HRMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRMS.Web.ViewModels
{
    public class EmployeeFormViewModel
    {
        public int Id { get; set; }

        // ---------- البيانات الاساسية ----------
        [Display(Name = "اسم الموظف")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "العنوان")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "رقم تليفون")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "رقم التليفون يجب ان يتكون من 11 رقم")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "النوع")]
        public Gender Gender { get; set; }

        [Display(Name = "الجنسية")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Nationality { get; set; } = string.Empty;

        [Display(Name = "تاريخ الميلاد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; } = DateTime.Today;

        [Display(Name = "الرقم القومي")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [RegularExpression(@"^\d{14}$", ErrorMessage = "الرقم القومي يجب ان يتكون من 14 رقم")]
        public string NationalId { get; set; } = string.Empty;

        // ---------- بيانات العمل ----------
        [Display(Name = "تاريخ التعاقد")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ContractDate { get; set; } = DateTime.Today;

        [Display(Name = "الراتب")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "الراتب يجب ان يكون رقم صحيح اكبر من صفر")]
        public decimal Salary { get; set; }

        [Display(Name = "موعد الحضور")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan AttendanceTime { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "موعد الانصراف")]
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan DepartureTime { get; set; } = new TimeSpan(16, 0, 0);

        public List<SelectListItem> GenderOptions { get; set; } = new();
    }
}
