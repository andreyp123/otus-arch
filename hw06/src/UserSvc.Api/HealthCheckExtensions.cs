using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserSvc.Api
{
    public static class HealthCheckExtensions
    {
        public static IEndpointConventionBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapHealthChecks("/health",
                new HealthCheckOptions
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
                    ResponseWriter = WriteResponse
                });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var options = new JsonWriterOptions { Indented = true };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("status", result.Status.ToString());
                    writer.WriteStartObject("results");
                    foreach (var entry in result.Entries)
                    {
                        writer.WriteStartObject(entry.Key);
                        writer.WriteString("status", entry.Value.Status.ToString());
                        writer.WriteString("description",
                            string.IsNullOrEmpty(entry.Value.Description)
                                ? entry.Value.Status == HealthStatus.Healthy || entry.Value.Exception == null
                                    ? entry.Value.Status.ToString()
                                    : entry.Value.Exception.Message
                                : entry.Value.Description);
                        writer.WriteEndObject();
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }

                var json = Encoding.UTF8.GetString(stream.ToArray());
                return context.Response.WriteAsync(json);
            }
        }
    }
}
