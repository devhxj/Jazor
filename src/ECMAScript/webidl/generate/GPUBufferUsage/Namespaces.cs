namespace ECMAScript.GPUBufferUsage;

/// <summary>
/// GPUBufferUsage
/// </summary>
[ECMAScript]
[Description("@#GPUBufferUsage")]
public static class GPUBufferUsage
{
    /// <summary>
    /// MAP_READ
    /// </summary>
    [Description("@#MAP_READ")]
    public const GPUFlagsConstant MAP_READ = 0x0001;

    /// <summary>
    /// MAP_WRITE
    /// </summary>
    [Description("@#MAP_WRITE")]
    public const GPUFlagsConstant MAP_WRITE = 0x0002;

    /// <summary>
    /// COPY_SRC
    /// </summary>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x0004;

    /// <summary>
    /// COPY_DST
    /// </summary>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x0008;

    /// <summary>
    /// INDEX
    /// </summary>
    [Description("@#INDEX")]
    public const GPUFlagsConstant INDEX = 0x0010;

    /// <summary>
    /// VERTEX
    /// </summary>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x0020;

    /// <summary>
    /// UNIFORM
    /// </summary>
    [Description("@#UNIFORM")]
    public const GPUFlagsConstant UNIFORM = 0x0040;

    /// <summary>
    /// STORAGE
    /// </summary>
    [Description("@#STORAGE")]
    public const GPUFlagsConstant STORAGE = 0x0080;

    /// <summary>
    /// INDIRECT
    /// </summary>
    [Description("@#INDIRECT")]
    public const GPUFlagsConstant INDIRECT = 0x0100;

    /// <summary>
    /// QUERY_RESOLVE
    /// </summary>
    [Description("@#QUERY_RESOLVE")]
    public const GPUFlagsConstant QUERY_RESOLVE = 0x0200;
}

/// <summary>
/// GPUColorWrite
/// </summary>
[ECMAScript]
[Description("@#GPUColorWrite")]
public static class GPUColorWrite
{
    /// <summary>
    /// RED
    /// </summary>
    [Description("@#RED")]
    public const GPUFlagsConstant RED = 0x1;

    /// <summary>
    /// GREEN
    /// </summary>
    [Description("@#GREEN")]
    public const GPUFlagsConstant GREEN = 0x2;

    /// <summary>
    /// BLUE
    /// </summary>
    [Description("@#BLUE")]
    public const GPUFlagsConstant BLUE = 0x4;

    /// <summary>
    /// ALPHA
    /// </summary>
    [Description("@#ALPHA")]
    public const GPUFlagsConstant ALPHA = 0x8;

    /// <summary>
    /// ALL
    /// </summary>
    [Description("@#ALL")]
    public const GPUFlagsConstant ALL = 0xF;
}

/// <summary>
/// GPUMapMode
/// </summary>
[ECMAScript]
[Description("@#GPUMapMode")]
public static class GPUMapMode
{
    /// <summary>
    /// READ
    /// </summary>
    [Description("@#READ")]
    public const GPUFlagsConstant READ = 0x0001;

    /// <summary>
    /// WRITE
    /// </summary>
    [Description("@#WRITE")]
    public const GPUFlagsConstant WRITE = 0x0002;
}

/// <summary>
/// GPUShaderStage
/// </summary>
[ECMAScript]
[Description("@#GPUShaderStage")]
public static class GPUShaderStage
{
    /// <summary>
    /// VERTEX
    /// </summary>
    [Description("@#VERTEX")]
    public const GPUFlagsConstant VERTEX = 0x1;

    /// <summary>
    /// FRAGMENT
    /// </summary>
    [Description("@#FRAGMENT")]
    public const GPUFlagsConstant FRAGMENT = 0x2;

    /// <summary>
    /// COMPUTE
    /// </summary>
    [Description("@#COMPUTE")]
    public const GPUFlagsConstant COMPUTE = 0x4;
}

/// <summary>
/// GPUTextureUsage
/// </summary>
[ECMAScript]
[Description("@#GPUTextureUsage")]
public static class GPUTextureUsage
{
    /// <summary>
    /// COPY_SRC
    /// </summary>
    [Description("@#COPY_SRC")]
    public const GPUFlagsConstant COPY_SRC = 0x01;

    /// <summary>
    /// COPY_DST
    /// </summary>
    [Description("@#COPY_DST")]
    public const GPUFlagsConstant COPY_DST = 0x02;

    /// <summary>
    /// TEXTURE_BINDING
    /// </summary>
    [Description("@#TEXTURE_BINDING")]
    public const GPUFlagsConstant TEXTURE_BINDING = 0x04;

    /// <summary>
    /// STORAGE_BINDING
    /// </summary>
    [Description("@#STORAGE_BINDING")]
    public const GPUFlagsConstant STORAGE_BINDING = 0x08;

    /// <summary>
    /// RENDER_ATTACHMENT
    /// </summary>
    [Description("@#RENDER_ATTACHMENT")]
    public const GPUFlagsConstant RENDER_ATTACHMENT = 0x10;
}
