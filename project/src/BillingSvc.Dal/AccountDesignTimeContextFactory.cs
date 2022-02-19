using BillingSvc.Dal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BillingSvc.Dal
{
    internal sealed class AccountDesignTimeContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
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
            optionsBuilder.SetUpAccountDbContext(connectionString);

            return new AccountDbContext(optionsBuilder.Options);
        }
    }
}
