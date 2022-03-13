using System.Threading;
using System.Threading.Tasks;

namespace RentSvc.Api.Cache;

public interface ICacheManager
{
    Task<bool> SetIfNotExistAsync(string key, string value, CancellationToken ct = default);
}