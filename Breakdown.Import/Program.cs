using AutoMapper;
using Breakdown.Data;
using Breakdown.Import.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Breakdown.Import
{
    public class Program
    {
        static async Task Main()
        {
            var financePath = @"<Import path>";
            using (var scope = GetProvider().CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                var reader = scope.ServiceProvider.GetService<ITransactionsReader>();
                var importer = scope.ServiceProvider.GetService<Importer>();

                var users = Directory.EnumerateDirectories(financePath);

                foreach (var userName in users)
                {
                    logger.LogInformation("User {user}", userName);

                    foreach (var file in GetFiles(Path.Combine(financePath, userName)))
                    {
                        logger.LogInformation("Starting import from {file}", file);
                        try
                        {
                            await importer.Import(reader.GetTransactions(file), userName);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "failed to read file {fileName}", Path.GetFileName(file));
                        }
                    }
                }
                logger.LogInformation("Completed");
                Console.ReadKey();
            }
        }

        private static IServiceProvider GetProvider()
        {
            var services = new ServiceCollection();
            services.AddDbContext<BreakdownContext>();
            services.AddLogging(config =>
            {
                config.ClearProviders();
                config.AddConsole();
                config.AddDebug();
            });
            services.AddTransient<ITransactionsReader, FileReader>();
            services.AddTransient<Importer>();
            services.AddAutoMapper(typeof(Program));
            return services.BuildServiceProvider();
        }

        private static IEnumerable<string> GetFiles(string folder)
        {
            if (Directory.Exists(folder))
            {
                return Directory.EnumerateFiles(folder, "*.csv");
            }
            else return new List<string>();
        }

        
    }
}
