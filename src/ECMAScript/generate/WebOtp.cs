namespace ECMAScript;

/// <summary>
/// OTPCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#OTPCredentialRequestOptions")]
public record OTPCredentialRequestOptions(
    [property: Description("@#transport")]OTPCredentialTransportType[]? Transport = default);

/// <summary>
/// OTPCredential
/// </summary>
[ECMAScript]
[Description("@#OTPCredential")]
public class OTPCredential : Credential
{
    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern string Code { get; }
}