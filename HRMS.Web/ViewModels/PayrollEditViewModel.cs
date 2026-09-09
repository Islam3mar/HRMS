using System.ComponentModel.DataAnnotations;

namespace HRMS.Web.ViewModels
{
    public class PayrollEditViewModel
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // للعرض فقط، مش قابلة للتعديل
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int AttendanceDaysCount { get; set; }
        public int AbsenceDaysCount { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal DeductionHours { get; set; }

        // الحقول القابلة للتعديل اليدوي
        [Display(Name = "اجمالى الاضافى")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0, double.MaxValue, ErrorMessage = "من فضلك ادخل قيمة صحيحة اكبر من او تساوى صفر")]
        public decimal TotalOvertimeAmount { get; set; }

        [Display(Name = "اجمالى الخصم")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0, double.MaxValue, ErrorMessage = "من فضلك ادخل قيمة صحيحة اكبر من او تساوى صفر")]
        public decimal TotalDeductionAmount { get; set; }

        [Display(Name = "الصافى")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(0.01, double.MaxValue, ErrorMessage = "الصافى لا يمكن ان يكون صفر او اقل")]
        public decimal NetSalary { get; set; }
    }
}
