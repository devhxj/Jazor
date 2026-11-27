namespace Jero.Web.Application;

public sealed record LangTextView
{
    public required string Id { get; init; }
    [ECMAScriptName("entityName")]
    public required string EntityName { get; init; }
    [ECMAScriptName("entityId")]
    public required string EntityId { get; init; }
    [ECMAScriptName("fieldName")]
    public required string FieldName { get; init; }
    [ECMAScriptName("langCode")]
    public required string LangCode { get; init; }
    public required string Content { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}
