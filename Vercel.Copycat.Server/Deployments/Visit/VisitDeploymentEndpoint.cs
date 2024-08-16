using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Deployments.Visit;

public static class VisitDeploymentEndpoint
{
    public static void MapVisitDeploymentEndpoint(this IEndpointRouteBuilder builder) => builder.MapGet(
        pattern: "apps/{projectId:guid}",
        handler: Handler);

    private const string AppEntryPoint = "index.html"; 
    
    private static async Task<IResult> Handler(
        [FromServices] IGrainFactory grains,
        [FromRoute] Guid projectId,
        HttpContext ctx)
    {
        var maybeDeploymentFile = await grains
            .GetGrain<IVisitDeployment>(0)
            .GetDeploymentFile(projectId, AppEntryPoint);
        ctx.Response.Cookies.Append("visit-deployment", projectId.ToString());
        return await ApiResultsUtils.FormatApiResultFormDeploymentFile(maybeDeploymentFile);
    }
}