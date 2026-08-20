namespace Jazor.CLR;

[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions", "Object")]
public static class NavigationOptionsModule
{
	// NavigationOptions is a value struct in .NET but has no JavaScript runtime identity.
	// Construct it as a plain object so object initializers lower through the existing alias
	// setters and NavigateTo receives exactly the browser options shape. The CLR struct
	// default is `false`/`null` per field, so the literal spells every field out instead of
	// leaving unset properties as JavaScript undefined.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.NavigationOptions.NavigationOptions()", "({ forceLoad: false, replaceHistoryEntry: false, relativeToCurrentUri: false, historyEntryState: null })")]
	public extern static Object _1dc425421ae36b94();

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.ForceLoad.get", "forceLoad")]
	public extern static bool _a60d63c0a9602cec(Object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.ForceLoad.init", "forceLoad")]
	public extern static void _3a723dbbf052d509(Object instance, bool value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.ReplaceHistoryEntry.get", "replaceHistoryEntry")]
	public extern static bool _f09c8f5da9121da1(Object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.ReplaceHistoryEntry.init", "replaceHistoryEntry")]
	public extern static void _1e0b27fd5c889390(Object instance, bool value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.HistoryEntryState.get", "historyEntryState")]
	public extern static string? _05e50d3ef6a775f0(Object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.HistoryEntryState.init", "historyEntryState")]
	public extern static void _72530ede60b3278f(Object instance, string? value);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.RelativeToCurrentUri.get", "relativeToCurrentUri")]
	public extern static bool _ba2530ce074a9c3f(Object instance);

	[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.NavigationOptions.RelativeToCurrentUri.init", "relativeToCurrentUri")]
	public extern static void _f735be5076ee98fe(Object instance, bool value);
}
