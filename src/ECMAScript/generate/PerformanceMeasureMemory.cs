namespace ECMAScript;

/// <summary>
/// MemoryMeasurement
/// </summary>
[ECMAScript]
[Description("@#MemoryMeasurement")]
public record MemoryMeasurement(
    [property: Description("@#bytes")]ulong Bytes = default,
	[property: Description("@#breakdown")]MemoryBreakdownEntry[]? Breakdown = default);

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