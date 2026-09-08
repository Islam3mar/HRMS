using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<DepartmentResult> CreateAsync(DepartmentInput input);
        Task<DepartmentResult> UpdateAsync(int id, DepartmentInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
