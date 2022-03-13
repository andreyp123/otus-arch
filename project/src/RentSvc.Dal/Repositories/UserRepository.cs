using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Model.RentSvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Model;

namespace RentSvc.Dal.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ILogger<UserRepository> _logger;
    private readonly IDbContextFactory<RentDbContext> _dbContextFactory;

    public UserRepository(ILogger<UserRepository> logger, IDbContextFactory<RentDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
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

    public async Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == user.UserId, ct);
        if (userEntity == null)
        {
            // update existing user
            await dbContext.Users.AddAsync(MapUserEntity(user), ct);
            await dbContext.SaveChangesAsync(ct);
        }
        else if (userEntity.DeletedDate == null && userEntity.ModifiedDate < user.ModifiedDate)
        {
            // update existing user if newer
            userEntity.Username = user.Username;
            userEntity.FullName = user.FullName;
            userEntity.Email = user.Email;
            userEntity.PhoneNumber = user.PhoneNumber;
            userEntity.DriverLicense = user.DriverLicense;
            userEntity.Verified = user.Verified;
            userEntity.ModifiedDate = user.ModifiedDate;
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteUserAsync(string userId, DateTime deletedDate, CancellationToken ct = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        var userEntity = await dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
        if (userEntity != null && userEntity.DeletedDate == null)
        {
            userEntity.DeletedDate = deletedDate;
            await dbContext.SaveChangesAsync(ct);
        }
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
            ModifiedDate = u.ModifiedDate,
            DeletedDate = u.DeletedDate
        };
    }

    private static User MapUser(UserEntity ue)
    {
        return new User
        {
            UserId = ue.UserId,
            Username = ue.Username,
            FullName = ue.FullName,
            Email = ue.Email,
            PhoneNumber = ue.PhoneNumber,
            DriverLicense = ue.DriverLicense,
            Verified = ue.Verified,
            ModifiedDate = ue.ModifiedDate,
            DeletedDate = ue.DeletedDate
        };
    }
}