namespace Jazor.CLR.Test;

internal static class ClrRuntimeUriScenarios
{
	private const string ModulePath = "System/UriModule.js";
	private const string PathAndQuery = "System.Uri.PathAndQuery.get";
	private const string Port = "System.Uri.Port.get";

	public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
	[
		Success(
			"uri.path-and-query",
			PathAndQuery,
			[Url("https://example.test/app/orders?state=open#details")],
			Text("/app/orders?state=open")),
		Success(
			"uri.path-and-query.without-query",
			PathAndQuery,
			[Url("https://example.test/app/orders")],
			Text("/app/orders")),
		// URL.port is empty for a scheme default port, while Uri.Port resolves it.
		Success("uri.port.explicit", Port, [Url("https://example.test:8443/app/")], Number(8443)),
		Success("uri.port.default-https", Port, [Url("https://example.test/app/")], Number(443)),
		Success("uri.port.default-http", Port, [Url("http://example.test/app/")], Number(80)),
		Success("uri.port.default-web-socket", Port, [Url("ws://example.test/socket")], Number(80)),
		Success("uri.port.default-secure-web-socket", Port, [Url("wss://example.test/socket")], Number(443)),
		Success("uri.port.default-ftp", Port, [Url("ftp://example.test/pub/")], Number(21)),
		Success("uri.port.unknown-scheme", Port, [Url("mailto:team@example.test")], Number(-1))
	];

	private static ClrRuntimeScenario Success(
		string id,
		string member,
		IReadOnlyList<ClrRuntimeValue> arguments,
		ClrRuntimeValue expected)
		=> new(id, member, ModulePath, arguments, expected);

	private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
	private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
	private static ClrRuntimeValue Url(string href) => ClrRuntimeValue.Url(href);
}
