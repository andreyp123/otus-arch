using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using UserManager.Repository.Model;

namespace UserManager.Repository.Migration
{
    internal class Program
    {
        private static IHost _host;
        private static ILogger<Program> _logger = null;
        private static ILogger<Program> Logger => _logger ?? (_logger = _host.GetService<ILogger<Program>>());

        internal static int Main(string[] args)
        {
            try
            {
                _host = Host
                    .CreateDefaultBuilder(args)
                    .ConfigureServices((_, services) => services.AddUserRepository())
                    .Build();

                Logger.LogInformation("Applying migrations...");

                using (var context = _host.GetService<UserDbContext>())
                {
                    context.Database.Migrate();
                }

                Logger.LogInformation("Done");
                return 0;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error");
                return -1;
            }
        }
    }

    internal static class HostExtensions
    {
        internal static ServiceT GetService<ServiceT>(this IHost host)
        {
            return (ServiceT)host.Services.GetService(typeof(ServiceT));
        }
    }
}
