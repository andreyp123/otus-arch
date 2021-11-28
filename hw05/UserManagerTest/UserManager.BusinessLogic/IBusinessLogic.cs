using UserManager.Common.Model;
using UserManager.Common.Model.API;

namespace UserManager.BusinessLogic
{
    public interface IBusinessLogic
    {
        Task<UserSecurityToken> AuthUserAsync(string username, string password, CancellationToken ct);

        Task<string> CreateUserAsync(UserDto user, bool selfCreate, CancellationToken ct);

        Task<ListResult<UserDto>> GetUsersAsync(int start, int size, CancellationToken ct);

        Task<UserDto> GetUserAsync(string userId, CancellationToken ct);

        Task UpdateUserAsync(string userId, UserDto user, bool selfUpdate, CancellationToken ct);

        Task DeleteUserAsync(string userId, CancellationToken ct);
    }
}
