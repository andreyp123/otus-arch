using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Model.UserSvc;
using UserSvc.Repository.Model;

namespace UserSvc.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserDbContext _dbContext;

        public UserRepository(ILogger<UserRepository> logger, UserDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> CreateUserAsync(User user, CancellationToken ct = default)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(user.UserId, nameof(user.UserId));
            Guard.NotNullOrEmpty(user.Username, nameof(user.Username));

            if (await _dbContext.Users.AnyAsync(ue => ue.Username == user.Username))
            {
                throw new EShopException($"Username {user.Username} already exists");
            }

            await _dbContext.Users.AddAsync(MapUserEntity(user), ct);
            await _dbContext.SaveChangesAsync(ct);
            return user.UserId;
        }

        public async Task<(User[], int)> GetUsersAsync(int start, int size, CancellationToken ct = default)
        {
            var total = await _dbContext.Users
                .CountAsync(ct);
            var users = await _dbContext.Users
                .Skip(start)
                .Take(size)
                .Select(ue => MapUser(ue))
                .ToArrayAsync(ct);
            return (users, total);
        }

        public async Task<User> GetUserAsync(string userId, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new EShopException($"User {userId} not found");
            }
            return MapUser(userEntity);
        }

        public async Task<User> GetUserByNameAsync(string username, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.Username == username, ct);
            return MapUser(userEntity);
        }

        public async Task UpdateUserAsync(string userId, User user, bool updateRoles = false, CancellationToken ct = default)
        {
            Guard.NotNull(user, nameof(user));

            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new EShopException($"User {userId} not found");
            }

            if (userEntity.Username != user.Username &&
                await _dbContext.Users.AnyAsync(ue => ue.Username == user.Username && ue.UserId != userId))
            {
                throw new EShopException($"Username {user.Username} already exists");
            }

            var newUserEntity = MapUserEntity(user);
            userEntity.Username = newUserEntity.Username;
            userEntity.Email = newUserEntity.Email;
            if (updateRoles)
            {
                userEntity.Roles = newUserEntity.Roles;
            }
            if (newUserEntity.PasswordHash != null)
            {
                userEntity.PasswordHash = newUserEntity.PasswordHash;
            }

            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new EShopException($"User {userId} not found");
            }
            _dbContext.Users.Remove(userEntity);
            await _dbContext.SaveChangesAsync(ct);
        }

        private static UserEntity MapUserEntity(User u)
        {
            return new UserEntity
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                PasswordHash = u.PasswordHash,
                Roles = u.Roles != null
                    ? string.Join(",", u.Roles)
                    : string.Empty
            };
        }

        private static User MapUser(UserEntity ue)
        {
            return new User
            {
                UserId = ue.UserId,
                Username = ue.Username,
                Email = ue.Email,
                PasswordHash = ue.PasswordHash,
                Roles = !string.IsNullOrWhiteSpace(ue.Roles)
                    ? ue.Roles.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    : new string[0]
            };
        }
    }
}
