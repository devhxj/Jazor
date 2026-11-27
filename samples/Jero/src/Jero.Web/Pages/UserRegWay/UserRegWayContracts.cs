namespace Jero.Web.Pages.UserRegWay;

public sealed record UserRegWayTableRow(
    string Id,
    [property: ECMAScriptName("name")] string Name,
    string OrgName,
    string RoleName,
    string PosName,
    [property: ECMAScriptName("orderNo")] int OrderNo,
    string AccountTypeLabel,
    string AccountTypeTag);
