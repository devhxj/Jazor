namespace ECMAScript;

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