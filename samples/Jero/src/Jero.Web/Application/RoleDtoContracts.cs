using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record RoleView : VueProps
{
    public required string Id { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required int OrderNo { get; init; }
    public required int DataScope { get; init; }
    public string Remark { get; init; } = string.Empty;
    public required int Status { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record RoleTenantOption : VueProps
{
    public required string Value { get; init; }
    public required string Label { get; init; }
    public required string Host { get; init; }
}

public sealed record RoleTreeNode
{
    public required string Id { get; init; }
    public string ParentId { get; init; } = string.Empty;
    public required string Label { get; init; }
    public int Depth { get; init; }
    public bool Disabled { get; init; }
    public bool HasChildren { get; init; }
}
