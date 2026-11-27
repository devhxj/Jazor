namespace Jero.Web.Pages.Logs;

public sealed record LogOption
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}

public sealed record LogRow
{
    public required string Id { get; init; }
    public required string ControllerName { get; init; }
    public required string DisplayTitle { get; init; }
    public required string ActionName { get; init; }
    public required string HttpMethod { get; init; }
    public required string RequestUrl { get; init; }
    public required string EventId { get; init; }
    [ECMAScriptName("threadId")]
    public required string ThreadId { get; init; }
    [ECMAScriptName("traceId")]
    public required string TraceId { get; init; }
    public required string Account { get; init; }
    public required string RealName { get; init; }
    public required string RemoteIp { get; init; }
    public required string Location { get; init; }
    public required string Browser { get; init; }
    public required string Os { get; init; }
    public required string Longitude { get; init; }
    public required string Latitude { get; init; }
    public required string Status { get; init; }
    public required string Elapsed { get; init; }
    public required string LogDateTime { get; init; }
    public required string Exception { get; init; }
    public required string LogLevel { get; init; }
}
