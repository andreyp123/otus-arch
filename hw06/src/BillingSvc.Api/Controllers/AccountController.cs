using BillingSvc.Repository;
using Common;
using Common.Helpers;
using Common.Model.BillingSvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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

        [HttpPost("")]
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

        [HttpPost("deposit")]
        [Authorize]
        public async Task<AccountDto> DepositAccount([FromQuery] decimal amount)
        {
            return await UpdateBalanceAsync(amount, false, HttpContext.RequestAborted);
        }

        [HttpPost("withdraw")]
        [Authorize]
        public async Task<AccountDto> WithdrawAccount([FromQuery] decimal amount)
        {
            return await UpdateBalanceAsync(amount, true, HttpContext.RequestAborted);
        }

        private async Task<AccountDto> UpdateBalanceAsync(decimal amount, bool withdraw, CancellationToken ct)
        {
            if (amount <= 0)
            {
                throw new EShopException("Incorrect amount");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var account = await _repository.UpdateBalanceAsync(userId, amount * (withdraw ? -1 : 1), ct);
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
