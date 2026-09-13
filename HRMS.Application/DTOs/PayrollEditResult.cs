using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class PayrollEditResult
    {
        public bool Success { get; set; }
        public PayrollRowDto? Row { get; set; }

        // القيم قبل التعديل (عشان نعرض "اتغيرت من كذا لكذا")
        public decimal PreviousTotalOvertimeAmount { get; set; }
        public decimal PreviousTotalDeductionAmount { get; set; }
        public decimal PreviousNetSalary { get; set; }

        public string? NotFoundError { get; set; }
        public string? AdditionRateError { get; set; }
        public string? DeductionRateError { get; set; }
        public string? NetSalaryError { get; set; }   // بتتظهر لو الصافى الناتج بعد الحساب طلع صفر او سالب

        public bool HasErrors =>
            NotFoundError != null || AdditionRateError != null ||
            DeductionRateError != null || NetSalaryError != null;
    }
}
