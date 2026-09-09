using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    // صف واحد فى تقرير رواتب الموظفين (المطلوب الثامن)
    public class PayrollRowDto
    {
        // بيانات الموظف
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }

        // بيانات الحضور و الانصراف خلال الشهر المحدد
        public int AttendanceDaysCount { get; set; }   // عدد ايام الحضور
        public int AbsenceDaysCount { get; set; }      // عدد ايام الغياب

        public decimal OvertimeHours { get; set; }     // الاضافى بالساعات
        public decimal DeductionHours { get; set; }    // الخصم بالساعات

        public decimal TotalOvertimeAmount { get; set; } // اجمالى الاضافى
        public decimal TotalDeductionAmount { get; set; } // اجمالى الخصم

        public decimal NetSalary { get; set; }          // الصافى

        public int Month { get; set; }
        public int Year { get; set; }

        // هل الراتب ده اتحفظ كسجل ثابت (Snapshot) بالفعل، ولا لسه محسوب لحظيًا؟
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
