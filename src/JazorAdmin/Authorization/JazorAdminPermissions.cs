// Centralizes stable resource and operation keys used by APIs, persisted grants, and authorization policies.
// 集中定义 API、持久化授权和策略共用的稳定资源与操作键，避免分散的魔法字符串。
namespace JazorAdmin.Authorization;

public static class JazorAdminResources
{
    public const string Organizations = "organizations";
    public const string Authorization = "authorization";
}

public static class JazorAdminOperations
{
    public const string Read = "read";
    public const string Manage = "manage";
}

public static class JazorAdminRoles
{
    public const string PlatformAdministrator = "platform-administrator";
}

public static class JazorAdminPolicies
{
    public const string PlatformAdministrator = "jazoradmin.platform-administrator";
}

public static class JazorAdminScopes
{
    public const string Api = "jazoradmin_api";
}
