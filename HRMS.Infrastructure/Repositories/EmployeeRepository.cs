using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context) { }

        // GetByIdAsync العادية بترجع من غير Include للـ Department
        // فبنعمل Override بسيطة هنا عشان نضيف الـ Include المطلوب
        public override async Task<Employee?> GetByIdAsync(int id)
        {
            return await Query
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await Query
                .Include(e => e.Department)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchByNameAsync(string name)
        {
            return await Query
                .Include(e => e.Department)
                .AsNoTracking()
                .Where(e => e.FullName.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        {
            return await Query
                .Include(e => e.Department)
                .AsNoTracking()
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeEmployeeId = null)
        {
            return await Query
                .AnyAsync(e => e.NationalId == nationalId &&
                               (excludeEmployeeId == null || e.Id != excludeEmployeeId));
        }
    }
}
