namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgroupdescriptor">WebGPU: 8.2.1 Bind Group Creation</see>
/// </summary>
/// <param name="Layout"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupdescriptor-layout">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Entries"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupdescriptor-entries">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupDescriptor")]
public record GPUBindGroupDescriptor(
    [property: Description("@#layout")]GPUBindGroupLayout? Layout = default,
    [property: Description("@#entries")]GPUBindGroupEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgroupentry">WebGPU: 8.2.1 Bind Group Creation</see>
/// </summary>
/// <param name="Binding"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupentry-binding">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Resource"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupentry-resource">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupEntry")]
public record GPUBindGroupEntry(
    [property: Description("@#binding")]GPUIndex32? Binding = default,
    [property: Description("@#resource")]GPUBindingResource? Resource = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgrouplayoutdescriptor">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="Entries"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutdescriptor-entries">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupLayoutDescriptor")]
public record GPUBindGroupLayoutDescriptor(
    [property: Description("@#entries")]GPUBindGroupLayoutEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgrouplayoutentry">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="Binding"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-binding">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Visibility"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-visibility">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-buffer">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Sampler"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-sampler">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Texture"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-texture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="StorageTexture"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-storagetexture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ExternalTexture"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-externaltexture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
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
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpublendcomponent">WebGPU: 10.3.5.1 Blend State</see>
/// </summary>
/// <param name="Operation"><see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-operation">WebGPU: 10.3.5.1 Blend State</see></param>
/// <param name="SrcFactor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-srcfactor">WebGPU: 10.3.5.1 Blend State</see></param>
/// <param name="DstFactor"><see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-dstfactor">WebGPU: 10.3.5.1 Blend State</see></param>
[ECMAScript]
[Description("@#GPUBlendComponent")]
public record GPUBlendComponent(
    [property: Description("@#operation")]GPUBlendOperation Operation = GPUBlendOperation.Add,
    [property: Description("@#srcFactor")]GPUBlendFactor SrcFactor = GPUBlendFactor.One,
    [property: Description("@#dstFactor")]GPUBlendFactor DstFactor = GPUBlendFactor.Zero);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpublendstate">WebGPU: 10.3.5 Color Target State</see>
/// </summary>
/// <param name="Color"><see href="https://gpuweb.github.io/gpuweb/#dom-gpublendstate-color">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="Alpha"><see href="https://gpuweb.github.io/gpuweb/#dom-gpublendstate-alpha">WebGPU: 10.3.5 Color Target State</see></param>
[ECMAScript]
[Description("@#GPUBlendState")]
public record GPUBlendState(
    [property: Description("@#color")]GPUBlendComponent? Color = default,
    [property: Description("@#alpha")]GPUBlendComponent? Alpha = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubufferbinding">WebGPU: 8.2.1 Bind Group Creation</see>
/// </summary>
/// <param name="Buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-buffer">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-offset">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-size">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBufferBinding")]
public record GPUBufferBinding(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#size")]GPUSize64? Size = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubufferbindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="Type"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-type">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="HasDynamicOffset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-hasdynamicoffset">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="MinBindingSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-minbindingsize">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUBufferBindingLayout")]
public record GPUBufferBindingLayout(
    [property: Description("@#type")]GPUBufferBindingType Type = GPUBufferBindingType.Uniform,
    [property: Description("@#hasDynamicOffset")]bool HasDynamicOffset = false,
    [property: Description("@#minBindingSize")]GPUSize64? MinBindingSize = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucanvasconfiguration">WebGPU: 21.4 GPUCanvasConfiguration</see>
/// </summary>
/// <param name="Device"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-device">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-format">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="Usage"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-usage">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ViewFormats"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-viewformats">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ColorSpace"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-colorspace">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ToneMapping"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-tonemapping">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="AlphaMode"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-alphamode">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
[ECMAScript]
[Description("@#GPUCanvasConfiguration")]
public record GPUCanvasConfiguration(
    [property: Description("@#device")]GPUDevice? Device = default,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#usage")]GPUTextureUsageFlags? Usage = default,
    [property: Description("@#viewFormats")]GPUTextureFormat[]? ViewFormats = default,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#toneMapping")]GPUCanvasToneMapping? ToneMapping = default,
    [property: Description("@#alphaMode")]GPUCanvasAlphaMode AlphaMode = GPUCanvasAlphaMode.Opaque);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucanvastonemapping">WebGPU: 21.4 GPUCanvasConfiguration</see>
/// </summary>
/// <param name="Mode"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvastonemapping-mode">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
[ECMAScript]
[Description("@#GPUCanvasToneMapping")]
public record GPUCanvasToneMapping(
    [property: Description("@#mode")]GPUCanvasToneMappingMode Mode = GPUCanvasToneMappingMode.Standard);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucolordict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </summary>
/// <param name="R"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-r">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="G"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-g">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="B"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-b">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="A"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-a">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUColorDict")]
public record GPUColorDict(
    [property: Description("@#r")]double R = default,
    [property: Description("@#g")]double G = default,
    [property: Description("@#b")]double B = default,
    [property: Description("@#a")]double A = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucolortargetstate">WebGPU: 10.3.5 Color Target State</see>
/// </summary>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-format">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="Blend"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-blend">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="WriteMask"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-writemask">WebGPU: 10.3.5 Color Target State</see></param>
[ECMAScript]
[Description("@#GPUColorTargetState")]
public record GPUColorTargetState(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#blend")]GPUBlendState? Blend = default,
    [property: Description("@#writeMask")]GPUColorWriteFlags? WriteMask = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucommandbufferdescriptor">WebGPU: 12.1.1 Command Buffer Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCommandBufferDescriptor")]
public abstract record GPUCommandBufferDescriptor();

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucommandencoderdescriptor">WebGPU: 13.2.1 Command Encoder Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPUCommandEncoderDescriptor")]
public abstract record GPUCommandEncoderDescriptor();

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepassdescriptor">WebGPU: 16.1.1 Compute Pass Encoder Creation</see>
/// </summary>
/// <param name="TimestampWrites"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassdescriptor-timestampwrites">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePassDescriptor")]
public record GPUComputePassDescriptor(
    [property: Description("@#timestampWrites")]GPUComputePassTimestampWrites? TimestampWrites = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepasstimestampwrites">WebGPU: 16.1.1 Compute Pass Encoder Creation</see>
/// </summary>
/// <param name="QuerySet"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-queryset">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
/// <param name="BeginningOfPassWriteIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-beginningofpasswriteindex">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
/// <param name="EndOfPassWriteIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-endofpasswriteindex">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePassTimestampWrites")]
public record GPUComputePassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepipelinedescriptor">WebGPU: 10.2.1 Compute Pipeline Creation</see>
/// </summary>
/// <param name="Compute"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepipelinedescriptor-compute">WebGPU: 10.2.1 Compute Pipeline Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePipelineDescriptor")]
public record GPUComputePipelineDescriptor(
    [property: Description("@#compute")]GPUProgrammableStage? Compute = default) : GPUPipelineDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpudepthstencilstate">WebGPU: 10.3.6 Depth/Stencil State</see>
/// </summary>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-format">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthWriteEnabled"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthwriteenabled">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthCompare"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthcompare">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilFront"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilfront">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilBack"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilback">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilReadMask"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilreadmask">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilWriteMask"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilwritemask">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBias"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbias">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBiasSlopeScale"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbiasslopescale">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBiasClamp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbiasclamp">WebGPU: 10.3.6 Depth/Stencil State</see></param>
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
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuextent3ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </summary>
/// <param name="Width"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-width">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Height"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-height">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="DepthOrArrayLayers"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-depthorarraylayers">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUExtent3DDict")]
public record GPUExtent3DDict(
    [property: Description("@#width")]GPUIntegerCoordinate? Width = default,
    [property: Description("@#height")]GPUIntegerCoordinate? Height = default,
    [property: Description("@#depthOrArrayLayers")]GPUIntegerCoordinate? DepthOrArrayLayers = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuexternaltexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPUExternalTextureBindingLayout")]
public abstract record GPUExternalTextureBindingLayout();

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuexternaltexturedescriptor">WebGPU: 6.4.1 Importing External Textures</see>
/// </summary>
/// <param name="Source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuexternaltexturedescriptor-source">WebGPU: 6.4.1 Importing External Textures</see></param>
/// <param name="ColorSpace"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuexternaltexturedescriptor-colorspace">WebGPU: 6.4.1 Importing External Textures</see></param>
[ECMAScript]
[Description("@#GPUExternalTextureDescriptor")]
public record GPUExternalTextureDescriptor(
    [property: Description("@#source")]GPUExternalTextureDescriptorSource? Source = default,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpufragmentstate">WebGPU: 10.3.4 Fragment State</see>
/// </summary>
/// <param name="Targets"><see href="https://gpuweb.github.io/gpuweb/#dom-gpufragmentstate-targets">WebGPU: 10.3.4 Fragment State</see></param>
[ECMAScript]
[Description("@#GPUFragmentState")]
public record GPUFragmentState(
    [property: Description("@#targets")]GPUColorTargetState?[]? Targets = default) : GPUProgrammableStage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpumultisamplestate">WebGPU: 10.3.3 Multisample State</see>
/// </summary>
/// <param name="Count"><see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-count">WebGPU: 10.3.3 Multisample State</see></param>
/// <param name="Mask"><see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-mask">WebGPU: 10.3.3 Multisample State</see></param>
/// <param name="AlphaToCoverageEnabled"><see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-alphatocoverageenabled">WebGPU: 10.3.3 Multisample State</see></param>
[ECMAScript]
[Description("@#GPUMultisampleState")]
public record GPUMultisampleState(
    [property: Description("@#count")]GPUSize32? Count = default,
    [property: Description("@#mask")]GPUSampleMask? Mask = default,
    [property: Description("@#alphaToCoverageEnabled")]bool AlphaToCoverageEnabled = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuobjectdescriptorbase">WebGPU: 3.1.3 Object Descriptors</see>
/// </summary>
/// <param name="Label"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectdescriptorbase-label">WebGPU: 3.1.3 Object Descriptors</see></param>
[ECMAScript]
[Description("@#GPUObjectDescriptorBase")]
public record GPUObjectDescriptorBase(
    [property: Description("@#label")]string? Label = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuorigin2ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </summary>
/// <param name="X"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin2ddict-x">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Y"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin2ddict-y">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUOrigin2DDict")]
public record GPUOrigin2DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuorigin3ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </summary>
/// <param name="X"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-x">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Y"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-y">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Z"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-z">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUOrigin3DDict")]
public record GPUOrigin3DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default,
    [property: Description("@#z")]GPUIntegerCoordinate? Z = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelinedescriptorbase">WebGPU: 10.1 Base pipelines</see>
/// </summary>
/// <param name="Layout"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinedescriptorbase-layout">WebGPU: 10.1 Base pipelines</see></param>
[ECMAScript]
[Description("@#GPUPipelineDescriptorBase")]
public record GPUPipelineDescriptorBase(
    [property: Description("@#layout")]GPUPipelineDescriptorBaseLayout? Layout = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelineerrorinit">WebGPU: 10. Pipelines</see>
/// </summary>
/// <param name="Reason"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerrorinit-reason">WebGPU: 10. Pipelines</see></param>
[ECMAScript]
[Description("@#GPUPipelineErrorInit")]
public record GPUPipelineErrorInit(
    [property: Description("@#reason")]GPUPipelineErrorReason? Reason = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelinelayoutdescriptor">WebGPU: 8.3.1 Pipeline Layout Creation</see>
/// </summary>
/// <param name="BindGroupLayouts"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinelayoutdescriptor-bindgrouplayouts">WebGPU: 8.3.1 Pipeline Layout Creation</see></param>
/// <param name="ImmediateSize"><see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinelayoutdescriptor-immediatesize">WebGPU: 8.3.1 Pipeline Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUPipelineLayoutDescriptor")]
public record GPUPipelineLayoutDescriptor(
    [property: Description("@#bindGroupLayouts")]GPUBindGroupLayout?[]? BindGroupLayouts = default,
    [property: Description("@#immediateSize")]GPUSize32? ImmediateSize = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuprimitivestate">WebGPU: 10.3.2 Primitive State</see>
/// </summary>
/// <param name="Topology"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-topology">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="StripIndexFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-stripindexformat">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="FrontFace"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-frontface">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="CullMode"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-cullmode">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="UnclippedDepth"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-unclippeddepth">WebGPU: 10.3.2 Primitive State</see></param>
[ECMAScript]
[Description("@#GPUPrimitiveState")]
public record GPUPrimitiveState(
    [property: Description("@#topology")]GPUPrimitiveTopology Topology = GPUPrimitiveTopology.TriangleList,
    [property: Description("@#stripIndexFormat")]GPUIndexFormat? StripIndexFormat = default,
    [property: Description("@#frontFace")]GPUFrontFace FrontFace = GPUFrontFace.Ccw,
    [property: Description("@#cullMode")]GPUCullMode CullMode = GPUCullMode.None,
    [property: Description("@#unclippedDepth")]bool UnclippedDepth = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuquerysetdescriptor">WebGPU: 20.1.1 QuerySet Creation</see>
/// </summary>
/// <param name="Type"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerysetdescriptor-type">WebGPU: 20.1.1 QuerySet Creation</see></param>
/// <param name="Count"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerysetdescriptor-count">WebGPU: 20.1.1 QuerySet Creation</see></param>
[ECMAScript]
[Description("@#GPUQuerySetDescriptor")]
public record GPUQuerySetDescriptor(
    [property: Description("@#type")]GPUQueryType? Type = default,
    [property: Description("@#count")]GPUSize32? Count = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderbundledescriptor">WebGPU: 18.1.1 Render Bundle Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPURenderBundleDescriptor")]
public abstract record GPURenderBundleDescriptor();

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderbundleencoderdescriptor">WebGPU: 18.1.2 Encoding</see>
/// </summary>
/// <param name="DepthReadOnly"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoderdescriptor-depthreadonly">WebGPU: 18.1.2 Encoding</see></param>
/// <param name="StencilReadOnly"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoderdescriptor-stencilreadonly">WebGPU: 18.1.2 Encoding</see></param>
[ECMAScript]
[Description("@#GPURenderBundleEncoderDescriptor")]
public record GPURenderBundleEncoderDescriptor(
    [property: Description("@#depthReadOnly")]bool DepthReadOnly = false,
    [property: Description("@#stencilReadOnly")]bool StencilReadOnly = false) : GPURenderPassLayout;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasscolorattachment">WebGPU: 17.1.1.1 Color Attachments</see>
/// </summary>
/// <param name="View"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-view">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="DepthSlice"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-depthslice">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="ResolveTarget"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-resolvetarget">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="ClearValue"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-clearvalue">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="LoadOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-loadop">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="StoreOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-storeop">WebGPU: 17.1.1.1 Color Attachments</see></param>
[ECMAScript]
[Description("@#GPURenderPassColorAttachment")]
public record GPURenderPassColorAttachment(
    [property: Description("@#view")]GPURenderPassColorAttachmentView? View = default,
    [property: Description("@#depthSlice")]GPUIntegerCoordinate? DepthSlice = default,
    [property: Description("@#resolveTarget")]GPURenderPassColorAttachmentResolveTarget? ResolveTarget = default,
    [property: Description("@#clearValue")]GPUColor? ClearValue = default,
    [property: Description("@#loadOp")]GPULoadOp? LoadOp = default,
    [property: Description("@#storeOp")]GPUStoreOp? StoreOp = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpassdepthstencilattachment">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see>
/// </summary>
/// <param name="View"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-view">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthClearValue"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthclearvalue">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthLoadOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthloadop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthStoreOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthstoreop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthReadOnly"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthreadonly">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilClearValue"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilclearvalue">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilLoadOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilloadop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilStoreOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilstoreop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilReadOnly"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilreadonly">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
[ECMAScript]
[Description("@#GPURenderPassDepthStencilAttachment")]
public record GPURenderPassDepthStencilAttachment(
    [property: Description("@#view")]GPURenderPassDepthStencilAttachmentView? View = default,
    [property: Description("@#depthClearValue")]float DepthClearValue = default,
    [property: Description("@#depthLoadOp")]GPULoadOp? DepthLoadOp = default,
    [property: Description("@#depthStoreOp")]GPUStoreOp? DepthStoreOp = default,
    [property: Description("@#depthReadOnly")]bool DepthReadOnly = false,
    [property: Description("@#stencilClearValue")]GPUStencilValue? StencilClearValue = default,
    [property: Description("@#stencilLoadOp")]GPULoadOp? StencilLoadOp = default,
    [property: Description("@#stencilStoreOp")]GPUStoreOp? StencilStoreOp = default,
    [property: Description("@#stencilReadOnly")]bool StencilReadOnly = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpassdescriptor">WebGPU: 17.1.1 Render Pass Encoder Creation</see>
/// </summary>
/// <param name="ColorAttachments"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-colorattachments">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="DepthStencilAttachment"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-depthstencilattachment">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="OcclusionQuerySet"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-occlusionqueryset">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="TimestampWrites"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-timestampwrites">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="MaxDrawCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-maxdrawcount">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPassDescriptor")]
public record GPURenderPassDescriptor(
    [property: Description("@#colorAttachments")]GPURenderPassColorAttachment?[]? ColorAttachments = default,
    [property: Description("@#depthStencilAttachment")]GPURenderPassDepthStencilAttachment? DepthStencilAttachment = default,
    [property: Description("@#occlusionQuerySet")]GPUQuerySet? OcclusionQuerySet = default,
    [property: Description("@#timestampWrites")]GPURenderPassTimestampWrites? TimestampWrites = default,
    [property: Description("@#maxDrawCount")]GPUSize64? MaxDrawCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasslayout">WebGPU: 17.1.1.4 Render Pass Layout</see>
/// </summary>
/// <param name="ColorFormats"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-colorformats">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
/// <param name="DepthStencilFormat"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-depthstencilformat">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
/// <param name="SampleCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-samplecount">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
[ECMAScript]
[Description("@#GPURenderPassLayout")]
public record GPURenderPassLayout(
    [property: Description("@#colorFormats")]GPUTextureFormat?[]? ColorFormats = default,
    [property: Description("@#depthStencilFormat")]GPUTextureFormat? DepthStencilFormat = default,
    [property: Description("@#sampleCount")]GPUSize32? SampleCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasstimestampwrites">WebGPU: 17.1.1 Render Pass Encoder Creation</see>
/// </summary>
/// <param name="QuerySet"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-queryset">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="BeginningOfPassWriteIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-beginningofpasswriteindex">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="EndOfPassWriteIndex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-endofpasswriteindex">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPassTimestampWrites")]
public record GPURenderPassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpipelinedescriptor">WebGPU: 10.3.1 Render Pipeline Creation</see>
/// </summary>
/// <param name="Vertex"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-vertex">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Primitive"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-primitive">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="DepthStencil"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-depthstencil">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Multisample"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-multisample">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Fragment"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-fragment">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPipelineDescriptor")]
public record GPURenderPipelineDescriptor(
    [property: Description("@#vertex")]GPUVertexState? Vertex = default,
    [property: Description("@#primitive")]GPUPrimitiveState? Primitive = default,
    [property: Description("@#depthStencil")]GPUDepthStencilState? DepthStencil = default,
    [property: Description("@#multisample")]GPUMultisampleState? Multisample = default,
    [property: Description("@#fragment")]GPUFragmentState? Fragment = default) : GPUPipelineDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurequestadapteroptions">WebGPU: 4.2.2 Adapter Selection</see>
/// </summary>
/// <param name="FeatureLevel"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-featurelevel">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="PowerPreference"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-powerpreference">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="ForceFallbackAdapter"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-forcefallbackadapter">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="XrCompatible"><see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-xrcompatible">WebGPU: 4.2.2 Adapter Selection</see></param>
[ECMAScript]
[Description("@#GPURequestAdapterOptions")]
public record GPURequestAdapterOptions(
    [property: Description("@#featureLevel")]string? FeatureLevel = default,
    [property: Description("@#powerPreference")]GPUPowerPreference? PowerPreference = default,
    [property: Description("@#forceFallbackAdapter")]bool ForceFallbackAdapter = false,
    [property: Description("@#xrCompatible")]bool XrCompatible = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpusamplerbindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="Type"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerbindinglayout-type">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUSamplerBindingLayout")]
public record GPUSamplerBindingLayout(
    [property: Description("@#type")]GPUSamplerBindingType Type = GPUSamplerBindingType.Filtering);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpusamplerdescriptor">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </summary>
/// <param name="AddressModeU"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodeu">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="AddressModeV"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodev">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="AddressModeW"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodew">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MagFilter"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-magfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MinFilter"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-minfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MipmapFilter"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-mipmapfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="LodMinClamp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-lodminclamp">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="LodMaxClamp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-lodmaxclamp">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="Compare"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-compare">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MaxAnisotropy"><see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-maxanisotropy">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
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
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpushadermodulecompilationhint">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see>
/// </summary>
/// <param name="EntryPoint"><see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermodulecompilationhint-entrypoint">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see></param>
/// <param name="Layout"><see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermodulecompilationhint-layout">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see></param>
[ECMAScript]
[Description("@#GPUShaderModuleCompilationHint")]
public record GPUShaderModuleCompilationHint(
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#layout")]GPUShaderModuleCompilationHintLayout? Layout = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpushadermoduledescriptor">WebGPU: 9.1.1 Shader Module Creation</see>
/// </summary>
/// <param name="Code"><see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermoduledescriptor-code">WebGPU: 9.1.1 Shader Module Creation</see></param>
/// <param name="CompilationHints"><see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermoduledescriptor-compilationhints">WebGPU: 9.1.1 Shader Module Creation</see></param>
[ECMAScript]
[Description("@#GPUShaderModuleDescriptor")]
public record GPUShaderModuleDescriptor(
    [property: Description("@#code")]string? Code = default,
    [property: Description("@#compilationHints")]GPUShaderModuleCompilationHint[]? CompilationHints = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpustencilfacestate">WebGPU: 10.3.6 Depth/Stencil State</see>
/// </summary>
/// <param name="Compare"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-compare">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="FailOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-failop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthFailOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-depthfailop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="PassOp"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-passop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
[ECMAScript]
[Description("@#GPUStencilFaceState")]
public record GPUStencilFaceState(
    [property: Description("@#compare")]GPUCompareFunction Compare = GPUCompareFunction.Always,
    [property: Description("@#failOp")]GPUStencilOperation FailOp = GPUStencilOperation.Keep,
    [property: Description("@#depthFailOp")]GPUStencilOperation DepthFailOp = GPUStencilOperation.Keep,
    [property: Description("@#passOp")]GPUStencilOperation PassOp = GPUStencilOperation.Keep);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpustoragetexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="Access"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-access">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-format">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ViewDimension"><see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-viewdimension">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUStorageTextureBindingLayout")]
public record GPUStorageTextureBindingLayout(
    [property: Description("@#access")]GPUStorageTextureAccess Access = GPUStorageTextureAccess.WriteOnly,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gputexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
/// <param name="SampleType"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-sampletype">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ViewDimension"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-viewdimension">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Multisampled"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-multisampled">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUTextureBindingLayout")]
public record GPUTextureBindingLayout(
    [property: Description("@#sampleType")]GPUTextureSampleType SampleType = GPUTextureSampleType.Float,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d,
    [property: Description("@#multisampled")]bool Multisampled = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gputextureviewdescriptor">WebGPU: 6.2.1 Texture View Creation</see>
/// </summary>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-format">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Dimension"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-dimension">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Usage"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-usage">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Aspect"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-aspect">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="BaseMipLevel"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-basemiplevel">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="MipLevelCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-miplevelcount">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="BaseArrayLayer"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-basearraylayer">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="ArrayLayerCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-arraylayercount">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Swizzle"><see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-swizzle">WebGPU: 6.2.1 Texture View Creation</see></param>
[ECMAScript]
[Description("@#GPUTextureViewDescriptor")]
public record GPUTextureViewDescriptor(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#dimension")]GPUTextureViewDimension? Dimension = default,
    [property: Description("@#usage")]GPUTextureUsageFlags? Usage = default,
    [property: Description("@#aspect")]GPUTextureAspect Aspect = GPUTextureAspect.All,
    [property: Description("@#baseMipLevel")]GPUIntegerCoordinate? BaseMipLevel = default,
    [property: Description("@#mipLevelCount")]GPUIntegerCoordinate? MipLevelCount = default,
    [property: Description("@#baseArrayLayer")]GPUIntegerCoordinate? BaseArrayLayer = default,
    [property: Description("@#arrayLayerCount")]GPUIntegerCoordinate? ArrayLayerCount = default,
    [property: Description("@#swizzle")]string? Swizzle = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuuncapturederroreventinit">WebGPU: 22.4 Telemetry</see>
/// </summary>
/// <param name="Error"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederroreventinit-error">WebGPU: 22.4 Telemetry</see></param>
[ECMAScript]
[Description("@#GPUUncapturedErrorEventInit")]
public record GPUUncapturedErrorEventInit(
    [property: Description("@#error")]GPUError? Error = default) : EventInit;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexattribute">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </summary>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-format">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="Offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-offset">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="ShaderLocation"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-shaderlocation">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexAttribute")]
public record GPUVertexAttribute(
    [property: Description("@#format")]GPUVertexFormat? Format = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#shaderLocation")]GPUIndex32? ShaderLocation = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexbufferlayout">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </summary>
/// <param name="ArrayStride"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-arraystride">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="StepMode"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-stepmode">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="Attributes"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-attributes">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexBufferLayout")]
public record GPUVertexBufferLayout(
    [property: Description("@#arrayStride")]GPUSize64? ArrayStride = default,
    [property: Description("@#stepMode")]GPUVertexStepMode StepMode = GPUVertexStepMode.Vertex,
    [property: Description("@#attributes")]GPUVertexAttribute[]? Attributes = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexstate">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </summary>
/// <param name="Buffers"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexstate-buffers">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexState")]
public record GPUVertexState(
    [property: Description("@#buffers")]GPUVertexBufferLayout?[]? Buffers = default) : GPUProgrammableStage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpubufferdescriptor">WebGPU: 5.1.1 GPUBufferDescriptor</see>
/// </summary>
/// <param name="Size"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-size">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
/// <param name="Usage"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-usage">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
/// <param name="MappedAtCreation"><see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-mappedatcreation">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
[ECMAScript]
[Description("@#GPUBufferDescriptor")]
public record GPUBufferDescriptor(
    [property: Description("@#size")]GPUSize64? Size = default,
    [property: Description("@#usage")]GPUBufferUsageFlags? Usage = default,
    [property: Description("@#mappedAtCreation")]bool MappedAtCreation = false) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucopyexternalimagedestinfo">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see>
/// </summary>
/// <param name="ColorSpace"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagedestinfo-colorspace">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see></param>
/// <param name="PremultipliedAlpha"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagedestinfo-premultipliedalpha">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see></param>
[ECMAScript]
[Description("@#GPUCopyExternalImageDestInfo")]
public record GPUCopyExternalImageDestInfo(
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#premultipliedAlpha")]bool PremultipliedAlpha = false) : GPUTexelCopyTextureInfo;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucopyexternalimagesourceinfo">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see>
/// </summary>
/// <param name="Source"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-source">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
/// <param name="Origin"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-origin">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
/// <param name="FlipY"><see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-flipy">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
[ECMAScript]
[Description("@#GPUCopyExternalImageSourceInfo")]
public record GPUCopyExternalImageSourceInfo(
    [property: Description("@#source")]GPUCopyExternalImageSource? Source = default,
    [property: Description("@#origin")]GPUOrigin2D? Origin = default,
    [property: Description("@#flipY")]bool FlipY = false);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpudevicedescriptor">WebGPU: 4.3.1 GPUDeviceDescriptor</see>
/// </summary>
/// <param name="RequiredFeatures"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-requiredfeatures">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
/// <param name="RequiredLimits"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-requiredlimits">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
/// <param name="DefaultQueue"><see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-defaultqueue">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
[ECMAScript]
[Description("@#GPUDeviceDescriptor")]
public record GPUDeviceDescriptor(
    [property: Description("@#requiredFeatures")]GPUFeatureName[]? RequiredFeatures = default,
    [property: Description("@#requiredLimits")]Dictionary<string, GPUSize64?>? RequiredLimits = default,
    [property: Description("@#defaultQueue")]GPUQueueDescriptor? DefaultQueue = default) : GPUObjectDescriptorBase;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuprogrammablestage">WebGPU: 10.1.2 GPUProgrammableStage</see>
/// </summary>
/// <param name="Module"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-module">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
/// <param name="EntryPoint"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-entrypoint">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
/// <param name="Constants"><see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-constants">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
[ECMAScript]
[Description("@#GPUProgrammableStage")]
public record GPUProgrammableStage(
    [property: Description("@#module")]GPUShaderModule? Module = default,
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#constants")]Dictionary<string, GPUPipelineConstantValue>? Constants = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuqueuedescriptor">WebGPU: 19.1 GPUQueueDescriptor</see>
/// </summary>
[ECMAScript]
[Description("@#GPUQueueDescriptor")]
public abstract record GPUQueueDescriptor();

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopybufferinfo">WebGPU: 11.2.2 GPUTexelCopyBufferInfo</see>
/// </summary>
/// <param name="Buffer"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferinfo-buffer">WebGPU: 11.2.2 GPUTexelCopyBufferInfo</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyBufferInfo")]
public record GPUTexelCopyBufferInfo(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default) : GPUTexelCopyBufferLayout;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopybufferlayout">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see>
/// </summary>
/// <param name="Offset"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-offset">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
/// <param name="BytesPerRow"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-bytesperrow">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
/// <param name="RowsPerImage"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-rowsperimage">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyBufferLayout")]
public record GPUTexelCopyBufferLayout(
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#bytesPerRow")]GPUSize32? BytesPerRow = default,
    [property: Description("@#rowsPerImage")]GPUSize32? RowsPerImage = default);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopytextureinfo">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see>
/// </summary>
/// <param name="Texture"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-texture">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="MipLevel"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-miplevel">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="Origin"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-origin">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="Aspect"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-aspect">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyTextureInfo")]
public record GPUTexelCopyTextureInfo(
    [property: Description("@#texture")]GPUTexture? Texture = default,
    [property: Description("@#mipLevel")]GPUIntegerCoordinate? MipLevel = default,
    [property: Description("@#origin")]GPUOrigin3D? Origin = default,
    [property: Description("@#aspect")]GPUTextureAspect Aspect = GPUTextureAspect.All);

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexturedescriptor">WebGPU: 6.1.1 GPUTextureDescriptor</see>
/// </summary>
/// <param name="Size"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-size">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="MipLevelCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-miplevelcount">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="SampleCount"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-samplecount">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Dimension"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-dimension">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Format"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-format">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Usage"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-usage">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="ViewFormats"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-viewformats">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="TextureBindingViewDimension"><see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-texturebindingviewdimension">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
[ECMAScript]
[Description("@#GPUTextureDescriptor")]
public record GPUTextureDescriptor(
    [property: Description("@#size")]GPUExtent3D? Size = default,
    [property: Description("@#mipLevelCount")]GPUIntegerCoordinate? MipLevelCount = default,
    [property: Description("@#sampleCount")]GPUSize32? SampleCount = default,
    [property: Description("@#dimension")]GPUTextureDimension Dimension = GPUTextureDimension._2d,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#usage")]GPUTextureUsageFlags? Usage = default,
    [property: Description("@#viewFormats")]GPUTextureFormat[]? ViewFormats = default,
    [property: Description("@#textureBindingViewDimension")]GPUTextureViewDimension? TextureBindingViewDimension = default) : GPUObjectDescriptorBase;
