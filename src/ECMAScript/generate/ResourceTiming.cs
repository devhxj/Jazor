namespace ECMAScript;

/// <summary>
/// PerformanceResourceTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceResourceTiming")]
public partial class PerformanceResourceTiming : PerformanceEntry
{
    /// <summary>
    /// initiatorType
    /// </summary>
    [Description("@#initiatorType")]
    public extern string InitiatorType { get; }

    /// <summary>
    /// deliveryType
    /// </summary>
    [Description("@#deliveryType")]
    public extern string DeliveryType { get; }

    /// <summary>
    /// nextHopProtocol
    /// </summary>
    [Description("@#nextHopProtocol")]
    public extern byte[] NextHopProtocol { get; }

    /// <summary>
    /// workerStart
    /// </summary>
    [Description("@#workerStart")]
    public extern double WorkerStart { get; }

    /// <summary>
    /// redirectStart
    /// </summary>
    [Description("@#redirectStart")]
    public extern double RedirectStart { get; }

    /// <summary>
    /// redirectEnd
    /// </summary>
    [Description("@#redirectEnd")]
    public extern double RedirectEnd { get; }

    /// <summary>
    /// fetchStart
    /// </summary>
    [Description("@#fetchStart")]
    public extern double FetchStart { get; }

    /// <summary>
    /// domainLookupStart
    /// </summary>
    [Description("@#domainLookupStart")]
    public extern double DomainLookupStart { get; }

    /// <summary>
    /// domainLookupEnd
    /// </summary>
    [Description("@#domainLookupEnd")]
    public extern double DomainLookupEnd { get; }

    /// <summary>
    /// connectStart
    /// </summary>
    [Description("@#connectStart")]
    public extern double ConnectStart { get; }

    /// <summary>
    /// connectEnd
    /// </summary>
    [Description("@#connectEnd")]
    public extern double ConnectEnd { get; }

    /// <summary>
    /// secureConnectionStart
    /// </summary>
    [Description("@#secureConnectionStart")]
    public extern double SecureConnectionStart { get; }

    /// <summary>
    /// requestStart
    /// </summary>
    [Description("@#requestStart")]
    public extern double RequestStart { get; }

    /// <summary>
    /// finalResponseHeadersStart
    /// </summary>
    [Description("@#finalResponseHeadersStart")]
    public extern double FinalResponseHeadersStart { get; }

    /// <summary>
    /// firstInterimResponseStart
    /// </summary>
    [Description("@#firstInterimResponseStart")]
    public extern double FirstInterimResponseStart { get; }

    /// <summary>
    /// responseStart
    /// </summary>
    [Description("@#responseStart")]
    public extern double ResponseStart { get; }

    /// <summary>
    /// responseEnd
    /// </summary>
    [Description("@#responseEnd")]
    public extern double ResponseEnd { get; }

    /// <summary>
    /// transferSize
    /// </summary>
    [Description("@#transferSize")]
    public extern ulong TransferSize { get; }

    /// <summary>
    /// encodedBodySize
    /// </summary>
    [Description("@#encodedBodySize")]
    public extern ulong EncodedBodySize { get; }

    /// <summary>
    /// decodedBodySize
    /// </summary>
    [Description("@#decodedBodySize")]
    public extern ulong DecodedBodySize { get; }

    /// <summary>
    /// responseStatus
    /// </summary>
    [Description("@#responseStatus")]
    public extern ushort ResponseStatus { get; }

    /// <summary>
    /// renderBlockingStatus
    /// </summary>
    [Description("@#renderBlockingStatus")]
    public extern RenderBlockingStatusType RenderBlockingStatus { get; }

    /// <summary>
    /// contentType
    /// </summary>
    [Description("@#contentType")]
    public extern string ContentType { get; }

    /// <summary>
    /// contentEncoding
    /// </summary>
    [Description("@#contentEncoding")]
    public extern string ContentEncoding { get; }

    /// <summary>
    /// serverTiming
    /// </summary>
    [Description("@#serverTiming")]
    public extern FrozenSet<PerformanceServerTiming> ServerTiming { get; }
}