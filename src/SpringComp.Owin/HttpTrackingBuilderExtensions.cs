using Owin;
using SpringComp.Owin.Interop;

namespace SpringComp.Owin
{
    public static class HttpTrackingBuilderExtensions
    {
        public static IAppBuilder UseHttpTracking(this IAppBuilder builder, HttpTrackingOptions options)
        {
            return builder.Use<HttpTrackingMiddleware>(options);
        }
    }
}