using Minio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OrleansTests
{
    public class MinioStorage : IBlobStorage
    {
        private string _accessKey;
        private string _secretKey;
        private string _endpoint;
        private string _containerPrefix;

        public MinioStorage(string accessKey, string secretKey, string endpoint, string containerPrefix)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
                throw new ArgumentException("Minio 'accessKey' is missing.");

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Minio 'secretKey' is missing.");

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Minio 'endpoint' is missing.");

            if (string.IsNullOrWhiteSpace(containerPrefix))
                throw new ArgumentException("Minio 'containerPrefix' is missing.");

            _accessKey = accessKey;
            _secretKey = secretKey;
            _endpoint = endpoint;
            _containerPrefix = containerPrefix;
        }

        private (MinioClient client, string bucket, string objectName) GetStorage(string blobContainer, string prefix, string blobName)
        {
            string appendPrefix(string p, string v) => $"{p}-{v}";
            string appendBlobPrefix(string c) => appendPrefix(_containerPrefix, c);
            string bucket = appendBlobPrefix(blobContainer);
            string objectName = appendPrefix(prefix, blobName);
            return (new MinioClient(_endpoint, _accessKey, _secretKey), bucket, objectName);
        }

        public async Task DeleteBlob(string blobContainer, string prefix, string blobName)
        {
            var (client, bucket, objectName) =
                GetStorage(blobContainer, prefix, blobName);

            await client.RemoveObjectAsync(bucket, objectName);
        }

        public Task DeleteBlob(string blobContainer, string prefix, Guid blobKey)
        {
            return DeleteBlob(blobContainer, prefix, blobKey.ToString());
        }

        public async Task<Stream> ReadBlob(string blobContainer, string prefix, string blobName)
        {
            var (client, bucket, objectName) =
                GetStorage(blobContainer, prefix, blobName);

            var ms = new MemoryStream();
            await client.GetObjectAsync(bucket, objectName, stream =>
            {
                stream.CopyTo(ms);
            });
            ms.Position = 0;
            return ms;
        }

        public Task<Stream> ReadBlob(string blobContainer, string prefix, Guid blobKey)
        {
            return ReadBlob(blobContainer, prefix, blobKey.ToString());
        }

        public async Task UploadBlob(string blobContainer, string prefix, string blobName, Stream blob, string contentType = null)
        {
            var (client, container, name) =
                GetStorage(blobContainer, prefix, blobName);

            if (!await client.BucketExistsAsync(container))
            {
                await client.MakeBucketAsync(container);
            }

            await client.PutObjectAsync(container, name, blob, blob.Length, contentType: contentType);
        }

        public Task UploadBlob(string blobContainer, string prefix, Guid blobKey, Stream blob, string contentType = null)
        {
            return UploadBlob(blobContainer, prefix, blobKey.ToString(), blob, contentType);
        }

        public async Task<BlobStorageConnectionStatus> CheckConnection()
        {
            try
            {
                var client = new MinioClient(_endpoint, _accessKey, _secretKey);
                var buckets = await client.ListBucketsAsync();
                return new BlobStorageConnectionStatus
                {
                    Success = true,
                    Message = $"{buckets.Buckets.Count} bucket(s) found in Minio Storage."
                };
            }
            catch (Exception ex)
            {
                return new BlobStorageConnectionStatus
                {
                    Success = false,
                    Message = "Minio Storage check failed. " + ex.Message
                };
            }
        }
    }
}
