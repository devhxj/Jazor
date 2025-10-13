namespace ECMAScript;

/// <summary>
/// RTCRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCRtpStreamStats")]
public record RTCRtpStreamStats(
    [property: Description("@#ssrc")]uint Ssrc = default,
	[property: Description("@#kind")]string? Kind = default,
	[property: Description("@#transportId")]string? TransportId = default,
	[property: Description("@#codecId")]string? CodecId = default): RTCStats;

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
	[property: Description("@#sdpFmtpLine")]string? SdpFmtpLine = default): RTCStats;

/// <summary>
/// RTCReceivedRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCReceivedRtpStreamStats")]
public record RTCReceivedRtpStreamStats(
    [property: Description("@#packetsReceived")]ulong PacketsReceived = default,
	[property: Description("@#packetsReceivedWithEct1")]ulong PacketsReceivedWithEct1 = default,
	[property: Description("@#packetsReceivedWithCe")]ulong PacketsReceivedWithCe = default,
	[property: Description("@#packetsReportedAsLost")]ulong PacketsReportedAsLost = default,
	[property: Description("@#packetsReportedAsLostButRecovered")]ulong PacketsReportedAsLostButRecovered = default,
	[property: Description("@#packetsLost")]long PacketsLost = default,
	[property: Description("@#jitter")]double Jitter = default): RTCRtpStreamStats;

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
	[property: Description("@#fecSsrc")]uint FecSsrc = default,
	[property: Description("@#totalCorruptionProbability")]double TotalCorruptionProbability = default,
	[property: Description("@#totalSquaredCorruptionProbability")]double TotalSquaredCorruptionProbability = default,
	[property: Description("@#corruptionMeasurements")]ulong CorruptionMeasurements = default): RTCReceivedRtpStreamStats;

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
	[property: Description("@#roundTripTimeMeasurements")]ulong RoundTripTimeMeasurements = default,
	[property: Description("@#packetsWithBleachedEct1Marking")]ulong PacketsWithBleachedEct1Marking = default): RTCReceivedRtpStreamStats;

/// <summary>
/// RTCSentRtpStreamStats
/// </summary>
[ECMAScript]
[Description("@#RTCSentRtpStreamStats")]
public record RTCSentRtpStreamStats(
    [property: Description("@#packetsSent")]ulong PacketsSent = default,
	[property: Description("@#bytesSent")]ulong BytesSent = default,
	[property: Description("@#packetsSentWithEct1")]ulong PacketsSentWithEct1 = default): RTCRtpStreamStats;

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
	[property: Description("@#encodingIndex")]uint EncodingIndex = default,
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
	[property: Description("@#scalabilityMode")]string? ScalabilityMode = default): RTCSentRtpStreamStats;

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
	[property: Description("@#roundTripTimeMeasurements")]ulong RoundTripTimeMeasurements = default): RTCSentRtpStreamStats;

/// <summary>
/// RTCMediaSourceStats
/// </summary>
[ECMAScript]
[Description("@#RTCMediaSourceStats")]
public record RTCMediaSourceStats(
    [property: Description("@#trackIdentifier")]string? TrackIdentifier = default,
	[property: Description("@#kind")]string? Kind = default): RTCStats;

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
	[property: Description("@#echoReturnLossEnhancement")]double EchoReturnLossEnhancement = default): RTCMediaSourceStats;

/// <summary>
/// RTCVideoSourceStats
/// </summary>
[ECMAScript]
[Description("@#RTCVideoSourceStats")]
public record RTCVideoSourceStats(
    [property: Description("@#width")]uint Width = default,
	[property: Description("@#height")]uint Height = default,
	[property: Description("@#frames")]uint Frames = default,
	[property: Description("@#framesPerSecond")]double FramesPerSecond = default): RTCMediaSourceStats;

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
	[property: Description("@#totalSamplesCount")]ulong TotalSamplesCount = default): RTCStats;

/// <summary>
/// RTCPeerConnectionStats
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionStats")]
public record RTCPeerConnectionStats(
    [property: Description("@#dataChannelsOpened")]uint DataChannelsOpened = default,
	[property: Description("@#dataChannelsClosed")]uint DataChannelsClosed = default): RTCStats;

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
	[property: Description("@#bytesReceived")]ulong BytesReceived = default): RTCStats;

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
	[property: Description("@#selectedCandidatePairChanges")]uint SelectedCandidatePairChanges = default,
	[property: Description("@#ccfbMessagesSent")]uint CcfbMessagesSent = default,
	[property: Description("@#ccfbMessagesReceived")]uint CcfbMessagesReceived = default): RTCStats;

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
	[property: Description("@#tcpType")]RTCIceTcpCandidateType? TcpType = default): RTCStats;

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
	[property: Description("@#bytesDiscardedOnSend")]ulong BytesDiscardedOnSend = default): RTCStats;

/// <summary>
/// RTCCertificateStats
/// </summary>
[ECMAScript]
[Description("@#RTCCertificateStats")]
public record RTCCertificateStats(
    [property: Description("@#fingerprint")]string? Fingerprint = default,
	[property: Description("@#fingerprintAlgorithm")]string? FingerprintAlgorithm = default,
	[property: Description("@#base64Certificate")]string? Base64Certificate = default,
	[property: Description("@#issuerCertificateId")]string? IssuerCertificateId = default): RTCStats;