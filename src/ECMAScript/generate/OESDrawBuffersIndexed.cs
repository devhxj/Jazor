namespace ECMAScript;

/// <summary>
/// OES_draw_buffers_indexed
/// </summary>
[ECMAScript]
[Description("@#OES_draw_buffers_indexed")]
public class OESDrawBuffersIndexed
{
    /// <summary>
    /// enableiOES
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="index">index</param>
    [Description("@#enableiOES")]
    public extern void EnableiOES(GLenum target, GLuint index);

    /// <summary>
    /// disableiOES
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="index">index</param>
    [Description("@#disableiOES")]
    public extern void DisableiOES(GLenum target, GLuint index);

    /// <summary>
    /// blendEquationiOES
    /// </summary>
    /// <param name="buf">buf</param>
    /// <param name="mode">mode</param>
    [Description("@#blendEquationiOES")]
    public extern void BlendEquationiOES(GLuint buf, GLenum mode);

    /// <summary>
    /// blendEquationSeparateiOES
    /// </summary>
    /// <param name="buf">buf</param>
    /// <param name="modeRgb">modeRGB</param>
    /// <param name="modeAlpha">modeAlpha</param>
    [Description("@#blendEquationSeparateiOES")]
    public extern void BlendEquationSeparateiOES(GLuint buf, GLenum modeRgb, GLenum modeAlpha);

    /// <summary>
    /// blendFunciOES
    /// </summary>
    /// <param name="buf">buf</param>
    /// <param name="src">src</param>
    /// <param name="dst">dst</param>
    [Description("@#blendFunciOES")]
    public extern void BlendFunciOES(GLuint buf, GLenum src, GLenum dst);

    /// <summary>
    /// blendFuncSeparateiOES
    /// </summary>
    /// <param name="buf">buf</param>
    /// <param name="srcRgb">srcRGB</param>
    /// <param name="dstRgb">dstRGB</param>
    /// <param name="srcAlpha">srcAlpha</param>
    /// <param name="dstAlpha">dstAlpha</param>
    [Description("@#blendFuncSeparateiOES")]
    public extern void BlendFuncSeparateiOES(GLuint buf, GLenum srcRgb, GLenum dstRgb, GLenum srcAlpha, GLenum dstAlpha);

    /// <summary>
    /// colorMaskiOES
    /// </summary>
    /// <param name="buf">buf</param>
    /// <param name="r">r</param>
    /// <param name="g">g</param>
    /// <param name="b">b</param>
    /// <param name="a">a</param>
    [Description("@#colorMaskiOES")]
    public extern void ColorMaskiOES(GLuint buf, GLboolean r, GLboolean g, GLboolean b, GLboolean a);
}