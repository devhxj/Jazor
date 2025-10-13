namespace ECMAScript;

/// <summary>
/// IdentityCredentialDisconnectOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityCredentialDisconnectOptions")]
public record IdentityCredentialDisconnectOptions(
    [property: Description("@#accountHint")]string? AccountHint = default): IdentityProviderConfig;

/// <summary>
/// DisconnectedAccount
/// </summary>
[ECMAScript]
[Description("@#DisconnectedAccount")]
public record DisconnectedAccount(
    [property: Description("@#account_id")]string? AccountId = default);

/// <summary>
/// IdentityCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityCredentialRequestOptions")]
public record IdentityCredentialRequestOptions(
    [property: Description("@#providers")]IdentityProviderRequestOptions[]? Providers = default,
	[property: Description("@#context")]IdentityCredentialRequestOptionsContext Context = IdentityCredentialRequestOptionsContext.Signin,
	[property: Description("@#mode")]IdentityCredentialRequestOptionsMode Mode = IdentityCredentialRequestOptionsMode.Passive);

/// <summary>
/// IdentityProviderConfig
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderConfig")]
public record IdentityProviderConfig(
    [property: Description("@#configURL")]string? ConfigURL = default,
	[property: Description("@#clientId")]string? ClientId = default);

/// <summary>
/// IdentityProviderRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderRequestOptions")]
public record IdentityProviderRequestOptions(
    [property: Description("@#nonce")]string? Nonce = default,
	[property: Description("@#loginHint")]string? LoginHint = default,
	[property: Description("@#domainHint")]string? DomainHint = default,
	[property: Description("@#fields")]string[]? Fields = default,
	[property: Description("@#params")]object? Params = default): IdentityProviderConfig;

/// <summary>
/// IdentityProviderWellKnown
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderWellKnown")]
public record IdentityProviderWellKnown(
    [property: Description("@#provider_urls")]string[]? ProviderUrls = default,
	[property: Description("@#accounts_endpoint")]string? AccountsEndpoint = default,
	[property: Description("@#login_url")]string? LoginUrl = default);

/// <summary>
/// IdentityProviderIcon
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderIcon")]
public record IdentityProviderIcon(
    [property: Description("@#url")]string? Url = default,
	[property: Description("@#size")]uint Size = default);

/// <summary>
/// IdentityProviderBranding
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderBranding")]
public record IdentityProviderBranding(
    [property: Description("@#background_color")]string? BackgroundColor = default,
	[property: Description("@#color")]string? Color = default,
	[property: Description("@#icons")]IdentityProviderIcon[]? Icons = default,
	[property: Description("@#name")]string? Name = default);

/// <summary>
/// IdentityProviderAPIConfig
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderAPIConfig")]
public record IdentityProviderAPIConfig(
    [property: Description("@#accounts_endpoint")]string? AccountsEndpoint = default,
	[property: Description("@#client_metadata_endpoint")]string? ClientMetadataEndpoint = default,
	[property: Description("@#id_assertion_endpoint")]string? IdAssertionEndpoint = default,
	[property: Description("@#login_url")]string? LoginUrl = default,
	[property: Description("@#disconnect_endpoint")]string? DisconnectEndpoint = default,
	[property: Description("@#branding")]IdentityProviderBranding? Branding = default,
	[property: Description("@#supports_use_other_account")]bool SupportsUseOtherAccount = false,
	[property: Description("@#account_label")]string? AccountLabel = default);

/// <summary>
/// IdentityProviderAccount
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderAccount")]
public record IdentityProviderAccount(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#email")]string? Email = default,
	[property: Description("@#tel")]string? Tel = default,
	[property: Description("@#username")]string? Username = default,
	[property: Description("@#given_name")]string? GivenName = default,
	[property: Description("@#picture")]string? Picture = default,
	[property: Description("@#approved_clients")]string[]? ApprovedClients = default,
	[property: Description("@#login_hints")]string[]? LoginHints = default,
	[property: Description("@#domain_hints")]string[]? DomainHints = default,
	[property: Description("@#label_hints")]string[]? LabelHints = default);

/// <summary>
/// IdentityProviderAccountList
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderAccountList")]
public record IdentityProviderAccountList(
    [property: Description("@#accounts")]IdentityProviderAccount[]? Accounts = default);

/// <summary>
/// IdentityAssertionResponse
/// </summary>
[ECMAScript]
[Description("@#IdentityAssertionResponse")]
public record IdentityAssertionResponse(
    [property: Description("@#token")]string? Token = default,
	[property: Description("@#continue_on")]string? ContinueOn = default);

/// <summary>
/// IdentityProviderClientMetadata
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderClientMetadata")]
public record IdentityProviderClientMetadata(
    [property: Description("@#privacy_policy_url")]string? PrivacyPolicyUrl = default,
	[property: Description("@#terms_of_service_url")]string? TermsOfServiceUrl = default);

/// <summary>
/// IdentityUserInfo
/// </summary>
[ECMAScript]
[Description("@#IdentityUserInfo")]
public record IdentityUserInfo(
    [property: Description("@#email")]string? Email = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#givenName")]string? GivenName = default,
	[property: Description("@#picture")]string? Picture = default);

/// <summary>
/// IdentityResolveOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityResolveOptions")]
public record IdentityResolveOptions(
    [property: Description("@#accountId")]string? AccountId = default);

/// <summary>
/// IdentityCredential
/// </summary>
[ECMAScript]
[Description("@#IdentityCredential")]
public class IdentityCredential : Credential
{
    /// <summary>
    /// disconnect
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#disconnect")]
    public extern static PromiseResult<object> Disconnect(IdentityCredentialDisconnectOptions options);

    /// <summary>
    /// token
    /// </summary>
    [Description("@#token")]
    public extern string? Token { get; }

    /// <summary>
    /// isAutoSelected
    /// </summary>
    [Description("@#isAutoSelected")]
    public extern bool IsAutoSelected { get; }

    /// <summary>
    /// configURL
    /// </summary>
    [Description("@#configURL")]
    public extern string ConfigURL { get; }
}

/// <summary>
/// IdentityProvider
/// </summary>
[ECMAScript]
[Description("@#IdentityProvider")]
public class IdentityProvider
{
    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern static void Close();

    /// <summary>
    /// resolve
    /// </summary>
    /// <param name="token">token</param>
    /// <param name="options">options</param>
    [Description("@#resolve")]
    public extern static PromiseResult<object> Resolve(string token, IdentityResolveOptions? options = default);

    /// <summary>
    /// getUserInfo
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#getUserInfo")]
    public extern static PromiseResult<IdentityUserInfo[]> GetUserInfo(IdentityProviderConfig config);
}