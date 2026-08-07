namespace ECMAScript;

/// <summary>
/// AacBitstreamFormat
/// </summary>
[Description("@#AacBitstreamFormat")]
[ECMAScript]
[String]
public enum AacBitstreamFormat
{
    [Description("@#aac")]
    Aac = 0,

    [Description("@#adts")]
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
    [Description("@#device")]
    Device = 0,

    [Description("@#screen")]
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
    [Description("@#start")]
    Start = 0,

    [Description("@#center")]
    Center = 1,

    [Description("@#end")]
    End = 2,

    [Description("@#left")]
    Left = 3,

    [Description("@#right")]
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
    [Description("@#keep")]
    Keep = 0,

    [Description("@#discard")]
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
    [Description("@#idle")]
    Idle = 0,

    [Description("@#running")]
    Running = 1,

    [Description("@#paused")]
    Paused = 2,

    [Description("@#finished")]
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
    [Description("@#active")]
    Active = 0,

    [Description("@#removed")]
    Removed = 1,

    [Description("@#persisted")]
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
    [Description("@#accepted")]
    Accepted = 0,

    [Description("@#dismissed")]
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
    [Description("@#segments")]
    Segments = 0,

    [Description("@#sequence")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#indirect")]
    Indirect = 1,

    [Description("@#direct")]
    Direct = 2,

    [Description("@#enterprise")]
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
    [Description("@#balanced")]
    Balanced = 0,

    [Description("@#interactive")]
    Interactive = 1,

    [Description("@#playback")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#hardware")]
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
    [Description("@#suspended")]
    Suspended = 0,

    [Description("@#running")]
    Running = 1,

    [Description("@#closed")]
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
    [Description("@#u8")]
    U8 = 0,

    [Description("@#s16")]
    S16 = 1,

    [Description("@#s32")]
    S32 = 2,

    [Description("@#f32")]
    F32 = 3,

    [Description("@#u8-planar")]
    U8Planar = 4,

    [Description("@#s16-planar")]
    S16Planar = 5,

    [Description("@#s32-planar")]
    S32Planar = 6,

    [Description("@#f32-planar")]
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
    [Description("@#inactive")]
    Inactive = 0,

    [Description("@#active")]
    Active = 1,

    [Description("@#interrupted")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#playback")]
    Playback = 1,

    [Description("@#transient")]
    Transient = 2,

    [Description("@#transient-solo")]
    TransientSolo = 3,

    [Description("@#ambient")]
    Ambient = 4,

    [Description("@#play-and-record")]
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
    [Description("@#none")]
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
    [Description("@#platform")]
    Platform = 0,

    [Description("@#cross-platform")]
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
    [Description("@#usb")]
    Usb = 0,

    [Description("@#nfc")]
    Nfc = 1,

    [Description("@#ble")]
    Ble = 2,

    [Description("@#smart-card")]
    SmartCard = 3,

    [Description("@#hybrid")]
    Hybrid = 4,

    [Description("@#internal")]
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
    [Description("@#auto")]
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
    [Description("@#a-rate")]
    ARate = 0,

    [Description("@#k-rate")]
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
    [Description("@#allowed")]
    Allowed = 0,

    [Description("@#allowed-muted")]
    AllowedMuted = 1,

    [Description("@#disallowed")]
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
    [Description("@#mediaelement")]
    Mediaelement = 0,

    [Description("@#audiocontext")]
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
    [Description("@#annexb")]
    Annexb = 0,

    [Description("@#avc")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#aborted")]
    Aborted = 1,

    [Description("@#bad-status")]
    BadStatus = 2,

    [Description("@#fetch-error")]
    FetchError = 3,

    [Description("@#quota-exceeded")]
    QuotaExceeded = 4,

    [Description("@#download-total-exceeded")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#success")]
    Success = 1,

    [Description("@#failure")]
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
    [Description("@#aztec")]
    Aztec = 0,

    [Description("@#code_128")]
    Code128 = 1,

    [Description("@#code_39")]
    Code39 = 2,

    [Description("@#code_93")]
    Code93 = 3,

    [Description("@#codabar")]
    Codabar = 4,

    [Description("@#data_matrix")]
    DataMatrix = 5,

    [Description("@#ean_13")]
    Ean13 = 6,

    [Description("@#ean_8")]
    Ean8 = 7,

    [Description("@#itf")]
    Itf = 8,

    [Description("@#pdf417")]
    Pdf417 = 9,

    [Description("@#qr_code")]
    QrCode = 10,

    [Description("@#unknown")]
    Unknown = 11,

    [Description("@#upc_a")]
    UpcA = 12,

    [Description("@#upc_e")]
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
    [Description("@#blob")]
    Blob = 0,

    [Description("@#arraybuffer")]
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
    [Description("@#lowpass")]
    Lowpass = 0,

    [Description("@#highpass")]
    Highpass = 1,

    [Description("@#bandpass")]
    Bandpass = 2,

    [Description("@#lowshelf")]
    Lowshelf = 3,

    [Description("@#highshelf")]
    Highshelf = 4,

    [Description("@#peaking")]
    Peaking = 5,

    [Description("@#notch")]
    Notch = 6,

    [Description("@#allpass")]
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
    [Description("@#constant")]
    Constant = 0,

    [Description("@#variable")]
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
    [Description("@#margin")]
    Margin = 0,

    [Description("@#border")]
    Border = 1,

    [Description("@#padding")]
    Padding = 2,

    [Description("@#content")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#maybe")]
    Maybe = 1,

    [Description("@#probably")]
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
    [Description("@#ltr")]
    Ltr = 0,

    [Description("@#rtl")]
    Rtl = 1,

    [Description("@#inherit")]
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
    [Description("@#nonzero")]
    Nonzero = 0,

    [Description("@#evenodd")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#normal")]
    Normal = 1,

    [Description("@#none")]
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
    [Description("@#ultra-condensed")]
    UltraCondensed = 0,

    [Description("@#extra-condensed")]
    ExtraCondensed = 1,

    [Description("@#condensed")]
    Condensed = 2,

    [Description("@#semi-condensed")]
    SemiCondensed = 3,

    [Description("@#normal")]
    Normal = 4,

    [Description("@#semi-expanded")]
    SemiExpanded = 5,

    [Description("@#expanded")]
    Expanded = 6,

    [Description("@#extra-expanded")]
    ExtraExpanded = 7,

    [Description("@#ultra-expanded")]
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
    [Description("@#normal")]
    Normal = 0,

    [Description("@#small-caps")]
    SmallCaps = 1,

    [Description("@#all-small-caps")]
    AllSmallCaps = 2,

    [Description("@#petite-caps")]
    PetiteCaps = 3,

    [Description("@#all-petite-caps")]
    AllPetiteCaps = 4,

    [Description("@#unicase")]
    Unicase = 5,

    [Description("@#titling-caps")]
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
    [Description("@#butt")]
    Butt = 0,

    [Description("@#round")]
    Round = 1,

    [Description("@#square")]
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
    [Description("@#round")]
    Round = 0,

    [Description("@#bevel")]
    Bevel = 1,

    [Description("@#miter")]
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
    [Description("@#start")]
    Start = 0,

    [Description("@#end")]
    End = 1,

    [Description("@#left")]
    Left = 2,

    [Description("@#right")]
    Right = 3,

    [Description("@#center")]
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
    [Description("@#top")]
    Top = 0,

    [Description("@#hanging")]
    Hanging = 1,

    [Description("@#middle")]
    Middle = 2,

    [Description("@#alphabetic")]
    Alphabetic = 3,

    [Description("@#ideographic")]
    Ideographic = 4,

    [Description("@#bottom")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#optimizeSpeed")]
    OptimizeSpeed = 1,

    [Description("@#optimizeLegibility")]
    OptimizeLegibility = 2,

    [Description("@#geometricPrecision")]
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
    [Description("@#next")]
    Next = 0,

    [Description("@#previous")]
    Previous = 1,

    [Description("@#first")]
    First = 2,

    [Description("@#last")]
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
    [Description("@#focus-capturing-application")]
    FocusCapturingApplication = 0,

    [Description("@#focus-captured-surface")]
    FocusCapturedSurface = 1,

    [Description("@#no-focus-change")]
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
    [Description("@#max")]
    Max = 0,

    [Description("@#clamped-max")]
    ClampedMax = 1,

    [Description("@#explicit")]
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
    [Description("@#speakers")]
    Speakers = 0,

    [Description("@#discrete")]
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
    [Description("@#conditionalCreate")]
    ConditionalCreate = 0,

    [Description("@#conditionalMediation")]
    ConditionalMediation = 1,

    [Description("@#hybridTransport")]
    HybridTransport = 2,

    [Description("@#passkeyPlatformAuthenticator")]
    PasskeyPlatformAuthenticator = 3,

    [Description("@#userVerifyingPlatformAuthenticator")]
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
    [Description("@#active")]
    Active = 0,

    [Description("@#frozen")]
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
    [Description("@#window")]
    Window = 0,

    [Description("@#worker")]
    Worker = 1,

    [Description("@#sharedworker")]
    Sharedworker = 2,

    [Description("@#all")]
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
    [Description("@#unconfigured")]
    Unconfigured = 0,

    [Description("@#configured")]
    Configured = 1,

    [Description("@#closed")]
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
    [Description("@#srgb")]
    Srgb = 0,

    [Description("@#p3")]
    P3 = 1,

    [Description("@#rec2020")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#default")]
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
    [Description("@#replace")]
    Replace = 0,

    [Description("@#add")]
    Add = 1,

    [Description("@#accumulate")]
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
    [Description("@#replace")]
    Replace = 0,

    [Description("@#add")]
    Add = 1,

    [Description("@#accumulate")]
    Accumulate = 2,

    [Description("@#auto")]
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
    [Description("@#deflate")]
    Deflate = 0,

    [Description("@#deflate-raw")]
    DeflateRaw = 1,

    [Description("@#gzip")]
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
    [Description("@#bluetooth")]
    Bluetooth = 0,

    [Description("@#cellular")]
    Cellular = 1,

    [Description("@#ethernet")]
    Ethernet = 2,

    [Description("@#mixed")]
    Mixed = 3,

    [Description("@#none")]
    None = 4,

    [Description("@#other")]
    Other = 5,

    [Description("@#unknown")]
    Unknown = 6,

    [Description("@#wifi")]
    Wifi = 7,

    [Description("@#wimax")]
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
    [Description("@#address")]
    Address = 0,

    [Description("@#email")]
    Email = 1,

    [Description("@#icon")]
    Icon = 2,

    [Description("@#name")]
    Name = 3,

    [Description("@#tel")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#homepage")]
    Homepage = 1,

    [Description("@#article")]
    Article = 2,

    [Description("@#video")]
    Video = 3,

    [Description("@#audio")]
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
    [Description("@#strict")]
    Strict = 0,

    [Description("@#lax")]
    Lax = 1,

    [Description("@#none")]
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
    [Description("@#silent")]
    Silent = 0,

    [Description("@#optional")]
    Optional = 1,

    [Description("@#conditional")]
    Conditional = 2,

    [Description("@#required")]
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
    [Description("@#never")]
    Never = 0,

    [Description("@#always")]
    Always = 1,

    [Description("@#motion")]
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
    [Description("@#text/html")]
    TextHtml = 0,

    [Description("@#text/xml")]
    TextXml = 1,

    [Description("@#application/xml")]
    ApplicationXml = 2,

    [Description("@#application/xhtml\u002Bxml")]
    ApplicationXhtmlXml = 3,

    [Description("@#image/svg\u002Bxml")]
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
    [Description("@#continuous")]
    Continuous = 0,

    [Description("@#folded")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#rl")]
    Rl = 1,

    [Description("@#lr")]
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
    [Description("@#monitor")]
    Monitor = 0,

    [Description("@#window")]
    Window = 1,

    [Description("@#browser")]
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
    [Description("@#linear")]
    Linear = 0,

    [Description("@#inverse")]
    Inverse = 1,

    [Description("@#exponential")]
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
    [Description("@#loading")]
    Loading = 0,

    [Description("@#interactive")]
    Interactive = 1,

    [Description("@#complete")]
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
    [Description("@#visible")]
    Visible = 0,

    [Description("@#hidden")]
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
    [Description("@#2g")]
    _2g = 0,

    [Description("@#3g")]
    _3g = 1,

    [Description("@#4g")]
    _4g = 2,

    [Description("@#slow-2g")]
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
    [Description("@#key")]
    Key = 0,

    [Description("@#delta")]
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
    [Description("@#key")]
    Key = 0,

    [Description("@#delta")]
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
    [Description("@#network")]
    Network = 0,

    [Description("@#decode")]
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
    [Description("@#transparent")]
    Transparent = 0,

    [Description("@#native")]
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
    [Description("@#buyer")]
    Buyer = 0,

    [Description("@#seller")]
    Seller = 1,

    [Description("@#component-seller")]
    ComponentSeller = 2,

    [Description("@#direct-seller")]
    DirectSeller = 3,

    [Description("@#shared-storage-select-url")]
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
    [Description("@#file")]
    File = 0,

    [Description("@#directory")]
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
    [Description("@#read")]
    Read = 0,

    [Description("@#readwrite")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#off")]
    Off = 1,

    [Description("@#flash")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#forwards")]
    Forwards = 1,

    [Description("@#backwards")]
    Backwards = 2,

    [Description("@#both")]
    Both = 3,

    [Description("@#auto")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#hardware")]
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
    [Description("@#visible")]
    Visible = 0,

    [Description("@#all")]
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
    [Description("@#unloaded")]
    Unloaded = 0,

    [Description("@#loading")]
    Loading = 1,

    [Description("@#loaded")]
    Loaded = 2,

    [Description("@#error")]
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
    [Description("@#loading")]
    Loading = 0,

    [Description("@#loaded")]
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
    [Description("@#auxiliary")]
    Auxiliary = 0,

    [Description("@#top-level")]
    TopLevel = 1,

    [Description("@#nested")]
    Nested = 2,

    [Description("@#none")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#show")]
    Show = 1,

    [Description("@#hide")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#left")]
    Left = 1,

    [Description("@#right")]
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
    [Description("@#dual-rumble")]
    DualRumble = 0,

    [Description("@#trigger-rumble")]
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
    [Description("@#complete")]
    Complete = 0,

    [Description("@#preempted")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#standard")]
    Standard = 1,

    [Description("@#xr-standard")]
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
    [Description("@#device")]
    Device = 0,

    [Description("@#screen")]
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
    [Description("@#1.0")]
    _10 = 0,

    [Description("@#1.1")]
    _11 = 1,

    [Description("@#1.2")]
    _12 = 2,

    [Description("@#1.3")]
    _13 = 3,

    [Description("@#1.4")]
    _14 = 4,

    [Description("@#2.0")]
    _20 = 5,

    [Description("@#2.1")]
    _21 = 6,

    [Description("@#2.2")]
    _22 = 7,

    [Description("@#2.3")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#si-linear")]
    SiLinear = 1,

    [Description("@#si-rotation")]
    SiRotation = 2,

    [Description("@#english-linear")]
    EnglishLinear = 3,

    [Description("@#english-rotation")]
    EnglishRotation = 4,

    [Description("@#vendor-defined")]
    VendorDefined = 5,

    [Description("@#reserved")]
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
    [Description("@#no-preference")]
    NoPreference = 0,

    [Description("@#prefer-hardware")]
    PreferHardware = 1,

    [Description("@#prefer-software")]
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
    [Description("@#smpteSt2086")]
    SmpteSt2086 = 0,

    [Description("@#smpteSt2094-10")]
    SmpteSt209410 = 1,

    [Description("@#smpteSt2094-40")]
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
    [Description("@#annexb")]
    Annexb = 0,

    [Description("@#hevc")]
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
    [Description("@#next")]
    Next = 0,

    [Description("@#nextunique")]
    Nextunique = 1,

    [Description("@#prev")]
    Prev = 2,

    [Description("@#prevunique")]
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
    [Description("@#pending")]
    Pending = 0,

    [Description("@#done")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#strict")]
    Strict = 1,

    [Description("@#relaxed")]
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
    [Description("@#readonly")]
    Readonly = 0,

    [Description("@#readwrite")]
    Readwrite = 1,

    [Description("@#versionchange")]
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
    [Description("@#public")]
    Public = 0,

    [Description("@#private")]
    Private = 1,

    [Description("@#local")]
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
    [Description("@#signin")]
    Signin = 0,

    [Description("@#signup")]
    Signup = 1,

    [Description("@#use")]
    Use = 2,

    [Description("@#continue")]
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
    [Description("@#from-image")]
    FromImage = 0,

    [Description("@#flipY")]
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
    [Description("@#low")]
    Low = 0,

    [Description("@#medium")]
    Medium = 1,

    [Description("@#high")]
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
    [Description("@#product")]
    Product = 0,

    [Description("@#subscription")]
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
    [Description("@#replace")]
    Replace = 0,

    [Description("@#accumulate")]
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
    [Description("@#passedAndEnforced")]
    PassedAndEnforced = 0,

    [Description("@#passedNotEnforced")]
    PassedNotEnforced = 1,

    [Description("@#belowThreshold")]
    BelowThreshold = 2,

    [Description("@#notCalculated")]
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
    [Description("@#raw")]
    Raw = 0,

    [Description("@#spki")]
    Spki = 1,

    [Description("@#pkcs8")]
    Pkcs8 = 2,

    [Description("@#jwk")]
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
    [Description("@#public")]
    Public = 0,

    [Description("@#private")]
    Private = 1,

    [Description("@#secret")]
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
    [Description("@#encrypt")]
    Encrypt = 0,

    [Description("@#decrypt")]
    Decrypt = 1,

    [Description("@#sign")]
    Sign = 2,

    [Description("@#verify")]
    Verify = 3,

    [Description("@#deriveKey")]
    DeriveKey = 4,

    [Description("@#deriveBits")]
    DeriveBits = 5,

    [Description("@#wrapKey")]
    WrapKey = 6,

    [Description("@#unwrapKey")]
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
    [Description("@#mouth")]
    Mouth = 0,

    [Description("@#eye")]
    Eye = 1,

    [Description("@#nose")]
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
    [Description("@#required")]
    Required = 0,

    [Description("@#preferred")]
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
    [Description("@#quality")]
    Quality = 0,

    [Description("@#realtime")]
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
    [Description("@#start")]
    Start = 0,

    [Description("@#center")]
    Center = 1,

    [Description("@#end")]
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
    [Description("@#shared")]
    Shared = 0,

    [Description("@#exclusive")]
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
    [Description("@#logged-in")]
    LoggedIn = 0,

    [Description("@#logged-out")]
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
    [Description("@#open")]
    Open = 0,

    [Description("@#closed")]
    Closed = 1,

    [Description("@#pending")]
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
    [Description("@#disconnected")]
    Disconnected = 0,

    [Description("@#connected")]
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
    [Description("@#input")]
    Input = 0,

    [Description("@#output")]
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
    [Description("@#oihw")]
    Oihw = 0,

    [Description("@#hwio")]
    Hwio = 1,

    [Description("@#ohwi")]
    Ohwi = 2,

    [Description("@#ihwo")]
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
    [Description("@#iohw")]
    Iohw = 0,

    [Description("@#hwoi")]
    Hwoi = 1,

    [Description("@#ohwi")]
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
    [Description("@#cpu")]
    Cpu = 0,

    [Description("@#gpu")]
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
    [Description("@#zrn")]
    Zrn = 0,

    [Description("@#rzn")]
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
    [Description("@#nchw")]
    Nchw = 0,

    [Description("@#nhwc")]
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
    [Description("@#nearest-neighbor")]
    NearestNeighbor = 0,

    [Description("@#linear")]
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
    [Description("@#iofg")]
    Iofg = 0,

    [Description("@#ifgo")]
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
    [Description("@#float32")]
    Float32 = 0,

    [Description("@#float16")]
    Float16 = 1,

    [Description("@#int32")]
    Int32 = 2,

    [Description("@#uint32")]
    Uint32 = 3,

    [Description("@#int64")]
    Int64 = 4,

    [Description("@#uint64")]
    Uint64 = 5,

    [Description("@#int8")]
    Int8 = 6,

    [Description("@#uint8")]
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
    [Description("@#constant")]
    Constant = 0,

    [Description("@#edge")]
    Edge = 1,

    [Description("@#reflection")]
    Reflection = 2,

    [Description("@#symmetric")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#high-performance")]
    HighPerformance = 1,

    [Description("@#low-power")]
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
    [Description("@#forward")]
    Forward = 0,

    [Description("@#backward")]
    Backward = 1,

    [Description("@#both")]
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
    [Description("@#floor")]
    Floor = 0,

    [Description("@#ceil")]
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
    [Description("@#device")]
    Device = 0,

    [Description("@#screen")]
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
    [Description("@#file")]
    File = 0,

    [Description("@#media-source")]
    MediaSource = 1,

    [Description("@#webrtc")]
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
    [Description("@#audioinput")]
    Audioinput = 0,

    [Description("@#audiooutput")]
    Audiooutput = 1,

    [Description("@#videoinput")]
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
    [Description("@#record")]
    Record = 0,

    [Description("@#webrtc")]
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
    [Description("@#license-request")]
    LicenseRequest = 0,

    [Description("@#license-renewal")]
    LicenseRenewal = 1,

    [Description("@#license-release")]
    LicenseRelease = 2,

    [Description("@#individualization-request")]
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
    [Description("@#internal-error")]
    InternalError = 0,

    [Description("@#closed-by-application")]
    ClosedByApplication = 1,

    [Description("@#release-acknowledged")]
    ReleaseAcknowledged = 2,

    [Description("@#hardware-context-reset")]
    HardwareContextReset = 3,

    [Description("@#resource-evicted")]
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
    [Description("@#temporary")]
    Temporary = 0,

    [Description("@#persistent-license")]
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
    [Description("@#usable")]
    Usable = 0,

    [Description("@#expired")]
    Expired = 1,

    [Description("@#released")]
    Released = 2,

    [Description("@#output-restricted")]
    OutputRestricted = 3,

    [Description("@#output-downscaled")]
    OutputDownscaled = 4,

    [Description("@#usable-in-future")]
    UsableInFuture = 5,

    [Description("@#status-pending")]
    StatusPending = 6,

    [Description("@#internal-error")]
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
    [Description("@#required")]
    Required = 0,

    [Description("@#optional")]
    Optional = 1,

    [Description("@#not-allowed")]
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
    [Description("@#play")]
    Play = 0,

    [Description("@#pause")]
    Pause = 1,

    [Description("@#seekbackward")]
    Seekbackward = 2,

    [Description("@#seekforward")]
    Seekforward = 3,

    [Description("@#previoustrack")]
    Previoustrack = 4,

    [Description("@#nexttrack")]
    Nexttrack = 5,

    [Description("@#skipad")]
    Skipad = 6,

    [Description("@#stop")]
    Stop = 7,

    [Description("@#seekto")]
    Seekto = 8,

    [Description("@#togglemicrophone")]
    Togglemicrophone = 9,

    [Description("@#togglecamera")]
    Togglecamera = 10,

    [Description("@#hangup")]
    Hangup = 11,

    [Description("@#previousslide")]
    Previousslide = 12,

    [Description("@#nextslide")]
    Nextslide = 13,

    [Description("@#enterpictureinpicture")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#paused")]
    Paused = 1,

    [Description("@#playing")]
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
    [Description("@#live")]
    Live = 0,

    [Description("@#ended")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#manual")]
    Manual = 1,

    [Description("@#single-shot")]
    SingleShot = 2,

    [Description("@#continuous")]
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
    [Description("@#granted")]
    Granted = 0,

    [Description("@#denied")]
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
    [Description("@#include")]
    Include = 0,

    [Description("@#exclude")]
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
    [Description("@#after-transition")]
    AfterTransition = 0,

    [Description("@#manual")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#push")]
    Push = 1,

    [Description("@#replace")]
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
    [Description("@#after-transition")]
    AfterTransition = 0,

    [Description("@#manual")]
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
    [Description("@#navigate")]
    Navigate = 0,

    [Description("@#reload")]
    Reload = 1,

    [Description("@#back_forward")]
    BackForward = 2,

    [Description("@#prerender")]
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
    [Description("@#push")]
    Push = 0,

    [Description("@#replace")]
    Replace = 1,

    [Description("@#reload")]
    Reload = 2,

    [Description("@#traverse")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#ltr")]
    Ltr = 1,

    [Description("@#rtl")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#denied")]
    Denied = 1,

    [Description("@#granted")]
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
    [Description("@#sms")]
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
    [Description("@#2d")]
    _2d = 0,

    [Description("@#bitmaprenderer")]
    Bitmaprenderer = 1,

    [Description("@#webgl")]
    Webgl = 2,

    [Description("@#webgl2")]
    Webgl2 = 3,

    [Description("@#webgpu")]
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
    [Description("@#opaque")]
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
    [Description("@#token-request")]
    TokenRequest = 0,

    [Description("@#send-redemption-record")]
    SendRedemptionRecord = 1,

    [Description("@#token-redemption")]
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
    [Description("@#voip")]
    Voip = 0,

    [Description("@#audio")]
    Audio = 1,

    [Description("@#lowdelay")]
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
    [Description("@#opus")]
    Opus = 0,

    [Description("@#ogg")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#music")]
    Music = 1,

    [Description("@#voice")]
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
    [Description("@#any")]
    Any = 0,

    [Description("@#natural")]
    Natural = 1,

    [Description("@#landscape")]
    Landscape = 2,

    [Description("@#portrait")]
    Portrait = 3,

    [Description("@#portrait-primary")]
    PortraitPrimary = 4,

    [Description("@#portrait-secondary")]
    PortraitSecondary = 5,

    [Description("@#landscape-primary")]
    LandscapePrimary = 6,

    [Description("@#landscape-secondary")]
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
    [Description("@#device")]
    Device = 0,

    [Description("@#screen")]
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
    [Description("@#portrait-primary")]
    PortraitPrimary = 0,

    [Description("@#portrait-secondary")]
    PortraitSecondary = 1,

    [Description("@#landscape-primary")]
    LandscapePrimary = 2,

    [Description("@#landscape-secondary")]
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
    [Description("@#sine")]
    Sine = 0,

    [Description("@#square")]
    Square = 1,

    [Description("@#sawtooth")]
    Sawtooth = 2,

    [Description("@#triangle")]
    Triangle = 3,

    [Description("@#custom")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#2x")]
    _2x = 1,

    [Description("@#4x")]
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
    [Description("@#equalpower")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#even")]
    Even = 1,

    [Description("@#odd")]
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
    [Description("@#fail")]
    Fail = 0,

    [Description("@#success")]
    Success = 1,

    [Description("@#unknown")]
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
    [Description("@#shippingAddress")]
    ShippingAddress = 0,

    [Description("@#payerName")]
    PayerName = 1,

    [Description("@#payerPhone")]
    PayerPhone = 2,

    [Description("@#payerEmail")]
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
    [Description("@#shipping")]
    Shipping = 0,

    [Description("@#delivery")]
    Delivery = 1,

    [Description("@#pickup")]
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
    [Description("@#granted")]
    Granted = 0,

    [Description("@#denied")]
    Denied = 1,

    [Description("@#prompt")]
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
    [Description("@#normal")]
    Normal = 0,

    [Description("@#reverse")]
    Reverse = 1,

    [Description("@#alternate")]
    Alternate = 2,

    [Description("@#alternate-reverse")]
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
    [Description("@#line-left")]
    LineLeft = 0,

    [Description("@#center")]
    Center = 1,

    [Description("@#line-right")]
    LineRight = 2,

    [Description("@#auto")]
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
    [Description("@#srgb")]
    Srgb = 0,

    [Description("@#display-p3")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#premultiply")]
    Premultiply = 1,

    [Description("@#default")]
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
    [Description("@#error")]
    Error = 0,

    [Description("@#closed")]
    Closed = 1,

    [Description("@#wentaway")]
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
    [Description("@#connecting")]
    Connecting = 0,

    [Description("@#connected")]
    Connected = 1,

    [Description("@#closed")]
    Closed = 2,

    [Description("@#terminated")]
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
    [Description("@#unspecified")]
    Unspecified = 0,

    [Description("@#inline")]
    Inline = 1,

    [Description("@#attachment")]
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
    [Description("@#thermals")]
    Thermals = 0,

    [Description("@#cpu")]
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
    [Description("@#nominal")]
    Nominal = 0,

    [Description("@#fair")]
    Fair = 1,

    [Description("@#serious")]
    Serious = 2,

    [Description("@#critical")]
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
    [Description("@#security-key")]
    SecurityKey = 0,

    [Description("@#client-device")]
    ClientDevice = 1,

    [Description("@#hybrid")]
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
    [Description("@#public-key")]
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
    [Description("@#p256dh")]
    P256dh = 0,

    [Description("@#auth")]
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
    [Description("@#balanced")]
    Balanced = 0,

    [Description("@#max-compat")]
    MaxCompat = 1,

    [Description("@#max-bundle")]
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
    [Description("@#connecting")]
    Connecting = 0,

    [Description("@#open")]
    Open = 1,

    [Description("@#closing")]
    Closing = 2,

    [Description("@#closed")]
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
    [Description("@#maintain-framerate")]
    MaintainFramerate = 0,

    [Description("@#maintain-resolution")]
    MaintainResolution = 1,

    [Description("@#balanced")]
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
    [Description("@#client")]
    Client = 0,

    [Description("@#server")]
    Server = 1,

    [Description("@#unknown")]
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
    [Description("@#new")]
    New = 0,

    [Description("@#connecting")]
    Connecting = 1,

    [Description("@#connected")]
    Connected = 2,

    [Description("@#closed")]
    Closed = 3,

    [Description("@#failed")]
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
    [Description("@#empty")]
    Empty = 0,

    [Description("@#key")]
    Key = 1,

    [Description("@#delta")]
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
    [Description("@#data-channel-failure")]
    DataChannelFailure = 0,

    [Description("@#dtls-failure")]
    DtlsFailure = 1,

    [Description("@#fingerprint-failure")]
    FingerprintFailure = 2,

    [Description("@#sctp-failure")]
    SctpFailure = 3,

    [Description("@#sdp-syntax-error")]
    SdpSyntaxError = 4,

    [Description("@#hardware-encoder-not-available")]
    HardwareEncoderNotAvailable = 5,

    [Description("@#hardware-encoder-error")]
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
    [Description("@#idp-bad-script-failure")]
    IdpBadScriptFailure = 0,

    [Description("@#idp-execution-failure")]
    IdpExecutionFailure = 1,

    [Description("@#idp-load-failure")]
    IdpLoadFailure = 2,

    [Description("@#idp-need-login")]
    IdpNeedLogin = 3,

    [Description("@#idp-timeout")]
    IdpTimeout = 4,

    [Description("@#idp-tls-failure")]
    IdpTlsFailure = 5,

    [Description("@#idp-token-expired")]
    IdpTokenExpired = 6,

    [Description("@#idp-token-invalid")]
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
    [Description("@#host")]
    Host = 0,

    [Description("@#srflx")]
    Srflx = 1,

    [Description("@#prflx")]
    Prflx = 2,

    [Description("@#relay")]
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
    [Description("@#rtp")]
    Rtp = 0,

    [Description("@#rtcp")]
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
    [Description("@#closed")]
    Closed = 0,

    [Description("@#failed")]
    Failed = 1,

    [Description("@#disconnected")]
    Disconnected = 2,

    [Description("@#new")]
    New = 3,

    [Description("@#checking")]
    Checking = 4,

    [Description("@#completed")]
    Completed = 5,

    [Description("@#connected")]
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
    [Description("@#new")]
    New = 0,

    [Description("@#gathering")]
    Gathering = 1,

    [Description("@#complete")]
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
    [Description("@#new")]
    New = 0,

    [Description("@#gathering")]
    Gathering = 1,

    [Description("@#complete")]
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
    [Description("@#udp")]
    Udp = 0,

    [Description("@#tcp")]
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
    [Description("@#unknown")]
    Unknown = 0,

    [Description("@#controlling")]
    Controlling = 1,

    [Description("@#controlled")]
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
    [Description("@#udp")]
    Udp = 0,

    [Description("@#tcp")]
    Tcp = 1,

    [Description("@#tls")]
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
    [Description("@#active")]
    Active = 0,

    [Description("@#passive")]
    Passive = 1,

    [Description("@#so")]
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
    [Description("@#relay")]
    Relay = 0,

    [Description("@#all")]
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
    [Description("@#closed")]
    Closed = 0,

    [Description("@#failed")]
    Failed = 1,

    [Description("@#disconnected")]
    Disconnected = 2,

    [Description("@#new")]
    New = 3,

    [Description("@#checking")]
    Checking = 4,

    [Description("@#completed")]
    Completed = 5,

    [Description("@#connected")]
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
    [Description("@#closed")]
    Closed = 0,

    [Description("@#failed")]
    Failed = 1,

    [Description("@#disconnected")]
    Disconnected = 2,

    [Description("@#new")]
    New = 3,

    [Description("@#connecting")]
    Connecting = 4,

    [Description("@#connected")]
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
    [Description("@#very-low")]
    VeryLow = 0,

    [Description("@#low")]
    Low = 1,

    [Description("@#medium")]
    Medium = 2,

    [Description("@#high")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#cpu")]
    Cpu = 1,

    [Description("@#bandwidth")]
    Bandwidth = 2,

    [Description("@#other")]
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
    [Description("@#require")]
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
    [Description("@#sendrecv")]
    Sendrecv = 0,

    [Description("@#sendonly")]
    Sendonly = 1,

    [Description("@#recvonly")]
    Recvonly = 2,

    [Description("@#inactive")]
    Inactive = 3,

    [Description("@#stopped")]
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
    [Description("@#connecting")]
    Connecting = 0,

    [Description("@#connected")]
    Connected = 1,

    [Description("@#closed")]
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
    [Description("@#offer")]
    Offer = 0,

    [Description("@#pranswer")]
    Pranswer = 1,

    [Description("@#answer")]
    Answer = 2,

    [Description("@#rollback")]
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
    [Description("@#stable")]
    Stable = 0,

    [Description("@#have-local-offer")]
    HaveLocalOffer = 1,

    [Description("@#have-remote-offer")]
    HaveRemoteOffer = 2,

    [Description("@#have-local-pranswer")]
    HaveLocalPranswer = 3,

    [Description("@#have-remote-pranswer")]
    HaveRemotePranswer = 4,

    [Description("@#closed")]
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
    [Description("@#frozen")]
    Frozen = 0,

    [Description("@#waiting")]
    Waiting = 1,

    [Description("@#in-progress")]
    InProgress = 2,

    [Description("@#failed")]
    Failed = 3,

    [Description("@#succeeded")]
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
    [Description("@#codec")]
    Codec = 0,

    [Description("@#inbound-rtp")]
    InboundRtp = 1,

    [Description("@#outbound-rtp")]
    OutboundRtp = 2,

    [Description("@#remote-inbound-rtp")]
    RemoteInboundRtp = 3,

    [Description("@#remote-outbound-rtp")]
    RemoteOutboundRtp = 4,

    [Description("@#media-source")]
    MediaSource = 5,

    [Description("@#media-playout")]
    MediaPlayout = 6,

    [Description("@#peer-connection")]
    PeerConnection = 7,

    [Description("@#data-channel")]
    DataChannel = 8,

    [Description("@#transport")]
    Transport = 9,

    [Description("@#candidate-pair")]
    CandidatePair = 10,

    [Description("@#local-candidate")]
    LocalCandidate = 11,

    [Description("@#remote-candidate")]
    RemoteCandidate = 12,

    [Description("@#certificate")]
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
    [Description("@#byob")]
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
    [Description("@#bytes")]
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
    [Description("@#closed")]
    Closed = 0,

    [Description("@#open")]
    Open = 1,

    [Description("@#ended")]
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
    [Description("@#inactive")]
    Inactive = 0,

    [Description("@#recording")]
    Recording = 1,

    [Description("@#paused")]
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
    [Description("@#never")]
    Never = 0,

    [Description("@#always")]
    Always = 1,

    [Description("@#controllable")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#no-referrer")]
    NoReferrer = 1,

    [Description("@#no-referrer-when-downgrade")]
    NoReferrerWhenDowngrade = 2,

    [Description("@#same-origin")]
    SameOrigin = 3,

    [Description("@#origin")]
    Origin = 4,

    [Description("@#strict-origin")]
    StrictOrigin = 5,

    [Description("@#origin-when-cross-origin")]
    OriginWhenCrossOrigin = 6,

    [Description("@#strict-origin-when-cross-origin")]
    StrictOriginWhenCrossOrigin = 7,

    [Description("@#unsafe-url")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#refresh")]
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
    [Description("@#connecting")]
    Connecting = 0,

    [Description("@#connected")]
    Connected = 1,

    [Description("@#disconnected")]
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
    [Description("@#blocking")]
    Blocking = 0,

    [Description("@#non-blocking")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#no-store")]
    NoStore = 1,

    [Description("@#reload")]
    Reload = 2,

    [Description("@#no-cache")]
    NoCache = 3,

    [Description("@#force-cache")]
    ForceCache = 4,

    [Description("@#only-if-cached")]
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
    [Description("@#omit")]
    Omit = 0,

    [Description("@#same-origin")]
    SameOrigin = 1,

    [Description("@#include")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#audio")]
    Audio = 1,

    [Description("@#audioworklet")]
    Audioworklet = 2,

    [Description("@#document")]
    Document = 3,

    [Description("@#embed")]
    Embed = 4,

    [Description("@#font")]
    Font = 5,

    [Description("@#frame")]
    Frame = 6,

    [Description("@#iframe")]
    Iframe = 7,

    [Description("@#image")]
    Image = 8,

    [Description("@#json")]
    Json = 9,

    [Description("@#manifest")]
    Manifest = 10,

    [Description("@#object")]
    Object = 11,

    [Description("@#paintworklet")]
    Paintworklet = 12,

    [Description("@#report")]
    Report = 13,

    [Description("@#script")]
    Script = 14,

    [Description("@#sharedworker")]
    Sharedworker = 15,

    [Description("@#style")]
    Style = 16,

    [Description("@#track")]
    Track = 17,

    [Description("@#video")]
    Video = 18,

    [Description("@#worker")]
    Worker = 19,

    [Description("@#xslt")]
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
    [Description("@#half")]
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
    [Description("@#navigate")]
    Navigate = 0,

    [Description("@#same-origin")]
    SameOrigin = 1,

    [Description("@#no-cors")]
    NoCors = 2,

    [Description("@#cors")]
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
    [Description("@#high")]
    High = 0,

    [Description("@#low")]
    Low = 1,

    [Description("@#auto")]
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
    [Description("@#follow")]
    Follow = 0,

    [Description("@#error")]
    Error = 1,

    [Description("@#manual")]
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
    [Description("@#discouraged")]
    Discouraged = 0,

    [Description("@#preferred")]
    Preferred = 1,

    [Description("@#required")]
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
    [Description("@#border-box")]
    BorderBox = 0,

    [Description("@#content-box")]
    ContentBox = 1,

    [Description("@#device-pixel-content-box")]
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
    [Description("@#pixelated")]
    Pixelated = 0,

    [Description("@#low")]
    Low = 1,

    [Description("@#medium")]
    Medium = 2,

    [Description("@#high")]
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
    [Description("@#basic")]
    Basic = 0,

    [Description("@#cors")]
    Cors = 1,

    [Description("@#default")]
    Default = 2,

    [Description("@#error")]
    Error = 3,

    [Description("@#opaque")]
    Opaque = 4,

    [Description("@#opaqueredirect")]
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
    [Description("@#cache")]
    Cache = 0,

    [Description("@#fetch-event")]
    FetchEvent = 1,

    [Description("@#network")]
    Network = 2,

    [Description("@#race-network-and-fetch-handler")]
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
    [Description("@#running")]
    Running = 0,

    [Description("@#not-running")]
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
    [Description("@#authentication")]
    Authentication = 0,

    [Description("@#keyID")]
    KeyID = 1,

    [Description("@#syntax")]
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
    [Description("@#encrypt")]
    Encrypt = 0,

    [Description("@#decrypt")]
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
    [Description("@#locked")]
    Locked = 0,

    [Description("@#unlocked")]
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
    [Description("@#classic-script")]
    ClassicScript = 0,

    [Description("@#module-script")]
    ModuleScript = 1,

    [Description("@#event-listener")]
    EventListener = 2,

    [Description("@#user-callback")]
    UserCallback = 3,

    [Description("@#resolve-promise")]
    ResolvePromise = 4,

    [Description("@#reject-promise")]
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
    [Description("@#self")]
    Self = 0,

    [Description("@#descendant")]
    Descendant = 1,

    [Description("@#ancestor")]
    Ancestor = 2,

    [Description("@#same-page")]
    SamePage = 3,

    [Description("@#other")]
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
    [Description("@#externalScript")]
    ExternalScript = 0,

    [Description("@#inlineScript")]
    InlineScript = 1,

    [Description("@#inlineEventHandler")]
    InlineEventHandler = 2,

    [Description("@#eval")]
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
    [Description("@#block")]
    Block = 0,

    [Description("@#inline")]
    Inline = 1,

    [Description("@#x")]
    X = 2,

    [Description("@#y")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#instant")]
    Instant = 1,

    [Description("@#smooth")]
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
    [Description("@#start")]
    Start = 0,

    [Description("@#center")]
    Center = 1,

    [Description("@#end")]
    End = 2,

    [Description("@#nearest")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#manual")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#up")]
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
    [Description("@#enforce")]
    Enforce = 0,

    [Description("@#report")]
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
    [Description("@#select")]
    Select = 0,

    [Description("@#start")]
    Start = 1,

    [Description("@#end")]
    End = 2,

    [Description("@#preserve")]
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
    [Description("@#include")]
    Include = 0,

    [Description("@#exclude")]
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
    [Description("@#parsed")]
    Parsed = 0,

    [Description("@#installing")]
    Installing = 1,

    [Description("@#installed")]
    Installed = 2,

    [Description("@#activating")]
    Activating = 3,

    [Description("@#activated")]
    Activated = 4,

    [Description("@#redundant")]
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
    [Description("@#imports")]
    Imports = 0,

    [Description("@#all")]
    All = 1,

    [Description("@#none")]
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
    [Description("@#open")]
    Open = 0,

    [Description("@#closed")]
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
    [Description("@#manual")]
    Manual = 0,

    [Description("@#named")]
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
    [Description("@#up")]
    Up = 0,

    [Description("@#down")]
    Down = 1,

    [Description("@#left")]
    Left = 2,

    [Description("@#right")]
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
    [Description("@#no-speech")]
    NoSpeech = 0,

    [Description("@#aborted")]
    Aborted = 1,

    [Description("@#audio-capture")]
    AudioCapture = 2,

    [Description("@#network")]
    Network = 3,

    [Description("@#not-allowed")]
    NotAllowed = 4,

    [Description("@#service-not-allowed")]
    ServiceNotAllowed = 5,

    [Description("@#bad-grammar")]
    BadGrammar = 6,

    [Description("@#language-not-supported")]
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
    [Description("@#canceled")]
    Canceled = 0,

    [Description("@#interrupted")]
    Interrupted = 1,

    [Description("@#audio-busy")]
    AudioBusy = 2,

    [Description("@#audio-hardware")]
    AudioHardware = 3,

    [Description("@#network")]
    Network = 4,

    [Description("@#synthesis-unavailable")]
    SynthesisUnavailable = 5,

    [Description("@#synthesis-failed")]
    SynthesisFailed = 6,

    [Description("@#language-unavailable")]
    LanguageUnavailable = 7,

    [Description("@#voice-unavailable")]
    VoiceUnavailable = 8,

    [Description("@#text-too-long")]
    TextTooLong = 9,

    [Description("@#invalid-argument")]
    InvalidArgument = 10,

    [Description("@#not-allowed")]
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
    [Description("@#include")]
    Include = 0,

    [Description("@#exclude")]
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
    [Description("@#include")]
    Include = 0,

    [Description("@#exclude")]
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
    [Description("@#user-blocking")]
    UserBlocking = 0,

    [Description("@#user-visible")]
    UserVisible = 1,

    [Description("@#background")]
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
    [Description("@#subtitles")]
    Subtitles = 0,

    [Description("@#captions")]
    Captions = 1,

    [Description("@#descriptions")]
    Descriptions = 2,

    [Description("@#chapters")]
    Chapters = 3,

    [Description("@#metadata")]
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
    [Description("@#disabled")]
    Disabled = 0,

    [Description("@#hidden")]
    Hidden = 1,

    [Description("@#showing")]
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
    [Description("@#present")]
    Present = 0,

    [Description("@#supported")]
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
    [Description("@#1")]
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
    [Description("@#direct")]
    Direct = 0,

    [Description("@#stylus")]
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
    [Description("@#srgb")]
    Srgb = 0,

    [Description("@#pq")]
    Pq = 1,

    [Description("@#hlg")]
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
    [Description("@#in")]
    In = 0,

    [Description("@#out")]
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
    [Description("@#bulk")]
    Bulk = 0,

    [Description("@#interrupt")]
    Interrupt = 1,

    [Description("@#isochronous")]
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
    [Description("@#device")]
    Device = 0,

    [Description("@#interface")]
    Interface = 1,

    [Description("@#endpoint")]
    Endpoint = 2,

    [Description("@#other")]
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
    [Description("@#standard")]
    Standard = 0,

    [Description("@#class")]
    Class = 1,

    [Description("@#vendor")]
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
    [Description("@#ok")]
    Ok = 0,

    [Description("@#stall")]
    Stall = 1,

    [Description("@#babble")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#solid")]
    Solid = 1,

    [Description("@#dotted")]
    Dotted = 2,

    [Description("@#dashed")]
    Dashed = 3,

    [Description("@#wavy")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#thin")]
    Thin = 1,

    [Description("@#thick")]
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
    [Description("@#active")]
    Active = 0,

    [Description("@#idle")]
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
    [Description("@#required")]
    Required = 0,

    [Description("@#preferred")]
    Preferred = 1,

    [Description("@#discouraged")]
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
    [Description("@#bt709")]
    Bt709 = 0,

    [Description("@#bt470bg")]
    Bt470bg = 1,

    [Description("@#smpte170m")]
    Smpte170m = 2,

    [Description("@#bt2020")]
    Bt2020 = 3,

    [Description("@#smpte432")]
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
    [Description("@#constant")]
    Constant = 0,

    [Description("@#variable")]
    Variable = 1,

    [Description("@#quantizer")]
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
    [Description("@#user")]
    User = 0,

    [Description("@#environment")]
    Environment = 1,

    [Description("@#left")]
    Left = 2,

    [Description("@#right")]
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
    [Description("@#rgb")]
    Rgb = 0,

    [Description("@#bt709")]
    Bt709 = 1,

    [Description("@#bt470bg")]
    Bt470bg = 2,

    [Description("@#smpte170m")]
    Smpte170m = 3,

    [Description("@#bt2020-ncl")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#crop-and-scale")]
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
    [Description("@#bt709")]
    Bt709 = 0,

    [Description("@#smpte170m")]
    Smpte170m = 1,

    [Description("@#iec61966-2-1")]
    Iec6196621 = 2,

    [Description("@#linear")]
    Linear = 3,

    [Description("@#pq")]
    Pq = 4,

    [Description("@#hlg")]
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
    [Description("@#auto")]
    Auto = 0,

    [Description("@#none")]
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
    [Description("@#screen")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#low-power")]
    LowPower = 1,

    [Description("@#high-performance")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#throughput")]
    Throughput = 1,

    [Description("@#low-latency")]
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
    [Description("@#stream")]
    Stream = 0,

    [Description("@#session")]
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
    [Description("@#pending")]
    Pending = 0,

    [Description("@#reliable-only")]
    ReliableOnly = 1,

    [Description("@#supports-unreliable")]
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
    [Description("@#desktop")]
    Desktop = 0,

    [Description("@#documents")]
    Documents = 1,

    [Description("@#downloads")]
    Downloads = 2,

    [Description("@#music")]
    Music = 3,

    [Description("@#pictures")]
    Pictures = 4,

    [Description("@#videos")]
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
    [Description("@#classic")]
    Classic = 0,

    [Description("@#module")]
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
    [Description("@#write")]
    Write = 0,

    [Description("@#seek")]
    Seek = 1,

    [Description("@#truncate")]
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
    [Description("@#")]
    Empty = 0,

    [Description("@#arraybuffer")]
    Arraybuffer = 1,

    [Description("@#blob")]
    Blob = 2,

    [Description("@#document")]
    Document = 3,

    [Description("@#json")]
    Json = 4,

    [Description("@#text")]
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
    [Description("@#screen")]
    Screen = 0,

    [Description("@#floating")]
    Floating = 1,

    [Description("@#head-locked")]
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
    [Description("@#luminance-alpha")]
    LuminanceAlpha = 0,

    [Description("@#float32")]
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
    [Description("@#cpu-optimized")]
    CpuOptimized = 0,

    [Description("@#gpu-optimized")]
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
    [Description("@#opaque")]
    Opaque = 0,

    [Description("@#alpha-blend")]
    AlphaBlend = 1,

    [Description("@#additive")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#left")]
    Left = 1,

    [Description("@#right")]
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
    [Description("@#wrist")]
    Wrist = 0,

    [Description("@#thumb-metacarpal")]
    ThumbMetacarpal = 1,

    [Description("@#thumb-phalanx-proximal")]
    ThumbPhalanxProximal = 2,

    [Description("@#thumb-phalanx-distal")]
    ThumbPhalanxDistal = 3,

    [Description("@#thumb-tip")]
    ThumbTip = 4,

    [Description("@#index-finger-metacarpal")]
    IndexFingerMetacarpal = 5,

    [Description("@#index-finger-phalanx-proximal")]
    IndexFingerPhalanxProximal = 6,

    [Description("@#index-finger-phalanx-intermediate")]
    IndexFingerPhalanxIntermediate = 7,

    [Description("@#index-finger-phalanx-distal")]
    IndexFingerPhalanxDistal = 8,

    [Description("@#index-finger-tip")]
    IndexFingerTip = 9,

    [Description("@#middle-finger-metacarpal")]
    MiddleFingerMetacarpal = 10,

    [Description("@#middle-finger-phalanx-proximal")]
    MiddleFingerPhalanxProximal = 11,

    [Description("@#middle-finger-phalanx-intermediate")]
    MiddleFingerPhalanxIntermediate = 12,

    [Description("@#middle-finger-phalanx-distal")]
    MiddleFingerPhalanxDistal = 13,

    [Description("@#middle-finger-tip")]
    MiddleFingerTip = 14,

    [Description("@#ring-finger-metacarpal")]
    RingFingerMetacarpal = 15,

    [Description("@#ring-finger-phalanx-proximal")]
    RingFingerPhalanxProximal = 16,

    [Description("@#ring-finger-phalanx-intermediate")]
    RingFingerPhalanxIntermediate = 17,

    [Description("@#ring-finger-phalanx-distal")]
    RingFingerPhalanxDistal = 18,

    [Description("@#ring-finger-tip")]
    RingFingerTip = 19,

    [Description("@#pinky-finger-metacarpal")]
    PinkyFingerMetacarpal = 20,

    [Description("@#pinky-finger-phalanx-proximal")]
    PinkyFingerPhalanxProximal = 21,

    [Description("@#pinky-finger-phalanx-intermediate")]
    PinkyFingerPhalanxIntermediate = 22,

    [Description("@#pinky-finger-phalanx-distal")]
    PinkyFingerPhalanxDistal = 23,

    [Description("@#pinky-finger-tip")]
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
    [Description("@#none")]
    None = 0,

    [Description("@#left")]
    Left = 1,

    [Description("@#right")]
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
    [Description("@#point")]
    Point = 0,

    [Description("@#plane")]
    Plane = 1,

    [Description("@#mesh")]
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
    [Description("@#screen-space")]
    ScreenSpace = 0,

    [Description("@#world-space")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#mono")]
    Mono = 1,

    [Description("@#stereo")]
    Stereo = 2,

    [Description("@#stereo-left-right")]
    StereoLeftRight = 3,

    [Description("@#stereo-top-bottom")]
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
    [Description("@#default")]
    Default = 0,

    [Description("@#text-optimized")]
    TextOptimized = 1,

    [Description("@#graphics-optimized")]
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
    [Description("@#horizontal")]
    Horizontal = 0,

    [Description("@#vertical")]
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
    [Description("@#viewer")]
    Viewer = 0,

    [Description("@#local")]
    Local = 1,

    [Description("@#local-floor")]
    LocalFloor = 2,

    [Description("@#bounded-floor")]
    BoundedFloor = 3,

    [Description("@#unbounded")]
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
    [Description("@#srgba8")]
    Srgba8 = 0,

    [Description("@#rgba16f")]
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
    [Description("@#inline")]
    Inline = 0,

    [Description("@#immersive-vr")]
    ImmersiveVr = 1,

    [Description("@#immersive-ar")]
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
    [Description("@#gaze")]
    Gaze = 0,

    [Description("@#tracked-pointer")]
    TrackedPointer = 1,

    [Description("@#screen")]
    Screen = 2,

    [Description("@#transient-pointer")]
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
    [Description("@#texture")]
    Texture = 0,

    [Description("@#texture-array")]
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
    [Description("@#visible")]
    Visible = 0,

    [Description("@#visible-blurred")]
    VisibleBlurred = 1,

    [Description("@#hidden")]
    Hidden = 2
}
