using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Common
{
    // مجموعة سجلات الحضور الخاصة بموظف واحد - وحدة العرض الجديدة فى شاشة الحضور (Group by Employee)
    public class EmployeeAttendanceGroup
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = "-";
        public int RecordsCount { get; set; }
        public List<AttendanceRecord> Records { get; set; } = new();
    }
}
