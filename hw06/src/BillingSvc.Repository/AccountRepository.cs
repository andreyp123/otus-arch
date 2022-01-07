using BillingSvc.Repository.Model;
using Common;
using Common.Model.BillingSvc;
using Microsoft.EntityFrameworkCore;

namespace BillingSvc.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountRepositoryConfig _config;
        private readonly AccountDbContext _dbContext;

        public AccountRepository(AccountRepositoryConfig config, AccountDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<string> CreateAccountAsync(Account account, CancellationToken ct = default)
        {
            Guard.NotNull(account, nameof(account));
            Guard.NotNullOrEmpty(account.AccountId, nameof(account.AccountId));
            Guard.NotNullOrEmpty(account.UserId, nameof(account.UserId));

            if (await _dbContext.Accounts.AnyAsync(ae => ae.AccountId == account.AccountId || ae.UserId == account.UserId))
            {
                throw new EShopException("Account already exists");
            }
            await _dbContext.Accounts.AddAsync(MapAccountEntity(account), ct);
            await _dbContext.SaveChangesAsync(ct);

            return account.AccountId;
        }

        public async Task<Account> GetAccountAsync(string userId, CancellationToken ct = default)
        {
            var accountEntity = await _dbContext.Accounts.FirstOrDefaultAsync(oe => oe.UserId == userId, ct);
            if (accountEntity == null)
            {
                throw new EShopException($"Account not found for user {userId}");
            }
            return MapAccount(accountEntity);
        }

        public async Task<Account> UpdateBalanceAsync(string userId, decimal amount, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));

            var accountEntity = await _dbContext.Accounts.FirstOrDefaultAsync(ae => ae.UserId == userId);
            if (accountEntity == null)
            {
                throw new EShopException($"Account not found for user {userId}");
            }

            accountEntity.Balance += amount;
            await _dbContext.SaveChangesAsync();

            return MapAccount(accountEntity);
        }

        private static AccountEntity MapAccountEntity(Account a)
        {
            return new AccountEntity
            {
                AccountId = a.AccountId,
                UserId = a.UserId,
                Balance = a.Balance,
                Currency = a.Currency,
                CreatedDate = a.CreatedDate
            };
        }

        private static Account MapAccount(AccountEntity ae)
        {
            return new Account
            {
                AccountId = ae.AccountId,
                UserId = ae.UserId,
                Balance = ae.Balance,
                Currency = ae.Currency,
                CreatedDate = ae.CreatedDate
            };
        }
    }
}
