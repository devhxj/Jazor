namespace ECMAScript;

/// <summary>
/// WEBGL_multi_draw
/// </summary>
[ECMAScript]
[Description("@#WEBGL_multi_draw")]
public class WEBGLMultiDraw
{
    /// <summary>
    /// multiDrawArraysWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="firstsList">firstsList</param>
    /// <param name="firstsOffset">firstsOffset</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawArraysWEBGL")]
    public extern void MultiDrawArraysWEBGL(GLenum mode, Either<Int32Array, GLint[]> firstsList, ulong firstsOffset, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, GLsizei drawcount);

    /// <summary>
    /// multiDrawElementsWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="type">type</param>
    /// <param name="offsetsList">offsetsList</param>
    /// <param name="offsetsOffset">offsetsOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawElementsWEBGL")]
    public extern void MultiDrawElementsWEBGL(GLenum mode, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, GLenum type, Either<Int32Array, GLsizei[]> offsetsList, ulong offsetsOffset, GLsizei drawcount);

    /// <summary>
    /// multiDrawArraysInstancedWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="firstsList">firstsList</param>
    /// <param name="firstsOffset">firstsOffset</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="instanceCountsList">instanceCountsList</param>
    /// <param name="instanceCountsOffset">instanceCountsOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawArraysInstancedWEBGL")]
    public extern void MultiDrawArraysInstancedWEBGL(GLenum mode, Either<Int32Array, GLint[]> firstsList, ulong firstsOffset, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, Either<Int32Array, GLsizei[]> instanceCountsList, ulong instanceCountsOffset, GLsizei drawcount);

    /// <summary>
    /// multiDrawElementsInstancedWEBGL
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="countsList">countsList</param>
    /// <param name="countsOffset">countsOffset</param>
    /// <param name="type">type</param>
    /// <param name="offsetsList">offsetsList</param>
    /// <param name="offsetsOffset">offsetsOffset</param>
    /// <param name="instanceCountsList">instanceCountsList</param>
    /// <param name="instanceCountsOffset">instanceCountsOffset</param>
    /// <param name="drawcount">drawcount</param>
    [Description("@#multiDrawElementsInstancedWEBGL")]
    public extern void MultiDrawElementsInstancedWEBGL(GLenum mode, Either<Int32Array, GLsizei[]> countsList, ulong countsOffset, GLenum type, Either<Int32Array, GLsizei[]> offsetsList, ulong offsetsOffset, Either<Int32Array, GLsizei[]> instanceCountsList, ulong instanceCountsOffset, GLsizei drawcount);
}