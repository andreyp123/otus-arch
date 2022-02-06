using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Helpers;
using Common.Model.NotificationSvc;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationSvc.Dal;
using NotificationSvc.Dal.Repositories;

namespace NotificationSvc.Api.KafkaConsumer;

public class KafkaConsumer : BackgroundService
{
    private const string TOPIC_NAME = "notifications";
    private const string GROUP_ID = "NotificationSvc";
    
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly INotificationRepository _repository;
    private readonly KafkaConsumerConfig _config;
    private readonly IConsumer<Ignore, string> _consumer;
    
    public KafkaConsumer(ILogger<KafkaConsumer> logger, INotificationRepository repository, KafkaConsumerConfig config)
    {
        _logger = logger;
        _repository = repository;
        _config = config;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = GROUP_ID,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Thread(() => StartConsumerLoop(stoppingToken)).Start();
        return Task.CompletedTask;
    }
    
    private void StartConsumerLoop(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(TOPIC_NAME);
        _logger.LogInformation($"Subscribed to topic {TOPIC_NAME}");

        // consuming loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                
                _logger.LogInformation($"Consumed message: {result.Message.Value}");
                
                var message = JsonHelper.Deserialize<NotificationMessage>(result.Message.Value);
                if (message == null)
                {
                    throw new CrashException("Unable to deserialize notification message");
                }

                Task.WaitAny(_repository.CreateNotificationAsync(
                    new Notification
                    {
                        NotificationId = IdGenerator.Generate(),
                        UserId = message.UserId,
                        Data = message.Data,
                        CreatedDate = DateTime.UtcNow
                    },
                    stoppingToken));
                _logger.LogInformation("Saved message into DB");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notifications consuming error");
                if (ex is ConsumeException cex)
                {
                    _logger.LogError($"ConsumeException details: {cex.Error}");
                }
            }
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}