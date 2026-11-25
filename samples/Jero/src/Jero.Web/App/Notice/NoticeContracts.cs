namespace Jero.Web.App.Notice;

public sealed record NoticeSelectOption(int value, string label);

public sealed record NoticeTableRow(
    string id, string title, string contentSummary, string typeLabel, string typeTag,
    string createUserName, string createTime, string statusLabel, string statusTag,
    string publicUserName, string publicTime, bool canMutate, string disabledReason);
