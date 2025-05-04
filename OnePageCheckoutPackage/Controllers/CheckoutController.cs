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

namespace OnePageCheckoutPackage.Controllers
{
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
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning(error.ErrorMessage);
                }

                // Reload cart from session
                var cartJson = HttpContext.Session.GetString("Cart");
                model.CartItems = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>
                    {
            // Add hardcoded items for testing
            new CartItem { ProductName = "Blue T-Shirt", Quantity = 2, Price = 25.00m, Color = "Blue" },
            new CartItem { ProductName = "Red Hat", Quantity = 1, Price = 15.00m, Color = "Red" }
                }
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                return View("Index", model);
            }

            try
            {
                _logger.LogInformation("Processing new order for {Email}", model.BillingDetails.Email);

                // Persist cart data in session
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(model.CartItems));

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
}
