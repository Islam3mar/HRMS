using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;
using HRMS.Domain.Specifications.Roles;

namespace HRMS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    .Select(p => _mapper.Map<RolePermission>(p))
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

        public async Task<PagedResult<Role>> GetPagedRolesAsync(int page, int pageSize)
        {
            var spec = new RolesPagedSpecification(page, pageSize);
            var items = await _unitOfWork.Roles.ListAsync(spec);
            var total = await _unitOfWork.Roles.CountAsync(spec);

            return new PagedResult<Role> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }
    }
}