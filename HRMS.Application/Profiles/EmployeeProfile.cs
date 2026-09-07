using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<EmployeeInput, Employee>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName.Trim()))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.Trim()))
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber.Trim()))
                .ForMember(d => d.Nationality, o => o.MapFrom(s => s.Nationality.Trim()))
                .ForMember(d => d.NationalId, o => o.MapFrom(s => s.NationalId.Trim()))
                // CreatedAt و UpdatedAt بيتحطوا يدويًا في الـ Service (مش جزء من بيانات الـ Input)
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());
        }
    }
}