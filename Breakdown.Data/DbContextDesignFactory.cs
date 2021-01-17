using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Breakdown.Data
{
    public class DbContextDesignFactory : IDesignTimeDbContextFactory<BreakdownContext>
    {
        public BreakdownContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT")}.json")
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine(Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT"));

            var optionsBuilder = new DbContextOptionsBuilder<BreakdownContext>();
            optionsBuilder.UseSqlite(config.GetConnectionString("Breakdown"));
            return new BreakdownContext(optionsBuilder.Options);
        }
    }
}
