using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Common
{
    // فلاتر شاشة تقرير الحضور و الانصراف (بحث باسم الموظف / القسم / فترة تاريخ) + الصفحة المطلوبة
    public class AttendanceSearchFilter
    {
        public string? EmployeeName { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
