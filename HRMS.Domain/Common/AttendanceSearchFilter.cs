using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Common
{
    // فلاتر شاشة تقرير الحضور و الانصراف (بحث باسم الموظف / القسم / فترة تاريخ) + الصفحة المطلوبة
    public class AttendanceSearchFilter
    {
        // فلترة دقيقة بكود موظف واحد بالظبط (لعرض راتب موظف واحد) - مختلفة عن EmployeeName
        // اللي بتعمل بحث تقريبى (Contains) وممكن تطابق اكتر من موظف
        public int? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
