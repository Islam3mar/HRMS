using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RoleResult> CreateRoleAsync(string name, List<PermissionInput> permissions)
        {
            var result = new RoleResult();

            if (string.IsNullOrWhiteSpace(name))
                result.NameError = "من فضلك ادخل اسم المجموعة";

            bool hasAnyPermission = permissions?.Any(p => p.HasAnyPermission) == true;
            if (!hasAnyPermission)
                result.PermissionsError = "من فضلك قم بتحديد صلاحيات المجموعة قبل الاضافة";

            if (result.HasErrors) return result;

            var role = new Role
            {
                Name = name.Trim(),
                Permissions = permissions!
                    .Where(p => p.HasAnyPermission)
                    .Select(p => new RolePermission
                    {
                        SystemPage = p.SystemPage,
                        CanView = p.CanView,
                        CanAdd = p.CanAdd,
                        CanEdit = p.CanEdit,
                        CanDelete = p.CanDelete
                    })
                    .ToList()
            };

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Role = role;
            return result;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _unitOfWork.Roles.GetAllAsync();
        }
    }
}
