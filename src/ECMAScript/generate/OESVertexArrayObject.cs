namespace ECMAScript;

/// <summary>
/// WebGLVertexArrayObjectOES
/// </summary>
[ECMAScript]
[Description("@#WebGLVertexArrayObjectOES")]
public class WebGLVertexArrayObjectOES : WebGLObject
{

}

/// <summary>
/// OES_vertex_array_object
/// </summary>
[ECMAScript]
[Description("@#OES_vertex_array_object")]
public class OESVertexArrayObject
{
    /// <summary>
    /// VERTEX_ARRAY_BINDING_OES
    /// </summary>
    [Description("@#VERTEX_ARRAY_BINDING_OES")]
    public const GLenum VERTEX_ARRAY_BINDING_OES = 0x85B5;

    /// <summary>
    /// createVertexArrayOES
    /// </summary>
    [Description("@#createVertexArrayOES")]
    public extern WebGLVertexArrayObjectOES CreateVertexArrayOES();

    /// <summary>
    /// deleteVertexArrayOES
    /// </summary>
    /// <param name="arrayObject">arrayObject</param>
    [Description("@#deleteVertexArrayOES")]
    public extern void DeleteVertexArrayOES(WebGLVertexArrayObjectOES? arrayObject);

    /// <summary>
    /// isVertexArrayOES
    /// </summary>
    /// <param name="arrayObject">arrayObject</param>
    [Description("@#isVertexArrayOES")]
    public extern GLboolean IsVertexArrayOES(WebGLVertexArrayObjectOES? arrayObject);

    /// <summary>
    /// bindVertexArrayOES
    /// </summary>
    /// <param name="arrayObject">arrayObject</param>
    [Description("@#bindVertexArrayOES")]
    public extern void BindVertexArrayOES(WebGLVertexArrayObjectOES? arrayObject);
}