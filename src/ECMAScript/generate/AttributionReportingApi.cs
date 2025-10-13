namespace ECMAScript;

/// <summary>
/// AttributionReportingRequestOptions
/// </summary>
[ECMAScript]
[Description("@#AttributionReportingRequestOptions")]
public record AttributionReportingRequestOptions(
    [property: Description("@#eventSourceEligible")]bool EventSourceEligible = default,
	[property: Description("@#triggerEligible")]bool TriggerEligible = default);

/// <summary>
/// RequestInit
/// </summary>
[ECMAScript]
[Description("@#RequestInit")]
public record RequestInit(
    [property: Description("@#attributionReporting")]AttributionReportingRequestOptions? AttributionReporting = default,
	[property: Description("@#method")]byte[]? Method = default,
	[property: Description("@#headers")]HeadersInit? Headers = default,
	[property: Description("@#body")]BodyInit? Body = default,
	[property: Description("@#referrer")]string? Referrer = default,
	[property: Description("@#referrerPolicy")]ReferrerPolicy? ReferrerPolicy = default,
	[property: Description("@#mode")]RequestMode? Mode = default,
	[property: Description("@#credentials")]RequestCredentials? Credentials = default,
	[property: Description("@#cache")]RequestCache? Cache = default,
	[property: Description("@#redirect")]RequestRedirect? Redirect = default,
	[property: Description("@#integrity")]string? Integrity = default,
	[property: Description("@#keepalive")]bool Keepalive = default,
	[property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#duplex")]RequestDuplex? Duplex = default,
	[property: Description("@#priority")]RequestPriority? Priority = default,
	[property: Description("@#window")]object? Window = default,
	[property: Description("@#targetAddressSpace")]IPAddressSpace? TargetAddressSpace = default,
	[property: Description("@#sharedStorageWritable")]bool SharedStorageWritable = default,
	[property: Description("@#privateToken")]PrivateToken? PrivateToken = default,
	[property: Description("@#adAuctionHeaders")]bool AdAuctionHeaders = default)
{
    [Category("optional")]
    public extern static RequestInit OptionalAttributionReporting(
        [Description("@#attributionReporting")]AttributionReportingRequestOptions? AttributionReporting = default);

    [Category("optional")]
    public extern static RequestInit OptionalMethodHeadersBody15(
        [Description("@#method")]byte[]? Method = default,
        [Description("@#headers")]HeadersInit? Headers = default,
        [Description("@#body")]BodyInit? Body = default,
        [Description("@#referrer")]string? Referrer = default,
        [Description("@#referrerPolicy")]ReferrerPolicy? ReferrerPolicy = default,
        [Description("@#mode")]RequestMode? Mode = default,
        [Description("@#credentials")]RequestCredentials? Credentials = default,
        [Description("@#cache")]RequestCache? Cache = default,
        [Description("@#redirect")]RequestRedirect? Redirect = default,
        [Description("@#integrity")]string? Integrity = default,
        [Description("@#keepalive")]bool Keepalive = default,
        [Description("@#signal")]AbortSignal? Signal = default,
        [Description("@#duplex")]RequestDuplex? Duplex = default,
        [Description("@#priority")]RequestPriority? Priority = default,
        [Description("@#window")]object? Window = default);

    [Category("optional")]
    public extern static RequestInit OptionalTargetAddressSpace(
        [Description("@#targetAddressSpace")]IPAddressSpace? TargetAddressSpace = default);

    [Category("optional")]
    public extern static RequestInit OptionalSharedStorageWritable(
        [Description("@#sharedStorageWritable")]bool SharedStorageWritable = default);

    [Category("optional")]
    public extern static RequestInit OptionalPrivateToken(
        [Description("@#privateToken")]PrivateToken? PrivateToken = default);

    [Category("optional")]
    public extern static RequestInit OptionalAdAuctionHeaders(
        [Description("@#adAuctionHeaders")]bool AdAuctionHeaders = default);
}

/// <summary>
/// XMLHttpRequest
/// </summary>
[ECMAScript]
[Description("@#XMLHttpRequest")]
public partial class XMLHttpRequest : XMLHttpRequestEventTarget
{
    /// <summary>
    /// setAttributionReporting
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#setAttributionReporting")]
    public extern void SetAttributionReporting(AttributionReportingRequestOptions options);

    /// <summary>
    /// setPrivateToken
    /// </summary>
    /// <param name="privateToken">privateToken</param>
    [Description("@#setPrivateToken")]
    public extern void SetPrivateToken(PrivateToken privateToken);

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern XMLHttpRequest();

    /// <summary>
    /// onreadystatechange
    /// </summary>
    [Description("@#onreadystatechange")]
    public extern EventHandler Onreadystatechange { get; set; }

    /// <summary>
    /// UNSENT
    /// </summary>
    [Description("@#UNSENT")]
    public const ushort UNSENT = 0;

    /// <summary>
    /// OPENED
    /// </summary>
    [Description("@#OPENED")]
    public const ushort OPENED = 1;

    /// <summary>
    /// HEADERS_RECEIVED
    /// </summary>
    [Description("@#HEADERS_RECEIVED")]
    public const ushort HEADERS_RECEIVED = 2;

    /// <summary>
    /// LOADING
    /// </summary>
    [Description("@#LOADING")]
    public const ushort LOADING = 3;

    /// <summary>
    /// DONE
    /// </summary>
    [Description("@#DONE")]
    public const ushort DONE = 4;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// open
    /// </summary>
    /// <param name="method">method</param>
    /// <param name="url">url</param>
    [Description("@#open")]
    public extern void Open(byte[] method, string url);

    /// <summary>
    /// open
    /// </summary>
    /// <param name="method">method</param>
    /// <param name="url">url</param>
    /// <param name="async">async</param>
    /// <param name="username">username</param>
    /// <param name="password">password</param>
    [Description("@#open")]
    public extern void Open(byte[] method, string url, bool @async, string? username = null, string? password = null);

    /// <summary>
    /// setRequestHeader
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#setRequestHeader")]
    public extern void SetRequestHeader(byte[] name, byte[] value);

    /// <summary>
    /// timeout
    /// </summary>
    [Description("@#timeout")]
    public extern uint Timeout { get; set; }

    /// <summary>
    /// withCredentials
    /// </summary>
    [Description("@#withCredentials")]
    public extern bool WithCredentials { get; set; }

    /// <summary>
    /// upload
    /// </summary>
    [Description("@#upload")]
    public extern XMLHttpRequestUpload Upload { get; }

    /// <summary>
    /// send
    /// </summary>
    /// <param name="body">body</param>
    [Description("@#send")]
    public extern void Send(Either<Document, XMLHttpRequestBodyInit>? body);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="body">body</param>
    [Description("@#send")]
    public extern void Send(Document body);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="body">body</param>
    [Description("@#send")]
    public extern void Send(XMLHttpRequestBodyInit body);

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern void Abort();

    /// <summary>
    /// responseURL
    /// </summary>
    [Description("@#responseURL")]
    public extern string ResponseURL { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern ushort Status { get; }

    /// <summary>
    /// statusText
    /// </summary>
    [Description("@#statusText")]
    public extern byte[] StatusText { get; }

    /// <summary>
    /// getResponseHeader
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getResponseHeader")]
    public extern byte[]? GetResponseHeader(byte[] name);

    /// <summary>
    /// getAllResponseHeaders
    /// </summary>
    [Description("@#getAllResponseHeaders")]
    public extern byte[] GetAllResponseHeaders();

    /// <summary>
    /// overrideMimeType
    /// </summary>
    /// <param name="mime">mime</param>
    [Description("@#overrideMimeType")]
    public extern void OverrideMimeType(string mime);

    /// <summary>
    /// responseType
    /// </summary>
    [Description("@#responseType")]
    public extern XMLHttpRequestResponseType ResponseType { get; set; }

    /// <summary>
    /// response
    /// </summary>
    [Description("@#response")]
    public extern object Response { get; }

    /// <summary>
    /// responseText
    /// </summary>
    [Description("@#responseText")]
    public extern string ResponseText { get; }

    /// <summary>
    /// responseXML
    /// </summary>
    [Description("@#responseXML")]
    public extern Document? ResponseXML { get; }
}