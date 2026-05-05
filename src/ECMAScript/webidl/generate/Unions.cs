namespace ECMAScript;

/// <summary>
/// AddBefore
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AddBefore
{
    private readonly byte _kind;
    private readonly HTMLElement? _value1;
    private readonly int? _value2;

    private AddBefore(HTMLElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AddBefore(int value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLElement? AsHTMLElement => _kind == 1 ? _value1 : default;

    public int? AsInt => _kind == 2 ? _value2 : default;

    public static implicit operator AddBefore(HTMLElement value)
        => new(value);

    public static implicit operator AddBefore(int value)
        => new(value);
}

/// <summary>
/// AddEventListenerOptionsValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AddEventListenerOptionsValue
{
    private readonly byte _kind;
    private readonly AddEventListenerOptions? _value1;
    private readonly bool? _value2;

    private AddEventListenerOptionsValue(AddEventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AddEventListenerOptionsValue(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AddEventListenerOptions? AsAddEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator AddEventListenerOptionsValue(AddEventListenerOptions value)
        => new(value);

    public static implicit operator AddEventListenerOptionsValue(bool value)
        => new(value);
}

/// <summary>
/// AddRoutesRules
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AddRoutesRulesCollectionBuilder), nameof(AddRoutesRulesCollectionBuilder.Create))]
public readonly struct AddRoutesRules : IEnumerable<RouterRule>
{
    private readonly byte _kind;
    private readonly RouterRule? _value1;
    private readonly RouterRule[]? _value2;

    private AddRoutesRules(RouterRule value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AddRoutesRules(RouterRule[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RouterRule? AsRouterRule => _kind == 1 ? _value1 : default;

    public RouterRule[]? AsRouterRuleArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AfterNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private AfterNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AfterNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator AfterNodes(Node value)
        => new(value);

    public static implicit operator AfterNodes(string value)
        => new(value);
}

/// <summary>
/// AlgorithmIdentifier
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder), nameof(AllowedBluetoothDeviceAllowedServicesCollectionBuilder.Create))]
public readonly struct AllowedBluetoothDeviceAllowedServices : IEnumerable<UUID>
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
/// AnimateOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AnimateOptions
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly KeyframeAnimationOptions? _value2;

    private AnimateOptions(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AnimateOptions(KeyframeAnimationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator AnimateOptions(double value)
        => new(value);

    public static implicit operator AnimateOptions(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>
/// AppendNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AppendNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private AppendNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AppendNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator AppendNodes(Node value)
        => new(value);

    public static implicit operator AppendNodes(string value)
        => new(value);
}

/// <summary>
/// ArrayBufferView
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ArrayBufferView
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
/// AssignNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AssignNodes
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Text? _value2;

    private AssignNodes(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private AssignNodes(Text value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Text? AsText => _kind == 2 ? _value2 : default;

    public static implicit operator AssignNodes(Element value)
        => new(value);

    public static implicit operator AssignNodes(Text value)
        => new(value);
}

/// <summary>
/// AudioContextOptionsLatencyHint
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AudioContextOptionsLatencyHint
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AudioContextOptionsRenderSizeHint
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AudioContextOptionsSinkId
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AudioContextSetSinkId
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct AudioContextSinkId
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BackgroundFetchManagerFetchRequestsCollectionBuilder), nameof(BackgroundFetchManagerFetchRequestsCollectionBuilder.Create))]
public readonly struct BackgroundFetchManagerFetchRequests : IEnumerable<RequestInfo>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeCompositeCollectionBuilder), nameof(BasePropertyIndexedKeyframeCompositeCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeComposite : IEnumerable<CompositeOperationOrAuto>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeEasingCollectionBuilder), nameof(BasePropertyIndexedKeyframeEasingCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeEasing : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(BasePropertyIndexedKeyframeOffsetCollectionBuilder), nameof(BasePropertyIndexedKeyframeOffsetCollectionBuilder.Create))]
public readonly struct BasePropertyIndexedKeyframeOffset : IEnumerable<double?>
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
/// BeforeNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BeforeNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private BeforeNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private BeforeNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator BeforeNodes(Node value)
        => new(value);

    public static implicit operator BeforeNodes(string value)
        => new(value);
}

/// <summary>
/// BinaryData
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothAdvertisingEventInitUUIDs
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothCharacteristicUUID
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothDescriptorUUID
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothServiceUUID
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothUUIDGetCharacteristicName
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothUUIDGetDescriptorName
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BluetoothUUIDGetServiceName
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct BodyInit
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
/// BufferSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CSSFontFeatureValuesMapSetValuesCollectionBuilder), nameof(CSSFontFeatureValuesMapSetValuesCollectionBuilder.Create))]
public readonly struct CSSFontFeatureValuesMapSetValues : IEnumerable<uint>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CSSPseudoElementParent
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CanvasImageSource
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CanvasRenderingContext2DFillStyle
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CanvasRenderingContext2DRoundRectRadii
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(CanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct CanvasRenderingContext2DRoundRectRadiiValue : IEnumerable<CanvasRenderingContext2DRoundRectRadii>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CanvasRenderingContext2DStrokeStyle
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CharacterDataAfterNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CharacterDataBeforeNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CharacterDataReplaceWithNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ClipboardItemDataValue
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ConstrainBoolean
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringCollectionBuilder), nameof(ConstrainDOMStringCollectionBuilder.Create))]
public readonly struct ConstrainDOMString : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersExactCollectionBuilder), nameof(ConstrainDOMStringParametersExactCollectionBuilder.Create))]
public readonly struct ConstrainDOMStringParametersExact : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainDOMStringParametersIdealCollectionBuilder), nameof(ConstrainDOMStringParametersIdealCollectionBuilder.Create))]
public readonly struct ConstrainDOMStringParametersIdeal : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ConstrainDouble
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ConstrainPoint2DCollectionBuilder), nameof(ConstrainPoint2DCollectionBuilder.Create))]
public readonly struct ConstrainPoint2D : IEnumerable<Point2D>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ConstrainULong
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
/// CreateElementNSOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CreateElementNSOptions
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ElementCreationOptions? _value2;

    private CreateElementNSOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CreateElementNSOptions(ElementCreationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ElementCreationOptions? AsElementCreationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator CreateElementNSOptions(string value)
        => new(value);

    public static implicit operator CreateElementNSOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// CreateElementOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CreateElementOptions
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ElementCreationOptions? _value2;

    private CreateElementOptions(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CreateElementOptions(ElementCreationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ElementCreationOptions? AsElementCreationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator CreateElementOptions(string value)
        => new(value);

    public static implicit operator CreateElementOptions(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// CreateObjectURLObj
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CreateObjectURLObj
{
    private readonly byte _kind;
    private readonly Blob? _value1;
    private readonly MediaSource? _value2;

    private CreateObjectURLObj(Blob value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private CreateObjectURLObj(MediaSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Blob? AsBlob => _kind == 1 ? _value1 : default;

    public MediaSource? AsMediaSource => _kind == 2 ? _value2 : default;

    public static implicit operator CreateObjectURLObj(Blob value)
        => new(value);

    public static implicit operator CreateObjectURLObj(MediaSource value)
        => new(value);
}

/// <summary>
/// CryptoKeyID
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct CryptoKeyID
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixInitValueCollectionBuilder), nameof(DOMMatrixInitValueCollectionBuilder.Create))]
public readonly struct DOMMatrixInitValue : IEnumerable<double>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DOMMatrixReadOnlyInitCollectionBuilder), nameof(DOMMatrixReadOnlyInitCollectionBuilder.Create))]
public readonly struct DOMMatrixReadOnlyInit : IEnumerable<double>
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
/// DefaultValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueCollectionBuilder), nameof(DefaultValueCollectionBuilder.Create))]
public readonly struct DefaultValue : IEnumerable<double?>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly double?[]? _value2;

    private DefaultValue(double? value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DefaultValue(double?[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public double?[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValueCollectionBuilder), nameof(DefaultValueValueCollectionBuilder.Create))]
public readonly struct DefaultValueValue : IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private DefaultValueValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DefaultValueValue(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue2CollectionBuilder), nameof(DefaultValueValue2CollectionBuilder.Create))]
public readonly struct DefaultValueValue2 : IEnumerable<CompositeOperationOrAuto>
{
    private readonly byte _kind;
    private readonly CompositeOperationOrAuto? _value1;
    private readonly CompositeOperationOrAuto[]? _value2;

    private DefaultValueValue2(CompositeOperationOrAuto value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DefaultValueValue2(CompositeOperationOrAuto[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public CompositeOperationOrAuto? AsCompositeOperationOrAuto => _kind == 1 ? _value1 : default;

    public CompositeOperationOrAuto[]? AsCompositeOperationOrAutoArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(DefaultValueValue3CollectionBuilder), nameof(DefaultValueValue3CollectionBuilder.Create))]
public readonly struct DefaultValueValue3 : IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private DefaultValueValue3(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private DefaultValueValue3(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DisplayMediaStreamOptionsAudio
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DisplayMediaStreamOptionsVideo
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentAppendNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentCreateElementNSOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentCreateElementOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentFragmentAppendNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentFragmentPrependNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentFragmentReplaceChildrenNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentPrependNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentReplaceChildrenNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentStartViewTransitionCallbackOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentTypeAfterNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentTypeBeforeNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct DocumentTypeReplaceWithNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct EffectTimingDuration
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementAfterNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementAnimateOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementAppendNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementBeforeNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementInternalsSetFormValue
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementInternalsSetFormValueState
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementPrependNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementReplaceChildrenNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementReplaceWithNodes
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ElementScrollIntoViewArg
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
/// EventListenerValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct EventListenerValue
{
    private readonly byte _kind;
    private readonly EventListenerLiteral? _value1;
    private readonly HandleEventCallback? _value2;

    private EventListenerValue(EventListenerLiteral value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private EventListenerValue(HandleEventCallback value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public EventListenerLiteral? AsEventListenerLiteral => _kind == 1 ? _value1 : default;

    public HandleEventCallback? AsHandleEventCallback => _kind == 2 ? _value2 : default;

    public static implicit operator EventListenerValue(EventListenerLiteral value)
        => new(value);

    public static implicit operator EventListenerValue(HandleEventCallback value)
        => new(value);
}

/// <summary>
/// EventTargetAddEventListenerOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct EventTargetAddEventListenerOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct EventTargetRemoveEventListenerOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ExtendableMessageEventInitSource
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ExtendableMessageEventSource
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct FencedFrameConfigSize
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(FilePickerAcceptTypeAcceptValueCollectionBuilder), nameof(FilePickerAcceptTypeAcceptValueCollectionBuilder.Create))]
public readonly struct FilePickerAcceptTypeAcceptValue : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct FileReaderResult
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
/// FileSystemWriteChunkType
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Float32ListCollectionBuilder), nameof(Float32ListCollectionBuilder.Create))]
public readonly struct Float32List : IEnumerable<GLfloat>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct FontFaceSourceValue
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct FormDataEntryValue
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GenerateBidOutputAdComponents
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GenerateBidOutputRender
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GeometryNode
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
/// GetCharacteristicName
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GetCharacteristicName
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private GetCharacteristicName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GetCharacteristicName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator GetCharacteristicName(string value)
        => new(value);

    public static implicit operator GetCharacteristicName(uint value)
        => new(value);
}

/// <summary>
/// GetDescriptorName
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GetDescriptorName
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private GetDescriptorName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GetDescriptorName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator GetDescriptorName(string value)
        => new(value);

    public static implicit operator GetDescriptorName(uint value)
        => new(value);
}

/// <summary>
/// GetServiceName
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GetServiceName
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private GetServiceName(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private GetServiceName(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator GetServiceName(string value)
        => new(value);

    public static implicit operator GetServiceName(uint value)
        => new(value);
}

/// <summary>
/// GroupEffectTiming
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct GroupEffectTiming
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLAllCollectionItemResult
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLAllCollectionNamedItemResult
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLElementHidden
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLFormControlsCollectionNamedItemResult
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLFormElementResult
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLOptionsCollectionAddBefore
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLOptionsCollectionAddElement
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLOrSVGImageElement
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLOrSVGScriptElement
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLSelectElementAddBefore
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLSelectElementAddElement
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct HTMLSlotElementAssignNodes
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(HeadersInitCollectionBuilder), nameof(HeadersInitCollectionBuilder.Create))]
public readonly struct HeadersInit : IEnumerable<byte[][]>
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
/// IDBCursorSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBDatabaseTransactionStoreNamesCollectionBuilder), nameof(IDBDatabaseTransactionStoreNamesCollectionBuilder.Create))]
public readonly struct IDBDatabaseTransactionStoreNames : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder), nameof(IDBObjectStoreCreateIndexKeyPathCollectionBuilder.Create))]
public readonly struct IDBObjectStoreCreateIndexKeyPath : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IDBObjectStoreParametersKeyPathCollectionBuilder), nameof(IDBObjectStoreParametersKeyPathCollectionBuilder.Create))]
public readonly struct IDBObjectStoreParametersKeyPath : IEnumerable<string>
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
/// IDBRequestSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ImageBitmapRenderingContextCanvas
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ImageBitmapSource
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
/// ImageBufferSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(InstallEventAddRoutesRulesCollectionBuilder), nameof(InstallEventAddRoutesRulesCollectionBuilder.Create))]
public readonly struct InstallEventAddRoutesRules : IEnumerable<RouterRule>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Int32ListCollectionBuilder), nameof(Int32ListCollectionBuilder.Create))]
public readonly struct Int32List : IEnumerable<GLint>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntersectionObserverInitRoot
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(IntersectionObserverInitThresholdCollectionBuilder), nameof(IntersectionObserverInitThresholdCollectionBuilder.Create))]
public readonly struct IntersectionObserverInitThreshold : IEnumerable<double>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct IntersectionObserverRoot
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct KeyframeAnimationOptionsRangeEnd
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct KeyframeAnimationOptionsRangeStart
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct KeyframeEffectOptionsValue
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct LineAndPositionSetting
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(MLGraphBuilderSplitSplitsCollectionBuilder), nameof(MLGraphBuilderSplitSplitsCollectionBuilder.Create))]
public readonly struct MLGraphBuilderSplitSplits : IEnumerable<uint>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaProvider
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaStreamConstraintsAudio
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaStreamConstraintsVideo
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaTrackConstraintSetPan
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaTrackConstraintSetTilt
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MediaTrackConstraintSetZoom
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct MessageEventSource
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
/// NDEFMessageSource
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NavigatorRunAdAuctionResultValue
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
/// NodeFilterValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct NodeFilterValue
{
    private readonly byte _kind;
    private readonly NodeFilterLiteral? _value1;
    private readonly AcceptNodeCallback? _value2;

    private NodeFilterValue(NodeFilterLiteral value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private NodeFilterValue(AcceptNodeCallback value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public NodeFilterLiteral? AsNodeFilterLiteral => _kind == 1 ? _value1 : default;

    public AcceptNodeCallback? AsAcceptNodeCallback => _kind == 2 ? _value2 : default;

    public static implicit operator NodeFilterValue(NodeFilterLiteral value)
        => new(value);

    public static implicit operator NodeFilterValue(AcceptNodeCallback value)
        => new(value);
}

/// <summary>
/// OfflineAudioContextOptionsRenderSizeHint
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OfflineAudioContextOptionsRenderSizeHint
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DFillStyle
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DRoundRectRadii
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder), nameof(OffscreenCanvasRenderingContext2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct OffscreenCanvasRenderingContext2DRoundRectRadiiValue : IEnumerable<OffscreenCanvasRenderingContext2DRoundRectRadii>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OffscreenCanvasRenderingContext2DStrokeStyle
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OffscreenRenderingContext
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct OptionalEffectTimingDuration
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ParameterCurrentTarget
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ParameterEvent
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PasswordCredentialInit
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct Path2DPath
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct Path2DRoundRectRadii
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Path2DRoundRectRadiiValueCollectionBuilder), nameof(Path2DRoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct Path2DRoundRectRadiiValue : IEnumerable<Path2DRoundRectRadii>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PerformanceMeasureOptionsEnd
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PerformanceMeasureOptionsStart
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PerformanceMeasureStartOrMeasureOptions
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
/// PrependNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct PrependNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private PrependNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private PrependNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator PrependNodes(Node value)
        => new(value);

    public static implicit operator PrependNodes(string value)
        => new(value);
}

/// <summary>
/// PushMessageDataInit
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RTCIceServerUrlsCollectionBuilder), nameof(RTCIceServerUrlsCollectionBuilder.Create))]
public readonly struct RTCIceServerUrls : IEnumerable<string>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RTCPeerConnectionAddTransceiverTrackOrKind
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RTCRtpTransform
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ReadableStreamController
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ReadableStreamReader
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
/// RemoveEventListenerOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RemoveEventListenerOptions
{
    private readonly byte _kind;
    private readonly EventListenerOptions? _value1;
    private readonly bool? _value2;

    private RemoveEventListenerOptions(EventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private RemoveEventListenerOptions(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public EventListenerOptions? AsEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator RemoveEventListenerOptions(EventListenerOptions value)
        => new(value);

    public static implicit operator RemoveEventListenerOptions(bool value)
        => new(value);
}

/// <summary>
/// RenderingContext
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RenderingContext
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
/// ReplaceChildrenNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ReplaceChildrenNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ReplaceChildrenNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ReplaceChildrenNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ReplaceChildrenNodes(Node value)
        => new(value);

    public static implicit operator ReplaceChildrenNodes(string value)
        => new(value);
}

/// <summary>
/// ReplaceWithNodes
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ReplaceWithNodes
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private ReplaceWithNodes(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ReplaceWithNodes(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator ReplaceWithNodes(Node value)
        => new(value);

    public static implicit operator ReplaceWithNodes(string value)
        => new(value);
}

/// <summary>
/// ReportEventType
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ReportEventType
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RequestInfo
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RotationMatrixType
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RoundRectRadii
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
/// RoundRectRadiiValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(RoundRectRadiiValueCollectionBuilder), nameof(RoundRectRadiiValueCollectionBuilder.Create))]
public readonly struct RoundRectRadiiValue : IEnumerable<RoundRectRadii>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;
    private readonly RoundRectRadii[]? _value3;

    private RoundRectRadiiValue(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private RoundRectRadiiValue(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private RoundRectRadiiValue(RoundRectRadii[] value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public RoundRectRadii[]? AsRoundRectRadiiArray => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct RouterSource
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SanitizerAttribute
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SanitizerElement
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SanitizerElementWithAttributes
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
/// ScrollIntoViewArg
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ScrollIntoViewArg
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ScrollIntoViewOptions? _value2;

    private ScrollIntoViewArg(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private ScrollIntoViewArg(ScrollIntoViewOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => _kind == 2 ? _value2 : default;

    public static implicit operator ScrollIntoViewArg(bool value)
        => new(value);

    public static implicit operator ScrollIntoViewArg(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>
/// SendBody
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SendBody
{
    private readonly byte _kind;
    private readonly Document? _value1;
    private readonly XMLHttpRequestBodyInit? _value2;

    private SendBody(Document value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SendBody(XMLHttpRequestBodyInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Document? AsDocument => _kind == 1 ? _value1 : default;

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => _kind == 2 ? _value2 : default;

    public static implicit operator SendBody(Document value)
        => new(value);

    public static implicit operator SendBody(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// SendData
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SequenceEffectTiming
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
/// SetFormValueState
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SetFormValueState
{
    private readonly byte _kind;
    private readonly File? _value1;
    private readonly string? _value2;
    private readonly FormData? _value3;

    private SetFormValueState(File value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private SetFormValueState(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private SetFormValueState(FormData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public File? AsFile => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public FormData? AsFormData => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SetSinkId
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkOptions? _value2;

    private SetSinkId(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SetSinkId(AudioSinkOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkOptions? AsAudioSinkOptions => _kind == 2 ? _value2 : default;

    public static implicit operator SetSinkId(string value)
        => new(value);

    public static implicit operator SetSinkId(AudioSinkOptions value)
        => new(value);
}

/// <summary>
/// SetValues
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(SetValuesCollectionBuilder), nameof(SetValuesCollectionBuilder.Create))]
public readonly struct SetValues : IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private SetValues(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private SetValues(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ShadowAnimationNewTarget
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SharedStorageResponse
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct SharedWorkerOptions
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StartInDirectory
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
/// StartViewTransitionCallbackOptions
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StartViewTransitionCallbackOptions
{
    private readonly byte _kind;
    private readonly UpdateCallback? _value1;
    private readonly StartViewTransitionOptions? _value2;

    private StartViewTransitionCallbackOptions(UpdateCallback value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StartViewTransitionCallbackOptions(StartViewTransitionOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public UpdateCallback? AsUpdateCallback => _kind == 1 ? _value1 : default;

    public StartViewTransitionOptions? AsStartViewTransitionOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StartViewTransitionCallbackOptions(UpdateCallback value)
        => new(value);

    public static implicit operator StartViewTransitionCallbackOptions(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// StructuralCache
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCache
{
    private readonly byte _kind;
    private readonly Node? _value1;
    private readonly string? _value2;

    private StructuralCache(Node value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCache(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Node? AsNode => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCache(Node value)
        => new(value);

    public static implicit operator StructuralCache(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly CanvasGradient? _value2;
    private readonly CanvasPattern? _value3;

    private StructuralCacheValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue(CanvasGradient value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue(CanvasPattern value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public CanvasGradient? AsCanvasGradient => _kind == 2 ? _value2 : default;

    public CanvasPattern? AsCanvasPattern => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue10
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly ScrollIntoViewOptions? _value2;

    private StructuralCacheValue10(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue10(ScrollIntoViewOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public ScrollIntoViewOptions? AsScrollIntoViewOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue10(bool value)
        => new(value);

    public static implicit operator StructuralCacheValue10(ScrollIntoViewOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue11
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue11
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly CSSPseudoElement? _value2;

    private StructuralCacheValue11(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue11(CSSPseudoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue11(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue11(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue12
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue12
{
    private readonly byte _kind;
    private readonly UpdateCallback? _value1;
    private readonly StartViewTransitionOptions? _value2;

    private StructuralCacheValue12(UpdateCallback value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue12(StartViewTransitionOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public UpdateCallback? AsUpdateCallback => _kind == 1 ? _value1 : default;

    public StartViewTransitionOptions? AsStartViewTransitionOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue12(UpdateCallback value)
        => new(value);

    public static implicit operator StructuralCacheValue12(StartViewTransitionOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue13
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue13
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ElementCreationOptions? _value2;

    private StructuralCacheValue13(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue13(ElementCreationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ElementCreationOptions? AsElementCreationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue13(string value)
        => new(value);

    public static implicit operator StructuralCacheValue13(ElementCreationOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue14
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue14
{
    private readonly byte _kind;
    private readonly bool? _value1;
    private readonly double? _value2;
    private readonly string? _value3;

    private StructuralCacheValue14(bool value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue14(double value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue14(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public bool? AsBool => _kind == 1 ? _value1 : default;

    public double? AsDouble => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue15
{
    private readonly byte _kind;
    private readonly AddEventListenerOptions? _value1;
    private readonly bool? _value2;

    private StructuralCacheValue15(AddEventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue15(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public AddEventListenerOptions? AsAddEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue15(AddEventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue15(bool value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue16
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue16
{
    private readonly byte _kind;
    private readonly EventListenerOptions? _value1;
    private readonly bool? _value2;

    private StructuralCacheValue16(EventListenerOptions value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue16(bool value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public EventListenerOptions? AsEventListenerOptions => _kind == 1 ? _value1 : default;

    public bool? AsBool => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue16(EventListenerOptions value)
        => new(value);

    public static implicit operator StructuralCacheValue16(bool value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue17
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue17
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly PerformanceMeasureOptions? _value2;

    private StructuralCacheValue17(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue17(PerformanceMeasureOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public PerformanceMeasureOptions? AsPerformanceMeasureOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue17(string value)
        => new(value);

    public static implicit operator StructuralCacheValue17(PerformanceMeasureOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue18
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue18
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly ArrayBuffer? _value2;

    private StructuralCacheValue18(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue18(ArrayBuffer value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public ArrayBuffer? AsArrayBuffer => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue18(string value)
        => new(value);

    public static implicit operator StructuralCacheValue18(ArrayBuffer value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue19
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue19
{
    private readonly byte _kind;
    private readonly Blob? _value1;
    private readonly MediaSource? _value2;

    private StructuralCacheValue19(Blob value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue19(MediaSource value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Blob? AsBlob => _kind == 1 ? _value1 : default;

    public MediaSource? AsMediaSource => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue19(Blob value)
        => new(value);

    public static implicit operator StructuralCacheValue19(MediaSource value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue2
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue2
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;

    private StructuralCacheValue2(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue2(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue2(double value)
        => new(value);

    public static implicit operator StructuralCacheValue2(DOMPointInit value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue20
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue20CollectionBuilder), nameof(StructuralCacheValue20CollectionBuilder.Create))]
public readonly struct StructuralCacheValue20 : IEnumerable<double>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly double[]? _value2;

    private StructuralCacheValue20(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue20(double[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public double[]? AsDoubleArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue21
{
    private readonly byte _kind;
    private readonly HTMLCollection? _value1;
    private readonly Element? _value2;

    private StructuralCacheValue21(HTMLCollection value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue21(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCollection? AsHTMLCollection => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue21(HTMLCollection value)
        => new(value);

    public static implicit operator StructuralCacheValue21(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue22
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue22
{
    private readonly byte _kind;
    private readonly RadioNodeList? _value1;
    private readonly Element? _value2;

    private StructuralCacheValue22(RadioNodeList value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue22(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RadioNodeList? AsRadioNodeList => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue22(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue22(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue23
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue23
{
    private readonly byte _kind;
    private readonly HTMLOptionElement? _value1;
    private readonly HTMLOptGroupElement? _value2;

    private StructuralCacheValue23(HTMLOptionElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue23(HTMLOptGroupElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLOptionElement? AsHTMLOptionElement => _kind == 1 ? _value1 : default;

    public HTMLOptGroupElement? AsHTMLOptGroupElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue23(HTMLOptionElement value)
        => new(value);

    public static implicit operator StructuralCacheValue23(HTMLOptGroupElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue24
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue24
{
    private readonly byte _kind;
    private readonly HTMLElement? _value1;
    private readonly int? _value2;

    private StructuralCacheValue24(HTMLElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue24(int value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLElement? AsHTMLElement => _kind == 1 ? _value1 : default;

    public int? AsInt => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue24(HTMLElement value)
        => new(value);

    public static implicit operator StructuralCacheValue24(int value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue25
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue25
{
    private readonly byte _kind;
    private readonly VideoTrack? _value1;
    private readonly AudioTrack? _value2;
    private readonly TextTrack? _value3;

    private StructuralCacheValue25(VideoTrack value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue25(AudioTrack value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue25(TextTrack value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public VideoTrack? AsVideoTrack => _kind == 1 ? _value1 : default;

    public AudioTrack? AsAudioTrack => _kind == 2 ? _value2 : default;

    public TextTrack? AsTextTrack => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue26
{
    private readonly byte _kind;
    private readonly RadioNodeList? _value1;
    private readonly Element? _value2;

    private StructuralCacheValue26(RadioNodeList value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue26(Element value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RadioNodeList? AsRadioNodeList => _kind == 1 ? _value1 : default;

    public Element? AsElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue26(RadioNodeList value)
        => new(value);

    public static implicit operator StructuralCacheValue26(Element value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue27
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue27
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Text? _value2;

    private StructuralCacheValue27(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue27(Text value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Text? AsText => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue27(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue27(Text value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue28
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue28
{
    private readonly byte _kind;
    private readonly Path2D? _value1;
    private readonly string? _value2;

    private StructuralCacheValue28(Path2D value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue28(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Path2D? AsPath2D => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue28(Path2D value)
        => new(value);

    public static implicit operator StructuralCacheValue28(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue29
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue29
{
    private readonly byte _kind;
    private readonly File? _value1;
    private readonly string? _value2;
    private readonly FormData? _value3;

    private StructuralCacheValue29(File value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue29(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue29(FormData value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public File? AsFile => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public FormData? AsFormData => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue3CollectionBuilder), nameof(StructuralCacheValue3CollectionBuilder.Create))]
public readonly struct StructuralCacheValue3 : IEnumerable<StructuralCacheValue2>
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly DOMPointInit? _value2;
    private readonly StructuralCacheValue2[]? _value3;

    private StructuralCacheValue3(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue3(DOMPointInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue3(StructuralCacheValue2[] value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public DOMPointInit? AsDOMPointInit => _kind == 2 ? _value2 : default;

    public StructuralCacheValue2[]? AsStructuralCacheValue2Array => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue30
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly WorkerOptions? _value2;

    private StructuralCacheValue30(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue30(WorkerOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public WorkerOptions? AsWorkerOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue30(string value)
        => new(value);

    public static implicit operator StructuralCacheValue30(WorkerOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue31
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue32CollectionBuilder), nameof(StructuralCacheValue32CollectionBuilder.Create))]
public readonly struct StructuralCacheValue32 : IEnumerable<string>
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly string[]? _value2;

    private StructuralCacheValue32(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue32(string[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public string[]? AsStringArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue34
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly Document? _value2;

    private StructuralCacheValue34(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue34(Document value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public Document? AsDocument => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue34(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue34(Document value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue35
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue35CollectionBuilder), nameof(StructuralCacheValue35CollectionBuilder.Create))]
public readonly struct StructuralCacheValue35 : IEnumerable<RouterRule>
{
    private readonly byte _kind;
    private readonly RouterRule? _value1;
    private readonly RouterRule[]? _value2;

    private StructuralCacheValue35(RouterRule value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue35(RouterRule[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RouterRule? AsRouterRule => _kind == 1 ? _value1 : default;

    public RouterRule[]? AsRouterRuleArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue36
{
    private readonly byte _kind;
    private readonly Client? _value1;
    private readonly ServiceWorker? _value2;
    private readonly MessagePort? _value3;

    private StructuralCacheValue36(Client value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue36(ServiceWorker value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue36(MessagePort value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public Client? AsClient => _kind == 1 ? _value1 : default;

    public ServiceWorker? AsServiceWorker => _kind == 2 ? _value2 : default;

    public MessagePort? AsMessagePort => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue37
{
    private readonly byte _kind;
    private readonly Element? _value1;
    private readonly CSSPseudoElement? _value2;

    private StructuralCacheValue37(Element value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue37(CSSPseudoElement value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Element? AsElement => _kind == 1 ? _value1 : default;

    public CSSPseudoElement? AsCSSPseudoElement => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue37(Element value)
        => new(value);

    public static implicit operator StructuralCacheValue37(CSSPseudoElement value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue38
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue38CollectionBuilder), nameof(StructuralCacheValue38CollectionBuilder.Create))]
public readonly struct StructuralCacheValue38 : IEnumerable<string[]>
{
    private readonly byte _kind;
    private readonly string[][]? _value1;
    private readonly Dictionary<string, string>? _value2;
    private readonly string? _value3;

    private StructuralCacheValue38(string[][] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
    }

    private StructuralCacheValue38(Dictionary<string, string> value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
    }

    private StructuralCacheValue38(string value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
    }

    public string[][]? AsStringArrayArray => _kind == 1 ? _value1 : default;

    public Dictionary<string, string>? AsDictionaryStringString => _kind == 2 ? _value2 : default;

    public string? AsString => _kind == 3 ? _value3 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue39
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly KeyframeEffectOptions? _value2;

    private StructuralCacheValue39(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue39(KeyframeEffectOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public KeyframeEffectOptions? AsKeyframeEffectOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue39(double value)
        => new(value);

    public static implicit operator StructuralCacheValue39(KeyframeEffectOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue4
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue4
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly KeyframeAnimationOptions? _value2;

    private StructuralCacheValue4(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue4(KeyframeAnimationOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public KeyframeAnimationOptions? AsKeyframeAnimationOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue4(double value)
        => new(value);

    public static implicit operator StructuralCacheValue4(KeyframeAnimationOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue40
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue40
{
    private readonly byte _kind;
    private readonly double? _value1;
    private readonly EffectTiming? _value2;

    private StructuralCacheValue40(double value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue40(EffectTiming value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double? AsDouble => _kind == 1 ? _value1 : default;

    public EffectTiming? AsEffectTiming => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue40(double value)
        => new(value);

    public static implicit operator StructuralCacheValue40(EffectTiming value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue41
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue41
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly uint? _value2;

    private StructuralCacheValue41(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue41(uint value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public uint? AsUint => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue41(string value)
        => new(value);

    public static implicit operator StructuralCacheValue41(uint value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue42
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue42
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkInfo? _value2;

    private StructuralCacheValue42(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue42(AudioSinkInfo value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkInfo? AsAudioSinkInfo => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue42(string value)
        => new(value);

    public static implicit operator StructuralCacheValue42(AudioSinkInfo value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue43
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue43
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly AudioSinkOptions? _value2;

    private StructuralCacheValue43(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue43(AudioSinkOptions value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public AudioSinkOptions? AsAudioSinkOptions => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue43(string value)
        => new(value);

    public static implicit operator StructuralCacheValue43(AudioSinkOptions value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue44
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue45CollectionBuilder), nameof(StructuralCacheValue45CollectionBuilder.Create))]
public readonly struct StructuralCacheValue45 : IEnumerable<GLint>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLint[]? _value2;

    private StructuralCacheValue45(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue45(GLint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLint[]? AsGLintArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue46CollectionBuilder), nameof(StructuralCacheValue46CollectionBuilder.Create))]
public readonly struct StructuralCacheValue46 : IEnumerable<GLsizei>
{
    private readonly byte _kind;
    private readonly Int32Array? _value1;
    private readonly GLsizei[]? _value2;

    private StructuralCacheValue46(Int32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue46(GLsizei[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Int32Array? AsInt32Array => _kind == 1 ? _value1 : default;

    public GLsizei[]? AsGLsizeiArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue47CollectionBuilder), nameof(StructuralCacheValue47CollectionBuilder.Create))]
public readonly struct StructuralCacheValue47 : IEnumerable<GLuint>
{
    private readonly byte _kind;
    private readonly Uint32Array? _value1;
    private readonly GLuint[]? _value2;

    private StructuralCacheValue47(Uint32Array value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue47(GLuint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Uint32Array? AsUint32Array => _kind == 1 ? _value1 : default;

    public GLuint[]? AsGLuintArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue48CollectionBuilder), nameof(StructuralCacheValue48CollectionBuilder.Create))]
public readonly struct StructuralCacheValue48 : IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private StructuralCacheValue48(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue48(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue49
{
    private readonly byte _kind;
    private readonly MediaStreamTrack? _value1;
    private readonly string? _value2;

    private StructuralCacheValue49(MediaStreamTrack value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue49(string value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public MediaStreamTrack? AsMediaStreamTrack => _kind == 1 ? _value1 : default;

    public string? AsString => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue49(MediaStreamTrack value)
        => new(value);

    public static implicit operator StructuralCacheValue49(string value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue5
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue5
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    private StructuralCacheValue5(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue5(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue5(HTMLCanvasElement value)
        => new(value);

    public static implicit operator StructuralCacheValue5(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue50
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue6
{
    private readonly byte _kind;
    private readonly Document? _value1;
    private readonly XMLHttpRequestBodyInit? _value2;

    private StructuralCacheValue6(Document value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue6(XMLHttpRequestBodyInit value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public Document? AsDocument => _kind == 1 ? _value1 : default;

    public XMLHttpRequestBodyInit? AsXMLHttpRequestBodyInit => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue6(Document value)
        => new(value);

    public static implicit operator StructuralCacheValue6(XMLHttpRequestBodyInit value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue7
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue7CollectionBuilder), nameof(StructuralCacheValue7CollectionBuilder.Create))]
public readonly struct StructuralCacheValue7 : IEnumerable<RequestInfo>
{
    private readonly byte _kind;
    private readonly RequestInfo? _value1;
    private readonly RequestInfo[]? _value2;

    private StructuralCacheValue7(RequestInfo value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue7(RequestInfo[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public RequestInfo? AsRequestInfo => _kind == 1 ? _value1 : default;

    public RequestInfo[]? AsRequestInfoArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValue8
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly BinaryData? _value2;

    private StructuralCacheValue8(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue8(BinaryData value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public BinaryData? AsBinaryData => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValue8(string value)
        => new(value);

    public static implicit operator StructuralCacheValue8(BinaryData value)
        => new(value);
}

/// <summary>
/// StructuralCacheValue9
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(StructuralCacheValue9CollectionBuilder), nameof(StructuralCacheValue9CollectionBuilder.Create))]
public readonly struct StructuralCacheValue9 : IEnumerable<uint>
{
    private readonly byte _kind;
    private readonly uint? _value1;
    private readonly uint[]? _value2;

    private StructuralCacheValue9(uint value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValue9(uint[] value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public uint? AsUint => _kind == 1 ? _value1 : default;

    public uint[]? AsUintArray => _kind == 2 ? _value2 : default;

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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct StructuralCacheValueValue
{
    private readonly byte _kind;
    private readonly string? _value1;
    private readonly FencedFrameConfig? _value2;

    private StructuralCacheValueValue(string value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private StructuralCacheValueValue(FencedFrameConfig value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public string? AsString => _kind == 1 ? _value1 : default;

    public FencedFrameConfig? AsFencedFrameConfig => _kind == 2 ? _value2 : default;

    public static implicit operator StructuralCacheValueValue(string value)
        => new(value);

    public static implicit operator StructuralCacheValueValue(FencedFrameConfig value)
        => new(value);
}

/// <summary>
/// SubtleCryptoImportKeyKeyData
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TaskSignalAnyInitPriority
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TexImageSource
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TimerHandler
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TrackEventInitTrack
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TrackEventTrack
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct TrustedType
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct URLCreateObjectURLObj
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct URLPatternCompatible
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct URLPatternInput
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(URLSearchParamsInitCollectionBuilder), nameof(URLSearchParamsInitCollectionBuilder.Create))]
public readonly struct URLSearchParamsInit : IEnumerable<string[]>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(Uint32ListCollectionBuilder), nameof(Uint32ListCollectionBuilder.Create))]
public readonly struct Uint32List : IEnumerable<GLuint>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct UrnOrConfig
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(VibratePatternCollectionBuilder), nameof(VibratePatternCollectionBuilder.Create))]
public readonly struct VibratePattern : IEnumerable<uint>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ViewTimelineOptionsInset
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ViewTimelineOptionsInsetValueCollectionBuilder), nameof(ViewTimelineOptionsInsetValueCollectionBuilder.Create))]
public readonly struct ViewTimelineOptionsInsetValue : IEnumerable<ViewTimelineOptionsInset>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ViewportMediaStreamConstraintsAudio
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct ViewportMediaStreamConstraintsVideo
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLBaseInstancesList : IEnumerable<GLuint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLFirstsList : IEnumerable<GLint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawArraysInstancedBaseInstanceWEBGLInstanceCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseInstancesList : IEnumerable<GLuint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLBaseVerticesList : IEnumerable<GLint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLInstanceCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawInstancedBaseVertexBaseInstanceMultiDrawElementsInstancedBaseVertexBaseInstanceWEBGLOffsetsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLFirstsList : IEnumerable<GLint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysInstancedWEBGLInstanceCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawArraysWEBGLFirstsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawArraysWEBGLFirstsList : IEnumerable<GLint>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLInstanceCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsInstancedWEBGLOffsetsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLCountsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsWEBGLCountsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder), nameof(WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsListCollectionBuilder.Create))]
public readonly struct WEBGLMultiDrawMultiDrawElementsWEBGLOffsetsList : IEnumerable<GLsizei>
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct WebGL2RenderingContextCanvas
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct WebGLRenderingContextCanvas
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
[ECMAScriptUnion]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(WebSocketProtocolsCollectionBuilder), nameof(WebSocketProtocolsCollectionBuilder.Create))]
public readonly struct WebSocketProtocols : IEnumerable<string>
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
/// WebSocketSendData
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
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
[ECMAScriptUnion]
[Description("@#")]
public readonly struct XMLHttpRequestSendBody
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
/// XPathNSResolverValue
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct XPathNSResolverValue
{
    private readonly byte _kind;
    private readonly XPathNSResolverLiteral? _value1;
    private readonly LookupNamespaceURICallback? _value2;

    private XPathNSResolverValue(XPathNSResolverLiteral value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    private XPathNSResolverValue(LookupNamespaceURICallback value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public XPathNSResolverLiteral? AsXPathNSResolverLiteral => _kind == 1 ? _value1 : default;

    public LookupNamespaceURICallback? AsLookupNamespaceURICallback => _kind == 2 ? _value2 : default;

    public static implicit operator XPathNSResolverValue(XPathNSResolverLiteral value)
        => new(value);

    public static implicit operator XPathNSResolverValue(LookupNamespaceURICallback value)
        => new(value);
}

/// <summary>
/// XRWebGLRenderingContext
/// </summary>
[ECMAScript]
[ECMAScriptUnion]
[Description("@#")]
public readonly struct XRWebGLRenderingContext
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
