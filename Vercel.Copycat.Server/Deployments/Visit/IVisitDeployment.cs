using Orleans.Concurrency;

namespace Vercel.Copycat.Server.Deployments.Visit;

[Alias(nameof(IVisitDeployment))]
public interface IVisitDeployment : IGrainWithIntegerKey
{
    [Alias(nameof(GetDeploymentFile))]
    Task<IResult> GetDeploymentFile(Guid projectId, string file);
}

[StatelessWorker]
public class VisitDeployment(IGrainFactory grains, HttpContext ctx) : Grain, IVisitDeployment
{
    // TODO - check setting cookie on http context
    // can be injected or needs to be passed down?
    public async Task<IResult> GetDeploymentFile(Guid projectId, string file)
    {
        var deployment = await grains.GetGrain<ICurrentDeployment>(projectId).GetDeployment();
        var deploymentFile = await deployment.GetFile(file);
        return deploymentFile is null
            ? Results.NotFound()
            : Results.Redirect(deploymentFile.FileUri.ToString(), preserveMethod: true); // TODO - use redirect
    }
}