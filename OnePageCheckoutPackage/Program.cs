using OnePageCheckoutPackage.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages for your project.
builder.Services.AddRazorPages();

// Add the OnePageCheckout package services with a default database configuration for testing.
builder.Services.AddOnePageCheckout(
    builder.Configuration,
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Bind CheckoutConfig to the "OnePageCheckout" section in appsettings.json and register it.
builder.Services.Configure<CheckoutConfig>(builder.Configuration.GetSection("OnePageCheckout"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<CheckoutConfig>>().Value);

// Enable session support.
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware.
app.UseSession();

app.UseAuthorization();

// Top-level route registrations
app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout/{action=Index}/{id?}",
    defaults: new { controller = "Checkout" });

app.MapGet("/", context =>
{
    context.Response.Redirect("/checkout");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
