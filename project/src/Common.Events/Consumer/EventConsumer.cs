using Common.Helpers;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Events.Consumer;

public class EventConsumer : BackgroundService
{
    private readonly ILogger<EventConsumer> _logger;
    private readonly IDictionary<EventKey, IEventHandler> _eventHandlers;
    private readonly EventConsumerConfig _config;
    private readonly IConsumer<string, string> _consumer;
    
    public EventConsumer(ILogger<EventConsumer> logger, EventConsumerConfig config, IEnumerable<IEventHandler> eventHandlers)
    {
        _logger = logger;
        _config = config;
        
        _logger.LogInformation($"Building event consumer. Config: {JsonHelper.Serialize(_config)}");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

        _eventHandlers = eventHandlers.ToDictionary(x => new EventKey(x.Topic, x.EventType));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        new Thread(() => StartConsumerLoop(stoppingToken)).Start();
        return Task.CompletedTask;
    }
    
    private void StartConsumerLoop(CancellationToken stoppingToken)
    {
        var topics = _eventHandlers.Select(x => x.Key.Topic).Distinct().ToList();
        _consumer.Subscribe(topics);
        _logger.LogInformation($"Subscribed to topics: {string.Join(", ", topics)}");

        // consuming loop
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                
                _logger.LogInformation($"Consumed event {result.Message.Key} from topic {result.Topic}: {result.Message.Value}");

                if (_eventHandlers.TryGetValue(new EventKey(result.Topic, result.Message.Key), out var handler))
                {
                    Task.WaitAny(handler.HandleEventAsync(result.Message.Value, stoppingToken));
                }
                else
                {
                    _logger.LogInformation($"No handler found. Skipping event");
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