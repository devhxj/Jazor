namespace ECMAScript;

/// <summary>
/// WebGLTimerQueryEXT
/// </summary>
[ECMAScript]
[Description("@#WebGLTimerQueryEXT")]
public class WebGLTimerQueryEXT : WebGLObject
{

}

/// <summary>
/// EXT_disjoint_timer_query
/// </summary>
[ECMAScript]
[Description("@#EXT_disjoint_timer_query")]
public class EXTDisjointTimerQuery
{
    /// <summary>
    /// QUERY_COUNTER_BITS_EXT
    /// </summary>
    [Description("@#QUERY_COUNTER_BITS_EXT")]
    public const GLenum QUERY_COUNTER_BITS_EXT = 0x8864;

    /// <summary>
    /// CURRENT_QUERY_EXT
    /// </summary>
    [Description("@#CURRENT_QUERY_EXT")]
    public const GLenum CURRENT_QUERY_EXT = 0x8865;

    /// <summary>
    /// QUERY_RESULT_EXT
    /// </summary>
    [Description("@#QUERY_RESULT_EXT")]
    public const GLenum QUERY_RESULT_EXT = 0x8866;

    /// <summary>
    /// QUERY_RESULT_AVAILABLE_EXT
    /// </summary>
    [Description("@#QUERY_RESULT_AVAILABLE_EXT")]
    public const GLenum QUERY_RESULT_AVAILABLE_EXT = 0x8867;

    /// <summary>
    /// TIME_ELAPSED_EXT
    /// </summary>
    [Description("@#TIME_ELAPSED_EXT")]
    public const GLenum TIME_ELAPSED_EXT = 0x88BF;

    /// <summary>
    /// TIMESTAMP_EXT
    /// </summary>
    [Description("@#TIMESTAMP_EXT")]
    public const GLenum TIMESTAMP_EXT = 0x8E28;

    /// <summary>
    /// GPU_DISJOINT_EXT
    /// </summary>
    [Description("@#GPU_DISJOINT_EXT")]
    public const GLenum GPU_DISJOINT_EXT = 0x8FBB;

    /// <summary>
    /// createQueryEXT
    /// </summary>
    [Description("@#createQueryEXT")]
    public extern WebGLTimerQueryEXT CreateQueryEXT();

    /// <summary>
    /// deleteQueryEXT
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#deleteQueryEXT")]
    public extern void DeleteQueryEXT(WebGLTimerQueryEXT? query);

    /// <summary>
    /// isQueryEXT
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#isQueryEXT")]
    public extern bool IsQueryEXT(WebGLTimerQueryEXT? query);

    /// <summary>
    /// beginQueryEXT
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="query">query</param>
    [Description("@#beginQueryEXT")]
    public extern void BeginQueryEXT(GLenum target, WebGLTimerQueryEXT query);

    /// <summary>
    /// endQueryEXT
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#endQueryEXT")]
    public extern void EndQueryEXT(GLenum target);

    /// <summary>
    /// queryCounterEXT
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="target">target</param>
    [Description("@#queryCounterEXT")]
    public extern void QueryCounterEXT(WebGLTimerQueryEXT query, GLenum target);

    /// <summary>
    /// getQueryEXT
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="pname">pname</param>
    [Description("@#getQueryEXT")]
    public extern object GetQueryEXT(GLenum target, GLenum pname);

    /// <summary>
    /// getQueryObjectEXT
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="pname">pname</param>
    [Description("@#getQueryObjectEXT")]
    public extern object GetQueryObjectEXT(WebGLTimerQueryEXT query, GLenum pname);
}