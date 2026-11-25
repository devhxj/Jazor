namespace Jero.Web.App.File;

public sealed record FileView
{
    public required string id { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string? updateTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string orgId { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string? provider { get; init; }
    public string? bucketName { get; init; }
    public required string? fileName { get; init; }
    public required string? suffix { get; init; }
    public string? filePath { get; init; }
    public required string sizeKb { get; init; }
    public required string? sizeInfo { get; init; }
    public string? url { get; init; }
    public string? fileMd5 { get; init; }
    public string? fileType { get; init; }
    public required string? fileAlias { get; init; }
    public required bool isPublic { get; init; }
    public string? dataId { get; init; }
}

public sealed record FileFolderView
{
    public string label { get; init; } = string.Empty;
    public string fullPath { get; init; } = string.Empty;
    public int depth { get; init; }
}
