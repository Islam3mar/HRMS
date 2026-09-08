using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
