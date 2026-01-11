var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OsService_ApiService>("apiservice")
    .WithExternalHttpEndpoints();

builder.Build().Run();
