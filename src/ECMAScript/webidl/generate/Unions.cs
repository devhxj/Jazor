namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public readonly struct AlgorithmIdentifier
{
    private readonly byte _kind;
    private readonly object? _value1;
    private readonly string? _value2;

    private AlgorithmIdentifier(object value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AlgorithmIdentifier(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public object? AsObject => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static AlgorithmIdentifier FromObject(object value)
        => new(value);

    public static implicit operator AlgorithmIdentifier(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct AllowSharedBufferSource
{
    private readonly byte _kind;
    private readonly ArrayBuffer? _value1;
    private readonly SharedArrayBuffer? _value2;
    private readonly IArrayBufferView? _value3;

    private AllowSharedBufferSource(ArrayBuffer value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private AllowSharedBufferSource(SharedArrayBuffer value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private AllowSharedBufferSource(IArrayBufferView value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public ArrayBuffer? AsArrayBuffer => _kind == 1 ? _value1 : default;

    public SharedArrayBuffer? AsSharedArrayBuffer => _kind == 2 ? _value2 : default;

    public IArrayBufferView? AsIArrayBufferView => _kind == 3 ? _value3 : default;

    public static implicit operator AllowSharedBufferSource(ArrayBuffer value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(SharedArrayBuffer value)
        => new(value);

    public static AllowSharedBufferSource FromIArrayBufferView(IArrayBufferView value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(DataView value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Uint8Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Uint8ClampedArray value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Int8Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Int16Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Uint16Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Int32Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Uint32Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Float16Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Float32Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(Float64Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(BigInt64Array value)
        => new(value);

    public static implicit operator AllowSharedBufferSource(BigUint64Array value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct BlobPart
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private BlobPart(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private BlobPart(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private BlobPart(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static BlobPart FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator BlobPart(ArrayBuffer value)
        => new(value);

    public static implicit operator BlobPart(DataView value)
        => new(value);

    public static implicit operator BlobPart(Uint8Array value)
        => new(value);

    public static implicit operator BlobPart(Uint8ClampedArray value)
        => new(value);

    public static implicit operator BlobPart(Int8Array value)
        => new(value);

    public static implicit operator BlobPart(Int16Array value)
        => new(value);

    public static implicit operator BlobPart(Uint16Array value)
        => new(value);

    public static implicit operator BlobPart(Int32Array value)
        => new(value);

    public static implicit operator BlobPart(Uint32Array value)
        => new(value);

    public static implicit operator BlobPart(Float16Array value)
        => new(value);

    public static implicit operator BlobPart(Float32Array value)
        => new(value);

    public static implicit operator BlobPart(Float64Array value)
        => new(value);

    public static implicit operator BlobPart(BigInt64Array value)
        => new(value);

    public static implicit operator BlobPart(BigUint64Array value)
        => new(value);

    public static implicit operator BlobPart(Blob value)
        => new(value);

    public static implicit operator BlobPart(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct BufferSource
{
    private readonly byte _kind;
    private readonly IArrayBufferView? _value1;
    private readonly ArrayBuffer? _value2;

    private BufferSource(IArrayBufferView value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BufferSource(ArrayBuffer value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IArrayBufferView? AsIArrayBufferView => _kind == 1 ? _value1 : default;

    public ArrayBuffer? AsArrayBuffer => _kind == 2 ? _value2 : default;

    public static BufferSource FromIArrayBufferView(IArrayBufferView value)
        => new(value);

    public static implicit operator BufferSource(DataView value)
        => new(value);

    public static implicit operator BufferSource(Uint8Array value)
        => new(value);

    public static implicit operator BufferSource(Uint8ClampedArray value)
        => new(value);

    public static implicit operator BufferSource(Int8Array value)
        => new(value);

    public static implicit operator BufferSource(Int16Array value)
        => new(value);

    public static implicit operator BufferSource(Uint16Array value)
        => new(value);

    public static implicit operator BufferSource(Int32Array value)
        => new(value);

    public static implicit operator BufferSource(Uint32Array value)
        => new(value);

    public static implicit operator BufferSource(Float16Array value)
        => new(value);

    public static implicit operator BufferSource(Float32Array value)
        => new(value);

    public static implicit operator BufferSource(Float64Array value)
        => new(value);

    public static implicit operator BufferSource(BigInt64Array value)
        => new(value);

    public static implicit operator BufferSource(BigUint64Array value)
        => new(value);

    public static implicit operator BufferSource(ArrayBuffer value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct FileSystemWriteChunkType
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;
    private readonly WriteParams? _value4;

    private FileSystemWriteChunkType(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    private FileSystemWriteChunkType(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    private FileSystemWriteChunkType(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    private FileSystemWriteChunkType(WriteParams value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public WriteParams? AsWriteParams => _kind == 4 ? _value4 : default;

    public static FileSystemWriteChunkType FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(ArrayBuffer value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(DataView value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Uint8Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Uint8ClampedArray value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Int8Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Int16Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Uint16Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Int32Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Uint32Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Float16Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Float32Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Float64Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(BigInt64Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(BigUint64Array value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(Blob value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(string value)
        => new(value);

    public static implicit operator FileSystemWriteChunkType(WriteParams value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct FontFaceSourceValue
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly IBufferSource? _value2;

    private FontFaceSourceValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FontFaceSourceValue(IBufferSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    public static implicit operator FontFaceSourceValue(string value)
        => new(value);

    public static FontFaceSourceValue FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator FontFaceSourceValue(ArrayBuffer value)
        => new(value);

    public static implicit operator FontFaceSourceValue(DataView value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Uint8Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Uint8ClampedArray value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Int8Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Int16Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Uint16Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Int32Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Uint32Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Float16Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Float32Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(Float64Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(BigInt64Array value)
        => new(value);

    public static implicit operator FontFaceSourceValue(BigUint64Array value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct IDBCursorSource
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;

    private IDBCursorSource(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IDBCursorSource(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public static IDBCursorSource FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static IDBCursorSource FromIDBIndex(IDBIndex value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct IDBRequestSource
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;
    private readonly IDBCursor? _value3;

    private IDBRequestSource(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private IDBRequestSource(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private IDBRequestSource(IDBCursor value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public IDBCursor? AsIDBCursor => _kind == 3 ? _value3 : default;

    public static IDBRequestSource FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static IDBRequestSource FromIDBIndex(IDBIndex value)
        => new(value);

    public static IDBRequestSource FromIDBCursor(IDBCursor value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct ImageBufferSource
{
    private readonly byte _kind;
    private readonly IAllowSharedBufferSource? _value1;
    private readonly ReadableStream? _value2;

    private ImageBufferSource(IAllowSharedBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ImageBufferSource(ReadableStream value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IAllowSharedBufferSource? AsIAllowSharedBufferSource => _kind == 1 ? _value1 : default;

    public ReadableStream? AsReadableStream => _kind == 2 ? _value2 : default;

    public static ImageBufferSource FromIAllowSharedBufferSource(IAllowSharedBufferSource value)
        => new(value);

    public static implicit operator ImageBufferSource(ArrayBuffer value)
        => new(value);

    public static implicit operator ImageBufferSource(SharedArrayBuffer value)
        => new(value);

    public static implicit operator ImageBufferSource(DataView value)
        => new(value);

    public static implicit operator ImageBufferSource(Uint8Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Uint8ClampedArray value)
        => new(value);

    public static implicit operator ImageBufferSource(Int8Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Int16Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Uint16Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Int32Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Uint32Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Float16Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Float32Array value)
        => new(value);

    public static implicit operator ImageBufferSource(Float64Array value)
        => new(value);

    public static implicit operator ImageBufferSource(BigInt64Array value)
        => new(value);

    public static implicit operator ImageBufferSource(BigUint64Array value)
        => new(value);

    public static implicit operator ImageBufferSource(ReadableStream value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct LanguageModelMessageValue
{
    private readonly byte _kind;
    private readonly ImageBitmapSource? _value1;
    private readonly AudioBuffer? _value2;
    private readonly IBufferSource? _value3;
    private readonly string? _value4;

    private LanguageModelMessageValue(ImageBitmapSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    private LanguageModelMessageValue(AudioBuffer value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    private LanguageModelMessageValue(IBufferSource value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    private LanguageModelMessageValue(string value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public ImageBitmapSource? AsImageBitmapSource => _kind == 1 ? _value1 : default;

    public AudioBuffer? AsAudioBuffer => _kind == 2 ? _value2 : default;

    public IBufferSource? AsIBufferSource => _kind == 3 ? _value3 : default;

    public string? AsString => _kind == 4 ? _value4 : default;

    public static implicit operator LanguageModelMessageValue(ImageBitmapSource value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(AudioBuffer value)
        => new(value);

    public static LanguageModelMessageValue FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(ArrayBuffer value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(DataView value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Uint8Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Uint8ClampedArray value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Int8Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Int16Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Uint16Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Int32Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Uint32Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Float16Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Float32Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(Float64Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(BigInt64Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(BigUint64Array value)
        => new(value);

    public static implicit operator LanguageModelMessageValue(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct NDEFMessageSource
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly IBufferSource? _value2;
    private readonly NDEFMessageInit? _value3;

    private NDEFMessageSource(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private NDEFMessageSource(IBufferSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private NDEFMessageSource(NDEFMessageInit value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    public NDEFMessageInit? AsNDEFMessageInit => _kind == 3 ? _value3 : default;

    public static implicit operator NDEFMessageSource(string value)
        => new(value);

    public static NDEFMessageSource FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator NDEFMessageSource(ArrayBuffer value)
        => new(value);

    public static implicit operator NDEFMessageSource(DataView value)
        => new(value);

    public static implicit operator NDEFMessageSource(Uint8Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Uint8ClampedArray value)
        => new(value);

    public static implicit operator NDEFMessageSource(Int8Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Int16Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Uint16Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Int32Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Uint32Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Float16Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Float32Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(Float64Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(BigInt64Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(BigUint64Array value)
        => new(value);

    public static implicit operator NDEFMessageSource(NDEFMessageInit value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct PushMessageDataInit
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly string? _value2;

    private PushMessageDataInit(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PushMessageDataInit(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static PushMessageDataInit FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator PushMessageDataInit(ArrayBuffer value)
        => new(value);

    public static implicit operator PushMessageDataInit(DataView value)
        => new(value);

    public static implicit operator PushMessageDataInit(Uint8Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Uint8ClampedArray value)
        => new(value);

    public static implicit operator PushMessageDataInit(Int8Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Int16Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Uint16Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Int32Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Uint32Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Float16Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Float32Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(Float64Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(BigInt64Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(BigUint64Array value)
        => new(value);

    public static implicit operator PushMessageDataInit(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct PushSubscriptionOptionsInitApplicationServerKey
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly string? _value2;

    private PushSubscriptionOptionsInitApplicationServerKey(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PushSubscriptionOptionsInitApplicationServerKey(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static PushSubscriptionOptionsInitApplicationServerKey FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(ArrayBuffer value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(DataView value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint8Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint8ClampedArray value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int8Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int16Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint16Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int32Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint32Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float16Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float32Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float64Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(BigInt64Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(BigUint64Array value)
        => new(value);

    public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct SendData
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private SendData(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private SendData(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private SendData(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static SendData FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator SendData(ArrayBuffer value)
        => new(value);

    public static implicit operator SendData(DataView value)
        => new(value);

    public static implicit operator SendData(Uint8Array value)
        => new(value);

    public static implicit operator SendData(Uint8ClampedArray value)
        => new(value);

    public static implicit operator SendData(Int8Array value)
        => new(value);

    public static implicit operator SendData(Int16Array value)
        => new(value);

    public static implicit operator SendData(Uint16Array value)
        => new(value);

    public static implicit operator SendData(Int32Array value)
        => new(value);

    public static implicit operator SendData(Uint32Array value)
        => new(value);

    public static implicit operator SendData(Float16Array value)
        => new(value);

    public static implicit operator SendData(Float32Array value)
        => new(value);

    public static implicit operator SendData(Float64Array value)
        => new(value);

    public static implicit operator SendData(BigInt64Array value)
        => new(value);

    public static implicit operator SendData(BigUint64Array value)
        => new(value);

    public static implicit operator SendData(Blob value)
        => new(value);

    public static implicit operator SendData(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue13
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly IBufferSource? _value2;

    private StructuralCacheValue13(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue13(IBufferSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue13(string value)
        => new(value);

    public static StructuralCacheValue13 FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator StructuralCacheValue13(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValue13(DataView value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Uint8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Uint8ClampedArray value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Int8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Int16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Uint16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Float16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Float32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(Float64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(BigInt64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue13(BigUint64Array value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue38
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;
    private readonly IDBCursor? _value3;

    private StructuralCacheValue38(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue38(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue38(IDBCursor value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public IDBCursor? AsIDBCursor => _kind == 3 ? _value3 : default;

    public static StructuralCacheValue38 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static StructuralCacheValue38 FromIDBIndex(IDBIndex value)
        => new(value);

    public static StructuralCacheValue38 FromIDBCursor(IDBCursor value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue40
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;

    private StructuralCacheValue40(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue40(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public static StructuralCacheValue40 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static StructuralCacheValue40 FromIDBIndex(IDBIndex value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue53
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly JsonWebKey? _value2;

    private StructuralCacheValue53(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue53(JsonWebKey value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public JsonWebKey? AsJsonWebKey => _kind == 2 ? _value2 : default;

    public static StructuralCacheValue53 FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator StructuralCacheValue53(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValue53(DataView value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Uint8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Uint8ClampedArray value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Int8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Int16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Uint16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Float16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Float32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(Float64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(BigInt64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(BigUint64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue53(JsonWebKey value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue60
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private StructuralCacheValue60(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue60(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue60(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static StructuralCacheValue60 FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator StructuralCacheValue60(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValue60(DataView value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Uint8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Uint8ClampedArray value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Int8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Int16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Uint16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Float16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Float32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Float64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(BigInt64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(BigUint64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue60(Blob value)
        => new(value);

    public static implicit operator StructuralCacheValue60(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct SubtleCryptoImportKeyKeyData
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly JsonWebKey? _value2;

    private SubtleCryptoImportKeyKeyData(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SubtleCryptoImportKeyKeyData(JsonWebKey value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public JsonWebKey? AsJsonWebKey => _kind == 2 ? _value2 : default;

    public static SubtleCryptoImportKeyKeyData FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(ArrayBuffer value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(DataView value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Uint8Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Uint8ClampedArray value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Int8Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Int16Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Uint16Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Int32Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Uint32Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Float16Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Float32Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(Float64Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(BigInt64Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(BigUint64Array value)
        => new(value);

    public static implicit operator SubtleCryptoImportKeyKeyData(JsonWebKey value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct WebSocketSendData
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private WebSocketSendData(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private WebSocketSendData(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private WebSocketSendData(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static WebSocketSendData FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator WebSocketSendData(ArrayBuffer value)
        => new(value);

    public static implicit operator WebSocketSendData(DataView value)
        => new(value);

    public static implicit operator WebSocketSendData(Uint8Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Uint8ClampedArray value)
        => new(value);

    public static implicit operator WebSocketSendData(Int8Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Int16Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Uint16Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Int32Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Uint32Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Float16Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Float32Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Float64Array value)
        => new(value);

    public static implicit operator WebSocketSendData(BigInt64Array value)
        => new(value);

    public static implicit operator WebSocketSendData(BigUint64Array value)
        => new(value);

    public static implicit operator WebSocketSendData(Blob value)
        => new(value);

    public static implicit operator WebSocketSendData(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct WriteParamsData
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private WriteParamsData(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private WriteParamsData(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private WriteParamsData(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static WriteParamsData FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator WriteParamsData(ArrayBuffer value)
        => new(value);

    public static implicit operator WriteParamsData(DataView value)
        => new(value);

    public static implicit operator WriteParamsData(Uint8Array value)
        => new(value);

    public static implicit operator WriteParamsData(Uint8ClampedArray value)
        => new(value);

    public static implicit operator WriteParamsData(Int8Array value)
        => new(value);

    public static implicit operator WriteParamsData(Int16Array value)
        => new(value);

    public static implicit operator WriteParamsData(Uint16Array value)
        => new(value);

    public static implicit operator WriteParamsData(Int32Array value)
        => new(value);

    public static implicit operator WriteParamsData(Uint32Array value)
        => new(value);

    public static implicit operator WriteParamsData(Float16Array value)
        => new(value);

    public static implicit operator WriteParamsData(Float32Array value)
        => new(value);

    public static implicit operator WriteParamsData(Float64Array value)
        => new(value);

    public static implicit operator WriteParamsData(BigInt64Array value)
        => new(value);

    public static implicit operator WriteParamsData(BigUint64Array value)
        => new(value);

    public static implicit operator WriteParamsData(Blob value)
        => new(value);

    public static implicit operator WriteParamsData(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
public readonly struct XMLHttpRequestBodyInit
{
    private readonly byte _kind;
    private readonly Blob? _value1;
    private readonly IBufferSource? _value2;
    private readonly FormData? _value3;
    private readonly URLSearchParams? _value4;
    private readonly string? _value5;

    private XMLHttpRequestBodyInit(Blob value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private XMLHttpRequestBodyInit(IBufferSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private XMLHttpRequestBodyInit(FormData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
    }

    private XMLHttpRequestBodyInit(URLSearchParams value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
    }

    private XMLHttpRequestBodyInit(string value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
    }

    public Blob? AsBlob => _kind == 1 ? _value1 : default;

    public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    public FormData? AsFormData => _kind == 3 ? _value3 : default;

    public URLSearchParams? AsURLSearchParams => _kind == 4 ? _value4 : default;

    public string? AsString => _kind == 5 ? _value5 : default;

    public static implicit operator XMLHttpRequestBodyInit(Blob value)
        => new(value);

    public static XMLHttpRequestBodyInit FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(ArrayBuffer value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(DataView value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Uint8Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Uint8ClampedArray value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Int8Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Int16Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Uint16Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Int32Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Uint32Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Float16Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Float32Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(Float64Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(BigInt64Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(BigUint64Array value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(FormData value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(URLSearchParams value)
        => new(value);

    public static implicit operator XMLHttpRequestBodyInit(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AddRoutesRulesCollectionBuilder), nameof(AddRoutesRulesCollectionBuilder.Create))]
public readonly union AddRoutesRules(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    public static implicit operator AddRoutesRules(RouterRule value)
        => new(value);

    public static implicit operator AddRoutesRules(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class AddRoutesRulesCollectionBuilder
{
    public static AddRoutesRules Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder), nameof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder.Create))]
public readonly union AllowedBluetoothDeviceAllowedServices(string, UUID[]) : IEnumerable<UUID>
{

    public string? AsString => Value is string value ? value : default(string?);

    public UUID[]? AsUUIDArray => Value is UUID[] value ? value : default(UUID[]?);

    public static implicit operator AllowedBluetoothDeviceAllowedServices(string value)
        => new(value);

    public static implicit operator AllowedBluetoothDeviceAllowedServices(UUID[] value)
        => new(value);

    IEnumerator<UUID> IEnumerable<UUID>.GetEnumerator()
        => ((IEnumerable<UUID>)(AsUUIDArray ?? Array.Empty<UUID>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<UUID>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllowedBluetoothDeviceAllowedServicesCollectionBuilder
{
    public static AllowedBluetoothDeviceAllowedServices Create(ReadOnlySpan<UUID> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BackgroundFetchManagerFetchRequestsCollectionBuilder), nameof(BackgroundFetchManagerFetchRequestsCollectionBuilder.Create))]
public readonly union BackgroundFetchManagerFetchRequests(RequestInfo, RequestInfo[]) : IEnumerable<RequestInfo>
{

    public RequestInfo? AsRequestInfo => Value is RequestInfo value ? value : default(RequestInfo?);

    public RequestInfo[]? AsRequestInfoArray => Value is RequestInfo[] value ? value : default(RequestInfo[]?);

    public static implicit operator BackgroundFetchManagerFetchRequests(RequestInfo value)
        => new(value);

    public static implicit operator BackgroundFetchManagerFetchRequests(RequestInfo[] value)
        => new(value);

    IEnumerator<RequestInfo> IEnumerable<RequestInfo>.GetEnumerator()
        => ((IEnumerable<RequestInfo>)(AsRequestInfoArray ?? Array.Empty<RequestInfo>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RequestInfo>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class BackgroundFetchManagerFetchRequestsCollectionBuilder
{
    public static BackgroundFetchManagerFetchRequests Create(ReadOnlySpan<RequestInfo> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeCompositeCollectionBuilder), nameof(BasePropertyIndexedKeyframeCompositeCollectionBuilder.Create))]
public readonly union BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto, CompositeOperationOrAuto[]) : IEnumerable<CompositeOperationOrAuto>
{

    public CompositeOperationOrAuto? AsCompositeOperationOrAuto => Value is CompositeOperationOrAuto value ? value : default(CompositeOperationOrAuto?);

    public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => Value is CompositeOperationOrAuto[] value ? value : default(CompositeOperationOrAuto[]?);

    public static implicit operator BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto value)
        => new(value);

    public static implicit operator BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto[] value)
        => new(value);

    IEnumerator<CompositeOperationOrAuto> IEnumerable<CompositeOperationOrAuto>.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)(AsCompositeOperationOrAutoArray ?? Array.Empty<CompositeOperationOrAuto>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeCompositeCollectionBuilder
{
    public static BasePropertyIndexedKeyframeComposite Create(ReadOnlySpan<CompositeOperationOrAuto> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeEasingCollectionBuilder), nameof(BasePropertyIndexedKeyframeEasingCollectionBuilder.Create))]
public readonly union BasePropertyIndexedKeyframeEasing(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator BasePropertyIndexedKeyframeEasing(string value)
        => new(value);

    public static implicit operator BasePropertyIndexedKeyframeEasing(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeEasingCollectionBuilder
{
    public static BasePropertyIndexedKeyframeEasing Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeOffsetCollectionBuilder), nameof(BasePropertyIndexedKeyframeOffsetCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeOffset : System.Runtime.CompilerServices.IUnion, IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    public BasePropertyIndexedKeyframeOffset(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public BasePropertyIndexedKeyframeOffset(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator BasePropertyIndexedKeyframeOffset(double? value)
        => new(value);

    public static implicit operator BasePropertyIndexedKeyframeOffset(double?[] value)
        => new(value);

    IEnumerator<double?> IEnumerable<double?>.GetEnumerator()
        => ((IEnumerable<double?>)(AsDoubleArray ?? Array.Empty<double?>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double?>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeOffsetCollectionBuilder
{
    public static BasePropertyIndexedKeyframeOffset Create(ReadOnlySpan<double?> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CSSFontFeatureValuesMapSetValuesCollectionBuilder), nameof(CSSFontFeatureValuesMapSetValuesCollectionBuilder.Create))]
public readonly union CSSFontFeatureValuesMapSetValues(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator CSSFontFeatureValuesMapSetValues(uint value)
        => new(value);

    public static implicit operator CSSFontFeatureValuesMapSetValues(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CSSFontFeatureValuesMapSetValuesCollectionBuilder
{
    public static CSSFontFeatureValuesMapSetValues Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union CanvasRenderingContext2DRoundRectRadiiValue(double, DOMPointInit, CanvasRenderingContext2DRoundRectRadii[]) : IEnumerable<CanvasRenderingContext2DRoundRectRadii>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public CanvasRenderingContext2DRoundRectRadii[]? AsCanvasRenderingContext2DRoundRectRadiiArray => Value is CanvasRenderingContext2DRoundRectRadii[] value ? value : default(CanvasRenderingContext2DRoundRectRadii[]?);

    public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(double value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(CanvasRenderingContext2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<CanvasRenderingContext2DRoundRectRadii> IEnumerable<CanvasRenderingContext2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<CanvasRenderingContext2DRoundRectRadii>)(AsCanvasRenderingContext2DRoundRectRadiiArray ?? Array.Empty<CanvasRenderingContext2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CanvasRenderingContext2DRoundRectRadii>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder
{
    public static CanvasRenderingContext2DRoundRectRadiiValue Create(ReadOnlySpan<CanvasRenderingContext2DRoundRectRadii> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringCollectionBuilder), nameof(ConstrainDOMStringCollectionBuilder.Create))]
public readonly union ConstrainDOMString(string, string[], ConstrainDOMStringParameters) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public ConstrainDOMStringParameters? AsConstrainDOMStringParameters => Value is ConstrainDOMStringParameters value ? value : default(ConstrainDOMStringParameters?);

    public static implicit operator ConstrainDOMString(string value)
        => new(value);

    public static implicit operator ConstrainDOMString(string[] value)
        => new(value);

    public static implicit operator ConstrainDOMString(ConstrainDOMStringParameters value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringCollectionBuilder
{
    public static ConstrainDOMString Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersExactCollectionBuilder), nameof(ConstrainDOMStringParametersExactCollectionBuilder.Create))]
public readonly union ConstrainDOMStringParametersExact(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator ConstrainDOMStringParametersExact(string value)
        => new(value);

    public static implicit operator ConstrainDOMStringParametersExact(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringParametersExactCollectionBuilder
{
    public static ConstrainDOMStringParametersExact Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersIdealCollectionBuilder), nameof(ConstrainDOMStringParametersIdealCollectionBuilder.Create))]
public readonly union ConstrainDOMStringParametersIdeal(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator ConstrainDOMStringParametersIdeal(string value)
        => new(value);

    public static implicit operator ConstrainDOMStringParametersIdeal(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringParametersIdealCollectionBuilder
{
    public static ConstrainDOMStringParametersIdeal Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainPoint2DCollectionBuilder), nameof(ConstrainPoint2DCollectionBuilder.Create))]
public readonly union ConstrainPoint2D(Point2D[], ConstrainPoint2DParameters) : IEnumerable<Point2D>
{

    public Point2D[]? AsPoint2DArray => Value is Point2D[] value ? value : default(Point2D[]?);

    public ConstrainPoint2DParameters? AsConstrainPoint2DParameters => Value is ConstrainPoint2DParameters value ? value : default(ConstrainPoint2DParameters?);

    public static implicit operator ConstrainPoint2D(Point2D[] value)
        => new(value);

    public static implicit operator ConstrainPoint2D(ConstrainPoint2DParameters value)
        => new(value);

    IEnumerator<Point2D> IEnumerable<Point2D>.GetEnumerator()
        => ((IEnumerable<Point2D>)(AsPoint2DArray ?? Array.Empty<Point2D>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Point2D>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainPoint2DCollectionBuilder
{
    public static ConstrainPoint2D Create(ReadOnlySpan<Point2D> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder), nameof(CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder.Create))]
public readonly union CrossOriginStorageRequestFileHandleOptionsOrigins(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator CrossOriginStorageRequestFileHandleOptionsOrigins(string value)
        => new(value);

    public static implicit operator CrossOriginStorageRequestFileHandleOptionsOrigins(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder
{
    public static CrossOriginStorageRequestFileHandleOptionsOrigins Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixInitValueCollectionBuilder), nameof(DOMMatrixInitValueCollectionBuilder.Create))]
public readonly union DOMMatrixInitValue(string, double[]) : IEnumerable<double>
{

    public string? AsString => Value is string value ? value : default(string?);

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public static implicit operator DOMMatrixInitValue(string value)
        => new(value);

    public static implicit operator DOMMatrixInitValue(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DOMMatrixInitValueCollectionBuilder
{
    public static DOMMatrixInitValue Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixReadOnlyInitCollectionBuilder), nameof(DOMMatrixReadOnlyInitCollectionBuilder.Create))]
public readonly union DOMMatrixReadOnlyInit(string, double[]) : IEnumerable<double>
{

    public string? AsString => Value is string value ? value : default(string?);

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public static implicit operator DOMMatrixReadOnlyInit(string value)
        => new(value);

    public static implicit operator DOMMatrixReadOnlyInit(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DOMMatrixReadOnlyInitCollectionBuilder
{
    public static DOMMatrixReadOnlyInit Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueCollectionBuilder), nameof(DefaultValueCollectionBuilder.Create))]
public readonly struct DefaultValue : System.Runtime.CompilerServices.IUnion, IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    public DefaultValue(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public DefaultValue(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator DefaultValue(double? value)
        => new(value);

    public static implicit operator DefaultValue(double?[] value)
        => new(value);

    IEnumerator<double?> IEnumerable<double?>.GetEnumerator()
        => ((IEnumerable<double?>)(AsDoubleArray ?? Array.Empty<double?>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double?>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueCollectionBuilder
{
    public static DefaultValue Create(ReadOnlySpan<double?> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue2CollectionBuilder), nameof(DefaultValueValue2CollectionBuilder.Create))]
public readonly union DefaultValueValue2(CompositeOperationOrAuto, CompositeOperationOrAuto[]) : IEnumerable<CompositeOperationOrAuto>
{

    public CompositeOperationOrAuto? AsCompositeOperationOrAuto => Value is CompositeOperationOrAuto value ? value : default(CompositeOperationOrAuto?);

    public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => Value is CompositeOperationOrAuto[] value ? value : default(CompositeOperationOrAuto[]?);

    public static implicit operator DefaultValueValue2(CompositeOperationOrAuto value)
        => new(value);

    public static implicit operator DefaultValueValue2(CompositeOperationOrAuto[] value)
        => new(value);

    IEnumerator<CompositeOperationOrAuto> IEnumerable<CompositeOperationOrAuto>.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)(AsCompositeOperationOrAutoArray ?? Array.Empty<CompositeOperationOrAuto>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValue2CollectionBuilder
{
    public static DefaultValueValue2 Create(ReadOnlySpan<CompositeOperationOrAuto> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue3CollectionBuilder), nameof(DefaultValueValue3CollectionBuilder.Create))]
public readonly union DefaultValueValue3(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator DefaultValueValue3(string value)
        => new(value);

    public static implicit operator DefaultValueValue3(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValue3CollectionBuilder
{
    public static DefaultValueValue3 Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValueCollectionBuilder), nameof(DefaultValueValueCollectionBuilder.Create))]
public readonly union DefaultValueValue(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator DefaultValueValue(string value)
        => new(value);

    public static implicit operator DefaultValueValue(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValueCollectionBuilder
{
    public static DefaultValueValue Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(FilePickerAcceptTypeAcceptValueCollectionBuilder), nameof(FilePickerAcceptTypeAcceptValueCollectionBuilder.Create))]
public readonly union FilePickerAcceptTypeAcceptValue(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator FilePickerAcceptTypeAcceptValue(string value)
        => new(value);

    public static implicit operator FilePickerAcceptTypeAcceptValue(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class FilePickerAcceptTypeAcceptValueCollectionBuilder
{
    public static FilePickerAcceptTypeAcceptValue Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Float32ListCollectionBuilder), nameof(Float32ListCollectionBuilder.Create))]
public readonly union Float32List(Float32Array, GLfloat[]) : IEnumerable<GLfloat>
{

    public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    public GLfloat[]? AsGLfloatArray => Value is GLfloat[] value ? value : default(GLfloat[]?);

    public static implicit operator Float32List(Float32Array value)
        => new(value);

    public static implicit operator Float32List(GLfloat[] value)
        => new(value);

    IEnumerator<GLfloat> IEnumerable<GLfloat>.GetEnumerator()
        => ((IEnumerable<GLfloat>)(AsGLfloatArray ?? Array.Empty<GLfloat>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLfloat>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class Float32ListCollectionBuilder
{
    public static Float32List Create(ReadOnlySpan<GLfloat> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(HeadersInitCollectionBuilder), nameof(HeadersInitCollectionBuilder.Create))]
public readonly union HeadersInit(string[][], Dictionary<string, string>) : IEnumerable<string[]>
{

    public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    public static implicit operator HeadersInit(string[][] value)
        => new(value);

    public static implicit operator HeadersInit(Dictionary<string, string> value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HeadersInitCollectionBuilder
{
    public static HeadersInit Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBDatabaseTransactionStoreNamesCollectionBuilder), nameof(IDBDatabaseTransactionStoreNamesCollectionBuilder.Create))]
public readonly union IDBDatabaseTransactionStoreNames(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator IDBDatabaseTransactionStoreNames(string value)
        => new(value);

    public static implicit operator IDBDatabaseTransactionStoreNames(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBDatabaseTransactionStoreNamesCollectionBuilder
{
    public static IDBDatabaseTransactionStoreNames Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder), nameof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder.Create))]
public readonly union IDBObjectStoreCreateIndexKeyPath(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator IDBObjectStoreCreateIndexKeyPath(string value)
        => new(value);

    public static implicit operator IDBObjectStoreCreateIndexKeyPath(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBObjectStoreCreateIndexKeyPathCollectionBuilder
{
    public static IDBObjectStoreCreateIndexKeyPath Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreParametersKeyPathCollectionBuilder), nameof(IDBObjectStoreParametersKeyPathCollectionBuilder.Create))]
public readonly union IDBObjectStoreParametersKeyPath(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator IDBObjectStoreParametersKeyPath(string value)
        => new(value);

    public static implicit operator IDBObjectStoreParametersKeyPath(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBObjectStoreParametersKeyPathCollectionBuilder
{
    public static IDBObjectStoreParametersKeyPath Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(InstallEventAddRoutesRulesCollectionBuilder), nameof(InstallEventAddRoutesRulesCollectionBuilder.Create))]
public readonly union InstallEventAddRoutesRules(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    public static implicit operator InstallEventAddRoutesRules(RouterRule value)
        => new(value);

    public static implicit operator InstallEventAddRoutesRules(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class InstallEventAddRoutesRulesCollectionBuilder
{
    public static InstallEventAddRoutesRules Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Int32ListCollectionBuilder), nameof(Int32ListCollectionBuilder.Create))]
public readonly union Int32List(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator Int32List(Int32Array value)
        => new(value);

    public static implicit operator Int32List(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class Int32ListCollectionBuilder
{
    public static Int32List Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IntersectionObserverInitThresholdCollectionBuilder), nameof(IntersectionObserverInitThresholdCollectionBuilder.Create))]
public readonly union IntersectionObserverInitThreshold(double, double[]) : IEnumerable<double>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public static implicit operator IntersectionObserverInitThreshold(double value)
        => new(value);

    public static implicit operator IntersectionObserverInitThreshold(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class IntersectionObserverInitThresholdCollectionBuilder
{
    public static IntersectionObserverInitThreshold Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(LanguageModelMessageContentValueCollectionBuilder), nameof(LanguageModelMessageContentValueCollectionBuilder.Create))]
public readonly union LanguageModelMessageContentValue(string, LanguageModelMessageContent[]) : IEnumerable<LanguageModelMessageContent>
{

    public string? AsString => Value is string value ? value : default(string?);

    public LanguageModelMessageContent[]? AsLanguageModelMessageContentArray => Value is LanguageModelMessageContent[] value ? value : default(LanguageModelMessageContent[]?);

    public static implicit operator LanguageModelMessageContentValue(string value)
        => new(value);

    public static implicit operator LanguageModelMessageContentValue(LanguageModelMessageContent[] value)
        => new(value);

    IEnumerator<LanguageModelMessageContent> IEnumerable<LanguageModelMessageContent>.GetEnumerator()
        => ((IEnumerable<LanguageModelMessageContent>)(AsLanguageModelMessageContentArray ?? Array.Empty<LanguageModelMessageContent>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<LanguageModelMessageContent>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class LanguageModelMessageContentValueCollectionBuilder
{
    public static LanguageModelMessageContentValue Create(ReadOnlySpan<LanguageModelMessageContent> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(LanguageModelPromptCollectionBuilder), nameof(LanguageModelPromptCollectionBuilder.Create))]
public readonly union LanguageModelPrompt(LanguageModelMessage[], string) : IEnumerable<LanguageModelMessage>
{

    public LanguageModelMessage[]? AsLanguageModelMessageArray => Value is LanguageModelMessage[] value ? value : default(LanguageModelMessage[]?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator LanguageModelPrompt(LanguageModelMessage[] value)
        => new(value);

    public static implicit operator LanguageModelPrompt(string value)
        => new(value);

    IEnumerator<LanguageModelMessage> IEnumerable<LanguageModelMessage>.GetEnumerator()
        => ((IEnumerable<LanguageModelMessage>)(AsLanguageModelMessageArray ?? Array.Empty<LanguageModelMessage>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<LanguageModelMessage>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class LanguageModelPromptCollectionBuilder
{
    public static LanguageModelPrompt Create(ReadOnlySpan<LanguageModelMessage> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(MLGraphBuilderSplitSplitsCollectionBuilder), nameof(MLGraphBuilderSplitSplitsCollectionBuilder.Create))]
public readonly union MLGraphBuilderSplitSplits(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator MLGraphBuilderSplitSplits(uint value)
        => new(value);

    public static implicit operator MLGraphBuilderSplitSplits(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class MLGraphBuilderSplitSplitsCollectionBuilder
{
    public static MLGraphBuilderSplitSplits Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union OffscreenCanvasRenderingContext2DRoundRectRadiiValue(double, DOMPointInit, OffscreenCanvasRenderingContext2DRoundRectRadii[]) : IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public OffscreenCanvasRenderingContext2DRoundRectRadii[]? AsOffscreenCanvasRenderingContext2DRoundRectRadiiArray => Value is OffscreenCanvasRenderingContext2DRoundRectRadii[] value ? value : default(OffscreenCanvasRenderingContext2DRoundRectRadii[]?);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(double value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(OffscreenCanvasRenderingContext2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<OffscreenCanvasRenderingContext2DRoundRectRadii> IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>)(AsOffscreenCanvasRenderingContext2DRoundRectRadiiArray ?? Array.Empty<OffscreenCanvasRenderingContext2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder
{
    public static OffscreenCanvasRenderingContext2DRoundRectRadiiValue Create(ReadOnlySpan<OffscreenCanvasRenderingContext2DRoundRectRadii> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Path2DRoundRectRadiiValueCollectionBuilder), nameof(Path2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union Path2DRoundRectRadiiValue(double, DOMPointInit, Path2DRoundRectRadii[]) : IEnumerable<Path2DRoundRectRadii>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public Path2DRoundRectRadii[]? AsPath2DRoundRectRadiiArray => Value is Path2DRoundRectRadii[] value ? value : default(Path2DRoundRectRadii[]?);

    public static implicit operator Path2DRoundRectRadiiValue(double value)
        => new(value);

    public static implicit operator Path2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    public static implicit operator Path2DRoundRectRadiiValue(Path2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<Path2DRoundRectRadii> IEnumerable<Path2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<Path2DRoundRectRadii>)(AsPath2DRoundRectRadiiArray ?? Array.Empty<Path2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Path2DRoundRectRadii>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class Path2DRoundRectRadiiValueCollectionBuilder
{
    public static Path2DRoundRectRadiiValue Create(ReadOnlySpan<Path2DRoundRectRadii> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RTCIceServerUrlsCollectionBuilder), nameof(RTCIceServerUrlsCollectionBuilder.Create))]
public readonly union RTCIceServerUrls(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator RTCIceServerUrls(string value)
        => new(value);

    public static implicit operator RTCIceServerUrls(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class RTCIceServerUrlsCollectionBuilder
{
    public static RTCIceServerUrls Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RoundRectRadiiValueCollectionBuilder), nameof(RoundRectRadiiValueCollectionBuilder.Create))]
public readonly union RoundRectRadiiValue(double, DOMPointInit, RoundRectRadii[]) : IEnumerable<RoundRectRadii>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public RoundRectRadii[]? AsRoundRectRadiiArray => Value is RoundRectRadii[] value ? value : default(RoundRectRadii[]?);

    public static implicit operator RoundRectRadiiValue(double value)
        => new(value);

    public static implicit operator RoundRectRadiiValue(DOMPointInit value)
        => new(value);

    public static implicit operator RoundRectRadiiValue(RoundRectRadii[] value)
        => new(value);

    IEnumerator<RoundRectRadii> IEnumerable<RoundRectRadii>.GetEnumerator()
        => ((IEnumerable<RoundRectRadii>)(AsRoundRectRadiiArray ?? Array.Empty<RoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RoundRectRadii>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class RoundRectRadiiValueCollectionBuilder
{
    public static RoundRectRadiiValue Create(ReadOnlySpan<RoundRectRadii> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(SetValuesCollectionBuilder), nameof(SetValuesCollectionBuilder.Create))]
public readonly union SetValues(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator SetValues(uint value)
        => new(value);

    public static implicit operator SetValues(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class SetValuesCollectionBuilder
{
    public static SetValues Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue14CollectionBuilder), nameof(StructuralCacheValue14CollectionBuilder.Create))]
public readonly union StructuralCacheValue14(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator StructuralCacheValue14(uint value)
        => new(value);

    public static implicit operator StructuralCacheValue14(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue14CollectionBuilder
{
    public static StructuralCacheValue14 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue25CollectionBuilder), nameof(StructuralCacheValue25CollectionBuilder.Create))]
public readonly union StructuralCacheValue25(string, double[]) : IEnumerable<double>
{

    public string? AsString => Value is string value ? value : default(string?);

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public static implicit operator StructuralCacheValue25(string value)
        => new(value);

    public static implicit operator StructuralCacheValue25(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue25CollectionBuilder
{
    public static StructuralCacheValue25 Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue39CollectionBuilder), nameof(StructuralCacheValue39CollectionBuilder.Create))]
public readonly union StructuralCacheValue39(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator StructuralCacheValue39(string value)
        => new(value);

    public static implicit operator StructuralCacheValue39(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue39CollectionBuilder
{
    public static StructuralCacheValue39 Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue3CollectionBuilder), nameof(StructuralCacheValue3CollectionBuilder.Create))]
public readonly union StructuralCacheValue3(double, DOMPointInit, StructuralCacheValue2[]) : IEnumerable<StructuralCacheValue2>
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public StructuralCacheValue2[]? AsStructuralCacheValue2Array => Value is StructuralCacheValue2[] value ? value : default(StructuralCacheValue2[]?);

    public static implicit operator StructuralCacheValue3(double value)
        => new(value);

    public static implicit operator StructuralCacheValue3(DOMPointInit value)
        => new(value);

    public static implicit operator StructuralCacheValue3(StructuralCacheValue2[] value)
        => new(value);

    IEnumerator<StructuralCacheValue2> IEnumerable<StructuralCacheValue2>.GetEnumerator()
        => ((IEnumerable<StructuralCacheValue2>)(AsStructuralCacheValue2Array ?? Array.Empty<StructuralCacheValue2>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<StructuralCacheValue2>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue3CollectionBuilder
{
    public static StructuralCacheValue3 Create(ReadOnlySpan<StructuralCacheValue2> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue42CollectionBuilder), nameof(StructuralCacheValue42CollectionBuilder.Create))]
public readonly union StructuralCacheValue42(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    public static implicit operator StructuralCacheValue42(RouterRule value)
        => new(value);

    public static implicit operator StructuralCacheValue42(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue42CollectionBuilder
{
    public static StructuralCacheValue42 Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue47CollectionBuilder), nameof(StructuralCacheValue47CollectionBuilder.Create))]
public readonly union StructuralCacheValue47(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>
{

    public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue47(string[][] value)
        => new(value);

    public static implicit operator StructuralCacheValue47(Dictionary<string, string> value)
        => new(value);

    public static implicit operator StructuralCacheValue47(string value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue47CollectionBuilder
{
    public static StructuralCacheValue47 Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue54CollectionBuilder), nameof(StructuralCacheValue54CollectionBuilder.Create))]
public readonly union StructuralCacheValue54(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator StructuralCacheValue54(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue54(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue54CollectionBuilder
{
    public static StructuralCacheValue54 Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue55CollectionBuilder), nameof(StructuralCacheValue55CollectionBuilder.Create))]
public readonly union StructuralCacheValue55(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator StructuralCacheValue55(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue55(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue55CollectionBuilder
{
    public static StructuralCacheValue55 Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue56CollectionBuilder), nameof(StructuralCacheValue56CollectionBuilder.Create))]
public readonly union StructuralCacheValue56(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    public static implicit operator StructuralCacheValue56(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue56(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue56CollectionBuilder
{
    public static StructuralCacheValue56 Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue57CollectionBuilder), nameof(StructuralCacheValue57CollectionBuilder.Create))]
public readonly union StructuralCacheValue57(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator StructuralCacheValue57(uint value)
        => new(value);

    public static implicit operator StructuralCacheValue57(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue57CollectionBuilder
{
    public static StructuralCacheValue57 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue7CollectionBuilder), nameof(StructuralCacheValue7CollectionBuilder.Create))]
public readonly union StructuralCacheValue7(RequestInfo, RequestInfo[]) : IEnumerable<RequestInfo>
{

    public RequestInfo? AsRequestInfo => Value is RequestInfo value ? value : default(RequestInfo?);

    public RequestInfo[]? AsRequestInfoArray => Value is RequestInfo[] value ? value : default(RequestInfo[]?);

    public static implicit operator StructuralCacheValue7(RequestInfo value)
        => new(value);

    public static implicit operator StructuralCacheValue7(RequestInfo[] value)
        => new(value);

    IEnumerator<RequestInfo> IEnumerable<RequestInfo>.GetEnumerator()
        => ((IEnumerable<RequestInfo>)(AsRequestInfoArray ?? Array.Empty<RequestInfo>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RequestInfo>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue7CollectionBuilder
{
    public static StructuralCacheValue7 Create(ReadOnlySpan<RequestInfo> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(URLSearchParamsInitCollectionBuilder), nameof(URLSearchParamsInitCollectionBuilder.Create))]
public readonly union URLSearchParamsInit(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>
{

    public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator URLSearchParamsInit(string[][] value)
        => new(value);

    public static implicit operator URLSearchParamsInit(Dictionary<string, string> value)
        => new(value);

    public static implicit operator URLSearchParamsInit(string value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class URLSearchParamsInitCollectionBuilder
{
    public static URLSearchParamsInit Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Uint32ListCollectionBuilder), nameof(Uint32ListCollectionBuilder.Create))]
public readonly union Uint32List(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    public static implicit operator Uint32List(Uint32Array value)
        => new(value);

    public static implicit operator Uint32List(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class Uint32ListCollectionBuilder
{
    public static Uint32List Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(VibratePatternCollectionBuilder), nameof(VibratePatternCollectionBuilder.Create))]
public readonly union VibratePattern(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator VibratePattern(uint value)
        => new(value);

    public static implicit operator VibratePattern(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VibratePatternCollectionBuilder
{
    public static VibratePattern Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ViewTimelineOptionsInsetValueCollectionBuilder), nameof(ViewTimelineOptionsInsetValueCollectionBuilder.Create))]
public readonly union ViewTimelineOptionsInsetValue(string, ViewTimelineOptionsInset[]) : IEnumerable<ViewTimelineOptionsInset>
{

    public string? AsString => Value is string value ? value : default(string?);

    public ViewTimelineOptionsInset[]? AsViewTimelineOptionsInsetArray => Value is ViewTimelineOptionsInset[] value ? value : default(ViewTimelineOptionsInset[]?);

    public static implicit operator ViewTimelineOptionsInsetValue(string value)
        => new(value);

    public static implicit operator ViewTimelineOptionsInsetValue(ViewTimelineOptionsInset[] value)
        => new(value);

    IEnumerator<ViewTimelineOptionsInset> IEnumerable<ViewTimelineOptionsInset>.GetEnumerator()
        => ((IEnumerable<ViewTimelineOptionsInset>)(AsViewTimelineOptionsInsetArray ?? Array.Empty<ViewTimelineOptionsInset>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ViewTimelineOptionsInset>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewTimelineOptionsInsetValueCollectionBuilder
{
    public static ViewTimelineOptionsInsetValue Create(ReadOnlySpan<ViewTimelineOptionsInset> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder
{
    public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawArraysWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawElementsWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(Int32Array value)
        => new(value);

    public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder
{
    public static WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WebSocketProtocolsCollectionBuilder), nameof(WebSocketProtocolsCollectionBuilder.Create))]
public readonly union WebSocketProtocols(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator WebSocketProtocols(string value)
        => new(value);

    public static implicit operator WebSocketProtocols(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class WebSocketProtocolsCollectionBuilder
{
    public static WebSocketProtocols Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AddBefore(HTMLElement, int)
{

    public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    public int? AsInt => Value is int value ? value : default(int?);

    public static implicit operator AddBefore(HTMLElement value)
        => new(value);

    public static implicit operator AddBefore(int value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AddEventListenerOptionsValue(AddEventListenerOptions, bool)
{

    public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator AddEventListenerOptionsValue(AddEventListenerOptions value)
        => new(value);

    public static implicit operator AddEventListenerOptionsValue(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AfterNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AfterNodes(Node value)
        => new(value);

    public static implicit operator AfterNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimateOptions(double, KeyframeAnimationOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    public static implicit operator AnimateOptions(double value)
        => new(value);

    public static implicit operator AnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsExitRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AnimationTriggerOptionsExitRangeEnd(TimelineRangeOffset value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeEnd(CSSNumericValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeEnd(CSSKeywordValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeEnd(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsExitRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AnimationTriggerOptionsExitRangeStart(TimelineRangeOffset value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeStart(CSSNumericValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeStart(CSSKeywordValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsExitRangeStart(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AnimationTriggerOptionsRangeEnd(TimelineRangeOffset value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeEnd(CSSNumericValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeEnd(CSSKeywordValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeEnd(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AnimationTriggerOptionsRangeStart(TimelineRangeOffset value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeStart(CSSNumericValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeStart(CSSKeywordValue value)
        => new(value);

    public static implicit operator AnimationTriggerOptionsRangeStart(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AppendNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator AppendNodes(Node value)
        => new(value);

    public static implicit operator AppendNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ArrayBufferView(Int8Array, Int16Array, Int32Array, Uint8Array, Uint16Array, Uint32Array, Uint8ClampedArray, BigInt64Array, BigUint64Array, Float16Array, Float32Array, Float64Array, DataView)
{

    public Int8Array? AsInt8Array => Value is Int8Array value ? value : default(Int8Array?);

    public Int16Array? AsInt16Array => Value is Int16Array value ? value : default(Int16Array?);

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public Uint8Array? AsUint8Array => Value is Uint8Array value ? value : default(Uint8Array?);

    public Uint16Array? AsUint16Array => Value is Uint16Array value ? value : default(Uint16Array?);

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public Uint8ClampedArray? AsUint8ClampedArray => Value is Uint8ClampedArray value ? value : default(Uint8ClampedArray?);

    public BigInt64Array? AsBigInt64Array => Value is BigInt64Array value ? value : default(BigInt64Array?);

    public BigUint64Array? AsBigUint64Array => Value is BigUint64Array value ? value : default(BigUint64Array?);

    public Float16Array? AsFloat16Array => Value is Float16Array value ? value : default(Float16Array?);

    public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    public Float64Array? AsFloat64Array => Value is Float64Array value ? value : default(Float64Array?);

    public DataView? AsDataView => Value is DataView value ? value : default(DataView?);

    public static implicit operator ArrayBufferView(Int8Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Int16Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Int32Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Uint8Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Uint16Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Uint32Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Uint8ClampedArray value)
        => new(value);

    public static implicit operator ArrayBufferView(BigInt64Array value)
        => new(value);

    public static implicit operator ArrayBufferView(BigUint64Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Float16Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Float32Array value)
        => new(value);

    public static implicit operator ArrayBufferView(Float64Array value)
        => new(value);

    public static implicit operator ArrayBufferView(DataView value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AssignNodes(Element, Text)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Text? AsText => Value is Text value ? value : default(Text?);

    public static implicit operator AssignNodes(Element value)
        => new(value);

    public static implicit operator AssignNodes(Text value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsLatencyHint(AudioContextLatencyCategory, double)
{

    public AudioContextLatencyCategory? AsAudioContextLatencyCategory => Value is AudioContextLatencyCategory value ? value : default(AudioContextLatencyCategory?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public static implicit operator AudioContextOptionsLatencyHint(AudioContextLatencyCategory value)
        => new(value);

    public static implicit operator AudioContextOptionsLatencyHint(double value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory, uint)
{

    public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => Value is AudioContextRenderSizeCategory value ? value : default(AudioContextRenderSizeCategory?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    public static implicit operator AudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsSinkId(string, AudioSinkOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    public static implicit operator AudioContextOptionsSinkId(string value)
        => new(value);

    public static implicit operator AudioContextOptionsSinkId(AudioSinkOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextSetSinkId(string, AudioSinkOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    public static implicit operator AudioContextSetSinkId(string value)
        => new(value);

    public static implicit operator AudioContextSetSinkId(AudioSinkOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextSinkId(string, AudioSinkInfo)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkInfo? AsAudioSinkInfo => Value is AudioSinkInfo value ? value : default(AudioSinkInfo?);

    public static implicit operator AudioContextSinkId(string value)
        => new(value);

    public static implicit operator AudioContextSinkId(AudioSinkInfo value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BeforeNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator BeforeNodes(Node value)
        => new(value);

    public static implicit operator BeforeNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothAdvertisingEventInitUUIDs(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothAdvertisingEventInitUUIDs(string value)
        => new(value);

    public static implicit operator BluetoothAdvertisingEventInitUUIDs(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothCharacteristicUUID(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothCharacteristicUUID(string value)
        => new(value);

    public static implicit operator BluetoothCharacteristicUUID(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothDescriptorUUID(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothDescriptorUUID(string value)
        => new(value);

    public static implicit operator BluetoothDescriptorUUID(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothServiceUUID(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothServiceUUID(string value)
        => new(value);

    public static implicit operator BluetoothServiceUUID(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetCharacteristicName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothUUIDGetCharacteristicName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetCharacteristicName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetDescriptorName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothUUIDGetDescriptorName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetDescriptorName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetServiceName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator BluetoothUUIDGetServiceName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetServiceName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BodyInit(ReadableStream, XMLHttpRequestBodyInit)
{

    public ReadableStream? AsReadableStream => Value is ReadableStream value ? value : default(ReadableStream?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator BodyInit(ReadableStream value)
        => new(value);

    public static implicit operator BodyInit(XMLHttpRequestBodyInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSPseudoElementParent(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator CSSPseudoElementParent(Element value)
        => new(value);

    public static implicit operator CSSPseudoElementParent(CSSPseudoElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasImageSource(HTMLOrSVGImageElement, HTMLVideoElement, HTMLCanvasElement, ImageBitmap, OffscreenCanvas, VideoFrame)
{

    public HTMLOrSVGImageElement? AsHTMLOrSVGImageElement => Value is HTMLOrSVGImageElement value ? value : default(HTMLOrSVGImageElement?);

    public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    public static implicit operator CanvasImageSource(HTMLOrSVGImageElement value)
        => new(value);

    public static implicit operator CanvasImageSource(HTMLVideoElement value)
        => new(value);

    public static implicit operator CanvasImageSource(HTMLCanvasElement value)
        => new(value);

    public static implicit operator CanvasImageSource(ImageBitmap value)
        => new(value);

    public static implicit operator CanvasImageSource(OffscreenCanvas value)
        => new(value);

    public static implicit operator CanvasImageSource(VideoFrame value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DFillStyle(string, CanvasGradient, CanvasPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    public static implicit operator CanvasRenderingContext2DFillStyle(string value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DRoundRectRadii(double, DOMPointInit)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public static implicit operator CanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DStrokeStyle(string, CanvasGradient, CanvasPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    public static implicit operator CanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataAfterNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator CharacterDataAfterNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataAfterNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataBeforeNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator CharacterDataBeforeNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataBeforeNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataReplaceWithNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator CharacterDataReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataReplaceWithNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ClipboardItemDataValue(string, Blob)
{

    public string? AsString => Value is string value ? value : default(string?);

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public static implicit operator ClipboardItemDataValue(string value)
        => new(value);

    public static implicit operator ClipboardItemDataValue(Blob value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentData, CollectedClientAdditionalPaymentRegistrationData)
{

    public CollectedClientAdditionalPaymentData? AsCollectedClientAdditionalPaymentData => Value is CollectedClientAdditionalPaymentData value ? value : default(CollectedClientAdditionalPaymentData?);

    public CollectedClientAdditionalPaymentRegistrationData? AsCollectedClientAdditionalPaymentRegistrationData => Value is CollectedClientAdditionalPaymentRegistrationData value ? value : default(CollectedClientAdditionalPaymentRegistrationData?);

    public static implicit operator CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentData value)
        => new(value);

    public static implicit operator CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentRegistrationData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBoolean(bool, ConstrainBooleanParameters)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ConstrainBooleanParameters? AsConstrainBooleanParameters => Value is ConstrainBooleanParameters value ? value : default(ConstrainBooleanParameters?);

    public static implicit operator ConstrainBoolean(bool value)
        => new(value);

    public static implicit operator ConstrainBoolean(ConstrainBooleanParameters value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMString(bool, string, ConstrainBooleanOrDOMStringParameters)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value is string value ? value : default(string?);

    public ConstrainBooleanOrDOMStringParameters? AsConstrainBooleanOrDOMStringParameters => Value is ConstrainBooleanOrDOMStringParameters value ? value : default(ConstrainBooleanOrDOMStringParameters?);

    public static implicit operator ConstrainBooleanOrDOMString(bool value)
        => new(value);

    public static implicit operator ConstrainBooleanOrDOMString(string value)
        => new(value);

    public static implicit operator ConstrainBooleanOrDOMString(ConstrainBooleanOrDOMStringParameters value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMStringParametersExact(bool, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ConstrainBooleanOrDOMStringParametersExact(bool value)
        => new(value);

    public static implicit operator ConstrainBooleanOrDOMStringParametersExact(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMStringParametersIdeal(bool, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ConstrainBooleanOrDOMStringParametersIdeal(bool value)
        => new(value);

    public static implicit operator ConstrainBooleanOrDOMStringParametersIdeal(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainDouble(double, ConstrainDoubleRange)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public ConstrainDoubleRange? AsConstrainDoubleRange => Value is ConstrainDoubleRange value ? value : default(ConstrainDoubleRange?);

    public static implicit operator ConstrainDouble(double value)
        => new(value);

    public static implicit operator ConstrainDouble(ConstrainDoubleRange value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainULong(uint, ConstrainULongRange)
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public ConstrainULongRange? AsConstrainULongRange => Value is ConstrainULongRange value ? value : default(ConstrainULongRange?);

    public static implicit operator ConstrainULong(uint value)
        => new(value);

    public static implicit operator ConstrainULong(ConstrainULongRange value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateContextualFragmentString(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator CreateContextualFragmentString(TrustedHTML value)
        => new(value);

    public static implicit operator CreateContextualFragmentString(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateElementNSOptions(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator CreateElementNSOptions(string value)
        => new(value);

    public static implicit operator CreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateElementOptions(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator CreateElementOptions(string value)
        => new(value);

    public static implicit operator CreateElementOptions(ElementCreationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateObjectURLObj(Blob, MediaSource)
{

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public static implicit operator CreateObjectURLObj(Blob value)
        => new(value);

    public static implicit operator CreateObjectURLObj(MediaSource value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CryptoKeyID(SmallCryptoKeyID, System.Numerics.BigInteger)
{

    public SmallCryptoKeyID? AsSmallCryptoKeyID => Value is SmallCryptoKeyID value ? value : default(SmallCryptoKeyID?);

    public System.Numerics.BigInteger? AsBigInteger => Value is System.Numerics.BigInteger value ? value : default(System.Numerics.BigInteger?);

    public static implicit operator CryptoKeyID(SmallCryptoKeyID value)
        => new(value);

    public static implicit operator CryptoKeyID(System.Numerics.BigInteger value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CustomMediaQuery(MediaList, bool)
{

    public MediaList? AsMediaList => Value is MediaList value ? value : default(MediaList?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator CustomMediaQuery(MediaList value)
        => new(value);

    public static implicit operator CustomMediaQuery(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DOMParserParseFromString(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DOMParserParseFromString(TrustedHTML value)
        => new(value);

    public static implicit operator DOMParserParseFromString(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DigitalCredentialProtocol(DigitalCredentialPresentationProtocol, DigitalCredentialIssuanceProtocol)
{

    public DigitalCredentialPresentationProtocol? AsDigitalCredentialPresentationProtocol => Value is DigitalCredentialPresentationProtocol value ? value : default(DigitalCredentialPresentationProtocol?);

    public DigitalCredentialIssuanceProtocol? AsDigitalCredentialIssuanceProtocol => Value is DigitalCredentialIssuanceProtocol value ? value : default(DigitalCredentialIssuanceProtocol?);

    public static implicit operator DigitalCredentialProtocol(DigitalCredentialPresentationProtocol value)
        => new(value);

    public static implicit operator DigitalCredentialProtocol(DigitalCredentialIssuanceProtocol value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DisplayMediaStreamOptionsAudio(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator DisplayMediaStreamOptionsAudio(bool value)
        => new(value);

    public static implicit operator DisplayMediaStreamOptionsAudio(MediaTrackConstraints value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DisplayMediaStreamOptionsVideo(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator DisplayMediaStreamOptionsVideo(bool value)
        => new(value);

    public static implicit operator DisplayMediaStreamOptionsVideo(MediaTrackConstraints value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentAppendNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentAppendNodes(Node value)
        => new(value);

    public static implicit operator DocumentAppendNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentCreateElementNSOptions(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator DocumentCreateElementNSOptions(string value)
        => new(value);

    public static implicit operator DocumentCreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentCreateElementOptions(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator DocumentCreateElementOptions(string value)
        => new(value);

    public static implicit operator DocumentCreateElementOptions(ElementCreationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentAppendNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentFragmentAppendNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentAppendNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentPrependNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentFragmentPrependNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentPrependNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentReplaceChildrenNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentFragmentReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentReplaceChildrenNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentImportNodeOptions(bool, ImportNodeOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    public static implicit operator DocumentImportNodeOptions(bool value)
        => new(value);

    public static implicit operator DocumentImportNodeOptions(ImportNodeOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentParseHTMLUnsafeHtml(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentParseHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    public static implicit operator DocumentParseHTMLUnsafeHtml(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentPrependNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentPrependNodes(Node value)
        => new(value);

    public static implicit operator DocumentPrependNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentReplaceChildrenNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator DocumentReplaceChildrenNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator DocumentStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    public static implicit operator DocumentStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeAfterNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentTypeAfterNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeAfterNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeBeforeNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentTypeBeforeNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeBeforeNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeReplaceWithNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentTypeReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeReplaceWithNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentWriteText(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentWriteText(TrustedHTML value)
        => new(value);

    public static implicit operator DocumentWriteText(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentWritelnText(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator DocumentWritelnText(TrustedHTML value)
        => new(value);

    public static implicit operator DocumentWritelnText(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EffectTimingDuration(double, CSSNumericValue, string)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator EffectTimingDuration(double value)
        => new(value);

    public static implicit operator EffectTimingDuration(CSSNumericValue value)
        => new(value);

    public static implicit operator EffectTimingDuration(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAfterNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementAfterNodes(Node value)
        => new(value);

    public static implicit operator ElementAfterNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAnimateOptions(double, KeyframeAnimationOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    public static implicit operator ElementAnimateOptions(double value)
        => new(value);

    public static implicit operator ElementAnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAppendNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementAppendNodes(Node value)
        => new(value);

    public static implicit operator ElementAppendNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementBeforeNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementBeforeNodes(Node value)
        => new(value);

    public static implicit operator ElementBeforeNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInnerHTML(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementInnerHTML(TrustedHTML value)
        => new(value);

    public static implicit operator ElementInnerHTML(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInsertAdjacentHTMLString(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementInsertAdjacentHTMLString(TrustedHTML value)
        => new(value);

    public static implicit operator ElementInsertAdjacentHTMLString(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValue(JSFile, string, FormData)
{

    public JSFile? AsJSFile => Value is JSFile value ? value : default(JSFile?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator ElementInternalsSetFormValue(JSFile value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValue(string value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValue(FormData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValueState(JSFile, string, FormData)
{

    public JSFile? AsJSFile => Value is JSFile value ? value : default(JSFile?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator ElementInternalsSetFormValueState(JSFile value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValueState(string value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValueState(FormData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementOuterHTML(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementOuterHTML(TrustedHTML value)
        => new(value);

    public static implicit operator ElementOuterHTML(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementPrependNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementPrependNodes(Node value)
        => new(value);

    public static implicit operator ElementPrependNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementReplaceChildrenNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator ElementReplaceChildrenNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementReplaceWithNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator ElementReplaceWithNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementScrollIntoViewArg(bool, ScrollIntoViewOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    public static implicit operator ElementScrollIntoViewArg(bool value)
        => new(value);

    public static implicit operator ElementScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetAttributeNSValue(TrustedType, string)
{

    public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementSetAttributeNSValue(TrustedType value)
        => new(value);

    public static implicit operator ElementSetAttributeNSValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetAttributeValue(TrustedType, string)
{

    public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementSetAttributeValue(TrustedType value)
        => new(value);

    public static implicit operator ElementSetAttributeValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetHTMLUnsafeHtml(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ElementSetHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    public static implicit operator ElementSetHTMLUnsafeHtml(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator ElementStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    public static implicit operator ElementStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventListenerValue(EventListenerLiteral, HandleEventCallback)
{

    public EventListenerLiteral? AsEventListenerLiteral => Value is EventListenerLiteral value ? value : default(EventListenerLiteral?);

    public HandleEventCallback? AsHandleEventCallback => Value is HandleEventCallback value ? value : default(HandleEventCallback?);

    public static implicit operator EventListenerValue(EventListenerLiteral value)
        => new(value);

    public static implicit operator EventListenerValue(HandleEventCallback value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventTargetAddEventListenerOptions(AddEventListenerOptions, bool)
{

    public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator EventTargetAddEventListenerOptions(AddEventListenerOptions value)
        => new(value);

    public static implicit operator EventTargetAddEventListenerOptions(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventTargetRemoveEventListenerOptions(EventListenerOptions, bool)
{

    public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator EventTargetRemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    public static implicit operator EventTargetRemoveEventListenerOptions(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ExtendableMessageEventInitSource(Client, ServiceWorker, MessagePort)
{

    public Client? AsClient => Value is Client value ? value : default(Client?);

    public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    public static implicit operator ExtendableMessageEventInitSource(Client value)
        => new(value);

    public static implicit operator ExtendableMessageEventInitSource(ServiceWorker value)
        => new(value);

    public static implicit operator ExtendableMessageEventInitSource(MessagePort value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ExtendableMessageEventSource(Client, ServiceWorker, MessagePort)
{

    public Client? AsClient => Value is Client value ? value : default(Client?);

    public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    public static implicit operator ExtendableMessageEventSource(Client value)
        => new(value);

    public static implicit operator ExtendableMessageEventSource(ServiceWorker value)
        => new(value);

    public static implicit operator ExtendableMessageEventSource(MessagePort value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FileReaderResult(string, ArrayBuffer)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    public static implicit operator FileReaderResult(string value)
        => new(value);

    public static implicit operator FileReaderResult(ArrayBuffer value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FormDataEntryValue(JSFile, string)
{

    public JSFile? AsJSFile => Value is JSFile value ? value : default(JSFile?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator FormDataEntryValue(JSFile value)
        => new(value);

    public static implicit operator FormDataEntryValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GeometryNode(Text, Element, CSSPseudoElement, JazorDocument)
{

    public Text? AsText => Value is Text value ? value : default(Text?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public static implicit operator GeometryNode(Text value)
        => new(value);

    public static implicit operator GeometryNode(Element value)
        => new(value);

    public static implicit operator GeometryNode(CSSPseudoElement value)
        => new(value);

    public static implicit operator GeometryNode(JazorDocument value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetCharacteristicName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator GetCharacteristicName(string value)
        => new(value);

    public static implicit operator GetCharacteristicName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetDescriptorName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator GetDescriptorName(string value)
        => new(value);

    public static implicit operator GetDescriptorName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetServiceName(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator GetServiceName(string value)
        => new(value);

    public static implicit operator GetServiceName(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GroupEffectTiming(double, EffectTiming)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    public static implicit operator GroupEffectTiming(double value)
        => new(value);

    public static implicit operator GroupEffectTiming(EffectTiming value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLAllCollectionItemResult(HTMLCollection, Element)
{

    public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator HTMLAllCollectionItemResult(HTMLCollection value)
        => new(value);

    public static implicit operator HTMLAllCollectionItemResult(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLAllCollectionNamedItemResult(HTMLCollection, Element)
{

    public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator HTMLAllCollectionNamedItemResult(HTMLCollection value)
        => new(value);

    public static implicit operator HTMLAllCollectionNamedItemResult(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLElementHidden(bool, double, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator HTMLElementHidden(bool value)
        => new(value);

    public static implicit operator HTMLElementHidden(double value)
        => new(value);

    public static implicit operator HTMLElementHidden(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLElementTogglePopoverOptions(TogglePopoverOptions, bool)
{

    public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator HTMLElementTogglePopoverOptions(TogglePopoverOptions value)
        => new(value);

    public static implicit operator HTMLElementTogglePopoverOptions(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLFormControlsCollectionNamedItemResult(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator HTMLFormControlsCollectionNamedItemResult(RadioNodeList value)
        => new(value);

    public static implicit operator HTMLFormControlsCollectionNamedItemResult(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLFormElementResult(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator HTMLFormElementResult(RadioNodeList value)
        => new(value);

    public static implicit operator HTMLFormElementResult(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLIFrameElementSrcdoc(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator HTMLIFrameElementSrcdoc(TrustedHTML value)
        => new(value);

    public static implicit operator HTMLIFrameElementSrcdoc(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOptionsCollectionAddBefore(HTMLElement, int)
{

    public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    public int? AsInt => Value is int value ? value : default(int?);

    public static implicit operator HTMLOptionsCollectionAddBefore(HTMLElement value)
        => new(value);

    public static implicit operator HTMLOptionsCollectionAddBefore(int value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOptionsCollectionAddElement(HTMLOptionElement, HTMLOptGroupElement)
{

    public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptionElement value)
        => new(value);

    public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptGroupElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOrSVGImageElement(HTMLImageElement, SVGImageElement)
{

    public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    public SVGImageElement? AsSVGImageElement => Value is SVGImageElement value ? value : default(SVGImageElement?);

    public static implicit operator HTMLOrSVGImageElement(HTMLImageElement value)
        => new(value);

    public static implicit operator HTMLOrSVGImageElement(SVGImageElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOrSVGScriptElement(HTMLScriptElement, SVGScriptElement)
{

    public HTMLScriptElement? AsHTMLScriptElement => Value is HTMLScriptElement value ? value : default(HTMLScriptElement?);

    public SVGScriptElement? AsSVGScriptElement => Value is SVGScriptElement value ? value : default(SVGScriptElement?);

    public static implicit operator HTMLOrSVGScriptElement(HTMLScriptElement value)
        => new(value);

    public static implicit operator HTMLOrSVGScriptElement(SVGScriptElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSelectElementAddBefore(HTMLElement, int)
{

    public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    public int? AsInt => Value is int value ? value : default(int?);

    public static implicit operator HTMLSelectElementAddBefore(HTMLElement value)
        => new(value);

    public static implicit operator HTMLSelectElementAddBefore(int value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSelectElementAddElement(HTMLOptionElement, HTMLOptGroupElement)
{

    public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    public static implicit operator HTMLSelectElementAddElement(HTMLOptionElement value)
        => new(value);

    public static implicit operator HTMLSelectElementAddElement(HTMLOptGroupElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSlotElementAssignNodes(Element, Text)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Text? AsText => Value is Text value ? value : default(Text?);

    public static implicit operator HTMLSlotElementAssignNodes(Element value)
        => new(value);

    public static implicit operator HTMLSlotElementAssignNodes(Text value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageBitmapRenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public static implicit operator ImageBitmapRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator ImageBitmapRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageBitmapSource(CanvasImageSource, Blob, ImageData)
{

    public CanvasImageSource? AsCanvasImageSource => Value is CanvasImageSource value ? value : default(CanvasImageSource?);

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    public static implicit operator ImageBitmapSource(CanvasImageSource value)
        => new(value);

    public static implicit operator ImageBitmapSource(Blob value)
        => new(value);

    public static implicit operator ImageBitmapSource(ImageData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageDataArray(Uint8ClampedArray, Float16Array)
{

    public Uint8ClampedArray? AsUint8ClampedArray => Value is Uint8ClampedArray value ? value : default(Uint8ClampedArray?);

    public Float16Array? AsFloat16Array => Value is Float16Array value ? value : default(Float16Array?);

    public static implicit operator ImageDataArray(Uint8ClampedArray value)
        => new(value);

    public static implicit operator ImageDataArray(Float16Array value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImportNodeOptionsValue(bool, ImportNodeOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    public static implicit operator ImportNodeOptionsValue(bool value)
        => new(value);

    public static implicit operator ImportNodeOptionsValue(ImportNodeOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImportScriptsUrls(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ImportScriptsUrls(TrustedScriptURL value)
        => new(value);

    public static implicit operator ImportScriptsUrls(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union InsertAdjacentHTMLString(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator InsertAdjacentHTMLString(TrustedHTML value)
        => new(value);

    public static implicit operator InsertAdjacentHTMLString(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverInitRoot(Element, JazorDocument)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public static implicit operator IntersectionObserverInitRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverInitRoot(JazorDocument value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverRoot(Element, JazorDocument)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public static implicit operator IntersectionObserverRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverRoot(JazorDocument value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSNumericValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSKeywordValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeAnimationOptionsRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator KeyframeAnimationOptionsRangeStart(TimelineRangeOffset value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(CSSNumericValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(CSSKeywordValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeEffectOptionsValue(double, KeyframeEffectOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeEffectOptions? AsKeyframeEffectOptions => Value is KeyframeEffectOptions value ? value : default(KeyframeEffectOptions?);

    public static implicit operator KeyframeEffectOptionsValue(double value)
        => new(value);

    public static implicit operator KeyframeEffectOptionsValue(KeyframeEffectOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union LineAndPositionSetting(double, AutoKeyword)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public AutoKeyword? AsAutoKeyword => Value is AutoKeyword value ? value : default(AutoKeyword?);

    public static implicit operator LineAndPositionSetting(double value)
        => new(value);

    public static implicit operator LineAndPositionSetting(AutoKeyword value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MLNumber(System.Numerics.BigInteger, double)
{

    public System.Numerics.BigInteger? AsBigInteger => Value is System.Numerics.BigInteger value ? value : default(System.Numerics.BigInteger?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public static implicit operator MLNumber(System.Numerics.BigInteger value)
        => new(value);

    public static implicit operator MLNumber(double value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaProvider(MediaStream, MediaSource, Blob)
{

    public MediaStream? AsMediaStream => Value is MediaStream value ? value : default(MediaStream?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public static implicit operator MediaProvider(MediaStream value)
        => new(value);

    public static implicit operator MediaProvider(MediaSource value)
        => new(value);

    public static implicit operator MediaProvider(Blob value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamConstraintsAudio(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator MediaStreamConstraintsAudio(bool value)
        => new(value);

    public static implicit operator MediaStreamConstraintsAudio(MediaTrackConstraints value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamConstraintsVideo(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator MediaStreamConstraintsVideo(bool value)
        => new(value);

    public static implicit operator MediaStreamConstraintsVideo(MediaTrackConstraints value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamTrackOrHandle(MediaStreamTrack, MediaStreamTrackHandle)
{

    public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    public MediaStreamTrackHandle? AsMediaStreamTrackHandle => Value is MediaStreamTrackHandle value ? value : default(MediaStreamTrackHandle?);

    public static implicit operator MediaStreamTrackOrHandle(MediaStreamTrack value)
        => new(value);

    public static implicit operator MediaStreamTrackOrHandle(MediaStreamTrackHandle value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackCapabilitiesEchoCancellation(bool, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator MediaTrackCapabilitiesEchoCancellation(bool value)
        => new(value);

    public static implicit operator MediaTrackCapabilitiesEchoCancellation(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetPan(bool, ConstrainDouble)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    public static implicit operator MediaTrackConstraintSetPan(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetPan(ConstrainDouble value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetTilt(bool, ConstrainDouble)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    public static implicit operator MediaTrackConstraintSetTilt(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetTilt(ConstrainDouble value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetZoom(bool, ConstrainDouble)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    public static implicit operator MediaTrackConstraintSetZoom(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetZoom(ConstrainDouble value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackSettingsEchoCancellation(bool, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator MediaTrackSettingsEchoCancellation(bool value)
        => new(value);

    public static implicit operator MediaTrackSettingsEchoCancellation(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MessageEventSource(WindowProxy, MessagePort, ServiceWorker)
{

    public WindowProxy? AsWindowProxy => Value is WindowProxy value ? value : default(WindowProxy?);

    public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    public static implicit operator MessageEventSource(WindowProxy value)
        => new(value);

    public static implicit operator MessageEventSource(MessagePort value)
        => new(value);

    public static implicit operator MessageEventSource(ServiceWorker value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union NodeFilterValue(NodeFilterLiteral, AcceptNodeCallback)
{

    public NodeFilterLiteral? AsNodeFilterLiteral => Value is NodeFilterLiteral value ? value : default(NodeFilterLiteral?);

    public AcceptNodeCallback? AsAcceptNodeCallback => Value is AcceptNodeCallback value ? value : default(AcceptNodeCallback?);

    public static implicit operator NodeFilterValue(NodeFilterLiteral value)
        => new(value);

    public static implicit operator NodeFilterValue(AcceptNodeCallback value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ObservableInspectorUnion(ObservableSubscriptionCallback, ObservableInspector)
{

    public ObservableSubscriptionCallback? AsObservableSubscriptionCallback => Value is ObservableSubscriptionCallback value ? value : default(ObservableSubscriptionCallback?);

    public ObservableInspector? AsObservableInspector => Value is ObservableInspector value ? value : default(ObservableInspector?);

    public static implicit operator ObservableInspectorUnion(ObservableSubscriptionCallback value)
        => new(value);

    public static implicit operator ObservableInspectorUnion(ObservableInspector value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ObserverUnion(ObservableSubscriptionCallback, SubscriptionObserver)
{

    public ObservableSubscriptionCallback? AsObservableSubscriptionCallback => Value is ObservableSubscriptionCallback value ? value : default(ObservableSubscriptionCallback?);

    public SubscriptionObserver? AsSubscriptionObserver => Value is SubscriptionObserver value ? value : default(SubscriptionObserver?);

    public static implicit operator ObserverUnion(ObservableSubscriptionCallback value)
        => new(value);

    public static implicit operator ObserverUnion(SubscriptionObserver value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory, uint)
{

    public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => Value is AudioContextRenderSizeCategory value ? value : default(AudioContextRenderSizeCategory?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    public static implicit operator OfflineAudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DFillStyle(string, CanvasGradient, CanvasPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(string value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DRoundRectRadii(double, DOMPointInit)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DStrokeStyle(string, CanvasGradient, CanvasPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenRenderingContext(OffscreenCanvasRenderingContext2D, ImageBitmapRenderingContext, WebGLRenderingContext, WebGL2RenderingContext, GPUCanvasContext)
{

    public OffscreenCanvasRenderingContext2D? AsOffscreenCanvasRenderingContext2D => Value is OffscreenCanvasRenderingContext2D value ? value : default(OffscreenCanvasRenderingContext2D?);

    public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => Value is ImageBitmapRenderingContext value ? value : default(ImageBitmapRenderingContext?);

    public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    public GPUCanvasContext? AsGPUCanvasContext => Value is GPUCanvasContext value ? value : default(GPUCanvasContext?);

    public static implicit operator OffscreenRenderingContext(OffscreenCanvasRenderingContext2D value)
        => new(value);

    public static implicit operator OffscreenRenderingContext(ImageBitmapRenderingContext value)
        => new(value);

    public static implicit operator OffscreenRenderingContext(WebGLRenderingContext value)
        => new(value);

    public static implicit operator OffscreenRenderingContext(WebGL2RenderingContext value)
        => new(value);

    public static implicit operator OffscreenRenderingContext(GPUCanvasContext value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OptionalEffectTimingDuration(double, string)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator OptionalEffectTimingDuration(double value)
        => new(value);

    public static implicit operator OptionalEffectTimingDuration(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParameterCurrentTarget(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator ParameterCurrentTarget(Element value)
        => new(value);

    public static implicit operator ParameterCurrentTarget(CSSPseudoElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParameterEvent(JazorEvent, string)
{

    public JazorEvent? AsEvent => Value is JazorEvent value ? value : default(JazorEvent?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ParameterEvent(JazorEvent value)
        => new(value);

    public static implicit operator ParameterEvent(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParseHTMLUnsafeOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    public static implicit operator ParseHTMLUnsafeOptionsSanitizer(Sanitizer value)
        => new(value);

    public static implicit operator ParseHTMLUnsafeOptionsSanitizer(SanitizerConfig value)
        => new(value);

    public static implicit operator ParseHTMLUnsafeOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PasswordCredentialInit(PasswordCredentialData, HTMLFormElement)
{

    public PasswordCredentialData? AsPasswordCredentialData => Value is PasswordCredentialData value ? value : default(PasswordCredentialData?);

    public HTMLFormElement? AsHTMLFormElement => Value is HTMLFormElement value ? value : default(HTMLFormElement?);

    public static implicit operator PasswordCredentialInit(PasswordCredentialData value)
        => new(value);

    public static implicit operator PasswordCredentialInit(HTMLFormElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union Path2DPath(Path2D, string)
{

    public Path2D? AsPath2D => Value is Path2D value ? value : default(Path2D?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator Path2DPath(Path2D value)
        => new(value);

    public static implicit operator Path2DPath(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union Path2DRoundRectRadii(double, DOMPointInit)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public static implicit operator Path2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator Path2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureOptionsEnd(string, double)
{

    public string? AsString => Value is string value ? value : default(string?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public static implicit operator PerformanceMeasureOptionsEnd(string value)
        => new(value);

    public static implicit operator PerformanceMeasureOptionsEnd(double value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureOptionsStart(string, double)
{

    public string? AsString => Value is string value ? value : default(string?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public static implicit operator PerformanceMeasureOptionsStart(string value)
        => new(value);

    public static implicit operator PerformanceMeasureOptionsStart(double value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureStartOrMeasureOptions(string, PerformanceMeasureOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public PerformanceMeasureOptions? AsPerformanceMeasureOptions => Value is PerformanceMeasureOptions value ? value : default(PerformanceMeasureOptions?);

    public static implicit operator PerformanceMeasureStartOrMeasureOptions(string value)
        => new(value);

    public static implicit operator PerformanceMeasureStartOrMeasureOptions(PerformanceMeasureOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PrependNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator PrependNodes(Node value)
        => new(value);

    public static implicit operator PrependNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack, string)
{

    public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack value)
        => new(value);

    public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCRtpReceiverTransform(RTCRtpSFrameDecryptor, RTCRtpScriptTransform)
{

    public RTCRtpSFrameDecryptor? AsRTCRtpSFrameDecryptor => Value is RTCRtpSFrameDecryptor value ? value : default(RTCRtpSFrameDecryptor?);

    public RTCRtpScriptTransform? AsRTCRtpScriptTransform => Value is RTCRtpScriptTransform value ? value : default(RTCRtpScriptTransform?);

    public static implicit operator RTCRtpReceiverTransform(RTCRtpSFrameDecryptor value)
        => new(value);

    public static implicit operator RTCRtpReceiverTransform(RTCRtpScriptTransform value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCRtpSenderTransform(RTCRtpSFrameEncryptor, RTCRtpScriptTransform)
{

    public RTCRtpSFrameEncryptor? AsRTCRtpSFrameEncryptor => Value is RTCRtpSFrameEncryptor value ? value : default(RTCRtpSFrameEncryptor?);

    public RTCRtpScriptTransform? AsRTCRtpScriptTransform => Value is RTCRtpScriptTransform value ? value : default(RTCRtpScriptTransform?);

    public static implicit operator RTCRtpSenderTransform(RTCRtpSFrameEncryptor value)
        => new(value);

    public static implicit operator RTCRtpSenderTransform(RTCRtpScriptTransform value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RangeCreateContextualFragmentString(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator RangeCreateContextualFragmentString(TrustedHTML value)
        => new(value);

    public static implicit operator RangeCreateContextualFragmentString(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReadableStreamController(ReadableStreamDefaultController, ReadableByteStreamController)
{

    public ReadableStreamDefaultController? AsReadableStreamDefaultController => Value is ReadableStreamDefaultController value ? value : default(ReadableStreamDefaultController?);

    public ReadableByteStreamController? AsReadableByteStreamController => Value is ReadableByteStreamController value ? value : default(ReadableByteStreamController?);

    public static implicit operator ReadableStreamController(ReadableStreamDefaultController value)
        => new(value);

    public static implicit operator ReadableStreamController(ReadableByteStreamController value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReadableStreamReader(ReadableStreamDefaultReader, ReadableStreamBYOBReader)
{

    public ReadableStreamDefaultReader? AsReadableStreamDefaultReader => Value is ReadableStreamDefaultReader value ? value : default(ReadableStreamDefaultReader?);

    public ReadableStreamBYOBReader? AsReadableStreamBYOBReader => Value is ReadableStreamBYOBReader value ? value : default(ReadableStreamBYOBReader?);

    public static implicit operator ReadableStreamReader(ReadableStreamDefaultReader value)
        => new(value);

    public static implicit operator ReadableStreamReader(ReadableStreamBYOBReader value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RemoveEventListenerOptions(EventListenerOptions, bool)
{

    public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator RemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    public static implicit operator RemoveEventListenerOptions(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RenderingContext(CanvasRenderingContext2D, ImageBitmapRenderingContext, WebGLRenderingContext, WebGL2RenderingContext, GPUCanvasContext)
{

    public CanvasRenderingContext2D? AsCanvasRenderingContext2D => Value is CanvasRenderingContext2D value ? value : default(CanvasRenderingContext2D?);

    public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => Value is ImageBitmapRenderingContext value ? value : default(ImageBitmapRenderingContext?);

    public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    public GPUCanvasContext? AsGPUCanvasContext => Value is GPUCanvasContext value ? value : default(GPUCanvasContext?);

    public static implicit operator RenderingContext(CanvasRenderingContext2D value)
        => new(value);

    public static implicit operator RenderingContext(ImageBitmapRenderingContext value)
        => new(value);

    public static implicit operator RenderingContext(WebGLRenderingContext value)
        => new(value);

    public static implicit operator RenderingContext(WebGL2RenderingContext value)
        => new(value);

    public static implicit operator RenderingContext(GPUCanvasContext value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReplaceChildrenNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator ReplaceChildrenNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReplaceWithNodes(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator ReplaceWithNodes(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReportEventType(FenceEvent, string)
{

    public FenceEvent? AsFenceEvent => Value is FenceEvent value ? value : default(FenceEvent?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ReportEventType(FenceEvent value)
        => new(value);

    public static implicit operator ReportEventType(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RequestInfo(Request, string)
{

    public Request? AsRequest => Value is Request value ? value : default(Request?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator RequestInfo(Request value)
        => new(value);

    public static implicit operator RequestInfo(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RotationMatrixType(Float32Array, Float64Array, DOMMatrix)
{

    public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    public Float64Array? AsFloat64Array => Value is Float64Array value ? value : default(Float64Array?);

    public DOMMatrix? AsDOMMatrix => Value is DOMMatrix value ? value : default(DOMMatrix?);

    public static implicit operator RotationMatrixType(Float32Array value)
        => new(value);

    public static implicit operator RotationMatrixType(Float64Array value)
        => new(value);

    public static implicit operator RotationMatrixType(DOMMatrix value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RoundRectRadii(double, DOMPointInit)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public static implicit operator RoundRectRadii(double value)
        => new(value);

    public static implicit operator RoundRectRadii(DOMPointInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RouterSource(RouterSourceDict, RouterSourceEnum)
{

    public RouterSourceDict? AsRouterSourceDict => Value is RouterSourceDict value ? value : default(RouterSourceDict?);

    public RouterSourceEnum? AsRouterSourceEnum => Value is RouterSourceEnum value ? value : default(RouterSourceEnum?);

    public static implicit operator RouterSource(RouterSourceDict value)
        => new(value);

    public static implicit operator RouterSource(RouterSourceEnum value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SFrameTransformErrorEventFrame(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    public static implicit operator SFrameTransformErrorEventFrame(RTCEncodedVideoFrame value)
        => new(value);

    public static implicit operator SFrameTransformErrorEventFrame(RTCEncodedAudioFrame value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SFrameTransformErrorEventInitFrame(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    public static implicit operator SFrameTransformErrorEventInitFrame(RTCEncodedVideoFrame value)
        => new(value);

    public static implicit operator SFrameTransformErrorEventInitFrame(RTCEncodedAudioFrame value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SVGAnimatedStringBaseVal(string, TrustedScriptURL)
{

    public string? AsString => Value is string value ? value : default(string?);

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public static implicit operator SVGAnimatedStringBaseVal(string value)
        => new(value);

    public static implicit operator SVGAnimatedStringBaseVal(TrustedScriptURL value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerAttribute(string, SanitizerAttributeNamespace)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SanitizerAttributeNamespace? AsSanitizerAttributeNamespace => Value is SanitizerAttributeNamespace value ? value : default(SanitizerAttributeNamespace?);

    public static implicit operator SanitizerAttribute(string value)
        => new(value);

    public static implicit operator SanitizerAttribute(SanitizerAttributeNamespace value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerConfiguration(SanitizerConfig, SanitizerPresets)
{

    public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    public static implicit operator SanitizerConfiguration(SanitizerConfig value)
        => new(value);

    public static implicit operator SanitizerConfiguration(SanitizerPresets value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerElement(string, SanitizerElementNamespace)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SanitizerElementNamespace? AsSanitizerElementNamespace => Value is SanitizerElementNamespace value ? value : default(SanitizerElementNamespace?);

    public static implicit operator SanitizerElement(string value)
        => new(value);

    public static implicit operator SanitizerElement(SanitizerElementNamespace value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerElementWithAttributes(string, SanitizerElementNamespaceWithAttributes)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SanitizerElementNamespaceWithAttributes? AsSanitizerElementNamespaceWithAttributes => Value is SanitizerElementNamespaceWithAttributes value ? value : default(SanitizerElementNamespaceWithAttributes?);

    public static implicit operator SanitizerElementWithAttributes(string value)
        => new(value);

    public static implicit operator SanitizerElementWithAttributes(SanitizerElementNamespaceWithAttributes value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerPI(string, SanitizerProcessingInstruction)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SanitizerProcessingInstruction? AsSanitizerProcessingInstruction => Value is SanitizerProcessingInstruction value ? value : default(SanitizerProcessingInstruction?);

    public static implicit operator SanitizerPI(string value)
        => new(value);

    public static implicit operator SanitizerPI(SanitizerProcessingInstruction value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ScrollIntoViewArg(bool, ScrollIntoViewOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    public static implicit operator ScrollIntoViewArg(bool value)
        => new(value);

    public static implicit operator ScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SendBody(JazorDocument, XMLHttpRequestBodyInit)
{

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator SendBody(JazorDocument value)
        => new(value);

    public static implicit operator SendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SequenceEffectTiming(double, EffectTiming)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    public static implicit operator SequenceEffectTiming(double value)
        => new(value);

    public static implicit operator SequenceEffectTiming(EffectTiming value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ServiceWorkerContainerRegisterScriptURL(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ServiceWorkerContainerRegisterScriptURL(TrustedScriptURL value)
        => new(value);

    public static implicit operator ServiceWorkerContainerRegisterScriptURL(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetAttributeNSValue(TrustedType, string)
{

    public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator SetAttributeNSValue(TrustedType value)
        => new(value);

    public static implicit operator SetAttributeNSValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetAttributeValue(TrustedType, string)
{

    public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator SetAttributeValue(TrustedType value)
        => new(value);

    public static implicit operator SetAttributeValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetFormValueState(JSFile, string, FormData)
{

    public JSFile? AsJSFile => Value is JSFile value ? value : default(JSFile?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator SetFormValueState(JSFile value)
        => new(value);

    public static implicit operator SetFormValueState(string value)
        => new(value);

    public static implicit operator SetFormValueState(FormData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetHTMLOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    public static implicit operator SetHTMLOptionsSanitizer(Sanitizer value)
        => new(value);

    public static implicit operator SetHTMLOptionsSanitizer(SanitizerConfig value)
        => new(value);

    public static implicit operator SetHTMLOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetHTMLUnsafeOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    public static implicit operator SetHTMLUnsafeOptionsSanitizer(Sanitizer value)
        => new(value);

    public static implicit operator SetHTMLUnsafeOptionsSanitizer(SanitizerConfig value)
        => new(value);

    public static implicit operator SetHTMLUnsafeOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetSinkId(string, AudioSinkOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    public static implicit operator SetSinkId(string value)
        => new(value);

    public static implicit operator SetSinkId(AudioSinkOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowAnimationNewTarget(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator ShadowAnimationNewTarget(Element value)
        => new(value);

    public static implicit operator ShadowAnimationNewTarget(CSSPseudoElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowRootInnerHTML(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ShadowRootInnerHTML(TrustedHTML value)
        => new(value);

    public static implicit operator ShadowRootInnerHTML(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowRootSetHTMLUnsafeHtml(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ShadowRootSetHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    public static implicit operator ShadowRootSetHTMLUnsafeHtml(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedWorkerOptionsValue(string, SharedWorkerOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    public static implicit operator SharedWorkerOptionsValue(string value)
        => new(value);

    public static implicit operator SharedWorkerOptionsValue(SharedWorkerOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedWorkerScriptURL(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator SharedWorkerScriptURL(TrustedScriptURL value)
        => new(value);

    public static implicit operator SharedWorkerScriptURL(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StartInDirectory(WellKnownDirectory, FileSystemHandle)
{

    public WellKnownDirectory? AsWellKnownDirectory => Value is WellKnownDirectory value ? value : default(WellKnownDirectory?);

    public FileSystemHandle? AsFileSystemHandle => Value is FileSystemHandle value ? value : default(FileSystemHandle?);

    public static implicit operator StartInDirectory(WellKnownDirectory value)
        => new(value);

    public static implicit operator StartInDirectory(FileSystemHandle value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator StartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    public static implicit operator StartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StorageAccessHandleCreateObjectURLObj(Blob, MediaSource)
{

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public static implicit operator StorageAccessHandleCreateObjectURLObj(Blob value)
        => new(value);

    public static implicit operator StorageAccessHandleCreateObjectURLObj(MediaSource value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StorageAccessHandleSharedWorkerOptions(string, SharedWorkerOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    public static implicit operator StorageAccessHandleSharedWorkerOptions(string value)
        => new(value);

    public static implicit operator StorageAccessHandleSharedWorkerOptions(SharedWorkerOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCache(Node, string)
{

    public Node? AsNode => Value is Node value ? value : default(Node?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCache(Node value)
        => new(value);

    public static implicit operator StructuralCache(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue(string, CanvasGradient, CanvasPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    public static implicit operator StructuralCacheValue(string value)
        => new(value);

    public static implicit operator StructuralCacheValue(CanvasGradient value)
        => new(value);

    public static implicit operator StructuralCacheValue(CanvasPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue10(TrustedType, string)
{

    public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue10(TrustedType value)
        => new(value);

    public static implicit operator StructuralCacheValue10(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue11(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue11(TrustedHTML value)
        => new(value);

    public static implicit operator StructuralCacheValue11(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue12(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue12(TrustedHTML value)
        => new(value);

    public static implicit operator StructuralCacheValue12(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue15(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator StructuralCacheValue15(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue15(CSSPseudoElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue16(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator StructuralCacheValue16(string value)
        => new(value);

    public static implicit operator StructuralCacheValue16(ElementCreationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue17(bool, ImportNodeOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    public static implicit operator StructuralCacheValue17(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue17(ImportNodeOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue18(bool, double, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue18(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue18(double value)
        => new(value);

    public static implicit operator StructuralCacheValue18(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue19(TogglePopoverOptions, bool)
{

    public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator StructuralCacheValue19(TogglePopoverOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue19(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue2(double, DOMPointInit)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    public static implicit operator StructuralCacheValue2(double value)
        => new(value);

    public static implicit operator StructuralCacheValue2(DOMPointInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue20(AddEventListenerOptions, bool)
{

    public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator StructuralCacheValue20(AddEventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue20(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue21(EventListenerOptions, bool)
{

    public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator StructuralCacheValue21(EventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue21(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue22(string, PerformanceMeasureOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public PerformanceMeasureOptions? AsPerformanceMeasureOptions => Value is PerformanceMeasureOptions value ? value : default(PerformanceMeasureOptions?);

    public static implicit operator StructuralCacheValue22(string value)
        => new(value);

    public static implicit operator StructuralCacheValue22(PerformanceMeasureOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue23(string, ArrayBuffer)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    public static implicit operator StructuralCacheValue23(string value)
        => new(value);

    public static implicit operator StructuralCacheValue23(ArrayBuffer value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue24(Blob, MediaSource)
{

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public static implicit operator StructuralCacheValue24(Blob value)
        => new(value);

    public static implicit operator StructuralCacheValue24(MediaSource value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue26(HTMLCollection, Element)
{

    public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue26(HTMLCollection value)
        => new(value);

    public static implicit operator StructuralCacheValue26(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue27(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue27(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue27(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue28(HTMLOptionElement, HTMLOptGroupElement)
{

    public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    public static implicit operator StructuralCacheValue28(HTMLOptionElement value)
        => new(value);

    public static implicit operator StructuralCacheValue28(HTMLOptGroupElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue29(HTMLElement, int)
{

    public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    public int? AsInt => Value is int value ? value : default(int?);

    public static implicit operator StructuralCacheValue29(HTMLElement value)
        => new(value);

    public static implicit operator StructuralCacheValue29(int value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue30(VideoTrack, AudioTrack, TextTrack)
{

    public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    public static implicit operator StructuralCacheValue30(VideoTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue30(AudioTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue30(TextTrack value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue31(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue31(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue31(Element value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue32(Element, Text)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Text? AsText => Value is Text value ? value : default(Text?);

    public static implicit operator StructuralCacheValue32(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue32(Text value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue33(Path2D, string)
{

    public Path2D? AsPath2D => Value is Path2D value ? value : default(Path2D?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue33(Path2D value)
        => new(value);

    public static implicit operator StructuralCacheValue33(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue34(JSFile, string, FormData)
{

    public JSFile? AsJSFile => Value is JSFile value ? value : default(JSFile?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator StructuralCacheValue34(JSFile value)
        => new(value);

    public static implicit operator StructuralCacheValue34(string value)
        => new(value);

    public static implicit operator StructuralCacheValue34(FormData value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue35(SanitizerConfig, SanitizerPresets)
{

    public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    public static implicit operator StructuralCacheValue35(SanitizerConfig value)
        => new(value);

    public static implicit operator StructuralCacheValue35(SanitizerPresets value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue36(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue36(TrustedScriptURL value)
        => new(value);

    public static implicit operator StructuralCacheValue36(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue37(string, SharedWorkerOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    public static implicit operator StructuralCacheValue37(string value)
        => new(value);

    public static implicit operator StructuralCacheValue37(SharedWorkerOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue4(double, KeyframeAnimationOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    public static implicit operator StructuralCacheValue4(double value)
        => new(value);

    public static implicit operator StructuralCacheValue4(KeyframeAnimationOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue41(Element, JazorDocument)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public static implicit operator StructuralCacheValue41(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue41(JazorDocument value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue43(Client, ServiceWorker, MessagePort)
{

    public Client? AsClient => Value is Client value ? value : default(Client?);

    public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    public static implicit operator StructuralCacheValue43(Client value)
        => new(value);

    public static implicit operator StructuralCacheValue43(ServiceWorker value)
        => new(value);

    public static implicit operator StructuralCacheValue43(MessagePort value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue44(string, TrustedScriptURL)
{

    public string? AsString => Value is string value ? value : default(string?);

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public static implicit operator StructuralCacheValue44(string value)
        => new(value);

    public static implicit operator StructuralCacheValue44(TrustedScriptURL value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue45(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator StructuralCacheValue45(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue45(CSSPseudoElement value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue46(JazorDocument, XMLHttpRequestBodyInit)
{

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator StructuralCacheValue46(JazorDocument value)
        => new(value);

    public static implicit operator StructuralCacheValue46(XMLHttpRequestBodyInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue48(double, KeyframeEffectOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeEffectOptions? AsKeyframeEffectOptions => Value is KeyframeEffectOptions value ? value : default(KeyframeEffectOptions?);

    public static implicit operator StructuralCacheValue48(double value)
        => new(value);

    public static implicit operator StructuralCacheValue48(KeyframeEffectOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue49(double, EffectTiming)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    public static implicit operator StructuralCacheValue49(double value)
        => new(value);

    public static implicit operator StructuralCacheValue49(EffectTiming value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue5(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public static implicit operator StructuralCacheValue5(HTMLCanvasElement value)
        => new(value);

    public static implicit operator StructuralCacheValue5(OffscreenCanvas value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue50(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator StructuralCacheValue50(string value)
        => new(value);

    public static implicit operator StructuralCacheValue50(uint value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue51(string, AudioSinkInfo)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkInfo? AsAudioSinkInfo => Value is AudioSinkInfo value ? value : default(AudioSinkInfo?);

    public static implicit operator StructuralCacheValue51(string value)
        => new(value);

    public static implicit operator StructuralCacheValue51(AudioSinkInfo value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue52(string, AudioSinkOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    public static implicit operator StructuralCacheValue52(string value)
        => new(value);

    public static implicit operator StructuralCacheValue52(AudioSinkOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue58(MediaStreamTrack, string)
{

    public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue58(MediaStreamTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue58(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue59(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    public static implicit operator StructuralCacheValue59(RTCEncodedVideoFrame value)
        => new(value);

    public static implicit operator StructuralCacheValue59(RTCEncodedAudioFrame value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue6(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue6(TrustedHTML value)
        => new(value);

    public static implicit operator StructuralCacheValue6(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue8(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator StructuralCacheValue8(ViewTransitionUpdateCallback value)
        => new(value);

    public static implicit operator StructuralCacheValue8(StartViewTransitionOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue9(bool, ScrollIntoViewOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    public static implicit operator StructuralCacheValue9(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue9(ScrollIntoViewOptions value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValueValue(CryptoKey, CryptoKeyPair)
{

    public CryptoKey? AsCryptoKey => Value is CryptoKey value ? value : default(CryptoKey?);

    public CryptoKeyPair? AsCryptoKeyPair => Value is CryptoKeyPair value ? value : default(CryptoKeyPair?);

    public static implicit operator StructuralCacheValueValue(CryptoKey value)
        => new(value);

    public static implicit operator StructuralCacheValueValue(CryptoKeyPair value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValueValue2(ArrayBuffer, JsonWebKey)
{

    public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    public JsonWebKey? AsJsonWebKey => Value is JsonWebKey value ? value : default(JsonWebKey?);

    public static implicit operator StructuralCacheValueValue2(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValueValue2(JsonWebKey value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SubtleCryptoExportKeyResultValue(ArrayBuffer, JsonWebKey)
{

    public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    public JsonWebKey? AsJsonWebKey => Value is JsonWebKey value ? value : default(JsonWebKey?);

    public static implicit operator SubtleCryptoExportKeyResultValue(ArrayBuffer value)
        => new(value);

    public static implicit operator SubtleCryptoExportKeyResultValue(JsonWebKey value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SubtleCryptoGenerateKeyResultValue(CryptoKey, CryptoKeyPair)
{

    public CryptoKey? AsCryptoKey => Value is CryptoKey value ? value : default(CryptoKey?);

    public CryptoKeyPair? AsCryptoKeyPair => Value is CryptoKeyPair value ? value : default(CryptoKeyPair?);

    public static implicit operator SubtleCryptoGenerateKeyResultValue(CryptoKey value)
        => new(value);

    public static implicit operator SubtleCryptoGenerateKeyResultValue(CryptoKeyPair value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TaskSignalAnyInitPriority(TaskPriority, TaskSignal)
{

    public TaskPriority? AsTaskPriority => Value is TaskPriority value ? value : default(TaskPriority?);

    public TaskSignal? AsTaskSignal => Value is TaskSignal value ? value : default(TaskSignal?);

    public static implicit operator TaskSignalAnyInitPriority(TaskPriority value)
        => new(value);

    public static implicit operator TaskSignalAnyInitPriority(TaskSignal value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TexImageSource(ImageBitmap, ImageData, HTMLImageElement, HTMLCanvasElement, HTMLVideoElement, OffscreenCanvas, VideoFrame)
{

    public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    public static implicit operator TexImageSource(ImageBitmap value)
        => new(value);

    public static implicit operator TexImageSource(ImageData value)
        => new(value);

    public static implicit operator TexImageSource(HTMLImageElement value)
        => new(value);

    public static implicit operator TexImageSource(HTMLCanvasElement value)
        => new(value);

    public static implicit operator TexImageSource(HTMLVideoElement value)
        => new(value);

    public static implicit operator TexImageSource(OffscreenCanvas value)
        => new(value);

    public static implicit operator TexImageSource(VideoFrame value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TimerHandler(string, Delegate, TrustedScript)
{

    public string? AsString => Value is string value ? value : default(string?);

    public Delegate? AsDelegate => Value is Delegate value ? value : default(Delegate?);

    public TrustedScript? AsTrustedScript => Value is TrustedScript value ? value : default(TrustedScript?);

    public static implicit operator TimerHandler(string value)
        => new(value);

    public static implicit operator TimerHandler(Delegate value)
        => new(value);

    public static implicit operator TimerHandler(TrustedScript value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TogglePopoverOptionsValue(TogglePopoverOptions, bool)
{

    public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator TogglePopoverOptionsValue(TogglePopoverOptions value)
        => new(value);

    public static implicit operator TogglePopoverOptionsValue(bool value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrackEventInitTrack(VideoTrack, AudioTrack, TextTrack)
{

    public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    public static implicit operator TrackEventInitTrack(VideoTrack value)
        => new(value);

    public static implicit operator TrackEventInitTrack(AudioTrack value)
        => new(value);

    public static implicit operator TrackEventInitTrack(TextTrack value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrackEventTrack(VideoTrack, AudioTrack, TextTrack)
{

    public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    public static implicit operator TrackEventTrack(VideoTrack value)
        => new(value);

    public static implicit operator TrackEventTrack(AudioTrack value)
        => new(value);

    public static implicit operator TrackEventTrack(TextTrack value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrustedType(TrustedHTML, TrustedScript, TrustedScriptURL)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public TrustedScript? AsTrustedScript => Value is TrustedScript value ? value : default(TrustedScript?);

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public static implicit operator TrustedType(TrustedHTML value)
        => new(value);

    public static implicit operator TrustedType(TrustedScript value)
        => new(value);

    public static implicit operator TrustedType(TrustedScriptURL value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLCreateObjectURLObj(Blob, MediaSource)
{

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public static implicit operator URLCreateObjectURLObj(Blob value)
        => new(value);

    public static implicit operator URLCreateObjectURLObj(MediaSource value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLPatternCompatible(string, URLPatternInit, URLPattern)
{

    public string? AsString => Value is string value ? value : default(string?);

    public URLPatternInit? AsURLPatternInit => Value is URLPatternInit value ? value : default(URLPatternInit?);

    public URLPattern? AsURLPattern => Value is URLPattern value ? value : default(URLPattern?);

    public static implicit operator URLPatternCompatible(string value)
        => new(value);

    public static implicit operator URLPatternCompatible(URLPatternInit value)
        => new(value);

    public static implicit operator URLPatternCompatible(URLPattern value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLPatternInput(string, URLPatternInit)
{

    public string? AsString => Value is string value ? value : default(string?);

    public URLPatternInit? AsURLPatternInit => Value is URLPatternInit value ? value : default(URLPatternInit?);

    public static implicit operator URLPatternInput(string value)
        => new(value);

    public static implicit operator URLPatternInput(URLPatternInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union UrnOrConfig(string, FencedFrameConfig)
{

    public string? AsString => Value is string value ? value : default(string?);

    public FencedFrameConfig? AsFencedFrameConfig => Value is FencedFrameConfig value ? value : default(FencedFrameConfig?);

    public static implicit operator UrnOrConfig(string value)
        => new(value);

    public static implicit operator UrnOrConfig(FencedFrameConfig value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ViewTimelineOptionsInset(CSSNumericValue, CSSKeywordValue)
{

    public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    public static implicit operator ViewTimelineOptionsInset(CSSNumericValue value)
        => new(value);

    public static implicit operator ViewTimelineOptionsInset(CSSKeywordValue value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WebGL2RenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public static implicit operator WebGL2RenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator WebGL2RenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WebGLRenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public static implicit operator WebGLRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator WebGLRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerGlobalScopeImportScriptsUrls(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator WorkerGlobalScopeImportScriptsUrls(TrustedScriptURL value)
        => new(value);

    public static implicit operator WorkerGlobalScopeImportScriptsUrls(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerOrWorkerAndParameters(Worker, WorkerAndParameters)
{

    public Worker? AsWorker => Value is Worker value ? value : default(Worker?);

    public WorkerAndParameters? AsWorkerAndParameters => Value is WorkerAndParameters value ? value : default(WorkerAndParameters?);

    public static implicit operator WorkerOrWorkerAndParameters(Worker value)
        => new(value);

    public static implicit operator WorkerOrWorkerAndParameters(WorkerAndParameters value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerScriptURL(TrustedScriptURL, string)
{

    public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator WorkerScriptURL(TrustedScriptURL value)
        => new(value);

    public static implicit operator WorkerScriptURL(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WriteText(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator WriteText(TrustedHTML value)
        => new(value);

    public static implicit operator WriteText(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WritelnText(TrustedHTML, string)
{

    public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator WritelnText(TrustedHTML value)
        => new(value);

    public static implicit operator WritelnText(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XMLHttpRequestSendBody(JazorDocument, XMLHttpRequestBodyInit)
{

    public JazorDocument? AsDocument => Value is JazorDocument value ? value : default(JazorDocument?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator XMLHttpRequestSendBody(JazorDocument value)
        => new(value);

    public static implicit operator XMLHttpRequestSendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XPathNSResolverValue(XPathNSResolverLiteral, LookupNamespaceURICallback)
{

    public XPathNSResolverLiteral? AsXPathNSResolverLiteral => Value is XPathNSResolverLiteral value ? value : default(XPathNSResolverLiteral?);

    public LookupNamespaceURICallback? AsLookupNamespaceURICallback => Value is LookupNamespaceURICallback value ? value : default(LookupNamespaceURICallback?);

    public static implicit operator XPathNSResolverValue(XPathNSResolverLiteral value)
        => new(value);

    public static implicit operator XPathNSResolverValue(LookupNamespaceURICallback value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XRWebGLRenderingContext(WebGLRenderingContext, WebGL2RenderingContext)
{

    public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    public static implicit operator XRWebGLRenderingContext(WebGLRenderingContext value)
        => new(value);

    public static implicit operator XRWebGLRenderingContext(WebGL2RenderingContext value)
        => new(value);
}
