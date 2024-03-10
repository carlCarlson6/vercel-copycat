using Mediator;
using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Services.Upload;

[assembly: MediatorOptions(ServiceLifetime = ServiceLifetime.Scoped)]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration, builder.Environment);

var serverApp = builder.Build();

serverApp.MapGet("/", () => "Hello World!");
serverApp.MapCreateDeploymentEndpoint();

serverApp.Run();