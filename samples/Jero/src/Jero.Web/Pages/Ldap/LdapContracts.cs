namespace Jero.Web.Pages.Ldap;

public sealed record LdapTableRow(
    string Id,
    string TenantId,
    string Host,
    int Port,
    string BaseDn,
    string BindDn,
    string AuthFilter,
    int Version,
    int Status,
    bool HasPassword);
