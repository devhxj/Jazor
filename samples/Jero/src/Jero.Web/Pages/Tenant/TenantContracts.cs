namespace Jero.Web.Pages.Tenant;

public sealed record TenantTableRow(
    string Id,
    string Name,
    string AdminAccount,
    string Phone,
    string Title,
    string ViceTitle,
    string Host,
    string ConfigId,
    string Icp,
    string Watermark,
    string Copyright,
    string Connection,
    string SlaveConnections,
    string TenantLogoUrl,
    string DbTypeText,
    string TenantTypeText,
    string TenantTypeTag,
    string StatusText,
    string StatusTag,
    string StatusActionText);

public sealed record TenantMenuRow(string Id, string Label, string Path, int Depth, string RowStyle,
    string CheckboxClass, string AriaChecked, string CheckMark);
