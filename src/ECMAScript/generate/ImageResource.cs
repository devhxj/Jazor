namespace ECMAScript;

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