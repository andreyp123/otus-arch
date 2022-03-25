using Microsoft.Extensions.Configuration;

namespace Common.Tracing;

public class TracerConfig
{
    private const string SECTION_NAME = "Tracing";
    
    public string ServiceName { get; set; }
    public string AgentHost { get; set; }
    public int AgentPort {  get;  set;  }

    public TracerConfig(IConfiguration configuration)
    {
        configuration.GetSection(SECTION_NAME).Bind(this);
    }
}