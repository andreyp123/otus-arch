using Common.Model.BillingSvc;

namespace BillingSvc.Repository
{
    public interface IAccountRepository
    {
        Task<string> CreateAccountAsync(Account account, CancellationToken ct);
        Task<Account> GetAccountAsync(string userId, CancellationToken ct);
        Task<Account> UpdateBalanceAsync(string userId, decimal amount, CancellationToken ct);
    }
}
