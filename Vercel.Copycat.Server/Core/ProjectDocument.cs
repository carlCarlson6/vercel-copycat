namespace Vercel.Copycat.Server.Core;

public record ProjectDocument(string Id, string Name ,string RepoUrl)
{
    public Guid ProjectId() => Guid.Parse(Id.Replace("deployments/", ""));
    
    public static string BuildDocId(Guid id) => $"deployments/{id.ToString()}";
}

public record ProjectEventsStreamDocument(string Id, List<Event> Events);