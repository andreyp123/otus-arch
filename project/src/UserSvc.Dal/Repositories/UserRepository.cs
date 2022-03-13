using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Model.UserSvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserSvc.Dal.Model;

namespace UserSvc.Dal.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly IDbContextFactory<UserDbContext> _dbContextFactory;

    public UserRepository(ILogger<UserRepository> logger, IDbContextFactory<UserDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken ct = default)
    {
        Guard.NotNull(user, nameof(user));
        Guard.NotNullOrEmpty(user.UserId, nameof(user.UserId));
        Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
        
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

        if (await dbContext.Users.AnyAsync(ue => ue.Username == user.Username, ct))
        {
            throw new CrashException($"Username {user.Username} already exists");
        }

        var now = DateTime.UtcNow;
        var userEntity = MapUserEntity(user);
        userEntity.CreatedDate = now;
        userEntity.ModifiedDate = now;

        await dbContext.Users.AddAsync(userEntity, ct);
        await dbContext.SaveChangesAsync(ct);
        
        return MapUser(userEntity);
    }

    public async Task<(User[], int)> GetUsersAsync(int start, int size, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var total = await dbContext.Users
            .CountAsync(ct);
        var users = await dbContext.Users
            .Skip(start)
            .Take(size)
            .Select(ue => MapUser(ue))
            .ToArrayAsync(ct);
        return (users, total);
    }

    public async Task<User> GetUserAsync(string userId, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
        if (userEntity == null)
        {
            throw new CrashException($"User {userId} not found");
        }

        return MapUser(userEntity);
    }

    public async Task<User> GetUserByNameAsync(string username, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.Username == username, ct);
        return MapUser(userEntity);
    }

    public async Task<User> UpdateUserAsync(string userId, User user, bool selfUpdate = true, CancellationToken ct = default)
    {
        Guard.NotNull(user, nameof(user));

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
        if (userEntity == null)
        {
            throw new CrashException($"User {userId} not found");
        }

        if (userEntity.Username != user.Username &&
            await dbContext.Users.AnyAsync(ue => ue.Username == user.Username && ue.UserId != userId, ct))
        {
            throw new CrashException($"Username {user.Username} already exists");
        }

        var newUserEntity = MapUserEntity(user);
        userEntity.Username = newUserEntity.Username;
        userEntity.FullName = newUserEntity.FullName;
        userEntity.Email = newUserEntity.Email;
        userEntity.PhoneNumber = newUserEntity.PhoneNumber;
        userEntity.DriverLicense = newUserEntity.DriverLicense;
        if (newUserEntity.PasswordHash != null)
        {
            userEntity.PasswordHash = newUserEntity.PasswordHash;
        }

        if (!selfUpdate) // update by admin
        {
            userEntity.Verified = newUserEntity.Verified;
            userEntity.Roles = newUserEntity.Roles;
        }
        
        userEntity.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return MapUser(userEntity);
    }

    public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
        if (userEntity == null)
        {
            throw new CrashException($"User {userId} not found");
        }

        dbContext.Users.Remove(userEntity);
        await dbContext.SaveChangesAsync(ct);
    }

    private static UserEntity MapUserEntity(User u)
    {
        return new UserEntity
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            DriverLicense = u.DriverLicense,
            Verified = u.Verified,
            PasswordHash = u.PasswordHash,
            Roles = u.Roles != null
                ? string.Join(",", u.Roles)
                : string.Empty
        };
    }

    private static User MapUser(UserEntity ue)
    {
        return ue == null
            ? null
            : new User
            {
                UserId = ue.UserId,
                Username = ue.Username,
                FullName = ue.FullName,
                Email = ue.Email,
                PhoneNumber = ue.PhoneNumber,
                DriverLicense = ue.DriverLicense,
                Verified = ue.Verified,
                PasswordHash = ue.PasswordHash,
                Roles = !string.IsNullOrWhiteSpace(ue.Roles)
                    ? ue.Roles.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>(),
                CreatedDate = ue.CreatedDate,
                ModifiedDate = ue.ModifiedDate
            };
    }
}