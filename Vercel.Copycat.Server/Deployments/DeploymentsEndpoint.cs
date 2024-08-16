using Microsoft.AspNetCore.Mvc;
using Vercel.Copycat.Server.Projects;

namespace Vercel.Copycat.Server.Deployments;

public static class DeploymentsEndpoint
{
    public static void MapDeploymentsEndpoint(this IEndpointRouteBuilder builder) => builder.MapPost(
        pattern: "api/{projectId:guid}/deployments",
        handler: Handler);

    private static async Task<IResult> Handler(
        [FromServices] IGrainFactory grains,
        [FromRoute] Guid projectId)
    {
        await grains.GetGrain<IProject>(projectId).ExecuteNewDeployment();
        return Results.Created();
    }
}

public static class CurrentDeploymentEndpoint
{
    public static void MapUpdateCurrentDeploymentEndpoint(this IEndpointRouteBuilder builder) => builder.MapPut(
        pattern: "api/{projectId:guid}/deployments/current/{deploymentId:guid}",
        handler: Handler);

    private static async Task<IResult> Handler(
        [FromServices] IGrainFactory grains,
        [FromRoute] Guid projectId,
        [FromRoute] Guid deploymentId)
    {
        // TODO - validate input deployment exists
        await grains.GetGrain<ICurrentDeployment>(projectId).SetDeployment(deploymentId);
        return Results.Ok();
    }
}