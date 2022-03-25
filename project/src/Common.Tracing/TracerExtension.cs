using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Tracing;

public static class TracerExtension
{
    public static IServiceCollection AddTracing(this IServiceCollection services)
    {
        // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Hosting/README.md
        
        services.AddOpenTelemetryTracing(traceBuilder =>
        {
            traceBuilder.Configure((serviceProvider, builder) =>
            {
                var config = serviceProvider.GetRequiredService<TracerConfig>();
                
                builder.AddAspNetCoreInstrumentation();
                builder.AddSource(config.ServiceName);
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(config.ServiceName));
                builder.AddJaegerExporter(o =>
                {
                    o.Protocol = JaegerExportProtocol.UdpCompactThrift;
                    o.AgentHost = config.AgentHost;
                    o.AgentPort = config.AgentPort;
                });
            });
            
        });
        
        services.AddSingleton<TracerConfig>();
        services.AddSingleton<ITracer, Tracer>();
        
        return services;
    }
}