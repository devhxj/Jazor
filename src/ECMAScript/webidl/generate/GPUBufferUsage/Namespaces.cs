namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// WebIDL namespace GPUBufferUsage。定义于 WebGPU。该 API 仅暴露于安全上下文。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpubufferusage">WebGPU: 5.1.2 Buffer Usages</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUBufferUsage")]
public static class GPUBufferUsage
{
    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#MAP_READ")]
    public const GPUFlagsConstant MAP_READ = 0x0001;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#MAP_WRITE")]
    public const GPUFlagsConstant MAP_WRITE = 0x0002;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x0004;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x0008;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#INDEX")]
    public const GPUFlagsConstant INDEX = 0x0010;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x0020;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#UNIFORM")]
    public const GPUFlagsConstant UNIFORM = 0x0040;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#STORAGE")]
    public const GPUFlagsConstant STORAGE = 0x0080;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#INDIRECT")]
    public const GPUFlagsConstant INDIRECT = 0x0100;

    /// <summary>
    /// GPUBufferUsage 的规范常量 QUERY_RESOLVE，WebIDL 类型为 GPUFlagsConstant，值为 0x0200。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpubufferusage-query_resolve">WebGPU: 5.1.2 Buffer Usages</see>
    /// </remarks>
    [Description("@#QUERY_RESOLVE")]
    public const GPUFlagsConstant QUERY_RESOLVE = 0x0200;
}

/// <summary>
/// WebIDL namespace GPUColorWrite。定义于 WebGPU。该 API 仅暴露于安全上下文。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpucolorwrite">WebGPU: 10.3.5 Color Target State</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUColorWrite")]
public static class GPUColorWrite
{
    /// <summary>
    /// GPUColorWrite 的规范常量 ALL，WebIDL 类型为 GPUFlagsConstant，值为 0xF。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </remarks>
    [Description("@#RED")]
    public const GPUFlagsConstant RED = 0x1;

    /// <summary>
    /// GPUColorWrite 的规范常量 ALL，WebIDL 类型为 GPUFlagsConstant，值为 0xF。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </remarks>
    [Description("@#GREEN")]
    public const GPUFlagsConstant GREEN = 0x2;

    /// <summary>
    /// GPUColorWrite 的规范常量 ALL，WebIDL 类型为 GPUFlagsConstant，值为 0xF。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </remarks>
    [Description("@#BLUE")]
    public const GPUFlagsConstant BLUE = 0x4;

    /// <summary>
    /// GPUColorWrite 的规范常量 ALL，WebIDL 类型为 GPUFlagsConstant，值为 0xF。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </remarks>
    [Description("@#ALPHA")]
    public const GPUFlagsConstant ALPHA = 0x8;

    /// <summary>
    /// GPUColorWrite 的规范常量 ALL，WebIDL 类型为 GPUFlagsConstant，值为 0xF。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpucolorwrite-all">WebGPU: 10.3.5 Color Target State</see>
    /// </remarks>
    [Description("@#ALL")]
    public const GPUFlagsConstant ALL = 0xF;
}

/// <summary>
/// WebIDL namespace GPUMapMode。定义于 WebGPU。该 API 仅暴露于安全上下文。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpumapmode">WebGPU: 5.2 Buffer Mapping</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUMapMode")]
public static class GPUMapMode
{
    /// <summary>
    /// GPUMapMode 的规范常量 WRITE，WebIDL 类型为 GPUFlagsConstant，值为 0x0002。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumapmode-write">WebGPU: 5.2 Buffer Mapping</see>
    /// </remarks>
    [Description("@#READ")]
    public const GPUFlagsConstant READ = 0x0001;

    /// <summary>
    /// GPUMapMode 的规范常量 WRITE，WebIDL 类型为 GPUFlagsConstant，值为 0x0002。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpumapmode-write">WebGPU: 5.2 Buffer Mapping</see>
    /// </remarks>
    [Description("@#WRITE")]
    public const GPUFlagsConstant WRITE = 0x0002;
}

/// <summary>
/// WebIDL namespace GPUShaderStage。定义于 WebGPU。该 API 仅暴露于安全上下文。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gpushaderstage">WebGPU: 8.1.1 Bind Group Layout Creation</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUShaderStage")]
public static class GPUShaderStage
{
    /// <summary>
    /// GPUShaderStage 的规范常量 COMPUTE，WebIDL 类型为 GPUFlagsConstant，值为 0x4。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-compute">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x1;

    /// <summary>
    /// GPUShaderStage 的规范常量 COMPUTE，WebIDL 类型为 GPUFlagsConstant，值为 0x4。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-compute">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#FRAGMENT")]
    public const GPUFlagsConstant FRAGMENT = 0x2;

    /// <summary>
    /// GPUShaderStage 的规范常量 COMPUTE，WebIDL 类型为 GPUFlagsConstant，值为 0x4。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gpushaderstage-compute">WebGPU: 8.1.1 Bind Group Layout Creation</see>
    /// </remarks>
    [Description("@#COMPUTE")]
    public const GPUFlagsConstant COMPUTE = 0x4;
}

/// <summary>
/// WebIDL namespace GPUTextureUsage。定义于 WebGPU。该 API 仅暴露于安全上下文。
/// </summary>
/// <remarks>
/// <see href="https://gpuweb.github.io/gpuweb/#namespacedef-gputextureusage">WebGPU: 6.1.2 Texture Usages</see>
/// </remarks>
[ECMAScript]
[Description("@#GPUTextureUsage")]
public static class GPUTextureUsage
{
    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x01;

    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x02;

    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#TEXTURE_BINDING")]
    public const GPUFlagsConstant TEXTURE_BINDING = 0x04;

    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#STORAGE_BINDING")]
    public const GPUFlagsConstant STORAGE_BINDING = 0x08;

    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#RENDER_ATTACHMENT")]
    public const GPUFlagsConstant RENDER_ATTACHMENT = 0x10;

    /// <summary>
    /// GPUTextureUsage 的规范常量 TRANSIENT_ATTACHMENT，WebIDL 类型为 GPUFlagsConstant，值为 0x20。
    /// </summary>
    /// <remarks>
    /// <see href="https://gpuweb.github.io/gpuweb/#dom-gputextureusage-transient_attachment">WebGPU: 6.1.2 Texture Usages</see>
    /// </remarks>
    [Description("@#TRANSIENT_ATTACHMENT")]
    public const GPUFlagsConstant TRANSIENT_ATTACHMENT = 0x20;
}