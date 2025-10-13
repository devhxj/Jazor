namespace ECMAScript;

/// <summary>
/// AudioSession
/// </summary>
[ECMAScript]
[Description("@#AudioSession")]
public class AudioSession : EventTarget
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern AudioSessionType Type { get; set; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern AudioSessionState State { get; }

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }
}

/// <summary>
/// Navigator
/// </summary>
[ECMAScript]
[Description("@#Navigator")]
public partial class Navigator
{
    /// <summary>
    /// audioSession
    /// </summary>
    [Description("@#audioSession")]
    public extern AudioSession AudioSession { get; }

    /// <summary>
    /// getAutoplayPolicy
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#getAutoplayPolicy")]
    public extern AutoplayPolicy GetAutoplayPolicy(AutoplayPolicyMediaType type);

    /// <summary>
    /// getAutoplayPolicy
    /// </summary>
    /// <param name="element">element</param>
    [Description("@#getAutoplayPolicy")]
    public extern AutoplayPolicy GetAutoplayPolicy(HTMLMediaElement element);

    /// <summary>
    /// getAutoplayPolicy
    /// </summary>
    /// <param name="context">context</param>
    [Description("@#getAutoplayPolicy")]
    public extern AutoplayPolicy GetAutoplayPolicy(AudioContext context);

    /// <summary>
    /// getBattery
    /// </summary>
    [Description("@#getBattery")]
    public extern PromiseResult<BatteryManager> GetBattery();

    /// <summary>
    /// sendBeacon
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="data">data</param>
    [Description("@#sendBeacon")]
    public extern bool SendBeacon(string url, BodyInit? data = null);

    /// <summary>
    /// clipboard
    /// </summary>
    [Description("@#clipboard")]
    public extern Clipboard Clipboard { get; }

    /// <summary>
    /// contacts
    /// </summary>
    [Description("@#contacts")]
    public extern ContactsManager Contacts { get; }

    /// <summary>
    /// credentials
    /// </summary>
    [Description("@#credentials")]
    public extern CredentialsContainer Credentials { get; }

    /// <summary>
    /// devicePosture
    /// </summary>
    [Description("@#devicePosture")]
    public extern DevicePosture DevicePosture { get; }

    /// <summary>
    /// requestMediaKeySystemAccess
    /// </summary>
    /// <param name="keySystem">keySystem</param>
    /// <param name="supportedConfigurations">supportedConfigurations</param>
    [Description("@#requestMediaKeySystemAccess")]
    public extern PromiseResult<MediaKeySystemAccess> RequestMediaKeySystemAccess(string keySystem, MediaKeySystemConfiguration[] supportedConfigurations);

    /// <summary>
    /// deprecatedReplaceInURN
    /// </summary>
    /// <param name="urnOrConfig">urnOrConfig</param>
    /// <param name="replacements">replacements</param>
    [Description("@#deprecatedReplaceInURN")]
    public extern PromiseResult<object> DeprecatedReplaceInURN(UrnOrConfig urnOrConfig, Dictionary<string, string> replacements);

    /// <summary>
    /// deprecatedURNtoURL
    /// </summary>
    /// <param name="urnOrConfig">urnOrConfig</param>
    /// <param name="sendReports">send_reports</param>
    [Description("@#deprecatedURNtoURL")]
    public extern PromiseResult<string> DeprecatedURNtoURL(UrnOrConfig urnOrConfig, bool sendReports = false);

    /// <summary>
    /// adAuctionComponents
    /// </summary>
    /// <param name="numAdComponents">numAdComponents</param>
    [Description("@#adAuctionComponents")]
    public extern string[] AdAuctionComponents(ushort numAdComponents);

    /// <summary>
    /// getGamepads
    /// </summary>
    [Description("@#getGamepads")]
    public extern Gamepad?[] GetGamepads();

    /// <summary>
    /// geolocation
    /// </summary>
    [Description("@#geolocation")]
    public extern Geolocation Geolocation { get; }

    /// <summary>
    /// getInstalledRelatedApps
    /// </summary>
    [Description("@#getInstalledRelatedApps")]
    public extern PromiseResult<RelatedApplication[]> GetInstalledRelatedApps();

    /// <summary>
    /// queryHandwritingRecognizer
    /// </summary>
    /// <param name="constraint">constraint</param>
    [Description("@#queryHandwritingRecognizer")]
    public extern PromiseResult<HandwritingRecognizerQueryResult?> QueryHandwritingRecognizer(HandwritingModelConstraint constraint);

    /// <summary>
    /// createHandwritingRecognizer
    /// </summary>
    /// <param name="constraint">constraint</param>
    [Description("@#createHandwritingRecognizer")]
    public extern PromiseResult<HandwritingRecognizer> CreateHandwritingRecognizer(HandwritingModelConstraint constraint);

    /// <summary>
    /// userActivation
    /// </summary>
    [Description("@#userActivation")]
    public extern UserActivation UserActivation { get; }

    /// <summary>
    /// ink
    /// </summary>
    [Description("@#ink")]
    public extern Ink Ink { get; }

    /// <summary>
    /// scheduling
    /// </summary>
    [Description("@#scheduling")]
    public extern Scheduling Scheduling { get; }

    /// <summary>
    /// keyboard
    /// </summary>
    [Description("@#keyboard")]
    public extern Keyboard Keyboard { get; }

    /// <summary>
    /// login
    /// </summary>
    [Description("@#login")]
    public extern NavigatorLogin Login { get; }

    /// <summary>
    /// managed
    /// </summary>
    [Description("@#managed")]
    public extern NavigatorManagedData Managed { get; }

    /// <summary>
    /// mediaCapabilities
    /// </summary>
    [Description("@#mediaCapabilities")]
    public extern MediaCapabilities MediaCapabilities { get; }

    /// <summary>
    /// mediaDevices
    /// </summary>
    [Description("@#mediaDevices")]
    public extern MediaDevices MediaDevices { get; }

    /// <summary>
    /// preferences
    /// </summary>
    [Description("@#preferences")]
    public extern PreferenceManager Preferences { get; }

    /// <summary>
    /// mediaSession
    /// </summary>
    [Description("@#mediaSession")]
    public extern MediaSession MediaSession { get; }

    /// <summary>
    /// permissions
    /// </summary>
    [Description("@#permissions")]
    public extern Permissions Permissions { get; }

    /// <summary>
    /// maxTouchPoints
    /// </summary>
    [Description("@#maxTouchPoints")]
    public extern int MaxTouchPoints { get; }

    /// <summary>
    /// presentation
    /// </summary>
    [Description("@#presentation")]
    public extern Presentation Presentation { get; }

    /// <summary>
    /// attribution
    /// </summary>
    [Description("@#attribution")]
    public extern Attribution Attribution { get; }

    /// <summary>
    /// wakeLock
    /// </summary>
    [Description("@#wakeLock")]
    public extern WakeLock WakeLock { get; }

    /// <summary>
    /// serial
    /// </summary>
    [Description("@#serial")]
    public extern Serial Serial { get; }

    /// <summary>
    /// serviceWorker
    /// </summary>
    [Description("@#serviceWorker")]
    public extern ServiceWorkerContainer ServiceWorker { get; }

    /// <summary>
    /// joinAdInterestGroup
    /// </summary>
    /// <param name="group">group</param>
    [Description("@#joinAdInterestGroup")]
    public extern PromiseResult<object> JoinAdInterestGroup(AuctionAdInterestGroup group);

    /// <summary>
    /// leaveAdInterestGroup
    /// </summary>
    /// <param name="group">group</param>
    [Description("@#leaveAdInterestGroup")]
    public extern PromiseResult<object> LeaveAdInterestGroup(AuctionAdInterestGroupKey? group = default);

    /// <summary>
    /// clearOriginJoinedAdInterestGroups
    /// </summary>
    /// <param name="owner">owner</param>
    /// <param name="interestGroupsToKeep">interestGroupsToKeep</param>
    [Description("@#clearOriginJoinedAdInterestGroups")]
    public extern PromiseResult<object> ClearOriginJoinedAdInterestGroups(string owner, string[]? interestGroupsToKeep = default);

    /// <summary>
    /// runAdAuction
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#runAdAuction")]
    public extern PromiseResult<Either<string, FencedFrameConfig>?> RunAdAuction(AuctionAdConfig config);

    /// <summary>
    /// deprecatedRunAdAuctionEnforcesKAnonymity
    /// </summary>
    [Description("@#deprecatedRunAdAuctionEnforcesKAnonymity")]
    public extern bool DeprecatedRunAdAuctionEnforcesKAnonymity { get; }

    /// <summary>
    /// canLoadAdAuctionFencedFrame
    /// </summary>
    [Description("@#canLoadAdAuctionFencedFrame")]
    public extern bool CanLoadAdAuctionFencedFrame();

    /// <summary>
    /// getInterestGroupAdAuctionData
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#getInterestGroupAdAuctionData")]
    public extern PromiseResult<AdAuctionData> GetInterestGroupAdAuctionData(AdAuctionDataConfig? config = default);

    /// <summary>
    /// createAuctionNonce
    /// </summary>
    [Description("@#createAuctionNonce")]
    public extern PromiseResult<string> CreateAuctionNonce();

    /// <summary>
    /// updateAdInterestGroups
    /// </summary>
    [Description("@#updateAdInterestGroups")]
    public extern void UpdateAdInterestGroups();

    /// <summary>
    /// protectedAudience
    /// </summary>
    [Description("@#protectedAudience")]
    public extern ProtectedAudience ProtectedAudience { get; }

    /// <summary>
    /// vibrate
    /// </summary>
    /// <param name="pattern">pattern</param>
    [Description("@#vibrate")]
    public extern bool Vibrate(VibratePattern pattern);

    /// <summary>
    /// virtualKeyboard
    /// </summary>
    [Description("@#virtualKeyboard")]
    public extern VirtualKeyboard VirtualKeyboard { get; }

    /// <summary>
    /// bluetooth
    /// </summary>
    [Description("@#bluetooth")]
    public extern Bluetooth Bluetooth { get; }

    /// <summary>
    /// share
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#share")]
    public extern PromiseResult<object> Share(ShareData? data = default);

    /// <summary>
    /// canShare
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#canShare")]
    public extern bool CanShare(ShareData? data = default);

    /// <summary>
    /// hid
    /// </summary>
    [Description("@#hid")]
    public extern HID Hid { get; }

    /// <summary>
    /// requestMIDIAccess
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestMIDIAccess")]
    public extern PromiseResult<MIDIAccess> RequestMIDIAccess(MIDIOptions? options = default);

    /// <summary>
    /// usb
    /// </summary>
    [Description("@#usb")]
    public extern USB Usb { get; }

    /// <summary>
    /// xr
    /// </summary>
    [Description("@#xr")]
    public extern XRSystem Xr { get; }

    /// <summary>
    /// windowControlsOverlay
    /// </summary>
    [Description("@#windowControlsOverlay")]
    public extern WindowControlsOverlay WindowControlsOverlay { get; }

    #region mixin NavigatorBadge
    /// <summary>
    /// setAppBadge
    /// </summary>
    /// <param name="contents">contents</param>
    [Description("@#setAppBadge")]
    public extern PromiseResult<object> SetAppBadge(ulong contents);

    /// <summary>
    /// clearAppBadge
    /// </summary>
    [Description("@#clearAppBadge")]
    public extern PromiseResult<object> ClearAppBadge();
    #endregion

    #region mixin NavigatorDeviceMemory
    /// <summary>
    /// deviceMemory
    /// </summary>
    [Description("@#deviceMemory")]
    public extern double DeviceMemory { get; }
    #endregion

    #region mixin GlobalPrivacyControl
    /// <summary>
    /// globalPrivacyControl
    /// </summary>
    [Description("@#globalPrivacyControl")]
    public extern bool GlobalPrivacyControl_ { get; }
    #endregion

    #region mixin NavigatorID
    /// <summary>
    /// appCodeName
    /// </summary>
    [Description("@#appCodeName")]
    public extern string AppCodeName { get; }

    /// <summary>
    /// appName
    /// </summary>
    [Description("@#appName")]
    public extern string AppName { get; }

    /// <summary>
    /// appVersion
    /// </summary>
    [Description("@#appVersion")]
    public extern string AppVersion { get; }

    /// <summary>
    /// platform
    /// </summary>
    [Description("@#platform")]
    public extern string Platform { get; }

    /// <summary>
    /// product
    /// </summary>
    [Description("@#product")]
    public extern string Product { get; }

    /// <summary>
    /// productSub
    /// </summary>
    [Description("@#productSub")]
    public extern string ProductSub { get; }

    /// <summary>
    /// userAgent
    /// </summary>
    [Description("@#userAgent")]
    public extern string UserAgent { get; }

    /// <summary>
    /// vendor
    /// </summary>
    [Description("@#vendor")]
    public extern string Vendor { get; }

    /// <summary>
    /// vendorSub
    /// </summary>
    [Description("@#vendorSub")]
    public extern string VendorSub { get; }

    /// <summary>
    /// taintEnabled
    /// </summary>
    [Description("@#taintEnabled")]
    public extern bool TaintEnabled();

    /// <summary>
    /// oscpu
    /// </summary>
    [Description("@#oscpu")]
    public extern string Oscpu { get; }
    #endregion

    #region mixin NavigatorLanguage
    /// <summary>
    /// language
    /// </summary>
    [Description("@#language")]
    public extern string Language { get; }

    /// <summary>
    /// languages
    /// </summary>
    [Description("@#languages")]
    public extern FrozenSet<string> Languages { get; }
    #endregion

    #region mixin NavigatorOnLine
    /// <summary>
    /// onLine
    /// </summary>
    [Description("@#onLine")]
    public extern bool OnLine { get; }
    #endregion

    #region mixin NavigatorContentUtils
    /// <summary>
    /// registerProtocolHandler
    /// </summary>
    /// <param name="scheme">scheme</param>
    /// <param name="url">url</param>
    [Description("@#registerProtocolHandler")]
    public extern void RegisterProtocolHandler(string scheme, string url);

    /// <summary>
    /// unregisterProtocolHandler
    /// </summary>
    /// <param name="scheme">scheme</param>
    /// <param name="url">url</param>
    [Description("@#unregisterProtocolHandler")]
    public extern void UnregisterProtocolHandler(string scheme, string url);
    #endregion

    #region mixin NavigatorCookies
    /// <summary>
    /// cookieEnabled
    /// </summary>
    [Description("@#cookieEnabled")]
    public extern bool CookieEnabled { get; }
    #endregion

    #region mixin NavigatorPlugins
    /// <summary>
    /// plugins
    /// </summary>
    [Description("@#plugins")]
    public extern PluginArray Plugins { get; }

    /// <summary>
    /// mimeTypes
    /// </summary>
    [Description("@#mimeTypes")]
    public extern MimeTypeArray MimeTypes { get; }

    /// <summary>
    /// javaEnabled
    /// </summary>
    [Description("@#javaEnabled")]
    public extern bool JavaEnabled();

    /// <summary>
    /// pdfViewerEnabled
    /// </summary>
    [Description("@#pdfViewerEnabled")]
    public extern bool PdfViewerEnabled { get; }
    #endregion

    #region mixin NavigatorConcurrentHardware
    /// <summary>
    /// hardwareConcurrency
    /// </summary>
    [Description("@#hardwareConcurrency")]
    public extern ulong HardwareConcurrency { get; }
    #endregion

    #region mixin NavigatorNetworkInformation
    /// <summary>
    /// connection
    /// </summary>
    [Description("@#connection")]
    public extern NetworkInformation Connection { get; }
    #endregion

    #region mixin NavigatorStorageBuckets
    /// <summary>
    /// storageBuckets
    /// </summary>
    [Description("@#storageBuckets")]
    public extern StorageBucketManager StorageBuckets { get; }
    #endregion

    #region mixin NavigatorStorage
    /// <summary>
    /// storage
    /// </summary>
    [Description("@#storage")]
    public extern StorageManager Storage { get; }
    #endregion

    #region mixin NavigatorUA
    /// <summary>
    /// userAgentData
    /// </summary>
    [Description("@#userAgentData")]
    public extern NavigatorUAData UserAgentData { get; }
    #endregion

    #region mixin NavigatorLocks
    /// <summary>
    /// locks
    /// </summary>
    [Description("@#locks")]
    public extern LockManager Locks { get; }
    #endregion

    #region mixin NavigatorAutomationInformation
    /// <summary>
    /// webdriver
    /// </summary>
    [Description("@#webdriver")]
    public extern bool Webdriver { get; }
    #endregion

    #region mixin NavigatorGPU
    /// <summary>
    /// gpu
    /// </summary>
    [Description("@#gpu")]
    public extern GPU Gpu { get; }
    #endregion

    #region mixin NavigatorML
    /// <summary>
    /// ml
    /// </summary>
    [Description("@#ml")]
    public extern ML Ml { get; }
    #endregion
}