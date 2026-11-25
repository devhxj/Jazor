namespace Jero.Web.App.User;

public sealed record UserTableRow(
    string id,
    string account,
    string realName,
    string phone,
    string roleName,
    string orgName,
    string posName,
    int orderNo,
    string updateTime,
    string avatarSource,
    string accountTypeTag,
    string accountTypeLabel,
    string statusTag,
    string statusText,
    string statusConfirm,
    string statusAction,
    bool isChangingStatus,
    bool isResetting,
    bool isUnlocking,
    bool isDeleting);

public sealed record UserTreeRow(string id, string name, string path, string tenantId, int depth, bool disabled);
public sealed record UserRoleRow(string id, string name);
public sealed record UserExtOrganizationRow(string orgId, string posId);
