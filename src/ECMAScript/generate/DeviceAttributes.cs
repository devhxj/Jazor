namespace ECMAScript;

/// <summary>
/// NavigatorManagedData
/// </summary>
[ECMAScript]
[Description("@#NavigatorManagedData")]
public partial class NavigatorManagedData : EventTarget
{
    /// <summary>
    /// getAnnotatedAssetId
    /// </summary>
    [Description("@#getAnnotatedAssetId")]
    public extern PromiseResult<string> GetAnnotatedAssetId();

    /// <summary>
    /// getAnnotatedLocation
    /// </summary>
    [Description("@#getAnnotatedLocation")]
    public extern PromiseResult<string> GetAnnotatedLocation();

    /// <summary>
    /// getDirectoryId
    /// </summary>
    [Description("@#getDirectoryId")]
    public extern PromiseResult<string> GetDirectoryId();

    /// <summary>
    /// getHostname
    /// </summary>
    [Description("@#getHostname")]
    public extern PromiseResult<string> GetHostname();

    /// <summary>
    /// getSerialNumber
    /// </summary>
    [Description("@#getSerialNumber")]
    public extern PromiseResult<string> GetSerialNumber();

    /// <summary>
    /// getManagedConfiguration
    /// </summary>
    /// <param name="keys">keys</param>
    [Description("@#getManagedConfiguration")]
    public extern PromiseResult<Dictionary<string, object>> GetManagedConfiguration(string[] keys);

    /// <summary>
    /// onmanagedconfigurationchange
    /// </summary>
    [Description("@#onmanagedconfigurationchange")]
    public extern EventHandler Onmanagedconfigurationchange { get; set; }
}