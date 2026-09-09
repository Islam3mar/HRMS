using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    // بيانات التعديل اليدوي على راتب معتمد (زرار "تعديل" بجانب الطباعة)
    public class PayrollManualEditInput
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal TotalOvertimeAmount { get; set; }   // اجمالى الاضافى
        public decimal TotalDeductionAmount { get; set; }  // اجمالى الخصم
        public decimal NetSalary { get; set; }              // الصافى
    }
}
