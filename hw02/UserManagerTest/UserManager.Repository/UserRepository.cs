using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UserManager.Common;
using UserManager.Repository.Model;

namespace UserManager.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserRepository(ILogger<UserRepository> logger, UserDbContext dbContext, IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<string> CreateUserAsync(User user, CancellationToken ct = default)
        {
            Guard.NotNull(user, nameof(user));

            user.UserId = Guid.NewGuid().ToString();
            await _dbContext.Users.AddAsync(_mapper.Map<UserEntity>(user), ct);
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
                .Select(ue => _mapper.Map<User>(ue))
                .ToArrayAsync(ct);
            return (users, total);
        }

        public async Task<User> GetUserAsync(string userId, CancellationToken ct = default)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(ue => ue.UserId == userId, ct);
            if (userEntity == null)
            {
                throw new UserManagerException($"User {userId} not found");
            }
            return _mapper.Map<User>(userEntity);
        }

        public async Task UpdateUserAsync(string userId, User user, CancellationToken ct = default)
        {
            Guard.NotNull(user, nameof(user));

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
    }
}
