namespace Vercel.Copycat.Server.Deployments.Visit;

public class VisitDeploymentAssestsMiddleware(IGrainFactory grains) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Request.Cookies.TryGetValue("visit-deployment", out var projectId);
        if (string.IsNullOrWhiteSpace(projectId))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.Value!.StartsWith("/api")|| context.Request.Path.Value.StartsWith("/apps"))
        {
            await next(context);
            return;
        }

        var maybeDeploymentFile = await grains
            .GetGrain<IVisitDeployment>(0)
            .GetDeploymentFile(Guid.Parse(projectId), context.Request.Path);
        var result = await maybeDeploymentFile.ToApiResponse();
        await result.ExecuteAsync(context);
    }
}