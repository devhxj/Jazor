namespace ECMAScript;

/// <summary>
/// ItemDetails
/// </summary>
[ECMAScript]
[Description("@#ItemDetails")]
public record ItemDetails(
    [property: Description("@#itemId")]string? ItemId = default,
	[property: Description("@#title")]string? Title = default,
	[property: Description("@#price")]PaymentCurrencyAmount? Price = default,
	[property: Description("@#type")]ItemType? Type = default,
	[property: Description("@#description")]string? Description = default,
	[property: Description("@#iconURLs")]string[]? IconURLs = default,
	[property: Description("@#subscriptionPeriod")]string? SubscriptionPeriod = default,
	[property: Description("@#freeTrialPeriod")]string? FreeTrialPeriod = default,
	[property: Description("@#introductoryPrice")]PaymentCurrencyAmount? IntroductoryPrice = default,
	[property: Description("@#introductoryPricePeriod")]string? IntroductoryPricePeriod = default,
	[property: Description("@#introductoryPriceCycles")]ulong IntroductoryPriceCycles = default);

/// <summary>
/// PurchaseDetails
/// </summary>
[ECMAScript]
[Description("@#PurchaseDetails")]
public record PurchaseDetails(
    [property: Description("@#itemId")]string? ItemId = default,
	[property: Description("@#purchaseToken")]string? PurchaseToken = default);

/// <summary>
/// DigitalGoodsService
/// </summary>
[ECMAScript]
[Description("@#DigitalGoodsService")]
public class DigitalGoodsService
{
    /// <summary>
    /// getDetails
    /// </summary>
    /// <param name="itemIds">itemIds</param>
    [Description("@#getDetails")]
    public extern PromiseResult<ItemDetails[]> GetDetails(string[] itemIds);

    /// <summary>
    /// listPurchases
    /// </summary>
    [Description("@#listPurchases")]
    public extern PromiseResult<PurchaseDetails[]> ListPurchases();

    /// <summary>
    /// listPurchaseHistory
    /// </summary>
    [Description("@#listPurchaseHistory")]
    public extern PromiseResult<PurchaseDetails[]> ListPurchaseHistory();

    /// <summary>
    /// consume
    /// </summary>
    /// <param name="purchaseToken">purchaseToken</param>
    [Description("@#consume")]
    public extern PromiseResult<object> Consume(string purchaseToken);
}