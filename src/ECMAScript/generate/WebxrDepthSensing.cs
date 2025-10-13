namespace ECMAScript;

/// <summary>
/// XRDepthStateInit
/// </summary>
[ECMAScript]
[Description("@#XRDepthStateInit")]
public record XRDepthStateInit(
    [property: Description("@#usagePreference")]XRDepthUsage[]? UsagePreference = default,
	[property: Description("@#dataFormatPreference")]XRDepthDataFormat[]? DataFormatPreference = default,
	[property: Description("@#depthTypeRequest")]XRDepthType[]? DepthTypeRequest = default,
	[property: Description("@#matchDepthView")]bool MatchDepthView = true);

/// <summary>
/// XRSessionInit
/// </summary>
[ECMAScript]
[Description("@#XRSessionInit")]
public record XRSessionInit(
    [property: Description("@#depthSensing")]XRDepthStateInit? DepthSensing = default,
	[property: Description("@#domOverlay")]XRDOMOverlayInit? DomOverlay = default,
	[property: Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
	[property: Description("@#optionalFeatures")]string[]? OptionalFeatures = default)
{
    [Category("optional")]
    public extern static XRSessionInit OptionalDepthSensing(
        [Description("@#depthSensing")]XRDepthStateInit? DepthSensing = default);

    [Category("optional")]
    public extern static XRSessionInit OptionalDomOverlay(
        [Description("@#domOverlay")]XRDOMOverlayInit? DomOverlay = default);

    [Category("optional")]
    public extern static XRSessionInit OptionalRequiredFeaturesOptionalFeatures(
        [Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
        [Description("@#optionalFeatures")]string[]? OptionalFeatures = default);
}

/// <summary>
/// XRDepthInformation
/// </summary>
[ECMAScript]
[Description("@#XRDepthInformation")]
public class XRDepthInformation
{
    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; }

    /// <summary>
    /// normDepthBufferFromNormView
    /// </summary>
    [Description("@#normDepthBufferFromNormView")]
    public extern XRRigidTransform NormDepthBufferFromNormView { get; }

    /// <summary>
    /// rawValueToMeters
    /// </summary>
    [Description("@#rawValueToMeters")]
    public extern float RawValueToMeters { get; }

    #region mixin XRViewGeometry
    /// <summary>
    /// projectionMatrix
    /// </summary>
    [Description("@#projectionMatrix")]
    public extern Float32Array ProjectionMatrix { get; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern XRRigidTransform Transform { get; }
    #endregion
}

/// <summary>
/// XRCPUDepthInformation
/// </summary>
[ECMAScript]
[Description("@#XRCPUDepthInformation")]
public class XRCPUDepthInformation : XRDepthInformation
{
    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern ArrayBuffer Data { get; }

    /// <summary>
    /// getDepthInMeters
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#getDepthInMeters")]
    public extern float GetDepthInMeters(float x, float y);
}

/// <summary>
/// XRWebGLDepthInformation
/// </summary>
[ECMAScript]
[Description("@#XRWebGLDepthInformation")]
public class XRWebGLDepthInformation : XRDepthInformation
{
    /// <summary>
    /// texture
    /// </summary>
    [Description("@#texture")]
    public extern WebGLTexture Texture { get; }

    /// <summary>
    /// textureType
    /// </summary>
    [Description("@#textureType")]
    public extern XRTextureType TextureType { get; }

    /// <summary>
    /// imageIndex
    /// </summary>
    [Description("@#imageIndex")]
    public extern uint? ImageIndex { get; }
}