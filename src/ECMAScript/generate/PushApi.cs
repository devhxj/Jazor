namespace ECMAScript;

/// <summary>
/// PushPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PushPermissionDescriptor")]
public record PushPermissionDescriptor(
    [property: Description("@#userVisibleOnly")]bool UserVisibleOnly = false): PermissionDescriptor;

/// <summary>
/// PushSubscriptionOptionsInit
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionOptionsInit")]
public record PushSubscriptionOptionsInit(
    [property: Description("@#userVisibleOnly")]bool UserVisibleOnly = false,
	[property: Description("@#applicationServerKey")]Either<IBufferSource, string>? ApplicationServerKey = default);

/// <summary>
/// PushSubscriptionJSON
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionJSON")]
public record PushSubscriptionJSON(
    [property: Description("@#endpoint")]string? Endpoint = default,
	[property: Description("@#expirationTime")]EpochTimeStamp? ExpirationTime = null,
	[property: Description("@#keys")]Dictionary<string, string>? Keys = default);

/// <summary>
/// PushEventInit
/// </summary>
[ECMAScript]
[Description("@#PushEventInit")]
public record PushEventInit(
    [property: Description("@#data")]PushMessageDataInit? Data = default): ExtendableEventInit;

/// <summary>
/// PushSubscriptionChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionChangeEventInit")]
public record PushSubscriptionChangeEventInit(
    [property: Description("@#newSubscription")]PushSubscription? NewSubscription = default,
	[property: Description("@#oldSubscription")]PushSubscription? OldSubscription = default): ExtendableEventInit;

/// <summary>
/// PushManager
/// </summary>
[ECMAScript]
[Description("@#PushManager")]
public class PushManager
{
    /// <summary>
    /// supportedContentEncodings
    /// </summary>
    [Description("@#supportedContentEncodings")]
    public extern static FrozenSet<string> SupportedContentEncodings { get; }

    /// <summary>
    /// subscribe
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#subscribe")]
    public extern PromiseResult<PushSubscription> Subscribe(PushSubscriptionOptionsInit? options = default);

    /// <summary>
    /// getSubscription
    /// </summary>
    [Description("@#getSubscription")]
    public extern PromiseResult<PushSubscription?> GetSubscription();

    /// <summary>
    /// permissionState
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#permissionState")]
    public extern PromiseResult<PermissionState> PermissionState(PushSubscriptionOptionsInit? options = default);
}

/// <summary>
/// PushSubscriptionOptions
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionOptions")]
public class PushSubscriptionOptions
{
    /// <summary>
    /// userVisibleOnly
    /// </summary>
    [Description("@#userVisibleOnly")]
    public extern bool UserVisibleOnly { get; }

    /// <summary>
    /// applicationServerKey
    /// </summary>
    [Description("@#applicationServerKey")]
    public extern ArrayBuffer? ApplicationServerKey { get; }
}

/// <summary>
/// PushSubscription
/// </summary>
[ECMAScript]
[Description("@#PushSubscription")]
public class PushSubscription
{
    /// <summary>
    /// endpoint
    /// </summary>
    [Description("@#endpoint")]
    public extern string Endpoint { get; }

    /// <summary>
    /// expirationTime
    /// </summary>
    [Description("@#expirationTime")]
    public extern EpochTimeStamp? ExpirationTime { get; }

    /// <summary>
    /// options
    /// </summary>
    [Description("@#options")]
    public extern PushSubscriptionOptions Options { get; }

    /// <summary>
    /// getKey
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getKey")]
    public extern ArrayBuffer? GetKey(PushEncryptionKeyName name);

    /// <summary>
    /// unsubscribe
    /// </summary>
    [Description("@#unsubscribe")]
    public extern PromiseResult<bool> Unsubscribe();

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern PushSubscriptionJSON ToJSON();
}

/// <summary>
/// PushMessageData
/// </summary>
[ECMAScript]
[Description("@#PushMessageData")]
public class PushMessageData
{
    /// <summary>
    /// arrayBuffer
    /// </summary>
    [Description("@#arrayBuffer")]
    public extern ArrayBuffer ArrayBuffer();

    /// <summary>
    /// blob
    /// </summary>
    [Description("@#blob")]
    public extern Blob Blob();

    /// <summary>
    /// bytes
    /// </summary>
    [Description("@#bytes")]
    public extern Uint8Array Bytes();

    /// <summary>
    /// json
    /// </summary>
    [Description("@#json")]
    public extern object Json();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text();
}

/// <summary>
/// PushEvent
/// </summary>
[ECMAScript]
[Description("@#PushEvent")]
public class PushEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PushEvent(string type, PushEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern PushMessageData? Data { get; }
}

/// <summary>
/// PushSubscriptionChangeEvent
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionChangeEvent")]
public class PushSubscriptionChangeEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PushSubscriptionChangeEvent(string type, PushSubscriptionChangeEventInit eventInitDict);

    /// <summary>
    /// newSubscription
    /// </summary>
    [Description("@#newSubscription")]
    public extern PushSubscription? NewSubscription { get; }

    /// <summary>
    /// oldSubscription
    /// </summary>
    [Description("@#oldSubscription")]
    public extern PushSubscription? OldSubscription { get; }
}