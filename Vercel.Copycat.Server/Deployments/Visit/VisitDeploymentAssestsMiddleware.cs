namespace Vercel.Copycat.Server.Deployments.Visit;

public class VisitDeploymentAssestsMiddleware(IGrainFactory grains) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var (shouldSkipMiddleware, projectId) = ShouldSkipMiddlewareAction(context);
        if (shouldSkipMiddleware)
        {
            await next(context);
            return;
        }

        var maybeDeploymentFile = await grains
            .GetGrain<IVisitDeployment>(0)
            .GetDeploymentFile(projectId, context.Request.Path);
        var result = await ApiResultsUtils.FormatApiResultFormDeploymentFile(maybeDeploymentFile);
        await result.ExecuteAsync(context);
    }

    private static (bool shouldSkipMiddlewareAction, Guid projectId) ShouldSkipMiddlewareAction(HttpContext context)
    {
        context.Request.Cookies.TryGetValue("visit-deployment", out var projectId);
        var isProjectIdEmpty = string.IsNullOrWhiteSpace(projectId); 
        var routeIsForApi = context.Request.Path.Value!.StartsWith("/api");
        var routeIsForApps = context.Request.Path.Value.StartsWith("/apps");
        return (
            shouldSkipMiddlewareAction: isProjectIdEmpty || routeIsForApi || routeIsForApps,
            projectId: isProjectIdEmpty ? new Guid(): Guid.Parse(projectId!));
    }
}