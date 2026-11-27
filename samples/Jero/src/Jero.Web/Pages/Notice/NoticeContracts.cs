namespace Jero.Web.Pages.Notice;

public sealed record NoticeSelectOption(int Value, string Label);

public sealed record NoticeTableRow(
    string Id, string Title, string ContentSummary, string TypeLabel, string TypeTag,
    string CreateUserName, string CreateTime, string StatusLabel, string StatusTag,
    string PublicUserName, string PublicTime, bool CanMutate, string DisabledReason);
