using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using CarSvc.Dal.Model;

namespace CarSvc.Dal;

internal sealed class CarDesignTimeContextFactory : IDesignTimeDbContextFactory<CarDbContext>
{
    public CarDbContext CreateDbContext(string[] args)
    {
        const string connectionStringKey = "connectionString";

        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        var connectionString = configuration.AsEnumerable().Any(kv => kv.Key == connectionStringKey)
            ? configuration[connectionStringKey]
            : "none";

        var optionsBuilder = new DbContextOptionsBuilder<CarDbContext>();
        optionsBuilder.SetUpCarDbContext(connectionString);

        return new CarDbContext(optionsBuilder.Options);
    }
}