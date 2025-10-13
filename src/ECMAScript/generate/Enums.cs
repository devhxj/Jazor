namespace ECMAScript;

/// <summary>
/// AccelerometerLocalCoordinateSystem
/// </summary>
[Description("@#AccelerometerLocalCoordinateSystem")]
[ECMAScript]
public enum AccelerometerLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// AudioSessionType
/// </summary>
[Description("@#AudioSessionType")]
[ECMAScript]
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
/// AudioSessionState
/// </summary>
[Description("@#AudioSessionState")]
[ECMAScript]
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
/// AutoplayPolicy
/// </summary>
[Description("@#AutoplayPolicy")]
[ECMAScript]
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
public enum AutoplayPolicyMediaType
{
    [Description("@#Mediaelement")]
    Mediaelement = 0,

    [Description("@#Audiocontext")]
    Audiocontext = 1
}

/// <summary>
/// BackgroundFetchResult
/// </summary>
[Description("@#BackgroundFetchResult")]
[ECMAScript]
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
/// BackgroundFetchFailureReason
/// </summary>
[Description("@#BackgroundFetchFailureReason")]
[ECMAScript]
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
/// PresentationStyle
/// </summary>
[Description("@#PresentationStyle")]
[ECMAScript]
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
/// CompressionFormat
/// </summary>
[Description("@#CompressionFormat")]
[ECMAScript]
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
/// PressureSource
/// </summary>
[Description("@#PressureSource")]
[ECMAScript]
public enum PressureSource
{
    [Description("@#Cpu")]
    Cpu = 0
}

/// <summary>
/// PressureState
/// </summary>
[Description("@#PressureState")]
[ECMAScript]
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
/// ContactProperty
/// </summary>
[Description("@#ContactProperty")]
[ECMAScript]
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
/// ScriptingPolicyViolationType
/// </summary>
[Description("@#ScriptingPolicyViolationType")]
[ECMAScript]
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
/// SecurityPolicyViolationEventDisposition
/// </summary>
[Description("@#SecurityPolicyViolationEventDisposition")]
[ECMAScript]
public enum SecurityPolicyViolationEventDisposition
{
    [Description("@#Enforce")]
    Enforce = 0,

    [Description("@#Report")]
    Report = 1
}

/// <summary>
/// FontFaceLoadStatus
/// </summary>
[Description("@#FontFaceLoadStatus")]
[ECMAScript]
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
public enum FontFaceSetLoadStatus
{
    [Description("@#Loading")]
    Loading = 0,

    [Description("@#Loaded")]
    Loaded = 1
}

/// <summary>
/// SpatialNavigationDirection
/// </summary>
[Description("@#SpatialNavigationDirection")]
[ECMAScript]
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
/// FocusableAreaSearchMode
/// </summary>
[Description("@#FocusableAreaSearchMode")]
[ECMAScript]
public enum FocusableAreaSearchMode
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#All")]
    All = 1
}

/// <summary>
/// ScrollBehavior
/// </summary>
[Description("@#ScrollBehavior")]
[ECMAScript]
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
/// ScrollIntoViewContainer
/// </summary>
[Description("@#ScrollIntoViewContainer")]
[ECMAScript]
public enum ScrollIntoViewContainer
{
    [Description("@#All")]
    All = 0,

    [Description("@#Nearest")]
    Nearest = 1
}

/// <summary>
/// CSSBoxType
/// </summary>
[Description("@#CSSBoxType")]
[ECMAScript]
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
/// DevicePostureType
/// </summary>
[Description("@#DevicePostureType")]
[ECMAScript]
public enum DevicePostureType
{
    [Description("@#Continuous")]
    Continuous = 0,

    [Description("@#Folded")]
    Folded = 1
}

/// <summary>
/// ItemType
/// </summary>
[Description("@#ItemType")]
[ECMAScript]
public enum ItemType
{
    [Description("@#Product")]
    Product = 0,

    [Description("@#Subscription")]
    Subscription = 1
}

/// <summary>
/// ShadowRootMode
/// </summary>
[Description("@#ShadowRootMode")]
[ECMAScript]
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
public enum SlotAssignmentMode
{
    [Description("@#Manual")]
    Manual = 0,

    [Description("@#Named")]
    Named = 1
}

/// <summary>
/// UnderlineStyle
/// </summary>
[Description("@#UnderlineStyle")]
[ECMAScript]
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
/// MediaKeysRequirement
/// </summary>
[Description("@#MediaKeysRequirement")]
[ECMAScript]
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
/// MediaKeySessionType
/// </summary>
[Description("@#MediaKeySessionType")]
[ECMAScript]
public enum MediaKeySessionType
{
    [Description("@#Temporary")]
    Temporary = 0,

    [Description("@#PersistentLicense")]
    PersistentLicense = 1
}

/// <summary>
/// MediaKeySessionClosedReason
/// </summary>
[Description("@#MediaKeySessionClosedReason")]
[ECMAScript]
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
/// MediaKeyStatus
/// </summary>
[Description("@#MediaKeyStatus")]
[ECMAScript]
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
/// MediaKeyMessageType
/// </summary>
[Description("@#MediaKeyMessageType")]
[ECMAScript]
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
/// IdentityCredentialRequestOptionsContext
/// </summary>
[Description("@#IdentityCredentialRequestOptionsContext")]
[ECMAScript]
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
/// IdentityCredentialRequestOptionsMode
/// </summary>
[Description("@#IdentityCredentialRequestOptionsMode")]
[ECMAScript]
public enum IdentityCredentialRequestOptionsMode
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Passive")]
    Passive = 1
}

/// <summary>
/// OpaqueProperty
/// </summary>
[Description("@#OpaqueProperty")]
[ECMAScript]
public enum OpaqueProperty
{
    [Description("@#Opaque")]
    Opaque = 0
}

/// <summary>
/// FenceReportingDestination
/// </summary>
[Description("@#FenceReportingDestination")]
[ECMAScript]
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
/// RequestDestination
/// </summary>
[Description("@#RequestDestination")]
[ECMAScript]
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
/// RequestMode
/// </summary>
[Description("@#RequestMode")]
[ECMAScript]
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
/// RequestCredentials
/// </summary>
[Description("@#RequestCredentials")]
[ECMAScript]
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
/// RequestCache
/// </summary>
[Description("@#RequestCache")]
[ECMAScript]
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
/// RequestRedirect
/// </summary>
[Description("@#RequestRedirect")]
[ECMAScript]
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
/// RequestDuplex
/// </summary>
[Description("@#RequestDuplex")]
[ECMAScript]
public enum RequestDuplex
{
    [Description("@#Half")]
    Half = 0
}

/// <summary>
/// RequestPriority
/// </summary>
[Description("@#RequestPriority")]
[ECMAScript]
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
/// ResponseType
/// </summary>
[Description("@#ResponseType")]
[ECMAScript]
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
/// FileSystemPermissionMode
/// </summary>
[Description("@#FileSystemPermissionMode")]
[ECMAScript]
public enum FileSystemPermissionMode
{
    [Description("@#Read")]
    Read = 0,

    [Description("@#Readwrite")]
    Readwrite = 1
}

/// <summary>
/// WellKnownDirectory
/// </summary>
[Description("@#WellKnownDirectory")]
[ECMAScript]
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
/// EndingType
/// </summary>
[Description("@#EndingType")]
[ECMAScript]
public enum EndingType
{
    [Description("@#Transparent")]
    Transparent = 0,

    [Description("@#Native")]
    Native = 1
}

/// <summary>
/// FileSystemHandleKind
/// </summary>
[Description("@#FileSystemHandleKind")]
[ECMAScript]
public enum FileSystemHandleKind
{
    [Description("@#File")]
    File = 0,

    [Description("@#Directory")]
    Directory = 1
}

/// <summary>
/// WriteCommandType
/// </summary>
[Description("@#WriteCommandType")]
[ECMAScript]
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
/// FullscreenNavigationUI
/// </summary>
[Description("@#FullscreenNavigationUI")]
[ECMAScript]
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
/// GamepadMappingType
/// </summary>
[Description("@#GamepadMappingType")]
[ECMAScript]
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
/// GamepadHapticsResult
/// </summary>
[Description("@#GamepadHapticsResult")]
[ECMAScript]
public enum GamepadHapticsResult
{
    [Description("@#Complete")]
    Complete = 0,

    [Description("@#Preempted")]
    Preempted = 1
}

/// <summary>
/// GamepadHapticEffectType
/// </summary>
[Description("@#GamepadHapticEffectType")]
[ECMAScript]
public enum GamepadHapticEffectType
{
    [Description("@#DualRumble")]
    DualRumble = 0,

    [Description("@#TriggerRumble")]
    TriggerRumble = 1
}

/// <summary>
/// GyroscopeLocalCoordinateSystem
/// </summary>
[Description("@#GyroscopeLocalCoordinateSystem")]
[ECMAScript]
public enum GyroscopeLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// HandwritingRecognitionType
/// </summary>
[Description("@#HandwritingRecognitionType")]
[ECMAScript]
public enum HandwritingRecognitionType
{
    [Description("@#Text")]
    Text = 0,

    [Description("@#PerCharacter")]
    PerCharacter = 1
}

/// <summary>
/// HandwritingInputType
/// </summary>
[Description("@#HandwritingInputType")]
[ECMAScript]
public enum HandwritingInputType
{
    [Description("@#Mouse")]
    Mouse = 0,

    [Description("@#Stylus")]
    Stylus = 1,

    [Description("@#Touch")]
    Touch = 2
}

/// <summary>
/// DocumentReadyState
/// </summary>
[Description("@#DocumentReadyState")]
[ECMAScript]
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
public enum DocumentVisibilityState
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#Hidden")]
    Hidden = 1
}

/// <summary>
/// CanPlayTypeResult
/// </summary>
[Description("@#CanPlayTypeResult")]
[ECMAScript]
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
/// TextTrackMode
/// </summary>
[Description("@#TextTrackMode")]
[ECMAScript]
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
/// TextTrackKind
/// </summary>
[Description("@#TextTrackKind")]
[ECMAScript]
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
/// SelectionMode
/// </summary>
[Description("@#SelectionMode")]
[ECMAScript]
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
/// PredefinedColorSpace
/// </summary>
[Description("@#PredefinedColorSpace")]
[ECMAScript]
public enum PredefinedColorSpace
{
    [Description("@#Srgb")]
    Srgb = 0,

    [Description("@#DisplayP3")]
    DisplayP3 = 1
}

/// <summary>
/// CanvasColorType
/// </summary>
[Description("@#CanvasColorType")]
[ECMAScript]
public enum CanvasColorType
{
    [Description("@#Unorm8")]
    Unorm8 = 0,

    [Description("@#Float16")]
    Float16 = 1
}

/// <summary>
/// CanvasFillRule
/// </summary>
[Description("@#CanvasFillRule")]
[ECMAScript]
public enum CanvasFillRule
{
    [Description("@#Nonzero")]
    Nonzero = 0,

    [Description("@#Evenodd")]
    Evenodd = 1
}

/// <summary>
/// ImageSmoothingQuality
/// </summary>
[Description("@#ImageSmoothingQuality")]
[ECMAScript]
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
/// CanvasLineCap
/// </summary>
[Description("@#CanvasLineCap")]
[ECMAScript]
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
/// CanvasDirection
/// </summary>
[Description("@#CanvasDirection")]
[ECMAScript]
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
/// CanvasFontKerning
/// </summary>
[Description("@#CanvasFontKerning")]
[ECMAScript]
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
/// CanvasTextRendering
/// </summary>
[Description("@#CanvasTextRendering")]
[ECMAScript]
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
/// OffscreenRenderingContextId
/// </summary>
[Description("@#OffscreenRenderingContextId")]
[ECMAScript]
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
/// ScrollRestoration
/// </summary>
[Description("@#ScrollRestoration")]
[ECMAScript]
public enum ScrollRestoration
{
    [Description("@#Auto")]
    Auto = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// NavigationHistoryBehavior
/// </summary>
[Description("@#NavigationHistoryBehavior")]
[ECMAScript]
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
/// NavigationType
/// </summary>
[Description("@#NavigationType")]
[ECMAScript]
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
/// NavigationFocusReset
/// </summary>
[Description("@#NavigationFocusReset")]
[ECMAScript]
public enum NavigationFocusReset
{
    [Description("@#AfterTransition")]
    AfterTransition = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// NavigationScrollBehavior
/// </summary>
[Description("@#NavigationScrollBehavior")]
[ECMAScript]
public enum NavigationScrollBehavior
{
    [Description("@#AfterTransition")]
    AfterTransition = 0,

    [Description("@#Manual")]
    Manual = 1
}

/// <summary>
/// DOMParserSupportedType
/// </summary>
[Description("@#DOMParserSupportedType")]
[ECMAScript]
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
/// ImageDataPixelFormat
/// </summary>
[Description("@#ImageDataPixelFormat")]
[ECMAScript]
public enum ImageDataPixelFormat
{
    [Description("@#RgbaUnorm8")]
    RgbaUnorm8 = 0,

    [Description("@#RgbaFloat16")]
    RgbaFloat16 = 1
}

/// <summary>
/// ImageOrientation
/// </summary>
[Description("@#ImageOrientation")]
[ECMAScript]
public enum ImageOrientation
{
    [Description("@#FromImage")]
    FromImage = 0,

    [Description("@#FlipY")]
    FlipY = 1
}

/// <summary>
/// PremultiplyAlpha
/// </summary>
[Description("@#PremultiplyAlpha")]
[ECMAScript]
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
/// ColorSpaceConversion
/// </summary>
[Description("@#ColorSpaceConversion")]
[ECMAScript]
public enum ColorSpaceConversion
{
    [Description("@#None")]
    None = 0,

    [Description("@#Default")]
    Default = 1
}

/// <summary>
/// ResizeQuality
/// </summary>
[Description("@#ResizeQuality")]
[ECMAScript]
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
/// WorkerType
/// </summary>
[Description("@#WorkerType")]
[ECMAScript]
public enum WorkerType
{
    [Description("@#Classic")]
    Classic = 0,

    [Description("@#Module")]
    Module = 1
}

/// <summary>
/// UserIdleState
/// </summary>
[Description("@#UserIdleState")]
[ECMAScript]
public enum UserIdleState
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Idle")]
    Idle = 1
}

/// <summary>
/// ScreenIdleState
/// </summary>
[Description("@#ScreenIdleState")]
[ECMAScript]
public enum ScreenIdleState
{
    [Description("@#Locked")]
    Locked = 0,

    [Description("@#Unlocked")]
    Unlocked = 1
}

/// <summary>
/// RedEyeReduction
/// </summary>
[Description("@#RedEyeReduction")]
[ECMAScript]
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
/// FillLightMode
/// </summary>
[Description("@#FillLightMode")]
[ECMAScript]
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
/// MeteringMode
/// </summary>
[Description("@#MeteringMode")]
[ECMAScript]
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
/// IDBRequestReadyState
/// </summary>
[Description("@#IDBRequestReadyState")]
[ECMAScript]
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
/// IDBCursorDirection
/// </summary>
[Description("@#IDBCursorDirection")]
[ECMAScript]
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
/// IDBTransactionMode
/// </summary>
[Description("@#IDBTransactionMode")]
[ECMAScript]
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
/// LoginStatus
/// </summary>
[Description("@#LoginStatus")]
[ECMAScript]
public enum LoginStatus
{
    [Description("@#LoggedIn")]
    LoggedIn = 0,

    [Description("@#LoggedOut")]
    LoggedOut = 1
}

/// <summary>
/// ScriptInvokerType
/// </summary>
[Description("@#ScriptInvokerType")]
[ECMAScript]
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
/// MagnetometerLocalCoordinateSystem
/// </summary>
[Description("@#MagnetometerLocalCoordinateSystem")]
[ECMAScript]
public enum MagnetometerLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// AppBannerPromptOutcome
/// </summary>
[Description("@#AppBannerPromptOutcome")]
[ECMAScript]
public enum AppBannerPromptOutcome
{
    [Description("@#Accepted")]
    Accepted = 0,

    [Description("@#Dismissed")]
    Dismissed = 1
}

/// <summary>
/// MediaDecodingType
/// </summary>
[Description("@#MediaDecodingType")]
[ECMAScript]
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
/// MediaEncodingType
/// </summary>
[Description("@#MediaEncodingType")]
[ECMAScript]
public enum MediaEncodingType
{
    [Description("@#Record")]
    Record = 0,

    [Description("@#Webrtc")]
    Webrtc = 1
}

/// <summary>
/// HdrMetadataType
/// </summary>
[Description("@#HdrMetadataType")]
[ECMAScript]
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
/// ColorGamut
/// </summary>
[Description("@#ColorGamut")]
[ECMAScript]
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
/// TransferFunction
/// </summary>
[Description("@#TransferFunction")]
[ECMAScript]
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
/// ReadyState
/// </summary>
[Description("@#ReadyState")]
[ECMAScript]
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
/// EndOfStreamError
/// </summary>
[Description("@#EndOfStreamError")]
[ECMAScript]
public enum EndOfStreamError
{
    [Description("@#Network")]
    Network = 0,

    [Description("@#Decode")]
    Decode = 1
}

/// <summary>
/// AppendMode
/// </summary>
[Description("@#AppendMode")]
[ECMAScript]
public enum AppendMode
{
    [Description("@#Segments")]
    Segments = 0,

    [Description("@#Sequence")]
    Sequence = 1
}

/// <summary>
/// MockCapturePromptResult
/// </summary>
[Description("@#MockCapturePromptResult")]
[ECMAScript]
public enum MockCapturePromptResult
{
    [Description("@#Granted")]
    Granted = 0,

    [Description("@#Denied")]
    Denied = 1
}

/// <summary>
/// CaptureAction
/// </summary>
[Description("@#CaptureAction")]
[ECMAScript]
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
/// MediaStreamTrackState
/// </summary>
[Description("@#MediaStreamTrackState")]
[ECMAScript]
public enum MediaStreamTrackState
{
    [Description("@#Live")]
    Live = 0,

    [Description("@#Ended")]
    Ended = 1
}

/// <summary>
/// VideoFacingModeEnum
/// </summary>
[Description("@#VideoFacingModeEnum")]
[ECMAScript]
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
/// VideoResizeModeEnum
/// </summary>
[Description("@#VideoResizeModeEnum")]
[ECMAScript]
public enum VideoResizeModeEnum
{
    [Description("@#None")]
    None = 0,

    [Description("@#CropAndScale")]
    CropAndScale = 1
}

/// <summary>
/// EchoCancellationModeEnum
/// </summary>
[Description("@#EchoCancellationModeEnum")]
[ECMAScript]
public enum EchoCancellationModeEnum
{
    [Description("@#All")]
    All = 0,

    [Description("@#RemoteOnly")]
    RemoteOnly = 1
}

/// <summary>
/// MediaDeviceKind
/// </summary>
[Description("@#MediaDeviceKind")]
[ECMAScript]
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
/// MediaSessionPlaybackState
/// </summary>
[Description("@#MediaSessionPlaybackState")]
[ECMAScript]
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
/// MediaSessionAction
/// </summary>
[Description("@#MediaSessionAction")]
[ECMAScript]
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

    [Description("@#Togglescreenshare")]
    Togglescreenshare = 11,

    [Description("@#Hangup")]
    Hangup = 12,

    [Description("@#Previousslide")]
    Previousslide = 13,

    [Description("@#Nextslide")]
    Nextslide = 14,

    [Description("@#Enterpictureinpicture")]
    Enterpictureinpicture = 15,

    [Description("@#Voiceactivity")]
    Voiceactivity = 16
}

/// <summary>
/// BitrateMode
/// </summary>
[Description("@#BitrateMode")]
[ECMAScript]
public enum BitrateMode
{
    [Description("@#Constant")]
    Constant = 0,

    [Description("@#Variable")]
    Variable = 1
}

/// <summary>
/// RecordingState
/// </summary>
[Description("@#RecordingState")]
[ECMAScript]
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
/// RTCDegradationPreference
/// </summary>
[Description("@#RTCDegradationPreference")]
[ECMAScript]
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
/// NavigationTimingType
/// </summary>
[Description("@#NavigationTimingType")]
[ECMAScript]
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
/// ConnectionType
/// </summary>
[Description("@#ConnectionType")]
[ECMAScript]
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
/// EffectiveConnectionType
/// </summary>
[Description("@#EffectiveConnectionType")]
[ECMAScript]
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
/// NotificationPermission
/// </summary>
[Description("@#NotificationPermission")]
[ECMAScript]
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
/// NotificationDirection
/// </summary>
[Description("@#NotificationDirection")]
[ECMAScript]
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
/// OrientationSensorLocalCoordinateSystem
/// </summary>
[Description("@#OrientationSensorLocalCoordinateSystem")]
[ECMAScript]
public enum OrientationSensorLocalCoordinateSystem
{
    [Description("@#Device")]
    Device = 0,

    [Description("@#Screen")]
    Screen = 1
}

/// <summary>
/// ClientLifecycleState
/// </summary>
[Description("@#ClientLifecycleState")]
[ECMAScript]
public enum ClientLifecycleState
{
    [Description("@#Active")]
    Active = 0,

    [Description("@#Frozen")]
    Frozen = 1
}

/// <summary>
/// PaymentDelegation
/// </summary>
[Description("@#PaymentDelegation")]
[ECMAScript]
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
/// PaymentComplete
/// </summary>
[Description("@#PaymentComplete")]
[ECMAScript]
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
/// PermissionState
/// </summary>
[Description("@#PermissionState")]
[ECMAScript]
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
/// PointerAxis
/// </summary>
[Description("@#PointerAxis")]
[ECMAScript]
public enum PointerAxis
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
/// PresentationConnectionState
/// </summary>
[Description("@#PresentationConnectionState")]
[ECMAScript]
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
/// PresentationConnectionCloseReason
/// </summary>
[Description("@#PresentationConnectionCloseReason")]
[ECMAScript]
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
/// PrivateAttributionAggregationProtocol
/// </summary>
[Description("@#PrivateAttributionAggregationProtocol")]
[ECMAScript]
public enum PrivateAttributionAggregationProtocol
{
    [Description("@#Dap12Histogram")]
    Dap12Histogram = 0,

    [Description("@#Tee00")]
    Tee00 = 1
}

/// <summary>
/// AttributionLogic
/// </summary>
[Description("@#AttributionLogic")]
[ECMAScript]
public enum AttributionLogic
{
    [Description("@#LastTouch")]
    LastTouch = 0
}

/// <summary>
/// IPAddressSpace
/// </summary>
[Description("@#IPAddressSpace")]
[ECMAScript]
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
/// PushEncryptionKeyName
/// </summary>
[Description("@#PushEncryptionKeyName")]
[ECMAScript]
public enum PushEncryptionKeyName
{
    [Description("@#P256dh")]
    P256dh = 0,

    [Description("@#Auth")]
    Auth = 1
}

/// <summary>
/// ReferrerPolicy
/// </summary>
[Description("@#ReferrerPolicy")]
[ECMAScript]
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
/// RemotePlaybackState
/// </summary>
[Description("@#RemotePlaybackState")]
[ECMAScript]
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
/// ResizeObserverBoxOptions
/// </summary>
[Description("@#ResizeObserverBoxOptions")]
[ECMAScript]
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
/// RenderBlockingStatusType
/// </summary>
[Description("@#RenderBlockingStatusType")]
[ECMAScript]
public enum RenderBlockingStatusType
{
    [Description("@#Blocking")]
    Blocking = 0,

    [Description("@#NonBlocking")]
    NonBlocking = 1
}

/// <summary>
/// SameSiteCookiesType
/// </summary>
[Description("@#SameSiteCookiesType")]
[ECMAScript]
public enum SameSiteCookiesType
{
    [Description("@#All")]
    All = 0,

    [Description("@#None")]
    None = 1
}

/// <summary>
/// SanitizerPresets
/// </summary>
[Description("@#SanitizerPresets")]
[ECMAScript]
public enum SanitizerPresets
{
    [Description("@#Default")]
    Default = 0
}

/// <summary>
/// TaskPriority
/// </summary>
[Description("@#TaskPriority")]
[ECMAScript]
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
/// CaptureStartFocusBehavior
/// </summary>
[Description("@#CaptureStartFocusBehavior")]
[ECMAScript]
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
/// SelfCapturePreferenceEnum
/// </summary>
[Description("@#SelfCapturePreferenceEnum")]
[ECMAScript]
public enum SelfCapturePreferenceEnum
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
public enum SystemAudioPreferenceEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// WindowAudioPreferenceEnum
/// </summary>
[Description("@#WindowAudioPreferenceEnum")]
[ECMAScript]
public enum WindowAudioPreferenceEnum
{
    [Description("@#System")]
    System = 0,

    [Description("@#Window")]
    Window = 1,

    [Description("@#Exclude")]
    Exclude = 2
}

/// <summary>
/// SurfaceSwitchingPreferenceEnum
/// </summary>
[Description("@#SurfaceSwitchingPreferenceEnum")]
[ECMAScript]
public enum SurfaceSwitchingPreferenceEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// MonitorTypeSurfacesEnum
/// </summary>
[Description("@#MonitorTypeSurfacesEnum")]
[ECMAScript]
public enum MonitorTypeSurfacesEnum
{
    [Description("@#Include")]
    Include = 0,

    [Description("@#Exclude")]
    Exclude = 1
}

/// <summary>
/// DisplayCaptureSurfaceType
/// </summary>
[Description("@#DisplayCaptureSurfaceType")]
[ECMAScript]
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
/// CursorCaptureConstraint
/// </summary>
[Description("@#CursorCaptureConstraint")]
[ECMAScript]
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
/// OrientationLockType
/// </summary>
[Description("@#OrientationLockType")]
[ECMAScript]
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
/// OrientationType
/// </summary>
[Description("@#OrientationType")]
[ECMAScript]
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
/// WakeLockType
/// </summary>
[Description("@#WakeLockType")]
[ECMAScript]
public enum WakeLockType
{
    [Description("@#Screen")]
    Screen = 0
}

/// <summary>
/// ScrollAxis
/// </summary>
[Description("@#ScrollAxis")]
[ECMAScript]
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
/// SecurePaymentConfirmationAvailability
/// </summary>
[Description("@#SecurePaymentConfirmationAvailability")]
[ECMAScript]
public enum SecurePaymentConfirmationAvailability
{
    [Description("@#Available")]
    Available = 0,

    [Description("@#UnavailableUnknownReason")]
    UnavailableUnknownReason = 1,

    [Description("@#UnavailableFeatureNotEnabled")]
    UnavailableFeatureNotEnabled = 2,

    [Description("@#UnavailableNoPermissionPolicy")]
    UnavailableNoPermissionPolicy = 3,

    [Description("@#UnavailableNoUserVerifyingPlatformAuthenticator")]
    UnavailableNoUserVerifyingPlatformAuthenticator = 4
}

/// <summary>
/// ParityType
/// </summary>
[Description("@#ParityType")]
[ECMAScript]
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
/// FlowControlType
/// </summary>
[Description("@#FlowControlType")]
[ECMAScript]
public enum FlowControlType
{
    [Description("@#None")]
    None = 0,

    [Description("@#Hardware")]
    Hardware = 1
}

/// <summary>
/// ServiceWorkerState
/// </summary>
[Description("@#ServiceWorkerState")]
[ECMAScript]
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
/// FrameType
/// </summary>
[Description("@#FrameType")]
[ECMAScript]
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
/// ClientType
/// </summary>
[Description("@#ClientType")]
[ECMAScript]
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
/// RunningStatus
/// </summary>
[Description("@#RunningStatus")]
[ECMAScript]
public enum RunningStatus
{
    [Description("@#Running")]
    Running = 0,

    [Description("@#NotRunning")]
    NotRunning = 1
}

/// <summary>
/// RouterSourceEnum
/// </summary>
[Description("@#RouterSourceEnum")]
[ECMAScript]
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
/// LandmarkType
/// </summary>
[Description("@#LandmarkType")]
[ECMAScript]
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
/// BarcodeFormat
/// </summary>
[Description("@#BarcodeFormat")]
[ECMAScript]
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
/// SpeechRecognitionErrorCode
/// </summary>
[Description("@#SpeechRecognitionErrorCode")]
[ECMAScript]
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

    [Description("@#LanguageNotSupported")]
    LanguageNotSupported = 6,

    [Description("@#PhrasesNotSupported")]
    PhrasesNotSupported = 7
}

/// <summary>
/// AvailabilityStatus
/// </summary>
[Description("@#AvailabilityStatus")]
[ECMAScript]
public enum AvailabilityStatus
{
    [Description("@#Unavailable")]
    Unavailable = 0,

    [Description("@#Downloadable")]
    Downloadable = 1,

    [Description("@#Downloading")]
    Downloading = 2,

    [Description("@#Available")]
    Available = 3
}

/// <summary>
/// SpeechSynthesisErrorCode
/// </summary>
[Description("@#SpeechSynthesisErrorCode")]
[ECMAScript]
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
/// ReadableStreamReaderMode
/// </summary>
[Description("@#ReadableStreamReaderMode")]
[ECMAScript]
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
public enum ReadableStreamType
{
    [Description("@#Bytes")]
    Bytes = 0
}

/// <summary>
/// TouchType
/// </summary>
[Description("@#TouchType")]
[ECMAScript]
public enum TouchType
{
    [Description("@#Direct")]
    Direct = 0,

    [Description("@#Stylus")]
    Stylus = 1
}

/// <summary>
/// RefreshPolicy
/// </summary>
[Description("@#RefreshPolicy")]
[ECMAScript]
public enum RefreshPolicy
{
    [Description("@#None")]
    None = 0,

    [Description("@#Refresh")]
    Refresh = 1
}

/// <summary>
/// TokenVersion
/// </summary>
[Description("@#TokenVersion")]
[ECMAScript]
public enum TokenVersion
{
    [Description("@#_1")]
    _1 = 0
}

/// <summary>
/// OperationType
/// </summary>
[Description("@#OperationType")]
[ECMAScript]
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
/// KAnonStatus
/// </summary>
[Description("@#KAnonStatus")]
[ECMAScript]
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
/// IterationCompositeOperation
/// </summary>
[Description("@#IterationCompositeOperation")]
[ECMAScript]
public enum IterationCompositeOperation
{
    [Description("@#Replace")]
    Replace = 0,

    [Description("@#Accumulate")]
    Accumulate = 1
}

/// <summary>
/// AnimationTriggerBehavior
/// </summary>
[Description("@#AnimationTriggerBehavior")]
[ECMAScript]
public enum AnimationTriggerBehavior
{
    [Description("@#Once")]
    Once = 0,

    [Description("@#Repeat")]
    Repeat = 1,

    [Description("@#Alternate")]
    Alternate = 2,

    [Description("@#State")]
    State = 3
}

/// <summary>
/// AnimationPlayState
/// </summary>
[Description("@#AnimationPlayState")]
[ECMAScript]
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
/// FillMode
/// </summary>
[Description("@#FillMode")]
[ECMAScript]
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
/// PlaybackDirection
/// </summary>
[Description("@#PlaybackDirection")]
[ECMAScript]
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
/// CompositeOperation
/// </summary>
[Description("@#CompositeOperation")]
[ECMAScript]
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
/// LockMode
/// </summary>
[Description("@#LockMode")]
[ECMAScript]
public enum LockMode
{
    [Description("@#Shared")]
    Shared = 0,

    [Description("@#Exclusive")]
    Exclusive = 1
}

/// <summary>
/// OTPCredentialTransportType
/// </summary>
[Description("@#OTPCredentialTransportType")]
[ECMAScript]
public enum OTPCredentialTransportType
{
    [Description("@#Sms")]
    Sms = 0
}

/// <summary>
/// AudioContextState
/// </summary>
[Description("@#AudioContextState")]
[ECMAScript]
public enum AudioContextState
{
    [Description("@#Suspended")]
    Suspended = 0,

    [Description("@#Running")]
    Running = 1,

    [Description("@#Closed")]
    Closed = 2,

    [Description("@#Interrupted")]
    Interrupted = 3
}

/// <summary>
/// AudioContextRenderSizeCategory
/// </summary>
[Description("@#AudioContextRenderSizeCategory")]
[ECMAScript]
public enum AudioContextRenderSizeCategory
{
    [Description("@#Default")]
    Default = 0,

    [Description("@#Hardware")]
    Hardware = 1
}

/// <summary>
/// AudioContextLatencyCategory
/// </summary>
[Description("@#AudioContextLatencyCategory")]
[ECMAScript]
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
/// AudioSinkType
/// </summary>
[Description("@#AudioSinkType")]
[ECMAScript]
public enum AudioSinkType
{
    [Description("@#None")]
    None = 0
}

/// <summary>
/// ChannelCountMode
/// </summary>
[Description("@#ChannelCountMode")]
[ECMAScript]
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
public enum ChannelInterpretation
{
    [Description("@#Speakers")]
    Speakers = 0,

    [Description("@#Discrete")]
    Discrete = 1
}

/// <summary>
/// AutomationRate
/// </summary>
[Description("@#AutomationRate")]
[ECMAScript]
public enum AutomationRate
{
    [Description("@#ARate")]
    ARate = 0,

    [Description("@#KRate")]
    KRate = 1
}

/// <summary>
/// BiquadFilterType
/// </summary>
[Description("@#BiquadFilterType")]
[ECMAScript]
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
/// OscillatorType
/// </summary>
[Description("@#OscillatorType")]
[ECMAScript]
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
/// PanningModelType
/// </summary>
[Description("@#PanningModelType")]
[ECMAScript]
public enum PanningModelType
{
    [Description("@#Equalpower")]
    Equalpower = 0,

    [Description("@#HRTF")]
    HRTF = 1
}

/// <summary>
/// DistanceModelType
/// </summary>
[Description("@#DistanceModelType")]
[ECMAScript]
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
/// OverSampleType
/// </summary>
[Description("@#OverSampleType")]
[ECMAScript]
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
/// AuthenticatorAttachment
/// </summary>
[Description("@#AuthenticatorAttachment")]
[ECMAScript]
public enum AuthenticatorAttachment
{
    [Description("@#Platform")]
    Platform = 0,

    [Description("@#CrossPlatform")]
    CrossPlatform = 1
}

/// <summary>
/// ResidentKeyRequirement
/// </summary>
[Description("@#ResidentKeyRequirement")]
[ECMAScript]
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
/// AttestationConveyancePreference
/// </summary>
[Description("@#AttestationConveyancePreference")]
[ECMAScript]
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
/// TokenBindingStatus
/// </summary>
[Description("@#TokenBindingStatus")]
[ECMAScript]
public enum TokenBindingStatus
{
    [Description("@#Present")]
    Present = 0,

    [Description("@#Supported")]
    Supported = 1
}

/// <summary>
/// PublicKeyCredentialType
/// </summary>
[Description("@#PublicKeyCredentialType")]
[ECMAScript]
public enum PublicKeyCredentialType
{
    [Description("@#PublicKey")]
    PublicKey = 0
}

/// <summary>
/// AuthenticatorTransport
/// </summary>
[Description("@#AuthenticatorTransport")]
[ECMAScript]
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
/// UserVerificationRequirement
/// </summary>
[Description("@#UserVerificationRequirement")]
[ECMAScript]
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
/// ClientCapability
/// </summary>
[Description("@#ClientCapability")]
[ECMAScript]
public enum ClientCapability
{
    [Description("@#ConditionalCreate")]
    ConditionalCreate = 0,

    [Description("@#ConditionalGet")]
    ConditionalGet = 1,

    [Description("@#HybridTransport")]
    HybridTransport = 2,

    [Description("@#PasskeyPlatformAuthenticator")]
    PasskeyPlatformAuthenticator = 3,

    [Description("@#UserVerifyingPlatformAuthenticator")]
    UserVerifyingPlatformAuthenticator = 4,

    [Description("@#RelatedOrigins")]
    RelatedOrigins = 5,

    [Description("@#SignalAllAcceptedCredentials")]
    SignalAllAcceptedCredentials = 6,

    [Description("@#SignalCurrentUserDetails")]
    SignalCurrentUserDetails = 7,

    [Description("@#SignalUnknownCredential")]
    SignalUnknownCredential = 8
}

/// <summary>
/// PublicKeyCredentialHint
/// </summary>
[Description("@#PublicKeyCredentialHint")]
[ECMAScript]
public enum PublicKeyCredentialHint
{
    [Description("@#SecurityKey")]
    SecurityKey = 0,

    [Description("@#ClientDevice")]
    ClientDevice = 1,

    [Description("@#Hybrid")]
    Hybrid = 2
}

/// <summary>
/// LargeBlobSupport
/// </summary>
[Description("@#LargeBlobSupport")]
[ECMAScript]
public enum LargeBlobSupport
{
    [Description("@#Required")]
    Required = 0,

    [Description("@#Preferred")]
    Preferred = 1
}

/// <summary>
/// AacBitstreamFormat
/// </summary>
[Description("@#AacBitstreamFormat")]
[ECMAScript]
public enum AacBitstreamFormat
{
    [Description("@#Aac")]
    Aac = 0,

    [Description("@#Adts")]
    Adts = 1
}

/// <summary>
/// AvcBitstreamFormat
/// </summary>
[Description("@#AvcBitstreamFormat")]
[ECMAScript]
public enum AvcBitstreamFormat
{
    [Description("@#Annexb")]
    Annexb = 0,

    [Description("@#Avc")]
    Avc = 1
}

/// <summary>
/// HevcBitstreamFormat
/// </summary>
[Description("@#HevcBitstreamFormat")]
[ECMAScript]
public enum HevcBitstreamFormat
{
    [Description("@#Annexb")]
    Annexb = 0,

    [Description("@#Hevc")]
    Hevc = 1
}

/// <summary>
/// OpusBitstreamFormat
/// </summary>
[Description("@#OpusBitstreamFormat")]
[ECMAScript]
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
/// OpusApplication
/// </summary>
[Description("@#OpusApplication")]
[ECMAScript]
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
/// HardwareAcceleration
/// </summary>
[Description("@#HardwareAcceleration")]
[ECMAScript]
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
/// AlphaOption
/// </summary>
[Description("@#AlphaOption")]
[ECMAScript]
public enum AlphaOption
{
    [Description("@#Keep")]
    Keep = 0,

    [Description("@#Discard")]
    Discard = 1
}

/// <summary>
/// LatencyMode
/// </summary>
[Description("@#LatencyMode")]
[ECMAScript]
public enum LatencyMode
{
    [Description("@#Quality")]
    Quality = 0,

    [Description("@#Realtime")]
    Realtime = 1
}

/// <summary>
/// VideoEncoderBitrateMode
/// </summary>
[Description("@#VideoEncoderBitrateMode")]
[ECMAScript]
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
/// CodecState
/// </summary>
[Description("@#CodecState")]
[ECMAScript]
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
/// EncodedAudioChunkType
/// </summary>
[Description("@#EncodedAudioChunkType")]
[ECMAScript]
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
public enum EncodedVideoChunkType
{
    [Description("@#Key")]
    Key = 0,

    [Description("@#Delta")]
    Delta = 1
}

/// <summary>
/// AudioSampleFormat
/// </summary>
[Description("@#AudioSampleFormat")]
[ECMAScript]
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
/// VideoPixelFormat
/// </summary>
[Description("@#VideoPixelFormat")]
[ECMAScript]
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
/// VideoColorPrimaries
/// </summary>
[Description("@#VideoColorPrimaries")]
[ECMAScript]
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
/// VideoTransferCharacteristics
/// </summary>
[Description("@#VideoTransferCharacteristics")]
[ECMAScript]
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
/// VideoMatrixCoefficients
/// </summary>
[Description("@#VideoMatrixCoefficients")]
[ECMAScript]
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
/// KeyType
/// </summary>
[Description("@#KeyType")]
[ECMAScript]
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
/// KeyFormat
/// </summary>
[Description("@#KeyFormat")]
[ECMAScript]
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
/// WebGLPowerPreference
/// </summary>
[Description("@#WebGLPowerPreference")]
[ECMAScript]
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
/// HIDUnitSystem
/// </summary>
[Description("@#HIDUnitSystem")]
[ECMAScript]
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
/// MIDIPortType
/// </summary>
[Description("@#MIDIPortType")]
[ECMAScript]
public enum MIDIPortType
{
    [Description("@#Input")]
    Input = 0,

    [Description("@#Output")]
    Output = 1
}

/// <summary>
/// MIDIPortDeviceState
/// </summary>
[Description("@#MIDIPortDeviceState")]
[ECMAScript]
public enum MIDIPortDeviceState
{
    [Description("@#Disconnected")]
    Disconnected = 0,

    [Description("@#Connected")]
    Connected = 1
}

/// <summary>
/// MIDIPortConnectionState
/// </summary>
[Description("@#MIDIPortConnectionState")]
[ECMAScript]
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
/// MLPowerPreference
/// </summary>
[Description("@#MLPowerPreference")]
[ECMAScript]
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
/// MLInputOperandLayout
/// </summary>
[Description("@#MLInputOperandLayout")]
[ECMAScript]
public enum MLInputOperandLayout
{
    [Description("@#Nchw")]
    Nchw = 0,

    [Description("@#Nhwc")]
    Nhwc = 1
}

/// <summary>
/// MLOperandDataType
/// </summary>
[Description("@#MLOperandDataType")]
[ECMAScript]
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
/// MLConv2dFilterOperandLayout
/// </summary>
[Description("@#MLConv2dFilterOperandLayout")]
[ECMAScript]
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
/// MLGruWeightLayout
/// </summary>
[Description("@#MLGruWeightLayout")]
[ECMAScript]
public enum MLGruWeightLayout
{
    [Description("@#Zrn")]
    Zrn = 0,

    [Description("@#Rzn")]
    Rzn = 1
}

/// <summary>
/// MLRecurrentNetworkActivation
/// </summary>
[Description("@#MLRecurrentNetworkActivation")]
[ECMAScript]
public enum MLRecurrentNetworkActivation
{
    [Description("@#Relu")]
    Relu = 0,

    [Description("@#Sigmoid")]
    Sigmoid = 1,

    [Description("@#Tanh")]
    Tanh = 2
}

/// <summary>
/// MLRecurrentNetworkDirection
/// </summary>
[Description("@#MLRecurrentNetworkDirection")]
[ECMAScript]
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
/// MLLstmWeightLayout
/// </summary>
[Description("@#MLLstmWeightLayout")]
[ECMAScript]
public enum MLLstmWeightLayout
{
    [Description("@#Iofg")]
    Iofg = 0,

    [Description("@#Ifgo")]
    Ifgo = 1
}

/// <summary>
/// MLPaddingMode
/// </summary>
[Description("@#MLPaddingMode")]
[ECMAScript]
public enum MLPaddingMode
{
    [Description("@#Constant")]
    Constant = 0,

    [Description("@#Edge")]
    Edge = 1,

    [Description("@#Reflection")]
    Reflection = 2
}

/// <summary>
/// MLRoundingType
/// </summary>
[Description("@#MLRoundingType")]
[ECMAScript]
public enum MLRoundingType
{
    [Description("@#Floor")]
    Floor = 0,

    [Description("@#Ceil")]
    Ceil = 1
}

/// <summary>
/// MLInterpolationMode
/// </summary>
[Description("@#MLInterpolationMode")]
[ECMAScript]
public enum MLInterpolationMode
{
    [Description("@#NearestNeighbor")]
    NearestNeighbor = 0,

    [Description("@#Linear")]
    Linear = 1
}

/// <summary>
/// SFrameTransformRole
/// </summary>
[Description("@#SFrameTransformRole")]
[ECMAScript]
public enum SFrameTransformRole
{
    [Description("@#Encrypt")]
    Encrypt = 0,

    [Description("@#Decrypt")]
    Decrypt = 1
}

/// <summary>
/// SFrameTransformErrorEventType
/// </summary>
[Description("@#SFrameTransformErrorEventType")]
[ECMAScript]
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
/// RTCEncodedVideoFrameType
/// </summary>
[Description("@#RTCEncodedVideoFrameType")]
[ECMAScript]
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
/// RTCErrorDetailTypeIdp
/// </summary>
[Description("@#RTCErrorDetailTypeIdp")]
[ECMAScript]
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
/// RTCPriorityType
/// </summary>
[Description("@#RTCPriorityType")]
[ECMAScript]
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
/// RTCStatsType
/// </summary>
[Description("@#RTCStatsType")]
[ECMAScript]
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
/// RTCQualityLimitationReason
/// </summary>
[Description("@#RTCQualityLimitationReason")]
[ECMAScript]
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
/// RTCDtlsRole
/// </summary>
[Description("@#RTCDtlsRole")]
[ECMAScript]
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
/// RTCStatsIceCandidatePairState
/// </summary>
[Description("@#RTCStatsIceCandidatePairState")]
[ECMAScript]
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
/// RTCIceTransportPolicy
/// </summary>
[Description("@#RTCIceTransportPolicy")]
[ECMAScript]
public enum RTCIceTransportPolicy
{
    [Description("@#Relay")]
    Relay = 0,

    [Description("@#All")]
    All = 1
}

/// <summary>
/// RTCBundlePolicy
/// </summary>
[Description("@#RTCBundlePolicy")]
[ECMAScript]
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
/// RTCRtcpMuxPolicy
/// </summary>
[Description("@#RTCRtcpMuxPolicy")]
[ECMAScript]
public enum RTCRtcpMuxPolicy
{
    [Description("@#Require")]
    Require = 0
}

/// <summary>
/// RTCSignalingState
/// </summary>
[Description("@#RTCSignalingState")]
[ECMAScript]
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
/// RTCIceGatheringState
/// </summary>
[Description("@#RTCIceGatheringState")]
[ECMAScript]
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
/// RTCPeerConnectionState
/// </summary>
[Description("@#RTCPeerConnectionState")]
[ECMAScript]
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
/// RTCIceConnectionState
/// </summary>
[Description("@#RTCIceConnectionState")]
[ECMAScript]
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
/// RTCSdpType
/// </summary>
[Description("@#RTCSdpType")]
[ECMAScript]
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
/// RTCIceProtocol
/// </summary>
[Description("@#RTCIceProtocol")]
[ECMAScript]
public enum RTCIceProtocol
{
    [Description("@#Udp")]
    Udp = 0,

    [Description("@#Tcp")]
    Tcp = 1
}

/// <summary>
/// RTCIceTcpCandidateType
/// </summary>
[Description("@#RTCIceTcpCandidateType")]
[ECMAScript]
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
/// RTCIceCandidateType
/// </summary>
[Description("@#RTCIceCandidateType")]
[ECMAScript]
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
/// RTCIceServerTransportProtocol
/// </summary>
[Description("@#RTCIceServerTransportProtocol")]
[ECMAScript]
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
/// RTCRtpTransceiverDirection
/// </summary>
[Description("@#RTCRtpTransceiverDirection")]
[ECMAScript]
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
/// RTCDtlsTransportState
/// </summary>
[Description("@#RTCDtlsTransportState")]
[ECMAScript]
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
/// RTCIceGathererState
/// </summary>
[Description("@#RTCIceGathererState")]
[ECMAScript]
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
/// RTCIceTransportState
/// </summary>
[Description("@#RTCIceTransportState")]
[ECMAScript]
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
/// RTCIceRole
/// </summary>
[Description("@#RTCIceRole")]
[ECMAScript]
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
/// RTCIceComponent
/// </summary>
[Description("@#RTCIceComponent")]
[ECMAScript]
public enum RTCIceComponent
{
    [Description("@#Rtp")]
    Rtp = 0,

    [Description("@#Rtcp")]
    Rtcp = 1
}

/// <summary>
/// RTCSctpTransportState
/// </summary>
[Description("@#RTCSctpTransportState")]
[ECMAScript]
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
/// RTCDataChannelState
/// </summary>
[Description("@#RTCDataChannelState")]
[ECMAScript]
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
/// RTCErrorDetailType
/// </summary>
[Description("@#RTCErrorDetailType")]
[ECMAScript]
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
/// BinaryType
/// </summary>
[Description("@#BinaryType")]
[ECMAScript]
public enum BinaryType
{
    [Description("@#Blob")]
    Blob = 0,

    [Description("@#Arraybuffer")]
    Arraybuffer = 1
}

/// <summary>
/// WebTransportReliabilityMode
/// </summary>
[Description("@#WebTransportReliabilityMode")]
[ECMAScript]
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
/// WebTransportCongestionControl
/// </summary>
[Description("@#WebTransportCongestionControl")]
[ECMAScript]
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
public enum WebTransportErrorSource
{
    [Description("@#Stream")]
    Stream = 0,

    [Description("@#Session")]
    Session = 1
}

/// <summary>
/// USBTransferStatus
/// </summary>
[Description("@#USBTransferStatus")]
[ECMAScript]
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
/// USBRequestType
/// </summary>
[Description("@#USBRequestType")]
[ECMAScript]
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
/// USBRecipient
/// </summary>
[Description("@#USBRecipient")]
[ECMAScript]
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
/// USBDirection
/// </summary>
[Description("@#USBDirection")]
[ECMAScript]
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
/// AutoKeyword
/// </summary>
[Description("@#AutoKeyword")]
[ECMAScript]
public enum AutoKeyword
{
    [Description("@#Auto")]
    Auto = 0
}

/// <summary>
/// DirectionSetting
/// </summary>
[Description("@#DirectionSetting")]
[ECMAScript]
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
/// LineAlignSetting
/// </summary>
[Description("@#LineAlignSetting")]
[ECMAScript]
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
/// PositionAlignSetting
/// </summary>
[Description("@#PositionAlignSetting")]
[ECMAScript]
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
/// AlignSetting
/// </summary>
[Description("@#AlignSetting")]
[ECMAScript]
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
/// ScrollSetting
/// </summary>
[Description("@#ScrollSetting")]
[ECMAScript]
public enum ScrollSetting
{
    [Description("@#Empty")]
    Empty = 0,

    [Description("@#Up")]
    Up = 1
}

/// <summary>
/// XREnvironmentBlendMode
/// </summary>
[Description("@#XREnvironmentBlendMode")]
[ECMAScript]
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
/// XRInteractionMode
/// </summary>
[Description("@#XRInteractionMode")]
[ECMAScript]
public enum XRInteractionMode
{
    [Description("@#ScreenSpace")]
    ScreenSpace = 0,

    [Description("@#WorldSpace")]
    WorldSpace = 1
}

/// <summary>
/// XRDepthType
/// </summary>
[Description("@#XRDepthType")]
[ECMAScript]
public enum XRDepthType
{
    [Description("@#Raw")]
    Raw = 0,

    [Description("@#Smooth")]
    Smooth = 1
}

/// <summary>
/// XRDepthUsage
/// </summary>
[Description("@#XRDepthUsage")]
[ECMAScript]
public enum XRDepthUsage
{
    [Description("@#CpuOptimized")]
    CpuOptimized = 0,

    [Description("@#GpuOptimized")]
    GpuOptimized = 1
}

/// <summary>
/// XRDepthDataFormat
/// </summary>
[Description("@#XRDepthDataFormat")]
[ECMAScript]
public enum XRDepthDataFormat
{
    [Description("@#LuminanceAlpha")]
    LuminanceAlpha = 0,

    [Description("@#Float32")]
    Float32 = 1,

    [Description("@#UnsignedShort")]
    UnsignedShort = 2
}

/// <summary>
/// XRDOMOverlayType
/// </summary>
[Description("@#XRDOMOverlayType")]
[ECMAScript]
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
/// XRHandJoint
/// </summary>
[Description("@#XRHandJoint")]
[ECMAScript]
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
/// XRHitTestTrackableType
/// </summary>
[Description("@#XRHitTestTrackableType")]
[ECMAScript]
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
/// XRReflectionFormat
/// </summary>
[Description("@#XRReflectionFormat")]
[ECMAScript]
public enum XRReflectionFormat
{
    [Description("@#Srgba8")]
    Srgba8 = 0,

    [Description("@#Rgba16f")]
    Rgba16f = 1
}

/// <summary>
/// XRPlaneOrientation
/// </summary>
[Description("@#XRPlaneOrientation")]
[ECMAScript]
public enum XRPlaneOrientation
{
    [Description("@#Horizontal")]
    Horizontal = 0,

    [Description("@#Vertical")]
    Vertical = 1
}

/// <summary>
/// XRSessionMode
/// </summary>
[Description("@#XRSessionMode")]
[ECMAScript]
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
/// XRVisibilityState
/// </summary>
[Description("@#XRVisibilityState")]
[ECMAScript]
public enum XRVisibilityState
{
    [Description("@#Visible")]
    Visible = 0,

    [Description("@#VisibleBlurred")]
    VisibleBlurred = 1,

    [Description("@#Hidden")]
    Hidden = 2
}

/// <summary>
/// XRReferenceSpaceType
/// </summary>
[Description("@#XRReferenceSpaceType")]
[ECMAScript]
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
/// XREye
/// </summary>
[Description("@#XREye")]
[ECMAScript]
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
/// XRHandedness
/// </summary>
[Description("@#XRHandedness")]
[ECMAScript]
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
/// XRTargetRayMode
/// </summary>
[Description("@#XRTargetRayMode")]
[ECMAScript]
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
/// XRLayerLayout
/// </summary>
[Description("@#XRLayerLayout")]
[ECMAScript]
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
/// XRTextureType
/// </summary>
[Description("@#XRTextureType")]
[ECMAScript]
public enum XRTextureType
{
    [Description("@#Texture")]
    Texture = 0,

    [Description("@#TextureArray")]
    TextureArray = 1
}

/// <summary>
/// SummarizerType
/// </summary>
[Description("@#SummarizerType")]
[ECMAScript]
public enum SummarizerType
{
    [Description("@#Tldr")]
    Tldr = 0,

    [Description("@#Teaser")]
    Teaser = 1,

    [Description("@#KeyPoints")]
    KeyPoints = 2,

    [Description("@#Headline")]
    Headline = 3
}

/// <summary>
/// SummarizerFormat
/// </summary>
[Description("@#SummarizerFormat")]
[ECMAScript]
public enum SummarizerFormat
{
    [Description("@#PlainText")]
    PlainText = 0,

    [Description("@#Markdown")]
    Markdown = 1
}

/// <summary>
/// SummarizerLength
/// </summary>
[Description("@#SummarizerLength")]
[ECMAScript]
public enum SummarizerLength
{
    [Description("@#Short")]
    Short = 0,

    [Description("@#Medium")]
    Medium = 1,

    [Description("@#Long")]
    Long = 2
}

/// <summary>
/// WriterTone
/// </summary>
[Description("@#WriterTone")]
[ECMAScript]
public enum WriterTone
{
    [Description("@#Formal")]
    Formal = 0,

    [Description("@#Neutral")]
    Neutral = 1,

    [Description("@#Casual")]
    Casual = 2
}

/// <summary>
/// WriterFormat
/// </summary>
[Description("@#WriterFormat")]
[ECMAScript]
public enum WriterFormat
{
    [Description("@#PlainText")]
    PlainText = 0,

    [Description("@#Markdown")]
    Markdown = 1
}

/// <summary>
/// WriterLength
/// </summary>
[Description("@#WriterLength")]
[ECMAScript]
public enum WriterLength
{
    [Description("@#Short")]
    Short = 0,

    [Description("@#Medium")]
    Medium = 1,

    [Description("@#Long")]
    Long = 2
}

/// <summary>
/// RewriterTone
/// </summary>
[Description("@#RewriterTone")]
[ECMAScript]
public enum RewriterTone
{
    [Description("@#AsIs")]
    AsIs = 0,

    [Description("@#MoreFormal")]
    MoreFormal = 1,

    [Description("@#MoreCasual")]
    MoreCasual = 2
}

/// <summary>
/// RewriterFormat
/// </summary>
[Description("@#RewriterFormat")]
[ECMAScript]
public enum RewriterFormat
{
    [Description("@#AsIs")]
    AsIs = 0,

    [Description("@#PlainText")]
    PlainText = 1,

    [Description("@#Markdown")]
    Markdown = 2
}

/// <summary>
/// RewriterLength
/// </summary>
[Description("@#RewriterLength")]
[ECMAScript]
public enum RewriterLength
{
    [Description("@#AsIs")]
    AsIs = 0,

    [Description("@#Shorter")]
    Shorter = 1,

    [Description("@#Longer")]
    Longer = 2
}

/// <summary>
/// Availability
/// </summary>
[Description("@#Availability")]
[ECMAScript]
public enum Availability
{
    [Description("@#Unavailable")]
    Unavailable = 0,

    [Description("@#Downloadable")]
    Downloadable = 1,

    [Description("@#Downloading")]
    Downloading = 2,

    [Description("@#Available")]
    Available = 3
}

/// <summary>
/// XMLHttpRequestResponseType
/// </summary>
[Description("@#XMLHttpRequestResponseType")]
[ECMAScript]
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