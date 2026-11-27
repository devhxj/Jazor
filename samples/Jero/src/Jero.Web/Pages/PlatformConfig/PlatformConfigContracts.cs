namespace Jero.Web.Pages.PlatformConfig;

public sealed record PlatformConfigTableRow(
    string Id,
    [property: ECMAScriptName("name")] string Name,
    [property: ECMAScriptName("code")] string Code,
    string Value,
    [property: ECMAScriptName("groupCode")] string GroupCode,
    [property: ECMAScriptName("orderNo")] int OrderNo,
    int SysFlag,
    bool CanEditRow,
    string EditDisabledReason,
    bool CanDeleteRow,
    string DeleteDisabledReason);
