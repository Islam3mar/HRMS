using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Specifications.Employees
{
    public class EmployeesByNameSpecification : BaseSpecification<Employee>
    {
        public EmployeesByNameSpecification(string name)
            : base(e => e.FullName.Contains(name))
        {
            AddInclude(e => e.Department);
        }
    }
}
