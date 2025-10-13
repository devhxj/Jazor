namespace ECMAScript;

/// <summary>
/// NavigatorLogin
/// </summary>
[ECMAScript]
[Description("@#NavigatorLogin")]
public class NavigatorLogin
{
    /// <summary>
    /// setStatus
    /// </summary>
    /// <param name="status">status</param>
    [Description("@#setStatus")]
    public extern PromiseResult<object> SetStatus(LoginStatus status);
}