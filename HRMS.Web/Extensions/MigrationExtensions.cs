using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Web.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task MigrationAndSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var identitySeeder = services.GetRequiredKeyedService<IDataSeeder>("Identity");
            await identitySeeder.SeedAsync();
        }
    }
}
