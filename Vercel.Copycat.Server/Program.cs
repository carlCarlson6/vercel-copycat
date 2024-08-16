using Vercel.Copycat.Server.Deployments;
using Vercel.Copycat.Server.Deployments.Visit;
using Vercel.Copycat.Server.Infrastructure;
using Vercel.Copycat.Server.Projects;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(x => x.UseLocalhostClustering()); 

builder.Services
    .AddServices(builder.Configuration, builder.Environment);

var serverApp = builder.Build();

serverApp.UseMiddleware<VisitDeploymentAssestsMiddleware>();
serverApp.MapGet("/", () => "Hello World!");
serverApp.MapDeploymentsEndpoint();
serverApp.MapUpdateCurrentDeploymentEndpoint();
serverApp.MapCreateProjectEndpoint();
serverApp.MapVisitDeploymentEndpoint();

serverApp.Run();