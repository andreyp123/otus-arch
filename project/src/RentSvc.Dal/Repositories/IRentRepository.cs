using System.Threading;
using System.Threading.Tasks;
using Common.Model.RentSvc;

namespace RentSvc.Dal.Repositories
{
    public interface IRentRepository
    {
        Task<string> CreateRentAsync(Rent rent, CancellationToken ct = default);
        Task<Rent> GetUserRentAsync(string userId, string rentId, CancellationToken ct = default);
        Task<(Rent[], int)> GetUserRentsAsync(string userId, int start, int size, CancellationToken ct = default);
        Task<bool> HasUserActiveRentsAsync(string userId, CancellationToken ct = default);
        Task UpdateRentAsync(string rentId, Rent rent, CancellationToken ct = default);
        Task UpdateActiveRentAsync(string carId, int? mileage, CancellationToken ct = default);
    }
}
