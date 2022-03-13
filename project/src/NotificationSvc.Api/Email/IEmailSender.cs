using System.Threading;
using System.Threading.Tasks;

namespace NotificationSvc.Api.Email;

public interface IEmailSender
{
    Task SendAsync(Email email, CancellationToken ct = default);
}