using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    private readonly CheckoutDbContext _dbContext;

    public CheckoutController(
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<CheckoutController> logger,
        CheckoutDbContext dbContext)
    {
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("checkout")]
    public ActionResult Index()
    {
        // Retrieve cart from session
        var cartJson = HttpContext.Session.GetString("Cart");
        var cartItems = string.IsNullOrEmpty(cartJson)
            ? new List<CartItem>
            {
        // Add hardcoded items for testing
        new CartItem { ProductName = "Blue T-Shirt", Quantity = 2, Price = 25.00m, Color = "Blue" },
        new CartItem { ProductName = "Red Hat", Quantity = 1, Price = 15.00m, Color = "Red" }
            }
            : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

        var model = new CheckoutViewModel
        {
            CartItems = cartItems,
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

            // Save order to database
            var order = new Order
            {
                OrderNumber = model.OrderNumber,
                CustomerName = $"{model.BillingDetails.FirstName} {model.BillingDetails.LastName}",
                CustomerEmail = model.BillingDetails.Email,
                PaymentMethod = model.SelectedPaymentMethod.ToString(),
                TotalAmount = model.Total,
                BillingDetails = model.BillingDetails,
                ShippingDetails = model.ShippingDetails,
                CartItems = model.CartItems
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Send email and notification
            await _emailService.SendOrderConfirmationEmailAsync(model.BillingDetails.Email, model);
            await _notificationService.SendOrderNotificationAsync(model);

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

    [HttpGet("checkout/complete-order")]
    public IActionResult CompleteOrder()
    {
        // Logic to finalize the order (e.g., save to database, send confirmation email)
        TempData["OrderNumber"] = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        TempData["CustomerEmail"] = "sb-slned26457170@personal.example.com"; // Replace with actual email
        TempData["CustomerName"] = "John Doe"; // Replace with actual customer name
        TempData["OrderTotal"] = "$76.50"; // Replace with actual total

        return RedirectToAction("OrderConfirmation");
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
