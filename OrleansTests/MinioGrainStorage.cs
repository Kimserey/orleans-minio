using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OrleansTests
{
    public static class MinioSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddMinioGrainStorage(this ISiloHostBuilder builder, string providerName, Action<MinioGrainStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddMinioGrainStorage(providerName, ob => ob.Configure(options)));
        }

        public static IServiceCollection AddMinioGrainStorage(this IServiceCollection services, string providerName, Action<OptionsBuilder<MinioGrainStorageOptions>> options)
        {
            options?.Invoke(services.AddOptions<MinioGrainStorageOptions>(providerName));
            return services.AddSingletonNamedService(providerName, MinioGrainStorageFactory.Create);
        }
    }

    public class MinioGrainStorage : IGrainStorage
    {
        private readonly string _name;
        private readonly string _container;
        private readonly ILogger<MinioGrainStorage> _logger;
        private readonly IBlobStorage _storage;

        public MinioGrainStorage(string name, string container, ILogger<MinioGrainStorage> logger, IBlobStorage storage)
        {
            _name = name;
            _container = container;
            _logger = logger;
            _storage = storage;
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            string blobName = GetBlobNameString(grainType, grainReference);

            try
            {
                if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Reading: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4}", grainType, grainReference, grainState.ETag, blobName, _container);

                // TODO FIX when not exists
                var data = await _storage.ReadBlob(_container, "orleans", blobName);
                grainState.State = await ConvertToGrainState(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4} Exception={5}", grainType, grainReference, grainState.ETag, blobName, _container, ex.Message);

                throw ex;
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            string blobName = GetBlobNameString(grainType, grainReference);

            try
            {
                if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Writing: GrainType={0} Grainid={1} ETag={2} to BlobName={3} in Container={4}", grainType, grainReference, grainState.ETag, blobName, _container);

                var (data, mimeType) = await ConvertToStorageFormat(grainState.State);
                await _storage.UploadBlob(_container, "orleans", blobName, data, mimeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing: GrainType={0} Grainid={1} ETag={2} from BlobName={3} in Container={4} Exception={5}", grainType, grainReference, grainState.ETag, blobName, _container, ex.Message);

                throw ex;
            }
        }

        private string GetBlobNameString(string grainType, GrainReference grainReference)
        {
            return $"{grainType}-{grainReference.ToKeyString()}";
        }

        private async Task<(Stream, string)> ConvertToStorageFormat(object grainState)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    string data = JsonConvert.SerializeObject(grainState);
                    await writer.WriteLineAsync(data);
                    writer.Flush();
                    stream.Position = 0;
                }
                string mimeType = "application/json";
                return (stream, mimeType);
            }
        }

        private async Task<object> ConvertToGrainState(Stream data)
        {
            using (var reader = new StreamReader(data))
            {
                var json = await reader.ReadToEndAsync();
                var state = JsonConvert.DeserializeObject(json);
                return state;
            }
        }
    }

    public static class MinioGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<MinioGrainStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<MinioGrainStorageOptions>>();
            var options = optionsSnapshot.Get(name);
            IBlobStorage storage = new MinioStorage(options.AccessKey, options.SecretKey, options.Endpoint, name);
            return ActivatorUtilities.CreateInstance<MinioGrainStorage>(services, name, options.Container, storage);
        }
    }
}
