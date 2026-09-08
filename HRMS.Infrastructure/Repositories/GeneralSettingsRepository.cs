using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class GeneralSettingsRepository : BaseRepository<GeneralSettings>, IGeneralSettingsRepository
    {
        public GeneralSettingsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<GeneralSettings?> GetSingleAsync()
            => await Query.FirstOrDefaultAsync();
    }
}
