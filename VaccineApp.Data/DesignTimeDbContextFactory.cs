using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VaccineApp.Data.Context;

namespace WebAPI
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = config.GetConnectionString("PostgresConnection");

            builder.UseNpgsql(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}
