namespace ECMAScript;

/// <summary>
/// RTCRtpSendParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpSendParameters")]
public record RTCRtpSendParameters(
    [property: Description("@#degradationPreference")]RTCDegradationPreference? DegradationPreference = default,
	[property: Description("@#transactionId")]string? TransactionId = default,
	[property: Description("@#encodings")]RTCRtpEncodingParameters[]? Encodings = default): RTCRtpParameters
{
    [Category("optional")]
    public extern static RTCRtpSendParameters OptionalDegradationPreference(
        [Description("@#degradationPreference")]RTCDegradationPreference? DegradationPreference = default);

    [Category("optional")]
    public extern static RTCRtpSendParameters OptionalTransactionIdEncodings(
        [Description("@#transactionId")]string? TransactionId = default,
        [Description("@#encodings")]RTCRtpEncodingParameters[]? Encodings = default);
}