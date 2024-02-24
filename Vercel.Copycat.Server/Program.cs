using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Services.Upload;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHostedService<DirectoriesCreator>();

var serverApp = builder.Build();

serverApp.MapUploadRepoEndpoint();
serverApp.MapGet("/", () => "Hello World!");

serverApp.Run();