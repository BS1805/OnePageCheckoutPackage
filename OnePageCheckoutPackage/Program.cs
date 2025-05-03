using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace OnePageCheckoutPackage
{
    public static class OnePageCheckoutPackageExtensions
    {
        // Method to register services required by the package
        public static IServiceCollection AddOnePageCheckout(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Razor Pages and MVC services
            services.AddControllersWithViews();

            // Add any additional services or configurations required by the package
            // Example: services.AddSingleton<IMyService, MyService>();

            return services;
        }

        // Method to configure middleware required by the package
        public static void UseOnePageCheckout(this IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Map routes for the package
                endpoints.MapControllerRoute(
                    name: "checkout",
                    pattern: "checkout/{action=Index}/{id?}",
                    defaults: new { controller = "Checkout" });

                // Remove the default route pointing to HomeController
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Checkout}/{action=Index}/{id?}");
            });
        }


    }
}
