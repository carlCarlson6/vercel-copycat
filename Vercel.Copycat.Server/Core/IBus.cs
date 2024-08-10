using Mediator;
using StackExchange.Redis;

namespace Vercel.Copycat.Server.Core;

public interface IBus
{
    Task<Unit> Publish(Event @event);
    Task<Unit> Publish(Event @event, ITransaction transaction);
}