using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IRoleService
    {
        Task<RoleResult> CreateRoleAsync(string name, List<PermissionInput> permissions);
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}
