namespace ECMAScript;

/// <summary>
/// WEBGL_lose_context
/// </summary>
[ECMAScript]
[Description("@#WEBGL_lose_context")]
public class WEBGLLoseContext
{
    /// <summary>
    /// loseContext
    /// </summary>
    [Description("@#loseContext")]
    public extern void LoseContext();

    /// <summary>
    /// restoreContext
    /// </summary>
    [Description("@#restoreContext")]
    public extern void RestoreContext();
}