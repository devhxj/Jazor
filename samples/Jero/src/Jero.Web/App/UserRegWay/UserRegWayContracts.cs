namespace Jero.Web.App.UserRegWay;

public sealed record UserRegWayTableRow(
    string id,
    string name,
    string orgName,
    string roleName,
    string posName,
    int orderNo,
    string accountTypeLabel,
    string accountTypeTag);
