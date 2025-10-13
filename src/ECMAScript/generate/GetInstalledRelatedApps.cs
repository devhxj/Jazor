namespace ECMAScript;

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