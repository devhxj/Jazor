namespace ECMAScript;

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