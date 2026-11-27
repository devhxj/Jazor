namespace Jero.Web.Pages.TenantConfig;

public sealed record TenantConfigTableRow(
    string Id,
    [property: ECMAScriptName("name")] string Name,
    [property: ECMAScriptName("code")] string Code,
    string Value,
    [property: ECMAScriptName("groupCode")] string GroupCode,
    [property: ECMAScriptName("orderNo")] int OrderNo,
    [property: ECMAScriptName("sysFlag")] int SysFlag,
    bool CanDeleteRow,
    string DeleteDisabledReason);
