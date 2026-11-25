namespace Jero.Web.App.OnlineUser;

public sealed partial record OnlineUserTenantOption
{
    public required string value { get; init; }
    public required string label { get; init; }
    public required string host { get; init; }
}

public sealed partial record OnlineUserView
{
    public required string id { get; init; }
    public string tenantId { get; init; } = string.Empty;
    public string connectionId { get; init; } = string.Empty;
    public string userId { get; init; } = string.Empty;
    public required string userName { get; init; }
    public required string realName { get; init; }
    public required string time { get; init; }
    public required string ip { get; init; }
    public required string browser { get; init; }
    public required string os { get; init; }
}
