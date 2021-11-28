using Microsoft.Extensions.Logging;
using UserManager.Common;
using UserManager.Common.Helpers;
using UserManager.Common.Model;
using UserManager.Common.Model.API;
using UserManager.Repository;
using UserManager.TokenManager;

namespace UserManager.BusinessLogic
{
    public class BusinessLogic : IBusinessLogic
    {
        private readonly ILogger<BusinessLogic> _logger;
        private readonly ITokenManager _tokenManager;
        private readonly IUserRepository _repository;

        public BusinessLogic(ILogger<BusinessLogic> logger, ITokenManager tokenManager, IUserRepository repository)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _repository = repository;
        }

        public async Task<UserSecurityToken> AuthUserAsync(string username, string password, CancellationToken ct = default)
        {
            var user = await _repository.GetUserByNameAsync(username, ct);
            if (user == null || user.PasswordHash != Hasher.CalculateHash(password))
            {
                throw new UserManagerException("Incorrect username or password.");
            }

            var accessToken = _tokenManager.IssueToken(user);
            return new UserSecurityToken { AccessToken = accessToken };
        }

        public async Task<string> CreateUserAsync(UserDto user, bool selfCreate = false, CancellationToken ct = default)
        {
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(user.Username, nameof(user.Username));
            Guard.NotNullOrEmpty(user.Password, nameof(user.Password));

            return await _repository.CreateUserAsync(
                new User
                {
                    UserId = IdGenerator.Generate(),
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = Hasher.CalculateHash(user.Password),
                    Roles = selfCreate
                        ? new[] { UserRoles.User }
                        : user.Roles
                }, ct);
        }

        public async Task<ListResult<UserDto>> GetUsersAsync(int start, int size, CancellationToken ct = default)
        {
            (User[] users, int total) = await _repository.GetUsersAsync(start, size, ct);
            return new ListResult<UserDto>(
                users.Select(u => MapUserDto(u)).ToArray(),
                total);
        }

        public async Task<UserDto> GetUserAsync(string userId, CancellationToken ct = default)
        {
            User user = await _repository.GetUserAsync(userId, ct);
            return MapUserDto(user);
        }

        public async Task UpdateUserAsync(string userId, UserDto user, bool selfUpdate, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(user.Username, nameof(user.Username));

            await _repository.UpdateUserAsync(userId,
                new User
                {
                    UserId = userId,
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = !string.IsNullOrEmpty(user.Password)
                        ? Hasher.CalculateHash(user.Password)
                        : null,
                    Roles = user.Roles
                },
                !selfUpdate, ct);
        }

        public async Task DeleteUserAsync(string userId, CancellationToken ct = default)
        {
            await _repository.DeleteUserAsync(userId, ct);
        }

        private UserDto MapUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Password = null,
                Roles = user.Roles
            };
        }
    }
}