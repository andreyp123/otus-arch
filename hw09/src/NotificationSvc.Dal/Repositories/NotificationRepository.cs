using Common;
using Common.Model.NotificationSvc;
using Microsoft.EntityFrameworkCore;
using NotificationSvc.Dal.Model;

namespace NotificationSvc.Dal.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDalConfig _config;
        private readonly IDbContextFactory<NotificationDbContext> _dbContextFactory;

        public NotificationRepository(NotificationDalConfig config, IDbContextFactory<NotificationDbContext> dbContextFactory)
        {
            _config = config;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<string> CreateNotificationAsync(Notification notification, CancellationToken ct = default)
        {
            Guard.NotNull(notification, nameof(notification));
            Guard.NotNullOrEmpty(notification.NotificationId, nameof(notification.NotificationId));
            Guard.NotNullOrEmpty(notification.UserId, nameof(notification.UserId));

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            if (await dbContext.Notifications.AnyAsync(ne => ne.NotificationId == notification.NotificationId, ct))
            {
                throw new CrashException("Notification already exists");
            }
            await dbContext.Notifications.AddAsync(MapNotificationEntity(notification), ct);
            await dbContext.SaveChangesAsync(ct);

            return notification.NotificationId;
        }

        public async Task<(Notification[], int)> GetUserNotificationsAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);

            var query = dbContext.Notifications
                .Where(ne => ne.UserId == userId);
            var total = await query
                .CountAsync(ct);
            var notifications = await query
                .OrderByDescending(ne => ne.CreatedDate)
                .Skip(start)
                .Take(size)
                .Select(ne => MapNotification(ne))
                .ToArrayAsync(ct);
            return (notifications, total);
        }

        private static NotificationEntity MapNotificationEntity(Notification n)
        {
            return new NotificationEntity
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Data = n.Data,
                CreatedDate = n.CreatedDate
            };
        }

        private static Notification MapNotification(NotificationEntity ne)
        {
            return new Notification
            {
                NotificationId = ne.NotificationId,
                UserId = ne.UserId,
                Data = ne.Data,
                CreatedDate = ne.CreatedDate
            };
        }
    }
}
