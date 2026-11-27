namespace Jero.Web.Application;

public sealed record ReceivedNoticeView
{
    public required string Id { get; init; }
    public string NoticeId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public required int ReadStatus { get; set; }
    public string ReadTime { get; set; } = string.Empty;
    public required string Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public required string ContentSummary { get; init; }
    public required int Type { get; init; }
    public required string PublicUserName { get; init; }
    public required string PublicTime { get; init; }
}

public sealed record ReceivedNoticeOption
{
    public required int Value { get; init; }
    public required string Label { get; init; }
    public required string TagType { get; init; }
}
