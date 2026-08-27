namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpu">WebGPU: 4.2 GPU</see>
/// </summary>
[ECMAScript]
[Description("@#GPU")]
public class GPU
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpu-requestadapter">WebGPU: 4.2 GPU</see>
    /// </summary>
    /// <param name="options"><see href="https://gpuweb.github.io/gpuweb/#dom-gpu-requestadapter-options-options">WebGPU: 4.2 GPU</see></param>
    [Description("@#requestAdapter")]
    public extern PromiseResult<GPUAdapter?> RequestAdapter(GPURequestAdapterOptions? options = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpu-getpreferredcanvasformat">WebGPU: 4.2 GPU</see>
    /// </summary>
    [Description("@#getPreferredCanvasFormat")]
    public extern GPUTextureFormat GetPreferredCanvasFormat();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpu-wgsllanguagefeatures">WebGPU: 4.2 GPU</see>
    /// </summary>
    [Description("@#wgslLanguageFeatures")]
    public extern WGSLLanguageFeatures WgslLanguageFeatures { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuadapter">WebGPU: 4.3 GPUAdapter</see>
/// </summary>
[ECMAScript]
[Description("@#GPUAdapter")]
public class GPUAdapter
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapter-features">WebGPU: 4.3 GPUAdapter</see>
    /// </summary>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapter-limits">WebGPU: 4.3 GPUAdapter</see>
    /// </summary>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapter-info">WebGPU: 4.3 GPUAdapter</see>
    /// </summary>
    [Description("@#info")]
    public extern GPUAdapterInfo Info { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapter-requestdevice">WebGPU: 4.3 GPUAdapter</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapter-requestdevice-descriptor-descriptor">WebGPU: 4.3 GPUAdapter</see></param>
    [Description("@#requestDevice")]
    public extern PromiseResult<GPUDevice> RequestDevice(GPUDeviceDescriptor? descriptor = default);
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuadapterinfo">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
/// </summary>
[ECMAScript]
[Description("@#GPUAdapterInfo")]
public class GPUAdapterInfo
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-vendor">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#vendor")]
    public extern string Vendor { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-architecture">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#architecture")]
    public extern string Architecture { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-device">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#device")]
    public extern string Device { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-description">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#description")]
    public extern string Description { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-subgroupminsize">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#subgroupMinSize")]
    public extern uint SubgroupMinSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-subgroupmaxsize">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#subgroupMaxSize")]
    public extern uint SubgroupMaxSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuadapterinfo-isfallbackadapter">WebGPU: 3.6.2.4 GPUAdapterInfo</see>
    /// </summary>
    [Description("@#isFallbackAdapter")]
    public extern bool IsFallbackAdapter { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpubindgroup">WebGPU: 8.2 GPUBindGroup</see>
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroup")]
public class GPUBindGroup
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpubindgrouplayout">WebGPU: 8.1 GPUBindGroupLayout</see>
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupLayout")]
public class GPUBindGroupLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpubuffer">WebGPU: 5.1 GPUBuffer</see>
/// </summary>
[ECMAScript]
[Description("@#GPUBuffer")]
public class GPUBuffer
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-size">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#size")]
    public extern GPUSize64Out Size { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-usage">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-mapstate">WebGPU: 5.1 GPUBuffer</see>
    /// </summary>
    [Description("@#mapState")]
    public extern GPUBufferMapState MapState { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-mapasync">WebGPU: 5.2 Buffer Mapping</see>
    /// </summary>
    /// <param name="mode"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-mapasync-mode-offset-size-mode">WebGPU: 5.2 Buffer Mapping</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-mapasync-mode-offset-size-offset">WebGPU: 5.2 Buffer Mapping</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-mapasync-mode-offset-size-size">WebGPU: 5.2 Buffer Mapping</see></param>
    [Description("@#mapAsync")]
    public extern PromiseResult MapAsync(GPUMapModeFlags mode, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-getmappedrange">WebGPU: 5.2 Buffer Mapping</see>
    /// </summary>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-getmappedrange-offset-size-offset">WebGPU: 5.2 Buffer Mapping</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-getmappedrange-offset-size-size">WebGPU: 5.2 Buffer Mapping</see></param>
    [Description("@#getMappedRange")]
    public extern ArrayBuffer GetMappedRange(GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-unmap">WebGPU: 5.2 Buffer Mapping</see>
    /// </summary>
    [Description("@#unmap")]
    public extern void Unmap();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubuffer-destroy">WebGPU: 5.1.4 Buffer Destruction</see>
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucanvascontext">WebGPU: 21.2 GPUCanvasContext</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCanvasContext")]
public class GPUCanvasContext
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-canvas">WebGPU: 21.2 GPUCanvasContext</see>
    /// </summary>
    [Description("@#canvas")]
    public extern GPUCanvasContextCanvas Canvas { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-configure">WebGPU: 21.2 GPUCanvasContext</see>
    /// </summary>
    /// <param name="configuration"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-configure-configuration-configuration">WebGPU: 21.2 GPUCanvasContext</see></param>
    [Description("@#configure")]
    public extern void Configure(GPUCanvasConfiguration configuration);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-unconfigure">WebGPU: 21.2 GPUCanvasContext</see>
    /// </summary>
    [Description("@#unconfigure")]
    public extern void Unconfigure();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-getconfiguration">WebGPU: 21.2 GPUCanvasContext</see>
    /// </summary>
    [Description("@#getConfiguration")]
    public extern GPUCanvasConfiguration? GetConfiguration();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvascontext-getcurrenttexture">WebGPU: 21.2 GPUCanvasContext</see>
    /// </summary>
    [Description("@#getCurrentTexture")]
    public extern GPUTexture GetCurrentTexture();
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucommandbuffer">WebGPU: 12.1 GPUCommandBuffer</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCommandBuffer")]
public class GPUCommandBuffer
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucommandencoder">WebGPU: 13.2 GPUCommandEncoder</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCommandEncoder")]
public class GPUCommandEncoder
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-beginrenderpass">WebGPU: 13.3 Pass Encoding</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-beginrenderpass-descriptor-descriptor">WebGPU: 13.3 Pass Encoding</see></param>
    [Description("@#beginRenderPass")]
    public extern GPURenderPassEncoder BeginRenderPass(GPURenderPassDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-begincomputepass">WebGPU: 13.3 Pass Encoding</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-begincomputepass-descriptor-descriptor">WebGPU: 13.3 Pass Encoding</see></param>
    [Description("@#beginComputePass")]
    public extern GPUComputePassEncoder BeginComputePass(GPUComputePassDescriptor? descriptor = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer">WebGPU: 13.4 Buffer Copy Commands</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-destination-size-source">WebGPU: 13.2 GPUCommandEncoder</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-destination-size-destination">WebGPU: 13.2 GPUCommandEncoder</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-destination-size-size">WebGPU: 13.2 GPUCommandEncoder</see></param>
    [Description("@#copyBufferToBuffer")]
    public extern void CopyBufferToBuffer(GPUBuffer source, GPUBuffer destination, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size">WebGPU: 13.4 Buffer Copy Commands</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size-source">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="sourceOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size-sourceoffset">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size-destination">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="destinationOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size-destinationoffset">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertobuffer-source-sourceoffset-destination-destinationoffset-size-size">WebGPU: 13.4 Buffer Copy Commands</see></param>
    [Description("@#copyBufferToBuffer")]
    public extern void CopyBufferToBuffer(GPUBuffer source, GPUSize64 sourceOffset, GPUBuffer destination, GPUSize64 destinationOffset, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertotexture">WebGPU: 13.5 Texel Copy Commands</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertotexture-source-destination-copysize-source">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertotexture-source-destination-copysize-destination">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="copySize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copybuffertotexture-source-destination-copysize-copysize">WebGPU: 13.5 Texel Copy Commands</see></param>
    [Description("@#copyBufferToTexture")]
    public extern void CopyBufferToTexture(GPUTexelCopyBufferInfo source, GPUTexelCopyTextureInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetobuffer">WebGPU: 13.5 Texel Copy Commands</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetobuffer-source-destination-copysize-source">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetobuffer-source-destination-copysize-destination">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="copySize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetobuffer-source-destination-copysize-copysize">WebGPU: 13.5 Texel Copy Commands</see></param>
    [Description("@#copyTextureToBuffer")]
    public extern void CopyTextureToBuffer(GPUTexelCopyTextureInfo source, GPUTexelCopyBufferInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetotexture">WebGPU: 13.5 Texel Copy Commands</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetotexture-source-destination-copysize-source">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetotexture-source-destination-copysize-destination">WebGPU: 13.5 Texel Copy Commands</see></param>
    /// <param name="copySize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-copytexturetotexture-source-destination-copysize-copysize">WebGPU: 13.5 Texel Copy Commands</see></param>
    [Description("@#copyTextureToTexture")]
    public extern void CopyTextureToTexture(GPUTexelCopyTextureInfo source, GPUTexelCopyTextureInfo destination, GPUExtent3D copySize);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-clearbuffer">WebGPU: 13.4 Buffer Copy Commands</see>
    /// </summary>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-clearbuffer-buffer-offset-size-buffer">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-clearbuffer-buffer-offset-size-offset">WebGPU: 13.4 Buffer Copy Commands</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-clearbuffer-buffer-offset-size-size">WebGPU: 13.4 Buffer Copy Commands</see></param>
    [Description("@#clearBuffer")]
    public extern void ClearBuffer(GPUBuffer buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset">WebGPU: 13.6 Queries</see>
    /// </summary>
    /// <param name="querySet"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset-queryset-firstquery-querycount-destination-destinationoffset-queryset">WebGPU: 13.6 Queries</see></param>
    /// <param name="firstQuery"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset-queryset-firstquery-querycount-destination-destinationoffset-firstquery">WebGPU: 13.6 Queries</see></param>
    /// <param name="queryCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset-queryset-firstquery-querycount-destination-destinationoffset-querycount">WebGPU: 13.6 Queries</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset-queryset-firstquery-querycount-destination-destinationoffset-destination">WebGPU: 13.6 Queries</see></param>
    /// <param name="destinationOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-resolvequeryset-queryset-firstquery-querycount-destination-destinationoffset-destinationoffset">WebGPU: 13.6 Queries</see></param>
    [Description("@#resolveQuerySet")]
    public extern void ResolveQuerySet(GPUQuerySet querySet, GPUSize32 firstQuery, GPUSize32 queryCount, GPUBuffer destination, GPUSize64 destinationOffset);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-finish">WebGPU: 13.7 Finalization</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucommandencoder-finish-descriptor-descriptor">WebGPU: 13.7 Finalization</see></param>
    [Description("@#finish")]
    public extern GPUCommandBuffer Finish(GPUCommandBufferDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucompilationinfo">WebGPU: 9.1.2 Shader Module Compilation Information</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCompilationInfo")]
public class GPUCompilationInfo
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationinfo-messages">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#messages")]
    public extern FrozenSet<GPUCompilationMessage> Messages { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucompilationmessage">WebGPU: 9.1.2 Shader Module Compilation Information</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCompilationMessage")]
public class GPUCompilationMessage
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-message">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-type">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#type")]
    public extern GPUCompilationMessageType Type { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-linenum">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#lineNum")]
    public extern Number LineNum { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-linepos">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#linePos")]
    public extern Number LinePos { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-offset">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#offset")]
    public extern Number Offset { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucompilationmessage-length">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#length")]
    public extern Number Length { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucomputepassencoder">WebGPU: 16.1 GPUComputePassEncoder</see>
/// </summary>
[ECMAScript]
[Description("@#GPUComputePassEncoder")]
public class GPUComputePassEncoder
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-setpipeline">WebGPU: 16.1.2 Dispatch</see>
    /// </summary>
    /// <param name="pipeline"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-setpipeline-pipeline-pipeline">WebGPU: 16.1.2 Dispatch</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPUComputePipeline pipeline);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroups">WebGPU: 16.1.2 Dispatch</see>
    /// </summary>
    /// <param name="workgroupCountX"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroups-workgroupcountx-workgroupcounty-workgroupcountz-workgroupcountx">WebGPU: 16.1.2 Dispatch</see></param>
    /// <param name="workgroupCountY"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroups-workgroupcountx-workgroupcounty-workgroupcountz-workgroupcounty">WebGPU: 16.1.2 Dispatch</see></param>
    /// <param name="workgroupCountZ"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroups-workgroupcountx-workgroupcounty-workgroupcountz-workgroupcountz">WebGPU: 16.1.2 Dispatch</see></param>
    [Description("@#dispatchWorkgroups")]
    public extern void DispatchWorkgroups(GPUSize32 workgroupCountX, GPUSize32? workgroupCountY = default, GPUSize32? workgroupCountZ = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroupsindirect">WebGPU: 16.1.2 Dispatch</see>
    /// </summary>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroupsindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 16.1.2 Dispatch</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-dispatchworkgroupsindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 16.1.2 Dispatch</see></param>
    [Description("@#dispatchWorkgroupsIndirect")]
    public extern void DispatchWorkgroupsIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassencoder-end">WebGPU: 16.1.3 Finalization</see>
    /// </summary>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </summary>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucomputepipeline">WebGPU: 10.2 GPUComputePipeline</see>
/// </summary>
[ECMAScript]
[Description("@#GPUComputePipeline")]
public class GPUComputePipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout">WebGPU: 10.1 Base pipelines</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout-index-index">WebGPU: 10.1 Base pipelines</see></param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpudevice">WebGPU: 4.4 GPUDevice</see>
/// </summary>
[ECMAScript]
[Description("@#GPUDevice")]
public partial class GPUDevice : EventTarget
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-features">WebGPU: 4.4 GPUDevice</see>
    /// </summary>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-limits">WebGPU: 4.4 GPUDevice</see>
    /// </summary>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-adapterinfo">WebGPU: 4.4 GPUDevice</see>
    /// </summary>
    [Description("@#adapterInfo")]
    public extern GPUAdapterInfo AdapterInfo { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-queue">WebGPU: 4.4 GPUDevice</see>
    /// </summary>
    [Description("@#queue")]
    public extern GPUQueue Queue { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-destroy">WebGPU: 4.4 GPUDevice</see>
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbuffer">WebGPU: 5.1.3 Buffer Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbuffer-descriptor-descriptor">WebGPU: 5.1.3 Buffer Creation</see></param>
    [Description("@#createBuffer")]
    public extern GPUBuffer CreateBuffer(GPUBufferDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createtexture">WebGPU: 6.1.3 Texture Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createtexture-descriptor-descriptor">WebGPU: 6.1.3 Texture Creation</see></param>
    [Description("@#createTexture")]
    public extern GPUTexture CreateTexture(GPUTextureDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createsampler">WebGPU: 7.1.2 Sampler Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createsampler-descriptor-descriptor">WebGPU: 7.1.2 Sampler Creation</see></param>
    [Description("@#createSampler")]
    public extern GPUSampler CreateSampler(GPUSamplerDescriptor? descriptor = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-importexternaltexture">WebGPU: 6.4.1 Importing External Textures</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-importexternaltexture-descriptor-descriptor">WebGPU: 6.4.1 Importing External Textures</see></param>
    [Description("@#importExternalTexture")]
    public extern GPUExternalTexture ImportExternalTexture(GPUExternalTextureDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbindgrouplayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbindgrouplayout-descriptor-descriptor">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
    [Description("@#createBindGroupLayout")]
    public extern GPUBindGroupLayout CreateBindGroupLayout(GPUBindGroupLayoutDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createpipelinelayout">WebGPU: 8.3.1 Pipeline Layout Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createpipelinelayout-descriptor-descriptor">WebGPU: 8.3.1 Pipeline Layout Creation</see></param>
    [Description("@#createPipelineLayout")]
    public extern GPUPipelineLayout CreatePipelineLayout(GPUPipelineLayoutDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbindgroup">WebGPU: 8.2.1 Bind Group Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createbindgroup-descriptor-descriptor">WebGPU: 8.2.1 Bind Group Creation</see></param>
    [Description("@#createBindGroup")]
    public extern GPUBindGroup CreateBindGroup(GPUBindGroupDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createshadermodule">WebGPU: 9.1.1 Shader Module Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createshadermodule-descriptor-descriptor">WebGPU: 9.1.1 Shader Module Creation</see></param>
    [Description("@#createShaderModule")]
    public extern GPUShaderModule CreateShaderModule(GPUShaderModuleDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcomputepipeline">WebGPU: 10.2.1 Compute Pipeline Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcomputepipeline-descriptor-descriptor">WebGPU: 10.2.1 Compute Pipeline Creation</see></param>
    [Description("@#createComputePipeline")]
    public extern GPUComputePipeline CreateComputePipeline(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderpipeline">WebGPU: 10.3.1 Render Pipeline Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderpipeline-descriptor-descriptor">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
    [Description("@#createRenderPipeline")]
    public extern GPURenderPipeline CreateRenderPipeline(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcomputepipelineasync">WebGPU: 10.2.1 Compute Pipeline Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcomputepipelineasync-descriptor-descriptor">WebGPU: 10.2.1 Compute Pipeline Creation</see></param>
    [Description("@#createComputePipelineAsync")]
    public extern PromiseResult<GPUComputePipeline> CreateComputePipelineAsync(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderpipelineasync">WebGPU: 10.3.1 Render Pipeline Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderpipelineasync-descriptor-descriptor">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
    [Description("@#createRenderPipelineAsync")]
    public extern PromiseResult<GPURenderPipeline> CreateRenderPipelineAsync(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcommandencoder">WebGPU: 13.2.1 Command Encoder Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createcommandencoder-descriptor-descriptor">WebGPU: 13.2.1 Command Encoder Creation</see></param>
    [Description("@#createCommandEncoder")]
    public extern GPUCommandEncoder CreateCommandEncoder(GPUCommandEncoderDescriptor? descriptor = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderbundleencoder">WebGPU: 18.1.1 Render Bundle Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createrenderbundleencoder-descriptor-descriptor">WebGPU: 18.1.1 Render Bundle Creation</see></param>
    [Description("@#createRenderBundleEncoder")]
    public extern GPURenderBundleEncoder CreateRenderBundleEncoder(GPURenderBundleEncoderDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createqueryset">WebGPU: 20.1.1 QuerySet Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-createqueryset-descriptor-descriptor">WebGPU: 20.1.1 QuerySet Creation</see></param>
    [Description("@#createQuerySet")]
    public extern GPUQuerySet CreateQuerySet(GPUQuerySetDescriptor descriptor);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-lost">WebGPU: 22.1 Fatal Errors</see>
    /// </summary>
    [Description("@#lost")]
    public extern PromiseResult<GPUDeviceLostInfo> Lost { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-pusherrorscope">WebGPU: 22.3 Error Scopes</see>
    /// </summary>
    /// <param name="filter"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-pusherrorscope-filter-filter">WebGPU: 22.3 Error Scopes</see></param>
    [Description("@#pushErrorScope")]
    public extern void PushErrorScope(GPUErrorFilter filter);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-poperrorscope">WebGPU: 22.3 Error Scopes</see>
    /// </summary>
    [Description("@#popErrorScope")]
    public extern PromiseResult<GPUError?> PopErrorScope();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevice-onuncapturederror">WebGPU: 22.4 Telemetry</see>
    /// </summary>
    [Description("@#onuncapturederror")]
    public extern EventHandler Onuncapturederror { get; set; }

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpudevicelostinfo">WebGPU: 22.1 Fatal Errors</see>
/// </summary>
[ECMAScript]
[Description("@#GPUDeviceLostInfo")]
public class GPUDeviceLostInfo
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicelostinfo-reason">WebGPU: 22.1 Fatal Errors</see>
    /// </summary>
    [Description("@#reason")]
    public extern GPUDeviceLostReason Reason { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicelostinfo-message">WebGPU: 22.1 Fatal Errors</see>
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuerror">WebGPU: 22.2 GPUError</see>
/// </summary>
[ECMAScript]
[Description("@#GPUError")]
public class GPUError
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuerror-message">WebGPU: 22.2 GPUError</see>
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuexternaltexture">WebGPU: 6.4 GPUExternalTexture</see>
/// </summary>
[ECMAScript]
[Description("@#GPUExternalTexture")]
public class GPUExternalTexture
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuinternalerror">WebGPU: 22.2 GPUError</see>
/// </summary>
[ECMAScript]
[Description("@#GPUInternalError")]
public class GPUInternalError : GPUError
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuinternalerror-gpuinternalerror">WebGPU: 22.2 GPUError</see>
    /// </summary>
    /// <param name="message"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuinternalerror-gpuinternalerror-message-message">WebGPU: 22.2 GPUError</see></param>
    public extern GPUInternalError(string message);
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuoutofmemoryerror">WebGPU: 22.2 GPUError</see>
/// </summary>
[ECMAScript]
[Description("@#GPUOutOfMemoryError")]
public class GPUOutOfMemoryError : GPUError
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuoutofmemoryerror-gpuoutofmemoryerror">WebGPU: 22.2 GPUError</see>
    /// </summary>
    /// <param name="message"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuoutofmemoryerror-gpuoutofmemoryerror-message-message">WebGPU: 22.2 GPUError</see></param>
    public extern GPUOutOfMemoryError(string message);
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpupipelineerror">WebGPU: 10. Pipelines</see>
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineError")]
public class GPUPipelineError(string message, string name) : DOMException(message, name)
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerror-constructor">WebGPU: 10. Pipelines</see>
    /// </summary>
    /// <param name="message"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerror-constructor-message">WebGPU: 10. Pipelines</see></param>
    /// <param name="options"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerror-constructor-options">WebGPU: 10. Pipelines</see></param>
    public extern GPUPipelineError(string message = "", GPUPipelineErrorInit? options = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerror-reason">WebGPU: 10. Pipelines</see>
    /// </summary>
    [Description("@#reason")]
    public extern GPUPipelineErrorReason Reason { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpupipelinelayout">WebGPU: 8.3 GPUPipelineLayout</see>
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineLayout")]
public class GPUPipelineLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuqueryset">WebGPU: 20.1 GPUQuerySet</see>
/// </summary>
[ECMAScript]
[Description("@#GPUQuerySet")]
public class GPUQuerySet
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueryset-destroy">WebGPU: 20.1.2 Query Set Destruction</see>
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueryset-type">WebGPU: 20.1 GPUQuerySet</see>
    /// </summary>
    [Description("@#type")]
    public extern GPUQueryType Type { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueryset-count">WebGPU: 20.1 GPUQuerySet</see>
    /// </summary>
    [Description("@#count")]
    public extern GPUSize32Out Count { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuqueue">WebGPU: 19.2 GPUQueue</see>
/// </summary>
[ECMAScript]
[Description("@#GPUQueue")]
public class GPUQueue
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-submit">WebGPU: 19.2 GPUQueue</see>
    /// </summary>
    /// <param name="commandBuffers"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-submit-commandbuffers-commandbuffers">WebGPU: 19.2 GPUQueue</see></param>
    [Description("@#submit")]
    public extern void Submit(GPUCommandBuffer[] commandBuffers);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-onsubmittedworkdone">WebGPU: 19.2 GPUQueue</see>
    /// </summary>
    [Description("@#onSubmittedWorkDone")]
    public extern PromiseResult OnSubmittedWorkDone();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer">WebGPU: 19.2 GPUQueue</see>
    /// </summary>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer-buffer-bufferoffset-data-dataoffset-size-buffer">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="bufferOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer-buffer-bufferoffset-data-dataoffset-size-bufferoffset">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer-buffer-bufferoffset-data-dataoffset-size-data">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer-buffer-bufferoffset-data-dataoffset-size-dataoffset">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writebuffer-buffer-bufferoffset-data-dataoffset-size-size">WebGPU: 19.2 GPUQueue</see></param>
    [Description("@#writeBuffer")]
    public extern void WriteBuffer(GPUBuffer buffer, GPUSize64 bufferOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writetexture">WebGPU: 19.2 GPUQueue</see>
    /// </summary>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writetexture-destination-data-datalayout-size-destination">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writetexture-destination-data-datalayout-size-data">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="dataLayout"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writetexture-destination-data-datalayout-size-datalayout">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-writetexture-destination-data-datalayout-size-size">WebGPU: 19.2 GPUQueue</see></param>
    [Description("@#writeTexture")]
    public extern void WriteTexture(GPUTexelCopyTextureInfo destination, IAllowSharedBufferSource data, GPUTexelCopyBufferLayout dataLayout, GPUExtent3D size);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-copyexternalimagetotexture">WebGPU: 19.2 GPUQueue</see>
    /// </summary>
    /// <param name="source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-copyexternalimagetotexture-source-destination-copysize-source">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="destination"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-copyexternalimagetotexture-source-destination-copysize-destination">WebGPU: 19.2 GPUQueue</see></param>
    /// <param name="copySize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuqueue-copyexternalimagetotexture-source-destination-copysize-copysize">WebGPU: 19.2 GPUQueue</see></param>
    [Description("@#copyExternalImageToTexture")]
    public extern void CopyExternalImageToTexture(GPUCopyExternalImageSourceInfo source, GPUCopyExternalImageDestInfo destination, GPUExtent3D copySize);

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpurenderbundle">WebGPU: 18.1 GPURenderBundle</see>
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundle")]
public class GPURenderBundle
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpurenderbundleencoder">WebGPU: 18.1.1 Render Bundle Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundleEncoder")]
public class GPURenderBundleEncoder
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoder-finish">WebGPU: 18.1.3 Finalization</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoder-finish-descriptor-descriptor">WebGPU: 18.1.3 Finalization</see></param>
    [Description("@#finish")]
    public extern GPURenderBundle Finish(GPURenderBundleDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </summary>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="pipeline"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline-pipeline-pipeline">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indexFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-indexformat">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="slot"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-slot">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="vertexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-vertexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstvertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-indexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstindex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="baseVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-basevertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpurenderpassencoder">WebGPU: 17.1 GPURenderPassEncoder</see>
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassEncoder")]
public class GPURenderPassEncoder
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport">WebGPU: 17.2.2 Rasterization state</see>
    /// </summary>
    /// <param name="x"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-x">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="y"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-y">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="width"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-width">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="height"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-height">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="minDepth"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-mindepth">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="maxDepth"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setviewport-x-y-width-height-mindepth-maxdepth-maxdepth">WebGPU: 17.2.2 Rasterization state</see></param>
    [Description("@#setViewport")]
    public extern void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setscissorrect">WebGPU: 17.2.2 Rasterization state</see>
    /// </summary>
    /// <param name="x"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setscissorrect-x-y-width-height-x">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="y"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setscissorrect-x-y-width-height-y">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="width"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setscissorrect-x-y-width-height-width">WebGPU: 17.2.2 Rasterization state</see></param>
    /// <param name="height"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setscissorrect-x-y-width-height-height">WebGPU: 17.2.2 Rasterization state</see></param>
    [Description("@#setScissorRect")]
    public extern void SetScissorRect(GPUIntegerCoordinate x, GPUIntegerCoordinate y, GPUIntegerCoordinate width, GPUIntegerCoordinate height);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setblendconstant">WebGPU: 17.2.2 Rasterization state</see>
    /// </summary>
    /// <param name="color"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setblendconstant-color-color">WebGPU: 17.2.2 Rasterization state</see></param>
    [Description("@#setBlendConstant")]
    public extern void SetBlendConstant(GPUColor color);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setstencilreference">WebGPU: 17.2.2 Rasterization state</see>
    /// </summary>
    /// <param name="reference"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-setstencilreference-reference-reference">WebGPU: 17.2.2 Rasterization state</see></param>
    [Description("@#setStencilReference")]
    public extern void SetStencilReference(GPUStencilValue reference);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-beginocclusionquery">WebGPU: 17.2.3 Queries</see>
    /// </summary>
    /// <param name="queryIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-beginocclusionquery-queryindex-queryindex">WebGPU: 17.2.3 Queries</see></param>
    [Description("@#beginOcclusionQuery")]
    public extern void BeginOcclusionQuery(GPUSize32 queryIndex);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-endocclusionquery">WebGPU: 17.2.3 Queries</see>
    /// </summary>
    [Description("@#endOcclusionQuery")]
    public extern void EndOcclusionQuery();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-executebundles">WebGPU: 17.2.4 Bundles</see>
    /// </summary>
    /// <param name="bundles"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-executebundles-bundles-bundles">WebGPU: 17.2.4 Bundles</see></param>
    [Description("@#executeBundles")]
    public extern void ExecuteBundles(GPURenderBundle[] bundles);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassencoder-end">WebGPU: 17.1.2 Finalization</see>
    /// </summary>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="groupLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-pushdebuggroup-grouplabel-grouplabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-popdebuggroup">WebGPU: 15. Debug Markers</see>
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker">WebGPU: 15. Debug Markers</see>
    /// </summary>
    /// <param name="markerLabel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudebugcommandsmixin-insertdebugmarker-markerlabel-markerlabel">WebGPU: 15. Debug Markers</see></param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsets-dynamicoffsets">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-index">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="bindGroup"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-bindgroup">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsData"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdata">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataStart"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatastart">WebGPU: 14.1 Bind Groups</see></param>
    /// <param name="dynamicOffsetsDataLength"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setbindgroup-index-bindgroup-dynamicoffsetsdata-dynamicoffsetsdatastart-dynamicoffsetsdatalength-dynamicoffsetsdatalength">WebGPU: 14.1 Bind Groups</see></param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates">WebGPU: 14.2 Immediate Data</see>
    /// </summary>
    /// <param name="rangeOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-rangeoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="data"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-data">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-dataoffset">WebGPU: 14.2 Immediate Data</see></param>
    /// <param name="dataSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindingcommandsmixin-setimmediates-rangeoffset-data-dataoffset-datasize-datasize">WebGPU: 14.2 Immediate Data</see></param>
    [Description("@#setImmediates")]
    public extern void SetImmediates(GPUSize32 rangeOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? dataSize = default);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="pipeline"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setpipeline-pipeline-pipeline">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indexFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-indexformat">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setindexbuffer-buffer-indexformat-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="slot"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-slot">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-buffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-offset">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-setvertexbuffer-slot-buffer-offset-size-size">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="vertexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-vertexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstvertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-draw-vertexcount-instancecount-firstvertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indexCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-indexcount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="instanceCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-instancecount">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstindex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="baseVertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-basevertex">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="firstInstance"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexed-indexcount-instancecount-firstindex-basevertex-firstinstance-firstinstance">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect">WebGPU: 17.2.1 Drawing</see>
    /// </summary>
    /// <param name="indirectBuffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectbuffer">WebGPU: 17.2.1 Drawing</see></param>
    /// <param name="indirectOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurendercommandsmixin-drawindexedindirect-indirectbuffer-indirectoffset-indirectoffset">WebGPU: 17.2.1 Drawing</see></param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpurenderpipeline">WebGPU: 10.3 GPURenderPipeline</see>
/// </summary>
[ECMAScript]
[Description("@#GPURenderPipeline")]
public class GPURenderPipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout">WebGPU: 10.1 Base pipelines</see>
    /// </summary>
    /// <param name="index"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinebase-getbindgrouplayout-index-index">WebGPU: 10.1 Base pipelines</see></param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpusampler">WebGPU: 7.1 GPUSampler</see>
/// </summary>
[ECMAScript]
[Description("@#GPUSampler")]
public class GPUSampler
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpushadermodule">WebGPU: 9.1 GPUShaderModule</see>
/// </summary>
[ECMAScript]
[Description("@#GPUShaderModule")]
public class GPUShaderModule
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermodule-getcompilationinfo">WebGPU: 9.1.2 Shader Module Compilation Information</see>
    /// </summary>
    [Description("@#getCompilationInfo")]
    public extern PromiseResult<GPUCompilationInfo> GetCompilationInfo();

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpusupportedfeatures">WebGPU: 3.6.2.2 GPUSupportedFeatures</see>
/// </summary>
[ECMAScript]
[Description("@#GPUSupportedFeatures")]
public class GPUSupportedFeatures : ISet<string>
{
    #region Set
    extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpusupportedlimits">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
/// </summary>
[ECMAScript]
[Description("@#GPUSupportedLimits")]
public class GPUSupportedLimits
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension1d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxTextureDimension1D")]
    public extern uint MaxTextureDimension1D { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension2d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxTextureDimension2D")]
    public extern uint MaxTextureDimension2D { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturedimension3d">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxTextureDimension3D")]
    public extern uint MaxTextureDimension3D { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxtexturearraylayers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxTextureArrayLayers")]
    public extern uint MaxTextureArrayLayers { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindgroups">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxBindGroups")]
    public extern uint MaxBindGroups { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindgroupsplusvertexbuffers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxBindGroupsPlusVertexBuffers")]
    public extern uint MaxBindGroupsPlusVertexBuffers { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maximmediatesize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxImmediateSize")]
    public extern uint MaxImmediateSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbindingsperbindgroup">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxBindingsPerBindGroup")]
    public extern uint MaxBindingsPerBindGroup { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxdynamicuniformbuffersperpipelinelayout">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxDynamicUniformBuffersPerPipelineLayout")]
    public extern uint MaxDynamicUniformBuffersPerPipelineLayout { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxdynamicstoragebuffersperpipelinelayout">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxDynamicStorageBuffersPerPipelineLayout")]
    public extern uint MaxDynamicStorageBuffersPerPipelineLayout { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxsampledtexturespershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxSampledTexturesPerShaderStage")]
    public extern uint MaxSampledTexturesPerShaderStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxsamplerspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxSamplersPerShaderStage")]
    public extern uint MaxSamplersPerShaderStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebufferspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageBuffersPerShaderStage")]
    public extern uint MaxStorageBuffersPerShaderStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebuffersinvertexstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageBuffersInVertexStage")]
    public extern uint MaxStorageBuffersInVertexStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebuffersinfragmentstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageBuffersInFragmentStage")]
    public extern uint MaxStorageBuffersInFragmentStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturespershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageTexturesPerShaderStage")]
    public extern uint MaxStorageTexturesPerShaderStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturesinvertexstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageTexturesInVertexStage")]
    public extern uint MaxStorageTexturesInVertexStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragetexturesinfragmentstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageTexturesInFragmentStage")]
    public extern uint MaxStorageTexturesInFragmentStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxuniformbufferspershaderstage">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxUniformBuffersPerShaderStage")]
    public extern uint MaxUniformBuffersPerShaderStage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxuniformbufferbindingsize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxUniformBufferBindingSize")]
    public extern Number MaxUniformBufferBindingSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxstoragebufferbindingsize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxStorageBufferBindingSize")]
    public extern Number MaxStorageBufferBindingSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-minuniformbufferoffsetalignment">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#minUniformBufferOffsetAlignment")]
    public extern uint MinUniformBufferOffsetAlignment { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-minstoragebufferoffsetalignment">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#minStorageBufferOffsetAlignment")]
    public extern uint MinStorageBufferOffsetAlignment { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexbuffers">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxVertexBuffers")]
    public extern uint MaxVertexBuffers { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxbuffersize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxBufferSize")]
    public extern Number MaxBufferSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexattributes">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxVertexAttributes")]
    public extern uint MaxVertexAttributes { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxvertexbufferarraystride">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxVertexBufferArrayStride")]
    public extern uint MaxVertexBufferArrayStride { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxinterstageshadervariables">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxInterStageShaderVariables")]
    public extern uint MaxInterStageShaderVariables { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcolorattachments">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxColorAttachments")]
    public extern uint MaxColorAttachments { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcolorattachmentbytespersample">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxColorAttachmentBytesPerSample")]
    public extern uint MaxColorAttachmentBytesPerSample { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupstoragesize">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeWorkgroupStorageSize")]
    public extern uint MaxComputeWorkgroupStorageSize { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeinvocationsperworkgroup">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeInvocationsPerWorkgroup")]
    public extern uint MaxComputeInvocationsPerWorkgroup { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizex">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeX")]
    public extern uint MaxComputeWorkgroupSizeX { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizey">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeY")]
    public extern uint MaxComputeWorkgroupSizeY { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsizez">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeZ")]
    public extern uint MaxComputeWorkgroupSizeZ { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpusupportedlimits-maxcomputeworkgroupsperdimension">WebGPU: 3.6.2.1 GPUSupportedLimits</see>
    /// </summary>
    [Description("@#maxComputeWorkgroupsPerDimension")]
    public extern uint MaxComputeWorkgroupsPerDimension { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexture">WebGPU: 6.1 GPUTexture</see>
/// </summary>
[ECMAScript]
[Description("@#GPUTexture")]
public class GPUTexture
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-createview">WebGPU: 6.2.1 Texture View Creation</see>
    /// </summary>
    /// <param name="descriptor"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-createview-descriptor-descriptor">WebGPU: 6.2.1 Texture View Creation</see></param>
    [Description("@#createView")]
    public extern GPUTextureView CreateView(GPUTextureViewDescriptor? descriptor = default);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-destroy">WebGPU: 6.1.4 Texture Destruction</see>
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// &quot;unfilterable-float&quot; ✓ If &quot;float32-blendable&quot; is enabled ✓ ✓ If &quot;texture-formats-tier2&quot; is enabled 16 mixed component width, 32 bits per texel (4-byte render target component alignment) rgb10a2uint &quot;uint&quot; ✓ If &quot;core-features-and-limits&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8 rgb10a2unorm &quot;float&quot;, &quot;unfilterable-float&quot; ✓ ✓ ✓ ✓ If &quot;texture-formats-tier1&quot; is enabled 4 8 rg11b10ufloat &quot;float&quot;, &quot;unfilterable-float&quot; If &quot;rg11b10ufloat-renderable&quot; is enabled If &quot;texture-formats-tier1&quot; is enabled 4 8
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-width">WebGPU: 6.1 GPUTexture</see>
    /// </remarks>
    [Description("@#width")]
    public extern GPUIntegerCoordinateOut Width { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-height">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#height")]
    public extern GPUIntegerCoordinateOut Height { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-depthorarraylayers">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#depthOrArrayLayers")]
    public extern GPUIntegerCoordinateOut DepthOrArrayLayers { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-miplevelcount">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#mipLevelCount")]
    public extern GPUIntegerCoordinateOut MipLevelCount { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-samplecount">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#sampleCount")]
    public extern GPUSize32Out SampleCount { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-dimension">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#dimension")]
    public extern GPUTextureDimension Dimension { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-format">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#format")]
    public extern GPUTextureFormat Format { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-usage">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputexture-texturebindingviewdimension">WebGPU: 6.1 GPUTexture</see>
    /// </summary>
    [Description("@#textureBindingViewDimension")]
    public extern GPUTextureViewDimension? TextureBindingViewDimension { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputextureview">WebGPU: 6.2 GPUTextureView</see>
/// </summary>
[ECMAScript]
[Description("@#GPUTextureView")]
public class GPUTextureView
{
    #region mixin GPUObjectBase
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectbase-label">WebGPU: 3.1.2 WebGPU Objects</see>
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuuncapturederrorevent">WebGPU: 22.4 Telemetry</see>
/// </summary>
[ECMAScript]
[Description("@#GPUUncapturedErrorEvent")]
public class GPUUncapturedErrorEvent(string type, EventInit eventInitDict) : JazorEvent(type, eventInitDict)
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederrorevent-gpuuncapturederrorevent">WebGPU: 22.4 Telemetry</see>
    /// </summary>
    /// <param name="type"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederrorevent-gpuuncapturederrorevent-type-gpuuncapturederroreventinitdict-type">WebGPU: 22.4 Telemetry</see></param>
    /// <param name="gpuUncapturedErrorEventInitDict"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederrorevent-gpuuncapturederrorevent-type-gpuuncapturederroreventinitdict-gpuuncapturederroreventinitdict">WebGPU: 22.4 Telemetry</see></param>
    public extern GPUUncapturedErrorEvent(string type, GPUUncapturedErrorEventInit gpuUncapturedErrorEventInitDict);

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederrorevent-error">WebGPU: 22.4 Telemetry</see>
    /// </summary>
    [Description("@#error")]
    public extern GPUError Error { get; }
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuvalidationerror">WebGPU: 22.2 GPUError</see>
/// </summary>
[ECMAScript]
[Description("@#GPUValidationError")]
public class GPUValidationError : GPUError
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvalidationerror-gpuvalidationerror">WebGPU: 22.2 GPUError</see>
    /// </summary>
    /// <param name="message"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvalidationerror-gpuvalidationerror-message-message">WebGPU: 22.2 GPUError</see></param>
    public extern GPUValidationError(string message);
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuwgsllanguagefeatures">WebGPU: 3.6.2.3 WGSLLanguageFeatures</see>
/// </summary>
[ECMAScript]
[Description("@#WGSLLanguageFeatures")]
public class WGSLLanguageFeatures : ISet<string>
{
    #region Set
    extern int ICollection<string>.Count { get; }
    extern bool ICollection<string>.IsReadOnly { get; }
    extern bool ISet<string>.Add(string item);
    extern void ICollection<string>.Clear();
    extern bool ICollection<string>.Contains(string item);
    extern void ICollection<string>.CopyTo(string[] array, int arrayIndex);
    extern void ISet<string>.ExceptWith(IEnumerable<string> other);
    extern IEnumerator<string> IEnumerable<string>.GetEnumerator();
    extern void ISet<string>.IntersectWith(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsProperSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSubsetOf(IEnumerable<string> other);
    extern bool ISet<string>.IsSupersetOf(IEnumerable<string> other);
    extern bool ISet<string>.Overlaps(IEnumerable<string> other);
    extern bool ICollection<string>.Remove(string item);
    extern bool ISet<string>.SetEquals(IEnumerable<string> other);
    extern void ISet<string>.SymmetricExceptWith(IEnumerable<string> other);
    extern void ISet<string>.UnionWith(IEnumerable<string> other);
    extern void ICollection<string>.Add(string item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}
