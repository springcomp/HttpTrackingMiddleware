using System;
using System.Collections.Generic;

namespace SpringComp.Owin.Interop
{
    /// <summary>
    /// A simple class to hold details of an HTTP call.
    /// </summary>
    public sealed class HttpEntry
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="HttpEntry" /> class.
        /// </summary>
        public HttpEntry()
        {
            TrackingId = Guid.NewGuid();
            CallDateTime = DateTime.UtcNow;
        }

        /// <summary>
        /// An unique identifier for the HTTP call.
        /// </summary>
        public Guid TrackingId { get; private set; }

        /// <summary>
        /// Identity of the caller.
        /// </summary>
        public string CallerIdentity { get; set; }

        /// <summary>
        /// Timestamp at which the HTTP call took place.
        /// </summary>
        public DateTime CallDateTime { get; set; }

        /// <summary>
        /// Verb associated with the HTTP call.
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Http request URI.
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// Http request headers.
        /// </summary>
        public IDictionary<String, String[]> RequestHeaders { get; set; }

        /// <summary>
        /// Http request content-length
        /// In the case of chunked transfer encoding,
        /// the request headers do not mention the content length.
        /// </summary>
        public long RequestLength { get; set; }

        /// <summary>
        /// Http request body, if any.
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Http response status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Http response status line.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Http response headers.
        /// </summary>
        public IDictionary<String, String[]> ResponseHeaders { get; set; }

        /// <summary>
        /// Http response content-length
        /// In the case of chunked transfer encoding,
        /// the response headers do not mention the content length.
        /// </summary>
        public long ResponseLength { get; set; }

        /// <summary>
        /// Http response body.
        /// </summary>
        public string Response { get; set; }


        /// <summary>
        /// Timestamp representing the duration of the HTTP call.
        /// </summary>
        public TimeSpan CallDuration { get; set; }
    }
}