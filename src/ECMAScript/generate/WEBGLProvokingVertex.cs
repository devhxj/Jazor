namespace ECMAScript;

/// <summary>
/// WEBGL_provoking_vertex
/// </summary>
[ECMAScript]
[Description("@#WEBGL_provoking_vertex")]
public class WEBGLProvokingVertex
{
    /// <summary>
    /// FIRST_VERTEX_CONVENTION_WEBGL
    /// </summary>
    [Description("@#FIRST_VERTEX_CONVENTION_WEBGL")]
    public const GLenum FIRST_VERTEX_CONVENTION_WEBGL = 0x8E4D;

    /// <summary>
    /// LAST_VERTEX_CONVENTION_WEBGL
    /// </summary>
    [Description("@#LAST_VERTEX_CONVENTION_WEBGL")]
    public const GLenum LAST_VERTEX_CONVENTION_WEBGL = 0x8E4E;

    /// <summary>
    /// PROVOKING_VERTEX_WEBGL
    /// </summary>
    [Description("@#PROVOKING_VERTEX_WEBGL")]
    public const GLenum PROVOKING_VERTEX_WEBGL = 0x8E4F;

    /// <summary>
    /// provokingVertexWEBGL
    /// </summary>
    /// <param name="provokeMode">provokeMode</param>
    [Description("@#provokingVertexWEBGL")]
    public extern void ProvokingVertexWEBGL(GLenum provokeMode);
}