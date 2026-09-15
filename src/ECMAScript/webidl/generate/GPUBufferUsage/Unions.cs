namespace ECMAScript.GPUBufferUsage;

/// <summary>WebIDL 联合值：double[]、GPUColorDict。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUColorCollectionBuilder), nameof(GPUColorCollectionBuilder.Create))]
public readonly union GPUColor(double[], GPUColorDict) : IEnumerable<double>
{

    /// <summary>读取 double[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public double[]? AsDoubleArray => Value is double[] value ? value : default(double[]?);

    /// <summary>读取 GPUColorDict 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUColorDict? AsGPUColorDict => Value is GPUColorDict value ? value : default(GPUColorDict?);

    /// <summary>将 double[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUColor(double[] value)
        => new(value);

    /// <summary>将 GPUColorDict 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUColor(GPUColorDict value)
        => new(value);

    IEnumerator<double> IEnumerable<double>.GetEnumerator()
        => ((IEnumerable<double>)(AsDoubleArray ?? Array.Empty<double>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<double>)this).GetEnumerator();
}

/// <summary>为 GPUColor 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUColorCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static GPUColor Create(ReadOnlySpan<double> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：GPUIntegerCoordinate[]、GPUExtent3DDict。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUExtent3DCollectionBuilder), nameof(GPUExtent3DCollectionBuilder.Create))]
public readonly union GPUExtent3D(GPUIntegerCoordinate[], GPUExtent3DDict) : IEnumerable<GPUIntegerCoordinate>
{

    /// <summary>读取 GPUIntegerCoordinate[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    /// <summary>读取 GPUExtent3DDict 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUExtent3DDict? AsGPUExtent3DDict => Value is GPUExtent3DDict value ? value : default(GPUExtent3DDict?);

    /// <summary>将 GPUIntegerCoordinate[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUExtent3D(GPUIntegerCoordinate[] value)
        => new(value);

    /// <summary>将 GPUExtent3DDict 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUExtent3D(GPUExtent3DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

/// <summary>为 GPUExtent3D 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUExtent3DCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static GPUExtent3D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：GPUIntegerCoordinate[]、GPUOrigin2DDict。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUOrigin2DCollectionBuilder), nameof(GPUOrigin2DCollectionBuilder.Create))]
public readonly union GPUOrigin2D(GPUIntegerCoordinate[], GPUOrigin2DDict) : IEnumerable<GPUIntegerCoordinate>
{

    /// <summary>读取 GPUIntegerCoordinate[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    /// <summary>读取 GPUOrigin2DDict 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUOrigin2DDict? AsGPUOrigin2DDict => Value is GPUOrigin2DDict value ? value : default(GPUOrigin2DDict?);

    /// <summary>将 GPUIntegerCoordinate[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUOrigin2D(GPUIntegerCoordinate[] value)
        => new(value);

    /// <summary>将 GPUOrigin2DDict 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUOrigin2D(GPUOrigin2DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

/// <summary>为 GPUOrigin2D 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUOrigin2DCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static GPUOrigin2D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：GPUIntegerCoordinate[]、GPUOrigin3DDict。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GPUOrigin3DCollectionBuilder), nameof(GPUOrigin3DCollectionBuilder.Create))]
public readonly union GPUOrigin3D(GPUIntegerCoordinate[], GPUOrigin3DDict) : IEnumerable<GPUIntegerCoordinate>
{

    /// <summary>读取 GPUIntegerCoordinate[] 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUIntegerCoordinate[]? AsGPUIntegerCoordinateArray => Value is GPUIntegerCoordinate[] value ? value : default(GPUIntegerCoordinate[]?);

    /// <summary>读取 GPUOrigin3DDict 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUOrigin3DDict? AsGPUOrigin3DDict => Value is GPUOrigin3DDict value ? value : default(GPUOrigin3DDict?);

    /// <summary>将 GPUIntegerCoordinate[] 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUOrigin3D(GPUIntegerCoordinate[] value)
        => new(value);

    /// <summary>将 GPUOrigin3DDict 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUOrigin3D(GPUOrigin3DDict value)
        => new(value);

    IEnumerator<GPUIntegerCoordinate> IEnumerable<GPUIntegerCoordinate>.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)(AsGPUIntegerCoordinateArray ?? Array.Empty<GPUIntegerCoordinate>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<GPUIntegerCoordinate>)this).GetEnumerator();
}

/// <summary>为 GPUOrigin3D 的数组分支提供 C# 集合表达式支持；应用可使用 [item1, item2] 构造该联合值。</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GPUOrigin3DCollectionBuilder
{
    /// <summary>按传入顺序复制元素到新数组，并保存为联合值的数组分支。</summary>
    /// <param name="items">要复制的有序元素；方法不保留临时 Span。</param>
    /// <returns>包含新数组的联合值。</returns>
public static GPUOrigin3D Create(ReadOnlySpan<GPUIntegerCoordinate> items)
        => items.ToArray();
}

/// <summary>WebIDL 联合值：GPUSampler、GPUTexture、GPUTextureView、GPUBuffer、GPUBufferBinding、GPUExternalTexture。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUBindingResource(GPUSampler, GPUTexture, GPUTextureView, GPUBuffer, GPUBufferBinding, GPUExternalTexture)
{

    /// <summary>读取 GPUSampler 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUSampler? AsGPUSampler => Value is GPUSampler value ? value : default(GPUSampler?);

    /// <summary>读取 GPUTexture 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTexture? AsGPUTexture => Value is GPUTexture value ? value : default(GPUTexture?);

    /// <summary>读取 GPUTextureView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTextureView? AsGPUTextureView => Value is GPUTextureView value ? value : default(GPUTextureView?);

    /// <summary>读取 GPUBuffer 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUBuffer? AsGPUBuffer => Value is GPUBuffer value ? value : default(GPUBuffer?);

    /// <summary>读取 GPUBufferBinding 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUBufferBinding? AsGPUBufferBinding => Value is GPUBufferBinding value ? value : default(GPUBufferBinding?);

    /// <summary>读取 GPUExternalTexture 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUExternalTexture? AsGPUExternalTexture => Value is GPUExternalTexture value ? value : default(GPUExternalTexture?);

    /// <summary>将 GPUSampler 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUSampler value)
        => new(value);

    /// <summary>将 GPUTexture 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUTexture value)
        => new(value);

    /// <summary>将 GPUTextureView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUTextureView value)
        => new(value);

    /// <summary>将 GPUBuffer 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUBuffer value)
        => new(value);

    /// <summary>将 GPUBufferBinding 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUBufferBinding value)
        => new(value);

    /// <summary>将 GPUExternalTexture 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUBindingResource(GPUExternalTexture value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUCanvasContextCanvas(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCanvasContextCanvas(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCanvasContextCanvas(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：ImageBitmap、ImageData、HTMLImageElement、HTMLVideoElement、VideoFrame、HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUCopyExternalImageSource(ImageBitmap, ImageData, HTMLImageElement, HTMLVideoElement, VideoFrame, HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 ImageBitmap 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageBitmap? AsImageBitmap => Value is ImageBitmap value ? value : default(ImageBitmap?);

    /// <summary>读取 ImageData 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public ImageData? AsImageData => Value is ImageData value ? value : default(ImageData?);

    /// <summary>读取 HTMLImageElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLImageElement? AsHTMLImageElement => Value is HTMLImageElement value ? value : default(HTMLImageElement?);

    /// <summary>读取 HTMLVideoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    /// <summary>读取 VideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 ImageBitmap 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(ImageBitmap value)
        => new(value);

    /// <summary>将 ImageData 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(ImageData value)
        => new(value);

    /// <summary>将 HTMLImageElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(HTMLImageElement value)
        => new(value);

    /// <summary>将 HTMLVideoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(HTMLVideoElement value)
        => new(value);

    /// <summary>将 VideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(VideoFrame value)
        => new(value);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUCopyExternalImageSource(OffscreenCanvas value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLVideoElement、VideoFrame。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUExternalTextureDescriptorSource(HTMLVideoElement, VideoFrame)
{

    /// <summary>读取 HTMLVideoElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLVideoElement? AsHTMLVideoElement => Value is HTMLVideoElement value ? value : default(HTMLVideoElement?);

    /// <summary>读取 VideoFrame 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public VideoFrame? AsVideoFrame => Value is VideoFrame value ? value : default(VideoFrame?);

    /// <summary>将 HTMLVideoElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUExternalTextureDescriptorSource(HTMLVideoElement value)
        => new(value);

    /// <summary>将 VideoFrame 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUExternalTextureDescriptorSource(VideoFrame value)
        => new(value);
}

/// <summary>WebIDL 联合值：GPUPipelineLayout、GPUAutoLayoutMode。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUPipelineDescriptorBaseLayout(GPUPipelineLayout, GPUAutoLayoutMode)
{

    /// <summary>读取 GPUPipelineLayout 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUPipelineLayout? AsGPUPipelineLayout => Value is GPUPipelineLayout value ? value : default(GPUPipelineLayout?);

    /// <summary>读取 GPUAutoLayoutMode 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUAutoLayoutMode? AsGPUAutoLayoutMode => Value is GPUAutoLayoutMode value ? value : default(GPUAutoLayoutMode?);

    /// <summary>将 GPUPipelineLayout 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUPipelineDescriptorBaseLayout(GPUPipelineLayout value)
        => new(value);

    /// <summary>将 GPUAutoLayoutMode 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUPipelineDescriptorBaseLayout(GPUAutoLayoutMode value)
        => new(value);
}

/// <summary>WebIDL 联合值：GPUTexture、GPUTextureView。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPURenderPassColorAttachmentResolveTarget(GPUTexture, GPUTextureView)
{

    /// <summary>读取 GPUTexture 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTexture? AsGPUTexture => Value is GPUTexture value ? value : default(GPUTexture?);

    /// <summary>读取 GPUTextureView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTextureView? AsGPUTextureView => Value is GPUTextureView value ? value : default(GPUTextureView?);

    /// <summary>将 GPUTexture 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassColorAttachmentResolveTarget(GPUTexture value)
        => new(value);

    /// <summary>将 GPUTextureView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassColorAttachmentResolveTarget(GPUTextureView value)
        => new(value);
}

/// <summary>WebIDL 联合值：GPUTexture、GPUTextureView。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPURenderPassColorAttachmentView(GPUTexture, GPUTextureView)
{

    /// <summary>读取 GPUTexture 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTexture? AsGPUTexture => Value is GPUTexture value ? value : default(GPUTexture?);

    /// <summary>读取 GPUTextureView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTextureView? AsGPUTextureView => Value is GPUTextureView value ? value : default(GPUTextureView?);

    /// <summary>将 GPUTexture 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassColorAttachmentView(GPUTexture value)
        => new(value);

    /// <summary>将 GPUTextureView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassColorAttachmentView(GPUTextureView value)
        => new(value);
}

/// <summary>WebIDL 联合值：GPUTexture、GPUTextureView。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPURenderPassDepthStencilAttachmentView(GPUTexture, GPUTextureView)
{

    /// <summary>读取 GPUTexture 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTexture? AsGPUTexture => Value is GPUTexture value ? value : default(GPUTexture?);

    /// <summary>读取 GPUTextureView 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUTextureView? AsGPUTextureView => Value is GPUTextureView value ? value : default(GPUTextureView?);

    /// <summary>将 GPUTexture 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassDepthStencilAttachmentView(GPUTexture value)
        => new(value);

    /// <summary>将 GPUTextureView 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPURenderPassDepthStencilAttachmentView(GPUTextureView value)
        => new(value);
}

/// <summary>WebIDL 联合值：GPUPipelineLayout、GPUAutoLayoutMode。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union GPUShaderModuleCompilationHintLayout(GPUPipelineLayout, GPUAutoLayoutMode)
{

    /// <summary>读取 GPUPipelineLayout 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUPipelineLayout? AsGPUPipelineLayout => Value is GPUPipelineLayout value ? value : default(GPUPipelineLayout?);

    /// <summary>读取 GPUAutoLayoutMode 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public GPUAutoLayoutMode? AsGPUAutoLayoutMode => Value is GPUAutoLayoutMode value ? value : default(GPUAutoLayoutMode?);

    /// <summary>将 GPUPipelineLayout 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUShaderModuleCompilationHintLayout(GPUPipelineLayout value)
        => new(value);

    /// <summary>将 GPUAutoLayoutMode 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator GPUShaderModuleCompilationHintLayout(GPUAutoLayoutMode value)
        => new(value);
}

/// <summary>WebIDL 联合值：HTMLCanvasElement、OffscreenCanvas。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取对应分支。</summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly union StructuralCache(HTMLCanvasElement, OffscreenCanvas)
{

    /// <summary>读取 HTMLCanvasElement 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public HTMLCanvasElement? AsHTMLCanvasElement => Value is HTMLCanvasElement value ? value : default(HTMLCanvasElement?);

    /// <summary>读取 OffscreenCanvas 分支；当前值不属于该分支时返回 null，不进行类型强制转换。</summary>
public OffscreenCanvas? AsOffscreenCanvas => Value is OffscreenCanvas value ? value : default(OffscreenCanvas?);

    /// <summary>将 HTMLCanvasElement 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(HTMLCanvasElement value)
        => new(value);

    /// <summary>将 OffscreenCanvas 保存为联合值的对应分支，保留输入值。</summary>
    /// <param name="value">要保存到该分支的值。</param>
    /// <returns>保存该分支值的联合值。</returns>
public static implicit operator StructuralCache(OffscreenCanvas value)
        => new(value);
}