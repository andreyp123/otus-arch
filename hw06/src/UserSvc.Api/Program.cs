using Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using UserSvc.Repository;
using UserSvc.Repository.Model;

namespace UserSvc.Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "migrate")
            {
                return RunMigration(args);
            }

            RunWebHost(args);
            return 0;
        }

        private static int RunMigration(string[] args)
        {
            ILogger<Program> logger = NullLogger<Program>.Instance;
            try
            {
                IHost host = Host
                    .CreateDefaultBuilder(args)
                    .ConfigureServices((_, services) => services.AddUserRepository())
                    .Build();

                logger = host.GetService<ILogger<Program>>();
                logger.LogInformation("Applying migrations (manual run)...");

                using (var context = host.GetService<UserDbContext>())
                {
                    context.Database.Migrate();
                }

                logger.LogInformation("Done");
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return -1;
            }
        }

        private static void RunWebHost(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
            
            // Auto-migrate if it is configured
            if (host.GetService<UserRepositoryConfig>().AutoMigrate)
            {
                var logger = host.GetService<ILogger<Program>>();
                logger.LogInformation("Applying migrations (auto run)...");

                using (var context = host.GetService<UserDbContext>())
                {
                    context.Database.Migrate();
                }

                logger.LogInformation("Done");
            }
            
            host.Run();
        }
    }
}
