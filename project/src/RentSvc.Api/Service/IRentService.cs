using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Model;
using Common.Model.CarSvc;
using Common.Model.RentSvc;
using Common.Model.UserSvc;

namespace RentSvc.Api.Service;

public interface IRentService
{
    Task<RentDto> GetUserRentAsync(string userId, string rentId, CancellationToken ct = default);
    Task<ListResult<RentDto>> GetUserRentsAsync(string userId, int start, int size, CancellationToken ct = default);

    Task UpdateUserAsync(string userId, UserDto user, DateTime? deletedDate, CancellationToken ct = default);
    
    Task<string> InitializeRentStartAsync(string userId, StartRentDto rentToStart, string idempotenceKey, CancellationToken ct = default);
    Task CompleteRentStartAsync(string userId, string rentId, CancellationToken ct = default);
    Task FailRentStartAsync(string userId, string rentId, string errorMessage, Dictionary<string, string> tracingContext = null, CancellationToken ct = default);
    
    Task UpdateInitialCarStateAsync(string userId, string rentId, CarDto car, Dictionary<string, string> tracingContext = null, CancellationToken ct = default);
    Task UpdateRuntimeCarStateAsync(string carId, int? mileage, CancellationToken ct = default);
    
    Task InitializeRentFinishAsync(string userId, string rentId, string idempotenceKey, CancellationToken ct = default);
    Task IssueInvoiceToFinishRentAsync(string userId, string rentId, CarDto car, CancellationToken ct = default);
    Task CompleteRentFinishAsync(string userId, string rentId, CancellationToken ct = default);
    Task FailRentFinishAsync(string userId, string rentId, string errorMessage, CancellationToken ct = default);
}