namespace ECMAScript;

/// <summary>
/// AacBitstreamFormat
/// </summary>
[Description("@#AacBitstreamFormat")]
[ECMAScript]
[String]
public enum AacBitstreamFormat
{
    [Description("@#Aac")]
    Aac = 0,

    [Description("@#Adts")]
    Adts = 1
}

/// <summary>
/// AccelerometerLocalCoordinateSystem
/// </summary>
[Description("@#AccelerometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum AccelerometerLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// AlignSetting
/// </summary>
[Description("@#AlignSetting")]
[ECMAScript]
[String]
public enum AlignSetting
{
    [Description("@#Start")]
    Start = 0,

    [Description("@#Center")]
    Center = 1,

    [Description("@#End")]
    End = 2,

    [Description("@#Left")]
    Left = 3,

    [Description("@#Right")]
    Right = 4
}

/// <summary>
/// AlphaOption
/// </summary>
[Description("@#AlphaOption")]
[ECMAScript]
[String]
public enum AlphaOption
{
    [Description("@#Keep")]
    Keep = 0,

    [Description("@#Discard")]
    Discard = 1
}

/// <summary>
/// AnimationPlayState
/// </summary>
[Description("@#AnimationPlayState")]
[ECMAScript]
[String]
public enum AnimationPlayState
{
    [Description("@#Idle")]
    Idle = 0,

    [Description("@#Running")]
    Running = 1,

    [Description("@#Paused")]
    Paused = 2,

    [Description("@#Finished")]
    Finished = 3
}

/// <summary>
/// AnimationReplaceState
/// </summary>
[Description("@#AnimationReplaceState")]
[ECMAScript]
[String]
public enum AnimationReplaceState
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Removed")]
    Removed = 1,

    [Description("@#Persisted")]
    Persisted = 2
}

/// <summary>
/// AppBannerPromptOutcome
/// </summary>
[Description("@#AppBannerPromptOutcome")]
[ECMAScript]
[String]
public enum AppBannerPromptOutcome
{
    [Description("@#Accepted")]
    Accepted = 0,

    [Description("@#Dismissed")]
    Dismissed = 1
}

/// <summary>
/// AppendMode
/// </summary>
[Description("@#AppendMode")]
[ECMAScript]
[String]
public enum AppendMode
{
    [Description("@#Segments")]
    Segments = 0,

    [Description("@#Sequence")]
    Sequence = 1
}

/// <summary>
/// AttestationConveyancePreference
/// </summary>
[Description("@#AttestationConveyancePreference")]
[ECMAScript]
[String]
public enum AttestationConveyancePreference
{
    [Description("@#None")]
    None = 0,

    [Description("@#Indirect")]
    Indirect = 1,

    [Description("@#Direct")]
    Direct = 2,

    [Description("@#Enterprise")]
    Enterprise = 3
}

/// <summary>
/// AudioContextLatencyCategory
/// </summary>
[Description("@#AudioContextLatencyCategory")]
[ECMAScript]
[String]
public enum AudioContextLatencyCategory
{
    [Description("@#Balanced")]
    Balanced = 0,

    [Description("@#Interactive")]
    Interactive = 1,

    [Description("@#Playback")]
    Playback = 2
}

/// <summary>
/// AudioContextRenderSizeCategory
/// </summary>
[Description("@#AudioContextRenderSizeCategory")]
[ECMAScript]
[String]
public enum AudioContextRenderSizeCategory
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Hardware")]
    Hardware = 1
}

/// <summary>
/// AudioContextState
/// </summary>
[Description("@#AudioContextState")]
[ECMAScript]
[String]
public enum AudioContextState
{
    [Description("@#Suspended")]
    Suspended = 0,

    [Description("@#Running")]
    Running = 1,

    [Description("@#Closed")]
    Closed = 2
}

/// <summary>
/// AudioSampleFormat
/// </summary>
[Description("@#AudioSampleFormat")]
[ECMAScript]
[String]
public enum AudioSampleFormat
{
    [Description("@#U8")]
    U8 = 0,

    [Description("@#S16")]
    S16 = 1,

    [Description("@#S32")]
    S32 = 2,

    [Description("@#F32")]
    F32 = 3,

    [Description("@#U8Planar")]
    U8Planar = 4,

    [Description("@#S16Planar")]
    S16Planar = 5,

    [Description("@#S32Planar")]
    S32Planar = 6,

    [Description("@#F32Planar")]
    F32Planar = 7
}

/// <summary>
/// AudioSessionState
/// </summary>
[Description("@#AudioSessionState")]
[ECMAScript]
[String]
public enum AudioSessionState
{
    [Description("@#Inactive")]
    Inactive = 0,

    [Description("@#Active")]
    Active = 1,

    [Description("@#Interrupted")]
    Interrupted = 2
}

/// <summary>
/// AudioSessionType
/// </summary>
[Description("@#AudioSessionType")]
[ECMAScript]
[String]
public enum AudioSessionType
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Playback")]
    Playback = 1,

    [Description("@#Transient")]
    Transient = 2,

    [Description("@#TransientSolo")]
    TransientSolo = 3,

    [Description("@#Ambient")]
    Ambient = 4,

    [Description("@#PlayAndRecord")]
    PlayAndRecord = 5
}

/// <summary>
/// AudioSinkType
/// </summary>
[Description("@#AudioSinkType")]
[ECMAScript]
[String]
public enum AudioSinkType
{
    [Description("@#None")]
    None = 0
}

/// <summary>
/// AuthenticatorAttachment
/// </summary>
[Description("@#AuthenticatorAttachment")]
[ECMAScript]
[String]
public enum AuthenticatorAttachment
{
    [Description("@#Platform")]
    Platform = 0,

    [Description("@#CrossPlatform")]
    CrossPlatform = 1
}

/// <summary>
/// AuthenticatorTransport
/// </summary>
[Description("@#AuthenticatorTransport")]
[ECMAScript]
[String]
public enum AuthenticatorTransport
{
    [Description("@#Usb")]
    Usb = 0,

    [Description("@#Nfc")]
    Nfc = 1,

    [Description("@#Ble")]
    Ble = 2,

    [Description("@#SmartCard")]
    SmartCard = 3,

    [Description("@#Hybrid")]
    Hybrid = 4,

    [Description("@#Internal")]
    Internal = 5
}

/// <summary>
/// AutoKeyword
/// </summary>
[Description("@#AutoKeyword")]
[ECMAScript]
[String]
public enum AutoKeyword
{
    [Description("@#Auto")]
    Auto = 0
}

/// <summary>
/// AutomationRate
/// </summary>
[Description("@#AutomationRate")]
[ECMAScript]
[String]
public enum AutomationRate
{
    [Description("@#ARate")]
    ARate = 0,

    [Description("@#KRate")]
    KRate = 1
}

/// <summary>
/// AutoplayPolicy
/// </summary>
[Description("@#AutoplayPolicy")]
[ECMAScript]
[String]
public enum AutoplayPolicy
{
    [Description("@#Allowed")]
    Allowed = 0,

    [Description("@#AllowedMuted")]
    AllowedMuted = 1,

    [Description("@#Disallowed")]
    Disallowed = 2
}

/// <summary>
/// AutoplayPolicyMediaType
/// </summary>
[Description("@#AutoplayPolicyMediaType")]
[ECMAScript]
[String]
public enum AutoplayPolicyMediaType
{
    [Description("@#Mediaelement")]
    Mediaelement = 0,

    [Description("@#Audiocontext")]
    Audiocontext = 1
}

/// <summary>
/// AvcBitstreamFormat
/// </summary>
[Description("@#AvcBitstreamFormat")]
[ECMAScript]
[String]
public enum AvcBitstreamFormat
{
    [Description("@#Annexb")]
    Annexb = 0,

    [Description("@#Avc")]
    Avc = 1
}

/// <summary>
/// BackgroundFetchFailureReason
/// </summary>
[Description("@#BackgroundFetchFailureReason")]
[ECMAScript]
[String]
public enum BackgroundFetchFailureReason
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Aborted")]
    Aborted = 1,

    [Description("@#BadStatus")]
    BadStatus = 2,

    [Description("@#FetchError")]
    FetchError = 3,

    [Description("@#QuotaExceeded")]
    QuotaExceeded = 4,

    [Description("@#DownloadTotalExceeded")]
    DownloadTotalExceeded = 5
}

/// <summary>
/// BackgroundFetchResult
/// </summary>
[Description("@#BackgroundFetchResult")]
[ECMAScript]
[String]
public enum BackgroundFetchResult
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Success")]
    Success = 1,

    [Description("@#Failure")]
    Failure = 2
}

/// <summary>
/// BarcodeFormat
/// </summary>
[Description("@#BarcodeFormat")]
[ECMAScript]
[String]
public enum BarcodeFormat
{
    [Description("@#Aztec")]
    Aztec = 0,

    [Description("@#Code128")]
    Code128 = 1,

    [Description("@#Code39")]
    Code39 = 2,

    [Description("@#Code93")]
    Code93 = 3,

    [Description("@#Codabar")]
    Codabar = 4,

    [Description("@#DataMatrix")]
    DataMatrix = 5,

    [Description("@#Ean13")]
    Ean13 = 6,

    [Description("@#Ean8")]
    Ean8 = 7,

    [Description("@#Itf")]
    Itf = 8,

    [Description("@#Pdf417")]
    Pdf417 = 9,

    [Description("@#QrCode")]
    QrCode = 10,

    [Description("@#Unknown")]
    Unknown = 11,

    [Description("@#UpcA")]
    UpcA = 12,

    [Description("@#UpcE")]
    UpcE = 13
}

/// <summary>
/// BinaryType
/// </summary>
[Description("@#BinaryType")]
[ECMAScript]
[String]
public enum BinaryType
{
    [Description("@#Blob")]
    Blob = 0,

    [Description("@#Arraybuffer")]
    Arraybuffer = 1
}

/// <summary>
/// BiquadFilterType
/// </summary>
[Description("@#BiquadFilterType")]
[ECMAScript]
[String]
public enum BiquadFilterType
{
    [Description("@#Lowpass")]
    Lowpass = 0,

    [Description("@#Highpass")]
    Highpass = 1,

    [Description("@#Bandpass")]
    Bandpass = 2,

    [Description("@#Lowshelf")]
    Lowshelf = 3,

    [Description("@#Highshelf")]
    Highshelf = 4,

    [Description("@#Peaking")]
    Peaking = 5,

    [Description("@#Notch")]
    Notch = 6,

    [Description("@#Allpass")]
    Allpass = 7
}

/// <summary>
/// BitrateMode
/// </summary>
[Description("@#BitrateMode")]
[ECMAScript]
[String]
public enum BitrateMode
{
    [Description("@#Constant")]
    Constant = 0,

    [Description("@#Variable")]
    Variable = 1
}

/// <summary>
/// CSSBoxType
/// </summary>
[Description("@#CSSBoxType")]
[ECMAScript]
[String]
public enum CSSBoxType
{
    [Description("@#Margin")]
    Margin = 0,

    [Description("@#Border")]
    Border = 1,

    [Description("@#Padding")]
    Padding = 2,

    [Description("@#Content")]
    Content = 3
}

/// <summary>
/// CanPlayTypeResult
/// </summary>
[Description("@#CanPlayTypeResult")]
[ECMAScript]
[String]
public enum CanPlayTypeResult
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Maybe")]
    Maybe = 1,

    [Description("@#Probably")]
    Probably = 2
}

/// <summary>
/// CanvasDirection
/// </summary>
[Description("@#CanvasDirection")]
[ECMAScript]
[String]
public enum CanvasDirection
{
    [Description("@#Ltr")]
    Ltr = 0,

    [Description("@#Rtl")]
    Rtl = 1,

    [Description("@#Inherit")]
    Inherit = 2
}

/// <summary>
/// CanvasFillRule
/// </summary>
[Description("@#CanvasFillRule")]
[ECMAScript]
[String]
public enum CanvasFillRule
{
    [Description("@#Nonzero")]
    Nonzero = 0,

    [Description("@#Evenodd")]
    Evenodd = 1
}

/// <summary>
/// CanvasFontKerning
/// </summary>
[Description("@#CanvasFontKerning")]
[ECMAScript]
[String]
public enum CanvasFontKerning
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Normal")]
    Normal = 1,

    [Description("@#None")]
    None = 2
}

/// <summary>
/// CanvasFontStretch
/// </summary>
[Description("@#CanvasFontStretch")]
[ECMAScript]
[String]
public enum CanvasFontStretch
{
    [Description("@#UltraCondensed")]
    UltraCondensed = 0,

    [Description("@#ExtraCondensed")]
    ExtraCondensed = 1,

    [Description("@#Condensed")]
    Condensed = 2,

    [Description("@#SemiCondensed")]
    SemiCondensed = 3,

    [Description("@#Normal")]
    Normal = 4,

    [Description("@#SemiExpanded")]
    SemiExpanded = 5,

    [Description("@#Expanded")]
    Expanded = 6,

    [Description("@#ExtraExpanded")]
    ExtraExpanded = 7,

    [Description("@#UltraExpanded")]
    UltraExpanded = 8
}

/// <summary>
/// CanvasFontVariantCaps
/// </summary>
[Description("@#CanvasFontVariantCaps")]
[ECMAScript]
[String]
public enum CanvasFontVariantCaps
{
    [Description("@#Normal")]
    Normal = 0,

    [Description("@#SmallCaps")]
    SmallCaps = 1,

    [Description("@#AllSmallCaps")]
    AllSmallCaps = 2,

    [Description("@#PetiteCaps")]
    PetiteCaps = 3,

    [Description("@#AllPetiteCaps")]
    AllPetiteCaps = 4,

    [Description("@#Unicase")]
    Unicase = 5,

    [Description("@#TitlingCaps")]
    TitlingCaps = 6
}

/// <summary>
/// CanvasLineCap
/// </summary>
[Description("@#CanvasLineCap")]
[ECMAScript]
[String]
public enum CanvasLineCap
{
    [Description("@#Butt")]
    Butt = 0,

    [Description("@#Round")]
    Round = 1,

    [Description("@#Square")]
    Square = 2
}

/// <summary>
/// CanvasLineJoin
/// </summary>
[Description("@#CanvasLineJoin")]
[ECMAScript]
[String]
public enum CanvasLineJoin
{
    [Description("@#Round")]
    Round = 0,

    [Description("@#Bevel")]
    Bevel = 1,

    [Description("@#Miter")]
    Miter = 2
}

/// <summary>
/// CanvasTextAlign
/// </summary>
[Description("@#CanvasTextAlign")]
[ECMAScript]
[String]
public enum CanvasTextAlign
{
    [Description("@#Start")]
    Start = 0,

    [Description("@#End")]
    End = 1,

    [Description("@#Left")]
    Left = 2,

    [Description("@#Right")]
    Right = 3,

    [Description("@#Center")]
    Center = 4
}

/// <summary>
/// CanvasTextBaseline
/// </summary>
[Description("@#CanvasTextBaseline")]
[ECMAScript]
[String]
public enum CanvasTextBaseline
{
    [Description("@#Top")]
    Top = 0,

    [Description("@#Hanging")]
    Hanging = 1,

    [Description("@#Middle")]
    Middle = 2,

    [Description("@#Alphabetic")]
    Alphabetic = 3,

    [Description("@#Ideographic")]
    Ideographic = 4,

    [Description("@#Bottom")]
    Bottom = 5
}

/// <summary>
/// CanvasTextRendering
/// </summary>
[Description("@#CanvasTextRendering")]
[ECMAScript]
[String]
public enum CanvasTextRendering
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#OptimizeSpeed")]
    OptimizeSpeed = 1,

    [Description("@#OptimizeLegibility")]
    OptimizeLegibility = 2,

    [Description("@#GeometricPrecision")]
    GeometricPrecision = 3
}

/// <summary>
/// CaptureAction
/// </summary>
[Description("@#CaptureAction")]
[ECMAScript]
[String]
public enum CaptureAction
{
    [Description("@#Next")]
    Next = 0,

    [Description("@#Previous")]
    Previous = 1,

    [Description("@#First")]
    First = 2,

    [Description("@#Last")]
    Last = 3
}

/// <summary>
/// CaptureStartFocusBehavior
/// </summary>
[Description("@#CaptureStartFocusBehavior")]
[ECMAScript]
[String]
public enum CaptureStartFocusBehavior
{
    [Description("@#FocusCapturingApplication")]
    FocusCapturingApplication = 0,

    [Description("@#FocusCapturedSurface")]
    FocusCapturedSurface = 1,

    [Description("@#NoFocusChange")]
    NoFocusChange = 2
}

/// <summary>
/// ChannelCountMode
/// </summary>
[Description("@#ChannelCountMode")]
[ECMAScript]
[String]
public enum ChannelCountMode
{
    [Description("@#Max")]
    Max = 0,

    [Description("@#ClampedMax")]
    ClampedMax = 1,

    [Description("@#Explicit")]
    Explicit = 2
}

/// <summary>
/// ChannelInterpretation
/// </summary>
[Description("@#ChannelInterpretation")]
[ECMAScript]
[String]
public enum ChannelInterpretation
{
    [Description("@#Speakers")]
    Speakers = 0,

    [Description("@#Discrete")]
    Discrete = 1
}

/// <summary>
/// ClientCapability
/// </summary>
[Description("@#ClientCapability")]
[ECMAScript]
[String]
public enum ClientCapability
{
    [Description("@#ConditionalCreate")]
    ConditionalCreate = 0,

    [Description("@#ConditionalMediation")]
    ConditionalMediation = 1,

    [Description("@#HybridTransport")]
    HybridTransport = 2,

    [Description("@#PasskeyPlatformAuthenticator")]
    PasskeyPlatformAuthenticator = 3,

    [Description("@#UserVerifyingPlatformAuthenticator")]
    UserVerifyingPlatformAuthenticator = 4
}

/// <summary>
/// ClientLifecycleState
/// </summary>
[Description("@#ClientLifecycleState")]
[ECMAScript]
[String]
public enum ClientLifecycleState
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Frozen")]
    Frozen = 1
}

/// <summary>
/// ClientType
/// </summary>
[Description("@#ClientType")]
[ECMAScript]
[String]
public enum ClientType
{
    [Description("@#Window")]
    Window = 0,

    [Description("@#Worker")]
    Worker = 1,

    [Description("@#Sharedworker")]
    Sharedworker = 2,

    [Description("@#All")]
    All = 3
}

/// <summary>
/// CodecState
/// </summary>
[Description("@#CodecState")]
[ECMAScript]
[String]
public enum CodecState
{
    [Description("@#Unconfigured")]
    Unconfigured = 0,

    [Description("@#Configured")]
    Configured = 1,

    [Description("@#Closed")]
    Closed = 2
}

/// <summary>
/// ColorGamut
/// </summary>
[Description("@#ColorGamut")]
[ECMAScript]
[String]
public enum ColorGamut
{
    [Description("@#Srgb")]
    Srgb = 0,

    [Description("@#P3")]
    P3 = 1,

    [Description("@#Rec2020")]
    Rec2020 = 2
}

/// <summary>
/// ColorSpaceConversion
/// </summary>
[Description("@#ColorSpaceConversion")]
[ECMAScript]
[String]
public enum ColorSpaceConversion
{
    [Description("@#None")]
    None = 0,

    [Description("@#Default")]
    Default = 1
}

/// <summary>
/// CompositeOperation
/// </summary>
[Description("@#CompositeOperation")]
[ECMAScript]
[String]
public enum CompositeOperation
{
    [Description("@#Replace")]
    Replace = 0,

    [Description("@#Add")]
    Add = 1,

    [Description("@#Accumulate")]
    Accumulate = 2
}

/// <summary>
/// CompositeOperationOrAuto
/// </summary>
[Description("@#CompositeOperationOrAuto")]
[ECMAScript]
[String]
public enum CompositeOperationOrAuto
{
    [Description("@#Replace")]
    Replace = 0,

    [Description("@#Add")]
    Add = 1,

    [Description("@#Accumulate")]
    Accumulate = 2,

    [Description("@#Auto")]
    Auto = 3
}

/// <summary>
/// CompressionFormat
/// </summary>
[Description("@#CompressionFormat")]
[ECMAScript]
[String]
public enum CompressionFormat
{
    [Description("@#Deflate")]
    Deflate = 0,

    [Description("@#DeflateRaw")]
    DeflateRaw = 1,

    [Description("@#Gzip")]
    Gzip = 2
}

/// <summary>
/// ConnectionType
/// </summary>
[Description("@#ConnectionType")]
[ECMAScript]
[String]
public enum ConnectionType
{
    [Description("@#Bluetooth")]
    Bluetooth = 0,

    [Description("@#Cellular")]
    Cellular = 1,

    [Description("@#Ethernet")]
    Ethernet = 2,

    [Description("@#Mixed")]
    Mixed = 3,

    [Description("@#None")]
    None = 4,

    [Description("@#Other")]
    Other = 5,

    [Description("@#Unknown")]
    Unknown = 6,

    [Description("@#Wifi")]
    Wifi = 7,

    [Description("@#Wimax")]
    Wimax = 8
}

/// <summary>
/// ContactProperty
/// </summary>
[Description("@#ContactProperty")]
[ECMAScript]
[String]
public enum ContactProperty
{
    [Description("@#Address")]
    Address = 0,

    [Description("@#Email")]
    Email = 1,

    [Description("@#Icon")]
    Icon = 2,

    [Description("@#Name")]
    Name = 3,

    [Description("@#Tel")]
    Tel = 4
}

/// <summary>
/// ContentCategory
/// </summary>
[Description("@#ContentCategory")]
[ECMAScript]
[String]
public enum ContentCategory
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Homepage")]
    Homepage = 1,

    [Description("@#Article")]
    Article = 2,

    [Description("@#Video")]
    Video = 3,

    [Description("@#Audio")]
    Audio = 4
}

/// <summary>
/// CookieSameSite
/// </summary>
[Description("@#CookieSameSite")]
[ECMAScript]
[String]
public enum CookieSameSite
{
    [Description("@#Strict")]
    Strict = 0,

    [Description("@#Lax")]
    Lax = 1,

    [Description("@#None")]
    None = 2
}

/// <summary>
/// CredentialMediationRequirement
/// </summary>
[Description("@#CredentialMediationRequirement")]
[ECMAScript]
[String]
public enum CredentialMediationRequirement
{
    [Description("@#Silent")]
    Silent = 0,

    [Description("@#Optional")]
    Optional = 1,

    [Description("@#Conditional")]
    Conditional = 2,

    [Description("@#Required")]
    Required = 3
}

/// <summary>
/// CursorCaptureConstraint
/// </summary>
[Description("@#CursorCaptureConstraint")]
[ECMAScript]
[String]
public enum CursorCaptureConstraint
{
    [Description("@#Never")]
    Never = 0,

    [Description("@#Always")]
    Always = 1,

    [Description("@#Motion")]
    Motion = 2
}

/// <summary>
/// DOMParserSupportedType
/// </summary>
[Description("@#DOMParserSupportedType")]
[ECMAScript]
[String]
public enum DOMParserSupportedType
{
    [Description("@#TextHtml")]
    TextHtml = 0,

    [Description("@#TextXml")]
    TextXml = 1,

    [Description("@#ApplicationXml")]
    ApplicationXml = 2,

    [Description("@#ApplicationXhtmlXml")]
    ApplicationXhtmlXml = 3,

    [Description("@#ImageSvgXml")]
    ImageSvgXml = 4
}

/// <summary>
/// DevicePostureType
/// </summary>
[Description("@#DevicePostureType")]
[ECMAScript]
[String]
public enum DevicePostureType
{
    [Description("@#Continuous")]
    Continuous = 0,

    [Description("@#Folded")]
    Folded = 1
}

/// <summary>
/// DirectionSetting
/// </summary>
[Description("@#DirectionSetting")]
[ECMAScript]
[String]
public enum DirectionSetting
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Rl")]
    Rl = 1,

    [Description("@#Lr")]
    Lr = 2
}

/// <summary>
/// DisplayCaptureSurfaceType
/// </summary>
[Description("@#DisplayCaptureSurfaceType")]
[ECMAScript]
[String]
public enum DisplayCaptureSurfaceType
{
    [Description("@#Monitor")]
    Monitor = 0,

    [Description("@#Window")]
    Window = 1,

    [Description("@#Browser")]
    Browser = 2
}

/// <summary>
/// DistanceModelType
/// </summary>
[Description("@#DistanceModelType")]
[ECMAScript]
[String]
public enum DistanceModelType
{
    [Description("@#Linear")]
    Linear = 0,

    [Description("@#Inverse")]
    Inverse = 1,

    [Description("@#Exponential")]
    Exponential = 2
}

/// <summary>
/// DocumentReadyState
/// </summary>
[Description("@#DocumentReadyState")]
[ECMAScript]
[String]
public enum DocumentReadyState
{
    [Description("@#Loading")]
    Loading = 0,

    [Description("@#Interactive")]
    Interactive = 1,

    [Description("@#Complete")]
    Complete = 2
}

/// <summary>
/// DocumentVisibilityState
/// </summary>
[Description("@#DocumentVisibilityState")]
[ECMAScript]
[String]
public enum DocumentVisibilityState
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#Hidden")]
    Hidden = 1
}

/// <summary>
/// EffectiveConnectionType
/// </summary>
[Description("@#EffectiveConnectionType")]
[ECMAScript]
[String]
public enum EffectiveConnectionType
{
    [Description("@#_2g")]
    _2g = 0,

    [Description("@#_3g")]
    _3g = 1,

    [Description("@#_4g")]
    _4g = 2,

    [Description("@#Slow2g")]
    Slow2g = 3
}

/// <summary>
/// EncodedAudioChunkType
/// </summary>
[Description("@#EncodedAudioChunkType")]
[ECMAScript]
[String]
public enum EncodedAudioChunkType
{
    [Description("@#Key")]
    Key = 0,

    [Description("@#Delta")]
    Delta = 1
}

/// <summary>
/// EncodedVideoChunkType
/// </summary>
[Description("@#EncodedVideoChunkType")]
[ECMAScript]
[String]
public enum EncodedVideoChunkType
{
    [Description("@#Key")]
    Key = 0,

    [Description("@#Delta")]
    Delta = 1
}

/// <summary>
/// EndOfStreamError
/// </summary>
[Description("@#EndOfStreamError")]
[ECMAScript]
[String]
public enum EndOfStreamError
{
    [Description("@#Network")]
    Network = 0,

    [Description("@#Decode")]
    Decode = 1
}

/// <summary>
/// EndingType
/// </summary>
[Description("@#EndingType")]
[ECMAScript]
[String]
public enum EndingType
{
    [Description("@#Transparent")]
    Transparent = 0,

    [Description("@#Native")]
    Native = 1
}

/// <summary>
/// FenceReportingDestination
/// </summary>
[Description("@#FenceReportingDestination")]
[ECMAScript]
[String]
public enum FenceReportingDestination
{
    [Description("@#Buyer")]
    Buyer = 0,

    [Description("@#Seller")]
    Seller = 1,

    [Description("@#ComponentSeller")]
    ComponentSeller = 2,

    [Description("@#DirectSeller")]
    DirectSeller = 3,

    [Description("@#SharedStorageSelectUrl")]
    SharedStorageSelectUrl = 4
}

/// <summary>
/// FileSystemHandleKind
/// </summary>
[Description("@#FileSystemHandleKind")]
[ECMAScript]
[String]
public enum FileSystemHandleKind
{
    [Description("@#File")]
    File = 0,

    [Description("@#Directory")]
    Directory = 1
}

/// <summary>
/// FileSystemPermissionMode
/// </summary>
[Description("@#FileSystemPermissionMode")]
[ECMAScript]
[String]
public enum FileSystemPermissionMode
{
    [Description("@#Read")]
    Read = 0,

    [Description("@#Readwrite")]
    Readwrite = 1
}

/// <summary>
/// FillLightMode
/// </summary>
[Description("@#FillLightMode")]
[ECMAScript]
[String]
public enum FillLightMode
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Off")]
    Off = 1,

    [Description("@#Flash")]
    Flash = 2
}

/// <summary>
/// FillMode
/// </summary>
[Description("@#FillMode")]
[ECMAScript]
[String]
public enum FillMode
{
    [Description("@#None")]
    None = 0,

    [Description("@#Forwards")]
    Forwards = 1,

    [Description("@#Backwards")]
    Backwards = 2,

    [Description("@#Both")]
    Both = 3,

    [Description("@#Auto")]
    Auto = 4
}

/// <summary>
/// FlowControlType
/// </summary>
[Description("@#FlowControlType")]
[ECMAScript]
[String]
public enum FlowControlType
{
    [Description("@#None")]
    None = 0,

    [Description("@#Hardware")]
    Hardware = 1
}

/// <summary>
/// FocusableAreaSearchMode
/// </summary>
[Description("@#FocusableAreaSearchMode")]
[ECMAScript]
[String]
public enum FocusableAreaSearchMode
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#All")]
    All = 1
}

/// <summary>
/// FontFaceLoadStatus
/// </summary>
[Description("@#FontFaceLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceLoadStatus
{
    [Description("@#Unloaded")]
    Unloaded = 0,

    [Description("@#Loading")]
    Loading = 1,

    [Description("@#Loaded")]
    Loaded = 2,

    [Description("@#Error")]
    Error = 3
}

/// <summary>
/// FontFaceSetLoadStatus
/// </summary>
[Description("@#FontFaceSetLoadStatus")]
[ECMAScript]
[String]
public enum FontFaceSetLoadStatus
{
    [Description("@#Loading")]
    Loading = 0,

    [Description("@#Loaded")]
    Loaded = 1
}

/// <summary>
/// FrameType
/// </summary>
[Description("@#FrameType")]
[ECMAScript]
[String]
public enum FrameType
{
    [Description("@#Auxiliary")]
    Auxiliary = 0,

    [Description("@#TopLevel")]
    TopLevel = 1,

    [Description("@#Nested")]
    Nested = 2,

    [Description("@#None")]
    None = 3
}

/// <summary>
/// FullscreenNavigationUI
/// </summary>
[Description("@#FullscreenNavigationUI")]
[ECMAScript]
[String]
public enum FullscreenNavigationUI
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Show")]
    Show = 1,

    [Description("@#Hide")]
    Hide = 2
}

/// <summary>
/// GamepadHand
/// </summary>
[Description("@#GamepadHand")]
[ECMAScript]
[String]
public enum GamepadHand
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Left")]
    Left = 1,

    [Description("@#Right")]
    Right = 2
}

/// <summary>
/// GamepadHapticEffectType
/// </summary>
[Description("@#GamepadHapticEffectType")]
[ECMAScript]
[String]
public enum GamepadHapticEffectType
{
    [Description("@#DualRumble")]
    DualRumble = 0,

    [Description("@#TriggerRumble")]
    TriggerRumble = 1
}

/// <summary>
/// GamepadHapticsResult
/// </summary>
[Description("@#GamepadHapticsResult")]
[ECMAScript]
[String]
public enum GamepadHapticsResult
{
    [Description("@#Complete")]
    Complete = 0,

    [Description("@#Preempted")]
    Preempted = 1
}

/// <summary>
/// GamepadMappingType
/// </summary>
[Description("@#GamepadMappingType")]
[ECMAScript]
[String]
public enum GamepadMappingType
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Standard")]
    Standard = 1,

    [Description("@#XrStandard")]
    XrStandard = 2
}

/// <summary>
/// GyroscopeLocalCoordinateSystem
/// </summary>
[Description("@#GyroscopeLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum GyroscopeLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// HDCPVersion
/// </summary>
[Description("@#HDCPVersion")]
[ECMAScript]
[String]
public enum HDCPVersion
{
    [Description("@#_10")]
    _10 = 0,

    [Description("@#_11")]
    _11 = 1,

    [Description("@#_12")]
    _12 = 2,

    [Description("@#_13")]
    _13 = 3,

    [Description("@#_14")]
    _14 = 4,

    [Description("@#_20")]
    _20 = 5,

    [Description("@#_21")]
    _21 = 6,

    [Description("@#_22")]
    _22 = 7,

    [Description("@#_23")]
    _23 = 8
}

/// <summary>
/// HIDUnitSystem
/// </summary>
[Description("@#HIDUnitSystem")]
[ECMAScript]
[String]
public enum HIDUnitSystem
{
    [Description("@#None")]
    None = 0,

    [Description("@#SiLinear")]
    SiLinear = 1,

    [Description("@#SiRotation")]
    SiRotation = 2,

    [Description("@#EnglishLinear")]
    EnglishLinear = 3,

    [Description("@#EnglishRotation")]
    EnglishRotation = 4,

    [Description("@#VendorDefined")]
    VendorDefined = 5,

    [Description("@#Reserved")]
    Reserved = 6
}

/// <summary>
/// HardwareAcceleration
/// </summary>
[Description("@#HardwareAcceleration")]
[ECMAScript]
[String]
public enum HardwareAcceleration
{
    [Description("@#NoPreference")]
    NoPreference = 0,

    [Description("@#PreferHardware")]
    PreferHardware = 1,

    [Description("@#PreferSoftware")]
    PreferSoftware = 2
}

/// <summary>
/// HdrMetadataType
/// </summary>
[Description("@#HdrMetadataType")]
[ECMAScript]
[String]
public enum HdrMetadataType
{
    [Description("@#SmpteSt2086")]
    SmpteSt2086 = 0,

    [Description("@#SmpteSt209410")]
    SmpteSt209410 = 1,

    [Description("@#SmpteSt209440")]
    SmpteSt209440 = 2
}

/// <summary>
/// HevcBitstreamFormat
/// </summary>
[Description("@#HevcBitstreamFormat")]
[ECMAScript]
[String]
public enum HevcBitstreamFormat
{
    [Description("@#Annexb")]
    Annexb = 0,

    [Description("@#Hevc")]
    Hevc = 1
}

/// <summary>
/// IDBCursorDirection
/// </summary>
[Description("@#IDBCursorDirection")]
[ECMAScript]
[String]
public enum IDBCursorDirection
{
    [Description("@#Next")]
    Next = 0,

    [Description("@#Nextunique")]
    Nextunique = 1,

    [Description("@#Prev")]
    Prev = 2,

    [Description("@#Prevunique")]
    Prevunique = 3
}

/// <summary>
/// IDBRequestReadyState
/// </summary>
[Description("@#IDBRequestReadyState")]
[ECMAScript]
[String]
public enum IDBRequestReadyState
{
    [Description("@#Pending")]
    Pending = 0,

    [Description("@#Done")]
    Done = 1
}

/// <summary>
/// IDBTransactionDurability
/// </summary>
[Description("@#IDBTransactionDurability")]
[ECMAScript]
[String]
public enum IDBTransactionDurability
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Strict")]
    Strict = 1,

    [Description("@#Relaxed")]
    Relaxed = 2
}

/// <summary>
/// IDBTransactionMode
/// </summary>
[Description("@#IDBTransactionMode")]
[ECMAScript]
[String]
public enum IDBTransactionMode
{
    [Description("@#Readonly")]
    Readonly = 0,

    [Description("@#Readwrite")]
    Readwrite = 1,

    [Description("@#Versionchange")]
    Versionchange = 2
}

/// <summary>
/// IPAddressSpace
/// </summary>
[Description("@#IPAddressSpace")]
[ECMAScript]
[String]
public enum IPAddressSpace
{
    [Description("@#Public")]
    Public = 0,

    [Description("@#Private")]
    Private = 1,

    [Description("@#Local")]
    Local = 2
}

/// <summary>
/// IdentityCredentialRequestOptionsContext
/// </summary>
[Description("@#IdentityCredentialRequestOptionsContext")]
[ECMAScript]
[String]
public enum IdentityCredentialRequestOptionsContext
{
    [Description("@#Signin")]
    Signin = 0,

    [Description("@#Signup")]
    Signup = 1,

    [Description("@#Use")]
    Use = 2,

    [Description("@#Continue")]
    Continue = 3
}

/// <summary>
/// ImageOrientation
/// </summary>
[Description("@#ImageOrientation")]
[ECMAScript]
[String]
public enum ImageOrientation
{
    [Description("@#FromImage")]
    FromImage = 0,

    [Description("@#FlipY")]
    FlipY = 1
}

/// <summary>
/// ImageSmoothingQuality
/// </summary>
[Description("@#ImageSmoothingQuality")]
[ECMAScript]
[String]
public enum ImageSmoothingQuality
{
    [Description("@#Low")]
    Low = 0,

    [Description("@#Medium")]
    Medium = 1,

    [Description("@#High")]
    High = 2
}

/// <summary>
/// ItemType
/// </summary>
[Description("@#ItemType")]
[ECMAScript]
[String]
public enum ItemType
{
    [Description("@#Product")]
    Product = 0,

    [Description("@#Subscription")]
    Subscription = 1
}

/// <summary>
/// IterationCompositeOperation
/// </summary>
[Description("@#IterationCompositeOperation")]
[ECMAScript]
[String]
public enum IterationCompositeOperation
{
    [Description("@#Replace")]
    Replace = 0,

    [Description("@#Accumulate")]
    Accumulate = 1
}

/// <summary>
/// KAnonStatus
/// </summary>
[Description("@#KAnonStatus")]
[ECMAScript]
[String]
public enum KAnonStatus
{
    [Description("@#PassedAndEnforced")]
    PassedAndEnforced = 0,

    [Description("@#PassedNotEnforced")]
    PassedNotEnforced = 1,

    [Description("@#BelowThreshold")]
    BelowThreshold = 2,

    [Description("@#NotCalculated")]
    NotCalculated = 3
}

/// <summary>
/// KeyFormat
/// </summary>
[Description("@#KeyFormat")]
[ECMAScript]
[String]
public enum KeyFormat
{
    [Description("@#Raw")]
    Raw = 0,

    [Description("@#Spki")]
    Spki = 1,

    [Description("@#Pkcs8")]
    Pkcs8 = 2,

    [Description("@#Jwk")]
    Jwk = 3
}

/// <summary>
/// KeyType
/// </summary>
[Description("@#KeyType")]
[ECMAScript]
[String]
public enum KeyType
{
    [Description("@#Public")]
    Public = 0,

    [Description("@#Private")]
    Private = 1,

    [Description("@#Secret")]
    Secret = 2
}

/// <summary>
/// KeyUsage
/// </summary>
[Description("@#KeyUsage")]
[ECMAScript]
[String]
public enum KeyUsage
{
    [Description("@#Encrypt")]
    Encrypt = 0,

    [Description("@#Decrypt")]
    Decrypt = 1,

    [Description("@#Sign")]
    Sign = 2,

    [Description("@#Verify")]
    Verify = 3,

    [Description("@#DeriveKey")]
    DeriveKey = 4,

    [Description("@#DeriveBits")]
    DeriveBits = 5,

    [Description("@#WrapKey")]
    WrapKey = 6,

    [Description("@#UnwrapKey")]
    UnwrapKey = 7
}

/// <summary>
/// LandmarkType
/// </summary>
[Description("@#LandmarkType")]
[ECMAScript]
[String]
public enum LandmarkType
{
    [Description("@#Mouth")]
    Mouth = 0,

    [Description("@#Eye")]
    Eye = 1,

    [Description("@#Nose")]
    Nose = 2
}

/// <summary>
/// LargeBlobSupport
/// </summary>
[Description("@#LargeBlobSupport")]
[ECMAScript]
[String]
public enum LargeBlobSupport
{
    [Description("@#Required")]
    Required = 0,

    [Description("@#Preferred")]
    Preferred = 1
}

/// <summary>
/// LatencyMode
/// </summary>
[Description("@#LatencyMode")]
[ECMAScript]
[String]
public enum LatencyMode
{
    [Description("@#Quality")]
    Quality = 0,

    [Description("@#Realtime")]
    Realtime = 1
}

/// <summary>
/// LineAlignSetting
/// </summary>
[Description("@#LineAlignSetting")]
[ECMAScript]
[String]
public enum LineAlignSetting
{
    [Description("@#Start")]
    Start = 0,

    [Description("@#Center")]
    Center = 1,

    [Description("@#End")]
    End = 2
}

/// <summary>
/// LockMode
/// </summary>
[Description("@#LockMode")]
[ECMAScript]
[String]
public enum LockMode
{
    [Description("@#Shared")]
    Shared = 0,

    [Description("@#Exclusive")]
    Exclusive = 1
}

/// <summary>
/// LoginStatus
/// </summary>
[Description("@#LoginStatus")]
[ECMAScript]
[String]
public enum LoginStatus
{
    [Description("@#LoggedIn")]
    LoggedIn = 0,

    [Description("@#LoggedOut")]
    LoggedOut = 1
}

/// <summary>
/// MIDIPortConnectionState
/// </summary>
[Description("@#MIDIPortConnectionState")]
[ECMAScript]
[String]
public enum MIDIPortConnectionState
{
    [Description("@#Open")]
    Open = 0,

    [Description("@#Closed")]
    Closed = 1,

    [Description("@#Pending")]
    Pending = 2
}

/// <summary>
/// MIDIPortDeviceState
/// </summary>
[Description("@#MIDIPortDeviceState")]
[ECMAScript]
[String]
public enum MIDIPortDeviceState
{
    [Description("@#Disconnected")]
    Disconnected = 0,

    [Description("@#Connected")]
    Connected = 1
}

/// <summary>
/// MIDIPortType
/// </summary>
[Description("@#MIDIPortType")]
[ECMAScript]
[String]
public enum MIDIPortType
{
    [Description("@#Input")]
    Input = 0,

    [Description("@#Output")]
    Output = 1
}

/// <summary>
/// MLConv2dFilterOperandLayout
/// </summary>
[Description("@#MLConv2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConv2dFilterOperandLayout
{
    [Description("@#Oihw")]
    Oihw = 0,

    [Description("@#Hwio")]
    Hwio = 1,

    [Description("@#Ohwi")]
    Ohwi = 2,

    [Description("@#Ihwo")]
    Ihwo = 3
}

/// <summary>
/// MLConvTranspose2dFilterOperandLayout
/// </summary>
[Description("@#MLConvTranspose2dFilterOperandLayout")]
[ECMAScript]
[String]
public enum MLConvTranspose2dFilterOperandLayout
{
    [Description("@#Iohw")]
    Iohw = 0,

    [Description("@#Hwoi")]
    Hwoi = 1,

    [Description("@#Ohwi")]
    Ohwi = 2
}

/// <summary>
/// MLDeviceType
/// </summary>
[Description("@#MLDeviceType")]
[ECMAScript]
[String]
public enum MLDeviceType
{
    [Description("@#Cpu")]
    Cpu = 0,

    [Description("@#Gpu")]
    Gpu = 1
}

/// <summary>
/// MLGruWeightLayout
/// </summary>
[Description("@#MLGruWeightLayout")]
[ECMAScript]
[String]
public enum MLGruWeightLayout
{
    [Description("@#Zrn")]
    Zrn = 0,

    [Description("@#Rzn")]
    Rzn = 1
}

/// <summary>
/// MLInputOperandLayout
/// </summary>
[Description("@#MLInputOperandLayout")]
[ECMAScript]
[String]
public enum MLInputOperandLayout
{
    [Description("@#Nchw")]
    Nchw = 0,

    [Description("@#Nhwc")]
    Nhwc = 1
}

/// <summary>
/// MLInterpolationMode
/// </summary>
[Description("@#MLInterpolationMode")]
[ECMAScript]
[String]
public enum MLInterpolationMode
{
    [Description("@#NearestNeighbor")]
    NearestNeighbor = 0,

    [Description("@#Linear")]
    Linear = 1
}

/// <summary>
/// MLLstmWeightLayout
/// </summary>
[Description("@#MLLstmWeightLayout")]
[ECMAScript]
[String]
public enum MLLstmWeightLayout
{
    [Description("@#Iofg")]
    Iofg = 0,

    [Description("@#Ifgo")]
    Ifgo = 1
}

/// <summary>
/// MLOperandDataType
/// </summary>
[Description("@#MLOperandDataType")]
[ECMAScript]
[String]
public enum MLOperandDataType
{
    [Description("@#Float32")]
    Float32 = 0,

    [Description("@#Float16")]
    Float16 = 1,

    [Description("@#Int32")]
    Int32 = 2,

    [Description("@#Uint32")]
    Uint32 = 3,

    [Description("@#Int64")]
    Int64 = 4,

    [Description("@#Uint64")]
    Uint64 = 5,

    [Description("@#Int8")]
    Int8 = 6,

    [Description("@#Uint8")]
    Uint8 = 7
}

/// <summary>
/// MLPaddingMode
/// </summary>
[Description("@#MLPaddingMode")]
[ECMAScript]
[String]
public enum MLPaddingMode
{
    [Description("@#Constant")]
    Constant = 0,

    [Description("@#Edge")]
    Edge = 1,

    [Description("@#Reflection")]
    Reflection = 2,

    [Description("@#Symmetric")]
    Symmetric = 3
}

/// <summary>
/// MLPowerPreference
/// </summary>
[Description("@#MLPowerPreference")]
[ECMAScript]
[String]
public enum MLPowerPreference
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#HighPerformance")]
    HighPerformance = 1,

    [Description("@#LowPower")]
    LowPower = 2
}

/// <summary>
/// MLRecurrentNetworkDirection
/// </summary>
[Description("@#MLRecurrentNetworkDirection")]
[ECMAScript]
[String]
public enum MLRecurrentNetworkDirection
{
    [Description("@#Forward")]
    Forward = 0,

    [Description("@#Backward")]
    Backward = 1,

    [Description("@#Both")]
    Both = 2
}

/// <summary>
/// MLRoundingType
/// </summary>
[Description("@#MLRoundingType")]
[ECMAScript]
[String]
public enum MLRoundingType
{
    [Description("@#Floor")]
    Floor = 0,

    [Description("@#Ceil")]
    Ceil = 1
}

/// <summary>
/// MagnetometerLocalCoordinateSystem
/// </summary>
[Description("@#MagnetometerLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum MagnetometerLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// MediaDecodingType
/// </summary>
[Description("@#MediaDecodingType")]
[ECMAScript]
[String]
public enum MediaDecodingType
{
    [Description("@#File")]
    File = 0,

    [Description("@#MediaSource")]
    MediaSource = 1,

    [Description("@#Webrtc")]
    Webrtc = 2
}

/// <summary>
/// MediaDeviceKind
/// </summary>
[Description("@#MediaDeviceKind")]
[ECMAScript]
[String]
public enum MediaDeviceKind
{
    [Description("@#Audioinput")]
    Audioinput = 0,

    [Description("@#Audiooutput")]
    Audiooutput = 1,

    [Description("@#Videoinput")]
    Videoinput = 2
}

/// <summary>
/// MediaEncodingType
/// </summary>
[Description("@#MediaEncodingType")]
[ECMAScript]
[String]
public enum MediaEncodingType
{
    [Description("@#Record")]
    Record = 0,

    [Description("@#Webrtc")]
    Webrtc = 1
}

/// <summary>
/// MediaKeyMessageType
/// </summary>
[Description("@#MediaKeyMessageType")]
[ECMAScript]
[String]
public enum MediaKeyMessageType
{
    [Description("@#LicenseRequest")]
    LicenseRequest = 0,

    [Description("@#LicenseRenewal")]
    LicenseRenewal = 1,

    [Description("@#LicenseRelease")]
    LicenseRelease = 2,

    [Description("@#IndividualizationRequest")]
    IndividualizationRequest = 3
}

/// <summary>
/// MediaKeySessionClosedReason
/// </summary>
[Description("@#MediaKeySessionClosedReason")]
[ECMAScript]
[String]
public enum MediaKeySessionClosedReason
{
    [Description("@#InternalError")]
    InternalError = 0,

    [Description("@#ClosedByApplication")]
    ClosedByApplication = 1,

    [Description("@#ReleaseAcknowledged")]
    ReleaseAcknowledged = 2,

    [Description("@#HardwareContextReset")]
    HardwareContextReset = 3,

    [Description("@#ResourceEvicted")]
    ResourceEvicted = 4
}

/// <summary>
/// MediaKeySessionType
/// </summary>
[Description("@#MediaKeySessionType")]
[ECMAScript]
[String]
public enum MediaKeySessionType
{
    [Description("@#Temporary")]
    Temporary = 0,

    [Description("@#PersistentLicense")]
    PersistentLicense = 1
}

/// <summary>
/// MediaKeyStatus
/// </summary>
[Description("@#MediaKeyStatus")]
[ECMAScript]
[String]
public enum MediaKeyStatus
{
    [Description("@#Usable")]
    Usable = 0,

    [Description("@#Expired")]
    Expired = 1,

    [Description("@#Released")]
    Released = 2,

    [Description("@#OutputRestricted")]
    OutputRestricted = 3,

    [Description("@#OutputDownscaled")]
    OutputDownscaled = 4,

    [Description("@#UsableInFuture")]
    UsableInFuture = 5,

    [Description("@#StatusPending")]
    StatusPending = 6,

    [Description("@#InternalError")]
    InternalError = 7
}

/// <summary>
/// MediaKeysRequirement
/// </summary>
[Description("@#MediaKeysRequirement")]
[ECMAScript]
[String]
public enum MediaKeysRequirement
{
    [Description("@#Required")]
    Required = 0,

    [Description("@#Optional")]
    Optional = 1,

    [Description("@#NotAllowed")]
    NotAllowed = 2
}

/// <summary>
/// MediaSessionAction
/// </summary>
[Description("@#MediaSessionAction")]
[ECMAScript]
[String]
public enum MediaSessionAction
{
    [Description("@#Play")]
    Play = 0,

    [Description("@#Pause")]
    Pause = 1,

    [Description("@#Seekbackward")]
    Seekbackward = 2,

    [Description("@#Seekforward")]
    Seekforward = 3,

    [Description("@#Previoustrack")]
    Previoustrack = 4,

    [Description("@#Nexttrack")]
    Nexttrack = 5,

    [Description("@#Skipad")]
    Skipad = 6,

    [Description("@#Stop")]
    Stop = 7,

    [Description("@#Seekto")]
    Seekto = 8,

    [Description("@#Togglemicrophone")]
    Togglemicrophone = 9,

    [Description("@#Togglecamera")]
    Togglecamera = 10,

    [Description("@#Hangup")]
    Hangup = 11,

    [Description("@#Previousslide")]
    Previousslide = 12,

    [Description("@#Nextslide")]
    Nextslide = 13,

    [Description("@#Enterpictureinpicture")]
    Enterpictureinpicture = 14
}

/// <summary>
/// MediaSessionPlaybackState
/// </summary>
[Description("@#MediaSessionPlaybackState")]
[ECMAScript]
[String]
public enum MediaSessionPlaybackState
{
    [Description("@#None")]
    None = 0,

    [Description("@#Paused")]
    Paused = 1,

    [Description("@#Playing")]
    Playing = 2
}

/// <summary>
/// MediaStreamTrackState
/// </summary>
[Description("@#MediaStreamTrackState")]
[ECMAScript]
[String]
public enum MediaStreamTrackState
{
    [Description("@#Live")]
    Live = 0,

    [Description("@#Ended")]
    Ended = 1
}

/// <summary>
/// MeteringMode
/// </summary>
[Description("@#MeteringMode")]
[ECMAScript]
[String]
public enum MeteringMode
{
    [Description("@#None")]
    None = 0,

    [Description("@#Manual")]
    Manual = 1,

    [Description("@#SingleShot")]
    SingleShot = 2,

    [Description("@#Continuous")]
    Continuous = 3
}

/// <summary>
/// MockCapturePromptResult
/// </summary>
[Description("@#MockCapturePromptResult")]
[ECMAScript]
[String]
public enum MockCapturePromptResult
{
    [Description("@#Granted")]
    Granted = 0,

    [Description("@#Denied")]
    Denied = 1
}

/// <summary>
/// MonitorTypeSurfacesEnum
/// </summary>
[Description("@#MonitorTypeSurfacesEnum")]
[ECMAScript]
[String]
public enum MonitorTypeSurfacesEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// NavigationFocusReset
/// </summary>
[Description("@#NavigationFocusReset")]
[ECMAScript]
[String]
public enum NavigationFocusReset
{
    [Description("@#AfterTransition")]
    AfterTransition = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// NavigationHistoryBehavior
/// </summary>
[Description("@#NavigationHistoryBehavior")]
[ECMAScript]
[String]
public enum NavigationHistoryBehavior
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Push")]
    Push = 1,

    [Description("@#Replace")]
    Replace = 2
}

/// <summary>
/// NavigationScrollBehavior
/// </summary>
[Description("@#NavigationScrollBehavior")]
[ECMAScript]
[String]
public enum NavigationScrollBehavior
{
    [Description("@#AfterTransition")]
    AfterTransition = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// NavigationTimingType
/// </summary>
[Description("@#NavigationTimingType")]
[ECMAScript]
[String]
public enum NavigationTimingType
{
    [Description("@#Navigate")]
    Navigate = 0,

    [Description("@#Reload")]
    Reload = 1,

    [Description("@#BackForward")]
    BackForward = 2,

    [Description("@#Prerender")]
    Prerender = 3
}

/// <summary>
/// NavigationType
/// </summary>
[Description("@#NavigationType")]
[ECMAScript]
[String]
public enum NavigationType
{
    [Description("@#Push")]
    Push = 0,

    [Description("@#Replace")]
    Replace = 1,

    [Description("@#Reload")]
    Reload = 2,

    [Description("@#Traverse")]
    Traverse = 3
}

/// <summary>
/// NotificationDirection
/// </summary>
[Description("@#NotificationDirection")]
[ECMAScript]
[String]
public enum NotificationDirection
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Ltr")]
    Ltr = 1,

    [Description("@#Rtl")]
    Rtl = 2
}

/// <summary>
/// NotificationPermission
/// </summary>
[Description("@#NotificationPermission")]
[ECMAScript]
[String]
public enum NotificationPermission
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Denied")]
    Denied = 1,

    [Description("@#Granted")]
    Granted = 2
}

/// <summary>
/// OTPCredentialTransportType
/// </summary>
[Description("@#OTPCredentialTransportType")]
[ECMAScript]
[String]
public enum OTPCredentialTransportType
{
    [Description("@#Sms")]
    Sms = 0
}

/// <summary>
/// OffscreenRenderingContextId
/// </summary>
[Description("@#OffscreenRenderingContextId")]
[ECMAScript]
[String]
public enum OffscreenRenderingContextId
{
    [Description("@#_2d")]
    _2d = 0,

    [Description("@#Bitmaprenderer")]
    Bitmaprenderer = 1,

    [Description("@#Webgl")]
    Webgl = 2,

    [Description("@#Webgl2")]
    Webgl2 = 3,

    [Description("@#Webgpu")]
    Webgpu = 4
}

/// <summary>
/// OpaqueProperty
/// </summary>
[Description("@#OpaqueProperty")]
[ECMAScript]
[String]
public enum OpaqueProperty
{
    [Description("@#Opaque")]
    Opaque = 0
}

/// <summary>
/// OperationType
/// </summary>
[Description("@#OperationType")]
[ECMAScript]
[String]
public enum OperationType
{
    [Description("@#TokenRequest")]
    TokenRequest = 0,

    [Description("@#SendRedemptionRecord")]
    SendRedemptionRecord = 1,

    [Description("@#TokenRedemption")]
    TokenRedemption = 2
}

/// <summary>
/// OpusApplication
/// </summary>
[Description("@#OpusApplication")]
[ECMAScript]
[String]
public enum OpusApplication
{
    [Description("@#Voip")]
    Voip = 0,

    [Description("@#Audio")]
    Audio = 1,

    [Description("@#Lowdelay")]
    Lowdelay = 2
}

/// <summary>
/// OpusBitstreamFormat
/// </summary>
[Description("@#OpusBitstreamFormat")]
[ECMAScript]
[String]
public enum OpusBitstreamFormat
{
    [Description("@#Opus")]
    Opus = 0,

    [Description("@#Ogg")]
    Ogg = 1
}

/// <summary>
/// OpusSignal
/// </summary>
[Description("@#OpusSignal")]
[ECMAScript]
[String]
public enum OpusSignal
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Music")]
    Music = 1,

    [Description("@#Voice")]
    Voice = 2
}

/// <summary>
/// OrientationLockType
/// </summary>
[Description("@#OrientationLockType")]
[ECMAScript]
[String]
public enum OrientationLockType
{
    [Description("@#Any")]
    Any = 0,

    [Description("@#Natural")]
    Natural = 1,

    [Description("@#Landscape")]
    Landscape = 2,

    [Description("@#Portrait")]
    Portrait = 3,

    [Description("@#PortraitPrimary")]
    PortraitPrimary = 4,

    [Description("@#PortraitSecondary")]
    PortraitSecondary = 5,

    [Description("@#LandscapePrimary")]
    LandscapePrimary = 6,

    [Description("@#LandscapeSecondary")]
    LandscapeSecondary = 7
}

/// <summary>
/// OrientationSensorLocalCoordinateSystem
/// </summary>
[Description("@#OrientationSensorLocalCoordinateSystem")]
[ECMAScript]
[String]
public enum OrientationSensorLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// OrientationType
/// </summary>
[Description("@#OrientationType")]
[ECMAScript]
[String]
public enum OrientationType
{
    [Description("@#PortraitPrimary")]
    PortraitPrimary = 0,

    [Description("@#PortraitSecondary")]
    PortraitSecondary = 1,

    [Description("@#LandscapePrimary")]
    LandscapePrimary = 2,

    [Description("@#LandscapeSecondary")]
    LandscapeSecondary = 3
}

/// <summary>
/// OscillatorType
/// </summary>
[Description("@#OscillatorType")]
[ECMAScript]
[String]
public enum OscillatorType
{
    [Description("@#Sine")]
    Sine = 0,

    [Description("@#Square")]
    Square = 1,

    [Description("@#Sawtooth")]
    Sawtooth = 2,

    [Description("@#Triangle")]
    Triangle = 3,

    [Description("@#Custom")]
    Custom = 4
}

/// <summary>
/// OverSampleType
/// </summary>
[Description("@#OverSampleType")]
[ECMAScript]
[String]
public enum OverSampleType
{
    [Description("@#None")]
    None = 0,

    [Description("@#_2x")]
    _2x = 1,

    [Description("@#_4x")]
    _4x = 2
}

/// <summary>
/// PanningModelType
/// </summary>
[Description("@#PanningModelType")]
[ECMAScript]
[String]
public enum PanningModelType
{
    [Description("@#Equalpower")]
    Equalpower = 0,

    [Description("@#HRTF")]
    HRTF = 1
}

/// <summary>
/// ParityType
/// </summary>
[Description("@#ParityType")]
[ECMAScript]
[String]
public enum ParityType
{
    [Description("@#None")]
    None = 0,

    [Description("@#Even")]
    Even = 1,

    [Description("@#Odd")]
    Odd = 2
}

/// <summary>
/// PaymentComplete
/// </summary>
[Description("@#PaymentComplete")]
[ECMAScript]
[String]
public enum PaymentComplete
{
    [Description("@#Fail")]
    Fail = 0,

    [Description("@#Success")]
    Success = 1,

    [Description("@#Unknown")]
    Unknown = 2
}

/// <summary>
/// PaymentDelegation
/// </summary>
[Description("@#PaymentDelegation")]
[ECMAScript]
[String]
public enum PaymentDelegation
{
    [Description("@#ShippingAddress")]
    ShippingAddress = 0,

    [Description("@#PayerName")]
    PayerName = 1,

    [Description("@#PayerPhone")]
    PayerPhone = 2,

    [Description("@#PayerEmail")]
    PayerEmail = 3
}

/// <summary>
/// PaymentShippingType
/// </summary>
[Description("@#PaymentShippingType")]
[ECMAScript]
[String]
public enum PaymentShippingType
{
    [Description("@#Shipping")]
    Shipping = 0,

    [Description("@#Delivery")]
    Delivery = 1,

    [Description("@#Pickup")]
    Pickup = 2
}

/// <summary>
/// PermissionState
/// </summary>
[Description("@#PermissionState")]
[ECMAScript]
[String]
public enum PermissionState
{
    [Description("@#Granted")]
    Granted = 0,

    [Description("@#Denied")]
    Denied = 1,

    [Description("@#Prompt")]
    Prompt = 2
}

/// <summary>
/// PlaybackDirection
/// </summary>
[Description("@#PlaybackDirection")]
[ECMAScript]
[String]
public enum PlaybackDirection
{
    [Description("@#Normal")]
    Normal = 0,

    [Description("@#Reverse")]
    Reverse = 1,

    [Description("@#Alternate")]
    Alternate = 2,

    [Description("@#AlternateReverse")]
    AlternateReverse = 3
}

/// <summary>
/// PositionAlignSetting
/// </summary>
[Description("@#PositionAlignSetting")]
[ECMAScript]
[String]
public enum PositionAlignSetting
{
    [Description("@#LineLeft")]
    LineLeft = 0,

    [Description("@#Center")]
    Center = 1,

    [Description("@#LineRight")]
    LineRight = 2,

    [Description("@#Auto")]
    Auto = 3
}

/// <summary>
/// PredefinedColorSpace
/// </summary>
[Description("@#PredefinedColorSpace")]
[ECMAScript]
[String]
public enum PredefinedColorSpace
{
    [Description("@#Srgb")]
    Srgb = 0,

    [Description("@#DisplayP3")]
    DisplayP3 = 1
}

/// <summary>
/// PremultiplyAlpha
/// </summary>
[Description("@#PremultiplyAlpha")]
[ECMAScript]
[String]
public enum PremultiplyAlpha
{
    [Description("@#None")]
    None = 0,

    [Description("@#Premultiply")]
    Premultiply = 1,

    [Description("@#Default")]
    Default = 2
}

/// <summary>
/// PresentationConnectionCloseReason
/// </summary>
[Description("@#PresentationConnectionCloseReason")]
[ECMAScript]
[String]
public enum PresentationConnectionCloseReason
{
    [Description("@#Error")]
    Error = 0,

    [Description("@#Closed")]
    Closed = 1,

    [Description("@#Wentaway")]
    Wentaway = 2
}

/// <summary>
/// PresentationConnectionState
/// </summary>
[Description("@#PresentationConnectionState")]
[ECMAScript]
[String]
public enum PresentationConnectionState
{
    [Description("@#Connecting")]
    Connecting = 0,

    [Description("@#Connected")]
    Connected = 1,

    [Description("@#Closed")]
    Closed = 2,

    [Description("@#Terminated")]
    Terminated = 3
}

/// <summary>
/// PresentationStyle
/// </summary>
[Description("@#PresentationStyle")]
[ECMAScript]
[String]
public enum PresentationStyle
{
    [Description("@#Unspecified")]
    Unspecified = 0,

    [Description("@#Inline")]
    Inline = 1,

    [Description("@#Attachment")]
    Attachment = 2
}

/// <summary>
/// PressureSource
/// </summary>
[Description("@#PressureSource")]
[ECMAScript]
[String]
public enum PressureSource
{
    [Description("@#Thermals")]
    Thermals = 0,

    [Description("@#Cpu")]
    Cpu = 1
}

/// <summary>
/// PressureState
/// </summary>
[Description("@#PressureState")]
[ECMAScript]
[String]
public enum PressureState
{
    [Description("@#Nominal")]
    Nominal = 0,

    [Description("@#Fair")]
    Fair = 1,

    [Description("@#Serious")]
    Serious = 2,

    [Description("@#Critical")]
    Critical = 3
}

/// <summary>
/// PublicKeyCredentialHints
/// </summary>
[Description("@#PublicKeyCredentialHints")]
[ECMAScript]
[String]
public enum PublicKeyCredentialHints
{
    [Description("@#SecurityKey")]
    SecurityKey = 0,

    [Description("@#ClientDevice")]
    ClientDevice = 1,

    [Description("@#Hybrid")]
    Hybrid = 2
}

/// <summary>
/// PublicKeyCredentialType
/// </summary>
[Description("@#PublicKeyCredentialType")]
[ECMAScript]
[String]
public enum PublicKeyCredentialType
{
    [Description("@#PublicKey")]
    PublicKey = 0
}

/// <summary>
/// PushEncryptionKeyName
/// </summary>
[Description("@#PushEncryptionKeyName")]
[ECMAScript]
[String]
public enum PushEncryptionKeyName
{
    [Description("@#P256dh")]
    P256dh = 0,

    [Description("@#Auth")]
    Auth = 1
}

/// <summary>
/// RTCBundlePolicy
/// </summary>
[Description("@#RTCBundlePolicy")]
[ECMAScript]
[String]
public enum RTCBundlePolicy
{
    [Description("@#Balanced")]
    Balanced = 0,

    [Description("@#MaxCompat")]
    MaxCompat = 1,

    [Description("@#MaxBundle")]
    MaxBundle = 2
}

/// <summary>
/// RTCDataChannelState
/// </summary>
[Description("@#RTCDataChannelState")]
[ECMAScript]
[String]
public enum RTCDataChannelState
{
    [Description("@#Connecting")]
    Connecting = 0,

    [Description("@#Open")]
    Open = 1,

    [Description("@#Closing")]
    Closing = 2,

    [Description("@#Closed")]
    Closed = 3
}

/// <summary>
/// RTCDegradationPreference
/// </summary>
[Description("@#RTCDegradationPreference")]
[ECMAScript]
[String]
public enum RTCDegradationPreference
{
    [Description("@#MaintainFramerate")]
    MaintainFramerate = 0,

    [Description("@#MaintainResolution")]
    MaintainResolution = 1,

    [Description("@#Balanced")]
    Balanced = 2
}

/// <summary>
/// RTCDtlsRole
/// </summary>
[Description("@#RTCDtlsRole")]
[ECMAScript]
[String]
public enum RTCDtlsRole
{
    [Description("@#Client")]
    Client = 0,

    [Description("@#Server")]
    Server = 1,

    [Description("@#Unknown")]
    Unknown = 2
}

/// <summary>
/// RTCDtlsTransportState
/// </summary>
[Description("@#RTCDtlsTransportState")]
[ECMAScript]
[String]
public enum RTCDtlsTransportState
{
    [Description("@#New")]
    New = 0,

    [Description("@#Connecting")]
    Connecting = 1,

    [Description("@#Connected")]
    Connected = 2,

    [Description("@#Closed")]
    Closed = 3,

    [Description("@#Failed")]
    Failed = 4
}

/// <summary>
/// RTCEncodedVideoFrameType
/// </summary>
[Description("@#RTCEncodedVideoFrameType")]
[ECMAScript]
[String]
public enum RTCEncodedVideoFrameType
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Key")]
    Key = 1,

    [Description("@#Delta")]
    Delta = 2
}

/// <summary>
/// RTCErrorDetailType
/// </summary>
[Description("@#RTCErrorDetailType")]
[ECMAScript]
[String]
public enum RTCErrorDetailType
{
    [Description("@#DataChannelFailure")]
    DataChannelFailure = 0,

    [Description("@#DtlsFailure")]
    DtlsFailure = 1,

    [Description("@#FingerprintFailure")]
    FingerprintFailure = 2,

    [Description("@#SctpFailure")]
    SctpFailure = 3,

    [Description("@#SdpSyntaxError")]
    SdpSyntaxError = 4,

    [Description("@#HardwareEncoderNotAvailable")]
    HardwareEncoderNotAvailable = 5,

    [Description("@#HardwareEncoderError")]
    HardwareEncoderError = 6
}

/// <summary>
/// RTCErrorDetailTypeIdp
/// </summary>
[Description("@#RTCErrorDetailTypeIdp")]
[ECMAScript]
[String]
public enum RTCErrorDetailTypeIdp
{
    [Description("@#IdpBadScriptFailure")]
    IdpBadScriptFailure = 0,

    [Description("@#IdpExecutionFailure")]
    IdpExecutionFailure = 1,

    [Description("@#IdpLoadFailure")]
    IdpLoadFailure = 2,

    [Description("@#IdpNeedLogin")]
    IdpNeedLogin = 3,

    [Description("@#IdpTimeout")]
    IdpTimeout = 4,

    [Description("@#IdpTlsFailure")]
    IdpTlsFailure = 5,

    [Description("@#IdpTokenExpired")]
    IdpTokenExpired = 6,

    [Description("@#IdpTokenInvalid")]
    IdpTokenInvalid = 7
}

/// <summary>
/// RTCIceCandidateType
/// </summary>
[Description("@#RTCIceCandidateType")]
[ECMAScript]
[String]
public enum RTCIceCandidateType
{
    [Description("@#Host")]
    Host = 0,

    [Description("@#Srflx")]
    Srflx = 1,

    [Description("@#Prflx")]
    Prflx = 2,

    [Description("@#Relay")]
    Relay = 3
}

/// <summary>
/// RTCIceComponent
/// </summary>
[Description("@#RTCIceComponent")]
[ECMAScript]
[String]
public enum RTCIceComponent
{
    [Description("@#Rtp")]
    Rtp = 0,

    [Description("@#Rtcp")]
    Rtcp = 1
}

/// <summary>
/// RTCIceConnectionState
/// </summary>
[Description("@#RTCIceConnectionState")]
[ECMAScript]
[String]
public enum RTCIceConnectionState
{
    [Description("@#Closed")]
    Closed = 0,

    [Description("@#Failed")]
    Failed = 1,

    [Description("@#Disconnected")]
    Disconnected = 2,

    [Description("@#New")]
    New = 3,

    [Description("@#Checking")]
    Checking = 4,

    [Description("@#Completed")]
    Completed = 5,

    [Description("@#Connected")]
    Connected = 6
}

/// <summary>
/// RTCIceGathererState
/// </summary>
[Description("@#RTCIceGathererState")]
[ECMAScript]
[String]
public enum RTCIceGathererState
{
    [Description("@#New")]
    New = 0,

    [Description("@#Gathering")]
    Gathering = 1,

    [Description("@#Complete")]
    Complete = 2
}

/// <summary>
/// RTCIceGatheringState
/// </summary>
[Description("@#RTCIceGatheringState")]
[ECMAScript]
[String]
public enum RTCIceGatheringState
{
    [Description("@#New")]
    New = 0,

    [Description("@#Gathering")]
    Gathering = 1,

    [Description("@#Complete")]
    Complete = 2
}

/// <summary>
/// RTCIceProtocol
/// </summary>
[Description("@#RTCIceProtocol")]
[ECMAScript]
[String]
public enum RTCIceProtocol
{
    [Description("@#Udp")]
    Udp = 0,

    [Description("@#Tcp")]
    Tcp = 1
}

/// <summary>
/// RTCIceRole
/// </summary>
[Description("@#RTCIceRole")]
[ECMAScript]
[String]
public enum RTCIceRole
{
    [Description("@#Unknown")]
    Unknown = 0,

    [Description("@#Controlling")]
    Controlling = 1,

    [Description("@#Controlled")]
    Controlled = 2
}

/// <summary>
/// RTCIceServerTransportProtocol
/// </summary>
[Description("@#RTCIceServerTransportProtocol")]
[ECMAScript]
[String]
public enum RTCIceServerTransportProtocol
{
    [Description("@#Udp")]
    Udp = 0,

    [Description("@#Tcp")]
    Tcp = 1,

    [Description("@#Tls")]
    Tls = 2
}

/// <summary>
/// RTCIceTcpCandidateType
/// </summary>
[Description("@#RTCIceTcpCandidateType")]
[ECMAScript]
[String]
public enum RTCIceTcpCandidateType
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Passive")]
    Passive = 1,

    [Description("@#So")]
    So = 2
}

/// <summary>
/// RTCIceTransportPolicy
/// </summary>
[Description("@#RTCIceTransportPolicy")]
[ECMAScript]
[String]
public enum RTCIceTransportPolicy
{
    [Description("@#Relay")]
    Relay = 0,

    [Description("@#All")]
    All = 1
}

/// <summary>
/// RTCIceTransportState
/// </summary>
[Description("@#RTCIceTransportState")]
[ECMAScript]
[String]
public enum RTCIceTransportState
{
    [Description("@#Closed")]
    Closed = 0,

    [Description("@#Failed")]
    Failed = 1,

    [Description("@#Disconnected")]
    Disconnected = 2,

    [Description("@#New")]
    New = 3,

    [Description("@#Checking")]
    Checking = 4,

    [Description("@#Completed")]
    Completed = 5,

    [Description("@#Connected")]
    Connected = 6
}

/// <summary>
/// RTCPeerConnectionState
/// </summary>
[Description("@#RTCPeerConnectionState")]
[ECMAScript]
[String]
public enum RTCPeerConnectionState
{
    [Description("@#Closed")]
    Closed = 0,

    [Description("@#Failed")]
    Failed = 1,

    [Description("@#Disconnected")]
    Disconnected = 2,

    [Description("@#New")]
    New = 3,

    [Description("@#Connecting")]
    Connecting = 4,

    [Description("@#Connected")]
    Connected = 5
}

/// <summary>
/// RTCPriorityType
/// </summary>
[Description("@#RTCPriorityType")]
[ECMAScript]
[String]
public enum RTCPriorityType
{
    [Description("@#VeryLow")]
    VeryLow = 0,

    [Description("@#Low")]
    Low = 1,

    [Description("@#Medium")]
    Medium = 2,

    [Description("@#High")]
    High = 3
}

/// <summary>
/// RTCQualityLimitationReason
/// </summary>
[Description("@#RTCQualityLimitationReason")]
[ECMAScript]
[String]
public enum RTCQualityLimitationReason
{
    [Description("@#None")]
    None = 0,

    [Description("@#Cpu")]
    Cpu = 1,

    [Description("@#Bandwidth")]
    Bandwidth = 2,

    [Description("@#Other")]
    Other = 3
}

/// <summary>
/// RTCRtcpMuxPolicy
/// </summary>
[Description("@#RTCRtcpMuxPolicy")]
[ECMAScript]
[String]
public enum RTCRtcpMuxPolicy
{
    [Description("@#Require")]
    Require = 0
}

/// <summary>
/// RTCRtpTransceiverDirection
/// </summary>
[Description("@#RTCRtpTransceiverDirection")]
[ECMAScript]
[String]
public enum RTCRtpTransceiverDirection
{
    [Description("@#Sendrecv")]
    Sendrecv = 0,

    [Description("@#Sendonly")]
    Sendonly = 1,

    [Description("@#Recvonly")]
    Recvonly = 2,

    [Description("@#Inactive")]
    Inactive = 3,

    [Description("@#Stopped")]
    Stopped = 4
}

/// <summary>
/// RTCSctpTransportState
/// </summary>
[Description("@#RTCSctpTransportState")]
[ECMAScript]
[String]
public enum RTCSctpTransportState
{
    [Description("@#Connecting")]
    Connecting = 0,

    [Description("@#Connected")]
    Connected = 1,

    [Description("@#Closed")]
    Closed = 2
}

/// <summary>
/// RTCSdpType
/// </summary>
[Description("@#RTCSdpType")]
[ECMAScript]
[String]
public enum RTCSdpType
{
    [Description("@#Offer")]
    Offer = 0,

    [Description("@#Pranswer")]
    Pranswer = 1,

    [Description("@#Answer")]
    Answer = 2,

    [Description("@#Rollback")]
    Rollback = 3
}

/// <summary>
/// RTCSignalingState
/// </summary>
[Description("@#RTCSignalingState")]
[ECMAScript]
[String]
public enum RTCSignalingState
{
    [Description("@#Stable")]
    Stable = 0,

    [Description("@#HaveLocalOffer")]
    HaveLocalOffer = 1,

    [Description("@#HaveRemoteOffer")]
    HaveRemoteOffer = 2,

    [Description("@#HaveLocalPranswer")]
    HaveLocalPranswer = 3,

    [Description("@#HaveRemotePranswer")]
    HaveRemotePranswer = 4,

    [Description("@#Closed")]
    Closed = 5
}

/// <summary>
/// RTCStatsIceCandidatePairState
/// </summary>
[Description("@#RTCStatsIceCandidatePairState")]
[ECMAScript]
[String]
public enum RTCStatsIceCandidatePairState
{
    [Description("@#Frozen")]
    Frozen = 0,

    [Description("@#Waiting")]
    Waiting = 1,

    [Description("@#InProgress")]
    InProgress = 2,

    [Description("@#Failed")]
    Failed = 3,

    [Description("@#Succeeded")]
    Succeeded = 4
}

/// <summary>
/// RTCStatsType
/// </summary>
[Description("@#RTCStatsType")]
[ECMAScript]
[String]
public enum RTCStatsType
{
    [Description("@#Codec")]
    Codec = 0,

    [Description("@#InboundRtp")]
    InboundRtp = 1,

    [Description("@#OutboundRtp")]
    OutboundRtp = 2,

    [Description("@#RemoteInboundRtp")]
    RemoteInboundRtp = 3,

    [Description("@#RemoteOutboundRtp")]
    RemoteOutboundRtp = 4,

    [Description("@#MediaSource")]
    MediaSource = 5,

    [Description("@#MediaPlayout")]
    MediaPlayout = 6,

    [Description("@#PeerConnection")]
    PeerConnection = 7,

    [Description("@#DataChannel")]
    DataChannel = 8,

    [Description("@#Transport")]
    Transport = 9,

    [Description("@#CandidatePair")]
    CandidatePair = 10,

    [Description("@#LocalCandidate")]
    LocalCandidate = 11,

    [Description("@#RemoteCandidate")]
    RemoteCandidate = 12,

    [Description("@#Certificate")]
    Certificate = 13
}

/// <summary>
/// ReadableStreamReaderMode
/// </summary>
[Description("@#ReadableStreamReaderMode")]
[ECMAScript]
[String]
public enum ReadableStreamReaderMode
{
    [Description("@#Byob")]
    Byob = 0
}

/// <summary>
/// ReadableStreamType
/// </summary>
[Description("@#ReadableStreamType")]
[ECMAScript]
[String]
public enum ReadableStreamType
{
    [Description("@#Bytes")]
    Bytes = 0
}

/// <summary>
/// ReadyState
/// </summary>
[Description("@#ReadyState")]
[ECMAScript]
[String]
public enum ReadyState
{
    [Description("@#Closed")]
    Closed = 0,

    [Description("@#Open")]
    Open = 1,

    [Description("@#Ended")]
    Ended = 2
}

/// <summary>
/// RecordingState
/// </summary>
[Description("@#RecordingState")]
[ECMAScript]
[String]
public enum RecordingState
{
    [Description("@#Inactive")]
    Inactive = 0,

    [Description("@#Recording")]
    Recording = 1,

    [Description("@#Paused")]
    Paused = 2
}

/// <summary>
/// RedEyeReduction
/// </summary>
[Description("@#RedEyeReduction")]
[ECMAScript]
[String]
public enum RedEyeReduction
{
    [Description("@#Never")]
    Never = 0,

    [Description("@#Always")]
    Always = 1,

    [Description("@#Controllable")]
    Controllable = 2
}

/// <summary>
/// ReferrerPolicy
/// </summary>
[Description("@#ReferrerPolicy")]
[ECMAScript]
[String]
public enum ReferrerPolicy
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#NoReferrer")]
    NoReferrer = 1,

    [Description("@#NoReferrerWhenDowngrade")]
    NoReferrerWhenDowngrade = 2,

    [Description("@#SameOrigin")]
    SameOrigin = 3,

    [Description("@#Origin")]
    Origin = 4,

    [Description("@#StrictOrigin")]
    StrictOrigin = 5,

    [Description("@#OriginWhenCrossOrigin")]
    OriginWhenCrossOrigin = 6,

    [Description("@#StrictOriginWhenCrossOrigin")]
    StrictOriginWhenCrossOrigin = 7,

    [Description("@#UnsafeUrl")]
    UnsafeUrl = 8
}

/// <summary>
/// RefreshPolicy
/// </summary>
[Description("@#RefreshPolicy")]
[ECMAScript]
[String]
public enum RefreshPolicy
{
    [Description("@#None")]
    None = 0,

    [Description("@#Refresh")]
    Refresh = 1
}

/// <summary>
/// RemotePlaybackState
/// </summary>
[Description("@#RemotePlaybackState")]
[ECMAScript]
[String]
public enum RemotePlaybackState
{
    [Description("@#Connecting")]
    Connecting = 0,

    [Description("@#Connected")]
    Connected = 1,

    [Description("@#Disconnected")]
    Disconnected = 2
}

/// <summary>
/// RenderBlockingStatusType
/// </summary>
[Description("@#RenderBlockingStatusType")]
[ECMAScript]
[String]
public enum RenderBlockingStatusType
{
    [Description("@#Blocking")]
    Blocking = 0,

    [Description("@#NonBlocking")]
    NonBlocking = 1
}

/// <summary>
/// RequestCache
/// </summary>
[Description("@#RequestCache")]
[ECMAScript]
[String]
public enum RequestCache
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#NoStore")]
    NoStore = 1,

    [Description("@#Reload")]
    Reload = 2,

    [Description("@#NoCache")]
    NoCache = 3,

    [Description("@#ForceCache")]
    ForceCache = 4,

    [Description("@#OnlyIfCached")]
    OnlyIfCached = 5
}

/// <summary>
/// RequestCredentials
/// </summary>
[Description("@#RequestCredentials")]
[ECMAScript]
[String]
public enum RequestCredentials
{
    [Description("@#Omit")]
    Omit = 0,

    [Description("@#SameOrigin")]
    SameOrigin = 1,

    [Description("@#Include")]
    Include = 2
}

/// <summary>
/// RequestDestination
/// </summary>
[Description("@#RequestDestination")]
[ECMAScript]
[String]
public enum RequestDestination
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Audio")]
    Audio = 1,

    [Description("@#Audioworklet")]
    Audioworklet = 2,

    [Description("@#Document")]
    Document = 3,

    [Description("@#Embed")]
    Embed = 4,

    [Description("@#Font")]
    Font = 5,

    [Description("@#Frame")]
    Frame = 6,

    [Description("@#Iframe")]
    Iframe = 7,

    [Description("@#Image")]
    Image = 8,

    [Description("@#Json")]
    Json = 9,

    [Description("@#Manifest")]
    Manifest = 10,

    [Description("@#Object")]
    Object = 11,

    [Description("@#Paintworklet")]
    Paintworklet = 12,

    [Description("@#Report")]
    Report = 13,

    [Description("@#Script")]
    Script = 14,

    [Description("@#Sharedworker")]
    Sharedworker = 15,

    [Description("@#Style")]
    Style = 16,

    [Description("@#Track")]
    Track = 17,

    [Description("@#Video")]
    Video = 18,

    [Description("@#Worker")]
    Worker = 19,

    [Description("@#Xslt")]
    Xslt = 20
}

/// <summary>
/// RequestDuplex
/// </summary>
[Description("@#RequestDuplex")]
[ECMAScript]
[String]
public enum RequestDuplex
{
    [Description("@#Half")]
    Half = 0
}

/// <summary>
/// RequestMode
/// </summary>
[Description("@#RequestMode")]
[ECMAScript]
[String]
public enum RequestMode
{
    [Description("@#Navigate")]
    Navigate = 0,

    [Description("@#SameOrigin")]
    SameOrigin = 1,

    [Description("@#NoCors")]
    NoCors = 2,

    [Description("@#Cors")]
    Cors = 3
}

/// <summary>
/// RequestPriority
/// </summary>
[Description("@#RequestPriority")]
[ECMAScript]
[String]
public enum RequestPriority
{
    [Description("@#High")]
    High = 0,

    [Description("@#Low")]
    Low = 1,

    [Description("@#Auto")]
    Auto = 2
}

/// <summary>
/// RequestRedirect
/// </summary>
[Description("@#RequestRedirect")]
[ECMAScript]
[String]
public enum RequestRedirect
{
    [Description("@#Follow")]
    Follow = 0,

    [Description("@#Error")]
    Error = 1,

    [Description("@#Manual")]
    Manual = 2
}

/// <summary>
/// ResidentKeyRequirement
/// </summary>
[Description("@#ResidentKeyRequirement")]
[ECMAScript]
[String]
public enum ResidentKeyRequirement
{
    [Description("@#Discouraged")]
    Discouraged = 0,

    [Description("@#Preferred")]
    Preferred = 1,

    [Description("@#Required")]
    Required = 2
}

/// <summary>
/// ResizeObserverBoxOptions
/// </summary>
[Description("@#ResizeObserverBoxOptions")]
[ECMAScript]
[String]
public enum ResizeObserverBoxOptions
{
    [Description("@#BorderBox")]
    BorderBox = 0,

    [Description("@#ContentBox")]
    ContentBox = 1,

    [Description("@#DevicePixelContentBox")]
    DevicePixelContentBox = 2
}

/// <summary>
/// ResizeQuality
/// </summary>
[Description("@#ResizeQuality")]
[ECMAScript]
[String]
public enum ResizeQuality
{
    [Description("@#Pixelated")]
    Pixelated = 0,

    [Description("@#Low")]
    Low = 1,

    [Description("@#Medium")]
    Medium = 2,

    [Description("@#High")]
    High = 3
}

/// <summary>
/// ResponseType
/// </summary>
[Description("@#ResponseType")]
[ECMAScript]
[String]
public enum ResponseType
{
    [Description("@#Basic")]
    Basic = 0,

    [Description("@#Cors")]
    Cors = 1,

    [Description("@#Default")]
    Default = 2,

    [Description("@#Error")]
    Error = 3,

    [Description("@#Opaque")]
    Opaque = 4,

    [Description("@#Opaqueredirect")]
    Opaqueredirect = 5
}

/// <summary>
/// RouterSourceEnum
/// </summary>
[Description("@#RouterSourceEnum")]
[ECMAScript]
[String]
public enum RouterSourceEnum
{
    [Description("@#Cache")]
    Cache = 0,

    [Description("@#FetchEvent")]
    FetchEvent = 1,

    [Description("@#Network")]
    Network = 2,

    [Description("@#RaceNetworkAndFetchHandler")]
    RaceNetworkAndFetchHandler = 3
}

/// <summary>
/// RunningStatus
/// </summary>
[Description("@#RunningStatus")]
[ECMAScript]
[String]
public enum RunningStatus
{
    [Description("@#Running")]
    Running = 0,

    [Description("@#NotRunning")]
    NotRunning = 1
}

/// <summary>
/// SFrameTransformErrorEventType
/// </summary>
[Description("@#SFrameTransformErrorEventType")]
[ECMAScript]
[String]
public enum SFrameTransformErrorEventType
{
    [Description("@#Authentication")]
    Authentication = 0,

    [Description("@#KeyID")]
    KeyID = 1,

    [Description("@#Syntax")]
    Syntax = 2
}

/// <summary>
/// SFrameTransformRole
/// </summary>
[Description("@#SFrameTransformRole")]
[ECMAScript]
[String]
public enum SFrameTransformRole
{
    [Description("@#Encrypt")]
    Encrypt = 0,

    [Description("@#Decrypt")]
    Decrypt = 1
}

/// <summary>
/// ScreenIdleState
/// </summary>
[Description("@#ScreenIdleState")]
[ECMAScript]
[String]
public enum ScreenIdleState
{
    [Description("@#Locked")]
    Locked = 0,

    [Description("@#Unlocked")]
    Unlocked = 1
}

/// <summary>
/// ScriptInvokerType
/// </summary>
[Description("@#ScriptInvokerType")]
[ECMAScript]
[String]
public enum ScriptInvokerType
{
    [Description("@#ClassicScript")]
    ClassicScript = 0,

    [Description("@#ModuleScript")]
    ModuleScript = 1,

    [Description("@#EventListener")]
    EventListener = 2,

    [Description("@#UserCallback")]
    UserCallback = 3,

    [Description("@#ResolvePromise")]
    ResolvePromise = 4,

    [Description("@#RejectPromise")]
    RejectPromise = 5
}

/// <summary>
/// ScriptWindowAttribution
/// </summary>
[Description("@#ScriptWindowAttribution")]
[ECMAScript]
[String]
public enum ScriptWindowAttribution
{
    [Description("@#Self")]
    Self = 0,

    [Description("@#Descendant")]
    Descendant = 1,

    [Description("@#Ancestor")]
    Ancestor = 2,

    [Description("@#SamePage")]
    SamePage = 3,

    [Description("@#Other")]
    Other = 4
}

/// <summary>
/// ScriptingPolicyViolationType
/// </summary>
[Description("@#ScriptingPolicyViolationType")]
[ECMAScript]
[String]
public enum ScriptingPolicyViolationType
{
    [Description("@#ExternalScript")]
    ExternalScript = 0,

    [Description("@#InlineScript")]
    InlineScript = 1,

    [Description("@#InlineEventHandler")]
    InlineEventHandler = 2,

    [Description("@#Eval")]
    Eval = 3
}

/// <summary>
/// ScrollAxis
/// </summary>
[Description("@#ScrollAxis")]
[ECMAScript]
[String]
public enum ScrollAxis
{
    [Description("@#Block")]
    Block = 0,

    [Description("@#Inline")]
    Inline = 1,

    [Description("@#X")]
    X = 2,

    [Description("@#Y")]
    Y = 3
}

/// <summary>
/// ScrollBehavior
/// </summary>
[Description("@#ScrollBehavior")]
[ECMAScript]
[String]
public enum ScrollBehavior
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Instant")]
    Instant = 1,

    [Description("@#Smooth")]
    Smooth = 2
}

/// <summary>
/// ScrollLogicalPosition
/// </summary>
[Description("@#ScrollLogicalPosition")]
[ECMAScript]
[String]
public enum ScrollLogicalPosition
{
    [Description("@#Start")]
    Start = 0,

    [Description("@#Center")]
    Center = 1,

    [Description("@#End")]
    End = 2,

    [Description("@#Nearest")]
    Nearest = 3
}

/// <summary>
/// ScrollRestoration
/// </summary>
[Description("@#ScrollRestoration")]
[ECMAScript]
[String]
public enum ScrollRestoration
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// ScrollSetting
/// </summary>
[Description("@#ScrollSetting")]
[ECMAScript]
[String]
public enum ScrollSetting
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Up")]
    Up = 1
}

/// <summary>
/// SecurityPolicyViolationEventDisposition
/// </summary>
[Description("@#SecurityPolicyViolationEventDisposition")]
[ECMAScript]
[String]
public enum SecurityPolicyViolationEventDisposition
{
    [Description("@#Enforce")]
    Enforce = 0,

    [Description("@#Report")]
    Report = 1
}

/// <summary>
/// SelectionMode
/// </summary>
[Description("@#SelectionMode")]
[ECMAScript]
[String]
public enum SelectionMode
{
    [Description("@#Select")]
    Select = 0,

    [Description("@#Start")]
    Start = 1,

    [Description("@#End")]
    End = 2,

    [Description("@#Preserve")]
    Preserve = 3
}

/// <summary>
/// SelfCapturePreferenceEnum
/// </summary>
[Description("@#SelfCapturePreferenceEnum")]
[ECMAScript]
[String]
public enum SelfCapturePreferenceEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// ServiceWorkerState
/// </summary>
[Description("@#ServiceWorkerState")]
[ECMAScript]
[String]
public enum ServiceWorkerState
{
    [Description("@#Parsed")]
    Parsed = 0,

    [Description("@#Installing")]
    Installing = 1,

    [Description("@#Installed")]
    Installed = 2,

    [Description("@#Activating")]
    Activating = 3,

    [Description("@#Activated")]
    Activated = 4,

    [Description("@#Redundant")]
    Redundant = 5
}

/// <summary>
/// ServiceWorkerUpdateViaCache
/// </summary>
[Description("@#ServiceWorkerUpdateViaCache")]
[ECMAScript]
[String]
public enum ServiceWorkerUpdateViaCache
{
    [Description("@#Imports")]
    Imports = 0,

    [Description("@#All")]
    All = 1,

    [Description("@#None")]
    None = 2
}

/// <summary>
/// ShadowRootMode
/// </summary>
[Description("@#ShadowRootMode")]
[ECMAScript]
[String]
public enum ShadowRootMode
{
    [Description("@#Open")]
    Open = 0,

    [Description("@#Closed")]
    Closed = 1
}

/// <summary>
/// SlotAssignmentMode
/// </summary>
[Description("@#SlotAssignmentMode")]
[ECMAScript]
[String]
public enum SlotAssignmentMode
{
    [Description("@#Manual")]
    Manual = 0,

    [Description("@#Named")]
    Named = 1
}

/// <summary>
/// SpatialNavigationDirection
/// </summary>
[Description("@#SpatialNavigationDirection")]
[ECMAScript]
[String]
public enum SpatialNavigationDirection
{
    [Description("@#Up")]
    Up = 0,

    [Description("@#Down")]
    Down = 1,

    [Description("@#Left")]
    Left = 2,

    [Description("@#Right")]
    Right = 3
}

/// <summary>
/// SpeechRecognitionErrorCode
/// </summary>
[Description("@#SpeechRecognitionErrorCode")]
[ECMAScript]
[String]
public enum SpeechRecognitionErrorCode
{
    [Description("@#NoSpeech")]
    NoSpeech = 0,

    [Description("@#Aborted")]
    Aborted = 1,

    [Description("@#AudioCapture")]
    AudioCapture = 2,

    [Description("@#Network")]
    Network = 3,

    [Description("@#NotAllowed")]
    NotAllowed = 4,

    [Description("@#ServiceNotAllowed")]
    ServiceNotAllowed = 5,

    [Description("@#BadGrammar")]
    BadGrammar = 6,

    [Description("@#LanguageNotSupported")]
    LanguageNotSupported = 7
}

/// <summary>
/// SpeechSynthesisErrorCode
/// </summary>
[Description("@#SpeechSynthesisErrorCode")]
[ECMAScript]
[String]
public enum SpeechSynthesisErrorCode
{
    [Description("@#Canceled")]
    Canceled = 0,

    [Description("@#Interrupted")]
    Interrupted = 1,

    [Description("@#AudioBusy")]
    AudioBusy = 2,

    [Description("@#AudioHardware")]
    AudioHardware = 3,

    [Description("@#Network")]
    Network = 4,

    [Description("@#SynthesisUnavailable")]
    SynthesisUnavailable = 5,

    [Description("@#SynthesisFailed")]
    SynthesisFailed = 6,

    [Description("@#LanguageUnavailable")]
    LanguageUnavailable = 7,

    [Description("@#VoiceUnavailable")]
    VoiceUnavailable = 8,

    [Description("@#TextTooLong")]
    TextTooLong = 9,

    [Description("@#InvalidArgument")]
    InvalidArgument = 10,

    [Description("@#NotAllowed")]
    NotAllowed = 11
}

/// <summary>
/// SurfaceSwitchingPreferenceEnum
/// </summary>
[Description("@#SurfaceSwitchingPreferenceEnum")]
[ECMAScript]
[String]
public enum SurfaceSwitchingPreferenceEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// SystemAudioPreferenceEnum
/// </summary>
[Description("@#SystemAudioPreferenceEnum")]
[ECMAScript]
[String]
public enum SystemAudioPreferenceEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// TaskPriority
/// </summary>
[Description("@#TaskPriority")]
[ECMAScript]
[String]
public enum TaskPriority
{
    [Description("@#UserBlocking")]
    UserBlocking = 0,

    [Description("@#UserVisible")]
    UserVisible = 1,

    [Description("@#Background")]
    Background = 2
}

/// <summary>
/// TextTrackKind
/// </summary>
[Description("@#TextTrackKind")]
[ECMAScript]
[String]
public enum TextTrackKind
{
    [Description("@#Subtitles")]
    Subtitles = 0,

    [Description("@#Captions")]
    Captions = 1,

    [Description("@#Descriptions")]
    Descriptions = 2,

    [Description("@#Chapters")]
    Chapters = 3,

    [Description("@#Metadata")]
    Metadata = 4
}

/// <summary>
/// TextTrackMode
/// </summary>
[Description("@#TextTrackMode")]
[ECMAScript]
[String]
public enum TextTrackMode
{
    [Description("@#Disabled")]
    Disabled = 0,

    [Description("@#Hidden")]
    Hidden = 1,

    [Description("@#Showing")]
    Showing = 2
}

/// <summary>
/// TokenBindingStatus
/// </summary>
[Description("@#TokenBindingStatus")]
[ECMAScript]
[String]
public enum TokenBindingStatus
{
    [Description("@#Present")]
    Present = 0,

    [Description("@#Supported")]
    Supported = 1
}

/// <summary>
/// TokenVersion
/// </summary>
[Description("@#TokenVersion")]
[ECMAScript]
[String]
public enum TokenVersion
{
    [Description("@#_1")]
    _1 = 0
}

/// <summary>
/// TouchType
/// </summary>
[Description("@#TouchType")]
[ECMAScript]
[String]
public enum TouchType
{
    [Description("@#Direct")]
    Direct = 0,

    [Description("@#Stylus")]
    Stylus = 1
}

/// <summary>
/// TransferFunction
/// </summary>
[Description("@#TransferFunction")]
[ECMAScript]
[String]
public enum TransferFunction
{
    [Description("@#Srgb")]
    Srgb = 0,

    [Description("@#Pq")]
    Pq = 1,

    [Description("@#Hlg")]
    Hlg = 2
}

/// <summary>
/// USBDirection
/// </summary>
[Description("@#USBDirection")]
[ECMAScript]
[String]
public enum USBDirection
{
    [Description("@#In")]
    In = 0,

    [Description("@#Out")]
    Out = 1
}

/// <summary>
/// USBEndpointType
/// </summary>
[Description("@#USBEndpointType")]
[ECMAScript]
[String]
public enum USBEndpointType
{
    [Description("@#Bulk")]
    Bulk = 0,

    [Description("@#Interrupt")]
    Interrupt = 1,

    [Description("@#Isochronous")]
    Isochronous = 2
}

/// <summary>
/// USBRecipient
/// </summary>
[Description("@#USBRecipient")]
[ECMAScript]
[String]
public enum USBRecipient
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Interface")]
    Interface = 1,

    [Description("@#Endpoint")]
    Endpoint = 2,

    [Description("@#Other")]
    Other = 3
}

/// <summary>
/// USBRequestType
/// </summary>
[Description("@#USBRequestType")]
[ECMAScript]
[String]
public enum USBRequestType
{
    [Description("@#Standard")]
    Standard = 0,

    [Description("@#Class")]
    Class = 1,

    [Description("@#Vendor")]
    Vendor = 2
}

/// <summary>
/// USBTransferStatus
/// </summary>
[Description("@#USBTransferStatus")]
[ECMAScript]
[String]
public enum USBTransferStatus
{
    [Description("@#Ok")]
    Ok = 0,

    [Description("@#Stall")]
    Stall = 1,

    [Description("@#Babble")]
    Babble = 2
}

/// <summary>
/// UnderlineStyle
/// </summary>
[Description("@#UnderlineStyle")]
[ECMAScript]
[String]
public enum UnderlineStyle
{
    [Description("@#None")]
    None = 0,

    [Description("@#Solid")]
    Solid = 1,

    [Description("@#Dotted")]
    Dotted = 2,

    [Description("@#Dashed")]
    Dashed = 3,

    [Description("@#Wavy")]
    Wavy = 4
}

/// <summary>
/// UnderlineThickness
/// </summary>
[Description("@#UnderlineThickness")]
[ECMAScript]
[String]
public enum UnderlineThickness
{
    [Description("@#None")]
    None = 0,

    [Description("@#Thin")]
    Thin = 1,

    [Description("@#Thick")]
    Thick = 2
}

/// <summary>
/// UserIdleState
/// </summary>
[Description("@#UserIdleState")]
[ECMAScript]
[String]
public enum UserIdleState
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Idle")]
    Idle = 1
}

/// <summary>
/// UserVerificationRequirement
/// </summary>
[Description("@#UserVerificationRequirement")]
[ECMAScript]
[String]
public enum UserVerificationRequirement
{
    [Description("@#Required")]
    Required = 0,

    [Description("@#Preferred")]
    Preferred = 1,

    [Description("@#Discouraged")]
    Discouraged = 2
}

/// <summary>
/// VideoColorPrimaries
/// </summary>
[Description("@#VideoColorPrimaries")]
[ECMAScript]
[String]
public enum VideoColorPrimaries
{
    [Description("@#Bt709")]
    Bt709 = 0,

    [Description("@#Bt470bg")]
    Bt470bg = 1,

    [Description("@#Smpte170m")]
    Smpte170m = 2,

    [Description("@#Bt2020")]
    Bt2020 = 3,

    [Description("@#Smpte432")]
    Smpte432 = 4
}

/// <summary>
/// VideoEncoderBitrateMode
/// </summary>
[Description("@#VideoEncoderBitrateMode")]
[ECMAScript]
[String]
public enum VideoEncoderBitrateMode
{
    [Description("@#Constant")]
    Constant = 0,

    [Description("@#Variable")]
    Variable = 1,

    [Description("@#Quantizer")]
    Quantizer = 2
}

/// <summary>
/// VideoFacingModeEnum
/// </summary>
[Description("@#VideoFacingModeEnum")]
[ECMAScript]
[String]
public enum VideoFacingModeEnum
{
    [Description("@#User")]
    User = 0,

    [Description("@#Environment")]
    Environment = 1,

    [Description("@#Left")]
    Left = 2,

    [Description("@#Right")]
    Right = 3
}

/// <summary>
/// VideoMatrixCoefficients
/// </summary>
[Description("@#VideoMatrixCoefficients")]
[ECMAScript]
[String]
public enum VideoMatrixCoefficients
{
    [Description("@#Rgb")]
    Rgb = 0,

    [Description("@#Bt709")]
    Bt709 = 1,

    [Description("@#Bt470bg")]
    Bt470bg = 2,

    [Description("@#Smpte170m")]
    Smpte170m = 3,

    [Description("@#Bt2020Ncl")]
    Bt2020Ncl = 4
}

/// <summary>
/// VideoPixelFormat
/// </summary>
[Description("@#VideoPixelFormat")]
[ECMAScript]
[String]
public enum VideoPixelFormat
{
    [Description("@#I420")]
    I420 = 0,

    [Description("@#I420P10")]
    I420P10 = 1,

    [Description("@#I420P12")]
    I420P12 = 2,

    [Description("@#I420A")]
    I420A = 3,

    [Description("@#I420AP10")]
    I420AP10 = 4,

    [Description("@#I420AP12")]
    I420AP12 = 5,

    [Description("@#I422")]
    I422 = 6,

    [Description("@#I422P10")]
    I422P10 = 7,

    [Description("@#I422P12")]
    I422P12 = 8,

    [Description("@#I422A")]
    I422A = 9,

    [Description("@#I422AP10")]
    I422AP10 = 10,

    [Description("@#I422AP12")]
    I422AP12 = 11,

    [Description("@#I444")]
    I444 = 12,

    [Description("@#I444P10")]
    I444P10 = 13,

    [Description("@#I444P12")]
    I444P12 = 14,

    [Description("@#I444A")]
    I444A = 15,

    [Description("@#I444AP10")]
    I444AP10 = 16,

    [Description("@#I444AP12")]
    I444AP12 = 17,

    [Description("@#NV12")]
    NV12 = 18,

    [Description("@#RGBA")]
    RGBA = 19,

    [Description("@#RGBX")]
    RGBX = 20,

    [Description("@#BGRA")]
    BGRA = 21,

    [Description("@#BGRX")]
    BGRX = 22
}

/// <summary>
/// VideoResizeModeEnum
/// </summary>
[Description("@#VideoResizeModeEnum")]
[ECMAScript]
[String]
public enum VideoResizeModeEnum
{
    [Description("@#None")]
    None = 0,

    [Description("@#CropAndScale")]
    CropAndScale = 1
}

/// <summary>
/// VideoTransferCharacteristics
/// </summary>
[Description("@#VideoTransferCharacteristics")]
[ECMAScript]
[String]
public enum VideoTransferCharacteristics
{
    [Description("@#Bt709")]
    Bt709 = 0,

    [Description("@#Smpte170m")]
    Smpte170m = 1,

    [Description("@#Iec6196621")]
    Iec6196621 = 2,

    [Description("@#Linear")]
    Linear = 3,

    [Description("@#Pq")]
    Pq = 4,

    [Description("@#Hlg")]
    Hlg = 5
}

/// <summary>
/// ViewTransitionNavigation
/// </summary>
[Description("@#ViewTransitionNavigation")]
[ECMAScript]
[String]
public enum ViewTransitionNavigation
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#None")]
    None = 1
}

/// <summary>
/// WakeLockType
/// </summary>
[Description("@#WakeLockType")]
[ECMAScript]
[String]
public enum WakeLockType
{
    [Description("@#Screen")]
    Screen = 0
}

/// <summary>
/// WebGLPowerPreference
/// </summary>
[Description("@#WebGLPowerPreference")]
[ECMAScript]
[String]
public enum WebGLPowerPreference
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#LowPower")]
    LowPower = 1,

    [Description("@#HighPerformance")]
    HighPerformance = 2
}

/// <summary>
/// WebTransportCongestionControl
/// </summary>
[Description("@#WebTransportCongestionControl")]
[ECMAScript]
[String]
public enum WebTransportCongestionControl
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Throughput")]
    Throughput = 1,

    [Description("@#LowLatency")]
    LowLatency = 2
}

/// <summary>
/// WebTransportErrorSource
/// </summary>
[Description("@#WebTransportErrorSource")]
[ECMAScript]
[String]
public enum WebTransportErrorSource
{
    [Description("@#Stream")]
    Stream = 0,

    [Description("@#Session")]
    Session = 1
}

/// <summary>
/// WebTransportReliabilityMode
/// </summary>
[Description("@#WebTransportReliabilityMode")]
[ECMAScript]
[String]
public enum WebTransportReliabilityMode
{
    [Description("@#Pending")]
    Pending = 0,

    [Description("@#ReliableOnly")]
    ReliableOnly = 1,

    [Description("@#SupportsUnreliable")]
    SupportsUnreliable = 2
}

/// <summary>
/// WellKnownDirectory
/// </summary>
[Description("@#WellKnownDirectory")]
[ECMAScript]
[String]
public enum WellKnownDirectory
{
    [Description("@#Desktop")]
    Desktop = 0,

    [Description("@#Documents")]
    Documents = 1,

    [Description("@#Downloads")]
    Downloads = 2,

    [Description("@#Music")]
    Music = 3,

    [Description("@#Pictures")]
    Pictures = 4,

    [Description("@#Videos")]
    Videos = 5
}

/// <summary>
/// WorkerType
/// </summary>
[Description("@#WorkerType")]
[ECMAScript]
[String]
public enum WorkerType
{
    [Description("@#Classic")]
    Classic = 0,

    [Description("@#Module")]
    Module = 1
}

/// <summary>
/// WriteCommandType
/// </summary>
[Description("@#WriteCommandType")]
[ECMAScript]
[String]
public enum WriteCommandType
{
    [Description("@#Write")]
    Write = 0,

    [Description("@#Seek")]
    Seek = 1,

    [Description("@#Truncate")]
    Truncate = 2
}

/// <summary>
/// XMLHttpRequestResponseType
/// </summary>
[Description("@#XMLHttpRequestResponseType")]
[ECMAScript]
[String]
public enum XMLHttpRequestResponseType
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Arraybuffer")]
    Arraybuffer = 1,

    [Description("@#Blob")]
    Blob = 2,

    [Description("@#Document")]
    Document = 3,

    [Description("@#Json")]
    Json = 4,

    [Description("@#Text")]
    Text = 5
}

/// <summary>
/// XRDOMOverlayType
/// </summary>
[Description("@#XRDOMOverlayType")]
[ECMAScript]
[String]
public enum XRDOMOverlayType
{
    [Description("@#Screen")]
    Screen = 0,

    [Description("@#Floating")]
    Floating = 1,

    [Description("@#HeadLocked")]
    HeadLocked = 2
}

/// <summary>
/// XRDepthDataFormat
/// </summary>
[Description("@#XRDepthDataFormat")]
[ECMAScript]
[String]
public enum XRDepthDataFormat
{
    [Description("@#LuminanceAlpha")]
    LuminanceAlpha = 0,

    [Description("@#Float32")]
    Float32 = 1
}

/// <summary>
/// XRDepthUsage
/// </summary>
[Description("@#XRDepthUsage")]
[ECMAScript]
[String]
public enum XRDepthUsage
{
    [Description("@#CpuOptimized")]
    CpuOptimized = 0,

    [Description("@#GpuOptimized")]
    GpuOptimized = 1
}

/// <summary>
/// XREnvironmentBlendMode
/// </summary>
[Description("@#XREnvironmentBlendMode")]
[ECMAScript]
[String]
public enum XREnvironmentBlendMode
{
    [Description("@#Opaque")]
    Opaque = 0,

    [Description("@#AlphaBlend")]
    AlphaBlend = 1,

    [Description("@#Additive")]
    Additive = 2
}

/// <summary>
/// XREye
/// </summary>
[Description("@#XREye")]
[ECMAScript]
[String]
public enum XREye
{
    [Description("@#None")]
    None = 0,

    [Description("@#Left")]
    Left = 1,

    [Description("@#Right")]
    Right = 2
}

/// <summary>
/// XRHandJoint
/// </summary>
[Description("@#XRHandJoint")]
[ECMAScript]
[String]
public enum XRHandJoint
{
    [Description("@#Wrist")]
    Wrist = 0,

    [Description("@#ThumbMetacarpal")]
    ThumbMetacarpal = 1,

    [Description("@#ThumbPhalanxProximal")]
    ThumbPhalanxProximal = 2,

    [Description("@#ThumbPhalanxDistal")]
    ThumbPhalanxDistal = 3,

    [Description("@#ThumbTip")]
    ThumbTip = 4,

    [Description("@#IndexFingerMetacarpal")]
    IndexFingerMetacarpal = 5,

    [Description("@#IndexFingerPhalanxProximal")]
    IndexFingerPhalanxProximal = 6,

    [Description("@#IndexFingerPhalanxIntermediate")]
    IndexFingerPhalanxIntermediate = 7,

    [Description("@#IndexFingerPhalanxDistal")]
    IndexFingerPhalanxDistal = 8,

    [Description("@#IndexFingerTip")]
    IndexFingerTip = 9,

    [Description("@#MiddleFingerMetacarpal")]
    MiddleFingerMetacarpal = 10,

    [Description("@#MiddleFingerPhalanxProximal")]
    MiddleFingerPhalanxProximal = 11,

    [Description("@#MiddleFingerPhalanxIntermediate")]
    MiddleFingerPhalanxIntermediate = 12,

    [Description("@#MiddleFingerPhalanxDistal")]
    MiddleFingerPhalanxDistal = 13,

    [Description("@#MiddleFingerTip")]
    MiddleFingerTip = 14,

    [Description("@#RingFingerMetacarpal")]
    RingFingerMetacarpal = 15,

    [Description("@#RingFingerPhalanxProximal")]
    RingFingerPhalanxProximal = 16,

    [Description("@#RingFingerPhalanxIntermediate")]
    RingFingerPhalanxIntermediate = 17,

    [Description("@#RingFingerPhalanxDistal")]
    RingFingerPhalanxDistal = 18,

    [Description("@#RingFingerTip")]
    RingFingerTip = 19,

    [Description("@#PinkyFingerMetacarpal")]
    PinkyFingerMetacarpal = 20,

    [Description("@#PinkyFingerPhalanxProximal")]
    PinkyFingerPhalanxProximal = 21,

    [Description("@#PinkyFingerPhalanxIntermediate")]
    PinkyFingerPhalanxIntermediate = 22,

    [Description("@#PinkyFingerPhalanxDistal")]
    PinkyFingerPhalanxDistal = 23,

    [Description("@#PinkyFingerTip")]
    PinkyFingerTip = 24
}

/// <summary>
/// XRHandedness
/// </summary>
[Description("@#XRHandedness")]
[ECMAScript]
[String]
public enum XRHandedness
{
    [Description("@#None")]
    None = 0,

    [Description("@#Left")]
    Left = 1,

    [Description("@#Right")]
    Right = 2
}

/// <summary>
/// XRHitTestTrackableType
/// </summary>
[Description("@#XRHitTestTrackableType")]
[ECMAScript]
[String]
public enum XRHitTestTrackableType
{
    [Description("@#Point")]
    Point = 0,

    [Description("@#Plane")]
    Plane = 1,

    [Description("@#Mesh")]
    Mesh = 2
}

/// <summary>
/// XRInteractionMode
/// </summary>
[Description("@#XRInteractionMode")]
[ECMAScript]
[String]
public enum XRInteractionMode
{
    [Description("@#ScreenSpace")]
    ScreenSpace = 0,

    [Description("@#WorldSpace")]
    WorldSpace = 1
}

/// <summary>
/// XRLayerLayout
/// </summary>
[Description("@#XRLayerLayout")]
[ECMAScript]
[String]
public enum XRLayerLayout
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Mono")]
    Mono = 1,

    [Description("@#Stereo")]
    Stereo = 2,

    [Description("@#StereoLeftRight")]
    StereoLeftRight = 3,

    [Description("@#StereoTopBottom")]
    StereoTopBottom = 4
}

/// <summary>
/// XRLayerQuality
/// </summary>
[Description("@#XRLayerQuality")]
[ECMAScript]
[String]
public enum XRLayerQuality
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#TextOptimized")]
    TextOptimized = 1,

    [Description("@#GraphicsOptimized")]
    GraphicsOptimized = 2
}

/// <summary>
/// XRPlaneOrientation
/// </summary>
[Description("@#XRPlaneOrientation")]
[ECMAScript]
[String]
public enum XRPlaneOrientation
{
    [Description("@#Horizontal")]
    Horizontal = 0,

    [Description("@#Vertical")]
    Vertical = 1
}

/// <summary>
/// XRReferenceSpaceType
/// </summary>
[Description("@#XRReferenceSpaceType")]
[ECMAScript]
[String]
public enum XRReferenceSpaceType
{
    [Description("@#Viewer")]
    Viewer = 0,

    [Description("@#Local")]
    Local = 1,

    [Description("@#LocalFloor")]
    LocalFloor = 2,

    [Description("@#BoundedFloor")]
    BoundedFloor = 3,

    [Description("@#Unbounded")]
    Unbounded = 4
}

/// <summary>
/// XRReflectionFormat
/// </summary>
[Description("@#XRReflectionFormat")]
[ECMAScript]
[String]
public enum XRReflectionFormat
{
    [Description("@#Srgba8")]
    Srgba8 = 0,

    [Description("@#Rgba16f")]
    Rgba16f = 1
}

/// <summary>
/// XRSessionMode
/// </summary>
[Description("@#XRSessionMode")]
[ECMAScript]
[String]
public enum XRSessionMode
{
    [Description("@#Inline")]
    Inline = 0,

    [Description("@#ImmersiveVr")]
    ImmersiveVr = 1,

    [Description("@#ImmersiveAr")]
    ImmersiveAr = 2
}

/// <summary>
/// XRTargetRayMode
/// </summary>
[Description("@#XRTargetRayMode")]
[ECMAScript]
[String]
public enum XRTargetRayMode
{
    [Description("@#Gaze")]
    Gaze = 0,

    [Description("@#TrackedPointer")]
    TrackedPointer = 1,

    [Description("@#Screen")]
    Screen = 2,

    [Description("@#TransientPointer")]
    TransientPointer = 3
}

/// <summary>
/// XRTextureType
/// </summary>
[Description("@#XRTextureType")]
[ECMAScript]
[String]
public enum XRTextureType
{
    [Description("@#Texture")]
    Texture = 0,

    [Description("@#TextureArray")]
    TextureArray = 1
}

/// <summary>
/// XRVisibilityState
/// </summary>
[Description("@#XRVisibilityState")]
[ECMAScript]
[String]
public enum XRVisibilityState
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#VisibleBlurred")]
    VisibleBlurred = 1,

    [Description("@#Hidden")]
    Hidden = 2
}
