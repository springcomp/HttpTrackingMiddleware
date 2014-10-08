using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SpringComp.Owin.Interop;

namespace App
{
    /// <summary>
    /// Dummy implementation of the <see cref="HttpEntry"/> interface to file, for illustration purposes.
    /// </summary>
    public sealed class HttpTrackingStore : IHttpTrackingStore
    {
        private readonly string path_ = Path.GetTempPath();

        public async System.Threading.Tasks.Task InsertRecordAsync(HttpEntry record)
        {
            var path = Path.Combine(path_, record.TrackingId.ToString("d"));
            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
                await writer.WriteAsync(JsonConvert.SerializeObject(record));

            Console.WriteLine("Verb: {0}", record.Verb);
            Console.WriteLine("RequestUri: {0}", record.RequestUri);
            Console.WriteLine("Request: {0}", record.Request);
            Console.WriteLine("RequestLength: {0}", record.RequestLength);

            Console.WriteLine("StatusCode: {0}", record.StatusCode);
            Console.WriteLine("ReasonPhrase: {0}", record.ReasonPhrase);
            Console.WriteLine("Response: {0}", record.Response);
            Console.WriteLine("Content-Length: {0}", record.ResponseLength);

            Console.WriteLine("FILE {0} saved.", path);
        }
    }
}