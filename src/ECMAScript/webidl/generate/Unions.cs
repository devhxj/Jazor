namespace ECMAScript;

/// <summary>
/// AllowedBluetoothDeviceAllowedServices
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder), nameof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder.Create))]
public readonly struct AllowedBluetoothDeviceAllowedServices : IEither, IEnumerable<UUID>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly UUID[]? _value2;

    private AllowedBluetoothDeviceAllowedServices(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AllowedBluetoothDeviceAllowedServices(UUID[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public UUID[]? AsUUIDArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ArrayBufferView
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ArrayBufferView : IEither
{
    private readonly byte _kind;
    private readonly Int8Array? _value1;
    private readonly Int16Array? _value2;
    private readonly Int32Array? _value3;
    private readonly Uint8Array? _value4;
    private readonly Uint16Array? _value5;
    private readonly Uint32Array? _value6;
    private readonly Uint8ClampedArray? _value7;
    private readonly BigInt64Array? _value8;
    private readonly BigUint64Array? _value9;
    private readonly Float16Array? _value10;
    private readonly Float32Array? _value11;
    private readonly Float64Array? _value12;
    private readonly DataView? _value13;

    private ArrayBufferView(Int8Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Int16Array value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Int32Array value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Uint8Array value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Uint16Array value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Uint32Array value)
    {
        _kind = 6;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = value;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Uint8ClampedArray value)
    {
        _kind = 7;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = value;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(BigInt64Array value)
    {
        _kind = 8;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = value;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(BigUint64Array value)
    {
        _kind = 9;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = value;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Float16Array value)
    {
        _kind = 10;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = value;
        _value11 = default;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Float32Array value)
    {
        _kind = 11;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = value;
        _value12 = default;
        _value13 = default;
    }

    private ArrayBufferView(Float64Array value)
    {
        _kind = 12;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = value;
        _value13 = default;
    }

    private ArrayBufferView(DataView value)
    {
        _kind = 13;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
        _value8 = default;
        _value9 = default;
        _value10 = default;
        _value11 = default;
        _value12 = default;
        _value13 = value;
    }

    public Int8Array? AsInt8Array => _kind == 1 ? _value1 : default;

    public Int16Array? AsInt16Array => _kind == 2 ? _value2 : default;

    public Int32Array? AsInt32Array => _kind == 3 ? _value3 : default;

    public Uint8Array? AsUint8Array => _kind == 4 ? _value4 : default;

    public Uint16Array? AsUint16Array => _kind == 5 ? _value5 : default;

    public Uint32Array? AsUint32Array => _kind == 6 ? _value6 : default;

    public Uint8ClampedArray? AsUint8ClampedArray => _kind == 7 ? _value7 : default;

    public BigInt64Array? AsBigInt64Array => _kind == 8 ? _value8 : default;

    public BigUint64Array? AsBigUint64Array => _kind == 9 ? _value9 : default;

    public Float16Array? AsFloat16Array => _kind == 10 ? _value10 : default;

    public Float32Array? AsFloat32Array => _kind == 11 ? _value11 : default;

    public Float64Array? AsFloat64Array => _kind == 12 ? _value12 : default;

    public DataView? AsDataView => _kind == 13 ? _value13 : default;

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

/// <summary>
/// AudioContextOptionsLatencyHint
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct AudioContextOptionsLatencyHint : IEither
{
    private readonly byte _kind;
    private readonly AudioContextLatencyCategory? _value1;
    private readonly double? _value2;

    private AudioContextOptionsLatencyHint(AudioContextLatencyCategory value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AudioContextOptionsLatencyHint(double value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AudioContextLatencyCategory? AsAudioContextLatencyCategory => _kind == 1 ? _value1 : default;

    public double? AsDouble => _kind == 2 ? _value2 : default;

    public static implicit operator AudioContextOptionsLatencyHint(AudioContextLatencyCategory value)
        => new(value);

    public static implicit operator AudioContextOptionsLatencyHint(double value)
        => new(value);
}

/// <summary>
/// AudioContextOptionsRenderSizeHint
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct AudioContextOptionsRenderSizeHint : IEither
{
    private readonly byte _kind;
    private readonly AudioContextRenderSizeCategory? _value1;
    private readonly uint? _value2;

    private AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AudioContextOptionsRenderSizeHint(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator AudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    public static implicit operator AudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

/// <summary>
/// AudioContextOptionsSinkId
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct AudioContextOptionsSinkId : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkOptions? _value2;

    private AudioContextOptionsSinkId(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AudioContextOptionsSinkId(AudioSinkOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkOptions? AsAudioSinkOptions => _kind == 2 ? _value2 : default;

    public static implicit operator AudioContextOptionsSinkId(string value)
        => new(value);

    public static implicit operator AudioContextOptionsSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>
/// AudioContextSetSinkId
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct AudioContextSetSinkId : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkOptions? _value2;

    private AudioContextSetSinkId(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AudioContextSetSinkId(AudioSinkOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkOptions? AsAudioSinkOptions => _kind == 2 ? _value2 : default;

    public static implicit operator AudioContextSetSinkId(string value)
        => new(value);

    public static implicit operator AudioContextSetSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>
/// AudioContextSinkId
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct AudioContextSinkId : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkInfo? _value2;

    private AudioContextSinkId(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AudioContextSinkId(AudioSinkInfo value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkInfo? AsAudioSinkInfo => _kind == 2 ? _value2 : default;

    public static implicit operator AudioContextSinkId(string value)
        => new(value);

    public static implicit operator AudioContextSinkId(AudioSinkInfo value)
        => new(value);
}

/// <summary>
/// BackgroundFetchManagerFetchRequests
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BackgroundFetchManagerFetchRequestsCollectionBuilder), nameof(BackgroundFetchManagerFetchRequestsCollectionBuilder.Create))]
public readonly struct BackgroundFetchManagerFetchRequests : IEither, IEnumerable<RequestInfo>
{
    private readonly byte _kind;
    private readonly RequestInfo? _value1;
    private readonly RequestInfo[]? _value2;

    private BackgroundFetchManagerFetchRequests(RequestInfo value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BackgroundFetchManagerFetchRequests(RequestInfo[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RequestInfo? AsRequestInfo => _kind == 1 ? _value1 : default;

    public RequestInfo[]? AsRequestInfoArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// BasePropertyIndexedKeyframeComposite
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeCompositeCollectionBuilder), nameof(BasePropertyIndexedKeyframeCompositeCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeComposite : IEither, IEnumerable<CompositeOperationOrAuto>
{
    private readonly byte _kind;
    private readonly CompositeOperationOrAuto? _value1;
    private readonly CompositeOperationOrAuto[]? _value2;

    private BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BasePropertyIndexedKeyframeComposite(CompositeOperationOrAuto[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CompositeOperationOrAuto? AsCompositeOperationOrAuto => _kind == 1 ? _value1 : default;

    public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// BasePropertyIndexedKeyframeEasing
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeEasingCollectionBuilder), nameof(BasePropertyIndexedKeyframeEasingCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeEasing : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private BasePropertyIndexedKeyframeEasing(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BasePropertyIndexedKeyframeEasing(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// BasePropertyIndexedKeyframeOffset
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeOffsetCollectionBuilder), nameof(BasePropertyIndexedKeyframeOffsetCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeOffset : IEither, IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    private BasePropertyIndexedKeyframeOffset(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BasePropertyIndexedKeyframeOffset(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// BluetoothAdvertisingEventInitUUIDs
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothAdvertisingEventInitUUIDs : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothAdvertisingEventInitUUIDs(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothAdvertisingEventInitUUIDs(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothAdvertisingEventInitUUIDs(string value)
        => new(value);

    public static implicit operator BluetoothAdvertisingEventInitUUIDs(uint value)
        => new(value);
}

/// <summary>
/// BluetoothCharacteristicUUID
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothCharacteristicUUID : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothCharacteristicUUID(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothCharacteristicUUID(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothCharacteristicUUID(string value)
        => new(value);

    public static implicit operator BluetoothCharacteristicUUID(uint value)
        => new(value);
}

/// <summary>
/// BluetoothDescriptorUUID
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothDescriptorUUID : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothDescriptorUUID(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothDescriptorUUID(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothDescriptorUUID(string value)
        => new(value);

    public static implicit operator BluetoothDescriptorUUID(uint value)
        => new(value);
}

/// <summary>
/// BluetoothServiceUUID
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothServiceUUID : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothServiceUUID(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothServiceUUID(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothServiceUUID(string value)
        => new(value);

    public static implicit operator BluetoothServiceUUID(uint value)
        => new(value);
}

/// <summary>
/// BluetoothUUIDGetCharacteristicName
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothUUIDGetCharacteristicName : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothUUIDGetCharacteristicName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothUUIDGetCharacteristicName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothUUIDGetCharacteristicName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetCharacteristicName(uint value)
        => new(value);
}

/// <summary>
/// BluetoothUUIDGetDescriptorName
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothUUIDGetDescriptorName : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothUUIDGetDescriptorName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothUUIDGetDescriptorName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothUUIDGetDescriptorName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetDescriptorName(uint value)
        => new(value);
}

/// <summary>
/// BluetoothUUIDGetServiceName
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BluetoothUUIDGetServiceName : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private BluetoothUUIDGetServiceName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BluetoothUUIDGetServiceName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator BluetoothUUIDGetServiceName(string value)
        => new(value);

    public static implicit operator BluetoothUUIDGetServiceName(uint value)
        => new(value);
}

/// <summary>
/// BodyInit
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BodyInit : IEither
{
    private readonly byte _kind;
    private readonly ReadableStream? _value1;
    private readonly XMLHttpRequestBodyInit? _value2;

    private BodyInit(ReadableStream value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BodyInit(XMLHttpRequestBodyInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public ReadableStream? AsReadableStream => _kind == 1 ? _value1 : default;

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => _kind == 2 ? _value2 : default;

    public static implicit operator BodyInit(ReadableStream value)
        => new(value);

    public static implicit operator BodyInit(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// CSSFontFeatureValuesMapSetValues
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CSSFontFeatureValuesMapSetValuesCollectionBuilder), nameof(CSSFontFeatureValuesMapSetValuesCollectionBuilder.Create))]
public readonly struct CSSFontFeatureValuesMapSetValues : IEither, IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private CSSFontFeatureValuesMapSetValues(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSFontFeatureValuesMapSetValues(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// CSSPseudoElementParent
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CSSPseudoElementParent : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly CSSPseudoElement? _value2;

    private CSSPseudoElementParent(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CSSPseudoElementParent(CSSPseudoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 2 ? _value2 : default;

    public static implicit operator CSSPseudoElementParent(Element value)
        => new(value);

    public static implicit operator CSSPseudoElementParent(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// CanvasImageSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CanvasImageSource : IEither
{
    private readonly byte _kind;
    private readonly HTMLOrSVGImageElement? _value1;
    private readonly HTMLVideoElement? _value2;
    private readonly HTMLCanvasElement? _value3;
    private readonly ImageBitmap? _value4;
    private readonly OffscreenCanvas? _value5;
    private readonly VideoFrame? _value6;

    private CanvasImageSource(HTMLOrSVGImageElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
    }

    private CanvasImageSource(HTMLVideoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
    }

    private CanvasImageSource(HTMLCanvasElement value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
        _value6 = default;
    }

    private CanvasImageSource(ImageBitmap value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
        _value6 = default;
    }

    private CanvasImageSource(OffscreenCanvas value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
        _value6 = default;
    }

    private CanvasImageSource(VideoFrame value)
    {
        _kind = 6;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = value;
    }

    public HTMLOrSVGImageElement? AsHTMLOrSVGImageElement => _kind == 1 ? _value1 : default;

    public HTMLVideoElement? AsHTMLVideoElement => _kind == 2 ? _value2 : default;

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 3 ? _value3 : default;

    public ImageBitmap? AsImageBitmap => _kind == 4 ? _value4 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 5 ? _value5 : default;

    public VideoFrame? AsVideoFrame => _kind == 6 ? _value6 : default;

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

/// <summary>
/// CanvasRenderingContext2DFillStyle
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CanvasRenderingContext2DFillStyle : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CanvasGradient? _value2;
    private readonly CanvasPattern? _value3;

    private CanvasRenderingContext2DFillStyle(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private CanvasRenderingContext2DFillStyle(CanvasGradient value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private CanvasRenderingContext2DFillStyle(CanvasPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CanvasGradient? AsCanvasGradient => _kind == 2 ? _value2 : default;

    public CanvasPattern? AsCanvasPattern => _kind == 3 ? _value3 : default;

    public static implicit operator CanvasRenderingContext2DFillStyle(string value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

/// <summary>
/// CanvasRenderingContext2DRoundRectRadii
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CanvasRenderingContext2DRoundRectRadii : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;

    private CanvasRenderingContext2DRoundRectRadii(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public static implicit operator CanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>
/// CanvasRenderingContext2DRoundRectRadiiValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct CanvasRenderingContext2DRoundRectRadiiValue : IEither, IEnumerable<CanvasRenderingContext2DRoundRectRadii>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;
    private readonly CanvasRenderingContext2DRoundRectRadii[]? _value3;

    private CanvasRenderingContext2DRoundRectRadiiValue(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private CanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private CanvasRenderingContext2DRoundRectRadiiValue(CanvasRenderingContext2DRoundRectRadii[] value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public CanvasRenderingContext2DRoundRectRadii[]? AsCanvasRenderingContext2DRoundRectRadiiArray => _kind == 3 ? _value3 : default;

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

/// <summary>
/// CanvasRenderingContext2DStrokeStyle
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CanvasRenderingContext2DStrokeStyle : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CanvasGradient? _value2;
    private readonly CanvasPattern? _value3;

    private CanvasRenderingContext2DStrokeStyle(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private CanvasRenderingContext2DStrokeStyle(CanvasGradient value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private CanvasRenderingContext2DStrokeStyle(CanvasPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CanvasGradient? AsCanvasGradient => _kind == 2 ? _value2 : default;

    public CanvasPattern? AsCanvasPattern => _kind == 3 ? _value3 : default;

    public static implicit operator CanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    public static implicit operator CanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

/// <summary>
/// CharacterDataAfterNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CharacterDataAfterNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private CharacterDataAfterNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CharacterDataAfterNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator CharacterDataAfterNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataAfterNodes(string value)
        => new(value);
}

/// <summary>
/// CharacterDataBeforeNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CharacterDataBeforeNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private CharacterDataBeforeNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CharacterDataBeforeNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator CharacterDataBeforeNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataBeforeNodes(string value)
        => new(value);
}

/// <summary>
/// CharacterDataReplaceWithNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CharacterDataReplaceWithNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private CharacterDataReplaceWithNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CharacterDataReplaceWithNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator CharacterDataReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator CharacterDataReplaceWithNodes(string value)
        => new(value);
}

/// <summary>
/// ClipboardItemDataValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ClipboardItemDataValue : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly Blob? _value2;

    private ClipboardItemDataValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ClipboardItemDataValue(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public static implicit operator ClipboardItemDataValue(string value)
        => new(value);

    public static implicit operator ClipboardItemDataValue(Blob value)
        => new(value);
}

/// <summary>
/// ConstrainBoolean
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ConstrainBoolean : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ConstrainBooleanParameters? _value2;

    private ConstrainBoolean(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainBoolean(ConstrainBooleanParameters value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ConstrainBooleanParameters? AsConstrainBooleanParameters => _kind == 2 ? _value2 : default;

    public static implicit operator ConstrainBoolean(bool value)
        => new(value);

    public static implicit operator ConstrainBoolean(ConstrainBooleanParameters value)
        => new(value);
}

/// <summary>
/// ConstrainDOMString
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringCollectionBuilder), nameof(ConstrainDOMStringCollectionBuilder.Create))]
public readonly struct ConstrainDOMString : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;
    private readonly ConstrainDOMStringParameters? _value3;

    private ConstrainDOMString(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ConstrainDOMString(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ConstrainDOMString(ConstrainDOMStringParameters value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

    public ConstrainDOMStringParameters? AsConstrainDOMStringParameters => _kind == 3 ? _value3 : default;

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

/// <summary>
/// ConstrainDOMStringParametersExact
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersExactCollectionBuilder), nameof(ConstrainDOMStringParametersExactCollectionBuilder.Create))]
public readonly struct ConstrainDOMStringParametersExact : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private ConstrainDOMStringParametersExact(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainDOMStringParametersExact(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ConstrainDOMStringParametersIdeal
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersIdealCollectionBuilder), nameof(ConstrainDOMStringParametersIdealCollectionBuilder.Create))]
public readonly struct ConstrainDOMStringParametersIdeal : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private ConstrainDOMStringParametersIdeal(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainDOMStringParametersIdeal(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ConstrainDouble
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ConstrainDouble : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly ConstrainDoubleRange? _value2;

    private ConstrainDouble(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainDouble(ConstrainDoubleRange value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public ConstrainDoubleRange? AsConstrainDoubleRange => _kind == 2 ? _value2 : default;

    public static implicit operator ConstrainDouble(double value)
        => new(value);

    public static implicit operator ConstrainDouble(ConstrainDoubleRange value)
        => new(value);
}

/// <summary>
/// ConstrainPoint2D
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainPoint2DCollectionBuilder), nameof(ConstrainPoint2DCollectionBuilder.Create))]
public readonly struct ConstrainPoint2D : IEither, IEnumerable<Point2D>
{
    private readonly byte _kind;
    private readonly Point2D[]? _value1;
    private readonly ConstrainPoint2DParameters? _value2;

    private ConstrainPoint2D(Point2D[] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainPoint2D(ConstrainPoint2DParameters value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Point2D[]? AsPoint2DArray => _kind == 1 ? _value1 : default;

    public ConstrainPoint2DParameters? AsConstrainPoint2DParameters => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ConstrainULong
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ConstrainULong : IEither
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly ConstrainULongRange? _value2;

    private ConstrainULong(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ConstrainULong(ConstrainULongRange value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public ConstrainULongRange? AsConstrainULongRange => _kind == 2 ? _value2 : default;

    public static implicit operator ConstrainULong(uint value)
        => new(value);

    public static implicit operator ConstrainULong(ConstrainULongRange value)
        => new(value);
}

/// <summary>
/// CryptoKeyID
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct CryptoKeyID : IEither
{
    private readonly byte _kind;
    private readonly SmallCryptoKeyID? _value1;
    private readonly BigInteger? _value2;

    private CryptoKeyID(SmallCryptoKeyID value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CryptoKeyID(BigInteger value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public SmallCryptoKeyID? AsSmallCryptoKeyID => _kind == 1 ? _value1 : default;

    public BigInteger? AsBigInteger => _kind == 2 ? _value2 : default;

    public static implicit operator CryptoKeyID(SmallCryptoKeyID value)
        => new(value);

    public static implicit operator CryptoKeyID(BigInteger value)
        => new(value);
}

/// <summary>
/// DOMMatrixInitValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixInitValueCollectionBuilder), nameof(DOMMatrixInitValueCollectionBuilder.Create))]
public readonly struct DOMMatrixInitValue : IEither, IEnumerable<double>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly double[]? _value2;

    private DOMMatrixInitValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DOMMatrixInitValue(double[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public double[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// DOMMatrixReadOnlyInit
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixReadOnlyInitCollectionBuilder), nameof(DOMMatrixReadOnlyInitCollectionBuilder.Create))]
public readonly struct DOMMatrixReadOnlyInit : IEither, IEnumerable<double>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly double[]? _value2;

    private DOMMatrixReadOnlyInit(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DOMMatrixReadOnlyInit(double[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public double[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// DisplayMediaStreamOptionsAudio
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DisplayMediaStreamOptionsAudio : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private DisplayMediaStreamOptionsAudio(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DisplayMediaStreamOptionsAudio(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator DisplayMediaStreamOptionsAudio(bool value)
        => new(value);

    public static implicit operator DisplayMediaStreamOptionsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// DisplayMediaStreamOptionsVideo
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DisplayMediaStreamOptionsVideo : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private DisplayMediaStreamOptionsVideo(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DisplayMediaStreamOptionsVideo(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator DisplayMediaStreamOptionsVideo(bool value)
        => new(value);

    public static implicit operator DisplayMediaStreamOptionsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// DocumentAppendNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentAppendNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentAppendNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentAppendNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentAppendNodes(Node value)
        => new(value);

    public static implicit operator DocumentAppendNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentCreateElementNSOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentCreateElementNSOptions : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ElementCreationOptions? _value2;

    private DocumentCreateElementNSOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentCreateElementNSOptions(ElementCreationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ElementCreationOptions? AsElementCreationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentCreateElementNSOptions(string value)
        => new(value);

    public static implicit operator DocumentCreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// DocumentCreateElementOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentCreateElementOptions : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ElementCreationOptions? _value2;

    private DocumentCreateElementOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentCreateElementOptions(ElementCreationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ElementCreationOptions? AsElementCreationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentCreateElementOptions(string value)
        => new(value);

    public static implicit operator DocumentCreateElementOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// DocumentFragmentAppendNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentFragmentAppendNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentFragmentAppendNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentFragmentAppendNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentFragmentAppendNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentAppendNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentFragmentPrependNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentFragmentPrependNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentFragmentPrependNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentFragmentPrependNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentFragmentPrependNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentPrependNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentFragmentReplaceChildrenNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentFragmentReplaceChildrenNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentFragmentReplaceChildrenNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentFragmentReplaceChildrenNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentFragmentReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator DocumentFragmentReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentPrependNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentPrependNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentPrependNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentPrependNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentPrependNodes(Node value)
        => new(value);

    public static implicit operator DocumentPrependNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentReplaceChildrenNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentReplaceChildrenNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentReplaceChildrenNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentReplaceChildrenNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator DocumentReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentStartViewTransitionCallbackOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentStartViewTransitionCallbackOptions : IEither
{
    private readonly byte _kind;
    private readonly UpdateCallback? _value1;
    private readonly StartViewTransitionOptions? _value2;

    private DocumentStartViewTransitionCallbackOptions(UpdateCallback value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public UpdateCallback? AsUpdateCallback => _kind == 1 ? _value1 : default;

    public StartViewTransitionOptions? AsStartViewTransitionOptions => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentStartViewTransitionCallbackOptions(UpdateCallback value)
        => new(value);

    public static implicit operator DocumentStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// DocumentTypeAfterNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentTypeAfterNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentTypeAfterNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentTypeAfterNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentTypeAfterNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeAfterNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentTypeBeforeNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentTypeBeforeNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentTypeBeforeNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentTypeBeforeNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentTypeBeforeNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeBeforeNodes(string value)
        => new(value);
}

/// <summary>
/// DocumentTypeReplaceWithNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct DocumentTypeReplaceWithNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private DocumentTypeReplaceWithNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DocumentTypeReplaceWithNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator DocumentTypeReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator DocumentTypeReplaceWithNodes(string value)
        => new(value);
}

/// <summary>
/// EffectTimingDuration
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct EffectTimingDuration : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly CSSNumericValue? _value2;
    private readonly string? _value3;

    private EffectTimingDuration(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private EffectTimingDuration(CSSNumericValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private EffectTimingDuration(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public CSSNumericValue? AsCSSNumericValue => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static implicit operator EffectTimingDuration(double value)
        => new(value);

    public static implicit operator EffectTimingDuration(CSSNumericValue value)
        => new(value);

    public static implicit operator EffectTimingDuration(string value)
        => new(value);
}

/// <summary>
/// ElementAfterNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementAfterNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementAfterNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementAfterNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementAfterNodes(Node value)
        => new(value);

    public static implicit operator ElementAfterNodes(string value)
        => new(value);
}

/// <summary>
/// ElementAnimateOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementAnimateOptions : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly KeyframeAnimationOptions? _value2;

    private ElementAnimateOptions(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementAnimateOptions(KeyframeAnimationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator ElementAnimateOptions(double value)
        => new(value);

    public static implicit operator ElementAnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>
/// ElementAppendNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementAppendNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementAppendNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementAppendNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementAppendNodes(Node value)
        => new(value);

    public static implicit operator ElementAppendNodes(string value)
        => new(value);
}

/// <summary>
/// ElementBeforeNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementBeforeNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementBeforeNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementBeforeNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementBeforeNodes(Node value)
        => new(value);

    public static implicit operator ElementBeforeNodes(string value)
        => new(value);
}

/// <summary>
/// ElementInternalsSetFormValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementInternalsSetFormValue : IEither
{
    private readonly byte _kind;
    private readonly File? _value1;
    private readonly string? _value2;
    private readonly FormData? _value3;

    private ElementInternalsSetFormValue(File value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ElementInternalsSetFormValue(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ElementInternalsSetFormValue(FormData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public File? AsFile => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public FormData? AsFormData => _kind == 3 ? _value3 : default;

    public static implicit operator ElementInternalsSetFormValue(File value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValue(string value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValue(FormData value)
        => new(value);
}

/// <summary>
/// ElementInternalsSetFormValueState
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementInternalsSetFormValueState : IEither
{
    private readonly byte _kind;
    private readonly File? _value1;
    private readonly string? _value2;
    private readonly FormData? _value3;

    private ElementInternalsSetFormValueState(File value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ElementInternalsSetFormValueState(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ElementInternalsSetFormValueState(FormData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public File? AsFile => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public FormData? AsFormData => _kind == 3 ? _value3 : default;

    public static implicit operator ElementInternalsSetFormValueState(File value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValueState(string value)
        => new(value);

    public static implicit operator ElementInternalsSetFormValueState(FormData value)
        => new(value);
}

/// <summary>
/// ElementPrependNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementPrependNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementPrependNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementPrependNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementPrependNodes(Node value)
        => new(value);

    public static implicit operator ElementPrependNodes(string value)
        => new(value);
}

/// <summary>
/// ElementReplaceChildrenNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementReplaceChildrenNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementReplaceChildrenNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementReplaceChildrenNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator ElementReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>
/// ElementReplaceWithNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementReplaceWithNodes : IEither
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ElementReplaceWithNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementReplaceWithNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ElementReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator ElementReplaceWithNodes(string value)
        => new(value);
}

/// <summary>
/// ElementScrollIntoViewArg
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ElementScrollIntoViewArg : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ScrollIntoViewOptions? _value2;

    private ElementScrollIntoViewArg(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ElementScrollIntoViewArg(ScrollIntoViewOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => _kind == 2 ? _value2 : default;

    public static implicit operator ElementScrollIntoViewArg(bool value)
        => new(value);

    public static implicit operator ElementScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>
/// EventTargetAddEventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct EventTargetAddEventListenerOptions : IEither
{
    private readonly byte _kind;
    private readonly AddEventListenerOptions? _value1;
    private readonly bool? _value2;

    private EventTargetAddEventListenerOptions(AddEventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private EventTargetAddEventListenerOptions(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AddEventListenerOptions? AsAddEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator EventTargetAddEventListenerOptions(AddEventListenerOptions value)
        => new(value);

    public static implicit operator EventTargetAddEventListenerOptions(bool value)
        => new(value);
}

/// <summary>
/// EventTargetRemoveEventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct EventTargetRemoveEventListenerOptions : IEither
{
    private readonly byte _kind;
    private readonly EventListenerOptions? _value1;
    private readonly bool? _value2;

    private EventTargetRemoveEventListenerOptions(EventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private EventTargetRemoveEventListenerOptions(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public EventListenerOptions? AsEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator EventTargetRemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    public static implicit operator EventTargetRemoveEventListenerOptions(bool value)
        => new(value);
}

/// <summary>
/// ExtendableMessageEventInitSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ExtendableMessageEventInitSource : IEither
{
    private readonly byte _kind;
    private readonly Client? _value1;
    private readonly ServiceWorker? _value2;
    private readonly MessagePort? _value3;

    private ExtendableMessageEventInitSource(Client value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ExtendableMessageEventInitSource(ServiceWorker value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ExtendableMessageEventInitSource(MessagePort value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public Client? AsClient => _kind == 1 ? _value1 : default;

    public ServiceWorker? AsServiceWorker => _kind == 2 ? _value2 : default;

    public MessagePort? AsMessagePort => _kind == 3 ? _value3 : default;

    public static implicit operator ExtendableMessageEventInitSource(Client value)
        => new(value);

    public static implicit operator ExtendableMessageEventInitSource(ServiceWorker value)
        => new(value);

    public static implicit operator ExtendableMessageEventInitSource(MessagePort value)
        => new(value);
}

/// <summary>
/// ExtendableMessageEventSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ExtendableMessageEventSource : IEither
{
    private readonly byte _kind;
    private readonly Client? _value1;
    private readonly ServiceWorker? _value2;
    private readonly MessagePort? _value3;

    private ExtendableMessageEventSource(Client value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ExtendableMessageEventSource(ServiceWorker value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ExtendableMessageEventSource(MessagePort value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public Client? AsClient => _kind == 1 ? _value1 : default;

    public ServiceWorker? AsServiceWorker => _kind == 2 ? _value2 : default;

    public MessagePort? AsMessagePort => _kind == 3 ? _value3 : default;

    public static implicit operator ExtendableMessageEventSource(Client value)
        => new(value);

    public static implicit operator ExtendableMessageEventSource(ServiceWorker value)
        => new(value);

    public static implicit operator ExtendableMessageEventSource(MessagePort value)
        => new(value);
}

/// <summary>
/// FencedFrameConfigSize
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct FencedFrameConfigSize : IEither
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly OpaqueProperty? _value2;

    private FencedFrameConfigSize(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FencedFrameConfigSize(OpaqueProperty value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public OpaqueProperty? AsOpaqueProperty => _kind == 2 ? _value2 : default;

    public static implicit operator FencedFrameConfigSize(uint value)
        => new(value);

    public static implicit operator FencedFrameConfigSize(OpaqueProperty value)
        => new(value);
}

/// <summary>
/// FilePickerAcceptTypeAcceptValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(FilePickerAcceptTypeAcceptValueCollectionBuilder), nameof(FilePickerAcceptTypeAcceptValueCollectionBuilder.Create))]
public readonly struct FilePickerAcceptTypeAcceptValue : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private FilePickerAcceptTypeAcceptValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FilePickerAcceptTypeAcceptValue(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// FileReaderResult
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct FileReaderResult : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ArrayBuffer? _value2;

    private FileReaderResult(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FileReaderResult(ArrayBuffer value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ArrayBuffer? AsArrayBuffer => _kind == 2 ? _value2 : default;

    public static implicit operator FileReaderResult(string value)
        => new(value);

    public static implicit operator FileReaderResult(ArrayBuffer value)
        => new(value);
}

/// <summary>
/// Float32List
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Float32ListCollectionBuilder), nameof(Float32ListCollectionBuilder.Create))]
public readonly struct Float32List : IEither, IEnumerable<GLfloat>
{
    private readonly byte _kind;
    private readonly Float32Array? _value1;
    private readonly GLfloat[]? _value2;

    private Float32List(Float32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private Float32List(GLfloat[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Float32Array? AsFloat32Array => _kind == 1 ? _value1 : default;

    public GLfloat[]? AsGLfloatArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// FontFaceSourceValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct FontFaceSourceValue : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly BinaryData? _value2;

    private FontFaceSourceValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FontFaceSourceValue(BinaryData value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public BinaryData? AsBinaryData => _kind == 2 ? _value2 : default;

    public static implicit operator FontFaceSourceValue(string value)
        => new(value);

    public static implicit operator FontFaceSourceValue(BinaryData value)
        => new(value);
}

/// <summary>
/// FormDataEntryValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct FormDataEntryValue : IEither
{
    private readonly byte _kind;
    private readonly File? _value1;
    private readonly string? _value2;

    private FormDataEntryValue(File value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private FormDataEntryValue(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public File? AsFile => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator FormDataEntryValue(File value)
        => new(value);

    public static implicit operator FormDataEntryValue(string value)
        => new(value);
}

/// <summary>
/// GenerateBidOutputAdComponents
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct GenerateBidOutputAdComponents : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AdRender? _value2;

    private GenerateBidOutputAdComponents(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GenerateBidOutputAdComponents(AdRender value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AdRender? AsAdRender => _kind == 2 ? _value2 : default;

    public static implicit operator GenerateBidOutputAdComponents(string value)
        => new(value);

    public static implicit operator GenerateBidOutputAdComponents(AdRender value)
        => new(value);
}

/// <summary>
/// GenerateBidOutputRender
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct GenerateBidOutputRender : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AdRender? _value2;

    private GenerateBidOutputRender(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GenerateBidOutputRender(AdRender value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AdRender? AsAdRender => _kind == 2 ? _value2 : default;

    public static implicit operator GenerateBidOutputRender(string value)
        => new(value);

    public static implicit operator GenerateBidOutputRender(AdRender value)
        => new(value);
}

/// <summary>
/// GeometryNode
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct GeometryNode : IEither
{
    private readonly byte _kind;
    private readonly Text? _value1;
    private readonly Element? _value2;
    private readonly CSSPseudoElement? _value3;
    private readonly Document? _value4;

    private GeometryNode(Text value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    private GeometryNode(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    private GeometryNode(CSSPseudoElement value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    private GeometryNode(Document value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public Text? AsText => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 3 ? _value3 : default;

    public Document? AsDocument => _kind == 4 ? _value4 : default;

    public static implicit operator GeometryNode(Text value)
        => new(value);

    public static implicit operator GeometryNode(Element value)
        => new(value);

    public static implicit operator GeometryNode(CSSPseudoElement value)
        => new(value);

    public static implicit operator GeometryNode(Document value)
        => new(value);
}

/// <summary>
/// GroupEffectTiming
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct GroupEffectTiming : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly EffectTiming? _value2;

    private GroupEffectTiming(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GroupEffectTiming(EffectTiming value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public EffectTiming? AsEffectTiming => _kind == 2 ? _value2 : default;

    public static implicit operator GroupEffectTiming(double value)
        => new(value);

    public static implicit operator GroupEffectTiming(EffectTiming value)
        => new(value);
}

/// <summary>
/// HTMLAllCollectionItemResult
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLAllCollectionItemResult : IEither
{
    private readonly byte _kind;
    private readonly HTMLCollection? _value1;
    private readonly Element? _value2;

    private HTMLAllCollectionItemResult(HTMLCollection value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLAllCollectionItemResult(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCollection? AsHTMLCollection => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLAllCollectionItemResult(HTMLCollection value)
        => new(value);

    public static implicit operator HTMLAllCollectionItemResult(Element value)
        => new(value);
}

/// <summary>
/// HTMLAllCollectionNamedItemResult
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLAllCollectionNamedItemResult : IEither
{
    private readonly byte _kind;
    private readonly HTMLCollection? _value1;
    private readonly Element? _value2;

    private HTMLAllCollectionNamedItemResult(HTMLCollection value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLAllCollectionNamedItemResult(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCollection? AsHTMLCollection => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLAllCollectionNamedItemResult(HTMLCollection value)
        => new(value);

    public static implicit operator HTMLAllCollectionNamedItemResult(Element value)
        => new(value);
}

/// <summary>
/// HTMLElementHidden
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLElementHidden : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly double? _value2;
    private readonly string? _value3;

    private HTMLElementHidden(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private HTMLElementHidden(double value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private HTMLElementHidden(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public double? AsDouble => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static implicit operator HTMLElementHidden(bool value)
        => new(value);

    public static implicit operator HTMLElementHidden(double value)
        => new(value);

    public static implicit operator HTMLElementHidden(string value)
        => new(value);
}

/// <summary>
/// HTMLFormControlsCollectionNamedItemResult
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLFormControlsCollectionNamedItemResult : IEither
{
    private readonly byte _kind;
    private readonly RadioNodeList? _value1;
    private readonly Element? _value2;

    private HTMLFormControlsCollectionNamedItemResult(RadioNodeList value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLFormControlsCollectionNamedItemResult(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RadioNodeList? AsRadioNodeList => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLFormControlsCollectionNamedItemResult(RadioNodeList value)
        => new(value);

    public static implicit operator HTMLFormControlsCollectionNamedItemResult(Element value)
        => new(value);
}

/// <summary>
/// HTMLFormElementResult
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLFormElementResult : IEither
{
    private readonly byte _kind;
    private readonly RadioNodeList? _value1;
    private readonly Element? _value2;

    private HTMLFormElementResult(RadioNodeList value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLFormElementResult(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RadioNodeList? AsRadioNodeList => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLFormElementResult(RadioNodeList value)
        => new(value);

    public static implicit operator HTMLFormElementResult(Element value)
        => new(value);
}

/// <summary>
/// HTMLOptionsCollectionAddBefore
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLOptionsCollectionAddBefore : IEither
{
    private readonly byte _kind;
    private readonly HTMLElement? _value1;
    private readonly int? _value2;

    private HTMLOptionsCollectionAddBefore(HTMLElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLOptionsCollectionAddBefore(int value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLElement? AsHTMLElement => _kind == 1 ? _value1 : default;

    public int? AsInt => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLOptionsCollectionAddBefore(HTMLElement value)
        => new(value);

    public static implicit operator HTMLOptionsCollectionAddBefore(int value)
        => new(value);
}

/// <summary>
/// HTMLOptionsCollectionAddElement
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLOptionsCollectionAddElement : IEither
{
    private readonly byte _kind;
    private readonly HTMLOptionElement? _value1;
    private readonly HTMLOptGroupElement? _value2;

    private HTMLOptionsCollectionAddElement(HTMLOptionElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLOptionsCollectionAddElement(HTMLOptGroupElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLOptionElement? AsHTMLOptionElement => _kind == 1 ? _value1 : default;

    public HTMLOptGroupElement? AsHTMLOptGroupElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptionElement value)
        => new(value);

    public static implicit operator HTMLOptionsCollectionAddElement(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>
/// HTMLOrSVGImageElement
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLOrSVGImageElement : IEither
{
    private readonly byte _kind;
    private readonly HTMLImageElement? _value1;
    private readonly SVGImageElement? _value2;

    private HTMLOrSVGImageElement(HTMLImageElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLOrSVGImageElement(SVGImageElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLImageElement? AsHTMLImageElement => _kind == 1 ? _value1 : default;

    public SVGImageElement? AsSVGImageElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLOrSVGImageElement(HTMLImageElement value)
        => new(value);

    public static implicit operator HTMLOrSVGImageElement(SVGImageElement value)
        => new(value);
}

/// <summary>
/// HTMLOrSVGScriptElement
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLOrSVGScriptElement : IEither
{
    private readonly byte _kind;
    private readonly HTMLScriptElement? _value1;
    private readonly SVGScriptElement? _value2;

    private HTMLOrSVGScriptElement(HTMLScriptElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLOrSVGScriptElement(SVGScriptElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLScriptElement? AsHTMLScriptElement => _kind == 1 ? _value1 : default;

    public SVGScriptElement? AsSVGScriptElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLOrSVGScriptElement(HTMLScriptElement value)
        => new(value);

    public static implicit operator HTMLOrSVGScriptElement(SVGScriptElement value)
        => new(value);
}

/// <summary>
/// HTMLSelectElementAddBefore
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLSelectElementAddBefore : IEither
{
    private readonly byte _kind;
    private readonly HTMLElement? _value1;
    private readonly int? _value2;

    private HTMLSelectElementAddBefore(HTMLElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLSelectElementAddBefore(int value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLElement? AsHTMLElement => _kind == 1 ? _value1 : default;

    public int? AsInt => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLSelectElementAddBefore(HTMLElement value)
        => new(value);

    public static implicit operator HTMLSelectElementAddBefore(int value)
        => new(value);
}

/// <summary>
/// HTMLSelectElementAddElement
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLSelectElementAddElement : IEither
{
    private readonly byte _kind;
    private readonly HTMLOptionElement? _value1;
    private readonly HTMLOptGroupElement? _value2;

    private HTMLSelectElementAddElement(HTMLOptionElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLSelectElementAddElement(HTMLOptGroupElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLOptionElement? AsHTMLOptionElement => _kind == 1 ? _value1 : default;

    public HTMLOptGroupElement? AsHTMLOptGroupElement => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLSelectElementAddElement(HTMLOptionElement value)
        => new(value);

    public static implicit operator HTMLSelectElementAddElement(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>
/// HTMLSlotElementAssignNodes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct HTMLSlotElementAssignNodes : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Text? _value2;

    private HTMLSlotElementAssignNodes(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HTMLSlotElementAssignNodes(Text value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Text? AsText => _kind == 2 ? _value2 : default;

    public static implicit operator HTMLSlotElementAssignNodes(Element value)
        => new(value);

    public static implicit operator HTMLSlotElementAssignNodes(Text value)
        => new(value);
}

/// <summary>
/// HeadersInit
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(HeadersInitCollectionBuilder), nameof(HeadersInitCollectionBuilder.Create))]
public readonly struct HeadersInit : IEither, IEnumerable<byte[][]>
{
    private readonly byte _kind;
    private readonly byte[][][]? _value1;
    private readonly Dictionary<byte[], byte[]>? _value2;

    private HeadersInit(byte[][][] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private HeadersInit(Dictionary<byte[], byte[]> value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public byte[][][]? AsByteArrayArrayArray => _kind == 1 ? _value1 : default;

    public Dictionary<byte[], byte[]>? AsDictionaryByteArrayByteArray => _kind == 2 ? _value2 : default;

    public static implicit operator HeadersInit(byte[][][] value)
        => new(value);

    public static implicit operator HeadersInit(Dictionary<byte[], byte[]> value)
        => new(value);

    IEnumerator<byte[][]> IEnumerable<byte[][]>.GetEnumerator()
        => ((IEnumerable<byte[][]>)(AsByteArrayArrayArray ?? Array.Empty<byte[][]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<byte[][]>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class HeadersInitCollectionBuilder
{
    public static HeadersInit Create(ReadOnlySpan<byte[][]> items)
        => items.ToArray();
}

/// <summary>
/// IDBDatabaseTransactionStoreNames
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBDatabaseTransactionStoreNamesCollectionBuilder), nameof(IDBDatabaseTransactionStoreNamesCollectionBuilder.Create))]
public readonly struct IDBDatabaseTransactionStoreNames : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private IDBDatabaseTransactionStoreNames(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IDBDatabaseTransactionStoreNames(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// IDBObjectStoreCreateIndexKeyPath
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder), nameof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder.Create))]
public readonly struct IDBObjectStoreCreateIndexKeyPath : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private IDBObjectStoreCreateIndexKeyPath(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IDBObjectStoreCreateIndexKeyPath(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// IDBObjectStoreParametersKeyPath
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreParametersKeyPathCollectionBuilder), nameof(IDBObjectStoreParametersKeyPathCollectionBuilder.Create))]
public readonly struct IDBObjectStoreParametersKeyPath : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private IDBObjectStoreParametersKeyPath(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IDBObjectStoreParametersKeyPath(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ImageBitmapRenderingContextCanvas
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ImageBitmapRenderingContextCanvas : IEither
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    private ImageBitmapRenderingContextCanvas(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ImageBitmapRenderingContextCanvas(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public static implicit operator ImageBitmapRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator ImageBitmapRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// ImageBitmapSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ImageBitmapSource : IEither
{
    private readonly byte _kind;
    private readonly CanvasImageSource? _value1;
    private readonly Blob? _value2;
    private readonly ImageData? _value3;

    private ImageBitmapSource(CanvasImageSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private ImageBitmapSource(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private ImageBitmapSource(ImageData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public CanvasImageSource? AsCanvasImageSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public ImageData? AsImageData => _kind == 3 ? _value3 : default;

    public static implicit operator ImageBitmapSource(CanvasImageSource value)
        => new(value);

    public static implicit operator ImageBitmapSource(Blob value)
        => new(value);

    public static implicit operator ImageBitmapSource(ImageData value)
        => new(value);
}

/// <summary>
/// InstallEventAddRoutesRules
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(InstallEventAddRoutesRulesCollectionBuilder), nameof(InstallEventAddRoutesRulesCollectionBuilder.Create))]
public readonly struct InstallEventAddRoutesRules : IEither, IEnumerable<RouterRule>
{
    private readonly byte _kind;
    private readonly RouterRule? _value1;
    private readonly RouterRule[]? _value2;

    private InstallEventAddRoutesRules(RouterRule value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private InstallEventAddRoutesRules(RouterRule[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RouterRule? AsRouterRule => _kind == 1 ? _value1 : default;

    public RouterRule[]? AsRouterRuleArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// Int32List
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Int32ListCollectionBuilder), nameof(Int32ListCollectionBuilder.Create))]
public readonly struct Int32List : IEither, IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private Int32List(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private Int32List(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// IntersectionObserverInitRoot
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct IntersectionObserverInitRoot : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Document? _value2;

    private IntersectionObserverInitRoot(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IntersectionObserverInitRoot(Document value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Document? AsDocument => _kind == 2 ? _value2 : default;

    public static implicit operator IntersectionObserverInitRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverInitRoot(Document value)
        => new(value);
}

/// <summary>
/// IntersectionObserverInitThreshold
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IntersectionObserverInitThresholdCollectionBuilder), nameof(IntersectionObserverInitThresholdCollectionBuilder.Create))]
public readonly struct IntersectionObserverInitThreshold : IEither, IEnumerable<double>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double[]? _value2;

    private IntersectionObserverInitThreshold(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IntersectionObserverInitThreshold(double[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public double[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// IntersectionObserverRoot
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct IntersectionObserverRoot : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Document? _value2;

    private IntersectionObserverRoot(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private IntersectionObserverRoot(Document value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Document? AsDocument => _kind == 2 ? _value2 : default;

    public static implicit operator IntersectionObserverRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverRoot(Document value)
        => new(value);
}

/// <summary>
/// KeyframeAnimationOptionsRangeEnd
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct KeyframeAnimationOptionsRangeEnd : IEither
{
    private readonly byte _kind;
    private readonly TimelineRangeOffset? _value1;
    private readonly CSSNumericValue? _value2;
    private readonly CSSKeywordValue? _value3;
    private readonly string? _value4;

    private KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeEnd(CSSNumericValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeEnd(CSSKeywordValue value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeEnd(string value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public TimelineRangeOffset? AsTimelineRangeOffset => _kind == 1 ? _value1 : default;

    public CSSNumericValue? AsCSSNumericValue => _kind == 2 ? _value2 : default;

    public CSSKeywordValue? AsCSSKeywordValue => _kind == 3 ? _value3 : default;

    public string? AsString => _kind == 4 ? _value4 : default;

    public static implicit operator KeyframeAnimationOptionsRangeEnd(TimelineRangeOffset value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSNumericValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(CSSKeywordValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeEnd(string value)
        => new(value);
}

/// <summary>
/// KeyframeAnimationOptionsRangeStart
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct KeyframeAnimationOptionsRangeStart : IEither
{
    private readonly byte _kind;
    private readonly TimelineRangeOffset? _value1;
    private readonly CSSNumericValue? _value2;
    private readonly CSSKeywordValue? _value3;
    private readonly string? _value4;

    private KeyframeAnimationOptionsRangeStart(TimelineRangeOffset value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeStart(CSSNumericValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeStart(CSSKeywordValue value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    private KeyframeAnimationOptionsRangeStart(string value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public TimelineRangeOffset? AsTimelineRangeOffset => _kind == 1 ? _value1 : default;

    public CSSNumericValue? AsCSSNumericValue => _kind == 2 ? _value2 : default;

    public CSSKeywordValue? AsCSSKeywordValue => _kind == 3 ? _value3 : default;

    public string? AsString => _kind == 4 ? _value4 : default;

    public static implicit operator KeyframeAnimationOptionsRangeStart(TimelineRangeOffset value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(CSSNumericValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(CSSKeywordValue value)
        => new(value);

    public static implicit operator KeyframeAnimationOptionsRangeStart(string value)
        => new(value);
}

/// <summary>
/// KeyframeEffectOptionsValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct KeyframeEffectOptionsValue : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly KeyframeEffectOptions? _value2;

    private KeyframeEffectOptionsValue(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private KeyframeEffectOptionsValue(KeyframeEffectOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public KeyframeEffectOptions? AsKeyframeEffectOptions => _kind == 2 ? _value2 : default;

    public static implicit operator KeyframeEffectOptionsValue(double value)
        => new(value);

    public static implicit operator KeyframeEffectOptionsValue(KeyframeEffectOptions value)
        => new(value);
}

/// <summary>
/// LineAndPositionSetting
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct LineAndPositionSetting : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly AutoKeyword? _value2;

    private LineAndPositionSetting(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private LineAndPositionSetting(AutoKeyword value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public AutoKeyword? AsAutoKeyword => _kind == 2 ? _value2 : default;

    public static implicit operator LineAndPositionSetting(double value)
        => new(value);

    public static implicit operator LineAndPositionSetting(AutoKeyword value)
        => new(value);
}

/// <summary>
/// MLGraphBuilderSplitSplits
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(MLGraphBuilderSplitSplitsCollectionBuilder), nameof(MLGraphBuilderSplitSplitsCollectionBuilder.Create))]
public readonly struct MLGraphBuilderSplitSplits : IEither, IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private MLGraphBuilderSplitSplits(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MLGraphBuilderSplitSplits(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// MediaProvider
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaProvider : IEither
{
    private readonly byte _kind;
    private readonly MediaStream? _value1;
    private readonly MediaSource? _value2;
    private readonly Blob? _value3;

    private MediaProvider(MediaStream value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private MediaProvider(MediaSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private MediaProvider(Blob value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public MediaStream? AsMediaStream => _kind == 1 ? _value1 : default;

    public MediaSource? AsMediaSource => _kind == 2 ? _value2 : default;

    public Blob? AsBlob => _kind == 3 ? _value3 : default;

    public static implicit operator MediaProvider(MediaStream value)
        => new(value);

    public static implicit operator MediaProvider(MediaSource value)
        => new(value);

    public static implicit operator MediaProvider(Blob value)
        => new(value);
}

/// <summary>
/// MediaStreamConstraintsAudio
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaStreamConstraintsAudio : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private MediaStreamConstraintsAudio(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MediaStreamConstraintsAudio(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator MediaStreamConstraintsAudio(bool value)
        => new(value);

    public static implicit operator MediaStreamConstraintsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// MediaStreamConstraintsVideo
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaStreamConstraintsVideo : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private MediaStreamConstraintsVideo(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MediaStreamConstraintsVideo(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator MediaStreamConstraintsVideo(bool value)
        => new(value);

    public static implicit operator MediaStreamConstraintsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// MediaTrackConstraintSetPan
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaTrackConstraintSetPan : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ConstrainDouble? _value2;

    private MediaTrackConstraintSetPan(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MediaTrackConstraintSetPan(ConstrainDouble value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ConstrainDouble? AsConstrainDouble => _kind == 2 ? _value2 : default;

    public static implicit operator MediaTrackConstraintSetPan(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetPan(ConstrainDouble value)
        => new(value);
}

/// <summary>
/// MediaTrackConstraintSetTilt
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaTrackConstraintSetTilt : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ConstrainDouble? _value2;

    private MediaTrackConstraintSetTilt(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MediaTrackConstraintSetTilt(ConstrainDouble value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ConstrainDouble? AsConstrainDouble => _kind == 2 ? _value2 : default;

    public static implicit operator MediaTrackConstraintSetTilt(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetTilt(ConstrainDouble value)
        => new(value);
}

/// <summary>
/// MediaTrackConstraintSetZoom
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MediaTrackConstraintSetZoom : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ConstrainDouble? _value2;

    private MediaTrackConstraintSetZoom(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private MediaTrackConstraintSetZoom(ConstrainDouble value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ConstrainDouble? AsConstrainDouble => _kind == 2 ? _value2 : default;

    public static implicit operator MediaTrackConstraintSetZoom(bool value)
        => new(value);

    public static implicit operator MediaTrackConstraintSetZoom(ConstrainDouble value)
        => new(value);
}

/// <summary>
/// MessageEventSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct MessageEventSource : IEither
{
    private readonly byte _kind;
    private readonly WindowProxy? _value1;
    private readonly MessagePort? _value2;
    private readonly ServiceWorker? _value3;

    private MessageEventSource(WindowProxy value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private MessageEventSource(MessagePort value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private MessageEventSource(ServiceWorker value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public WindowProxy? AsWindowProxy => _kind == 1 ? _value1 : default;

    public MessagePort? AsMessagePort => _kind == 2 ? _value2 : default;

    public ServiceWorker? AsServiceWorker => _kind == 3 ? _value3 : default;

    public static implicit operator MessageEventSource(WindowProxy value)
        => new(value);

    public static implicit operator MessageEventSource(MessagePort value)
        => new(value);

    public static implicit operator MessageEventSource(ServiceWorker value)
        => new(value);
}

/// <summary>
/// NavigatorRunAdAuctionResultValue
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct NavigatorRunAdAuctionResultValue : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly FencedFrameConfig? _value2;

    private NavigatorRunAdAuctionResultValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private NavigatorRunAdAuctionResultValue(FencedFrameConfig value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public FencedFrameConfig? AsFencedFrameConfig => _kind == 2 ? _value2 : default;

    public static implicit operator NavigatorRunAdAuctionResultValue(string value)
        => new(value);

    public static implicit operator NavigatorRunAdAuctionResultValue(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// OfflineAudioContextOptionsRenderSizeHint
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OfflineAudioContextOptionsRenderSizeHint : IEither
{
    private readonly byte _kind;
    private readonly AudioContextRenderSizeCategory? _value1;
    private readonly uint? _value2;

    private OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private OfflineAudioContextOptionsRenderSizeHint(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AudioContextRenderSizeCategory? AsAudioContextRenderSizeCategory => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator OfflineAudioContextOptionsRenderSizeHint(AudioContextRenderSizeCategory value)
        => new(value);

    public static implicit operator OfflineAudioContextOptionsRenderSizeHint(uint value)
        => new(value);
}

/// <summary>
/// OffscreenCanvasRenderingContext2DFillStyle
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DFillStyle : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CanvasGradient? _value2;
    private readonly CanvasPattern? _value3;

    private OffscreenCanvasRenderingContext2DFillStyle(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DFillStyle(CanvasGradient value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DFillStyle(CanvasPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CanvasGradient? AsCanvasGradient => _kind == 2 ? _value2 : default;

    public CanvasPattern? AsCanvasPattern => _kind == 3 ? _value3 : default;

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(string value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasGradient value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DFillStyle(CanvasPattern value)
        => new(value);
}

/// <summary>
/// OffscreenCanvasRenderingContext2DRoundRectRadii
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DRoundRectRadii : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;

    private OffscreenCanvasRenderingContext2DRoundRectRadii(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private OffscreenCanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>
/// OffscreenCanvasRenderingContext2DRoundRectRadiiValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct OffscreenCanvasRenderingContext2DRoundRectRadiiValue : IEither, IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;
    private readonly OffscreenCanvasRenderingContext2DRoundRectRadii[]? _value3;

    private OffscreenCanvasRenderingContext2DRoundRectRadiiValue(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DRoundRectRadiiValue(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DRoundRectRadiiValue(OffscreenCanvasRenderingContext2DRoundRectRadii[] value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public OffscreenCanvasRenderingContext2DRoundRectRadii[]? AsOffscreenCanvasRenderingContext2DRoundRectRadiiArray => _kind == 3 ? _value3 : default;

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

/// <summary>
/// OffscreenCanvasRenderingContext2DStrokeStyle
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DStrokeStyle : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CanvasGradient? _value2;
    private readonly CanvasPattern? _value3;

    private OffscreenCanvasRenderingContext2DStrokeStyle(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DStrokeStyle(CanvasGradient value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private OffscreenCanvasRenderingContext2DStrokeStyle(CanvasPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CanvasGradient? AsCanvasGradient => _kind == 2 ? _value2 : default;

    public CanvasPattern? AsCanvasPattern => _kind == 3 ? _value3 : default;

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(string value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasGradient value)
        => new(value);

    public static implicit operator OffscreenCanvasRenderingContext2DStrokeStyle(CanvasPattern value)
        => new(value);
}

/// <summary>
/// OffscreenRenderingContext
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OffscreenRenderingContext : IEither
{
    private readonly byte _kind;
    private readonly OffscreenCanvasRenderingContext2D? _value1;
    private readonly ImageBitmapRenderingContext? _value2;
    private readonly WebGLRenderingContext? _value3;
    private readonly WebGL2RenderingContext? _value4;
    private readonly GPUCanvasContext? _value5;

    private OffscreenRenderingContext(OffscreenCanvasRenderingContext2D value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private OffscreenRenderingContext(ImageBitmapRenderingContext value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private OffscreenRenderingContext(WebGLRenderingContext value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
    }

    private OffscreenRenderingContext(WebGL2RenderingContext value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
    }

    private OffscreenRenderingContext(GPUCanvasContext value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
    }

    public OffscreenCanvasRenderingContext2D? AsOffscreenCanvasRenderingContext2D => _kind == 1 ? _value1 : default;

    public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => _kind == 2 ? _value2 : default;

    public WebGLRenderingContext? AsWebGLRenderingContext => _kind == 3 ? _value3 : default;

    public WebGL2RenderingContext? AsWebGL2RenderingContext => _kind == 4 ? _value4 : default;

    public GPUCanvasContext? AsGPUCanvasContext => _kind == 5 ? _value5 : default;

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

/// <summary>
/// OptionalEffectTimingDuration
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct OptionalEffectTimingDuration : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly string? _value2;

    private OptionalEffectTimingDuration(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private OptionalEffectTimingDuration(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator OptionalEffectTimingDuration(double value)
        => new(value);

    public static implicit operator OptionalEffectTimingDuration(string value)
        => new(value);
}

/// <summary>
/// ParameterCurrentTarget
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ParameterCurrentTarget : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly CSSPseudoElement? _value2;

    private ParameterCurrentTarget(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ParameterCurrentTarget(CSSPseudoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 2 ? _value2 : default;

    public static implicit operator ParameterCurrentTarget(Element value)
        => new(value);

    public static implicit operator ParameterCurrentTarget(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// ParameterEvent
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ParameterEvent : IEither
{
    private readonly byte _kind;
    private readonly Event? _value1;
    private readonly string? _value2;

    private ParameterEvent(Event value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ParameterEvent(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Event? AsEvent => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ParameterEvent(Event value)
        => new(value);

    public static implicit operator ParameterEvent(string value)
        => new(value);
}

/// <summary>
/// PasswordCredentialInit
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct PasswordCredentialInit : IEither
{
    private readonly byte _kind;
    private readonly PasswordCredentialData? _value1;
    private readonly HTMLFormElement? _value2;

    private PasswordCredentialInit(PasswordCredentialData value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PasswordCredentialInit(HTMLFormElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public PasswordCredentialData? AsPasswordCredentialData => _kind == 1 ? _value1 : default;

    public HTMLFormElement? AsHTMLFormElement => _kind == 2 ? _value2 : default;

    public static implicit operator PasswordCredentialInit(PasswordCredentialData value)
        => new(value);

    public static implicit operator PasswordCredentialInit(HTMLFormElement value)
        => new(value);
}

/// <summary>
/// Path2DPath
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct Path2DPath : IEither
{
    private readonly byte _kind;
    private readonly Path2D? _value1;
    private readonly string? _value2;

    private Path2DPath(Path2D value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private Path2DPath(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Path2D? AsPath2D => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator Path2DPath(Path2D value)
        => new(value);

    public static implicit operator Path2DPath(string value)
        => new(value);
}

/// <summary>
/// Path2DRoundRectRadii
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct Path2DRoundRectRadii : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;

    private Path2DRoundRectRadii(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private Path2DRoundRectRadii(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public static implicit operator Path2DRoundRectRadii(double value)
        => new(value);

    public static implicit operator Path2DRoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>
/// Path2DRoundRectRadiiValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Path2DRoundRectRadiiValueCollectionBuilder), nameof(Path2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct Path2DRoundRectRadiiValue : IEither, IEnumerable<Path2DRoundRectRadii>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;
    private readonly Path2DRoundRectRadii[]? _value3;

    private Path2DRoundRectRadiiValue(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private Path2DRoundRectRadiiValue(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private Path2DRoundRectRadiiValue(Path2DRoundRectRadii[] value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public Path2DRoundRectRadii[]? AsPath2DRoundRectRadiiArray => _kind == 3 ? _value3 : default;

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

/// <summary>
/// PerformanceMeasureOptionsEnd
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct PerformanceMeasureOptionsEnd : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly double? _value2;

    private PerformanceMeasureOptionsEnd(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PerformanceMeasureOptionsEnd(double value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public double? AsDouble => _kind == 2 ? _value2 : default;

    public static implicit operator PerformanceMeasureOptionsEnd(string value)
        => new(value);

    public static implicit operator PerformanceMeasureOptionsEnd(double value)
        => new(value);
}

/// <summary>
/// PerformanceMeasureOptionsStart
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct PerformanceMeasureOptionsStart : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly double? _value2;

    private PerformanceMeasureOptionsStart(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PerformanceMeasureOptionsStart(double value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public double? AsDouble => _kind == 2 ? _value2 : default;

    public static implicit operator PerformanceMeasureOptionsStart(string value)
        => new(value);

    public static implicit operator PerformanceMeasureOptionsStart(double value)
        => new(value);
}

/// <summary>
/// PerformanceMeasureStartOrMeasureOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct PerformanceMeasureStartOrMeasureOptions : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly PerformanceMeasureOptions? _value2;

    private PerformanceMeasureStartOrMeasureOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PerformanceMeasureStartOrMeasureOptions(PerformanceMeasureOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public PerformanceMeasureOptions? AsPerformanceMeasureOptions => _kind == 2 ? _value2 : default;

    public static implicit operator PerformanceMeasureStartOrMeasureOptions(string value)
        => new(value);

    public static implicit operator PerformanceMeasureStartOrMeasureOptions(PerformanceMeasureOptions value)
        => new(value);
}

/// <summary>
/// RTCIceServerUrls
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RTCIceServerUrlsCollectionBuilder), nameof(RTCIceServerUrlsCollectionBuilder.Create))]
public readonly struct RTCIceServerUrls : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private RTCIceServerUrls(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RTCIceServerUrls(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// RTCPeerConnectionAddTransceiverTrackOrKind
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RTCPeerConnectionAddTransceiverTrackOrKind : IEither
{
    private readonly byte _kind;
    private readonly MediaStreamTrack? _value1;
    private readonly string? _value2;

    private RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RTCPeerConnectionAddTransceiverTrackOrKind(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public MediaStreamTrack? AsMediaStreamTrack => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(MediaStreamTrack value)
        => new(value);

    public static implicit operator RTCPeerConnectionAddTransceiverTrackOrKind(string value)
        => new(value);
}

/// <summary>
/// RTCRtpTransform
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RTCRtpTransform : IEither
{
    private readonly byte _kind;
    private readonly SFrameTransform? _value1;
    private readonly RTCRtpScriptTransform? _value2;

    private RTCRtpTransform(SFrameTransform value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RTCRtpTransform(RTCRtpScriptTransform value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public SFrameTransform? AsSFrameTransform => _kind == 1 ? _value1 : default;

    public RTCRtpScriptTransform? AsRTCRtpScriptTransform => _kind == 2 ? _value2 : default;

    public static implicit operator RTCRtpTransform(SFrameTransform value)
        => new(value);

    public static implicit operator RTCRtpTransform(RTCRtpScriptTransform value)
        => new(value);
}

/// <summary>
/// ReadableStreamController
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ReadableStreamController : IEither
{
    private readonly byte _kind;
    private readonly ReadableStreamDefaultController? _value1;
    private readonly ReadableByteStreamController? _value2;

    private ReadableStreamController(ReadableStreamDefaultController value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ReadableStreamController(ReadableByteStreamController value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public ReadableStreamDefaultController? AsReadableStreamDefaultController => _kind == 1 ? _value1 : default;

    public ReadableByteStreamController? AsReadableByteStreamController => _kind == 2 ? _value2 : default;

    public static implicit operator ReadableStreamController(ReadableStreamDefaultController value)
        => new(value);

    public static implicit operator ReadableStreamController(ReadableByteStreamController value)
        => new(value);
}

/// <summary>
/// ReadableStreamReader
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ReadableStreamReader : IEither
{
    private readonly byte _kind;
    private readonly ReadableStreamDefaultReader? _value1;
    private readonly ReadableStreamBYOBReader? _value2;

    private ReadableStreamReader(ReadableStreamDefaultReader value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ReadableStreamReader(ReadableStreamBYOBReader value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public ReadableStreamDefaultReader? AsReadableStreamDefaultReader => _kind == 1 ? _value1 : default;

    public ReadableStreamBYOBReader? AsReadableStreamBYOBReader => _kind == 2 ? _value2 : default;

    public static implicit operator ReadableStreamReader(ReadableStreamDefaultReader value)
        => new(value);

    public static implicit operator ReadableStreamReader(ReadableStreamBYOBReader value)
        => new(value);
}

/// <summary>
/// RenderingContext
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RenderingContext : IEither
{
    private readonly byte _kind;
    private readonly CanvasRenderingContext2D? _value1;
    private readonly ImageBitmapRenderingContext? _value2;
    private readonly WebGLRenderingContext? _value3;
    private readonly WebGL2RenderingContext? _value4;
    private readonly GPUCanvasContext? _value5;

    private RenderingContext(CanvasRenderingContext2D value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private RenderingContext(ImageBitmapRenderingContext value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
    }

    private RenderingContext(WebGLRenderingContext value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
    }

    private RenderingContext(WebGL2RenderingContext value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
    }

    private RenderingContext(GPUCanvasContext value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
    }

    public CanvasRenderingContext2D? AsCanvasRenderingContext2D => _kind == 1 ? _value1 : default;

    public ImageBitmapRenderingContext? AsImageBitmapRenderingContext => _kind == 2 ? _value2 : default;

    public WebGLRenderingContext? AsWebGLRenderingContext => _kind == 3 ? _value3 : default;

    public WebGL2RenderingContext? AsWebGL2RenderingContext => _kind == 4 ? _value4 : default;

    public GPUCanvasContext? AsGPUCanvasContext => _kind == 5 ? _value5 : default;

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

/// <summary>
/// ReportEventType
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ReportEventType : IEither
{
    private readonly byte _kind;
    private readonly FenceEvent? _value1;
    private readonly string? _value2;

    private ReportEventType(FenceEvent value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ReportEventType(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public FenceEvent? AsFenceEvent => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ReportEventType(FenceEvent value)
        => new(value);

    public static implicit operator ReportEventType(string value)
        => new(value);
}

/// <summary>
/// RequestInfo
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RequestInfo : IEither
{
    private readonly byte _kind;
    private readonly Request? _value1;
    private readonly string? _value2;

    private RequestInfo(Request value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RequestInfo(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Request? AsRequest => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator RequestInfo(Request value)
        => new(value);

    public static implicit operator RequestInfo(string value)
        => new(value);
}

/// <summary>
/// RotationMatrixType
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RotationMatrixType : IEither
{
    private readonly byte _kind;
    private readonly Float32Array? _value1;
    private readonly Float64Array? _value2;
    private readonly DOMMatrix? _value3;

    private RotationMatrixType(Float32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private RotationMatrixType(Float64Array value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private RotationMatrixType(DOMMatrix value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public Float32Array? AsFloat32Array => _kind == 1 ? _value1 : default;

    public Float64Array? AsFloat64Array => _kind == 2 ? _value2 : default;

    public DOMMatrix? AsDOMMatrix => _kind == 3 ? _value3 : default;

    public static implicit operator RotationMatrixType(Float32Array value)
        => new(value);

    public static implicit operator RotationMatrixType(Float64Array value)
        => new(value);

    public static implicit operator RotationMatrixType(DOMMatrix value)
        => new(value);
}

/// <summary>
/// RoundRectRadii
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RoundRectRadii : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;

    private RoundRectRadii(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RoundRectRadii(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public static implicit operator RoundRectRadii(double value)
        => new(value);

    public static implicit operator RoundRectRadii(DOMPointInit value)
        => new(value);
}

/// <summary>
/// RouterSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct RouterSource : IEither
{
    private readonly byte _kind;
    private readonly RouterSourceDict? _value1;
    private readonly RouterSourceEnum? _value2;

    private RouterSource(RouterSourceDict value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RouterSource(RouterSourceEnum value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RouterSourceDict? AsRouterSourceDict => _kind == 1 ? _value1 : default;

    public RouterSourceEnum? AsRouterSourceEnum => _kind == 2 ? _value2 : default;

    public static implicit operator RouterSource(RouterSourceDict value)
        => new(value);

    public static implicit operator RouterSource(RouterSourceEnum value)
        => new(value);
}

/// <summary>
/// SanitizerAttribute
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SanitizerAttribute : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly SanitizerAttributeNamespace? _value2;

    private SanitizerAttribute(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SanitizerAttribute(SanitizerAttributeNamespace value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public SanitizerAttributeNamespace? AsSanitizerAttributeNamespace => _kind == 2 ? _value2 : default;

    public static implicit operator SanitizerAttribute(string value)
        => new(value);

    public static implicit operator SanitizerAttribute(SanitizerAttributeNamespace value)
        => new(value);
}

/// <summary>
/// SanitizerElement
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SanitizerElement : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly SanitizerElementNamespace? _value2;

    private SanitizerElement(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SanitizerElement(SanitizerElementNamespace value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public SanitizerElementNamespace? AsSanitizerElementNamespace => _kind == 2 ? _value2 : default;

    public static implicit operator SanitizerElement(string value)
        => new(value);

    public static implicit operator SanitizerElement(SanitizerElementNamespace value)
        => new(value);
}

/// <summary>
/// SanitizerElementWithAttributes
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SanitizerElementWithAttributes : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly SanitizerElementNamespaceWithAttributes? _value2;

    private SanitizerElementWithAttributes(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SanitizerElementWithAttributes(SanitizerElementNamespaceWithAttributes value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public SanitizerElementNamespaceWithAttributes? AsSanitizerElementNamespaceWithAttributes => _kind == 2 ? _value2 : default;

    public static implicit operator SanitizerElementWithAttributes(string value)
        => new(value);

    public static implicit operator SanitizerElementWithAttributes(SanitizerElementNamespaceWithAttributes value)
        => new(value);
}

/// <summary>
/// SequenceEffectTiming
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SequenceEffectTiming : IEither
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly EffectTiming? _value2;

    private SequenceEffectTiming(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SequenceEffectTiming(EffectTiming value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public EffectTiming? AsEffectTiming => _kind == 2 ? _value2 : default;

    public static implicit operator SequenceEffectTiming(double value)
        => new(value);

    public static implicit operator SequenceEffectTiming(EffectTiming value)
        => new(value);
}

/// <summary>
/// ShadowAnimationNewTarget
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ShadowAnimationNewTarget : IEither
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly CSSPseudoElement? _value2;

    private ShadowAnimationNewTarget(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ShadowAnimationNewTarget(CSSPseudoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 2 ? _value2 : default;

    public static implicit operator ShadowAnimationNewTarget(Element value)
        => new(value);

    public static implicit operator ShadowAnimationNewTarget(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// SharedStorageResponse
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SharedStorageResponse : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly FencedFrameConfig? _value2;

    private SharedStorageResponse(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SharedStorageResponse(FencedFrameConfig value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public FencedFrameConfig? AsFencedFrameConfig => _kind == 2 ? _value2 : default;

    public static implicit operator SharedStorageResponse(string value)
        => new(value);

    public static implicit operator SharedStorageResponse(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// SharedWorkerOptions
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct SharedWorkerOptions : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly WorkerOptions? _value2;

    private SharedWorkerOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SharedWorkerOptions(WorkerOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public WorkerOptions? AsWorkerOptions => _kind == 2 ? _value2 : default;

    public static implicit operator SharedWorkerOptions(string value)
        => new(value);

    public static implicit operator SharedWorkerOptions(WorkerOptions value)
        => new(value);
}

/// <summary>
/// StartInDirectory
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct StartInDirectory : IEither
{
    private readonly byte _kind;
    private readonly WellKnownDirectory? _value1;
    private readonly FileSystemHandle? _value2;

    private StartInDirectory(WellKnownDirectory value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StartInDirectory(FileSystemHandle value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public WellKnownDirectory? AsWellKnownDirectory => _kind == 1 ? _value1 : default;

    public FileSystemHandle? AsFileSystemHandle => _kind == 2 ? _value2 : default;

    public static implicit operator StartInDirectory(WellKnownDirectory value)
        => new(value);

    public static implicit operator StartInDirectory(FileSystemHandle value)
        => new(value);
}

/// <summary>
/// TaskSignalAnyInitPriority
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TaskSignalAnyInitPriority : IEither
{
    private readonly byte _kind;
    private readonly TaskPriority? _value1;
    private readonly TaskSignal? _value2;

    private TaskSignalAnyInitPriority(TaskPriority value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private TaskSignalAnyInitPriority(TaskSignal value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public TaskPriority? AsTaskPriority => _kind == 1 ? _value1 : default;

    public TaskSignal? AsTaskSignal => _kind == 2 ? _value2 : default;

    public static implicit operator TaskSignalAnyInitPriority(TaskPriority value)
        => new(value);

    public static implicit operator TaskSignalAnyInitPriority(TaskSignal value)
        => new(value);
}

/// <summary>
/// TexImageSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TexImageSource : IEither
{
    private readonly byte _kind;
    private readonly ImageBitmap? _value1;
    private readonly ImageData? _value2;
    private readonly HTMLImageElement? _value3;
    private readonly HTMLCanvasElement? _value4;
    private readonly HTMLVideoElement? _value5;
    private readonly OffscreenCanvas? _value6;
    private readonly VideoFrame? _value7;

    private TexImageSource(ImageBitmap value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
    }

    private TexImageSource(ImageData value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
    }

    private TexImageSource(HTMLImageElement value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = default;
    }

    private TexImageSource(HTMLCanvasElement value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
        _value5 = default;
        _value6 = default;
        _value7 = default;
    }

    private TexImageSource(HTMLVideoElement value)
    {
        _kind = 5;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = value;
        _value6 = default;
        _value7 = default;
    }

    private TexImageSource(OffscreenCanvas value)
    {
        _kind = 6;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = value;
        _value7 = default;
    }

    private TexImageSource(VideoFrame value)
    {
        _kind = 7;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = default;
        _value5 = default;
        _value6 = default;
        _value7 = value;
    }

    public ImageBitmap? AsImageBitmap => _kind == 1 ? _value1 : default;

    public ImageData? AsImageData => _kind == 2 ? _value2 : default;

    public HTMLImageElement? AsHTMLImageElement => _kind == 3 ? _value3 : default;

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 4 ? _value4 : default;

    public HTMLVideoElement? AsHTMLVideoElement => _kind == 5 ? _value5 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 6 ? _value6 : default;

    public VideoFrame? AsVideoFrame => _kind == 7 ? _value7 : default;

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

/// <summary>
/// TimerHandler
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TimerHandler : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly Delegate? _value2;

    private TimerHandler(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private TimerHandler(Delegate value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public Delegate? AsDelegate => _kind == 2 ? _value2 : default;

    public static implicit operator TimerHandler(string value)
        => new(value);

    public static implicit operator TimerHandler(Delegate value)
        => new(value);
}

/// <summary>
/// TrackEventInitTrack
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TrackEventInitTrack : IEither
{
    private readonly byte _kind;
    private readonly VideoTrack? _value1;
    private readonly AudioTrack? _value2;
    private readonly TextTrack? _value3;

    private TrackEventInitTrack(VideoTrack value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private TrackEventInitTrack(AudioTrack value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private TrackEventInitTrack(TextTrack value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public VideoTrack? AsVideoTrack => _kind == 1 ? _value1 : default;

    public AudioTrack? AsAudioTrack => _kind == 2 ? _value2 : default;

    public TextTrack? AsTextTrack => _kind == 3 ? _value3 : default;

    public static implicit operator TrackEventInitTrack(VideoTrack value)
        => new(value);

    public static implicit operator TrackEventInitTrack(AudioTrack value)
        => new(value);

    public static implicit operator TrackEventInitTrack(TextTrack value)
        => new(value);
}

/// <summary>
/// TrackEventTrack
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TrackEventTrack : IEither
{
    private readonly byte _kind;
    private readonly VideoTrack? _value1;
    private readonly AudioTrack? _value2;
    private readonly TextTrack? _value3;

    private TrackEventTrack(VideoTrack value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private TrackEventTrack(AudioTrack value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private TrackEventTrack(TextTrack value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public VideoTrack? AsVideoTrack => _kind == 1 ? _value1 : default;

    public AudioTrack? AsAudioTrack => _kind == 2 ? _value2 : default;

    public TextTrack? AsTextTrack => _kind == 3 ? _value3 : default;

    public static implicit operator TrackEventTrack(VideoTrack value)
        => new(value);

    public static implicit operator TrackEventTrack(AudioTrack value)
        => new(value);

    public static implicit operator TrackEventTrack(TextTrack value)
        => new(value);
}

/// <summary>
/// TrustedType
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct TrustedType : IEither
{
    private readonly byte _kind;
    private readonly TrustedHTML? _value1;
    private readonly TrustedScript? _value2;
    private readonly TrustedScriptURL? _value3;

    private TrustedType(TrustedHTML value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private TrustedType(TrustedScript value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private TrustedType(TrustedScriptURL value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public TrustedHTML? AsTrustedHTML => _kind == 1 ? _value1 : default;

    public TrustedScript? AsTrustedScript => _kind == 2 ? _value2 : default;

    public TrustedScriptURL? AsTrustedScriptURL => _kind == 3 ? _value3 : default;

    public static implicit operator TrustedType(TrustedHTML value)
        => new(value);

    public static implicit operator TrustedType(TrustedScript value)
        => new(value);

    public static implicit operator TrustedType(TrustedScriptURL value)
        => new(value);
}

/// <summary>
/// URLCreateObjectURLObj
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct URLCreateObjectURLObj : IEither
{
    private readonly byte _kind;
    private readonly Blob? _value1;
    private readonly MediaSource? _value2;

    private URLCreateObjectURLObj(Blob value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private URLCreateObjectURLObj(MediaSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Blob? AsBlob => _kind == 1 ? _value1 : default;

    public MediaSource? AsMediaSource => _kind == 2 ? _value2 : default;

    public static implicit operator URLCreateObjectURLObj(Blob value)
        => new(value);

    public static implicit operator URLCreateObjectURLObj(MediaSource value)
        => new(value);
}

/// <summary>
/// URLPatternCompatible
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct URLPatternCompatible : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly URLPatternInit? _value2;
    private readonly URLPattern? _value3;

    private URLPatternCompatible(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private URLPatternCompatible(URLPatternInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private URLPatternCompatible(URLPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public URLPatternInit? AsURLPatternInit => _kind == 2 ? _value2 : default;

    public URLPattern? AsURLPattern => _kind == 3 ? _value3 : default;

    public static implicit operator URLPatternCompatible(string value)
        => new(value);

    public static implicit operator URLPatternCompatible(URLPatternInit value)
        => new(value);

    public static implicit operator URLPatternCompatible(URLPattern value)
        => new(value);
}

/// <summary>
/// URLPatternInput
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct URLPatternInput : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly URLPatternInit? _value2;

    private URLPatternInput(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private URLPatternInput(URLPatternInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public URLPatternInit? AsURLPatternInit => _kind == 2 ? _value2 : default;

    public static implicit operator URLPatternInput(string value)
        => new(value);

    public static implicit operator URLPatternInput(URLPatternInit value)
        => new(value);
}

/// <summary>
/// URLSearchParamsInit
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(URLSearchParamsInitCollectionBuilder), nameof(URLSearchParamsInitCollectionBuilder.Create))]
public readonly struct URLSearchParamsInit : IEither, IEnumerable<string[]>
{
    private readonly byte _kind;
    private readonly string[][]? _value1;
    private readonly Dictionary<string, string>? _value2;
    private readonly string? _value3;

    private URLSearchParamsInit(string[][] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private URLSearchParamsInit(Dictionary<string, string> value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private URLSearchParamsInit(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string[][]? AsStringArrayArray => _kind == 1 ? _value1 : default;

    public Dictionary<string, string>? AsDictionaryStringString => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

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

/// <summary>
/// Uint32List
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Uint32ListCollectionBuilder), nameof(Uint32ListCollectionBuilder.Create))]
public readonly struct Uint32List : IEither, IEnumerable<GLuint>
{
    private readonly byte _kind;
    private readonly Uint32Array? _value1;
    private readonly GLuint[]? _value2;

    private Uint32List(Uint32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private Uint32List(GLuint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Uint32Array? AsUint32Array => _kind == 1 ? _value1 : default;

    public GLuint[]? AsGLuintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// UrnOrConfig
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct UrnOrConfig : IEither
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly FencedFrameConfig? _value2;

    private UrnOrConfig(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private UrnOrConfig(FencedFrameConfig value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public FencedFrameConfig? AsFencedFrameConfig => _kind == 2 ? _value2 : default;

    public static implicit operator UrnOrConfig(string value)
        => new(value);

    public static implicit operator UrnOrConfig(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// VibratePattern
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(VibratePatternCollectionBuilder), nameof(VibratePatternCollectionBuilder.Create))]
public readonly struct VibratePattern : IEither, IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private VibratePattern(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private VibratePattern(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ViewTimelineOptionsInset
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ViewTimelineOptionsInset : IEither
{
    private readonly byte _kind;
    private readonly CSSNumericValue? _value1;
    private readonly CSSKeywordValue? _value2;

    private ViewTimelineOptionsInset(CSSNumericValue value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ViewTimelineOptionsInset(CSSKeywordValue value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CSSNumericValue? AsCSSNumericValue => _kind == 1 ? _value1 : default;

    public CSSKeywordValue? AsCSSKeywordValue => _kind == 2 ? _value2 : default;

    public static implicit operator ViewTimelineOptionsInset(CSSNumericValue value)
        => new(value);

    public static implicit operator ViewTimelineOptionsInset(CSSKeywordValue value)
        => new(value);
}

/// <summary>
/// ViewTimelineOptionsInsetValue
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ViewTimelineOptionsInsetValueCollectionBuilder), nameof(ViewTimelineOptionsInsetValueCollectionBuilder.Create))]
public readonly struct ViewTimelineOptionsInsetValue : IEither, IEnumerable<ViewTimelineOptionsInset>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ViewTimelineOptionsInset[]? _value2;

    private ViewTimelineOptionsInsetValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ViewTimelineOptionsInsetValue(ViewTimelineOptionsInset[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ViewTimelineOptionsInset[]? AsViewTimelineOptionsInsetArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// ViewportMediaStreamConstraintsAudio
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ViewportMediaStreamConstraintsAudio : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private ViewportMediaStreamConstraintsAudio(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ViewportMediaStreamConstraintsAudio(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator ViewportMediaStreamConstraintsAudio(bool value)
        => new(value);

    public static implicit operator ViewportMediaStreamConstraintsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// ViewportMediaStreamConstraintsVideo
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ViewportMediaStreamConstraintsVideo : IEither
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly MediaTrackConstraints? _value2;

    private ViewportMediaStreamConstraintsVideo(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ViewportMediaStreamConstraintsVideo(MediaTrackConstraints value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public MediaTrackConstraints? AsMediaTrackConstraints => _kind == 2 ? _value2 : default;

    public static implicit operator ViewportMediaStreamConstraintsVideo(bool value)
        => new(value);

    public static implicit operator ViewportMediaStreamConstraintsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList : IEither, IEnumerable<GLuint>
{
    private readonly byte _kind;
    private readonly Uint32Array? _value1;
    private readonly GLuint[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Uint32Array? AsUint32Array => _kind == 1 ? _value1 : default;

    public GLuint[]? AsGLuintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList : IEither, IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList : IEither, IEnumerable<GLuint>
{
    private readonly byte _kind;
    private readonly Uint32Array? _value1;
    private readonly GLuint[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(Uint32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList(GLuint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Uint32Array? AsUint32Array => _kind == 1 ? _value1 : default;

    public GLuint[]? AsGLuintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList : IEither, IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList : IEither, IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawArraysWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList : IEither, IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsWEBGLCountsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsWEBGLCountsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawElementsWEBGLCountsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList : IEither, IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// WebGL2RenderingContextCanvas
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct WebGL2RenderingContextCanvas : IEither
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    private WebGL2RenderingContextCanvas(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WebGL2RenderingContextCanvas(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public static implicit operator WebGL2RenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator WebGL2RenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// WebGLRenderingContextCanvas
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct WebGLRenderingContextCanvas : IEither
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    private WebGLRenderingContextCanvas(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WebGLRenderingContextCanvas(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public static implicit operator WebGLRenderingContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator WebGLRenderingContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// WebSocketProtocols
/// </summary>
[ECMAScript]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WebSocketProtocolsCollectionBuilder), nameof(WebSocketProtocolsCollectionBuilder.Create))]
public readonly struct WebSocketProtocols : IEither, IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private WebSocketProtocols(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private WebSocketProtocols(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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

/// <summary>
/// XMLHttpRequestSendBody
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct XMLHttpRequestSendBody : IEither
{
    private readonly byte _kind;
    private readonly Document? _value1;
    private readonly XMLHttpRequestBodyInit? _value2;

    private XMLHttpRequestSendBody(Document value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private XMLHttpRequestSendBody(XMLHttpRequestBodyInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Document? AsDocument => _kind == 1 ? _value1 : default;

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => _kind == 2 ? _value2 : default;

    public static implicit operator XMLHttpRequestSendBody(Document value)
        => new(value);

    public static implicit operator XMLHttpRequestSendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// XRWebGLRenderingContext
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct XRWebGLRenderingContext : IEither
{
    private readonly byte _kind;
    private readonly WebGLRenderingContext? _value1;
    private readonly WebGL2RenderingContext? _value2;

    private XRWebGLRenderingContext(WebGLRenderingContext value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private XRWebGLRenderingContext(WebGL2RenderingContext value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public WebGLRenderingContext? AsWebGLRenderingContext => _kind == 1 ? _value1 : default;

    public WebGL2RenderingContext? AsWebGL2RenderingContext => _kind == 2 ? _value2 : default;

    public static implicit operator XRWebGLRenderingContext(WebGLRenderingContext value)
        => new(value);

    public static implicit operator XRWebGLRenderingContext(WebGL2RenderingContext value)
        => new(value);
}
