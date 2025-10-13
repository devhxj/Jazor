namespace ECMAScript;

/// <summary>
/// StorageEstimate
/// </summary>
[ECMAScript]
[Description("@#StorageEstimate")]
public record StorageEstimate(
    [property: Description("@#usage")]ulong Usage = default,
	[property: Description("@#quota")]ulong Quota = default);