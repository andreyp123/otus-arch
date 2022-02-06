using System;
using System.Threading;
using System.Threading.Tasks;

namespace RentSvc.Dal.Repositories;

public interface IRequestRepository
{
    Task<bool> CheckCreateRequestAsync(string id, string name, DateTime date, CancellationToken ct = default);
}
