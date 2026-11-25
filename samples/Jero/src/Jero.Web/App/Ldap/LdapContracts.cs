namespace Jero.Web.App.Ldap;

public sealed record LdapTableRow(
    string id,
    string tenantId,
    string host,
    int port,
    string baseDn,
    string bindDn,
    string authFilter,
    int version,
    int status,
    bool hasPassword);
