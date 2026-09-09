using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    // سجل راتب ثابت (Snapshot) لموظف معين عن شهر/سنة معينة.
    // بيتسجل لحظة الاعتماد او الطباعة، وبعد كده مبيتأثرش حتى لو اتعدلت
    // بيانات الحضور او الراتب الاساسى بتاعة الموظف بعد التاريخ ده.
    public class PayrollRecord : BaseEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int Month { get; set; }
        public int Year { get; set; }

        // نفس القيم المحسوبة وقت الاعتماد - محفوظة كـ Snapshot
        public decimal BaseSalary { get; set; }

        public int AttendanceDaysCount { get; set; }
        public int AbsenceDaysCount { get; set; }

        public decimal OvertimeHours { get; set; }
        public decimal DeductionHours { get; set; }

        public decimal TotalOvertimeAmount { get; set; }
        public decimal TotalDeductionAmount { get; set; }

        public decimal NetSalary { get; set; }

        public DateTime ApprovedAt { get; set; }
    }
}
