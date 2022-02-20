﻿using Common.Model.CarSvc;

namespace CarSvc.Dal.Repositories;

public interface ICarRepository
{
    Task<string> CreateCarAsync(Car car, CancellationToken ct = default);
    Task UpdateCarAsync(string carId, Car car, CancellationToken ct = default);
    Task<Car> GetCarAsync(string carId, CancellationToken ct = default);
    Task<(Car[], int)> GetCarsAsync(int start, int size, bool all = true, CancellationToken ct = default);
    Task UpdateCarStateAsync(string carId, CarState carState, CancellationToken ct = default);
    Task<Car> StartCarRent(string carId, string rentId, DateTime rentStartDate, CancellationToken ct = default);
    Task FinishCarRent(string carId, string rentId, DateTime rentEndDate, CancellationToken ct = default);
}