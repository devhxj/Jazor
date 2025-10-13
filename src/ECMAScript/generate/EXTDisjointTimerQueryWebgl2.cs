namespace ECMAScript;

/// <summary>
/// EXT_disjoint_timer_query_webgl2
/// </summary>
[ECMAScript]
[Description("@#EXT_disjoint_timer_query_webgl2")]
public class EXTDisjointTimerQueryWebgl2
{
    /// <summary>
    /// QUERY_COUNTER_BITS_EXT
    /// </summary>
    [Description("@#QUERY_COUNTER_BITS_EXT")]
    public const GLenum QUERY_COUNTER_BITS_EXT = 0x8864;

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
    /// queryCounterEXT
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="target">target</param>
    [Description("@#queryCounterEXT")]
    public extern void QueryCounterEXT(WebGLQuery query, GLenum target);
}