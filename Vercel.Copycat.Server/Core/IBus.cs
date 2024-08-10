using Mediator;

namespace Vercel.Copycat.Server.Core;

public interface IBus
{
    Task<Unit> Publish(Event @event);
}