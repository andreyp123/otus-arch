﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Common.Tracing;

public class Tracer : ITracer
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    
    private readonly ILogger<Tracer> _logger;
    private readonly TracerConfig _config;
    private readonly ActivitySource _activitySrc;
    
    public Tracer(ILogger<Tracer> logger, TracerConfig config)
    {
        _logger = logger;
        _config = config;
        _activitySrc = new ActivitySource(_config.ServiceName);
    }

    public Activity? StartActivity(string name, Dictionary<string, string>? parentContext = null)
    {
        Activity? activity = null;
        try
        {
            if (parentContext == null || parentContext.Count == 0)
            {
                activity = _activitySrc.StartActivity(name, ActivityKind.Producer);
            }
            else
            {
                var context = Propagator.Extract(default, parentContext,
                    (dictionary, key) =>
                    {
                        return dictionary.TryGetValue(key, out var value)
                            ? new []{ value }
                            : Enumerable.Empty<string>();
                    });
                Baggage.Current = context.Baggage;
                activity = _activitySrc.StartActivity(name, ActivityKind.Producer, context.ActivityContext);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting activity '{name}'.");
        }

        return activity;
    }

    public Dictionary<string, string> GetContext(Activity? activity)
    {
        var tracingContext = new Dictionary<string, string>();
        if (activity != null)
        {
            Propagator.Inject(
                new PropagationContext(activity.Context, Baggage.Current),
                tracingContext,
                (dictionary, key, value) =>
                {
                    dictionary[key] = value;
                });
        }

        return tracingContext;
    }
}