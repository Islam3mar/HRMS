using AutoMapper;
using HRMS.Application.Interfaces;
using HRMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Application
{
    public static class ApplicationServicesRegistrations
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IGeneralSettingsService, GeneralSettingsService>();
            services.AddScoped<IOfficialHolidayService, OfficialHolidayService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IAttendanceRecordService, AttendanceRecordService>();

            // AutoMapper 13+ بيشتغل بس بالـ Action overload، فبنقوله يدوّر
            // على كل الـ Profiles الموجودة في نفس الـ Assembly بتاع HRMS.Application
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ApplicationServicesRegistrations).Assembly);
            });

            return services;
        }
    }
}