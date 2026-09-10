using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Role?> GetByIdAsync(int id)
        {
            return await Query
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await Query
                .Include(r => r.Permissions)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeRoleId = null)
        {
            return await Query
                .AnyAsync(r => r.Name == name &&
                               (excludeRoleId == null || r.Id != excludeRoleId));
        }
    }
}
