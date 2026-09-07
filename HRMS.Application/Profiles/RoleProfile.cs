using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            // أسماء الحقول متطابقة تمامًا (SystemPage, CanView, CanAdd, CanEdit, CanDelete)
            // فمش محتاجين ForMember هنا، AutoMapper هيعملها Convention-based
            CreateMap<PermissionInput, RolePermission>();
        }
    }
}