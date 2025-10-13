namespace ECMAScript;

/// <summary>
/// VideoEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderConfig")]
public record VideoEncoderConfig(
    [property: Description("@#avc")]AvcEncoderConfig? Avc = default,
	[property: Description("@#hevc")]HevcEncoderConfig? Hevc = default,
	[property: Description("@#codec")]string? Codec = default,
	[property: Description("@#width")]uint Width = default,
	[property: Description("@#height")]uint Height = default,
	[property: Description("@#displayWidth")]uint DisplayWidth = default,
	[property: Description("@#displayHeight")]uint DisplayHeight = default,
	[property: Description("@#bitrate")]ulong Bitrate = default,
	[property: Description("@#framerate")]double Framerate = default,
	[property: Description("@#hardwareAcceleration")]HardwareAcceleration HardwareAcceleration = HardwareAcceleration.NoPreference,
	[property: Description("@#alpha")]AlphaOption Alpha = AlphaOption.Discard,
	[property: Description("@#scalabilityMode")]string? ScalabilityMode = default,
	[property: Description("@#bitrateMode")]VideoEncoderBitrateMode BitrateMode = VideoEncoderBitrateMode.Variable,
	[property: Description("@#latencyMode")]LatencyMode LatencyMode = LatencyMode.Quality,
	[property: Description("@#contentHint")]string? ContentHint = default)
{
    [Category("optional")]
    public extern static VideoEncoderConfig OptionalAvc(
        [Description("@#avc")]AvcEncoderConfig? Avc = default);

    [Category("optional")]
    public extern static VideoEncoderConfig OptionalHevc(
        [Description("@#hevc")]HevcEncoderConfig? Hevc = default);

    [Category("optional")]
    public extern static VideoEncoderConfig OptionalCodecWidthHeight13(
        [Description("@#codec")]string? Codec = default,
        [Description("@#width")]uint Width = default,
        [Description("@#height")]uint Height = default,
        [Description("@#displayWidth")]uint DisplayWidth = default,
        [Description("@#displayHeight")]uint DisplayHeight = default,
        [Description("@#bitrate")]ulong Bitrate = default,
        [Description("@#framerate")]double Framerate = default,
        [Description("@#hardwareAcceleration")]HardwareAcceleration hardwareAcceleration = HardwareAcceleration.NoPreference,
        [Description("@#alpha")]AlphaOption alpha = AlphaOption.Discard,
        [Description("@#scalabilityMode")]string? ScalabilityMode = default,
        [Description("@#bitrateMode")]VideoEncoderBitrateMode bitrateMode = VideoEncoderBitrateMode.Variable,
        [Description("@#latencyMode")]LatencyMode latencyMode = LatencyMode.Quality,
        [Description("@#contentHint")]string? ContentHint = default);
}

/// <summary>
/// AvcEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AvcEncoderConfig")]
public record AvcEncoderConfig(
    [property: Description("@#format")]AvcBitstreamFormat Format = AvcBitstreamFormat.Avc);

/// <summary>
/// VideoEncoderEncodeOptionsForAvc
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForAvc")]
public record VideoEncoderEncodeOptionsForAvc(
    [property: Description("@#quantizer")]ushort Quantizer = default);