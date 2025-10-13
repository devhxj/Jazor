namespace ECMAScript;

/// <summary>
/// BufferedChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#BufferedChangeEventInit")]
public record BufferedChangeEventInit(
    [property: Description("@#addedRanges")]TimeRanges? AddedRanges = default,
	[property: Description("@#removedRanges")]TimeRanges? RemovedRanges = default): EventInit;

/// <summary>
/// MediaSource
/// </summary>
[ECMAScript]
[Description("@#MediaSource")]
public class MediaSource : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern MediaSource();

    /// <summary>
    /// handle
    /// </summary>
    [Description("@#handle")]
    public extern MediaSourceHandle Handle { get; }

    /// <summary>
    /// sourceBuffers
    /// </summary>
    [Description("@#sourceBuffers")]
    public extern SourceBufferList SourceBuffers { get; }

    /// <summary>
    /// activeSourceBuffers
    /// </summary>
    [Description("@#activeSourceBuffers")]
    public extern SourceBufferList ActiveSourceBuffers { get; }

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ReadyState ReadyState { get; }

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern double Duration { get; set; }

    /// <summary>
    /// onsourceopen
    /// </summary>
    [Description("@#onsourceopen")]
    public extern EventHandler Onsourceopen { get; set; }

    /// <summary>
    /// onsourceended
    /// </summary>
    [Description("@#onsourceended")]
    public extern EventHandler Onsourceended { get; set; }

    /// <summary>
    /// onsourceclose
    /// </summary>
    [Description("@#onsourceclose")]
    public extern EventHandler Onsourceclose { get; set; }

    /// <summary>
    /// canConstructInDedicatedWorker
    /// </summary>
    [Description("@#canConstructInDedicatedWorker")]
    public extern static bool CanConstructInDedicatedWorker { get; }

    /// <summary>
    /// addSourceBuffer
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#addSourceBuffer")]
    public extern SourceBuffer AddSourceBuffer(string type);

    /// <summary>
    /// removeSourceBuffer
    /// </summary>
    /// <param name="sourceBuffer">sourceBuffer</param>
    [Description("@#removeSourceBuffer")]
    public extern void RemoveSourceBuffer(SourceBuffer sourceBuffer);

    /// <summary>
    /// endOfStream
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#endOfStream")]
    public extern void EndOfStream(EndOfStreamError error);

    /// <summary>
    /// setLiveSeekableRange
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    [Description("@#setLiveSeekableRange")]
    public extern void SetLiveSeekableRange(double start, double end);

    /// <summary>
    /// clearLiveSeekableRange
    /// </summary>
    [Description("@#clearLiveSeekableRange")]
    public extern void ClearLiveSeekableRange();

    /// <summary>
    /// isTypeSupported
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#isTypeSupported")]
    public extern static bool IsTypeSupported(string type);
}

/// <summary>
/// MediaSourceHandle
/// </summary>
[ECMAScript]
[Description("@#MediaSourceHandle")]
public class MediaSourceHandle
{

}

/// <summary>
/// SourceBuffer
/// </summary>
[ECMAScript]
[Description("@#SourceBuffer")]
public class SourceBuffer : EventTarget
{
    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern AppendMode Mode { get; set; }

    /// <summary>
    /// updating
    /// </summary>
    [Description("@#updating")]
    public extern bool Updating { get; }

    /// <summary>
    /// buffered
    /// </summary>
    [Description("@#buffered")]
    public extern TimeRanges Buffered { get; }

    /// <summary>
    /// timestampOffset
    /// </summary>
    [Description("@#timestampOffset")]
    public extern double TimestampOffset { get; set; }

    /// <summary>
    /// audioTracks
    /// </summary>
    [Description("@#audioTracks")]
    public extern AudioTrackList AudioTracks { get; }

    /// <summary>
    /// videoTracks
    /// </summary>
    [Description("@#videoTracks")]
    public extern VideoTrackList VideoTracks { get; }

    /// <summary>
    /// textTracks
    /// </summary>
    [Description("@#textTracks")]
    public extern TextTrackList TextTracks { get; }

    /// <summary>
    /// appendWindowStart
    /// </summary>
    [Description("@#appendWindowStart")]
    public extern double AppendWindowStart { get; set; }

    /// <summary>
    /// appendWindowEnd
    /// </summary>
    [Description("@#appendWindowEnd")]
    public extern double AppendWindowEnd { get; set; }

    /// <summary>
    /// onupdatestart
    /// </summary>
    [Description("@#onupdatestart")]
    public extern EventHandler Onupdatestart { get; set; }

    /// <summary>
    /// onupdate
    /// </summary>
    [Description("@#onupdate")]
    public extern EventHandler Onupdate { get; set; }

    /// <summary>
    /// onupdateend
    /// </summary>
    [Description("@#onupdateend")]
    public extern EventHandler Onupdateend { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// appendBuffer
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#appendBuffer")]
    public extern void AppendBuffer(IBufferSource data);

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern void Abort();

    /// <summary>
    /// changeType
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#changeType")]
    public extern void ChangeType(string type);

    /// <summary>
    /// remove
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    [Description("@#remove")]
    public extern void Remove(double start, double end);
}

/// <summary>
/// SourceBufferList
/// </summary>
[ECMAScript]
[Description("@#SourceBufferList")]
public class SourceBufferList : EventTarget
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// onaddsourcebuffer
    /// </summary>
    [Description("@#onaddsourcebuffer")]
    public extern EventHandler Onaddsourcebuffer { get; set; }

    /// <summary>
    /// onremovesourcebuffer
    /// </summary>
    [Description("@#onremovesourcebuffer")]
    public extern EventHandler Onremovesourcebuffer { get; set; }

    [Description("@#")]
    public extern SourceBuffer this[uint index] { get; }
}

/// <summary>
/// ManagedMediaSource
/// </summary>
[ECMAScript]
[Description("@#ManagedMediaSource")]
public class ManagedMediaSource : MediaSource
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern ManagedMediaSource();

    /// <summary>
    /// streaming
    /// </summary>
    [Description("@#streaming")]
    public extern bool Streaming { get; }

    /// <summary>
    /// onstartstreaming
    /// </summary>
    [Description("@#onstartstreaming")]
    public extern EventHandler Onstartstreaming { get; set; }

    /// <summary>
    /// onendstreaming
    /// </summary>
    [Description("@#onendstreaming")]
    public extern EventHandler Onendstreaming { get; set; }
}

/// <summary>
/// BufferedChangeEvent
/// </summary>
[ECMAScript]
[Description("@#BufferedChangeEvent")]
public class BufferedChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern BufferedChangeEvent(string type, BufferedChangeEventInit eventInitDict);

    /// <summary>
    /// addedRanges
    /// </summary>
    [Description("@#addedRanges")]
    public extern TimeRanges AddedRanges { get; }

    /// <summary>
    /// removedRanges
    /// </summary>
    [Description("@#removedRanges")]
    public extern TimeRanges RemovedRanges { get; }
}

/// <summary>
/// ManagedSourceBuffer
/// </summary>
[ECMAScript]
[Description("@#ManagedSourceBuffer")]
public class ManagedSourceBuffer : SourceBuffer
{
    /// <summary>
    /// onbufferedchange
    /// </summary>
    [Description("@#onbufferedchange")]
    public extern EventHandler Onbufferedchange { get; set; }
}