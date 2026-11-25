namespace Jero.Web.App.Logs;

public sealed record LogOption
{
    public required string value { get; init; }
    public required string label { get; init; }
}

public sealed record LogRow
{
    public required string id { get; init; }
    public required string controllerName { get; init; }
    public required string displayTitle { get; init; }
    public required string actionName { get; init; }
    public required string httpMethod { get; init; }
    public required string requestUrl { get; init; }
    public required string eventId { get; init; }
    public required string threadId { get; init; }
    public required string traceId { get; init; }
    public required string account { get; init; }
    public required string realName { get; init; }
    public required string remoteIp { get; init; }
    public required string location { get; init; }
    public required string browser { get; init; }
    public required string os { get; init; }
    public required string longitude { get; init; }
    public required string latitude { get; init; }
    public required string status { get; init; }
    public required string elapsed { get; init; }
    public required string logDateTime { get; init; }
    public required string exception { get; init; }
    public required string logLevel { get; init; }
}
