using Mediator;
using OneOf;
using Vercel.Copycat.Server.Projects.Create;
using Vercel.Copycat.Server.Services.Build;
using static System.Text.Json.JsonSerializer;

namespace Vercel.Copycat.Server.Core;

[GenerateOneOf]
public partial class Event : OneOfBase<
    ProjectCreated,
    DeploymentCodeUploaded
>, IEvent
{
    public Guid EventId => Match(
        x => x.EventId,
        x => x.EventId);
    
    public Guid ProjectId => Match(
        x => x.ProjectId,
        x => x.ProjectId);
    
    public string ToSerializedMessage() => Match(
        x => Serialize(x),
        x => Serialize(x));

    public string TypeName => Match(
        x => x.GetType().Name,
        x => x.GetType().Name);
}

public interface IEvent : IRequest
{
    public Guid EventId { get; }
    public Guid ProjectId { get; }
}

public interface IEventConsumer
{
    ValueTask<Unit> Consume(Event @event);
}