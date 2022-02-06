namespace NotificationSvc.Dal.Model
{
    public class NotificationEntity
    {
        public int Id { get; set; }
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public string Data { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
