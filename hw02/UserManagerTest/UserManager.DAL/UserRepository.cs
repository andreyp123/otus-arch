using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UserManager.Common;
using UserManager.DAL.Model;

namespace UserManager.DAL
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
            string newUserId = Guid.NewGuid().ToString();
            await _dbContext.Users.AddAsync(Map2Entity(user, newUserId), ct);
            await _dbContext.SaveChangesAsync(ct);
            return newUserId;
        }

        public async Task<User> GetUserAsync(string userId, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            return Map2User(userEntity);
        }

        public async Task UpdateUserAsync(string userId, User user, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new UserManagerException($"User {userId} not found");
            }
            userEntity.Username = user.Username;
            userEntity.Email = user.Email;
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new UserManagerException($"User {userId} not found");
            }
            _dbContext.Users.Remove(userEntity);
            await _dbContext.SaveChangesAsync(ct);
        }

        private User Map2User(UserEntity entity)
        {
            return entity != null
                ? new User { UserId = entity.UserId, Username = entity.Username, Email = entity.Email }
                : null;
        }

        private UserEntity Map2Entity(User user, string userId = null)
        {
            return user != null
                ? new UserEntity { UserId = userId ?? user.UserId, Username = user.Username, Email = user.Email }
                : null;
        }
    }
}
