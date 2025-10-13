namespace ECMAScript;

/// <summary>
/// AuctionAd
/// </summary>
[ECMAScript]
[Description("@#AuctionAd")]
public record AuctionAd(
    [property: Description("@#renderURL")]string? RenderURL = default,
	[property: Description("@#sizeGroup")]string? SizeGroup = default,
	[property: Description("@#metadata")]object? Metadata = default,
	[property: Description("@#buyerReportingId")]string? BuyerReportingId = default,
	[property: Description("@#buyerAndSellerReportingId")]string? BuyerAndSellerReportingId = default,
	[property: Description("@#selectableBuyerAndSellerReportingIds")]string[]? SelectableBuyerAndSellerReportingIds = default,
	[property: Description("@#allowedReportingOrigins")]string[]? AllowedReportingOrigins = default,
	[property: Description("@#adRenderId")]string? AdRenderId = default,
	[property: Description("@#creativeScanningMetadata")]string? CreativeScanningMetadata = default);

/// <summary>
/// AuctionAdInterestGroupSize
/// </summary>
[ECMAScript]
[Description("@#AuctionAdInterestGroupSize")]
public record AuctionAdInterestGroupSize(
    [property: Description("@#width")]string? Width = default,
	[property: Description("@#height")]string? Height = default);

/// <summary>
/// GenerateBidInterestGroup
/// </summary>
[ECMAScript]
[Description("@#GenerateBidInterestGroup")]
public record GenerateBidInterestGroup(
    [property: Description("@#owner")]string? Owner = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#enableBiddingSignalsPrioritization")]bool EnableBiddingSignalsPrioritization = false,
	[property: Description("@#priorityVector")]Dictionary<string, double>? PriorityVector = default,
	[property: Description("@#sellerCapabilities")]Dictionary<string, string[]>? SellerCapabilities = default,
	[property: Description("@#executionMode")]string? ExecutionMode = default,
	[property: Description("@#biddingLogicURL")]string? BiddingLogicURL = default,
	[property: Description("@#biddingWasmHelperURL")]string? BiddingWasmHelperURL = default,
	[property: Description("@#updateURL")]string? UpdateURL = default,
	[property: Description("@#trustedBiddingSignalsURL")]string? TrustedBiddingSignalsURL = default,
	[property: Description("@#trustedBiddingSignalsKeys")]string[]? TrustedBiddingSignalsKeys = default,
	[property: Description("@#trustedBiddingSignalsSlotSizeMode")]string? TrustedBiddingSignalsSlotSizeMode = default,
	[property: Description("@#maxTrustedBiddingSignalsURLLength")]int MaxTrustedBiddingSignalsURLLength = default,
	[property: Description("@#trustedBiddingSignalsCoordinator")]string? TrustedBiddingSignalsCoordinator = default,
	[property: Description("@#userBiddingSignals")]object? UserBiddingSignals = default,
	[property: Description("@#ads")]AuctionAd[]? Ads = default,
	[property: Description("@#adComponents")]AuctionAd[]? AdComponents = default,
	[property: Description("@#adSizes")]Dictionary<string, AuctionAdInterestGroupSize>? AdSizes = default,
	[property: Description("@#sizeGroups")]Dictionary<string, string[]>? SizeGroups = default);

/// <summary>
/// ProtectedAudiencePrivateAggregationConfig
/// </summary>
[ECMAScript]
[Description("@#ProtectedAudiencePrivateAggregationConfig")]
public record ProtectedAudiencePrivateAggregationConfig(
    [property: Description("@#aggregationCoordinatorOrigin")]string? AggregationCoordinatorOrigin = default);

/// <summary>
/// AuctionAdInterestGroup
/// </summary>
[ECMAScript]
[Description("@#AuctionAdInterestGroup")]
public record AuctionAdInterestGroup(
    [property: Description("@#priority")]double Priority = 0.0d,
	[property: Description("@#prioritySignalsOverrides")]Dictionary<string, double>? PrioritySignalsOverrides = default,
	[property: Description("@#lifetimeMs")]double LifetimeMs = default,
	[property: Description("@#additionalBidKey")]string? AdditionalBidKey = default,
	[property: Description("@#privateAggregationConfig")]ProtectedAudiencePrivateAggregationConfig? PrivateAggregationConfig = default): GenerateBidInterestGroup;

/// <summary>
/// AuctionAdInterestGroupKey
/// </summary>
[ECMAScript]
[Description("@#AuctionAdInterestGroupKey")]
public record AuctionAdInterestGroupKey(
    [property: Description("@#owner")]string? Owner = default,
	[property: Description("@#name")]string? Name = default);

/// <summary>
/// AuctionReportBuyersConfig
/// </summary>
[ECMAScript]
[Description("@#AuctionReportBuyersConfig")]
public record AuctionReportBuyersConfig(
    [property: Description("@#bucket")]BigInteger? Bucket = default,
	[property: Description("@#scale")]double Scale = default);

/// <summary>
/// AuctionReportBuyerDebugModeConfig
/// </summary>
[ECMAScript]
[Description("@#AuctionReportBuyerDebugModeConfig")]
public record AuctionReportBuyerDebugModeConfig(
    [property: Description("@#enabled")]bool Enabled = false,
	[property: Description("@#debugKey")]BigInteger? DebugKey = default);

/// <summary>
/// AuctionRealTimeReportingConfig
/// </summary>
[ECMAScript]
[Description("@#AuctionRealTimeReportingConfig")]
public record AuctionRealTimeReportingConfig(
    [property: Description("@#type")]string? Type = default);

/// <summary>
/// AuctionAdConfig
/// </summary>
[ECMAScript]
[Description("@#AuctionAdConfig")]
public record AuctionAdConfig(
    [property: Description("@#seller")]string? Seller = default,
	[property: Description("@#decisionLogicURL")]string? DecisionLogicURL = default,
	[property: Description("@#trustedScoringSignalsURL")]string? TrustedScoringSignalsURL = default,
	[property: Description("@#maxTrustedScoringSignalsURLLength")]int MaxTrustedScoringSignalsURLLength = default,
	[property: Description("@#trustedScoringSignalsCoordinator")]string? TrustedScoringSignalsCoordinator = default,
	[property: Description("@#sendCreativeScanningMetadata")]bool SendCreativeScanningMetadata = default,
	[property: Description("@#interestGroupBuyers")]string[]? InterestGroupBuyers = default,
	[property: Description("@#auctionSignals")]PromiseResult<object>? AuctionSignals = default,
	[property: Description("@#sellerSignals")]PromiseResult<object>? SellerSignals = default,
	[property: Description("@#directFromSellerSignalsHeaderAdSlot")]PromiseResult<string?>? DirectFromSellerSignalsHeaderAdSlot = default,
	[property: Description("@#deprecatedRenderURLReplacements")]PromiseResult<Dictionary<string, string>?>? DeprecatedRenderURLReplacements = default,
	[property: Description("@#sellerTimeout")]ulong SellerTimeout = default,
	[property: Description("@#sellerExperimentGroupId")]ushort SellerExperimentGroupId = default,
	[property: Description("@#perBuyerSignals")]PromiseResult<Dictionary<string, object>?>? PerBuyerSignals = default,
	[property: Description("@#perBuyerTimeouts")]PromiseResult<Dictionary<string, ulong>?>? PerBuyerTimeouts = default,
	[property: Description("@#perBuyerCumulativeTimeouts")]PromiseResult<Dictionary<string, ulong>?>? PerBuyerCumulativeTimeouts = default,
	[property: Description("@#reportingTimeout")]ulong ReportingTimeout = default,
	[property: Description("@#sellerCurrency")]string? SellerCurrency = default,
	[property: Description("@#perBuyerCurrencies")]PromiseResult<Dictionary<string, string>?>? PerBuyerCurrencies = default,
	[property: Description("@#perBuyerMultiBidLimits")]Dictionary<string, ushort>? PerBuyerMultiBidLimits = default,
	[property: Description("@#perBuyerGroupLimits")]Dictionary<string, ushort>? PerBuyerGroupLimits = default,
	[property: Description("@#perBuyerExperimentGroupIds")]Dictionary<string, ushort>? PerBuyerExperimentGroupIds = default,
	[property: Description("@#perBuyerPrioritySignals")]Dictionary<string, Dictionary<string, double>>? PerBuyerPrioritySignals = default,
	[property: Description("@#auctionReportBuyerKeys")]BigInteger[]? AuctionReportBuyerKeys = default,
	[property: Description("@#auctionReportBuyers")]Dictionary<string, AuctionReportBuyersConfig>? AuctionReportBuyers = default,
	[property: Description("@#auctionReportBuyerDebugModeConfig")]AuctionReportBuyerDebugModeConfig? AuctionReportBuyerDebugModeConfig = default,
	[property: Description("@#requiredSellerCapabilities")]string[]? RequiredSellerCapabilities = default,
	[property: Description("@#privateAggregationConfig")]ProtectedAudiencePrivateAggregationConfig? PrivateAggregationConfig = default,
	[property: Description("@#requestedSize")]Dictionary<string, string>? RequestedSize = default,
	[property: Description("@#allSlotsRequestedSizes")]Dictionary<string, string>[]? AllSlotsRequestedSizes = default,
	[property: Description("@#additionalBids")]PromiseResult<object>? AdditionalBids = default,
	[property: Description("@#auctionNonce")]string? AuctionNonce = default,
	[property: Description("@#sellerRealTimeReportingConfig")]AuctionRealTimeReportingConfig? SellerRealTimeReportingConfig = default,
	[property: Description("@#perBuyerRealTimeReportingConfig")]Dictionary<string, AuctionRealTimeReportingConfig>? PerBuyerRealTimeReportingConfig = default,
	[property: Description("@#componentAuctions")]AuctionAdConfig[]? ComponentAuctions = default,
	[property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#resolveToConfig")]PromiseResult<bool>? ResolveToConfig = default,
	[property: Description("@#serverResponse")]PromiseResult<Uint8Array>? ServerResponse = default,
	[property: Description("@#requestId")]string? RequestId = default);

/// <summary>
/// AdAuctionPerSellerData
/// </summary>
[ECMAScript]
[Description("@#AdAuctionPerSellerData")]
public record AdAuctionPerSellerData(
    [property: Description("@#seller")]string? Seller = default,
	[property: Description("@#request")]Uint8Array? Request = default,
	[property: Description("@#error")]string? Error = default);

/// <summary>
/// AdAuctionData
/// </summary>
[ECMAScript]
[Description("@#AdAuctionData")]
public record AdAuctionData(
    [property: Description("@#requestId")]string? RequestId = default,
	[property: Description("@#request")]Uint8Array? Request = default,
	[property: Description("@#requests")]AdAuctionPerSellerData[]? Requests = default);

/// <summary>
/// AdAuctionOneSeller
/// </summary>
[ECMAScript]
[Description("@#AdAuctionOneSeller")]
public record AdAuctionOneSeller(
    [property: Description("@#seller")]string? Seller = default,
	[property: Description("@#coordinatorOrigin")]string? CoordinatorOrigin = default);

/// <summary>
/// AdAuctionDataConfig
/// </summary>
[ECMAScript]
[Description("@#AdAuctionDataConfig")]
public record AdAuctionDataConfig(
    [property: Description("@#seller")]string? Seller = default,
	[property: Description("@#coordinatorOrigin")]string? CoordinatorOrigin = default,
	[property: Description("@#sellers")]AdAuctionOneSeller[]? Sellers = default,
	[property: Description("@#requestSize")]uint RequestSize = default,
	[property: Description("@#perBuyerConfig")]Dictionary<string, AdAuctionDataBuyerConfig>? PerBuyerConfig = default);

/// <summary>
/// AdAuctionDataBuyerConfig
/// </summary>
[ECMAScript]
[Description("@#AdAuctionDataBuyerConfig")]
public record AdAuctionDataBuyerConfig(
    [property: Description("@#targetSize")]uint TargetSize = default);

/// <summary>
/// StorageInterestGroup
/// </summary>
[ECMAScript]
[Description("@#StorageInterestGroup")]
public record StorageInterestGroup(
    [property: Description("@#joinCount")]ulong JoinCount = default,
	[property: Description("@#bidCount")]ulong BidCount = default,
	[property: Description("@#prevWinsMs")]PreviousWin[]? PrevWinsMs = default,
	[property: Description("@#joiningOrigin")]string? JoiningOrigin = default,
	[property: Description("@#timeSinceGroupJoinedMs")]long TimeSinceGroupJoinedMs = default,
	[property: Description("@#lifetimeRemainingMs")]long LifetimeRemainingMs = default,
	[property: Description("@#timeSinceLastUpdateMs")]long TimeSinceLastUpdateMs = default,
	[property: Description("@#timeUntilNextUpdateMs")]long TimeUntilNextUpdateMs = default,
	[property: Description("@#estimatedSize")]ulong EstimatedSize = default): AuctionAdInterestGroup;

/// <summary>
/// PASignalValue
/// </summary>
[ECMAScript]
[Description("@#PASignalValue")]
public record PASignalValue(
    [property: Description("@#baseValue")]string? BaseValue = default,
	[property: Description("@#scale")]double Scale = default,
	[property: Description("@#offset")]Either<BigInteger, int>? Offset = default);

/// <summary>
/// PAExtendedHistogramContribution
/// </summary>
[ECMAScript]
[Description("@#PAExtendedHistogramContribution")]
public record PAExtendedHistogramContribution(
    [property: Description("@#bucket")]Either<PASignalValue, BigInteger>? Bucket = default,
	[property: Description("@#value")]Either<PASignalValue, int>? Value = default,
	[property: Description("@#filteringId")]BigInteger? FilteringId = default);

/// <summary>
/// RealTimeContribution
/// </summary>
[ECMAScript]
[Description("@#RealTimeContribution")]
public record RealTimeContribution(
    [property: Description("@#bucket")]int Bucket = default,
	[property: Description("@#priorityWeight")]double PriorityWeight = default,
	[property: Description("@#latencyThreshold")]int LatencyThreshold = default);

/// <summary>
/// AdRender
/// </summary>
[ECMAScript]
[Description("@#AdRender")]
public record AdRender(
    [property: Description("@#url")]string? Url = default,
	[property: Description("@#width")]string? Width = default,
	[property: Description("@#height")]string? Height = default);

/// <summary>
/// GenerateBidOutput
/// </summary>
[ECMAScript]
[Description("@#GenerateBidOutput")]
public record GenerateBidOutput(
    [property: Description("@#bid")]double Bid = -1d,
	[property: Description("@#bidCurrency")]string? BidCurrency = default,
	[property: Description("@#render")]Either<string, AdRender>? Render = default,
	[property: Description("@#ad")]object? Ad = default,
	[property: Description("@#selectedBuyerAndSellerReportingId")]string? SelectedBuyerAndSellerReportingId = default,
	[property: Description("@#adComponents")]Either<string, AdRender>[]? AdComponents = default,
	[property: Description("@#adCost")]double AdCost = default,
	[property: Description("@#modelingSignals")]double ModelingSignals = default,
	[property: Description("@#allowComponentAuction")]bool AllowComponentAuction = false,
	[property: Description("@#targetNumAdComponents")]uint TargetNumAdComponents = default,
	[property: Description("@#numMandatoryAdComponents")]uint NumMandatoryAdComponents = 0);

/// <summary>
/// BiddingBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#BiddingBrowserSignals")]
public record BiddingBrowserSignals(
    [property: Description("@#topWindowHostname")]string? TopWindowHostname = default,
	[property: Description("@#seller")]string? Seller = default,
	[property: Description("@#joinCount")]int JoinCount = default,
	[property: Description("@#bidCount")]int BidCount = default,
	[property: Description("@#recency")]int Recency = default,
	[property: Description("@#adComponentsLimit")]int AdComponentsLimit = default,
	[property: Description("@#multiBidLimit")]ushort MultiBidLimit = default,
	[property: Description("@#requestedSize")]Dictionary<string, string>? RequestedSize = default,
	[property: Description("@#topLevelSeller")]string? TopLevelSeller = default,
	[property: Description("@#prevWinsMs")]PreviousWin[]? PrevWinsMs = default,
	[property: Description("@#wasmHelper")]object? WasmHelper = default,
	[property: Description("@#dataVersion")]uint DataVersion = default,
	[property: Description("@#crossOriginDataVersion")]uint CrossOriginDataVersion = default,
	[property: Description("@#forDebuggingOnlyInCooldownOrLockout")]bool ForDebuggingOnlyInCooldownOrLockout = false);

/// <summary>
/// ScoringBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#ScoringBrowserSignals")]
public record ScoringBrowserSignals(
    [property: Description("@#topWindowHostname")]string? TopWindowHostname = default,
	[property: Description("@#interestGroupOwner")]string? InterestGroupOwner = default,
	[property: Description("@#renderURL")]string? RenderURL = default,
	[property: Description("@#biddingDurationMsec")]uint BiddingDurationMsec = default,
	[property: Description("@#bidCurrency")]string? BidCurrency = default,
	[property: Description("@#renderSize")]Dictionary<string, string>? RenderSize = default,
	[property: Description("@#dataVersion")]uint DataVersion = default,
	[property: Description("@#crossOriginDataVersion")]uint CrossOriginDataVersion = default,
	[property: Description("@#adComponents")]string[]? AdComponents = default,
	[property: Description("@#forDebuggingOnlyInCooldownOrLockout")]bool ForDebuggingOnlyInCooldownOrLockout = false,
	[property: Description("@#creativeScanningMetadata")]string? CreativeScanningMetadata = default,
	[property: Description("@#adComponentsCreativeScanningMetadata")]string?[]? AdComponentsCreativeScanningMetadata = default);

/// <summary>
/// ReportingBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#ReportingBrowserSignals")]
public record ReportingBrowserSignals(
    [property: Description("@#topWindowHostname")]string? TopWindowHostname = default,
	[property: Description("@#interestGroupOwner")]string? InterestGroupOwner = default,
	[property: Description("@#renderURL")]string? RenderURL = default,
	[property: Description("@#bid")]double Bid = default,
	[property: Description("@#highestScoringOtherBid")]double HighestScoringOtherBid = default,
	[property: Description("@#bidCurrency")]string? BidCurrency = default,
	[property: Description("@#highestScoringOtherBidCurrency")]string? HighestScoringOtherBidCurrency = default,
	[property: Description("@#topLevelSeller")]string? TopLevelSeller = default,
	[property: Description("@#componentSeller")]string? ComponentSeller = default,
	[property: Description("@#buyerAndSellerReportingId")]string? BuyerAndSellerReportingId = default,
	[property: Description("@#selectedBuyerAndSellerReportingId")]string? SelectedBuyerAndSellerReportingId = default);

/// <summary>
/// ReportResultBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#ReportResultBrowserSignals")]
public record ReportResultBrowserSignals(
    [property: Description("@#desirability")]double Desirability = default,
	[property: Description("@#topLevelSellerSignals")]string? TopLevelSellerSignals = default,
	[property: Description("@#modifiedBid")]double ModifiedBid = default,
	[property: Description("@#dataVersion")]uint DataVersion = default): ReportingBrowserSignals;

/// <summary>
/// ReportWinBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#ReportWinBrowserSignals")]
public record ReportWinBrowserSignals(
    [property: Description("@#adCost")]double AdCost = default,
	[property: Description("@#seller")]string? Seller = default,
	[property: Description("@#madeHighestScoringOtherBid")]bool MadeHighestScoringOtherBid = default,
	[property: Description("@#interestGroupName")]string? InterestGroupName = default,
	[property: Description("@#buyerReportingId")]string? BuyerReportingId = default,
	[property: Description("@#modelingSignals")]ushort ModelingSignals = default,
	[property: Description("@#dataVersion")]uint DataVersion = default,
	[property: Description("@#kAnonStatus")]KAnonStatus? KAnonStatus = default): ReportingBrowserSignals;

/// <summary>
/// DirectFromSellerSignalsForBuyer
/// </summary>
[ECMAScript]
[Description("@#DirectFromSellerSignalsForBuyer")]
public record DirectFromSellerSignalsForBuyer(
    [property: Description("@#auctionSignals")]object? AuctionSignals = default,
	[property: Description("@#perBuyerSignals")]object? PerBuyerSignals = default);

/// <summary>
/// DirectFromSellerSignalsForSeller
/// </summary>
[ECMAScript]
[Description("@#DirectFromSellerSignalsForSeller")]
public record DirectFromSellerSignalsForSeller(
    [property: Description("@#auctionSignals")]object? AuctionSignals = default,
	[property: Description("@#sellerSignals")]object? SellerSignals = default);

/// <summary>
/// ScoreAdOutput
/// </summary>
[ECMAScript]
[Description("@#ScoreAdOutput")]
public record ScoreAdOutput(
    [property: Description("@#desirability")]double Desirability = default,
	[property: Description("@#bid")]double Bid = default,
	[property: Description("@#bidCurrency")]string? BidCurrency = default,
	[property: Description("@#incomingBidInSellerCurrency")]double IncomingBidInSellerCurrency = default,
	[property: Description("@#allowComponentAuction")]bool AllowComponentAuction = false);

/// <summary>
/// InterestGroupScriptRunnerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#InterestGroupScriptRunnerGlobalScope")]
public class InterestGroupScriptRunnerGlobalScope
{
    /// <summary>
    /// privateAggregation
    /// </summary>
    [Description("@#privateAggregation")]
    public extern PrivateAggregation? PrivateAggregation { get; }

    /// <summary>
    /// protectedAudience
    /// </summary>
    [Description("@#protectedAudience")]
    public extern ProtectedAudienceUtilities ProtectedAudience { get; }
}

/// <summary>
/// ProtectedAudienceUtilities
/// </summary>
[ECMAScript]
[Description("@#ProtectedAudienceUtilities")]
public class ProtectedAudienceUtilities
{
    /// <summary>
    /// encodeUtf8
    /// </summary>
    /// <param name="input">input</param>
    [Description("@#encodeUtf8")]
    public extern Uint8Array EncodeUtf8(string input);

    /// <summary>
    /// decodeUtf8
    /// </summary>
    /// <param name="bytes">bytes</param>
    [Description("@#decodeUtf8")]
    public extern string DecodeUtf8(Uint8Array bytes);
}

/// <summary>
/// ForDebuggingOnly
/// </summary>
[ECMAScript]
[Description("@#ForDebuggingOnly")]
public class ForDebuggingOnly
{
    /// <summary>
    /// reportAdAuctionWin
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#reportAdAuctionWin")]
    public extern void ReportAdAuctionWin(string url);

    /// <summary>
    /// reportAdAuctionLoss
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#reportAdAuctionLoss")]
    public extern void ReportAdAuctionLoss(string url);
}

/// <summary>
/// RealTimeReporting
/// </summary>
[ECMAScript]
[Description("@#RealTimeReporting")]
public class RealTimeReporting
{
    /// <summary>
    /// contributeToHistogram
    /// </summary>
    /// <param name="contribution">contribution</param>
    [Description("@#contributeToHistogram")]
    public extern void ContributeToHistogram(RealTimeContribution contribution);
}

/// <summary>
/// InterestGroupBiddingAndScoringScriptRunnerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#InterestGroupBiddingAndScoringScriptRunnerGlobalScope")]
public class InterestGroupBiddingAndScoringScriptRunnerGlobalScope : InterestGroupScriptRunnerGlobalScope
{
    /// <summary>
    /// forDebuggingOnly
    /// </summary>
    [Description("@#forDebuggingOnly")]
    public extern ForDebuggingOnly ForDebuggingOnly { get; }

    /// <summary>
    /// realTimeReporting
    /// </summary>
    [Description("@#realTimeReporting")]
    public extern RealTimeReporting RealTimeReporting { get; }
}

/// <summary>
/// InterestGroupBiddingScriptRunnerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#InterestGroupBiddingScriptRunnerGlobalScope")]
public class InterestGroupBiddingScriptRunnerGlobalScope : InterestGroupBiddingAndScoringScriptRunnerGlobalScope
{
    /// <summary>
    /// setBid
    /// </summary>
    /// <param name="oneOrManyBids">oneOrManyBids</param>
    [Description("@#setBid")]
    public extern bool SetBid(Either<GenerateBidOutput, GenerateBidOutput[]> oneOrManyBids);

    /// <summary>
    /// setBid
    /// </summary>
    /// <param name="oneOrManyBids">oneOrManyBids</param>
    [Description("@#setBid")]
    public extern bool SetBid(GenerateBidOutput? oneOrManyBids = default);

    /// <summary>
    /// setBid
    /// </summary>
    /// <param name="oneOrManyBids">oneOrManyBids</param>
    [Description("@#setBid")]
    public extern bool SetBid(GenerateBidOutput[] oneOrManyBids);

    /// <summary>
    /// setPriority
    /// </summary>
    /// <param name="priority">priority</param>
    [Description("@#setPriority")]
    public extern void SetPriority(double priority);

    /// <summary>
    /// setPrioritySignalsOverride
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="priority">priority</param>
    [Description("@#setPrioritySignalsOverride")]
    public extern void SetPrioritySignalsOverride(string key, double? priority);
}

/// <summary>
/// InterestGroupScoringScriptRunnerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#InterestGroupScoringScriptRunnerGlobalScope")]
public class InterestGroupScoringScriptRunnerGlobalScope : InterestGroupBiddingAndScoringScriptRunnerGlobalScope
{

}

/// <summary>
/// InterestGroupReportingScriptRunnerGlobalScope
/// </summary>
[ECMAScript]
[Description("@#InterestGroupReportingScriptRunnerGlobalScope")]
public class InterestGroupReportingScriptRunnerGlobalScope : InterestGroupScriptRunnerGlobalScope
{
    /// <summary>
    /// sendReportTo
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#sendReportTo")]
    public extern void SendReportTo(string url);

    /// <summary>
    /// registerAdBeacon
    /// </summary>
    /// <param name="map">map</param>
    [Description("@#registerAdBeacon")]
    public extern void RegisterAdBeacon(Dictionary<string, string> map);

    /// <summary>
    /// registerAdMacro
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#registerAdMacro")]
    public extern void RegisterAdMacro(string name, string value);
}

/// <summary>
/// ProtectedAudience
/// </summary>
[ECMAScript]
[Description("@#ProtectedAudience")]
public class ProtectedAudience
{
    /// <summary>
    /// queryFeatureSupport
    /// </summary>
    /// <param name="feature">feature</param>
    [Description("@#queryFeatureSupport")]
    public extern object QueryFeatureSupport(string feature);
}