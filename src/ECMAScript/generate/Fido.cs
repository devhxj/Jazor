namespace ECMAScript;

/// <summary>
/// AuthenticationExtensionsClientInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientInputs")]
public record AuthenticationExtensionsClientInputs(
    [property: Description("@#credentialProtectionPolicy")]string? CredentialProtectionPolicy = default,
	[property: Description("@#enforceCredentialProtectionPolicy")]bool EnforceCredentialProtectionPolicy = false,
	[property: Description("@#credBlob")]ArrayBuffer? CredBlob = default,
	[property: Description("@#getCredBlob")]bool GetCredBlob = default,
	[property: Description("@#minPinLength")]bool MinPinLength = default,
	[property: Description("@#hmacCreateSecret")]bool HmacCreateSecret = default,
	[property: Description("@#hmacGetSecret")]HMACGetSecretInput? HmacGetSecret = default,
	[property: Description("@#payment")]AuthenticationExtensionsPaymentInputs? Payment = default,
	[property: Description("@#appid")]string? Appid = default,
	[property: Description("@#appidExclude")]string? AppidExclude = default,
	[property: Description("@#credProps")]bool CredProps = default,
	[property: Description("@#prf")]AuthenticationExtensionsPRFInputs? Prf = default,
	[property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobInputs? LargeBlob = default)
{
    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalCredentialProtectionPolicyEnforceCredentialProtectionPolicy(
        [Description("@#credentialProtectionPolicy")]string? CredentialProtectionPolicy = default,
        [Description("@#enforceCredentialProtectionPolicy")]bool enforceCredentialProtectionPolicy = false);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalCredBlob(
        [Description("@#credBlob")]ArrayBuffer? CredBlob = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalGetCredBlob(
        [Description("@#getCredBlob")]bool GetCredBlob = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalMinPinLength(
        [Description("@#minPinLength")]bool MinPinLength = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalHmacCreateSecret(
        [Description("@#hmacCreateSecret")]bool HmacCreateSecret = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalHmacGetSecret(
        [Description("@#hmacGetSecret")]HMACGetSecretInput? HmacGetSecret = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalPayment(
        [Description("@#payment")]AuthenticationExtensionsPaymentInputs? Payment = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs Optional();

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalAppid(
        [Description("@#appid")]string? Appid = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalAppidExclude(
        [Description("@#appidExclude")]string? AppidExclude = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalCredProps(
        [Description("@#credProps")]bool CredProps = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalPrf(
        [Description("@#prf")]AuthenticationExtensionsPRFInputs? Prf = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalLargeBlob(
        [Description("@#largeBlob")]AuthenticationExtensionsLargeBlobInputs? LargeBlob = default);
}

/// <summary>
/// HMACGetSecretInput
/// </summary>
[ECMAScript]
[Description("@#HMACGetSecretInput")]
public record HMACGetSecretInput(
    [property: Description("@#salt1")]ArrayBuffer? Salt1 = default,
	[property: Description("@#salt2")]ArrayBuffer? Salt2 = default);

/// <summary>
/// AuthenticationExtensionsClientOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientOutputs")]
public record AuthenticationExtensionsClientOutputs(
    [property: Description("@#hmacCreateSecret")]bool HmacCreateSecret = default,
	[property: Description("@#hmacGetSecret")]HMACGetSecretOutput? HmacGetSecret = default,
	[property: Description("@#payment")]AuthenticationExtensionsPaymentOutputs? Payment = default,
	[property: Description("@#appid")]bool Appid = default,
	[property: Description("@#appidExclude")]bool AppidExclude = default,
	[property: Description("@#credProps")]CredentialPropertiesOutput? CredProps = default,
	[property: Description("@#prf")]AuthenticationExtensionsPRFOutputs? Prf = default,
	[property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobOutputs? LargeBlob = default)
{
    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalHmacCreateSecret(
        [Description("@#hmacCreateSecret")]bool HmacCreateSecret = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalHmacGetSecret(
        [Description("@#hmacGetSecret")]HMACGetSecretOutput? HmacGetSecret = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalPayment(
        [Description("@#payment")]AuthenticationExtensionsPaymentOutputs? Payment = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs Optional();

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalAppid(
        [Description("@#appid")]bool Appid = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalAppidExclude(
        [Description("@#appidExclude")]bool AppidExclude = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalCredProps(
        [Description("@#credProps")]CredentialPropertiesOutput? CredProps = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalPrf(
        [Description("@#prf")]AuthenticationExtensionsPRFOutputs? Prf = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalLargeBlob(
        [Description("@#largeBlob")]AuthenticationExtensionsLargeBlobOutputs? LargeBlob = default);
}

/// <summary>
/// HMACGetSecretOutput
/// </summary>
[ECMAScript]
[Description("@#HMACGetSecretOutput")]
public record HMACGetSecretOutput(
    [property: Description("@#output1")]ArrayBuffer? Output1 = default,
	[property: Description("@#output2")]ArrayBuffer? Output2 = default);