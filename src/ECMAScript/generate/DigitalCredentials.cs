namespace ECMAScript;

/// <summary>
/// DigitalCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#DigitalCredentialRequestOptions")]
public record DigitalCredentialRequestOptions(
    [property: Description("@#requests")]DigitalCredentialGetRequest[]? Requests = default);

/// <summary>
/// DigitalCredentialGetRequest
/// </summary>
[ECMAScript]
[Description("@#DigitalCredentialGetRequest")]
public record DigitalCredentialGetRequest(
    [property: Description("@#protocol")]string? Protocol = default,
	[property: Description("@#data")]object? Data = default);

/// <summary>
/// DigitalCredentialCreationOptions
/// </summary>
[ECMAScript]
[Description("@#DigitalCredentialCreationOptions")]
public record DigitalCredentialCreationOptions(
    [property: Description("@#requests")]DigitalCredentialCreateRequest[]? Requests = default);

/// <summary>
/// DigitalCredentialCreateRequest
/// </summary>
[ECMAScript]
[Description("@#DigitalCredentialCreateRequest")]
public record DigitalCredentialCreateRequest(
    [property: Description("@#protocol")]string? Protocol = default,
	[property: Description("@#data")]object? Data = default);

/// <summary>
/// DigitalCredential
/// </summary>
[ECMAScript]
[Description("@#DigitalCredential")]
public class DigitalCredential : Credential
{
    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}