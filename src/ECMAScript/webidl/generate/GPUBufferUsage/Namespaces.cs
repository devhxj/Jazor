namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpubufferusage">WebGPU: 5.1.2 Buffer Usages</see>
/// </summary>
[ECMAScript]
[Description("@#GPUBufferUsage")]
public static class GPUBufferUsage
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-map_read">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#MAP_READ")]
    public const GPUFlagsConstant MAP_READ = 0x0001;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-map_write">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#MAP_WRITE")]
    public const GPUFlagsConstant MAP_WRITE = 0x0002;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-copy_src">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x0004;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-copy_dst">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x0008;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-index">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#INDEX")]
    public const GPUFlagsConstant INDEX = 0x0010;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-vertex">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x0020;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-uniform">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#UNIFORM")]
    public const GPUFlagsConstant UNIFORM = 0x0040;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-storage">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#STORAGE")]
    public const GPUFlagsConstant STORAGE = 0x0080;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-indirect">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#INDIRECT")]
    public const GPUFlagsConstant INDIRECT = 0x0100;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </summary>
    [Description("@#QUERY_RESOLVE")]
    public const GPUFlagsConstant QUERY_RESOLVE = 0x0200;
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpucolorwrite">WebGPU: 10.3.5 Color Target State</see>
/// </summary>
[ECMAScript]
[Description("@#GPUColorWrite")]
public static class GPUColorWrite
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-red">WebGPU: 10.3.5 Color Target State</see>
    /// </summary>
    [Description("@#RED")]
    public const GPUFlagsConstant RED = 0x1;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-green">WebGPU: 10.3.5 Color Target State</see>
    /// </summary>
    [Description("@#GREEN")]
    public const GPUFlagsConstant GREEN = 0x2;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-blue">WebGPU: 10.3.5 Color Target State</see>
    /// </summary>
    [Description("@#BLUE")]
    public const GPUFlagsConstant BLUE = 0x4;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-alpha">WebGPU: 10.3.5 Color Target State</see>
    /// </summary>
    [Description("@#ALPHA")]
    public const GPUFlagsConstant ALPHA = 0x8;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </summary>
    [Description("@#ALL")]
    public const GPUFlagsConstant ALL = 0xF;
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpumapmode">WebGPU: 5.2 Buffer Mapping</see>
/// </summary>
[ECMAScript]
[Description("@#GPUMapMode")]
public static class GPUMapMode
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumapmode-read">WebGPU: 5.2 Buffer Mapping</see>
    /// </summary>
    [Description("@#READ")]
    public const GPUFlagsConstant READ = 0x0001;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumapmode-write">WebGPU: 5.2 Buffer Mapping</see>
    /// </summary>
    [Description("@#WRITE")]
    public const GPUFlagsConstant WRITE = 0x0002;
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpushaderstage">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </summary>
[ECMAScript]
[Description("@#GPUShaderStage")]
public static class GPUShaderStage
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-vertex">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x1;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-fragment">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#FRAGMENT")]
    public const GPUFlagsConstant FRAGMENT = 0x2;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-compute">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </summary>
    [Description("@#COMPUTE")]
    public const GPUFlagsConstant COMPUTE = 0x4;
}

/// <summary>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gputextureusage">WebGPU: 6.1.2 Texture Usages</see>
/// </summary>
[ECMAScript]
[Description("@#GPUTextureUsage")]
public static class GPUTextureUsage
{
    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-copy_src">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x01;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-copy_dst">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x02;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-texture_binding">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#TEXTURE_BINDING")]
    public const GPUFlagsConstant TEXTURE_BINDING = 0x04;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-storage_binding">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#STORAGE_BINDING")]
    public const GPUFlagsConstant STORAGE_BINDING = 0x08;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-render_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#RENDER_ATTACHMENT")]
    public const GPUFlagsConstant RENDER_ATTACHMENT = 0x10;

    /// <summary>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </summary>
    [Description("@#TRANSIENT_ATTACHMENT")]
    public const GPUFlagsConstant TRANSIENT_ATTACHMENT = 0x20;
}
