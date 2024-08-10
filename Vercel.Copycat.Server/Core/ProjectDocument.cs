namespace Vercel.Copycat.Server.Core;

public record ProjectDocument(string Id, string Name, string RepoUrl, string BuildOutputPath)
{
    public Guid ProjectId() => Guid.Parse(Id.Replace("projects:", ""));
    
    public static string BuildDocId(Guid id) => $"projects:{id.ToString()}";
}

public record ProjectEventsStreamDocument(string Id, List<Event> Events)
{
    public static string BuildDocId(string projectDocId) => $"events:{projectDocId}";
};