namespace Jero.Web.App.Tenant;

public sealed record TenantTableRow(
    string id,
    string name,
    string adminAccount,
    string phone,
    string title,
    string viceTitle,
    string host,
    string configId,
    string icp,
    string watermark,
    string copyright,
    string connection,
    string slaveConnections,
    string tenantLogoUrl,
    string dbTypeText,
    string tenantTypeText,
    string tenantTypeTag,
    string statusText,
    string statusTag,
    string statusActionText);

public sealed record TenantMenuRow(string id, string label, string path, int depth, string rowStyle,
    string checkboxClass, string ariaChecked, string checkMark);
