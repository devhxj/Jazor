namespace ECMAScript;

/// <summary>
/// ANGLE_instanced_arrays
/// </summary>
[ECMAScript]
[Description("@#ANGLE_instanced_arrays")]
public class ANGLEInstancedArrays
{
    /// <summary>
    /// VERTEX_ATTRIB_ARRAY_DIVISOR_ANGLE
    /// </summary>
    [Description("@#VERTEX_ATTRIB_ARRAY_DIVISOR_ANGLE")]
    public const GLenum VERTEX_ATTRIB_ARRAY_DIVISOR_ANGLE = 0x88FE;

    /// <summary>
    /// drawArraysInstancedANGLE
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="first">first</param>
    /// <param name="count">count</param>
    /// <param name="primcount">primcount</param>
    [Description("@#drawArraysInstancedANGLE")]
    public extern void DrawArraysInstancedANGLE(GLenum mode, GLint first, GLsizei count, GLsizei primcount);

    /// <summary>
    /// drawElementsInstancedANGLE
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="count">count</param>
    /// <param name="type">type</param>
    /// <param name="offset">offset</param>
    /// <param name="primcount">primcount</param>
    [Description("@#drawElementsInstancedANGLE")]
    public extern void DrawElementsInstancedANGLE(GLenum mode, GLsizei count, GLenum type, GLintptr offset, GLsizei primcount);

    /// <summary>
    /// vertexAttribDivisorANGLE
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="divisor">divisor</param>
    [Description("@#vertexAttribDivisorANGLE")]
    public extern void VertexAttribDivisorANGLE(GLuint index, GLuint divisor);
}