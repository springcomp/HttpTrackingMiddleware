using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpringComp.Owin
{
    /// <summary>
    /// A class to wrap a stream for interception purposes
    /// and recording the number of bytes written to or read from
    /// the wrapped stream.
    /// </summary>
    public class ContentStream : Stream
    {
        protected readonly Stream buffer_;
        protected readonly Stream stream_;

        private long contentLength_ = 0L;
        
        /// <summary>
        /// Initialize a new instance of the <see cref="ContentStream"/> class.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="stream"></param>
        public ContentStream(Stream buffer, Stream stream)
        {
            buffer_ = buffer;
            stream_ = stream;
        }

        /// <summary>
        /// Returns the recorded length of the underlying stream.
        /// </summary>
        public virtual long ContentLength
        {
            get { return contentLength_; }
        }

        /// <summary>
        /// Reads the content of the stream as a string.
        /// 
        /// If the contentType is not specified (null) or does not
        /// refer to a string, this function returns the content type
        /// followed by the number of bytes in the response.
        /// 
        /// If the contentType is one of the following values, the
        /// resulting content is decoded as a string and truncated to
        /// the maximum count specified.
        /// </summary>
        /// <param name="contentType">HTTP header content type.</param>
        /// <param name="maxCount">Max number of bytes returned from the stream.</param>
        /// <returns></returns>
        public async Task<String> ReadContentAsync(string contentType, long maxCount)
        {
            if (!IsTextContentType(contentType))
            {
                contentType = String.IsNullOrEmpty(contentType) ? "N/A" : contentType;
                return String.Format("{0} [{1} bytes]", contentType, ContentLength);
            }

            buffer_.Seek(0, SeekOrigin.Begin);

            var length = Math.Min(ContentLength, maxCount);

            var buffer = new byte[length];
            var count = await buffer_.ReadAsync(buffer, 0, buffer.Length);

            return
                GetEncoding(contentType)
                .GetString(buffer, 0, count)
                ;
        }

        protected void WriteContent(byte[] buffer, int offset, int count)
        {
            buffer_.Write(buffer, offset, count);
        }

        #region Implementation

        private static bool IsTextContentType(string contentType)
        {
            if (contentType == null)
                return false;

            var isTextContentType =
                contentType.StartsWith("application/json") ||
                contentType.StartsWith("application/xml") ||
                contentType.StartsWith("text/")
                ;
            return isTextContentType;
        }

        private static Encoding GetEncoding(string contentType)
        {
            var charset = "utf-8";
            var regex = new Regex(@";\s*charset=(?<charset>[^\s;]+)");
            var match = regex.Match(contentType);
            if (match.Success)
                charset = match.Groups["charset"].Value;

            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (ArgumentException e)
            {
                return Encoding.UTF8;
            }
        }

        #endregion

        #region System.IO.Stream Overrides

        public override bool CanRead
        {
            get { return stream_.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream_.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return stream_.CanWrite; }
        }

        public override void Flush()
        {
            stream_.Flush();
        }

        public override long Length
        {
            get { return stream_.Length; }
        }

        public override long Position
        {
            get { return stream_.Position; }
            set { stream_.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // read content from the request stream

            count = stream_.Read(buffer, offset, count);
            contentLength_ += count;

            // record the read content into our temporary buffer

            if (count != 0)
                WriteContent(buffer, offset, count);

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream_.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream_.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // store the bytes into our local stream

            WriteContent(buffer, 0, count);

            // write the bytes to the response stream
            // and record the actual number of bytes written

            stream_.Write(buffer, offset, count);
            contentLength_ += count;
        }

        #endregion
       
        #region IDisposable Implementation

        protected override void Dispose(bool disposing)
        {
            buffer_.Dispose();
        }

        #endregion
    }
}