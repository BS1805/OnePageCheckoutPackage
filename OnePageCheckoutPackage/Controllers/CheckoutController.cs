using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnePageCheckoutPackage.Models;
using OnePageCheckoutPackage.Services;
namespace OnePageCheckoutPackage.Controllers;

public class CheckoutController : Controller
{
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<CheckoutController> logger)
    {
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("checkout")]
    public ActionResult Index()
    {
        // This is a demo initialization - in a real application,
        // cart items would typically come from a cart service or session
        var model = new CheckoutViewModel
        {
            CartItems = new List<CartItem>
            {
                new CartItem { ProductName = "Blue T-Shirt", Quantity = 1, Price = 50.00m, Color = "Blue" },
                new CartItem { ProductName = "Happy Ninja", Quantity = 1, Price = 5.00m, Color = "Red" }
            },
            BillingDetails = new BillingDetails(),
            ShippingDetails = new ShippingDetails()
        };

        return View(model);
    }

    [HttpPost("checkout/place-order")]
    public async Task<ActionResult> PlaceOrder(CheckoutViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state during checkout");
            return View("Index", model);
        }

        try
        {
            _logger.LogInformation("Processing new order for {Email}", model.BillingDetails.Email);

            // Step 1: Send Invoice Email to Customer
            await _emailService.SendOrderConfirmationEmailAsync(model.BillingDetails.Email, model);

            // Step 2: Send Notification to Admin
            await _notificationService.SendOrderNotificationAsync(model);

            // Store order information in TempData to display on confirmation page
            TempData["OrderNumber"] = model.OrderNumber;
            TempData["CustomerEmail"] = model.BillingDetails.Email;
            TempData["CustomerName"] = $"{model.BillingDetails.FirstName} {model.BillingDetails.LastName}";
            TempData["OrderTotal"] = model.Total.ToString("C");

            return RedirectToAction("OrderConfirmation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            ModelState.AddModelError("", "An error occurred while processing your order. Please try again.");
            return View("Index", model);
        }
    }

    [HttpGet("checkout/confirmation")]
    public ActionResult OrderConfirmation()
    {
        // If there's no order number in TempData, the user probably navigated directly to this page
        if (TempData["OrderNumber"] == null)
        {
            return RedirectToAction("Index");
        }

        return View();
    }
}