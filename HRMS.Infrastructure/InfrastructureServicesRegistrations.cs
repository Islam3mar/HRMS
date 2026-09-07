using HRMS.Domain.Interfaces;
using HRMS.Domain.Repositories.Classes;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Seeding;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure
{
    public static class InfrastructureServicesRegistrations
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Unit of Work (وده بيغني عن تسجيل كل Repository لوحده)
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Cookie Authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
     {
         options.LoginPath = "/Account/Login";
         options.AccessDeniedPath = "/Account/AccessDenied";
         options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
         options.SlidingExpiration = true;   // كل ما المستخدم نشط، المدة بتتجدد تلقائي
     });

            services.AddKeyedScoped<IDataSeeder, IdentitySeeder>("Identity");

            return services;
        }
    }
}