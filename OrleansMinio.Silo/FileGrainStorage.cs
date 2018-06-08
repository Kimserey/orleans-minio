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
using Orleans.Hosting;

namespace OrleansMinio.Silo
{
    public class FileGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly string _rootDirectory;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        public FileGrainStorage(string storageName, string rootDirectory, IGrainFactory grainFactory, ITypeResolver typeResolver)
        {
            _storageName = storageName;
            _rootDirectory = rootDirectory;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = grainReference.ToKeyString() + "." + grainType;
            var path = Path.Combine(_rootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
                fileInfo.Delete();

            return Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = grainReference.ToKeyString() + "." + grainType;
            var path = Path.Combine(_rootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                return;

            using (var stream = fileInfo.OpenText())
            {
                var storedData = await stream.ReadToEndAsync();
                grainState.State = JsonConvert.DeserializeObject(storedData, grainState.State.GetType(), _jsonSettings);
            }
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var storedData = JsonConvert.SerializeObject(grainState.State, _jsonSettings);

            var fName = grainReference.ToKeyString() + "." + grainType;
            var path = Path.Combine(_rootDirectory, fName);

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

            var directory = new System.IO.DirectoryInfo(_rootDirectory);
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
            var options = optionsSnapshot.Get(name);
            return ActivatorUtilities.CreateInstance<FileGrainStorage>(services, name, options.RootDirectory);
        }
    }

    public static class FileSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddFileGrainStorage(this ISiloHostBuilder builder, string providerName, Action<FileGrainStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddFileGrainStorage(providerName, ob => ob.Configure(options)));
        }

        public static IServiceCollection AddFileGrainStorage(this IServiceCollection services, string providerName, Action<OptionsBuilder<FileGrainStorageOptions>> options)
        {
            options?.Invoke(services.AddOptions<FileGrainStorageOptions>(providerName));
            return services
                .AddSingletonNamedService(providerName, FileGrainStorageFactory.Create)
                .AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }
}
