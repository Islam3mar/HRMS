using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;   // ← جديد، ضروري عشان IConfiguration
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
        private readonly IConfiguration _configuration;   // ← جديد

        public IdentitySeeder(ApplicationDbContext context, IConfiguration configuration)   // ← ضفنا الباراميتر هنا
        {
            _context = context;
            _configuration = configuration;   // ← جديد
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.AnyAsync(u => u.Username == "admin"))
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
                    new() { SystemPage = SystemPage.RolesManagement, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                    new() { SystemPage = SystemPage.Departments, CanView = true, CanAdd = true, CanEdit = true, CanDelete = true },
                }
            };

            // القيم دلوقتي بتتقرا من User Secrets / appsettings بدل ما تكون مكتوبة هنا صريح
            var adminUser = new User
            {
                FullName = _configuration["SeedAdmin:FullName"] ?? "HR Admin",
                Username = _configuration["SeedAdmin:Username"] ?? "admin",
                Email = _configuration["SeedAdmin:Email"] ?? "admin@localhost",
                PasswordHash = PasswordHasher.Hash(_configuration["SeedAdmin:Password"] ?? "ChangeMe@123"),
                Role = adminRole
            };

            await _context.Roles.AddAsync(adminRole);
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();
        }
    }
}