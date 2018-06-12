using Orleans;
using System;
using Orleans.Storage;
using Orleans.Runtime;
using System.Threading.Tasks;
using Orleans.Serialization;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration.Overrides;
using Orleans.Configuration;

namespace OrleansMinio.Silo
{
    public class FileGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly FileGrainStorageOptions _options;
        private readonly ClusterOptions _clusterOptions;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        public FileGrainStorage(string storageName, FileGrainStorageOptions options, IOptions<ClusterOptions> clusterOptions, IGrainFactory grainFactory, ITypeResolver typeResolver)
        {
            _storageName = storageName;
            _options = options;
            _clusterOptions = clusterOptions.Value;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
        }

        private string GetKeyString(string grainType, GrainReference grainReference)
        {
            return $"{_clusterOptions.ServiceId}.{grainReference.ToKeyString()}.{grainType}";
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                grainState.ETag = null;
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                fileInfo.Delete();
            }

            return Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                return;
            }

            using (var stream = fileInfo.OpenText())
            {
                var storedData = await stream.ReadToEndAsync();
                grainState.State = JsonConvert.DeserializeObject(storedData, _jsonSettings);
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var storedData = JsonConvert.SerializeObject(grainState.State, _jsonSettings);

            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);

            using (var stream = new StreamWriter(fileInfo.Open(FileMode.Create, FileAccess.Write)))
            {
                await stream.WriteAsync(storedData);
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<FileGrainStorage>(_storageName), ServiceLifecycleStage.ApplicationServices, Init);
        }

        private Task Init(CancellationToken ct)
        {
            _jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(OrleansJsonSerializer.GetDefaultSerializerSettings(_typeResolver, _grainFactory), false, false, null);

            var directory = new DirectoryInfo(_options.RootDirectory);
            if (!directory.Exists)
                directory.Create();

            return Task.CompletedTask;
        }
    }

    public class FileGrainStorageOptions
    {
        public string RootDirectory { get; set; }
    }

    public static class FileGrainStorageFactory
    {
        internal static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<FileGrainStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<FileGrainStorageOptions>>();
            return ActivatorUtilities.CreateInstance<FileGrainStorage>(services, name, optionsSnapshot.Get(name), services.GetProviderClusterOptions(name));
        }
    }
}
