using Mediator;
using OneOf;
using Vercel.Copycat.Server.Services.Build;
using static System.Text.Json.JsonSerializer;

namespace Vercel.Copycat.Server.Core;

[GenerateOneOf]
public partial class Event : OneOfBase<
    ProjectCreated,
    DeploymentCodeUploaded
>, IEvent
{
    public Guid Id => Match(
        x => x.Id,
        x => x.Id);
    
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
    public Guid Id { get; }
    public Guid ProjectId { get; }
}

public record ProjectCreated(Guid Id, Guid ProjectId) : IEvent;

public interface IEventConsumer
{
    ValueTask<Unit> Consume(Event @event);
}