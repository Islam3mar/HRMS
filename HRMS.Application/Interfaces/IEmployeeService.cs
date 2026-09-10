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
        Task<Employee?> GetEmployeeByIdAsync(int id);

        Task<EmployeeResult> CreateEmployeeAsync(EmployeeInput input);
        Task<EmployeeResult> UpdateEmployeeAsync(int id, EmployeeInput input);
        Task<bool> DeleteEmployeeAsync(int id);

        Task<PagedResult<Employee>> GetPagedEmployeesAsync(int page, int pageSize);
    }
}
