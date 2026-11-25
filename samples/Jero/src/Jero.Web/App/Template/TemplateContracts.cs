using static ECMAScript.Vue;

namespace Jero.Web.App.Template;

public sealed record TemplateView
{
    public required string id { get; init; }
    public required string name { get; init; }
    public required string code { get; init; }
    public required int type { get; init; }
    public required string groupName { get; init; }
    public string content { get; init; } = string.Empty;
    public required int orderNo { get; init; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record TemplateTypeOption
{
    public required int value { get; init; }
    public required string label { get; init; }
    public required string tagType { get; init; }
}

public sealed record TemplateRenderEntry : VueProps
{
    public string key { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
}
