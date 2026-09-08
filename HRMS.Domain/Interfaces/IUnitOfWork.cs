using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IEmployeeRepository Employees { get; }
        IRoleRepository Roles { get; }
        IGeneralSettingsRepository GeneralSettings { get; }
        IOfficialHolidayRepository OfficialHolidays { get; }

        Task<int> SaveChangesAsync();
    }
}
