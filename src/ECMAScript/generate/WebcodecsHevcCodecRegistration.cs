namespace ECMAScript;

/// <summary>
/// HevcEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#HevcEncoderConfig")]
public record HevcEncoderConfig(
    [property: Description("@#format")]HevcBitstreamFormat Format = HevcBitstreamFormat.Hevc);

/// <summary>
/// VideoEncoderEncodeOptionsForHevc
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForHevc")]
public record VideoEncoderEncodeOptionsForHevc(
    [property: Description("@#quantizer")]ushort Quantizer = default);