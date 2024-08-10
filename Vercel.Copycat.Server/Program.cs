using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Projects.Create;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration, builder.Environment);

var serverApp = builder.Build();

serverApp.MapGet("/", () => "Hello World!");
serverApp.MapCreateProjectEndpoint();

serverApp.Run();