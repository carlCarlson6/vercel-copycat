using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Projects;
using Vercel.Copycat.Server.VisitDeployment;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(x => x.UseLocalhostClustering()); 

builder.Services
    .AddServices(builder.Configuration, builder.Environment);

var serverApp = builder.Build();

serverApp.MapGet("/", () => "Hello World!");
serverApp.MapCreateProjectEndpoint();
serverApp.MapVisitDeploymentEndpoint();
serverApp.UseMiddleware<VisitDeploymentAssestsMiddleware>();

serverApp.Run();