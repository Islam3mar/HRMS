using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        // الخاصة بالموظفين بس
        Task<IEnumerable<Employee>> SearchByNameAsync(string name);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
        Task<bool> NationalIdExistsAsync(string nationalId, int? excludeEmployeeId = null);
    }
}
