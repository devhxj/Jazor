namespace Jero.Web.Application;

public sealed record FileView
{
    public required string Id { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string? UpdateTime { get; init; }
    public string? CreateUserId { get; init; }
    public string? CreateUserName { get; init; }
    public string? UpdateUserId { get; init; }
    public string? UpdateUserName { get; init; }
    public string OrgId { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public string? Provider { get; init; }
    public string? BucketName { get; init; }
    public required string? FileName { get; init; }
    public required string? Suffix { get; init; }
    public string? FilePath { get; init; }
    public required string SizeKb { get; init; }
    public required string? SizeInfo { get; init; }
    public string? Url { get; init; }
    public string? FileMd5 { get; init; }
    public string? FileType { get; init; }
    public required string? FileAlias { get; init; }
    public required bool IsPublic { get; init; }
    public string? DataId { get; init; }
}

public sealed record FileFolderView
{
    public string Label { get; init; } = string.Empty;
    public string FullPath { get; init; } = string.Empty;
    public int Depth { get; init; }
}
