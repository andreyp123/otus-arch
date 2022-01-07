using Common;
using Common.Model.NotificationSvc;
using Microsoft.EntityFrameworkCore;
using NotificationSvc.Repository.Model;

namespace NotificationSvc.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationRepositoryConfig _config;
        private readonly NotificationDbContext _dbContext;

        public NotificationRepository(NotificationRepositoryConfig config, NotificationDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        public async Task<string> CreateNotificationAsync(Notification notification, CancellationToken ct = default)
        {
            Guard.NotNull(notification, nameof(notification));
            Guard.NotNullOrEmpty(notification.NotificationId, nameof(notification.NotificationId));
            Guard.NotNullOrEmpty(notification.UserId, nameof(notification.UserId));

            if (await _dbContext.Notifications.AnyAsync(ne => ne.NotificationId == notification.NotificationId))
            {
                throw new EShopException("Notification already exists");
            }
            await _dbContext.Notifications.AddAsync(MapNotificationEntity(notification), ct);
            await _dbContext.SaveChangesAsync(ct);

            return notification.NotificationId;
        }

        public async Task<(Notification[], int)> GetUserNotificationsAsync(string userId, int start, int size, CancellationToken ct = default)
        {
            var query = _dbContext.Notifications
                .Where(ne => ne.UserId == userId);
            var total = await query
                .CountAsync(ct);
            var notifications = await query
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
