namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUCommandBufferDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUCommandBufferDescriptor")]
public abstract record GPUCommandBufferDescriptor();

/// <summary>
/// GPUCommandEncoderDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUCommandEncoderDescriptor")]
public abstract record GPUCommandEncoderDescriptor();

/// <summary>
/// GPUExternalTextureBindingLayout
/// </summary>
[ECMAScript]
[Description("@#GPUExternalTextureBindingLayout")]
public abstract record GPUExternalTextureBindingLayout();

/// <summary>
/// GPUQueueDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUQueueDescriptor")]
public abstract record GPUQueueDescriptor();

/// <summary>
/// GPURenderBundleDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundleDescriptor")]
public abstract record GPURenderBundleDescriptor();

/// <summary>
/// GPUBindGroupDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupDescriptor")]
public record GPUBindGroupDescriptor(
    [property: Description("@#layout")]GPUBindGroupLayout? Layout = default,
    [property: Description("@#entries")]GPUBindGroupEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUBindGroupEntry
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupEntry")]
public record GPUBindGroupEntry(
    [property: Description("@#binding")]GPUIndex32? Binding = default,
    [property: Description("@#resource")]GPUBindingResource? Resource = default);

/// <summary>
/// GPUBindGroupLayoutDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupLayoutDescriptor")]
public record GPUBindGroupLayoutDescriptor(
    [property: Description("@#entries")]GPUBindGroupLayoutEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUBindGroupLayoutEntry
/// </summary>
[ECMAScript]
[Description("@#GPUBindGroupLayoutEntry")]
public record GPUBindGroupLayoutEntry(
    [property: Description("@#binding")]GPUIndex32? Binding = default,
    [property: Description("@#visibility")]GPUShaderStageFlags? Visibility = default,
    [property: Description("@#buffer")]GPUBufferBindingLayout? Buffer = default,
    [property: Description("@#sampler")]GPUSamplerBindingLayout? Sampler = default,
    [property: Description("@#texture")]GPUTextureBindingLayout? Texture = default,
    [property: Description("@#storageTexture")]GPUStorageTextureBindingLayout? StorageTexture = default,
    [property: Description("@#externalTexture")]GPUExternalTextureBindingLayout? ExternalTexture = default);

/// <summary>
/// GPUBlendComponent
/// </summary>
[ECMAScript]
[Description("@#GPUBlendComponent")]
public record GPUBlendComponent(
    [property: Description("@#operation")]GPUBlendOperation Operation = GPUBlendOperation.Add,
    [property: Description("@#srcFactor")]GPUBlendFactor SrcFactor = GPUBlendFactor.One,
    [property: Description("@#dstFactor")]GPUBlendFactor DstFactor = GPUBlendFactor.Zero);

/// <summary>
/// GPUBlendState
/// </summary>
[ECMAScript]
[Description("@#GPUBlendState")]
public record GPUBlendState(
    [property: Description("@#color")]GPUBlendComponent? Color = default,
    [property: Description("@#alpha")]GPUBlendComponent? Alpha = default);

/// <summary>
/// GPUBufferBinding
/// </summary>
[ECMAScript]
[Description("@#GPUBufferBinding")]
public record GPUBufferBinding(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#size")]GPUSize64? Size = default);

/// <summary>
/// GPUBufferBindingLayout
/// </summary>
[ECMAScript]
[Description("@#GPUBufferBindingLayout")]
public record GPUBufferBindingLayout(
    [property: Description("@#type")]GPUBufferBindingType Type = GPUBufferBindingType.Uniform,
    [property: Description("@#hasDynamicOffset")]bool HasDynamicOffset = false,
    [property: Description("@#minBindingSize")]GPUSize64? MinBindingSize = default);

/// <summary>
/// GPUBufferDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUBufferDescriptor")]
public record GPUBufferDescriptor(
    [property: Description("@#size")]GPUSize64? Size = default,
    [property: Description("@#usage")]GPUBufferUsageFlags? Usage = default,
    [property: Description("@#mappedAtCreation")]bool MappedAtCreation = false) : GPUObjectDescriptorBase;

/// <summary>
/// GPUCanvasConfiguration
/// </summary>
[ECMAScript]
[Description("@#GPUCanvasConfiguration")]
public record GPUCanvasConfiguration(
    [property: Description("@#device")]GPUDevice? Device = default,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#usage")]GPUTextureUsageFlags? Usage = default,
    [property: Description("@#viewFormats")]GPUTextureFormat[]? ViewFormats = default,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#alphaMode")]GPUCanvasAlphaMode AlphaMode = GPUCanvasAlphaMode.Opaque);

/// <summary>
/// GPUColorDict
/// </summary>
[ECMAScript]
[Description("@#GPUColorDict")]
public record GPUColorDict(
    [property: Description("@#r")]double R = default,
    [property: Description("@#g")]double G = default,
    [property: Description("@#b")]double B = default,
    [property: Description("@#a")]double A = default);

/// <summary>
/// GPUColorTargetState
/// </summary>
[ECMAScript]
[Description("@#GPUColorTargetState")]
public record GPUColorTargetState(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#blend")]GPUBlendState? Blend = default,
    [property: Description("@#writeMask")]GPUColorWriteFlags? WriteMask = default);

/// <summary>
/// GPUComputePassDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUComputePassDescriptor")]
public record GPUComputePassDescriptor(
    [property: Description("@#timestampWrites")]GPUComputePassTimestampWrites? TimestampWrites = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUComputePassTimestampWrites
/// </summary>
[ECMAScript]
[Description("@#GPUComputePassTimestampWrites")]
public record GPUComputePassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// GPUComputePipelineDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUComputePipelineDescriptor")]
public record GPUComputePipelineDescriptor(
    [property: Description("@#compute")]GPUProgrammableStage? Compute = default) : GPUPipelineDescriptorBase;

/// <summary>
/// GPUDepthStencilState
/// </summary>
[ECMAScript]
[Description("@#GPUDepthStencilState")]
public record GPUDepthStencilState(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#depthWriteEnabled")]bool DepthWriteEnabled = default,
    [property: Description("@#depthCompare")]GPUCompareFunction? DepthCompare = default,
    [property: Description("@#stencilFront")]GPUStencilFaceState? StencilFront = default,
    [property: Description("@#stencilBack")]GPUStencilFaceState? StencilBack = default,
    [property: Description("@#stencilReadMask")]GPUStencilValue? StencilReadMask = default,
    [property: Description("@#stencilWriteMask")]GPUStencilValue? StencilWriteMask = default,
    [property: Description("@#depthBias")]GPUDepthBias? DepthBias = default,
    [property: Description("@#depthBiasSlopeScale")]float DepthBiasSlopeScale = 0f,
    [property: Description("@#depthBiasClamp")]float DepthBiasClamp = 0f);

/// <summary>
/// GPUDeviceDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUDeviceDescriptor")]
public record GPUDeviceDescriptor(
    [property: Description("@#requiredFeatures")]GPUFeatureName[]? RequiredFeatures = default,
    [property: Description("@#requiredLimits")]Dictionary<string, GPUSize64>? RequiredLimits = default,
    [property: Description("@#defaultQueue")]GPUQueueDescriptor? DefaultQueue = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUExtent3DDict
/// </summary>
[ECMAScript]
[Description("@#GPUExtent3DDict")]
public record GPUExtent3DDict(
    [property: Description("@#width")]GPUIntegerCoordinate? Width = default,
    [property: Description("@#height")]GPUIntegerCoordinate? Height = default,
    [property: Description("@#depthOrArrayLayers")]GPUIntegerCoordinate? DepthOrArrayLayers = default);

/// <summary>
/// GPUExternalTextureDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUExternalTextureDescriptor")]
public record GPUExternalTextureDescriptor(
    [property: Description("@#source")]Either<HTMLVideoElement, VideoFrame>? Source = default,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb) : GPUObjectDescriptorBase;

/// <summary>
/// GPUFragmentState
/// </summary>
[ECMAScript]
[Description("@#GPUFragmentState")]
public record GPUFragmentState(
    [property: Description("@#targets")]GPUColorTargetState?[]? Targets = default) : GPUProgrammableStage;

/// <summary>
/// GPUImageCopyBuffer
/// </summary>
[ECMAScript]
[Description("@#GPUImageCopyBuffer")]
public record GPUImageCopyBuffer(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default) : GPUImageDataLayout;

/// <summary>
/// GPUImageCopyExternalImage
/// </summary>
[ECMAScript]
[Description("@#GPUImageCopyExternalImage")]
public record GPUImageCopyExternalImage(
    [property: Description("@#source")]GPUImageCopyExternalImageSource? Source = default,
    [property: Description("@#origin")]GPUOrigin2D? Origin = default,
    [property: Description("@#flipY")]bool FlipY = false);

/// <summary>
/// GPUImageCopyTexture
/// </summary>
[ECMAScript]
[Description("@#GPUImageCopyTexture")]
public record GPUImageCopyTexture(
    [property: Description("@#texture")]GPUTexture? Texture = default,
    [property: Description("@#mipLevel")]GPUIntegerCoordinate? MipLevel = default,
    [property: Description("@#origin")]GPUOrigin3D? Origin = default,
    [property: Description("@#aspect")]GPUTextureAspect Aspect = GPUTextureAspect.All);

/// <summary>
/// GPUImageCopyTextureTagged
/// </summary>
[ECMAScript]
[Description("@#GPUImageCopyTextureTagged")]
public record GPUImageCopyTextureTagged(
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#premultipliedAlpha")]bool PremultipliedAlpha = false) : GPUImageCopyTexture;

/// <summary>
/// GPUImageDataLayout
/// </summary>
[ECMAScript]
[Description("@#GPUImageDataLayout")]
public record GPUImageDataLayout(
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#bytesPerRow")]GPUSize32? BytesPerRow = default,
    [property: Description("@#rowsPerImage")]GPUSize32? RowsPerImage = default);

/// <summary>
/// GPUMultisampleState
/// </summary>
[ECMAScript]
[Description("@#GPUMultisampleState")]
public record GPUMultisampleState(
    [property: Description("@#count")]GPUSize32? Count = default,
    [property: Description("@#mask")]GPUSampleMask? Mask = default,
    [property: Description("@#alphaToCoverageEnabled")]bool AlphaToCoverageEnabled = false);

/// <summary>
/// GPUObjectDescriptorBase
/// </summary>
[ECMAScript]
[Description("@#GPUObjectDescriptorBase")]
public record GPUObjectDescriptorBase(
    [property: Description("@#label")]string? Label = default);

/// <summary>
/// GPUOrigin2DDict
/// </summary>
[ECMAScript]
[Description("@#GPUOrigin2DDict")]
public record GPUOrigin2DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default);

/// <summary>
/// GPUOrigin3DDict
/// </summary>
[ECMAScript]
[Description("@#GPUOrigin3DDict")]
public record GPUOrigin3DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default,
    [property: Description("@#z")]GPUIntegerCoordinate? Z = default);

/// <summary>
/// GPUPipelineDescriptorBase
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineDescriptorBase")]
public record GPUPipelineDescriptorBase(
    [property: Description("@#layout")]Either<GPUPipelineLayout, GPUAutoLayoutMode>? Layout = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUPipelineErrorInit
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineErrorInit")]
public record GPUPipelineErrorInit(
    [property: Description("@#reason")]GPUPipelineErrorReason? Reason = default);

/// <summary>
/// GPUPipelineLayoutDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUPipelineLayoutDescriptor")]
public record GPUPipelineLayoutDescriptor(
    [property: Description("@#bindGroupLayouts")]GPUBindGroupLayout[]? BindGroupLayouts = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUPrimitiveState
/// </summary>
[ECMAScript]
[Description("@#GPUPrimitiveState")]
public record GPUPrimitiveState(
    [property: Description("@#topology")]GPUPrimitiveTopology Topology = GPUPrimitiveTopology.TriangleList,
    [property: Description("@#stripIndexFormat")]GPUIndexFormat? StripIndexFormat = default,
    [property: Description("@#frontFace")]GPUFrontFace FrontFace = GPUFrontFace.Ccw,
    [property: Description("@#cullMode")]GPUCullMode CullMode = GPUCullMode.None,
    [property: Description("@#unclippedDepth")]bool UnclippedDepth = false);

/// <summary>
/// GPUProgrammableStage
/// </summary>
[ECMAScript]
[Description("@#GPUProgrammableStage")]
public record GPUProgrammableStage(
    [property: Description("@#module")]GPUShaderModule? Module = default,
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#constants")]Dictionary<string, GPUPipelineConstantValue>? Constants = default);

/// <summary>
/// GPUQuerySetDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUQuerySetDescriptor")]
public record GPUQuerySetDescriptor(
    [property: Description("@#type")]GPUQueryType? Type = default,
    [property: Description("@#count")]GPUSize32? Count = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPURenderBundleEncoderDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundleEncoderDescriptor")]
public record GPURenderBundleEncoderDescriptor(
    [property: Description("@#depthReadOnly")]bool DepthReadOnly = false,
    [property: Description("@#stencilReadOnly")]bool StencilReadOnly = false) : GPURenderPassLayout;

/// <summary>
/// GPURenderPassColorAttachment
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassColorAttachment")]
public record GPURenderPassColorAttachment(
    [property: Description("@#view")]GPUTextureView? View = default,
    [property: Description("@#depthSlice")]GPUIntegerCoordinate? DepthSlice = default,
    [property: Description("@#resolveTarget")]GPUTextureView? ResolveTarget = default,
    [property: Description("@#clearValue")]GPUColor? ClearValue = default,
    [property: Description("@#loadOp")]GPULoadOp? LoadOp = default,
    [property: Description("@#storeOp")]GPUStoreOp? StoreOp = default);

/// <summary>
/// GPURenderPassDepthStencilAttachment
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassDepthStencilAttachment")]
public record GPURenderPassDepthStencilAttachment(
    [property: Description("@#view")]GPUTextureView? View = default,
    [property: Description("@#depthClearValue")]float DepthClearValue = default,
    [property: Description("@#depthLoadOp")]GPULoadOp? DepthLoadOp = default,
    [property: Description("@#depthStoreOp")]GPUStoreOp? DepthStoreOp = default,
    [property: Description("@#depthReadOnly")]bool DepthReadOnly = false,
    [property: Description("@#stencilClearValue")]GPUStencilValue? StencilClearValue = default,
    [property: Description("@#stencilLoadOp")]GPULoadOp? StencilLoadOp = default,
    [property: Description("@#stencilStoreOp")]GPUStoreOp? StencilStoreOp = default,
    [property: Description("@#stencilReadOnly")]bool StencilReadOnly = false);

/// <summary>
/// GPURenderPassDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassDescriptor")]
public record GPURenderPassDescriptor(
    [property: Description("@#colorAttachments")]GPURenderPassColorAttachment?[]? ColorAttachments = default,
    [property: Description("@#depthStencilAttachment")]GPURenderPassDepthStencilAttachment? DepthStencilAttachment = default,
    [property: Description("@#occlusionQuerySet")]GPUQuerySet? OcclusionQuerySet = default,
    [property: Description("@#timestampWrites")]GPURenderPassTimestampWrites? TimestampWrites = default,
    [property: Description("@#maxDrawCount")]GPUSize64? MaxDrawCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPURenderPassLayout
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassLayout")]
public record GPURenderPassLayout(
    [property: Description("@#colorFormats")]GPUTextureFormat?[]? ColorFormats = default,
    [property: Description("@#depthStencilFormat")]GPUTextureFormat? DepthStencilFormat = default,
    [property: Description("@#sampleCount")]GPUSize32? SampleCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPURenderPassTimestampWrites
/// </summary>
[ECMAScript]
[Description("@#GPURenderPassTimestampWrites")]
public record GPURenderPassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// GPURenderPipelineDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPURenderPipelineDescriptor")]
public record GPURenderPipelineDescriptor(
    [property: Description("@#vertex")]GPUVertexState? Vertex = default,
    [property: Description("@#primitive")]GPUPrimitiveState? Primitive = default,
    [property: Description("@#depthStencil")]GPUDepthStencilState? DepthStencil = default,
    [property: Description("@#multisample")]GPUMultisampleState? Multisample = default,
    [property: Description("@#fragment")]GPUFragmentState? Fragment = default) : GPUPipelineDescriptorBase;

/// <summary>
/// GPURequestAdapterOptions
/// </summary>
[ECMAScript]
[Description("@#GPURequestAdapterOptions")]
public record GPURequestAdapterOptions(
    [property: Description("@#powerPreference")]GPUPowerPreference? PowerPreference = default,
    [property: Description("@#forceFallbackAdapter")]bool ForceFallbackAdapter = false);

/// <summary>
/// GPUSamplerBindingLayout
/// </summary>
[ECMAScript]
[Description("@#GPUSamplerBindingLayout")]
public record GPUSamplerBindingLayout(
    [property: Description("@#type")]GPUSamplerBindingType Type = GPUSamplerBindingType.Filtering);

/// <summary>
/// GPUSamplerDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUSamplerDescriptor")]
public record GPUSamplerDescriptor(
    [property: Description("@#addressModeU")]GPUAddressMode AddressModeU = GPUAddressMode.ClampToEdge,
    [property: Description("@#addressModeV")]GPUAddressMode AddressModeV = GPUAddressMode.ClampToEdge,
    [property: Description("@#addressModeW")]GPUAddressMode AddressModeW = GPUAddressMode.ClampToEdge,
    [property: Description("@#magFilter")]GPUFilterMode MagFilter = GPUFilterMode.Nearest,
    [property: Description("@#minFilter")]GPUFilterMode MinFilter = GPUFilterMode.Nearest,
    [property: Description("@#mipmapFilter")]GPUMipmapFilterMode MipmapFilter = GPUMipmapFilterMode.Nearest,
    [property: Description("@#lodMinClamp")]float LodMinClamp = 0f,
    [property: Description("@#lodMaxClamp")]float LodMaxClamp = 32f,
    [property: Description("@#compare")]GPUCompareFunction? Compare = default,
    [property: Description("@#maxAnisotropy")]ushort MaxAnisotropy = 1) : GPUObjectDescriptorBase;

/// <summary>
/// GPUShaderModuleCompilationHint
/// </summary>
[ECMAScript]
[Description("@#GPUShaderModuleCompilationHint")]
public record GPUShaderModuleCompilationHint(
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#layout")]Either<GPUPipelineLayout, GPUAutoLayoutMode>? Layout = default);

/// <summary>
/// GPUShaderModuleDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUShaderModuleDescriptor")]
public record GPUShaderModuleDescriptor(
    [property: Description("@#code")]string? Code = default,
    [property: Description("@#sourceMap")]object? SourceMap = default,
    [property: Description("@#compilationHints")]GPUShaderModuleCompilationHint[]? CompilationHints = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUStencilFaceState
/// </summary>
[ECMAScript]
[Description("@#GPUStencilFaceState")]
public record GPUStencilFaceState(
    [property: Description("@#compare")]GPUCompareFunction Compare = GPUCompareFunction.Always,
    [property: Description("@#failOp")]GPUStencilOperation FailOp = GPUStencilOperation.Keep,
    [property: Description("@#depthFailOp")]GPUStencilOperation DepthFailOp = GPUStencilOperation.Keep,
    [property: Description("@#passOp")]GPUStencilOperation PassOp = GPUStencilOperation.Keep);

/// <summary>
/// GPUStorageTextureBindingLayout
/// </summary>
[ECMAScript]
[Description("@#GPUStorageTextureBindingLayout")]
public record GPUStorageTextureBindingLayout(
    [property: Description("@#access")]GPUStorageTextureAccess Access = GPUStorageTextureAccess.WriteOnly,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d);

/// <summary>
/// GPUTextureBindingLayout
/// </summary>
[ECMAScript]
[Description("@#GPUTextureBindingLayout")]
public record GPUTextureBindingLayout(
    [property: Description("@#sampleType")]GPUTextureSampleType SampleType = GPUTextureSampleType.Float,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d,
    [property: Description("@#multisampled")]bool Multisampled = false);

/// <summary>
/// GPUTextureDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUTextureDescriptor")]
public record GPUTextureDescriptor(
    [property: Description("@#size")]GPUExtent3D? Size = default,
    [property: Description("@#mipLevelCount")]GPUIntegerCoordinate? MipLevelCount = default,
    [property: Description("@#sampleCount")]GPUSize32? SampleCount = default,
    [property: Description("@#dimension")]GPUTextureDimension Dimension = GPUTextureDimension._2d,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#usage")]GPUTextureUsageFlags? Usage = default,
    [property: Description("@#viewFormats")]GPUTextureFormat[]? ViewFormats = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUTextureViewDescriptor
/// </summary>
[ECMAScript]
[Description("@#GPUTextureViewDescriptor")]
public record GPUTextureViewDescriptor(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#dimension")]GPUTextureViewDimension? Dimension = default,
    [property: Description("@#aspect")]GPUTextureAspect Aspect = GPUTextureAspect.All,
    [property: Description("@#baseMipLevel")]GPUIntegerCoordinate? BaseMipLevel = default,
    [property: Description("@#mipLevelCount")]GPUIntegerCoordinate? MipLevelCount = default,
    [property: Description("@#baseArrayLayer")]GPUIntegerCoordinate? BaseArrayLayer = default,
    [property: Description("@#arrayLayerCount")]GPUIntegerCoordinate? ArrayLayerCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// GPUUncapturedErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#GPUUncapturedErrorEventInit")]
public record GPUUncapturedErrorEventInit(
    [property: Description("@#error")]GPUError? Error = default) : EventInit;

/// <summary>
/// GPUVertexAttribute
/// </summary>
[ECMAScript]
[Description("@#GPUVertexAttribute")]
public record GPUVertexAttribute(
    [property: Description("@#format")]GPUVertexFormat? Format = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#shaderLocation")]GPUIndex32? ShaderLocation = default);

/// <summary>
/// GPUVertexBufferLayout
/// </summary>
[ECMAScript]
[Description("@#GPUVertexBufferLayout")]
public record GPUVertexBufferLayout(
    [property: Description("@#arrayStride")]GPUSize64? ArrayStride = default,
    [property: Description("@#stepMode")]GPUVertexStepMode StepMode = GPUVertexStepMode.Vertex,
    [property: Description("@#attributes")]GPUVertexAttribute[]? Attributes = default);

/// <summary>
/// GPUVertexState
/// </summary>
[ECMAScript]
[Description("@#GPUVertexState")]
public record GPUVertexState(
    [property: Description("@#buffers")]GPUVertexBufferLayout?[]? Buffers = default) : GPUProgrammableStage;
