using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Employees
{
    public class EmployeesByDepartmentSpecification : BaseSpecification<Employee>
    {
        public EmployeesByDepartmentSpecification(int departmentId)
            : base(e => e.DepartmentId == departmentId)
        {
            AddInclude(e => e.Department);
        }
    }
}
