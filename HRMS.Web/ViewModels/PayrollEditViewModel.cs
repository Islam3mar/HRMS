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
        public decimal HourlyRate { get; set; }        // سعر الساعة العادى المحسوب - للعرض والمعاينة (Preview) بس
        public decimal CurrentNetSalary { get; set; }   // الصافى الحالى قبل التعديل - للعرض بس

        // الحقول القابلة للتعديل اليدوي - بقوا نسبة % بدل مبلغ ثابت (زي الاعدادات العامة بالظبط)
        [Display(Name = "نسبة الاضافة (%)")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(typeof(decimal), "0", "500", ErrorMessage = "من فضلك ادخل نسبة صحيحة اكبر من او تساوى صفر")]
        public decimal AdditionRatePercentage { get; set; }

        [Display(Name = "نسبة الخصم (%)")]
        [Required(ErrorMessage = "من فضلك ادخل بيانات الحقل")]
        [Range(typeof(decimal), "0", "300", ErrorMessage = "من فضلك ادخل نسبة صحيحة اكبر من او تساوى صفر")]
        public decimal DeductionRatePercentage { get; set; }

        public decimal AbsenceDeductionAmount { get; set; }
    }
}
