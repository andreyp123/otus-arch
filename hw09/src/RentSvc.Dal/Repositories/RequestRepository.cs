using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Model;

namespace RentSvc.Dal.Repositories;

public class RequestRepository : IRequestRepository
{
    private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromHours(1);
    
    private readonly ILogger<RequestRepository> _logger;
    private readonly RentDbContext _dbContext;

    public RequestRepository(ILogger<RequestRepository> logger, RentDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    public async Task<bool> CheckCreateRequestAsync(string id, string name, DateTime date, CancellationToken ct = default)
    {
        var created = false;
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        if (!await _dbContext.Requests.AnyAsync(
                re => re.Id == id && re.Name == name && date - re.Date < REQUEST_TIMEOUT, ct))
        {
            _dbContext.Requests.Add(new RequestEntity {Id = id, Name = name, Date = date});
            await _dbContext.SaveChangesAsync(ct);
            created = true;
        }
        
        scope.Complete();
        return created;
    }
}