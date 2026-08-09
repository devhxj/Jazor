// Centralizes stable resource and operation keys used by APIs, persisted grants, and authorization policies.
// 集中定义 API、持久化授权和策略共用的稳定资源与操作键，避免分散的魔法字符串。
namespace JazorAdmin.Authorization;

public static class ResourceKeys
{
    public const string Organizations = "organizations";
    public const string Authorization = "authorization";
}

public static class OperationKeys
{
    public const string Read = "read";
    public const string Manage = "manage";
}

public static class RoleKeys
{
    public const string PlatformAdministrator = "platform-administrator";
}

public static class PolicyKeys
{
    public const string PlatformAdministrator = "jazoradmin.platform-administrator";
}

public static class ScopeKeys
{
    public const string Api = "jazoradmin_api";
}
