namespace ECMAScript;

/// <summary>
/// CredentialData
/// </summary>
[ECMAScript]
[Description("@#CredentialData")]
public record CredentialData(
    [property: Description("@#id")]string? Id = default);

/// <summary>
/// CredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#CredentialRequestOptions")]
public record CredentialRequestOptions(
    [property: Description("@#mediation")]CredentialMediationRequirement Mediation = CredentialMediationRequirement.Optional,
	[property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#password")]bool Password = false,
	[property: Description("@#federated")]FederatedCredentialRequestOptions? Federated = default,
	[property: Description("@#digital")]DigitalCredentialRequestOptions? Digital = default,
	[property: Description("@#identity")]IdentityCredentialRequestOptions? Identity = default,
	[property: Description("@#otp")]OTPCredentialRequestOptions? Otp = default,
	[property: Description("@#publicKey")]PublicKeyCredentialRequestOptions? PublicKey = default)
{
    [Category("optional")]
    public extern static CredentialRequestOptions OptionalMediationSignal(
        [Description("@#mediation")]CredentialMediationRequirement mediation = CredentialMediationRequirement.Optional,
        [Description("@#signal")]AbortSignal? Signal = default);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalPassword(
        [Description("@#password")]bool password = false);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalFederated(
        [Description("@#federated")]FederatedCredentialRequestOptions? Federated = default);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalDigital(
        [Description("@#digital")]DigitalCredentialRequestOptions? Digital = default);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalIdentity(
        [Description("@#identity")]IdentityCredentialRequestOptions? Identity = default);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalOtp(
        [Description("@#otp")]OTPCredentialRequestOptions? Otp = default);

    [Category("optional")]
    public extern static CredentialRequestOptions OptionalPublicKey(
        [Description("@#publicKey")]PublicKeyCredentialRequestOptions? PublicKey = default);
}

/// <summary>
/// CredentialCreationOptions
/// </summary>
[ECMAScript]
[Description("@#CredentialCreationOptions")]
public record CredentialCreationOptions(
    [property: Description("@#mediation")]CredentialMediationRequirement Mediation = CredentialMediationRequirement.Optional,
	[property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#password")]PasswordCredentialInit? Password = default,
	[property: Description("@#federated")]FederatedCredentialInit? Federated = default,
	[property: Description("@#digital")]DigitalCredentialCreationOptions? Digital = default,
	[property: Description("@#publicKey")]PublicKeyCredentialCreationOptions? PublicKey = default)
{
    [Category("optional")]
    public extern static CredentialCreationOptions OptionalMediationSignal(
        [Description("@#mediation")]CredentialMediationRequirement mediation = CredentialMediationRequirement.Optional,
        [Description("@#signal")]AbortSignal? Signal = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalPassword(
        [Description("@#password")]PasswordCredentialInit? Password = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalFederated(
        [Description("@#federated")]FederatedCredentialInit? Federated = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalDigital(
        [Description("@#digital")]DigitalCredentialCreationOptions? Digital = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalPublicKey(
        [Description("@#publicKey")]PublicKeyCredentialCreationOptions? PublicKey = default);
}

/// <summary>
/// PasswordCredentialData
/// </summary>
[ECMAScript]
[Description("@#PasswordCredentialData")]
public record PasswordCredentialData(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#iconURL")]string? IconURL = default,
	[property: Description("@#origin")]string? Origin = default,
	[property: Description("@#password")]string? Password = default): CredentialData;

/// <summary>
/// FederatedCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#FederatedCredentialRequestOptions")]
public record FederatedCredentialRequestOptions(
    [property: Description("@#providers")]string[]? Providers = default,
	[property: Description("@#protocols")]string[]? Protocols = default);

/// <summary>
/// FederatedCredentialInit
/// </summary>
[ECMAScript]
[Description("@#FederatedCredentialInit")]
public record FederatedCredentialInit(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#iconURL")]string? IconURL = default,
	[property: Description("@#origin")]string? Origin = default,
	[property: Description("@#provider")]string? Provider = default,
	[property: Description("@#protocol")]string? Protocol = default): CredentialData;

/// <summary>
/// Credential
/// </summary>
[ECMAScript]
[Description("@#Credential")]
public class Credential
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// isConditionalMediationAvailable
    /// </summary>
    [Description("@#isConditionalMediationAvailable")]
    public extern static PromiseResult<bool> IsConditionalMediationAvailable();

    /// <summary>
    /// willRequestConditionalCreation
    /// </summary>
    [Description("@#willRequestConditionalCreation")]
    public extern static PromiseResult<object> WillRequestConditionalCreation();
}

/// <summary>
/// CredentialsContainer
/// </summary>
[ECMAScript]
[Description("@#CredentialsContainer")]
public class CredentialsContainer
{
    /// <summary>
    /// get
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#get")]
    public extern PromiseResult<Credential?> Get(CredentialRequestOptions? options = default);

    /// <summary>
    /// store
    /// </summary>
    /// <param name="credential">credential</param>
    [Description("@#store")]
    public extern PromiseResult<object> Store(Credential credential);

    /// <summary>
    /// create
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#create")]
    public extern PromiseResult<Credential?> Create(CredentialCreationOptions? options = default);

    /// <summary>
    /// preventSilentAccess
    /// </summary>
    [Description("@#preventSilentAccess")]
    public extern PromiseResult<object> PreventSilentAccess();
}

/// <summary>
/// PasswordCredential
/// </summary>
[ECMAScript]
[Description("@#PasswordCredential")]
public class PasswordCredential : Credential
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="form">form</param>
    public extern PasswordCredential(HTMLFormElement form);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    public extern PasswordCredential(PasswordCredentialData data);

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; }

    #region mixin CredentialUserData
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// iconURL
    /// </summary>
    [Description("@#iconURL")]
    public extern string IconURL { get; }
    #endregion
}

/// <summary>
/// FederatedCredential
/// </summary>
[ECMAScript]
[Description("@#FederatedCredential")]
public class FederatedCredential : Credential
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="data">data</param>
    public extern FederatedCredential(FederatedCredentialInit data);

    /// <summary>
    /// provider
    /// </summary>
    [Description("@#provider")]
    public extern string Provider { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string? Protocol { get; }

    #region mixin CredentialUserData
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// iconURL
    /// </summary>
    [Description("@#iconURL")]
    public extern string IconURL { get; }
    #endregion
}