using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrderSvc.Repository;

public interface IRequestRepository
{
    Task<bool> CheckCreateRequestAsync(string id, string name, DateTime date, CancellationToken ct = default);
}
