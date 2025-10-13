namespace ECMAScript;

/// <summary>
/// PhotoCapabilities
/// </summary>
[ECMAScript]
[Description("@#PhotoCapabilities")]
public record PhotoCapabilities(
    [property: Description("@#redEyeReduction")]RedEyeReduction? RedEyeReduction = default,
	[property: Description("@#imageHeight")]MediaSettingsRange? ImageHeight = default,
	[property: Description("@#imageWidth")]MediaSettingsRange? ImageWidth = default,
	[property: Description("@#fillLightMode")]FillLightMode[]? FillLightMode = default);

/// <summary>
/// PhotoSettings
/// </summary>
[ECMAScript]
[Description("@#PhotoSettings")]
public record PhotoSettings(
    [property: Description("@#fillLightMode")]FillLightMode? FillLightMode = default,
	[property: Description("@#imageHeight")]double ImageHeight = default,
	[property: Description("@#imageWidth")]double ImageWidth = default,
	[property: Description("@#redEyeReduction")]bool RedEyeReduction = default);

/// <summary>
/// MediaSettingsRange
/// </summary>
[ECMAScript]
[Description("@#MediaSettingsRange")]
public record MediaSettingsRange(
    [property: Description("@#max")]double Max = default,
	[property: Description("@#min")]double Min = default,
	[property: Description("@#step")]double Step = default);

/// <summary>
/// MediaTrackSupportedConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaTrackSupportedConstraints")]
public record MediaTrackSupportedConstraints(
    [property: Description("@#whiteBalanceMode")]bool WhiteBalanceMode = true,
	[property: Description("@#exposureMode")]bool ExposureMode = true,
	[property: Description("@#focusMode")]bool FocusMode = true,
	[property: Description("@#pointsOfInterest")]bool PointsOfInterest = true,
	[property: Description("@#exposureCompensation")]bool ExposureCompensation = true,
	[property: Description("@#exposureTime")]bool ExposureTime = true,
	[property: Description("@#colorTemperature")]bool ColorTemperature = true,
	[property: Description("@#iso")]bool Iso = true,
	[property: Description("@#brightness")]bool Brightness = true,
	[property: Description("@#contrast")]bool Contrast = true,
	[property: Description("@#pan")]bool Pan = true,
	[property: Description("@#saturation")]bool Saturation = true,
	[property: Description("@#sharpness")]bool Sharpness = true,
	[property: Description("@#focusDistance")]bool FocusDistance = true,
	[property: Description("@#tilt")]bool Tilt = true,
	[property: Description("@#zoom")]bool Zoom = true,
	[property: Description("@#torch")]bool Torch = true,
	[property: Description("@#width")]bool Width = true,
	[property: Description("@#height")]bool Height = true,
	[property: Description("@#aspectRatio")]bool AspectRatio = true,
	[property: Description("@#frameRate")]bool FrameRate = true,
	[property: Description("@#facingMode")]bool FacingMode = true,
	[property: Description("@#resizeMode")]bool ResizeMode = true,
	[property: Description("@#sampleRate")]bool SampleRate = true,
	[property: Description("@#sampleSize")]bool SampleSize = true,
	[property: Description("@#echoCancellation")]bool EchoCancellation = true,
	[property: Description("@#autoGainControl")]bool AutoGainControl = true,
	[property: Description("@#noiseSuppression")]bool NoiseSuppression = true,
	[property: Description("@#latency")]bool Latency = true,
	[property: Description("@#channelCount")]bool ChannelCount = true,
	[property: Description("@#deviceId")]bool DeviceId = true,
	[property: Description("@#groupId")]bool GroupId = true,
	[property: Description("@#backgroundBlur")]bool BackgroundBlur = true,
	[property: Description("@#displaySurface")]bool DisplaySurface = true,
	[property: Description("@#logicalSurface")]bool LogicalSurface = true,
	[property: Description("@#cursor")]bool Cursor = true,
	[property: Description("@#restrictOwnAudio")]bool RestrictOwnAudio = true,
	[property: Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = true)
{
    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]bool whiteBalanceMode = true,
        [Description("@#exposureMode")]bool exposureMode = true,
        [Description("@#focusMode")]bool focusMode = true,
        [Description("@#pointsOfInterest")]bool pointsOfInterest = true,
        [Description("@#exposureCompensation")]bool exposureCompensation = true,
        [Description("@#exposureTime")]bool exposureTime = true,
        [Description("@#colorTemperature")]bool colorTemperature = true,
        [Description("@#iso")]bool iso = true,
        [Description("@#brightness")]bool brightness = true,
        [Description("@#contrast")]bool contrast = true,
        [Description("@#pan")]bool pan = true,
        [Description("@#saturation")]bool saturation = true,
        [Description("@#sharpness")]bool sharpness = true,
        [Description("@#focusDistance")]bool focusDistance = true,
        [Description("@#tilt")]bool tilt = true,
        [Description("@#zoom")]bool zoom = true,
        [Description("@#torch")]bool torch = true);

    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalWidthHeightAspectRatio16(
        [Description("@#width")]bool width = true,
        [Description("@#height")]bool height = true,
        [Description("@#aspectRatio")]bool aspectRatio = true,
        [Description("@#frameRate")]bool frameRate = true,
        [Description("@#facingMode")]bool facingMode = true,
        [Description("@#resizeMode")]bool resizeMode = true,
        [Description("@#sampleRate")]bool sampleRate = true,
        [Description("@#sampleSize")]bool sampleSize = true,
        [Description("@#echoCancellation")]bool echoCancellation = true,
        [Description("@#autoGainControl")]bool autoGainControl = true,
        [Description("@#noiseSuppression")]bool noiseSuppression = true,
        [Description("@#latency")]bool latency = true,
        [Description("@#channelCount")]bool channelCount = true,
        [Description("@#deviceId")]bool deviceId = true,
        [Description("@#groupId")]bool groupId = true,
        [Description("@#backgroundBlur")]bool backgroundBlur = true);

    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalDisplaySurfaceLogicalSurfaceCursor5(
        [Description("@#displaySurface")]bool displaySurface = true,
        [Description("@#logicalSurface")]bool logicalSurface = true,
        [Description("@#cursor")]bool cursor = true,
        [Description("@#restrictOwnAudio")]bool restrictOwnAudio = true,
        [Description("@#suppressLocalAudioPlayback")]bool suppressLocalAudioPlayback = true);
}

/// <summary>
/// MediaTrackCapabilities
/// </summary>
[ECMAScript]
[Description("@#MediaTrackCapabilities")]
public record MediaTrackCapabilities(
    [property: Description("@#whiteBalanceMode")]string[]? WhiteBalanceMode = default,
	[property: Description("@#exposureMode")]string[]? ExposureMode = default,
	[property: Description("@#focusMode")]string[]? FocusMode = default,
	[property: Description("@#exposureCompensation")]MediaSettingsRange? ExposureCompensation = default,
	[property: Description("@#exposureTime")]MediaSettingsRange? ExposureTime = default,
	[property: Description("@#colorTemperature")]MediaSettingsRange? ColorTemperature = default,
	[property: Description("@#iso")]MediaSettingsRange? Iso = default,
	[property: Description("@#brightness")]MediaSettingsRange? Brightness = default,
	[property: Description("@#contrast")]MediaSettingsRange? Contrast = default,
	[property: Description("@#saturation")]MediaSettingsRange? Saturation = default,
	[property: Description("@#sharpness")]MediaSettingsRange? Sharpness = default,
	[property: Description("@#focusDistance")]MediaSettingsRange? FocusDistance = default,
	[property: Description("@#pan")]MediaSettingsRange? Pan = default,
	[property: Description("@#tilt")]MediaSettingsRange? Tilt = default,
	[property: Description("@#zoom")]MediaSettingsRange? Zoom = default,
	[property: Description("@#torch")]bool[]? Torch = default,
	[property: Description("@#width")]ULongRange? Width = default,
	[property: Description("@#height")]ULongRange? Height = default,
	[property: Description("@#aspectRatio")]DoubleRange? AspectRatio = default,
	[property: Description("@#frameRate")]DoubleRange? FrameRate = default,
	[property: Description("@#facingMode")]string[]? FacingMode = default,
	[property: Description("@#resizeMode")]string[]? ResizeMode = default,
	[property: Description("@#sampleRate")]ULongRange? SampleRate = default,
	[property: Description("@#sampleSize")]ULongRange? SampleSize = default,
	[property: Description("@#echoCancellation")]Either<bool, string>[]? EchoCancellation = default,
	[property: Description("@#autoGainControl")]bool[]? AutoGainControl = default,
	[property: Description("@#noiseSuppression")]bool[]? NoiseSuppression = default,
	[property: Description("@#latency")]DoubleRange? Latency = default,
	[property: Description("@#channelCount")]ULongRange? ChannelCount = default,
	[property: Description("@#deviceId")]string? DeviceId = default,
	[property: Description("@#groupId")]string? GroupId = default,
	[property: Description("@#backgroundBlur")]bool[]? BackgroundBlur = default,
	[property: Description("@#displaySurface")]string? DisplaySurface = default,
	[property: Description("@#logicalSurface")]bool LogicalSurface = default,
	[property: Description("@#cursor")]string[]? Cursor = default)
{
    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalWhiteBalanceModeExposureModeFocusMode16(
        [Description("@#whiteBalanceMode")]string[]? WhiteBalanceMode = default,
        [Description("@#exposureMode")]string[]? ExposureMode = default,
        [Description("@#focusMode")]string[]? FocusMode = default,
        [Description("@#exposureCompensation")]MediaSettingsRange? ExposureCompensation = default,
        [Description("@#exposureTime")]MediaSettingsRange? ExposureTime = default,
        [Description("@#colorTemperature")]MediaSettingsRange? ColorTemperature = default,
        [Description("@#iso")]MediaSettingsRange? Iso = default,
        [Description("@#brightness")]MediaSettingsRange? Brightness = default,
        [Description("@#contrast")]MediaSettingsRange? Contrast = default,
        [Description("@#saturation")]MediaSettingsRange? Saturation = default,
        [Description("@#sharpness")]MediaSettingsRange? Sharpness = default,
        [Description("@#focusDistance")]MediaSettingsRange? FocusDistance = default,
        [Description("@#pan")]MediaSettingsRange? Pan = default,
        [Description("@#tilt")]MediaSettingsRange? Tilt = default,
        [Description("@#zoom")]MediaSettingsRange? Zoom = default,
        [Description("@#torch")]bool[]? Torch = default);

    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalWidthHeightAspectRatio16(
        [Description("@#width")]ULongRange? Width = default,
        [Description("@#height")]ULongRange? Height = default,
        [Description("@#aspectRatio")]DoubleRange? AspectRatio = default,
        [Description("@#frameRate")]DoubleRange? FrameRate = default,
        [Description("@#facingMode")]string[]? FacingMode = default,
        [Description("@#resizeMode")]string[]? ResizeMode = default,
        [Description("@#sampleRate")]ULongRange? SampleRate = default,
        [Description("@#sampleSize")]ULongRange? SampleSize = default,
        [Description("@#echoCancellation")]Either<bool, string>[]? EchoCancellation = default,
        [Description("@#autoGainControl")]bool[]? AutoGainControl = default,
        [Description("@#noiseSuppression")]bool[]? NoiseSuppression = default,
        [Description("@#latency")]DoubleRange? Latency = default,
        [Description("@#channelCount")]ULongRange? ChannelCount = default,
        [Description("@#deviceId")]string? DeviceId = default,
        [Description("@#groupId")]string? GroupId = default,
        [Description("@#backgroundBlur")]bool[]? BackgroundBlur = default);

    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalDisplaySurfaceLogicalSurfaceCursor(
        [Description("@#displaySurface")]string? DisplaySurface = default,
        [Description("@#logicalSurface")]bool LogicalSurface = default,
        [Description("@#cursor")]string[]? Cursor = default);
}

/// <summary>
/// MediaTrackConstraintSet
/// </summary>
[ECMAScript]
[Description("@#MediaTrackConstraintSet")]
public record MediaTrackConstraintSet(
    [property: Description("@#whiteBalanceMode")]ConstrainDOMString? WhiteBalanceMode = default,
	[property: Description("@#exposureMode")]ConstrainDOMString? ExposureMode = default,
	[property: Description("@#focusMode")]ConstrainDOMString? FocusMode = default,
	[property: Description("@#pointsOfInterest")]ConstrainPoint2D? PointsOfInterest = default,
	[property: Description("@#exposureCompensation")]ConstrainDouble? ExposureCompensation = default,
	[property: Description("@#exposureTime")]ConstrainDouble? ExposureTime = default,
	[property: Description("@#colorTemperature")]ConstrainDouble? ColorTemperature = default,
	[property: Description("@#iso")]ConstrainDouble? Iso = default,
	[property: Description("@#brightness")]ConstrainDouble? Brightness = default,
	[property: Description("@#contrast")]ConstrainDouble? Contrast = default,
	[property: Description("@#saturation")]ConstrainDouble? Saturation = default,
	[property: Description("@#sharpness")]ConstrainDouble? Sharpness = default,
	[property: Description("@#focusDistance")]ConstrainDouble? FocusDistance = default,
	[property: Description("@#pan")]Either<bool, ConstrainDouble>? Pan = default,
	[property: Description("@#tilt")]Either<bool, ConstrainDouble>? Tilt = default,
	[property: Description("@#zoom")]Either<bool, ConstrainDouble>? Zoom = default,
	[property: Description("@#torch")]ConstrainBoolean? Torch = default,
	[property: Description("@#width")]ConstrainULong? Width = default,
	[property: Description("@#height")]ConstrainULong? Height = default,
	[property: Description("@#aspectRatio")]ConstrainDouble? AspectRatio = default,
	[property: Description("@#frameRate")]ConstrainDouble? FrameRate = default,
	[property: Description("@#facingMode")]ConstrainDOMString? FacingMode = default,
	[property: Description("@#resizeMode")]ConstrainDOMString? ResizeMode = default,
	[property: Description("@#sampleRate")]ConstrainULong? SampleRate = default,
	[property: Description("@#sampleSize")]ConstrainULong? SampleSize = default,
	[property: Description("@#echoCancellation")]ConstrainBooleanOrDOMString? EchoCancellation = default,
	[property: Description("@#autoGainControl")]ConstrainBoolean? AutoGainControl = default,
	[property: Description("@#noiseSuppression")]ConstrainBoolean? NoiseSuppression = default,
	[property: Description("@#latency")]ConstrainDouble? Latency = default,
	[property: Description("@#channelCount")]ConstrainULong? ChannelCount = default,
	[property: Description("@#deviceId")]ConstrainDOMString? DeviceId = default,
	[property: Description("@#groupId")]ConstrainDOMString? GroupId = default,
	[property: Description("@#backgroundBlur")]ConstrainBoolean? BackgroundBlur = default,
	[property: Description("@#displaySurface")]ConstrainDOMString? DisplaySurface = default,
	[property: Description("@#logicalSurface")]ConstrainBoolean? LogicalSurface = default,
	[property: Description("@#cursor")]ConstrainDOMString? Cursor = default,
	[property: Description("@#restrictOwnAudio")]ConstrainBoolean? RestrictOwnAudio = default,
	[property: Description("@#suppressLocalAudioPlayback")]ConstrainBoolean? SuppressLocalAudioPlayback = default)
{
    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]ConstrainDOMString? WhiteBalanceMode = default,
        [Description("@#exposureMode")]ConstrainDOMString? ExposureMode = default,
        [Description("@#focusMode")]ConstrainDOMString? FocusMode = default,
        [Description("@#pointsOfInterest")]ConstrainPoint2D? PointsOfInterest = default,
        [Description("@#exposureCompensation")]ConstrainDouble? ExposureCompensation = default,
        [Description("@#exposureTime")]ConstrainDouble? ExposureTime = default,
        [Description("@#colorTemperature")]ConstrainDouble? ColorTemperature = default,
        [Description("@#iso")]ConstrainDouble? Iso = default,
        [Description("@#brightness")]ConstrainDouble? Brightness = default,
        [Description("@#contrast")]ConstrainDouble? Contrast = default,
        [Description("@#saturation")]ConstrainDouble? Saturation = default,
        [Description("@#sharpness")]ConstrainDouble? Sharpness = default,
        [Description("@#focusDistance")]ConstrainDouble? FocusDistance = default,
        [Description("@#pan")]Either<bool, ConstrainDouble>? Pan = default,
        [Description("@#tilt")]Either<bool, ConstrainDouble>? Tilt = default,
        [Description("@#zoom")]Either<bool, ConstrainDouble>? Zoom = default,
        [Description("@#torch")]ConstrainBoolean? Torch = default);

    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalWidthHeightAspectRatio16(
        [Description("@#width")]ConstrainULong? Width = default,
        [Description("@#height")]ConstrainULong? Height = default,
        [Description("@#aspectRatio")]ConstrainDouble? AspectRatio = default,
        [Description("@#frameRate")]ConstrainDouble? FrameRate = default,
        [Description("@#facingMode")]ConstrainDOMString? FacingMode = default,
        [Description("@#resizeMode")]ConstrainDOMString? ResizeMode = default,
        [Description("@#sampleRate")]ConstrainULong? SampleRate = default,
        [Description("@#sampleSize")]ConstrainULong? SampleSize = default,
        [Description("@#echoCancellation")]ConstrainBooleanOrDOMString? EchoCancellation = default,
        [Description("@#autoGainControl")]ConstrainBoolean? AutoGainControl = default,
        [Description("@#noiseSuppression")]ConstrainBoolean? NoiseSuppression = default,
        [Description("@#latency")]ConstrainDouble? Latency = default,
        [Description("@#channelCount")]ConstrainULong? ChannelCount = default,
        [Description("@#deviceId")]ConstrainDOMString? DeviceId = default,
        [Description("@#groupId")]ConstrainDOMString? GroupId = default,
        [Description("@#backgroundBlur")]ConstrainBoolean? BackgroundBlur = default);

    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalDisplaySurfaceLogicalSurfaceCursor5(
        [Description("@#displaySurface")]ConstrainDOMString? DisplaySurface = default,
        [Description("@#logicalSurface")]ConstrainBoolean? LogicalSurface = default,
        [Description("@#cursor")]ConstrainDOMString? Cursor = default,
        [Description("@#restrictOwnAudio")]ConstrainBoolean? RestrictOwnAudio = default,
        [Description("@#suppressLocalAudioPlayback")]ConstrainBoolean? SuppressLocalAudioPlayback = default);
}

/// <summary>
/// MediaTrackSettings
/// </summary>
[ECMAScript]
[Description("@#MediaTrackSettings")]
public record MediaTrackSettings(
    [property: Description("@#whiteBalanceMode")]string? WhiteBalanceMode = default,
	[property: Description("@#exposureMode")]string? ExposureMode = default,
	[property: Description("@#focusMode")]string? FocusMode = default,
	[property: Description("@#pointsOfInterest")]Point2D[]? PointsOfInterest = default,
	[property: Description("@#exposureCompensation")]double ExposureCompensation = default,
	[property: Description("@#exposureTime")]double ExposureTime = default,
	[property: Description("@#colorTemperature")]double ColorTemperature = default,
	[property: Description("@#iso")]double Iso = default,
	[property: Description("@#brightness")]double Brightness = default,
	[property: Description("@#contrast")]double Contrast = default,
	[property: Description("@#saturation")]double Saturation = default,
	[property: Description("@#sharpness")]double Sharpness = default,
	[property: Description("@#focusDistance")]double FocusDistance = default,
	[property: Description("@#pan")]double Pan = default,
	[property: Description("@#tilt")]double Tilt = default,
	[property: Description("@#zoom")]double Zoom = default,
	[property: Description("@#torch")]bool Torch = default,
	[property: Description("@#width")]uint Width = default,
	[property: Description("@#height")]uint Height = default,
	[property: Description("@#aspectRatio")]double AspectRatio = default,
	[property: Description("@#frameRate")]double FrameRate = default,
	[property: Description("@#facingMode")]string? FacingMode = default,
	[property: Description("@#resizeMode")]string? ResizeMode = default,
	[property: Description("@#sampleRate")]uint SampleRate = default,
	[property: Description("@#sampleSize")]uint SampleSize = default,
	[property: Description("@#echoCancellation")]Either<bool, string>? EchoCancellation = default,
	[property: Description("@#autoGainControl")]bool AutoGainControl = default,
	[property: Description("@#noiseSuppression")]bool NoiseSuppression = default,
	[property: Description("@#latency")]double Latency = default,
	[property: Description("@#channelCount")]uint ChannelCount = default,
	[property: Description("@#deviceId")]string? DeviceId = default,
	[property: Description("@#groupId")]string? GroupId = default,
	[property: Description("@#backgroundBlur")]bool BackgroundBlur = default,
	[property: Description("@#displaySurface")]string? DisplaySurface = default,
	[property: Description("@#logicalSurface")]bool LogicalSurface = default,
	[property: Description("@#cursor")]string? Cursor = default,
	[property: Description("@#restrictOwnAudio")]bool RestrictOwnAudio = default,
	[property: Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = default,
	[property: Description("@#screenPixelRatio")]double ScreenPixelRatio = default)
{
    [Category("optional")]
    public extern static MediaTrackSettings OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]string? WhiteBalanceMode = default,
        [Description("@#exposureMode")]string? ExposureMode = default,
        [Description("@#focusMode")]string? FocusMode = default,
        [Description("@#pointsOfInterest")]Point2D[]? PointsOfInterest = default,
        [Description("@#exposureCompensation")]double ExposureCompensation = default,
        [Description("@#exposureTime")]double ExposureTime = default,
        [Description("@#colorTemperature")]double ColorTemperature = default,
        [Description("@#iso")]double Iso = default,
        [Description("@#brightness")]double Brightness = default,
        [Description("@#contrast")]double Contrast = default,
        [Description("@#saturation")]double Saturation = default,
        [Description("@#sharpness")]double Sharpness = default,
        [Description("@#focusDistance")]double FocusDistance = default,
        [Description("@#pan")]double Pan = default,
        [Description("@#tilt")]double Tilt = default,
        [Description("@#zoom")]double Zoom = default,
        [Description("@#torch")]bool Torch = default);

    [Category("optional")]
    public extern static MediaTrackSettings OptionalWidthHeightAspectRatio16(
        [Description("@#width")]uint Width = default,
        [Description("@#height")]uint Height = default,
        [Description("@#aspectRatio")]double AspectRatio = default,
        [Description("@#frameRate")]double FrameRate = default,
        [Description("@#facingMode")]string? FacingMode = default,
        [Description("@#resizeMode")]string? ResizeMode = default,
        [Description("@#sampleRate")]uint SampleRate = default,
        [Description("@#sampleSize")]uint SampleSize = default,
        [Description("@#echoCancellation")]Either<bool, string>? EchoCancellation = default,
        [Description("@#autoGainControl")]bool AutoGainControl = default,
        [Description("@#noiseSuppression")]bool NoiseSuppression = default,
        [Description("@#latency")]double Latency = default,
        [Description("@#channelCount")]uint ChannelCount = default,
        [Description("@#deviceId")]string? DeviceId = default,
        [Description("@#groupId")]string? GroupId = default,
        [Description("@#backgroundBlur")]bool BackgroundBlur = default);

    [Category("optional")]
    public extern static MediaTrackSettings OptionalDisplaySurfaceLogicalSurfaceCursor6(
        [Description("@#displaySurface")]string? DisplaySurface = default,
        [Description("@#logicalSurface")]bool LogicalSurface = default,
        [Description("@#cursor")]string? Cursor = default,
        [Description("@#restrictOwnAudio")]bool RestrictOwnAudio = default,
        [Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = default,
        [Description("@#screenPixelRatio")]double ScreenPixelRatio = default);
}

/// <summary>
/// ConstrainPoint2DParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainPoint2DParameters")]
public record ConstrainPoint2DParameters(
    [property: Description("@#exact")]Point2D[]? Exact = default,
	[property: Description("@#ideal")]Point2D[]? Ideal = default);

/// <summary>
/// Point2D
/// </summary>
[ECMAScript]
[Description("@#Point2D")]
public record Point2D(
    [property: Description("@#x")]double X = 0.0d,
	[property: Description("@#y")]double Y = 0.0d);

/// <summary>
/// ImageCapture
/// </summary>
[ECMAScript]
[Description("@#ImageCapture")]
public class ImageCapture
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="videoTrack">videoTrack</param>
    public extern ImageCapture(MediaStreamTrack videoTrack);

    /// <summary>
    /// takePhoto
    /// </summary>
    /// <param name="photoSettings">photoSettings</param>
    [Description("@#takePhoto")]
    public extern PromiseResult<Blob> TakePhoto(PhotoSettings? photoSettings = default);

    /// <summary>
    /// getPhotoCapabilities
    /// </summary>
    [Description("@#getPhotoCapabilities")]
    public extern PromiseResult<PhotoCapabilities> GetPhotoCapabilities();

    /// <summary>
    /// getPhotoSettings
    /// </summary>
    [Description("@#getPhotoSettings")]
    public extern PromiseResult<PhotoSettings> GetPhotoSettings();

    /// <summary>
    /// grabFrame
    /// </summary>
    [Description("@#grabFrame")]
    public extern PromiseResult<ImageBitmap> GrabFrame();

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack Track { get; }
}