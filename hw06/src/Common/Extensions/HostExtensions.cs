using Microsoft.Extensions.Hosting;

namespace Common.Extensions
{
    public static class HostExtensions
    {
        public static ServiceT GetService<ServiceT>(this IHost host)
        {
            return (ServiceT)host.Services.GetService(typeof(ServiceT));
        }
    }
}
