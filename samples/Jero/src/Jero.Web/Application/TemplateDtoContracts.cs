namespace Jero.Web.Application;

public sealed record TemplateView
{
    public required string Id { get; init; }
    [ECMAScriptName("name")]
    public required string Name { get; init; }
    [ECMAScriptName("code")]
    public required string Code { get; init; }
    public required int Type { get; init; }
    [ECMAScriptName("groupName")]
    public required string GroupName { get; init; }
    public string Content { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public required int OrderNo { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record TemplateTypeOption
{
    public required int Value { get; init; }
    public required string Label { get; init; }
    public required string TagType { get; init; }
}
