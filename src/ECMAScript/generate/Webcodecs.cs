namespace ECMAScript;

/// <summary>
/// AudioDecoderInit
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderInit")]
public record AudioDecoderInit(
    [property: Description("@#output")]AudioDataOutputCallback? Output = default,
	[property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// VideoDecoderInit
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderInit")]
public record VideoDecoderInit(
    [property: Description("@#output")]VideoFrameOutputCallback? Output = default,
	[property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// AudioEncoderInit
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderInit")]
public record AudioEncoderInit(
    [property: Description("@#output")]EncodedAudioChunkOutputCallback? Output = default,
	[property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// EncodedAudioChunkMetadata
/// </summary>
[ECMAScript]
[Description("@#EncodedAudioChunkMetadata")]
public record EncodedAudioChunkMetadata(
    [property: Description("@#decoderConfig")]AudioDecoderConfig? DecoderConfig = default);

/// <summary>
/// VideoEncoderInit
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderInit")]
public record VideoEncoderInit(
    [property: Description("@#output")]EncodedVideoChunkOutputCallback? Output = default,
	[property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// EncodedVideoChunkMetadata
/// </summary>
[ECMAScript]
[Description("@#EncodedVideoChunkMetadata")]
public record EncodedVideoChunkMetadata(
    [property: Description("@#decoderConfig")]VideoDecoderConfig? DecoderConfig = default,
	[property: Description("@#svc")]SvcOutputMetadata? Svc = default,
	[property: Description("@#alphaSideData")]IBufferSource? AlphaSideData = default);

/// <summary>
/// SvcOutputMetadata
/// </summary>
[ECMAScript]
[Description("@#SvcOutputMetadata")]
public record SvcOutputMetadata(
    [property: Description("@#temporalLayerId")]uint TemporalLayerId = default);

/// <summary>
/// AudioDecoderSupport
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderSupport")]
public record AudioDecoderSupport(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#config")]AudioDecoderConfig? Config = default);

/// <summary>
/// VideoDecoderSupport
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderSupport")]
public record VideoDecoderSupport(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#config")]VideoDecoderConfig? Config = default);

/// <summary>
/// AudioEncoderSupport
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderSupport")]
public record AudioEncoderSupport(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#config")]AudioEncoderConfig? Config = default);

/// <summary>
/// VideoEncoderSupport
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderSupport")]
public record VideoEncoderSupport(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#config")]VideoEncoderConfig? Config = default);

/// <summary>
/// AudioDecoderConfig
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderConfig")]
public record AudioDecoderConfig(
    [property: Description("@#codec")]string? Codec = default,
	[property: Description("@#sampleRate")]uint SampleRate = default,
	[property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
	[property: Description("@#description")]IAllowSharedBufferSource? Description = default);

/// <summary>
/// VideoDecoderConfig
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderConfig")]
public record VideoDecoderConfig(
    [property: Description("@#codec")]string? Codec = default,
	[property: Description("@#description")]IAllowSharedBufferSource? Description = default,
	[property: Description("@#codedWidth")]uint CodedWidth = default,
	[property: Description("@#codedHeight")]uint CodedHeight = default,
	[property: Description("@#displayAspectWidth")]uint DisplayAspectWidth = default,
	[property: Description("@#displayAspectHeight")]uint DisplayAspectHeight = default,
	[property: Description("@#colorSpace")]VideoColorSpaceInit? ColorSpace = default,
	[property: Description("@#hardwareAcceleration")]HardwareAcceleration HardwareAcceleration = HardwareAcceleration.NoPreference,
	[property: Description("@#optimizeForLatency")]bool OptimizeForLatency = default,
	[property: Description("@#rotation")]double Rotation = 0d,
	[property: Description("@#flip")]bool Flip = false);

/// <summary>
/// EncodedAudioChunkInit
/// </summary>
[ECMAScript]
[Description("@#EncodedAudioChunkInit")]
public record EncodedAudioChunkInit(
    [property: Description("@#type")]EncodedAudioChunkType? Type = default,
	[property: Description("@#timestamp")]long Timestamp = default,
	[property: Description("@#duration")]ulong Duration = default,
	[property: Description("@#data")]IAllowSharedBufferSource? Data = default,
	[property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// EncodedVideoChunkInit
/// </summary>
[ECMAScript]
[Description("@#EncodedVideoChunkInit")]
public record EncodedVideoChunkInit(
    [property: Description("@#type")]EncodedVideoChunkType? Type = default,
	[property: Description("@#timestamp")]long Timestamp = default,
	[property: Description("@#duration")]ulong Duration = default,
	[property: Description("@#data")]IAllowSharedBufferSource? Data = default,
	[property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// AudioDataInit
/// </summary>
[ECMAScript]
[Description("@#AudioDataInit")]
public record AudioDataInit(
    [property: Description("@#format")]AudioSampleFormat? Format = default,
	[property: Description("@#sampleRate")]float SampleRate = default,
	[property: Description("@#numberOfFrames")]uint NumberOfFrames = default,
	[property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
	[property: Description("@#timestamp")]long Timestamp = default,
	[property: Description("@#data")]IBufferSource? Data = default,
	[property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// AudioDataCopyToOptions
/// </summary>
[ECMAScript]
[Description("@#AudioDataCopyToOptions")]
public record AudioDataCopyToOptions(
    [property: Description("@#planeIndex")]uint PlaneIndex = default,
	[property: Description("@#frameOffset")]uint FrameOffset = 0,
	[property: Description("@#frameCount")]uint FrameCount = default,
	[property: Description("@#format")]AudioSampleFormat? Format = default);

/// <summary>
/// VideoFrameInit
/// </summary>
[ECMAScript]
[Description("@#VideoFrameInit")]
public record VideoFrameInit(
    [property: Description("@#duration")]ulong Duration = default,
	[property: Description("@#timestamp")]long Timestamp = default,
	[property: Description("@#alpha")]AlphaOption Alpha = AlphaOption.Keep,
	[property: Description("@#visibleRect")]DOMRectInit? VisibleRect = default,
	[property: Description("@#rotation")]double Rotation = 0d,
	[property: Description("@#flip")]bool Flip = false,
	[property: Description("@#displayWidth")]uint DisplayWidth = default,
	[property: Description("@#displayHeight")]uint DisplayHeight = default,
	[property: Description("@#metadata")]VideoFrameMetadata? Metadata = default);

/// <summary>
/// VideoFrameBufferInit
/// </summary>
[ECMAScript]
[Description("@#VideoFrameBufferInit")]
public record VideoFrameBufferInit(
    [property: Description("@#format")]VideoPixelFormat? Format = default,
	[property: Description("@#codedWidth")]uint CodedWidth = default,
	[property: Description("@#codedHeight")]uint CodedHeight = default,
	[property: Description("@#timestamp")]long Timestamp = default,
	[property: Description("@#duration")]ulong Duration = default,
	[property: Description("@#layout")]PlaneLayout[]? Layout = default,
	[property: Description("@#visibleRect")]DOMRectInit? VisibleRect = default,
	[property: Description("@#rotation")]double Rotation = 0d,
	[property: Description("@#flip")]bool Flip = false,
	[property: Description("@#displayWidth")]uint DisplayWidth = default,
	[property: Description("@#displayHeight")]uint DisplayHeight = default,
	[property: Description("@#colorSpace")]VideoColorSpaceInit? ColorSpace = default,
	[property: Description("@#transfer")]ArrayBuffer[]? Transfer = default,
	[property: Description("@#metadata")]VideoFrameMetadata? Metadata = default);

/// <summary>
/// VideoFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#VideoFrameMetadata")]
public abstract record VideoFrameMetadata();

/// <summary>
/// VideoFrameCopyToOptions
/// </summary>
[ECMAScript]
[Description("@#VideoFrameCopyToOptions")]
public record VideoFrameCopyToOptions(
    [property: Description("@#rect")]DOMRectInit? Rect = default,
	[property: Description("@#layout")]PlaneLayout[]? Layout = default,
	[property: Description("@#format")]VideoPixelFormat? Format = default,
	[property: Description("@#colorSpace")]PredefinedColorSpace? ColorSpace = default);

/// <summary>
/// PlaneLayout
/// </summary>
[ECMAScript]
[Description("@#PlaneLayout")]
public record PlaneLayout(
    [property: Description("@#offset")]uint Offset = default,
	[property: Description("@#stride")]uint Stride = default);

/// <summary>
/// VideoColorSpaceInit
/// </summary>
[ECMAScript]
[Description("@#VideoColorSpaceInit")]
public record VideoColorSpaceInit(
    [property: Description("@#primaries")]VideoColorPrimaries? Primaries = null,
	[property: Description("@#transfer")]VideoTransferCharacteristics? Transfer = null,
	[property: Description("@#matrix")]VideoMatrixCoefficients? Matrix = null,
	[property: Description("@#fullRange")]bool? FullRange = null);

/// <summary>
/// ImageDecoderInit
/// </summary>
[ECMAScript]
[Description("@#ImageDecoderInit")]
public record ImageDecoderInit(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#data")]ImageBufferSource? Data = default,
	[property: Description("@#colorSpaceConversion")]ColorSpaceConversion ColorSpaceConversion = ColorSpaceConversion.Default,
	[property: Description("@#desiredWidth")]uint DesiredWidth = default,
	[property: Description("@#desiredHeight")]uint DesiredHeight = default,
	[property: Description("@#preferAnimation")]bool PreferAnimation = default,
	[property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// ImageDecodeOptions
/// </summary>
[ECMAScript]
[Description("@#ImageDecodeOptions")]
public record ImageDecodeOptions(
    [property: Description("@#frameIndex")]uint FrameIndex = 0,
	[property: Description("@#completeFramesOnly")]bool CompleteFramesOnly = true);

/// <summary>
/// ImageDecodeResult
/// </summary>
[ECMAScript]
[Description("@#ImageDecodeResult")]
public record ImageDecodeResult(
    [property: Description("@#image")]VideoFrame? Image = default,
	[property: Description("@#complete")]bool Complete = default);

/// <summary>
/// AudioDecoder
/// </summary>
[ECMAScript]
[Description("@#AudioDecoder")]
public class AudioDecoder : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern AudioDecoder(AudioDecoderInit init);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern CodecState State { get; }

    /// <summary>
    /// decodeQueueSize
    /// </summary>
    [Description("@#decodeQueueSize")]
    public extern uint DecodeQueueSize { get; }

    /// <summary>
    /// ondequeue
    /// </summary>
    [Description("@#ondequeue")]
    public extern EventHandler Ondequeue { get; set; }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#configure")]
    public extern void Configure(AudioDecoderConfig config);

    /// <summary>
    /// decode
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#decode")]
    public extern void Decode(EncodedAudioChunk chunk);

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern PromiseResult<object> Flush();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// isConfigSupported
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#isConfigSupported")]
    public extern static PromiseResult<AudioDecoderSupport> IsConfigSupported(AudioDecoderConfig config);
}

/// <summary>
/// VideoDecoder
/// </summary>
[ECMAScript]
[Description("@#VideoDecoder")]
public class VideoDecoder : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern VideoDecoder(VideoDecoderInit init);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern CodecState State { get; }

    /// <summary>
    /// decodeQueueSize
    /// </summary>
    [Description("@#decodeQueueSize")]
    public extern uint DecodeQueueSize { get; }

    /// <summary>
    /// ondequeue
    /// </summary>
    [Description("@#ondequeue")]
    public extern EventHandler Ondequeue { get; set; }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#configure")]
    public extern void Configure(VideoDecoderConfig config);

    /// <summary>
    /// decode
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#decode")]
    public extern void Decode(EncodedVideoChunk chunk);

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern PromiseResult<object> Flush();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// isConfigSupported
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#isConfigSupported")]
    public extern static PromiseResult<VideoDecoderSupport> IsConfigSupported(VideoDecoderConfig config);
}

/// <summary>
/// AudioEncoder
/// </summary>
[ECMAScript]
[Description("@#AudioEncoder")]
public class AudioEncoder : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern AudioEncoder(AudioEncoderInit init);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern CodecState State { get; }

    /// <summary>
    /// encodeQueueSize
    /// </summary>
    [Description("@#encodeQueueSize")]
    public extern uint EncodeQueueSize { get; }

    /// <summary>
    /// ondequeue
    /// </summary>
    [Description("@#ondequeue")]
    public extern EventHandler Ondequeue { get; set; }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#configure")]
    public extern void Configure(AudioEncoderConfig config);

    /// <summary>
    /// encode
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#encode")]
    public extern void Encode(AudioData data);

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern PromiseResult<object> Flush();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// isConfigSupported
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#isConfigSupported")]
    public extern static PromiseResult<AudioEncoderSupport> IsConfigSupported(AudioEncoderConfig config);
}

/// <summary>
/// VideoEncoder
/// </summary>
[ECMAScript]
[Description("@#VideoEncoder")]
public class VideoEncoder : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern VideoEncoder(VideoEncoderInit init);

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern CodecState State { get; }

    /// <summary>
    /// encodeQueueSize
    /// </summary>
    [Description("@#encodeQueueSize")]
    public extern uint EncodeQueueSize { get; }

    /// <summary>
    /// ondequeue
    /// </summary>
    [Description("@#ondequeue")]
    public extern EventHandler Ondequeue { get; set; }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#configure")]
    public extern void Configure(VideoEncoderConfig config);

    /// <summary>
    /// encode
    /// </summary>
    /// <param name="frame">frame</param>
    /// <param name="options">options</param>
    [Description("@#encode")]
    public extern void Encode(VideoFrame frame, VideoEncoderEncodeOptions? options = default);

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern PromiseResult<object> Flush();

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// isConfigSupported
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#isConfigSupported")]
    public extern static PromiseResult<VideoEncoderSupport> IsConfigSupported(VideoEncoderConfig config);
}

/// <summary>
/// EncodedAudioChunk
/// </summary>
[ECMAScript]
[Description("@#EncodedAudioChunk")]
public class EncodedAudioChunk
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern EncodedAudioChunk(EncodedAudioChunkInit init);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern EncodedAudioChunkType Type { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern long Timestamp { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern ulong? Duration { get; }

    /// <summary>
    /// byteLength
    /// </summary>
    [Description("@#byteLength")]
    public extern uint ByteLength { get; }

    /// <summary>
    /// copyTo
    /// </summary>
    /// <param name="destination">destination</param>
    [Description("@#copyTo")]
    public extern void CopyTo(IAllowSharedBufferSource destination);
}

/// <summary>
/// EncodedVideoChunk
/// </summary>
[ECMAScript]
[Description("@#EncodedVideoChunk")]
public class EncodedVideoChunk
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern EncodedVideoChunk(EncodedVideoChunkInit init);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern EncodedVideoChunkType Type { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern long Timestamp { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern ulong? Duration { get; }

    /// <summary>
    /// byteLength
    /// </summary>
    [Description("@#byteLength")]
    public extern uint ByteLength { get; }

    /// <summary>
    /// copyTo
    /// </summary>
    /// <param name="destination">destination</param>
    [Description("@#copyTo")]
    public extern void CopyTo(IAllowSharedBufferSource destination);
}

/// <summary>
/// AudioData
/// </summary>
[ECMAScript]
[Description("@#AudioData")]
public class AudioData
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern AudioData(AudioDataInit init);

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern AudioSampleFormat? Format { get; }

    /// <summary>
    /// sampleRate
    /// </summary>
    [Description("@#sampleRate")]
    public extern float SampleRate { get; }

    /// <summary>
    /// numberOfFrames
    /// </summary>
    [Description("@#numberOfFrames")]
    public extern uint NumberOfFrames { get; }

    /// <summary>
    /// numberOfChannels
    /// </summary>
    [Description("@#numberOfChannels")]
    public extern uint NumberOfChannels { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern ulong Duration { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern long Timestamp { get; }

    /// <summary>
    /// allocationSize
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#allocationSize")]
    public extern uint AllocationSize(AudioDataCopyToOptions options);

    /// <summary>
    /// copyTo
    /// </summary>
    /// <param name="destination">destination</param>
    /// <param name="options">options</param>
    [Description("@#copyTo")]
    public extern void CopyTo(IAllowSharedBufferSource destination, AudioDataCopyToOptions options);

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern AudioData Clone();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// VideoFrame
/// </summary>
[ECMAScript]
[Description("@#VideoFrame")]
public class VideoFrame
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="init">init</param>
    public extern VideoFrame(CanvasImageSource image, VideoFrameInit init);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="init">init</param>
    public extern VideoFrame(IAllowSharedBufferSource data, VideoFrameBufferInit init);

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern VideoPixelFormat? Format { get; }

    /// <summary>
    /// codedWidth
    /// </summary>
    [Description("@#codedWidth")]
    public extern uint CodedWidth { get; }

    /// <summary>
    /// codedHeight
    /// </summary>
    [Description("@#codedHeight")]
    public extern uint CodedHeight { get; }

    /// <summary>
    /// codedRect
    /// </summary>
    [Description("@#codedRect")]
    public extern DOMRectReadOnly? CodedRect { get; }

    /// <summary>
    /// visibleRect
    /// </summary>
    [Description("@#visibleRect")]
    public extern DOMRectReadOnly? VisibleRect { get; }

    /// <summary>
    /// rotation
    /// </summary>
    [Description("@#rotation")]
    public extern double Rotation { get; }

    /// <summary>
    /// flip
    /// </summary>
    [Description("@#flip")]
    public extern bool Flip { get; }

    /// <summary>
    /// displayWidth
    /// </summary>
    [Description("@#displayWidth")]
    public extern uint DisplayWidth { get; }

    /// <summary>
    /// displayHeight
    /// </summary>
    [Description("@#displayHeight")]
    public extern uint DisplayHeight { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern ulong? Duration { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern long Timestamp { get; }

    /// <summary>
    /// colorSpace
    /// </summary>
    [Description("@#colorSpace")]
    public extern VideoColorSpace ColorSpace { get; }

    /// <summary>
    /// metadata
    /// </summary>
    [Description("@#metadata")]
    public extern VideoFrameMetadata Metadata();

    /// <summary>
    /// allocationSize
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#allocationSize")]
    public extern uint AllocationSize(VideoFrameCopyToOptions? options = default);

    /// <summary>
    /// copyTo
    /// </summary>
    /// <param name="destination">destination</param>
    /// <param name="options">options</param>
    [Description("@#copyTo")]
    public extern PromiseResult<PlaneLayout[]> CopyTo(IAllowSharedBufferSource destination, VideoFrameCopyToOptions? options = default);

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern VideoFrame Clone();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// VideoColorSpace
/// </summary>
[ECMAScript]
[Description("@#VideoColorSpace")]
public class VideoColorSpace
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern VideoColorSpace(VideoColorSpaceInit init);

    /// <summary>
    /// primaries
    /// </summary>
    [Description("@#primaries")]
    public extern VideoColorPrimaries? Primaries { get; }

    /// <summary>
    /// transfer
    /// </summary>
    [Description("@#transfer")]
    public extern VideoTransferCharacteristics? Transfer { get; }

    /// <summary>
    /// matrix
    /// </summary>
    [Description("@#matrix")]
    public extern VideoMatrixCoefficients? Matrix { get; }

    /// <summary>
    /// fullRange
    /// </summary>
    [Description("@#fullRange")]
    public extern bool? FullRange { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern VideoColorSpaceInit ToJSON();
}

/// <summary>
/// ImageDecoder
/// </summary>
[ECMAScript]
[Description("@#ImageDecoder")]
public class ImageDecoder
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern ImageDecoder(ImageDecoderInit init);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// complete
    /// </summary>
    [Description("@#complete")]
    public extern bool Complete { get; }

    /// <summary>
    /// completed
    /// </summary>
    [Description("@#completed")]
    public extern PromiseResult<object> Completed { get; }

    /// <summary>
    /// tracks
    /// </summary>
    [Description("@#tracks")]
    public extern ImageTrackList Tracks { get; }

    /// <summary>
    /// decode
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#decode")]
    public extern PromiseResult<ImageDecodeResult> Decode(ImageDecodeOptions? options = default);

    /// <summary>
    /// reset
    /// </summary>
    [Description("@#reset")]
    public extern void Reset();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// isTypeSupported
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#isTypeSupported")]
    public extern static PromiseResult<bool> IsTypeSupported(string type);
}

/// <summary>
/// ImageTrackList
/// </summary>
[ECMAScript]
[Description("@#ImageTrackList")]
public class ImageTrackList
{
    [Description("@#")]
    public extern ImageTrack this[uint index] { get; }

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<object> Ready { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// selectedIndex
    /// </summary>
    [Description("@#selectedIndex")]
    public extern int SelectedIndex { get; }

    /// <summary>
    /// selectedTrack
    /// </summary>
    [Description("@#selectedTrack")]
    public extern ImageTrack? SelectedTrack { get; }
}

/// <summary>
/// ImageTrack
/// </summary>
[ECMAScript]
[Description("@#ImageTrack")]
public class ImageTrack
{
    /// <summary>
    /// animated
    /// </summary>
    [Description("@#animated")]
    public extern bool Animated { get; }

    /// <summary>
    /// frameCount
    /// </summary>
    [Description("@#frameCount")]
    public extern uint FrameCount { get; }

    /// <summary>
    /// repetitionCount
    /// </summary>
    [Description("@#repetitionCount")]
    public extern float RepetitionCount { get; }

    /// <summary>
    /// selected
    /// </summary>
    [Description("@#selected")]
    public extern bool Selected { get; set; }
}