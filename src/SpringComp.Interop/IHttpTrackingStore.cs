using System.Threading.Tasks;

namespace SpringComp.Interop
{
    /// <summary>
    /// Interface for tracking details about HTTP calls.
    /// </summary>
    public interface IHttpTrackingStore
    {
        /// <summary>
        /// Persist details of an HTTP call into durable storage.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task InsertRecordAsync(HttpEntry record);
    }
}