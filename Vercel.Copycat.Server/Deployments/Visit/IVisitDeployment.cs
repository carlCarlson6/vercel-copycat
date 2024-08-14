using Orleans.Concurrency;

namespace Vercel.Copycat.Server.Deployments.Visit;

[Alias(nameof(IVisitDeployment))]
public interface IVisitDeployment : IGrainWithIntegerKey
{
    [Alias(nameof(GetDeploymentFile))]
    Task<DeploymentFile?> GetDeploymentFile(Guid projectId, string file);
}

[StatelessWorker]
public class VisitDeployment(IGrainFactory grains) : Grain, IVisitDeployment
{
    public async Task<DeploymentFile?> GetDeploymentFile(Guid projectId, string file)
    {
        var deployment = await grains.GetGrain<ICurrentDeployment>(projectId).GetDeployment();
        return await deployment.GetFile(file);
    }
}