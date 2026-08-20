namespace JazorAdmin.Features.Audit;

public sealed record AuditEventView(
    string Id,
    string OccurredAt,
    string? ActorId,
    string? ActorName,
    string Action,
    string ObjectType,
    string ObjectId,
    string? Summary);

public sealed class AuditQuery
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public string? Actor { get; init; }

    public string? Object { get; init; }

    public string? Action { get; init; }
}
