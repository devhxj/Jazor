namespace ECMAScript;

/// <summary>
/// DisplayMediaStreamOptions
/// </summary>
[ECMAScript]
[Description("@#DisplayMediaStreamOptions")]
public record DisplayMediaStreamOptions(
    [property: Description("@#video")]Either<bool, MediaTrackConstraints>? Video = default,
	[property: Description("@#audio")]Either<bool, MediaTrackConstraints>? Audio = default,
	[property: Description("@#controller")]CaptureController? Controller = default,
	[property: Description("@#selfBrowserSurface")]SelfCapturePreferenceEnum? SelfBrowserSurface = default,
	[property: Description("@#systemAudio")]SystemAudioPreferenceEnum? SystemAudio = default,
	[property: Description("@#windowAudio")]WindowAudioPreferenceEnum? WindowAudio = default,
	[property: Description("@#surfaceSwitching")]SurfaceSwitchingPreferenceEnum? SurfaceSwitching = default,
	[property: Description("@#monitorTypeSurfaces")]MonitorTypeSurfacesEnum? MonitorTypeSurfaces = default);