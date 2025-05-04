using OnePageCheckoutPackage;
using OnePageCheckoutPackage.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages for your project.
builder.Services.AddRazorPages();

// Add the OnePageCheckout package services.
builder.Services.AddOnePageCheckout(builder.Configuration);

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
