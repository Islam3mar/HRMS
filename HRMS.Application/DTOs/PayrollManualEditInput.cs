using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    // بيانات التعديل اليدوي على راتب معتمد (زرار "تعديل" بجانب الطباعة)
    // الاضافة والخصم بقوا نسبة % بدل مبلغ ثابت، بنفس منطق الاعدادات العامة
    public class PayrollManualEditInput
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public decimal AdditionRatePercentage { get; set; }    // نسبة الاضافة %
        public decimal DeductionRatePercentage { get; set; }   // نسبة الخصم %
    }
}
