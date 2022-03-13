using System.Threading;
using System.Threading.Tasks;
using Common.Model.UserSvc;

namespace UserSvc.Dal.Repositories;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user, CancellationToken ct = default);
    Task<(User[], int)> GetUsersAsync(int start, int size, CancellationToken ct = default);
    Task<User> GetUserAsync(string userId, CancellationToken ct = default);
    Task<User> GetUserByNameAsync(string username, CancellationToken ct = default);
    Task<User> UpdateUserAsync(string userId, User user, bool selfUpdate = true, CancellationToken ct = default);
    Task DeleteUserAsync(string userId, CancellationToken ct = default);
}