namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUBindingResource
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUBindingResource : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly GPUSampler? _value1;
    private readonly GPUTextureView? _value2;
    private readonly GPUBufferBinding? _value3;
    private readonly GPUExternalTexture? _value4;

    public GPUBindingResource(GPUSampler value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
        _value3 = default;
        _value4 = default;
    }

    public GPUBindingResource(GPUTextureView value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
        _value3 = default;
        _value4 = default;
    }

    public GPUBindingResource(GPUBufferBinding value)
    {
        _kind = 3;
        _value1 = default;
        _value2 = default;
        _value3 = value;
        _value4 = default;
    }

    public GPUBindingResource(GPUExternalTexture value)
    {
        _kind = 4;
        _value1 = default;
        _value2 = default;
        _value3 = default;
        _value4 = value;
    }

    public GPUSampler? AsGPUSampler => _kind == 1 ? _value1 : default;

    public GPUTextureView? AsGPUTextureView => _kind == 2 ? _value2 : default;

    public GPUBufferBinding? AsGPUBufferBinding => _kind == 3 ? _value3 : default;

    public GPUExternalTexture? AsGPUExternalTexture => _kind == 4 ? _value4 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        3 => _value3,
        4 => _value4,
        _ => default
    };

    public static implicit operator GPUBindingResource(GPUSampler value)
        => new(value);

    public static implicit operator GPUBindingResource(GPUTextureView value)
        => new(value);

    public static implicit operator GPUBindingResource(GPUBufferBinding value)
        => new(value);

    public static implicit operator GPUBindingResource(GPUExternalTexture value)
        => new(value);
}

/// <summary>
/// GPUCanvasContextCanvas
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUCanvasContextCanvas : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    public GPUCanvasContextCanvas(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUCanvasContextCanvas(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUCanvasContextCanvas(HTMLCanvasElement value)
        => new(value);

    public static implicit operator GPUCanvasContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// GPUColor
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUColorCollectionBuilder), nameof(GPUColorCollectionBuilder.Create))]
public readonly struct GPUColor : System.Runtime.CompilerServices.IUnion, IEnumerable<double>
{
    private readonly byte _kind;
    private readonly double[]? _value1;
    private readonly GPUColorDict? _value2;

    public GPUColor(double[] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUColor(GPUColorDict value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public double[]? AsDoubleArray => _kind == 1 ? _value1 : default;

    public GPUColorDict? AsGPUColorDict => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUColor(double[] value)
        => new(value);

    public static implicit operator GPUColor(GPUColorDict value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUColorCollectionBuilder
{
    public static GPUColor Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>
/// GPUExtent3D
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUExtent3DCollectionBuilder), nameof(GPUExtent3DCollectionBuilder.Create))]
public readonly struct GPUExtent3D : System.Runtime.CompilerServices.IUnion, IEnumerable<GPUIntegerCoordinate>
{
    private readonly byte _kind;
    private readonly GPUIntegerCoordinate[]? _value1;
    private readonly GPUExtent3DDict? _value2;

    public GPUExtent3D(GPUIntegerCoordinate[] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUExtent3D(GPUExtent3DDict value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => _kind == 1 ? _value1 : default;

    public GPUExtent3DDict? AsGPUExtent3DDict => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUExtent3D(GPUIntegerCoordinate[] value)
        => new(value);

    public static implicit operator GPUExtent3D(GPUExtent3DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUExtent3DCollectionBuilder
{
    public static GPUExtent3D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>
/// GPUExternalTextureDescriptorSource
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUExternalTextureDescriptorSource : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly HTMLVideoElement? _value1;
    private readonly VideoFrame? _value2;

    public GPUExternalTextureDescriptorSource(HTMLVideoElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUExternalTextureDescriptorSource(VideoFrame value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLVideoElement? AsHTMLVideoElement => _kind == 1 ? _value1 : default;

    public VideoFrame? AsVideoFrame => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUExternalTextureDescriptorSource(HTMLVideoElement value)
        => new(value);

    public static implicit operator GPUExternalTextureDescriptorSource(VideoFrame value)
        => new(value);
}

/// <summary>
/// GPUImageCopyExternalImageSource
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUImageCopyExternalImageSource : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly ImageBitmap? _value1;
    private readonly ImageData? _value2;
    private readonly HTMLImageElement? _value3;
    private readonly HTMLVideoElement? _value4;
    private readonly VideoFrame? _value5;
    private readonly HTMLCanvasElement? _value6;
    private readonly OffscreenCanvas? _value7;

    public GPUImageCopyExternalImageSource(ImageBitmap value)
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

    public GPUImageCopyExternalImageSource(ImageData value)
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

    public GPUImageCopyExternalImageSource(HTMLImageElement value)
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

    public GPUImageCopyExternalImageSource(HTMLVideoElement value)
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

    public GPUImageCopyExternalImageSource(VideoFrame value)
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

    public GPUImageCopyExternalImageSource(HTMLCanvasElement value)
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

    public GPUImageCopyExternalImageSource(OffscreenCanvas value)
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

    public HTMLVideoElement? AsHTMLVideoElement => _kind == 4 ? _value4 : default;

    public VideoFrame? AsVideoFrame => _kind == 5 ? _value5 : default;

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 6 ? _value6 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 7 ? _value7 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        3 => _value3,
        4 => _value4,
        5 => _value5,
        6 => _value6,
        7 => _value7,
        _ => default
    };

    public static implicit operator GPUImageCopyExternalImageSource(ImageBitmap value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(ImageData value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(HTMLImageElement value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(HTMLVideoElement value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(VideoFrame value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(HTMLCanvasElement value)
        => new(value);

    public static implicit operator GPUImageCopyExternalImageSource(OffscreenCanvas value)
        => new(value);
}

/// <summary>
/// GPUOrigin2D
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUOrigin2DCollectionBuilder), nameof(GPUOrigin2DCollectionBuilder.Create))]
public readonly struct GPUOrigin2D : System.Runtime.CompilerServices.IUnion, IEnumerable<GPUIntegerCoordinate>
{
    private readonly byte _kind;
    private readonly GPUIntegerCoordinate[]? _value1;
    private readonly GPUOrigin2DDict? _value2;

    public GPUOrigin2D(GPUIntegerCoordinate[] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUOrigin2D(GPUOrigin2DDict value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => _kind == 1 ? _value1 : default;

    public GPUOrigin2DDict? AsGPUOrigin2DDict => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUOrigin2D(GPUIntegerCoordinate[] value)
        => new(value);

    public static implicit operator GPUOrigin2D(GPUOrigin2DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUOrigin2DCollectionBuilder
{
    public static GPUOrigin2D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>
/// GPUOrigin3D
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUOrigin3DCollectionBuilder), nameof(GPUOrigin3DCollectionBuilder.Create))]
public readonly struct GPUOrigin3D : System.Runtime.CompilerServices.IUnion, IEnumerable<GPUIntegerCoordinate>
{
    private readonly byte _kind;
    private readonly GPUIntegerCoordinate[]? _value1;
    private readonly GPUOrigin3DDict? _value2;

    public GPUOrigin3D(GPUIntegerCoordinate[] value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUOrigin3D(GPUOrigin3DDict value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => _kind == 1 ? _value1 : default;

    public GPUOrigin3DDict? AsGPUOrigin3DDict => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUOrigin3D(GPUIntegerCoordinate[] value)
        => new(value);

    public static implicit operator GPUOrigin3D(GPUOrigin3DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUOrigin3DCollectionBuilder
{
    public static GPUOrigin3D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>
/// GPUPipelineDescriptorBaseLayout
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUPipelineDescriptorBaseLayout : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly GPUPipelineLayout? _value1;
    private readonly GPUAutoLayoutMode? _value2;

    public GPUPipelineDescriptorBaseLayout(GPUPipelineLayout value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUPipelineDescriptorBaseLayout(GPUAutoLayoutMode value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public GPUPipelineLayout? AsGPUPipelineLayout => _kind == 1 ? _value1 : default;

    public GPUAutoLayoutMode? AsGPUAutoLayoutMode => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUPipelineDescriptorBaseLayout(GPUPipelineLayout value)
        => new(value);

    public static implicit operator GPUPipelineDescriptorBaseLayout(GPUAutoLayoutMode value)
        => new(value);
}

/// <summary>
/// GPUShaderModuleCompilationHintLayout
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct GPUShaderModuleCompilationHintLayout : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly GPUPipelineLayout? _value1;
    private readonly GPUAutoLayoutMode? _value2;

    public GPUShaderModuleCompilationHintLayout(GPUPipelineLayout value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public GPUShaderModuleCompilationHintLayout(GPUAutoLayoutMode value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public GPUPipelineLayout? AsGPUPipelineLayout => _kind == 1 ? _value1 : default;

    public GPUAutoLayoutMode? AsGPUAutoLayoutMode => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator GPUShaderModuleCompilationHintLayout(GPUPipelineLayout value)
        => new(value);

    public static implicit operator GPUShaderModuleCompilationHintLayout(GPUAutoLayoutMode value)
        => new(value);
}

/// <summary>
/// StructuralCache
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct StructuralCache : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly HTMLCanvasElement? _value1;
    private readonly OffscreenCanvas? _value2;

    public StructuralCache(HTMLCanvasElement value)
    {
        _kind = 1;
        _value1 = value;
        _value2 = default;
    }

    public StructuralCache(OffscreenCanvas value)
    {
        _kind = 2;
        _value1 = default;
        _value2 = value;
    }

    public HTMLCanvasElement? AsHTMLCanvasElement => _kind == 1 ? _value1 : default;

    public OffscreenCanvas? AsOffscreenCanvas => _kind == 2 ? _value2 : default;

    public object? Value => _kind switch
    {
        1 => _value1,
        2 => _value2,
        _ => default
    };

    public static implicit operator StructuralCache(HTMLCanvasElement value)
        => new(value);

    public static implicit operator StructuralCache(OffscreenCanvas value)
        => new(value);
}
