using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi
{
    /// <summary>
    /// A simple AspNet WebApi Controller for illustration purposes.
    /// </summary>
    [RoutePrefix("api/custom")]
    public sealed class CustomController : ApiController
    {
        /// <summary>
        /// GET http://host:port/api/custom/resources
        /// Returns a list of resources.
        /// </summary>
        /// <returns>A list of strings.</returns>
        [HttpGet]
        [Route("resources")]
        public IHttpActionResult GetAllResources()
        {
            var collection = new[]
            {
                "one",
                "two",
                "three",
            };


            return Ok(collection);
        }

        /// <summary>
        /// GET http://host:port/api/custom/stream
        /// Returns a streamed buffer.
        /// </summary>
        /// <returns>A streamed buffer.</returns>
        [HttpGet]
        [Route("stream/{count}")]
        public HttpResponseMessage GetStream(int count)
        {
            Action<Stream, HttpContent, TransportContext> onStreamAvailable = (stream, content, transport) =>
            {
                using (stream)
                {
                    foreach (var s in GetStrings(count))
                    {
                        var buffer = Encoding.UTF8.GetBytes(s);
                        stream.WriteAsync(buffer, 0, buffer.Length);
                    }

                    stream.FlushAsync();
                }
            };

            var response = Request.CreateResponse();
            response.Content = new PushStreamContent(onStreamAvailable);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return response;
        }

        private IEnumerable<string> GetStrings(int count)
        {
            for (var index = 0; index < count; index++)
                yield return "Sed ut perspiciatis nulla pariatur?\r\n";
        }


        [HttpGet]
        [Route("resources-async")]
        public async Task<IHttpActionResult>GetAllResourcesAsync()
        {
            var collection = new[]
            {
                "one",
                "two",
                "three",
            };

            var task = Task<IEnumerable<string>>.Factory.StartNew(() => collection);
            var c = await task;
            return Ok(c);
        }

        /// <summary>
        /// POST http://host:port/api/custom/post
        /// Reads the incoming HTTP Request stream.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("post-async")]
        public async Task<IHttpActionResult> PostStreamAsync()
        {
            // consume the stream, to simulate processing
            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                var count = 0;
                var buffer = new byte[4096];
                while ((count = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    /* do nothing */;
            }

            return Ok();
        }
    }
}