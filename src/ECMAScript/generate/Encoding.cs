namespace ECMAScript;

/// <summary>
/// TextDecoderOptions
/// </summary>
[ECMAScript]
[Description("@#TextDecoderOptions")]
public record TextDecoderOptions(
    [property: Description("@#fatal")]bool Fatal = false,
	[property: Description("@#ignoreBOM")]bool IgnoreBOM = false);

/// <summary>
/// TextDecodeOptions
/// </summary>
[ECMAScript]
[Description("@#TextDecodeOptions")]
public record TextDecodeOptions(
    [property: Description("@#stream")]bool Stream = false);

/// <summary>
/// TextEncoderEncodeIntoResult
/// </summary>
[ECMAScript]
[Description("@#TextEncoderEncodeIntoResult")]
public record TextEncoderEncodeIntoResult(
    [property: Description("@#read")]ulong Read = default,
	[property: Description("@#written")]ulong Written = default);

/// <summary>
/// TextDecoder
/// </summary>
[ECMAScript]
[Description("@#TextDecoder")]
public class TextDecoder
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="label">label</param>
    /// <param name="options">options</param>
    public extern TextDecoder(string label, TextDecoderOptions options);

    /// <summary>
    /// decode
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    [Description("@#decode")]
    public extern string Decode(IAllowSharedBufferSource input, TextDecodeOptions? options = default);

    #region mixin TextDecoderCommon
    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string Encoding { get; }

    /// <summary>
    /// fatal
    /// </summary>
    [Description("@#fatal")]
    public extern bool Fatal { get; }

    /// <summary>
    /// ignoreBOM
    /// </summary>
    [Description("@#ignoreBOM")]
    public extern bool IgnoreBOM { get; }
    #endregion
}

/// <summary>
/// TextEncoder
/// </summary>
[ECMAScript]
[Description("@#TextEncoder")]
public class TextEncoder
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern TextEncoder();

    /// <summary>
    /// encode
    /// </summary>
    /// <param name="input">input</param>
    [Description("@#encode")]
    public extern Uint8Array Encode(string? input = default);

    /// <summary>
    /// encodeInto
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="destination">destination</param>
    [Description("@#encodeInto")]
    public extern TextEncoderEncodeIntoResult EncodeInto(string source, Uint8Array destination);

    #region mixin TextEncoderCommon
    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string Encoding { get; }
    #endregion
}

/// <summary>
/// TextDecoderStream
/// </summary>
[ECMAScript]
[Description("@#TextDecoderStream")]
public class TextDecoderStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="label">label</param>
    /// <param name="options">options</param>
    public extern TextDecoderStream(string label, TextDecoderOptions options);

    #region mixin TextDecoderCommon
    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string Encoding { get; }

    /// <summary>
    /// fatal
    /// </summary>
    [Description("@#fatal")]
    public extern bool Fatal { get; }

    /// <summary>
    /// ignoreBOM
    /// </summary>
    [Description("@#ignoreBOM")]
    public extern bool IgnoreBOM { get; }
    #endregion

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
/// TextEncoderStream
/// </summary>
[ECMAScript]
[Description("@#TextEncoderStream")]
public class TextEncoderStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern TextEncoderStream();

    #region mixin TextEncoderCommon
    /// <summary>
    /// encoding
    /// </summary>
    [Description("@#encoding")]
    public extern string Encoding { get; }
    #endregion

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