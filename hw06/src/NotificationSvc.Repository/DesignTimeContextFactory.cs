using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NotificationSvc.Repository.Model;

namespace NotificationSvc.Repository
{
    internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            const string connectionStringKey = "connectionString";

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var connectionString = configuration.AsEnumerable().Any(kv => kv.Key == connectionStringKey)
                ? configuration[connectionStringKey]
                : "none";

            var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new NotificationDbContext(optionsBuilder.Options);
        }
    }
}
