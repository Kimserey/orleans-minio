using Microsoft.Extensions.Logging;
using Minio;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OrleansMinio.Storage
{
    public class MinioStorage : IMinioStorage
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _containerPrefix;
        private readonly ILogger<MinioStorage> _logger;

        public MinioStorage(ILogger<MinioStorage> logger, string accessKey, string secretKey, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
                throw new ArgumentException("Minio 'accessKey' is missing.");

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Minio 'secretKey' is missing.");

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Minio 'endpoint' is missing.");

            _accessKey = accessKey;
            _secretKey = secretKey;
            _endpoint = endpoint;
            _logger = logger;
        }

        public MinioStorage(ILogger<MinioStorage> logger, string accessKey, string secretKey, string endpoint, string containerPrefix)
            : this(logger, accessKey, secretKey, endpoint)
        {
            if (string.IsNullOrWhiteSpace(containerPrefix))
                throw new ArgumentException("Minio 'containerPrefix' is missing.");

            _containerPrefix = containerPrefix;
        }

        private MinioClient CreateMinioClient() => new MinioClient(_endpoint, _accessKey, _secretKey);

        private string AppendPrefix(string prefix, string value) => string.IsNullOrEmpty(prefix) ? value : $"{prefix}-{value}";

        private string AppendContainerPrefix(string container) => string.IsNullOrEmpty(_containerPrefix) ? container : AppendPrefix(_containerPrefix, container);

        private (MinioClient client, string bucket, string objectName) GetStorage(string blobContainer, string prefix, string blobName) => (CreateMinioClient(), AppendContainerPrefix(blobContainer), AppendPrefix(prefix, blobName));

        public Task<bool> ContainerExits(string blobContainer)
        {
            return CreateMinioClient().BucketExistsAsync(AppendContainerPrefix(blobContainer));
        }

        public async Task DeleteBlob(string blobContainer, string blobName, string blobPrefix = null)
        {
            var (client, bucket, objectName) =
                GetStorage(blobContainer, blobPrefix, blobName);

            await client.RemoveObjectAsync(bucket, objectName);
        }

        public Task DeleteBlob(string blobContainer, Guid blobKey, string blobPrefix = null)
        {
            return DeleteBlob(blobContainer, blobKey.ToString(), blobPrefix);
        }

        public async Task<Stream> ReadBlob(string blobContainer, string blobName, string blobPrefix = null)
        {
            var (client, bucket, objectName) =
                GetStorage(blobContainer, blobPrefix, blobName);

            var ms = new MemoryStream();
            await client.GetObjectAsync(bucket, objectName, stream =>
            {
                stream.CopyTo(ms);
            });
            ms.Position = 0;
            return ms;
        }

        public Task<Stream> ReadBlob(string blobContainer, Guid blobKey, string blobPrefix = null)
        {
            return ReadBlob(blobContainer, blobKey.ToString(), blobPrefix);
        }

        public async Task UploadBlob(string blobContainer, string blobName, Stream blob, string blobPrefix = null, string contentType = null)
        {
            var (client, container, name) =
                GetStorage(blobContainer, blobPrefix, blobName);

            if (!await client.BucketExistsAsync(container))
            {
                await client.MakeBucketAsync(container);
            }

            await client.PutObjectAsync(container, name, blob, blob.Length, contentType: contentType);
        }

        public Task UploadBlob(string blobContainer, Guid blobKey, Stream blob, string blobPrefix = null, string contentType = null)
        {
            return UploadBlob(blobContainer, blobKey.ToString(), blob, blobPrefix, contentType);
        }

        public async Task<MinioStorageConnectionStatus> CheckConnection()
        {
            try
            {
                var client = new MinioClient(_endpoint, _accessKey, _secretKey);
                var buckets = await client.ListBucketsAsync();
                return new MinioStorageConnectionStatus
                {
                    Success = true,
                    Message = $"{buckets.Buckets.Count} bucket(s) found in Minio Storage."
                };
            }
            catch (Exception ex)
            {
                return new MinioStorageConnectionStatus
                {
                    Success = false,
                    Message = "Minio Storage check failed. " + ex.Message
                };
            }
        }
    }
}
