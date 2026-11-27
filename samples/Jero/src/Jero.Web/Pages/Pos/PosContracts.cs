namespace Jero.Web.Pages.Pos;

public sealed record PosTableRow(
    string Id,
    string Name,
    string Code,
    int OrderNo,
    int Status,
    int UserCount,
    bool CanEdit,
    bool CanDelete,
    string EditDisabledReason,
    string DeleteDisabledReason);

public sealed record PosUserTableRow(string Id, string Account, string RealName);
