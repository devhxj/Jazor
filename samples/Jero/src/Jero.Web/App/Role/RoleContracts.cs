namespace Jero.Web.App.Role;

public sealed partial record RoleView
{
    public required string id { get; init; }
    public string tenantId { get; init; } = string.Empty;
    public required string name { get; init; }
    public required string code { get; init; }
    public required int orderNo { get; init; }
    public required int dataScope { get; init; }
    public string remark { get; init; } = string.Empty;
    public required int status { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed partial record RoleTenantOption
{
    public required string value { get; init; }
    public required string label { get; init; }
    public required string host { get; init; }
}

public sealed partial record RoleTreeNode
{
    public required string id { get; init; }
    public string parentId { get; init; } = string.Empty;
    public required string label { get; init; }
    public int depth { get; init; }
    public bool disabled { get; init; }
    public bool hasChildren { get; init; }
}
