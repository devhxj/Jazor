namespace ECMAScript;

/// <summary>
/// EXT_texture_compression_bptc
/// </summary>
[ECMAScript]
[Description("@#EXT_texture_compression_bptc")]
public class EXTTextureCompressionBptc
{
    /// <summary>
    /// COMPRESSED_RGBA_BPTC_UNORM_EXT
    /// </summary>
    [Description("@#COMPRESSED_RGBA_BPTC_UNORM_EXT")]
    public const GLenum COMPRESSED_RGBA_BPTC_UNORM_EXT = 0x8E8C;

    /// <summary>
    /// COMPRESSED_SRGB_ALPHA_BPTC_UNORM_EXT
    /// </summary>
    [Description("@#COMPRESSED_SRGB_ALPHA_BPTC_UNORM_EXT")]
    public const GLenum COMPRESSED_SRGB_ALPHA_BPTC_UNORM_EXT = 0x8E8D;

    /// <summary>
    /// COMPRESSED_RGB_BPTC_SIGNED_FLOAT_EXT
    /// </summary>
    [Description("@#COMPRESSED_RGB_BPTC_SIGNED_FLOAT_EXT")]
    public const GLenum COMPRESSED_RGB_BPTC_SIGNED_FLOAT_EXT = 0x8E8E;

    /// <summary>
    /// COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_EXT
    /// </summary>
    [Description("@#COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_EXT")]
    public const GLenum COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_EXT = 0x8E8F;
}