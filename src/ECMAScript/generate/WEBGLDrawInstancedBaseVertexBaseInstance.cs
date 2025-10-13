namespace ECMAScript;

/// <summary>
/// WEBGL_draw_instanced_base_vertex_base_instance
/// </summary>
[ECMAScript]
[Description("@#WEBGL_draw_instanced_base_vertex_base_instance")]
public class WEBGLDrawInstancedBaseVertexBaseInstance
{
    /// <summary>
    /// drawArraysInstancedBaseInstanceWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="first">first</param>
    /// <param name="count">count</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="baseInstance">baseInstance</param>
    [Description("@#drawArraysInstancedBaseInstanceWEBGL")]
    public extern void DrawArraysInstancedBaseInstanceWEBGL(GLenum mode, GLint first, GLsizei count, GLsizei instanceCount, GLuint baseInstance);

    /// <summary>
    /// drawElementsInstancedBaseVertexBaseInstanceWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="count">count</param>
    /// <param name="type">type</param>
    /// <param name="offset">offset</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="baseVertex">baseVertex</param>
    /// <param name="baseInstance">baseInstance</param>
    [Description("@#drawElementsInstancedBaseVertexBaseInstanceWEBGL")]
    public extern void DrawElementsInstancedBaseVertexBaseInstanceWEBGL(GLenum mode, GLsizei count, GLenum type, GLintptr offset, GLsizei instanceCount, GLint baseVertex, GLuint baseInstance);
}