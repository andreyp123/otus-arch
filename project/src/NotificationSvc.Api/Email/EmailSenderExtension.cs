using Microsoft.Extensions.DependencyInjection;

namespace NotificationSvc.Api.Email;

public static class EmailSenderExtension
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services)
    {
        services.AddSingleton<EmailSenderConfig>();
        services.AddSingleton<IEmailSender, EmailSender>();
        return services;
    }
}