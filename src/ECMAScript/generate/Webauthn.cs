namespace ECMAScript;

/// <summary>
/// RegistrationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#RegistrationResponseJSON")]
public record RegistrationResponseJSON(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#rawId")]Base64URLString? RawId = default,
	[property: Description("@#response")]AuthenticatorAttestationResponseJSON? Response = default,
	[property: Description("@#authenticatorAttachment")]string? AuthenticatorAttachment = default,
	[property: Description("@#clientExtensionResults")]AuthenticationExtensionsClientOutputsJSON? ClientExtensionResults = default,
	[property: Description("@#type")]string? Type = default);

/// <summary>
/// AuthenticatorAttestationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorAttestationResponseJSON")]
public record AuthenticatorAttestationResponseJSON(
    [property: Description("@#clientDataJSON")]Base64URLString? ClientDataJSON = default,
	[property: Description("@#authenticatorData")]Base64URLString? AuthenticatorData = default,
	[property: Description("@#transports")]string[]? Transports = default,
	[property: Description("@#publicKey")]Base64URLString? PublicKey = default,
	[property: Description("@#publicKeyAlgorithm")]COSEAlgorithmIdentifier? PublicKeyAlgorithm = default,
	[property: Description("@#attestationObject")]Base64URLString? AttestationObject = default);

/// <summary>
/// AuthenticationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationResponseJSON")]
public record AuthenticationResponseJSON(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#rawId")]Base64URLString? RawId = default,
	[property: Description("@#response")]AuthenticatorAssertionResponseJSON? Response = default,
	[property: Description("@#authenticatorAttachment")]string? AuthenticatorAttachment = default,
	[property: Description("@#clientExtensionResults")]AuthenticationExtensionsClientOutputsJSON? ClientExtensionResults = default,
	[property: Description("@#type")]string? Type = default);

/// <summary>
/// AuthenticatorAssertionResponseJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorAssertionResponseJSON")]
public record AuthenticatorAssertionResponseJSON(
    [property: Description("@#clientDataJSON")]Base64URLString? ClientDataJSON = default,
	[property: Description("@#authenticatorData")]Base64URLString? AuthenticatorData = default,
	[property: Description("@#signature")]Base64URLString? Signature = default,
	[property: Description("@#userHandle")]Base64URLString? UserHandle = default);

/// <summary>
/// AuthenticationExtensionsClientOutputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientOutputsJSON")]
public record AuthenticationExtensionsClientOutputsJSON(
    [property: Description("@#appid")]bool Appid = default,
	[property: Description("@#appidExclude")]bool AppidExclude = default,
	[property: Description("@#credProps")]CredentialPropertiesOutput? CredProps = default,
	[property: Description("@#prf")]AuthenticationExtensionsPRFOutputsJSON? Prf = default,
	[property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobOutputsJSON? LargeBlob = default)
{
    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON Optional();

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON OptionalAppid(
        [Description("@#appid")]bool Appid = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON OptionalAppidExclude(
        [Description("@#appidExclude")]bool AppidExclude = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON OptionalCredProps(
        [Description("@#credProps")]CredentialPropertiesOutput? CredProps = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON OptionalPrf(
        [Description("@#prf")]AuthenticationExtensionsPRFOutputsJSON? Prf = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputsJSON OptionalLargeBlob(
        [Description("@#largeBlob")]AuthenticationExtensionsLargeBlobOutputsJSON? LargeBlob = default);
}

/// <summary>
/// PublicKeyCredentialCreationOptionsJSON
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialCreationOptionsJSON")]
public record PublicKeyCredentialCreationOptionsJSON(
    [property: Description("@#rp")]PublicKeyCredentialRpEntity? Rp = default,
	[property: Description("@#user")]PublicKeyCredentialUserEntityJSON? User = default,
	[property: Description("@#challenge")]Base64URLString? Challenge = default,
	[property: Description("@#pubKeyCredParams")]PublicKeyCredentialParameters[]? PubKeyCredParams = default,
	[property: Description("@#timeout")]uint Timeout = default,
	[property: Description("@#excludeCredentials")]PublicKeyCredentialDescriptorJSON[]? ExcludeCredentials = default,
	[property: Description("@#authenticatorSelection")]AuthenticatorSelectionCriteria? AuthenticatorSelection = default,
	[property: Description("@#hints")]string[]? Hints = default,
	[property: Description("@#attestation")]string? Attestation = default,
	[property: Description("@#attestationFormats")]string[]? AttestationFormats = default,
	[property: Description("@#extensions")]AuthenticationExtensionsClientInputsJSON? Extensions = default);

/// <summary>
/// PublicKeyCredentialUserEntityJSON
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialUserEntityJSON")]
public record PublicKeyCredentialUserEntityJSON(
    [property: Description("@#id")]Base64URLString? Id = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#displayName")]string? DisplayName = default);

/// <summary>
/// PublicKeyCredentialDescriptorJSON
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialDescriptorJSON")]
public record PublicKeyCredentialDescriptorJSON(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#id")]Base64URLString? Id = default,
	[property: Description("@#transports")]string[]? Transports = default);

/// <summary>
/// AuthenticationExtensionsClientInputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientInputsJSON")]
public record AuthenticationExtensionsClientInputsJSON(
    [property: Description("@#appid")]string? Appid = default,
	[property: Description("@#appidExclude")]string? AppidExclude = default,
	[property: Description("@#credProps")]bool CredProps = default,
	[property: Description("@#prf")]AuthenticationExtensionsPRFInputsJSON? Prf = default,
	[property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobInputsJSON? LargeBlob = default)
{
    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON Optional();

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON OptionalAppid(
        [Description("@#appid")]string? Appid = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON OptionalAppidExclude(
        [Description("@#appidExclude")]string? AppidExclude = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON OptionalCredProps(
        [Description("@#credProps")]bool CredProps = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON OptionalPrf(
        [Description("@#prf")]AuthenticationExtensionsPRFInputsJSON? Prf = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputsJSON OptionalLargeBlob(
        [Description("@#largeBlob")]AuthenticationExtensionsLargeBlobInputsJSON? LargeBlob = default);
}

/// <summary>
/// PublicKeyCredentialRequestOptionsJSON
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialRequestOptionsJSON")]
public record PublicKeyCredentialRequestOptionsJSON(
    [property: Description("@#challenge")]Base64URLString? Challenge = default,
	[property: Description("@#timeout")]uint Timeout = default,
	[property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#allowCredentials")]PublicKeyCredentialDescriptorJSON[]? AllowCredentials = default,
	[property: Description("@#userVerification")]string? UserVerification = default,
	[property: Description("@#hints")]string[]? Hints = default,
	[property: Description("@#extensions")]AuthenticationExtensionsClientInputsJSON? Extensions = default);

/// <summary>
/// UnknownCredentialOptions
/// </summary>
[ECMAScript]
[Description("@#UnknownCredentialOptions")]
public record UnknownCredentialOptions(
    [property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#credentialId")]Base64URLString? CredentialId = default);

/// <summary>
/// AllAcceptedCredentialsOptions
/// </summary>
[ECMAScript]
[Description("@#AllAcceptedCredentialsOptions")]
public record AllAcceptedCredentialsOptions(
    [property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#userId")]Base64URLString? UserId = default,
	[property: Description("@#allAcceptedCredentialIds")]Base64URLString[]? AllAcceptedCredentialIds = default);

/// <summary>
/// CurrentUserDetailsOptions
/// </summary>
[ECMAScript]
[Description("@#CurrentUserDetailsOptions")]
public record CurrentUserDetailsOptions(
    [property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#userId")]Base64URLString? UserId = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#displayName")]string? DisplayName = default);

/// <summary>
/// PublicKeyCredentialParameters
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialParameters")]
public record PublicKeyCredentialParameters(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#alg")]COSEAlgorithmIdentifier? Alg = default);

/// <summary>
/// PublicKeyCredentialCreationOptions
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialCreationOptions")]
public record PublicKeyCredentialCreationOptions(
    [property: Description("@#rp")]PublicKeyCredentialRpEntity? Rp = default,
	[property: Description("@#user")]PublicKeyCredentialUserEntity? User = default,
	[property: Description("@#challenge")]IBufferSource? Challenge = default,
	[property: Description("@#pubKeyCredParams")]PublicKeyCredentialParameters[]? PubKeyCredParams = default,
	[property: Description("@#timeout")]uint Timeout = default,
	[property: Description("@#excludeCredentials")]PublicKeyCredentialDescriptor[]? ExcludeCredentials = default,
	[property: Description("@#authenticatorSelection")]AuthenticatorSelectionCriteria? AuthenticatorSelection = default,
	[property: Description("@#hints")]string[]? Hints = default,
	[property: Description("@#attestation")]string? Attestation = default,
	[property: Description("@#attestationFormats")]string[]? AttestationFormats = default,
	[property: Description("@#extensions")]AuthenticationExtensionsClientInputs? Extensions = default);

/// <summary>
/// PublicKeyCredentialEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialEntity")]
public record PublicKeyCredentialEntity(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// PublicKeyCredentialRpEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialRpEntity")]
public record PublicKeyCredentialRpEntity(
    [property: Description("@#id")]string? Id = default): PublicKeyCredentialEntity;

/// <summary>
/// PublicKeyCredentialUserEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialUserEntity")]
public record PublicKeyCredentialUserEntity(
    [property: Description("@#id")]IBufferSource? Id = default,
	[property: Description("@#displayName")]string? DisplayName = default): PublicKeyCredentialEntity;

/// <summary>
/// AuthenticatorSelectionCriteria
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorSelectionCriteria")]
public record AuthenticatorSelectionCriteria(
    [property: Description("@#authenticatorAttachment")]string? AuthenticatorAttachment = default,
	[property: Description("@#residentKey")]string? ResidentKey = default,
	[property: Description("@#requireResidentKey")]bool RequireResidentKey = false,
	[property: Description("@#userVerification")]string? UserVerification = default);

/// <summary>
/// PublicKeyCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialRequestOptions")]
public record PublicKeyCredentialRequestOptions(
    [property: Description("@#challenge")]IBufferSource? Challenge = default,
	[property: Description("@#timeout")]uint Timeout = default,
	[property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#allowCredentials")]PublicKeyCredentialDescriptor[]? AllowCredentials = default,
	[property: Description("@#userVerification")]string? UserVerification = default,
	[property: Description("@#hints")]string[]? Hints = default,
	[property: Description("@#extensions")]AuthenticationExtensionsClientInputs? Extensions = default);

/// <summary>
/// CollectedClientData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientData")]
public record CollectedClientData(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#challenge")]string? Challenge = default,
	[property: Description("@#origin")]string? Origin = default,
	[property: Description("@#crossOrigin")]bool CrossOrigin = default,
	[property: Description("@#topOrigin")]string? TopOrigin = default);

/// <summary>
/// TokenBinding
/// </summary>
[ECMAScript]
[Description("@#TokenBinding")]
public record TokenBinding(
    [property: Description("@#status")]string? Status = default,
	[property: Description("@#id")]string? Id = default);

/// <summary>
/// PublicKeyCredentialDescriptor
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialDescriptor")]
public record PublicKeyCredentialDescriptor(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#id")]IBufferSource? Id = default,
	[property: Description("@#transports")]string[]? Transports = default);

/// <summary>
/// CredentialPropertiesOutput
/// </summary>
[ECMAScript]
[Description("@#CredentialPropertiesOutput")]
public record CredentialPropertiesOutput(
    [property: Description("@#rk")]bool Rk = default);

/// <summary>
/// AuthenticationExtensionsPRFValues
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFValues")]
public record AuthenticationExtensionsPRFValues(
    [property: Description("@#first")]IBufferSource? First = default,
	[property: Description("@#second")]IBufferSource? Second = default);

/// <summary>
/// AuthenticationExtensionsPRFValuesJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFValuesJSON")]
public record AuthenticationExtensionsPRFValuesJSON(
    [property: Description("@#first")]Base64URLString? First = default,
	[property: Description("@#second")]Base64URLString? Second = default);

/// <summary>
/// AuthenticationExtensionsPRFInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFInputs")]
public record AuthenticationExtensionsPRFInputs(
    [property: Description("@#eval")]AuthenticationExtensionsPRFValues? Eval = default,
	[property: Description("@#evalByCredential")]Dictionary<string, AuthenticationExtensionsPRFValues>? EvalByCredential = default);

/// <summary>
/// AuthenticationExtensionsPRFInputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFInputsJSON")]
public record AuthenticationExtensionsPRFInputsJSON(
    [property: Description("@#eval")]AuthenticationExtensionsPRFValuesJSON? Eval = default,
	[property: Description("@#evalByCredential")]Dictionary<string, AuthenticationExtensionsPRFValuesJSON>? EvalByCredential = default);

/// <summary>
/// AuthenticationExtensionsPRFOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFOutputs")]
public record AuthenticationExtensionsPRFOutputs(
    [property: Description("@#enabled")]bool Enabled = default,
	[property: Description("@#results")]AuthenticationExtensionsPRFValues? Results = default);

/// <summary>
/// AuthenticationExtensionsPRFOutputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFOutputsJSON")]
public record AuthenticationExtensionsPRFOutputsJSON(
    [property: Description("@#enabled")]bool Enabled = default,
	[property: Description("@#results")]AuthenticationExtensionsPRFValuesJSON? Results = default);

/// <summary>
/// AuthenticationExtensionsLargeBlobInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsLargeBlobInputs")]
public record AuthenticationExtensionsLargeBlobInputs(
    [property: Description("@#support")]string? Support = default,
	[property: Description("@#read")]bool Read = default,
	[property: Description("@#write")]IBufferSource? Write = default);

/// <summary>
/// AuthenticationExtensionsLargeBlobInputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsLargeBlobInputsJSON")]
public record AuthenticationExtensionsLargeBlobInputsJSON(
    [property: Description("@#support")]string? Support = default,
	[property: Description("@#read")]bool Read = default,
	[property: Description("@#write")]Base64URLString? Write = default);

/// <summary>
/// AuthenticationExtensionsLargeBlobOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsLargeBlobOutputs")]
public record AuthenticationExtensionsLargeBlobOutputs(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#blob")]ArrayBuffer? Blob = default,
	[property: Description("@#written")]bool Written = default);

/// <summary>
/// AuthenticationExtensionsLargeBlobOutputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsLargeBlobOutputsJSON")]
public record AuthenticationExtensionsLargeBlobOutputsJSON(
    [property: Description("@#supported")]bool Supported = default,
	[property: Description("@#blob")]Base64URLString? Blob = default,
	[property: Description("@#written")]bool Written = default);

/// <summary>
/// PublicKeyCredential
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredential")]
public partial class PublicKeyCredential : Credential
{
    /// <summary>
    /// rawId
    /// </summary>
    [Description("@#rawId")]
    public extern ArrayBuffer RawId { get; }

    /// <summary>
    /// response
    /// </summary>
    [Description("@#response")]
    public extern AuthenticatorResponse Response { get; }

    /// <summary>
    /// authenticatorAttachment
    /// </summary>
    [Description("@#authenticatorAttachment")]
    public extern string? AuthenticatorAttachment { get; }

    /// <summary>
    /// getClientExtensionResults
    /// </summary>
    [Description("@#getClientExtensionResults")]
    public extern AuthenticationExtensionsClientOutputs GetClientExtensionResults();

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern PublicKeyCredentialJSON ToJSON();

    /// <summary>
    /// isUserVerifyingPlatformAuthenticatorAvailable
    /// </summary>
    [Description("@#isUserVerifyingPlatformAuthenticatorAvailable")]
    public extern static PromiseResult<bool> IsUserVerifyingPlatformAuthenticatorAvailable();

    /// <summary>
    /// getClientCapabilities
    /// </summary>
    [Description("@#getClientCapabilities")]
    public extern static PromiseResult<PublicKeyCredentialClientCapabilities> GetClientCapabilities();

    /// <summary>
    /// parseCreationOptionsFromJSON
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#parseCreationOptionsFromJSON")]
    public extern static PublicKeyCredentialCreationOptions ParseCreationOptionsFromJSON(PublicKeyCredentialCreationOptionsJSON options);

    /// <summary>
    /// parseRequestOptionsFromJSON
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#parseRequestOptionsFromJSON")]
    public extern static PublicKeyCredentialRequestOptions ParseRequestOptionsFromJSON(PublicKeyCredentialRequestOptionsJSON options);

    /// <summary>
    /// signalUnknownCredential
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#signalUnknownCredential")]
    public extern static PromiseResult<object> SignalUnknownCredential(UnknownCredentialOptions options);

    /// <summary>
    /// signalAllAcceptedCredentials
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#signalAllAcceptedCredentials")]
    public extern static PromiseResult<object> SignalAllAcceptedCredentials(AllAcceptedCredentialsOptions options);

    /// <summary>
    /// signalCurrentUserDetails
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#signalCurrentUserDetails")]
    public extern static PromiseResult<object> SignalCurrentUserDetails(CurrentUserDetailsOptions options);
}

/// <summary>
/// AuthenticatorResponse
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorResponse")]
public class AuthenticatorResponse
{
    /// <summary>
    /// clientDataJSON
    /// </summary>
    [Description("@#clientDataJSON")]
    public extern ArrayBuffer ClientDataJSON { get; }
}

/// <summary>
/// AuthenticatorAttestationResponse
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorAttestationResponse")]
public class AuthenticatorAttestationResponse : AuthenticatorResponse
{
    /// <summary>
    /// attestationObject
    /// </summary>
    [Description("@#attestationObject")]
    public extern ArrayBuffer AttestationObject { get; }

    /// <summary>
    /// getTransports
    /// </summary>
    [Description("@#getTransports")]
    public extern string[] GetTransports();

    /// <summary>
    /// getAuthenticatorData
    /// </summary>
    [Description("@#getAuthenticatorData")]
    public extern ArrayBuffer GetAuthenticatorData();

    /// <summary>
    /// getPublicKey
    /// </summary>
    [Description("@#getPublicKey")]
    public extern ArrayBuffer? GetPublicKey();

    /// <summary>
    /// getPublicKeyAlgorithm
    /// </summary>
    [Description("@#getPublicKeyAlgorithm")]
    public extern COSEAlgorithmIdentifier GetPublicKeyAlgorithm();
}

/// <summary>
/// AuthenticatorAssertionResponse
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorAssertionResponse")]
public class AuthenticatorAssertionResponse : AuthenticatorResponse
{
    /// <summary>
    /// authenticatorData
    /// </summary>
    [Description("@#authenticatorData")]
    public extern ArrayBuffer AuthenticatorData { get; }

    /// <summary>
    /// signature
    /// </summary>
    [Description("@#signature")]
    public extern ArrayBuffer Signature { get; }

    /// <summary>
    /// userHandle
    /// </summary>
    [Description("@#userHandle")]
    public extern ArrayBuffer? UserHandle { get; }
}