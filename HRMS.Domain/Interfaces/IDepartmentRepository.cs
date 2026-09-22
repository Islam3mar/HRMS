using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<bool> NameExistsAsync(string name, int? excludeDepartmentId = null);
        Task<bool> HasActiveEmployeesAsync(int departmentId);   // اتغير الاسم عشان يوضح إنه بيفحص النشطين بس
    }
}
