using System.Threading;
using System.Threading.Tasks;
using Common.Model.BillingSvc;

namespace OrderSvc.Api.BillingClient;

public interface IBillingClient
{
    void SetToken(string accessToken);
    Task<AccountDto> WithdrawAccountAsync(decimal amount, CancellationToken ct);
}