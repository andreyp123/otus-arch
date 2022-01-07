﻿namespace Common.Model.NotificationSvc
{
    public class Notification
    {
        public string? NotificationId { get; set; }
        public string? UserId { get; set; }
        public string? Data { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
