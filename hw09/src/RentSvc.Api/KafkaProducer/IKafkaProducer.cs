using System.Threading;
using System.Threading.Tasks;
using Common.Model.NotificationSvc;

namespace RentSvc.Api.KafkaProducer;

public interface IKafkaProducer
{
    Task SendAsync(string topic, NotificationMessage message, CancellationToken ct);
}