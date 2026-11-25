namespace Jero.Web.App.ReceivedNotice;

public sealed record ReceivedNoticeView
{
    public required string id { get; init; }
    public string noticeId { get; init; } = string.Empty;
    public string userId { get; init; } = string.Empty;
    public required int readStatus { get; set; }
    public string readTime { get; set; } = string.Empty;
    public required string title { get; init; }
    public string content { get; init; } = string.Empty;
    public required string contentSummary { get; init; }
    public required int type { get; init; }
    public required string publicUserName { get; init; }
    public required string publicTime { get; init; }
}

public sealed record ReceivedNoticeOption
{
    public required int value { get; init; }
    public required string label { get; init; }
    public required string tagType { get; init; }
}

