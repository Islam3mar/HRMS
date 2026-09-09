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
        public string? TotalOvertimeError { get; set; }
        public string? TotalDeductionError { get; set; }
        public string? NetSalaryError { get; set; }

        public bool HasErrors =>
            NotFoundError != null || TotalOvertimeError != null ||
            TotalDeductionError != null || NetSalaryError != null;
    }
}
