using System;
using System.IO;
using System.Threading.Tasks;

namespace OrleansMinio.Storage
{
    internal static class MinioStorageExtensions
    {
        public static Task DeleteBlob(this IMinioStorage storage, string blobContainer, Guid blobKey, string blobPrefix = null)
        {
            return storage.DeleteBlob(blobContainer, blobKey.ToString(), blobPrefix);
        }

        public static Task<Stream> ReadBlob(this IMinioStorage storage, string blobContainer, Guid blobKey, string blobPrefix = null)
        {
            return storage.ReadBlob(blobContainer, blobKey.ToString(), blobPrefix);
        }

        public static Task UploadBlob(this IMinioStorage storage, string blobContainer, Guid blobKey, Stream blob, string blobPrefix = null, string contentType = null)
        {
            return storage.UploadBlob(blobContainer, blobKey.ToString(), blob, blobPrefix, contentType);
        }
    }
}
