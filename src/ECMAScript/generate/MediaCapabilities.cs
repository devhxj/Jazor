namespace ECMAScript;

/// <summary>
/// MediaConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaConfiguration")]
public record MediaConfiguration(
    [property: Description("@#video")]VideoConfiguration? Video = default,
	[property: Description("@#audio")]AudioConfiguration? Audio = default);

/// <summary>
/// MediaDecodingConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaDecodingConfiguration")]
public record MediaDecodingConfiguration(
    [property: Description("@#type")]MediaDecodingType? Type = default,
	[property: Description("@#keySystemConfiguration")]MediaCapabilitiesKeySystemConfiguration? KeySystemConfiguration = default): MediaConfiguration;

/// <summary>
/// MediaEncodingConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaEncodingConfiguration")]
public record MediaEncodingConfiguration(
    [property: Description("@#type")]MediaEncodingType? Type = default): MediaConfiguration;

/// <summary>
/// VideoConfiguration
/// </summary>
[ECMAScript]
[Description("@#VideoConfiguration")]
public record VideoConfiguration(
    [property: Description("@#contentType")]string? ContentType = default,
	[property: Description("@#width")]uint Width = default,
	[property: Description("@#height")]uint Height = default,
	[property: Description("@#bitrate")]ulong Bitrate = default,
	[property: Description("@#framerate")]double Framerate = default,
	[property: Description("@#hasAlphaChannel")]bool HasAlphaChannel = default,
	[property: Description("@#hdrMetadataType")]HdrMetadataType? HdrMetadataType = default,
	[property: Description("@#colorGamut")]ColorGamut? ColorGamut = default,
	[property: Description("@#transferFunction")]TransferFunction? TransferFunction = default,
	[property: Description("@#scalabilityMode")]string? ScalabilityMode = default,
	[property: Description("@#spatialScalability")]bool SpatialScalability = default);

/// <summary>
/// AudioConfiguration
/// </summary>
[ECMAScript]
[Description("@#AudioConfiguration")]
public record AudioConfiguration(
    [property: Description("@#contentType")]string? ContentType = default,
	[property: Description("@#channels")]string? Channels = default,
	[property: Description("@#bitrate")]ulong Bitrate = default,
	[property: Description("@#samplerate")]uint Samplerate = default,
	[property: Description("@#spatialRendering")]bool SpatialRendering = default);

/// <summary>
/// MediaCapabilitiesKeySystemConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesKeySystemConfiguration")]
public record MediaCapabilitiesKeySystemConfiguration(
    [property: Description("@#keySystem")]string? KeySystem = default,
	[property: Description("@#initDataType")]string? InitDataType = default,
	[property: Description("@#distinctiveIdentifier")]MediaKeysRequirement DistinctiveIdentifier = MediaKeysRequirement.Optional,
	[property: Description("@#persistentState")]MediaKeysRequirement PersistentState = MediaKeysRequirement.Optional,
	[property: Description("@#sessionTypes")]string[]? SessionTypes = default,
	[property: Description("@#audio")]KeySystemTrackConfiguration? Audio = default,
	[property: Description("@#video")]KeySystemTrackConfiguration? Video = default);

/// <summary>
/// KeySystemTrackConfiguration
/// </summary>
[ECMAScript]
[Description("@#KeySystemTrackConfiguration")]
public record KeySystemTrackConfiguration(
    [property: Description("@#robustness")]string? Robustness = default,
	[property: Description("@#encryptionScheme")]string? EncryptionScheme = null);

/// <summary>
/// MediaCapabilitiesInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesInfo")]
public record MediaCapabilitiesInfo(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#smooth")]bool Smooth = default,
	[property: Description("@#powerEfficient")]bool PowerEfficient = default);

/// <summary>
/// MediaCapabilitiesDecodingInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesDecodingInfo")]
public record MediaCapabilitiesDecodingInfo(
    [property: Description("@#keySystemAccess")]MediaKeySystemAccess? KeySystemAccess = default,
	[property: Description("@#configuration")]MediaDecodingConfiguration? Configuration = default): MediaCapabilitiesInfo;

/// <summary>
/// MediaCapabilitiesEncodingInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesEncodingInfo")]
public record MediaCapabilitiesEncodingInfo(
    [property: Description("@#configuration")]MediaEncodingConfiguration? Configuration = default): MediaCapabilitiesInfo;

/// <summary>
/// MediaCapabilities
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilities")]
public class MediaCapabilities
{
    /// <summary>
    /// decodingInfo
    /// </summary>
    /// <param name="configuration">configuration</param>
    [Description("@#decodingInfo")]
    public extern PromiseResult<MediaCapabilitiesDecodingInfo> DecodingInfo(MediaDecodingConfiguration configuration);

    /// <summary>
    /// encodingInfo
    /// </summary>
    /// <param name="configuration">configuration</param>
    [Description("@#encodingInfo")]
    public extern PromiseResult<MediaCapabilitiesEncodingInfo> EncodingInfo(MediaEncodingConfiguration configuration);
}