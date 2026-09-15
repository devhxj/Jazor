namespace ECMAScript;

/// <summary>WebIDL 联合值：object、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 object 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public object? AsObject => _kind == 1 ? _value1 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 2 ? _value2 : default;

    /// <summary>将 object 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static AlgorithmIdentifier FromObject(object value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AlgorithmIdentifier(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：ArrayBuffer、SharedArrayBuffer、IArrayBufferView。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => _kind == 1 ? _value1 : default;

    /// <summary>读取 SharedArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SharedArrayBuffer? AsSharedArrayBuffer => _kind == 2 ? _value2 : default;

    /// <summary>读取 IArrayBufferView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IArrayBufferView? AsIArrayBufferView => _kind == 3 ? _value3 : default;

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(ArrayBuffer value)
        => new(value);

    /// <summary>将 SharedArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(SharedArrayBuffer value)
        => new(value);

    /// <summary>将 IArrayBufferView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static AllowSharedBufferSource FromIArrayBufferView(IArrayBufferView value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowSharedBufferSource(BigUint64Array value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static BlobPart FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BlobPart(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：IArrayBufferView、ArrayBuffer。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IArrayBufferView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IArrayBufferView? AsIArrayBufferView => _kind == 1 ? _value1 : default;

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => _kind == 2 ? _value2 : default;

    /// <summary>将 IArrayBufferView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static BufferSource FromIArrayBufferView(IArrayBufferView value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(BigUint64Array value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BufferSource(ArrayBuffer value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string、WriteParams。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>读取 WriteParams 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WriteParams? AsWriteParams => _kind == 4 ? _value4 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static FileSystemWriteChunkType FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(string value)
        => new(value);

    /// <summary>将 WriteParams 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileSystemWriteChunkType(WriteParams value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、IBufferSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 1 ? _value1 : default;

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(string value)
        => new(value);

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static FontFaceSourceValue FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FontFaceSourceValue(BigUint64Array value)
        => new(value);
}

/// <summary>WebIDL 联合值：IDBObjectStore、IDBIndex。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IDBObjectStore 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    /// <summary>读取 IDBIndex 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    /// <summary>将 IDBObjectStore 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static IDBCursorSource FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    /// <summary>将 IDBIndex 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static IDBCursorSource FromIDBIndex(IDBIndex value)
        => new(value);
}

/// <summary>WebIDL 联合值：IDBObjectStore、IDBIndex、IDBCursor。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IDBObjectStore 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    /// <summary>读取 IDBIndex 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    /// <summary>读取 IDBCursor 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBCursor? AsIDBCursor => _kind == 3 ? _value3 : default;

    /// <summary>将 IDBObjectStore 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static IDBRequestSource FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    /// <summary>将 IDBIndex 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static IDBRequestSource FromIDBIndex(IDBIndex value)
        => new(value);

    /// <summary>将 IDBCursor 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static IDBRequestSource FromIDBCursor(IDBCursor value)
        => new(value);
}

/// <summary>WebIDL 联合值：IAllowSharedBufferSource、ReadableStream。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IAllowSharedBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IAllowSharedBufferSource? AsIAllowSharedBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 ReadableStream 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStream? AsReadableStream => _kind == 2 ? _value2 : default;

    /// <summary>将 IAllowSharedBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static ImageBufferSource FromIAllowSharedBufferSource(IAllowSharedBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(ArrayBuffer value)
        => new(value);

    /// <summary>将 SharedArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(SharedArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(BigUint64Array value)
        => new(value);

    /// <summary>将 ReadableStream 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBufferSource(ReadableStream value)
        => new(value);
}

/// <summary>WebIDL 联合值：ImageBitmapSource、AudioBuffer、IBufferSource、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 ImageBitmapSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmapSource? AsImageBitmapSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 AudioBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioBuffer? AsAudioBuffer => _kind == 2 ? _value2 : default;

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 3 ? _value3 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 4 ? _value4 : default;

    /// <summary>将 ImageBitmapSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(ImageBitmapSource value)
        => new(value);

    /// <summary>将 AudioBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(AudioBuffer value)
        => new(value);

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static LanguageModelMessageValue FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(BigUint64Array value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、IBufferSource、NDEFMessageInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 1 ? _value1 : default;

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    /// <summary>读取 NDEFMessageInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public NDEFMessageInit? AsNDEFMessageInit => _kind == 3 ? _value3 : default;

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(string value)
        => new(value);

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static NDEFMessageSource FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(BigUint64Array value)
        => new(value);

    /// <summary>将 NDEFMessageInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NDEFMessageSource(NDEFMessageInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 2 ? _value2 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static PushMessageDataInit FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(BigUint64Array value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushMessageDataInit(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 2 ? _value2 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static PushSubscriptionOptionsInitApplicationServerKey FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(BigUint64Array value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PushSubscriptionOptionsInitApplicationServerKey(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static SendData FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendData(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、IBufferSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue11
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly IBufferSource? _value2;

    private StructuralCacheValue11(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue11(IBufferSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 1 ? _value1 : default;

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(string value)
        => new(value);

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue11 FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue11(BigUint64Array value)
        => new(value);
}

/// <summary>WebIDL 联合值：IDBObjectStore、IDBIndex、IDBCursor。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue39
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;
    private readonly IDBCursor? _value3;

    private StructuralCacheValue39(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue39(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue39(IDBCursor value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    /// <summary>读取 IDBObjectStore 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    /// <summary>读取 IDBIndex 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    /// <summary>读取 IDBCursor 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBCursor? AsIDBCursor => _kind == 3 ? _value3 : default;

    /// <summary>将 IDBObjectStore 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue39 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    /// <summary>将 IDBIndex 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue39 FromIDBIndex(IDBIndex value)
        => new(value);

    /// <summary>将 IDBCursor 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue39 FromIDBCursor(IDBCursor value)
        => new(value);
}

/// <summary>WebIDL 联合值：IDBObjectStore、IDBIndex。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue41
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;

    private StructuralCacheValue41(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue41(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 IDBObjectStore 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    /// <summary>读取 IDBIndex 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    /// <summary>将 IDBObjectStore 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue41 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    /// <summary>将 IDBIndex 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue41 FromIDBIndex(IDBIndex value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、JsonWebKey。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue54
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly JsonWebKey? _value2;

    private StructuralCacheValue54(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue54(JsonWebKey value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 JsonWebKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public JsonWebKey? AsJsonWebKey => _kind == 2 ? _value2 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue54 FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(BigUint64Array value)
        => new(value);

    /// <summary>将 JsonWebKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue54(JsonWebKey value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue61
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private StructuralCacheValue61(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue61(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue61(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static StructuralCacheValue61 FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue61(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、JsonWebKey。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 JsonWebKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public JsonWebKey? AsJsonWebKey => _kind == 2 ? _value2 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static SubtleCryptoImportKeyKeyData FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(BigUint64Array value)
        => new(value);

    /// <summary>将 JsonWebKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoImportKeyKeyData(JsonWebKey value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static WebSocketSendData FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketSendData(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：IBufferSource、Blob、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 2 ? _value2 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 3 ? _value3 : default;

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static WriteParamsData FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(BigUint64Array value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(Blob value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteParamsData(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Blob、IBufferSource、FormData、URLSearchParams、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
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

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => _kind == 1 ? _value1 : default;

    /// <summary>读取 IBufferSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public IBufferSource? AsIBufferSource => _kind == 2 ? _value2 : default;

    /// <summary>读取 FormData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FormData? AsFormData => _kind == 3 ? _value3 : default;

    /// <summary>读取 URLSearchParams 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public URLSearchParams? AsURLSearchParams => _kind == 4 ? _value4 : default;

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => _kind == 5 ? _value5 : default;

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Blob value)
        => new(value);

    /// <summary>将 IBufferSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static XMLHttpRequestBodyInit FromIBufferSource(IBufferSource value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(ArrayBuffer value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(DataView value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Uint8Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Int16Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Uint16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Int32Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Uint32Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(Float64Array value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(BigUint64Array value)
        => new(value);

    /// <summary>将 FormData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(FormData value)
        => new(value);

    /// <summary>将 URLSearchParams 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(URLSearchParams value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestBodyInit(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：RouterRule、RouterRule[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AddRoutesRulesCollectionBuilder), nameof(AddRoutesRulesCollectionBuilder.Create))]
public readonly union AddRoutesRules(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    /// <summary>读取 RouterRule 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    /// <summary>读取 RouterRule[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    /// <summary>将 RouterRule 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddRoutesRules(RouterRule value)
        => new(value);

    /// <summary>将 RouterRule[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddRoutesRules(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

/// <summary>为 AddRoutesRules 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AddRoutesRulesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static AddRoutesRules Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、UUID[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder), nameof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder.Create))]
public readonly union AllowedBluetoothDeviceAllowedServices(string, UUID[]) : IEnumerable<UUID>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 UUID[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public UUID[]? AsUUIDArray => Value is UUID[] value ? value : default(UUID[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowedBluetoothDeviceAllowedServices(string value)
        => new(value);

    /// <summary>将 UUID[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AllowedBluetoothDeviceAllowedServices(UUID[] value)
        => new(value);

    IEnumerator<UUID> IEnumerable<UUID>.GetEnumerator()
        => ((IEnumerable<UUID>)(AsUUIDArray ?? Array.Empty<UUID>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<UUID>)this).GetEnumerator();
}

/// <summary>为 AllowedBluetoothDeviceAllowedServices 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AllowedBluetoothDeviceAllowedServicesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static AllowedBluetoothDeviceAllowedServices Create(ReadOnlySpan<UUID> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：RequestInfo、RequestInfo[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BackgroundFetchManagerFetchRequestsCollectionBuilder), nameof(BackgroundFetchManagerFetchRequestsCollectionBuilder.Create))]
public readonly union BackgroundFetchManagerFetchRequests(RequestInfo, RequestInfo[]) : IEnumerable<RequestInfo>
{

    /// <summary>读取 RequestInfo 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RequestInfo? AsRequestInfo => Value is RequestInfo value ? value : default(RequestInfo?);

    /// <summary>读取 RequestInfo[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RequestInfo[]? AsRequestInfoArray => Value is RequestInfo[] value ? value : default(RequestInfo[]?);

    /// <summary>将 RequestInfo 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BackgroundFetchManagerFetchRequests(RequestInfo value)
        => new(value);

    /// <summary>将 RequestInfo[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BackgroundFetchManagerFetchRequests(RequestInfo[] value)
        => new(value);

    IEnumerator<RequestInfo> IEnumerable<RequestInfo>.GetEnumerator()
        => ((IEnumerable<RequestInfo>)(AsRequestInfoArray ?? Array.Empty<RequestInfo>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RequestInfo>)this).GetEnumerator();
}

/// <summary>为 BackgroundFetchManagerFetchRequests 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class BackgroundFetchManagerFetchRequestsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static BackgroundFetchManagerFetchRequests Create(ReadOnlySpan<RequestInfo> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：CompositeOperationOrAuto、CompositeOperationOrAuto[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeCompositeCollectionBuilder), nameof(BasePropertyIndexedKeyframeCompositeCollectionBuilder.Create))]
public readonly union BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto, CompositeOperationOrAuto[]) : IEnumerable<CompositeOperationOrAuto>
{

    /// <summary>读取 CompositeOperationOrAuto 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CompositeOperationOrAuto? AsCompositeOperationOrAuto => Value is CompositeOperationOrAuto value ? value : default(CompositeOperationOrAuto?);

    /// <summary>读取 CompositeOperationOrAuto[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => Value is CompositeOperationOrAuto[] value ? value : default(CompositeOperationOrAuto[]?);

    /// <summary>将 CompositeOperationOrAuto 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto value)
        => new(value);

    /// <summary>将 CompositeOperationOrAuto[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto[] value)
        => new(value);

    IEnumerator<CompositeOperationOrAuto> IEnumerable<CompositeOperationOrAuto>.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)(AsCompositeOperationOrAutoArray ?? Array.Empty<CompositeOperationOrAuto>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)this).GetEnumerator();
}

/// <summary>为 BasePropertyIndexedKeyframeComposite 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeCompositeCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static BasePropertyIndexedKeyframeComposite Create(ReadOnlySpan<CompositeOperationOrAuto> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeEasingCollectionBuilder), nameof(BasePropertyIndexedKeyframeEasingCollectionBuilder.Create))]
public readonly union BasePropertyIndexedKeyframeEasing(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeEasing(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeEasing(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 BasePropertyIndexedKeyframeEasing 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeEasingCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static BasePropertyIndexedKeyframeEasing Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double?、double?[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeOffsetCollectionBuilder), nameof(BasePropertyIndexedKeyframeOffsetCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeOffset : System.Runtime.CompilerServices.IUnion, IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    /// <summary>将 double? 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public BasePropertyIndexedKeyframeOffset(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    /// <summary>将 double?[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public BasePropertyIndexedKeyframeOffset(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 double? 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => _kind == 1 ? _value1 : default;

    /// <summary>读取 double?[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

    /// <summary>读取当前分支保存的原始值；未初始化的联合值返回 null。此属性不进行分支转换。</summary>
public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    /// <summary>将 double? 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeOffset(double? value)
        => new(value);

    /// <summary>将 double?[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BasePropertyIndexedKeyframeOffset(double?[] value)
        => new(value);

    IEnumerator<double?> IEnumerable<double?>.GetEnumerator()
        => ((IEnumerable<double?>)(AsDoubleArray ?? Array.Empty<double?>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double?>)this).GetEnumerator();
}

/// <summary>为 BasePropertyIndexedKeyframeOffset 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class BasePropertyIndexedKeyframeOffsetCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static BasePropertyIndexedKeyframeOffset Create(ReadOnlySpan<double?> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CSSFontFeatureValuesMapSetValuesCollectionBuilder), nameof(CSSFontFeatureValuesMapSetValuesCollectionBuilder.Create))]
public readonly union CSSFontFeatureValuesMapSetValues(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSFontFeatureValuesMapSetValues(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSFontFeatureValuesMapSetValues(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 CSSFontFeatureValuesMapSetValues 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CSSFontFeatureValuesMapSetValuesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static CSSFontFeatureValuesMapSetValues Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、DOMPointInit、CanvasRenderingContext2DRoundRectRadii[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union CanvasRenderingContext2DRoundRectRadiiValue(double, DOMPointInit, CanvasRenderingContext2DRoundRectRadii[]) : IEnumerable<CanvasRenderingContext2DRoundRectRadii>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>读取 CanvasRenderingContext2DRoundRectRadii[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasRenderingContext2DRoundRectRadii[]? AsCanvasRenderingContext2DRoundRectRadiiArray => Value is CanvasRenderingContext2DRoundRectRadii[] value ? value : default(CanvasRenderingContext2DRoundRectRadii[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    /// <summary>将 CanvasRenderingContext2DRoundRectRadii[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DRoundRectRadiiValue(CanvasRenderingContext2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<CanvasRenderingContext2DRoundRectRadii> IEnumerable<CanvasRenderingContext2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<CanvasRenderingContext2DRoundRectRadii>)(AsCanvasRenderingContext2DRoundRectRadiiArray ?? Array.Empty<CanvasRenderingContext2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CanvasRenderingContext2DRoundRectRadii>)this).GetEnumerator();
}

/// <summary>为 CanvasRenderingContext2DRoundRectRadiiValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static CanvasRenderingContext2DRoundRectRadiiValue Create(ReadOnlySpan<CanvasRenderingContext2DRoundRectRadii> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]、ConstrainDOMStringParameters。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringCollectionBuilder), nameof(ConstrainDOMStringCollectionBuilder.Create))]
public readonly union ConstrainDOMString(string, string[], ConstrainDOMStringParameters) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>读取 ConstrainDOMStringParameters 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainDOMStringParameters? AsConstrainDOMStringParameters => Value is ConstrainDOMStringParameters value ? value : default(ConstrainDOMStringParameters?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMString(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMString(string[] value)
        => new(value);

    /// <summary>将 ConstrainDOMStringParameters 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMString(ConstrainDOMStringParameters value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 ConstrainDOMString 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static ConstrainDOMString Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersExactCollectionBuilder), nameof(ConstrainDOMStringParametersExactCollectionBuilder.Create))]
public readonly union ConstrainDOMStringParametersExact(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMStringParametersExact(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMStringParametersExact(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 ConstrainDOMStringParametersExact 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringParametersExactCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static ConstrainDOMStringParametersExact Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersIdealCollectionBuilder), nameof(ConstrainDOMStringParametersIdealCollectionBuilder.Create))]
public readonly union ConstrainDOMStringParametersIdeal(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMStringParametersIdeal(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDOMStringParametersIdeal(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 ConstrainDOMStringParametersIdeal 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainDOMStringParametersIdealCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static ConstrainDOMStringParametersIdeal Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Point2D[]、ConstrainPoint2DParameters。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainPoint2DCollectionBuilder), nameof(ConstrainPoint2DCollectionBuilder.Create))]
public readonly union ConstrainPoint2D(Point2D[], ConstrainPoint2DParameters) : IEnumerable<Point2D>
{

    /// <summary>读取 Point2D[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Point2D[]? AsPoint2DArray => Value is Point2D[] value ? value : default(Point2D[]?);

    /// <summary>读取 ConstrainPoint2DParameters 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainPoint2DParameters? AsConstrainPoint2DParameters => Value is ConstrainPoint2DParameters value ? value : default(ConstrainPoint2DParameters?);

    /// <summary>将 Point2D[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainPoint2D(Point2D[] value)
        => new(value);

    /// <summary>将 ConstrainPoint2DParameters 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainPoint2D(ConstrainPoint2DParameters value)
        => new(value);

    IEnumerator<Point2D> IEnumerable<Point2D>.GetEnumerator()
        => ((IEnumerable<Point2D>)(AsPoint2DArray ?? Array.Empty<Point2D>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Point2D>)this).GetEnumerator();
}

/// <summary>为 ConstrainPoint2D 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ConstrainPoint2DCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static ConstrainPoint2D Create(ReadOnlySpan<Point2D> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder), nameof(CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder.Create))]
public readonly union CrossOriginStorageRequestFileHandleOptionsOrigins(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CrossOriginStorageRequestFileHandleOptionsOrigins(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CrossOriginStorageRequestFileHandleOptionsOrigins(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 CrossOriginStorageRequestFileHandleOptionsOrigins 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CrossOriginStorageRequestFileHandleOptionsOriginsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static CrossOriginStorageRequestFileHandleOptionsOrigins Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、double[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixInitValueCollectionBuilder), nameof(DOMMatrixInitValueCollectionBuilder.Create))]
public readonly union DOMMatrixInitValue(string, double[]) : IEnumerable<double>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 double[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMMatrixInitValue(string value)
        => new(value);

    /// <summary>将 double[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMMatrixInitValue(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

/// <summary>为 DOMMatrixInitValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DOMMatrixInitValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DOMMatrixInitValue Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、double[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixReadOnlyInitCollectionBuilder), nameof(DOMMatrixReadOnlyInitCollectionBuilder.Create))]
public readonly union DOMMatrixReadOnlyInit(string, double[]) : IEnumerable<double>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 double[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMMatrixReadOnlyInit(string value)
        => new(value);

    /// <summary>将 double[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMMatrixReadOnlyInit(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

/// <summary>为 DOMMatrixReadOnlyInit 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DOMMatrixReadOnlyInitCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DOMMatrixReadOnlyInit Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double?、double?[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueCollectionBuilder), nameof(DefaultValueCollectionBuilder.Create))]
public readonly struct DefaultValue : System.Runtime.CompilerServices.IUnion, IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    /// <summary>将 double? 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public DefaultValue(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    /// <summary>将 double?[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
public DefaultValue(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    /// <summary>读取 double? 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => _kind == 1 ? _value1 : default;

    /// <summary>读取 double?[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

    /// <summary>读取当前分支保存的原始值；未初始化的联合值返回 null。此属性不进行分支转换。</summary>
public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    /// <summary>将 double? 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValue(double? value)
        => new(value);

    /// <summary>将 double?[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValue(double?[] value)
        => new(value);

    IEnumerator<double?> IEnumerable<double?>.GetEnumerator()
        => ((IEnumerable<double?>)(AsDoubleArray ?? Array.Empty<double?>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double?>)this).GetEnumerator();
}

/// <summary>为 DefaultValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DefaultValue Create(ReadOnlySpan<double?> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：CompositeOperationOrAuto、CompositeOperationOrAuto[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue2CollectionBuilder), nameof(DefaultValueValue2CollectionBuilder.Create))]
public readonly union DefaultValueValue2(CompositeOperationOrAuto, CompositeOperationOrAuto[]) : IEnumerable<CompositeOperationOrAuto>
{

    /// <summary>读取 CompositeOperationOrAuto 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CompositeOperationOrAuto? AsCompositeOperationOrAuto => Value is CompositeOperationOrAuto value ? value : default(CompositeOperationOrAuto?);

    /// <summary>读取 CompositeOperationOrAuto[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => Value is CompositeOperationOrAuto[] value ? value : default(CompositeOperationOrAuto[]?);

    /// <summary>将 CompositeOperationOrAuto 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue2(CompositeOperationOrAuto value)
        => new(value);

    /// <summary>将 CompositeOperationOrAuto[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue2(CompositeOperationOrAuto[] value)
        => new(value);

    IEnumerator<CompositeOperationOrAuto> IEnumerable<CompositeOperationOrAuto>.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)(AsCompositeOperationOrAutoArray ?? Array.Empty<CompositeOperationOrAuto>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<CompositeOperationOrAuto>)this).GetEnumerator();
}

/// <summary>为 DefaultValueValue2 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValue2CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DefaultValueValue2 Create(ReadOnlySpan<CompositeOperationOrAuto> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue3CollectionBuilder), nameof(DefaultValueValue3CollectionBuilder.Create))]
public readonly union DefaultValueValue3(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue3(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue3(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 DefaultValueValue3 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValue3CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DefaultValueValue3 Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValueCollectionBuilder), nameof(DefaultValueValueCollectionBuilder.Create))]
public readonly union DefaultValueValue(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DefaultValueValue(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 DefaultValueValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DefaultValueValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static DefaultValueValue Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(FilePickerAcceptTypeAcceptValueCollectionBuilder), nameof(FilePickerAcceptTypeAcceptValueCollectionBuilder.Create))]
public readonly union FilePickerAcceptTypeAcceptValue(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FilePickerAcceptTypeAcceptValue(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FilePickerAcceptTypeAcceptValue(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 FilePickerAcceptTypeAcceptValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class FilePickerAcceptTypeAcceptValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static FilePickerAcceptTypeAcceptValue Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Float32Array、GLfloat[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Float32ListCollectionBuilder), nameof(Float32ListCollectionBuilder.Create))]
public readonly union Float32List(Float32Array, GLfloat[]) : IEnumerable<GLfloat>
{

    /// <summary>读取 Float32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    /// <summary>读取 GLfloat[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLfloat[]? AsGLfloatArray => Value is GLfloat[] value ? value : default(GLfloat[]?);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Float32List(Float32Array value)
        => new(value);

    /// <summary>将 GLfloat[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Float32List(GLfloat[] value)
        => new(value);

    IEnumerator<GLfloat> IEnumerable<GLfloat>.GetEnumerator()
        => ((IEnumerable<GLfloat>)(AsGLfloatArray ?? Array.Empty<GLfloat>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLfloat>)this).GetEnumerator();
}

/// <summary>为 Float32List 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Float32ListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static Float32List Create(ReadOnlySpan<GLfloat> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string[][]、Dictionary&lt;string, string&gt;。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(HeadersInitCollectionBuilder), nameof(HeadersInitCollectionBuilder.Create))]
public readonly union HeadersInit(string[][], Dictionary<string, string>) : IEnumerable<string[]>
{

    /// <summary>读取 string[][] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    /// <summary>读取 Dictionary&lt;string, string&gt; 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    /// <summary>将 string[][] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HeadersInit(string[][] value)
        => new(value);

    /// <summary>将 Dictionary&lt;string, string&gt; 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HeadersInit(Dictionary<string, string> value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

/// <summary>为 HeadersInit 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HeadersInitCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static HeadersInit Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBDatabaseTransactionStoreNamesCollectionBuilder), nameof(IDBDatabaseTransactionStoreNamesCollectionBuilder.Create))]
public readonly union IDBDatabaseTransactionStoreNames(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBDatabaseTransactionStoreNames(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBDatabaseTransactionStoreNames(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 IDBDatabaseTransactionStoreNames 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBDatabaseTransactionStoreNamesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static IDBDatabaseTransactionStoreNames Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder), nameof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder.Create))]
public readonly union IDBObjectStoreCreateIndexKeyPath(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBObjectStoreCreateIndexKeyPath(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBObjectStoreCreateIndexKeyPath(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 IDBObjectStoreCreateIndexKeyPath 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBObjectStoreCreateIndexKeyPathCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static IDBObjectStoreCreateIndexKeyPath Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreParametersKeyPathCollectionBuilder), nameof(IDBObjectStoreParametersKeyPathCollectionBuilder.Create))]
public readonly union IDBObjectStoreParametersKeyPath(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBObjectStoreParametersKeyPath(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IDBObjectStoreParametersKeyPath(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 IDBObjectStoreParametersKeyPath 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IDBObjectStoreParametersKeyPathCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static IDBObjectStoreParametersKeyPath Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：RouterRule、RouterRule[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(InstallEventAddRoutesRulesCollectionBuilder), nameof(InstallEventAddRoutesRulesCollectionBuilder.Create))]
public readonly union InstallEventAddRoutesRules(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    /// <summary>读取 RouterRule 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    /// <summary>读取 RouterRule[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    /// <summary>将 RouterRule 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator InstallEventAddRoutesRules(RouterRule value)
        => new(value);

    /// <summary>将 RouterRule[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator InstallEventAddRoutesRules(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

/// <summary>为 InstallEventAddRoutesRules 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class InstallEventAddRoutesRulesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static InstallEventAddRoutesRules Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Int32ListCollectionBuilder), nameof(Int32ListCollectionBuilder.Create))]
public readonly union Int32List(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Int32List(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Int32List(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 Int32List 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Int32ListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static Int32List Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、double[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IntersectionObserverInitThresholdCollectionBuilder), nameof(IntersectionObserverInitThresholdCollectionBuilder.Create))]
public readonly union IntersectionObserverInitThreshold(double, double[]) : IEnumerable<double>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 double[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverInitThreshold(double value)
        => new(value);

    /// <summary>将 double[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverInitThreshold(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

/// <summary>为 IntersectionObserverInitThreshold 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IntersectionObserverInitThresholdCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static IntersectionObserverInitThreshold Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、LanguageModelMessageContent[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(LanguageModelMessageContentValueCollectionBuilder), nameof(LanguageModelMessageContentValueCollectionBuilder.Create))]
public readonly union LanguageModelMessageContentValue(string, LanguageModelMessageContent[]) : IEnumerable<LanguageModelMessageContent>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 LanguageModelMessageContent[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public LanguageModelMessageContent[]? AsLanguageModelMessageContentArray => Value is LanguageModelMessageContent[] value ? value : default(LanguageModelMessageContent[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageContentValue(string value)
        => new(value);

    /// <summary>将 LanguageModelMessageContent[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelMessageContentValue(LanguageModelMessageContent[] value)
        => new(value);

    IEnumerator<LanguageModelMessageContent> IEnumerable<LanguageModelMessageContent>.GetEnumerator()
        => ((IEnumerable<LanguageModelMessageContent>)(AsLanguageModelMessageContentArray ?? Array.Empty<LanguageModelMessageContent>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<LanguageModelMessageContent>)this).GetEnumerator();
}

/// <summary>为 LanguageModelMessageContentValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class LanguageModelMessageContentValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static LanguageModelMessageContentValue Create(ReadOnlySpan<LanguageModelMessageContent> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：LanguageModelMessage[]、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(LanguageModelPromptCollectionBuilder), nameof(LanguageModelPromptCollectionBuilder.Create))]
public readonly union LanguageModelPrompt(LanguageModelMessage[], string) : IEnumerable<LanguageModelMessage>
{

    /// <summary>读取 LanguageModelMessage[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public LanguageModelMessage[]? AsLanguageModelMessageArray => Value is LanguageModelMessage[] value ? value : default(LanguageModelMessage[]?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 LanguageModelMessage[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelPrompt(LanguageModelMessage[] value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LanguageModelPrompt(string value)
        => new(value);

    IEnumerator<LanguageModelMessage> IEnumerable<LanguageModelMessage>.GetEnumerator()
        => ((IEnumerable<LanguageModelMessage>)(AsLanguageModelMessageArray ?? Array.Empty<LanguageModelMessage>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<LanguageModelMessage>)this).GetEnumerator();
}

/// <summary>为 LanguageModelPrompt 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class LanguageModelPromptCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static LanguageModelPrompt Create(ReadOnlySpan<LanguageModelMessage> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(MLGraphBuilderSplitSplitsCollectionBuilder), nameof(MLGraphBuilderSplitSplitsCollectionBuilder.Create))]
public readonly union MLGraphBuilderSplitSplits(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MLGraphBuilderSplitSplits(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MLGraphBuilderSplitSplits(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 MLGraphBuilderSplitSplits 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class MLGraphBuilderSplitSplitsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static MLGraphBuilderSplitSplits Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、DOMPointInit、OffscreenCanvasRenderingContext2DRoundRectRadii[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union OffscreenCanvasRenderingContext2DRoundRectRadiiValue(double, DOMPointInit, OffscreenCanvasRenderingContext2DRoundRectRadii[]) : IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>读取 OffscreenCanvasRenderingContext2DRoundRectRadii[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvasRenderingContext2DRoundRectRadii[]? AsOffscreenCanvasRenderingContext2DRoundRectRadiiArray => Value is OffscreenCanvasRenderingContext2DRoundRectRadii[] value ? value : default(OffscreenCanvasRenderingContext2DRoundRectRadii[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    /// <summary>将 OffscreenCanvasRenderingContext2DRoundRectRadii[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadiiValue(OffscreenCanvasRenderingContext2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<OffscreenCanvasRenderingContext2DRoundRectRadii> IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>)(AsOffscreenCanvasRenderingContext2DRoundRectRadiiArray ?? Array.Empty<OffscreenCanvasRenderingContext2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>)this).GetEnumerator();
}

/// <summary>为 OffscreenCanvasRenderingContext2DRoundRectRadiiValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static OffscreenCanvasRenderingContext2DRoundRectRadiiValue Create(ReadOnlySpan<OffscreenCanvasRenderingContext2DRoundRectRadii> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、DOMPointInit、Path2DRoundRectRadii[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Path2DRoundRectRadiiValueCollectionBuilder), nameof(Path2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly union Path2DRoundRectRadiiValue(double, DOMPointInit, Path2DRoundRectRadii[]) : IEnumerable<Path2DRoundRectRadii>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>读取 Path2DRoundRectRadii[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Path2DRoundRectRadii[]? AsPath2DRoundRectRadiiArray => Value is Path2DRoundRectRadii[] value ? value : default(Path2DRoundRectRadii[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DRoundRectRadiiValue(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DRoundRectRadiiValue(DOMPointInit value)
        => new(value);

    /// <summary>将 Path2DRoundRectRadii[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DRoundRectRadiiValue(Path2DRoundRectRadii[] value)
        => new(value);

    IEnumerator<Path2DRoundRectRadii> IEnumerable<Path2DRoundRectRadii>.GetEnumerator()
        => ((IEnumerable<Path2DRoundRectRadii>)(AsPath2DRoundRectRadiiArray ?? Array.Empty<Path2DRoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<Path2DRoundRectRadii>)this).GetEnumerator();
}

/// <summary>为 Path2DRoundRectRadiiValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Path2DRoundRectRadiiValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static Path2DRoundRectRadiiValue Create(ReadOnlySpan<Path2DRoundRectRadii> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RTCIceServerUrlsCollectionBuilder), nameof(RTCIceServerUrlsCollectionBuilder.Create))]
public readonly union RTCIceServerUrls(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCIceServerUrls(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCIceServerUrls(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 RTCIceServerUrls 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RTCIceServerUrlsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static RTCIceServerUrls Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、DOMPointInit、RoundRectRadii[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RoundRectRadiiValueCollectionBuilder), nameof(RoundRectRadiiValueCollectionBuilder.Create))]
public readonly union RoundRectRadiiValue(double, DOMPointInit, RoundRectRadii[]) : IEnumerable<RoundRectRadii>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>读取 RoundRectRadii[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RoundRectRadii[]? AsRoundRectRadiiArray => Value is RoundRectRadii[] value ? value : default(RoundRectRadii[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RoundRectRadiiValue(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RoundRectRadiiValue(DOMPointInit value)
        => new(value);

    /// <summary>将 RoundRectRadii[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RoundRectRadiiValue(RoundRectRadii[] value)
        => new(value);

    IEnumerator<RoundRectRadii> IEnumerable<RoundRectRadii>.GetEnumerator()
        => ((IEnumerable<RoundRectRadii>)(AsRoundRectRadiiArray ?? Array.Empty<RoundRectRadii>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RoundRectRadii>)this).GetEnumerator();
}

/// <summary>为 RoundRectRadiiValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RoundRectRadiiValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static RoundRectRadiiValue Create(ReadOnlySpan<RoundRectRadii> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(SetValuesCollectionBuilder), nameof(SetValuesCollectionBuilder.Create))]
public readonly union SetValues(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetValues(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetValues(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 SetValues 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class SetValuesCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static SetValues Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue12CollectionBuilder), nameof(StructuralCacheValue12CollectionBuilder.Create))]
public readonly union StructuralCacheValue12(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue12(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue12(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue12 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue12CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue12 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、double[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue26CollectionBuilder), nameof(StructuralCacheValue26CollectionBuilder.Create))]
public readonly union StructuralCacheValue26(string, double[]) : IEnumerable<double>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 double[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue26(string value)
        => new(value);

    /// <summary>将 double[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue26(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue26 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue26CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue26 Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：double、DOMPointInit、StructuralCacheValue2[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue3CollectionBuilder), nameof(StructuralCacheValue3CollectionBuilder.Create))]
public readonly union StructuralCacheValue3(double, DOMPointInit, StructuralCacheValue2[]) : IEnumerable<StructuralCacheValue2>
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>读取 StructuralCacheValue2[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public StructuralCacheValue2[]? AsStructuralCacheValue2Array => Value is StructuralCacheValue2[] value ? value : default(StructuralCacheValue2[]?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue3(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue3(DOMPointInit value)
        => new(value);

    /// <summary>将 StructuralCacheValue2[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue3(StructuralCacheValue2[] value)
        => new(value);

    IEnumerator<StructuralCacheValue2> IEnumerable<StructuralCacheValue2>.GetEnumerator()
        => ((IEnumerable<StructuralCacheValue2>)(AsStructuralCacheValue2Array ?? Array.Empty<StructuralCacheValue2>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<StructuralCacheValue2>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue3 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue3CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue3 Create(ReadOnlySpan<StructuralCacheValue2> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue40CollectionBuilder), nameof(StructuralCacheValue40CollectionBuilder.Create))]
public readonly union StructuralCacheValue40(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue40(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue40(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue40 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue40CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue40 Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：RouterRule、RouterRule[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue43CollectionBuilder), nameof(StructuralCacheValue43CollectionBuilder.Create))]
public readonly union StructuralCacheValue43(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    /// <summary>读取 RouterRule 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    /// <summary>读取 RouterRule[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    /// <summary>将 RouterRule 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue43(RouterRule value)
        => new(value);

    /// <summary>将 RouterRule[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue43(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue43 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue43CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue43 Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string[][]、Dictionary&lt;string, string&gt;、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue48CollectionBuilder), nameof(StructuralCacheValue48CollectionBuilder.Create))]
public readonly union StructuralCacheValue48(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>
{

    /// <summary>读取 string[][] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    /// <summary>读取 Dictionary&lt;string, string&gt; 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 string[][] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue48(string[][] value)
        => new(value);

    /// <summary>将 Dictionary&lt;string, string&gt; 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue48(Dictionary<string, string> value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue48(string value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue48 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue48CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue48 Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue55CollectionBuilder), nameof(StructuralCacheValue55CollectionBuilder.Create))]
public readonly union StructuralCacheValue55(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue55(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue55(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue55 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue55CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue55 Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue56CollectionBuilder), nameof(StructuralCacheValue56CollectionBuilder.Create))]
public readonly union StructuralCacheValue56(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue56(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue56(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue56 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue56CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue56 Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Uint32Array、GLuint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue57CollectionBuilder), nameof(StructuralCacheValue57CollectionBuilder.Create))]
public readonly union StructuralCacheValue57(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    /// <summary>读取 Uint32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    /// <summary>读取 GLuint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue57(Uint32Array value)
        => new(value);

    /// <summary>将 GLuint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue57(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue57 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue57CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue57 Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue58CollectionBuilder), nameof(StructuralCacheValue58CollectionBuilder.Create))]
public readonly union StructuralCacheValue58(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue58(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue58(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue58 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue58CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue58 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：RequestInfo、RequestInfo[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue7CollectionBuilder), nameof(StructuralCacheValue7CollectionBuilder.Create))]
public readonly union StructuralCacheValue7(RequestInfo, RequestInfo[]) : IEnumerable<RequestInfo>
{

    /// <summary>读取 RequestInfo 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RequestInfo? AsRequestInfo => Value is RequestInfo value ? value : default(RequestInfo?);

    /// <summary>读取 RequestInfo[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RequestInfo[]? AsRequestInfoArray => Value is RequestInfo[] value ? value : default(RequestInfo[]?);

    /// <summary>将 RequestInfo 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue7(RequestInfo value)
        => new(value);

    /// <summary>将 RequestInfo[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue7(RequestInfo[] value)
        => new(value);

    IEnumerator<RequestInfo> IEnumerable<RequestInfo>.GetEnumerator()
        => ((IEnumerable<RequestInfo>)(AsRequestInfoArray ?? Array.Empty<RequestInfo>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RequestInfo>)this).GetEnumerator();
}

/// <summary>为 StructuralCacheValue7 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue7CollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static StructuralCacheValue7 Create(ReadOnlySpan<RequestInfo> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string[][]、Dictionary&lt;string, string&gt;、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(URLSearchParamsInitCollectionBuilder), nameof(URLSearchParamsInitCollectionBuilder.Create))]
public readonly union URLSearchParamsInit(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>
{

    /// <summary>读取 string[][] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    /// <summary>读取 Dictionary&lt;string, string&gt; 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 string[][] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLSearchParamsInit(string[][] value)
        => new(value);

    /// <summary>将 Dictionary&lt;string, string&gt; 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLSearchParamsInit(Dictionary<string, string> value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLSearchParamsInit(string value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

/// <summary>为 URLSearchParamsInit 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class URLSearchParamsInitCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static URLSearchParamsInit Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Uint32Array、GLuint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Uint32ListCollectionBuilder), nameof(Uint32ListCollectionBuilder.Create))]
public readonly union Uint32List(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    /// <summary>读取 Uint32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    /// <summary>读取 GLuint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Uint32List(Uint32Array value)
        => new(value);

    /// <summary>将 GLuint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Uint32List(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

/// <summary>为 Uint32List 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class Uint32ListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static Uint32List Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：uint、uint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(VibratePatternCollectionBuilder), nameof(VibratePatternCollectionBuilder.Create))]
public readonly union VibratePattern(uint, uint[]) : IEnumerable<uint>
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 uint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator VibratePattern(uint value)
        => new(value);

    /// <summary>将 uint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator VibratePattern(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

/// <summary>为 VibratePattern 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VibratePatternCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static VibratePattern Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、ViewTimelineOptionsInset[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ViewTimelineOptionsInsetValueCollectionBuilder), nameof(ViewTimelineOptionsInsetValueCollectionBuilder.Create))]
public readonly union ViewTimelineOptionsInsetValue(string, ViewTimelineOptionsInset[]) : IEnumerable<ViewTimelineOptionsInset>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ViewTimelineOptionsInset[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ViewTimelineOptionsInset[]? AsViewTimelineOptionsInsetArray => Value is ViewTimelineOptionsInset[] value ? value : default(ViewTimelineOptionsInset[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ViewTimelineOptionsInsetValue(string value)
        => new(value);

    /// <summary>将 ViewTimelineOptionsInset[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ViewTimelineOptionsInsetValue(ViewTimelineOptionsInset[] value)
        => new(value);

    IEnumerator<ViewTimelineOptionsInset> IEnumerable<ViewTimelineOptionsInset>.GetEnumerator()
        => ((IEnumerable<ViewTimelineOptionsInset>)(AsViewTimelineOptionsInsetArray ?? Array.Empty<ViewTimelineOptionsInset>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ViewTimelineOptionsInset>)this).GetEnumerator();
}

/// <summary>为 ViewTimelineOptionsInsetValue 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewTimelineOptionsInsetValueCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static ViewTimelineOptionsInsetValue Create(ReadOnlySpan<ViewTimelineOptionsInset> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Uint32Array、GLuint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    /// <summary>读取 Uint32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    /// <summary>读取 GLuint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
        => new(value);

    /// <summary>将 GLuint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Uint32Array、GLuint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    /// <summary>读取 Uint32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    /// <summary>读取 GLuint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
        => new(value);

    /// <summary>将 GLuint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawArraysWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawArraysWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLint[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(Int32Array, GLint[]) : IEnumerable<GLint>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLint[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(Int32Array value)
        => new(value);

    /// <summary>将 GLint[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawElementsWEBGLCountsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawElementsWEBGLCountsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：Int32Array、GLsizei[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder.Create))]
public readonly union WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 GLsizei[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(Int32Array value)
        => new(value);

    /// <summary>将 GLsizei[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

/// <summary>为 WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：string、string[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WebSocketProtocolsCollectionBuilder), nameof(WebSocketProtocolsCollectionBuilder.Create))]
public readonly union WebSocketProtocols(string, string[]) : IEnumerable<string>
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 string[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketProtocols(string value)
        => new(value);

    /// <summary>将 string[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebSocketProtocols(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

/// <summary>为 WebSocketProtocols 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WebSocketProtocolsCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static WebSocketProtocols Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：HTMLElement、int。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AddBefore(HTMLElement, int)
{

    /// <summary>读取 HTMLElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    /// <summary>读取 int 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public int? AsInt => Value is int value ? value : default(int?);

    /// <summary>将 HTMLElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddBefore(HTMLElement value)
        => new(value);

    /// <summary>将 int 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddBefore(int value)
        => new(value);
}

/// <summary>WebIDL 联合值：AddEventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AddEventListenerOptionsValue(AddEventListenerOptions, bool)
{

    /// <summary>读取 AddEventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 AddEventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddEventListenerOptionsValue(AddEventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AddEventListenerOptionsValue(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AfterNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AfterNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AfterNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、KeyframeAnimationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimateOptions(double, KeyframeAnimationOptions)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 KeyframeAnimationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimateOptions(double value)
        => new(value);

    /// <summary>将 KeyframeAnimationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsExitRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeEnd(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeEnd(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeEnd(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeEnd(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsExitRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeStart(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeStart(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeStart(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsExitRangeStart(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeEnd(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeEnd(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeEnd(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeEnd(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AnimationTriggerOptionsRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeStart(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeStart(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeStart(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AnimationTriggerOptionsRangeStart(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AppendNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AppendNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AppendNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Int8Array、Int16Array、Int32Array、Uint8Array、Uint16Array、Uint32Array、Uint8ClampedArray、BigInt64Array、BigUint64Array、Float16Array、Float32Array、Float64Array、DataView。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ArrayBufferView(Int8Array, Int16Array, Int32Array, Uint8Array, Uint16Array, Uint32Array, Uint8ClampedArray, BigInt64Array, BigUint64Array, Float16Array, Float32Array, Float64Array, DataView)
{

    /// <summary>读取 Int8Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int8Array? AsInt8Array => Value is Int8Array value ? value : default(Int8Array?);

    /// <summary>读取 Int16Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int16Array? AsInt16Array => Value is Int16Array value ? value : default(Int16Array?);

    /// <summary>读取 Int32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    /// <summary>读取 Uint8Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint8Array? AsUint8Array => Value is Uint8Array value ? value : default(Uint8Array?);

    /// <summary>读取 Uint16Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint16Array? AsUint16Array => Value is Uint16Array value ? value : default(Uint16Array?);

    /// <summary>读取 Uint32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    /// <summary>读取 Uint8ClampedArray 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint8ClampedArray? AsUint8ClampedArray => Value is Uint8ClampedArray value ? value : default(Uint8ClampedArray?);

    /// <summary>读取 BigInt64Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public BigInt64Array? AsBigInt64Array => Value is BigInt64Array value ? value : default(BigInt64Array?);

    /// <summary>读取 BigUint64Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public BigUint64Array? AsBigUint64Array => Value is BigUint64Array value ? value : default(BigUint64Array?);

    /// <summary>读取 Float16Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float16Array? AsFloat16Array => Value is Float16Array value ? value : default(Float16Array?);

    /// <summary>读取 Float32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    /// <summary>读取 Float64Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float64Array? AsFloat64Array => Value is Float64Array value ? value : default(Float64Array?);

    /// <summary>读取 DataView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DataView? AsDataView => Value is DataView value ? value : default(DataView?);

    /// <summary>将 Int8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Int8Array value)
        => new(value);

    /// <summary>将 Int16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Int16Array value)
        => new(value);

    /// <summary>将 Int32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Int32Array value)
        => new(value);

    /// <summary>将 Uint8Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Uint8Array value)
        => new(value);

    /// <summary>将 Uint16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Uint16Array value)
        => new(value);

    /// <summary>将 Uint32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Uint32Array value)
        => new(value);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 BigInt64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(BigInt64Array value)
        => new(value);

    /// <summary>将 BigUint64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(BigUint64Array value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Float16Array value)
        => new(value);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(Float64Array value)
        => new(value);

    /// <summary>将 DataView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ArrayBufferView(DataView value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、Text。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AssignNodes(Element, Text)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 Text 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Text? AsText => Value is Text value ? value : default(Text?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AssignNodes(Element value)
        => new(value);

    /// <summary>将 Text 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AssignNodes(Text value)
        => new(value);
}

/// <summary>WebIDL 联合值：AudioContextLatencyCategory、double。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsLatencyHint(AudioContextLatencyCategory, double)
{

    /// <summary>读取 AudioContextLatencyCategory 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioContextLatencyCategory? AsAudioContextLatencyCategory => Value is AudioContextLatencyCategory value ? value : default(AudioContextLatencyCategory?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>将 AudioContextLatencyCategory 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsLatencyHint(AudioContextLatencyCategory value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsLatencyHint(double value)
        => new(value);
}

/// <summary>WebIDL 联合值：AudioContextRenderSizeCategory、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory, uint)
{

    /// <summary>读取 AudioContextRenderSizeCategory 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => Value is AudioContextRenderSizeCategory value ? value : default(AudioContextRenderSizeCategory?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 AudioContextRenderSizeCategory 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextOptionsSinkId(string, AudioSinkOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsSinkId(string value)
        => new(value);

    /// <summary>将 AudioSinkOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextOptionsSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextSetSinkId(string, AudioSinkOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextSetSinkId(string value)
        => new(value);

    /// <summary>将 AudioSinkOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextSetSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkInfo。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union AudioContextSinkId(string, AudioSinkInfo)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkInfo 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkInfo? AsAudioSinkInfo => Value is AudioSinkInfo value ? value : default(AudioSinkInfo?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextSinkId(string value)
        => new(value);

    /// <summary>将 AudioSinkInfo 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator AudioContextSinkId(AudioSinkInfo value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BeforeNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BeforeNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BeforeNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothAdvertisingEventInitUUIDs(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothAdvertisingEventInitUUIDs(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothAdvertisingEventInitUUIDs(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothCharacteristicUUID(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothCharacteristicUUID(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothCharacteristicUUID(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothDescriptorUUID(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothDescriptorUUID(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothDescriptorUUID(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothServiceUUID(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothServiceUUID(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothServiceUUID(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetCharacteristicName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetCharacteristicName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetCharacteristicName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetDescriptorName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetDescriptorName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetDescriptorName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BluetoothUUIDGetServiceName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetServiceName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BluetoothUUIDGetServiceName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：ReadableStream、XMLHttpRequestBodyInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union BodyInit(ReadableStream, XMLHttpRequestBodyInit)
{

    /// <summary>读取 ReadableStream 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStream? AsReadableStream => Value is ReadableStream value ? value : default(ReadableStream?);

    /// <summary>读取 XMLHttpRequestBodyInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    /// <summary>将 ReadableStream 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BodyInit(ReadableStream value)
        => new(value);

    /// <summary>将 XMLHttpRequestBodyInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator BodyInit(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、CSSPseudoElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CSSPseudoElementParent(Element, CSSPseudoElement)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSPseudoElementParent(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CSSPseudoElementParent(CSSPseudoElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLOrSVGImageElement、HTMLVideoElement、HTMLCanvasElement、ImageBitmap、OffscreenCanvas、VideoFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasImageSource(HTMLOrSVGImageElement, HTMLVideoElement, HTMLCanvasElement, ImageBitmap, OffscreenCanvas, VideoFrame)
{

    /// <summary>读取 HTMLOrSVGImageElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOrSVGImageElement? AsHTMLOrSVGImageElement => Value is HTMLOrSVGImageElement value ? value : default(HTMLOrSVGImageElement?);

    /// <summary>读取 HTMLVideoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 ImageBitmap 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>读取 VideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    /// <summary>将 HTMLOrSVGImageElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(HTMLOrSVGImageElement value)
        => new(value);

    /// <summary>将 HTMLVideoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(HTMLVideoElement value)
        => new(value);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 ImageBitmap 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(ImageBitmap value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(OffscreenCanvas value)
        => new(value);

    /// <summary>将 VideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasImageSource(VideoFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CanvasGradient、CanvasPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DFillStyle(string, CanvasGradient, CanvasPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CanvasGradient 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    /// <summary>读取 CanvasPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DFillStyle(string value)
        => new(value);

    /// <summary>将 CanvasGradient 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    /// <summary>将 CanvasPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、DOMPointInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DRoundRectRadii(double, DOMPointInit)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CanvasGradient、CanvasPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CanvasRenderingContext2DStrokeStyle(string, CanvasGradient, CanvasPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CanvasGradient 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    /// <summary>读取 CanvasPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    /// <summary>将 CanvasGradient 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    /// <summary>将 CanvasPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataAfterNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataAfterNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataAfterNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataBeforeNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataBeforeNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataBeforeNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CharacterDataReplaceWithNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataReplaceWithNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CharacterDataReplaceWithNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、Blob。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ClipboardItemDataValue(string, Blob)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ClipboardItemDataValue(string value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ClipboardItemDataValue(Blob value)
        => new(value);
}

/// <summary>WebIDL 联合值：CollectedClientAdditionalPaymentData、CollectedClientAdditionalPaymentRegistrationData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentData, CollectedClientAdditionalPaymentRegistrationData)
{

    /// <summary>读取 CollectedClientAdditionalPaymentData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CollectedClientAdditionalPaymentData? AsCollectedClientAdditionalPaymentData => Value is CollectedClientAdditionalPaymentData value ? value : default(CollectedClientAdditionalPaymentData?);

    /// <summary>读取 CollectedClientAdditionalPaymentRegistrationData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CollectedClientAdditionalPaymentRegistrationData? AsCollectedClientAdditionalPaymentRegistrationData => Value is CollectedClientAdditionalPaymentRegistrationData value ? value : default(CollectedClientAdditionalPaymentRegistrationData?);

    /// <summary>将 CollectedClientAdditionalPaymentData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentData value)
        => new(value);

    /// <summary>将 CollectedClientAdditionalPaymentRegistrationData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CollectedClientPaymentDataPayment(CollectedClientAdditionalPaymentRegistrationData value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ConstrainBooleanParameters。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBoolean(bool, ConstrainBooleanParameters)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ConstrainBooleanParameters 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainBooleanParameters? AsConstrainBooleanParameters => Value is ConstrainBooleanParameters value ? value : default(ConstrainBooleanParameters?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBoolean(bool value)
        => new(value);

    /// <summary>将 ConstrainBooleanParameters 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBoolean(ConstrainBooleanParameters value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、string、ConstrainBooleanOrDOMStringParameters。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMString(bool, string, ConstrainBooleanOrDOMStringParameters)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ConstrainBooleanOrDOMStringParameters 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainBooleanOrDOMStringParameters? AsConstrainBooleanOrDOMStringParameters => Value is ConstrainBooleanOrDOMStringParameters value ? value : default(ConstrainBooleanOrDOMStringParameters?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMString(bool value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMString(string value)
        => new(value);

    /// <summary>将 ConstrainBooleanOrDOMStringParameters 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMString(ConstrainBooleanOrDOMStringParameters value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMStringParametersExact(bool, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMStringParametersExact(bool value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMStringParametersExact(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainBooleanOrDOMStringParametersIdeal(bool, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMStringParametersIdeal(bool value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainBooleanOrDOMStringParametersIdeal(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、ConstrainDoubleRange。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainDouble(double, ConstrainDoubleRange)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 ConstrainDoubleRange 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainDoubleRange? AsConstrainDoubleRange => Value is ConstrainDoubleRange value ? value : default(ConstrainDoubleRange?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDouble(double value)
        => new(value);

    /// <summary>将 ConstrainDoubleRange 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainDouble(ConstrainDoubleRange value)
        => new(value);
}

/// <summary>WebIDL 联合值：uint、ConstrainULongRange。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ConstrainULong(uint, ConstrainULongRange)
{

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>读取 ConstrainULongRange 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainULongRange? AsConstrainULongRange => Value is ConstrainULongRange value ? value : default(ConstrainULongRange?);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainULong(uint value)
        => new(value);

    /// <summary>将 ConstrainULongRange 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ConstrainULong(ConstrainULongRange value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateContextualFragmentString(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateContextualFragmentString(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateContextualFragmentString(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ElementCreationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateElementNSOptions(string, ElementCreationOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ElementCreationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateElementNSOptions(string value)
        => new(value);

    /// <summary>将 ElementCreationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ElementCreationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateElementOptions(string, ElementCreationOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ElementCreationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateElementOptions(string value)
        => new(value);

    /// <summary>将 ElementCreationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateElementOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Blob、MediaSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CreateObjectURLObj(Blob, MediaSource)
{

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>读取 MediaSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateObjectURLObj(Blob value)
        => new(value);

    /// <summary>将 MediaSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CreateObjectURLObj(MediaSource value)
        => new(value);
}

/// <summary>WebIDL 联合值：SmallCryptoKeyID、BigInt。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CryptoKeyID(SmallCryptoKeyID, BigInt)
{

    /// <summary>读取 SmallCryptoKeyID 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SmallCryptoKeyID? AsSmallCryptoKeyID => Value is SmallCryptoKeyID value ? value : default(SmallCryptoKeyID?);

    /// <summary>读取 BigInt 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public BigInt? AsBigInt => Value is BigInt value ? value : default(BigInt?);

    /// <summary>将 SmallCryptoKeyID 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CryptoKeyID(SmallCryptoKeyID value)
        => new(value);

    /// <summary>将 BigInt 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CryptoKeyID(BigInt value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaList、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CustomMediaQuery(MediaList, bool)
{

    /// <summary>读取 MediaList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaList? AsMediaList => Value is MediaList value ? value : default(MediaList?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 MediaList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CustomMediaQuery(MediaList value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator CustomMediaQuery(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DOMParserParseFromString(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMParserParseFromString(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DOMParserParseFromString(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：DigitalCredentialPresentationProtocol、DigitalCredentialIssuanceProtocol。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DigitalCredentialProtocol(DigitalCredentialPresentationProtocol, DigitalCredentialIssuanceProtocol)
{

    /// <summary>读取 DigitalCredentialPresentationProtocol 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DigitalCredentialPresentationProtocol? AsDigitalCredentialPresentationProtocol => Value is DigitalCredentialPresentationProtocol value ? value : default(DigitalCredentialPresentationProtocol?);

    /// <summary>读取 DigitalCredentialIssuanceProtocol 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DigitalCredentialIssuanceProtocol? AsDigitalCredentialIssuanceProtocol => Value is DigitalCredentialIssuanceProtocol value ? value : default(DigitalCredentialIssuanceProtocol?);

    /// <summary>将 DigitalCredentialPresentationProtocol 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DigitalCredentialProtocol(DigitalCredentialPresentationProtocol value)
        => new(value);

    /// <summary>将 DigitalCredentialIssuanceProtocol 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DigitalCredentialProtocol(DigitalCredentialIssuanceProtocol value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、MediaTrackConstraints。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DisplayMediaStreamOptionsAudio(bool, MediaTrackConstraints)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 MediaTrackConstraints 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DisplayMediaStreamOptionsAudio(bool value)
        => new(value);

    /// <summary>将 MediaTrackConstraints 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DisplayMediaStreamOptionsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、MediaTrackConstraints。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DisplayMediaStreamOptionsVideo(bool, MediaTrackConstraints)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 MediaTrackConstraints 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DisplayMediaStreamOptionsVideo(bool value)
        => new(value);

    /// <summary>将 MediaTrackConstraints 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DisplayMediaStreamOptionsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentAppendNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentAppendNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentAppendNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ElementCreationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentCreateElementNSOptions(string, ElementCreationOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ElementCreationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentCreateElementNSOptions(string value)
        => new(value);

    /// <summary>将 ElementCreationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentCreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ElementCreationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentCreateElementOptions(string, ElementCreationOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ElementCreationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentCreateElementOptions(string value)
        => new(value);

    /// <summary>将 ElementCreationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentCreateElementOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentAppendNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentAppendNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentAppendNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentPrependNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentPrependNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentPrependNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentFragmentReplaceChildrenNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentReplaceChildrenNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentFragmentReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ImportNodeOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentImportNodeOptions(bool, ImportNodeOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ImportNodeOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentImportNodeOptions(bool value)
        => new(value);

    /// <summary>将 ImportNodeOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentImportNodeOptions(ImportNodeOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentParseHTMLUnsafeHtml(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentParseHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentParseHTMLUnsafeHtml(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentPrependNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentPrependNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentPrependNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentReplaceChildrenNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentReplaceChildrenNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：ViewTransitionUpdateCallback、StartViewTransitionOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    /// <summary>读取 ViewTransitionUpdateCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    /// <summary>读取 StartViewTransitionOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    /// <summary>将 ViewTransitionUpdateCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    /// <summary>将 StartViewTransitionOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeAfterNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeAfterNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeAfterNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeBeforeNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeBeforeNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeBeforeNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentTypeReplaceWithNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeReplaceWithNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentTypeReplaceWithNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentWriteText(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentWriteText(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentWriteText(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentWritelnText(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentWritelnText(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator DocumentWritelnText(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、CSSNumericValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EffectTimingDuration(double, CSSNumericValue, string)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EffectTimingDuration(double value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EffectTimingDuration(CSSNumericValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EffectTimingDuration(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAfterNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAfterNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAfterNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、KeyframeAnimationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAnimateOptions(double, KeyframeAnimationOptions)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 KeyframeAnimationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAnimateOptions(double value)
        => new(value);

    /// <summary>将 KeyframeAnimationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementAppendNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAppendNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementAppendNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementBeforeNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementBeforeNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementBeforeNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInnerHTML(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInnerHTML(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInnerHTML(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInsertAdjacentHTMLString(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInsertAdjacentHTMLString(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInsertAdjacentHTMLString(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：FileRef、string、FormData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValue(FileRef, string, FormData)
{

    /// <summary>读取 FileRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileRef? AsFile => Value is FileRef value ? value : default(FileRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 FormData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    /// <summary>将 FileRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValue(FileRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValue(string value)
        => new(value);

    /// <summary>将 FormData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValue(FormData value)
        => new(value);
}

/// <summary>WebIDL 联合值：FileRef、string、FormData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValueState(FileRef, string, FormData)
{

    /// <summary>读取 FileRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileRef? AsFile => Value is FileRef value ? value : default(FileRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 FormData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    /// <summary>将 FileRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValueState(FileRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValueState(string value)
        => new(value);

    /// <summary>将 FormData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementInternalsSetFormValueState(FormData value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementOuterHTML(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementOuterHTML(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementOuterHTML(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementPrependNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementPrependNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementPrependNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementReplaceChildrenNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementReplaceChildrenNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementReplaceWithNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementReplaceWithNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementReplaceWithNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ScrollIntoViewOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementScrollIntoViewArg(bool, ScrollIntoViewOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ScrollIntoViewOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementScrollIntoViewArg(bool value)
        => new(value);

    /// <summary>将 ScrollIntoViewOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedType、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetAttributeNSValue(TrustedType, string)
{

    /// <summary>读取 TrustedType 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedType 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetAttributeNSValue(TrustedType value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetAttributeNSValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedType、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetAttributeValue(TrustedType, string)
{

    /// <summary>读取 TrustedType 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedType 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetAttributeValue(TrustedType value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetAttributeValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementSetHTMLUnsafeHtml(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementSetHTMLUnsafeHtml(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：ViewTransitionUpdateCallback、StartViewTransitionOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    /// <summary>读取 ViewTransitionUpdateCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    /// <summary>读取 StartViewTransitionOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    /// <summary>将 ViewTransitionUpdateCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementStartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    /// <summary>将 StartViewTransitionOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ElementStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：EventListenerLiteral、HandleEventCallback。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventListenerValue(EventListenerLiteral, HandleEventCallback)
{

    /// <summary>读取 EventListenerLiteral 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EventListenerLiteral? AsEventListenerLiteral => Value is EventListenerLiteral value ? value : default(EventListenerLiteral?);

    /// <summary>读取 HandleEventCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HandleEventCallback? AsHandleEventCallback => Value is HandleEventCallback value ? value : default(HandleEventCallback?);

    /// <summary>将 EventListenerLiteral 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventListenerValue(EventListenerLiteral value)
        => new(value);

    /// <summary>将 HandleEventCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventListenerValue(HandleEventCallback value)
        => new(value);
}

/// <summary>WebIDL 联合值：AddEventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventTargetAddEventListenerOptions(AddEventListenerOptions, bool)
{

    /// <summary>读取 AddEventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 AddEventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventTargetAddEventListenerOptions(AddEventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventTargetAddEventListenerOptions(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：EventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union EventTargetRemoveEventListenerOptions(EventListenerOptions, bool)
{

    /// <summary>读取 EventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 EventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventTargetRemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator EventTargetRemoveEventListenerOptions(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：Client、ServiceWorker、MessagePort。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ExtendableMessageEventInitSource(Client, ServiceWorker, MessagePort)
{

    /// <summary>读取 Client 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Client? AsClient => Value is Client value ? value : default(Client?);

    /// <summary>读取 ServiceWorker 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    /// <summary>读取 MessagePort 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    /// <summary>将 Client 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventInitSource(Client value)
        => new(value);

    /// <summary>将 ServiceWorker 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventInitSource(ServiceWorker value)
        => new(value);

    /// <summary>将 MessagePort 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventInitSource(MessagePort value)
        => new(value);
}

/// <summary>WebIDL 联合值：Client、ServiceWorker、MessagePort。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ExtendableMessageEventSource(Client, ServiceWorker, MessagePort)
{

    /// <summary>读取 Client 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Client? AsClient => Value is Client value ? value : default(Client?);

    /// <summary>读取 ServiceWorker 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    /// <summary>读取 MessagePort 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    /// <summary>将 Client 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventSource(Client value)
        => new(value);

    /// <summary>将 ServiceWorker 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventSource(ServiceWorker value)
        => new(value);

    /// <summary>将 MessagePort 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ExtendableMessageEventSource(MessagePort value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ArrayBuffer。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FileReaderResult(string, ArrayBuffer)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileReaderResult(string value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FileReaderResult(ArrayBuffer value)
        => new(value);
}

/// <summary>WebIDL 联合值：FileRef、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FormDataEntryValue(FileRef, string)
{

    /// <summary>读取 FileRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileRef? AsFile => Value is FileRef value ? value : default(FileRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 FileRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FormDataEntryValue(FileRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator FormDataEntryValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Text、Element、CSSPseudoElement、DocumentRef。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GeometryNode(Text, Element, CSSPseudoElement, DocumentRef)
{

    /// <summary>读取 Text 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Text? AsText => Value is Text value ? value : default(Text?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>将 Text 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GeometryNode(Text value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GeometryNode(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GeometryNode(CSSPseudoElement value)
        => new(value);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GeometryNode(DocumentRef value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetCharacteristicName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetCharacteristicName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetCharacteristicName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetDescriptorName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetDescriptorName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetDescriptorName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GetServiceName(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetServiceName(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GetServiceName(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、EffectTiming。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GroupEffectTiming(double, EffectTiming)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 EffectTiming 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GroupEffectTiming(double value)
        => new(value);

    /// <summary>将 EffectTiming 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GroupEffectTiming(EffectTiming value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCollection、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLAllCollectionItemResult(HTMLCollection, Element)
{

    /// <summary>读取 HTMLCollection 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 HTMLCollection 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLAllCollectionItemResult(HTMLCollection value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLAllCollectionItemResult(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCollection、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLAllCollectionNamedItemResult(HTMLCollection, Element)
{

    /// <summary>读取 HTMLCollection 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 HTMLCollection 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLAllCollectionNamedItemResult(HTMLCollection value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLAllCollectionNamedItemResult(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、double、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLElementHidden(bool, double, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLElementHidden(bool value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLElementHidden(double value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLElementHidden(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TogglePopoverOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLElementTogglePopoverOptions(TogglePopoverOptions, bool)
{

    /// <summary>读取 TogglePopoverOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 TogglePopoverOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLElementTogglePopoverOptions(TogglePopoverOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLElementTogglePopoverOptions(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：RadioNodeList、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLFormControlsCollectionNamedItemResult(RadioNodeList, Element)
{

    /// <summary>读取 RadioNodeList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 RadioNodeList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLFormControlsCollectionNamedItemResult(RadioNodeList value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLFormControlsCollectionNamedItemResult(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：RadioNodeList、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLFormElementResult(RadioNodeList, Element)
{

    /// <summary>读取 RadioNodeList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 RadioNodeList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLFormElementResult(RadioNodeList value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLFormElementResult(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLIFrameElementSrcdoc(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLIFrameElementSrcdoc(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLIFrameElementSrcdoc(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLElement、int。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOptionsCollectionAddBefore(HTMLElement, int)
{

    /// <summary>读取 HTMLElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    /// <summary>读取 int 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public int? AsInt => Value is int value ? value : default(int?);

    /// <summary>将 HTMLElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOptionsCollectionAddBefore(HTMLElement value)
        => new(value);

    /// <summary>将 int 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOptionsCollectionAddBefore(int value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLOptionElement、HTMLOptGroupElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOptionsCollectionAddElement(HTMLOptionElement, HTMLOptGroupElement)
{

    /// <summary>读取 HTMLOptionElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    /// <summary>读取 HTMLOptGroupElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    /// <summary>将 HTMLOptionElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptionElement value)
        => new(value);

    /// <summary>将 HTMLOptGroupElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLImageElement、SVGImageElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOrSVGImageElement(HTMLImageElement, SVGImageElement)
{

    /// <summary>读取 HTMLImageElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    /// <summary>读取 SVGImageElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SVGImageElement? AsSVGImageElement => Value is SVGImageElement value ? value : default(SVGImageElement?);

    /// <summary>将 HTMLImageElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOrSVGImageElement(HTMLImageElement value)
        => new(value);

    /// <summary>将 SVGImageElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOrSVGImageElement(SVGImageElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLScriptElement、SVGScriptElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLOrSVGScriptElement(HTMLScriptElement, SVGScriptElement)
{

    /// <summary>读取 HTMLScriptElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLScriptElement? AsHTMLScriptElement => Value is HTMLScriptElement value ? value : default(HTMLScriptElement?);

    /// <summary>读取 SVGScriptElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SVGScriptElement? AsSVGScriptElement => Value is SVGScriptElement value ? value : default(SVGScriptElement?);

    /// <summary>将 HTMLScriptElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOrSVGScriptElement(HTMLScriptElement value)
        => new(value);

    /// <summary>将 SVGScriptElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLOrSVGScriptElement(SVGScriptElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLElement、int。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSelectElementAddBefore(HTMLElement, int)
{

    /// <summary>读取 HTMLElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    /// <summary>读取 int 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public int? AsInt => Value is int value ? value : default(int?);

    /// <summary>将 HTMLElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSelectElementAddBefore(HTMLElement value)
        => new(value);

    /// <summary>将 int 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSelectElementAddBefore(int value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLOptionElement、HTMLOptGroupElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSelectElementAddElement(HTMLOptionElement, HTMLOptGroupElement)
{

    /// <summary>读取 HTMLOptionElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    /// <summary>读取 HTMLOptGroupElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    /// <summary>将 HTMLOptionElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSelectElementAddElement(HTMLOptionElement value)
        => new(value);

    /// <summary>将 HTMLOptGroupElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSelectElementAddElement(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、Text。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union HTMLSlotElementAssignNodes(Element, Text)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 Text 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Text? AsText => Value is Text value ? value : default(Text?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSlotElementAssignNodes(Element value)
        => new(value);

    /// <summary>将 Text 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator HTMLSlotElementAssignNodes(Text value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageBitmapRenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBitmapRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBitmapRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：CanvasImageSource、Blob、ImageData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageBitmapSource(CanvasImageSource, Blob, ImageData)
{

    /// <summary>读取 CanvasImageSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasImageSource? AsCanvasImageSource => Value is CanvasImageSource value ? value : default(CanvasImageSource?);

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>读取 ImageData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    /// <summary>将 CanvasImageSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBitmapSource(CanvasImageSource value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBitmapSource(Blob value)
        => new(value);

    /// <summary>将 ImageData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageBitmapSource(ImageData value)
        => new(value);
}

/// <summary>WebIDL 联合值：Uint8ClampedArray、Float16Array。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImageDataArray(Uint8ClampedArray, Float16Array)
{

    /// <summary>读取 Uint8ClampedArray 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Uint8ClampedArray? AsUint8ClampedArray => Value is Uint8ClampedArray value ? value : default(Uint8ClampedArray?);

    /// <summary>读取 Float16Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float16Array? AsFloat16Array => Value is Float16Array value ? value : default(Float16Array?);

    /// <summary>将 Uint8ClampedArray 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageDataArray(Uint8ClampedArray value)
        => new(value);

    /// <summary>将 Float16Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImageDataArray(Float16Array value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ImportNodeOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImportNodeOptionsValue(bool, ImportNodeOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ImportNodeOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImportNodeOptionsValue(bool value)
        => new(value);

    /// <summary>将 ImportNodeOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImportNodeOptionsValue(ImportNodeOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ImportScriptsUrls(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImportScriptsUrls(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ImportScriptsUrls(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union InsertAdjacentHTMLString(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator InsertAdjacentHTMLString(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator InsertAdjacentHTMLString(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、DocumentRef。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverInitRoot(Element, DocumentRef)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverInitRoot(Element value)
        => new(value);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverInitRoot(DocumentRef value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、DocumentRef。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverRoot(Element, DocumentRef)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverRoot(Element value)
        => new(value);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator IntersectionObserverRoot(DocumentRef value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeEnd(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TimelineRangeOffset、CSSNumericValue、CSSKeywordValue、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeAnimationOptionsRangeStart(TimelineRangeOffset, CSSNumericValue, CSSKeywordValue, string)
{

    /// <summary>读取 TimelineRangeOffset 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TimelineRangeOffset? AsTimelineRangeOffset => Value is TimelineRangeOffset value ? value : default(TimelineRangeOffset?);

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TimelineRangeOffset 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeStart(TimelineRangeOffset value)
        => new(value);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeStart(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeStart(CSSKeywordValue value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeAnimationOptionsRangeStart(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、KeyframeEffectOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union KeyframeEffectOptionsValue(double, KeyframeEffectOptions)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 KeyframeEffectOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public KeyframeEffectOptions? AsKeyframeEffectOptions => Value is KeyframeEffectOptions value ? value : default(KeyframeEffectOptions?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeEffectOptionsValue(double value)
        => new(value);

    /// <summary>将 KeyframeEffectOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator KeyframeEffectOptionsValue(KeyframeEffectOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、AutoKeyword。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union LineAndPositionSetting(double, AutoKeyword)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 AutoKeyword 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AutoKeyword? AsAutoKeyword => Value is AutoKeyword value ? value : default(AutoKeyword?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LineAndPositionSetting(double value)
        => new(value);

    /// <summary>将 AutoKeyword 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator LineAndPositionSetting(AutoKeyword value)
        => new(value);
}

/// <summary>WebIDL 联合值：BigInt、double。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MLNumber(BigInt, double)
{

    /// <summary>读取 BigInt 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public BigInt? AsBigInt => Value is BigInt value ? value : default(BigInt?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>将 BigInt 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MLNumber(BigInt value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MLNumber(double value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStream、MediaSource、Blob。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaProvider(MediaStream, MediaSource, Blob)
{

    /// <summary>读取 MediaStream 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStream? AsMediaStream => Value is MediaStream value ? value : default(MediaStream?);

    /// <summary>读取 MediaSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>将 MediaStream 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaProvider(MediaStream value)
        => new(value);

    /// <summary>将 MediaSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaProvider(MediaSource value)
        => new(value);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaProvider(Blob value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、MediaTrackConstraints。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamConstraintsAudio(bool, MediaTrackConstraints)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 MediaTrackConstraints 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamConstraintsAudio(bool value)
        => new(value);

    /// <summary>将 MediaTrackConstraints 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamConstraintsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、MediaTrackConstraints。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamConstraintsVideo(bool, MediaTrackConstraints)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 MediaTrackConstraints 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamConstraintsVideo(bool value)
        => new(value);

    /// <summary>将 MediaTrackConstraints 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamConstraintsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStreamTrack、MediaStreamTrackHandle。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamTrackOrHandle(MediaStreamTrack, MediaStreamTrackHandle)
{

    /// <summary>读取 MediaStreamTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    /// <summary>读取 MediaStreamTrackHandle 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrackHandle? AsMediaStreamTrackHandle => Value is MediaStreamTrackHandle value ? value : default(MediaStreamTrackHandle?);

    /// <summary>将 MediaStreamTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamTrackOrHandle(MediaStreamTrack value)
        => new(value);

    /// <summary>将 MediaStreamTrackHandle 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamTrackOrHandle(MediaStreamTrackHandle value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStreamTrackAudioStats、MediaStreamTrackVideoStats。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaStreamTrackStats(MediaStreamTrackAudioStats, MediaStreamTrackVideoStats)
{

    /// <summary>读取 MediaStreamTrackAudioStats 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrackAudioStats? AsMediaStreamTrackAudioStats => Value is MediaStreamTrackAudioStats value ? value : default(MediaStreamTrackAudioStats?);

    /// <summary>读取 MediaStreamTrackVideoStats 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrackVideoStats? AsMediaStreamTrackVideoStats => Value is MediaStreamTrackVideoStats value ? value : default(MediaStreamTrackVideoStats?);

    /// <summary>将 MediaStreamTrackAudioStats 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamTrackStats(MediaStreamTrackAudioStats value)
        => new(value);

    /// <summary>将 MediaStreamTrackVideoStats 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaStreamTrackStats(MediaStreamTrackVideoStats value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackCapabilitiesEchoCancellation(bool, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackCapabilitiesEchoCancellation(bool value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackCapabilitiesEchoCancellation(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ConstrainDouble。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetPan(bool, ConstrainDouble)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ConstrainDouble 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetPan(bool value)
        => new(value);

    /// <summary>将 ConstrainDouble 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetPan(ConstrainDouble value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ConstrainDouble。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetTilt(bool, ConstrainDouble)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ConstrainDouble 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetTilt(bool value)
        => new(value);

    /// <summary>将 ConstrainDouble 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetTilt(ConstrainDouble value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ConstrainDouble。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackConstraintSetZoom(bool, ConstrainDouble)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ConstrainDouble 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ConstrainDouble? AsConstrainDouble => Value is ConstrainDouble value ? value : default(ConstrainDouble?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetZoom(bool value)
        => new(value);

    /// <summary>将 ConstrainDouble 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackConstraintSetZoom(ConstrainDouble value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MediaTrackSettingsEchoCancellation(bool, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackSettingsEchoCancellation(bool value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MediaTrackSettingsEchoCancellation(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：WindowProxy、MessagePort、ServiceWorker。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union MessageEventSource(WindowProxy, MessagePort, ServiceWorker)
{

    /// <summary>读取 WindowProxy 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WindowProxy? AsWindowProxy => Value is WindowProxy value ? value : default(WindowProxy?);

    /// <summary>读取 MessagePort 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    /// <summary>读取 ServiceWorker 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    /// <summary>将 WindowProxy 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MessageEventSource(WindowProxy value)
        => new(value);

    /// <summary>将 MessagePort 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MessageEventSource(MessagePort value)
        => new(value);

    /// <summary>将 ServiceWorker 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator MessageEventSource(ServiceWorker value)
        => new(value);
}

/// <summary>WebIDL 联合值：NodeFilterLiteral、AcceptNodeCallback。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union NodeFilterValue(NodeFilterLiteral, AcceptNodeCallback)
{

    /// <summary>读取 NodeFilterLiteral 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public NodeFilterLiteral? AsNodeFilterLiteral => Value is NodeFilterLiteral value ? value : default(NodeFilterLiteral?);

    /// <summary>读取 AcceptNodeCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AcceptNodeCallback? AsAcceptNodeCallback => Value is AcceptNodeCallback value ? value : default(AcceptNodeCallback?);

    /// <summary>将 NodeFilterLiteral 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NodeFilterValue(NodeFilterLiteral value)
        => new(value);

    /// <summary>将 AcceptNodeCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator NodeFilterValue(AcceptNodeCallback value)
        => new(value);
}

/// <summary>WebIDL 联合值：ObservableSubscriptionCallback、ObservableInspector。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ObservableInspectorUnion(ObservableSubscriptionCallback, ObservableInspector)
{

    /// <summary>读取 ObservableSubscriptionCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ObservableSubscriptionCallback? AsObservableSubscriptionCallback => Value is ObservableSubscriptionCallback value ? value : default(ObservableSubscriptionCallback?);

    /// <summary>读取 ObservableInspector 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ObservableInspector? AsObservableInspector => Value is ObservableInspector value ? value : default(ObservableInspector?);

    /// <summary>将 ObservableSubscriptionCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ObservableInspectorUnion(ObservableSubscriptionCallback value)
        => new(value);

    /// <summary>将 ObservableInspector 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ObservableInspectorUnion(ObservableInspector value)
        => new(value);
}

/// <summary>WebIDL 联合值：ObservableSubscriptionCallback、SubscriptionObserver。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ObserverUnion(ObservableSubscriptionCallback, SubscriptionObserver)
{

    /// <summary>读取 ObservableSubscriptionCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ObservableSubscriptionCallback? AsObservableSubscriptionCallback => Value is ObservableSubscriptionCallback value ? value : default(ObservableSubscriptionCallback?);

    /// <summary>读取 SubscriptionObserver 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SubscriptionObserver? AsSubscriptionObserver => Value is SubscriptionObserver value ? value : default(SubscriptionObserver?);

    /// <summary>将 ObservableSubscriptionCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ObserverUnion(ObservableSubscriptionCallback value)
        => new(value);

    /// <summary>将 SubscriptionObserver 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ObserverUnion(SubscriptionObserver value)
        => new(value);
}

/// <summary>WebIDL 联合值：AudioContextRenderSizeCategory、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory, uint)
{

    /// <summary>读取 AudioContextRenderSizeCategory 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => Value is AudioContextRenderSizeCategory value ? value : default(AudioContextRenderSizeCategory?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 AudioContextRenderSizeCategory 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OfflineAudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CanvasGradient、CanvasPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DFillStyle(string, CanvasGradient, CanvasPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CanvasGradient 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    /// <summary>读取 CanvasPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(string value)
        => new(value);

    /// <summary>将 CanvasGradient 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    /// <summary>将 CanvasPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、DOMPointInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DRoundRectRadii(double, DOMPointInit)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CanvasGradient、CanvasPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenCanvasRenderingContext2DStrokeStyle(string, CanvasGradient, CanvasPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CanvasGradient 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    /// <summary>读取 CanvasPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    /// <summary>将 CanvasGradient 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    /// <summary>将 CanvasPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：OffscreenCanvasRenderingContext2D、ImageBitmapRenderingContext、WebGLRenderingContext、WebGL2RenderingContext、GPUCanvasContext。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OffscreenRenderingContext(OffscreenCanvasRenderingContext2D, ImageBitmapRenderingContext, WebGLRenderingContext, WebGL2RenderingContext, GPUCanvasContext)
{

    /// <summary>读取 OffscreenCanvasRenderingContext2D 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvasRenderingContext2D? AsOffscreenCanvasRenderingContext2D => Value is OffscreenCanvasRenderingContext2D value ? value : default(OffscreenCanvasRenderingContext2D?);

    /// <summary>读取 ImageBitmapRenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => Value is ImageBitmapRenderingContext value ? value : default(ImageBitmapRenderingContext?);

    /// <summary>读取 WebGLRenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    /// <summary>读取 WebGL2RenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    /// <summary>读取 GPUCanvasContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUCanvasContext? AsGPUCanvasContext => Value is GPUCanvasContext value ? value : default(GPUCanvasContext?);

    /// <summary>将 OffscreenCanvasRenderingContext2D 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenRenderingContext(OffscreenCanvasRenderingContext2D value)
        => new(value);

    /// <summary>将 ImageBitmapRenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenRenderingContext(ImageBitmapRenderingContext value)
        => new(value);

    /// <summary>将 WebGLRenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenRenderingContext(WebGLRenderingContext value)
        => new(value);

    /// <summary>将 WebGL2RenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenRenderingContext(WebGL2RenderingContext value)
        => new(value);

    /// <summary>将 GPUCanvasContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OffscreenRenderingContext(GPUCanvasContext value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union OptionalEffectTimingDuration(double, string)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OptionalEffectTimingDuration(double value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator OptionalEffectTimingDuration(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、CSSPseudoElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParameterCurrentTarget(Element, CSSPseudoElement)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParameterCurrentTarget(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParameterCurrentTarget(CSSPseudoElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：EventRef、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParameterEvent(EventRef, string)
{

    /// <summary>读取 EventRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EventRef? AsEventRef => Value is EventRef value ? value : default(EventRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 EventRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParameterEvent(EventRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParameterEvent(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Sanitizer、SanitizerConfig、SanitizerPresets。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParseHTMLUnsafeOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    /// <summary>读取 Sanitizer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    /// <summary>读取 SanitizerConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    /// <summary>读取 SanitizerPresets 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    /// <summary>将 Sanitizer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParseHTMLUnsafeOptionsSanitizer(Sanitizer value)
        => new(value);

    /// <summary>将 SanitizerConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParseHTMLUnsafeOptionsSanitizer(SanitizerConfig value)
        => new(value);

    /// <summary>将 SanitizerPresets 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ParseHTMLUnsafeOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

/// <summary>WebIDL 联合值：PasswordCredentialData、HTMLFormElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PasswordCredentialInit(PasswordCredentialData, HTMLFormElement)
{

    /// <summary>读取 PasswordCredentialData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public PasswordCredentialData? AsPasswordCredentialData => Value is PasswordCredentialData value ? value : default(PasswordCredentialData?);

    /// <summary>读取 HTMLFormElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLFormElement? AsHTMLFormElement => Value is HTMLFormElement value ? value : default(HTMLFormElement?);

    /// <summary>将 PasswordCredentialData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PasswordCredentialInit(PasswordCredentialData value)
        => new(value);

    /// <summary>将 HTMLFormElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PasswordCredentialInit(HTMLFormElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：Path2D、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union Path2DPath(Path2D, string)
{

    /// <summary>读取 Path2D 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Path2D? AsPath2D => Value is Path2D value ? value : default(Path2D?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Path2D 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DPath(Path2D value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DPath(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、DOMPointInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union Path2DRoundRectRadii(double, DOMPointInit)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DRoundRectRadii(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator Path2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、double。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureOptionsEnd(string, double)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureOptionsEnd(string value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureOptionsEnd(double value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、double。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureOptionsStart(string, double)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureOptionsStart(string value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureOptionsStart(double value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、PerformanceMeasureOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PerformanceMeasureStartOrMeasureOptions(string, PerformanceMeasureOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 PerformanceMeasureOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public PerformanceMeasureOptions? AsPerformanceMeasureOptions => Value is PerformanceMeasureOptions value ? value : default(PerformanceMeasureOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureStartOrMeasureOptions(string value)
        => new(value);

    /// <summary>将 PerformanceMeasureOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PerformanceMeasureStartOrMeasureOptions(PerformanceMeasureOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union PrependNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PrependNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator PrependNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStreamTrack、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack, string)
{

    /// <summary>读取 MediaStreamTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 MediaStreamTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：RTCRtpSFrameDecryptor、RTCRtpScriptTransform。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCRtpReceiverTransform(RTCRtpSFrameDecryptor, RTCRtpScriptTransform)
{

    /// <summary>读取 RTCRtpSFrameDecryptor 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCRtpSFrameDecryptor? AsRTCRtpSFrameDecryptor => Value is RTCRtpSFrameDecryptor value ? value : default(RTCRtpSFrameDecryptor?);

    /// <summary>读取 RTCRtpScriptTransform 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCRtpScriptTransform? AsRTCRtpScriptTransform => Value is RTCRtpScriptTransform value ? value : default(RTCRtpScriptTransform?);

    /// <summary>将 RTCRtpSFrameDecryptor 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCRtpReceiverTransform(RTCRtpSFrameDecryptor value)
        => new(value);

    /// <summary>将 RTCRtpScriptTransform 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCRtpReceiverTransform(RTCRtpScriptTransform value)
        => new(value);
}

/// <summary>WebIDL 联合值：RTCRtpSFrameEncryptor、RTCRtpScriptTransform。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCRtpSenderTransform(RTCRtpSFrameEncryptor, RTCRtpScriptTransform)
{

    /// <summary>读取 RTCRtpSFrameEncryptor 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCRtpSFrameEncryptor? AsRTCRtpSFrameEncryptor => Value is RTCRtpSFrameEncryptor value ? value : default(RTCRtpSFrameEncryptor?);

    /// <summary>读取 RTCRtpScriptTransform 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCRtpScriptTransform? AsRTCRtpScriptTransform => Value is RTCRtpScriptTransform value ? value : default(RTCRtpScriptTransform?);

    /// <summary>将 RTCRtpSFrameEncryptor 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCRtpSenderTransform(RTCRtpSFrameEncryptor value)
        => new(value);

    /// <summary>将 RTCRtpScriptTransform 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RTCRtpSenderTransform(RTCRtpScriptTransform value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RangeCreateContextualFragmentString(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RangeCreateContextualFragmentString(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RangeCreateContextualFragmentString(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：ReadableStreamDefaultController、ReadableByteStreamController。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReadableStreamController(ReadableStreamDefaultController, ReadableByteStreamController)
{

    /// <summary>读取 ReadableStreamDefaultController 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStreamDefaultController? AsReadableStreamDefaultController => Value is ReadableStreamDefaultController value ? value : default(ReadableStreamDefaultController?);

    /// <summary>读取 ReadableByteStreamController 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableByteStreamController? AsReadableByteStreamController => Value is ReadableByteStreamController value ? value : default(ReadableByteStreamController?);

    /// <summary>将 ReadableStreamDefaultController 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReadableStreamController(ReadableStreamDefaultController value)
        => new(value);

    /// <summary>将 ReadableByteStreamController 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReadableStreamController(ReadableByteStreamController value)
        => new(value);
}

/// <summary>WebIDL 联合值：ReadableStreamDefaultReader、ReadableStreamBYOBReader。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReadableStreamReader(ReadableStreamDefaultReader, ReadableStreamBYOBReader)
{

    /// <summary>读取 ReadableStreamDefaultReader 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStreamDefaultReader? AsReadableStreamDefaultReader => Value is ReadableStreamDefaultReader value ? value : default(ReadableStreamDefaultReader?);

    /// <summary>读取 ReadableStreamBYOBReader 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ReadableStreamBYOBReader? AsReadableStreamBYOBReader => Value is ReadableStreamBYOBReader value ? value : default(ReadableStreamBYOBReader?);

    /// <summary>将 ReadableStreamDefaultReader 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReadableStreamReader(ReadableStreamDefaultReader value)
        => new(value);

    /// <summary>将 ReadableStreamBYOBReader 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReadableStreamReader(ReadableStreamBYOBReader value)
        => new(value);
}

/// <summary>WebIDL 联合值：EventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RemoveEventListenerOptions(EventListenerOptions, bool)
{

    /// <summary>读取 EventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 EventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RemoveEventListenerOptions(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：CanvasRenderingContext2D、ImageBitmapRenderingContext、WebGLRenderingContext、WebGL2RenderingContext、GPUCanvasContext。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RenderingContext(CanvasRenderingContext2D, ImageBitmapRenderingContext, WebGLRenderingContext, WebGL2RenderingContext, GPUCanvasContext)
{

    /// <summary>读取 CanvasRenderingContext2D 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasRenderingContext2D? AsCanvasRenderingContext2D => Value is CanvasRenderingContext2D value ? value : default(CanvasRenderingContext2D?);

    /// <summary>读取 ImageBitmapRenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => Value is ImageBitmapRenderingContext value ? value : default(ImageBitmapRenderingContext?);

    /// <summary>读取 WebGLRenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    /// <summary>读取 WebGL2RenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    /// <summary>读取 GPUCanvasContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUCanvasContext? AsGPUCanvasContext => Value is GPUCanvasContext value ? value : default(GPUCanvasContext?);

    /// <summary>将 CanvasRenderingContext2D 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RenderingContext(CanvasRenderingContext2D value)
        => new(value);

    /// <summary>将 ImageBitmapRenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RenderingContext(ImageBitmapRenderingContext value)
        => new(value);

    /// <summary>将 WebGLRenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RenderingContext(WebGLRenderingContext value)
        => new(value);

    /// <summary>将 WebGL2RenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RenderingContext(WebGL2RenderingContext value)
        => new(value);

    /// <summary>将 GPUCanvasContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RenderingContext(GPUCanvasContext value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReplaceChildrenNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReplaceChildrenNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReplaceWithNodes(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReplaceWithNodes(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReplaceWithNodes(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：FenceEvent、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ReportEventType(FenceEvent, string)
{

    /// <summary>读取 FenceEvent 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FenceEvent? AsFenceEvent => Value is FenceEvent value ? value : default(FenceEvent?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 FenceEvent 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReportEventType(FenceEvent value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ReportEventType(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Request、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RequestInfo(Request, string)
{

    /// <summary>读取 Request 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Request? AsRequest => Value is Request value ? value : default(Request?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Request 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RequestInfo(Request value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RequestInfo(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Float32Array、Float64Array、DOMMatrix。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RotationMatrixType(Float32Array, Float64Array, DOMMatrix)
{

    /// <summary>读取 Float32Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float32Array? AsFloat32Array => Value is Float32Array value ? value : default(Float32Array?);

    /// <summary>读取 Float64Array 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Float64Array? AsFloat64Array => Value is Float64Array value ? value : default(Float64Array?);

    /// <summary>读取 DOMMatrix 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMMatrix? AsDOMMatrix => Value is DOMMatrix value ? value : default(DOMMatrix?);

    /// <summary>将 Float32Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RotationMatrixType(Float32Array value)
        => new(value);

    /// <summary>将 Float64Array 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RotationMatrixType(Float64Array value)
        => new(value);

    /// <summary>将 DOMMatrix 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RotationMatrixType(DOMMatrix value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、DOMPointInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RoundRectRadii(double, DOMPointInit)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RoundRectRadii(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：RouterSourceDict、RouterSourceEnum。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RouterSource(RouterSourceDict, RouterSourceEnum)
{

    /// <summary>读取 RouterSourceDict 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterSourceDict? AsRouterSourceDict => Value is RouterSourceDict value ? value : default(RouterSourceDict?);

    /// <summary>读取 RouterSourceEnum 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RouterSourceEnum? AsRouterSourceEnum => Value is RouterSourceEnum value ? value : default(RouterSourceEnum?);

    /// <summary>将 RouterSourceDict 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RouterSource(RouterSourceDict value)
        => new(value);

    /// <summary>将 RouterSourceEnum 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator RouterSource(RouterSourceEnum value)
        => new(value);
}

/// <summary>WebIDL 联合值：RTCEncodedVideoFrame、RTCEncodedAudioFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SFrameTransformErrorEventFrame(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    /// <summary>读取 RTCEncodedVideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    /// <summary>读取 RTCEncodedAudioFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    /// <summary>将 RTCEncodedVideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SFrameTransformErrorEventFrame(RTCEncodedVideoFrame value)
        => new(value);

    /// <summary>将 RTCEncodedAudioFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SFrameTransformErrorEventFrame(RTCEncodedAudioFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：RTCEncodedVideoFrame、RTCEncodedAudioFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SFrameTransformErrorEventInitFrame(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    /// <summary>读取 RTCEncodedVideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    /// <summary>读取 RTCEncodedAudioFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    /// <summary>将 RTCEncodedVideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SFrameTransformErrorEventInitFrame(RTCEncodedVideoFrame value)
        => new(value);

    /// <summary>将 RTCEncodedAudioFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SFrameTransformErrorEventInitFrame(RTCEncodedAudioFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、TrustedScriptURL。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SVGAnimatedStringBaseVal(string, TrustedScriptURL)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SVGAnimatedStringBaseVal(string value)
        => new(value);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SVGAnimatedStringBaseVal(TrustedScriptURL value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SanitizerAttributeNamespace。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerAttribute(string, SanitizerAttributeNamespace)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SanitizerAttributeNamespace 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerAttributeNamespace? AsSanitizerAttributeNamespace => Value is SanitizerAttributeNamespace value ? value : default(SanitizerAttributeNamespace?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerAttribute(string value)
        => new(value);

    /// <summary>将 SanitizerAttributeNamespace 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerAttribute(SanitizerAttributeNamespace value)
        => new(value);
}

/// <summary>WebIDL 联合值：SanitizerConfig、SanitizerPresets。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerConfiguration(SanitizerConfig, SanitizerPresets)
{

    /// <summary>读取 SanitizerConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    /// <summary>读取 SanitizerPresets 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    /// <summary>将 SanitizerConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerConfiguration(SanitizerConfig value)
        => new(value);

    /// <summary>将 SanitizerPresets 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerConfiguration(SanitizerPresets value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SanitizerElementNamespace。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerElement(string, SanitizerElementNamespace)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SanitizerElementNamespace 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerElementNamespace? AsSanitizerElementNamespace => Value is SanitizerElementNamespace value ? value : default(SanitizerElementNamespace?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerElement(string value)
        => new(value);

    /// <summary>将 SanitizerElementNamespace 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerElement(SanitizerElementNamespace value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SanitizerElementNamespaceWithAttributes。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerElementWithAttributes(string, SanitizerElementNamespaceWithAttributes)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SanitizerElementNamespaceWithAttributes 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerElementNamespaceWithAttributes? AsSanitizerElementNamespaceWithAttributes => Value is SanitizerElementNamespaceWithAttributes value ? value : default(SanitizerElementNamespaceWithAttributes?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerElementWithAttributes(string value)
        => new(value);

    /// <summary>将 SanitizerElementNamespaceWithAttributes 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerElementWithAttributes(SanitizerElementNamespaceWithAttributes value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SanitizerProcessingInstruction。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SanitizerPI(string, SanitizerProcessingInstruction)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SanitizerProcessingInstruction 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerProcessingInstruction? AsSanitizerProcessingInstruction => Value is SanitizerProcessingInstruction value ? value : default(SanitizerProcessingInstruction?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerPI(string value)
        => new(value);

    /// <summary>将 SanitizerProcessingInstruction 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SanitizerPI(SanitizerProcessingInstruction value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ScrollIntoViewOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ScrollIntoViewArg(bool, ScrollIntoViewOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ScrollIntoViewOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ScrollIntoViewArg(bool value)
        => new(value);

    /// <summary>将 ScrollIntoViewOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：DocumentRef、XMLHttpRequestBodyInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SendBody(DocumentRef, XMLHttpRequestBodyInit)
{

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>读取 XMLHttpRequestBodyInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendBody(DocumentRef value)
        => new(value);

    /// <summary>将 XMLHttpRequestBodyInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、EffectTiming。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SequenceEffectTiming(double, EffectTiming)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 EffectTiming 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SequenceEffectTiming(double value)
        => new(value);

    /// <summary>将 EffectTiming 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SequenceEffectTiming(EffectTiming value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ServiceWorkerContainerRegisterScriptURL(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ServiceWorkerContainerRegisterScriptURL(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ServiceWorkerContainerRegisterScriptURL(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedType、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetAttributeNSValue(TrustedType, string)
{

    /// <summary>读取 TrustedType 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedType 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetAttributeNSValue(TrustedType value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetAttributeNSValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedType、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetAttributeValue(TrustedType, string)
{

    /// <summary>读取 TrustedType 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedType 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetAttributeValue(TrustedType value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetAttributeValue(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：FileRef、string、FormData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetFormValueState(FileRef, string, FormData)
{

    /// <summary>读取 FileRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileRef? AsFile => Value is FileRef value ? value : default(FileRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 FormData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    /// <summary>将 FileRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetFormValueState(FileRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetFormValueState(string value)
        => new(value);

    /// <summary>将 FormData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetFormValueState(FormData value)
        => new(value);
}

/// <summary>WebIDL 联合值：Sanitizer、SanitizerConfig、SanitizerPresets。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetHTMLOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    /// <summary>读取 Sanitizer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    /// <summary>读取 SanitizerConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    /// <summary>读取 SanitizerPresets 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    /// <summary>将 Sanitizer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLOptionsSanitizer(Sanitizer value)
        => new(value);

    /// <summary>将 SanitizerConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLOptionsSanitizer(SanitizerConfig value)
        => new(value);

    /// <summary>将 SanitizerPresets 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

/// <summary>WebIDL 联合值：Sanitizer、SanitizerConfig、SanitizerPresets。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetHTMLUnsafeOptionsSanitizer(Sanitizer, SanitizerConfig, SanitizerPresets)
{

    /// <summary>读取 Sanitizer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Sanitizer? AsSanitizer => Value is Sanitizer value ? value : default(Sanitizer?);

    /// <summary>读取 SanitizerConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    /// <summary>读取 SanitizerPresets 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    /// <summary>将 Sanitizer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLUnsafeOptionsSanitizer(Sanitizer value)
        => new(value);

    /// <summary>将 SanitizerConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLUnsafeOptionsSanitizer(SanitizerConfig value)
        => new(value);

    /// <summary>将 SanitizerPresets 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetHTMLUnsafeOptionsSanitizer(SanitizerPresets value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetSinkId(string, AudioSinkOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetSinkId(string value)
        => new(value);

    /// <summary>将 AudioSinkOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SetSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、CSSPseudoElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowAnimationNewTarget(Element, CSSPseudoElement)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowAnimationNewTarget(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowAnimationNewTarget(CSSPseudoElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowRootInnerHTML(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowRootInnerHTML(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowRootInnerHTML(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ShadowRootSetHTMLUnsafeHtml(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowRootSetHTMLUnsafeHtml(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ShadowRootSetHTMLUnsafeHtml(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SharedWorkerOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedWorkerOptionsValue(string, SharedWorkerOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SharedWorkerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SharedWorkerOptionsValue(string value)
        => new(value);

    /// <summary>将 SharedWorkerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SharedWorkerOptionsValue(SharedWorkerOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedWorkerScriptURL(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SharedWorkerScriptURL(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SharedWorkerScriptURL(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：WellKnownDirectory、FileSystemHandle。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StartInDirectory(WellKnownDirectory, FileSystemHandle)
{

    /// <summary>读取 WellKnownDirectory 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WellKnownDirectory? AsWellKnownDirectory => Value is WellKnownDirectory value ? value : default(WellKnownDirectory?);

    /// <summary>读取 FileSystemHandle 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileSystemHandle? AsFileSystemHandle => Value is FileSystemHandle value ? value : default(FileSystemHandle?);

    /// <summary>将 WellKnownDirectory 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StartInDirectory(WellKnownDirectory value)
        => new(value);

    /// <summary>将 FileSystemHandle 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StartInDirectory(FileSystemHandle value)
        => new(value);
}

/// <summary>WebIDL 联合值：ViewTransitionUpdateCallback、StartViewTransitionOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StartViewTransitionCallbackOptions(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    /// <summary>读取 ViewTransitionUpdateCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    /// <summary>读取 StartViewTransitionOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    /// <summary>将 ViewTransitionUpdateCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StartViewTransitionCallbackOptions(ViewTransitionUpdateCallback value)
        => new(value);

    /// <summary>将 StartViewTransitionOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Blob、MediaSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StorageAccessHandleCreateObjectURLObj(Blob, MediaSource)
{

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>读取 MediaSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StorageAccessHandleCreateObjectURLObj(Blob value)
        => new(value);

    /// <summary>将 MediaSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StorageAccessHandleCreateObjectURLObj(MediaSource value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SharedWorkerOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StorageAccessHandleSharedWorkerOptions(string, SharedWorkerOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SharedWorkerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StorageAccessHandleSharedWorkerOptions(string value)
        => new(value);

    /// <summary>将 SharedWorkerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StorageAccessHandleSharedWorkerOptions(SharedWorkerOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Node、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCache(Node, string)
{

    /// <summary>读取 Node 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Node? AsNode => Value is Node value ? value : default(Node?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Node 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(Node value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、CanvasGradient、CanvasPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue(string, CanvasGradient, CanvasPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 CanvasGradient 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasGradient? AsCanvasGradient => Value is CanvasGradient value ? value : default(CanvasGradient?);

    /// <summary>读取 CanvasPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CanvasPattern? AsCanvasPattern => Value is CanvasPattern value ? value : default(CanvasPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue(string value)
        => new(value);

    /// <summary>将 CanvasGradient 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue(CanvasGradient value)
        => new(value);

    /// <summary>将 CanvasPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue(CanvasPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：TogglePopoverOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue10(TogglePopoverOptions, bool)
{

    /// <summary>读取 TogglePopoverOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 TogglePopoverOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue10(TogglePopoverOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue10(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：ViewTransitionUpdateCallback、StartViewTransitionOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue13(ViewTransitionUpdateCallback, StartViewTransitionOptions)
{

    /// <summary>读取 ViewTransitionUpdateCallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ViewTransitionUpdateCallback? AsViewTransitionUpdateCallback => Value is ViewTransitionUpdateCallback value ? value : default(ViewTransitionUpdateCallback?);

    /// <summary>读取 StartViewTransitionOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    /// <summary>将 ViewTransitionUpdateCallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue13(ViewTransitionUpdateCallback value)
        => new(value);

    /// <summary>将 StartViewTransitionOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue13(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ScrollIntoViewOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue14(bool, ScrollIntoViewOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ScrollIntoViewOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue14(bool value)
        => new(value);

    /// <summary>将 ScrollIntoViewOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue14(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedType、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue15(TrustedType, string)
{

    /// <summary>读取 TrustedType 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedType? AsTrustedType => Value is TrustedType value ? value : default(TrustedType?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedType 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue15(TrustedType value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue15(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue16(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue16(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue16(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue17(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue17(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue17(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、CSSPseudoElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue18(Element, CSSPseudoElement)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue18(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue18(CSSPseudoElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ElementCreationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue19(string, ElementCreationOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ElementCreationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue19(string value)
        => new(value);

    /// <summary>将 ElementCreationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue19(ElementCreationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、DOMPointInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue2(double, DOMPointInit)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 DOMPointInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DOMPointInit? AsDOMPointInit => Value is DOMPointInit value ? value : default(DOMPointInit?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue2(double value)
        => new(value);

    /// <summary>将 DOMPointInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue2(DOMPointInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、ImportNodeOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue20(bool, ImportNodeOptions)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 ImportNodeOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImportNodeOptions? AsImportNodeOptions => Value is ImportNodeOptions value ? value : default(ImportNodeOptions?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue20(bool value)
        => new(value);

    /// <summary>将 ImportNodeOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue20(ImportNodeOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：AddEventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue21(AddEventListenerOptions, bool)
{

    /// <summary>读取 AddEventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 AddEventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue21(AddEventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue21(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：EventListenerOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue22(EventListenerOptions, bool)
{

    /// <summary>读取 EventListenerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 EventListenerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue22(EventListenerOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue22(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、PerformanceMeasureOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue23(string, PerformanceMeasureOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 PerformanceMeasureOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public PerformanceMeasureOptions? AsPerformanceMeasureOptions => Value is PerformanceMeasureOptions value ? value : default(PerformanceMeasureOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue23(string value)
        => new(value);

    /// <summary>将 PerformanceMeasureOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue23(PerformanceMeasureOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、ArrayBuffer。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue24(string, ArrayBuffer)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue24(string value)
        => new(value);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue24(ArrayBuffer value)
        => new(value);
}

/// <summary>WebIDL 联合值：Blob、MediaSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue25(Blob, MediaSource)
{

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>读取 MediaSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue25(Blob value)
        => new(value);

    /// <summary>将 MediaSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue25(MediaSource value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCollection、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue27(HTMLCollection, Element)
{

    /// <summary>读取 HTMLCollection 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 HTMLCollection 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue27(HTMLCollection value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue27(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：RadioNodeList、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue28(RadioNodeList, Element)
{

    /// <summary>读取 RadioNodeList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 RadioNodeList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue28(RadioNodeList value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue28(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLOptionElement、HTMLOptGroupElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue29(HTMLOptionElement, HTMLOptGroupElement)
{

    /// <summary>读取 HTMLOptionElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    /// <summary>读取 HTMLOptGroupElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    /// <summary>将 HTMLOptionElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue29(HTMLOptionElement value)
        => new(value);

    /// <summary>将 HTMLOptGroupElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue29(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLElement、int。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue30(HTMLElement, int)
{

    /// <summary>读取 HTMLElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    /// <summary>读取 int 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public int? AsInt => Value is int value ? value : default(int?);

    /// <summary>将 HTMLElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue30(HTMLElement value)
        => new(value);

    /// <summary>将 int 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue30(int value)
        => new(value);
}

/// <summary>WebIDL 联合值：VideoTrack、AudioTrack、TextTrack。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue31(VideoTrack, AudioTrack, TextTrack)
{

    /// <summary>读取 VideoTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    /// <summary>读取 AudioTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    /// <summary>读取 TextTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    /// <summary>将 VideoTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue31(VideoTrack value)
        => new(value);

    /// <summary>将 AudioTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue31(AudioTrack value)
        => new(value);

    /// <summary>将 TextTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue31(TextTrack value)
        => new(value);
}

/// <summary>WebIDL 联合值：RadioNodeList、Element。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue32(RadioNodeList, Element)
{

    /// <summary>读取 RadioNodeList 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>将 RadioNodeList 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue32(RadioNodeList value)
        => new(value);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue32(Element value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、Text。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue33(Element, Text)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 Text 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Text? AsText => Value is Text value ? value : default(Text?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue33(Element value)
        => new(value);

    /// <summary>将 Text 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue33(Text value)
        => new(value);
}

/// <summary>WebIDL 联合值：Path2D、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue34(Path2D, string)
{

    /// <summary>读取 Path2D 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Path2D? AsPath2D => Value is Path2D value ? value : default(Path2D?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 Path2D 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue34(Path2D value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue34(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：FileRef、string、FormData。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue35(FileRef, string, FormData)
{

    /// <summary>读取 FileRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FileRef? AsFile => Value is FileRef value ? value : default(FileRef?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 FormData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    /// <summary>将 FileRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue35(FileRef value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue35(string value)
        => new(value);

    /// <summary>将 FormData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue35(FormData value)
        => new(value);
}

/// <summary>WebIDL 联合值：SanitizerConfig、SanitizerPresets。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue36(SanitizerConfig, SanitizerPresets)
{

    /// <summary>读取 SanitizerConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerConfig? AsSanitizerConfig => Value is SanitizerConfig value ? value : default(SanitizerConfig?);

    /// <summary>读取 SanitizerPresets 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SanitizerPresets? AsSanitizerPresets => Value is SanitizerPresets value ? value : default(SanitizerPresets?);

    /// <summary>将 SanitizerConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue36(SanitizerConfig value)
        => new(value);

    /// <summary>将 SanitizerPresets 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue36(SanitizerPresets value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue37(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue37(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue37(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、SharedWorkerOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue38(string, SharedWorkerOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 SharedWorkerOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public SharedWorkerOptions? AsSharedWorkerOptions => Value is SharedWorkerOptions value ? value : default(SharedWorkerOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue38(string value)
        => new(value);

    /// <summary>将 SharedWorkerOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue38(SharedWorkerOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、KeyframeAnimationOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue4(double, KeyframeAnimationOptions)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 KeyframeAnimationOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public KeyframeAnimationOptions? AsKeyframeAnimationOptions => Value is KeyframeAnimationOptions value ? value : default(KeyframeAnimationOptions?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue4(double value)
        => new(value);

    /// <summary>将 KeyframeAnimationOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue4(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、DocumentRef。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue42(Element, DocumentRef)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue42(Element value)
        => new(value);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue42(DocumentRef value)
        => new(value);
}

/// <summary>WebIDL 联合值：Client、ServiceWorker、MessagePort。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue44(Client, ServiceWorker, MessagePort)
{

    /// <summary>读取 Client 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Client? AsClient => Value is Client value ? value : default(Client?);

    /// <summary>读取 ServiceWorker 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    /// <summary>读取 MessagePort 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    /// <summary>将 Client 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue44(Client value)
        => new(value);

    /// <summary>将 ServiceWorker 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue44(ServiceWorker value)
        => new(value);

    /// <summary>将 MessagePort 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue44(MessagePort value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、TrustedScriptURL。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue45(string, TrustedScriptURL)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue45(string value)
        => new(value);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue45(TrustedScriptURL value)
        => new(value);
}

/// <summary>WebIDL 联合值：Element、CSSPseudoElement。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue46(Element, CSSPseudoElement)
{

    /// <summary>读取 Element 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Element? AsElement => Value is Element value ? value : default(Element?);

    /// <summary>读取 CSSPseudoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    /// <summary>将 Element 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue46(Element value)
        => new(value);

    /// <summary>将 CSSPseudoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue46(CSSPseudoElement value)
        => new(value);
}

/// <summary>WebIDL 联合值：DocumentRef、XMLHttpRequestBodyInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue47(DocumentRef, XMLHttpRequestBodyInit)
{

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>读取 XMLHttpRequestBodyInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue47(DocumentRef value)
        => new(value);

    /// <summary>将 XMLHttpRequestBodyInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue47(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、KeyframeEffectOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue49(double, KeyframeEffectOptions)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 KeyframeEffectOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public KeyframeEffectOptions? AsKeyframeEffectOptions => Value is KeyframeEffectOptions value ? value : default(KeyframeEffectOptions?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue49(double value)
        => new(value);

    /// <summary>将 KeyframeEffectOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue49(KeyframeEffectOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue5(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue5(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue5(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：double、EffectTiming。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue50(double, EffectTiming)
{

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 EffectTiming 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue50(double value)
        => new(value);

    /// <summary>将 EffectTiming 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue50(EffectTiming value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、uint。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue51(string, uint)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 uint 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public uint? AsUint => Value is uint value ? value : default(uint?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue51(string value)
        => new(value);

    /// <summary>将 uint 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue51(uint value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkInfo。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue52(string, AudioSinkInfo)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkInfo 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkInfo? AsAudioSinkInfo => Value is AudioSinkInfo value ? value : default(AudioSinkInfo?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue52(string value)
        => new(value);

    /// <summary>将 AudioSinkInfo 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue52(AudioSinkInfo value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、AudioSinkOptions。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue53(string, AudioSinkOptions)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 AudioSinkOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue53(string value)
        => new(value);

    /// <summary>将 AudioSinkOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue53(AudioSinkOptions value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStreamTrack、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue59(MediaStreamTrack, string)
{

    /// <summary>读取 MediaStreamTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 MediaStreamTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue59(MediaStreamTrack value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue59(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue6(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue6(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue6(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：RTCEncodedVideoFrame、RTCEncodedAudioFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue60(RTCEncodedVideoFrame, RTCEncodedAudioFrame)
{

    /// <summary>读取 RTCEncodedVideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedVideoFrame? AsRTCEncodedVideoFrame => Value is RTCEncodedVideoFrame value ? value : default(RTCEncodedVideoFrame?);

    /// <summary>读取 RTCEncodedAudioFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public RTCEncodedAudioFrame? AsRTCEncodedAudioFrame => Value is RTCEncodedAudioFrame value ? value : default(RTCEncodedAudioFrame?);

    /// <summary>将 RTCEncodedVideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue60(RTCEncodedVideoFrame value)
        => new(value);

    /// <summary>将 RTCEncodedAudioFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue60(RTCEncodedAudioFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：MediaStreamTrackAudioStats、MediaStreamTrackVideoStats。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue8(MediaStreamTrackAudioStats, MediaStreamTrackVideoStats)
{

    /// <summary>读取 MediaStreamTrackAudioStats 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrackAudioStats? AsMediaStreamTrackAudioStats => Value is MediaStreamTrackAudioStats value ? value : default(MediaStreamTrackAudioStats?);

    /// <summary>读取 MediaStreamTrackVideoStats 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaStreamTrackVideoStats? AsMediaStreamTrackVideoStats => Value is MediaStreamTrackVideoStats value ? value : default(MediaStreamTrackVideoStats?);

    /// <summary>将 MediaStreamTrackAudioStats 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue8(MediaStreamTrackAudioStats value)
        => new(value);

    /// <summary>将 MediaStreamTrackVideoStats 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue8(MediaStreamTrackVideoStats value)
        => new(value);
}

/// <summary>WebIDL 联合值：bool、double、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue9(bool, double, string)
{

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>读取 double 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double? AsDouble => Value is double value ? value : default(double?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue9(bool value)
        => new(value);

    /// <summary>将 double 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue9(double value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValue9(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：CryptoKey、CryptoKeyPair。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValueValue(CryptoKey, CryptoKeyPair)
{

    /// <summary>读取 CryptoKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CryptoKey? AsCryptoKey => Value is CryptoKey value ? value : default(CryptoKey?);

    /// <summary>读取 CryptoKeyPair 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CryptoKeyPair? AsCryptoKeyPair => Value is CryptoKeyPair value ? value : default(CryptoKeyPair?);

    /// <summary>将 CryptoKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValueValue(CryptoKey value)
        => new(value);

    /// <summary>将 CryptoKeyPair 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValueValue(CryptoKeyPair value)
        => new(value);
}

/// <summary>WebIDL 联合值：ArrayBuffer、JsonWebKey。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValueValue2(ArrayBuffer, JsonWebKey)
{

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    /// <summary>读取 JsonWebKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public JsonWebKey? AsJsonWebKey => Value is JsonWebKey value ? value : default(JsonWebKey?);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValueValue2(ArrayBuffer value)
        => new(value);

    /// <summary>将 JsonWebKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCacheValueValue2(JsonWebKey value)
        => new(value);
}

/// <summary>WebIDL 联合值：ArrayBuffer、JsonWebKey。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SubtleCryptoExportKeyResultValue(ArrayBuffer, JsonWebKey)
{

    /// <summary>读取 ArrayBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    /// <summary>读取 JsonWebKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public JsonWebKey? AsJsonWebKey => Value is JsonWebKey value ? value : default(JsonWebKey?);

    /// <summary>将 ArrayBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoExportKeyResultValue(ArrayBuffer value)
        => new(value);

    /// <summary>将 JsonWebKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoExportKeyResultValue(JsonWebKey value)
        => new(value);
}

/// <summary>WebIDL 联合值：CryptoKey、CryptoKeyPair。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SubtleCryptoGenerateKeyResultValue(CryptoKey, CryptoKeyPair)
{

    /// <summary>读取 CryptoKey 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CryptoKey? AsCryptoKey => Value is CryptoKey value ? value : default(CryptoKey?);

    /// <summary>读取 CryptoKeyPair 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CryptoKeyPair? AsCryptoKeyPair => Value is CryptoKeyPair value ? value : default(CryptoKeyPair?);

    /// <summary>将 CryptoKey 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoGenerateKeyResultValue(CryptoKey value)
        => new(value);

    /// <summary>将 CryptoKeyPair 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator SubtleCryptoGenerateKeyResultValue(CryptoKeyPair value)
        => new(value);
}

/// <summary>WebIDL 联合值：TaskPriority、TaskSignal。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TaskSignalAnyInitPriority(TaskPriority, TaskSignal)
{

    /// <summary>读取 TaskPriority 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TaskPriority? AsTaskPriority => Value is TaskPriority value ? value : default(TaskPriority?);

    /// <summary>读取 TaskSignal 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TaskSignal? AsTaskSignal => Value is TaskSignal value ? value : default(TaskSignal?);

    /// <summary>将 TaskPriority 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TaskSignalAnyInitPriority(TaskPriority value)
        => new(value);

    /// <summary>将 TaskSignal 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TaskSignalAnyInitPriority(TaskSignal value)
        => new(value);
}

/// <summary>WebIDL 联合值：ImageBitmap、ImageData、HTMLImageElement、HTMLCanvasElement、HTMLVideoElement、OffscreenCanvas、VideoFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TexImageSource(ImageBitmap, ImageData, HTMLImageElement, HTMLCanvasElement, HTMLVideoElement, OffscreenCanvas, VideoFrame)
{

    /// <summary>读取 ImageBitmap 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    /// <summary>读取 ImageData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    /// <summary>读取 HTMLImageElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 HTMLVideoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>读取 VideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    /// <summary>将 ImageBitmap 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(ImageBitmap value)
        => new(value);

    /// <summary>将 ImageData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(ImageData value)
        => new(value);

    /// <summary>将 HTMLImageElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(HTMLImageElement value)
        => new(value);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 HTMLVideoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(HTMLVideoElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(OffscreenCanvas value)
        => new(value);

    /// <summary>将 VideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TexImageSource(VideoFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、Delegate、TrustedScript。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TimerHandler(string, Delegate, TrustedScript)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 Delegate 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Delegate? AsDelegate => Value is Delegate value ? value : default(Delegate?);

    /// <summary>读取 TrustedScript 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScript? AsTrustedScript => Value is TrustedScript value ? value : default(TrustedScript?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TimerHandler(string value)
        => new(value);

    /// <summary>将 Delegate 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TimerHandler(Delegate value)
        => new(value);

    /// <summary>将 TrustedScript 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TimerHandler(TrustedScript value)
        => new(value);
}

/// <summary>WebIDL 联合值：TogglePopoverOptions、bool。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TogglePopoverOptionsValue(TogglePopoverOptions, bool)
{

    /// <summary>读取 TogglePopoverOptions 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TogglePopoverOptions? AsTogglePopoverOptions => Value is TogglePopoverOptions value ? value : default(TogglePopoverOptions?);

    /// <summary>读取 bool 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public bool? AsBool => Value is bool value ? value : default(bool?);

    /// <summary>将 TogglePopoverOptions 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TogglePopoverOptionsValue(TogglePopoverOptions value)
        => new(value);

    /// <summary>将 bool 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TogglePopoverOptionsValue(bool value)
        => new(value);
}

/// <summary>WebIDL 联合值：VideoTrack、AudioTrack、TextTrack。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrackEventInitTrack(VideoTrack, AudioTrack, TextTrack)
{

    /// <summary>读取 VideoTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    /// <summary>读取 AudioTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    /// <summary>读取 TextTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    /// <summary>将 VideoTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventInitTrack(VideoTrack value)
        => new(value);

    /// <summary>将 AudioTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventInitTrack(AudioTrack value)
        => new(value);

    /// <summary>将 TextTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventInitTrack(TextTrack value)
        => new(value);
}

/// <summary>WebIDL 联合值：VideoTrack、AudioTrack、TextTrack。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrackEventTrack(VideoTrack, AudioTrack, TextTrack)
{

    /// <summary>读取 VideoTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    /// <summary>读取 AudioTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    /// <summary>读取 TextTrack 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    /// <summary>将 VideoTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventTrack(VideoTrack value)
        => new(value);

    /// <summary>将 AudioTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventTrack(AudioTrack value)
        => new(value);

    /// <summary>将 TextTrack 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrackEventTrack(TextTrack value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、TrustedScript、TrustedScriptURL。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TrustedType(TrustedHTML, TrustedScript, TrustedScriptURL)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 TrustedScript 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScript? AsTrustedScript => Value is TrustedScript value ? value : default(TrustedScript?);

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrustedType(TrustedHTML value)
        => new(value);

    /// <summary>将 TrustedScript 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrustedType(TrustedScript value)
        => new(value);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator TrustedType(TrustedScriptURL value)
        => new(value);
}

/// <summary>WebIDL 联合值：Blob、MediaSource。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLCreateObjectURLObj(Blob, MediaSource)
{

    /// <summary>读取 Blob 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    /// <summary>读取 MediaSource 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    /// <summary>将 Blob 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLCreateObjectURLObj(Blob value)
        => new(value);

    /// <summary>将 MediaSource 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLCreateObjectURLObj(MediaSource value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、URLPatternInit、URLPattern。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLPatternCompatible(string, URLPatternInit, URLPattern)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 URLPatternInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public URLPatternInit? AsURLPatternInit => Value is URLPatternInit value ? value : default(URLPatternInit?);

    /// <summary>读取 URLPattern 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public URLPattern? AsURLPattern => Value is URLPattern value ? value : default(URLPattern?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLPatternCompatible(string value)
        => new(value);

    /// <summary>将 URLPatternInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLPatternCompatible(URLPatternInit value)
        => new(value);

    /// <summary>将 URLPattern 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLPatternCompatible(URLPattern value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、URLPatternInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union URLPatternInput(string, URLPatternInit)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 URLPatternInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public URLPatternInit? AsURLPatternInit => Value is URLPatternInit value ? value : default(URLPatternInit?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLPatternInput(string value)
        => new(value);

    /// <summary>将 URLPatternInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator URLPatternInput(URLPatternInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：string、FencedFrameConfig。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union UrnOrConfig(string, FencedFrameConfig)
{

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>读取 FencedFrameConfig 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public FencedFrameConfig? AsFencedFrameConfig => Value is FencedFrameConfig value ? value : default(FencedFrameConfig?);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator UrnOrConfig(string value)
        => new(value);

    /// <summary>将 FencedFrameConfig 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator UrnOrConfig(FencedFrameConfig value)
        => new(value);
}

/// <summary>WebIDL 联合值：CSSNumericValue、CSSKeywordValue。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ViewTimelineOptionsInset(CSSNumericValue, CSSKeywordValue)
{

    /// <summary>读取 CSSNumericValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSNumericValue? AsCSSNumericValue => Value is CSSNumericValue value ? value : default(CSSNumericValue?);

    /// <summary>读取 CSSKeywordValue 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public CSSKeywordValue? AsCSSKeywordValue => Value is CSSKeywordValue value ? value : default(CSSKeywordValue?);

    /// <summary>将 CSSNumericValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ViewTimelineOptionsInset(CSSNumericValue value)
        => new(value);

    /// <summary>将 CSSKeywordValue 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator ViewTimelineOptionsInset(CSSKeywordValue value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WebGL2RenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebGL2RenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebGL2RenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WebGLRenderingContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebGLRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WebGLRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerGlobalScopeImportScriptsUrls(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerGlobalScopeImportScriptsUrls(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerGlobalScopeImportScriptsUrls(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：Worker、WorkerAndParameters。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerOrWorkerAndParameters(Worker, WorkerAndParameters)
{

    /// <summary>读取 Worker 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public Worker? AsWorker => Value is Worker value ? value : default(Worker?);

    /// <summary>读取 WorkerAndParameters 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WorkerAndParameters? AsWorkerAndParameters => Value is WorkerAndParameters value ? value : default(WorkerAndParameters?);

    /// <summary>将 Worker 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerOrWorkerAndParameters(Worker value)
        => new(value);

    /// <summary>将 WorkerAndParameters 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerOrWorkerAndParameters(WorkerAndParameters value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedScriptURL、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WorkerScriptURL(TrustedScriptURL, string)
{

    /// <summary>读取 TrustedScriptURL 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedScriptURL? AsTrustedScriptURL => Value is TrustedScriptURL value ? value : default(TrustedScriptURL?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedScriptURL 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerScriptURL(TrustedScriptURL value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WorkerScriptURL(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WriteText(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteText(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WriteText(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：TrustedHTML、string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union WritelnText(TrustedHTML, string)
{

    /// <summary>读取 TrustedHTML 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public TrustedHTML? AsTrustedHTML => Value is TrustedHTML value ? value : default(TrustedHTML?);

    /// <summary>读取 string 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public string? AsString => Value is string value ? value : default(string?);

    /// <summary>将 TrustedHTML 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WritelnText(TrustedHTML value)
        => new(value);

    /// <summary>将 string 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator WritelnText(string value)
        => new(value);
}

/// <summary>WebIDL 联合值：DocumentRef、XMLHttpRequestBodyInit。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XMLHttpRequestSendBody(DocumentRef, XMLHttpRequestBodyInit)
{

    /// <summary>读取 DocumentRef 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public DocumentRef? AsDocumentRef => Value is DocumentRef value ? value : default(DocumentRef?);

    /// <summary>读取 XMLHttpRequestBodyInit 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    /// <summary>将 DocumentRef 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestSendBody(DocumentRef value)
        => new(value);

    /// <summary>将 XMLHttpRequestBodyInit 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XMLHttpRequestSendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>WebIDL 联合值：XPathNSResolverLiteral、LookupNamespaceURICallback。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XPathNSResolverValue(XPathNSResolverLiteral, LookupNamespaceURICallback)
{

    /// <summary>读取 XPathNSResolverLiteral 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public XPathNSResolverLiteral? AsXPathNSResolverLiteral => Value is XPathNSResolverLiteral value ? value : default(XPathNSResolverLiteral?);

    /// <summary>读取 LookupNamespaceURICallback 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public LookupNamespaceURICallback? AsLookupNamespaceURICallback => Value is LookupNamespaceURICallback value ? value : default(LookupNamespaceURICallback?);

    /// <summary>将 XPathNSResolverLiteral 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XPathNSResolverValue(XPathNSResolverLiteral value)
        => new(value);

    /// <summary>将 LookupNamespaceURICallback 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XPathNSResolverValue(LookupNamespaceURICallback value)
        => new(value);
}

/// <summary>WebIDL 联合值：WebGLRenderingContext、WebGL2RenderingContext。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XRWebGLRenderingContext(WebGLRenderingContext, WebGL2RenderingContext)
{

    /// <summary>读取 WebGLRenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGLRenderingContext? AsWebGLRenderingContext => Value is WebGLRenderingContext value ? value : default(WebGLRenderingContext?);

    /// <summary>读取 WebGL2RenderingContext 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public WebGL2RenderingContext? AsWebGL2RenderingContext => Value is WebGL2RenderingContext value ? value : default(WebGL2RenderingContext?);

    /// <summary>将 WebGLRenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XRWebGLRenderingContext(WebGLRenderingContext value)
        => new(value);

    /// <summary>将 WebGL2RenderingContext 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator XRWebGLRenderingContext(WebGL2RenderingContext value)
        => new(value);
}