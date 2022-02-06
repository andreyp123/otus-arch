using Common.Model.CarSvc;

namespace CarSvc.Dal.Repositories
{
    public interface ICarRepository
    {
        Task<string> CreateCarAsync(Car car, CancellationToken ct = default);
        Task<(Car[], int)> GetCarsAsync(int start, int size, CancellationToken ct = default);
        Task StartCarRent(string carId, string rentId, DateTime rentStartDate, CancellationToken ct = default);
        Task FinishCarRent(string carId, string rentId, DateTime rentEndDate, CancellationToken ct = default);
    }
}
