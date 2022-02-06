using Common.Model.NotificationSvc;

namespace NotificationSvc.Dal.Repositories
{
    public interface INotificationRepository
    {
        Task<string> CreateNotificationAsync(Notification notification, CancellationToken ct = default);
        Task<(Notification[], int)> GetUserNotificationsAsync(string userId, int start, int size, CancellationToken ct = default);
    }
}
