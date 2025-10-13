namespace ECMAScript;

/// <summary>
/// WEBGL_multi_draw_instanced_base_vertex_base_instance
/// </summary>
[ECMAScript]
[Description("@#WEBGL_multi_draw_instanced_base_vertex_base_instance")]
public class WEBGLMultiDrawInstancedBaseVertexBaseInstance
{
    /// <summary>
    /// multiDrawArraysInstancedBaseInstanceWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="firstsList">firstsList</param>
    /// <param name="firstsOffset">firstsOffset</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="instanceCountsList">instanceCountsList</param>
    /// <param name="instanceCountsOffset">instanceCountsOffset</param>
    /// <param name="baseInstancesList">baseInstancesList</param>
    /// <param name="baseInstancesOffset">baseInstancesOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawArraysInstancedBaseInstanceWEBGL")]
    public extern void MultiDrawArraysInstancedBaseInstanceWEBGL(GLenum mode, Either<Int32Array, GLint[]> firstsList, ulong firstsOffset, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, Either<Int32Array, GLsizei[]> instanceCountsList, ulong instanceCountsOffset, Either<Uint32Array, GLuint[]> baseInstancesList, ulong baseInstancesOffset, GLsizei drawcount);

    /// <summary>
    /// multiDrawElementsInstancedBaseVertexBaseInstanceWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="type">type</param>
    /// <param name="offsetsList">offsetsList</param>
    /// <param name="offsetsOffset">offsetsOffset</param>
    /// <param name="instanceCountsList">instanceCountsList</param>
    /// <param name="instanceCountsOffset">instanceCountsOffset</param>
    /// <param name="baseVerticesList">baseVerticesList</param>
    /// <param name="baseVerticesOffset">baseVerticesOffset</param>
    /// <param name="baseInstancesList">baseInstancesList</param>
    /// <param name="baseInstancesOffset">baseInstancesOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawElementsInstancedBaseVertexBaseInstanceWEBGL")]
    public extern void MultiDrawElementsInstancedBaseVertexBaseInstanceWEBGL(GLenum mode, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, GLenum type, Either<Int32Array, GLsizei[]> offsetsList, ulong offsetsOffset, Either<Int32Array, GLsizei[]> instanceCountsList, ulong instanceCountsOffset, Either<Int32Array, GLint[]> baseVerticesList, ulong baseVerticesOffset, Either<Uint32Array, GLuint[]> baseInstancesList, ulong baseInstancesOffset, GLsizei drawcount);
}