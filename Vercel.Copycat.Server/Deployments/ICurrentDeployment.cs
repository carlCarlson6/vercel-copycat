namespace Vercel.Copycat.Server.Deployments;

[Alias(nameof(ICurrentDeployment))]
public interface ICurrentDeployment : IGrainWithGuidKey
{
    [Alias(nameof(SetDeployment))]
    Task SetDeployment(Guid deploymentId);
    
    [Alias(nameof(GetDeployment))]
    Task<IDeployment> GetDeployment();
}