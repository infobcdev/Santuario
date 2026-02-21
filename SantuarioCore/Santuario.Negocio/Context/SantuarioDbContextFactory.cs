using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Santuario.Negocio.Context
{
    public class SantuarioDbContextFactory : IDesignTimeDbContextFactory<SantuarioDbContext>
    {
        public SantuarioDbContext CreateDbContext(string[] args)
        {
            // Lê appsettings do Admin
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Santuario.Admin");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<SantuarioDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            return new SantuarioDbContext(optionsBuilder.Options);
        }
    }
}