using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Model.RentSvc;

namespace RentSvc.Dal.Repositories;

public interface IUserRepository
{
    Task<User> GetUserAsync(string userId, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    Task DeleteUserAsync(string userId, DateTime deletedDate, CancellationToken ct = default);
}