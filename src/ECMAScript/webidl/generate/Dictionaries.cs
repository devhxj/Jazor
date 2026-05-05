namespace ECMAScript;

/// <summary>
/// AuthenticationExtensionsClientInputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientInputsJSON")]
public abstract record AuthenticationExtensionsClientInputsJSON();

/// <summary>
/// AuthenticationExtensionsClientOutputsJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientOutputsJSON")]
public abstract record AuthenticationExtensionsClientOutputsJSON();

/// <summary>
/// ExtendableEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableEventInit")]
public abstract record ExtendableEventInit();

/// <summary>
/// GeolocationSensorOptions
/// </summary>
[ECMAScript]
[Description("@#GeolocationSensorOptions")]
public abstract record GeolocationSensorOptions();

/// <summary>
/// PaymentRequestUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#PaymentRequestUpdateEventInit")]
public abstract record PaymentRequestUpdateEventInit();

/// <summary>
/// RTCAnswerOptions
/// </summary>
[ECMAScript]
[Description("@#RTCAnswerOptions")]
public abstract record RTCAnswerOptions();

/// <summary>
/// RTCOfferAnswerOptions
/// </summary>
[ECMAScript]
[Description("@#RTCOfferAnswerOptions")]
public abstract record RTCOfferAnswerOptions();

/// <summary>
/// RTCRtpCodecCapability
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodecCapability")]
public abstract record RTCRtpCodecCapability();

/// <summary>
/// RTCRtpReceiveParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpReceiveParameters")]
public abstract record RTCRtpReceiveParameters();

/// <summary>
/// RTCRtpSynchronizationSource
/// </summary>
[ECMAScript]
[Description("@#RTCRtpSynchronizationSource")]
public abstract record RTCRtpSynchronizationSource();

/// <summary>
/// RTCSetParameterOptions
/// </summary>
[ECMAScript]
[Description("@#RTCSetParameterOptions")]
public abstract record RTCSetParameterOptions();

/// <summary>
/// VideoFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#VideoFrameMetadata")]
public abstract record VideoFrameMetadata();

/// <summary>
/// AV1EncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AV1EncoderConfig")]
public record AV1EncoderConfig(
    [property: Description("@#forceScreenContentTools")]bool ForceScreenContentTools = false);

/// <summary>
/// AacEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AacEncoderConfig")]
public record AacEncoderConfig(
    [property: Description("@#format")]AacBitstreamFormat Format = AacBitstreamFormat.Aac);

/// <summary>
/// AccelerometerSensorOptions
/// </summary>
[ECMAScript]
[Description("@#AccelerometerSensorOptions")]
public record AccelerometerSensorOptions(
    [property: Description("@#referenceFrame")]AccelerometerLocalCoordinateSystem ReferenceFrame = AccelerometerLocalCoordinateSystem.Device) : SensorOptions;

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
/// AddEventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#AddEventListenerOptions")]
public record AddEventListenerOptions(
    [property: Description("@#passive")]bool Passive = default,
    [property: Description("@#once")]bool Once = false,
    [property: Description("@#signal")]AbortSignal? Signal = default) : EventListenerOptions;

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
/// AesCbcParams
/// </summary>
[ECMAScript]
[Description("@#AesCbcParams")]
public record AesCbcParams(
    [property: Description("@#iv")]IBufferSource? Iv = default) : Algorithm;

/// <summary>
/// AesCtrParams
/// </summary>
[ECMAScript]
[Description("@#AesCtrParams")]
public record AesCtrParams(
    [property: Description("@#counter")]IBufferSource? Counter = default,
    [property: Description("@#length")]byte Length = default) : Algorithm;

/// <summary>
/// AesDerivedKeyParams
/// </summary>
[ECMAScript]
[Description("@#AesDerivedKeyParams")]
public record AesDerivedKeyParams(
    [property: Description("@#length")]ushort Length = default) : Algorithm;

/// <summary>
/// AesGcmParams
/// </summary>
[ECMAScript]
[Description("@#AesGcmParams")]
public record AesGcmParams(
    [property: Description("@#iv")]IBufferSource? Iv = default,
    [property: Description("@#additionalData")]IBufferSource? AdditionalData = default,
    [property: Description("@#tagLength")]byte TagLength = default) : Algorithm;

/// <summary>
/// AesKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#AesKeyAlgorithm")]
public record AesKeyAlgorithm(
    [property: Description("@#length")]ushort Length = default) : KeyAlgorithm;

/// <summary>
/// AesKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#AesKeyGenParams")]
public record AesKeyGenParams(
    [property: Description("@#length")]ushort Length = default) : Algorithm;

/// <summary>
/// Algorithm
/// </summary>
[ECMAScript]
[Description("@#Algorithm")]
public record Algorithm(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// AllowedBluetoothDevice
/// </summary>
[ECMAScript]
[Description("@#AllowedBluetoothDevice")]
public record AllowedBluetoothDevice(
    [property: Description("@#deviceId")]string? DeviceId = default,
    [property: Description("@#mayUseGATT")]bool MayUseGATT = default,
    [property: Description("@#allowedServices")]AllowedBluetoothDeviceAllowedServices? AllowedServices = default,
    [property: Description("@#allowedManufacturerData")]ushort[]? AllowedManufacturerData = default);

/// <summary>
/// AllowedUSBDevice
/// </summary>
[ECMAScript]
[Description("@#AllowedUSBDevice")]
public record AllowedUSBDevice(
    [property: Description("@#vendorId")]byte VendorId = default,
    [property: Description("@#productId")]byte ProductId = default,
    [property: Description("@#serialNumber")]string? SerialNumber = default);

/// <summary>
/// AnalyserOptions
/// </summary>
[ECMAScript]
[Description("@#AnalyserOptions")]
public record AnalyserOptions(
    [property: Description("@#fftSize")]uint FftSize = 2048,
    [property: Description("@#maxDecibels")]double MaxDecibels = -30d,
    [property: Description("@#minDecibels")]double MinDecibels = -100d,
    [property: Description("@#smoothingTimeConstant")]double SmoothingTimeConstant = 0.8d) : AudioNodeOptions;

/// <summary>
/// AnimationEventInit
/// </summary>
[ECMAScript]
[Description("@#AnimationEventInit")]
public record AnimationEventInit(
    [property: Description("@#animationName")]string? AnimationName = default,
    [property: Description("@#elapsedTime")]double ElapsedTime = 0.0d,
    [property: Description("@#pseudoElement")]string? PseudoElement = default) : EventInit;

/// <summary>
/// AnimationPlaybackEventInit
/// </summary>
[ECMAScript]
[Description("@#AnimationPlaybackEventInit")]
public record AnimationPlaybackEventInit(
    [property: Description("@#currentTime")]CSSNumberish? CurrentTime = null,
    [property: Description("@#timelineTime")]CSSNumberish? TimelineTime = null) : EventInit;

/// <summary>
/// AssignedNodesOptions
/// </summary>
[ECMAScript]
[Description("@#AssignedNodesOptions")]
public record AssignedNodesOptions(
    [property: Description("@#flatten")]bool Flatten = false);

/// <summary>
/// AttributionReportingRequestOptions
/// </summary>
[ECMAScript]
[Description("@#AttributionReportingRequestOptions")]
public record AttributionReportingRequestOptions(
    [property: Description("@#eventSourceEligible")]bool EventSourceEligible = default,
    [property: Description("@#triggerEligible")]bool TriggerEligible = default);

/// <summary>
/// AuctionAd
/// </summary>
[ECMAScript]
[Description("@#AuctionAd")]
public record AuctionAd(
    [property: Description("@#renderURL")]string? RenderURL = default,
    [property: Description("@#metadata")]object? Metadata = default,
    [property: Description("@#buyerReportingId")]string? BuyerReportingId = default,
    [property: Description("@#buyerAndSellerReportingId")]string? BuyerAndSellerReportingId = default,
    [property: Description("@#allowedReportingOrigins")]string[]? AllowedReportingOrigins = default);

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
    [property: Description("@#interestGroupBuyers")]string[]? InterestGroupBuyers = default,
    [property: Description("@#auctionSignals")]PromiseResult<object>? AuctionSignals = default,
    [property: Description("@#sellerSignals")]PromiseResult<object>? SellerSignals = default,
    [property: Description("@#directFromSellerSignalsHeaderAdSlot")]PromiseResult<string>? DirectFromSellerSignalsHeaderAdSlot = default,
    [property: Description("@#deprecatedRenderURLReplacements")]PromiseResult<Dictionary<string, string>>? DeprecatedRenderURLReplacements = default,
    [property: Description("@#sellerTimeout")]ulong SellerTimeout = default,
    [property: Description("@#sellerExperimentGroupId")]ushort SellerExperimentGroupId = default,
    [property: Description("@#perBuyerSignals")]PromiseResult<Dictionary<string, object>>? PerBuyerSignals = default,
    [property: Description("@#perBuyerTimeouts")]PromiseResult<Dictionary<string, ulong>>? PerBuyerTimeouts = default,
    [property: Description("@#perBuyerCumulativeTimeouts")]PromiseResult<Dictionary<string, ulong>>? PerBuyerCumulativeTimeouts = default,
    [property: Description("@#reportingTimeout")]ulong ReportingTimeout = default,
    [property: Description("@#sellerCurrency")]string? SellerCurrency = default,
    [property: Description("@#perBuyerCurrencies")]PromiseResult<Dictionary<string, string>>? PerBuyerCurrencies = default,
    [property: Description("@#perBuyerGroupLimits")]Dictionary<string, ushort>? PerBuyerGroupLimits = default,
    [property: Description("@#perBuyerExperimentGroupIds")]Dictionary<string, ushort>? PerBuyerExperimentGroupIds = default,
    [property: Description("@#perBuyerPrioritySignals")]Dictionary<string, Dictionary<string, double>>? PerBuyerPrioritySignals = default,
    [property: Description("@#requiredSellerCapabilities")]string[]? RequiredSellerCapabilities = default,
    [property: Description("@#requestedSize")]Dictionary<string, string>? RequestedSize = default,
    [property: Description("@#allSlotsRequestedSizes")]Dictionary<string, string>[]? AllSlotsRequestedSizes = default,
    [property: Description("@#additionalBids")]PromiseResult? AdditionalBids = default,
    [property: Description("@#auctionNonce")]string? AuctionNonce = default,
    [property: Description("@#componentAuctions")]AuctionAdConfig[]? ComponentAuctions = default,
    [property: Description("@#signal")]AbortSignal? Signal = default,
    [property: Description("@#resolveToConfig")]PromiseResult<bool>? ResolveToConfig = default);

/// <summary>
/// AuctionAdInterestGroup
/// </summary>
[ECMAScript]
[Description("@#AuctionAdInterestGroup")]
public record AuctionAdInterestGroup(
    [property: Description("@#priority")]double Priority = 0.0d,
    [property: Description("@#prioritySignalsOverrides")]Dictionary<string, double>? PrioritySignalsOverrides = default,
    [property: Description("@#additionalBidKey")]string? AdditionalBidKey = default) : GenerateBidInterestGroup;

/// <summary>
/// AuctionAdInterestGroupKey
/// </summary>
[ECMAScript]
[Description("@#AuctionAdInterestGroupKey")]
public record AuctionAdInterestGroupKey(
    [property: Description("@#owner")]string? Owner = default,
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// AudioBufferOptions
/// </summary>
[ECMAScript]
[Description("@#AudioBufferOptions")]
public record AudioBufferOptions(
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = 1,
    [property: Description("@#length")]uint Length = default,
    [property: Description("@#sampleRate")]float SampleRate = default);

/// <summary>
/// AudioBufferSourceOptions
/// </summary>
[ECMAScript]
[Description("@#AudioBufferSourceOptions")]
public record AudioBufferSourceOptions(
    [property: Description("@#buffer")]AudioBuffer? Buffer = default,
    [property: Description("@#detune")]float Detune = 0f,
    [property: Description("@#loop")]bool Loop = false,
    [property: Description("@#loopEnd")]double LoopEnd = 0d,
    [property: Description("@#loopStart")]double LoopStart = 0d,
    [property: Description("@#playbackRate")]float PlaybackRate = 1f);

/// <summary>
/// AudioConfiguration
/// </summary>
[ECMAScript]
[Description("@#AudioConfiguration")]
public record AudioConfiguration(
    [property: Description("@#contentType")]string? ContentType = default,
    [property: Description("@#channels")]string? Channels = default,
    [property: Description("@#bitrate")]ulong Bitrate = default,
    [property: Description("@#samplerate")]uint Samplerate = default,
    [property: Description("@#spatialRendering")]bool SpatialRendering = default);

/// <summary>
/// AudioContextOptions
/// </summary>
[ECMAScript]
[Description("@#AudioContextOptions")]
public record AudioContextOptions(
    [property: Description("@#latencyHint")]AudioContextOptionsLatencyHint? LatencyHint = default,
    [property: Description("@#sampleRate")]float SampleRate = default,
    [property: Description("@#sinkId")]AudioContextOptionsSinkId? SinkId = default,
    [property: Description("@#renderSizeHint")]AudioContextOptionsRenderSizeHint? RenderSizeHint = default);

/// <summary>
/// AudioDataCopyToOptions
/// </summary>
[ECMAScript]
[Description("@#AudioDataCopyToOptions")]
public record AudioDataCopyToOptions(
    [property: Description("@#planeIndex")]uint PlaneIndex = default,
    [property: Description("@#frameOffset")]uint FrameOffset = 0,
    [property: Description("@#frameCount")]uint FrameCount = default,
    [property: Description("@#format")]AudioSampleFormat? Format = default);

/// <summary>
/// AudioDataInit
/// </summary>
[ECMAScript]
[Description("@#AudioDataInit")]
public record AudioDataInit(
    [property: Description("@#format")]AudioSampleFormat? Format = default,
    [property: Description("@#sampleRate")]float SampleRate = default,
    [property: Description("@#numberOfFrames")]uint NumberOfFrames = default,
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#data")]IBufferSource? Data = default,
    [property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// AudioDecoderConfig
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderConfig")]
public record AudioDecoderConfig(
    [property: Description("@#codec")]string? Codec = default,
    [property: Description("@#sampleRate")]uint SampleRate = default,
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
    [property: Description("@#description")]IBufferSource? Description = default);

/// <summary>
/// AudioDecoderInit
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderInit")]
public record AudioDecoderInit(
    [property: Description("@#output")]AudioDataOutputCallback? Output = default,
    [property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// AudioDecoderSupport
/// </summary>
[ECMAScript]
[Description("@#AudioDecoderSupport")]
public record AudioDecoderSupport(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#config")]AudioDecoderConfig? Config = default);

/// <summary>
/// AudioEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderConfig")]
public record AudioEncoderConfig(
    [property: Description("@#codec")]string? Codec = default,
    [property: Description("@#sampleRate")]uint SampleRate = default,
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = default,
    [property: Description("@#bitrate")]ulong Bitrate = default,
    [property: Description("@#bitrateMode")]BitrateMode BitrateMode = BitrateMode.Variable,
    [property: Description("@#aac")]AacEncoderConfig? Aac = default,
    [property: Description("@#flac")]FlacEncoderConfig? Flac = default,
    [property: Description("@#opus")]OpusEncoderConfig? Opus = default)
{
    [Category("optional")]
    public extern static AudioEncoderConfig OptionalCodecSampleRateNumberOfChannels5(
        [Description("@#codec")]string? Codec = default,
        [Description("@#sampleRate")]uint SampleRate = default,
        [Description("@#numberOfChannels")]uint NumberOfChannels = default,
        [Description("@#bitrate")]ulong Bitrate = default,
        [Description("@#bitrateMode")]BitrateMode bitrateMode = BitrateMode.Variable);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalAac(
        [Description("@#aac")]AacEncoderConfig? Aac = default);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalFlac(
        [Description("@#flac")]FlacEncoderConfig? Flac = default);

    [Category("optional")]
    public extern static AudioEncoderConfig OptionalOpus(
        [Description("@#opus")]OpusEncoderConfig? Opus = default);
}

/// <summary>
/// AudioEncoderInit
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderInit")]
public record AudioEncoderInit(
    [property: Description("@#output")]EncodedAudioChunkOutputCallback? Output = default,
    [property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// AudioEncoderSupport
/// </summary>
[ECMAScript]
[Description("@#AudioEncoderSupport")]
public record AudioEncoderSupport(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#config")]AudioEncoderConfig? Config = default);

/// <summary>
/// AudioNodeOptions
/// </summary>
[ECMAScript]
[Description("@#AudioNodeOptions")]
public record AudioNodeOptions(
    [property: Description("@#channelCount")]uint ChannelCount = default,
    [property: Description("@#channelCountMode")]ChannelCountMode? ChannelCountMode = default,
    [property: Description("@#channelInterpretation")]ChannelInterpretation? ChannelInterpretation = default);

/// <summary>
/// AudioOutputOptions
/// </summary>
[ECMAScript]
[Description("@#AudioOutputOptions")]
public record AudioOutputOptions(
    [property: Description("@#deviceId")]string? DeviceId = default);

/// <summary>
/// AudioParamDescriptor
/// </summary>
[ECMAScript]
[Description("@#AudioParamDescriptor")]
public record AudioParamDescriptor(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#defaultValue")]float DefaultValue = 0f,
    [property: Description("@#minValue")]float MinValue = -3.4028235e38f,
    [property: Description("@#maxValue")]float MaxValue = 3.4028235e38f,
    [property: Description("@#automationRate")]AutomationRate AutomationRate = AutomationRate.ARate);

/// <summary>
/// AudioProcessingEventInit
/// </summary>
[ECMAScript]
[Description("@#AudioProcessingEventInit")]
public record AudioProcessingEventInit(
    [property: Description("@#playbackTime")]double PlaybackTime = default,
    [property: Description("@#inputBuffer")]AudioBuffer? InputBuffer = default,
    [property: Description("@#outputBuffer")]AudioBuffer? OutputBuffer = default) : EventInit;

/// <summary>
/// AudioRenderCapacityEventInit
/// </summary>
[ECMAScript]
[Description("@#AudioRenderCapacityEventInit")]
public record AudioRenderCapacityEventInit(
    [property: Description("@#timestamp")]double Timestamp = 0d,
    [property: Description("@#averageLoad")]double AverageLoad = 0d,
    [property: Description("@#peakLoad")]double PeakLoad = 0d,
    [property: Description("@#underrunRatio")]double UnderrunRatio = 0d) : EventInit;

/// <summary>
/// AudioRenderCapacityOptions
/// </summary>
[ECMAScript]
[Description("@#AudioRenderCapacityOptions")]
public record AudioRenderCapacityOptions(
    [property: Description("@#updateInterval")]double UpdateInterval = 1d);

/// <summary>
/// AudioSinkOptions
/// </summary>
[ECMAScript]
[Description("@#AudioSinkOptions")]
public record AudioSinkOptions(
    [property: Description("@#type")]AudioSinkType? Type = default);

/// <summary>
/// AudioTimestamp
/// </summary>
[ECMAScript]
[Description("@#AudioTimestamp")]
public record AudioTimestamp(
    [property: Description("@#contextTime")]double ContextTime = default,
    [property: Description("@#performanceTime")]double PerformanceTime = default);

/// <summary>
/// AudioWorkletNodeOptions
/// </summary>
[ECMAScript]
[Description("@#AudioWorkletNodeOptions")]
public record AudioWorkletNodeOptions(
    [property: Description("@#numberOfInputs")]uint NumberOfInputs = 1,
    [property: Description("@#numberOfOutputs")]uint NumberOfOutputs = 1,
    [property: Description("@#outputChannelCount")]uint[]? OutputChannelCount = default,
    [property: Description("@#parameterData")]Dictionary<string, double>? ParameterData = default,
    [property: Description("@#processorOptions")]object? ProcessorOptions = default) : AudioNodeOptions;

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
    [property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobInputs? LargeBlob = default,
    [property: Description("@#uvm")]bool Uvm = default,
    [property: Description("@#supplementalPubKeys")]AuthenticationExtensionsSupplementalPubKeysInputs? SupplementalPubKeys = default)
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

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalUvm(
        [Description("@#uvm")]bool Uvm = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientInputs OptionalSupplementalPubKeys(
        [Description("@#supplementalPubKeys")]AuthenticationExtensionsSupplementalPubKeysInputs? SupplementalPubKeys = default);
}

/// <summary>
/// AuthenticationExtensionsClientOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsClientOutputs")]
public record AuthenticationExtensionsClientOutputs(
    [property: Description("@#hmacCreateSecret")]bool HmacCreateSecret = default,
    [property: Description("@#hmacGetSecret")]HMACGetSecretOutput? HmacGetSecret = default,
    [property: Description("@#appid")]bool Appid = default,
    [property: Description("@#appidExclude")]bool AppidExclude = default,
    [property: Description("@#credProps")]CredentialPropertiesOutput? CredProps = default,
    [property: Description("@#prf")]AuthenticationExtensionsPRFOutputs? Prf = default,
    [property: Description("@#largeBlob")]AuthenticationExtensionsLargeBlobOutputs? LargeBlob = default,
    [property: Description("@#uvm")]UvmEntries? Uvm = default,
    [property: Description("@#supplementalPubKeys")]AuthenticationExtensionsSupplementalPubKeysOutputs? SupplementalPubKeys = default)
{
    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalHmacCreateSecret(
        [Description("@#hmacCreateSecret")]bool HmacCreateSecret = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalHmacGetSecret(
        [Description("@#hmacGetSecret")]HMACGetSecretOutput? HmacGetSecret = default);

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

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalUvm(
        [Description("@#uvm")]UvmEntries? Uvm = default);

    [Category("optional")]
    public extern static AuthenticationExtensionsClientOutputs OptionalSupplementalPubKeys(
        [Description("@#supplementalPubKeys")]AuthenticationExtensionsSupplementalPubKeysOutputs? SupplementalPubKeys = default);
}

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
/// AuthenticationExtensionsLargeBlobOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsLargeBlobOutputs")]
public record AuthenticationExtensionsLargeBlobOutputs(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#blob")]ArrayBuffer? Blob = default,
    [property: Description("@#written")]bool Written = default);

/// <summary>
/// AuthenticationExtensionsPRFInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFInputs")]
public record AuthenticationExtensionsPRFInputs(
    [property: Description("@#eval")]AuthenticationExtensionsPRFValues? Eval = default,
    [property: Description("@#evalByCredential")]Dictionary<string, AuthenticationExtensionsPRFValues>? EvalByCredential = default);

/// <summary>
/// AuthenticationExtensionsPRFOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFOutputs")]
public record AuthenticationExtensionsPRFOutputs(
    [property: Description("@#enabled")]bool Enabled = default,
    [property: Description("@#results")]AuthenticationExtensionsPRFValues? Results = default);

/// <summary>
/// AuthenticationExtensionsPRFValues
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPRFValues")]
public record AuthenticationExtensionsPRFValues(
    [property: Description("@#first")]IBufferSource? First = default,
    [property: Description("@#second")]IBufferSource? Second = default);

/// <summary>
/// AuthenticationExtensionsPaymentInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsPaymentInputs")]
public record AuthenticationExtensionsPaymentInputs(
    [property: Description("@#isPayment")]bool IsPayment = default,
    [property: Description("@#rpId")]string? RpId = default,
    [property: Description("@#topOrigin")]string? TopOrigin = default,
    [property: Description("@#payeeName")]string? PayeeName = default,
    [property: Description("@#payeeOrigin")]string? PayeeOrigin = default,
    [property: Description("@#total")]PaymentCurrencyAmount? Total = default,
    [property: Description("@#instrument")]PaymentCredentialInstrument? Instrument = default);

/// <summary>
/// AuthenticationExtensionsSupplementalPubKeysInputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsSupplementalPubKeysInputs")]
public record AuthenticationExtensionsSupplementalPubKeysInputs(
    [property: Description("@#scopes")]string[]? Scopes = default,
    [property: Description("@#attestation")]string? Attestation = default,
    [property: Description("@#attestationFormats")]string[]? AttestationFormats = default);

/// <summary>
/// AuthenticationExtensionsSupplementalPubKeysOutputs
/// </summary>
[ECMAScript]
[Description("@#AuthenticationExtensionsSupplementalPubKeysOutputs")]
public record AuthenticationExtensionsSupplementalPubKeysOutputs(
    [property: Description("@#signatures")]ArrayBuffer[]? Signatures = default);

/// <summary>
/// AuthenticationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticationResponseJSON")]
public record AuthenticationResponseJSON(
    [property: Description("@#id")]Base64URLString? Id = default,
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
/// AuthenticatorAttestationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#AuthenticatorAttestationResponseJSON")]
public record AuthenticatorAttestationResponseJSON(
    [property: Description("@#clientDataJSON")]Base64URLString? ClientDataJSON = default,
    [property: Description("@#authenticatorData")]Base64URLString? AuthenticatorData = default,
    [property: Description("@#transports")]string[]? Transports = default,
    [property: Description("@#publicKey")]Base64URLString? PublicKey = default,
    [property: Description("@#publicKeyAlgorithm")]long PublicKeyAlgorithm = default,
    [property: Description("@#attestationObject")]Base64URLString? AttestationObject = default);

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
/// AvcEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#AvcEncoderConfig")]
public record AvcEncoderConfig(
    [property: Description("@#format")]AvcBitstreamFormat Format = AvcBitstreamFormat.Avc);

/// <summary>
/// BackgroundFetchEventInit
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchEventInit")]
public record BackgroundFetchEventInit(
    [property: Description("@#registration")]BackgroundFetchRegistration? Registration = default) : ExtendableEventInit;

/// <summary>
/// BackgroundFetchOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchOptions")]
public record BackgroundFetchOptions(
    [property: Description("@#downloadTotal")]ulong DownloadTotal = 0) : BackgroundFetchUIOptions;

/// <summary>
/// BackgroundFetchUIOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundFetchUIOptions")]
public record BackgroundFetchUIOptions(
    [property: Description("@#icons")]ImageResource[]? Icons = default,
    [property: Description("@#title")]string? Title = default);

/// <summary>
/// BackgroundSyncOptions
/// </summary>
[ECMAScript]
[Description("@#BackgroundSyncOptions")]
public record BackgroundSyncOptions(
    [property: Description("@#minInterval")]ulong MinInterval = 0);

/// <summary>
/// BarcodeDetectorOptions
/// </summary>
[ECMAScript]
[Description("@#BarcodeDetectorOptions")]
public record BarcodeDetectorOptions(
    [property: Description("@#formats")]BarcodeFormat[]? Formats = default);

/// <summary>
/// BaseComputedKeyframe
/// </summary>
[ECMAScript]
[Description("@#BaseComputedKeyframe")]
public record BaseComputedKeyframe(
    [property: Description("@#offset")]double? Offset = null,
    [property: Description("@#computedOffset")]double ComputedOffset = default,
    [property: Description("@#easing")]string? Easing = default,
    [property: Description("@#composite")]CompositeOperationOrAuto Composite = CompositeOperationOrAuto.Auto);

/// <summary>
/// BaseKeyframe
/// </summary>
[ECMAScript]
[Description("@#BaseKeyframe")]
public record BaseKeyframe(
    [property: Description("@#offset")]double? Offset = null,
    [property: Description("@#easing")]string? Easing = default,
    [property: Description("@#composite")]CompositeOperationOrAuto Composite = CompositeOperationOrAuto.Auto);

/// <summary>
/// BasePropertyIndexedKeyframe
/// </summary>
[ECMAScript]
[Description("@#BasePropertyIndexedKeyframe")]
public record BasePropertyIndexedKeyframe(
    [property: Description("@#offset")]BasePropertyIndexedKeyframeOffset? Offset = default,
    [property: Description("@#easing")]BasePropertyIndexedKeyframeEasing? Easing = default,
    [property: Description("@#composite")]BasePropertyIndexedKeyframeComposite? Composite = default);

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
    [property: Description("@#topLevelSeller")]string? TopLevelSeller = default,
    [property: Description("@#prevWinsMs")]PreviousWin[]? PrevWinsMs = default,
    [property: Description("@#wasmHelper")]object? WasmHelper = default,
    [property: Description("@#dataVersion")]uint DataVersion = default,
    [property: Description("@#forDebuggingOnlyInCooldownOrLockout")]bool ForDebuggingOnlyInCooldownOrLockout = false);

/// <summary>
/// BiquadFilterOptions
/// </summary>
[ECMAScript]
[Description("@#BiquadFilterOptions")]
public record BiquadFilterOptions(
    [property: Description("@#type")]BiquadFilterType Type = BiquadFilterType.Lowpass,
    [property: Description("@#Q")]float Q = 1f,
    [property: Description("@#detune")]float Detune = 0f,
    [property: Description("@#frequency")]float Frequency = 350f,
    [property: Description("@#gain")]float Gain = 0f) : AudioNodeOptions;

/// <summary>
/// BlobEventInit
/// </summary>
[ECMAScript]
[Description("@#BlobEventInit")]
public record BlobEventInit(
    [property: Description("@#data")]Blob? Data = default,
    [property: Description("@#timecode")]double Timecode = default);

/// <summary>
/// BlobPropertyBag
/// </summary>
[ECMAScript]
[Description("@#BlobPropertyBag")]
public record BlobPropertyBag(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#endings")]EndingType Endings = EndingType.Transparent);

/// <summary>
/// BluetoothAdvertisingEventInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothAdvertisingEventInit")]
public record BluetoothAdvertisingEventInit(
    [property: Description("@#device")]BluetoothDevice? Device = default,
    [property: Description("@#uuids")]BluetoothAdvertisingEventInitUUIDs[]? UUIDs = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#appearance")]ushort Appearance = default,
    [property: Description("@#txPower")]sbyte TxPower = default,
    [property: Description("@#rssi")]sbyte Rssi = default,
    [property: Description("@#manufacturerData")]BluetoothManufacturerDataMap? ManufacturerData = default,
    [property: Description("@#serviceData")]BluetoothServiceDataMap? ServiceData = default) : EventInit;

/// <summary>
/// BluetoothDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothDataFilterInit")]
public record BluetoothDataFilterInit(
    [property: Description("@#dataPrefix")]IBufferSource? DataPrefix = default,
    [property: Description("@#mask")]IBufferSource? Mask = default);

/// <summary>
/// BluetoothLEScanFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanFilterInit")]
public record BluetoothLEScanFilterInit(
    [property: Description("@#services")]BluetoothServiceUUID[]? Services = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#namePrefix")]string? NamePrefix = default,
    [property: Description("@#manufacturerData")]BluetoothManufacturerDataFilterInit[]? ManufacturerData = default,
    [property: Description("@#serviceData")]BluetoothServiceDataFilterInit[]? ServiceData = default);

/// <summary>
/// BluetoothLEScanOptions
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanOptions")]
public record BluetoothLEScanOptions(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
    [property: Description("@#keepRepeatedDevices")]bool KeepRepeatedDevices = false,
    [property: Description("@#acceptAllAdvertisements")]bool AcceptAllAdvertisements = false);

/// <summary>
/// BluetoothLEScanPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanPermissionDescriptor")]
public record BluetoothLEScanPermissionDescriptor(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
    [property: Description("@#keepRepeatedDevices")]bool KeepRepeatedDevices = false,
    [property: Description("@#acceptAllAdvertisements")]bool AcceptAllAdvertisements = false) : PermissionDescriptor;

/// <summary>
/// BluetoothManufacturerDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothManufacturerDataFilterInit")]
public record BluetoothManufacturerDataFilterInit(
    [property: Description("@#companyIdentifier")]ushort CompanyIdentifier = default) : BluetoothDataFilterInit;

/// <summary>
/// BluetoothPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#BluetoothPermissionDescriptor")]
public record BluetoothPermissionDescriptor(
    [property: Description("@#deviceId")]string? DeviceId = default,
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
    [property: Description("@#optionalServices")]BluetoothServiceUUID[]? OptionalServices = default,
    [property: Description("@#optionalManufacturerData")]ushort[]? OptionalManufacturerData = default,
    [property: Description("@#acceptAllDevices")]bool AcceptAllDevices = false) : PermissionDescriptor;

/// <summary>
/// BluetoothPermissionStorage
/// </summary>
[ECMAScript]
[Description("@#BluetoothPermissionStorage")]
public record BluetoothPermissionStorage(
    [property: Description("@#allowedDevices")]AllowedBluetoothDevice[]? AllowedDevices = default);

/// <summary>
/// BluetoothServiceDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothServiceDataFilterInit")]
public record BluetoothServiceDataFilterInit(
    [property: Description("@#service")]BluetoothServiceUUID? Service = default) : BluetoothDataFilterInit;

/// <summary>
/// BoxQuadOptions
/// </summary>
[ECMAScript]
[Description("@#BoxQuadOptions")]
public record BoxQuadOptions(
    [property: Description("@#box")]CSSBoxType Box = CSSBoxType.Border,
    [property: Description("@#relativeTo")]GeometryNode? RelativeTo = default);

/// <summary>
/// BufferedChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#BufferedChangeEventInit")]
public record BufferedChangeEventInit(
    [property: Description("@#addedRanges")]TimeRanges? AddedRanges = default,
    [property: Description("@#removedRanges")]TimeRanges? RemovedRanges = default) : EventInit;

/// <summary>
/// CacheQueryOptions
/// </summary>
[ECMAScript]
[Description("@#CacheQueryOptions")]
public record CacheQueryOptions(
    [property: Description("@#ignoreSearch")]bool IgnoreSearch = false,
    [property: Description("@#ignoreMethod")]bool IgnoreMethod = false,
    [property: Description("@#ignoreVary")]bool IgnoreVary = false);

/// <summary>
/// CameraDevicePermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#CameraDevicePermissionDescriptor")]
public record CameraDevicePermissionDescriptor(
    [property: Description("@#panTiltZoom")]bool PanTiltZoom = false) : PermissionDescriptor;

/// <summary>
/// CanvasRenderingContext2DSettings
/// </summary>
[ECMAScript]
[Description("@#CanvasRenderingContext2DSettings")]
public record CanvasRenderingContext2DSettings(
    [property: Description("@#alpha")]bool Alpha = false,
    [property: Description("@#desynchronized")]bool Desynchronized = false,
    [property: Description("@#colorSpace")]PredefinedColorSpace ColorSpace = PredefinedColorSpace.Srgb,
    [property: Description("@#willReadFrequently")]bool WillReadFrequently = false);

/// <summary>
/// CaptureActionEventInit
/// </summary>
[ECMAScript]
[Description("@#CaptureActionEventInit")]
public record CaptureActionEventInit(
    [property: Description("@#action")]string? Action = default) : EventInit;

/// <summary>
/// CaptureHandle
/// </summary>
[ECMAScript]
[Description("@#CaptureHandle")]
public record CaptureHandle(
    [property: Description("@#origin")]string? Origin = default,
    [property: Description("@#handle")]string? Handle = default);

/// <summary>
/// CaptureHandleConfig
/// </summary>
[ECMAScript]
[Description("@#CaptureHandleConfig")]
public record CaptureHandleConfig(
    [property: Description("@#exposeOrigin")]bool ExposeOrigin = false,
    [property: Description("@#handle")]string? Handle = default,
    [property: Description("@#permittedOrigins")]string[]? PermittedOrigins = default);

/// <summary>
/// CapturedMouseEventInit
/// </summary>
[ECMAScript]
[Description("@#CapturedMouseEventInit")]
public record CapturedMouseEventInit(
    [property: Description("@#surfaceX")]int SurfaceX = -1,
    [property: Description("@#surfaceY")]int SurfaceY = -1) : EventInit;

/// <summary>
/// ChannelMergerOptions
/// </summary>
[ECMAScript]
[Description("@#ChannelMergerOptions")]
public record ChannelMergerOptions(
    [property: Description("@#numberOfInputs")]uint NumberOfInputs = 6) : AudioNodeOptions;

/// <summary>
/// ChannelSplitterOptions
/// </summary>
[ECMAScript]
[Description("@#ChannelSplitterOptions")]
public record ChannelSplitterOptions(
    [property: Description("@#numberOfOutputs")]uint NumberOfOutputs = 6) : AudioNodeOptions;

/// <summary>
/// ChapterInformationInit
/// </summary>
[ECMAScript]
[Description("@#ChapterInformationInit")]
public record ChapterInformationInit(
    [property: Description("@#title")]string? Title = default,
    [property: Description("@#startTime")]double StartTime = 0d,
    [property: Description("@#artwork")]MediaImage[]? Artwork = default);

/// <summary>
/// CharacterBoundsUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#CharacterBoundsUpdateEventInit")]
public record CharacterBoundsUpdateEventInit(
    [property: Description("@#rangeStart")]uint RangeStart = default,
    [property: Description("@#rangeEnd")]uint RangeEnd = default) : EventInit;

/// <summary>
/// CheckVisibilityOptions
/// </summary>
[ECMAScript]
[Description("@#CheckVisibilityOptions")]
public record CheckVisibilityOptions(
    [property: Description("@#checkOpacity")]bool CheckOpacity = false,
    [property: Description("@#checkVisibilityCSS")]bool CheckVisibilityCSS = false,
    [property: Description("@#contentVisibilityAuto")]bool ContentVisibilityAuto = false,
    [property: Description("@#opacityProperty")]bool OpacityProperty = false,
    [property: Description("@#visibilityProperty")]bool VisibilityProperty = false);

/// <summary>
/// ClientQueryOptions
/// </summary>
[ECMAScript]
[Description("@#ClientQueryOptions")]
public record ClientQueryOptions(
    [property: Description("@#includeUncontrolled")]bool IncludeUncontrolled = false,
    [property: Description("@#type")]ClientType Type = ClientType.Window);

/// <summary>
/// ClipboardEventInit
/// </summary>
[ECMAScript]
[Description("@#ClipboardEventInit")]
public record ClipboardEventInit(
    [property: Description("@#clipboardData")]DataTransfer? ClipboardData = null) : EventInit;

/// <summary>
/// ClipboardItemOptions
/// </summary>
[ECMAScript]
[Description("@#ClipboardItemOptions")]
public record ClipboardItemOptions(
    [property: Description("@#presentationStyle")]PresentationStyle PresentationStyle = PresentationStyle.Unspecified);

/// <summary>
/// ClipboardPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#ClipboardPermissionDescriptor")]
public record ClipboardPermissionDescriptor(
    [property: Description("@#allowWithoutGesture")]bool AllowWithoutGesture = false) : PermissionDescriptor;

/// <summary>
/// ClipboardUnsanitizedFormats
/// </summary>
[ECMAScript]
[Description("@#ClipboardUnsanitizedFormats")]
public record ClipboardUnsanitizedFormats(
    [property: Description("@#unsanitized")]string[]? Unsanitized = default);

/// <summary>
/// CloseEventInit
/// </summary>
[ECMAScript]
[Description("@#CloseEventInit")]
public record CloseEventInit(
    [property: Description("@#wasClean")]bool WasClean = false,
    [property: Description("@#code")]ushort Code = 0,
    [property: Description("@#reason")]string? Reason = default) : EventInit;

/// <summary>
/// CloseWatcherOptions
/// </summary>
[ECMAScript]
[Description("@#CloseWatcherOptions")]
public record CloseWatcherOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

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
    [property: Description("@#total")]PaymentCurrencyAmount? Total = default,
    [property: Description("@#instrument")]PaymentCredentialInstrument? Instrument = default);

/// <summary>
/// CollectedClientData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientData")]
public record CollectedClientData(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#challenge")]string? Challenge = default,
    [property: Description("@#origin")]string? Origin = default,
    [property: Description("@#topOrigin")]string? TopOrigin = default,
    [property: Description("@#crossOrigin")]bool CrossOrigin = default);

/// <summary>
/// CollectedClientPaymentData
/// </summary>
[ECMAScript]
[Description("@#CollectedClientPaymentData")]
public record CollectedClientPaymentData(
    [property: Description("@#payment")]CollectedClientAdditionalPaymentData? Payment = default) : CollectedClientData;

/// <summary>
/// ColorSelectionOptions
/// </summary>
[ECMAScript]
[Description("@#ColorSelectionOptions")]
public record ColorSelectionOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// ColorSelectionResult
/// </summary>
[ECMAScript]
[Description("@#ColorSelectionResult")]
public record ColorSelectionResult(
    [property: Description("@#sRGBHex")]string? SRGBHex = default);

/// <summary>
/// CompositionEventInit
/// </summary>
[ECMAScript]
[Description("@#CompositionEventInit")]
public record CompositionEventInit(
    [property: Description("@#data")]string? Data = default) : UIEventInit;

/// <summary>
/// ComputedEffectTiming
/// </summary>
[ECMAScript]
[Description("@#ComputedEffectTiming")]
public record ComputedEffectTiming(
    [property: Description("@#progress")]double Progress = default,
    [property: Description("@#currentIteration")]double CurrentIteration = default,
    [property: Description("@#startTime")]CSSNumberish? StartTime = default,
    [property: Description("@#endTime")]CSSNumberish? EndTime = default,
    [property: Description("@#activeDuration")]CSSNumberish? ActiveDuration = default,
    [property: Description("@#localTime")]CSSNumberish? LocalTime = default) : EffectTiming
{
    [Category("optional")]
    public extern static ComputedEffectTiming OptionalProgressCurrentIteration(
        [Description("@#progress")]double Progress = default,
        [Description("@#currentIteration")]double CurrentIteration = default);

    [Category("optional")]
    public extern static ComputedEffectTiming OptionalStartTimeEndTimeActiveDuration4(
        [Description("@#startTime")]CSSNumberish? StartTime = default,
        [Description("@#endTime")]CSSNumberish? EndTime = default,
        [Description("@#activeDuration")]CSSNumberish? ActiveDuration = default,
        [Description("@#localTime")]CSSNumberish? LocalTime = default);
}

/// <summary>
/// ConstantSourceOptions
/// </summary>
[ECMAScript]
[Description("@#ConstantSourceOptions")]
public record ConstantSourceOptions(
    [property: Description("@#offset")]float Offset = 1f);

/// <summary>
/// ConstrainBooleanParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainBooleanParameters")]
public record ConstrainBooleanParameters(
    [property: Description("@#exact")]bool Exact = default,
    [property: Description("@#ideal")]bool Ideal = default);

/// <summary>
/// ConstrainDOMStringParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainDOMStringParameters")]
public record ConstrainDOMStringParameters(
    [property: Description("@#exact")]ConstrainDOMStringParametersExact? Exact = default,
    [property: Description("@#ideal")]ConstrainDOMStringParametersIdeal? Ideal = default);

/// <summary>
/// ConstrainDoubleRange
/// </summary>
[ECMAScript]
[Description("@#ConstrainDoubleRange")]
public record ConstrainDoubleRange(
    [property: Description("@#exact")]double Exact = default,
    [property: Description("@#ideal")]double Ideal = default) : DoubleRange;

/// <summary>
/// ConstrainPoint2DParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainPoint2DParameters")]
public record ConstrainPoint2DParameters(
    [property: Description("@#exact")]Point2D[]? Exact = default,
    [property: Description("@#ideal")]Point2D[]? Ideal = default);

/// <summary>
/// ConstrainULongRange
/// </summary>
[ECMAScript]
[Description("@#ConstrainULongRange")]
public record ConstrainULongRange(
    [property: Description("@#exact")]uint Exact = default,
    [property: Description("@#ideal")]uint Ideal = default) : ULongRange;

/// <summary>
/// ContactInfo
/// </summary>
[ECMAScript]
[Description("@#ContactInfo")]
public record ContactInfo(
    [property: Description("@#address")]ContactAddress[]? Address = default,
    [property: Description("@#email")]string[]? Email = default,
    [property: Description("@#icon")]Blob[]? Icon = default,
    [property: Description("@#name")]string[]? Name = default,
    [property: Description("@#tel")]string[]? Tel = default);

/// <summary>
/// ContactsSelectOptions
/// </summary>
[ECMAScript]
[Description("@#ContactsSelectOptions")]
public record ContactsSelectOptions(
    [property: Description("@#multiple")]bool Multiple = false);

/// <summary>
/// ContentDescription
/// </summary>
[ECMAScript]
[Description("@#ContentDescription")]
public record ContentDescription(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#title")]string? Title = default,
    [property: Description("@#description")]string? Description = default,
    [property: Description("@#category")]ContentCategory Category = ContentCategory.Empty,
    [property: Description("@#icons")]ImageResource[]? Icons = default,
    [property: Description("@#url")]string? Url = default);

/// <summary>
/// ContentIndexEventInit
/// </summary>
[ECMAScript]
[Description("@#ContentIndexEventInit")]
public record ContentIndexEventInit(
    [property: Description("@#id")]string? Id = default) : ExtendableEventInit;

/// <summary>
/// ContentVisibilityAutoStateChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#ContentVisibilityAutoStateChangeEventInit")]
public record ContentVisibilityAutoStateChangeEventInit(
    [property: Description("@#skipped")]bool Skipped = false) : EventInit;

/// <summary>
/// ConvertCoordinateOptions
/// </summary>
[ECMAScript]
[Description("@#ConvertCoordinateOptions")]
public record ConvertCoordinateOptions(
    [property: Description("@#fromBox")]CSSBoxType FromBox = CSSBoxType.Border,
    [property: Description("@#toBox")]CSSBoxType ToBox = CSSBoxType.Border);

/// <summary>
/// ConvolverOptions
/// </summary>
[ECMAScript]
[Description("@#ConvolverOptions")]
public record ConvolverOptions(
    [property: Description("@#buffer")]AudioBuffer? Buffer = default,
    [property: Description("@#disableNormalization")]bool DisableNormalization = false) : AudioNodeOptions;

/// <summary>
/// CookieChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#CookieChangeEventInit")]
public record CookieChangeEventInit(
    [property: Description("@#changed")]CookieList? Changed = default,
    [property: Description("@#deleted")]CookieList? Deleted = default) : EventInit;

/// <summary>
/// CookieInit
/// </summary>
[ECMAScript]
[Description("@#CookieInit")]
public record CookieInit(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#value")]string? Value = default,
    [property: Description("@#expires")]double? Expires = null,
    [property: Description("@#domain")]string? Domain = null,
    [property: Description("@#path")]string? Path = default,
    [property: Description("@#sameSite")]CookieSameSite SameSite = CookieSameSite.Strict,
    [property: Description("@#partitioned")]bool Partitioned = false);

/// <summary>
/// CookieListItem
/// </summary>
[ECMAScript]
[Description("@#CookieListItem")]
public record CookieListItem(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#value")]string? Value = default,
    [property: Description("@#domain")]string? Domain = default,
    [property: Description("@#path")]string? Path = default,
    [property: Description("@#expires")]double Expires = default,
    [property: Description("@#secure")]bool Secure = default,
    [property: Description("@#sameSite")]CookieSameSite? SameSite = default,
    [property: Description("@#partitioned")]bool Partitioned = default);

/// <summary>
/// CookieStoreDeleteOptions
/// </summary>
[ECMAScript]
[Description("@#CookieStoreDeleteOptions")]
public record CookieStoreDeleteOptions(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#domain")]string? Domain = null,
    [property: Description("@#path")]string? Path = default,
    [property: Description("@#partitioned")]bool Partitioned = false);

/// <summary>
/// CookieStoreGetOptions
/// </summary>
[ECMAScript]
[Description("@#CookieStoreGetOptions")]
public record CookieStoreGetOptions(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#url")]string? Url = default);

/// <summary>
/// CredentialCreationOptions
/// </summary>
[ECMAScript]
[Description("@#CredentialCreationOptions")]
public record CredentialCreationOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
    [property: Description("@#password")]PasswordCredentialInit? Password = default,
    [property: Description("@#federated")]FederatedCredentialInit? Federated = default,
    [property: Description("@#publicKey")]PublicKeyCredentialCreationOptions? PublicKey = default)
{
    [Category("optional")]
    public extern static CredentialCreationOptions OptionalSignal(
        [Description("@#signal")]AbortSignal? Signal = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalPassword(
        [Description("@#password")]PasswordCredentialInit? Password = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalFederated(
        [Description("@#federated")]FederatedCredentialInit? Federated = default);

    [Category("optional")]
    public extern static CredentialCreationOptions OptionalPublicKey(
        [Description("@#publicKey")]PublicKeyCredentialCreationOptions? PublicKey = default);
}

/// <summary>
/// CredentialData
/// </summary>
[ECMAScript]
[Description("@#CredentialData")]
public record CredentialData(
    [property: Description("@#id")]string? Id = default);

/// <summary>
/// CredentialPropertiesOutput
/// </summary>
[ECMAScript]
[Description("@#CredentialPropertiesOutput")]
public record CredentialPropertiesOutput(
    [property: Description("@#rk")]bool Rk = default,
    [property: Description("@#authenticatorDisplayName")]string? AuthenticatorDisplayName = default);

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
/// CryptoKeyPair
/// </summary>
[ECMAScript]
[Description("@#CryptoKeyPair")]
public record CryptoKeyPair(
    [property: Description("@#publicKey")]CryptoKey? PublicKey = default,
    [property: Description("@#privateKey")]CryptoKey? PrivateKey = default);

/// <summary>
/// CustomEventInit
/// </summary>
[ECMAScript]
[Description("@#CustomEventInit")]
public record CustomEventInit(
    [property: Description("@#detail")]object? Detail = default) : EventInit;

/// <summary>
/// DOMMatrix2DInit
/// </summary>
[ECMAScript]
[Description("@#DOMMatrix2DInit")]
public record DOMMatrix2DInit(
    [property: Description("@#a")]double A = default,
    [property: Description("@#b")]double B = default,
    [property: Description("@#c")]double C = default,
    [property: Description("@#d")]double D = default,
    [property: Description("@#e")]double E = default,
    [property: Description("@#f")]double F = default,
    [property: Description("@#m11")]double M11 = default,
    [property: Description("@#m12")]double M12 = default,
    [property: Description("@#m21")]double M21 = default,
    [property: Description("@#m22")]double M22 = default,
    [property: Description("@#m41")]double M41 = default,
    [property: Description("@#m42")]double M42 = default);

/// <summary>
/// DOMMatrixInit
/// </summary>
[ECMAScript]
[Description("@#DOMMatrixInit")]
public record DOMMatrixInit(
    [property: Description("@#m13")]double M13 = 0d,
    [property: Description("@#m14")]double M14 = 0d,
    [property: Description("@#m23")]double M23 = 0d,
    [property: Description("@#m24")]double M24 = 0d,
    [property: Description("@#m31")]double M31 = 0d,
    [property: Description("@#m32")]double M32 = 0d,
    [property: Description("@#m33")]double M33 = 1d,
    [property: Description("@#m34")]double M34 = 0d,
    [property: Description("@#m43")]double M43 = 0d,
    [property: Description("@#m44")]double M44 = 1d,
    [property: Description("@#is2D")]bool Is2D = default) : DOMMatrix2DInit;

/// <summary>
/// DOMPointInit
/// </summary>
[ECMAScript]
[Description("@#DOMPointInit")]
public record DOMPointInit(
    [property: Description("@#x")]double X = 0d,
    [property: Description("@#y")]double Y = 0d,
    [property: Description("@#z")]double Z = 0d,
    [property: Description("@#w")]double W = 1d);

/// <summary>
/// DOMQuadInit
/// </summary>
[ECMAScript]
[Description("@#DOMQuadInit")]
public record DOMQuadInit(
    [property: Description("@#p1")]DOMPointInit? P1 = default,
    [property: Description("@#p2")]DOMPointInit? P2 = default,
    [property: Description("@#p3")]DOMPointInit? P3 = default,
    [property: Description("@#p4")]DOMPointInit? P4 = default);

/// <summary>
/// DOMRectInit
/// </summary>
[ECMAScript]
[Description("@#DOMRectInit")]
public record DOMRectInit(
    [property: Description("@#x")]double X = 0d,
    [property: Description("@#y")]double Y = 0d,
    [property: Description("@#width")]double Width = 0d,
    [property: Description("@#height")]double Height = 0d);

/// <summary>
/// DelayOptions
/// </summary>
[ECMAScript]
[Description("@#DelayOptions")]
public record DelayOptions(
    [property: Description("@#maxDelayTime")]double MaxDelayTime = 1d,
    [property: Description("@#delayTime")]double DelayTime = 0d) : AudioNodeOptions;

/// <summary>
/// DetectedBarcode
/// </summary>
[ECMAScript]
[Description("@#DetectedBarcode")]
public record DetectedBarcode(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
    [property: Description("@#rawValue")]string? RawValue = default,
    [property: Description("@#format")]BarcodeFormat? Format = default,
    [property: Description("@#cornerPoints")]Point2D[]? CornerPoints = default);

/// <summary>
/// DetectedFace
/// </summary>
[ECMAScript]
[Description("@#DetectedFace")]
public record DetectedFace(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
    [property: Description("@#landmarks")]Landmark[]? Landmarks = default);

/// <summary>
/// DetectedText
/// </summary>
[ECMAScript]
[Description("@#DetectedText")]
public record DetectedText(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
    [property: Description("@#rawValue")]string? RawValue = default,
    [property: Description("@#cornerPoints")]Point2D[]? CornerPoints = default);

/// <summary>
/// DeviceMotionEventAccelerationInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventAccelerationInit")]
public record DeviceMotionEventAccelerationInit(
    [property: Description("@#x")]double? X = null,
    [property: Description("@#y")]double? Y = null,
    [property: Description("@#z")]double? Z = null);

/// <summary>
/// DeviceMotionEventInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventInit")]
public record DeviceMotionEventInit(
    [property: Description("@#acceleration")]DeviceMotionEventAccelerationInit? Acceleration = default,
    [property: Description("@#accelerationIncludingGravity")]DeviceMotionEventAccelerationInit? AccelerationIncludingGravity = default,
    [property: Description("@#rotationRate")]DeviceMotionEventRotationRateInit? RotationRate = default,
    [property: Description("@#interval")]double Interval = 0d) : EventInit;

/// <summary>
/// DeviceMotionEventRotationRateInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventRotationRateInit")]
public record DeviceMotionEventRotationRateInit(
    [property: Description("@#alpha")]double? Alpha = null,
    [property: Description("@#beta")]double? Beta = null,
    [property: Description("@#gamma")]double? Gamma = null);

/// <summary>
/// DeviceOrientationEventInit
/// </summary>
[ECMAScript]
[Description("@#DeviceOrientationEventInit")]
public record DeviceOrientationEventInit(
    [property: Description("@#alpha")]double? Alpha = null,
    [property: Description("@#beta")]double? Beta = null,
    [property: Description("@#gamma")]double? Gamma = null,
    [property: Description("@#absolute")]bool Absolute = false) : EventInit;

/// <summary>
/// DigitalCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#DigitalCredentialRequestOptions")]
public record DigitalCredentialRequestOptions(
    [property: Description("@#providers")]IdentityRequestProvider[]? Providers = default);

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
/// DirectoryPickerOptions
/// </summary>
[ECMAScript]
[Description("@#DirectoryPickerOptions")]
public record DirectoryPickerOptions(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#startIn")]StartInDirectory? StartIn = default,
    [property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read);

/// <summary>
/// DisconnectedAccount
/// </summary>
[ECMAScript]
[Description("@#DisconnectedAccount")]
public record DisconnectedAccount(
    [property: Description("@#account_id")]string? AccountId = default);

/// <summary>
/// DisplayMediaStreamOptions
/// </summary>
[ECMAScript]
[Description("@#DisplayMediaStreamOptions")]
public record DisplayMediaStreamOptions(
    [property: Description("@#video")]DisplayMediaStreamOptionsVideo? Video = default,
    [property: Description("@#audio")]DisplayMediaStreamOptionsAudio? Audio = default,
    [property: Description("@#controller")]CaptureController? Controller = default,
    [property: Description("@#selfBrowserSurface")]SelfCapturePreferenceEnum? SelfBrowserSurface = default,
    [property: Description("@#systemAudio")]SystemAudioPreferenceEnum? SystemAudio = default,
    [property: Description("@#surfaceSwitching")]SurfaceSwitchingPreferenceEnum? SurfaceSwitching = default,
    [property: Description("@#monitorTypeSurfaces")]MonitorTypeSurfacesEnum? MonitorTypeSurfaces = default);

/// <summary>
/// DocumentPictureInPictureEventInit
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPictureEventInit")]
public record DocumentPictureInPictureEventInit(
    [property: Description("@#window")]Window? Window = default) : EventInit;

/// <summary>
/// DocumentPictureInPictureOptions
/// </summary>
[ECMAScript]
[Description("@#DocumentPictureInPictureOptions")]
public record DocumentPictureInPictureOptions(
    [property: Description("@#width")]ulong Width = 0,
    [property: Description("@#height")]ulong Height = 0,
    [property: Description("@#disallowReturnToOpener")]bool DisallowReturnToOpener = false);

/// <summary>
/// DocumentTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#DocumentTimelineOptions")]
public record DocumentTimelineOptions(
    [property: Description("@#originTime")]double OriginTime = 0d);

/// <summary>
/// DoubleRange
/// </summary>
[ECMAScript]
[Description("@#DoubleRange")]
public record DoubleRange(
    [property: Description("@#max")]double Max = default,
    [property: Description("@#min")]double Min = default);

/// <summary>
/// DragEventInit
/// </summary>
[ECMAScript]
[Description("@#DragEventInit")]
public record DragEventInit(
    [property: Description("@#dataTransfer")]DataTransfer? DataTransfer = null) : MouseEventInit;

/// <summary>
/// DynamicsCompressorOptions
/// </summary>
[ECMAScript]
[Description("@#DynamicsCompressorOptions")]
public record DynamicsCompressorOptions(
    [property: Description("@#attack")]float Attack = 0.003f,
    [property: Description("@#knee")]float Knee = 30f,
    [property: Description("@#ratio")]float Ratio = 12f,
    [property: Description("@#release")]float Release = 0.25f,
    [property: Description("@#threshold")]float Threshold = -24f) : AudioNodeOptions;

/// <summary>
/// EcKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#EcKeyAlgorithm")]
public record EcKeyAlgorithm(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default) : KeyAlgorithm;

/// <summary>
/// EcKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#EcKeyGenParams")]
public record EcKeyGenParams(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default) : Algorithm;

/// <summary>
/// EcKeyImportParams
/// </summary>
[ECMAScript]
[Description("@#EcKeyImportParams")]
public record EcKeyImportParams(
    [property: Description("@#namedCurve")]NamedCurve? NamedCurve = default) : Algorithm;

/// <summary>
/// EcdhKeyDeriveParams
/// </summary>
[ECMAScript]
[Description("@#EcdhKeyDeriveParams")]
public record EcdhKeyDeriveParams(
    [property: Description("@#public")]CryptoKey? Public = default) : Algorithm;

/// <summary>
/// EcdsaParams
/// </summary>
[ECMAScript]
[Description("@#EcdsaParams")]
public record EcdsaParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default) : Algorithm;

/// <summary>
/// Ed448Params
/// </summary>
[ECMAScript]
[Description("@#Ed448Params")]
public record Ed448Params(
    [property: Description("@#context")]IBufferSource? Context = default) : Algorithm;

/// <summary>
/// EditContextInit
/// </summary>
[ECMAScript]
[Description("@#EditContextInit")]
public record EditContextInit(
    [property: Description("@#text")]string? Text = default,
    [property: Description("@#selectionStart")]uint SelectionStart = default,
    [property: Description("@#selectionEnd")]uint SelectionEnd = default);

/// <summary>
/// EffectTiming
/// </summary>
[ECMAScript]
[Description("@#EffectTiming")]
public record EffectTiming(
    [property: Description("@#fill")]FillMode Fill = FillMode.Auto,
    [property: Description("@#iterationStart")]double IterationStart = 0.0d,
    [property: Description("@#iterations")]double Iterations = 1.0d,
    [property: Description("@#direction")]PlaybackDirection Direction = PlaybackDirection.Normal,
    [property: Description("@#easing")]string? Easing = default,
    [property: Description("@#delay")]double Delay = default,
    [property: Description("@#endDelay")]double EndDelay = default,
    [property: Description("@#playbackRate")]double PlaybackRate = 1.0d,
    [property: Description("@#duration")]EffectTimingDuration? Duration = default)
{
    [Category("optional")]
    public extern static EffectTiming OptionalFillIterationStartIterations5(
        [Description("@#fill")]FillMode fill = FillMode.Auto,
        [Description("@#iterationStart")]double iterationStart = 0.0d,
        [Description("@#iterations")]double iterations = 1.0d,
        [Description("@#direction")]PlaybackDirection direction = PlaybackDirection.Normal,
        [Description("@#easing")]string? easing = default);

    [Category("optional")]
    public extern static EffectTiming OptionalDelayEndDelayPlaybackRate4(
        [Description("@#delay")]double Delay = default,
        [Description("@#endDelay")]double EndDelay = default,
        [Description("@#playbackRate")]double playbackRate = 1.0d,
        [Description("@#duration")]EffectTimingDuration? duration = default);
}

/// <summary>
/// ElementCreationOptions
/// </summary>
[ECMAScript]
[Description("@#ElementCreationOptions")]
public record ElementCreationOptions(
    [property: Description("@#is")]string? Is = default);

/// <summary>
/// ElementDefinitionOptions
/// </summary>
[ECMAScript]
[Description("@#ElementDefinitionOptions")]
public record ElementDefinitionOptions(
    [property: Description("@#extends")]string? Extends = default);

/// <summary>
/// EncodedAudioChunkInit
/// </summary>
[ECMAScript]
[Description("@#EncodedAudioChunkInit")]
public record EncodedAudioChunkInit(
    [property: Description("@#type")]EncodedAudioChunkType? Type = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#duration")]ulong Duration = default,
    [property: Description("@#data")]IBufferSource? Data = default,
    [property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// EncodedAudioChunkMetadata
/// </summary>
[ECMAScript]
[Description("@#EncodedAudioChunkMetadata")]
public record EncodedAudioChunkMetadata(
    [property: Description("@#decoderConfig")]AudioDecoderConfig? DecoderConfig = default);

/// <summary>
/// EncodedVideoChunkInit
/// </summary>
[ECMAScript]
[Description("@#EncodedVideoChunkInit")]
public record EncodedVideoChunkInit(
    [property: Description("@#type")]EncodedVideoChunkType? Type = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#duration")]ulong Duration = default,
    [property: Description("@#data")]IAllowSharedBufferSource? Data = default,
    [property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// EncodedVideoChunkMetadata
/// </summary>
[ECMAScript]
[Description("@#EncodedVideoChunkMetadata")]
public record EncodedVideoChunkMetadata(
    [property: Description("@#decoderConfig")]VideoDecoderConfig? DecoderConfig = default,
    [property: Description("@#svc")]SvcOutputMetadata? Svc = default,
    [property: Description("@#alphaSideData")]IBufferSource? AlphaSideData = default);

/// <summary>
/// ErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#ErrorEventInit")]
public record ErrorEventInit(
    [property: Description("@#message")]string? Message = default,
    [property: Description("@#filename")]string? Filename = default,
    [property: Description("@#lineno")]uint Lineno = 0,
    [property: Description("@#colno")]uint Colno = 0,
    [property: Description("@#error")]object? Error = default) : EventInit;

/// <summary>
/// EventInit
/// </summary>
[ECMAScript]
[Description("@#EventInit")]
public record EventInit(
    [property: Description("@#bubbles")]bool Bubbles = false,
    [property: Description("@#cancelable")]bool Cancelable = false,
    [property: Description("@#composed")]bool Composed = false);

/// <summary>
/// EventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#EventListenerOptions")]
public record EventListenerOptions(
    [property: Description("@#capture")]bool Capture = false);

/// <summary>
/// EventModifierInit
/// </summary>
[ECMAScript]
[Description("@#EventModifierInit")]
public record EventModifierInit(
    [property: Description("@#ctrlKey")]bool CtrlKey = false,
    [property: Description("@#shiftKey")]bool ShiftKey = false,
    [property: Description("@#altKey")]bool AltKey = false,
    [property: Description("@#metaKey")]bool MetaKey = false,
    [property: Description("@#modifierAltGraph")]bool ModifierAltGraph = false,
    [property: Description("@#modifierCapsLock")]bool ModifierCapsLock = false,
    [property: Description("@#modifierFn")]bool ModifierFn = false,
    [property: Description("@#modifierFnLock")]bool ModifierFnLock = false,
    [property: Description("@#modifierHyper")]bool ModifierHyper = false,
    [property: Description("@#modifierNumLock")]bool ModifierNumLock = false,
    [property: Description("@#modifierScrollLock")]bool ModifierScrollLock = false,
    [property: Description("@#modifierSuper")]bool ModifierSuper = false,
    [property: Description("@#modifierSymbol")]bool ModifierSymbol = false,
    [property: Description("@#modifierSymbolLock")]bool ModifierSymbolLock = false) : UIEventInit;

/// <summary>
/// EventSourceInit
/// </summary>
[ECMAScript]
[Description("@#EventSourceInit")]
public record EventSourceInit(
    [property: Description("@#withCredentials")]bool WithCredentials = false);

/// <summary>
/// ExtendableCookieChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableCookieChangeEventInit")]
public record ExtendableCookieChangeEventInit(
    [property: Description("@#changed")]CookieList? Changed = default,
    [property: Description("@#deleted")]CookieList? Deleted = default) : ExtendableEventInit;

/// <summary>
/// ExtendableMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#ExtendableMessageEventInit")]
public record ExtendableMessageEventInit(
    [property: Description("@#data")]object? Data = default,
    [property: Description("@#origin")]string? Origin = default,
    [property: Description("@#lastEventId")]string? LastEventId = default,
    [property: Description("@#source")]ExtendableMessageEventInitSource? Source = default,
    [property: Description("@#ports")]MessagePort[]? Ports = default) : ExtendableEventInit;

/// <summary>
/// FaceDetectorOptions
/// </summary>
[ECMAScript]
[Description("@#FaceDetectorOptions")]
public record FaceDetectorOptions(
    [property: Description("@#maxDetectedFaces")]ushort MaxDetectedFaces = default,
    [property: Description("@#fastMode")]bool FastMode = default);

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
    [property: Description("@#protocol")]string? Protocol = default) : CredentialData;

/// <summary>
/// FederatedCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#FederatedCredentialRequestOptions")]
public record FederatedCredentialRequestOptions(
    [property: Description("@#providers")]string[]? Providers = default,
    [property: Description("@#protocols")]string[]? Protocols = default);

/// <summary>
/// FenceEvent
/// </summary>
[ECMAScript]
[Description("@#FenceEvent")]
public record FenceEvent(
    [property: Description("@#eventType")]string? EventType = default,
    [property: Description("@#eventData")]string? EventData = default,
    [property: Description("@#destination")]FenceReportingDestination[]? Destination = default,
    [property: Description("@#once")]bool Once = false,
    [property: Description("@#crossOriginExposed")]bool CrossOriginExposed = false,
    [property: Description("@#destinationURL")]string? DestinationURL = default);

/// <summary>
/// FetchEventInit
/// </summary>
[ECMAScript]
[Description("@#FetchEventInit")]
public record FetchEventInit(
    [property: Description("@#request")]Request? Request = default,
    [property: Description("@#preloadResponse")]PromiseResult<object>? PreloadResponse = default,
    [property: Description("@#clientId")]string? ClientId = default,
    [property: Description("@#resultingClientId")]string? ResultingClientId = default,
    [property: Description("@#replacesClientId")]string? ReplacesClientId = default,
    [property: Description("@#handled")]PromiseResult? Handled = default) : ExtendableEventInit;

/// <summary>
/// FilePickerAcceptType
/// </summary>
[ECMAScript]
[Description("@#FilePickerAcceptType")]
public record FilePickerAcceptType(
    [property: Description("@#description")]string? Description = default,
    [property: Description("@#accept")]Dictionary<string, FilePickerAcceptTypeAcceptValue>? Accept = default);

/// <summary>
/// FilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#FilePickerOptions")]
public record FilePickerOptions(
    [property: Description("@#types")]FilePickerAcceptType[]? Types = default,
    [property: Description("@#excludeAcceptAllOption")]bool ExcludeAcceptAllOption = false,
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#startIn")]StartInDirectory? StartIn = default);

/// <summary>
/// FilePropertyBag
/// </summary>
[ECMAScript]
[Description("@#FilePropertyBag")]
public record FilePropertyBag(
    [property: Description("@#lastModified")]long LastModified = default) : BlobPropertyBag;

/// <summary>
/// FileSystemCreateWritableOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemCreateWritableOptions")]
public record FileSystemCreateWritableOptions(
    [property: Description("@#keepExistingData")]bool KeepExistingData = false);

/// <summary>
/// FileSystemFlags
/// </summary>
[ECMAScript]
[Description("@#FileSystemFlags")]
public record FileSystemFlags(
    [property: Description("@#create")]bool Create = false,
    [property: Description("@#exclusive")]bool Exclusive = false);

/// <summary>
/// FileSystemGetDirectoryOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemGetDirectoryOptions")]
public record FileSystemGetDirectoryOptions(
    [property: Description("@#create")]bool Create = false);

/// <summary>
/// FileSystemGetFileOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemGetFileOptions")]
public record FileSystemGetFileOptions(
    [property: Description("@#create")]bool Create = false);

/// <summary>
/// FileSystemHandlePermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#FileSystemHandlePermissionDescriptor")]
public record FileSystemHandlePermissionDescriptor(
    [property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read);

/// <summary>
/// FileSystemPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#FileSystemPermissionDescriptor")]
public record FileSystemPermissionDescriptor(
    [property: Description("@#handle")]FileSystemHandle? Handle = default,
    [property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read) : PermissionDescriptor;

/// <summary>
/// FileSystemReadWriteOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemReadWriteOptions")]
public record FileSystemReadWriteOptions(
    [property: Description("@#at")]ulong At = default);

/// <summary>
/// FileSystemRemoveOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemRemoveOptions")]
public record FileSystemRemoveOptions(
    [property: Description("@#recursive")]bool Recursive = false);

/// <summary>
/// FlacEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#FlacEncoderConfig")]
public record FlacEncoderConfig(
    [property: Description("@#blockSize")]uint BlockSize = 0,
    [property: Description("@#compressLevel")]uint CompressLevel = 5);

/// <summary>
/// FocusEventInit
/// </summary>
[ECMAScript]
[Description("@#FocusEventInit")]
public record FocusEventInit(
    [property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null) : UIEventInit;

/// <summary>
/// FocusOptions
/// </summary>
[ECMAScript]
[Description("@#FocusOptions")]
public record FocusOptions(
    [property: Description("@#preventScroll")]bool PreventScroll = false,
    [property: Description("@#focusVisible")]bool FocusVisible = default);

/// <summary>
/// FocusableAreasOption
/// </summary>
[ECMAScript]
[Description("@#FocusableAreasOption")]
public record FocusableAreasOption(
    [property: Description("@#mode")]FocusableAreaSearchMode? Mode = default);

/// <summary>
/// FontFaceDescriptors
/// </summary>
[ECMAScript]
[Description("@#FontFaceDescriptors")]
public record FontFaceDescriptors(
    [property: Description("@#style")]string? Style = default,
    [property: Description("@#weight")]string? Weight = default,
    [property: Description("@#stretch")]string? Stretch = default,
    [property: Description("@#unicodeRange")]string? UnicodeRange = default,
    [property: Description("@#featureSettings")]string? FeatureSettings = default,
    [property: Description("@#variationSettings")]string? VariationSettings = default,
    [property: Description("@#display")]string? Display = default,
    [property: Description("@#ascentOverride")]string? AscentOverride = default,
    [property: Description("@#descentOverride")]string? DescentOverride = default,
    [property: Description("@#lineGapOverride")]string? LineGapOverride = default);

/// <summary>
/// FontFaceSetLoadEventInit
/// </summary>
[ECMAScript]
[Description("@#FontFaceSetLoadEventInit")]
public record FontFaceSetLoadEventInit(
    [property: Description("@#fontfaces")]FontFace[]? Fontfaces = default) : EventInit;

/// <summary>
/// FormDataEventInit
/// </summary>
[ECMAScript]
[Description("@#FormDataEventInit")]
public record FormDataEventInit(
    [property: Description("@#formData")]FormData? FormData = default) : EventInit;

/// <summary>
/// FullscreenOptions
/// </summary>
[ECMAScript]
[Description("@#FullscreenOptions")]
public record FullscreenOptions(
    [property: Description("@#navigationUI")]FullscreenNavigationUI NavigationUI = FullscreenNavigationUI.Auto,
    [property: Description("@#screen")]ScreenDetailed? Screen = default)
{
    [Category("optional")]
    public extern static FullscreenOptions OptionalNavigationUI(
        [Description("@#navigationUI")]FullscreenNavigationUI navigationUI = FullscreenNavigationUI.Auto);

    [Category("optional")]
    public extern static FullscreenOptions OptionalScreen(
        [Description("@#screen")]ScreenDetailed? Screen = default);
}

/// <summary>
/// GainOptions
/// </summary>
[ECMAScript]
[Description("@#GainOptions")]
public record GainOptions(
    [property: Description("@#gain")]float Gain = 1.0f) : AudioNodeOptions;

/// <summary>
/// GamepadEffectParameters
/// </summary>
[ECMAScript]
[Description("@#GamepadEffectParameters")]
public record GamepadEffectParameters(
    [property: Description("@#duration")]ulong Duration = 0,
    [property: Description("@#startDelay")]ulong StartDelay = 0,
    [property: Description("@#strongMagnitude")]double StrongMagnitude = 0.0d,
    [property: Description("@#weakMagnitude")]double WeakMagnitude = 0.0d,
    [property: Description("@#leftTrigger")]double LeftTrigger = 0.0d,
    [property: Description("@#rightTrigger")]double RightTrigger = 0.0d);

/// <summary>
/// GamepadEventInit
/// </summary>
[ECMAScript]
[Description("@#GamepadEventInit")]
public record GamepadEventInit(
    [property: Description("@#gamepad")]Gamepad? Gamepad = default) : EventInit;

/// <summary>
/// GenerateBidInterestGroup
/// </summary>
[ECMAScript]
[Description("@#GenerateBidInterestGroup")]
public record GenerateBidInterestGroup(
    [property: Description("@#owner")]string? Owner = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#lifetimeMs")]double LifetimeMs = default,
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
    [property: Description("@#userBiddingSignals")]object? UserBiddingSignals = default,
    [property: Description("@#ads")]AuctionAd[]? Ads = default,
    [property: Description("@#adComponents")]AuctionAd[]? AdComponents = default);

/// <summary>
/// GenerateBidOutput
/// </summary>
[ECMAScript]
[Description("@#GenerateBidOutput")]
public record GenerateBidOutput(
    [property: Description("@#bid")]double Bid = -1d,
    [property: Description("@#bidCurrency")]string? BidCurrency = default,
    [property: Description("@#render")]GenerateBidOutputRender? Render = default,
    [property: Description("@#ad")]object? Ad = default,
    [property: Description("@#adComponents")]GenerateBidOutputAdComponents[]? AdComponents = default,
    [property: Description("@#adCost")]double AdCost = default,
    [property: Description("@#modelingSignals")]double ModelingSignals = default,
    [property: Description("@#allowComponentAuction")]bool AllowComponentAuction = false);

/// <summary>
/// GenerateTestReportParameters
/// </summary>
[ECMAScript]
[Description("@#GenerateTestReportParameters")]
public record GenerateTestReportParameters(
    [property: Description("@#message")]string? Message = default,
    [property: Description("@#group")]string? Group = default);

/// <summary>
/// GeolocationSensorReading
/// </summary>
[ECMAScript]
[Description("@#GeolocationSensorReading")]
public record GeolocationSensorReading(
    [property: Description("@#timestamp")]double Timestamp = default,
    [property: Description("@#latitude")]double Latitude = default,
    [property: Description("@#longitude")]double Longitude = default,
    [property: Description("@#altitude")]double Altitude = default,
    [property: Description("@#accuracy")]double Accuracy = default,
    [property: Description("@#altitudeAccuracy")]double AltitudeAccuracy = default,
    [property: Description("@#heading")]double Heading = default,
    [property: Description("@#speed")]double Speed = default);

/// <summary>
/// GetAnimationsOptions
/// </summary>
[ECMAScript]
[Description("@#GetAnimationsOptions")]
public record GetAnimationsOptions(
    [property: Description("@#subtree")]bool Subtree = false);

/// <summary>
/// GetHTMLOptions
/// </summary>
[ECMAScript]
[Description("@#GetHTMLOptions")]
public record GetHTMLOptions(
    [property: Description("@#serializableShadowRoots")]bool SerializableShadowRoots = false,
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// GetNotificationOptions
/// </summary>
[ECMAScript]
[Description("@#GetNotificationOptions")]
public record GetNotificationOptions(
    [property: Description("@#tag")]string? Tag = default);

/// <summary>
/// GetRootNodeOptions
/// </summary>
[ECMAScript]
[Description("@#GetRootNodeOptions")]
public record GetRootNodeOptions(
    [property: Description("@#composed")]bool Composed = false);

/// <summary>
/// GyroscopeSensorOptions
/// </summary>
[ECMAScript]
[Description("@#GyroscopeSensorOptions")]
public record GyroscopeSensorOptions(
    [property: Description("@#referenceFrame")]GyroscopeLocalCoordinateSystem ReferenceFrame = GyroscopeLocalCoordinateSystem.Device) : SensorOptions;

/// <summary>
/// HIDCollectionInfo
/// </summary>
[ECMAScript]
[Description("@#HIDCollectionInfo")]
public record HIDCollectionInfo(
    [property: Description("@#usagePage")]ushort UsagePage = default,
    [property: Description("@#usage")]ushort Usage = default,
    [property: Description("@#type")]byte Type = default,
    [property: Description("@#children")]HIDCollectionInfo[]? Children = default,
    [property: Description("@#inputReports")]HIDReportInfo[]? InputReports = default,
    [property: Description("@#outputReports")]HIDReportInfo[]? OutputReports = default,
    [property: Description("@#featureReports")]HIDReportInfo[]? FeatureReports = default);

/// <summary>
/// HIDConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#HIDConnectionEventInit")]
public record HIDConnectionEventInit(
    [property: Description("@#device")]HIDDevice? Device = default) : EventInit;

/// <summary>
/// HIDDeviceFilter
/// </summary>
[ECMAScript]
[Description("@#HIDDeviceFilter")]
public record HIDDeviceFilter(
    [property: Description("@#vendorId")]uint VendorId = default,
    [property: Description("@#productId")]ushort ProductId = default,
    [property: Description("@#usagePage")]ushort UsagePage = default,
    [property: Description("@#usage")]ushort Usage = default);

/// <summary>
/// HIDDeviceRequestOptions
/// </summary>
[ECMAScript]
[Description("@#HIDDeviceRequestOptions")]
public record HIDDeviceRequestOptions(
    [property: Description("@#filters")]HIDDeviceFilter[]? Filters = default,
    [property: Description("@#exclusionFilters")]HIDDeviceFilter[]? ExclusionFilters = default);

/// <summary>
/// HIDInputReportEventInit
/// </summary>
[ECMAScript]
[Description("@#HIDInputReportEventInit")]
public record HIDInputReportEventInit(
    [property: Description("@#device")]HIDDevice? Device = default,
    [property: Description("@#reportId")]byte ReportId = default,
    [property: Description("@#data")]DataView? Data = default) : EventInit;

/// <summary>
/// HIDReportInfo
/// </summary>
[ECMAScript]
[Description("@#HIDReportInfo")]
public record HIDReportInfo(
    [property: Description("@#reportId")]byte ReportId = default,
    [property: Description("@#items")]HIDReportItem[]? Items = default);

/// <summary>
/// HIDReportItem
/// </summary>
[ECMAScript]
[Description("@#HIDReportItem")]
public record HIDReportItem(
    [property: Description("@#isAbsolute")]bool IsAbsolute = default,
    [property: Description("@#isArray")]bool IsArray = default,
    [property: Description("@#isBufferedBytes")]bool IsBufferedBytes = default,
    [property: Description("@#isConstant")]bool IsConstant = default,
    [property: Description("@#isLinear")]bool IsLinear = default,
    [property: Description("@#isRange")]bool IsRange = default,
    [property: Description("@#isVolatile")]bool IsVolatile = default,
    [property: Description("@#hasNull")]bool HasNull = default,
    [property: Description("@#hasPreferredState")]bool HasPreferredState = default,
    [property: Description("@#wrap")]bool Wrap = default,
    [property: Description("@#usages")]uint[]? Usages = default,
    [property: Description("@#usageMinimum")]uint UsageMinimum = default,
    [property: Description("@#usageMaximum")]uint UsageMaximum = default,
    [property: Description("@#reportSize")]ushort ReportSize = default,
    [property: Description("@#reportCount")]ushort ReportCount = default,
    [property: Description("@#unitExponent")]sbyte UnitExponent = default,
    [property: Description("@#unitSystem")]HIDUnitSystem? UnitSystem = default,
    [property: Description("@#unitFactorLengthExponent")]sbyte UnitFactorLengthExponent = default,
    [property: Description("@#unitFactorMassExponent")]sbyte UnitFactorMassExponent = default,
    [property: Description("@#unitFactorTimeExponent")]sbyte UnitFactorTimeExponent = default,
    [property: Description("@#unitFactorTemperatureExponent")]sbyte UnitFactorTemperatureExponent = default,
    [property: Description("@#unitFactorCurrentExponent")]sbyte UnitFactorCurrentExponent = default,
    [property: Description("@#unitFactorLuminousIntensityExponent")]sbyte UnitFactorLuminousIntensityExponent = default,
    [property: Description("@#logicalMinimum")]int LogicalMinimum = default,
    [property: Description("@#logicalMaximum")]int LogicalMaximum = default,
    [property: Description("@#physicalMinimum")]int PhysicalMinimum = default,
    [property: Description("@#physicalMaximum")]int PhysicalMaximum = default,
    [property: Description("@#strings")]string[]? Strings = default);

/// <summary>
/// HMACGetSecretInput
/// </summary>
[ECMAScript]
[Description("@#HMACGetSecretInput")]
public record HMACGetSecretInput(
    [property: Description("@#salt1")]ArrayBuffer? Salt1 = default,
    [property: Description("@#salt2")]ArrayBuffer? Salt2 = default);

/// <summary>
/// HMACGetSecretOutput
/// </summary>
[ECMAScript]
[Description("@#HMACGetSecretOutput")]
public record HMACGetSecretOutput(
    [property: Description("@#output1")]ArrayBuffer? Output1 = default,
    [property: Description("@#output2")]ArrayBuffer? Output2 = default);

/// <summary>
/// HashChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#HashChangeEventInit")]
public record HashChangeEventInit(
    [property: Description("@#oldURL")]string? OldURL = default,
    [property: Description("@#newURL")]string? NewURL = default) : EventInit;

/// <summary>
/// HevcEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#HevcEncoderConfig")]
public record HevcEncoderConfig(
    [property: Description("@#format")]HevcBitstreamFormat Format = HevcBitstreamFormat.Hevc);

/// <summary>
/// HkdfParams
/// </summary>
[ECMAScript]
[Description("@#HkdfParams")]
public record HkdfParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
    [property: Description("@#salt")]IBufferSource? Salt = default,
    [property: Description("@#info")]IBufferSource? Info = default) : Algorithm;

/// <summary>
/// HmacImportParams
/// </summary>
[ECMAScript]
[Description("@#HmacImportParams")]
public record HmacImportParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
    [property: Description("@#length")]uint Length = default) : Algorithm;

/// <summary>
/// HmacKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#HmacKeyAlgorithm")]
public record HmacKeyAlgorithm(
    [property: Description("@#hash")]KeyAlgorithm? Hash = default,
    [property: Description("@#length")]uint Length = default) : KeyAlgorithm;

/// <summary>
/// HmacKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#HmacKeyGenParams")]
public record HmacKeyGenParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default,
    [property: Description("@#length")]uint Length = default) : Algorithm;

/// <summary>
/// IDBDatabaseInfo
/// </summary>
[ECMAScript]
[Description("@#IDBDatabaseInfo")]
public record IDBDatabaseInfo(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#version")]ulong Version = default);

/// <summary>
/// IDBIndexParameters
/// </summary>
[ECMAScript]
[Description("@#IDBIndexParameters")]
public record IDBIndexParameters(
    [property: Description("@#unique")]bool Unique = false,
    [property: Description("@#multiEntry")]bool MultiEntry = false);

/// <summary>
/// IDBObjectStoreParameters
/// </summary>
[ECMAScript]
[Description("@#IDBObjectStoreParameters")]
public record IDBObjectStoreParameters(
    [property: Description("@#keyPath")]IDBObjectStoreParametersKeyPath? KeyPath = default,
    [property: Description("@#autoIncrement")]bool AutoIncrement = false);

/// <summary>
/// IDBTransactionOptions
/// </summary>
[ECMAScript]
[Description("@#IDBTransactionOptions")]
public record IDBTransactionOptions(
    [property: Description("@#durability")]IDBTransactionDurability Durability = IDBTransactionDurability.Default);

/// <summary>
/// IDBVersionChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#IDBVersionChangeEventInit")]
public record IDBVersionChangeEventInit(
    [property: Description("@#oldVersion")]ulong OldVersion = 0,
    [property: Description("@#newVersion")]ulong? NewVersion = null) : EventInit;

/// <summary>
/// IIRFilterOptions
/// </summary>
[ECMAScript]
[Description("@#IIRFilterOptions")]
public record IIRFilterOptions(
    [property: Description("@#feedforward")]double[]? Feedforward = default,
    [property: Description("@#feedback")]double[]? Feedback = default) : AudioNodeOptions;

/// <summary>
/// IdentityCredentialDisconnectOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityCredentialDisconnectOptions")]
public record IdentityCredentialDisconnectOptions(
    [property: Description("@#accountHint")]string? AccountHint = default) : IdentityProviderConfig;

/// <summary>
/// IdentityCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityCredentialRequestOptions")]
public record IdentityCredentialRequestOptions(
    [property: Description("@#providers")]IdentityProviderRequestOptions[]? Providers = default,
    [property: Description("@#context")]IdentityCredentialRequestOptionsContext Context = IdentityCredentialRequestOptionsContext.Signin);

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
    [property: Description("@#branding")]IdentityProviderBranding? Branding = default);

/// <summary>
/// IdentityProviderAccount
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderAccount")]
public record IdentityProviderAccount(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#email")]string? Email = default,
    [property: Description("@#given_name")]string? GivenName = default,
    [property: Description("@#picture")]string? Picture = default,
    [property: Description("@#approved_clients")]string[]? ApprovedClients = default,
    [property: Description("@#login_hints")]string[]? LoginHints = default,
    [property: Description("@#domain_hints")]string[]? DomainHints = default);

/// <summary>
/// IdentityProviderAccountList
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderAccountList")]
public record IdentityProviderAccountList(
    [property: Description("@#accounts")]IdentityProviderAccount[]? Accounts = default);

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
/// IdentityProviderClientMetadata
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderClientMetadata")]
public record IdentityProviderClientMetadata(
    [property: Description("@#privacy_policy_url")]string? PrivacyPolicyUrl = default,
    [property: Description("@#terms_of_service_url")]string? TermsOfServiceUrl = default);

/// <summary>
/// IdentityProviderConfig
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderConfig")]
public record IdentityProviderConfig(
    [property: Description("@#configURL")]string? ConfigURL = default,
    [property: Description("@#clientId")]string? ClientId = default);

/// <summary>
/// IdentityProviderIcon
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderIcon")]
public record IdentityProviderIcon(
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#size")]uint Size = default);

/// <summary>
/// IdentityProviderRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderRequestOptions")]
public record IdentityProviderRequestOptions(
    [property: Description("@#nonce")]string? Nonce = default,
    [property: Description("@#loginHint")]string? LoginHint = default,
    [property: Description("@#domainHint")]string? DomainHint = default) : IdentityProviderConfig;

/// <summary>
/// IdentityProviderToken
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderToken")]
public record IdentityProviderToken(
    [property: Description("@#token")]string? Token = default);

/// <summary>
/// IdentityProviderWellKnown
/// </summary>
[ECMAScript]
[Description("@#IdentityProviderWellKnown")]
public record IdentityProviderWellKnown(
    [property: Description("@#provider_urls")]string[]? ProviderUrls = default);

/// <summary>
/// IdentityRequestProvider
/// </summary>
[ECMAScript]
[Description("@#IdentityRequestProvider")]
public record IdentityRequestProvider(
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#request")]string? Request = default);

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
/// IdleOptions
/// </summary>
[ECMAScript]
[Description("@#IdleOptions")]
public record IdleOptions(
    [property: Description("@#threshold")]ulong Threshold = default,
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// IdleRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdleRequestOptions")]
public record IdleRequestOptions(
    [property: Description("@#timeout")]uint Timeout = default);

/// <summary>
/// ImageBitmapOptions
/// </summary>
[ECMAScript]
[Description("@#ImageBitmapOptions")]
public record ImageBitmapOptions(
    [property: Description("@#imageOrientation")]ImageOrientation ImageOrientation = ImageOrientation.FromImage,
    [property: Description("@#premultiplyAlpha")]PremultiplyAlpha PremultiplyAlpha = PremultiplyAlpha.Default,
    [property: Description("@#colorSpaceConversion")]ColorSpaceConversion ColorSpaceConversion = ColorSpaceConversion.Default,
    [property: Description("@#resizeWidth")]uint ResizeWidth = default,
    [property: Description("@#resizeHeight")]uint ResizeHeight = default,
    [property: Description("@#resizeQuality")]ResizeQuality ResizeQuality = ResizeQuality.Low);

/// <summary>
/// ImageBitmapRenderingContextSettings
/// </summary>
[ECMAScript]
[Description("@#ImageBitmapRenderingContextSettings")]
public record ImageBitmapRenderingContextSettings(
    [property: Description("@#alpha")]bool Alpha = false);

/// <summary>
/// ImageDataSettings
/// </summary>
[ECMAScript]
[Description("@#ImageDataSettings")]
public record ImageDataSettings(
    [property: Description("@#colorSpace")]PredefinedColorSpace? ColorSpace = default);

/// <summary>
/// ImageDecodeOptions
/// </summary>
[ECMAScript]
[Description("@#ImageDecodeOptions")]
public record ImageDecodeOptions(
    [property: Description("@#frameIndex")]uint FrameIndex = 0,
    [property: Description("@#completeFramesOnly")]bool CompleteFramesOnly = false);

/// <summary>
/// ImageDecodeResult
/// </summary>
[ECMAScript]
[Description("@#ImageDecodeResult")]
public record ImageDecodeResult(
    [property: Description("@#image")]VideoFrame? Image = default,
    [property: Description("@#complete")]bool Complete = default);

/// <summary>
/// ImageDecoderInit
/// </summary>
[ECMAScript]
[Description("@#ImageDecoderInit")]
public record ImageDecoderInit(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#data")]ImageBufferSource? Data = default,
    [property: Description("@#colorSpaceConversion")]ColorSpaceConversion ColorSpaceConversion = ColorSpaceConversion.Default,
    [property: Description("@#desiredWidth")]uint DesiredWidth = default,
    [property: Description("@#desiredHeight")]uint DesiredHeight = default,
    [property: Description("@#preferAnimation")]bool PreferAnimation = default,
    [property: Description("@#transfer")]ArrayBuffer[]? Transfer = default);

/// <summary>
/// ImageEncodeOptions
/// </summary>
[ECMAScript]
[Description("@#ImageEncodeOptions")]
public record ImageEncodeOptions(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#quality")]double Quality = default);

/// <summary>
/// ImageResource
/// </summary>
[ECMAScript]
[Description("@#ImageResource")]
public record ImageResource(
    [property: Description("@#src")]string? Src = default,
    [property: Description("@#sizes")]string? Sizes = default,
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#label")]string? Label = default);

/// <summary>
/// InkPresenterParam
/// </summary>
[ECMAScript]
[Description("@#InkPresenterParam")]
public record InkPresenterParam(
    [property: Description("@#presentationArea")]Element? PresentationArea = null);

/// <summary>
/// InkTrailStyle
/// </summary>
[ECMAScript]
[Description("@#InkTrailStyle")]
public record InkTrailStyle(
    [property: Description("@#color")]string? Color = default,
    [property: Description("@#diameter")]double Diameter = default);

/// <summary>
/// InputDeviceCapabilitiesInit
/// </summary>
[ECMAScript]
[Description("@#InputDeviceCapabilitiesInit")]
public record InputDeviceCapabilitiesInit(
    [property: Description("@#firesTouchEvents")]bool FiresTouchEvents = false,
    [property: Description("@#pointerMovementScrolls")]bool PointerMovementScrolls = false);

/// <summary>
/// InputEventInit
/// </summary>
[ECMAScript]
[Description("@#InputEventInit")]
public record InputEventInit(
    [property: Description("@#dataTransfer")]DataTransfer? DataTransfer = null,
    [property: Description("@#targetRanges")]StaticRange[]? TargetRanges = default,
    [property: Description("@#data")]string? Data = null,
    [property: Description("@#isComposing")]bool IsComposing = false,
    [property: Description("@#inputType")]string? InputType = default) : UIEventInit
{
    [Category("optional")]
    public extern static InputEventInit OptionalDataTransferTargetRanges(
        [Description("@#dataTransfer")]DataTransfer? dataTransfer = null,
        [Description("@#targetRanges")]StaticRange[]? targetRanges = default);

    [Category("optional")]
    public extern static InputEventInit OptionalDataIsComposingInputType(
        [Description("@#data")]string? data = null,
        [Description("@#isComposing")]bool isComposing = false,
        [Description("@#inputType")]string? inputType = default);
}

/// <summary>
/// IntersectionObserverEntryInit
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserverEntryInit")]
public record IntersectionObserverEntryInit(
    [property: Description("@#time")]double Time = default,
    [property: Description("@#rootBounds")]DOMRectInit? RootBounds = default,
    [property: Description("@#boundingClientRect")]DOMRectInit? BoundingClientRect = default,
    [property: Description("@#intersectionRect")]DOMRectInit? IntersectionRect = default,
    [property: Description("@#isIntersecting")]bool IsIntersecting = default,
    [property: Description("@#intersectionRatio")]double IntersectionRatio = default,
    [property: Description("@#target")]Element? Target = default);

/// <summary>
/// IntersectionObserverInit
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserverInit")]
public record IntersectionObserverInit(
    [property: Description("@#root")]IntersectionObserverInitRoot? Root = default,
    [property: Description("@#rootMargin")]string? RootMargin = default,
    [property: Description("@#scrollMargin")]string? ScrollMargin = default,
    [property: Description("@#threshold")]IntersectionObserverInitThreshold? Threshold = default);

/// <summary>
/// IsInputPendingOptions
/// </summary>
[ECMAScript]
[Description("@#IsInputPendingOptions")]
public record IsInputPendingOptions(
    [property: Description("@#includeContinuous")]bool IncludeContinuous = false);

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
/// JsonWebKey
/// </summary>
[ECMAScript]
[Description("@#JsonWebKey")]
public record JsonWebKey(
    [property: Description("@#kty")]string? Kty = default,
    [property: Description("@#use")]string? Use = default,
    [property: Description("@#key_ops")]string[]? KeyOps = default,
    [property: Description("@#alg")]string? Alg = default,
    [property: Description("@#ext")]bool Ext = default,
    [property: Description("@#crv")]string? Crv = default,
    [property: Description("@#x")]string? X = default,
    [property: Description("@#y")]string? Y = default,
    [property: Description("@#d")]string? D = default,
    [property: Description("@#n")]string? N = default,
    [property: Description("@#e")]string? E = default,
    [property: Description("@#p")]string? P = default,
    [property: Description("@#q")]string? Q = default,
    [property: Description("@#dp")]string? Dp = default,
    [property: Description("@#dq")]string? Dq = default,
    [property: Description("@#qi")]string? Qi = default,
    [property: Description("@#oth")]RsaOtherPrimesInfo[]? Oth = default,
    [property: Description("@#k")]string? K = default);

/// <summary>
/// KeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#KeyAlgorithm")]
public record KeyAlgorithm(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// KeySystemTrackConfiguration
/// </summary>
[ECMAScript]
[Description("@#KeySystemTrackConfiguration")]
public record KeySystemTrackConfiguration(
    [property: Description("@#robustness")]string? Robustness = default,
    [property: Description("@#encryptionScheme")]string? EncryptionScheme = null);

/// <summary>
/// KeyboardEventInit
/// </summary>
[ECMAScript]
[Description("@#KeyboardEventInit")]
public record KeyboardEventInit(
    [property: Description("@#key")]string? Key = default,
    [property: Description("@#code")]string? Code = default,
    [property: Description("@#location")]uint Location = 0,
    [property: Description("@#repeat")]bool Repeat = false,
    [property: Description("@#isComposing")]bool IsComposing = false,
    [property: Description("@#charCode")]uint CharCode = 0,
    [property: Description("@#keyCode")]uint KeyCode = 0) : EventModifierInit
{
    [Category("optional")]
    public extern static KeyboardEventInit OptionalKeyCodeLocation5(
        [Description("@#key")]string? key = default,
        [Description("@#code")]string? code = default,
        [Description("@#location")]uint location = 0,
        [Description("@#repeat")]bool repeat = false,
        [Description("@#isComposing")]bool isComposing = false);

    [Category("optional")]
    public extern static KeyboardEventInit OptionalCharCodeKeyCode(
        [Description("@#charCode")]uint charCode = 0,
        [Description("@#keyCode")]uint keyCode = 0);
}

/// <summary>
/// KeyframeAnimationOptions
/// </summary>
[ECMAScript]
[Description("@#KeyframeAnimationOptions")]
public record KeyframeAnimationOptions(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#timeline")]AnimationTimeline? Timeline = default,
    [property: Description("@#rangeStart")]KeyframeAnimationOptionsRangeStart? RangeStart = default,
    [property: Description("@#rangeEnd")]KeyframeAnimationOptionsRangeEnd? RangeEnd = default) : KeyframeEffectOptions
{
    [Category("optional")]
    public extern static KeyframeAnimationOptions OptionalIdTimeline(
        [Description("@#id")]string? id = default,
        [Description("@#timeline")]AnimationTimeline? Timeline = default);

    [Category("optional")]
    public extern static KeyframeAnimationOptions OptionalRangeStartRangeEnd(
        [Description("@#rangeStart")]KeyframeAnimationOptionsRangeStart? rangeStart = default,
        [Description("@#rangeEnd")]KeyframeAnimationOptionsRangeEnd? rangeEnd = default);
}

/// <summary>
/// KeyframeEffectOptions
/// </summary>
[ECMAScript]
[Description("@#KeyframeEffectOptions")]
public record KeyframeEffectOptions(
    [property: Description("@#composite")]CompositeOperation Composite = CompositeOperation.Replace,
    [property: Description("@#pseudoElement")]string? PseudoElement = null,
    [property: Description("@#iterationComposite")]IterationCompositeOperation IterationComposite = IterationCompositeOperation.Replace) : EffectTiming
{
    [Category("optional")]
    public extern static KeyframeEffectOptions OptionalCompositePseudoElement(
        [Description("@#composite")]CompositeOperation composite = CompositeOperation.Replace,
        [Description("@#pseudoElement")]string? pseudoElement = null);

    [Category("optional")]
    public extern static KeyframeEffectOptions OptionalIterationComposite(
        [Description("@#iterationComposite")]IterationCompositeOperation iterationComposite = IterationCompositeOperation.Replace);
}

/// <summary>
/// Landmark
/// </summary>
[ECMAScript]
[Description("@#Landmark")]
public record Landmark(
    [property: Description("@#locations")]Point2D[]? Locations = default,
    [property: Description("@#type")]LandmarkType? Type = default);

/// <summary>
/// LockInfo
/// </summary>
[ECMAScript]
[Description("@#LockInfo")]
public record LockInfo(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#mode")]LockMode? Mode = default,
    [property: Description("@#clientId")]string? ClientId = default);

/// <summary>
/// LockManagerSnapshot
/// </summary>
[ECMAScript]
[Description("@#LockManagerSnapshot")]
public record LockManagerSnapshot(
    [property: Description("@#held")]LockInfo[]? Held = default,
    [property: Description("@#pending")]LockInfo[]? Pending = default);

/// <summary>
/// LockOptions
/// </summary>
[ECMAScript]
[Description("@#LockOptions")]
public record LockOptions(
    [property: Description("@#mode")]LockMode Mode = LockMode.Exclusive,
    [property: Description("@#ifAvailable")]bool IfAvailable = false,
    [property: Description("@#steal")]bool Steal = false,
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// MIDIConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#MIDIConnectionEventInit")]
public record MIDIConnectionEventInit(
    [property: Description("@#port")]MIDIPort? Port = default) : EventInit;

/// <summary>
/// MIDIMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MIDIMessageEventInit")]
public record MIDIMessageEventInit(
    [property: Description("@#data")]Uint8Array? Data = default) : EventInit;

/// <summary>
/// MIDIOptions
/// </summary>
[ECMAScript]
[Description("@#MIDIOptions")]
public record MIDIOptions(
    [property: Description("@#sysex")]bool Sysex = default,
    [property: Description("@#software")]bool Software = default);

/// <summary>
/// MLArgMinMaxOptions
/// </summary>
[ECMAScript]
[Description("@#MLArgMinMaxOptions")]
public record MLArgMinMaxOptions(
    [property: Description("@#axes")]uint[]? Axes = default,
    [property: Description("@#keepDimensions")]bool KeepDimensions = false,
    [property: Description("@#selectLastIndex")]bool SelectLastIndex = false);

/// <summary>
/// MLBatchNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLBatchNormalizationOptions")]
public record MLBatchNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#axis")]uint Axis = 1,
    [property: Description("@#epsilon")]float Epsilon = 1e-5f,
    [property: Description("@#activation")]MLActivation? Activation = default);

/// <summary>
/// MLClampOptions
/// </summary>
[ECMAScript]
[Description("@#MLClampOptions")]
public record MLClampOptions(
    [property: Description("@#minValue")]float MinValue = default,
    [property: Description("@#maxValue")]float MaxValue = default);

/// <summary>
/// MLComputeResult
/// </summary>
[ECMAScript]
[Description("@#MLComputeResult")]
public record MLComputeResult(
    [property: Description("@#inputs")]MLNamedArrayBufferViews? Inputs = default,
    [property: Description("@#outputs")]MLNamedArrayBufferViews? Outputs = default);

/// <summary>
/// MLContextOptions
/// </summary>
[ECMAScript]
[Description("@#MLContextOptions")]
public record MLContextOptions(
    [property: Description("@#deviceType")]MLDeviceType DeviceType = MLDeviceType.Cpu,
    [property: Description("@#powerPreference")]MLPowerPreference PowerPreference = MLPowerPreference.Default);

/// <summary>
/// MLConv2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLConv2dOptions")]
public record MLConv2dOptions(
    [property: Description("@#padding")]uint[]? Padding = default,
    [property: Description("@#strides")]uint[]? Strides = default,
    [property: Description("@#dilations")]uint[]? Dilations = default,
    [property: Description("@#groups")]uint Groups = 1,
    [property: Description("@#inputLayout")]MLInputOperandLayout InputLayout = MLInputOperandLayout.Nchw,
    [property: Description("@#filterLayout")]MLConv2dFilterOperandLayout FilterLayout = MLConv2dFilterOperandLayout.Oihw,
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#activation")]MLActivation? Activation = default);

/// <summary>
/// MLConvTranspose2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLConvTranspose2dOptions")]
public record MLConvTranspose2dOptions(
    [property: Description("@#padding")]uint[]? Padding = default,
    [property: Description("@#strides")]uint[]? Strides = default,
    [property: Description("@#dilations")]uint[]? Dilations = default,
    [property: Description("@#outputPadding")]uint[]? OutputPadding = default,
    [property: Description("@#outputSizes")]uint[]? OutputSizes = default,
    [property: Description("@#groups")]uint Groups = 1,
    [property: Description("@#inputLayout")]MLInputOperandLayout InputLayout = MLInputOperandLayout.Nchw,
    [property: Description("@#filterLayout")]MLConvTranspose2dFilterOperandLayout FilterLayout = MLConvTranspose2dFilterOperandLayout.Iohw,
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#activation")]MLActivation? Activation = default);

/// <summary>
/// MLEluOptions
/// </summary>
[ECMAScript]
[Description("@#MLEluOptions")]
public record MLEluOptions(
    [property: Description("@#alpha")]float Alpha = 1f);

/// <summary>
/// MLGatherOptions
/// </summary>
[ECMAScript]
[Description("@#MLGatherOptions")]
public record MLGatherOptions(
    [property: Description("@#axis")]uint Axis = 0);

/// <summary>
/// MLGemmOptions
/// </summary>
[ECMAScript]
[Description("@#MLGemmOptions")]
public record MLGemmOptions(
    [property: Description("@#c")]MLOperand? C = default,
    [property: Description("@#alpha")]float Alpha = 1.0f,
    [property: Description("@#beta")]float Beta = 1.0f,
    [property: Description("@#aTranspose")]bool ATranspose = false,
    [property: Description("@#bTranspose")]bool BTranspose = false);

/// <summary>
/// MLGruCellOptions
/// </summary>
[ECMAScript]
[Description("@#MLGruCellOptions")]
public record MLGruCellOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
    [property: Description("@#resetAfter")]bool ResetAfter = false,
    [property: Description("@#layout")]MLGruWeightLayout Layout = MLGruWeightLayout.Zrn,
    [property: Description("@#activations")]MLActivation[]? Activations = default);

/// <summary>
/// MLGruOptions
/// </summary>
[ECMAScript]
[Description("@#MLGruOptions")]
public record MLGruOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
    [property: Description("@#initialHiddenState")]MLOperand? InitialHiddenState = default,
    [property: Description("@#resetAfter")]bool ResetAfter = false,
    [property: Description("@#returnSequence")]bool ReturnSequence = false,
    [property: Description("@#direction")]MLRecurrentNetworkDirection Direction = MLRecurrentNetworkDirection.Forward,
    [property: Description("@#layout")]MLGruWeightLayout Layout = MLGruWeightLayout.Zrn,
    [property: Description("@#activations")]MLActivation[]? Activations = default);

/// <summary>
/// MLHardSigmoidOptions
/// </summary>
[ECMAScript]
[Description("@#MLHardSigmoidOptions")]
public record MLHardSigmoidOptions(
    [property: Description("@#alpha")]float Alpha = 0.2f,
    [property: Description("@#beta")]float Beta = 0.5f);

/// <summary>
/// MLInstanceNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLInstanceNormalizationOptions")]
public record MLInstanceNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#epsilon")]float Epsilon = 1e-5f,
    [property: Description("@#layout")]MLInputOperandLayout Layout = MLInputOperandLayout.Nchw);

/// <summary>
/// MLLayerNormalizationOptions
/// </summary>
[ECMAScript]
[Description("@#MLLayerNormalizationOptions")]
public record MLLayerNormalizationOptions(
    [property: Description("@#scale")]MLOperand? Scale = default,
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#axes")]uint[]? Axes = default,
    [property: Description("@#epsilon")]float Epsilon = 1e-5f);

/// <summary>
/// MLLeakyReluOptions
/// </summary>
[ECMAScript]
[Description("@#MLLeakyReluOptions")]
public record MLLeakyReluOptions(
    [property: Description("@#alpha")]float Alpha = 0.01f);

/// <summary>
/// MLLinearOptions
/// </summary>
[ECMAScript]
[Description("@#MLLinearOptions")]
public record MLLinearOptions(
    [property: Description("@#alpha")]float Alpha = 1f,
    [property: Description("@#beta")]float Beta = 0f);

/// <summary>
/// MLLstmCellOptions
/// </summary>
[ECMAScript]
[Description("@#MLLstmCellOptions")]
public record MLLstmCellOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
    [property: Description("@#peepholeWeight")]MLOperand? PeepholeWeight = default,
    [property: Description("@#layout")]MLLstmWeightLayout Layout = MLLstmWeightLayout.Iofg,
    [property: Description("@#activations")]MLActivation[]? Activations = default);

/// <summary>
/// MLLstmOptions
/// </summary>
[ECMAScript]
[Description("@#MLLstmOptions")]
public record MLLstmOptions(
    [property: Description("@#bias")]MLOperand? Bias = default,
    [property: Description("@#recurrentBias")]MLOperand? RecurrentBias = default,
    [property: Description("@#peepholeWeight")]MLOperand? PeepholeWeight = default,
    [property: Description("@#initialHiddenState")]MLOperand? InitialHiddenState = default,
    [property: Description("@#initialCellState")]MLOperand? InitialCellState = default,
    [property: Description("@#returnSequence")]bool ReturnSequence = false,
    [property: Description("@#direction")]MLRecurrentNetworkDirection Direction = MLRecurrentNetworkDirection.Forward,
    [property: Description("@#layout")]MLLstmWeightLayout Layout = MLLstmWeightLayout.Iofg,
    [property: Description("@#activations")]MLActivation[]? Activations = default);

/// <summary>
/// MLOperandDescriptor
/// </summary>
[ECMAScript]
[Description("@#MLOperandDescriptor")]
public record MLOperandDescriptor(
    [property: Description("@#dataType")]MLOperandDataType? DataType = default,
    [property: Description("@#dimensions")]uint[]? Dimensions = default);

/// <summary>
/// MLPadOptions
/// </summary>
[ECMAScript]
[Description("@#MLPadOptions")]
public record MLPadOptions(
    [property: Description("@#mode")]MLPaddingMode Mode = MLPaddingMode.Constant,
    [property: Description("@#value")]float Value = 0f);

/// <summary>
/// MLPool2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLPool2dOptions")]
public record MLPool2dOptions(
    [property: Description("@#windowDimensions")]uint[]? WindowDimensions = default,
    [property: Description("@#padding")]uint[]? Padding = default,
    [property: Description("@#strides")]uint[]? Strides = default,
    [property: Description("@#dilations")]uint[]? Dilations = default,
    [property: Description("@#layout")]MLInputOperandLayout Layout = MLInputOperandLayout.Nchw,
    [property: Description("@#roundingType")]MLRoundingType RoundingType = MLRoundingType.Floor,
    [property: Description("@#outputSizes")]uint[]? OutputSizes = default);

/// <summary>
/// MLReduceOptions
/// </summary>
[ECMAScript]
[Description("@#MLReduceOptions")]
public record MLReduceOptions(
    [property: Description("@#axes")]uint[]? Axes = default,
    [property: Description("@#keepDimensions")]bool KeepDimensions = false);

/// <summary>
/// MLResample2dOptions
/// </summary>
[ECMAScript]
[Description("@#MLResample2dOptions")]
public record MLResample2dOptions(
    [property: Description("@#mode")]MLInterpolationMode Mode = MLInterpolationMode.NearestNeighbor,
    [property: Description("@#scales")]float[]? Scales = default,
    [property: Description("@#sizes")]uint[]? Sizes = default,
    [property: Description("@#axes")]uint[]? Axes = default);

/// <summary>
/// MLSoftplusOptions
/// </summary>
[ECMAScript]
[Description("@#MLSoftplusOptions")]
public record MLSoftplusOptions(
    [property: Description("@#steepness")]float Steepness = 1f);

/// <summary>
/// MLSplitOptions
/// </summary>
[ECMAScript]
[Description("@#MLSplitOptions")]
public record MLSplitOptions(
    [property: Description("@#axis")]uint Axis = 0);

/// <summary>
/// MLTransposeOptions
/// </summary>
[ECMAScript]
[Description("@#MLTransposeOptions")]
public record MLTransposeOptions(
    [property: Description("@#permutation")]uint[]? Permutation = default);

/// <summary>
/// MLTriangularOptions
/// </summary>
[ECMAScript]
[Description("@#MLTriangularOptions")]
public record MLTriangularOptions(
    [property: Description("@#upper")]bool Upper = false,
    [property: Description("@#diagonal")]int Diagonal = 0);

/// <summary>
/// MagnetometerSensorOptions
/// </summary>
[ECMAScript]
[Description("@#MagnetometerSensorOptions")]
public record MagnetometerSensorOptions(
    [property: Description("@#referenceFrame")]MagnetometerLocalCoordinateSystem ReferenceFrame = MagnetometerLocalCoordinateSystem.Device) : SensorOptions;

/// <summary>
/// MediaCapabilitiesDecodingInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesDecodingInfo")]
public record MediaCapabilitiesDecodingInfo(
    [property: Description("@#keySystemAccess")]MediaKeySystemAccess? KeySystemAccess = default,
    [property: Description("@#configuration")]MediaDecodingConfiguration? Configuration = default) : MediaCapabilitiesInfo;

/// <summary>
/// MediaCapabilitiesEncodingInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesEncodingInfo")]
public record MediaCapabilitiesEncodingInfo(
    [property: Description("@#configuration")]MediaEncodingConfiguration? Configuration = default) : MediaCapabilitiesInfo;

/// <summary>
/// MediaCapabilitiesInfo
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesInfo")]
public record MediaCapabilitiesInfo(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#smooth")]bool Smooth = default,
    [property: Description("@#powerEfficient")]bool PowerEfficient = default);

/// <summary>
/// MediaCapabilitiesKeySystemConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaCapabilitiesKeySystemConfiguration")]
public record MediaCapabilitiesKeySystemConfiguration(
    [property: Description("@#keySystem")]string? KeySystem = default,
    [property: Description("@#initDataType")]string? InitDataType = default,
    [property: Description("@#distinctiveIdentifier")]MediaKeysRequirement DistinctiveIdentifier = MediaKeysRequirement.Optional,
    [property: Description("@#persistentState")]MediaKeysRequirement PersistentState = MediaKeysRequirement.Optional,
    [property: Description("@#sessionTypes")]string[]? SessionTypes = default,
    [property: Description("@#audio")]KeySystemTrackConfiguration? Audio = default,
    [property: Description("@#video")]KeySystemTrackConfiguration? Video = default);

/// <summary>
/// MediaConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaConfiguration")]
public record MediaConfiguration(
    [property: Description("@#video")]VideoConfiguration? Video = default,
    [property: Description("@#audio")]AudioConfiguration? Audio = default);

/// <summary>
/// MediaDecodingConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaDecodingConfiguration")]
public record MediaDecodingConfiguration(
    [property: Description("@#type")]MediaDecodingType? Type = default,
    [property: Description("@#keySystemConfiguration")]MediaCapabilitiesKeySystemConfiguration? KeySystemConfiguration = default) : MediaConfiguration;

/// <summary>
/// MediaElementAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaElementAudioSourceOptions")]
public record MediaElementAudioSourceOptions(
    [property: Description("@#mediaElement")]HTMLMediaElement? MediaElement = default);

/// <summary>
/// MediaEncodingConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaEncodingConfiguration")]
public record MediaEncodingConfiguration(
    [property: Description("@#type")]MediaEncodingType? Type = default) : MediaConfiguration;

/// <summary>
/// MediaEncryptedEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaEncryptedEventInit")]
public record MediaEncryptedEventInit(
    [property: Description("@#initDataType")]string? InitDataType = default,
    [property: Description("@#initData")]ArrayBuffer? InitData = null) : EventInit;

/// <summary>
/// MediaImage
/// </summary>
[ECMAScript]
[Description("@#MediaImage")]
public record MediaImage(
    [property: Description("@#src")]string? Src = default,
    [property: Description("@#sizes")]string? Sizes = default,
    [property: Description("@#type")]string? Type = default);

/// <summary>
/// MediaKeyMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaKeyMessageEventInit")]
public record MediaKeyMessageEventInit(
    [property: Description("@#messageType")]MediaKeyMessageType? MessageType = default,
    [property: Description("@#message")]ArrayBuffer? Message = default) : EventInit;

/// <summary>
/// MediaKeySystemConfiguration
/// </summary>
[ECMAScript]
[Description("@#MediaKeySystemConfiguration")]
public record MediaKeySystemConfiguration(
    [property: Description("@#label")]string? Label = default,
    [property: Description("@#initDataTypes")]string[]? InitDataTypes = default,
    [property: Description("@#audioCapabilities")]MediaKeySystemMediaCapability[]? AudioCapabilities = default,
    [property: Description("@#videoCapabilities")]MediaKeySystemMediaCapability[]? VideoCapabilities = default,
    [property: Description("@#distinctiveIdentifier")]MediaKeysRequirement DistinctiveIdentifier = MediaKeysRequirement.Optional,
    [property: Description("@#persistentState")]MediaKeysRequirement PersistentState = MediaKeysRequirement.Optional,
    [property: Description("@#sessionTypes")]string[]? SessionTypes = default);

/// <summary>
/// MediaKeySystemMediaCapability
/// </summary>
[ECMAScript]
[Description("@#MediaKeySystemMediaCapability")]
public record MediaKeySystemMediaCapability(
    [property: Description("@#contentType")]string? ContentType = default,
    [property: Description("@#encryptionScheme")]string? EncryptionScheme = null,
    [property: Description("@#robustness")]string? Robustness = default);

/// <summary>
/// MediaKeysPolicy
/// </summary>
[ECMAScript]
[Description("@#MediaKeysPolicy")]
public record MediaKeysPolicy(
    [property: Description("@#minHdcpVersion")]HDCPVersion? MinHdcpVersion = default);

/// <summary>
/// MediaMetadataInit
/// </summary>
[ECMAScript]
[Description("@#MediaMetadataInit")]
public record MediaMetadataInit(
    [property: Description("@#title")]string? Title = default,
    [property: Description("@#artist")]string? Artist = default,
    [property: Description("@#album")]string? Album = default,
    [property: Description("@#artwork")]MediaImage[]? Artwork = default,
    [property: Description("@#chapterInfo")]ChapterInformationInit[]? ChapterInfo = default);

/// <summary>
/// MediaPositionState
/// </summary>
[ECMAScript]
[Description("@#MediaPositionState")]
public record MediaPositionState(
    [property: Description("@#duration")]double Duration = default,
    [property: Description("@#playbackRate")]double PlaybackRate = default,
    [property: Description("@#position")]double Position = default);

/// <summary>
/// MediaQueryListEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaQueryListEventInit")]
public record MediaQueryListEventInit(
    [property: Description("@#media")]string? Media = default,
    [property: Description("@#matches")]bool Matches = false) : EventInit;

/// <summary>
/// MediaRecorderOptions
/// </summary>
[ECMAScript]
[Description("@#MediaRecorderOptions")]
public record MediaRecorderOptions(
    [property: Description("@#mimeType")]string? MimeType = default,
    [property: Description("@#audioBitsPerSecond")]uint AudioBitsPerSecond = default,
    [property: Description("@#videoBitsPerSecond")]uint VideoBitsPerSecond = default,
    [property: Description("@#bitsPerSecond")]uint BitsPerSecond = default,
    [property: Description("@#audioBitrateMode")]BitrateMode AudioBitrateMode = BitrateMode.Variable,
    [property: Description("@#videoKeyFrameIntervalDuration")]double VideoKeyFrameIntervalDuration = default,
    [property: Description("@#videoKeyFrameIntervalCount")]uint VideoKeyFrameIntervalCount = default);

/// <summary>
/// MediaSessionActionDetails
/// </summary>
[ECMAScript]
[Description("@#MediaSessionActionDetails")]
public record MediaSessionActionDetails(
    [property: Description("@#action")]MediaSessionAction? Action = default);

/// <summary>
/// MediaSessionCaptureActionDetails
/// </summary>
[ECMAScript]
[Description("@#MediaSessionCaptureActionDetails")]
public record MediaSessionCaptureActionDetails(
    [property: Description("@#isActivating")]bool IsActivating = default) : MediaSessionActionDetails;

/// <summary>
/// MediaSessionSeekActionDetails
/// </summary>
[ECMAScript]
[Description("@#MediaSessionSeekActionDetails")]
public record MediaSessionSeekActionDetails(
    [property: Description("@#seekOffset")]double SeekOffset = default) : MediaSessionActionDetails;

/// <summary>
/// MediaSessionSeekToActionDetails
/// </summary>
[ECMAScript]
[Description("@#MediaSessionSeekToActionDetails")]
public record MediaSessionSeekToActionDetails(
    [property: Description("@#seekTime")]double SeekTime = default,
    [property: Description("@#fastSeek")]bool FastSeek = default) : MediaSessionActionDetails;

/// <summary>
/// MediaSettingsRange
/// </summary>
[ECMAScript]
[Description("@#MediaSettingsRange")]
public record MediaSettingsRange(
    [property: Description("@#max")]double Max = default,
    [property: Description("@#min")]double Min = default,
    [property: Description("@#step")]double Step = default);

/// <summary>
/// MediaStreamAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaStreamAudioSourceOptions")]
public record MediaStreamAudioSourceOptions(
    [property: Description("@#mediaStream")]MediaStream? MediaStream = default);

/// <summary>
/// MediaStreamConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaStreamConstraints")]
public record MediaStreamConstraints(
    [property: Description("@#video")]MediaStreamConstraintsVideo? Video = default,
    [property: Description("@#audio")]MediaStreamConstraintsAudio? Audio = default,
    [property: Description("@#preferCurrentTab")]bool PreferCurrentTab = false,
    [property: Description("@#peerIdentity")]string? PeerIdentity = default)
{
    [Category("optional")]
    public extern static MediaStreamConstraints OptionalVideoAudio(
        [Description("@#video")]MediaStreamConstraintsVideo? video = default,
        [Description("@#audio")]MediaStreamConstraintsAudio? audio = default);

    [Category("optional")]
    public extern static MediaStreamConstraints OptionalPreferCurrentTab(
        [Description("@#preferCurrentTab")]bool preferCurrentTab = false);

    [Category("optional")]
    public extern static MediaStreamConstraints OptionalPeerIdentity(
        [Description("@#peerIdentity")]string? PeerIdentity = default);
}

/// <summary>
/// MediaStreamTrackAudioSourceOptions
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackAudioSourceOptions")]
public record MediaStreamTrackAudioSourceOptions(
    [property: Description("@#mediaStreamTrack")]MediaStreamTrack? MediaStreamTrack = default);

/// <summary>
/// MediaStreamTrackEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackEventInit")]
public record MediaStreamTrackEventInit(
    [property: Description("@#track")]MediaStreamTrack? Track = default) : EventInit;

/// <summary>
/// MediaStreamTrackProcessorInit
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackProcessorInit")]
public record MediaStreamTrackProcessorInit(
    [property: Description("@#track")]MediaStreamTrack? Track = default,
    [property: Description("@#maxBufferSize")]ushort MaxBufferSize = default);

/// <summary>
/// MediaTrackCapabilities
/// </summary>
[ECMAScript]
[Description("@#MediaTrackCapabilities")]
public record MediaTrackCapabilities(
    [property: Description("@#whiteBalanceMode")]string[]? WhiteBalanceMode = default,
    [property: Description("@#exposureMode")]string[]? ExposureMode = default,
    [property: Description("@#focusMode")]string[]? FocusMode = default,
    [property: Description("@#exposureCompensation")]MediaSettingsRange? ExposureCompensation = default,
    [property: Description("@#exposureTime")]MediaSettingsRange? ExposureTime = default,
    [property: Description("@#colorTemperature")]MediaSettingsRange? ColorTemperature = default,
    [property: Description("@#iso")]MediaSettingsRange? Iso = default,
    [property: Description("@#brightness")]MediaSettingsRange? Brightness = default,
    [property: Description("@#contrast")]MediaSettingsRange? Contrast = default,
    [property: Description("@#saturation")]MediaSettingsRange? Saturation = default,
    [property: Description("@#sharpness")]MediaSettingsRange? Sharpness = default,
    [property: Description("@#focusDistance")]MediaSettingsRange? FocusDistance = default,
    [property: Description("@#pan")]MediaSettingsRange? Pan = default,
    [property: Description("@#tilt")]MediaSettingsRange? Tilt = default,
    [property: Description("@#zoom")]MediaSettingsRange? Zoom = default,
    [property: Description("@#torch")]bool[]? Torch = default,
    [property: Description("@#width")]ULongRange? Width = default,
    [property: Description("@#height")]ULongRange? Height = default,
    [property: Description("@#aspectRatio")]DoubleRange? AspectRatio = default,
    [property: Description("@#frameRate")]DoubleRange? FrameRate = default,
    [property: Description("@#facingMode")]string[]? FacingMode = default,
    [property: Description("@#resizeMode")]string[]? ResizeMode = default,
    [property: Description("@#sampleRate")]ULongRange? SampleRate = default,
    [property: Description("@#sampleSize")]ULongRange? SampleSize = default,
    [property: Description("@#echoCancellation")]bool[]? EchoCancellation = default,
    [property: Description("@#autoGainControl")]bool[]? AutoGainControl = default,
    [property: Description("@#noiseSuppression")]bool[]? NoiseSuppression = default,
    [property: Description("@#latency")]DoubleRange? Latency = default,
    [property: Description("@#channelCount")]ULongRange? ChannelCount = default,
    [property: Description("@#deviceId")]string? DeviceId = default,
    [property: Description("@#groupId")]string? GroupId = default,
    [property: Description("@#displaySurface")]string? DisplaySurface = default,
    [property: Description("@#logicalSurface")]bool LogicalSurface = default,
    [property: Description("@#cursor")]string[]? Cursor = default)
{
    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalWhiteBalanceModeExposureModeFocusMode16(
        [Description("@#whiteBalanceMode")]string[]? WhiteBalanceMode = default,
        [Description("@#exposureMode")]string[]? ExposureMode = default,
        [Description("@#focusMode")]string[]? FocusMode = default,
        [Description("@#exposureCompensation")]MediaSettingsRange? ExposureCompensation = default,
        [Description("@#exposureTime")]MediaSettingsRange? ExposureTime = default,
        [Description("@#colorTemperature")]MediaSettingsRange? ColorTemperature = default,
        [Description("@#iso")]MediaSettingsRange? Iso = default,
        [Description("@#brightness")]MediaSettingsRange? Brightness = default,
        [Description("@#contrast")]MediaSettingsRange? Contrast = default,
        [Description("@#saturation")]MediaSettingsRange? Saturation = default,
        [Description("@#sharpness")]MediaSettingsRange? Sharpness = default,
        [Description("@#focusDistance")]MediaSettingsRange? FocusDistance = default,
        [Description("@#pan")]MediaSettingsRange? Pan = default,
        [Description("@#tilt")]MediaSettingsRange? Tilt = default,
        [Description("@#zoom")]MediaSettingsRange? Zoom = default,
        [Description("@#torch")]bool[]? Torch = default);

    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalWidthHeightAspectRatio15(
        [Description("@#width")]ULongRange? Width = default,
        [Description("@#height")]ULongRange? Height = default,
        [Description("@#aspectRatio")]DoubleRange? AspectRatio = default,
        [Description("@#frameRate")]DoubleRange? FrameRate = default,
        [Description("@#facingMode")]string[]? FacingMode = default,
        [Description("@#resizeMode")]string[]? ResizeMode = default,
        [Description("@#sampleRate")]ULongRange? SampleRate = default,
        [Description("@#sampleSize")]ULongRange? SampleSize = default,
        [Description("@#echoCancellation")]bool[]? EchoCancellation = default,
        [Description("@#autoGainControl")]bool[]? AutoGainControl = default,
        [Description("@#noiseSuppression")]bool[]? NoiseSuppression = default,
        [Description("@#latency")]DoubleRange? Latency = default,
        [Description("@#channelCount")]ULongRange? ChannelCount = default,
        [Description("@#deviceId")]string? DeviceId = default,
        [Description("@#groupId")]string? GroupId = default);

    [Category("optional")]
    public extern static MediaTrackCapabilities OptionalDisplaySurfaceLogicalSurfaceCursor(
        [Description("@#displaySurface")]string? DisplaySurface = default,
        [Description("@#logicalSurface")]bool LogicalSurface = default,
        [Description("@#cursor")]string[]? Cursor = default);
}

/// <summary>
/// MediaTrackConstraintSet
/// </summary>
[ECMAScript]
[Description("@#MediaTrackConstraintSet")]
public record MediaTrackConstraintSet(
    [property: Description("@#whiteBalanceMode")]ConstrainDOMString? WhiteBalanceMode = default,
    [property: Description("@#exposureMode")]ConstrainDOMString? ExposureMode = default,
    [property: Description("@#focusMode")]ConstrainDOMString? FocusMode = default,
    [property: Description("@#pointsOfInterest")]ConstrainPoint2D? PointsOfInterest = default,
    [property: Description("@#exposureCompensation")]ConstrainDouble? ExposureCompensation = default,
    [property: Description("@#exposureTime")]ConstrainDouble? ExposureTime = default,
    [property: Description("@#colorTemperature")]ConstrainDouble? ColorTemperature = default,
    [property: Description("@#iso")]ConstrainDouble? Iso = default,
    [property: Description("@#brightness")]ConstrainDouble? Brightness = default,
    [property: Description("@#contrast")]ConstrainDouble? Contrast = default,
    [property: Description("@#saturation")]ConstrainDouble? Saturation = default,
    [property: Description("@#sharpness")]ConstrainDouble? Sharpness = default,
    [property: Description("@#focusDistance")]ConstrainDouble? FocusDistance = default,
    [property: Description("@#pan")]MediaTrackConstraintSetPan? Pan = default,
    [property: Description("@#tilt")]MediaTrackConstraintSetTilt? Tilt = default,
    [property: Description("@#zoom")]MediaTrackConstraintSetZoom? Zoom = default,
    [property: Description("@#torch")]ConstrainBoolean? Torch = default,
    [property: Description("@#width")]ConstrainULong? Width = default,
    [property: Description("@#height")]ConstrainULong? Height = default,
    [property: Description("@#aspectRatio")]ConstrainDouble? AspectRatio = default,
    [property: Description("@#frameRate")]ConstrainDouble? FrameRate = default,
    [property: Description("@#facingMode")]ConstrainDOMString? FacingMode = default,
    [property: Description("@#resizeMode")]ConstrainDOMString? ResizeMode = default,
    [property: Description("@#sampleRate")]ConstrainULong? SampleRate = default,
    [property: Description("@#sampleSize")]ConstrainULong? SampleSize = default,
    [property: Description("@#echoCancellation")]ConstrainBoolean? EchoCancellation = default,
    [property: Description("@#autoGainControl")]ConstrainBoolean? AutoGainControl = default,
    [property: Description("@#noiseSuppression")]ConstrainBoolean? NoiseSuppression = default,
    [property: Description("@#latency")]ConstrainDouble? Latency = default,
    [property: Description("@#channelCount")]ConstrainULong? ChannelCount = default,
    [property: Description("@#deviceId")]ConstrainDOMString? DeviceId = default,
    [property: Description("@#groupId")]ConstrainDOMString? GroupId = default,
    [property: Description("@#displaySurface")]ConstrainDOMString? DisplaySurface = default,
    [property: Description("@#logicalSurface")]ConstrainBoolean? LogicalSurface = default,
    [property: Description("@#cursor")]ConstrainDOMString? Cursor = default,
    [property: Description("@#restrictOwnAudio")]ConstrainBoolean? RestrictOwnAudio = default,
    [property: Description("@#suppressLocalAudioPlayback")]ConstrainBoolean? SuppressLocalAudioPlayback = default)
{
    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]ConstrainDOMString? WhiteBalanceMode = default,
        [Description("@#exposureMode")]ConstrainDOMString? ExposureMode = default,
        [Description("@#focusMode")]ConstrainDOMString? FocusMode = default,
        [Description("@#pointsOfInterest")]ConstrainPoint2D? PointsOfInterest = default,
        [Description("@#exposureCompensation")]ConstrainDouble? ExposureCompensation = default,
        [Description("@#exposureTime")]ConstrainDouble? ExposureTime = default,
        [Description("@#colorTemperature")]ConstrainDouble? ColorTemperature = default,
        [Description("@#iso")]ConstrainDouble? Iso = default,
        [Description("@#brightness")]ConstrainDouble? Brightness = default,
        [Description("@#contrast")]ConstrainDouble? Contrast = default,
        [Description("@#saturation")]ConstrainDouble? Saturation = default,
        [Description("@#sharpness")]ConstrainDouble? Sharpness = default,
        [Description("@#focusDistance")]ConstrainDouble? FocusDistance = default,
        [Description("@#pan")]MediaTrackConstraintSetPan? Pan = default,
        [Description("@#tilt")]MediaTrackConstraintSetTilt? Tilt = default,
        [Description("@#zoom")]MediaTrackConstraintSetZoom? Zoom = default,
        [Description("@#torch")]ConstrainBoolean? Torch = default);

    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalWidthHeightAspectRatio15(
        [Description("@#width")]ConstrainULong? Width = default,
        [Description("@#height")]ConstrainULong? Height = default,
        [Description("@#aspectRatio")]ConstrainDouble? AspectRatio = default,
        [Description("@#frameRate")]ConstrainDouble? FrameRate = default,
        [Description("@#facingMode")]ConstrainDOMString? FacingMode = default,
        [Description("@#resizeMode")]ConstrainDOMString? ResizeMode = default,
        [Description("@#sampleRate")]ConstrainULong? SampleRate = default,
        [Description("@#sampleSize")]ConstrainULong? SampleSize = default,
        [Description("@#echoCancellation")]ConstrainBoolean? EchoCancellation = default,
        [Description("@#autoGainControl")]ConstrainBoolean? AutoGainControl = default,
        [Description("@#noiseSuppression")]ConstrainBoolean? NoiseSuppression = default,
        [Description("@#latency")]ConstrainDouble? Latency = default,
        [Description("@#channelCount")]ConstrainULong? ChannelCount = default,
        [Description("@#deviceId")]ConstrainDOMString? DeviceId = default,
        [Description("@#groupId")]ConstrainDOMString? GroupId = default);

    [Category("optional")]
    public extern static MediaTrackConstraintSet OptionalDisplaySurfaceLogicalSurfaceCursor5(
        [Description("@#displaySurface")]ConstrainDOMString? DisplaySurface = default,
        [Description("@#logicalSurface")]ConstrainBoolean? LogicalSurface = default,
        [Description("@#cursor")]ConstrainDOMString? Cursor = default,
        [Description("@#restrictOwnAudio")]ConstrainBoolean? RestrictOwnAudio = default,
        [Description("@#suppressLocalAudioPlayback")]ConstrainBoolean? SuppressLocalAudioPlayback = default);
}

/// <summary>
/// MediaTrackConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaTrackConstraints")]
public record MediaTrackConstraints(
    [property: Description("@#advanced")]MediaTrackConstraintSet[]? Advanced = default) : MediaTrackConstraintSet;

/// <summary>
/// MediaTrackSettings
/// </summary>
[ECMAScript]
[Description("@#MediaTrackSettings")]
public record MediaTrackSettings(
    [property: Description("@#whiteBalanceMode")]string? WhiteBalanceMode = default,
    [property: Description("@#exposureMode")]string? ExposureMode = default,
    [property: Description("@#focusMode")]string? FocusMode = default,
    [property: Description("@#pointsOfInterest")]Point2D[]? PointsOfInterest = default,
    [property: Description("@#exposureCompensation")]double ExposureCompensation = default,
    [property: Description("@#exposureTime")]double ExposureTime = default,
    [property: Description("@#colorTemperature")]double ColorTemperature = default,
    [property: Description("@#iso")]double Iso = default,
    [property: Description("@#brightness")]double Brightness = default,
    [property: Description("@#contrast")]double Contrast = default,
    [property: Description("@#saturation")]double Saturation = default,
    [property: Description("@#sharpness")]double Sharpness = default,
    [property: Description("@#focusDistance")]double FocusDistance = default,
    [property: Description("@#pan")]double Pan = default,
    [property: Description("@#tilt")]double Tilt = default,
    [property: Description("@#zoom")]double Zoom = default,
    [property: Description("@#torch")]bool Torch = default,
    [property: Description("@#width")]uint Width = default,
    [property: Description("@#height")]uint Height = default,
    [property: Description("@#aspectRatio")]double AspectRatio = default,
    [property: Description("@#frameRate")]double FrameRate = default,
    [property: Description("@#facingMode")]string? FacingMode = default,
    [property: Description("@#resizeMode")]string? ResizeMode = default,
    [property: Description("@#sampleRate")]uint SampleRate = default,
    [property: Description("@#sampleSize")]uint SampleSize = default,
    [property: Description("@#echoCancellation")]bool EchoCancellation = default,
    [property: Description("@#autoGainControl")]bool AutoGainControl = default,
    [property: Description("@#noiseSuppression")]bool NoiseSuppression = default,
    [property: Description("@#latency")]double Latency = default,
    [property: Description("@#channelCount")]uint ChannelCount = default,
    [property: Description("@#deviceId")]string? DeviceId = default,
    [property: Description("@#groupId")]string? GroupId = default,
    [property: Description("@#displaySurface")]string? DisplaySurface = default,
    [property: Description("@#logicalSurface")]bool LogicalSurface = default,
    [property: Description("@#cursor")]string? Cursor = default,
    [property: Description("@#restrictOwnAudio")]bool RestrictOwnAudio = default,
    [property: Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = default)
{
    [Category("optional")]
    public extern static MediaTrackSettings OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]string? WhiteBalanceMode = default,
        [Description("@#exposureMode")]string? ExposureMode = default,
        [Description("@#focusMode")]string? FocusMode = default,
        [Description("@#pointsOfInterest")]Point2D[]? PointsOfInterest = default,
        [Description("@#exposureCompensation")]double ExposureCompensation = default,
        [Description("@#exposureTime")]double ExposureTime = default,
        [Description("@#colorTemperature")]double ColorTemperature = default,
        [Description("@#iso")]double Iso = default,
        [Description("@#brightness")]double Brightness = default,
        [Description("@#contrast")]double Contrast = default,
        [Description("@#saturation")]double Saturation = default,
        [Description("@#sharpness")]double Sharpness = default,
        [Description("@#focusDistance")]double FocusDistance = default,
        [Description("@#pan")]double Pan = default,
        [Description("@#tilt")]double Tilt = default,
        [Description("@#zoom")]double Zoom = default,
        [Description("@#torch")]bool Torch = default);

    [Category("optional")]
    public extern static MediaTrackSettings OptionalWidthHeightAspectRatio15(
        [Description("@#width")]uint Width = default,
        [Description("@#height")]uint Height = default,
        [Description("@#aspectRatio")]double AspectRatio = default,
        [Description("@#frameRate")]double FrameRate = default,
        [Description("@#facingMode")]string? FacingMode = default,
        [Description("@#resizeMode")]string? ResizeMode = default,
        [Description("@#sampleRate")]uint SampleRate = default,
        [Description("@#sampleSize")]uint SampleSize = default,
        [Description("@#echoCancellation")]bool EchoCancellation = default,
        [Description("@#autoGainControl")]bool AutoGainControl = default,
        [Description("@#noiseSuppression")]bool NoiseSuppression = default,
        [Description("@#latency")]double Latency = default,
        [Description("@#channelCount")]uint ChannelCount = default,
        [Description("@#deviceId")]string? DeviceId = default,
        [Description("@#groupId")]string? GroupId = default);

    [Category("optional")]
    public extern static MediaTrackSettings OptionalDisplaySurfaceLogicalSurfaceCursor5(
        [Description("@#displaySurface")]string? DisplaySurface = default,
        [Description("@#logicalSurface")]bool LogicalSurface = default,
        [Description("@#cursor")]string? Cursor = default,
        [Description("@#restrictOwnAudio")]bool RestrictOwnAudio = default,
        [Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = default);
}

/// <summary>
/// MediaTrackSupportedConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaTrackSupportedConstraints")]
public record MediaTrackSupportedConstraints(
    [property: Description("@#whiteBalanceMode")]bool WhiteBalanceMode = false,
    [property: Description("@#exposureMode")]bool ExposureMode = false,
    [property: Description("@#focusMode")]bool FocusMode = false,
    [property: Description("@#pointsOfInterest")]bool PointsOfInterest = false,
    [property: Description("@#exposureCompensation")]bool ExposureCompensation = false,
    [property: Description("@#exposureTime")]bool ExposureTime = false,
    [property: Description("@#colorTemperature")]bool ColorTemperature = false,
    [property: Description("@#iso")]bool Iso = false,
    [property: Description("@#brightness")]bool Brightness = false,
    [property: Description("@#contrast")]bool Contrast = false,
    [property: Description("@#pan")]bool Pan = false,
    [property: Description("@#saturation")]bool Saturation = false,
    [property: Description("@#sharpness")]bool Sharpness = false,
    [property: Description("@#focusDistance")]bool FocusDistance = false,
    [property: Description("@#tilt")]bool Tilt = false,
    [property: Description("@#zoom")]bool Zoom = false,
    [property: Description("@#torch")]bool Torch = false,
    [property: Description("@#width")]bool Width = false,
    [property: Description("@#height")]bool Height = false,
    [property: Description("@#aspectRatio")]bool AspectRatio = false,
    [property: Description("@#frameRate")]bool FrameRate = false,
    [property: Description("@#facingMode")]bool FacingMode = false,
    [property: Description("@#resizeMode")]bool ResizeMode = false,
    [property: Description("@#sampleRate")]bool SampleRate = false,
    [property: Description("@#sampleSize")]bool SampleSize = false,
    [property: Description("@#echoCancellation")]bool EchoCancellation = false,
    [property: Description("@#autoGainControl")]bool AutoGainControl = false,
    [property: Description("@#noiseSuppression")]bool NoiseSuppression = false,
    [property: Description("@#latency")]bool Latency = false,
    [property: Description("@#channelCount")]bool ChannelCount = false,
    [property: Description("@#deviceId")]bool DeviceId = false,
    [property: Description("@#groupId")]bool GroupId = false,
    [property: Description("@#displaySurface")]bool DisplaySurface = false,
    [property: Description("@#logicalSurface")]bool LogicalSurface = false,
    [property: Description("@#cursor")]bool Cursor = false,
    [property: Description("@#restrictOwnAudio")]bool RestrictOwnAudio = false,
    [property: Description("@#suppressLocalAudioPlayback")]bool SuppressLocalAudioPlayback = false)
{
    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalWhiteBalanceModeExposureModeFocusMode17(
        [Description("@#whiteBalanceMode")]bool whiteBalanceMode = false,
        [Description("@#exposureMode")]bool exposureMode = false,
        [Description("@#focusMode")]bool focusMode = false,
        [Description("@#pointsOfInterest")]bool pointsOfInterest = false,
        [Description("@#exposureCompensation")]bool exposureCompensation = false,
        [Description("@#exposureTime")]bool exposureTime = false,
        [Description("@#colorTemperature")]bool colorTemperature = false,
        [Description("@#iso")]bool iso = false,
        [Description("@#brightness")]bool brightness = false,
        [Description("@#contrast")]bool contrast = false,
        [Description("@#pan")]bool pan = false,
        [Description("@#saturation")]bool saturation = false,
        [Description("@#sharpness")]bool sharpness = false,
        [Description("@#focusDistance")]bool focusDistance = false,
        [Description("@#tilt")]bool tilt = false,
        [Description("@#zoom")]bool zoom = false,
        [Description("@#torch")]bool torch = false);

    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalWidthHeightAspectRatio15(
        [Description("@#width")]bool width = false,
        [Description("@#height")]bool height = false,
        [Description("@#aspectRatio")]bool aspectRatio = false,
        [Description("@#frameRate")]bool frameRate = false,
        [Description("@#facingMode")]bool facingMode = false,
        [Description("@#resizeMode")]bool resizeMode = false,
        [Description("@#sampleRate")]bool sampleRate = false,
        [Description("@#sampleSize")]bool sampleSize = false,
        [Description("@#echoCancellation")]bool echoCancellation = false,
        [Description("@#autoGainControl")]bool autoGainControl = false,
        [Description("@#noiseSuppression")]bool noiseSuppression = false,
        [Description("@#latency")]bool latency = false,
        [Description("@#channelCount")]bool channelCount = false,
        [Description("@#deviceId")]bool deviceId = false,
        [Description("@#groupId")]bool groupId = false);

    [Category("optional")]
    public extern static MediaTrackSupportedConstraints OptionalDisplaySurfaceLogicalSurfaceCursor5(
        [Description("@#displaySurface")]bool displaySurface = false,
        [Description("@#logicalSurface")]bool logicalSurface = false,
        [Description("@#cursor")]bool cursor = false,
        [Description("@#restrictOwnAudio")]bool restrictOwnAudio = false,
        [Description("@#suppressLocalAudioPlayback")]bool suppressLocalAudioPlayback = false);
}

/// <summary>
/// MemoryAttribution
/// </summary>
[ECMAScript]
[Description("@#MemoryAttribution")]
public record MemoryAttribution(
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#container")]MemoryAttributionContainer? Container = default,
    [property: Description("@#scope")]string? Scope = default);

/// <summary>
/// MemoryAttributionContainer
/// </summary>
[ECMAScript]
[Description("@#MemoryAttributionContainer")]
public record MemoryAttributionContainer(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#src")]string? Src = default);

/// <summary>
/// MemoryBreakdownEntry
/// </summary>
[ECMAScript]
[Description("@#MemoryBreakdownEntry")]
public record MemoryBreakdownEntry(
    [property: Description("@#bytes")]ulong Bytes = default,
    [property: Description("@#attribution")]MemoryAttribution[]? Attribution = default,
    [property: Description("@#types")]string[]? Types = default);

/// <summary>
/// MemoryMeasurement
/// </summary>
[ECMAScript]
[Description("@#MemoryMeasurement")]
public record MemoryMeasurement(
    [property: Description("@#bytes")]ulong Bytes = default,
    [property: Description("@#breakdown")]MemoryBreakdownEntry[]? Breakdown = default);

/// <summary>
/// MessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MessageEventInit")]
public record MessageEventInit(
    [property: Description("@#data")]object? Data = default,
    [property: Description("@#origin")]string? Origin = default,
    [property: Description("@#lastEventId")]string? LastEventId = default,
    [property: Description("@#source")]MessageEventSource? Source = null,
    [property: Description("@#ports")]MessagePort[]? Ports = default) : EventInit;

/// <summary>
/// MidiPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#MidiPermissionDescriptor")]
public record MidiPermissionDescriptor(
    [property: Description("@#sysex")]bool Sysex = false) : PermissionDescriptor;

/// <summary>
/// MockCameraConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCameraConfiguration")]
public record MockCameraConfiguration(
    [property: Description("@#defaultFrameRate")]double DefaultFrameRate = 30d,
    [property: Description("@#facingMode")]string? FacingMode = default) : MockCaptureDeviceConfiguration;

/// <summary>
/// MockCaptureDeviceConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCaptureDeviceConfiguration")]
public record MockCaptureDeviceConfiguration(
    [property: Description("@#label")]string? Label = default,
    [property: Description("@#deviceId")]string? DeviceId = default,
    [property: Description("@#groupId")]string? GroupId = default);

/// <summary>
/// MockCapturePromptResultConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockCapturePromptResultConfiguration")]
public record MockCapturePromptResultConfiguration(
    [property: Description("@#getUserMedia")]MockCapturePromptResult? GetUserMedia = default,
    [property: Description("@#getDisplayMedia")]MockCapturePromptResult? GetDisplayMedia = default);

/// <summary>
/// MockMicrophoneConfiguration
/// </summary>
[ECMAScript]
[Description("@#MockMicrophoneConfiguration")]
public record MockMicrophoneConfiguration(
    [property: Description("@#defaultSampleRate")]uint DefaultSampleRate = 44100) : MockCaptureDeviceConfiguration;

/// <summary>
/// MouseEventInit
/// </summary>
[ECMAScript]
[Description("@#MouseEventInit")]
public record MouseEventInit(
    [property: Description("@#movementX")]double MovementX = 0d,
    [property: Description("@#movementY")]double MovementY = 0d,
    [property: Description("@#screenX")]int ScreenX = 0,
    [property: Description("@#screenY")]int ScreenY = 0,
    [property: Description("@#clientX")]int ClientX = 0,
    [property: Description("@#clientY")]int ClientY = 0,
    [property: Description("@#button")]short Button = 0,
    [property: Description("@#buttons")]ushort Buttons = 0,
    [property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null) : EventModifierInit
{
    [Category("optional")]
    public extern static MouseEventInit OptionalMovementXMovementY(
        [Description("@#movementX")]double movementX = 0d,
        [Description("@#movementY")]double movementY = 0d);

    [Category("optional")]
    public extern static MouseEventInit OptionalScreenXScreenYClientX7(
        [Description("@#screenX")]int screenX = 0,
        [Description("@#screenY")]int screenY = 0,
        [Description("@#clientX")]int clientX = 0,
        [Description("@#clientY")]int clientY = 0,
        [Description("@#button")]short button = 0,
        [Description("@#buttons")]ushort buttons = 0,
        [Description("@#relatedTarget")]EventTarget? relatedTarget = null);
}

/// <summary>
/// MultiCacheQueryOptions
/// </summary>
[ECMAScript]
[Description("@#MultiCacheQueryOptions")]
public record MultiCacheQueryOptions(
    [property: Description("@#cacheName")]string? CacheName = default) : CacheQueryOptions;

/// <summary>
/// MutationObserverInit
/// </summary>
[ECMAScript]
[Description("@#MutationObserverInit")]
public record MutationObserverInit(
    [property: Description("@#childList")]bool ChildList = false,
    [property: Description("@#attributes")]bool Attributes = default,
    [property: Description("@#characterData")]bool CharacterData = default,
    [property: Description("@#subtree")]bool Subtree = false,
    [property: Description("@#attributeOldValue")]bool AttributeOldValue = default,
    [property: Description("@#characterDataOldValue")]bool CharacterDataOldValue = default,
    [property: Description("@#attributeFilter")]string[]? AttributeFilter = default);

/// <summary>
/// NDEFMakeReadOnlyOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFMakeReadOnlyOptions")]
public record NDEFMakeReadOnlyOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NDEFMessageInit
/// </summary>
[ECMAScript]
[Description("@#NDEFMessageInit")]
public record NDEFMessageInit(
    [property: Description("@#records")]NDEFRecordInit[]? Records = default);

/// <summary>
/// NDEFReadingEventInit
/// </summary>
[ECMAScript]
[Description("@#NDEFReadingEventInit")]
public record NDEFReadingEventInit(
    [property: Description("@#serialNumber")]string? SerialNumber = "",
    [property: Description("@#message")]NDEFMessageInit? Message = default) : EventInit;

/// <summary>
/// NDEFRecordInit
/// </summary>
[ECMAScript]
[Description("@#NDEFRecordInit")]
public record NDEFRecordInit(
    [property: Description("@#recordType")]string? RecordType = default,
    [property: Description("@#mediaType")]string? MediaType = default,
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#encoding")]string? Encoding = default,
    [property: Description("@#lang")]string? Lang = default,
    [property: Description("@#data")]object? Data = default);

/// <summary>
/// NDEFScanOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFScanOptions")]
public record NDEFScanOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NDEFWriteOptions
/// </summary>
[ECMAScript]
[Description("@#NDEFWriteOptions")]
public record NDEFWriteOptions(
    [property: Description("@#overwrite")]bool Overwrite = false,
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// NavigateEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigateEventInit")]
public record NavigateEventInit(
    [property: Description("@#navigationType")]NavigationType NavigationType = NavigationType.Push,
    [property: Description("@#destination")]NavigationDestination? Destination = default,
    [property: Description("@#canIntercept")]bool CanIntercept = false,
    [property: Description("@#userInitiated")]bool UserInitiated = false,
    [property: Description("@#hashChange")]bool HashChange = false,
    [property: Description("@#signal")]AbortSignal? Signal = default,
    [property: Description("@#formData")]FormData? FormData = null,
    [property: Description("@#downloadRequest")]string? DownloadRequest = null,
    [property: Description("@#info")]object? Info = default,
    [property: Description("@#hasUAVisualTransition")]bool HasUAVisualTransition = false) : EventInit;

/// <summary>
/// NavigationCurrentEntryChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigationCurrentEntryChangeEventInit")]
public record NavigationCurrentEntryChangeEventInit(
    [property: Description("@#navigationType")]NavigationType? NavigationType = null,
    [property: Description("@#from")]NavigationHistoryEntry? From = default) : EventInit;

/// <summary>
/// NavigationEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigationEventInit")]
public record NavigationEventInit(
    [property: Description("@#dir")]SpatialNavigationDirection? Dir = default,
    [property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null) : UIEventInit;

/// <summary>
/// NavigationInterceptOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationInterceptOptions")]
public record NavigationInterceptOptions(
    [property: Description("@#handler")]NavigationInterceptHandler? Handler = default,
    [property: Description("@#focusReset")]NavigationFocusReset? FocusReset = default,
    [property: Description("@#scroll")]NavigationScrollBehavior? Scroll = default);

/// <summary>
/// NavigationNavigateOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationNavigateOptions")]
public record NavigationNavigateOptions(
    [property: Description("@#state")]object? State = default,
    [property: Description("@#history")]NavigationHistoryBehavior History = NavigationHistoryBehavior.Auto) : NavigationOptions;

/// <summary>
/// NavigationOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationOptions")]
public record NavigationOptions(
    [property: Description("@#info")]object? Info = default);

/// <summary>
/// NavigationPreloadState
/// </summary>
[ECMAScript]
[Description("@#NavigationPreloadState")]
public record NavigationPreloadState(
    [property: Description("@#enabled")]bool Enabled = false,
    [property: Description("@#headerValue")]byte[]? HeaderValue = default);

/// <summary>
/// NavigationReloadOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationReloadOptions")]
public record NavigationReloadOptions(
    [property: Description("@#state")]object? State = default) : NavigationOptions;

/// <summary>
/// NavigationResult
/// </summary>
[ECMAScript]
[Description("@#NavigationResult")]
public record NavigationResult(
    [property: Description("@#committed")]PromiseResult<NavigationHistoryEntry>? Committed = default,
    [property: Description("@#finished")]PromiseResult<NavigationHistoryEntry>? Finished = default);

/// <summary>
/// NavigationUpdateCurrentEntryOptions
/// </summary>
[ECMAScript]
[Description("@#NavigationUpdateCurrentEntryOptions")]
public record NavigationUpdateCurrentEntryOptions(
    [property: Description("@#state")]object? State = default);

/// <summary>
/// NavigatorUABrandVersion
/// </summary>
[ECMAScript]
[Description("@#NavigatorUABrandVersion")]
public record NavigatorUABrandVersion(
    [property: Description("@#brand")]string? Brand = default,
    [property: Description("@#version")]string? Version = default);

/// <summary>
/// NotificationAction
/// </summary>
[ECMAScript]
[Description("@#NotificationAction")]
public record NotificationAction(
    [property: Description("@#action")]string? Action = default,
    [property: Description("@#title")]string? Title = default,
    [property: Description("@#icon")]string? Icon = default);

/// <summary>
/// NotificationEventInit
/// </summary>
[ECMAScript]
[Description("@#NotificationEventInit")]
public record NotificationEventInit(
    [property: Description("@#notification")]Notification? Notification = default,
    [property: Description("@#action")]string? Action = default) : ExtendableEventInit;

/// <summary>
/// NotificationOptions
/// </summary>
[ECMAScript]
[Description("@#NotificationOptions")]
public record NotificationOptions(
    [property: Description("@#dir")]NotificationDirection Dir = NotificationDirection.Auto,
    [property: Description("@#lang")]string? Lang = default,
    [property: Description("@#body")]string? Body = default,
    [property: Description("@#tag")]string? Tag = default,
    [property: Description("@#image")]string? Image = default,
    [property: Description("@#icon")]string? Icon = default,
    [property: Description("@#badge")]string? Badge = default,
    [property: Description("@#vibrate")]VibratePattern? Vibrate = default,
    [property: Description("@#timestamp")]EpochTimeStamp? Timestamp = default,
    [property: Description("@#renotify")]bool Renotify = false,
    [property: Description("@#silent")]bool? Silent = null,
    [property: Description("@#requireInteraction")]bool RequireInteraction = false,
    [property: Description("@#data")]object? Data = default,
    [property: Description("@#actions")]NotificationAction[]? Actions = default);

/// <summary>
/// OTPCredentialRequestOptions
/// </summary>
[ECMAScript]
[Description("@#OTPCredentialRequestOptions")]
public record OTPCredentialRequestOptions(
    [property: Description("@#transport")]OTPCredentialTransportType[]? Transport = default);

/// <summary>
/// OfflineAudioCompletionEventInit
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioCompletionEventInit")]
public record OfflineAudioCompletionEventInit(
    [property: Description("@#renderedBuffer")]AudioBuffer? RenderedBuffer = default) : EventInit;

/// <summary>
/// OfflineAudioContextOptions
/// </summary>
[ECMAScript]
[Description("@#OfflineAudioContextOptions")]
public record OfflineAudioContextOptions(
    [property: Description("@#numberOfChannels")]uint NumberOfChannels = 1,
    [property: Description("@#length")]uint Length = default,
    [property: Description("@#sampleRate")]float SampleRate = default,
    [property: Description("@#renderSizeHint")]OfflineAudioContextOptionsRenderSizeHint? RenderSizeHint = default);

/// <summary>
/// OpenFilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#OpenFilePickerOptions")]
public record OpenFilePickerOptions(
    [property: Description("@#multiple")]bool Multiple = false) : FilePickerOptions;

/// <summary>
/// OptionalEffectTiming
/// </summary>
[ECMAScript]
[Description("@#OptionalEffectTiming")]
public record OptionalEffectTiming(
    [property: Description("@#delay")]double Delay = default,
    [property: Description("@#endDelay")]double EndDelay = default,
    [property: Description("@#fill")]FillMode? Fill = default,
    [property: Description("@#iterationStart")]double IterationStart = default,
    [property: Description("@#iterations")]double Iterations = default,
    [property: Description("@#duration")]OptionalEffectTimingDuration? Duration = default,
    [property: Description("@#direction")]PlaybackDirection? Direction = default,
    [property: Description("@#easing")]string? Easing = default,
    [property: Description("@#playbackRate")]double PlaybackRate = default)
{
    [Category("optional")]
    public extern static OptionalEffectTiming OptionalDelayEndDelayFill8(
        [Description("@#delay")]double Delay = default,
        [Description("@#endDelay")]double EndDelay = default,
        [Description("@#fill")]FillMode? Fill = default,
        [Description("@#iterationStart")]double IterationStart = default,
        [Description("@#iterations")]double Iterations = default,
        [Description("@#duration")]OptionalEffectTimingDuration? Duration = default,
        [Description("@#direction")]PlaybackDirection? Direction = default,
        [Description("@#easing")]string? Easing = default);

    [Category("optional")]
    public extern static OptionalEffectTiming OptionalPlaybackRate(
        [Description("@#playbackRate")]double PlaybackRate = default);
}

/// <summary>
/// OpusEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#OpusEncoderConfig")]
public record OpusEncoderConfig(
    [property: Description("@#format")]OpusBitstreamFormat Format = OpusBitstreamFormat.Opus,
    [property: Description("@#signal")]OpusSignal Signal = OpusSignal.Auto,
    [property: Description("@#application")]OpusApplication Application = OpusApplication.Audio,
    [property: Description("@#frameDuration")]ulong FrameDuration = 20000,
    [property: Description("@#complexity")]uint Complexity = default,
    [property: Description("@#packetlossperc")]uint Packetlossperc = 0,
    [property: Description("@#useinbandfec")]bool Useinbandfec = false,
    [property: Description("@#usedtx")]bool Usedtx = false);

/// <summary>
/// OrientationSensorOptions
/// </summary>
[ECMAScript]
[Description("@#OrientationSensorOptions")]
public record OrientationSensorOptions(
    [property: Description("@#referenceFrame")]OrientationSensorLocalCoordinateSystem ReferenceFrame = OrientationSensorLocalCoordinateSystem.Device) : SensorOptions;

/// <summary>
/// OscillatorOptions
/// </summary>
[ECMAScript]
[Description("@#OscillatorOptions")]
public record OscillatorOptions(
    [property: Description("@#type")]OscillatorType Type = OscillatorType.Sine,
    [property: Description("@#frequency")]float Frequency = 440f,
    [property: Description("@#detune")]float Detune = 0f,
    [property: Description("@#periodicWave")]PeriodicWave? PeriodicWave = default) : AudioNodeOptions;

/// <summary>
/// PageRevealEventInit
/// </summary>
[ECMAScript]
[Description("@#PageRevealEventInit")]
public record PageRevealEventInit(
    [property: Description("@#viewTransition")]ViewTransition? ViewTransition = null) : EventInit;

/// <summary>
/// PageSwapEventInit
/// </summary>
[ECMAScript]
[Description("@#PageSwapEventInit")]
public record PageSwapEventInit(
    [property: Description("@#activation")]NavigationActivation? Activation = null,
    [property: Description("@#viewTransition")]ViewTransition? ViewTransition = null) : EventInit;

/// <summary>
/// PageTransitionEventInit
/// </summary>
[ECMAScript]
[Description("@#PageTransitionEventInit")]
public record PageTransitionEventInit(
    [property: Description("@#persisted")]bool Persisted = false) : EventInit;

/// <summary>
/// PannerOptions
/// </summary>
[ECMAScript]
[Description("@#PannerOptions")]
public record PannerOptions(
    [property: Description("@#panningModel")]PanningModelType PanningModel = PanningModelType.Equalpower,
    [property: Description("@#distanceModel")]DistanceModelType DistanceModel = DistanceModelType.Inverse,
    [property: Description("@#positionX")]float PositionX = 0f,
    [property: Description("@#positionY")]float PositionY = 0f,
    [property: Description("@#positionZ")]float PositionZ = 0f,
    [property: Description("@#orientationX")]float OrientationX = 1f,
    [property: Description("@#orientationY")]float OrientationY = 0f,
    [property: Description("@#orientationZ")]float OrientationZ = 0f,
    [property: Description("@#refDistance")]double RefDistance = 1d,
    [property: Description("@#maxDistance")]double MaxDistance = 10000d,
    [property: Description("@#rolloffFactor")]double RolloffFactor = 1d,
    [property: Description("@#coneInnerAngle")]double ConeInnerAngle = 360d,
    [property: Description("@#coneOuterAngle")]double ConeOuterAngle = 360d,
    [property: Description("@#coneOuterGain")]double ConeOuterGain = 0d) : AudioNodeOptions;

/// <summary>
/// PasswordCredentialData
/// </summary>
[ECMAScript]
[Description("@#PasswordCredentialData")]
public record PasswordCredentialData(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#iconURL")]string? IconURL = default,
    [property: Description("@#origin")]string? Origin = default,
    [property: Description("@#password")]string? Password = default) : CredentialData;

/// <summary>
/// PaymentCompleteDetails
/// </summary>
[ECMAScript]
[Description("@#PaymentCompleteDetails")]
public record PaymentCompleteDetails(
    [property: Description("@#data")]object? Data = null);

/// <summary>
/// PaymentCredentialInstrument
/// </summary>
[ECMAScript]
[Description("@#PaymentCredentialInstrument")]
public record PaymentCredentialInstrument(
    [property: Description("@#displayName")]string? DisplayName = default,
    [property: Description("@#icon")]string? Icon = default,
    [property: Description("@#iconMustBeShown")]bool IconMustBeShown = false);

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
    [property: Description("@#modifiers")]PaymentDetailsModifier[]? Modifiers = default);

/// <summary>
/// PaymentDetailsInit
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsInit")]
public record PaymentDetailsInit(
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#total")]PaymentItem? Total = default) : PaymentDetailsBase;

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
/// PaymentDetailsUpdate
/// </summary>
[ECMAScript]
[Description("@#PaymentDetailsUpdate")]
public record PaymentDetailsUpdate(
    [property: Description("@#total")]PaymentItem? Total = default,
    [property: Description("@#paymentMethodErrors")]object? PaymentMethodErrors = default) : PaymentDetailsBase;

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
/// PaymentItem
/// </summary>
[ECMAScript]
[Description("@#PaymentItem")]
public record PaymentItem(
    [property: Description("@#label")]string? Label = default,
    [property: Description("@#amount")]PaymentCurrencyAmount? Amount = default,
    [property: Description("@#pending")]bool Pending = false);

/// <summary>
/// PaymentMethodChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#PaymentMethodChangeEventInit")]
public record PaymentMethodChangeEventInit(
    [property: Description("@#methodName")]string? MethodName = default,
    [property: Description("@#methodDetails")]object? MethodDetails = null) : PaymentRequestUpdateEventInit;

/// <summary>
/// PaymentMethodData
/// </summary>
[ECMAScript]
[Description("@#PaymentMethodData")]
public record PaymentMethodData(
    [property: Description("@#supportedMethods")]string? SupportedMethods = default,
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
    [property: Description("@#shippingOptions")]PaymentShippingOption[]? ShippingOptions = default) : ExtendableEventInit;

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
    [property: Description("@#error")]string? Error = default,
    [property: Description("@#paymentMethod")]object? PaymentMethod = default);

/// <summary>
/// Pbkdf2Params
/// </summary>
[ECMAScript]
[Description("@#Pbkdf2Params")]
public record Pbkdf2Params(
    [property: Description("@#salt")]IBufferSource? Salt = default,
    [property: Description("@#iterations")]uint Iterations = default,
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default) : Algorithm;

/// <summary>
/// PerformanceMarkOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceMarkOptions")]
public record PerformanceMarkOptions(
    [property: Description("@#detail")]object? Detail = default,
    [property: Description("@#startTime")]double StartTime = default);

/// <summary>
/// PerformanceMeasureOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceMeasureOptions")]
public record PerformanceMeasureOptions(
    [property: Description("@#detail")]object? Detail = default,
    [property: Description("@#start")]PerformanceMeasureOptionsStart? Start = default,
    [property: Description("@#duration")]double Duration = default,
    [property: Description("@#end")]PerformanceMeasureOptionsEnd? End = default);

/// <summary>
/// PerformanceObserverCallbackOptions
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserverCallbackOptions")]
public record PerformanceObserverCallbackOptions(
    [property: Description("@#droppedEntriesCount")]ulong DroppedEntriesCount = default);

/// <summary>
/// PerformanceObserverInit
/// </summary>
[ECMAScript]
[Description("@#PerformanceObserverInit")]
public record PerformanceObserverInit(
    [property: Description("@#durationThreshold")]double DurationThreshold = default,
    [property: Description("@#entryTypes")]string[]? EntryTypes = default,
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#buffered")]bool Buffered = default)
{
    [Category("optional")]
    public extern static PerformanceObserverInit OptionalDurationThreshold(
        [Description("@#durationThreshold")]double DurationThreshold = default);

    [Category("optional")]
    public extern static PerformanceObserverInit OptionalEntryTypesTypeBuffered(
        [Description("@#entryTypes")]string[]? EntryTypes = default,
        [Description("@#type")]string? Type = default,
        [Description("@#buffered")]bool Buffered = default);
}

/// <summary>
/// PeriodicSyncEventInit
/// </summary>
[ECMAScript]
[Description("@#PeriodicSyncEventInit")]
public record PeriodicSyncEventInit(
    [property: Description("@#tag")]string? Tag = default) : ExtendableEventInit;

/// <summary>
/// PeriodicWaveConstraints
/// </summary>
[ECMAScript]
[Description("@#PeriodicWaveConstraints")]
public record PeriodicWaveConstraints(
    [property: Description("@#disableNormalization")]bool DisableNormalization = false);

/// <summary>
/// PeriodicWaveOptions
/// </summary>
[ECMAScript]
[Description("@#PeriodicWaveOptions")]
public record PeriodicWaveOptions(
    [property: Description("@#real")]float[]? Real = default,
    [property: Description("@#imag")]float[]? Imag = default) : PeriodicWaveConstraints;

/// <summary>
/// PermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PermissionDescriptor")]
public record PermissionDescriptor(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// PermissionSetParameters
/// </summary>
[ECMAScript]
[Description("@#PermissionSetParameters")]
public record PermissionSetParameters(
    [property: Description("@#descriptor")]object? Descriptor = default,
    [property: Description("@#state")]PermissionState? State = default);

/// <summary>
/// PhotoCapabilities
/// </summary>
[ECMAScript]
[Description("@#PhotoCapabilities")]
public record PhotoCapabilities(
    [property: Description("@#redEyeReduction")]RedEyeReduction? RedEyeReduction = default,
    [property: Description("@#imageHeight")]MediaSettingsRange? ImageHeight = default,
    [property: Description("@#imageWidth")]MediaSettingsRange? ImageWidth = default,
    [property: Description("@#fillLightMode")]FillLightMode[]? FillLightMode = default);

/// <summary>
/// PhotoSettings
/// </summary>
[ECMAScript]
[Description("@#PhotoSettings")]
public record PhotoSettings(
    [property: Description("@#fillLightMode")]FillLightMode? FillLightMode = default,
    [property: Description("@#imageHeight")]double ImageHeight = default,
    [property: Description("@#imageWidth")]double ImageWidth = default,
    [property: Description("@#redEyeReduction")]bool RedEyeReduction = default);

/// <summary>
/// PictureInPictureEventInit
/// </summary>
[ECMAScript]
[Description("@#PictureInPictureEventInit")]
public record PictureInPictureEventInit(
    [property: Description("@#pictureInPictureWindow")]PictureInPictureWindow? PictureInPictureWindow = default) : EventInit;

/// <summary>
/// PlaneLayout
/// </summary>
[ECMAScript]
[Description("@#PlaneLayout")]
public record PlaneLayout(
    [property: Description("@#offset")]uint Offset = default,
    [property: Description("@#stride")]uint Stride = default);

/// <summary>
/// Point2D
/// </summary>
[ECMAScript]
[Description("@#Point2D")]
public record Point2D(
    [property: Description("@#x")]double X = 0.0d,
    [property: Description("@#y")]double Y = 0.0d);

/// <summary>
/// PointerEventInit
/// </summary>
[ECMAScript]
[Description("@#PointerEventInit")]
public record PointerEventInit(
    [property: Description("@#pointerId")]int PointerId = 0,
    [property: Description("@#width")]double Width = 1d,
    [property: Description("@#height")]double Height = 1d,
    [property: Description("@#pressure")]float Pressure = 0f,
    [property: Description("@#tangentialPressure")]float TangentialPressure = 0f,
    [property: Description("@#tiltX")]int TiltX = default,
    [property: Description("@#tiltY")]int TiltY = default,
    [property: Description("@#twist")]int Twist = 0,
    [property: Description("@#altitudeAngle")]double AltitudeAngle = default,
    [property: Description("@#azimuthAngle")]double AzimuthAngle = default,
    [property: Description("@#pointerType")]string? PointerType = default,
    [property: Description("@#isPrimary")]bool IsPrimary = false,
    [property: Description("@#coalescedEvents")]PointerEvent[]? CoalescedEvents = default,
    [property: Description("@#predictedEvents")]PointerEvent[]? PredictedEvents = default) : MouseEventInit;

/// <summary>
/// PopStateEventInit
/// </summary>
[ECMAScript]
[Description("@#PopStateEventInit")]
public record PopStateEventInit(
    [property: Description("@#state")]object? State = default,
    [property: Description("@#hasUAVisualTransition")]bool HasUAVisualTransition = false) : EventInit;

/// <summary>
/// PortalActivateEventInit
/// </summary>
[ECMAScript]
[Description("@#PortalActivateEventInit")]
public record PortalActivateEventInit(
    [property: Description("@#data")]object? Data = default) : EventInit;

/// <summary>
/// PortalActivateOptions
/// </summary>
[ECMAScript]
[Description("@#PortalActivateOptions")]
public record PortalActivateOptions(
    [property: Description("@#data")]object? Data = default) : StructuredSerializeOptions;

/// <summary>
/// PositionOptions
/// </summary>
[ECMAScript]
[Description("@#PositionOptions")]
public record PositionOptions(
    [property: Description("@#enableHighAccuracy")]bool EnableHighAccuracy = false,
    [property: Description("@#timeout")]uint Timeout = 0xFFFFFFFF,
    [property: Description("@#maximumAge")]uint MaximumAge = 0);

/// <summary>
/// PresentationConnectionAvailableEventInit
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionAvailableEventInit")]
public record PresentationConnectionAvailableEventInit(
    [property: Description("@#connection")]PresentationConnection? Connection = default) : EventInit;

/// <summary>
/// PresentationConnectionCloseEventInit
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionCloseEventInit")]
public record PresentationConnectionCloseEventInit(
    [property: Description("@#reason")]PresentationConnectionCloseReason? Reason = default,
    [property: Description("@#message")]string? Message = default) : EventInit;

/// <summary>
/// PressureObserverOptions
/// </summary>
[ECMAScript]
[Description("@#PressureObserverOptions")]
public record PressureObserverOptions(
    [property: Description("@#sampleInterval")]uint SampleInterval = 0);

/// <summary>
/// PreviousWin
/// </summary>
[ECMAScript]
[Description("@#PreviousWin")]
public record PreviousWin(
    [property: Description("@#timeDelta")]long TimeDelta = default,
    [property: Description("@#adJSON")]string? AdJSON = default);

/// <summary>
/// PrivateNetworkAccessPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PrivateNetworkAccessPermissionDescriptor")]
public record PrivateNetworkAccessPermissionDescriptor(
    [property: Description("@#id")]string? Id = default) : PermissionDescriptor;

/// <summary>
/// PrivateToken
/// </summary>
[ECMAScript]
[Description("@#PrivateToken")]
public record PrivateToken(
    [property: Description("@#version")]TokenVersion? Version = default,
    [property: Description("@#operation")]OperationType? Operation = default,
    [property: Description("@#refreshPolicy")]RefreshPolicy RefreshPolicy = RefreshPolicy.None,
    [property: Description("@#issuers")]string[]? Issuers = default);

/// <summary>
/// ProfilerFrame
/// </summary>
[ECMAScript]
[Description("@#ProfilerFrame")]
public record ProfilerFrame(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#resourceId")]ulong ResourceId = default,
    [property: Description("@#line")]ulong Line = default,
    [property: Description("@#column")]ulong Column = default);

/// <summary>
/// ProfilerInitOptions
/// </summary>
[ECMAScript]
[Description("@#ProfilerInitOptions")]
public record ProfilerInitOptions(
    [property: Description("@#sampleInterval")]double SampleInterval = default,
    [property: Description("@#maxBufferSize")]uint MaxBufferSize = default);

/// <summary>
/// ProfilerSample
/// </summary>
[ECMAScript]
[Description("@#ProfilerSample")]
public record ProfilerSample(
    [property: Description("@#timestamp")]double Timestamp = default,
    [property: Description("@#stackId")]ulong StackId = default);

/// <summary>
/// ProfilerStack
/// </summary>
[ECMAScript]
[Description("@#ProfilerStack")]
public record ProfilerStack(
    [property: Description("@#parentId")]ulong ParentId = default,
    [property: Description("@#frameId")]ulong FrameId = default);

/// <summary>
/// ProfilerTrace
/// </summary>
[ECMAScript]
[Description("@#ProfilerTrace")]
public record ProfilerTrace(
    [property: Description("@#resources")]ProfilerResource[]? Resources = default,
    [property: Description("@#frames")]ProfilerFrame[]? Frames = default,
    [property: Description("@#stacks")]ProfilerStack[]? Stacks = default,
    [property: Description("@#samples")]ProfilerSample[]? Samples = default);

/// <summary>
/// ProgressEventInit
/// </summary>
[ECMAScript]
[Description("@#ProgressEventInit")]
public record ProgressEventInit(
    [property: Description("@#lengthComputable")]bool LengthComputable = false,
    [property: Description("@#loaded")]ulong Loaded = 0,
    [property: Description("@#total")]ulong Total = 0) : EventInit;

/// <summary>
/// PromiseRejectionEventInit
/// </summary>
[ECMAScript]
[Description("@#PromiseRejectionEventInit")]
public record PromiseRejectionEventInit(
    [property: Description("@#promise")]object? Promise = default,
    [property: Description("@#reason")]object? Reason = default) : EventInit;

/// <summary>
/// PromptResponseObject
/// </summary>
[ECMAScript]
[Description("@#PromptResponseObject")]
public record PromptResponseObject(
    [property: Description("@#userChoice")]AppBannerPromptOutcome? UserChoice = default);

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
/// PublicKeyCredentialDescriptor
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialDescriptor")]
public record PublicKeyCredentialDescriptor(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#id")]IBufferSource? Id = default,
    [property: Description("@#transports")]string[]? Transports = default);

/// <summary>
/// PublicKeyCredentialDescriptorJSON
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialDescriptorJSON")]
public record PublicKeyCredentialDescriptorJSON(
    [property: Description("@#id")]Base64URLString? Id = default,
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#transports")]string[]? Transports = default);

/// <summary>
/// PublicKeyCredentialEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialEntity")]
public record PublicKeyCredentialEntity(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// PublicKeyCredentialParameters
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialParameters")]
public record PublicKeyCredentialParameters(
    [property: Description("@#type")]string? Type = default,
    [property: Description("@#alg")]COSEAlgorithmIdentifier? Alg = default);

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
/// PublicKeyCredentialRpEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialRpEntity")]
public record PublicKeyCredentialRpEntity(
    [property: Description("@#id")]string? Id = default) : PublicKeyCredentialEntity;

/// <summary>
/// PublicKeyCredentialUserEntity
/// </summary>
[ECMAScript]
[Description("@#PublicKeyCredentialUserEntity")]
public record PublicKeyCredentialUserEntity(
    [property: Description("@#id")]IBufferSource? Id = default,
    [property: Description("@#displayName")]string? DisplayName = default) : PublicKeyCredentialEntity;

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
/// PurchaseDetails
/// </summary>
[ECMAScript]
[Description("@#PurchaseDetails")]
public record PurchaseDetails(
    [property: Description("@#itemId")]string? ItemId = default,
    [property: Description("@#purchaseToken")]string? PurchaseToken = default);

/// <summary>
/// PushEventInit
/// </summary>
[ECMAScript]
[Description("@#PushEventInit")]
public record PushEventInit(
    [property: Description("@#data")]PushMessageDataInit? Data = default) : ExtendableEventInit;

/// <summary>
/// PushPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PushPermissionDescriptor")]
public record PushPermissionDescriptor(
    [property: Description("@#userVisibleOnly")]bool UserVisibleOnly = false) : PermissionDescriptor;

/// <summary>
/// PushSubscriptionChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionChangeEventInit")]
public record PushSubscriptionChangeEventInit(
    [property: Description("@#newSubscription")]PushSubscription? NewSubscription = default,
    [property: Description("@#oldSubscription")]PushSubscription? OldSubscription = default) : ExtendableEventInit;

/// <summary>
/// PushSubscriptionJSON
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionJSON")]
public record PushSubscriptionJSON(
    [property: Description("@#endpoint")]string? Endpoint = default,
    [property: Description("@#expirationTime")]EpochTimeStamp? ExpirationTime = null,
    [property: Description("@#keys")]Dictionary<string, string>? Keys = default);

/// <summary>
/// PushSubscriptionOptionsInit
/// </summary>
[ECMAScript]
[Description("@#PushSubscriptionOptionsInit")]
public record PushSubscriptionOptionsInit(
    [property: Description("@#userVisibleOnly")]bool UserVisibleOnly = false,
    [property: Description("@#applicationServerKey")]PushSubscriptionOptionsInitApplicationServerKey? ApplicationServerKey = default);

/// <summary>
/// QueryOptions
/// </summary>
[ECMAScript]
[Description("@#QueryOptions")]
public record QueryOptions(
    [property: Description("@#postscriptNames")]string[]? PostscriptNames = default);

/// <summary>
/// QueuingStrategy
/// </summary>
[ECMAScript]
[Description("@#QueuingStrategy")]
public record QueuingStrategy(
    [property: Description("@#highWaterMark")]double HighWaterMark = default,
    [property: Description("@#size")]QueuingStrategySize? Size = default);

/// <summary>
/// QueuingStrategyInit
/// </summary>
[ECMAScript]
[Description("@#QueuingStrategyInit")]
public record QueuingStrategyInit(
    [property: Description("@#highWaterMark")]double HighWaterMark = default);

/// <summary>
/// RTCAudioPlayoutStats
/// </summary>
[ECMAScript]
[Description("@#RTCAudioPlayoutStats")]
public record RTCAudioPlayoutStats(
    [property: Description("@#kind")]string? Kind = default,
    [property: Description("@#synthesizedSamplesDuration")]double SynthesizedSamplesDuration = default,
    [property: Description("@#synthesizedSamplesEvents")]uint SynthesizedSamplesEvents = default,
    [property: Description("@#totalSamplesDuration")]double TotalSamplesDuration = default,
    [property: Description("@#totalPlayoutDelay")]double TotalPlayoutDelay = default,
    [property: Description("@#totalSamplesCount")]ulong TotalSamplesCount = default) : RTCStats;

/// <summary>
/// RTCAudioSourceStats
/// </summary>
[ECMAScript]
[Description("@#RTCAudioSourceStats")]
public record RTCAudioSourceStats(
    [property: Description("@#audioLevel")]double AudioLevel = default,
    [property: Description("@#totalAudioEnergy")]double TotalAudioEnergy = default,
    [property: Description("@#totalSamplesDuration")]double TotalSamplesDuration = default,
    [property: Description("@#echoReturnLoss")]double EchoReturnLoss = default,
    [property: Description("@#echoReturnLossEnhancement")]double EchoReturnLossEnhancement = default) : RTCMediaSourceStats;

/// <summary>
/// RTCCertificateExpiration
/// </summary>
[ECMAScript]
[Description("@#RTCCertificateExpiration")]
public record RTCCertificateExpiration(
    [property: Description("@#expires")]ulong Expires = default);

/// <summary>
/// RTCCertificateStats
/// </summary>
[ECMAScript]
[Description("@#RTCCertificateStats")]
public record RTCCertificateStats(
    [property: Description("@#fingerprint")]string? Fingerprint = default,
    [property: Description("@#fingerprintAlgorithm")]string? FingerprintAlgorithm = default,
    [property: Description("@#base64Certificate")]string? Base64Certificate = default,
    [property: Description("@#issuerCertificateId")]string? IssuerCertificateId = default) : RTCStats;

/// <summary>
/// RTCCodecStats
/// </summary>
[ECMAScript]
[Description("@#RTCCodecStats")]
public record RTCCodecStats(
    [property: Description("@#payloadType")]uint PayloadType = default,
    [property: Description("@#transportId")]string? TransportId = default,
    [property: Description("@#mimeType")]string? MimeType = default,
    [property: Description("@#clockRate")]uint ClockRate = default,
    [property: Description("@#channels")]uint Channels = default,
    [property: Description("@#sdpFmtpLine")]string? SdpFmtpLine = default) : RTCStats;

/// <summary>
/// RTCConfiguration
/// </summary>
[ECMAScript]
[Description("@#RTCConfiguration")]
public record RTCConfiguration(
    [property: Description("@#iceServers")]RTCIceServer[]? IceServers = default,
    [property: Description("@#iceTransportPolicy")]RTCIceTransportPolicy IceTransportPolicy = RTCIceTransportPolicy.All,
    [property: Description("@#bundlePolicy")]RTCBundlePolicy BundlePolicy = RTCBundlePolicy.Balanced,
    [property: Description("@#rtcpMuxPolicy")]RTCRtcpMuxPolicy RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
    [property: Description("@#certificates")]RTCCertificate[]? Certificates = default,
    [property: Description("@#iceCandidatePoolSize")]byte IceCandidatePoolSize = 0,
    [property: Description("@#peerIdentity")]string? PeerIdentity = default)
{
    [Category("optional")]
    public extern static RTCConfiguration OptionalIceServersIceTransportPolicyBundlePolicy6(
        [Description("@#iceServers")]RTCIceServer[]? iceServers = default,
        [Description("@#iceTransportPolicy")]RTCIceTransportPolicy iceTransportPolicy = RTCIceTransportPolicy.All,
        [Description("@#bundlePolicy")]RTCBundlePolicy bundlePolicy = RTCBundlePolicy.Balanced,
        [Description("@#rtcpMuxPolicy")]RTCRtcpMuxPolicy rtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
        [Description("@#certificates")]RTCCertificate[]? certificates = default,
        [Description("@#iceCandidatePoolSize")]byte iceCandidatePoolSize = 0);

    [Category("optional")]
    public extern static RTCConfiguration OptionalPeerIdentity(
        [Description("@#peerIdentity")]string? PeerIdentity = default);
}

/// <summary>
/// RTCDTMFToneChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCDTMFToneChangeEventInit")]
public record RTCDTMFToneChangeEventInit(
    [property: Description("@#tone")]string? Tone = default) : EventInit;

/// <summary>
/// RTCDataChannelEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelEventInit")]
public record RTCDataChannelEventInit(
    [property: Description("@#channel")]RTCDataChannel? Channel = default) : EventInit;

/// <summary>
/// RTCDataChannelInit
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelInit")]
public record RTCDataChannelInit(
    [property: Description("@#ordered")]bool Ordered = false,
    [property: Description("@#maxPacketLifeTime")]ushort MaxPacketLifeTime = default,
    [property: Description("@#maxRetransmits")]ushort MaxRetransmits = default,
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#negotiated")]bool Negotiated = false,
    [property: Description("@#id")]ushort Id = default,
    [property: Description("@#priority")]RTCPriorityType Priority = RTCPriorityType.Low)
{
    [Category("optional")]
    public extern static RTCDataChannelInit OptionalOrderedMaxPacketLifeTimeMaxRetransmits6(
        [Description("@#ordered")]bool ordered = false,
        [Description("@#maxPacketLifeTime")]ushort MaxPacketLifeTime = default,
        [Description("@#maxRetransmits")]ushort MaxRetransmits = default,
        [Description("@#protocol")]string? protocol = default,
        [Description("@#negotiated")]bool negotiated = false,
        [Description("@#id")]ushort Id = default);

    [Category("optional")]
    public extern static RTCDataChannelInit OptionalPriority(
        [Description("@#priority")]RTCPriorityType priority = RTCPriorityType.Low);
}

/// <summary>
/// RTCDataChannelStats
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelStats")]
public record RTCDataChannelStats(
    [property: Description("@#label")]string? Label = default,
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#dataChannelIdentifier")]ushort DataChannelIdentifier = default,
    [property: Description("@#state")]RTCDataChannelState? State = default,
    [property: Description("@#messagesSent")]uint MessagesSent = default,
    [property: Description("@#bytesSent")]ulong BytesSent = default,
    [property: Description("@#messagesReceived")]uint MessagesReceived = default,
    [property: Description("@#bytesReceived")]ulong BytesReceived = default) : RTCStats;

/// <summary>
/// RTCDtlsFingerprint
/// </summary>
[ECMAScript]
[Description("@#RTCDtlsFingerprint")]
public record RTCDtlsFingerprint(
    [property: Description("@#algorithm")]string? Algorithm = default,
    [property: Description("@#value")]string? Value = default);

/// <summary>
/// RTCEncodedAudioFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedAudioFrameMetadata")]
public record RTCEncodedAudioFrameMetadata(
    [property: Description("@#synchronizationSource")]uint SynchronizationSource = default,
    [property: Description("@#payloadType")]byte PayloadType = default,
    [property: Description("@#contributingSources")]uint[]? ContributingSources = default,
    [property: Description("@#sequenceNumber")]short SequenceNumber = default,
    [property: Description("@#rtpTimestamp")]uint RtpTimestamp = default,
    [property: Description("@#mimeType")]string? MimeType = default);

/// <summary>
/// RTCEncodedVideoFrameMetadata
/// </summary>
[ECMAScript]
[Description("@#RTCEncodedVideoFrameMetadata")]
public record RTCEncodedVideoFrameMetadata(
    [property: Description("@#frameId")]ulong FrameId = default,
    [property: Description("@#dependencies")]ulong[]? Dependencies = default,
    [property: Description("@#width")]ushort Width = default,
    [property: Description("@#height")]ushort Height = default,
    [property: Description("@#spatialIndex")]uint SpatialIndex = default,
    [property: Description("@#temporalIndex")]uint TemporalIndex = default,
    [property: Description("@#synchronizationSource")]uint SynchronizationSource = default,
    [property: Description("@#payloadType")]byte PayloadType = default,
    [property: Description("@#contributingSources")]uint[]? ContributingSources = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#rtpTimestamp")]uint RtpTimestamp = default,
    [property: Description("@#mimeType")]string? MimeType = default);

/// <summary>
/// RTCErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCErrorEventInit")]
public record RTCErrorEventInit(
    [property: Description("@#error")]RTCError? Error = default) : EventInit;

/// <summary>
/// RTCErrorInit
/// </summary>
[ECMAScript]
[Description("@#RTCErrorInit")]
public record RTCErrorInit(
    [property: Description("@#errorDetail")]RTCErrorDetailType? ErrorDetail = default,
    [property: Description("@#sdpLineNumber")]int SdpLineNumber = default,
    [property: Description("@#sctpCauseCode")]int SctpCauseCode = default,
    [property: Description("@#receivedAlert")]uint ReceivedAlert = default,
    [property: Description("@#sentAlert")]uint SentAlert = default,
    [property: Description("@#httpRequestStatusCode")]int HttpRequestStatusCode = default)
{
    [Category("optional")]
    public extern static RTCErrorInit OptionalErrorDetailSdpLineNumberSctpCauseCode5(
        [Description("@#errorDetail")]RTCErrorDetailType? ErrorDetail = default,
        [Description("@#sdpLineNumber")]int SdpLineNumber = default,
        [Description("@#sctpCauseCode")]int SctpCauseCode = default,
        [Description("@#receivedAlert")]uint ReceivedAlert = default,
        [Description("@#sentAlert")]uint SentAlert = default);

    [Category("optional")]
    public extern static RTCErrorInit OptionalHttpRequestStatusCode(
        [Description("@#httpRequestStatusCode")]int HttpRequestStatusCode = default);
}

/// <summary>
/// RTCIceCandidateInit
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidateInit")]
public record RTCIceCandidateInit(
    [property: Description("@#candidate")]string? Candidate = default,
    [property: Description("@#sdpMid")]string? SdpMid = null,
    [property: Description("@#sdpMLineIndex")]ushort? SdpMLineIndex = null,
    [property: Description("@#usernameFragment")]string? UsernameFragment = null);

/// <summary>
/// RTCIceCandidatePair
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidatePair")]
public record RTCIceCandidatePair(
    [property: Description("@#local")]RTCIceCandidate? Local = default,
    [property: Description("@#remote")]RTCIceCandidate? Remote = default);

/// <summary>
/// RTCIceCandidatePairStats
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidatePairStats")]
public record RTCIceCandidatePairStats(
    [property: Description("@#transportId")]string? TransportId = default,
    [property: Description("@#localCandidateId")]string? LocalCandidateId = default,
    [property: Description("@#remoteCandidateId")]string? RemoteCandidateId = default,
    [property: Description("@#state")]RTCStatsIceCandidatePairState? State = default,
    [property: Description("@#nominated")]bool Nominated = default,
    [property: Description("@#packetsSent")]ulong PacketsSent = default,
    [property: Description("@#packetsReceived")]ulong PacketsReceived = default,
    [property: Description("@#bytesSent")]ulong BytesSent = default,
    [property: Description("@#bytesReceived")]ulong BytesReceived = default,
    [property: Description("@#lastPacketSentTimestamp")]double LastPacketSentTimestamp = default,
    [property: Description("@#lastPacketReceivedTimestamp")]double LastPacketReceivedTimestamp = default,
    [property: Description("@#totalRoundTripTime")]double TotalRoundTripTime = default,
    [property: Description("@#currentRoundTripTime")]double CurrentRoundTripTime = default,
    [property: Description("@#availableOutgoingBitrate")]double AvailableOutgoingBitrate = default,
    [property: Description("@#availableIncomingBitrate")]double AvailableIncomingBitrate = default,
    [property: Description("@#requestsReceived")]ulong RequestsReceived = default,
    [property: Description("@#requestsSent")]ulong RequestsSent = default,
    [property: Description("@#responsesReceived")]ulong ResponsesReceived = default,
    [property: Description("@#responsesSent")]ulong ResponsesSent = default,
    [property: Description("@#consentRequestsSent")]ulong ConsentRequestsSent = default,
    [property: Description("@#packetsDiscardedOnSend")]uint PacketsDiscardedOnSend = default,
    [property: Description("@#bytesDiscardedOnSend")]ulong BytesDiscardedOnSend = default) : RTCStats;

/// <summary>
/// RTCIceCandidateStats
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidateStats")]
public record RTCIceCandidateStats(
    [property: Description("@#transportId")]string? TransportId = default,
    [property: Description("@#address")]string? Address = default,
    [property: Description("@#port")]int Port = default,
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#candidateType")]RTCIceCandidateType? CandidateType = default,
    [property: Description("@#priority")]int Priority = default,
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#relayProtocol")]RTCIceServerTransportProtocol? RelayProtocol = default,
    [property: Description("@#foundation")]string? Foundation = default,
    [property: Description("@#relatedAddress")]string? RelatedAddress = default,
    [property: Description("@#relatedPort")]int RelatedPort = default,
    [property: Description("@#usernameFragment")]string? UsernameFragment = default,
    [property: Description("@#tcpType")]RTCIceTcpCandidateType? TcpType = default) : RTCStats;

/// <summary>
/// RTCIceGatherOptions
/// </summary>
[ECMAScript]
[Description("@#RTCIceGatherOptions")]
public record RTCIceGatherOptions(
    [property: Description("@#gatherPolicy")]RTCIceTransportPolicy GatherPolicy = RTCIceTransportPolicy.All,
    [property: Description("@#iceServers")]RTCIceServer[]? IceServers = default);

/// <summary>
/// RTCIceParameters
/// </summary>
[ECMAScript]
[Description("@#RTCIceParameters")]
public record RTCIceParameters(
    [property: Description("@#usernameFragment")]string? UsernameFragment = default,
    [property: Description("@#password")]string? Password = default,
    [property: Description("@#iceLite")]bool IceLite = default)
{
    [Category("optional")]
    public extern static RTCIceParameters OptionalUsernameFragmentPassword(
        [Description("@#usernameFragment")]string? UsernameFragment = default,
        [Description("@#password")]string? Password = default);

    [Category("optional")]
    public extern static RTCIceParameters OptionalIceLite(
        [Description("@#iceLite")]bool IceLite = default);
}

/// <summary>
/// RTCIceServer
/// </summary>
[ECMAScript]
[Description("@#RTCIceServer")]
public record RTCIceServer(
    [property: Description("@#urls")]RTCIceServerUrls? Urls = default,
    [property: Description("@#username")]string? Username = default,
    [property: Description("@#credential")]string? Credential = default);

/// <summary>
/// RTCIdentityAssertionResult
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityAssertionResult")]
public record RTCIdentityAssertionResult(
    [property: Description("@#idp")]RTCIdentityProviderDetails? Idp = default,
    [property: Description("@#assertion")]string? Assertion = default);

/// <summary>
/// RTCIdentityProvider
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProvider")]
public record RTCIdentityProvider(
    [property: Description("@#generateAssertion")]GenerateAssertionCallback? GenerateAssertion = default,
    [property: Description("@#validateAssertion")]ValidateAssertionCallback? ValidateAssertion = default);

/// <summary>
/// RTCIdentityProviderDetails
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderDetails")]
public record RTCIdentityProviderDetails(
    [property: Description("@#domain")]string? Domain = default,
    [property: Description("@#protocol")]string? Protocol = default);

/// <summary>
/// RTCIdentityProviderOptions
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderOptions")]
public record RTCIdentityProviderOptions(
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#usernameHint")]string? UsernameHint = default,
    [property: Description("@#peerIdentity")]string? PeerIdentity = default);

/// <summary>
/// RTCIdentityValidationResult
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityValidationResult")]
public record RTCIdentityValidationResult(
    [property: Description("@#identity")]string? Identity = default,
    [property: Description("@#contents")]string? Contents = default);

/// <summary>
/// RTCInboundRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCInboundRtpStreamStats")]
public record RTCInboundRtpStreamStats(
    [property: Description("@#trackIdentifier")]string? TrackIdentifier = default,
    [property: Description("@#mid")]string? Mid = default,
    [property: Description("@#remoteId")]string? RemoteId = default,
    [property: Description("@#framesDecoded")]uint FramesDecoded = default,
    [property: Description("@#keyFramesDecoded")]uint KeyFramesDecoded = default,
    [property: Description("@#framesRendered")]uint FramesRendered = default,
    [property: Description("@#framesDropped")]uint FramesDropped = default,
    [property: Description("@#frameWidth")]uint FrameWidth = default,
    [property: Description("@#frameHeight")]uint FrameHeight = default,
    [property: Description("@#framesPerSecond")]double FramesPerSecond = default,
    [property: Description("@#qpSum")]ulong QpSum = default,
    [property: Description("@#totalDecodeTime")]double TotalDecodeTime = default,
    [property: Description("@#totalInterFrameDelay")]double TotalInterFrameDelay = default,
    [property: Description("@#totalSquaredInterFrameDelay")]double TotalSquaredInterFrameDelay = default,
    [property: Description("@#pauseCount")]uint PauseCount = default,
    [property: Description("@#totalPausesDuration")]double TotalPausesDuration = default,
    [property: Description("@#freezeCount")]uint FreezeCount = default,
    [property: Description("@#totalFreezesDuration")]double TotalFreezesDuration = default,
    [property: Description("@#lastPacketReceivedTimestamp")]double LastPacketReceivedTimestamp = default,
    [property: Description("@#headerBytesReceived")]ulong HeaderBytesReceived = default,
    [property: Description("@#packetsDiscarded")]ulong PacketsDiscarded = default,
    [property: Description("@#fecBytesReceived")]ulong FecBytesReceived = default,
    [property: Description("@#fecPacketsReceived")]ulong FecPacketsReceived = default,
    [property: Description("@#fecPacketsDiscarded")]ulong FecPacketsDiscarded = default,
    [property: Description("@#bytesReceived")]ulong BytesReceived = default,
    [property: Description("@#nackCount")]uint NackCount = default,
    [property: Description("@#firCount")]uint FirCount = default,
    [property: Description("@#pliCount")]uint PliCount = default,
    [property: Description("@#totalProcessingDelay")]double TotalProcessingDelay = default,
    [property: Description("@#estimatedPlayoutTimestamp")]double EstimatedPlayoutTimestamp = default,
    [property: Description("@#jitterBufferDelay")]double JitterBufferDelay = default,
    [property: Description("@#jitterBufferTargetDelay")]double JitterBufferTargetDelay = default,
    [property: Description("@#jitterBufferEmittedCount")]ulong JitterBufferEmittedCount = default,
    [property: Description("@#jitterBufferMinimumDelay")]double JitterBufferMinimumDelay = default,
    [property: Description("@#totalSamplesReceived")]ulong TotalSamplesReceived = default,
    [property: Description("@#concealedSamples")]ulong ConcealedSamples = default,
    [property: Description("@#silentConcealedSamples")]ulong SilentConcealedSamples = default,
    [property: Description("@#concealmentEvents")]ulong ConcealmentEvents = default,
    [property: Description("@#insertedSamplesForDeceleration")]ulong InsertedSamplesForDeceleration = default,
    [property: Description("@#removedSamplesForAcceleration")]ulong RemovedSamplesForAcceleration = default,
    [property: Description("@#audioLevel")]double AudioLevel = default,
    [property: Description("@#totalAudioEnergy")]double TotalAudioEnergy = default,
    [property: Description("@#totalSamplesDuration")]double TotalSamplesDuration = default,
    [property: Description("@#framesReceived")]uint FramesReceived = default,
    [property: Description("@#decoderImplementation")]string? DecoderImplementation = default,
    [property: Description("@#playoutId")]string? PlayoutId = default,
    [property: Description("@#powerEfficientDecoder")]bool PowerEfficientDecoder = default,
    [property: Description("@#framesAssembledFromMultiplePackets")]uint FramesAssembledFromMultiplePackets = default,
    [property: Description("@#totalAssemblyTime")]double TotalAssemblyTime = default,
    [property: Description("@#retransmittedPacketsReceived")]ulong RetransmittedPacketsReceived = default,
    [property: Description("@#retransmittedBytesReceived")]ulong RetransmittedBytesReceived = default,
    [property: Description("@#rtxSsrc")]uint RtxSsrc = default,
    [property: Description("@#fecSsrc")]uint FecSsrc = default) : RTCReceivedRtpStreamStats;

/// <summary>
/// RTCLocalSessionDescriptionInit
/// </summary>
[ECMAScript]
[Description("@#RTCLocalSessionDescriptionInit")]
public record RTCLocalSessionDescriptionInit(
    [property: Description("@#type")]RTCSdpType? Type = default,
    [property: Description("@#sdp")]string? Sdp = default);

/// <summary>
/// RTCMediaSourceStats
/// </summary>
[ECMAScript]
[Description("@#RTCMediaSourceStats")]
public record RTCMediaSourceStats(
    [property: Description("@#trackIdentifier")]string? TrackIdentifier = default,
    [property: Description("@#kind")]string? Kind = default) : RTCStats;

/// <summary>
/// RTCOfferOptions
/// </summary>
[ECMAScript]
[Description("@#RTCOfferOptions")]
public record RTCOfferOptions(
    [property: Description("@#iceRestart")]bool IceRestart = false,
    [property: Description("@#offerToReceiveAudio")]bool OfferToReceiveAudio = default,
    [property: Description("@#offerToReceiveVideo")]bool OfferToReceiveVideo = default) : RTCOfferAnswerOptions
{
    [Category("optional")]
    public extern static RTCOfferOptions OptionalIceRestart(
        [Description("@#iceRestart")]bool iceRestart = false);

    [Category("optional")]
    public extern static RTCOfferOptions OptionalOfferToReceiveAudioOfferToReceiveVideo(
        [Description("@#offerToReceiveAudio")]bool OfferToReceiveAudio = default,
        [Description("@#offerToReceiveVideo")]bool OfferToReceiveVideo = default);
}

/// <summary>
/// RTCOutboundRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCOutboundRtpStreamStats")]
public record RTCOutboundRtpStreamStats(
    [property: Description("@#mid")]string? Mid = default,
    [property: Description("@#mediaSourceId")]string? MediaSourceId = default,
    [property: Description("@#remoteId")]string? RemoteId = default,
    [property: Description("@#rid")]string? Rid = default,
    [property: Description("@#headerBytesSent")]ulong HeaderBytesSent = default,
    [property: Description("@#retransmittedPacketsSent")]ulong RetransmittedPacketsSent = default,
    [property: Description("@#retransmittedBytesSent")]ulong RetransmittedBytesSent = default,
    [property: Description("@#rtxSsrc")]uint RtxSsrc = default,
    [property: Description("@#targetBitrate")]double TargetBitrate = default,
    [property: Description("@#totalEncodedBytesTarget")]ulong TotalEncodedBytesTarget = default,
    [property: Description("@#frameWidth")]uint FrameWidth = default,
    [property: Description("@#frameHeight")]uint FrameHeight = default,
    [property: Description("@#framesPerSecond")]double FramesPerSecond = default,
    [property: Description("@#framesSent")]uint FramesSent = default,
    [property: Description("@#hugeFramesSent")]uint HugeFramesSent = default,
    [property: Description("@#framesEncoded")]uint FramesEncoded = default,
    [property: Description("@#keyFramesEncoded")]uint KeyFramesEncoded = default,
    [property: Description("@#qpSum")]ulong QpSum = default,
    [property: Description("@#totalEncodeTime")]double TotalEncodeTime = default,
    [property: Description("@#totalPacketSendDelay")]double TotalPacketSendDelay = default,
    [property: Description("@#qualityLimitationReason")]RTCQualityLimitationReason? QualityLimitationReason = default,
    [property: Description("@#qualityLimitationDurations")]Dictionary<string, double>? QualityLimitationDurations = default,
    [property: Description("@#qualityLimitationResolutionChanges")]uint QualityLimitationResolutionChanges = default,
    [property: Description("@#nackCount")]uint NackCount = default,
    [property: Description("@#firCount")]uint FirCount = default,
    [property: Description("@#pliCount")]uint PliCount = default,
    [property: Description("@#encoderImplementation")]string? EncoderImplementation = default,
    [property: Description("@#powerEfficientEncoder")]bool PowerEfficientEncoder = default,
    [property: Description("@#active")]bool Active = default,
    [property: Description("@#scalabilityMode")]string? ScalabilityMode = default) : RTCSentRtpStreamStats;

/// <summary>
/// RTCPeerConnectionIceErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceErrorEventInit")]
public record RTCPeerConnectionIceErrorEventInit(
    [property: Description("@#address")]string? Address = default,
    [property: Description("@#port")]ushort Port = default,
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#errorCode")]ushort ErrorCode = default,
    [property: Description("@#errorText")]string? ErrorText = default) : EventInit;

/// <summary>
/// RTCPeerConnectionIceEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceEventInit")]
public record RTCPeerConnectionIceEventInit(
    [property: Description("@#candidate")]RTCIceCandidate? Candidate = default,
    [property: Description("@#url")]string? Url = default) : EventInit;

/// <summary>
/// RTCPeerConnectionStats
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionStats")]
public record RTCPeerConnectionStats(
    [property: Description("@#dataChannelsOpened")]uint DataChannelsOpened = default,
    [property: Description("@#dataChannelsClosed")]uint DataChannelsClosed = default) : RTCStats;

/// <summary>
/// RTCReceivedRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCReceivedRtpStreamStats")]
public record RTCReceivedRtpStreamStats(
    [property: Description("@#packetsReceived")]ulong PacketsReceived = default,
    [property: Description("@#packetsLost")]long PacketsLost = default,
    [property: Description("@#jitter")]double Jitter = default) : RTCRtpStreamStats;

/// <summary>
/// RTCRemoteInboundRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCRemoteInboundRtpStreamStats")]
public record RTCRemoteInboundRtpStreamStats(
    [property: Description("@#localId")]string? LocalId = default,
    [property: Description("@#roundTripTime")]double RoundTripTime = default,
    [property: Description("@#totalRoundTripTime")]double TotalRoundTripTime = default,
    [property: Description("@#fractionLost")]double FractionLost = default,
    [property: Description("@#roundTripTimeMeasurements")]ulong RoundTripTimeMeasurements = default) : RTCReceivedRtpStreamStats;

/// <summary>
/// RTCRemoteOutboundRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCRemoteOutboundRtpStreamStats")]
public record RTCRemoteOutboundRtpStreamStats(
    [property: Description("@#localId")]string? LocalId = default,
    [property: Description("@#remoteTimestamp")]double RemoteTimestamp = default,
    [property: Description("@#reportsSent")]ulong ReportsSent = default,
    [property: Description("@#roundTripTime")]double RoundTripTime = default,
    [property: Description("@#totalRoundTripTime")]double TotalRoundTripTime = default,
    [property: Description("@#roundTripTimeMeasurements")]ulong RoundTripTimeMeasurements = default) : RTCSentRtpStreamStats;

/// <summary>
/// RTCRtcpParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtcpParameters")]
public record RTCRtcpParameters(
    [property: Description("@#cname")]string? Cname = default,
    [property: Description("@#reducedSize")]bool ReducedSize = default);

/// <summary>
/// RTCRtpCapabilities
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCapabilities")]
public record RTCRtpCapabilities(
    [property: Description("@#codecs")]RTCRtpCodecCapability[]? Codecs = default,
    [property: Description("@#headerExtensions")]RTCRtpHeaderExtensionCapability[]? HeaderExtensions = default);

/// <summary>
/// RTCRtpCodec
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodec")]
public record RTCRtpCodec(
    [property: Description("@#mimeType")]string? MimeType = default,
    [property: Description("@#clockRate")]uint ClockRate = default,
    [property: Description("@#channels")]ushort Channels = default,
    [property: Description("@#sdpFmtpLine")]string? SdpFmtpLine = default);

/// <summary>
/// RTCRtpCodecParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodecParameters")]
public record RTCRtpCodecParameters(
    [property: Description("@#payloadType")]byte PayloadType = default) : RTCRtpCodec;

/// <summary>
/// RTCRtpCodingParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodingParameters")]
public record RTCRtpCodingParameters(
    [property: Description("@#rid")]string? Rid = default);

/// <summary>
/// RTCRtpContributingSource
/// </summary>
[ECMAScript]
[Description("@#RTCRtpContributingSource")]
public record RTCRtpContributingSource(
    [property: Description("@#timestamp")]double Timestamp = default,
    [property: Description("@#source")]uint Source = default,
    [property: Description("@#audioLevel")]double AudioLevel = default,
    [property: Description("@#rtpTimestamp")]uint RtpTimestamp = default);

/// <summary>
/// RTCRtpEncodingParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpEncodingParameters")]
public record RTCRtpEncodingParameters(
    [property: Description("@#active")]bool Active = false,
    [property: Description("@#maxBitrate")]uint MaxBitrate = default,
    [property: Description("@#maxFramerate")]double MaxFramerate = default,
    [property: Description("@#scaleResolutionDownBy")]double ScaleResolutionDownBy = default,
    [property: Description("@#priority")]RTCPriorityType Priority = RTCPriorityType.Low,
    [property: Description("@#networkPriority")]RTCPriorityType? NetworkPriority = default,
    [property: Description("@#scalabilityMode")]string? ScalabilityMode = default) : RTCRtpCodingParameters
{
    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalActiveMaxBitrateMaxFramerate4(
        [Description("@#active")]bool active = false,
        [Description("@#maxBitrate")]uint MaxBitrate = default,
        [Description("@#maxFramerate")]double MaxFramerate = default,
        [Description("@#scaleResolutionDownBy")]double ScaleResolutionDownBy = default);

    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalPriorityNetworkPriority(
        [Description("@#priority")]RTCPriorityType priority = RTCPriorityType.Low,
        [Description("@#networkPriority")]RTCPriorityType? NetworkPriority = default);

    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalScalabilityMode(
        [Description("@#scalabilityMode")]string? ScalabilityMode = default);
}

/// <summary>
/// RTCRtpHeaderExtensionCapability
/// </summary>
[ECMAScript]
[Description("@#RTCRtpHeaderExtensionCapability")]
public record RTCRtpHeaderExtensionCapability(
    [property: Description("@#uri")]string? Uri = default);

/// <summary>
/// RTCRtpHeaderExtensionParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpHeaderExtensionParameters")]
public record RTCRtpHeaderExtensionParameters(
    [property: Description("@#uri")]string? Uri = default,
    [property: Description("@#id")]ushort Id = default,
    [property: Description("@#encrypted")]bool Encrypted = false);

/// <summary>
/// RTCRtpParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpParameters")]
public record RTCRtpParameters(
    [property: Description("@#headerExtensions")]RTCRtpHeaderExtensionParameters[]? HeaderExtensions = default,
    [property: Description("@#rtcp")]RTCRtcpParameters? Rtcp = default,
    [property: Description("@#codecs")]RTCRtpCodecParameters[]? Codecs = default);

/// <summary>
/// RTCRtpSendParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpSendParameters")]
public record RTCRtpSendParameters(
    [property: Description("@#degradationPreference")]RTCDegradationPreference? DegradationPreference = default,
    [property: Description("@#transactionId")]string? TransactionId = default,
    [property: Description("@#encodings")]RTCRtpEncodingParameters[]? Encodings = default) : RTCRtpParameters
{
    [Category("optional")]
    public extern static RTCRtpSendParameters OptionalDegradationPreference(
        [Description("@#degradationPreference")]RTCDegradationPreference? DegradationPreference = default);

    [Category("optional")]
    public extern static RTCRtpSendParameters OptionalTransactionIdEncodings(
        [Description("@#transactionId")]string? TransactionId = default,
        [Description("@#encodings")]RTCRtpEncodingParameters[]? Encodings = default);
}

/// <summary>
/// RTCRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCRtpStreamStats")]
public record RTCRtpStreamStats(
    [property: Description("@#ssrc")]uint Ssrc = default,
    [property: Description("@#kind")]string? Kind = default,
    [property: Description("@#transportId")]string? TransportId = default,
    [property: Description("@#codecId")]string? CodecId = default) : RTCStats;

/// <summary>
/// RTCRtpTransceiverInit
/// </summary>
[ECMAScript]
[Description("@#RTCRtpTransceiverInit")]
public record RTCRtpTransceiverInit(
    [property: Description("@#direction")]RTCRtpTransceiverDirection Direction = RTCRtpTransceiverDirection.Sendrecv,
    [property: Description("@#streams")]MediaStream[]? Streams = default,
    [property: Description("@#sendEncodings")]RTCRtpEncodingParameters[]? SendEncodings = default);

/// <summary>
/// RTCSentRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCSentRtpStreamStats")]
public record RTCSentRtpStreamStats(
    [property: Description("@#packetsSent")]ulong PacketsSent = default,
    [property: Description("@#bytesSent")]ulong BytesSent = default) : RTCRtpStreamStats;

/// <summary>
/// RTCSessionDescriptionInit
/// </summary>
[ECMAScript]
[Description("@#RTCSessionDescriptionInit")]
public record RTCSessionDescriptionInit(
    [property: Description("@#type")]RTCSdpType? Type = default,
    [property: Description("@#sdp")]string? Sdp = default);

/// <summary>
/// RTCStats
/// </summary>
[ECMAScript]
[Description("@#RTCStats")]
public record RTCStats(
    [property: Description("@#timestamp")]double Timestamp = default,
    [property: Description("@#type")]RTCStatsType? Type = default,
    [property: Description("@#id")]string? Id = default);

/// <summary>
/// RTCTrackEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCTrackEventInit")]
public record RTCTrackEventInit(
    [property: Description("@#receiver")]RTCRtpReceiver? Receiver = default,
    [property: Description("@#track")]MediaStreamTrack? Track = default,
    [property: Description("@#streams")]MediaStream[]? Streams = default,
    [property: Description("@#transceiver")]RTCRtpTransceiver? Transceiver = default) : EventInit;

/// <summary>
/// RTCTransportStats
/// </summary>
[ECMAScript]
[Description("@#RTCTransportStats")]
public record RTCTransportStats(
    [property: Description("@#packetsSent")]ulong PacketsSent = default,
    [property: Description("@#packetsReceived")]ulong PacketsReceived = default,
    [property: Description("@#bytesSent")]ulong BytesSent = default,
    [property: Description("@#bytesReceived")]ulong BytesReceived = default,
    [property: Description("@#iceRole")]RTCIceRole? IceRole = default,
    [property: Description("@#iceLocalUsernameFragment")]string? IceLocalUsernameFragment = default,
    [property: Description("@#dtlsState")]RTCDtlsTransportState? DtlsState = default,
    [property: Description("@#iceState")]RTCIceTransportState? IceState = default,
    [property: Description("@#selectedCandidatePairId")]string? SelectedCandidatePairId = default,
    [property: Description("@#localCertificateId")]string? LocalCertificateId = default,
    [property: Description("@#remoteCertificateId")]string? RemoteCertificateId = default,
    [property: Description("@#tlsVersion")]string? TlsVersion = default,
    [property: Description("@#dtlsCipher")]string? DtlsCipher = default,
    [property: Description("@#dtlsRole")]RTCDtlsRole? DtlsRole = default,
    [property: Description("@#srtpCipher")]string? SrtpCipher = default,
    [property: Description("@#selectedCandidatePairChanges")]uint SelectedCandidatePairChanges = default) : RTCStats;

/// <summary>
/// RTCVideoSourceStats
/// </summary>
[ECMAScript]
[Description("@#RTCVideoSourceStats")]
public record RTCVideoSourceStats(
    [property: Description("@#width")]uint Width = default,
    [property: Description("@#height")]uint Height = default,
    [property: Description("@#frames")]uint Frames = default,
    [property: Description("@#framesPerSecond")]double FramesPerSecond = default) : RTCMediaSourceStats;

/// <summary>
/// ReadOptions
/// </summary>
[ECMAScript]
[Description("@#ReadOptions")]
public record ReadOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default) : GeolocationSensorOptions;

/// <summary>
/// ReadableStreamBYOBReaderReadOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamBYOBReaderReadOptions")]
public record ReadableStreamBYOBReaderReadOptions(
    [property: Description("@#min")]ulong Min = 1);

/// <summary>
/// ReadableStreamGetReaderOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamGetReaderOptions")]
public record ReadableStreamGetReaderOptions(
    [property: Description("@#mode")]ReadableStreamReaderMode? Mode = default);

/// <summary>
/// ReadableStreamIteratorOptions
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamIteratorOptions")]
public record ReadableStreamIteratorOptions(
    [property: Description("@#preventCancel")]bool PreventCancel = false);

/// <summary>
/// ReadableStreamReadResult
/// </summary>
[ECMAScript]
[Description("@#ReadableStreamReadResult")]
public record ReadableStreamReadResult(
    [property: Description("@#value")]object? Value = default,
    [property: Description("@#done")]bool Done = default);

/// <summary>
/// ReadableWritablePair
/// </summary>
[ECMAScript]
[Description("@#ReadableWritablePair")]
public record ReadableWritablePair(
    [property: Description("@#readable")]ReadableStream? Readable = default,
    [property: Description("@#writable")]WritableStream? Writable = default);

/// <summary>
/// RegistrationOptions
/// </summary>
[ECMAScript]
[Description("@#RegistrationOptions")]
public record RegistrationOptions(
    [property: Description("@#scope")]string? Scope = default,
    [property: Description("@#type")]WorkerType Type = WorkerType.Classic,
    [property: Description("@#updateViaCache")]ServiceWorkerUpdateViaCache UpdateViaCache = ServiceWorkerUpdateViaCache.Imports);

/// <summary>
/// RegistrationResponseJSON
/// </summary>
[ECMAScript]
[Description("@#RegistrationResponseJSON")]
public record RegistrationResponseJSON(
    [property: Description("@#id")]Base64URLString? Id = default,
    [property: Description("@#rawId")]Base64URLString? RawId = default,
    [property: Description("@#response")]AuthenticatorAttestationResponseJSON? Response = default,
    [property: Description("@#authenticatorAttachment")]string? AuthenticatorAttachment = default,
    [property: Description("@#clientExtensionResults")]AuthenticationExtensionsClientOutputsJSON? ClientExtensionResults = default,
    [property: Description("@#type")]string? Type = default);

/// <summary>
/// RelatedApplication
/// </summary>
[ECMAScript]
[Description("@#RelatedApplication")]
public record RelatedApplication(
    [property: Description("@#platform")]string? Platform = default,
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#id")]string? Id = default,
    [property: Description("@#version")]string? Version = default);

/// <summary>
/// ReportResultBrowserSignals
/// </summary>
[ECMAScript]
[Description("@#ReportResultBrowserSignals")]
public record ReportResultBrowserSignals(
    [property: Description("@#desirability")]double Desirability = default,
    [property: Description("@#topLevelSellerSignals")]string? TopLevelSellerSignals = default,
    [property: Description("@#modifiedBid")]double ModifiedBid = default,
    [property: Description("@#dataVersion")]uint DataVersion = default) : ReportingBrowserSignals;

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
    [property: Description("@#kAnonStatus")]KAnonStatus? KAnonStatus = default) : ReportingBrowserSignals;

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
    [property: Description("@#buyerAndSellerReportingId")]string? BuyerAndSellerReportingId = default);

/// <summary>
/// ReportingObserverOptions
/// </summary>
[ECMAScript]
[Description("@#ReportingObserverOptions")]
public record ReportingObserverOptions(
    [property: Description("@#types")]string[]? Types = default,
    [property: Description("@#buffered")]bool Buffered = false);

/// <summary>
/// RequestDeviceOptions
/// </summary>
[ECMAScript]
[Description("@#RequestDeviceOptions")]
public record RequestDeviceOptions(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
    [property: Description("@#exclusionFilters")]BluetoothLEScanFilterInit[]? ExclusionFilters = default,
    [property: Description("@#optionalServices")]BluetoothServiceUUID[]? OptionalServices = default,
    [property: Description("@#optionalManufacturerData")]ushort[]? OptionalManufacturerData = default,
    [property: Description("@#acceptAllDevices")]bool AcceptAllDevices = false);

/// <summary>
/// RequestInit
/// </summary>
[ECMAScript]
[Description("@#RequestInit")]
public record RequestInit(
    [property: Description("@#attributionReporting")]AttributionReportingRequestOptions? AttributionReporting = default,
    [property: Description("@#method")]byte[]? Method = default,
    [property: Description("@#headers")]HeadersInit? Headers = default,
    [property: Description("@#body")]BodyInit? Body = default,
    [property: Description("@#referrer")]string? Referrer = default,
    [property: Description("@#referrerPolicy")]ReferrerPolicy? ReferrerPolicy = default,
    [property: Description("@#mode")]RequestMode? Mode = default,
    [property: Description("@#credentials")]RequestCredentials? Credentials = default,
    [property: Description("@#cache")]RequestCache? Cache = default,
    [property: Description("@#redirect")]RequestRedirect? Redirect = default,
    [property: Description("@#integrity")]string? Integrity = default,
    [property: Description("@#keepalive")]bool Keepalive = default,
    [property: Description("@#signal")]AbortSignal? Signal = default,
    [property: Description("@#duplex")]RequestDuplex? Duplex = default,
    [property: Description("@#priority")]RequestPriority? Priority = default,
    [property: Description("@#window")]object? Window = default,
    [property: Description("@#targetAddressSpace")]IPAddressSpace? TargetAddressSpace = default,
    [property: Description("@#sharedStorageWritable")]bool SharedStorageWritable = default,
    [property: Description("@#privateToken")]PrivateToken? PrivateToken = default,
    [property: Description("@#adAuctionHeaders")]bool AdAuctionHeaders = default)
{
    [Category("optional")]
    public extern static RequestInit OptionalAttributionReporting(
        [Description("@#attributionReporting")]AttributionReportingRequestOptions? AttributionReporting = default);

    [Category("optional")]
    public extern static RequestInit OptionalMethodHeadersBody15(
        [Description("@#method")]byte[]? Method = default,
        [Description("@#headers")]HeadersInit? Headers = default,
        [Description("@#body")]BodyInit? Body = default,
        [Description("@#referrer")]string? Referrer = default,
        [Description("@#referrerPolicy")]ReferrerPolicy? ReferrerPolicy = default,
        [Description("@#mode")]RequestMode? Mode = default,
        [Description("@#credentials")]RequestCredentials? Credentials = default,
        [Description("@#cache")]RequestCache? Cache = default,
        [Description("@#redirect")]RequestRedirect? Redirect = default,
        [Description("@#integrity")]string? Integrity = default,
        [Description("@#keepalive")]bool Keepalive = default,
        [Description("@#signal")]AbortSignal? Signal = default,
        [Description("@#duplex")]RequestDuplex? Duplex = default,
        [Description("@#priority")]RequestPriority? Priority = default,
        [Description("@#window")]object? Window = default);

    [Category("optional")]
    public extern static RequestInit OptionalTargetAddressSpace(
        [Description("@#targetAddressSpace")]IPAddressSpace? TargetAddressSpace = default);

    [Category("optional")]
    public extern static RequestInit OptionalSharedStorageWritable(
        [Description("@#sharedStorageWritable")]bool SharedStorageWritable = default);

    [Category("optional")]
    public extern static RequestInit OptionalPrivateToken(
        [Description("@#privateToken")]PrivateToken? PrivateToken = default);

    [Category("optional")]
    public extern static RequestInit OptionalAdAuctionHeaders(
        [Description("@#adAuctionHeaders")]bool AdAuctionHeaders = default);
}

/// <summary>
/// ResizeObserverOptions
/// </summary>
[ECMAScript]
[Description("@#ResizeObserverOptions")]
public record ResizeObserverOptions(
    [property: Description("@#box")]ResizeObserverBoxOptions Box = ResizeObserverBoxOptions.ContentBox);

/// <summary>
/// ResponseInit
/// </summary>
[ECMAScript]
[Description("@#ResponseInit")]
public record ResponseInit(
    [property: Description("@#status")]ushort Status = 200,
    [property: Description("@#statusText")]byte[]? StatusText = default,
    [property: Description("@#headers")]HeadersInit? Headers = default);

/// <summary>
/// RouterCondition
/// </summary>
[ECMAScript]
[Description("@#RouterCondition")]
public record RouterCondition(
    [property: Description("@#urlPattern")]URLPatternCompatible? UrlPattern = default,
    [property: Description("@#requestMethod")]byte[]? RequestMethod = default,
    [property: Description("@#requestMode")]RequestMode? RequestMode = default,
    [property: Description("@#requestDestination")]RequestDestination? RequestDestination = default,
    [property: Description("@#runningStatus")]RunningStatus? RunningStatus = default,
    [property: Description("@#or")]RouterCondition[]? Or = default);

/// <summary>
/// RouterRule
/// </summary>
[ECMAScript]
[Description("@#RouterRule")]
public record RouterRule(
    [property: Description("@#condition")]RouterCondition? Condition = default,
    [property: Description("@#source")]RouterSource? Source = default);

/// <summary>
/// RouterSourceDict
/// </summary>
[ECMAScript]
[Description("@#RouterSourceDict")]
public record RouterSourceDict(
    [property: Description("@#cacheName")]string? CacheName = default);

/// <summary>
/// RsaHashedImportParams
/// </summary>
[ECMAScript]
[Description("@#RsaHashedImportParams")]
public record RsaHashedImportParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default) : Algorithm;

/// <summary>
/// RsaHashedKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#RsaHashedKeyAlgorithm")]
public record RsaHashedKeyAlgorithm(
    [property: Description("@#hash")]KeyAlgorithm? Hash = default) : RsaKeyAlgorithm;

/// <summary>
/// RsaHashedKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#RsaHashedKeyGenParams")]
public record RsaHashedKeyGenParams(
    [property: Description("@#hash")]HashAlgorithmIdentifier? Hash = default) : RsaKeyGenParams;

/// <summary>
/// RsaKeyAlgorithm
/// </summary>
[ECMAScript]
[Description("@#RsaKeyAlgorithm")]
public record RsaKeyAlgorithm(
    [property: Description("@#modulusLength")]uint ModulusLength = default,
    [property: Description("@#publicExponent")]BigInteger? PublicExponent = default) : KeyAlgorithm;

/// <summary>
/// RsaKeyGenParams
/// </summary>
[ECMAScript]
[Description("@#RsaKeyGenParams")]
public record RsaKeyGenParams(
    [property: Description("@#modulusLength")]uint ModulusLength = default,
    [property: Description("@#publicExponent")]BigInteger? PublicExponent = default) : Algorithm;

/// <summary>
/// RsaOaepParams
/// </summary>
[ECMAScript]
[Description("@#RsaOaepParams")]
public record RsaOaepParams(
    [property: Description("@#label")]IBufferSource? Label = default) : Algorithm;

/// <summary>
/// RsaOtherPrimesInfo
/// </summary>
[ECMAScript]
[Description("@#RsaOtherPrimesInfo")]
public record RsaOtherPrimesInfo(
    [property: Description("@#r")]string? R = default,
    [property: Description("@#d")]string? D = default,
    [property: Description("@#t")]string? T = default);

/// <summary>
/// RsaPssParams
/// </summary>
[ECMAScript]
[Description("@#RsaPssParams")]
public record RsaPssParams(
    [property: Description("@#saltLength")]uint SaltLength = default) : Algorithm;

/// <summary>
/// SFrameTransformErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SFrameTransformErrorEventInit")]
public record SFrameTransformErrorEventInit(
    [property: Description("@#errorType")]SFrameTransformErrorEventType? ErrorType = default,
    [property: Description("@#frame")]object? Frame = default,
    [property: Description("@#keyID")]CryptoKeyID? KeyID = default) : EventInit;

/// <summary>
/// SFrameTransformOptions
/// </summary>
[ECMAScript]
[Description("@#SFrameTransformOptions")]
public record SFrameTransformOptions(
    [property: Description("@#role")]SFrameTransformRole Role = SFrameTransformRole.Encrypt);

/// <summary>
/// SVGBoundingBoxOptions
/// </summary>
[ECMAScript]
[Description("@#SVGBoundingBoxOptions")]
public record SVGBoundingBoxOptions(
    [property: Description("@#fill")]bool Fill = false,
    [property: Description("@#stroke")]bool Stroke = false,
    [property: Description("@#markers")]bool Markers = false,
    [property: Description("@#clipped")]bool Clipped = false);

/// <summary>
/// SanitizerAttributeNamespace
/// </summary>
[ECMAScript]
[Description("@#SanitizerAttributeNamespace")]
public record SanitizerAttributeNamespace(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#namespace")]string? Namespace = null);

/// <summary>
/// SanitizerConfig
/// </summary>
[ECMAScript]
[Description("@#SanitizerConfig")]
public record SanitizerConfig(
    [property: Description("@#elements")]SanitizerElementWithAttributes[]? Elements = default,
    [property: Description("@#removeElements")]SanitizerElement[]? RemoveElements = default,
    [property: Description("@#replaceWithChildrenElements")]SanitizerElement[]? ReplaceWithChildrenElements = default,
    [property: Description("@#attributes")]SanitizerAttribute[]? Attributes = default,
    [property: Description("@#removeAttributes")]SanitizerAttribute[]? RemoveAttributes = default,
    [property: Description("@#comments")]bool Comments = default,
    [property: Description("@#dataAttributes")]bool DataAttributes = default);

/// <summary>
/// SanitizerElementNamespace
/// </summary>
[ECMAScript]
[Description("@#SanitizerElementNamespace")]
public record SanitizerElementNamespace(
    [property: Description("@#name")]string? Name = default,
    [property: Description("@#namespace")]string? Namespace = "http://www.w3.org/1999/xhtml");

/// <summary>
/// SanitizerElementNamespaceWithAttributes
/// </summary>
[ECMAScript]
[Description("@#SanitizerElementNamespaceWithAttributes")]
public record SanitizerElementNamespaceWithAttributes(
    [property: Description("@#attributes")]SanitizerAttribute[]? Attributes = default,
    [property: Description("@#removeAttributes")]SanitizerAttribute[]? RemoveAttributes = default) : SanitizerElementNamespace;

/// <summary>
/// SaveFilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#SaveFilePickerOptions")]
public record SaveFilePickerOptions(
    [property: Description("@#suggestedName")]string? SuggestedName = default) : FilePickerOptions;

/// <summary>
/// SchedulerPostTaskOptions
/// </summary>
[ECMAScript]
[Description("@#SchedulerPostTaskOptions")]
public record SchedulerPostTaskOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
    [property: Description("@#priority")]TaskPriority? Priority = default,
    [property: Description("@#delay")]ulong Delay = 0);

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
    [property: Description("@#dataVersion")]uint DataVersion = default,
    [property: Description("@#adComponents")]string[]? AdComponents = default,
    [property: Description("@#forDebuggingOnlyInCooldownOrLockout")]bool ForDebuggingOnlyInCooldownOrLockout = false);

/// <summary>
/// ScrollIntoViewOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollIntoViewOptions")]
public record ScrollIntoViewOptions(
    [property: Description("@#block")]ScrollLogicalPosition Block = ScrollLogicalPosition.Start,
    [property: Description("@#inline")]ScrollLogicalPosition Inline = ScrollLogicalPosition.Nearest) : ScrollOptions;

/// <summary>
/// ScrollOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollOptions")]
public record ScrollOptions(
    [property: Description("@#behavior")]ScrollBehavior Behavior = ScrollBehavior.Auto);

/// <summary>
/// ScrollTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollTimelineOptions")]
public record ScrollTimelineOptions(
    [property: Description("@#source")]Element? Source = default,
    [property: Description("@#axis")]ScrollAxis Axis = ScrollAxis.Block);

/// <summary>
/// ScrollToOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollToOptions")]
public record ScrollToOptions(
    [property: Description("@#left")]double Left = default,
    [property: Description("@#top")]double Top = default) : ScrollOptions;

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
    [property: Description("@#extensions")]AuthenticationExtensionsClientInputs? Extensions = default,
    [property: Description("@#locale")]string[]? Locale = default,
    [property: Description("@#showOptOut")]bool ShowOptOut = default);

/// <summary>
/// SecurityPolicyViolationEventInit
/// </summary>
[ECMAScript]
[Description("@#SecurityPolicyViolationEventInit")]
public record SecurityPolicyViolationEventInit(
    [property: Description("@#documentURI")]string? DocumentURI = default,
    [property: Description("@#referrer")]string? Referrer = default,
    [property: Description("@#blockedURI")]string? BlockedURI = default,
    [property: Description("@#violatedDirective")]string? ViolatedDirective = default,
    [property: Description("@#effectiveDirective")]string? EffectiveDirective = default,
    [property: Description("@#originalPolicy")]string? OriginalPolicy = default,
    [property: Description("@#sourceFile")]string? SourceFile = default,
    [property: Description("@#sample")]string? Sample = default,
    [property: Description("@#disposition")]SecurityPolicyViolationEventDisposition Disposition = SecurityPolicyViolationEventDisposition.Enforce,
    [property: Description("@#statusCode")]ushort StatusCode = 0,
    [property: Description("@#lineNumber")]uint LineNumber = 0,
    [property: Description("@#columnNumber")]uint ColumnNumber = 0) : EventInit;

/// <summary>
/// SensorErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SensorErrorEventInit")]
public record SensorErrorEventInit(
    [property: Description("@#error")]DOMException? Error = default) : EventInit;

/// <summary>
/// SensorOptions
/// </summary>
[ECMAScript]
[Description("@#SensorOptions")]
public record SensorOptions(
    [property: Description("@#frequency")]double Frequency = default);

/// <summary>
/// SerialInputSignals
/// </summary>
[ECMAScript]
[Description("@#SerialInputSignals")]
public record SerialInputSignals(
    [property: Description("@#dataCarrierDetect")]bool DataCarrierDetect = default,
    [property: Description("@#clearToSend")]bool ClearToSend = default,
    [property: Description("@#ringIndicator")]bool RingIndicator = default,
    [property: Description("@#dataSetReady")]bool DataSetReady = default);

/// <summary>
/// SerialOptions
/// </summary>
[ECMAScript]
[Description("@#SerialOptions")]
public record SerialOptions(
    [property: Description("@#baudRate")]uint BaudRate = default,
    [property: Description("@#dataBits")]byte DataBits = 8,
    [property: Description("@#stopBits")]byte StopBits = 1,
    [property: Description("@#parity")]ParityType Parity = ParityType.None,
    [property: Description("@#bufferSize")]uint BufferSize = 255,
    [property: Description("@#flowControl")]FlowControlType FlowControl = FlowControlType.None);

/// <summary>
/// SerialOutputSignals
/// </summary>
[ECMAScript]
[Description("@#SerialOutputSignals")]
public record SerialOutputSignals(
    [property: Description("@#dataTerminalReady")]bool DataTerminalReady = default,
    [property: Description("@#requestToSend")]bool RequestToSend = default,
    [property: Description("@#break")]bool Break = default);

/// <summary>
/// SerialPortFilter
/// </summary>
[ECMAScript]
[Description("@#SerialPortFilter")]
public record SerialPortFilter(
    [property: Description("@#usbVendorId")]ushort UsbVendorId = default,
    [property: Description("@#usbProductId")]ushort UsbProductId = default,
    [property: Description("@#bluetoothServiceClassId")]BluetoothServiceUUID? BluetoothServiceClassId = default);

/// <summary>
/// SerialPortInfo
/// </summary>
[ECMAScript]
[Description("@#SerialPortInfo")]
public record SerialPortInfo(
    [property: Description("@#usbVendorId")]ushort UsbVendorId = default,
    [property: Description("@#usbProductId")]ushort UsbProductId = default,
    [property: Description("@#bluetoothServiceClassId")]BluetoothServiceUUID? BluetoothServiceClassId = default);

/// <summary>
/// SerialPortRequestOptions
/// </summary>
[ECMAScript]
[Description("@#SerialPortRequestOptions")]
public record SerialPortRequestOptions(
    [property: Description("@#filters")]SerialPortFilter[]? Filters = default,
    [property: Description("@#allowedBluetoothServiceClassIds")]BluetoothServiceUUID[]? AllowedBluetoothServiceClassIds = default);

/// <summary>
/// ShadowRootInit
/// </summary>
[ECMAScript]
[Description("@#ShadowRootInit")]
public record ShadowRootInit(
    [property: Description("@#mode")]ShadowRootMode? Mode = default,
    [property: Description("@#delegatesFocus")]bool DelegatesFocus = false,
    [property: Description("@#slotAssignment")]SlotAssignmentMode SlotAssignment = SlotAssignmentMode.Named,
    [property: Description("@#clonable")]bool Clonable = false,
    [property: Description("@#serializable")]bool Serializable = false);

/// <summary>
/// ShareData
/// </summary>
[ECMAScript]
[Description("@#ShareData")]
public record ShareData(
    [property: Description("@#files")]File[]? Files = default,
    [property: Description("@#title")]string? Title = default,
    [property: Description("@#text")]string? Text = default,
    [property: Description("@#url")]string? Url = default);

/// <summary>
/// SharedStorageRunOperationMethodOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageRunOperationMethodOptions")]
public record SharedStorageRunOperationMethodOptions(
    [property: Description("@#data")]object? Data = default,
    [property: Description("@#resolveToConfig")]bool ResolveToConfig = false,
    [property: Description("@#keepAlive")]bool KeepAlive = false);

/// <summary>
/// SharedStorageSetMethodOptions
/// </summary>
[ECMAScript]
[Description("@#SharedStorageSetMethodOptions")]
public record SharedStorageSetMethodOptions(
    [property: Description("@#ignoreIfPresent")]bool IgnoreIfPresent = false);

/// <summary>
/// SharedStorageUrlWithMetadata
/// </summary>
[ECMAScript]
[Description("@#SharedStorageUrlWithMetadata")]
public record SharedStorageUrlWithMetadata(
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#reportingMetadata")]object? ReportingMetadata = default);

/// <summary>
/// SnapEventInit
/// </summary>
[ECMAScript]
[Description("@#SnapEventInit")]
public record SnapEventInit(
    [property: Description("@#snapTargetBlock")]Node? SnapTargetBlock = default,
    [property: Description("@#snapTargetInline")]Node? SnapTargetInline = default) : EventInit;

/// <summary>
/// SpatialNavigationSearchOptions
/// </summary>
[ECMAScript]
[Description("@#SpatialNavigationSearchOptions")]
public record SpatialNavigationSearchOptions(
    [property: Description("@#candidates")]Node[]? Candidates = default,
    [property: Description("@#container")]Node? Container = default);

/// <summary>
/// SpeechRecognitionErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionErrorEventInit")]
public record SpeechRecognitionErrorEventInit(
    [property: Description("@#error")]SpeechRecognitionErrorCode? Error = default,
    [property: Description("@#message")]string? Message = default) : EventInit;

/// <summary>
/// SpeechRecognitionEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechRecognitionEventInit")]
public record SpeechRecognitionEventInit(
    [property: Description("@#resultIndex")]uint ResultIndex = 0,
    [property: Description("@#results")]SpeechRecognitionResultList? Results = default) : EventInit;

/// <summary>
/// SpeechSynthesisErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisErrorEventInit")]
public record SpeechSynthesisErrorEventInit(
    [property: Description("@#error")]SpeechSynthesisErrorCode? Error = default) : SpeechSynthesisEventInit;

/// <summary>
/// SpeechSynthesisEventInit
/// </summary>
[ECMAScript]
[Description("@#SpeechSynthesisEventInit")]
public record SpeechSynthesisEventInit(
    [property: Description("@#utterance")]SpeechSynthesisUtterance? Utterance = default,
    [property: Description("@#charIndex")]uint CharIndex = 0,
    [property: Description("@#charLength")]uint CharLength = 0,
    [property: Description("@#elapsedTime")]float ElapsedTime = 0f,
    [property: Description("@#name")]string? Name = default) : EventInit;

/// <summary>
/// StartViewTransitionOptions
/// </summary>
[ECMAScript]
[Description("@#StartViewTransitionOptions")]
public record StartViewTransitionOptions(
    [property: Description("@#update")]UpdateCallback? Update = null,
    [property: Description("@#types")]string[]? Types = null);

/// <summary>
/// StaticRangeInit
/// </summary>
[ECMAScript]
[Description("@#StaticRangeInit")]
public record StaticRangeInit(
    [property: Description("@#startContainer")]Node? StartContainer = default,
    [property: Description("@#startOffset")]uint StartOffset = default,
    [property: Description("@#endContainer")]Node? EndContainer = default,
    [property: Description("@#endOffset")]uint EndOffset = default);

/// <summary>
/// StereoPannerOptions
/// </summary>
[ECMAScript]
[Description("@#StereoPannerOptions")]
public record StereoPannerOptions(
    [property: Description("@#pan")]float Pan = 0f) : AudioNodeOptions;

/// <summary>
/// StorageBucketOptions
/// </summary>
[ECMAScript]
[Description("@#StorageBucketOptions")]
public record StorageBucketOptions(
    [property: Description("@#persisted")]bool Persisted = false,
    [property: Description("@#quota")]ulong Quota = default,
    [property: Description("@#expires")]double Expires = default);

/// <summary>
/// StorageEstimate
/// </summary>
[ECMAScript]
[Description("@#StorageEstimate")]
public record StorageEstimate(
    [property: Description("@#usage")]ulong Usage = default,
    [property: Description("@#quota")]ulong Quota = default);

/// <summary>
/// StorageEventInit
/// </summary>
[ECMAScript]
[Description("@#StorageEventInit")]
public record StorageEventInit(
    [property: Description("@#key")]string? Key = null,
    [property: Description("@#oldValue")]string? OldValue = null,
    [property: Description("@#newValue")]string? NewValue = null,
    [property: Description("@#url")]string? Url = default,
    [property: Description("@#storageArea")]Storage? StorageArea = null) : EventInit;

/// <summary>
/// StreamPipeOptions
/// </summary>
[ECMAScript]
[Description("@#StreamPipeOptions")]
public record StreamPipeOptions(
    [property: Description("@#preventClose")]bool PreventClose = false,
    [property: Description("@#preventAbort")]bool PreventAbort = false,
    [property: Description("@#preventCancel")]bool PreventCancel = false,
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// StructuredSerializeOptions
/// </summary>
[ECMAScript]
[Description("@#StructuredSerializeOptions")]
public record StructuredSerializeOptions(
    [property: Description("@#transfer")]object[]? Transfer = default);

/// <summary>
/// SubmitEventInit
/// </summary>
[ECMAScript]
[Description("@#SubmitEventInit")]
public record SubmitEventInit(
    [property: Description("@#submitter")]HTMLElement? Submitter = null) : EventInit;

/// <summary>
/// SvcOutputMetadata
/// </summary>
[ECMAScript]
[Description("@#SvcOutputMetadata")]
public record SvcOutputMetadata(
    [property: Description("@#temporalLayerId")]uint TemporalLayerId = default);

/// <summary>
/// SyncEventInit
/// </summary>
[ECMAScript]
[Description("@#SyncEventInit")]
public record SyncEventInit(
    [property: Description("@#tag")]string? Tag = default,
    [property: Description("@#lastChance")]bool LastChance = false) : ExtendableEventInit;

/// <summary>
/// TaskControllerInit
/// </summary>
[ECMAScript]
[Description("@#TaskControllerInit")]
public record TaskControllerInit(
    [property: Description("@#priority")]TaskPriority Priority = TaskPriority.UserVisible);

/// <summary>
/// TaskPriorityChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#TaskPriorityChangeEventInit")]
public record TaskPriorityChangeEventInit(
    [property: Description("@#previousPriority")]TaskPriority? PreviousPriority = default) : EventInit;

/// <summary>
/// TaskSignalAnyInit
/// </summary>
[ECMAScript]
[Description("@#TaskSignalAnyInit")]
public record TaskSignalAnyInit(
    [property: Description("@#priority")]TaskSignalAnyInitPriority? Priority = default);

/// <summary>
/// TextDecodeOptions
/// </summary>
[ECMAScript]
[Description("@#TextDecodeOptions")]
public record TextDecodeOptions(
    [property: Description("@#stream")]bool Stream = false);

/// <summary>
/// TextDecoderOptions
/// </summary>
[ECMAScript]
[Description("@#TextDecoderOptions")]
public record TextDecoderOptions(
    [property: Description("@#fatal")]bool Fatal = false,
    [property: Description("@#ignoreBOM")]bool IgnoreBOM = false);

/// <summary>
/// TextEncoderEncodeIntoResult
/// </summary>
[ECMAScript]
[Description("@#TextEncoderEncodeIntoResult")]
public record TextEncoderEncodeIntoResult(
    [property: Description("@#read")]ulong Read = default,
    [property: Description("@#written")]ulong Written = default);

/// <summary>
/// TextFormatInit
/// </summary>
[ECMAScript]
[Description("@#TextFormatInit")]
public record TextFormatInit(
    [property: Description("@#rangeStart")]uint RangeStart = default,
    [property: Description("@#rangeEnd")]uint RangeEnd = default,
    [property: Description("@#underlineStyle")]UnderlineStyle? UnderlineStyle = default,
    [property: Description("@#underlineThickness")]UnderlineThickness? UnderlineThickness = default);

/// <summary>
/// TextFormatUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#TextFormatUpdateEventInit")]
public record TextFormatUpdateEventInit(
    [property: Description("@#textFormats")]TextFormat[]? TextFormats = default) : EventInit;

/// <summary>
/// TextUpdateEventInit
/// </summary>
[ECMAScript]
[Description("@#TextUpdateEventInit")]
public record TextUpdateEventInit(
    [property: Description("@#updateRangeStart")]uint UpdateRangeStart = default,
    [property: Description("@#updateRangeEnd")]uint UpdateRangeEnd = default,
    [property: Description("@#text")]string? Text = default,
    [property: Description("@#selectionStart")]uint SelectionStart = default,
    [property: Description("@#selectionEnd")]uint SelectionEnd = default,
    [property: Description("@#compositionStart")]uint CompositionStart = default,
    [property: Description("@#compositionEnd")]uint CompositionEnd = default) : EventInit;

/// <summary>
/// TimelineRangeOffset
/// </summary>
[ECMAScript]
[Description("@#TimelineRangeOffset")]
public record TimelineRangeOffset(
    [property: Description("@#rangeName")]string? RangeName = default,
    [property: Description("@#offset")]CSSNumericValue? Offset = default);

/// <summary>
/// ToggleEventInit
/// </summary>
[ECMAScript]
[Description("@#ToggleEventInit")]
public record ToggleEventInit(
    [property: Description("@#oldState")]string? OldState = default,
    [property: Description("@#newState")]string? NewState = default) : EventInit;

/// <summary>
/// TokenBinding
/// </summary>
[ECMAScript]
[Description("@#TokenBinding")]
public record TokenBinding(
    [property: Description("@#status")]string? Status = default,
    [property: Description("@#id")]string? Id = default);

/// <summary>
/// TopLevelStorageAccessPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#TopLevelStorageAccessPermissionDescriptor")]
public record TopLevelStorageAccessPermissionDescriptor(
    [property: Description("@#requestedOrigin")]string? RequestedOrigin = default) : PermissionDescriptor;

/// <summary>
/// TouchEventInit
/// </summary>
[ECMAScript]
[Description("@#TouchEventInit")]
public record TouchEventInit(
    [property: Description("@#touches")]Touch[]? Touches = default,
    [property: Description("@#targetTouches")]Touch[]? TargetTouches = default,
    [property: Description("@#changedTouches")]Touch[]? ChangedTouches = default) : EventModifierInit;

/// <summary>
/// TouchInit
/// </summary>
[ECMAScript]
[Description("@#TouchInit")]
public record TouchInit(
    [property: Description("@#identifier")]int Identifier = default,
    [property: Description("@#target")]EventTarget? Target = default,
    [property: Description("@#clientX")]double ClientX = 0d,
    [property: Description("@#clientY")]double ClientY = 0d,
    [property: Description("@#screenX")]double ScreenX = 0d,
    [property: Description("@#screenY")]double ScreenY = 0d,
    [property: Description("@#pageX")]double PageX = 0d,
    [property: Description("@#pageY")]double PageY = 0d,
    [property: Description("@#radiusX")]float RadiusX = 0f,
    [property: Description("@#radiusY")]float RadiusY = 0f,
    [property: Description("@#rotationAngle")]float RotationAngle = 0f,
    [property: Description("@#force")]float Force = 0f,
    [property: Description("@#altitudeAngle")]double AltitudeAngle = 0d,
    [property: Description("@#azimuthAngle")]double AzimuthAngle = 0d,
    [property: Description("@#touchType")]TouchType TouchType = TouchType.Direct);

/// <summary>
/// TrackEventInit
/// </summary>
[ECMAScript]
[Description("@#TrackEventInit")]
public record TrackEventInit(
    [property: Description("@#track")]TrackEventInitTrack? Track = default) : EventInit;

/// <summary>
/// Transformer
/// </summary>
[ECMAScript]
[Description("@#Transformer")]
public record Transformer(
    [property: Description("@#start")]TransformerStartCallback? Start = default,
    [property: Description("@#transform")]TransformerTransformCallback? Transform = default,
    [property: Description("@#flush")]TransformerFlushCallback? Flush = default,
    [property: Description("@#cancel")]TransformerCancelCallback? Cancel = default,
    [property: Description("@#readableType")]object? ReadableType = default,
    [property: Description("@#writableType")]object? WritableType = default);

/// <summary>
/// TransitionEventInit
/// </summary>
[ECMAScript]
[Description("@#TransitionEventInit")]
public record TransitionEventInit(
    [property: Description("@#propertyName")]string? PropertyName = default,
    [property: Description("@#elapsedTime")]double ElapsedTime = 0.0d,
    [property: Description("@#pseudoElement")]string? PseudoElement = default) : EventInit;

/// <summary>
/// TrustedTypePolicyOptions
/// </summary>
[ECMAScript]
[Description("@#TrustedTypePolicyOptions")]
public record TrustedTypePolicyOptions(
    [property: Description("@#createHTML")]CreateHTMLCallback? CreateHTML = default,
    [property: Description("@#createScript")]CreateScriptCallback? CreateScript = default,
    [property: Description("@#createScriptURL")]CreateScriptURLCallback? CreateScriptURL = default);

/// <summary>
/// UADataValues
/// </summary>
[ECMAScript]
[Description("@#UADataValues")]
public record UADataValues(
    [property: Description("@#architecture")]string? Architecture = default,
    [property: Description("@#bitness")]string? Bitness = default,
    [property: Description("@#brands")]NavigatorUABrandVersion[]? Brands = default,
    [property: Description("@#formFactors")]string[]? FormFactors = default,
    [property: Description("@#fullVersionList")]NavigatorUABrandVersion[]? FullVersionList = default,
    [property: Description("@#model")]string? Model = default,
    [property: Description("@#mobile")]bool Mobile = default,
    [property: Description("@#platform")]string? Platform = default,
    [property: Description("@#platformVersion")]string? PlatformVersion = default,
    [property: Description("@#uaFullVersion")]string? UaFullVersion = default,
    [property: Description("@#wow64")]bool Wow64 = default);

/// <summary>
/// UALowEntropyJSON
/// </summary>
[ECMAScript]
[Description("@#UALowEntropyJSON")]
public record UALowEntropyJSON(
    [property: Description("@#brands")]NavigatorUABrandVersion[]? Brands = default,
    [property: Description("@#mobile")]bool Mobile = default,
    [property: Description("@#platform")]string? Platform = default);

/// <summary>
/// UIEventInit
/// </summary>
[ECMAScript]
[Description("@#UIEventInit")]
public record UIEventInit(
    [property: Description("@#sourceCapabilities")]InputDeviceCapabilities? SourceCapabilities = null,
    [property: Description("@#view")]Window? View = null,
    [property: Description("@#detail")]int Detail = 0,
    [property: Description("@#which")]uint Which = 0) : EventInit
{
    [Category("optional")]
    public extern static UIEventInit OptionalSourceCapabilities(
        [Description("@#sourceCapabilities")]InputDeviceCapabilities? sourceCapabilities = null);

    [Category("optional")]
    public extern static UIEventInit OptionalViewDetail(
        [Description("@#view")]Window? view = null,
        [Description("@#detail")]int detail = 0);

    [Category("optional")]
    public extern static UIEventInit OptionalWhich(
        [Description("@#which")]uint which = 0);
}

/// <summary>
/// ULongRange
/// </summary>
[ECMAScript]
[Description("@#ULongRange")]
public record ULongRange(
    [property: Description("@#max")]uint Max = default,
    [property: Description("@#min")]uint Min = default);

/// <summary>
/// URLPatternComponentResult
/// </summary>
[ECMAScript]
[Description("@#URLPatternComponentResult")]
public record URLPatternComponentResult(
    [property: Description("@#input")]string? Input = default,
    [property: Description("@#groups")]Dictionary<string, string?>? Groups = default);

/// <summary>
/// URLPatternInit
/// </summary>
[ECMAScript]
[Description("@#URLPatternInit")]
public record URLPatternInit(
    [property: Description("@#protocol")]string? Protocol = default,
    [property: Description("@#username")]string? Username = default,
    [property: Description("@#password")]string? Password = default,
    [property: Description("@#hostname")]string? Hostname = default,
    [property: Description("@#port")]string? Port = default,
    [property: Description("@#pathname")]string? Pathname = default,
    [property: Description("@#search")]string? Search = default,
    [property: Description("@#hash")]string? Hash = default,
    [property: Description("@#baseURL")]string? BaseURL = default);

/// <summary>
/// URLPatternOptions
/// </summary>
[ECMAScript]
[Description("@#URLPatternOptions")]
public record URLPatternOptions(
    [property: Description("@#ignoreCase")]bool IgnoreCase = false);

/// <summary>
/// URLPatternResult
/// </summary>
[ECMAScript]
[Description("@#URLPatternResult")]
public record URLPatternResult(
    [property: Description("@#inputs")]URLPatternInput[]? Inputs = default,
    [property: Description("@#protocol")]URLPatternComponentResult? Protocol = default,
    [property: Description("@#username")]URLPatternComponentResult? Username = default,
    [property: Description("@#password")]URLPatternComponentResult? Password = default,
    [property: Description("@#hostname")]URLPatternComponentResult? Hostname = default,
    [property: Description("@#port")]URLPatternComponentResult? Port = default,
    [property: Description("@#pathname")]URLPatternComponentResult? Pathname = default,
    [property: Description("@#search")]URLPatternComponentResult? Search = default,
    [property: Description("@#hash")]URLPatternComponentResult? Hash = default);

/// <summary>
/// USBBlocklistEntry
/// </summary>
[ECMAScript]
[Description("@#USBBlocklistEntry")]
public record USBBlocklistEntry(
    [property: Description("@#idVendor")]ushort IdVendor = default,
    [property: Description("@#idProduct")]ushort IdProduct = default,
    [property: Description("@#bcdDevice")]ushort BcdDevice = default);

/// <summary>
/// USBConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#USBConnectionEventInit")]
public record USBConnectionEventInit(
    [property: Description("@#device")]USBDevice? Device = default) : EventInit;

/// <summary>
/// USBControlTransferParameters
/// </summary>
[ECMAScript]
[Description("@#USBControlTransferParameters")]
public record USBControlTransferParameters(
    [property: Description("@#requestType")]USBRequestType? RequestType = default,
    [property: Description("@#recipient")]USBRecipient? Recipient = default,
    [property: Description("@#request")]byte Request = default,
    [property: Description("@#value")]ushort Value = default,
    [property: Description("@#index")]ushort Index = default);

/// <summary>
/// USBDeviceFilter
/// </summary>
[ECMAScript]
[Description("@#USBDeviceFilter")]
public record USBDeviceFilter(
    [property: Description("@#vendorId")]ushort VendorId = default,
    [property: Description("@#productId")]ushort ProductId = default,
    [property: Description("@#classCode")]byte ClassCode = default,
    [property: Description("@#subclassCode")]byte SubclassCode = default,
    [property: Description("@#protocolCode")]byte ProtocolCode = default,
    [property: Description("@#serialNumber")]string? SerialNumber = default);

/// <summary>
/// USBDeviceRequestOptions
/// </summary>
[ECMAScript]
[Description("@#USBDeviceRequestOptions")]
public record USBDeviceRequestOptions(
    [property: Description("@#filters")]USBDeviceFilter[]? Filters = default,
    [property: Description("@#exclusionFilters")]USBDeviceFilter[]? ExclusionFilters = default);

/// <summary>
/// USBPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#USBPermissionDescriptor")]
public record USBPermissionDescriptor(
    [property: Description("@#filters")]USBDeviceFilter[]? Filters = default,
    [property: Description("@#exclusionFilters")]USBDeviceFilter[]? ExclusionFilters = default) : PermissionDescriptor;

/// <summary>
/// USBPermissionStorage
/// </summary>
[ECMAScript]
[Description("@#USBPermissionStorage")]
public record USBPermissionStorage(
    [property: Description("@#allowedDevices")]AllowedUSBDevice[]? AllowedDevices = default);

/// <summary>
/// UnderlyingSink
/// </summary>
[ECMAScript]
[Description("@#UnderlyingSink")]
public record UnderlyingSink(
    [property: Description("@#start")]UnderlyingSinkStartCallback? Start = default,
    [property: Description("@#write")]UnderlyingSinkWriteCallback? Write = default,
    [property: Description("@#close")]UnderlyingSinkCloseCallback? Close = default,
    [property: Description("@#abort")]UnderlyingSinkAbortCallback? Abort = default,
    [property: Description("@#type")]object? Type = default);

/// <summary>
/// UnderlyingSource
/// </summary>
[ECMAScript]
[Description("@#UnderlyingSource")]
public record UnderlyingSource(
    [property: Description("@#start")]UnderlyingSourceStartCallback? Start = default,
    [property: Description("@#pull")]UnderlyingSourcePullCallback? Pull = default,
    [property: Description("@#cancel")]UnderlyingSourceCancelCallback? Cancel = default,
    [property: Description("@#type")]ReadableStreamType? Type = default,
    [property: Description("@#autoAllocateChunkSize")]ulong AutoAllocateChunkSize = default);

/// <summary>
/// ValidityStateFlags
/// </summary>
[ECMAScript]
[Description("@#ValidityStateFlags")]
public record ValidityStateFlags(
    [property: Description("@#valueMissing")]bool ValueMissing = false,
    [property: Description("@#typeMismatch")]bool TypeMismatch = false,
    [property: Description("@#patternMismatch")]bool PatternMismatch = false,
    [property: Description("@#tooLong")]bool TooLong = false,
    [property: Description("@#tooShort")]bool TooShort = false,
    [property: Description("@#rangeUnderflow")]bool RangeUnderflow = false,
    [property: Description("@#rangeOverflow")]bool RangeOverflow = false,
    [property: Description("@#stepMismatch")]bool StepMismatch = false,
    [property: Description("@#badInput")]bool BadInput = false,
    [property: Description("@#customError")]bool CustomError = false);

/// <summary>
/// ValueEventInit
/// </summary>
[ECMAScript]
[Description("@#ValueEventInit")]
public record ValueEventInit(
    [property: Description("@#value")]object? Value = default) : EventInit;

/// <summary>
/// VideoColorSpaceInit
/// </summary>
[ECMAScript]
[Description("@#VideoColorSpaceInit")]
public record VideoColorSpaceInit(
    [property: Description("@#primaries")]VideoColorPrimaries? Primaries = null,
    [property: Description("@#transfer")]VideoTransferCharacteristics? Transfer = null,
    [property: Description("@#matrix")]VideoMatrixCoefficients? Matrix = null,
    [property: Description("@#fullRange")]bool? FullRange = null);

/// <summary>
/// VideoConfiguration
/// </summary>
[ECMAScript]
[Description("@#VideoConfiguration")]
public record VideoConfiguration(
    [property: Description("@#contentType")]string? ContentType = default,
    [property: Description("@#width")]uint Width = default,
    [property: Description("@#height")]uint Height = default,
    [property: Description("@#bitrate")]ulong Bitrate = default,
    [property: Description("@#framerate")]double Framerate = default,
    [property: Description("@#hasAlphaChannel")]bool HasAlphaChannel = default,
    [property: Description("@#hdrMetadataType")]HdrMetadataType? HdrMetadataType = default,
    [property: Description("@#colorGamut")]ColorGamut? ColorGamut = default,
    [property: Description("@#transferFunction")]TransferFunction? TransferFunction = default,
    [property: Description("@#scalabilityMode")]string? ScalabilityMode = default,
    [property: Description("@#spatialScalability")]bool SpatialScalability = default);

/// <summary>
/// VideoDecoderConfig
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderConfig")]
public record VideoDecoderConfig(
    [property: Description("@#codec")]string? Codec = default,
    [property: Description("@#description")]IAllowSharedBufferSource? Description = default,
    [property: Description("@#codedWidth")]uint CodedWidth = default,
    [property: Description("@#codedHeight")]uint CodedHeight = default,
    [property: Description("@#displayAspectWidth")]uint DisplayAspectWidth = default,
    [property: Description("@#displayAspectHeight")]uint DisplayAspectHeight = default,
    [property: Description("@#colorSpace")]VideoColorSpaceInit? ColorSpace = default,
    [property: Description("@#hardwareAcceleration")]HardwareAcceleration HardwareAcceleration = HardwareAcceleration.NoPreference,
    [property: Description("@#optimizeForLatency")]bool OptimizeForLatency = default);

/// <summary>
/// VideoDecoderInit
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderInit")]
public record VideoDecoderInit(
    [property: Description("@#output")]VideoFrameOutputCallback? Output = default,
    [property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// VideoDecoderSupport
/// </summary>
[ECMAScript]
[Description("@#VideoDecoderSupport")]
public record VideoDecoderSupport(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#config")]VideoDecoderConfig? Config = default);

/// <summary>
/// VideoEncoderConfig
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderConfig")]
public record VideoEncoderConfig(
    [property: Description("@#codec")]string? Codec = default,
    [property: Description("@#width")]uint Width = default,
    [property: Description("@#height")]uint Height = default,
    [property: Description("@#displayWidth")]uint DisplayWidth = default,
    [property: Description("@#displayHeight")]uint DisplayHeight = default,
    [property: Description("@#bitrate")]ulong Bitrate = default,
    [property: Description("@#framerate")]double Framerate = default,
    [property: Description("@#hardwareAcceleration")]HardwareAcceleration HardwareAcceleration = HardwareAcceleration.NoPreference,
    [property: Description("@#alpha")]AlphaOption Alpha = AlphaOption.Discard,
    [property: Description("@#scalabilityMode")]string? ScalabilityMode = default,
    [property: Description("@#bitrateMode")]VideoEncoderBitrateMode BitrateMode = VideoEncoderBitrateMode.Variable,
    [property: Description("@#latencyMode")]LatencyMode LatencyMode = LatencyMode.Quality,
    [property: Description("@#contentHint")]string? ContentHint = default,
    [property: Description("@#av1")]AV1EncoderConfig? Av1 = default,
    [property: Description("@#avc")]AvcEncoderConfig? Avc = default,
    [property: Description("@#hevc")]HevcEncoderConfig? Hevc = default)
{
    [Category("optional")]
    public extern static VideoEncoderConfig OptionalCodecWidthHeight13(
        [Description("@#codec")]string? Codec = default,
        [Description("@#width")]uint Width = default,
        [Description("@#height")]uint Height = default,
        [Description("@#displayWidth")]uint DisplayWidth = default,
        [Description("@#displayHeight")]uint DisplayHeight = default,
        [Description("@#bitrate")]ulong Bitrate = default,
        [Description("@#framerate")]double Framerate = default,
        [Description("@#hardwareAcceleration")]HardwareAcceleration hardwareAcceleration = HardwareAcceleration.NoPreference,
        [Description("@#alpha")]AlphaOption alpha = AlphaOption.Discard,
        [Description("@#scalabilityMode")]string? ScalabilityMode = default,
        [Description("@#bitrateMode")]VideoEncoderBitrateMode bitrateMode = VideoEncoderBitrateMode.Variable,
        [Description("@#latencyMode")]LatencyMode latencyMode = LatencyMode.Quality,
        [Description("@#contentHint")]string? ContentHint = default);

    [Category("optional")]
    public extern static VideoEncoderConfig OptionalAv1(
        [Description("@#av1")]AV1EncoderConfig? Av1 = default);

    [Category("optional")]
    public extern static VideoEncoderConfig OptionalAvc(
        [Description("@#avc")]AvcEncoderConfig? Avc = default);

    [Category("optional")]
    public extern static VideoEncoderConfig OptionalHevc(
        [Description("@#hevc")]HevcEncoderConfig? Hevc = default);
}

/// <summary>
/// VideoEncoderEncodeOptions
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptions")]
public record VideoEncoderEncodeOptions(
    [property: Description("@#keyFrame")]bool KeyFrame = false,
    [property: Description("@#av1")]VideoEncoderEncodeOptionsForAv1? Av1 = default,
    [property: Description("@#avc")]VideoEncoderEncodeOptionsForAvc? Avc = default,
    [property: Description("@#hevc")]VideoEncoderEncodeOptionsForHevc? Hevc = default,
    [property: Description("@#vp9")]VideoEncoderEncodeOptionsForVp9? Vp9 = default)
{
    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalKeyFrame(
        [Description("@#keyFrame")]bool keyFrame = false);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalAv1(
        [Description("@#av1")]VideoEncoderEncodeOptionsForAv1? Av1 = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalAvc(
        [Description("@#avc")]VideoEncoderEncodeOptionsForAvc? Avc = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalHevc(
        [Description("@#hevc")]VideoEncoderEncodeOptionsForHevc? Hevc = default);

    [Category("optional")]
    public extern static VideoEncoderEncodeOptions OptionalVp9(
        [Description("@#vp9")]VideoEncoderEncodeOptionsForVp9? Vp9 = default);
}

/// <summary>
/// VideoEncoderEncodeOptionsForAv1
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForAv1")]
public record VideoEncoderEncodeOptionsForAv1(
    [property: Description("@#quantizer")]ushort Quantizer = default);

/// <summary>
/// VideoEncoderEncodeOptionsForAvc
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForAvc")]
public record VideoEncoderEncodeOptionsForAvc(
    [property: Description("@#quantizer")]ushort Quantizer = default);

/// <summary>
/// VideoEncoderEncodeOptionsForHevc
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForHevc")]
public record VideoEncoderEncodeOptionsForHevc(
    [property: Description("@#quantizer")]ushort Quantizer = default);

/// <summary>
/// VideoEncoderEncodeOptionsForVp9
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderEncodeOptionsForVp9")]
public record VideoEncoderEncodeOptionsForVp9(
    [property: Description("@#quantizer")]ushort Quantizer = default);

/// <summary>
/// VideoEncoderInit
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderInit")]
public record VideoEncoderInit(
    [property: Description("@#output")]EncodedVideoChunkOutputCallback? Output = default,
    [property: Description("@#error")]WebCodecsErrorCallback? Error = default);

/// <summary>
/// VideoEncoderSupport
/// </summary>
[ECMAScript]
[Description("@#VideoEncoderSupport")]
public record VideoEncoderSupport(
    [property: Description("@#supported")]bool Supported = default,
    [property: Description("@#config")]VideoEncoderConfig? Config = default);

/// <summary>
/// VideoFrameBufferInit
/// </summary>
[ECMAScript]
[Description("@#VideoFrameBufferInit")]
public record VideoFrameBufferInit(
    [property: Description("@#format")]VideoPixelFormat? Format = default,
    [property: Description("@#codedWidth")]uint CodedWidth = default,
    [property: Description("@#codedHeight")]uint CodedHeight = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#duration")]ulong Duration = default,
    [property: Description("@#layout")]PlaneLayout[]? Layout = default,
    [property: Description("@#visibleRect")]DOMRectInit? VisibleRect = default,
    [property: Description("@#displayWidth")]uint DisplayWidth = default,
    [property: Description("@#displayHeight")]uint DisplayHeight = default,
    [property: Description("@#colorSpace")]VideoColorSpaceInit? ColorSpace = default,
    [property: Description("@#transfer")]ArrayBuffer[]? Transfer = default,
    [property: Description("@#metadata")]VideoFrameMetadata? Metadata = default);

/// <summary>
/// VideoFrameCallbackMetadata
/// </summary>
[ECMAScript]
[Description("@#VideoFrameCallbackMetadata")]
public record VideoFrameCallbackMetadata(
    [property: Description("@#presentationTime")]double PresentationTime = default,
    [property: Description("@#expectedDisplayTime")]double ExpectedDisplayTime = default,
    [property: Description("@#width")]uint Width = default,
    [property: Description("@#height")]uint Height = default,
    [property: Description("@#mediaTime")]double MediaTime = default,
    [property: Description("@#presentedFrames")]uint PresentedFrames = default,
    [property: Description("@#processingDuration")]double ProcessingDuration = default,
    [property: Description("@#captureTime")]double CaptureTime = default,
    [property: Description("@#receiveTime")]double ReceiveTime = default,
    [property: Description("@#rtpTimestamp")]uint RtpTimestamp = default);

/// <summary>
/// VideoFrameCopyToOptions
/// </summary>
[ECMAScript]
[Description("@#VideoFrameCopyToOptions")]
public record VideoFrameCopyToOptions(
    [property: Description("@#rect")]DOMRectInit? Rect = default,
    [property: Description("@#layout")]PlaneLayout[]? Layout = default);

/// <summary>
/// VideoFrameInit
/// </summary>
[ECMAScript]
[Description("@#VideoFrameInit")]
public record VideoFrameInit(
    [property: Description("@#duration")]ulong Duration = default,
    [property: Description("@#timestamp")]long Timestamp = default,
    [property: Description("@#alpha")]AlphaOption Alpha = AlphaOption.Keep,
    [property: Description("@#visibleRect")]DOMRectInit? VisibleRect = default,
    [property: Description("@#displayWidth")]uint DisplayWidth = default,
    [property: Description("@#displayHeight")]uint DisplayHeight = default,
    [property: Description("@#metadata")]VideoFrameMetadata? Metadata = default);

/// <summary>
/// ViewTimelineOptions
/// </summary>
[ECMAScript]
[Description("@#ViewTimelineOptions")]
public record ViewTimelineOptions(
    [property: Description("@#subject")]Element? Subject = default,
    [property: Description("@#axis")]ScrollAxis Axis = ScrollAxis.Block,
    [property: Description("@#inset")]ViewTimelineOptionsInsetValue? Inset = default);

/// <summary>
/// ViewportMediaStreamConstraints
/// </summary>
[ECMAScript]
[Description("@#ViewportMediaStreamConstraints")]
public record ViewportMediaStreamConstraints(
    [property: Description("@#video")]ViewportMediaStreamConstraintsVideo? Video = default,
    [property: Description("@#audio")]ViewportMediaStreamConstraintsAudio? Audio = default);

/// <summary>
/// WatchAdvertisementsOptions
/// </summary>
[ECMAScript]
[Description("@#WatchAdvertisementsOptions")]
public record WatchAdvertisementsOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// WaveShaperOptions
/// </summary>
[ECMAScript]
[Description("@#WaveShaperOptions")]
public record WaveShaperOptions(
    [property: Description("@#curve")]float[]? Curve = default,
    [property: Description("@#oversample")]OverSampleType Oversample = OverSampleType.None) : AudioNodeOptions;

/// <summary>
/// WebGLContextAttributes
/// </summary>
[ECMAScript]
[Description("@#WebGLContextAttributes")]
public record WebGLContextAttributes(
    [property: Description("@#alpha")]bool Alpha = false,
    [property: Description("@#depth")]bool Depth = false,
    [property: Description("@#stencil")]bool Stencil = false,
    [property: Description("@#antialias")]bool Antialias = false,
    [property: Description("@#premultipliedAlpha")]bool PremultipliedAlpha = false,
    [property: Description("@#preserveDrawingBuffer")]bool PreserveDrawingBuffer = false,
    [property: Description("@#powerPreference")]WebGLPowerPreference PowerPreference = WebGLPowerPreference.Default,
    [property: Description("@#failIfMajorPerformanceCaveat")]bool FailIfMajorPerformanceCaveat = false,
    [property: Description("@#desynchronized")]bool Desynchronized = false,
    [property: Description("@#xrCompatible")]bool XrCompatible = false)
{
    [Category("optional")]
    public extern static WebGLContextAttributes OptionalAlphaDepthStencil9(
        [Description("@#alpha")]bool alpha = false,
        [Description("@#depth")]bool depth = false,
        [Description("@#stencil")]bool stencil = false,
        [Description("@#antialias")]bool antialias = false,
        [Description("@#premultipliedAlpha")]bool premultipliedAlpha = false,
        [Description("@#preserveDrawingBuffer")]bool preserveDrawingBuffer = false,
        [Description("@#powerPreference")]WebGLPowerPreference powerPreference = WebGLPowerPreference.Default,
        [Description("@#failIfMajorPerformanceCaveat")]bool failIfMajorPerformanceCaveat = false,
        [Description("@#desynchronized")]bool desynchronized = false);

    [Category("optional")]
    public extern static WebGLContextAttributes OptionalXrCompatible(
        [Description("@#xrCompatible")]bool xrCompatible = false);
}

/// <summary>
/// WebGLContextEventInit
/// </summary>
[ECMAScript]
[Description("@#WebGLContextEventInit")]
public record WebGLContextEventInit(
    [property: Description("@#statusMessage")]string? StatusMessage = default) : EventInit;

/// <summary>
/// WebTransportCloseInfo
/// </summary>
[ECMAScript]
[Description("@#WebTransportCloseInfo")]
public record WebTransportCloseInfo(
    [property: Description("@#closeCode")]uint CloseCode = 0,
    [property: Description("@#reason")]string? Reason = default);

/// <summary>
/// WebTransportConnectionStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportConnectionStats")]
public record WebTransportConnectionStats(
    [property: Description("@#bytesSent")]ulong BytesSent = default,
    [property: Description("@#packetsSent")]ulong PacketsSent = default,
    [property: Description("@#bytesLost")]ulong BytesLost = default,
    [property: Description("@#packetsLost")]ulong PacketsLost = default,
    [property: Description("@#bytesReceived")]ulong BytesReceived = default,
    [property: Description("@#packetsReceived")]ulong PacketsReceived = default,
    [property: Description("@#smoothedRtt")]double SmoothedRtt = default,
    [property: Description("@#rttVariation")]double RttVariation = default,
    [property: Description("@#minRtt")]double MinRtt = default,
    [property: Description("@#datagrams")]WebTransportDatagramStats? Datagrams = default,
    [property: Description("@#estimatedSendRate")]ulong EstimatedSendRate = default);

/// <summary>
/// WebTransportDatagramStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportDatagramStats")]
public record WebTransportDatagramStats(
    [property: Description("@#droppedIncoming")]ulong DroppedIncoming = default,
    [property: Description("@#expiredIncoming")]ulong ExpiredIncoming = default,
    [property: Description("@#expiredOutgoing")]ulong ExpiredOutgoing = default,
    [property: Description("@#lostOutgoing")]ulong LostOutgoing = default);

/// <summary>
/// WebTransportErrorOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportErrorOptions")]
public record WebTransportErrorOptions(
    [property: Description("@#source")]WebTransportErrorSource Source = WebTransportErrorSource.Stream,
    [property: Description("@#streamErrorCode")]uint? StreamErrorCode = null);

/// <summary>
/// WebTransportHash
/// </summary>
[ECMAScript]
[Description("@#WebTransportHash")]
public record WebTransportHash(
    [property: Description("@#algorithm")]string? Algorithm = default,
    [property: Description("@#value")]IBufferSource? Value = default);

/// <summary>
/// WebTransportOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportOptions")]
public record WebTransportOptions(
    [property: Description("@#allowPooling")]bool AllowPooling = false,
    [property: Description("@#requireUnreliable")]bool RequireUnreliable = false,
    [property: Description("@#serverCertificateHashes")]WebTransportHash[]? ServerCertificateHashes = default,
    [property: Description("@#congestionControl")]WebTransportCongestionControl CongestionControl = WebTransportCongestionControl.Default,
    [property: Description("@#anticipatedConcurrentIncomingUnidirectionalStreams")]ushort? AnticipatedConcurrentIncomingUnidirectionalStreams = null,
    [property: Description("@#anticipatedConcurrentIncomingBidirectionalStreams")]ushort? AnticipatedConcurrentIncomingBidirectionalStreams = null);

/// <summary>
/// WebTransportReceiveStreamStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportReceiveStreamStats")]
public record WebTransportReceiveStreamStats(
    [property: Description("@#bytesReceived")]ulong BytesReceived = default,
    [property: Description("@#bytesRead")]ulong BytesRead = default);

/// <summary>
/// WebTransportSendStreamOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendStreamOptions")]
public record WebTransportSendStreamOptions(
    [property: Description("@#sendGroup")]WebTransportSendGroup? SendGroup = null,
    [property: Description("@#sendOrder")]long SendOrder = 0,
    [property: Description("@#waitUntilAvailable")]bool WaitUntilAvailable = false);

/// <summary>
/// WebTransportSendStreamStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendStreamStats")]
public record WebTransportSendStreamStats(
    [property: Description("@#bytesWritten")]ulong BytesWritten = default,
    [property: Description("@#bytesSent")]ulong BytesSent = default,
    [property: Description("@#bytesAcknowledged")]ulong BytesAcknowledged = default);

/// <summary>
/// WheelEventInit
/// </summary>
[ECMAScript]
[Description("@#WheelEventInit")]
public record WheelEventInit(
    [property: Description("@#deltaX")]double DeltaX = 0.0d,
    [property: Description("@#deltaY")]double DeltaY = 0.0d,
    [property: Description("@#deltaZ")]double DeltaZ = 0.0d,
    [property: Description("@#deltaMode")]uint DeltaMode = 0) : MouseEventInit;

/// <summary>
/// WindowControlsOverlayGeometryChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#WindowControlsOverlayGeometryChangeEventInit")]
public record WindowControlsOverlayGeometryChangeEventInit(
    [property: Description("@#titlebarAreaRect")]DOMRect? TitlebarAreaRect = default,
    [property: Description("@#visible")]bool Visible = false) : EventInit;

/// <summary>
/// WindowPostMessageOptions
/// </summary>
[ECMAScript]
[Description("@#WindowPostMessageOptions")]
public record WindowPostMessageOptions(
    [property: Description("@#targetOrigin")]string? TargetOrigin = default) : StructuredSerializeOptions;

/// <summary>
/// WorkerOptions
/// </summary>
[ECMAScript]
[Description("@#WorkerOptions")]
public record WorkerOptions(
    [property: Description("@#type")]WorkerType Type = WorkerType.Classic,
    [property: Description("@#credentials")]RequestCredentials Credentials = RequestCredentials.SameOrigin,
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// WorkletOptions
/// </summary>
[ECMAScript]
[Description("@#WorkletOptions")]
public record WorkletOptions(
    [property: Description("@#credentials")]RequestCredentials Credentials = RequestCredentials.SameOrigin);

/// <summary>
/// WriteParams
/// </summary>
[ECMAScript]
[Description("@#WriteParams")]
public record WriteParams(
    [property: Description("@#type")]WriteCommandType? Type = default,
    [property: Description("@#size")]ulong Size = default,
    [property: Description("@#position")]ulong Position = default,
    [property: Description("@#data")]WriteParamsData? Data = default);

/// <summary>
/// XRCubeLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRCubeLayerInit")]
public record XRCubeLayerInit(
    [property: Description("@#orientation")]DOMPointReadOnly? Orientation = default) : XRLayerInit;

/// <summary>
/// XRCylinderLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRCylinderLayerInit")]
public record XRCylinderLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#radius")]float Radius = 2.0f,
    [property: Description("@#centralAngle")]float CentralAngle = 0.78539f,
    [property: Description("@#aspectRatio")]float AspectRatio = 2.0f) : XRLayerInit;

/// <summary>
/// XRDOMOverlayInit
/// </summary>
[ECMAScript]
[Description("@#XRDOMOverlayInit")]
public record XRDOMOverlayInit(
    [property: Description("@#root")]Element? Root = default);

/// <summary>
/// XRDOMOverlayState
/// </summary>
[ECMAScript]
[Description("@#XRDOMOverlayState")]
public record XRDOMOverlayState(
    [property: Description("@#type")]XRDOMOverlayType? Type = default);

/// <summary>
/// XRDepthStateInit
/// </summary>
[ECMAScript]
[Description("@#XRDepthStateInit")]
public record XRDepthStateInit(
    [property: Description("@#usagePreference")]XRDepthUsage[]? UsagePreference = default,
    [property: Description("@#dataFormatPreference")]XRDepthDataFormat[]? DataFormatPreference = default);

/// <summary>
/// XREquirectLayerInit
/// </summary>
[ECMAScript]
[Description("@#XREquirectLayerInit")]
public record XREquirectLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#radius")]float Radius = 0f,
    [property: Description("@#centralHorizontalAngle")]float CentralHorizontalAngle = 6.28318f,
    [property: Description("@#upperVerticalAngle")]float UpperVerticalAngle = 1.570795f,
    [property: Description("@#lowerVerticalAngle")]float LowerVerticalAngle = -1.570795f) : XRLayerInit;

/// <summary>
/// XRHitTestOptionsInit
/// </summary>
[ECMAScript]
[Description("@#XRHitTestOptionsInit")]
public record XRHitTestOptionsInit(
    [property: Description("@#space")]XRSpace? Space = default,
    [property: Description("@#entityTypes")]FrozenSet<XRHitTestTrackableType>? EntityTypes = default,
    [property: Description("@#offsetRay")]XRRay? OffsetRay = default);

/// <summary>
/// XRInputSourceEventInit
/// </summary>
[ECMAScript]
[Description("@#XRInputSourceEventInit")]
public record XRInputSourceEventInit(
    [property: Description("@#frame")]XRFrame? Frame = default,
    [property: Description("@#inputSource")]XRInputSource? InputSource = default) : EventInit;

/// <summary>
/// XRInputSourcesChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#XRInputSourcesChangeEventInit")]
public record XRInputSourcesChangeEventInit(
    [property: Description("@#session")]XRSession? Session = default,
    [property: Description("@#added")]XRInputSource[]? Added = default,
    [property: Description("@#removed")]XRInputSource[]? Removed = default) : EventInit;

/// <summary>
/// XRLayerEventInit
/// </summary>
[ECMAScript]
[Description("@#XRLayerEventInit")]
public record XRLayerEventInit(
    [property: Description("@#layer")]XRLayer? Layer = default) : EventInit;

/// <summary>
/// XRLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRLayerInit")]
public record XRLayerInit(
    [property: Description("@#space")]XRSpace? Space = default,
    [property: Description("@#colorFormat")]GLenum? ColorFormat = default,
    [property: Description("@#depthFormat")]GLenum? DepthFormat = default,
    [property: Description("@#mipLevels")]uint MipLevels = 1,
    [property: Description("@#viewPixelWidth")]uint ViewPixelWidth = default,
    [property: Description("@#viewPixelHeight")]uint ViewPixelHeight = default,
    [property: Description("@#layout")]XRLayerLayout Layout = XRLayerLayout.Mono,
    [property: Description("@#isStatic")]bool IsStatic = false,
    [property: Description("@#clearOnAccess")]bool ClearOnAccess = false);

/// <summary>
/// XRLightProbeInit
/// </summary>
[ECMAScript]
[Description("@#XRLightProbeInit")]
public record XRLightProbeInit(
    [property: Description("@#reflectionFormat")]XRReflectionFormat ReflectionFormat = XRReflectionFormat.Srgba8);

/// <summary>
/// XRMediaCylinderLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaCylinderLayerInit")]
public record XRMediaCylinderLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#radius")]float Radius = 2.0f,
    [property: Description("@#centralAngle")]float CentralAngle = 0.78539f,
    [property: Description("@#aspectRatio")]float AspectRatio = default) : XRMediaLayerInit;

/// <summary>
/// XRMediaEquirectLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaEquirectLayerInit")]
public record XRMediaEquirectLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#radius")]float Radius = 0.0f,
    [property: Description("@#centralHorizontalAngle")]float CentralHorizontalAngle = 6.28318f,
    [property: Description("@#upperVerticalAngle")]float UpperVerticalAngle = 1.570795f,
    [property: Description("@#lowerVerticalAngle")]float LowerVerticalAngle = -1.570795f) : XRMediaLayerInit;

/// <summary>
/// XRMediaLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaLayerInit")]
public record XRMediaLayerInit(
    [property: Description("@#space")]XRSpace? Space = default,
    [property: Description("@#layout")]XRLayerLayout Layout = XRLayerLayout.Mono,
    [property: Description("@#invertStereo")]bool InvertStereo = false);

/// <summary>
/// XRMediaQuadLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRMediaQuadLayerInit")]
public record XRMediaQuadLayerInit(
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#width")]float Width = default,
    [property: Description("@#height")]float Height = default) : XRMediaLayerInit;

/// <summary>
/// XRPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#XRPermissionDescriptor")]
public record XRPermissionDescriptor(
    [property: Description("@#mode")]XRSessionMode? Mode = default,
    [property: Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
    [property: Description("@#optionalFeatures")]string[]? OptionalFeatures = default) : PermissionDescriptor;

/// <summary>
/// XRProjectionLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRProjectionLayerInit")]
public record XRProjectionLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
    [property: Description("@#colorFormat")]GLenum? ColorFormat = default,
    [property: Description("@#depthFormat")]GLenum? DepthFormat = default,
    [property: Description("@#scaleFactor")]double ScaleFactor = 1.0d,
    [property: Description("@#clearOnAccess")]bool ClearOnAccess = false);

/// <summary>
/// XRQuadLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRQuadLayerInit")]
public record XRQuadLayerInit(
    [property: Description("@#textureType")]XRTextureType TextureType = XRTextureType.Texture,
    [property: Description("@#transform")]XRRigidTransform? Transform = default,
    [property: Description("@#width")]float Width = 1.0f,
    [property: Description("@#height")]float Height = 1.0f) : XRLayerInit;

/// <summary>
/// XRRayDirectionInit
/// </summary>
[ECMAScript]
[Description("@#XRRayDirectionInit")]
public record XRRayDirectionInit(
    [property: Description("@#x")]double X = 0d,
    [property: Description("@#y")]double Y = 0d,
    [property: Description("@#z")]double Z = -1d,
    [property: Description("@#w")]double W = 0d);

/// <summary>
/// XRReferenceSpaceEventInit
/// </summary>
[ECMAScript]
[Description("@#XRReferenceSpaceEventInit")]
public record XRReferenceSpaceEventInit(
    [property: Description("@#referenceSpace")]XRReferenceSpace? ReferenceSpace = default,
    [property: Description("@#transform")]XRRigidTransform? Transform = null) : EventInit;

/// <summary>
/// XRRenderStateInit
/// </summary>
[ECMAScript]
[Description("@#XRRenderStateInit")]
public record XRRenderStateInit(
    [property: Description("@#depthNear")]double DepthNear = default,
    [property: Description("@#depthFar")]double DepthFar = default,
    [property: Description("@#inlineVerticalFieldOfView")]double InlineVerticalFieldOfView = default,
    [property: Description("@#baseLayer")]XRWebGLLayer? BaseLayer = default,
    [property: Description("@#layers")]XRLayer[]? Layers = default);

/// <summary>
/// XRSessionEventInit
/// </summary>
[ECMAScript]
[Description("@#XRSessionEventInit")]
public record XRSessionEventInit(
    [property: Description("@#session")]XRSession? Session = default) : EventInit;

/// <summary>
/// XRSessionInit
/// </summary>
[ECMAScript]
[Description("@#XRSessionInit")]
public record XRSessionInit(
    [property: Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
    [property: Description("@#optionalFeatures")]string[]? OptionalFeatures = default,
    [property: Description("@#depthSensing")]XRDepthStateInit? DepthSensing = default,
    [property: Description("@#domOverlay")]XRDOMOverlayInit? DomOverlay = default)
{
    [Category("optional")]
    public extern static XRSessionInit OptionalRequiredFeaturesOptionalFeatures(
        [Description("@#requiredFeatures")]string[]? RequiredFeatures = default,
        [Description("@#optionalFeatures")]string[]? OptionalFeatures = default);

    [Category("optional")]
    public extern static XRSessionInit OptionalDepthSensing(
        [Description("@#depthSensing")]XRDepthStateInit? DepthSensing = default);

    [Category("optional")]
    public extern static XRSessionInit OptionalDomOverlay(
        [Description("@#domOverlay")]XRDOMOverlayInit? DomOverlay = default);
}

/// <summary>
/// XRSessionSupportedPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#XRSessionSupportedPermissionDescriptor")]
public record XRSessionSupportedPermissionDescriptor(
    [property: Description("@#mode")]XRSessionMode? Mode = default) : PermissionDescriptor;

/// <summary>
/// XRTransientInputHitTestOptionsInit
/// </summary>
[ECMAScript]
[Description("@#XRTransientInputHitTestOptionsInit")]
public record XRTransientInputHitTestOptionsInit(
    [property: Description("@#profile")]string? Profile = default,
    [property: Description("@#entityTypes")]FrozenSet<XRHitTestTrackableType>? EntityTypes = default,
    [property: Description("@#offsetRay")]XRRay? OffsetRay = default);

/// <summary>
/// XRWebGLLayerInit
/// </summary>
[ECMAScript]
[Description("@#XRWebGLLayerInit")]
public record XRWebGLLayerInit(
    [property: Description("@#antialias")]bool Antialias = false,
    [property: Description("@#depth")]bool Depth = false,
    [property: Description("@#stencil")]bool Stencil = false,
    [property: Description("@#alpha")]bool Alpha = false,
    [property: Description("@#ignoreDepthValues")]bool IgnoreDepthValues = false,
    [property: Description("@#framebufferScaleFactor")]double FramebufferScaleFactor = 1.0d);
