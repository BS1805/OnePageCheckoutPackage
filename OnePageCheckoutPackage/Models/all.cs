﻿
using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;

using System.Linq;
using System;

namespace OnePageCheckoutPackage.Models;

public class CartItem
{
    [Key] // Add this attribute to define the primary key
    public int Id { get; set; }
    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    public string Color { get; set; } = string.Empty;

    public string? ProductImageUrl { get; set; }

    // Dynamically calculate total for the item
    public decimal Total => Quantity * Price;
}



public class BillingDetails
{
    [Key] // Add this attribute to define the primary key
    public int Id { get; set; } // Primary key

    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal code is required")]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    public string Country { get; set; } = string.Empty;
}

public class ShippingDetails
{
    [Key] // Add this attribute to define the primary key
    public int Id { get; set; } // Primary key

    [Required(ErrorMessage = "Shipping address is required")]
    [Display(Name = "Address")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Shipping city is required")]
    [Display(Name = "City")]
    public string ShippingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Shipping postal code is required")]
    [Display(Name = "Postal Code")]
    public string ShippingPostalCode { get; set; } = string.Empty;

    [Display(Name = "Country")]
    public string ShippingCountry { get; set; } = string.Empty;
}




public enum PaymentMethod
{
    CreditCard,
    PayPal,
    BankTransfer
}

public class Order
{
    [Key]
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public BillingDetails BillingDetails { get; set; } = new();
    public ShippingDetails ShippingDetails { get; set; } = new();
    public List<CartItem> CartItems { get; set; } = new();
}



public class CheckoutViewModel
{
    public CheckoutViewModel()
    {
        CartItems = new List<CartItem>();
        BillingDetails = new BillingDetails();
        ShippingDetails = new ShippingDetails();
    }

    [Required]
    public List<CartItem> CartItems { get; set; }

    [Required]
    public BillingDetails BillingDetails { get; set; }

    [Required]
    public ShippingDetails ShippingDetails { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod SelectedPaymentMethod { get; set; }

    [Display(Name = "Order Notes")]
    public string? OrderNotes { get; set; }

    public bool SameAsShipping { get; set; } = true;

    // Dynamically calculate subtotal
    public decimal Subtotal => CartItems.Sum(item => item.Total);

    // Dynamically calculate tax (e.g., 10% tax rate)
    public decimal Tax => Subtotal * 0.1m;

    // Dynamically calculate shipping cost (e.g., flat rate)
    public decimal ShippingCost => CartItems.Any() ? 5.00m : 0.00m;

    // Dynamically calculate total
    public decimal Total => Subtotal + Tax + ShippingCost;

    public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
}
