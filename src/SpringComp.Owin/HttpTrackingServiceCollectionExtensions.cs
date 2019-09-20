using System;
using Microsoft.Extensions.DependencyInjection;

namespace SpringComp.Owin
{
    public static class HttpTrackingServiceCollectionExtensions
    {
        public static void AddHttpTracking(this IServiceCollection services, Action<HttpTrackingOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);
        }
    }
}