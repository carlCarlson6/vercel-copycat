using Microsoft.AspNetCore.Mvc;

namespace Vercel.Copycat.Server.Deployments.Visit;

public static class VisitDeploymentEndpoint
{
    public static void MapVisitDeploymentEndpoint(this IEndpointRouteBuilder builder) => builder.MapGet(
        pattern: "apps/{projectId:guid}",
        handler: async (
            [FromServices] IGrainFactory grains,
            [FromRoute] Guid projectId, 
            HttpContext ctx) =>
        {
            var result = await grains
                .GetGrain<IVisitDeployment>(0)
                .GetDeploymentFile(projectId, "index.html");
            ctx.Response.Cookies.Append("visit-deployment", projectId.ToString());
            return result; 
        });
}