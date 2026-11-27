using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record OrgFlatView : VueProps
{
    public required string Id { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required int Level { get; init; }
    public required string Type { get; init; }
    public string DirectorId { get; init; } = string.Empty;
    public required int OrderNo { get; init; }
    public required int Status { get; init; }
    public string Remark { get; init; } = string.Empty;
    public bool Disabled { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
    public required int Depth { get; init; }
    public bool HasChildren { get; init; }
    public string[] AncestorIds { get; init; } = [];
}

public sealed record OrgTenantOption : VueProps
{
    public required string Value { get; init; }
    public required string Label { get; init; }
    public required string Host { get; init; }
}

public sealed record OrgTypeOption : VueProps
{
    public string Id { get; init; } = string.Empty;
    public required string Value { get; init; }
    public required string Label { get; init; }
    public required string TagType { get; init; }
}

public sealed record OrgParentOption
{
    public required string Id { get; init; }
    public required string Label { get; init; }
}
