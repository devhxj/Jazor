namespace ECMAScript;

/// <summary>
/// AudioEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderConfig")]
public record AudioEncoderConfig(
    [property: Description("@#aac")]AacEncoderConfig? Aac = default,
	[property: Description("@#flac")]FlacEncoderConfig? Flac = default,
	[property: Description("@#opus")]OpusEncoderConfig? Opus = default,
	[property: Description("@#codec")]string? Codec = default,
	[property: Description("@#sampleRate")]uint SampleRate = default,
	[property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
	[property: Description("@#bitrate")]ulong Bitrate = default,
	[property: Description("@#bitrateMode")]BitrateMode BitrateMode = BitrateMode.Variable)
{
    [Category("optional")]
    public extern static AudioEncoderConfig OptionalAac(
        [Description("@#aac")]AacEncoderConfig? Aac = default);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalFlac(
        [Description("@#flac")]FlacEncoderConfig? Flac = default);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalOpus(
        [Description("@#opus")]OpusEncoderConfig? Opus = default);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalCodecSampleRateNumberOfChannels5(
        [Description("@#codec")]string? Codec = default,
        [Description("@#sampleRate")]uint SampleRate = default,
        [Description("@#numberOfChannels")]uint NumberOfChannels = default,
        [Description("@#bitrate")]ulong Bitrate = default,
        [Description("@#bitrateMode")]BitrateMode bitrateMode = BitrateMode.Variable);
}

/// <summary>
/// AacEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AacEncoderConfig")]
public record AacEncoderConfig(
    [property: Description("@#format")]AacBitstreamFormat Format = AacBitstreamFormat.Aac);