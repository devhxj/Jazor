namespace ECMAScript;

/// <summary>
/// XRLightProbeInit
/// </summary>
[ECMAScript]
[Description("@#XRLightProbeInit")]
public record XRLightProbeInit(
    [property: Description("@#reflectionFormat")]XRReflectionFormat ReflectionFormat = XRReflectionFormat.Srgba8);

/// <summary>
/// XRLightProbe
/// </summary>
[ECMAScript]
[Description("@#XRLightProbe")]
public class XRLightProbe : EventTarget
{
    /// <summary>
    /// probeSpace
    /// </summary>
    [Description("@#probeSpace")]
    public extern XRSpace ProbeSpace { get; }

    /// <summary>
    /// onreflectionchange
    /// </summary>
    [Description("@#onreflectionchange")]
    public extern EventHandler Onreflectionchange { get; set; }
}

/// <summary>
/// XRLightEstimate
/// </summary>
[ECMAScript]
[Description("@#XRLightEstimate")]
public class XRLightEstimate
{
    /// <summary>
    /// sphericalHarmonicsCoefficients
    /// </summary>
    [Description("@#sphericalHarmonicsCoefficients")]
    public extern Float32Array SphericalHarmonicsCoefficients { get; }

    /// <summary>
    /// primaryLightDirection
    /// </summary>
    [Description("@#primaryLightDirection")]
    public extern DOMPointReadOnly PrimaryLightDirection { get; }

    /// <summary>
    /// primaryLightIntensity
    /// </summary>
    [Description("@#primaryLightIntensity")]
    public extern DOMPointReadOnly PrimaryLightIntensity { get; }
}