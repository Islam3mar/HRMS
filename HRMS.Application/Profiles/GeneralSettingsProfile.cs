using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Profiles
{
    public class GeneralSettingsProfile : Profile
    {
        public GeneralSettingsProfile()
        {
            CreateMap<GeneralSettingsInput, GeneralSettings>()
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore());
        }
    }
}
