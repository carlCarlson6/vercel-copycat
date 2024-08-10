using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Projects.Create;
using Vercel.Copycat.Server.VisitDeployment;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<VisitDeploymentAssestsMiddleware>()
    .AddServices(builder.Configuration, builder.Environment);

var serverApp = builder.Build();

serverApp.MapGet("/", () => "Hello World!");
serverApp.MapCreateProjectEndpoint();
serverApp.MapVisitDeploymentEndpoint();
serverApp.UseMiddleware<VisitDeploymentAssestsMiddleware>();

serverApp.Run();