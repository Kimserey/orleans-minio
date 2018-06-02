# Minio Orleans Grain Storage

Minio implementation for Orleans Grain Storage

```c#
var silo = new SiloHostBuilder()
    .AddMinioGrainStorage("Minio", opts =>
    {
        opts.AccessKey = config["MINIO_ACCESS_KEY"];
        opts.SecretKey = config["MINIO_SECRET_KEY"];
        opts.Endpoint = "localhost:9000";
        opts.Container = "ek-grain-state";
    })
```