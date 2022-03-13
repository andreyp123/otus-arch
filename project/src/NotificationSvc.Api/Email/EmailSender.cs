using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NotificationSvc.Api.Email;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailSenderConfig _config;

    private readonly MailAddress _from;
    private readonly Encoding _encoding;
    
    public EmailSender(ILogger<EmailSender> logger, EmailSenderConfig config)
    {
        _logger = logger;
        _config = config;
        
        _from = new MailAddress(config.From);
        _encoding = Encoding.GetEncoding(config.Encoding);
    }

    public async Task SendAsync(Email email, CancellationToken ct = default)
    {
        try
        {
            using var mailMessage = new MailMessage
            {
                From = _from,
                SubjectEncoding = _encoding,
                HeadersEncoding = _encoding,
                BodyEncoding = _encoding,
                IsBodyHtml = email.IsBodyHtml,
                Body = email.Body,
                Subject = email.Subject
            };
            
            mailMessage.To.Add(new MailAddress(email.To));
            
            if (email.IsBodyHtml)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(email.Body, null, "text/html");
                mailMessage.AlternateViews.Add(htmlView);
            }

            using var smtpClient = CreateSmtpClient();
            await smtpClient.SendMailAsync(mailMessage, ct);
            
            _logger.LogInformation($"Sent email. From: '{_config.From}'. To: '{email.To}'. Body: '{email.Body}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email");
        }
    }
    
    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient
        {
            Host = _config.Host,
            Port = _config.Port,
            EnableSsl = _config.EnableSsl,
        };

        if (!string.IsNullOrEmpty(_config.Username))
        {
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(_config.Username, _config.Password);
        }

        return smtpClient;
    }
}