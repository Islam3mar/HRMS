using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using HRMS.Application.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Seeding
{
    public class IdentitySeeder : IDataSeeder
    {
        private readonly ApplicationDbContext _context;

        public IdentitySeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync())
                return;

            var adminRole = new Role
            {
                Name = "HR Admin",
                Permissions = new List<RolePermission>
                {
                    new() { SystemPage = SystemPage.Employees, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.GeneralSettings, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.AttendanceReport, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.PayrollReport, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.UsersManagement, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.OfficialHolidays, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                }
            };

            var adminUser = new User
            {
                FullName = "Islam Omar",
                Username = "admin",
                Email = "admin@pioneers-solutions.com",
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                Role = adminRole
            };

            await _context.Roles.AddAsync(adminRole);
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();
        }
    }
}
