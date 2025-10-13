namespace ECMAScript;

/// <summary>
/// VideoFrameCallbackMetadata
/// </summary>
[ECMAScript]
[Description("@#VideoFrameCallbackMetadata")]
public record VideoFrameCallbackMetadata(
    [property: Description("@#presentationTime")]double PresentationTime = default,
	[property: Description("@#expectedDisplayTime")]double ExpectedDisplayTime = default,
	[property: Description("@#width")]uint Width = default,
	[property: Description("@#height")]uint Height = default,
	[property: Description("@#mediaTime")]double MediaTime = default,
	[property: Description("@#presentedFrames")]uint PresentedFrames = default,
	[property: Description("@#processingDuration")]double ProcessingDuration = default,
	[property: Description("@#captureTime")]double CaptureTime = default,
	[property: Description("@#receiveTime")]double ReceiveTime = default,
	[property: Description("@#rtpTimestamp")]uint RtpTimestamp = default);