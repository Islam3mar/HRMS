using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<bool> NameExistsAsync(string name, int? excludeRoleId = null);
    }
}
