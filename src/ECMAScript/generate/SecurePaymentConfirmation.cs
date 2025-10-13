namespace ECMAScript;

/// <summary>
/// SecurePaymentConfirmationRequest
/// </summary>
[ECMAScript]
[Description("@#SecurePaymentConfirmationRequest")]
public record SecurePaymentConfirmationRequest(
    [property: Description("@#challenge")]IBufferSource? Challenge = default,
	[property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#credentialIds")]IBufferSource[]? CredentialIds = default,
	[property: Description("@#instrument")]PaymentCredentialInstrument? Instrument = default,
	[property: Description("@#timeout")]uint Timeout = default,
	[property: Description("@#payeeName")]string? PayeeName = default,
	[property: Description("@#payeeOrigin")]string? PayeeOrigin = default,
	[property: Description("@#paymentEntitiesLogos")]PaymentEntityLogo[]? PaymentEntitiesLogos = default,
	[property: Description("@#extensions")]AuthenticationExtensionsClientInputs? Extensions = default,
	[property: Description("@#browserBoundPubKeyCredParams")]PublicKeyCredentialParameters[]? BrowserBoundPubKeyCredParams = default,
	[property: Description("@#locale")]string[]? Locale = default,
	[property: Description("@#showOptOut")]bool ShowOptOut = default);

/// <summary>
/// AuthenticationExtensionsPaymentInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPaymentInputs")]
public record AuthenticationExtensionsPaymentInputs(
    [property: Description("@#isPayment")]bool IsPayment = default,
	[property: Description("@#browserBoundPubKeyCredParams")]PublicKeyCredentialParameters[]? BrowserBoundPubKeyCredParams = default,
	[property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#topOrigin")]string? TopOrigin = default,
	[property: Description("@#payeeName")]string? PayeeName = default,
	[property: Description("@#payeeOrigin")]string? PayeeOrigin = default,
	[property: Description("@#paymentEntitiesLogos")]PaymentEntityLogo[]? PaymentEntitiesLogos = default,
	[property: Description("@#total")]PaymentCurrencyAmount? Total = default,
	[property: Description("@#instrument")]PaymentCredentialInstrument? Instrument = default);

/// <summary>
/// AuthenticationExtensionsPaymentOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPaymentOutputs")]
public record AuthenticationExtensionsPaymentOutputs(
    [property: Description("@#browserBoundSignature")]BrowserBoundSignature? BrowserBoundSignature = default);

/// <summary>
/// BrowserBoundSignature
/// </summary>
[ECMAScript]
[Description("@#BrowserBoundSignature")]
public record BrowserBoundSignature(
    [property: Description("@#signature")]ArrayBuffer? Signature = default);

/// <summary>
/// CollectedClientPaymentData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientPaymentData")]
public record CollectedClientPaymentData(
    [property: Description("@#payment")]Either<CollectedClientAdditionalPaymentData, CollectedClientAdditionalPaymentRegistrationData>? Payment = default): CollectedClientData;

/// <summary>
/// CollectedClientAdditionalPaymentData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientAdditionalPaymentData")]
public record CollectedClientAdditionalPaymentData(
    [property: Description("@#rpId")]string? RpId = default,
	[property: Description("@#topOrigin")]string? TopOrigin = default,
	[property: Description("@#payeeName")]string? PayeeName = default,
	[property: Description("@#payeeOrigin")]string? PayeeOrigin = default,
	[property: Description("@#paymentEntitiesLogos")]PaymentEntityLogo[]? PaymentEntitiesLogos = default,
	[property: Description("@#total")]PaymentCurrencyAmount? Total = default,
	[property: Description("@#instrument")]PaymentCredentialInstrument? Instrument = default,
	[property: Description("@#browserBoundPublicKey")]string? BrowserBoundPublicKey = default);

/// <summary>
/// CollectedClientAdditionalPaymentRegistrationData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientAdditionalPaymentRegistrationData")]
public record CollectedClientAdditionalPaymentRegistrationData(
    [property: Description("@#browserBoundPublicKey")]string? BrowserBoundPublicKey = default);

/// <summary>
/// PaymentCredentialInstrument
/// </summary>
[ECMAScript]
[Description("@#PaymentCredentialInstrument")]
public record PaymentCredentialInstrument(
    [property: Description("@#displayName")]string? DisplayName = default,
	[property: Description("@#icon")]string? Icon = default,
	[property: Description("@#iconMustBeShown")]bool IconMustBeShown = true,
	[property: Description("@#details")]string? Details = default);

/// <summary>
/// PaymentEntityLogo
/// </summary>
[ECMAScript]
[Description("@#PaymentEntityLogo")]
public record PaymentEntityLogo(
    [property: Description("@#url")]string? Url = default,
	[property: Description("@#label")]string? Label = default);