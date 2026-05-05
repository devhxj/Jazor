namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPU
/// </summary>
[ECMAScript]
[Description("@#GPU")]
public class GPU
{
    /// <summary>
    /// requestAdapter
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestAdapter")]
    public extern PromiseResult<GPUAdapter?> RequestAdapter(GPURequestAdapterOptions? options = default);

    /// <summary>
    /// getPreferredCanvasFormat
    /// </summary>
    [Description("@#getPreferredCanvasFormat")]
    public extern GPUTextureFormat GetPreferredCanvasFormat();

    /// <summary>
    /// wgslLanguageFeatures
    /// </summary>
    [Description("@#wgslLanguageFeatures")]
    public extern WGSLLanguageFeatures WgslLanguageFeatures { get; }
}

/// <summary>
/// GPUAdapter
/// </summary>
[ECMAScript]
[Description("@#GPUAdapter")]
public class GPUAdapter
{
    /// <summary>
    /// features
    /// </summary>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// limits
    /// </summary>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// isFallbackAdapter
    /// </summary>
    [Description("@#isFallbackAdapter")]
    public extern bool IsFallbackAdapter { get; }

    /// <summary>
    /// requestDevice
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#requestDevice")]
    public extern PromiseResult<GPUDevice> RequestDevice(GPUDeviceDescriptor? descriptor = default);

    /// <summary>
    /// requestAdapterInfo
    /// </summary>
    [Description("@#requestAdapterInfo")]
    public extern PromiseResult<GPUAdapterInfo> RequestAdapterInfo();
}

/// <summary>
/// GPUAdapterInfo
/// </summary>
[ECMAScript]
[Description("@#GPUAdapterInfo")]
public class GPUAdapterInfo
{
    /// <summary>
    /// vendor
    /// </summary>
    [Description("@#vendor")]
    public extern string Vendor { get; }

    /// <summary>
    /// architecture
    /// </summary>
    [Description("@#architecture")]
    public extern string Architecture { get; }

    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern string Device { get; }

    /// <summary>
    /// description
    /// </summary>
    [Description("@#description")]
    public extern string Description { get; }
}

/// <summary>
/// GPUBindGroup
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroup")]
public class GPUBindGroup
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUBindGroupLayout
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupLayout")]
public class GPUBindGroupLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUBuffer
/// </summary>
[ECMAScript]
[Description("@#GPUBuffer")]
public class GPUBuffer
{
    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern GPUSize64Out Size { get; }

    /// <summary>
    /// usage
    /// </summary>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    /// <summary>
    /// mapState
    /// </summary>
    [Description("@#mapState")]
    public extern GPUBufferMapState MapState { get; }

    /// <summary>
    /// mapAsync
    /// </summary>
    /// <param name="mode">mode</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#mapAsync")]
    public extern PromiseResult MapAsync(GPUMapModeFlags mode, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// getMappedRange
    /// </summary>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#getMappedRange")]
    public extern ArrayBuffer GetMappedRange(GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// unmap
    /// </summary>
    [Description("@#unmap")]
    public extern void Unmap();

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUCanvasContext
/// </summary>
[ECMAScript]
[Description("@#GPUCanvasContext")]
public class GPUCanvasContext
{
    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern GPUCanvasContextCanvas Canvas { get; }

    /// <summary>
    /// configure
    /// </summary>
    /// <param name="configuration">configuration</param>
    [Description("@#configure")]
    public extern void Configure(GPUCanvasConfiguration configuration);

    /// <summary>
    /// unconfigure
    /// </summary>
    [Description("@#unconfigure")]
    public extern void Unconfigure();

    /// <summary>
    /// getCurrentTexture
    /// </summary>
    [Description("@#getCurrentTexture")]
    public extern GPUTexture GetCurrentTexture();
}

/// <summary>
/// GPUCommandBuffer
/// </summary>
[ECMAScript]
[Description("@#GPUCommandBuffer")]
public class GPUCommandBuffer
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUCommandEncoder
/// </summary>
[ECMAScript]
[Description("@#GPUCommandEncoder")]
public class GPUCommandEncoder
{
    /// <summary>
    /// beginRenderPass
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#beginRenderPass")]
    public extern GPURenderPassEncoder BeginRenderPass(GPURenderPassDescriptor descriptor);

    /// <summary>
    /// beginComputePass
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#beginComputePass")]
    public extern GPUComputePassEncoder BeginComputePass(GPUComputePassDescriptor? descriptor = default);

    /// <summary>
    /// copyBufferToBuffer
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="sourceOffset">sourceOffset</param>
    /// <param name="destination">destination</param>
    /// <param name="destinationOffset">destinationOffset</param>
    /// <param name="size">size</param>
    [Description("@#copyBufferToBuffer")]
    public extern void CopyBufferToBuffer(GPUBuffer source, GPUSize64 sourceOffset, GPUBuffer destination, GPUSize64 destinationOffset, GPUSize64 size);

    /// <summary>
    /// copyBufferToTexture
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="destination">destination</param>
    /// <param name="copySize">copySize</param>
    [Description("@#copyBufferToTexture")]
    public extern void CopyBufferToTexture(GPUImageCopyBuffer source, GPUImageCopyTexture destination, GPUExtent3D copySize);

    /// <summary>
    /// copyTextureToBuffer
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="destination">destination</param>
    /// <param name="copySize">copySize</param>
    [Description("@#copyTextureToBuffer")]
    public extern void CopyTextureToBuffer(GPUImageCopyTexture source, GPUImageCopyBuffer destination, GPUExtent3D copySize);

    /// <summary>
    /// copyTextureToTexture
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="destination">destination</param>
    /// <param name="copySize">copySize</param>
    [Description("@#copyTextureToTexture")]
    public extern void CopyTextureToTexture(GPUImageCopyTexture source, GPUImageCopyTexture destination, GPUExtent3D copySize);

    /// <summary>
    /// clearBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#clearBuffer")]
    public extern void ClearBuffer(GPUBuffer buffer, GPUSize64? offset = default, GPUSize64? size = default);

    /// <summary>
    /// resolveQuerySet
    /// </summary>
    /// <param name="querySet">querySet</param>
    /// <param name="firstQuery">firstQuery</param>
    /// <param name="queryCount">queryCount</param>
    /// <param name="destination">destination</param>
    /// <param name="destinationOffset">destinationOffset</param>
    [Description("@#resolveQuerySet")]
    public extern void ResolveQuerySet(GPUQuerySet querySet, GPUSize32 firstQuery, GPUSize32 queryCount, GPUBuffer destination, GPUSize64 destinationOffset);

    /// <summary>
    /// finish
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#finish")]
    public extern GPUCommandBuffer Finish(GPUCommandBufferDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// pushDebugGroup
    /// </summary>
    /// <param name="groupLabel">groupLabel</param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);
    
    /// <summary>
    /// popDebugGroup
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();
    
    /// <summary>
    /// insertDebugMarker
    /// </summary>
    /// <param name="markerLabel">markerLabel</param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion
}

/// <summary>
/// GPUCompilationInfo
/// </summary>
[ECMAScript]
[Description("@#GPUCompilationInfo")]
public class GPUCompilationInfo
{
    /// <summary>
    /// messages
    /// </summary>
    [Description("@#messages")]
    public extern FrozenSet<GPUCompilationMessage> Messages { get; }
}

/// <summary>
/// GPUCompilationMessage
/// </summary>
[ECMAScript]
[Description("@#GPUCompilationMessage")]
public class GPUCompilationMessage
{
    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern GPUCompilationMessageType Type { get; }

    /// <summary>
    /// lineNum
    /// </summary>
    [Description("@#lineNum")]
    public extern ulong LineNum { get; }

    /// <summary>
    /// linePos
    /// </summary>
    [Description("@#linePos")]
    public extern ulong LinePos { get; }

    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern ulong Offset { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern ulong Length { get; }
}

/// <summary>
/// GPUComputePassEncoder
/// </summary>
[ECMAScript]
[Description("@#GPUComputePassEncoder")]
public class GPUComputePassEncoder
{
    /// <summary>
    /// setPipeline
    /// </summary>
    /// <param name="pipeline">pipeline</param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPUComputePipeline pipeline);

    /// <summary>
    /// dispatchWorkgroups
    /// </summary>
    /// <param name="workgroupCountX">workgroupCountX</param>
    /// <param name="workgroupCountY">workgroupCountY</param>
    /// <param name="workgroupCountZ">workgroupCountZ</param>
    [Description("@#dispatchWorkgroups")]
    public extern void DispatchWorkgroups(GPUSize32 workgroupCountX, GPUSize32? workgroupCountY = default, GPUSize32? workgroupCountZ = default);

    /// <summary>
    /// dispatchWorkgroupsIndirect
    /// </summary>
    /// <param name="indirectBuffer">indirectBuffer</param>
    /// <param name="indirectOffset">indirectOffset</param>
    [Description("@#dispatchWorkgroupsIndirect")]
    public extern void DispatchWorkgroupsIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);

    /// <summary>
    /// end
    /// </summary>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// pushDebugGroup
    /// </summary>
    /// <param name="groupLabel">groupLabel</param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);
    
    /// <summary>
    /// popDebugGroup
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();
    
    /// <summary>
    /// insertDebugMarker
    /// </summary>
    /// <param name="markerLabel">markerLabel</param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsets">dynamicOffsets</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);
    
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsetsData">dynamicOffsetsData</param>
    /// <param name="dynamicOffsetsDataStart">dynamicOffsetsDataStart</param>
    /// <param name="dynamicOffsetsDataLength">dynamicOffsetsDataLength</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);
    #endregion
}

/// <summary>
/// GPUComputePipeline
/// </summary>
[ECMAScript]
[Description("@#GPUComputePipeline")]
public class GPUComputePipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// getBindGroupLayout
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// GPUDevice
/// </summary>
[ECMAScript]
[Description("@#GPUDevice")]
public partial class GPUDevice : EventTarget
{
    /// <summary>
    /// features
    /// </summary>
    [Description("@#features")]
    public extern GPUSupportedFeatures Features { get; }

    /// <summary>
    /// limits
    /// </summary>
    [Description("@#limits")]
    public extern GPUSupportedLimits Limits { get; }

    /// <summary>
    /// queue
    /// </summary>
    [Description("@#queue")]
    public extern GPUQueue Queue { get; }

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// createBuffer
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createBuffer")]
    public extern GPUBuffer CreateBuffer(GPUBufferDescriptor descriptor);

    /// <summary>
    /// createTexture
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createTexture")]
    public extern GPUTexture CreateTexture(GPUTextureDescriptor descriptor);

    /// <summary>
    /// createSampler
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createSampler")]
    public extern GPUSampler CreateSampler(GPUSamplerDescriptor? descriptor = default);

    /// <summary>
    /// importExternalTexture
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#importExternalTexture")]
    public extern GPUExternalTexture ImportExternalTexture(GPUExternalTextureDescriptor descriptor);

    /// <summary>
    /// createBindGroupLayout
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createBindGroupLayout")]
    public extern GPUBindGroupLayout CreateBindGroupLayout(GPUBindGroupLayoutDescriptor descriptor);

    /// <summary>
    /// createPipelineLayout
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createPipelineLayout")]
    public extern GPUPipelineLayout CreatePipelineLayout(GPUPipelineLayoutDescriptor descriptor);

    /// <summary>
    /// createBindGroup
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createBindGroup")]
    public extern GPUBindGroup CreateBindGroup(GPUBindGroupDescriptor descriptor);

    /// <summary>
    /// createShaderModule
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createShaderModule")]
    public extern GPUShaderModule CreateShaderModule(GPUShaderModuleDescriptor descriptor);

    /// <summary>
    /// createComputePipeline
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createComputePipeline")]
    public extern GPUComputePipeline CreateComputePipeline(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// createRenderPipeline
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createRenderPipeline")]
    public extern GPURenderPipeline CreateRenderPipeline(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// createComputePipelineAsync
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createComputePipelineAsync")]
    public extern PromiseResult<GPUComputePipeline> CreateComputePipelineAsync(GPUComputePipelineDescriptor descriptor);

    /// <summary>
    /// createRenderPipelineAsync
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createRenderPipelineAsync")]
    public extern PromiseResult<GPURenderPipeline> CreateRenderPipelineAsync(GPURenderPipelineDescriptor descriptor);

    /// <summary>
    /// createCommandEncoder
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createCommandEncoder")]
    public extern GPUCommandEncoder CreateCommandEncoder(GPUCommandEncoderDescriptor? descriptor = default);

    /// <summary>
    /// createRenderBundleEncoder
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createRenderBundleEncoder")]
    public extern GPURenderBundleEncoder CreateRenderBundleEncoder(GPURenderBundleEncoderDescriptor descriptor);

    /// <summary>
    /// createQuerySet
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createQuerySet")]
    public extern GPUQuerySet CreateQuerySet(GPUQuerySetDescriptor descriptor);

    /// <summary>
    /// lost
    /// </summary>
    [Description("@#lost")]
    public extern PromiseResult<GPUDeviceLostInfo> Lost { get; }

    /// <summary>
    /// pushErrorScope
    /// </summary>
    /// <param name="filter">filter</param>
    [Description("@#pushErrorScope")]
    public extern void PushErrorScope(GPUErrorFilter filter);

    /// <summary>
    /// popErrorScope
    /// </summary>
    [Description("@#popErrorScope")]
    public extern PromiseResult<GPUError?> PopErrorScope();

    /// <summary>
    /// onuncapturederror
    /// </summary>
    [Description("@#onuncapturederror")]
    public extern EventHandler Onuncapturederror { get; set; }

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUDeviceLostInfo
/// </summary>
[ECMAScript]
[Description("@#GPUDeviceLostInfo")]
public class GPUDeviceLostInfo
{
    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern GPUDeviceLostReason Reason { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// GPUError
/// </summary>
[ECMAScript]
[Description("@#GPUError")]
public class GPUError
{
    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// GPUExternalTexture
/// </summary>
[ECMAScript]
[Description("@#GPUExternalTexture")]
public class GPUExternalTexture
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUInternalError
/// </summary>
[ECMAScript]
[Description("@#GPUInternalError")]
public class GPUInternalError : GPUError
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    public extern GPUInternalError(string message);
}

/// <summary>
/// GPUOutOfMemoryError
/// </summary>
[ECMAScript]
[Description("@#GPUOutOfMemoryError")]
public class GPUOutOfMemoryError : GPUError
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    public extern GPUOutOfMemoryError(string message);
}

/// <summary>
/// GPUPipelineError
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineError")]
public class GPUPipelineError(string message, string name) : DOMException(message, name)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    public extern GPUPipelineError(string message = "", GPUPipelineErrorInit? options = default);

    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern GPUPipelineErrorReason Reason { get; }
}

/// <summary>
/// GPUPipelineLayout
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineLayout")]
public class GPUPipelineLayout
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUQuerySet
/// </summary>
[ECMAScript]
[Description("@#GPUQuerySet")]
public class GPUQuerySet
{
    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern GPUQueryType Type { get; }

    /// <summary>
    /// count
    /// </summary>
    [Description("@#count")]
    public extern GPUSize32Out Count { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUQueue
/// </summary>
[ECMAScript]
[Description("@#GPUQueue")]
public class GPUQueue
{
    /// <summary>
    /// submit
    /// </summary>
    /// <param name="commandBuffers">commandBuffers</param>
    [Description("@#submit")]
    public extern void Submit(GPUCommandBuffer[] commandBuffers);

    /// <summary>
    /// onSubmittedWorkDone
    /// </summary>
    [Description("@#onSubmittedWorkDone")]
    public extern PromiseResult OnSubmittedWorkDone();

    /// <summary>
    /// writeBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="bufferOffset">bufferOffset</param>
    /// <param name="data">data</param>
    /// <param name="dataOffset">dataOffset</param>
    /// <param name="size">size</param>
    [Description("@#writeBuffer")]
    public extern void WriteBuffer(GPUBuffer buffer, GPUSize64 bufferOffset, IAllowSharedBufferSource data, GPUSize64? dataOffset = default, GPUSize64? size = default);

    /// <summary>
    /// writeTexture
    /// </summary>
    /// <param name="destination">destination</param>
    /// <param name="data">data</param>
    /// <param name="dataLayout">dataLayout</param>
    /// <param name="size">size</param>
    [Description("@#writeTexture")]
    public extern void WriteTexture(GPUImageCopyTexture destination, IAllowSharedBufferSource data, GPUImageDataLayout dataLayout, GPUExtent3D size);

    /// <summary>
    /// copyExternalImageToTexture
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="destination">destination</param>
    /// <param name="copySize">copySize</param>
    [Description("@#copyExternalImageToTexture")]
    public extern void CopyExternalImageToTexture(GPUImageCopyExternalImage source, GPUImageCopyTextureTagged destination, GPUExtent3D copySize);

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPURenderBundle
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundle")]
public class GPURenderBundle
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPURenderBundleEncoder
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundleEncoder")]
public class GPURenderBundleEncoder
{
    /// <summary>
    /// finish
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#finish")]
    public extern GPURenderBundle Finish(GPURenderBundleDescriptor? descriptor = default);

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// pushDebugGroup
    /// </summary>
    /// <param name="groupLabel">groupLabel</param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);
    
    /// <summary>
    /// popDebugGroup
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();
    
    /// <summary>
    /// insertDebugMarker
    /// </summary>
    /// <param name="markerLabel">markerLabel</param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsets">dynamicOffsets</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);
    
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsetsData">dynamicOffsetsData</param>
    /// <param name="dynamicOffsetsDataStart">dynamicOffsetsDataStart</param>
    /// <param name="dynamicOffsetsDataLength">dynamicOffsetsDataLength</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// setPipeline
    /// </summary>
    /// <param name="pipeline">pipeline</param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);
    
    /// <summary>
    /// setIndexBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="indexFormat">indexFormat</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);
    
    /// <summary>
    /// setVertexBuffer
    /// </summary>
    /// <param name="slot">slot</param>
    /// <param name="buffer">buffer</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);
    
    /// <summary>
    /// draw
    /// </summary>
    /// <param name="vertexCount">vertexCount</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="firstVertex">firstVertex</param>
    /// <param name="firstInstance">firstInstance</param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);
    
    /// <summary>
    /// drawIndexed
    /// </summary>
    /// <param name="indexCount">indexCount</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="firstIndex">firstIndex</param>
    /// <param name="baseVertex">baseVertex</param>
    /// <param name="firstInstance">firstInstance</param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);
    
    /// <summary>
    /// drawIndirect
    /// </summary>
    /// <param name="indirectBuffer">indirectBuffer</param>
    /// <param name="indirectOffset">indirectOffset</param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    
    /// <summary>
    /// drawIndexedIndirect
    /// </summary>
    /// <param name="indirectBuffer">indirectBuffer</param>
    /// <param name="indirectOffset">indirectOffset</param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// GPURenderPassEncoder
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassEncoder")]
public class GPURenderPassEncoder
{
    /// <summary>
    /// setViewport
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    /// <param name="minDepth">minDepth</param>
    /// <param name="maxDepth">maxDepth</param>
    [Description("@#setViewport")]
    public extern void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth);

    /// <summary>
    /// setScissorRect
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#setScissorRect")]
    public extern void SetScissorRect(GPUIntegerCoordinate x, GPUIntegerCoordinate y, GPUIntegerCoordinate width, GPUIntegerCoordinate height);

    /// <summary>
    /// setBlendConstant
    /// </summary>
    /// <param name="color">color</param>
    [Description("@#setBlendConstant")]
    public extern void SetBlendConstant(GPUColor color);

    /// <summary>
    /// setStencilReference
    /// </summary>
    /// <param name="reference">reference</param>
    [Description("@#setStencilReference")]
    public extern void SetStencilReference(GPUStencilValue reference);

    /// <summary>
    /// beginOcclusionQuery
    /// </summary>
    /// <param name="queryIndex">queryIndex</param>
    [Description("@#beginOcclusionQuery")]
    public extern void BeginOcclusionQuery(GPUSize32 queryIndex);

    /// <summary>
    /// endOcclusionQuery
    /// </summary>
    [Description("@#endOcclusionQuery")]
    public extern void EndOcclusionQuery();

    /// <summary>
    /// executeBundles
    /// </summary>
    /// <param name="bundles">bundles</param>
    [Description("@#executeBundles")]
    public extern void ExecuteBundles(GPURenderBundle[] bundles);

    /// <summary>
    /// end
    /// </summary>
    [Description("@#end")]
    public extern void End();

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUDebugCommandsMixin
    /// <summary>
    /// pushDebugGroup
    /// </summary>
    /// <param name="groupLabel">groupLabel</param>
    [Description("@#pushDebugGroup")]
    public extern void PushDebugGroup(string groupLabel);
    
    /// <summary>
    /// popDebugGroup
    /// </summary>
    [Description("@#popDebugGroup")]
    public extern void PopDebugGroup();
    
    /// <summary>
    /// insertDebugMarker
    /// </summary>
    /// <param name="markerLabel">markerLabel</param>
    [Description("@#insertDebugMarker")]
    public extern void InsertDebugMarker(string markerLabel);
    #endregion

    #region mixin GPUBindingCommandsMixin
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsets">dynamicOffsets</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, GPUBufferDynamicOffset[]? dynamicOffsets = default);
    
    /// <summary>
    /// setBindGroup
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="bindGroup">bindGroup</param>
    /// <param name="dynamicOffsetsData">dynamicOffsetsData</param>
    /// <param name="dynamicOffsetsDataStart">dynamicOffsetsDataStart</param>
    /// <param name="dynamicOffsetsDataLength">dynamicOffsetsDataLength</param>
    [Description("@#setBindGroup")]
    public extern void SetBindGroup(GPUIndex32 index, GPUBindGroup? bindGroup, Uint32Array dynamicOffsetsData, GPUSize64 dynamicOffsetsDataStart, GPUSize32 dynamicOffsetsDataLength);
    #endregion

    #region mixin GPURenderCommandsMixin
    /// <summary>
    /// setPipeline
    /// </summary>
    /// <param name="pipeline">pipeline</param>
    [Description("@#setPipeline")]
    public extern void SetPipeline(GPURenderPipeline pipeline);
    
    /// <summary>
    /// setIndexBuffer
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="indexFormat">indexFormat</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#setIndexBuffer")]
    public extern void SetIndexBuffer(GPUBuffer buffer, GPUIndexFormat indexFormat, GPUSize64? offset = default, GPUSize64? size = default);
    
    /// <summary>
    /// setVertexBuffer
    /// </summary>
    /// <param name="slot">slot</param>
    /// <param name="buffer">buffer</param>
    /// <param name="offset">offset</param>
    /// <param name="size">size</param>
    [Description("@#setVertexBuffer")]
    public extern void SetVertexBuffer(GPUIndex32 slot, GPUBuffer? buffer, GPUSize64? offset = default, GPUSize64? size = default);
    
    /// <summary>
    /// draw
    /// </summary>
    /// <param name="vertexCount">vertexCount</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="firstVertex">firstVertex</param>
    /// <param name="firstInstance">firstInstance</param>
    [Description("@#draw")]
    public extern void Draw(GPUSize32 vertexCount, GPUSize32? instanceCount = default, GPUSize32? firstVertex = default, GPUSize32? firstInstance = default);
    
    /// <summary>
    /// drawIndexed
    /// </summary>
    /// <param name="indexCount">indexCount</param>
    /// <param name="instanceCount">instanceCount</param>
    /// <param name="firstIndex">firstIndex</param>
    /// <param name="baseVertex">baseVertex</param>
    /// <param name="firstInstance">firstInstance</param>
    [Description("@#drawIndexed")]
    public extern void DrawIndexed(GPUSize32 indexCount, GPUSize32? instanceCount = default, GPUSize32? firstIndex = default, GPUSignedOffset32? baseVertex = default, GPUSize32? firstInstance = default);
    
    /// <summary>
    /// drawIndirect
    /// </summary>
    /// <param name="indirectBuffer">indirectBuffer</param>
    /// <param name="indirectOffset">indirectOffset</param>
    [Description("@#drawIndirect")]
    public extern void DrawIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    
    /// <summary>
    /// drawIndexedIndirect
    /// </summary>
    /// <param name="indirectBuffer">indirectBuffer</param>
    /// <param name="indirectOffset">indirectOffset</param>
    [Description("@#drawIndexedIndirect")]
    public extern void DrawIndexedIndirect(GPUBuffer indirectBuffer, GPUSize64 indirectOffset);
    #endregion
}

/// <summary>
/// GPURenderPipeline
/// </summary>
[ECMAScript]
[Description("@#GPURenderPipeline")]
public class GPURenderPipeline
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion

    #region mixin GPUPipelineBase
    /// <summary>
    /// getBindGroupLayout
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getBindGroupLayout")]
    public extern GPUBindGroupLayout GetBindGroupLayout(uint index);
    #endregion
}

/// <summary>
/// GPUSampler
/// </summary>
[ECMAScript]
[Description("@#GPUSampler")]
public class GPUSampler
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUShaderModule
/// </summary>
[ECMAScript]
[Description("@#GPUShaderModule")]
public class GPUShaderModule
{
    /// <summary>
    /// getCompilationInfo
    /// </summary>
    [Description("@#getCompilationInfo")]
    public extern PromiseResult<GPUCompilationInfo> GetCompilationInfo();

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUSupportedFeatures
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
/// GPUSupportedLimits
/// </summary>
[ECMAScript]
[Description("@#GPUSupportedLimits")]
public class GPUSupportedLimits
{
    /// <summary>
    /// maxTextureDimension1D
    /// </summary>
    [Description("@#maxTextureDimension1D")]
    public extern uint MaxTextureDimension1D { get; }

    /// <summary>
    /// maxTextureDimension2D
    /// </summary>
    [Description("@#maxTextureDimension2D")]
    public extern uint MaxTextureDimension2D { get; }

    /// <summary>
    /// maxTextureDimension3D
    /// </summary>
    [Description("@#maxTextureDimension3D")]
    public extern uint MaxTextureDimension3D { get; }

    /// <summary>
    /// maxTextureArrayLayers
    /// </summary>
    [Description("@#maxTextureArrayLayers")]
    public extern uint MaxTextureArrayLayers { get; }

    /// <summary>
    /// maxBindGroups
    /// </summary>
    [Description("@#maxBindGroups")]
    public extern uint MaxBindGroups { get; }

    /// <summary>
    /// maxBindGroupsPlusVertexBuffers
    /// </summary>
    [Description("@#maxBindGroupsPlusVertexBuffers")]
    public extern uint MaxBindGroupsPlusVertexBuffers { get; }

    /// <summary>
    /// maxBindingsPerBindGroup
    /// </summary>
    [Description("@#maxBindingsPerBindGroup")]
    public extern uint MaxBindingsPerBindGroup { get; }

    /// <summary>
    /// maxDynamicUniformBuffersPerPipelineLayout
    /// </summary>
    [Description("@#maxDynamicUniformBuffersPerPipelineLayout")]
    public extern uint MaxDynamicUniformBuffersPerPipelineLayout { get; }

    /// <summary>
    /// maxDynamicStorageBuffersPerPipelineLayout
    /// </summary>
    [Description("@#maxDynamicStorageBuffersPerPipelineLayout")]
    public extern uint MaxDynamicStorageBuffersPerPipelineLayout { get; }

    /// <summary>
    /// maxSampledTexturesPerShaderStage
    /// </summary>
    [Description("@#maxSampledTexturesPerShaderStage")]
    public extern uint MaxSampledTexturesPerShaderStage { get; }

    /// <summary>
    /// maxSamplersPerShaderStage
    /// </summary>
    [Description("@#maxSamplersPerShaderStage")]
    public extern uint MaxSamplersPerShaderStage { get; }

    /// <summary>
    /// maxStorageBuffersPerShaderStage
    /// </summary>
    [Description("@#maxStorageBuffersPerShaderStage")]
    public extern uint MaxStorageBuffersPerShaderStage { get; }

    /// <summary>
    /// maxStorageTexturesPerShaderStage
    /// </summary>
    [Description("@#maxStorageTexturesPerShaderStage")]
    public extern uint MaxStorageTexturesPerShaderStage { get; }

    /// <summary>
    /// maxUniformBuffersPerShaderStage
    /// </summary>
    [Description("@#maxUniformBuffersPerShaderStage")]
    public extern uint MaxUniformBuffersPerShaderStage { get; }

    /// <summary>
    /// maxUniformBufferBindingSize
    /// </summary>
    [Description("@#maxUniformBufferBindingSize")]
    public extern ulong MaxUniformBufferBindingSize { get; }

    /// <summary>
    /// maxStorageBufferBindingSize
    /// </summary>
    [Description("@#maxStorageBufferBindingSize")]
    public extern ulong MaxStorageBufferBindingSize { get; }

    /// <summary>
    /// minUniformBufferOffsetAlignment
    /// </summary>
    [Description("@#minUniformBufferOffsetAlignment")]
    public extern uint MinUniformBufferOffsetAlignment { get; }

    /// <summary>
    /// minStorageBufferOffsetAlignment
    /// </summary>
    [Description("@#minStorageBufferOffsetAlignment")]
    public extern uint MinStorageBufferOffsetAlignment { get; }

    /// <summary>
    /// maxVertexBuffers
    /// </summary>
    [Description("@#maxVertexBuffers")]
    public extern uint MaxVertexBuffers { get; }

    /// <summary>
    /// maxBufferSize
    /// </summary>
    [Description("@#maxBufferSize")]
    public extern ulong MaxBufferSize { get; }

    /// <summary>
    /// maxVertexAttributes
    /// </summary>
    [Description("@#maxVertexAttributes")]
    public extern uint MaxVertexAttributes { get; }

    /// <summary>
    /// maxVertexBufferArrayStride
    /// </summary>
    [Description("@#maxVertexBufferArrayStride")]
    public extern uint MaxVertexBufferArrayStride { get; }

    /// <summary>
    /// maxInterStageShaderComponents
    /// </summary>
    [Description("@#maxInterStageShaderComponents")]
    public extern uint MaxInterStageShaderComponents { get; }

    /// <summary>
    /// maxInterStageShaderVariables
    /// </summary>
    [Description("@#maxInterStageShaderVariables")]
    public extern uint MaxInterStageShaderVariables { get; }

    /// <summary>
    /// maxColorAttachments
    /// </summary>
    [Description("@#maxColorAttachments")]
    public extern uint MaxColorAttachments { get; }

    /// <summary>
    /// maxColorAttachmentBytesPerSample
    /// </summary>
    [Description("@#maxColorAttachmentBytesPerSample")]
    public extern uint MaxColorAttachmentBytesPerSample { get; }

    /// <summary>
    /// maxComputeWorkgroupStorageSize
    /// </summary>
    [Description("@#maxComputeWorkgroupStorageSize")]
    public extern uint MaxComputeWorkgroupStorageSize { get; }

    /// <summary>
    /// maxComputeInvocationsPerWorkgroup
    /// </summary>
    [Description("@#maxComputeInvocationsPerWorkgroup")]
    public extern uint MaxComputeInvocationsPerWorkgroup { get; }

    /// <summary>
    /// maxComputeWorkgroupSizeX
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeX")]
    public extern uint MaxComputeWorkgroupSizeX { get; }

    /// <summary>
    /// maxComputeWorkgroupSizeY
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeY")]
    public extern uint MaxComputeWorkgroupSizeY { get; }

    /// <summary>
    /// maxComputeWorkgroupSizeZ
    /// </summary>
    [Description("@#maxComputeWorkgroupSizeZ")]
    public extern uint MaxComputeWorkgroupSizeZ { get; }

    /// <summary>
    /// maxComputeWorkgroupsPerDimension
    /// </summary>
    [Description("@#maxComputeWorkgroupsPerDimension")]
    public extern uint MaxComputeWorkgroupsPerDimension { get; }
}

/// <summary>
/// GPUTexture
/// </summary>
[ECMAScript]
[Description("@#GPUTexture")]
public class GPUTexture
{
    /// <summary>
    /// createView
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#createView")]
    public extern GPUTextureView CreateView(GPUTextureViewDescriptor? descriptor = default);

    /// <summary>
    /// destroy
    /// </summary>
    [Description("@#destroy")]
    public extern void Destroy();

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern GPUIntegerCoordinateOut Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern GPUIntegerCoordinateOut Height { get; }

    /// <summary>
    /// depthOrArrayLayers
    /// </summary>
    [Description("@#depthOrArrayLayers")]
    public extern GPUIntegerCoordinateOut DepthOrArrayLayers { get; }

    /// <summary>
    /// mipLevelCount
    /// </summary>
    [Description("@#mipLevelCount")]
    public extern GPUIntegerCoordinateOut MipLevelCount { get; }

    /// <summary>
    /// sampleCount
    /// </summary>
    [Description("@#sampleCount")]
    public extern GPUSize32Out SampleCount { get; }

    /// <summary>
    /// dimension
    /// </summary>
    [Description("@#dimension")]
    public extern GPUTextureDimension Dimension { get; }

    /// <summary>
    /// format
    /// </summary>
    [Description("@#format")]
    public extern GPUTextureFormat Format { get; }

    /// <summary>
    /// usage
    /// </summary>
    [Description("@#usage")]
    public extern GPUFlagsConstant Usage { get; }

    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUTextureView
/// </summary>
[ECMAScript]
[Description("@#GPUTextureView")]
public class GPUTextureView
{
    #region mixin GPUObjectBase
    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; set; }
    #endregion
}

/// <summary>
/// GPUUncapturedErrorEvent
/// </summary>
[ECMAScript]
[Description("@#GPUUncapturedErrorEvent")]
public class GPUUncapturedErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="gpuUncapturedErrorEventInitDict">gpuUncapturedErrorEventInitDict</param>
    public extern GPUUncapturedErrorEvent(string type, GPUUncapturedErrorEventInit gpuUncapturedErrorEventInitDict);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern GPUError Error { get; }
}

/// <summary>
/// GPUValidationError
/// </summary>
[ECMAScript]
[Description("@#GPUValidationError")]
public class GPUValidationError : GPUError
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    public extern GPUValidationError(string message);
}

/// <summary>
/// WGSLLanguageFeatures
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
