namespace ECMAScript;

/// <summary>
/// ReadableStreamGetReaderOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamGetReaderOptions")]
public record ReadableStreamGetReaderOptions(
    [property: Description("@#mode")]ReadableStreamReaderMode? Mode = default);

/// <summary>
/// ReadableStreamIteratorOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamIteratorOptions")]
public record ReadableStreamIteratorOptions(
    [property: Description("@#preventCancel")]bool PreventCancel = false);

/// <summary>
/// ReadableWritablePair
/// </summary>
[ECMAScript]
[Description("@#ReadableWritablePair")]
public record ReadableWritablePair(
    [property: Description("@#readable")]ReadableStream? Readable = default,
	[property: Description("@#writable")]WritableStream? Writable = default);

/// <summary>
/// StreamPipeOptions
/// </summary>
[ECMAScript]
[Description("@#StreamPipeOptions")]
public record StreamPipeOptions(
    [property: Description("@#preventClose")]bool PreventClose = false,
	[property: Description("@#preventAbort")]bool PreventAbort = false,
	[property: Description("@#preventCancel")]bool PreventCancel = false,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// UnderlyingSource
/// </summary>
[ECMAScript]
[Description("@#UnderlyingSource")]
public record UnderlyingSource(
    [property: Description("@#start")]UnderlyingSourceStartCallback? Start = default,
	[property: Description("@#pull")]UnderlyingSourcePullCallback? Pull = default,
	[property: Description("@#cancel")]UnderlyingSourceCancelCallback? Cancel = default,
	[property: Description("@#type")]ReadableStreamType? Type = default,
	[property: Description("@#autoAllocateChunkSize")]ulong AutoAllocateChunkSize = default);

/// <summary>
/// ReadableStreamReadResult
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamReadResult")]
public record ReadableStreamReadResult(
    [property: Description("@#value")]object? Value = default,
	[property: Description("@#done")]bool Done = default);

/// <summary>
/// ReadableStreamBYOBReaderReadOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamBYOBReaderReadOptions")]
public record ReadableStreamBYOBReaderReadOptions(
    [property: Description("@#min")]ulong Min = 1);

/// <summary>
/// UnderlyingSink
/// </summary>
[ECMAScript]
[Description("@#UnderlyingSink")]
public record UnderlyingSink(
    [property: Description("@#start")]UnderlyingSinkStartCallback? Start = default,
	[property: Description("@#write")]UnderlyingSinkWriteCallback? Write = default,
	[property: Description("@#close")]UnderlyingSinkCloseCallback? Close = default,
	[property: Description("@#abort")]UnderlyingSinkAbortCallback? Abort = default,
	[property: Description("@#type")]object? Type = default);

/// <summary>
/// Transformer
/// </summary>
[ECMAScript]
[Description("@#Transformer")]
public record Transformer(
    [property: Description("@#start")]TransformerStartCallback? Start = default,
	[property: Description("@#transform")]TransformerTransformCallback? Transform = default,
	[property: Description("@#flush")]TransformerFlushCallback? Flush = default,
	[property: Description("@#cancel")]TransformerCancelCallback? Cancel = default,
	[property: Description("@#readableType")]object? ReadableType = default,
	[property: Description("@#writableType")]object? WritableType = default);

/// <summary>
/// QueuingStrategy
/// </summary>
[ECMAScript]
[Description("@#QueuingStrategy")]
public record QueuingStrategy(
    [property: Description("@#highWaterMark")]double HighWaterMark = default,
	[property: Description("@#size")]QueuingStrategySize? Size = default);

/// <summary>
/// QueuingStrategyInit
/// </summary>
[ECMAScript]
[Description("@#QueuingStrategyInit")]
public record QueuingStrategyInit(
    [property: Description("@#highWaterMark")]double HighWaterMark = default);

/// <summary>
/// ReadableStream
/// </summary>
[ECMAScript]
[Description("@#ReadableStream")]
public class ReadableStream : IEnumerable<object>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="underlyingSource">underlyingSource</param>
    /// <param name="strategy">strategy</param>
    public extern ReadableStream(object underlyingSource, QueuingStrategy strategy);

    /// <summary>
    /// from
    /// </summary>
    /// <param name="asyncIterable">asyncIterable</param>
    [Description("@#from")]
    public extern static ReadableStream From(object asyncIterable);

    /// <summary>
    /// locked
    /// </summary>
    [Description("@#locked")]
    public extern bool Locked { get; }

    /// <summary>
    /// cancel
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#cancel")]
    public extern PromiseResult<object> Cancel(object reason);

    /// <summary>
    /// getReader
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getReader")]
    public extern ReadableStreamReader GetReader(ReadableStreamGetReaderOptions? options = default);

    /// <summary>
    /// pipeThrough
    /// </summary>
    /// <param name="transform">transform</param>
    /// <param name="options">options</param>
    [Description("@#pipeThrough")]
    public extern ReadableStream PipeThrough(ReadableWritablePair transform, StreamPipeOptions? options = default);

    /// <summary>
    /// pipeTo
    /// </summary>
    /// <param name="destination">destination</param>
    /// <param name="options">options</param>
    [Description("@#pipeTo")]
    public extern PromiseResult<object> PipeTo(WritableStream destination, StreamPipeOptions? options = default);

    /// <summary>
    /// tee
    /// </summary>
    [Description("@#tee")]
    public extern ReadableStream[] Tee();

    extern IEnumerator<object> IEnumerable<object>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// ReadableStreamDefaultReader
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamDefaultReader")]
public class ReadableStreamDefaultReader
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="stream">stream</param>
    public extern ReadableStreamDefaultReader(ReadableStream stream);

    /// <summary>
    /// read
    /// </summary>
    [Description("@#read")]
    public extern PromiseResult<ReadableStreamReadResult> Read();

    /// <summary>
    /// releaseLock
    /// </summary>
    [Description("@#releaseLock")]
    public extern void ReleaseLock();

    #region mixin ReadableStreamGenericReader
    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern PromiseResult<object> Closed { get; }

    /// <summary>
    /// cancel
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#cancel")]
    public extern PromiseResult<object> Cancel(object reason);
    #endregion
}

/// <summary>
/// ReadableStreamBYOBReader
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamBYOBReader")]
public class ReadableStreamBYOBReader
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="stream">stream</param>
    public extern ReadableStreamBYOBReader(ReadableStream stream);

    /// <summary>
    /// read
    /// </summary>
    /// <param name="view">view</param>
    /// <param name="options">options</param>
    [Description("@#read")]
    public extern PromiseResult<ReadableStreamReadResult> Read(IArrayBufferView view, ReadableStreamBYOBReaderReadOptions? options = default);

    /// <summary>
    /// releaseLock
    /// </summary>
    [Description("@#releaseLock")]
    public extern void ReleaseLock();

    #region mixin ReadableStreamGenericReader
    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern PromiseResult<object> Closed { get; }

    /// <summary>
    /// cancel
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#cancel")]
    public extern PromiseResult<object> Cancel(object reason);
    #endregion
}

/// <summary>
/// ReadableStreamDefaultController
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamDefaultController")]
public class ReadableStreamDefaultController
{
    /// <summary>
    /// desiredSize
    /// </summary>
    [Description("@#desiredSize")]
    public extern double? DesiredSize { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// enqueue
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#enqueue")]
    public extern void Enqueue(object chunk);

    /// <summary>
    /// error
    /// </summary>
    /// <param name="e">e</param>
    [Description("@#error")]
    public extern void Error(object e);
}

/// <summary>
/// ReadableByteStreamController
/// </summary>
[ECMAScript]
[Description("@#ReadableByteStreamController")]
public class ReadableByteStreamController
{
    /// <summary>
    /// byobRequest
    /// </summary>
    [Description("@#byobRequest")]
    public extern ReadableStreamBYOBRequest? ByobRequest { get; }

    /// <summary>
    /// desiredSize
    /// </summary>
    [Description("@#desiredSize")]
    public extern double? DesiredSize { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// enqueue
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#enqueue")]
    public extern void Enqueue(IArrayBufferView chunk);

    /// <summary>
    /// error
    /// </summary>
    /// <param name="e">e</param>
    [Description("@#error")]
    public extern void Error(object e);
}

/// <summary>
/// ReadableStreamBYOBRequest
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamBYOBRequest")]
public class ReadableStreamBYOBRequest
{
    /// <summary>
    /// view
    /// </summary>
    [Description("@#view")]
    public extern IArrayBufferView? View { get; }

    /// <summary>
    /// respond
    /// </summary>
    /// <param name="bytesWritten">bytesWritten</param>
    [Description("@#respond")]
    public extern void Respond(ulong bytesWritten);

    /// <summary>
    /// respondWithNewView
    /// </summary>
    /// <param name="view">view</param>
    [Description("@#respondWithNewView")]
    public extern void RespondWithNewView(IArrayBufferView view);
}

/// <summary>
/// WritableStream
/// </summary>
[ECMAScript]
[Description("@#WritableStream")]
public class WritableStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="underlyingSink">underlyingSink</param>
    /// <param name="strategy">strategy</param>
    public extern WritableStream(object underlyingSink, QueuingStrategy strategy);

    /// <summary>
    /// locked
    /// </summary>
    [Description("@#locked")]
    public extern bool Locked { get; }

    /// <summary>
    /// abort
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#abort")]
    public extern PromiseResult<object> Abort(object reason);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<object> Close();

    /// <summary>
    /// getWriter
    /// </summary>
    [Description("@#getWriter")]
    public extern WritableStreamDefaultWriter GetWriter();
}

/// <summary>
/// WritableStreamDefaultWriter
/// </summary>
[ECMAScript]
[Description("@#WritableStreamDefaultWriter")]
public class WritableStreamDefaultWriter
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="stream">stream</param>
    public extern WritableStreamDefaultWriter(WritableStream stream);

    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern PromiseResult<object> Closed { get; }

    /// <summary>
    /// desiredSize
    /// </summary>
    [Description("@#desiredSize")]
    public extern double? DesiredSize { get; }

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<object> Ready { get; }

    /// <summary>
    /// abort
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#abort")]
    public extern PromiseResult<object> Abort(object reason);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<object> Close();

    /// <summary>
    /// releaseLock
    /// </summary>
    [Description("@#releaseLock")]
    public extern void ReleaseLock();

    /// <summary>
    /// write
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#write")]
    public extern PromiseResult<object> Write(object chunk);
}

/// <summary>
/// WritableStreamDefaultController
/// </summary>
[ECMAScript]
[Description("@#WritableStreamDefaultController")]
public class WritableStreamDefaultController
{
    /// <summary>
    /// signal
    /// </summary>
    [Description("@#signal")]
    public extern AbortSignal Signal { get; }

    /// <summary>
    /// error
    /// </summary>
    /// <param name="e">e</param>
    [Description("@#error")]
    public extern void Error(object e);
}

/// <summary>
/// TransformStream
/// </summary>
[ECMAScript]
[Description("@#TransformStream")]
public class TransformStream
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="transformer">transformer</param>
    /// <param name="writableStrategy">writableStrategy</param>
    /// <param name="readableStrategy">readableStrategy</param>
    public extern TransformStream(object transformer, QueuingStrategy writableStrategy, QueuingStrategy readableStrategy);

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
}

/// <summary>
/// TransformStreamDefaultController
/// </summary>
[ECMAScript]
[Description("@#TransformStreamDefaultController")]
public class TransformStreamDefaultController
{
    /// <summary>
    /// desiredSize
    /// </summary>
    [Description("@#desiredSize")]
    public extern double? DesiredSize { get; }

    /// <summary>
    /// enqueue
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#enqueue")]
    public extern void Enqueue(object chunk);

    /// <summary>
    /// error
    /// </summary>
    /// <param name="reason">reason</param>
    [Description("@#error")]
    public extern void Error(object reason);

    /// <summary>
    /// terminate
    /// </summary>
    [Description("@#terminate")]
    public extern void Terminate();
}

/// <summary>
/// ByteLengthQueuingStrategy
/// </summary>
[ECMAScript]
[Description("@#ByteLengthQueuingStrategy")]
public class ByteLengthQueuingStrategy
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern ByteLengthQueuingStrategy(QueuingStrategyInit init);

    /// <summary>
    /// highWaterMark
    /// </summary>
    [Description("@#highWaterMark")]
    public extern double HighWaterMark { get; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern Delegate Size { get; }
}

/// <summary>
/// CountQueuingStrategy
/// </summary>
[ECMAScript]
[Description("@#CountQueuingStrategy")]
public class CountQueuingStrategy
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern CountQueuingStrategy(QueuingStrategyInit init);

    /// <summary>
    /// highWaterMark
    /// </summary>
    [Description("@#highWaterMark")]
    public extern double HighWaterMark { get; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern Delegate Size { get; }
}