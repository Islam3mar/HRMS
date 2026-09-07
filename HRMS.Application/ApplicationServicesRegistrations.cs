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

            // هنضيف هنا أي Service جديد في الـ Application Layer لما نوصله
            // services.AddScoped<IEmployeeService, EmployeeService>();
            // services.AddScoped<IRoleService, RoleService>();
            // services.AddScoped<IPayrollService, PayrollService>();

            return services;
        }
    }
}