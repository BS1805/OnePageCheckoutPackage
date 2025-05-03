// Configuration/CheckoutConfig.cs
namespace OnePageCheckoutPackage.Configuration;
// Configuration/ServiceExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnePageCheckoutPackage.Services;


public class CheckoutConfig
{
    public SmtpSettings SmtpSettings { get; set; } = new();
    public TelegramSettings Telegram { get; set; } = new();
}

public class SmtpSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty; // Optional: can use SmtpUsername if not set
    public string FromName { get; set; } = string.Empty;
}

public class TelegramSettings
{
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}


public static class ServiceExtensions
{
    public static IServiceCollection AddOnePageCheckout(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration
        var checkoutConfig = new CheckoutConfig();
        configuration.GetSection("OnePageCheckout").Bind(checkoutConfig);
        services.AddSingleton(checkoutConfig);

        // Register services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, TelegramNotificationService>();

        // Add controllers from this assembly
        services.AddControllersWithViews()
                .AddApplicationPart(typeof(ServiceExtensions).Assembly);

        return services;
    }
}