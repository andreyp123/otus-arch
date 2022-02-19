using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NotificationSvc.Dal.Model;

namespace NotificationSvc.Dal
{
    internal sealed class NotificationDesignTimeContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
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
            optionsBuilder.SetUpNotificationDbContext(connectionString);

            return new NotificationDbContext(optionsBuilder.Options);
        }
    }
}
