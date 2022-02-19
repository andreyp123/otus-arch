using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Linq;
using RentSvc.Dal.Model;

namespace RentSvc.Dal
{
    internal sealed class RentDesignTimeContextFactory : IDesignTimeDbContextFactory<RentDbContext>
    {
        public RentDbContext CreateDbContext(string[] args)
        {
            const string connectionStringKey = "connectionString";

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var connectionString = configuration.AsEnumerable().Any(kv => kv.Key == connectionStringKey)
                ? configuration[connectionStringKey]
                : "none";

            var optionsBuilder = new DbContextOptionsBuilder<RentDbContext>();
            optionsBuilder.SetUpRentDbContext(connectionString);

            return new RentDbContext(optionsBuilder.Options);
        }
    }
}
