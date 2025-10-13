namespace ECMAScript;

/// <summary>
/// SFrameTransformOptions
/// </summary>
[ECMAScript]
[Description("@#SFrameTransformOptions")]
public record SFrameTransformOptions(
    [property: Description("@#role")]SFrameTransformRole Role = SFrameTransformRole.Encrypt);

/// <summary>
/// SFrameTransformErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SFrameTransformErrorEventInit")]
public record SFrameTransformErrorEventInit(
    [property: Description("@#errorType")]SFrameTransformErrorEventType? ErrorType = default,
	[property: Description("@#frame")]object? Frame = default,
	[property: Description("@#keyID")]CryptoKeyID? KeyID = default): EventInit;

/// <summary>
/// RTCEncodedFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedFrameMetadata")]
public record RTCEncodedFrameMetadata(
    [property: Description("@#synchronizationSource")]uint SynchronizationSource = default,
	[property: Description("@#payloadType")]byte PayloadType = default,
	[property: Description("@#contributingSources")]uint[]? ContributingSources = default,
	[property: Description("@#rtpTimestamp")]uint RtpTimestamp = default,
	[property: Description("@#receiveTime")]double ReceiveTime = default,
	[property: Description("@#captureTime")]double CaptureTime = default,
	[property: Description("@#senderCaptureTimeOffset")]double SenderCaptureTimeOffset = default,
	[property: Description("@#mimeType")]string? MimeType = default);

/// <summary>
/// RTCEncodedVideoFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedVideoFrameMetadata")]
public record RTCEncodedVideoFrameMetadata(
    [property: Description("@#frameId")]ulong FrameId = default,
	[property: Description("@#dependencies")]ulong[]? Dependencies = default,
	[property: Description("@#width")]ushort Width = default,
	[property: Description("@#height")]ushort Height = default,
	[property: Description("@#spatialIndex")]uint SpatialIndex = default,
	[property: Description("@#temporalIndex")]uint TemporalIndex = default,
	[property: Description("@#timestamp")]long Timestamp = default): RTCEncodedFrameMetadata;

/// <summary>
/// RTCEncodedVideoFrameOptions
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedVideoFrameOptions")]
public record RTCEncodedVideoFrameOptions(
    [property: Description("@#metadata")]RTCEncodedVideoFrameMetadata? Metadata = default);

/// <summary>
/// RTCEncodedAudioFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedAudioFrameMetadata")]
public record RTCEncodedAudioFrameMetadata(
    [property: Description("@#sequenceNumber")]short SequenceNumber = default,
	[property: Description("@#audioLevel")]double AudioLevel = default): RTCEncodedFrameMetadata;

/// <summary>
/// RTCEncodedAudioFrameOptions
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedAudioFrameOptions")]
public record RTCEncodedAudioFrameOptions(
    [property: Description("@#metadata")]RTCEncodedAudioFrameMetadata? Metadata = default);

/// <summary>
/// RTCRtpSender
/// </summary>
[ECMAScript]
[Description("@#RTCRtpSender")]
public partial class RTCRtpSender
{
    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern RTCRtpTransform? Transform { get; set; }

    /// <summary>
    /// generateKeyFrame
    /// </summary>
    /// <param name="rids">rids</param>
    [Description("@#generateKeyFrame")]
    public extern PromiseResult<object> GenerateKeyFrame(string[] rids);

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack? Track { get; }

    /// <summary>
    /// transport
    /// </summary>
    [Description("@#transport")]
    public extern RTCDtlsTransport? Transport { get; }

    /// <summary>
    /// getCapabilities
    /// </summary>
    /// <param name="kind">kind</param>
    [Description("@#getCapabilities")]
    public extern static RTCRtpCapabilities? GetCapabilities(string kind);

    /// <summary>
    /// setParameters
    /// </summary>
    /// <param name="parameters">parameters</param>
    /// <param name="setParameterOptions">setParameterOptions</param>
    [Description("@#setParameters")]
    public extern PromiseResult<object> SetParameters(RTCRtpSendParameters parameters, RTCSetParameterOptions? setParameterOptions = default);

    /// <summary>
    /// getParameters
    /// </summary>
    [Description("@#getParameters")]
    public extern RTCRtpSendParameters GetParameters();

    /// <summary>
    /// replaceTrack
    /// </summary>
    /// <param name="withTrack">withTrack</param>
    [Description("@#replaceTrack")]
    public extern PromiseResult<object> ReplaceTrack(MediaStreamTrack? withTrack);

    /// <summary>
    /// setStreams
    /// </summary>
    /// <param name="streams">streams</param>
    [Description("@#setStreams")]
    public extern void SetStreams(MediaStream streams);

    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<RTCStatsReport> GetStats();

    /// <summary>
    /// dtmf
    /// </summary>
    [Description("@#dtmf")]
    public extern RTCDTMFSender? Dtmf { get; }
}

/// <summary>
/// RTCRtpReceiver
/// </summary>
[ECMAScript]
[Description("@#RTCRtpReceiver")]
public partial class RTCRtpReceiver
{
    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern RTCRtpTransform? Transform { get; set; }

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack Track { get; }

    /// <summary>
    /// transport
    /// </summary>
    [Description("@#transport")]
    public extern RTCDtlsTransport? Transport { get; }

    /// <summary>
    /// getCapabilities
    /// </summary>
    /// <param name="kind">kind</param>
    [Description("@#getCapabilities")]
    public extern static RTCRtpCapabilities? GetCapabilities(string kind);

    /// <summary>
    /// getParameters
    /// </summary>
    [Description("@#getParameters")]
    public extern RTCRtpReceiveParameters GetParameters();

    /// <summary>
    /// getContributingSources
    /// </summary>
    [Description("@#getContributingSources")]
    public extern RTCRtpContributingSource[] GetContributingSources();

    /// <summary>
    /// getSynchronizationSources
    /// </summary>
    [Description("@#getSynchronizationSources")]
    public extern RTCRtpSynchronizationSource[] GetSynchronizationSources();

    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<RTCStatsReport> GetStats();

    /// <summary>
    /// jitterBufferTarget
    /// </summary>
    [Description("@#jitterBufferTarget")]
    public extern double? JitterBufferTarget { get; set; }
}

/// <summary>
/// SFrameTransform
/// </summary>
[ECMAScript]
[Description("@#SFrameTransform")]
public class SFrameTransform : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern SFrameTransform(SFrameTransformOptions options);

    /// <summary>
    /// setEncryptionKey
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="keyId">keyID</param>
    [Description("@#setEncryptionKey")]
    public extern PromiseResult<object> SetEncryptionKey(CryptoKey key, CryptoKeyID keyId);

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    #region mixin GenericTransformStream
    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }
    #endregion
}

/// <summary>
/// SFrameTransformErrorEvent
/// </summary>
[ECMAScript]
[Description("@#SFrameTransformErrorEvent")]
public class SFrameTransformErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SFrameTransformErrorEvent(string type, SFrameTransformErrorEventInit eventInitDict);

    /// <summary>
    /// errorType
    /// </summary>
    [Description("@#errorType")]
    public extern SFrameTransformErrorEventType ErrorType { get; }

    /// <summary>
    /// keyID
    /// </summary>
    [Description("@#keyID")]
    public extern CryptoKeyID? KeyID { get; }

    /// <summary>
    /// frame
    /// </summary>
    [Description("@#frame")]
    public extern object Frame { get; }
}

/// <summary>
/// RTCEncodedVideoFrame
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedVideoFrame")]
public class RTCEncodedVideoFrame
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="originalFrame">originalFrame</param>
    /// <param name="options">options</param>
    public extern RTCEncodedVideoFrame(RTCEncodedVideoFrame originalFrame, RTCEncodedVideoFrameOptions options);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern RTCEncodedVideoFrameType Type { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern ArrayBuffer Data { get; set; }

    /// <summary>
    /// getMetadata
    /// </summary>
    [Description("@#getMetadata")]
    public extern RTCEncodedVideoFrameMetadata GetMetadata();
}

/// <summary>
/// RTCEncodedAudioFrame
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedAudioFrame")]
public class RTCEncodedAudioFrame
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="originalFrame">originalFrame</param>
    /// <param name="options">options</param>
    public extern RTCEncodedAudioFrame(RTCEncodedAudioFrame originalFrame, RTCEncodedAudioFrameOptions options);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern ArrayBuffer Data { get; set; }

    /// <summary>
    /// getMetadata
    /// </summary>
    [Description("@#getMetadata")]
    public extern RTCEncodedAudioFrameMetadata GetMetadata();
}

/// <summary>
/// RTCTransformEvent
/// </summary>
[ECMAScript]
[Description("@#RTCTransformEvent")]
public class RTCTransformEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// transformer
    /// </summary>
    [Description("@#transformer")]
    public extern RTCRtpScriptTransformer Transformer { get; }
}

/// <summary>
/// RTCRtpScriptTransformer
/// </summary>
[ECMAScript]
[Description("@#RTCRtpScriptTransformer")]
public class RTCRtpScriptTransformer : EventTarget
{
    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// generateKeyFrame
    /// </summary>
    /// <param name="rid">rid</param>
    [Description("@#generateKeyFrame")]
    public extern PromiseResult<ulong> GenerateKeyFrame(string rid);

    /// <summary>
    /// sendKeyFrameRequest
    /// </summary>
    [Description("@#sendKeyFrameRequest")]
    public extern PromiseResult<object> SendKeyFrameRequest();

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }

    /// <summary>
    /// onkeyframerequest
    /// </summary>
    [Description("@#onkeyframerequest")]
    public extern EventHandler Onkeyframerequest { get; set; }

    /// <summary>
    /// options
    /// </summary>
    [Description("@#options")]
    public extern object Options { get; }
}

/// <summary>
/// RTCRtpScriptTransform
/// </summary>
[ECMAScript]
[Description("@#RTCRtpScriptTransform")]
public class RTCRtpScriptTransform
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="worker">worker</param>
    /// <param name="options">options</param>
    /// <param name="transfer">transfer</param>
    public extern RTCRtpScriptTransform(Worker worker, object options, object[] transfer);
}

/// <summary>
/// KeyFrameRequestEvent
/// </summary>
[ECMAScript]
[Description("@#KeyFrameRequestEvent")]
public class KeyFrameRequestEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="rid">rid</param>
    public extern KeyFrameRequestEvent(string type, string rid);

    /// <summary>
    /// rid
    /// </summary>
    [Description("@#rid")]
    public extern string? Rid { get; }
}