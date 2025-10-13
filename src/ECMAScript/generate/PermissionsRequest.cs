namespace ECMAScript;

/// <summary>
/// Permissions
/// </summary>
[ECMAScript]
[Description("@#Permissions")]
public partial class Permissions
{
    /// <summary>
    /// request
    /// </summary>
    /// <param name="permissionDesc">permissionDesc</param>
    [Description("@#request")]
    public extern PromiseResult<PermissionStatus> Request(object permissionDesc);

    /// <summary>
    /// revoke
    /// </summary>
    /// <param name="permissionDesc">permissionDesc</param>
    [Description("@#revoke")]
    public extern PromiseResult<PermissionStatus> Revoke(object permissionDesc);

    /// <summary>
    /// query
    /// </summary>
    /// <param name="permissionDesc">permissionDesc</param>
    [Description("@#query")]
    public extern PromiseResult<PermissionStatus> Query(object permissionDesc);
}