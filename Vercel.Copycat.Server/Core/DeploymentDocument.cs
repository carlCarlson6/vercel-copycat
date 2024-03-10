using System.Text.Json.Serialization;

namespace Vercel.Copycat.Server.Core;

public class DeploymentDocument
{
    public string Id { get; }
    public string RepoUrl { get; }
    
    [JsonConstructor]
    public DeploymentDocument(string id, string repoUrl)
    {
        Id = id;
        RepoUrl = repoUrl;
    }

    public string DeploymentId() => Id.Replace("deployments/", "");
    
    public static string BuildDocId(string id) => $"deployments/{id}";
}