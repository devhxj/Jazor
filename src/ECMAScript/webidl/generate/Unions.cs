namespace ECMAScript;

/// <summary>
/// AddBefore
/// </summary>
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

/// <summary>
/// AddEventListenerOptionsValue
/// </summary>
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

/// <summary>
/// AddRoutesRules
/// </summary>
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

/// <summary>
/// AfterNodes
/// </summary>
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

/// <summary>
/// AlgorithmIdentifier
/// </summary>
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

/// <summary>
/// AllowSharedBufferSource
/// </summary>
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

/// <summary>
/// AllowedBluetoothDeviceAllowedServices
/// </summary>
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

/// <summary>
/// AnimateOptions
/// </summary>
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

/// <summary>
/// AppendNodes
/// </summary>
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

/// <summary>
/// ArrayBufferView
/// </summary>
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

/// <summary>
/// AssignNodes
/// </summary>
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

/// <summary>
/// AudioContextOptionsLatencyHint
/// </summary>
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

/// <summary>
/// AudioContextOptionsRenderSizeHint
/// </summary>
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

/// <summary>
/// AudioContextOptionsSinkId
/// </summary>
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

/// <summary>
/// AudioContextSetSinkId
/// </summary>
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

/// <summary>
/// AudioContextSinkId
/// </summary>
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

/// <summary>
/// BackgroundFetchManagerFetchRequests
/// </summary>
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

/// <summary>
/// BasePropertyIndexedKeyframeComposite
/// </summary>
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

/// <summary>
/// BasePropertyIndexedKeyframeEasing
/// </summary>
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

/// <summary>
/// BasePropertyIndexedKeyframeOffset
/// </summary>
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

/// <summary>
/// BeforeNodes
/// </summary>
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

/// <summary>
/// BinaryData
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct BinaryData
{
    private readonly byte _kind;
    private readonly ArrayBuffer? _value1;
    private readonly IArrayBufferView? _value2;

    private BinaryData(ArrayBuffer value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BinaryData(IArrayBufferView value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public ArrayBuffer? AsArrayBuffer => _kind == 1 ? _value1 : default;

    public IArrayBufferView? AsIArrayBufferView => _kind == 2 ? _value2 : default;

    public static implicit operator BinaryData(ArrayBuffer value)
        => new(value);

    public static BinaryData FromIArrayBufferView(IArrayBufferView value)
        => new(value);

    public static implicit operator BinaryData(DataView value)
        => new(value);

    public static implicit operator BinaryData(Uint8Array value)
        => new(value);

    public static implicit operator BinaryData(Uint8ClampedArray value)
        => new(value);

    public static implicit operator BinaryData(Int8Array value)
        => new(value);

    public static implicit operator BinaryData(Int16Array value)
        => new(value);

    public static implicit operator BinaryData(Uint16Array value)
        => new(value);

    public static implicit operator BinaryData(Int32Array value)
        => new(value);

    public static implicit operator BinaryData(Uint32Array value)
        => new(value);

    public static implicit operator BinaryData(Float16Array value)
        => new(value);

    public static implicit operator BinaryData(Float32Array value)
        => new(value);

    public static implicit operator BinaryData(Float64Array value)
        => new(value);

    public static implicit operator BinaryData(BigInt64Array value)
        => new(value);

    public static implicit operator BinaryData(BigUint64Array value)
        => new(value);
}

/// <summary>
/// BlobPart
/// </summary>
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

/// <summary>
/// BluetoothAdvertisingEventInitUUIDs
/// </summary>
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

/// <summary>
/// BluetoothCharacteristicUUID
/// </summary>
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

/// <summary>
/// BluetoothDescriptorUUID
/// </summary>
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

/// <summary>
/// BluetoothServiceUUID
/// </summary>
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

/// <summary>
/// BluetoothUUIDGetCharacteristicName
/// </summary>
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

/// <summary>
/// BluetoothUUIDGetDescriptorName
/// </summary>
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

/// <summary>
/// BluetoothUUIDGetServiceName
/// </summary>
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

/// <summary>
/// BodyInit
/// </summary>
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

/// <summary>
/// BufferSource
/// </summary>
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

/// <summary>
/// CSSFontFeatureValuesMapSetValues
/// </summary>
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

/// <summary>
/// CSSPseudoElementParent
/// </summary>
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

/// <summary>
/// CanvasImageSource
/// </summary>
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

/// <summary>
/// CanvasRenderingContext2DFillStyle
/// </summary>
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

/// <summary>
/// CanvasRenderingContext2DRoundRectRadii
/// </summary>
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

/// <summary>
/// CanvasRenderingContext2DRoundRectRadiiValue
/// </summary>
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

/// <summary>
/// CanvasRenderingContext2DStrokeStyle
/// </summary>
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

/// <summary>
/// CharacterDataAfterNodes
/// </summary>
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

/// <summary>
/// CharacterDataBeforeNodes
/// </summary>
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

/// <summary>
/// CharacterDataReplaceWithNodes
/// </summary>
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

/// <summary>
/// ClipboardItemDataValue
/// </summary>
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

/// <summary>
/// ConstrainBoolean
/// </summary>
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

/// <summary>
/// ConstrainDOMString
/// </summary>
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

/// <summary>
/// ConstrainDOMStringParametersExact
/// </summary>
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

/// <summary>
/// ConstrainDOMStringParametersIdeal
/// </summary>
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

/// <summary>
/// ConstrainDouble
/// </summary>
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

/// <summary>
/// ConstrainPoint2D
/// </summary>
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

/// <summary>
/// ConstrainULong
/// </summary>
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

/// <summary>
/// CreateElementNSOptions
/// </summary>
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

/// <summary>
/// CreateElementOptions
/// </summary>
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

/// <summary>
/// CreateObjectURLObj
/// </summary>
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

/// <summary>
/// CryptoKeyID
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union CryptoKeyID(SmallCryptoKeyID, BigInteger)
{

    public SmallCryptoKeyID? AsSmallCryptoKeyID => Value is SmallCryptoKeyID value ? value : default(SmallCryptoKeyID?);

    public BigInteger? AsBigInteger => Value is BigInteger value ? value : default(BigInteger?);

    public static implicit operator CryptoKeyID(SmallCryptoKeyID value)
        => new(value);

    public static implicit operator CryptoKeyID(BigInteger value)
        => new(value);
}

/// <summary>
/// DOMMatrixInitValue
/// </summary>
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

/// <summary>
/// DOMMatrixReadOnlyInit
/// </summary>
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

/// <summary>
/// DefaultValue
/// </summary>
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

/// <summary>
/// DefaultValueValue
/// </summary>
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

/// <summary>
/// DefaultValueValue2
/// </summary>
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

/// <summary>
/// DefaultValueValue3
/// </summary>
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

/// <summary>
/// DisplayMediaStreamOptionsAudio
/// </summary>
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

/// <summary>
/// DisplayMediaStreamOptionsVideo
/// </summary>
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

/// <summary>
/// DocumentAppendNodes
/// </summary>
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

/// <summary>
/// DocumentCreateElementNSOptions
/// </summary>
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

/// <summary>
/// DocumentCreateElementOptions
/// </summary>
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

/// <summary>
/// DocumentFragmentAppendNodes
/// </summary>
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

/// <summary>
/// DocumentFragmentPrependNodes
/// </summary>
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

/// <summary>
/// DocumentFragmentReplaceChildrenNodes
/// </summary>
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

/// <summary>
/// DocumentPrependNodes
/// </summary>
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

/// <summary>
/// DocumentReplaceChildrenNodes
/// </summary>
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

/// <summary>
/// DocumentStartViewTransitionCallbackOptions
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union DocumentStartViewTransitionCallbackOptions(UpdateCallback, StartViewTransitionOptions)
{

    public UpdateCallback? AsUpdateCallback => Value is UpdateCallback value ? value : default(UpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator DocumentStartViewTransitionCallbackOptions(UpdateCallback value)
        => new(value);

    public static implicit operator DocumentStartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// DocumentTypeAfterNodes
/// </summary>
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

/// <summary>
/// DocumentTypeBeforeNodes
/// </summary>
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

/// <summary>
/// DocumentTypeReplaceWithNodes
/// </summary>
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

/// <summary>
/// EffectTimingDuration
/// </summary>
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

/// <summary>
/// ElementAfterNodes
/// </summary>
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

/// <summary>
/// ElementAnimateOptions
/// </summary>
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

/// <summary>
/// ElementAppendNodes
/// </summary>
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

/// <summary>
/// ElementBeforeNodes
/// </summary>
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

/// <summary>
/// ElementInternalsSetFormValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValue(File, string, FormData)
{

    public File? AsFile => Value is File value ? value : default(File?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ElementInternalsSetFormValueState(File, string, FormData)
{

    public File? AsFile => Value is File value ? value : default(File?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

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

/// <summary>
/// ElementReplaceChildrenNodes
/// </summary>
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

/// <summary>
/// ElementReplaceWithNodes
/// </summary>
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

/// <summary>
/// ElementScrollIntoViewArg
/// </summary>
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

/// <summary>
/// EventListenerValue
/// </summary>
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

/// <summary>
/// EventTargetAddEventListenerOptions
/// </summary>
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

/// <summary>
/// EventTargetRemoveEventListenerOptions
/// </summary>
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

/// <summary>
/// ExtendableMessageEventInitSource
/// </summary>
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

/// <summary>
/// ExtendableMessageEventSource
/// </summary>
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

/// <summary>
/// FencedFrameConfigSize
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FencedFrameConfigSize(uint, OpaqueProperty)
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public OpaqueProperty? AsOpaqueProperty => Value is OpaqueProperty value ? value : default(OpaqueProperty?);

    public static implicit operator FencedFrameConfigSize(uint value)
        => new(value);

    public static implicit operator FencedFrameConfigSize(OpaqueProperty value)
        => new(value);
}

/// <summary>
/// FilePickerAcceptTypeAcceptValue
/// </summary>
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

/// <summary>
/// FileReaderResult
/// </summary>
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

/// <summary>
/// FileSystemWriteChunkType
/// </summary>
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

/// <summary>
/// Float32List
/// </summary>
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

/// <summary>
/// FontFaceSourceValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FontFaceSourceValue(string, BinaryData)
{

    public string? AsString => Value is string value ? value : default(string?);

    public BinaryData? AsBinaryData => Value is BinaryData value ? value : default(BinaryData?);

    public static implicit operator FontFaceSourceValue(string value)
        => new(value);

    public static implicit operator FontFaceSourceValue(BinaryData value)
        => new(value);
}

/// <summary>
/// FormDataEntryValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union FormDataEntryValue(File, string)
{

    public File? AsFile => Value is File value ? value : default(File?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator FormDataEntryValue(File value)
        => new(value);

    public static implicit operator FormDataEntryValue(string value)
        => new(value);
}

/// <summary>
/// GenerateBidOutputAdComponents
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GenerateBidOutputAdComponents(string, AdRender)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AdRender? AsAdRender => Value is AdRender value ? value : default(AdRender?);

    public static implicit operator GenerateBidOutputAdComponents(string value)
        => new(value);

    public static implicit operator GenerateBidOutputAdComponents(AdRender value)
        => new(value);
}

/// <summary>
/// GenerateBidOutputRender
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GenerateBidOutputRender(string, AdRender)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AdRender? AsAdRender => Value is AdRender value ? value : default(AdRender?);

    public static implicit operator GenerateBidOutputRender(string value)
        => new(value);

    public static implicit operator GenerateBidOutputRender(AdRender value)
        => new(value);
}

/// <summary>
/// GeometryNode
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GeometryNode(Text, Element, CSSPseudoElement, Document)
{

    public Text? AsText => Value is Text value ? value : default(Text?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public Document? AsDocument => Value is Document value ? value : default(Document?);

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
/// GetCharacteristicName
/// </summary>
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

/// <summary>
/// GetDescriptorName
/// </summary>
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

/// <summary>
/// GetServiceName
/// </summary>
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

/// <summary>
/// GroupEffectTiming
/// </summary>
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

/// <summary>
/// HTMLAllCollectionItemResult
/// </summary>
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

/// <summary>
/// HTMLAllCollectionNamedItemResult
/// </summary>
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

/// <summary>
/// HTMLElementHidden
/// </summary>
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

/// <summary>
/// HTMLFormControlsCollectionNamedItemResult
/// </summary>
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

/// <summary>
/// HTMLFormElementResult
/// </summary>
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

/// <summary>
/// HTMLOptionsCollectionAddBefore
/// </summary>
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

/// <summary>
/// HTMLOptionsCollectionAddElement
/// </summary>
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

/// <summary>
/// HTMLOrSVGImageElement
/// </summary>
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

/// <summary>
/// HTMLOrSVGScriptElement
/// </summary>
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

/// <summary>
/// HTMLSelectElementAddBefore
/// </summary>
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

/// <summary>
/// HTMLSelectElementAddElement
/// </summary>
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

/// <summary>
/// HTMLSlotElementAssignNodes
/// </summary>
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

/// <summary>
/// HeadersInit
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(HeadersInitCollectionBuilder), nameof(HeadersInitCollectionBuilder.Create))]
public readonly union HeadersInit(byte[][][], Dictionary<byte[], byte[]>) : IEnumerable<byte[][]>
{

    public byte[][][]? AsByteArrayArrayArray => Value is byte[][][] value ? value : default(byte[][][]?);

    public Dictionary<byte[], byte[]>? AsDictionaryByteArrayByteArray => Value is Dictionary<byte[], byte[]> value ? value : default(Dictionary<byte[], byte[]>?);

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
/// IDBCursorSource
/// </summary>
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

/// <summary>
/// IDBDatabaseTransactionStoreNames
/// </summary>
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

/// <summary>
/// IDBObjectStoreCreateIndexKeyPath
/// </summary>
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

/// <summary>
/// IDBObjectStoreParametersKeyPath
/// </summary>
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

/// <summary>
/// IDBRequestSource
/// </summary>
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

/// <summary>
/// ImageBitmapRenderingContextCanvas
/// </summary>
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

/// <summary>
/// ImageBitmapSource
/// </summary>
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

/// <summary>
/// ImageBufferSource
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct ImageBufferSource
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly ReadableStream? _value2;

    private ImageBufferSource(IBufferSource value)
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

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public ReadableStream? AsReadableStream => _kind == 2 ? _value2 : default;

    public static ImageBufferSource FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator ImageBufferSource(ArrayBuffer value)
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

/// <summary>
/// InstallEventAddRoutesRules
/// </summary>
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

/// <summary>
/// Int32List
/// </summary>
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

/// <summary>
/// IntersectionObserverInitRoot
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverInitRoot(Element, Document)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public static implicit operator IntersectionObserverInitRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverInitRoot(Document value)
        => new(value);
}

/// <summary>
/// IntersectionObserverInitThreshold
/// </summary>
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

/// <summary>
/// IntersectionObserverRoot
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union IntersectionObserverRoot(Element, Document)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public static implicit operator IntersectionObserverRoot(Element value)
        => new(value);

    public static implicit operator IntersectionObserverRoot(Document value)
        => new(value);
}

/// <summary>
/// KeyframeAnimationOptionsRangeEnd
/// </summary>
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

/// <summary>
/// KeyframeAnimationOptionsRangeStart
/// </summary>
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

/// <summary>
/// KeyframeEffectOptionsValue
/// </summary>
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

/// <summary>
/// LineAndPositionSetting
/// </summary>
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

/// <summary>
/// MLGraphBuilderSplitSplits
/// </summary>
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

/// <summary>
/// MediaProvider
/// </summary>
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

/// <summary>
/// MediaStreamConstraintsAudio
/// </summary>
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

/// <summary>
/// MediaStreamConstraintsVideo
/// </summary>
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

/// <summary>
/// MediaTrackConstraintSetPan
/// </summary>
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

/// <summary>
/// MediaTrackConstraintSetTilt
/// </summary>
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

/// <summary>
/// MediaTrackConstraintSetZoom
/// </summary>
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

/// <summary>
/// MessageEventSource
/// </summary>
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

/// <summary>
/// NDEFMessageSource
/// </summary>
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

/// <summary>
/// NavigatorRunAdAuctionResultValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union NavigatorRunAdAuctionResultValue(string, FencedFrameConfig)
{

    public string? AsString => Value is string value ? value : default(string?);

    public FencedFrameConfig? AsFencedFrameConfig => Value is FencedFrameConfig value ? value : default(FencedFrameConfig?);

    public static implicit operator NavigatorRunAdAuctionResultValue(string value)
        => new(value);

    public static implicit operator NavigatorRunAdAuctionResultValue(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// NodeFilterValue
/// </summary>
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

/// <summary>
/// OfflineAudioContextOptionsRenderSizeHint
/// </summary>
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

/// <summary>
/// OffscreenCanvasRenderingContext2DFillStyle
/// </summary>
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

/// <summary>
/// OffscreenCanvasRenderingContext2DRoundRectRadii
/// </summary>
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

/// <summary>
/// OffscreenCanvasRenderingContext2DRoundRectRadiiValue
/// </summary>
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

/// <summary>
/// OffscreenCanvasRenderingContext2DStrokeStyle
/// </summary>
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

/// <summary>
/// OffscreenRenderingContext
/// </summary>
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

/// <summary>
/// OptionalEffectTimingDuration
/// </summary>
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

/// <summary>
/// ParameterCurrentTarget
/// </summary>
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

/// <summary>
/// ParameterEvent
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ParameterEvent(Event, string)
{

    public Event? AsEvent => Value is Event value ? value : default(Event?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator ParameterEvent(Event value)
        => new(value);

    public static implicit operator ParameterEvent(string value)
        => new(value);
}

/// <summary>
/// PasswordCredentialInit
/// </summary>
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

/// <summary>
/// Path2DPath
/// </summary>
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

/// <summary>
/// Path2DRoundRectRadii
/// </summary>
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

/// <summary>
/// Path2DRoundRectRadiiValue
/// </summary>
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

/// <summary>
/// PerformanceMeasureOptionsEnd
/// </summary>
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

/// <summary>
/// PerformanceMeasureOptionsStart
/// </summary>
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

/// <summary>
/// PerformanceMeasureStartOrMeasureOptions
/// </summary>
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

/// <summary>
/// PrependNodes
/// </summary>
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

/// <summary>
/// PushMessageDataInit
/// </summary>
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

/// <summary>
/// PushSubscriptionOptionsInitApplicationServerKey
/// </summary>
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

/// <summary>
/// RTCIceServerUrls
/// </summary>
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

/// <summary>
/// RTCPeerConnectionAddTransceiverTrackOrKind
/// </summary>
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

/// <summary>
/// RTCRtpTransform
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union RTCRtpTransform(SFrameTransform, RTCRtpScriptTransform)
{

    public SFrameTransform? AsSFrameTransform => Value is SFrameTransform value ? value : default(SFrameTransform?);

    public RTCRtpScriptTransform? AsRTCRtpScriptTransform => Value is RTCRtpScriptTransform value ? value : default(RTCRtpScriptTransform?);

    public static implicit operator RTCRtpTransform(SFrameTransform value)
        => new(value);

    public static implicit operator RTCRtpTransform(RTCRtpScriptTransform value)
        => new(value);
}

/// <summary>
/// ReadableStreamController
/// </summary>
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

/// <summary>
/// ReadableStreamReader
/// </summary>
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

/// <summary>
/// RemoveEventListenerOptions
/// </summary>
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

/// <summary>
/// RenderingContext
/// </summary>
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

/// <summary>
/// ReplaceChildrenNodes
/// </summary>
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

/// <summary>
/// ReplaceWithNodes
/// </summary>
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

/// <summary>
/// ReportEventType
/// </summary>
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

/// <summary>
/// RequestInfo
/// </summary>
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

/// <summary>
/// RotationMatrixType
/// </summary>
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

/// <summary>
/// RoundRectRadii
/// </summary>
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

/// <summary>
/// RoundRectRadiiValue
/// </summary>
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

/// <summary>
/// RouterSource
/// </summary>
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

/// <summary>
/// SanitizerAttribute
/// </summary>
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

/// <summary>
/// SanitizerElement
/// </summary>
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

/// <summary>
/// SanitizerElementWithAttributes
/// </summary>
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

/// <summary>
/// ScrollIntoViewArg
/// </summary>
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

/// <summary>
/// SendBody
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SendBody(Document, XMLHttpRequestBodyInit)
{

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator SendBody(Document value)
        => new(value);

    public static implicit operator SendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// SendData
/// </summary>
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

/// <summary>
/// SequenceEffectTiming
/// </summary>
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

/// <summary>
/// SetFormValueState
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SetFormValueState(File, string, FormData)
{

    public File? AsFile => Value is File value ? value : default(File?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator SetFormValueState(File value)
        => new(value);

    public static implicit operator SetFormValueState(string value)
        => new(value);

    public static implicit operator SetFormValueState(FormData value)
        => new(value);
}

/// <summary>
/// SetSinkId
/// </summary>
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

/// <summary>
/// SetValues
/// </summary>
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

/// <summary>
/// ShadowAnimationNewTarget
/// </summary>
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

/// <summary>
/// SharedStorageResponse
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedStorageResponse(string, FencedFrameConfig)
{

    public string? AsString => Value is string value ? value : default(string?);

    public FencedFrameConfig? AsFencedFrameConfig => Value is FencedFrameConfig value ? value : default(FencedFrameConfig?);

    public static implicit operator SharedStorageResponse(string value)
        => new(value);

    public static implicit operator SharedStorageResponse(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// SharedWorkerOptions
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union SharedWorkerOptions(string, WorkerOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public WorkerOptions? AsWorkerOptions => Value is WorkerOptions value ? value : default(WorkerOptions?);

    public static implicit operator SharedWorkerOptions(string value)
        => new(value);

    public static implicit operator SharedWorkerOptions(WorkerOptions value)
        => new(value);
}

/// <summary>
/// StartInDirectory
/// </summary>
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

/// <summary>
/// StartViewTransitionCallbackOptions
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StartViewTransitionCallbackOptions(UpdateCallback, StartViewTransitionOptions)
{

    public UpdateCallback? AsUpdateCallback => Value is UpdateCallback value ? value : default(UpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator StartViewTransitionCallbackOptions(UpdateCallback value)
        => new(value);

    public static implicit operator StartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// StructuralCache
/// </summary>
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

/// <summary>
/// StructuralCacheValue
/// </summary>
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

/// <summary>
/// StructuralCacheValue10
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue10(bool, ScrollIntoViewOptions)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => Value is ScrollIntoViewOptions value ? value : default(ScrollIntoViewOptions?);

    public static implicit operator StructuralCacheValue10(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue10(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue11
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue11(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator StructuralCacheValue11(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue11(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue12
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue12(UpdateCallback, StartViewTransitionOptions)
{

    public UpdateCallback? AsUpdateCallback => Value is UpdateCallback value ? value : default(UpdateCallback?);

    public StartViewTransitionOptions? AsStartViewTransitionOptions => Value is StartViewTransitionOptions value ? value : default(StartViewTransitionOptions?);

    public static implicit operator StructuralCacheValue12(UpdateCallback value)
        => new(value);

    public static implicit operator StructuralCacheValue12(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue13
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue13(string, ElementCreationOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ElementCreationOptions? AsElementCreationOptions => Value is ElementCreationOptions value ? value : default(ElementCreationOptions?);

    public static implicit operator StructuralCacheValue13(string value)
        => new(value);

    public static implicit operator StructuralCacheValue13(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue14
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue14(bool, double, string)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public double? AsDouble => Value is double value ? value : default(double?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue14(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue14(double value)
        => new(value);

    public static implicit operator StructuralCacheValue14(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue15
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue15(AddEventListenerOptions, bool)
{

    public AddEventListenerOptions? AsAddEventListenerOptions => Value is AddEventListenerOptions value ? value : default(AddEventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator StructuralCacheValue15(AddEventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue15(bool value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue16
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue16(EventListenerOptions, bool)
{

    public EventListenerOptions? AsEventListenerOptions => Value is EventListenerOptions value ? value : default(EventListenerOptions?);

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public static implicit operator StructuralCacheValue16(EventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue16(bool value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue17
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue17(string, PerformanceMeasureOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public PerformanceMeasureOptions? AsPerformanceMeasureOptions => Value is PerformanceMeasureOptions value ? value : default(PerformanceMeasureOptions?);

    public static implicit operator StructuralCacheValue17(string value)
        => new(value);

    public static implicit operator StructuralCacheValue17(PerformanceMeasureOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue18
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue18(string, ArrayBuffer)
{

    public string? AsString => Value is string value ? value : default(string?);

    public ArrayBuffer? AsArrayBuffer => Value is ArrayBuffer value ? value : default(ArrayBuffer?);

    public static implicit operator StructuralCacheValue18(string value)
        => new(value);

    public static implicit operator StructuralCacheValue18(ArrayBuffer value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue19
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue19(Blob, MediaSource)
{

    public Blob? AsBlob => Value is Blob value ? value : default(Blob?);

    public MediaSource? AsMediaSource => Value is MediaSource value ? value : default(MediaSource?);

    public static implicit operator StructuralCacheValue19(Blob value)
        => new(value);

    public static implicit operator StructuralCacheValue19(MediaSource value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue2
/// </summary>
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

/// <summary>
/// StructuralCacheValue20
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue20CollectionBuilder), nameof(StructuralCacheValue20CollectionBuilder.Create))]
public readonly union StructuralCacheValue20(string, double[]) : IEnumerable<double>
{

    public string? AsString => Value is string value ? value : default(string?);

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public static implicit operator StructuralCacheValue20(string value)
        => new(value);

    public static implicit operator StructuralCacheValue20(double[] value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue20CollectionBuilder
{
    public static StructuralCacheValue20 Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue21
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue21(HTMLCollection, Element)
{

    public HTMLCollection? AsHTMLCollection => Value is HTMLCollection value ? value : default(HTMLCollection?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue21(HTMLCollection value)
        => new(value);

    public static implicit operator StructuralCacheValue21(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue22
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue22(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue22(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue22(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue23
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue23(HTMLOptionElement, HTMLOptGroupElement)
{

    public HTMLOptionElement? AsHTMLOptionElement => Value is HTMLOptionElement value ? value : default(HTMLOptionElement?);

    public HTMLOptGroupElement? AsHTMLOptGroupElement => Value is HTMLOptGroupElement value ? value : default(HTMLOptGroupElement?);

    public static implicit operator StructuralCacheValue23(HTMLOptionElement value)
        => new(value);

    public static implicit operator StructuralCacheValue23(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue24
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue24(HTMLElement, int)
{

    public HTMLElement? AsHTMLElement => Value is HTMLElement value ? value : default(HTMLElement?);

    public int? AsInt => Value is int value ? value : default(int?);

    public static implicit operator StructuralCacheValue24(HTMLElement value)
        => new(value);

    public static implicit operator StructuralCacheValue24(int value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue25
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue25(VideoTrack, AudioTrack, TextTrack)
{

    public VideoTrack? AsVideoTrack => Value is VideoTrack value ? value : default(VideoTrack?);

    public AudioTrack? AsAudioTrack => Value is AudioTrack value ? value : default(AudioTrack?);

    public TextTrack? AsTextTrack => Value is TextTrack value ? value : default(TextTrack?);

    public static implicit operator StructuralCacheValue25(VideoTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue25(AudioTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue25(TextTrack value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue26
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue26(RadioNodeList, Element)
{

    public RadioNodeList? AsRadioNodeList => Value is RadioNodeList value ? value : default(RadioNodeList?);

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public static implicit operator StructuralCacheValue26(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue26(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue27
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue27(Element, Text)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Text? AsText => Value is Text value ? value : default(Text?);

    public static implicit operator StructuralCacheValue27(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue27(Text value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue28
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue28(Path2D, string)
{

    public Path2D? AsPath2D => Value is Path2D value ? value : default(Path2D?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue28(Path2D value)
        => new(value);

    public static implicit operator StructuralCacheValue28(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue29
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue29(File, string, FormData)
{

    public File? AsFile => Value is File value ? value : default(File?);

    public string? AsString => Value is string value ? value : default(string?);

    public FormData? AsFormData => Value is FormData value ? value : default(FormData?);

    public static implicit operator StructuralCacheValue29(File value)
        => new(value);

    public static implicit operator StructuralCacheValue29(string value)
        => new(value);

    public static implicit operator StructuralCacheValue29(FormData value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue3
/// </summary>
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

/// <summary>
/// StructuralCacheValue30
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue30(string, WorkerOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public WorkerOptions? AsWorkerOptions => Value is WorkerOptions value ? value : default(WorkerOptions?);

    public static implicit operator StructuralCacheValue30(string value)
        => new(value);

    public static implicit operator StructuralCacheValue30(WorkerOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue31
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue31
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;
    private readonly IDBCursor? _value3;

    private StructuralCacheValue31(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue31(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue31(IDBCursor value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public IDBCursor? AsIDBCursor => _kind == 3 ? _value3 : default;

    public static StructuralCacheValue31 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static StructuralCacheValue31 FromIDBIndex(IDBIndex value)
        => new(value);

    public static StructuralCacheValue31 FromIDBCursor(IDBCursor value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue32
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue32CollectionBuilder), nameof(StructuralCacheValue32CollectionBuilder.Create))]
public readonly union StructuralCacheValue32(string, string[]) : IEnumerable<string>
{

    public string? AsString => Value is string value ? value : default(string?);

    public string[]? AsStringArray => Value is string[] value ? value : default(string[]?);

    public static implicit operator StructuralCacheValue32(string value)
        => new(value);

    public static implicit operator StructuralCacheValue32(string[] value)
        => new(value);

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
        => ((IEnumerable<string>)(AsStringArray ?? Array.Empty<string>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue32CollectionBuilder
{
    public static StructuralCacheValue32 Create(ReadOnlySpan<string> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue33
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue33
{
    private readonly byte _kind;
    private readonly IDBObjectStore? _value1;
    private readonly IDBIndex? _value2;

    private StructuralCacheValue33(IDBObjectStore value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue33(IDBIndex value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IDBObjectStore? AsIDBObjectStore => _kind == 1 ? _value1 : default;

    public IDBIndex? AsIDBIndex => _kind == 2 ? _value2 : default;

    public static StructuralCacheValue33 FromIDBObjectStore(IDBObjectStore value)
        => new(value);

    public static StructuralCacheValue33 FromIDBIndex(IDBIndex value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue34
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue34(Element, Document)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public static implicit operator StructuralCacheValue34(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue34(Document value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue35
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue35CollectionBuilder), nameof(StructuralCacheValue35CollectionBuilder.Create))]
public readonly union StructuralCacheValue35(RouterRule, RouterRule[]) : IEnumerable<RouterRule>
{

    public RouterRule? AsRouterRule => Value is RouterRule value ? value : default(RouterRule?);

    public RouterRule[]? AsRouterRuleArray => Value is RouterRule[] value ? value : default(RouterRule[]?);

    public static implicit operator StructuralCacheValue35(RouterRule value)
        => new(value);

    public static implicit operator StructuralCacheValue35(RouterRule[] value)
        => new(value);

    IEnumerator<RouterRule> IEnumerable<RouterRule>.GetEnumerator()
        => ((IEnumerable<RouterRule>)(AsRouterRuleArray ?? Array.Empty<RouterRule>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<RouterRule>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue35CollectionBuilder
{
    public static StructuralCacheValue35 Create(ReadOnlySpan<RouterRule> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue36
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue36(Client, ServiceWorker, MessagePort)
{

    public Client? AsClient => Value is Client value ? value : default(Client?);

    public ServiceWorker? AsServiceWorker => Value is ServiceWorker value ? value : default(ServiceWorker?);

    public MessagePort? AsMessagePort => Value is MessagePort value ? value : default(MessagePort?);

    public static implicit operator StructuralCacheValue36(Client value)
        => new(value);

    public static implicit operator StructuralCacheValue36(ServiceWorker value)
        => new(value);

    public static implicit operator StructuralCacheValue36(MessagePort value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue37
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue37(Element, CSSPseudoElement)
{

    public Element? AsElement => Value is Element value ? value : default(Element?);

    public CSSPseudoElement? AsCSSPseudoElement => Value is CSSPseudoElement value ? value : default(CSSPseudoElement?);

    public static implicit operator StructuralCacheValue37(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue37(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue38
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue38CollectionBuilder), nameof(StructuralCacheValue38CollectionBuilder.Create))]
public readonly union StructuralCacheValue38(string[][], Dictionary<string, string>, string) : IEnumerable<string[]>
{

    public string[][]? AsStringArrayArray => Value is string[][] value ? value : default(string[][]?);

    public Dictionary<string, string>? AsDictionaryStringString => Value is Dictionary<string, string> value ? value : default(Dictionary<string, string>?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue38(string[][] value)
        => new(value);

    public static implicit operator StructuralCacheValue38(Dictionary<string, string> value)
        => new(value);

    public static implicit operator StructuralCacheValue38(string value)
        => new(value);

    IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
        => ((IEnumerable<string[]>)(AsStringArrayArray ?? Array.Empty<string[]>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<string[]>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue38CollectionBuilder
{
    public static StructuralCacheValue38 Create(ReadOnlySpan<string[]> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue39
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue39(double, KeyframeEffectOptions)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public KeyframeEffectOptions? AsKeyframeEffectOptions => Value is KeyframeEffectOptions value ? value : default(KeyframeEffectOptions?);

    public static implicit operator StructuralCacheValue39(double value)
        => new(value);

    public static implicit operator StructuralCacheValue39(KeyframeEffectOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue4
/// </summary>
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

/// <summary>
/// StructuralCacheValue40
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue40(double, EffectTiming)
{

    public double? AsDouble => Value is double value ? value : default(double?);

    public EffectTiming? AsEffectTiming => Value is EffectTiming value ? value : default(EffectTiming?);

    public static implicit operator StructuralCacheValue40(double value)
        => new(value);

    public static implicit operator StructuralCacheValue40(EffectTiming value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue41
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue41(string, uint)
{

    public string? AsString => Value is string value ? value : default(string?);

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public static implicit operator StructuralCacheValue41(string value)
        => new(value);

    public static implicit operator StructuralCacheValue41(uint value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue42
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue42(string, AudioSinkInfo)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkInfo? AsAudioSinkInfo => Value is AudioSinkInfo value ? value : default(AudioSinkInfo?);

    public static implicit operator StructuralCacheValue42(string value)
        => new(value);

    public static implicit operator StructuralCacheValue42(AudioSinkInfo value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue43
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue43(string, AudioSinkOptions)
{

    public string? AsString => Value is string value ? value : default(string?);

    public AudioSinkOptions? AsAudioSinkOptions => Value is AudioSinkOptions value ? value : default(AudioSinkOptions?);

    public static implicit operator StructuralCacheValue43(string value)
        => new(value);

    public static implicit operator StructuralCacheValue43(AudioSinkOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue44
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue44
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly JsonWebKey? _value2;

    private StructuralCacheValue44(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue44(JsonWebKey value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public JsonWebKey? AsJsonWebKey => _kind == 2 ? _value2 : default;

    public static StructuralCacheValue44 FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator StructuralCacheValue44(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValue44(DataView value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Uint8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Uint8ClampedArray value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Int8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Int16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Uint16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Float16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Float32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(Float64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(BigInt64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(BigUint64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue44(JsonWebKey value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue45
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue45CollectionBuilder), nameof(StructuralCacheValue45CollectionBuilder.Create))]
public readonly union StructuralCacheValue45(Int32Array, GLint[]) : IEnumerable<GLint>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLint[]? AsGLintArray => Value is GLint[] value ? value : default(GLint[]?);

    public static implicit operator StructuralCacheValue45(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue45(GLint[] value)
        => new(value);

    IEnumerator<GLint> IEnumerable<GLint>.GetEnumerator()
        => ((IEnumerable<GLint>)(AsGLintArray ?? Array.Empty<GLint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue45CollectionBuilder
{
    public static StructuralCacheValue45 Create(ReadOnlySpan<GLint> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue46
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue46CollectionBuilder), nameof(StructuralCacheValue46CollectionBuilder.Create))]
public readonly union StructuralCacheValue46(Int32Array, GLsizei[]) : IEnumerable<GLsizei>
{

    public Int32Array? AsInt32Array => Value is Int32Array value ? value : default(Int32Array?);

    public GLsizei[]? AsGLsizeiArray => Value is GLsizei[] value ? value : default(GLsizei[]?);

    public static implicit operator StructuralCacheValue46(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue46(GLsizei[] value)
        => new(value);

    IEnumerator<GLsizei> IEnumerable<GLsizei>.GetEnumerator()
        => ((IEnumerable<GLsizei>)(AsGLsizeiArray ?? Array.Empty<GLsizei>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLsizei>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue46CollectionBuilder
{
    public static StructuralCacheValue46 Create(ReadOnlySpan<GLsizei> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue47
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue47CollectionBuilder), nameof(StructuralCacheValue47CollectionBuilder.Create))]
public readonly union StructuralCacheValue47(Uint32Array, GLuint[]) : IEnumerable<GLuint>
{

    public Uint32Array? AsUint32Array => Value is Uint32Array value ? value : default(Uint32Array?);

    public GLuint[]? AsGLuintArray => Value is GLuint[] value ? value : default(GLuint[]?);

    public static implicit operator StructuralCacheValue47(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue47(GLuint[] value)
        => new(value);

    IEnumerator<GLuint> IEnumerable<GLuint>.GetEnumerator()
        => ((IEnumerable<GLuint>)(AsGLuintArray ?? Array.Empty<GLuint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GLuint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue47CollectionBuilder
{
    public static StructuralCacheValue47 Create(ReadOnlySpan<GLuint> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue48
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue48CollectionBuilder), nameof(StructuralCacheValue48CollectionBuilder.Create))]
public readonly union StructuralCacheValue48(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator StructuralCacheValue48(uint value)
        => new(value);

    public static implicit operator StructuralCacheValue48(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue48CollectionBuilder
{
    public static StructuralCacheValue48 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValue49
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue49(MediaStreamTrack, string)
{

    public MediaStreamTrack? AsMediaStreamTrack => Value is MediaStreamTrack value ? value : default(MediaStreamTrack?);

    public string? AsString => Value is string value ? value : default(string?);

    public static implicit operator StructuralCacheValue49(MediaStreamTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue49(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue5
/// </summary>
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

/// <summary>
/// StructuralCacheValue50
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly struct StructuralCacheValue50
{
    private readonly byte _kind;
    private readonly IBufferSource? _value1;
    private readonly Blob? _value2;
    private readonly string? _value3;

    private StructuralCacheValue50(IBufferSource value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue50(Blob value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue50(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public IBufferSource? AsIBufferSource => _kind == 1 ? _value1 : default;

    public Blob? AsBlob => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

    public static StructuralCacheValue50 FromIBufferSource(IBufferSource value)
        => new(value);

    public static implicit operator StructuralCacheValue50(ArrayBuffer value)
        => new(value);

    public static implicit operator StructuralCacheValue50(DataView value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Uint8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Uint8ClampedArray value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Int8Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Int16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Uint16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Int32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Uint32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Float16Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Float32Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Float64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(BigInt64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(BigUint64Array value)
        => new(value);

    public static implicit operator StructuralCacheValue50(Blob value)
        => new(value);

    public static implicit operator StructuralCacheValue50(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue6
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue6(Document, XMLHttpRequestBodyInit)
{

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator StructuralCacheValue6(Document value)
        => new(value);

    public static implicit operator StructuralCacheValue6(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue7
/// </summary>
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

/// <summary>
/// StructuralCacheValue8
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValue8(string, BinaryData)
{

    public string? AsString => Value is string value ? value : default(string?);

    public BinaryData? AsBinaryData => Value is BinaryData value ? value : default(BinaryData?);

    public static implicit operator StructuralCacheValue8(string value)
        => new(value);

    public static implicit operator StructuralCacheValue8(BinaryData value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue9
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue9CollectionBuilder), nameof(StructuralCacheValue9CollectionBuilder.Create))]
public readonly union StructuralCacheValue9(uint, uint[]) : IEnumerable<uint>
{

    public uint? AsUint => Value is uint value ? value : default(uint?);

    public uint[]? AsUintArray => Value is uint[] value ? value : default(uint[]?);

    public static implicit operator StructuralCacheValue9(uint value)
        => new(value);

    public static implicit operator StructuralCacheValue9(uint[] value)
        => new(value);

    IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        => ((IEnumerable<uint>)(AsUintArray ?? Array.Empty<uint>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<uint>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class StructuralCacheValue9CollectionBuilder
{
    public static StructuralCacheValue9 Create(ReadOnlySpan<uint> items)
        => items.ToArray();
}

/// <summary>
/// StructuralCacheValueValue
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCacheValueValue(string, FencedFrameConfig)
{

    public string? AsString => Value is string value ? value : default(string?);

    public FencedFrameConfig? AsFencedFrameConfig => Value is FencedFrameConfig value ? value : default(FencedFrameConfig?);

    public static implicit operator StructuralCacheValueValue(string value)
        => new(value);

    public static implicit operator StructuralCacheValueValue(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// SubtleCryptoImportKeyKeyData
/// </summary>
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

/// <summary>
/// TaskSignalAnyInitPriority
/// </summary>
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

/// <summary>
/// TexImageSource
/// </summary>
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

/// <summary>
/// TimerHandler
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union TimerHandler(string, Delegate)
{

    public string? AsString => Value is string value ? value : default(string?);

    public Delegate? AsDelegate => Value is Delegate value ? value : default(Delegate?);

    public static implicit operator TimerHandler(string value)
        => new(value);

    public static implicit operator TimerHandler(Delegate value)
        => new(value);
}

/// <summary>
/// TrackEventInitTrack
/// </summary>
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

/// <summary>
/// TrackEventTrack
/// </summary>
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

/// <summary>
/// TrustedType
/// </summary>
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

/// <summary>
/// URLCreateObjectURLObj
/// </summary>
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

/// <summary>
/// URLPatternCompatible
/// </summary>
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

/// <summary>
/// URLPatternInput
/// </summary>
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

/// <summary>
/// URLSearchParamsInit
/// </summary>
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

/// <summary>
/// Uint32List
/// </summary>
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

/// <summary>
/// UrnOrConfig
/// </summary>
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

/// <summary>
/// VibratePattern
/// </summary>
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

/// <summary>
/// ViewTimelineOptionsInset
/// </summary>
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

/// <summary>
/// ViewTimelineOptionsInsetValue
/// </summary>
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

/// <summary>
/// ViewportMediaStreamConstraintsAudio
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ViewportMediaStreamConstraintsAudio(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator ViewportMediaStreamConstraintsAudio(bool value)
        => new(value);

    public static implicit operator ViewportMediaStreamConstraintsAudio(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// ViewportMediaStreamConstraintsVideo
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union ViewportMediaStreamConstraintsVideo(bool, MediaTrackConstraints)
{

    public bool? AsBool => Value is bool value ? value : default(bool?);

    public MediaTrackConstraints? AsMediaTrackConstraints => Value is MediaTrackConstraints value ? value : default(MediaTrackConstraints?);

    public static implicit operator ViewportMediaStreamConstraintsVideo(bool value)
        => new(value);

    public static implicit operator ViewportMediaStreamConstraintsVideo(MediaTrackConstraints value)
        => new(value);
}

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsWEBGLCountsList
/// </summary>
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

/// <summary>
/// WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList
/// </summary>
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

/// <summary>
/// WebGL2RenderingContextCanvas
/// </summary>
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

/// <summary>
/// WebGLRenderingContextCanvas
/// </summary>
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

/// <summary>
/// WebSocketProtocols
/// </summary>
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

/// <summary>
/// WebSocketSendData
/// </summary>
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

/// <summary>
/// WriteParamsData
/// </summary>
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

/// <summary>
/// XMLHttpRequestBodyInit
/// </summary>
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

/// <summary>
/// XMLHttpRequestSendBody
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union XMLHttpRequestSendBody(Document, XMLHttpRequestBodyInit)
{

    public Document? AsDocument => Value is Document value ? value : default(Document?);

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => Value is XMLHttpRequestBodyInit value ? value : default(XMLHttpRequestBodyInit?);

    public static implicit operator XMLHttpRequestSendBody(Document value)
        => new(value);

    public static implicit operator XMLHttpRequestSendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// XPathNSResolverValue
/// </summary>
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

/// <summary>
/// XRWebGLRenderingContext
/// </summary>
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
