namespace ECMAScript;

/// <summary>
/// PaymentRequestDetailsUpdate
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestDetailsUpdate")]
public record PaymentRequestDetailsUpdate(
    [property: Description("@#error")]string? Error = default,
	[property: Description("@#total")]PaymentCurrencyAmount? Total = default,
	[property: Description("@#modifiers")]PaymentDetailsModifier[]? Modifiers = default,
	[property: Description("@#shippingOptions")]PaymentShippingOption[]? ShippingOptions = default,
	[property: Description("@#paymentMethodErrors")]object? PaymentMethodErrors = default,
	[property: Description("@#shippingAddressErrors")]AddressErrors? ShippingAddressErrors = default);

/// <summary>
/// PaymentRequestEventInit
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestEventInit")]
public record PaymentRequestEventInit(
    [property: Description("@#topOrigin")]string? TopOrigin = default,
	[property: Description("@#paymentRequestOrigin")]string? PaymentRequestOrigin = default,
	[property: Description("@#paymentRequestId")]string? PaymentRequestId = default,
	[property: Description("@#methodData")]PaymentMethodData[]? MethodData = default,
	[property: Description("@#total")]PaymentCurrencyAmount? Total = default,
	[property: Description("@#modifiers")]PaymentDetailsModifier[]? Modifiers = default,
	[property: Description("@#paymentOptions")]PaymentOptions? PaymentOptions = default,
	[property: Description("@#shippingOptions")]PaymentShippingOption[]? ShippingOptions = default): ExtendableEventInit;

/// <summary>
/// PaymentHandlerResponse
/// </summary>
[ECMAScript]
[Description("@#PaymentHandlerResponse")]
public record PaymentHandlerResponse(
    [property: Description("@#methodName")]string? MethodName = default,
	[property: Description("@#details")]object? Details = default,
	[property: Description("@#payerName")]string? PayerName = default,
	[property: Description("@#payerEmail")]string? PayerEmail = default,
	[property: Description("@#payerPhone")]string? PayerPhone = default,
	[property: Description("@#shippingAddress")]AddressInit? ShippingAddress = default,
	[property: Description("@#shippingOption")]string? ShippingOption = default);

/// <summary>
/// AddressInit
/// </summary>
[ECMAScript]
[Description("@#AddressInit")]
public record AddressInit(
    [property: Description("@#country")]string? Country = default,
	[property: Description("@#addressLine")]string[]? AddressLine = default,
	[property: Description("@#region")]string? Region = default,
	[property: Description("@#city")]string? City = default,
	[property: Description("@#dependentLocality")]string? DependentLocality = default,
	[property: Description("@#postalCode")]string? PostalCode = default,
	[property: Description("@#sortingCode")]string? SortingCode = default,
	[property: Description("@#organization")]string? Organization = default,
	[property: Description("@#recipient")]string? Recipient = default,
	[property: Description("@#phone")]string? Phone = default);

/// <summary>
/// PaymentManager
/// </summary>
[ECMAScript]
[Description("@#PaymentManager")]
public class PaymentManager
{
    /// <summary>
    /// userHint
    /// </summary>
    [Description("@#userHint")]
    public extern string UserHint { get; set; }

    /// <summary>
    /// enableDelegations
    /// </summary>
    /// <param name="delegations">delegations</param>
    [Description("@#enableDelegations")]
    public extern PromiseResult<object> EnableDelegations(PaymentDelegation[] delegations);
}

/// <summary>
/// CanMakePaymentEvent
/// </summary>
[ECMAScript]
[Description("@#CanMakePaymentEvent")]
public class CanMakePaymentEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    public extern CanMakePaymentEvent(string type);

    /// <summary>
    /// respondWith
    /// </summary>
    /// <param name="canMakePaymentResponse">canMakePaymentResponse</param>
    [Description("@#respondWith")]
    public extern void RespondWith(PromiseResult<bool> canMakePaymentResponse);
}

/// <summary>
/// PaymentRequestEvent
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestEvent")]
public class PaymentRequestEvent(string type, ExtendableEventInit eventInitDict) : ExtendableEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PaymentRequestEvent(string type, PaymentRequestEventInit eventInitDict);

    /// <summary>
    /// topOrigin
    /// </summary>
    [Description("@#topOrigin")]
    public extern string TopOrigin { get; }

    /// <summary>
    /// paymentRequestOrigin
    /// </summary>
    [Description("@#paymentRequestOrigin")]
    public extern string PaymentRequestOrigin { get; }

    /// <summary>
    /// paymentRequestId
    /// </summary>
    [Description("@#paymentRequestId")]
    public extern string PaymentRequestId { get; }

    /// <summary>
    /// methodData
    /// </summary>
    [Description("@#methodData")]
    public extern FrozenSet<PaymentMethodData> MethodData { get; }

    /// <summary>
    /// total
    /// </summary>
    [Description("@#total")]
    public extern object Total { get; }

    /// <summary>
    /// modifiers
    /// </summary>
    [Description("@#modifiers")]
    public extern FrozenSet<PaymentDetailsModifier> Modifiers { get; }

    /// <summary>
    /// paymentOptions
    /// </summary>
    [Description("@#paymentOptions")]
    public extern object? PaymentOptions { get; }

    /// <summary>
    /// shippingOptions
    /// </summary>
    [Description("@#shippingOptions")]
    public extern FrozenSet<PaymentShippingOption>? ShippingOptions { get; }

    /// <summary>
    /// openWindow
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#openWindow")]
    public extern PromiseResult<WindowClient?> OpenWindow(string url);

    /// <summary>
    /// changePaymentMethod
    /// </summary>
    /// <param name="methodName">methodName</param>
    /// <param name="methodDetails">methodDetails</param>
    [Description("@#changePaymentMethod")]
    public extern PromiseResult<PaymentRequestDetailsUpdate?> ChangePaymentMethod(string methodName, object? methodDetails = null);

    /// <summary>
    /// changeShippingAddress
    /// </summary>
    /// <param name="shippingAddress">shippingAddress</param>
    [Description("@#changeShippingAddress")]
    public extern PromiseResult<PaymentRequestDetailsUpdate?> ChangeShippingAddress(AddressInit? shippingAddress = default);

    /// <summary>
    /// changeShippingOption
    /// </summary>
    /// <param name="shippingOption">shippingOption</param>
    [Description("@#changeShippingOption")]
    public extern PromiseResult<PaymentRequestDetailsUpdate?> ChangeShippingOption(string shippingOption);

    /// <summary>
    /// respondWith
    /// </summary>
    /// <param name="handlerResponsePromise">handlerResponsePromise</param>
    [Description("@#respondWith")]
    public extern void RespondWith(PromiseResult<PaymentHandlerResponse> handlerResponsePromise);
}