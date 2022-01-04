using System.Threading;
using System.Threading.Tasks;
using Common.Model.UserSvc;

namespace UserSvc.Repository
{
    public interface IUserRepository
    {
        Task<string> CreateUserAsync(User user, CancellationToken ct);
        Task<(User[], int)> GetUsersAsync(int start, int size, CancellationToken ct);
        Task<User> GetUserAsync(string userId, CancellationToken ct);
        Task<User> GetUserByNameAsync(string username, CancellationToken ct);
        Task UpdateUserAsync(string userId, User user, bool updateRoles, CancellationToken ct);
        Task DeleteUserAsync(string userId, CancellationToken ct);
    }
}
