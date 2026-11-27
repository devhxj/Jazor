using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record OnlineUserTenantOption : VueProps
{
    public required string Value { get; init; }
    public required string Label { get; init; }
    public required string Host { get; init; }
}

public sealed record OnlineUserView : VueProps
{
    public required string Id { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public required string UserName { get; init; }
    public required string RealName { get; init; }
    public required string Time { get; init; }
    public required string Ip { get; init; }
    public required string Browser { get; init; }
    public required string Os { get; init; }
}
