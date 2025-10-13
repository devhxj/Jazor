namespace ECMAScript;

/// <summary>
/// OVR_multiview2
/// </summary>
[ECMAScript]
[Description("@#OVR_multiview2")]
public class OVRMultiview2
{
    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_TEXTURE_NUM_VIEWS_OVR
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_TEXTURE_NUM_VIEWS_OVR")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_TEXTURE_NUM_VIEWS_OVR = 0x9630;

    /// <summary>
    /// FRAMEBUFFER_ATTACHMENT_TEXTURE_BASE_VIEW_INDEX_OVR
    /// </summary>
    [Description("@#FRAMEBUFFER_ATTACHMENT_TEXTURE_BASE_VIEW_INDEX_OVR")]
    public const GLenum FRAMEBUFFER_ATTACHMENT_TEXTURE_BASE_VIEW_INDEX_OVR = 0x9632;

    /// <summary>
    /// MAX_VIEWS_OVR
    /// </summary>
    [Description("@#MAX_VIEWS_OVR")]
    public const GLenum MAX_VIEWS_OVR = 0x9631;

    /// <summary>
    /// FRAMEBUFFER_INCOMPLETE_VIEW_TARGETS_OVR
    /// </summary>
    [Description("@#FRAMEBUFFER_INCOMPLETE_VIEW_TARGETS_OVR")]
    public const GLenum FRAMEBUFFER_INCOMPLETE_VIEW_TARGETS_OVR = 0x9633;

    /// <summary>
    /// framebufferTextureMultiviewOVR
    /// </summary>
    /// <param name="target">target</param>
    /// <param name="attachment">attachment</param>
    /// <param name="texture">texture</param>
    /// <param name="level">level</param>
    /// <param name="baseViewIndex">baseViewIndex</param>
    /// <param name="numViews">numViews</param>
    [Description("@#framebufferTextureMultiviewOVR")]
    public extern void FramebufferTextureMultiviewOVR(GLenum target, GLenum attachment, WebGLTexture? texture, GLint level, GLint baseViewIndex, GLsizei numViews);
}