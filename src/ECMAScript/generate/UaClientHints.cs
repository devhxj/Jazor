namespace ECMAScript;

/// <summary>
/// NavigatorUABrandVersion
/// </summary>
[ECMAScript]
[Description("@#NavigatorUABrandVersion")]
public record NavigatorUABrandVersion(
    [property: Description("@#brand")]string? Brand = default,
	[property: Description("@#version")]string? Version = default);

/// <summary>
/// UADataValues
/// </summary>
[ECMAScript]
[Description("@#UADataValues")]
public record UADataValues(
    [property: Description("@#architecture")]string? Architecture = default,
	[property: Description("@#bitness")]string? Bitness = default,
	[property: Description("@#brands")]NavigatorUABrandVersion[]? Brands = default,
	[property: Description("@#formFactors")]string[]? FormFactors = default,
	[property: Description("@#fullVersionList")]NavigatorUABrandVersion[]? FullVersionList = default,
	[property: Description("@#model")]string? Model = default,
	[property: Description("@#mobile")]bool Mobile = default,
	[property: Description("@#platform")]string? Platform = default,
	[property: Description("@#platformVersion")]string? PlatformVersion = default,
	[property: Description("@#uaFullVersion")]string? UaFullVersion = default,
	[property: Description("@#wow64")]bool Wow64 = default);

/// <summary>
/// UALowEntropyJSON
/// </summary>
[ECMAScript]
[Description("@#UALowEntropyJSON")]
public record UALowEntropyJSON(
    [property: Description("@#brands")]NavigatorUABrandVersion[]? Brands = default,
	[property: Description("@#mobile")]bool Mobile = default,
	[property: Description("@#platform")]string? Platform = default);

/// <summary>
/// NavigatorUAData
/// </summary>
[ECMAScript]
[Description("@#NavigatorUAData")]
public class NavigatorUAData
{
    /// <summary>
    /// brands
    /// </summary>
    [Description("@#brands")]
    public extern FrozenSet<NavigatorUABrandVersion> Brands { get; }

    /// <summary>
    /// mobile
    /// </summary>
    [Description("@#mobile")]
    public extern bool Mobile { get; }

    /// <summary>
    /// platform
    /// </summary>
    [Description("@#platform")]
    public extern string Platform { get; }

    /// <summary>
    /// getHighEntropyValues
    /// </summary>
    /// <param name="hints">hints</param>
    [Description("@#getHighEntropyValues")]
    public extern PromiseResult<UADataValues> GetHighEntropyValues(string[] hints);

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern UALowEntropyJSON ToJSON();
}