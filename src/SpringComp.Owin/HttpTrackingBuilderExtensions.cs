using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SpringComp.Owin
{
    public static class HttpTrackingAppBuilderExtensions
    {
        public static IApplicationBuilder UseHttpTracking(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = app.ApplicationServices.GetRequiredService<IOptions<HttpTrackingOptions>>();

            return app.UseMiddleware<HttpTrackingMiddleware>(options.Value);
        }
    }
}