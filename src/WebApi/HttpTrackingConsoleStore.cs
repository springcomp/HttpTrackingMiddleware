using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SpringComp.Interop;

namespace App
{
    /// <summary>
    /// Dummy implementation of the <see cref="HttpEntry"/> interface to file, for illustration purposes.
    /// </summary>
    public sealed class HttpTrackingConsoleStore : IHttpTrackingStore
    {
        private readonly string path_ = Path.GetTempPath();

        public async System.Threading.Tasks.Task InsertRecordAsync(HttpEntry record)
        {
            var path = Path.Combine(path_, record.TrackingId.ToString("d"));

            await using (var stream = File.OpenWrite(path))
            await using (var writer = new StreamWriter(stream))
                await writer.WriteAsync(JsonSerializer.Serialize(record));

            Console.WriteLine("Verb: {0}", record.Verb);
            Console.WriteLine("RequestUri: {0}", record.RequestUri);
            WriteHeaders(record.RequestHeaders);
            Console.WriteLine("Request: {0}", record.Request);
            Console.WriteLine("RequestLength: {0}", record.RequestLength);
            Console.WriteLine();

            Console.WriteLine("StatusCode: {0}", record.StatusCode);
            WriteHeaders(record.ResponseHeaders);
            Console.WriteLine("Response: {0}", record.Response);
            Console.WriteLine("Content-Length: {0}", record.ResponseLength);
            Console.WriteLine();

            Console.WriteLine("FILE {0} saved.", path);
        }

        private static void WriteHeaders(IDictionary<string, string[]> headers)
        {
            foreach (var (key, value) in headers)
                Console.WriteLine($"{key}: {string.Join(", ", value)}");
        }
    }
}