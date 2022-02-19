using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Linq;
using UserSvc.Dal.Model;

namespace UserSvc.Dal;

internal sealed class UserDesignTimeContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        const string connectionStringKey = "connectionString";

        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        var connectionString = configuration.AsEnumerable().Any(kv => kv.Key == connectionStringKey)
            ? configuration[connectionStringKey]
            : "none";

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.SetUpUserDbContext(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}