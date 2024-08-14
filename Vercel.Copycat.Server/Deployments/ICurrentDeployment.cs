using Orleans.Runtime;
using static Vercel.Copycat.Server.Infrastructure.ServiceCollectionExtensions;

namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(ICurrentDeployment))]
public interface ICurrentDeployment : IGrainWithGuidKey
{
    [Alias(nameof(SetDeployment))]
    Task SetDeployment(Guid deploymentId);
    
    [Alias(nameof(GetDeployment))]
    Task<IDeployment> GetDeployment();
}

public class CurrentDeployment(
    [PersistentState(
        stateName: "current-deployment", 
        storageName: CacheStorageName)
    ] IPersistentState<Guid> persistentCurrentDeployment,
    IGrainFactory grains
) 
    : Grain, ICurrentDeployment
{
    public Task SetDeployment(Guid deploymentId)
    {
        persistentCurrentDeployment.State = deploymentId;
        return persistentCurrentDeployment.WriteStateAsync();
    }

    public Task<IDeployment> GetDeployment()
    {
        var grain = grains.GetGrain<IDeployment>(persistentCurrentDeployment.State);
        return Task.FromResult(grain);
    }
}