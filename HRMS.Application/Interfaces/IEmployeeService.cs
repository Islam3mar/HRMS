using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();   // جديد
        Task<Employee?> GetEmployeeByIdAsync(int id);

        Task<EmployeeResult> CreateEmployeeAsync(EmployeeInput input);
        Task<EmployeeResult> UpdateEmployeeAsync(int id, EmployeeInput input);
        Task<(bool Success, string? Error, bool Deactivated)> DeleteEmployeeAsync(int id);   // اتغير الـ return type

        Task<PagedResult<Employee>> GetPagedEmployeesAsync(int page, int pageSize);

        Task<EmployeeResult> ReactivateEmployeeAsync(int id, EmployeeInput input);
    }
}
