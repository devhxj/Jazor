namespace ECMAScript;

/// <summary>
/// OpusEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#OpusEncoderConfig")]
public record OpusEncoderConfig(
    [property: Description("@#format")]OpusBitstreamFormat Format = OpusBitstreamFormat.Opus,
	[property: Description("@#signal")]OpusSignal Signal = OpusSignal.Auto,
	[property: Description("@#application")]OpusApplication Application = OpusApplication.Audio,
	[property: Description("@#frameDuration")]ulong FrameDuration = 20000,
	[property: Description("@#complexity")]uint Complexity = default,
	[property: Description("@#packetlossperc")]uint Packetlossperc = 0,
	[property: Description("@#useinbandfec")]bool Useinbandfec = false,
	[property: Description("@#usedtx")]bool Usedtx = false);