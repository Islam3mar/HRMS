using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Employees
{
    public class EmployeesPagedSpecification : BaseSpecification<Employee>
    {
        public EmployeesPagedSpecification(int page, int pageSize)
        {
            AddInclude(e => e.Department);
            ApplyOrderBy(e => e.FullName);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
