using Common.Helpers;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Events.Consumer;

public class EventConsumer : BackgroundService
{
    private readonly ILogger<EventConsumer> _logger;
    private readonly IDictionary<string, IEventHandler> _eventHandlers;
    private readonly EventConsumerConfig _config;
    private readonly IConsumer<Ignore, string> _consumer;
    
    public EventConsumer(ILogger<EventConsumer> logger, EventConsumerConfig config, IEnumerable<IEventHandler> eventHandlers)
    {
        _logger = logger;
        _config = config;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();

        _eventHandlers = eventHandlers.ToDictionary(x => x.Topic);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Thread(() => StartConsumerLoop(stoppingToken)).Start();
        return Task.CompletedTask;
    }
    
    private void StartConsumerLoop(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_eventHandlers.Keys);
        _logger.LogInformation($"Subscribed to topics: {string.Join(", ", _eventHandlers.Keys)}");

        // consuming loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                
                _logger.LogInformation($"Consumed message from {result.Topic}: {result.Message.Value}");

                if (_eventHandlers.TryGetValue(result.Topic, out var handler))
                {
                    var ev = JsonHelper.Deserialize<ConsumedEvent>(result.Message.Value);
                    if (ev == null)
                    {
                        throw new CrashException("Unable to deserialize event from the message consumed");
                    }
                    Task.WaitAny(handler.HandleEventAsync(ev, stoppingToken));
                }
                else
                {
                    _logger.LogWarning($"No handler found for topic {result.Topic}");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consuming error");
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