namespace Jero.Web.App.PlatformConfig;

public sealed record PlatformConfigTableRow(
    string id,
    string name,
    string code,
    string value,
    string groupCode,
    int orderNo,
    int sysFlag,
    bool canEditRow,
    string editDisabledReason,
    bool canDeleteRow,
    string deleteDisabledReason);


