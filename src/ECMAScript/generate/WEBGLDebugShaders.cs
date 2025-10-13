namespace ECMAScript;

/// <summary>
/// WEBGL_debug_shaders
/// </summary>
[ECMAScript]
[Description("@#WEBGL_debug_shaders")]
public class WEBGLDebugShaders
{
    /// <summary>
    /// getTranslatedShaderSource
    /// </summary>
    /// <param name="shader">shader</param>
    [Description("@#getTranslatedShaderSource")]
    public extern string GetTranslatedShaderSource(WebGLShader shader);
}