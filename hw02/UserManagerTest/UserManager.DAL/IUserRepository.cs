﻿using System.Threading;
using System.Threading.Tasks;
using UserManager.Common;

namespace UserManager.DAL
{
    public interface IUserRepository
    {
        Task<string> CreateUserAsync(User user, CancellationToken ct);
        Task<User> GetUserAsync(string userId, CancellationToken ct);
        Task UpdateUserAsync(string userId, User user, CancellationToken ct);
        Task DeleteUserAsync(string userId, CancellationToken ct);
    }
}
