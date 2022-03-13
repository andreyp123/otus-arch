namespace NotificationSvc.Api.Email;

public sealed class Email
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsBodyHtml { get; set; }
}