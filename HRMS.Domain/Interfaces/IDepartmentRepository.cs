using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<bool> NameExistsAsync(string name, int? excludeDepartmentId = null);
        Task<bool> HasEmployeesAsync(int departmentId);
    }
}
