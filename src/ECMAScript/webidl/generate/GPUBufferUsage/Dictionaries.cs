namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// WebIDL dictionary GPUBindGroupDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgroupdescriptor">WebGPU: 8.2.1 Bind Group Creation</see>
/// </remarks>
/// <param name="Layout">GPUBindGroupDescriptor 字典中的 layout 成员，WebIDL 类型为 GPUBindGroupLayout。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupdescriptor-layout">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Entries">GPUBindGroupDescriptor 字典中的 entries 成员，WebIDL 类型为 sequence&lt;GPUBindGroupEntry&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupdescriptor-entries">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupDescriptor")]
public record GPUBindGroupDescriptor(
    [property: Description("@#layout")]GPUBindGroupLayout? Layout = default,
    [property: Description("@#entries")]GPUBindGroupEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUBindGroupEntry。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgroupentry">WebGPU: 8.2.1 Bind Group Creation</see>
/// </remarks>
/// <param name="Binding">GPUBindGroupEntry 字典中的 binding 成员，WebIDL 类型为 GPUIndex32。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupentry-binding">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Resource">GPUBindGroupEntry 字典中的 resource 成员，WebIDL 类型为 GPUBindingResource。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgroupentry-resource">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupEntry")]
public record GPUBindGroupEntry(
    [property: Description("@#binding")]GPUIndex32? Binding = default,
    [property: Description("@#resource")]GPUBindingResource? Resource = default);

/// <summary>
/// WebIDL dictionary GPUBindGroupLayoutDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgrouplayoutdescriptor">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="Entries">GPUBindGroupLayoutDescriptor 字典中的 entries 成员，WebIDL 类型为 sequence&lt;GPUBindGroupLayoutEntry&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutdescriptor-entries">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUBindGroupLayoutDescriptor")]
public record GPUBindGroupLayoutDescriptor(
    [property: Description("@#entries")]GPUBindGroupLayoutEntry[]? Entries = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUBindGroupLayoutEntry。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubindgrouplayoutentry">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="Binding">GPUBindGroupLayoutEntry 字典中的 binding 成员，WebIDL 类型为 GPUIndex32。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-binding">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Visibility">GPUBindGroupLayoutEntry 字典中的 visibility 成员，WebIDL 类型为 GPUShaderStageFlags。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-visibility">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Buffer">GPUBindGroupLayoutEntry 字典中的 buffer 成员，WebIDL 类型为 GPUBufferBindingLayout。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-buffer">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Sampler">GPUBindGroupLayoutEntry 字典中的 sampler 成员，WebIDL 类型为 GPUSamplerBindingLayout。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-sampler">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Texture">GPUBindGroupLayoutEntry 字典中的 texture 成员，WebIDL 类型为 GPUTextureBindingLayout。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-texture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="StorageTexture">GPUBindGroupLayoutEntry 字典中的 storageTexture 成员，WebIDL 类型为 GPUStorageTextureBindingLayout。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-storagetexture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ExternalTexture">GPUBindGroupLayoutEntry 字典中的 externalTexture 成员，WebIDL 类型为 GPUExternalTextureBindingLayout。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubindgrouplayoutentry-externaltexture">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
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
/// WebIDL dictionary GPUBlendComponent。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpublendcomponent">WebGPU: 10.3.5.1 Blend State</see>
/// </remarks>
/// <param name="Operation">GPUBlendComponent 字典中的 operation 成员，WebIDL 类型为 GPUBlendOperation。可省略。WebIDL 默认值：add。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-operation">WebGPU: 10.3.5.1 Blend State</see></param>
/// <param name="SrcFactor">GPUBlendComponent 字典中的 srcFactor 成员，WebIDL 类型为 GPUBlendFactor。可省略。WebIDL 默认值：one。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-srcfactor">WebGPU: 10.3.5.1 Blend State</see></param>
/// <param name="DstFactor">GPUBlendComponent 字典中的 dstFactor 成员，WebIDL 类型为 GPUBlendFactor。可省略。WebIDL 默认值：zero。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendcomponent-dstfactor">WebGPU: 10.3.5.1 Blend State</see></param>
[ECMAScript]
[Description("@#GPUBlendComponent")]
public record GPUBlendComponent(
    [property: Description("@#operation")]GPUBlendOperation Operation = GPUBlendOperation.Add,
    [property: Description("@#srcFactor")]GPUBlendFactor SrcFactor = GPUBlendFactor.One,
    [property: Description("@#dstFactor")]GPUBlendFactor DstFactor = GPUBlendFactor.Zero);

/// <summary>
/// WebIDL dictionary GPUBlendState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpublendstate">WebGPU: 10.3.5 Color Target State</see>
/// </remarks>
/// <param name="Color">GPUBlendState 字典中的 color 成员，WebIDL 类型为 GPUBlendComponent。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendstate-color">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="Alpha">GPUBlendState 字典中的 alpha 成员，WebIDL 类型为 GPUBlendComponent。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpublendstate-alpha">WebGPU: 10.3.5 Color Target State</see></param>
[ECMAScript]
[Description("@#GPUBlendState")]
public record GPUBlendState(
    [property: Description("@#color")]GPUBlendComponent? Color = default,
    [property: Description("@#alpha")]GPUBlendComponent? Alpha = default);

/// <summary>
/// WebIDL dictionary GPUBufferBinding。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubufferbinding">WebGPU: 8.2.1 Bind Group Creation</see>
/// </remarks>
/// <param name="Buffer">GPUBufferBinding 字典中的 buffer 成员，WebIDL 类型为 GPUBuffer。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-buffer">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Offset">GPUBufferBinding 字典中的 offset 成员，WebIDL 类型为 GPUSize64。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-offset">WebGPU: 8.2.1 Bind Group Creation</see></param>
/// <param name="Size">GPUBufferBinding 字典中的 size 成员，WebIDL 类型为 GPUSize64。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbinding-size">WebGPU: 8.2.1 Bind Group Creation</see></param>
[ECMAScript]
[Description("@#GPUBufferBinding")]
public record GPUBufferBinding(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#size")]GPUSize64? Size = default);

/// <summary>
/// WebIDL dictionary GPUBufferBindingLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpubufferbindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="Type">GPUBufferBindingLayout 字典中的 type 成员，WebIDL 类型为 GPUBufferBindingType。可省略。WebIDL 默认值：uniform。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-type">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="HasDynamicOffset">GPUBufferBindingLayout 字典中的 hasDynamicOffset 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-hasdynamicoffset">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="MinBindingSize">GPUBufferBindingLayout 字典中的 minBindingSize 成员，WebIDL 类型为 GPUSize64。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferbindinglayout-minbindingsize">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUBufferBindingLayout")]
public record GPUBufferBindingLayout(
    [property: Description("@#type")]GPUBufferBindingType Type = GPUBufferBindingType.Uniform,
    [property: Description("@#hasDynamicOffset")]bool HasDynamicOffset = false,
    [property: Description("@#minBindingSize")]GPUSize64? MinBindingSize = default);

/// <summary>
/// WebIDL dictionary GPUCanvasConfiguration。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucanvasconfiguration">WebGPU: 21.4 GPUCanvasConfiguration</see>
/// </remarks>
/// <param name="Device">GPUCanvasConfiguration 字典中的 device 成员，WebIDL 类型为 GPUDevice。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-device">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="Format">GPUCanvasConfiguration 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-format">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="Usage">GPUCanvasConfiguration 字典中的 usage 成员，WebIDL 类型为 GPUTextureUsageFlags。可省略。WebIDL 默认值：0x10。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-usage">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ViewFormats">GPUCanvasConfiguration 字典中的 viewFormats 成员，WebIDL 类型为 sequence&lt;GPUTextureFormat&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-viewformats">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ColorSpace">GPUCanvasConfiguration 字典中的 colorSpace 成员，WebIDL 类型为 PredefinedColorSpace。可省略。WebIDL 默认值：srgb。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-colorspace">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="ToneMapping">GPUCanvasConfiguration 字典中的 toneMapping 成员，WebIDL 类型为 GPUCanvasToneMapping。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-tonemapping">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
/// <param name="AlphaMode">GPUCanvasConfiguration 字典中的 alphaMode 成员，WebIDL 类型为 GPUCanvasAlphaMode。可省略。WebIDL 默认值：opaque。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvasconfiguration-alphamode">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
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
/// WebIDL dictionary GPUCanvasToneMapping。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucanvastonemapping">WebGPU: 21.4 GPUCanvasConfiguration</see>
/// </remarks>
/// <param name="Mode">GPUCanvasToneMapping 字典中的 mode 成员，WebIDL 类型为 GPUCanvasToneMappingMode。可省略。WebIDL 默认值：standard。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucanvastonemapping-mode">WebGPU: 21.4 GPUCanvasConfiguration</see></param>
[ECMAScript]
[Description("@#GPUCanvasToneMapping")]
public record GPUCanvasToneMapping(
    [property: Description("@#mode")]GPUCanvasToneMappingMode Mode = GPUCanvasToneMappingMode.Standard);

/// <summary>
/// WebIDL dictionary GPUColorDict。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucolordict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </remarks>
/// <param name="R">GPUColorDict 字典中的 r 成员，WebIDL 类型为 double。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-r">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="G">GPUColorDict 字典中的 g 成员，WebIDL 类型为 double。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-g">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="B">GPUColorDict 字典中的 b 成员，WebIDL 类型为 double。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-b">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="A">GPUColorDict 字典中的 a 成员，WebIDL 类型为 double。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolordict-a">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUColorDict")]
public record GPUColorDict(
    [property: Description("@#r")]double R = default,
    [property: Description("@#g")]double G = default,
    [property: Description("@#b")]double B = default,
    [property: Description("@#a")]double A = default);

/// <summary>
/// WebIDL dictionary GPUColorTargetState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucolortargetstate">WebGPU: 10.3.5 Color Target State</see>
/// </remarks>
/// <param name="Format">GPUColorTargetState 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-format">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="Blend">GPUColorTargetState 字典中的 blend 成员，WebIDL 类型为 GPUBlendState。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-blend">WebGPU: 10.3.5 Color Target State</see></param>
/// <param name="WriteMask">GPUColorTargetState 字典中的 writeMask 成员，WebIDL 类型为 GPUColorWriteFlags。可省略。WebIDL 默认值：0xF。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolortargetstate-writemask">WebGPU: 10.3.5 Color Target State</see></param>
[ECMAScript]
[Description("@#GPUColorTargetState")]
public record GPUColorTargetState(
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#blend")]GPUBlendState? Blend = default,
    [property: Description("@#writeMask")]GPUColorWriteFlags? WriteMask = default);

/// <summary>
/// WebIDL dictionary GPUCommandBufferDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucommandbufferdescriptor">WebGPU: 12.1.1 Command Buffer Creation</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCommandBufferDescriptor")]
public abstract record GPUCommandBufferDescriptor();

/// <summary>
/// WebIDL dictionary GPUCommandEncoderDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucommandencoderdescriptor">WebGPU: 13.2.1 Command Encoder Creation</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUCommandEncoderDescriptor")]
public abstract record GPUCommandEncoderDescriptor();

/// <summary>
/// WebIDL dictionary GPUComputePassDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepassdescriptor">WebGPU: 16.1.1 Compute Pass Encoder Creation</see>
/// </remarks>
/// <param name="TimestampWrites">GPUComputePassDescriptor 字典中的 timestampWrites 成员，WebIDL 类型为 GPUComputePassTimestampWrites。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepassdescriptor-timestampwrites">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePassDescriptor")]
public record GPUComputePassDescriptor(
    [property: Description("@#timestampWrites")]GPUComputePassTimestampWrites? TimestampWrites = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUComputePassTimestampWrites。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepasstimestampwrites">WebGPU: 16.1.1 Compute Pass Encoder Creation</see>
/// </remarks>
/// <param name="QuerySet">GPUComputePassTimestampWrites 字典中的 querySet 成员，WebIDL 类型为 GPUQuerySet。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-queryset">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
/// <param name="BeginningOfPassWriteIndex">GPUComputePassTimestampWrites 字典中的 beginningOfPassWriteIndex 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-beginningofpasswriteindex">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
/// <param name="EndOfPassWriteIndex">GPUComputePassTimestampWrites 字典中的 endOfPassWriteIndex 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepasstimestampwrites-endofpasswriteindex">WebGPU: 16.1.1 Compute Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePassTimestampWrites")]
public record GPUComputePassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// WebIDL dictionary GPUComputePipelineDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpucomputepipelinedescriptor">WebGPU: 10.2.1 Compute Pipeline Creation</see>
/// </remarks>
/// <param name="Compute">GPUComputePipelineDescriptor 字典中的 compute 成员，WebIDL 类型为 GPUProgrammableStage。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucomputepipelinedescriptor-compute">WebGPU: 10.2.1 Compute Pipeline Creation</see></param>
[ECMAScript]
[Description("@#GPUComputePipelineDescriptor")]
public record GPUComputePipelineDescriptor(
    [property: Description("@#compute")]GPUProgrammableStage? Compute = default) : GPUPipelineDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUDepthStencilState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpudepthstencilstate">WebGPU: 10.3.6 Depth/Stencil State</see>
/// </remarks>
/// <param name="Format">GPUDepthStencilState 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-format">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthWriteEnabled">GPUDepthStencilState 字典中的 depthWriteEnabled 成员，WebIDL 类型为 boolean。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthwriteenabled">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthCompare">GPUDepthStencilState 字典中的 depthCompare 成员，WebIDL 类型为 GPUCompareFunction。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthcompare">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilFront">GPUDepthStencilState 字典中的 stencilFront 成员，WebIDL 类型为 GPUStencilFaceState。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilfront">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilBack">GPUDepthStencilState 字典中的 stencilBack 成员，WebIDL 类型为 GPUStencilFaceState。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilback">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilReadMask">GPUDepthStencilState 字典中的 stencilReadMask 成员，WebIDL 类型为 GPUStencilValue。可省略。WebIDL 默认值：0xFFFFFFFF。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilreadmask">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="StencilWriteMask">GPUDepthStencilState 字典中的 stencilWriteMask 成员，WebIDL 类型为 GPUStencilValue。可省略。WebIDL 默认值：0xFFFFFFFF。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-stencilwritemask">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBias">GPUDepthStencilState 字典中的 depthBias 成员，WebIDL 类型为 GPUDepthBias。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbias">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBiasSlopeScale">GPUDepthStencilState 字典中的 depthBiasSlopeScale 成员，WebIDL 类型为 float。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbiasslopescale">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthBiasClamp">GPUDepthStencilState 字典中的 depthBiasClamp 成员，WebIDL 类型为 float。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudepthstencilstate-depthbiasclamp">WebGPU: 10.3.6 Depth/Stencil State</see></param>
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
/// WebIDL dictionary GPUExtent3DDict。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuextent3ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </remarks>
/// <param name="Width">GPUExtent3DDict 字典中的 width 成员，WebIDL 类型为 GPUIntegerCoordinate。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-width">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Height">GPUExtent3DDict 字典中的 height 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-height">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="DepthOrArrayLayers">GPUExtent3DDict 字典中的 depthOrArrayLayers 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuextent3ddict-depthorarraylayers">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUExtent3DDict")]
public record GPUExtent3DDict(
    [property: Description("@#width")]GPUIntegerCoordinate? Width = default,
    [property: Description("@#height")]GPUIntegerCoordinate? Height = default,
    [property: Description("@#depthOrArrayLayers")]GPUIntegerCoordinate? DepthOrArrayLayers = default);

/// <summary>
/// WebIDL dictionary GPUExternalTextureBindingLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuexternaltexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUExternalTextureBindingLayout")]
public abstract record GPUExternalTextureBindingLayout();

/// <summary>
/// WebIDL dictionary GPUExternalTextureDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuexternaltexturedescriptor">WebGPU: 6.4.1 Importing External Textures</see>
/// </remarks>
/// <param name="Source">GPUExternalTextureDescriptor 字典中的 source 成员，WebIDL 类型为 HTMLVideoElement, VideoFrame。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuexternaltexturedescriptor-source">WebGPU: 6.4.1 Importing External Textures</see></param>
/// <param name="ColorSpace">GPUExternalTextureDescriptor 字典中的 colorSpace 成员，WebIDL 类型为 PredefinedColorSpace。可省略。WebIDL 默认值：srgb。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuexternaltexturedescriptor-colorspace">WebGPU: 6.4.1 Importing External Textures</see></param>
[ECMAScript]
[Description("@#GPUExternalTextureDescriptor")]
public record GPUExternalTextureDescriptor(
    [property: Description("@#source")]GPUExternalTextureDescriptorSource? Source = default,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUFragmentState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpufragmentstate">WebGPU: 10.3.4 Fragment State</see>
/// </remarks>
/// <param name="Targets">GPUFragmentState 字典中的 targets 成员，WebIDL 类型为 sequence&lt;GPUColorTargetState&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpufragmentstate-targets">WebGPU: 10.3.4 Fragment State</see></param>
[ECMAScript]
[Description("@#GPUFragmentState")]
public record GPUFragmentState(
    [property: Description("@#targets")]GPUColorTargetState?[]? Targets = default) : GPUProgrammableStage;

/// <summary>
/// WebIDL dictionary GPUMultisampleState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpumultisamplestate">WebGPU: 10.3.3 Multisample State</see>
/// </remarks>
/// <param name="Count">GPUMultisampleState 字典中的 count 成员，WebIDL 类型为 GPUSize32。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-count">WebGPU: 10.3.3 Multisample State</see></param>
/// <param name="Mask">GPUMultisampleState 字典中的 mask 成员，WebIDL 类型为 GPUSampleMask。可省略。WebIDL 默认值：0xFFFFFFFF。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-mask">WebGPU: 10.3.3 Multisample State</see></param>
/// <param name="AlphaToCoverageEnabled">GPUMultisampleState 字典中的 alphaToCoverageEnabled 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpumultisamplestate-alphatocoverageenabled">WebGPU: 10.3.3 Multisample State</see></param>
[ECMAScript]
[Description("@#GPUMultisampleState")]
public record GPUMultisampleState(
    [property: Description("@#count")]GPUSize32? Count = default,
    [property: Description("@#mask")]GPUSampleMask? Mask = default,
    [property: Description("@#alphaToCoverageEnabled")]bool AlphaToCoverageEnabled = false);

/// <summary>
/// WebIDL dictionary GPUObjectDescriptorBase。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuobjectdescriptorbase">WebGPU: 3.1.3 Object Descriptors</see>
/// </remarks>
/// <param name="Label">GPUObjectDescriptorBase 字典中的 label 成员，WebIDL 类型为 USVString。可省略。WebIDL 默认值：。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuobjectdescriptorbase-label">WebGPU: 3.1.3 Object Descriptors</see></param>
[ECMAScript]
[Description("@#GPUObjectDescriptorBase")]
public record GPUObjectDescriptorBase(
    [property: Description("@#label")]string? Label = default);

/// <summary>
/// WebIDL dictionary GPUOrigin2DDict。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuorigin2ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </remarks>
/// <param name="X">GPUOrigin2DDict 字典中的 x 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin2ddict-x">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Y">GPUOrigin2DDict 字典中的 y 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin2ddict-y">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUOrigin2DDict")]
public record GPUOrigin2DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default);

/// <summary>
/// WebIDL dictionary GPUOrigin3DDict。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuorigin3ddict">WebGPU: 24.1 Colors &amp; Vectors</see>
/// </remarks>
/// <param name="X">GPUOrigin3DDict 字典中的 x 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-x">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Y">GPUOrigin3DDict 字典中的 y 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-y">WebGPU: 24.1 Colors &amp; Vectors</see></param>
/// <param name="Z">GPUOrigin3DDict 字典中的 z 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuorigin3ddict-z">WebGPU: 24.1 Colors &amp; Vectors</see></param>
[ECMAScript]
[Description("@#GPUOrigin3DDict")]
public record GPUOrigin3DDict(
    [property: Description("@#x")]GPUIntegerCoordinate? X = default,
    [property: Description("@#y")]GPUIntegerCoordinate? Y = default,
    [property: Description("@#z")]GPUIntegerCoordinate? Z = default);

/// <summary>
/// WebIDL dictionary GPUPipelineDescriptorBase。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelinedescriptorbase">WebGPU: 10.1 Base pipelines</see>
/// </remarks>
/// <param name="Layout">GPUPipelineDescriptorBase 字典中的 layout 成员，WebIDL 类型为 GPUPipelineLayout, GPUAutoLayoutMode。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinedescriptorbase-layout">WebGPU: 10.1 Base pipelines</see></param>
[ECMAScript]
[Description("@#GPUPipelineDescriptorBase")]
public record GPUPipelineDescriptorBase(
    [property: Description("@#layout")]GPUPipelineDescriptorBaseLayout? Layout = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUPipelineErrorInit。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelineerrorinit">WebGPU: 10. Pipelines</see>
/// </remarks>
/// <param name="Reason">GPUPipelineErrorInit 字典中的 reason 成员，WebIDL 类型为 GPUPipelineErrorReason。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelineerrorinit-reason">WebGPU: 10. Pipelines</see></param>
[ECMAScript]
[Description("@#GPUPipelineErrorInit")]
public record GPUPipelineErrorInit(
    [property: Description("@#reason")]GPUPipelineErrorReason? Reason = default);

/// <summary>
/// WebIDL dictionary GPUPipelineLayoutDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpupipelinelayoutdescriptor">WebGPU: 8.3.1 Pipeline Layout Creation</see>
/// </remarks>
/// <param name="BindGroupLayouts">GPUPipelineLayoutDescriptor 字典中的 bindGroupLayouts 成员，WebIDL 类型为 sequence&lt;GPUBindGroupLayout&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinelayoutdescriptor-bindgrouplayouts">WebGPU: 8.3.1 Pipeline Layout Creation</see></param>
/// <param name="ImmediateSize">GPUPipelineLayoutDescriptor 字典中的 immediateSize 成员，WebIDL 类型为 GPUSize32。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpupipelinelayoutdescriptor-immediatesize">WebGPU: 8.3.1 Pipeline Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUPipelineLayoutDescriptor")]
public record GPUPipelineLayoutDescriptor(
    [property: Description("@#bindGroupLayouts")]GPUBindGroupLayout?[]? BindGroupLayouts = default,
    [property: Description("@#immediateSize")]GPUSize32? ImmediateSize = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUPrimitiveState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuprimitivestate">WebGPU: 10.3.2 Primitive State</see>
/// </remarks>
/// <param name="Topology">GPUPrimitiveState 字典中的 topology 成员，WebIDL 类型为 GPUPrimitiveTopology。可省略。WebIDL 默认值：triangle-list。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-topology">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="StripIndexFormat">GPUPrimitiveState 字典中的 stripIndexFormat 成员，WebIDL 类型为 GPUIndexFormat。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-stripindexformat">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="FrontFace">GPUPrimitiveState 字典中的 frontFace 成员，WebIDL 类型为 GPUFrontFace。可省略。WebIDL 默认值：ccw。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-frontface">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="CullMode">GPUPrimitiveState 字典中的 cullMode 成员，WebIDL 类型为 GPUCullMode。可省略。WebIDL 默认值：none。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-cullmode">WebGPU: 10.3.2 Primitive State</see></param>
/// <param name="UnclippedDepth">GPUPrimitiveState 字典中的 unclippedDepth 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprimitivestate-unclippeddepth">WebGPU: 10.3.2 Primitive State</see></param>
[ECMAScript]
[Description("@#GPUPrimitiveState")]
public record GPUPrimitiveState(
    [property: Description("@#topology")]GPUPrimitiveTopology Topology = GPUPrimitiveTopology.TriangleList,
    [property: Description("@#stripIndexFormat")]GPUIndexFormat? StripIndexFormat = default,
    [property: Description("@#frontFace")]GPUFrontFace FrontFace = GPUFrontFace.Ccw,
    [property: Description("@#cullMode")]GPUCullMode CullMode = GPUCullMode.None,
    [property: Description("@#unclippedDepth")]bool UnclippedDepth = false);

/// <summary>
/// WebIDL dictionary GPUQuerySetDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuquerysetdescriptor">WebGPU: 20.1.1 QuerySet Creation</see>
/// </remarks>
/// <param name="Type">GPUQuerySetDescriptor 字典中的 type 成员，WebIDL 类型为 GPUQueryType。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerysetdescriptor-type">WebGPU: 20.1.1 QuerySet Creation</see></param>
/// <param name="Count">GPUQuerySetDescriptor 字典中的 count 成员，WebIDL 类型为 GPUSize32。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuquerysetdescriptor-count">WebGPU: 20.1.1 QuerySet Creation</see></param>
[ECMAScript]
[Description("@#GPUQuerySetDescriptor")]
public record GPUQuerySetDescriptor(
    [property: Description("@#type")]GPUQueryType? Type = default,
    [property: Description("@#count")]GPUSize32? Count = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPURenderBundleDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderbundledescriptor">WebGPU: 18.1.1 Render Bundle Creation</see>
/// </remarks>
[ECMAScript]
[Description("@#GPURenderBundleDescriptor")]
public abstract record GPURenderBundleDescriptor();

/// <summary>
/// WebIDL dictionary GPURenderBundleEncoderDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderbundleencoderdescriptor">WebGPU: 18.1.2 Encoding</see>
/// </remarks>
/// <param name="DepthReadOnly">GPURenderBundleEncoderDescriptor 字典中的 depthReadOnly 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoderdescriptor-depthreadonly">WebGPU: 18.1.2 Encoding</see></param>
/// <param name="StencilReadOnly">GPURenderBundleEncoderDescriptor 字典中的 stencilReadOnly 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderbundleencoderdescriptor-stencilreadonly">WebGPU: 18.1.2 Encoding</see></param>
[ECMAScript]
[Description("@#GPURenderBundleEncoderDescriptor")]
public record GPURenderBundleEncoderDescriptor(
    [property: Description("@#depthReadOnly")]bool DepthReadOnly = false,
    [property: Description("@#stencilReadOnly")]bool StencilReadOnly = false) : GPURenderPassLayout;

/// <summary>
/// WebIDL dictionary GPURenderPassColorAttachment。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasscolorattachment">WebGPU: 17.1.1.1 Color Attachments</see>
/// </remarks>
/// <param name="View">GPURenderPassColorAttachment 字典中的 view 成员，WebIDL 类型为 GPUTexture, GPUTextureView。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-view">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="DepthSlice">GPURenderPassColorAttachment 字典中的 depthSlice 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-depthslice">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="ResolveTarget">GPURenderPassColorAttachment 字典中的 resolveTarget 成员，WebIDL 类型为 GPUTexture, GPUTextureView。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-resolvetarget">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="ClearValue">GPURenderPassColorAttachment 字典中的 clearValue 成员，WebIDL 类型为 GPUColor。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-clearvalue">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="LoadOp">GPURenderPassColorAttachment 字典中的 loadOp 成员，WebIDL 类型为 GPULoadOp。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-loadop">WebGPU: 17.1.1.1 Color Attachments</see></param>
/// <param name="StoreOp">GPURenderPassColorAttachment 字典中的 storeOp 成员，WebIDL 类型为 GPUStoreOp。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasscolorattachment-storeop">WebGPU: 17.1.1.1 Color Attachments</see></param>
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
/// WebIDL dictionary GPURenderPassDepthStencilAttachment。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpassdepthstencilattachment">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see>
/// </remarks>
/// <param name="View">GPURenderPassDepthStencilAttachment 字典中的 view 成员，WebIDL 类型为 GPUTexture, GPUTextureView。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-view">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthClearValue">GPURenderPassDepthStencilAttachment 字典中的 depthClearValue 成员，WebIDL 类型为 float。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthclearvalue">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthLoadOp">GPURenderPassDepthStencilAttachment 字典中的 depthLoadOp 成员，WebIDL 类型为 GPULoadOp。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthloadop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthStoreOp">GPURenderPassDepthStencilAttachment 字典中的 depthStoreOp 成员，WebIDL 类型为 GPUStoreOp。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthstoreop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="DepthReadOnly">GPURenderPassDepthStencilAttachment 字典中的 depthReadOnly 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-depthreadonly">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilClearValue">GPURenderPassDepthStencilAttachment 字典中的 stencilClearValue 成员，WebIDL 类型为 GPUStencilValue。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilclearvalue">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilLoadOp">GPURenderPassDepthStencilAttachment 字典中的 stencilLoadOp 成员，WebIDL 类型为 GPULoadOp。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilloadop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilStoreOp">GPURenderPassDepthStencilAttachment 字典中的 stencilStoreOp 成员，WebIDL 类型为 GPUStoreOp。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilstoreop">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
/// <param name="StencilReadOnly">GPURenderPassDepthStencilAttachment 字典中的 stencilReadOnly 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdepthstencilattachment-stencilreadonly">WebGPU: 17.1.1.2 Depth/Stencil Attachments</see></param>
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
/// WebIDL dictionary GPURenderPassDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpassdescriptor">WebGPU: 17.1.1 Render Pass Encoder Creation</see>
/// </remarks>
/// <param name="ColorAttachments">GPURenderPassDescriptor 字典中的 colorAttachments 成员，WebIDL 类型为 sequence&lt;GPURenderPassColorAttachment&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-colorattachments">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="DepthStencilAttachment">GPURenderPassDescriptor 字典中的 depthStencilAttachment 成员，WebIDL 类型为 GPURenderPassDepthStencilAttachment。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-depthstencilattachment">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="OcclusionQuerySet">GPURenderPassDescriptor 字典中的 occlusionQuerySet 成员，WebIDL 类型为 GPUQuerySet。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-occlusionqueryset">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="TimestampWrites">GPURenderPassDescriptor 字典中的 timestampWrites 成员，WebIDL 类型为 GPURenderPassTimestampWrites。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-timestampwrites">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="MaxDrawCount">GPURenderPassDescriptor 字典中的 maxDrawCount 成员，WebIDL 类型为 GPUSize64。可省略。WebIDL 默认值：50000000。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpassdescriptor-maxdrawcount">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPassDescriptor")]
public record GPURenderPassDescriptor(
    [property: Description("@#colorAttachments")]GPURenderPassColorAttachment?[]? ColorAttachments = default,
    [property: Description("@#depthStencilAttachment")]GPURenderPassDepthStencilAttachment? DepthStencilAttachment = default,
    [property: Description("@#occlusionQuerySet")]GPUQuerySet? OcclusionQuerySet = default,
    [property: Description("@#timestampWrites")]GPURenderPassTimestampWrites? TimestampWrites = default,
    [property: Description("@#maxDrawCount")]GPUSize64? MaxDrawCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPURenderPassLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasslayout">WebGPU: 17.1.1.4 Render Pass Layout</see>
/// </remarks>
/// <param name="ColorFormats">GPURenderPassLayout 字典中的 colorFormats 成员，WebIDL 类型为 sequence&lt;GPUTextureFormat&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-colorformats">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
/// <param name="DepthStencilFormat">GPURenderPassLayout 字典中的 depthStencilFormat 成员，WebIDL 类型为 GPUTextureFormat。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-depthstencilformat">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
/// <param name="SampleCount">GPURenderPassLayout 字典中的 sampleCount 成员，WebIDL 类型为 GPUSize32。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasslayout-samplecount">WebGPU: 17.1.1.4 Render Pass Layout</see></param>
[ECMAScript]
[Description("@#GPURenderPassLayout")]
public record GPURenderPassLayout(
    [property: Description("@#colorFormats")]GPUTextureFormat?[]? ColorFormats = default,
    [property: Description("@#depthStencilFormat")]GPUTextureFormat? DepthStencilFormat = default,
    [property: Description("@#sampleCount")]GPUSize32? SampleCount = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPURenderPassTimestampWrites。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpasstimestampwrites">WebGPU: 17.1.1 Render Pass Encoder Creation</see>
/// </remarks>
/// <param name="QuerySet">GPURenderPassTimestampWrites 字典中的 querySet 成员，WebIDL 类型为 GPUQuerySet。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-queryset">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="BeginningOfPassWriteIndex">GPURenderPassTimestampWrites 字典中的 beginningOfPassWriteIndex 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-beginningofpasswriteindex">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
/// <param name="EndOfPassWriteIndex">GPURenderPassTimestampWrites 字典中的 endOfPassWriteIndex 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpasstimestampwrites-endofpasswriteindex">WebGPU: 17.1.1 Render Pass Encoder Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPassTimestampWrites")]
public record GPURenderPassTimestampWrites(
    [property: Description("@#querySet")]GPUQuerySet? QuerySet = default,
    [property: Description("@#beginningOfPassWriteIndex")]GPUSize32? BeginningOfPassWriteIndex = default,
    [property: Description("@#endOfPassWriteIndex")]GPUSize32? EndOfPassWriteIndex = default);

/// <summary>
/// WebIDL dictionary GPURenderPipelineDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurenderpipelinedescriptor">WebGPU: 10.3.1 Render Pipeline Creation</see>
/// </remarks>
/// <param name="Vertex">GPURenderPipelineDescriptor 字典中的 vertex 成员，WebIDL 类型为 GPUVertexState。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-vertex">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Primitive">GPURenderPipelineDescriptor 字典中的 primitive 成员，WebIDL 类型为 GPUPrimitiveState。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-primitive">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="DepthStencil">GPURenderPipelineDescriptor 字典中的 depthStencil 成员，WebIDL 类型为 GPUDepthStencilState。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-depthstencil">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Multisample">GPURenderPipelineDescriptor 字典中的 multisample 成员，WebIDL 类型为 GPUMultisampleState。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-multisample">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
/// <param name="Fragment">GPURenderPipelineDescriptor 字典中的 fragment 成员，WebIDL 类型为 GPUFragmentState。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurenderpipelinedescriptor-fragment">WebGPU: 10.3.1 Render Pipeline Creation</see></param>
[ECMAScript]
[Description("@#GPURenderPipelineDescriptor")]
public record GPURenderPipelineDescriptor(
    [property: Description("@#vertex")]GPUVertexState? Vertex = default,
    [property: Description("@#primitive")]GPUPrimitiveState? Primitive = default,
    [property: Description("@#depthStencil")]GPUDepthStencilState? DepthStencil = default,
    [property: Description("@#multisample")]GPUMultisampleState? Multisample = default,
    [property: Description("@#fragment")]GPUFragmentState? Fragment = default) : GPUPipelineDescriptorBase;

/// <summary>
/// WebIDL dictionary GPURequestAdapterOptions。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpurequestadapteroptions">WebGPU: 4.2.2 Adapter Selection</see>
/// </remarks>
/// <param name="FeatureLevel">GPURequestAdapterOptions 字典中的 featureLevel 成员，WebIDL 类型为 DOMString。可省略。WebIDL 默认值：core。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-featurelevel">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="PowerPreference">GPURequestAdapterOptions 字典中的 powerPreference 成员，WebIDL 类型为 GPUPowerPreference。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-powerpreference">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="ForceFallbackAdapter">GPURequestAdapterOptions 字典中的 forceFallbackAdapter 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-forcefallbackadapter">WebGPU: 4.2.2 Adapter Selection</see></param>
/// <param name="XrCompatible">GPURequestAdapterOptions 字典中的 xrCompatible 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpurequestadapteroptions-xrcompatible">WebGPU: 4.2.2 Adapter Selection</see></param>
[ECMAScript]
[Description("@#GPURequestAdapterOptions")]
public record GPURequestAdapterOptions(
    [property: Description("@#featureLevel")]string? FeatureLevel = default,
    [property: Description("@#powerPreference")]GPUPowerPreference? PowerPreference = default,
    [property: Description("@#forceFallbackAdapter")]bool ForceFallbackAdapter = false,
    [property: Description("@#xrCompatible")]bool XrCompatible = false);

/// <summary>
/// WebIDL dictionary GPUSamplerBindingLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpusamplerbindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="Type">GPUSamplerBindingLayout 字典中的 type 成员，WebIDL 类型为 GPUSamplerBindingType。可省略。WebIDL 默认值：filtering。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerbindinglayout-type">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUSamplerBindingLayout")]
public record GPUSamplerBindingLayout(
    [property: Description("@#type")]GPUSamplerBindingType Type = GPUSamplerBindingType.Filtering);

/// <summary>
/// WebIDL dictionary GPUSamplerDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpusamplerdescriptor">WebGPU: 7.1.1 GPUSamplerDescriptor</see>
/// </remarks>
/// <param name="AddressModeU">GPUSamplerDescriptor 字典中的 addressModeU 成员，WebIDL 类型为 GPUAddressMode。可省略。WebIDL 默认值：clamp-to-edge。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodeu">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="AddressModeV">GPUSamplerDescriptor 字典中的 addressModeV 成员，WebIDL 类型为 GPUAddressMode。可省略。WebIDL 默认值：clamp-to-edge。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodev">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="AddressModeW">GPUSamplerDescriptor 字典中的 addressModeW 成员，WebIDL 类型为 GPUAddressMode。可省略。WebIDL 默认值：clamp-to-edge。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-addressmodew">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MagFilter">GPUSamplerDescriptor 字典中的 magFilter 成员，WebIDL 类型为 GPUFilterMode。可省略。WebIDL 默认值：nearest。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-magfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MinFilter">GPUSamplerDescriptor 字典中的 minFilter 成员，WebIDL 类型为 GPUFilterMode。可省略。WebIDL 默认值：nearest。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-minfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MipmapFilter">GPUSamplerDescriptor 字典中的 mipmapFilter 成员，WebIDL 类型为 GPUMipmapFilterMode。可省略。WebIDL 默认值：nearest。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-mipmapfilter">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="LodMinClamp">GPUSamplerDescriptor 字典中的 lodMinClamp 成员，WebIDL 类型为 float。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-lodminclamp">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="LodMaxClamp">GPUSamplerDescriptor 字典中的 lodMaxClamp 成员，WebIDL 类型为 float。可省略。WebIDL 默认值：32。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-lodmaxclamp">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="Compare">GPUSamplerDescriptor 字典中的 compare 成员，WebIDL 类型为 GPUCompareFunction。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-compare">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
/// <param name="MaxAnisotropy">GPUSamplerDescriptor 字典中的 maxAnisotropy 成员，WebIDL 类型为 unsigned short。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpusamplerdescriptor-maxanisotropy">WebGPU: 7.1.1 GPUSamplerDescriptor</see></param>
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
/// WebIDL dictionary GPUShaderModuleCompilationHint。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpushadermodulecompilationhint">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see>
/// </remarks>
/// <param name="EntryPoint">GPUShaderModuleCompilationHint 字典中的 entryPoint 成员，WebIDL 类型为 USVString。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermodulecompilationhint-entrypoint">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see></param>
/// <param name="Layout">GPUShaderModuleCompilationHint 字典中的 layout 成员，WebIDL 类型为 GPUPipelineLayout, GPUAutoLayoutMode。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermodulecompilationhint-layout">WebGPU: 9.1.1.1 Shader Module Compilation Hints</see></param>
[ECMAScript]
[Description("@#GPUShaderModuleCompilationHint")]
public record GPUShaderModuleCompilationHint(
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#layout")]GPUShaderModuleCompilationHintLayout? Layout = default);

/// <summary>
/// WebIDL dictionary GPUShaderModuleDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpushadermoduledescriptor">WebGPU: 9.1.1 Shader Module Creation</see>
/// </remarks>
/// <param name="Code">GPUShaderModuleDescriptor 字典中的 code 成员，WebIDL 类型为 USVString。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermoduledescriptor-code">WebGPU: 9.1.1 Shader Module Creation</see></param>
/// <param name="CompilationHints">GPUShaderModuleDescriptor 字典中的 compilationHints 成员，WebIDL 类型为 sequence&lt;GPUShaderModuleCompilationHint&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpushadermoduledescriptor-compilationhints">WebGPU: 9.1.1 Shader Module Creation</see></param>
[ECMAScript]
[Description("@#GPUShaderModuleDescriptor")]
public record GPUShaderModuleDescriptor(
    [property: Description("@#code")]string? Code = default,
    [property: Description("@#compilationHints")]GPUShaderModuleCompilationHint[]? CompilationHints = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUStencilFaceState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpustencilfacestate">WebGPU: 10.3.6 Depth/Stencil State</see>
/// </remarks>
/// <param name="Compare">GPUStencilFaceState 字典中的 compare 成员，WebIDL 类型为 GPUCompareFunction。可省略。WebIDL 默认值：always。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-compare">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="FailOp">GPUStencilFaceState 字典中的 failOp 成员，WebIDL 类型为 GPUStencilOperation。可省略。WebIDL 默认值：keep。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-failop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="DepthFailOp">GPUStencilFaceState 字典中的 depthFailOp 成员，WebIDL 类型为 GPUStencilOperation。可省略。WebIDL 默认值：keep。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-depthfailop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
/// <param name="PassOp">GPUStencilFaceState 字典中的 passOp 成员，WebIDL 类型为 GPUStencilOperation。可省略。WebIDL 默认值：keep。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustencilfacestate-passop">WebGPU: 10.3.6 Depth/Stencil State</see></param>
[ECMAScript]
[Description("@#GPUStencilFaceState")]
public record GPUStencilFaceState(
    [property: Description("@#compare")]GPUCompareFunction Compare = GPUCompareFunction.Always,
    [property: Description("@#failOp")]GPUStencilOperation FailOp = GPUStencilOperation.Keep,
    [property: Description("@#depthFailOp")]GPUStencilOperation DepthFailOp = GPUStencilOperation.Keep,
    [property: Description("@#passOp")]GPUStencilOperation PassOp = GPUStencilOperation.Keep);

/// <summary>
/// WebIDL dictionary GPUStorageTextureBindingLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpustoragetexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="Access">GPUStorageTextureBindingLayout 字典中的 access 成员，WebIDL 类型为 GPUStorageTextureAccess。可省略。WebIDL 默认值：write-only。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-access">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Format">GPUStorageTextureBindingLayout 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-format">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ViewDimension">GPUStorageTextureBindingLayout 字典中的 viewDimension 成员，WebIDL 类型为 GPUTextureViewDimension。可省略。WebIDL 默认值：2d。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpustoragetexturebindinglayout-viewdimension">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUStorageTextureBindingLayout")]
public record GPUStorageTextureBindingLayout(
    [property: Description("@#access")]GPUStorageTextureAccess Access = GPUStorageTextureAccess.WriteOnly,
    [property: Description("@#format")]GPUTextureFormat? Format = default,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d);

/// <summary>
/// WebIDL dictionary GPUTextureBindingLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gputexturebindinglayout">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
/// <param name="SampleType">GPUTextureBindingLayout 字典中的 sampleType 成员，WebIDL 类型为 GPUTextureSampleType。可省略。WebIDL 默认值：float。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-sampletype">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="ViewDimension">GPUTextureBindingLayout 字典中的 viewDimension 成员，WebIDL 类型为 GPUTextureViewDimension。可省略。WebIDL 默认值：2d。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-viewdimension">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
/// <param name="Multisampled">GPUTextureBindingLayout 字典中的 multisampled 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturebindinglayout-multisampled">WebGPU: 8.1.1 Bind Group Layout Creation</see></param>
[ECMAScript]
[Description("@#GPUTextureBindingLayout")]
public record GPUTextureBindingLayout(
    [property: Description("@#sampleType")]GPUTextureSampleType SampleType = GPUTextureSampleType.Float,
    [property: Description("@#viewDimension")]GPUTextureViewDimension ViewDimension = GPUTextureViewDimension._2d,
    [property: Description("@#multisampled")]bool Multisampled = false);

/// <summary>
/// WebIDL dictionary GPUTextureViewDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gputextureviewdescriptor">WebGPU: 6.2.1 Texture View Creation</see>
/// </remarks>
/// <param name="Format">GPUTextureViewDescriptor 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-format">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Dimension">GPUTextureViewDescriptor 字典中的 dimension 成员，WebIDL 类型为 GPUTextureViewDimension。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-dimension">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Usage">GPUTextureViewDescriptor 字典中的 usage 成员，WebIDL 类型为 GPUTextureUsageFlags。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-usage">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Aspect">GPUTextureViewDescriptor 字典中的 aspect 成员，WebIDL 类型为 GPUTextureAspect。可省略。WebIDL 默认值：all。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-aspect">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="BaseMipLevel">GPUTextureViewDescriptor 字典中的 baseMipLevel 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-basemiplevel">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="MipLevelCount">GPUTextureViewDescriptor 字典中的 mipLevelCount 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-miplevelcount">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="BaseArrayLayer">GPUTextureViewDescriptor 字典中的 baseArrayLayer 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-basearraylayer">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="ArrayLayerCount">GPUTextureViewDescriptor 字典中的 arrayLayerCount 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-arraylayercount">WebGPU: 6.2.1 Texture View Creation</see></param>
/// <param name="Swizzle">GPUTextureViewDescriptor 字典中的 swizzle 成员，WebIDL 类型为 DOMString。可省略。WebIDL 默认值：rgba。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureviewdescriptor-swizzle">WebGPU: 6.2.1 Texture View Creation</see></param>
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
/// WebIDL dictionary GPUUncapturedErrorEventInit。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuuncapturederroreventinit">WebGPU: 22.4 Telemetry</see>
/// </remarks>
/// <param name="Error">GPUUncapturedErrorEventInit 字典中的 error 成员，WebIDL 类型为 GPUError。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuuncapturederroreventinit-error">WebGPU: 22.4 Telemetry</see></param>
[ECMAScript]
[Description("@#GPUUncapturedErrorEventInit")]
public record GPUUncapturedErrorEventInit(
    [property: Description("@#error")]GPUError? Error = default) : EventInit;

/// <summary>
/// WebIDL dictionary GPUVertexAttribute。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexattribute">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </remarks>
/// <param name="Format">GPUVertexAttribute 字典中的 format 成员，WebIDL 类型为 GPUVertexFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-format">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="Offset">GPUVertexAttribute 字典中的 offset 成员，WebIDL 类型为 GPUSize64。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-offset">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="ShaderLocation">GPUVertexAttribute 字典中的 shaderLocation 成员，WebIDL 类型为 GPUIndex32。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexattribute-shaderlocation">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexAttribute")]
public record GPUVertexAttribute(
    [property: Description("@#format")]GPUVertexFormat? Format = default,
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#shaderLocation")]GPUIndex32? ShaderLocation = default);

/// <summary>
/// WebIDL dictionary GPUVertexBufferLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexbufferlayout">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </remarks>
/// <param name="ArrayStride">GPUVertexBufferLayout 字典中的 arrayStride 成员，WebIDL 类型为 GPUSize64。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-arraystride">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="StepMode">GPUVertexBufferLayout 字典中的 stepMode 成员，WebIDL 类型为 GPUVertexStepMode。可省略。WebIDL 默认值：vertex。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-stepmode">WebGPU: 10.3.7.1 Vertex Formats</see></param>
/// <param name="Attributes">GPUVertexBufferLayout 字典中的 attributes 成员，WebIDL 类型为 sequence&lt;GPUVertexAttribute&gt;。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexbufferlayout-attributes">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexBufferLayout")]
public record GPUVertexBufferLayout(
    [property: Description("@#arrayStride")]GPUSize64? ArrayStride = default,
    [property: Description("@#stepMode")]GPUVertexStepMode StepMode = GPUVertexStepMode.Vertex,
    [property: Description("@#attributes")]GPUVertexAttribute[]? Attributes = default);

/// <summary>
/// WebIDL dictionary GPUVertexState。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#dictdef-gpuvertexstate">WebGPU: 10.3.7.1 Vertex Formats</see>
/// </remarks>
/// <param name="Buffers">GPUVertexState 字典中的 buffers 成员，WebIDL 类型为 sequence&lt;GPUVertexBufferLayout&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuvertexstate-buffers">WebGPU: 10.3.7.1 Vertex Formats</see></param>
[ECMAScript]
[Description("@#GPUVertexState")]
public record GPUVertexState(
    [property: Description("@#buffers")]GPUVertexBufferLayout?[]? Buffers = default) : GPUProgrammableStage;

/// <summary>
/// WebIDL dictionary GPUBufferDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpubufferdescriptor">WebGPU: 5.1.1 GPUBufferDescriptor</see>
/// </remarks>
/// <param name="Size">GPUBufferDescriptor 字典中的 size 成员，WebIDL 类型为 GPUSize64。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-size">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
/// <param name="Usage">GPUBufferDescriptor 字典中的 usage 成员，WebIDL 类型为 GPUBufferUsageFlags。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-usage">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
/// <param name="MappedAtCreation">GPUBufferDescriptor 字典中的 mappedAtCreation 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferdescriptor-mappedatcreation">WebGPU: 5.1.1 GPUBufferDescriptor</see></param>
[ECMAScript]
[Description("@#GPUBufferDescriptor")]
public record GPUBufferDescriptor(
    [property: Description("@#size")]GPUSize64? Size = default,
    [property: Description("@#usage")]GPUBufferUsageFlags? Usage = default,
    [property: Description("@#mappedAtCreation")]bool MappedAtCreation = false) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUCopyExternalImageDestInfo。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucopyexternalimagedestinfo">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see>
/// </remarks>
/// <param name="ColorSpace">GPUCopyExternalImageDestInfo 字典中的 colorSpace 成员，WebIDL 类型为 PredefinedColorSpace。可省略。WebIDL 默认值：srgb。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagedestinfo-colorspace">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see></param>
/// <param name="PremultipliedAlpha">GPUCopyExternalImageDestInfo 字典中的 premultipliedAlpha 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagedestinfo-premultipliedalpha">WebGPU: 11.2.4 GPUCopyExternalImageDestInfo</see></param>
[ECMAScript]
[Description("@#GPUCopyExternalImageDestInfo")]
public record GPUCopyExternalImageDestInfo(
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#premultipliedAlpha")]bool PremultipliedAlpha = false) : GPUTexelCopyTextureInfo;

/// <summary>
/// WebIDL dictionary GPUCopyExternalImageSourceInfo。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpucopyexternalimagesourceinfo">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see>
/// </remarks>
/// <param name="Source">GPUCopyExternalImageSourceInfo 字典中的 source 成员，WebIDL 类型为 GPUCopyExternalImageSource。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-source">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
/// <param name="Origin">GPUCopyExternalImageSourceInfo 字典中的 origin 成员，WebIDL 类型为 GPUOrigin2D。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-origin">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
/// <param name="FlipY">GPUCopyExternalImageSourceInfo 字典中的 flipY 成员，WebIDL 类型为 boolean。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;boolean&quot;,&#10;                  &quot;value&quot;: false&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpucopyexternalimagesourceinfo-flipy">WebGPU: 11.2.5 GPUCopyExternalImageSourceInfo</see></param>
[ECMAScript]
[Description("@#GPUCopyExternalImageSourceInfo")]
public record GPUCopyExternalImageSourceInfo(
    [property: Description("@#source")]GPUCopyExternalImageSource? Source = default,
    [property: Description("@#origin")]GPUOrigin2D? Origin = default,
    [property: Description("@#flipY")]bool FlipY = false);

/// <summary>
/// WebIDL dictionary GPUDeviceDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpudevicedescriptor">WebGPU: 4.3.1 GPUDeviceDescriptor</see>
/// </remarks>
/// <param name="RequiredFeatures">GPUDeviceDescriptor 字典中的 requiredFeatures 成员，WebIDL 类型为 sequence&lt;GPUFeatureName&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-requiredfeatures">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
/// <param name="RequiredLimits">GPUDeviceDescriptor 字典中的 requiredLimits 成员，WebIDL 类型为 record&lt;DOMString, GPUSize64, undefined&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-requiredlimits">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
/// <param name="DefaultQueue">GPUDeviceDescriptor 字典中的 defaultQueue 成员，WebIDL 类型为 GPUQueueDescriptor。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpudevicedescriptor-defaultqueue">WebGPU: 4.3.1 GPUDeviceDescriptor</see></param>
[ECMAScript]
[Description("@#GPUDeviceDescriptor")]
public record GPUDeviceDescriptor(
    [property: Description("@#requiredFeatures")]GPUFeatureName[]? RequiredFeatures = default,
    [property: Description("@#requiredLimits")]Dictionary<string, GPUSize64?>? RequiredLimits = default,
    [property: Description("@#defaultQueue")]GPUQueueDescriptor? DefaultQueue = default) : GPUObjectDescriptorBase;

/// <summary>
/// WebIDL dictionary GPUProgrammableStage。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuprogrammablestage">WebGPU: 10.1.2 GPUProgrammableStage</see>
/// </remarks>
/// <param name="Module">GPUProgrammableStage 字典中的 module 成员，WebIDL 类型为 GPUShaderModule。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-module">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
/// <param name="EntryPoint">GPUProgrammableStage 字典中的 entryPoint 成员，WebIDL 类型为 USVString。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-entrypoint">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
/// <param name="Constants">GPUProgrammableStage 字典中的 constants 成员，WebIDL 类型为 record&lt;USVString, GPUPipelineConstantValue&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gpuprogrammablestage-constants">WebGPU: 10.1.2 GPUProgrammableStage</see></param>
[ECMAScript]
[Description("@#GPUProgrammableStage")]
public record GPUProgrammableStage(
    [property: Description("@#module")]GPUShaderModule? Module = default,
    [property: Description("@#entryPoint")]string? EntryPoint = default,
    [property: Description("@#constants")]Dictionary<string, GPUPipelineConstantValue>? Constants = default);

/// <summary>
/// WebIDL dictionary GPUQueueDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gpuqueuedescriptor">WebGPU: 19.1 GPUQueueDescriptor</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUQueueDescriptor")]
public abstract record GPUQueueDescriptor();

/// <summary>
/// WebIDL dictionary GPUTexelCopyBufferInfo。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopybufferinfo">WebGPU: 11.2.2 GPUTexelCopyBufferInfo</see>
/// </remarks>
/// <param name="Buffer">GPUTexelCopyBufferInfo 字典中的 buffer 成员，WebIDL 类型为 GPUBuffer。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferinfo-buffer">WebGPU: 11.2.2 GPUTexelCopyBufferInfo</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyBufferInfo")]
public record GPUTexelCopyBufferInfo(
    [property: Description("@#buffer")]GPUBuffer? Buffer = default) : GPUTexelCopyBufferLayout;

/// <summary>
/// WebIDL dictionary GPUTexelCopyBufferLayout。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopybufferlayout">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see>
/// </remarks>
/// <param name="Offset">GPUTexelCopyBufferLayout 字典中的 offset 成员，WebIDL 类型为 GPUSize64。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-offset">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
/// <param name="BytesPerRow">GPUTexelCopyBufferLayout 字典中的 bytesPerRow 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-bytesperrow">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
/// <param name="RowsPerImage">GPUTexelCopyBufferLayout 字典中的 rowsPerImage 成员，WebIDL 类型为 GPUSize32。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopybufferlayout-rowsperimage">WebGPU: 11.2.1 GPUTexelCopyBufferLayout</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyBufferLayout")]
public record GPUTexelCopyBufferLayout(
    [property: Description("@#offset")]GPUSize64? Offset = default,
    [property: Description("@#bytesPerRow")]GPUSize32? BytesPerRow = default,
    [property: Description("@#rowsPerImage")]GPUSize32? RowsPerImage = default);

/// <summary>
/// WebIDL dictionary GPUTexelCopyTextureInfo。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexelcopytextureinfo">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see>
/// </remarks>
/// <param name="Texture">GPUTexelCopyTextureInfo 字典中的 texture 成员，WebIDL 类型为 GPUTexture。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-texture">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="MipLevel">GPUTexelCopyTextureInfo 字典中的 mipLevel 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：0。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-miplevel">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="Origin">GPUTexelCopyTextureInfo 字典中的 origin 成员，WebIDL 类型为 GPUOrigin3D。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;dictionary&quot;&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-origin">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
/// <param name="Aspect">GPUTexelCopyTextureInfo 字典中的 aspect 成员，WebIDL 类型为 GPUTextureAspect。可省略。WebIDL 默认值：all。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexelcopytextureinfo-aspect">WebGPU: 11.2.3 GPUTexelCopyTextureInfo</see></param>
[ECMAScript]
[Description("@#GPUTexelCopyTextureInfo")]
public record GPUTexelCopyTextureInfo(
    [property: Description("@#texture")]GPUTexture? Texture = default,
    [property: Description("@#mipLevel")]GPUIntegerCoordinate? MipLevel = default,
    [property: Description("@#origin")]GPUOrigin3D? Origin = default,
    [property: Description("@#aspect")]GPUTextureAspect Aspect = GPUTextureAspect.All);

/// <summary>
/// WebIDL dictionary GPUTextureDescriptor。定义于 WebGPU。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#gputexturedescriptor">WebGPU: 6.1.1 GPUTextureDescriptor</see>
/// </remarks>
/// <param name="Size">GPUTextureDescriptor 字典中的 size 成员，WebIDL 类型为 GPUExtent3D。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-size">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="MipLevelCount">GPUTextureDescriptor 字典中的 mipLevelCount 成员，WebIDL 类型为 GPUIntegerCoordinate。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-miplevelcount">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="SampleCount">GPUTextureDescriptor 字典中的 sampleCount 成员，WebIDL 类型为 GPUSize32。可省略。WebIDL 默认值：1。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-samplecount">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Dimension">GPUTextureDescriptor 字典中的 dimension 成员，WebIDL 类型为 GPUTextureDimension。可省略。WebIDL 默认值：2d。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-dimension">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Format">GPUTextureDescriptor 字典中的 format 成员，WebIDL 类型为 GPUTextureFormat。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-format">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="Usage">GPUTextureDescriptor 字典中的 usage 成员，WebIDL 类型为 GPUTextureUsageFlags。必须提供该成员。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-usage">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="ViewFormats">GPUTextureDescriptor 字典中的 viewFormats 成员，WebIDL 类型为 sequence&lt;GPUTextureFormat&gt;。可省略。WebIDL 默认值：{&#10;                  &quot;type&quot;: &quot;sequence&quot;,&#10;                  &quot;value&quot;: []&#10;                }。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-viewformats">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
/// <param name="TextureBindingViewDimension">GPUTextureDescriptor 字典中的 textureBindingViewDimension 成员，WebIDL 类型为 GPUTextureViewDimension。可省略。 <see href="https://gpuweb.github.io/gpuweb/#dom-gputexturedescriptor-texturebindingviewdimension">WebGPU: 6.1.1 GPUTextureDescriptor</see></param>
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