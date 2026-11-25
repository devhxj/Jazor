namespace Jero.Web.App.TenantConfig;

public sealed record TenantConfigTableRow(
    string id,
    string name,
    string code,
    string value,
    string groupCode,
    int orderNo,
    int sysFlag,
    bool canDeleteRow,
    string deleteDisabledReason);
