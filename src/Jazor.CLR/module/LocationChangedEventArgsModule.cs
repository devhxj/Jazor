namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Routing/LocationChangedEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs", "Object")]
public static class LocationChangedEventArgsModule
{
	[Jazor(Op.Import, "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.LocationChangedEventArgs(string, bool)", "createLocationChangedEventArgs")]
	public static object _16454e1af5169b10(string location, bool isNavigationIntercepted)
	=> new
	{
		location,
		isNavigationIntercepted,
		historyEntryState = Window.History.State
	};

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.Location.get", "__arg1.location")]
	public extern static string _ddd54f7cf1558e81(object instance);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.IsNavigationIntercepted.get", "__arg1.isNavigationIntercepted")]
	public extern static bool _bcf2448cb62ca1d7(object instance);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.HistoryEntryState.get", "__arg1.historyEntryState")]
	public extern static string? _b0f232bac7760f91(object instance);
}
