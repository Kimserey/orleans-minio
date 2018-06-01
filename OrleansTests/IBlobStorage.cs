using System;
using System.IO;
using System.Threading.Tasks;

namespace OrleansTests
{
    public interface IBlobStorage
    {
        Task<Stream> ReadBlob(string blobContainer, string prefix, string blobName);
        Task<Stream> ReadBlob(string blobContainer, string prefix, Guid blobKey);
        Task UploadBlob(string blobContainer, string prefix, string blobName, Stream blob, string contentType = null);
        Task UploadBlob(string blobContainer, string prefix, Guid blobKey, Stream blob, string contentType = null);
        Task DeleteBlob(string blobContainer, string prefix, string blobName);
        Task DeleteBlob(string blobContainer, string prefix, Guid blobKey);
        Task<BlobStorageConnectionStatus> CheckConnection();
    }
}
