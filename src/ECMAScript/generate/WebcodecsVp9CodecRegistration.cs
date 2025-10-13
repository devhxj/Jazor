namespace ECMAScript;

/// <summary>
/// VideoEncoderEncodeOptionsForVp9
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForVp9")]
public record VideoEncoderEncodeOptionsForVp9(
    [property: Description("@#quantizer")]ushort Quantizer = default);