// Services/Interfaces/IEmailService.cs
using OnePageCheckoutPackage.Configuration;
using OnePageCheckoutPackage.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using Telegram.Bot;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;



namespace OnePageCheckoutPackage.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(string emailAddress, CheckoutViewModel model);
}



public interface INotificationService
{
    Task SendOrderNotificationAsync(CheckoutViewModel model);
}



public class EmailService : IEmailService
{
    private readonly CheckoutConfig _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(CheckoutConfig config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(string emailAddress, CheckoutViewModel model)
    {
        try
        {
            var smtpClient = new SmtpClient(_config.SmtpSettings.SmtpServer)
            {
                Port = _config.SmtpSettings.SmtpPort,
                Credentials = new NetworkCredential(_config.SmtpSettings.SmtpUsername, _config.SmtpSettings.SmtpPassword),
                EnableSsl = true,
            };

            var fromEmail = !string.IsNullOrEmpty(_config.SmtpSettings.FromEmail)
                ? _config.SmtpSettings.FromEmail
                : _config.SmtpSettings.SmtpUsername;

            var fromName = !string.IsNullOrEmpty(_config.SmtpSettings.FromName)
                ? _config.SmtpSettings.FromName
                : "Your Store";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"Your Order Confirmation #{model.OrderNumber}",
                Body = BuildHtmlEmail(model),
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailAddress);

            // Send the email
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order confirmation email");
            // Don't throw - we don't want to break the checkout flow if email fails
        }
    }


    private string BuildPlainTextEmail(CheckoutViewModel model)
    {
        var text = $"Thank you for your order #{model.OrderNumber}!\n\n";
        text += "Order Details:\n";
        text += $"Customer: {model.BillingDetails.FirstName} {model.BillingDetails.LastName}\n";
        text += $"Email: {model.BillingDetails.Email}\n";
        text += $"Shipping Address: {model.ShippingDetails.ShippingAddress}, {model.ShippingDetails.ShippingCity}, {model.ShippingDetails.ShippingPostalCode}\n\n";
        text += "Items:\n";

        foreach (var item in model.CartItems)
        {
            text += $"- {item.ProductName} - Qty: {item.Quantity} - Price: {item.Price:C} - Total: {item.Total:C}\n";
        }

        text += $"\nSubtotal: {model.Subtotal:C}\n";
        if (model.Tax > 0) text += $"Tax: {model.Tax:C}\n";
        if (model.ShippingCost > 0) text += $"Shipping: {model.ShippingCost:C}\n";
        text += $"Total: {model.Total:C}\n\n";
        text += "Thank you for shopping with us!";

        return text;
    }

    private string BuildHtmlEmail(CheckoutViewModel model)
    {
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #4a4a4a;'>Thank you for your order #{model.OrderNumber}!</h2>
            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px;'>
                <h3 style='margin-top: 0; color: #3d3d3d;'>Order Details</h3>
                <p><strong>Customer:</strong> {model.BillingDetails.FirstName} {model.BillingDetails.LastName}</p>
                <p><strong>Email:</strong> {model.BillingDetails.Email}</p>
                <p><strong>Shipping Address:</strong> {model.ShippingDetails.ShippingAddress}, {model.ShippingDetails.ShippingCity}, {model.ShippingDetails.ShippingPostalCode}</p>
            </div>
            
            <table style='width: 100%; border-collapse: collapse;'>
                <thead>
                    <tr style='background-color: #f3f3f3;'>
                        <th style='text-align: left; padding: 8px; border-bottom: 1px solid #ddd;'>Product</th>
                        <th style='text-align: center; padding: 8px; border-bottom: 1px solid #ddd;'>Quantity</th>
                        <th style='text-align: right; padding: 8px; border-bottom: 1px solid #ddd;'>Price</th>
                        <th style='text-align: right; padding: 8px; border-bottom: 1px solid #ddd;'>Total</th>
                    </tr>
                </thead>
                <tbody>";

        foreach (var item in model.CartItems)
        {
            html += $@"
                <tr>
                    <td style='text-align: left; padding: 8px; border-bottom: 1px solid #ddd;'>{item.ProductName} {(string.IsNullOrEmpty(item.Color) ? "" : $"(Color: {item.Color})")}</td>
                    <td style='text-align: center; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Quantity}</td>
                    <td style='text-align: right; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Price:C}</td>
                    <td style='text-align: right; padding: 8px; border-bottom: 1px solid #ddd;'>{item.Total:C}</td>
                </tr>";
        }

        html += $@"
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan='3' style='text-align: right; padding: 8px;'><strong>Subtotal:</strong></td>
                        <td style='text-align: right; padding: 8px;'>{model.Subtotal:C}</td>
                    </tr>";

        if (model.Tax > 0)
        {
            html += $@"
                    <tr>
                        <td colspan='3' style='text-align: right; padding: 8px;'><strong>Tax:</strong></td>
                        <td style='text-align: right; padding: 8px;'>{model.Tax:C}</td>
                    </tr>";
        }

        if (model.ShippingCost > 0)
        {
            html += $@"
                    <tr>
                        <td colspan='3' style='text-align: right; padding: 8px;'><strong>Shipping:</strong></td>
                        <td style='text-align: right; padding: 8px;'>{model.ShippingCost:C}</td>
                    </tr>";
        }

        html += $@"
                    <tr>
                        <td colspan='3' style='text-align: right; padding: 8px;'><strong>Total:</strong></td>
                        <td style='text-align: right; padding: 8px;'><strong>{model.Total:C}</strong></td>
                    </tr>
                </tfoot>
            </table>
            
            <div style='margin-top: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px;'>
                <p style='margin-bottom: 0;'>Thank you for shopping with us!</p>
            </div>
        </div>";

        return html;
    }
}



public class TelegramNotificationService : INotificationService
{
    private readonly CheckoutConfig _config;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(CheckoutConfig config, ILogger<TelegramNotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOrderNotificationAsync(CheckoutViewModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(_config.Telegram.BotToken) || string.IsNullOrEmpty(_config.Telegram.ChatId))
            {
                _logger.LogWarning("Telegram notification skipped: Bot token or chat ID not configured");
                return;
            }

            var botClient = new TelegramBotClient(_config.Telegram.BotToken);

            var messageText = $"🛒 New Order #{model.OrderNumber}!\n\n" +
                             $"👤 Name: {model.BillingDetails.FirstName} {model.BillingDetails.LastName}\n" +
                             $"✉️ Email: {model.BillingDetails.Email}\n" +
                             $"📱 Phone: {model.BillingDetails.Phone}\n" +
                             $"🏠 Shipping Address: {model.ShippingDetails.ShippingAddress}, {model.ShippingDetails.ShippingCity}\n\n" +
                             $"📦 Items Ordered: \n";

            foreach (var item in model.CartItems)
            {
                messageText += $"• {item.ProductName} - Qty: {item.Quantity} - {item.Price:C} each\n";
            }

            messageText += $"\n💰 Subtotal: {model.Subtotal:C}";
            if (model.Tax > 0) messageText += $"\n🧾 Tax: {model.Tax:C}";
            if (model.ShippingCost > 0) messageText += $"\n🚚 Shipping: {model.ShippingCost:C}";
            messageText += $"\n💵 Total: {model.Total:C}";

            await botClient.SendTextMessageAsync(
                chatId: _config.Telegram.ChatId,
                text: messageText
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram notification");
            // Don't throw - we don't want to break the checkout flow if notification fails
        }
    }
}