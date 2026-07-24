using System.Globalization;
using CvManager.Web.Hubs;
using Microsoft.AspNetCore.Localization;

namespace CvManager.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseAppRequestLocalization()
        {
            var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ru") };
            return app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapAppEndpoints()
        {
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapHub<DiscussionHub>("/hubs/discussion");
            return app;
        }
    }
}
