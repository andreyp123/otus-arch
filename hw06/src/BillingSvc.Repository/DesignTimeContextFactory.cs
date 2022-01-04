using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using BillingSvc.Repository.Model;

namespace BillingSvc.Repository
{
    internal sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
    {
        public AccountDbContext CreateDbContext(string[] args)
        {
            const string connectionStringKey = "connectionString";

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var connectionString = configuration.AsEnumerable().Any(kv => kv.Key == connectionStringKey)
                ? configuration[connectionStringKey]
                : "none";

            var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AccountDbContext(optionsBuilder.Options);
        }
    }
}
