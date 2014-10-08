using System.Threading.Tasks;
using Microsoft.Owin;
using SpringComp.Owin;

namespace App
{
    using System;
    using System.Web.Http;
    using Microsoft.Owin.Hosting;
    using Owin;

    class Program
    {
        static void Main(string[] args)
        {
            const string address = "http://localhost:12345";

            using (WebApp.Start<Startup>(address))
            {
                Console.WriteLine("Listening requests on {0}.", address);
                Console.WriteLine("Please, press ENTER to shutdown.");
                Console.ReadLine();
            }
        }
    }

    public sealed class OpaqueMiddleware : OwinMiddleware
    {
        public OpaqueMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);
        }
    }

    public sealed class Startup
    {
        private readonly HttpConfiguration configuration_ = new HttpConfiguration();

        public void Configuration(IAppBuilder application)
        {
            // Microsoft Azure table columns can hold values less than or equal to 64KB.

            application.UseHttpTracking(
                new HttpTrackingOptions
                {
                    TrackingStore = new HttpTrackingStore(),
                    TrackingIdPropertyName = "x-tracking-id",
                    MaximumRecordedRequestLength = 64 * 1024,
                    MaximumRecordedResponseLength = 64 * 1024,
                }
                );

            application.UseWebApi(configuration_);

            // automatically registers routes for the
            // AspNet WebApi Controllers available in this assembly

            configuration_.MapHttpAttributeRoutes();
        }
    }
}
