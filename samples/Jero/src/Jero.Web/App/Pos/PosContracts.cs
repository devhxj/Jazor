namespace Jero.Web.App.Pos;

public sealed record PosTableRow(
    string id,
    string name,
    string code,
    int orderNo,
    int status,
    int userCount,
    bool canEdit,
    bool canDelete,
    string editDisabledReason,
    string deleteDisabledReason);

public sealed record PosUserTableRow(string id, string account, string realName);
