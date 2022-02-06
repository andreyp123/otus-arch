using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Common;
using Common.Model.RentSvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentSvc.Dal.Model;

namespace RentSvc.Dal.Repositories
{
    public class RentRepository : IRentRepository
    {
        private readonly ILogger<RentRepository> _logger;
        private readonly RentDbContext _dbContext;

        public RentRepository(ILogger<RentRepository> logger, RentDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<string> CreateRentAsync(Rent rent, CancellationToken ct = default)
        {
            Guard.NotNull(rent, nameof(rent));
            Guard.NotNullOrEmpty(rent.RentId, nameof(rent.RentId));
            Guard.NotNullOrEmpty(rent.UserId, nameof(rent.UserId));

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            
            if (await _dbContext.Rents.AnyAsync(oe => oe.RentId == rent.RentId))
            {
                throw new CrashException("Rent already exists");
            }
            await _dbContext.Rents.AddAsync(MapRentEntity(rent), ct);
            await _dbContext.SaveChangesAsync(ct);

            scope.Complete();
            return rent.RentId;
        }

        public async Task<Rent> GetRentAsync(string rentId, CancellationToken ct = default)
        {
            var rentEntity = await _dbContext.Rents.FirstOrDefaultAsync(oe => oe.RentId == rentId, ct);
            if (rentEntity == null)
            {
                throw new CrashException($"Rent {rentId} not found");
            }
            return MapRent(rentEntity);
        }

        public async Task<(Rent[], int)> GetUserRentsAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            var query = _dbContext.Rents
                .Where(oe => oe.UserId == userId);
            var total = await query
                .CountAsync(ct);
            var rents = await query
                .OrderByDescending(oe => oe.CreatedDate)
                .Skip(start)
                .Take(size)
                .Select(oe => MapRent(oe))
                .ToArrayAsync(ct);
            return (rents, total);
        }

        public async Task UpdateRentAsync(string rentId, Rent rent, CancellationToken ct = default)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var rentEntity = await _dbContext.Rents.FirstOrDefaultAsync(oe => oe.RentId == rentId, ct);
            if (rentEntity == null)
            {
                throw new CrashException($"Rent {rentId} not found");
            }

            rentEntity.UserId = rent.UserId;
            rentEntity.CarId = rent.CarId;
            rentEntity.Data = rent.Data;
            rentEntity.CreatedDate = rent.CreatedDate;
            rentEntity.StartDate = rent.StartDate;
            rentEntity.EndDate = rent.EndDate;
            rentEntity.State = rent.State.ToString();
            rentEntity.Message = rent.Message;
            rentEntity.Distance = rent.Distance;
            rentEntity.Amount = rent.Amount;
            
            await _dbContext.SaveChangesAsync(ct);
            scope.Complete();
        }

        private static RentEntity MapRentEntity(Rent r)
        {
            return new RentEntity
            {
                RentId = r.RentId,
                UserId = r.UserId,
                CarId = r.CarId,
                Data = r.Data,
                CreatedDate = r.CreatedDate,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                State = r.State.ToString(),
                Message = r.Message,
                Distance = r.Distance,
                Amount = r.Amount,
            };
        }

        private static Rent MapRent(RentEntity re)
        {
            return new Rent
            {
                RentId = re.RentId,
                UserId = re.UserId,
                CarId = re.CarId,
                Data = re.Data,
                CreatedDate = re.CreatedDate,
                StartDate = re.StartDate,
                EndDate = re.EndDate,
                State = Enum.Parse<RentState>(re.State),
                Message = re.Message,
                Distance = re.Distance,
                Amount = re.Amount
            };
        }
    }
}
