using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IGeneralSettingsRepository : IGenericRepository<GeneralSettings>
    {
        Task<GeneralSettings?> GetSingleAsync();
    }
}
