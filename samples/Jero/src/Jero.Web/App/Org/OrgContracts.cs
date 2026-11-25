namespace Jero.Web.App.Org;

public sealed partial record OrgFlatView
{
    public required string id { get; init; }
    public string tenantId { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public required string name { get; init; }
    public required string code { get; init; }
    public required int level { get; init; }
    public required string type { get; init; }
    public string directorId { get; init; } = string.Empty;
    public required int orderNo { get; init; }
    public required int status { get; init; }
    public string remark { get; init; } = string.Empty;
    public bool disabled { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
    public required int depth { get; init; }
    public bool hasChildren { get; init; }
    public string[] ancestorIds { get; init; } = [];
}

public sealed partial record OrgTenantOption
{
    public required string value { get; init; }
    public required string label { get; init; }
    public required string host { get; init; }
}

public sealed partial record OrgTypeOption
{
    public string id { get; init; } = string.Empty;
    public required string value { get; init; }
    public required string label { get; init; }
    public required string tagType { get; init; }
}

public sealed partial record OrgParentOption
{
    public required string id { get; init; }
    public required string label { get; init; }
}
