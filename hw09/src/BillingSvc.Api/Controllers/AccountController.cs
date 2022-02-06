using BillingSvc.Dal;
using Common;
using Common.Helpers;
using Common.Model.BillingSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BillingSvc.Dal.Repositories;

namespace BillingSvc.Api.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountRepository _repository;

        public AccountController(ILogger<AccountController> logger, IAccountRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpPost]
        [Authorize]
        public async Task CreateAccount([FromBody] CreateAccountDto acc)
        {
            Guard.NotNull(acc, nameof(acc));

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            await _repository.CreateAccountAsync(
                new Account
                {
                    UserId = userId,
                    AccountId = IdGenerator.Generate(),
                    Currency = acc?.Currency ?? "rub",
                    Balance = acc.Balance,
                    CreatedDate = DateTime.UtcNow
                },
                HttpContext.RequestAborted);
        }

        [HttpGet]
        [Authorize]
        public async Task<AccountDto> GetAccount()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var account = await _repository.GetAccountAsync(userId, HttpContext.RequestAborted);
            return MapAccountDto(account);
        }

        [HttpPost("deposit")]
        [Authorize]
        public async Task<AccountDto> DepositAccount([FromQuery] decimal amount)
        {
            if (amount < 0)
            {
                throw new CrashException("Incorrect amount");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var account = await _repository.UpdateBalanceAsync(userId, amount, HttpContext.RequestAborted);
            return MapAccountDto(account);
        }

        [HttpPost("withdraw")]
        [Authorize]
        public async Task<AccountDto> WithdrawAccount([FromQuery] decimal amount)
        {
            if (amount < 0)
            {
                throw new CrashException("Incorrect amount");
            }

            var ct = HttpContext.RequestAborted;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var account = await _repository.GetAccountAsync(userId, ct);
            if (account.Balance - amount < 0)
            {
                throw new CrashException("Not enough money");
            }

            account = await _repository.UpdateBalanceAsync(userId, -amount, ct);
            return MapAccountDto(account);
        }

        private AccountDto MapAccountDto(Account account)
        {
            return new AccountDto
            {
                UserId = account.UserId,
                AccountId = account.AccountId,
                Currency = account.Currency,
                Balance = account.Balance,
                CreatedDate = account.CreatedDate
            };
        }
    }
}
