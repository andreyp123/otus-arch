using Microsoft.Extensions.DependencyInjection;

namespace UserManager.BusinessLogic
{
    public static class BusinessLogicExtension
    {
        public static IServiceCollection AddBusinessLogic(
            this IServiceCollection services)
        {
            services.AddScoped<IBusinessLogic, BusinessLogic>();

            return services;
        }
    }
}
