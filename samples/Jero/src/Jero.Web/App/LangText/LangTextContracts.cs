namespace Jero.Web.App.LangText;

public sealed record LangTextView
{
    public required string id { get; init; }
    public required string entityName { get; init; }
    public required string entityId { get; init; }
    public required string fieldName { get; init; }
    public required string langCode { get; init; }
    public required string content { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}
