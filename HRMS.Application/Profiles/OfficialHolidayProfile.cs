using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Profiles
{
    public class OfficialHolidayProfile : Profile
    {
        public OfficialHolidayProfile()
        {
            CreateMap<OfficialHolidayInput, OfficialHoliday>()
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());
        }
    }
}
