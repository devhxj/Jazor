namespace ECMAScript;

/// <summary>
/// FlacEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#FlacEncoderConfig")]
public record FlacEncoderConfig(
    [property: Description("@#blockSize")]uint BlockSize = 0,
	[property: Description("@#compressLevel")]uint CompressLevel = 5);