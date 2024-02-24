namespace Vercel.Copycat.Server.Core;

public interface IGit
{
    Task Clone(string repoUrl);
}

public class GitCli : IGit
{
    public Task Clone(string repoUrl)
    {
        throw new NotImplementedException();
    }
}