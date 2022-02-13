using BillingSvc.Dal.Model;
using Common;
using Common.Model.BillingSvc;
using Microsoft.EntityFrameworkCore;

namespace BillingSvc.Dal.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDalConfig _config;
        private readonly IDbContextFactory<AccountDbContext> _dbContextFactory;

        public AccountRepository(AccountDalConfig config, IDbContextFactory<AccountDbContext> dbContextFactory)
        {
            _config = config;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<string> CreateAccountAsync(Account account, CancellationToken ct = default)
        {
            Guard.NotNull(account, nameof(account));
            Guard.NotNullOrEmpty(account.AccountId, nameof(account.AccountId));
            Guard.NotNullOrEmpty(account.UserId, nameof(account.UserId));

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            if (await dbContext.Accounts.AnyAsync(ae => ae.AccountId == account.AccountId || ae.UserId == account.UserId, ct))
            {
                throw new CrashException("Account already exists");
            }
            await dbContext.Accounts.AddAsync(MapAccountEntity(account), ct);
            await dbContext.SaveChangesAsync(ct);

            return account.AccountId;
        }

        public async Task<Account> GetAccountAsync(string userId, CancellationToken ct = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            
            var accountEntity = await dbContext.Accounts.FirstOrDefaultAsync(oe => oe.UserId == userId, ct);
            if (accountEntity == null)
            {
                throw new CrashException($"Account not found for user {userId}");
            }
            return MapAccount(accountEntity);
        }

        public async Task<Account> UpdateBalanceAsync(string userId, decimal amount, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(userId, nameof(userId));

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            
            var accountEntity = await dbContext.Accounts.FirstOrDefaultAsync(ae => ae.UserId == userId, ct);
            if (accountEntity == null)
            {
                throw new CrashException($"Account not found for user {userId}");
            }

            accountEntity.Balance += amount;
            await dbContext.SaveChangesAsync(ct);

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
