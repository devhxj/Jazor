namespace ECMAScript;

/// <summary>
/// MediaRecorderOptions
/// </summary>
[ECMAScript]
[Description("@#MediaRecorderOptions")]
public record MediaRecorderOptions(
    [property: Description("@#mimeType")]string? MimeType = default,
	[property: Description("@#audioBitsPerSecond")]uint AudioBitsPerSecond = default,
	[property: Description("@#videoBitsPerSecond")]uint VideoBitsPerSecond = default,
	[property: Description("@#bitsPerSecond")]uint BitsPerSecond = default,
	[property: Description("@#audioBitrateMode")]BitrateMode AudioBitrateMode = BitrateMode.Variable,
	[property: Description("@#videoKeyFrameIntervalDuration")]double VideoKeyFrameIntervalDuration = default,
	[property: Description("@#videoKeyFrameIntervalCount")]uint VideoKeyFrameIntervalCount = default);

/// <summary>
/// BlobEventInit
/// </summary>
[ECMAScript]
[Description("@#BlobEventInit")]
public record BlobEventInit(
    [property: Description("@#data")]Blob? Data = default,
	[property: Description("@#timecode")]double Timecode = default): EventInit;

/// <summary>
/// MediaRecorder
/// </summary>
[ECMAScript]
[Description("@#MediaRecorder")]
public class MediaRecorder : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="stream">stream</param>
    /// <param name="options">options</param>
    public extern MediaRecorder(MediaStream stream, MediaRecorderOptions options);

    /// <summary>
    /// stream
    /// </summary>
    [Description("@#stream")]
    public extern MediaStream Stream { get; }

    /// <summary>
    /// mimeType
    /// </summary>
    [Description("@#mimeType")]
    public extern string MimeType { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern RecordingState State { get; }

    /// <summary>
    /// onstart
    /// </summary>
    [Description("@#onstart")]
    public extern EventHandler Onstart { get; set; }

    /// <summary>
    /// onstop
    /// </summary>
    [Description("@#onstop")]
    public extern EventHandler Onstop { get; set; }

    /// <summary>
    /// ondataavailable
    /// </summary>
    [Description("@#ondataavailable")]
    public extern EventHandler Ondataavailable { get; set; }

    /// <summary>
    /// onpause
    /// </summary>
    [Description("@#onpause")]
    public extern EventHandler Onpause { get; set; }

    /// <summary>
    /// onresume
    /// </summary>
    [Description("@#onresume")]
    public extern EventHandler Onresume { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// videoBitsPerSecond
    /// </summary>
    [Description("@#videoBitsPerSecond")]
    public extern uint VideoBitsPerSecond { get; }

    /// <summary>
    /// audioBitsPerSecond
    /// </summary>
    [Description("@#audioBitsPerSecond")]
    public extern uint AudioBitsPerSecond { get; }

    /// <summary>
    /// audioBitrateMode
    /// </summary>
    [Description("@#audioBitrateMode")]
    public extern BitrateMode AudioBitrateMode { get; }

    /// <summary>
    /// start
    /// </summary>
    /// <param name="timeslice">timeslice</param>
    [Description("@#start")]
    public extern void Start(uint timeslice);

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// pause
    /// </summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>
    /// resume
    /// </summary>
    [Description("@#resume")]
    public extern void Resume();

    /// <summary>
    /// requestData
    /// </summary>
    [Description("@#requestData")]
    public extern void RequestData();

    /// <summary>
    /// isTypeSupported
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#isTypeSupported")]
    public extern static bool IsTypeSupported(string type);
}

/// <summary>
/// BlobEvent
/// </summary>
[ECMAScript]
[Description("@#BlobEvent")]
public class BlobEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern BlobEvent(string type, BlobEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern Blob Data { get; }

    /// <summary>
    /// timecode
    /// </summary>
    [Description("@#timecode")]
    public extern double Timecode { get; }
}