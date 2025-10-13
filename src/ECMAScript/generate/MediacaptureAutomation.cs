namespace ECMAScript;

/// <summary>
/// MockCapturePromptResultConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCapturePromptResultConfiguration")]
public record MockCapturePromptResultConfiguration(
    [property: Description("@#getUserMedia")]MockCapturePromptResult? GetUserMedia = default,
	[property: Description("@#getDisplayMedia")]MockCapturePromptResult? GetDisplayMedia = default);

/// <summary>
/// MockCaptureDeviceConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCaptureDeviceConfiguration")]
public record MockCaptureDeviceConfiguration(
    [property: Description("@#label")]string? Label = default,
	[property: Description("@#deviceId")]string? DeviceId = default,
	[property: Description("@#groupId")]string? GroupId = default);

/// <summary>
/// MockCameraConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCameraConfiguration")]
public record MockCameraConfiguration(
    [property: Description("@#defaultFrameRate")]double DefaultFrameRate = 30d,
	[property: Description("@#facingMode")]string? FacingMode = default): MockCaptureDeviceConfiguration;

/// <summary>
/// MockMicrophoneConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockMicrophoneConfiguration")]
public record MockMicrophoneConfiguration(
    [property: Description("@#defaultSampleRate")]uint DefaultSampleRate = 44100): MockCaptureDeviceConfiguration;