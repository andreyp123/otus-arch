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
    private readonly IDbContextFactory<RentDbContext> _dbContextFactory;

    public RequestRepository(ILogger<RequestRepository> logger, IDbContextFactory<RentDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<bool> CheckCreateRequestAsync(string id, string name, DateTime date, CancellationToken ct = default)
    {
        var created = false;
        
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        
        if (!await dbContext.Requests.AnyAsync(
                re => re.Id == id && re.Name == name && date - re.Date < REQUEST_TIMEOUT, ct))
        {
            dbContext.Requests.Add(new RequestEntity {Id = id, Name = name, Date = date});
            await dbContext.SaveChangesAsync(ct);
            created = true;
        }
        
        scope.Complete();
        
        return created;
    }
}