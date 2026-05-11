namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUBindingResource
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUBindingResource(GPUSampler, GPUTextureView, GPUBufferBinding, GPUExternalTexture)
{

    public GPUSampler? AsGPUSampler => Value is GPUSampler value ? value : default(GPUSampler?);

    public GPUTextureView? AsGPUTextureView => Value is GPUTextureView value ? value : default(GPUTextureView?);

    public GPUBufferBinding? AsGPUBufferBinding => Value is GPUBufferBinding value ? value : default(GPUBufferBinding?);

    public GPUExternalTexture? AsGPUExternalTexture => Value is GPUExternalTexture value ? value : default(GPUExternalTexture?);

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
public readonly union GPUCanvasContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

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
public readonly union GPUColor(double[], GPUColorDict) : IEnumerable<double>
{

    public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    public GPUColorDict? AsGPUColorDict => Value is GPUColorDict value ? value : default(GPUColorDict?);

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
public readonly union GPUExtent3D(GPUIntegerCoordinate[], GPUExtent3DDict) : IEnumerable<GPUIntegerCoordinate>
{

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    public GPUExtent3DDict? AsGPUExtent3DDict => Value is GPUExtent3DDict value ? value : default(GPUExtent3DDict?);

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
public readonly union GPUExternalTextureDescriptorSource(HTMLVideoElement, VideoFrame)
{

    public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

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
public readonly union GPUImageCopyExternalImageSource(ImageBitmap, ImageData, HTMLImageElement, HTMLVideoElement, VideoFrame, HTMLCanvasElement, OffscreenCanvas)
{

    public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

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
public readonly union GPUOrigin2D(GPUIntegerCoordinate[], GPUOrigin2DDict) : IEnumerable<GPUIntegerCoordinate>
{

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    public GPUOrigin2DDict? AsGPUOrigin2DDict => Value is GPUOrigin2DDict value ? value : default(GPUOrigin2DDict?);

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
public readonly union GPUOrigin3D(GPUIntegerCoordinate[], GPUOrigin3DDict) : IEnumerable<GPUIntegerCoordinate>
{

    public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    public GPUOrigin3DDict? AsGPUOrigin3DDict => Value is GPUOrigin3DDict value ? value : default(GPUOrigin3DDict?);

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
public readonly union GPUPipelineDescriptorBaseLayout(GPUPipelineLayout, GPUAutoLayoutMode)
{

    public GPUPipelineLayout? AsGPUPipelineLayout => Value is GPUPipelineLayout value ? value : default(GPUPipelineLayout?);

    public GPUAutoLayoutMode? AsGPUAutoLayoutMode => Value is GPUAutoLayoutMode value ? value : default(GPUAutoLayoutMode?);

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
public readonly union GPUShaderModuleCompilationHintLayout(GPUPipelineLayout, GPUAutoLayoutMode)
{

    public GPUPipelineLayout? AsGPUPipelineLayout => Value is GPUPipelineLayout value ? value : default(GPUPipelineLayout?);

    public GPUAutoLayoutMode? AsGPUAutoLayoutMode => Value is GPUAutoLayoutMode value ? value : default(GPUAutoLayoutMode?);

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
public readonly union StructuralCache(HTMLCanvasElement, OffscreenCanvas)
{

    public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    public static implicit operator StructuralCache(HTMLCanvasElement value)
        => new(value);

    public static implicit operator StructuralCache(OffscreenCanvas value)
        => new(value);
}
