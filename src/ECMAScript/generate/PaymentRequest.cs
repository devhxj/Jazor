namespace ECMAScript;

/// <summary>
/// PaymentMethodData
/// </summary>
[ECMAScript]
[Description("@#PaymentMethodData")]
public record PaymentMethodData(
    [property: Description("@#supportedMethods")]string? SupportedMethods = default,
	[property: Description("@#data")]object? Data = default);

/// <summary>
/// PaymentCurrencyAmount
/// </summary>
[ECMAScript]
[Description("@#PaymentCurrencyAmount")]
public record PaymentCurrencyAmount(
    [property: Description("@#currency")]string? Currency = default,
	[property: Description("@#value")]string? Value = default);

/// <summary>
/// PaymentDetailsBase
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsBase")]
public record PaymentDetailsBase(
    [property: Description("@#displayItems")]PaymentItem[]? DisplayItems = default,
	[property: Description("@#shippingOptions")]PaymentShippingOption[]? ShippingOptions = default,
	[property: Description("@#modifiers")]PaymentDetailsModifier[]? Modifiers = default);

/// <summary>
/// PaymentDetailsInit
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsInit")]
public record PaymentDetailsInit(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#total")]PaymentItem? Total = default): PaymentDetailsBase;

/// <summary>
/// PaymentDetailsUpdate
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsUpdate")]
public record PaymentDetailsUpdate(
    [property: Description("@#error")]string? Error = default,
	[property: Description("@#total")]PaymentItem? Total = default,
	[property: Description("@#shippingAddressErrors")]AddressErrors? ShippingAddressErrors = default,
	[property: Description("@#payerErrors")]PayerErrors? PayerErrors = default,
	[property: Description("@#paymentMethodErrors")]object? PaymentMethodErrors = default): PaymentDetailsBase;

/// <summary>
/// PaymentDetailsModifier
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsModifier")]
public record PaymentDetailsModifier(
    [property: Description("@#supportedMethods")]string? SupportedMethods = default,
	[property: Description("@#total")]PaymentItem? Total = default,
	[property: Description("@#additionalDisplayItems")]PaymentItem[]? AdditionalDisplayItems = default,
	[property: Description("@#data")]object? Data = default);

/// <summary>
/// PaymentOptions
/// </summary>
[ECMAScript]
[Description("@#PaymentOptions")]
public record PaymentOptions(
    [property: Description("@#requestPayerName")]bool RequestPayerName = false,
	[property: Description("@#requestBillingAddress")]bool RequestBillingAddress = false,
	[property: Description("@#requestPayerEmail")]bool RequestPayerEmail = false,
	[property: Description("@#requestPayerPhone")]bool RequestPayerPhone = false,
	[property: Description("@#requestShipping")]bool RequestShipping = false,
	[property: Description("@#shippingType")]PaymentShippingType ShippingType = PaymentShippingType.Shipping);

/// <summary>
/// PaymentItem
/// </summary>
[ECMAScript]
[Description("@#PaymentItem")]
public record PaymentItem(
    [property: Description("@#label")]string? Label = default,
	[property: Description("@#amount")]PaymentCurrencyAmount? Amount = default,
	[property: Description("@#pending")]bool Pending = false);

/// <summary>
/// PaymentCompleteDetails
/// </summary>
[ECMAScript]
[Description("@#PaymentCompleteDetails")]
public record PaymentCompleteDetails(
    [property: Description("@#data")]object? Data = null);

/// <summary>
/// PaymentShippingOption
/// </summary>
[ECMAScript]
[Description("@#PaymentShippingOption")]
public record PaymentShippingOption(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#label")]string? Label = default,
	[property: Description("@#amount")]PaymentCurrencyAmount? Amount = default,
	[property: Description("@#selected")]bool Selected = false);

/// <summary>
/// PaymentValidationErrors
/// </summary>
[ECMAScript]
[Description("@#PaymentValidationErrors")]
public record PaymentValidationErrors(
    [property: Description("@#payer")]PayerErrors? Payer = default,
	[property: Description("@#shippingAddress")]AddressErrors? ShippingAddress = default,
	[property: Description("@#error")]string? Error = default,
	[property: Description("@#paymentMethod")]object? PaymentMethod = default);

/// <summary>
/// PayerErrors
/// </summary>
[ECMAScript]
[Description("@#PayerErrors")]
public record PayerErrors(
    [property: Description("@#email")]string? Email = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#phone")]string? Phone = default);

/// <summary>
/// AddressErrors
/// </summary>
[ECMAScript]
[Description("@#AddressErrors")]
public record AddressErrors(
    [property: Description("@#addressLine")]string? AddressLine = default,
	[property: Description("@#city")]string? City = default,
	[property: Description("@#country")]string? Country = default,
	[property: Description("@#dependentLocality")]string? DependentLocality = default,
	[property: Description("@#organization")]string? Organization = default,
	[property: Description("@#phone")]string? Phone = default,
	[property: Description("@#postalCode")]string? PostalCode = default,
	[property: Description("@#recipient")]string? Recipient = default,
	[property: Description("@#region")]string? Region = default,
	[property: Description("@#sortingCode")]string? SortingCode = default);

/// <summary>
/// PaymentMethodChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#PaymentMethodChangeEventInit")]
public record PaymentMethodChangeEventInit(
    [property: Description("@#methodName")]string? MethodName = default,
	[property: Description("@#methodDetails")]object? MethodDetails = null): PaymentRequestUpdateEventInit;

/// <summary>
/// PaymentRequestUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestUpdateEventInit")]
public abstract record PaymentRequestUpdateEventInit();

/// <summary>
/// PaymentRequest
/// </summary>
[ECMAScript]
[Description("@#PaymentRequest")]
public partial class PaymentRequest : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="methodData">methodData</param>
    /// <param name="details">details</param>
    /// <param name="options">options</param>
    public extern PaymentRequest(PaymentMethodData[] methodData, PaymentDetailsInit details, PaymentOptions options);

    /// <summary>
    /// show
    /// </summary>
    /// <param name="detailsPromise">detailsPromise</param>
    [Description("@#show")]
    public extern PromiseResult<PaymentResponse> Show(PromiseResult<PaymentDetailsUpdate> detailsPromise);

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern PromiseResult<object> Abort();

    /// <summary>
    /// canMakePayment
    /// </summary>
    [Description("@#canMakePayment")]
    public extern PromiseResult<bool> CanMakePayment();

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// shippingAddress
    /// </summary>
    [Description("@#shippingAddress")]
    public extern ContactAddress? ShippingAddress { get; }

    /// <summary>
    /// shippingOption
    /// </summary>
    [Description("@#shippingOption")]
    public extern string? ShippingOption { get; }

    /// <summary>
    /// shippingType
    /// </summary>
    [Description("@#shippingType")]
    public extern PaymentShippingType? ShippingType { get; }

    /// <summary>
    /// onshippingaddresschange
    /// </summary>
    [Description("@#onshippingaddresschange")]
    public extern EventHandler Onshippingaddresschange { get; set; }

    /// <summary>
    /// onshippingoptionchange
    /// </summary>
    [Description("@#onshippingoptionchange")]
    public extern EventHandler Onshippingoptionchange { get; set; }

    /// <summary>
    /// onpaymentmethodchange
    /// </summary>
    [Description("@#onpaymentmethodchange")]
    public extern EventHandler Onpaymentmethodchange { get; set; }

    /// <summary>
    /// securePaymentConfirmationAvailability
    /// </summary>
    [Description("@#securePaymentConfirmationAvailability")]
    public extern static PromiseResult<SecurePaymentConfirmationAvailability> SecurePaymentConfirmationAvailability();
}

/// <summary>
/// PaymentResponse
/// </summary>
[ECMAScript]
[Description("@#PaymentResponse")]
public class PaymentResponse : EventTarget
{
    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();

    /// <summary>
    /// requestId
    /// </summary>
    [Description("@#requestId")]
    public extern string RequestId { get; }

    /// <summary>
    /// methodName
    /// </summary>
    [Description("@#methodName")]
    public extern string MethodName { get; }

    /// <summary>
    /// details
    /// </summary>
    [Description("@#details")]
    public extern object Details { get; }

    /// <summary>
    /// shippingAddress
    /// </summary>
    [Description("@#shippingAddress")]
    public extern ContactAddress? ShippingAddress { get; }

    /// <summary>
    /// shippingOption
    /// </summary>
    [Description("@#shippingOption")]
    public extern string? ShippingOption { get; }

    /// <summary>
    /// payerName
    /// </summary>
    [Description("@#payerName")]
    public extern string? PayerName { get; }

    /// <summary>
    /// payerEmail
    /// </summary>
    [Description("@#payerEmail")]
    public extern string? PayerEmail { get; }

    /// <summary>
    /// payerPhone
    /// </summary>
    [Description("@#payerPhone")]
    public extern string? PayerPhone { get; }

    /// <summary>
    /// complete
    /// </summary>
    /// <param name="result">result</param>
    /// <param name="details">details</param>
    [Description("@#complete")]
    public extern PromiseResult<object> Complete(PaymentComplete result = PaymentComplete.Unknown, PaymentCompleteDetails? details = default);

    /// <summary>
    /// retry
    /// </summary>
    /// <param name="errorFields">errorFields</param>
    [Description("@#retry")]
    public extern PromiseResult<object> Retry(PaymentValidationErrors? errorFields = default);

    /// <summary>
    /// onpayerdetailchange
    /// </summary>
    [Description("@#onpayerdetailchange")]
    public extern EventHandler Onpayerdetailchange { get; set; }
}

/// <summary>
/// PaymentMethodChangeEvent
/// </summary>
[ECMAScript]
[Description("@#PaymentMethodChangeEvent")]
public class PaymentMethodChangeEvent(string type, PaymentRequestUpdateEventInit eventInitDict) : PaymentRequestUpdateEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PaymentMethodChangeEvent(string type, PaymentMethodChangeEventInit eventInitDict);

    /// <summary>
    /// methodName
    /// </summary>
    [Description("@#methodName")]
    public extern string MethodName { get; }

    /// <summary>
    /// methodDetails
    /// </summary>
    [Description("@#methodDetails")]
    public extern object? MethodDetails { get; }
}

/// <summary>
/// PaymentRequestUpdateEvent
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestUpdateEvent")]
public class PaymentRequestUpdateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PaymentRequestUpdateEvent(string type, PaymentRequestUpdateEventInit eventInitDict);

    /// <summary>
    /// updateWith
    /// </summary>
    /// <param name="detailsPromise">detailsPromise</param>
    [Description("@#updateWith")]
    public extern void UpdateWith(PromiseResult<PaymentDetailsUpdate> detailsPromise);
}