namespace Jazor.CLR.Test;

internal static class ClrRuntimeNavigationScenarios
{
    private const string NavigationModule = "Microsoft/AspNetCore/Components/NavigationManagerModule.js";
    private const string LocationChangedModule = "Microsoft/AspNetCore/Components/Routing/LocationChangedEventArgsModule.js";
    private const string ExtensionsModule = "Microsoft/AspNetCore/Components/NavigationManagerExtensionsModule.js";

    private const string BaseUri = "Microsoft.AspNetCore.Components.NavigationManager.BaseUri.get";
    private const string HistoryEntryState = "Microsoft.AspNetCore.Components.NavigationManager.HistoryEntryState.get";
    private const string LocationChangedAdd = "Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.add";
    private const string LocationChangedRemove = "Microsoft.AspNetCore.Components.NavigationManager.LocationChanged.remove";
    private const string NavigateToOptions = "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, Microsoft.AspNetCore.Components.NavigationOptions)";
    private const string NavigateToForceLoad = "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool)";
    private const string NavigateToForceLoadReplace = "Microsoft.AspNetCore.Components.NavigationManager.NavigateTo(string, bool, bool)";
    private const string Refresh = "virtual Microsoft.AspNetCore.Components.NavigationManager.Refresh(bool)";
    private const string ToAbsoluteUri = "Microsoft.AspNetCore.Components.NavigationManager.ToAbsoluteUri(string)";
    private const string ToBaseRelativePath = "Microsoft.AspNetCore.Components.NavigationManager.ToBaseRelativePath(string)";
    private const string Uri = "Microsoft.AspNetCore.Components.NavigationManager.Uri.get";
    private const string LocationChangedConstructor = "Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.LocationChangedEventArgs(string, bool)";

    private const string QueryParameterPrefix = "static Microsoft.AspNetCore.Components.NavigationManagerExtensions.GetUriWithQueryParameter(Microsoft.AspNetCore.Components.NavigationManager, string, ";
    private const string QueryParameters = "static Microsoft.AspNetCore.Components.NavigationManagerExtensions.GetUriWithQueryParameters(Microsoft.AspNetCore.Components.NavigationManager, System.Collections.Generic.IReadOnlyDictionary<string, object>)";
    private const string QueryParametersFromUri = "static Microsoft.AspNetCore.Components.NavigationManagerExtensions.GetUriWithQueryParameters(Microsoft.AspNetCore.Components.NavigationManager, string, System.Collections.Generic.IReadOnlyDictionary<string, object>)";
    private const string Fragment = "static Microsoft.AspNetCore.Components.NavigationManagerExtensions.GetUriWithFragment(Microsoft.AspNetCore.Components.NavigationManager, string)";

    private static readonly ClrRuntimeValue Navigation = ClrRuntimeValue.Record(
        ("uri", ClrRuntimeValue.Text("https://example.test/app/start")));

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("navigation.base-uri", BaseUri, NavigationModule, [Navigation], ClrRuntimeValue.Text("https://example.test/app/")),
        Success("navigation.uri", Uri, NavigationModule, [Navigation], ClrRuntimeValue.Text("https://example.test/app/start")),
        Success("navigation.history-entry-state", HistoryEntryState, NavigationModule, [Navigation], ClrRuntimeValue.Text("history-state")),
        Success("navigation.location-changed.add", LocationChangedAdd, NavigationModule, [Navigation, ClrRuntimeValue.Callable(ClrRuntimeCallableKind.Identity)], ClrRuntimeValue.Undefined()),
        Success("navigation.location-changed.remove", LocationChangedRemove, NavigationModule, [Navigation, ClrRuntimeValue.Callable(ClrRuntimeCallableKind.Identity)], ClrRuntimeValue.Undefined()),
        Success("navigation.navigate-to.force-load", NavigateToForceLoad, NavigationModule, [Navigation, ClrRuntimeValue.Text("/app/orders"), ClrRuntimeValue.Boolean(false)], ClrRuntimeValue.Undefined()),
        Success("navigation.navigate-to.force-load-replace", NavigateToForceLoadReplace, NavigationModule, [Navigation, ClrRuntimeValue.Text("/app/orders"), ClrRuntimeValue.Boolean(false), ClrRuntimeValue.Boolean(true)], ClrRuntimeValue.Undefined()),
        Success("navigation.navigate-to.options", NavigateToOptions, NavigationModule, [Navigation, ClrRuntimeValue.Text("/app/orders"), ClrRuntimeValue.Record(
            ("replaceHistoryEntry", ClrRuntimeValue.Boolean(true)),
            ("historyEntryState", ClrRuntimeValue.Text("next-state")))], ClrRuntimeValue.Undefined()),
        Success("navigation.to-absolute-uri", ToAbsoluteUri, NavigationModule, [Navigation, ClrRuntimeValue.Text("orders")], ClrRuntimeValue.Text("https://example.test/app/orders")),
        Success("navigation.refresh", Refresh, NavigationModule, [Navigation, ClrRuntimeValue.Boolean(false)], ClrRuntimeValue.Undefined()),
        Success("navigation.to-base-relative-path", ToBaseRelativePath, NavigationModule, [Navigation, ClrRuntimeValue.Text("https://example.test/app/orders?x=1")], ClrRuntimeValue.Text("orders?x=1")),
        Success("navigation.location-changed-event-args", LocationChangedConstructor, LocationChangedModule, [ClrRuntimeValue.Text("https://example.test/app/orders"), ClrRuntimeValue.Boolean(true)], ClrRuntimeValue.Record(
            ("historyEntryState", ClrRuntimeValue.Text("history-state")),
            ("isNavigationIntercepted", ClrRuntimeValue.Boolean(true)),
            ("location", ClrRuntimeValue.Text("https://example.test/app/orders")))),

        QueryParameter("bool", "bool"),
        QueryParameter("nullable-bool", "bool?"),
        QueryParameter("date-time", "System.DateTime"),
        QueryParameter("nullable-date-time", "System.DateTime?"),
        QueryParameter("date-only", "System.DateOnly"),
        QueryParameter("nullable-date-only", "System.DateOnly?"),
        QueryParameter("time-only", "System.TimeOnly"),
        QueryParameter("nullable-time-only", "System.TimeOnly?"),
        QueryParameter("decimal", "decimal"),
        QueryParameter("nullable-decimal", "decimal?"),
        QueryParameter("double", "double"),
        QueryParameter("nullable-double", "double?"),
        QueryParameter("float", "float"),
        QueryParameter("nullable-float", "float?"),
        QueryParameter("guid", "System.Guid"),
        QueryParameter("nullable-guid", "System.Guid?"),
        QueryParameter("int", "int"),
        QueryParameter("nullable-int", "int?"),
        QueryParameter("long", "long"),
        QueryParameter("nullable-long", "long?"),
        QueryParameter("string", "string"),
        Success("navigation.query-parameters", QueryParameters, ExtensionsModule, [Navigation, ClrRuntimeValue.Map((ClrRuntimeValue.Text("filter"), ClrRuntimeValue.Text("open")))], ClrRuntimeValue.Text("https://example.test/app/start?filter=open")),
        Success("navigation.query-parameters-from-uri", QueryParametersFromUri, ExtensionsModule, [Navigation, ClrRuntimeValue.Text("https://example.test/app/start"), ClrRuntimeValue.Map((ClrRuntimeValue.Text("filter"), ClrRuntimeValue.Text("open")))], ClrRuntimeValue.Text("https://example.test/app/start?filter=open")),
        Success("navigation.fragment", Fragment, ExtensionsModule, [Navigation, ClrRuntimeValue.Text("details")], ClrRuntimeValue.Text("https://example.test/app/start#details"))
    ];

    private static ClrRuntimeScenario QueryParameter(string id, string type)
    {
        var member = QueryParameterPrefix + type + ")";
        return Success(
            $"navigation.query-parameter.{id}",
            member,
            ExtensionsModule,
            [Navigation, ClrRuntimeValue.Text("filter"), ClrRuntimeValue.Text("open")],
            ClrRuntimeValue.Text("https://example.test/app/start?filter=open"));
    }

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);
}
