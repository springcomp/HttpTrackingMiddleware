using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SpringComp.Interop;

namespace SpringComp.Owin
{
    /// <summary>
    /// A simple Owin Middleware to capture HTTP requests and responses
    /// and store details of the call into a durable store.
    /// </summary>
    public sealed class HttpTrackingMiddleware
    {
        private readonly RequestDelegate next_;

        /// <summary>
        /// Default value for the TrackingId response header.
        /// This value can be changed by specifying the TrackingIdPropertyName
        /// in the <see cref="HttpTrackingOptions"/> class passed to the ctor.
        /// </summary>
        private readonly string trackingIdPropertyName_ = "http-tracking-id";

        private readonly IHttpTrackingStore storage_;
        private readonly long maxRequestLength_ = Int64.MaxValue;
        private readonly long maxResponseLength_ = Int64.MaxValue;

        /// <summary>
        /// Initialize a new instance of the <see cref="HttpTrackingMiddleware"/> class.
        /// </summary>
        /// <param name="next">A reference to the next OwinMiddleware object in the chain.</param>
        /// <param name="options">A reference to an <see cref="HttpTrackingOptions"/> class.</param>
        public HttpTrackingMiddleware(RequestDelegate next, HttpTrackingOptions options)
        {
            next_ = next;
            storage_ = options.TrackingStore;

            if (!string.IsNullOrEmpty(options.TrackingIdPropertyName))
                trackingIdPropertyName_ = options.TrackingIdPropertyName;

            maxRequestLength_ = options.MaximumRecordedRequestLength ?? maxRequestLength_;
            maxResponseLength_ = options.MaximumRecordedResponseLength ?? maxResponseLength_;
        }

        /// <summary>
        /// Processes the incoming HTTP call and capture details about
        /// the request, the response, the identity of the caller and the
        /// call duration to persistent storage.
        /// </summary>
        /// <param name="context">A reference to the HTTP context.</param>
        /// <returns />
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // capture details about the caller identity

            var identity =
                context.User != null && context.User.Identity.IsAuthenticated ?
                    context.User.Identity.Name :
                    "(anonymous)"
                    ;

            var record = new HttpEntry
            {
                CallerIdentity = identity,
            };

            // replace the request stream in order to intercept downstream reads

            var requestBuffer = new MemoryStream();
            var requestStream = new ContentStream(requestBuffer, request.Body)
            {
                MaxRecordedLength = maxRequestLength_,
            };
            request.Body = requestStream;

            // replace the response stream in order to intercept downstream writes

            var responseBuffer = new MemoryStream();
            var responseStream = new ContentStream(responseBuffer, response.Body)
            {
                MaxRecordedLength = maxResponseLength_,
            };
            response.Body = responseStream;

            // add the "Http-Tracking-Id" response header

            context.Response.OnStarting(OnSendingHeaders, (response, record));

            // invoke the next middleware in the pipeline

            await next_.Invoke(context);

            // rewind the request and response buffers
            // and record their content

            WriteRequestHeaders(context, record);
            record.RequestLength = requestStream.ContentLength;
            record.Request = await WriteContentAsync(requestStream, record.RequestHeaders, maxRequestLength_);

            WriteResponseHeaders(response, record);
            record.ResponseLength = responseStream.ContentLength;
            record.Response = await WriteContentAsync(responseStream, record.ResponseHeaders, maxResponseLength_);

            // persist details of the call to durable storage

            await storage_.InsertRecordAsync(record);
        }

        private Task OnSendingHeaders(object state)
        {
            if (state is ValueTuple<HttpResponse, HttpEntry> tuple)
            {
                var (response, record) = tuple;
                return OnSendingHeaders(response, record);
            }

            return Task.CompletedTask;
        }
        private async Task OnSendingHeaders(HttpResponse response, HttpEntry record)
        {
            // adding the tracking id response header so that the user
            // of the API can correlate the call back to this entry

            response.Headers.Add(trackingIdPropertyName_, new[] { record.TrackingId.ToString("d"), });

            await Task.CompletedTask;
        }

        private static void WriteRequestHeaders(HttpContext context, HttpEntry record)
        {
            var request = context.Request;

            record.Verb = request.Method;
            record.RequestUri = GetPath(context);
            record.RequestHeaders = request.Headers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
        }

        private static void WriteResponseHeaders(HttpResponse response, HttpEntry record)
        {
            record.StatusCode = response.StatusCode;
            record.ResponseHeaders = response.Headers.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
        }

        private static async Task<string> WriteContentAsync(ContentStream stream, IDictionary<string, string[]> headers, long maxLength)
        {
            const string ContentTypeHeaderName = "Content-Type";

            var contentType =
                headers.ContainsKey(ContentTypeHeaderName) ?
                headers[ContentTypeHeaderName][0] :
                null
                ;

            return await stream.ReadContentAsync(contentType, maxLength);
        }

        private static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}