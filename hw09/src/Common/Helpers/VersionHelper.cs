using Common.Model;
using System.Net;
using System.Reflection;

namespace Common.Helpers
{
    public static class VersionHelper
    {
        public static VersionInfo GetVersionInfo() => new VersionInfo
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
            Hostname = Dns.GetHostName()
        };
    }
}
