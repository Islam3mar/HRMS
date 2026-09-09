using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Attendance
{
    // بتلف نفس شرط الـ Where اللي كان جوه BuildFilteredQuery بالظبط
    public class AttendanceSearchSpecification : BaseSpecification<AttendanceRecord>
    {
        public AttendanceSearchSpecification(AttendanceSearchFilter filter)
            : base(BuildCriteria(filter))
        {
            // بديل الـ Include(a => a.Employee).ThenInclude(e => e.Department)
            AddInclude("Employee.Department");
        }

        private static Expression<Func<AttendanceRecord, bool>> BuildCriteria(AttendanceSearchFilter filter)
        {
            return a =>
                (string.IsNullOrWhiteSpace(filter.EmployeeName) ||
                    a.Employee.FullName.Contains(filter.EmployeeName) ||
                    (a.Employee.Department != null && a.Employee.Department.Name.Contains(filter.EmployeeName))) &&
                (!filter.DepartmentId.HasValue || a.Employee.DepartmentId == filter.DepartmentId.Value) &&
                (!filter.FromDate.HasValue || a.Date.Date >= filter.FromDate.Value.Date) &&
                (!filter.ToDate.HasValue || a.Date.Date <= filter.ToDate.Value.Date);
        }
    }
}
