namespace ECMAScript;

/// <summary>
/// VideoEncoderEncodeOptions
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptions")]
public record VideoEncoderEncodeOptions(
    [property: Description("@#av1")]VideoEncoderEncodeOptionsForAv1? Av1 = default,
	[property: Description("@#avc")]VideoEncoderEncodeOptionsForAvc? Avc = default,
	[property: Description("@#hevc")]VideoEncoderEncodeOptionsForHevc? Hevc = default,
	[property: Description("@#vp9")]VideoEncoderEncodeOptionsForVp9? Vp9 = default,
	[property: Description("@#keyFrame")]bool KeyFrame = false)
{
    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalAv1(
        [Description("@#av1")]VideoEncoderEncodeOptionsForAv1? Av1 = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalAvc(
        [Description("@#avc")]VideoEncoderEncodeOptionsForAvc? Avc = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalHevc(
        [Description("@#hevc")]VideoEncoderEncodeOptionsForHevc? Hevc = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalVp9(
        [Description("@#vp9")]VideoEncoderEncodeOptionsForVp9? Vp9 = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalKeyFrame(
        [Description("@#keyFrame")]bool keyFrame = false);
}

/// <summary>
/// VideoEncoderEncodeOptionsForAv1
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForAv1")]
public record VideoEncoderEncodeOptionsForAv1(
    [property: Description("@#quantizer")]ushort Quantizer = default);