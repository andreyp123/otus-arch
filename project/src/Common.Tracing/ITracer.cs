using System.Diagnostics;

namespace Common.Tracing;

public interface ITracer
{
    Activity? StartActivity(string name, Dictionary<string, string>? parentContext = null);
    Dictionary<string, string> GetContext(Activity? activity);
}