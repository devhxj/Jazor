namespace ECMAScript;

/// <summary>
/// CompressionStream
/// </summary>
[ECMAScript]
[Description("@#CompressionStream")]
public class CompressionStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="format">format</param>
    public extern CompressionStream(CompressionFormat format);

    #region mixin GenericTransformStream
    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }
    #endregion
}

/// <summary>
/// DecompressionStream
/// </summary>
[ECMAScript]
[Description("@#DecompressionStream")]
public class DecompressionStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="format">format</param>
    public extern DecompressionStream(CompressionFormat format);

    #region mixin GenericTransformStream
    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }
    #endregion
}