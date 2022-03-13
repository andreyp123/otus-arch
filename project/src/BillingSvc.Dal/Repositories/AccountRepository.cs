using System.Transactions;
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

            using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            if (await dbContext.Accounts.AnyAsync(ae => ae.AccountId == account.AccountId || ae.UserId == account.UserId, ct))
            {
                throw new CrashException("Account already exists");
            }

            var accountEntity = MapAccountEntity(account);
            await dbContext.Accounts.AddAsync(accountEntity, ct);
            await dbContext.SaveChangesAsync(ct);
            
            await dbContext.AccountEvents.AddAsync(
                new AccountEventEntity
                {
                    AccountId = accountEntity.Id,
                    EventDate = accountEntity.CreatedDate,
                    EventMessage = "Account created",
                    Amount = accountEntity.Balance,
                    Balance = accountEntity.Balance
                }, ct); 
            await dbContext.SaveChangesAsync(ct);
            
            tran.Complete();

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

            using var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            
            var accountEntity = await dbContext.Accounts.FirstOrDefaultAsync(ae => ae.UserId == userId, ct);
            if (accountEntity == null)
            {
                throw new CrashException($"Account not found for user {userId}");
            }

            accountEntity.Balance += amount;
            
            await dbContext.AccountEvents.AddAsync(
                new AccountEventEntity
                {
                    AccountId = accountEntity.Id,
                    EventDate = DateTime.UtcNow,
                    EventMessage = amount switch
                    {
                        > 0 => "Account deposited",
                        < 0 => "Account withdrawn",
                        _ => ""
                    },
                    Amount = amount,
                    Balance = accountEntity.Balance
                }, ct); 
            
            await dbContext.SaveChangesAsync(ct);
            tran.Complete();

            return MapAccount(accountEntity);
        }

        public async Task<(AccountEvent[], int)> GetAccountEventsAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            var query = dbContext.AccountEvents
                .Include(aee => aee.Account)
                .Where(aee => aee.Account.UserId == userId);
            var total = await query
                .CountAsync(ct);
            var events = await query
                .OrderByDescending(ae => ae.EventDate)
                .Skip(start)
                .Take(size)
                .Select(aee => MapAccountEvent(aee))
                .ToArrayAsync(ct);
            return (events, total);
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

        private static AccountEvent MapAccountEvent(AccountEventEntity aee)
        {
            return new AccountEvent
            {
                AccountId = aee.Account.AccountId,
                EventDate = aee.EventDate,
                EventMessage = aee.EventMessage,
                Amount = aee.Amount,
                Balance = aee.Balance
            };
        }
    }
}
