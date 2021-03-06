﻿using AutoMapper;
using Breakdown.Data;
using Breakdown.Import.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT", EnvironmentVariableTarget.User)}.json")
               .AddEnvironmentVariables()
               .Build();

            using var scope = GetProvider(config).CreateScope();

            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
            var reader = scope.ServiceProvider.GetService<ITransactionsReader>();
            var importer = scope.ServiceProvider.GetService<Importer>();

            var financePath = config.GetSection("DataPath").Value;
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

        private static IServiceProvider GetProvider(IConfiguration config)
        {
            var cstring = config.GetConnectionString("Breakdown");
            var services = new ServiceCollection();
            services.AddDbContext<BreakdownContext>(c => c.UseSqlite(cstring));
            services.AddLogging(c =>
            {
                c.AddConfiguration(config.GetSection("Logging"));
                c.ClearProviders();
                c.AddConsole();
                c.AddDebug();
            });
            
            services.AddTransient<ITransactionsReader, FileReader>();
            services.AddTransient<Importer>();
            services.AddTransient<Helpers.TransactionConverter>();
            services.AddTransient<CategoryService>();
            services.AddAutoMapper(typeof(Program));
            return services.BuildServiceProvider();
        }

        private static IEnumerable<string> GetFiles(string folder)
        {
            if (Directory.Exists(folder))
            {
                return Directory.EnumerateFiles(folder, "*import.csv");
            }
            else return new List<string>();
        }

        
    }
}
