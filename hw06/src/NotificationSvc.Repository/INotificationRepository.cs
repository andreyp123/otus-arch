﻿using Common.Model.NotificationSvc;

namespace NotificationSvc.Repository
{
    public interface INotificationRepository
    {
        Task<string> CreateNotificationAsync(Notification notification, CancellationToken ct);
        Task<(Notification[], int)> GetUserNotificationsAsync(string userId, int start, int size, CancellationToken ct);
    }
}
