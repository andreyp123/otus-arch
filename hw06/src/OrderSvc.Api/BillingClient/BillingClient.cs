using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Helpers;
using Common.Model;
using Common.Model.BillingSvc;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

namespace OrderSvc.Api.BillingClient;

public class BillingClient : IBillingClient
{
    private readonly BillingClientConfig _config;
    private readonly RestClient _client;
    private readonly ILogger<BillingClient> _logger;
    
    public BillingClient(ILogger<BillingClient> logger, BillingClientConfig config)
    {
        _logger = logger;
        _config = config;
        _client = new RestClient(_config.Url);
    }

    public void SetToken(string accessToken)
    {
        _client.Authenticator = new JwtAuthenticator(accessToken);
    }
    
    public async Task<AccountDto> WithdrawAccountAsync(decimal amount, CancellationToken ct = default)
    {
        var request = new RestRequest("account/withdraw");
        request.AddParameter("amount", amount, ParameterType.QueryString);
        
        var response = await _client.PostAsync(request, ct);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            return JsonHelper.Deserialize<AccountDto>(response.Content);
        }
        else
        {
            string errorMessage;
            try
            {
                var errorRes = JsonHelper.Deserialize<ErrorResult>(response.Content);
                errorMessage = errorRes.Message;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error requesting account withdraw. Code: {response.StatusCode}. Message: {ex.Message}";
            }
            throw new EShopException(errorMessage);
        }
    }
}