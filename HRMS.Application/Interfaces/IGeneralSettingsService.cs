using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IGeneralSettingsService
    {
        Task<GeneralSettings?> GetSettingsAsync();
        Task<GeneralSettingsResult> SaveSettingsAsync(GeneralSettingsInput input);
    }
}
