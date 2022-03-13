using Common.Model.BillingSvc;

namespace BillingSvc.Dal.Repositories
{
    public interface IAccountRepository
    {
        Task<string> CreateAccountAsync(Account account, CancellationToken ct);
        Task<Account> GetAccountAsync(string userId, CancellationToken ct);
        Task<Account> UpdateBalanceAsync(string userId, decimal amount, CancellationToken ct);
        Task<(AccountEvent[], int)> GetAccountEventsAsync(string userId, int start, int size, CancellationToken ct = default);
    }
}
