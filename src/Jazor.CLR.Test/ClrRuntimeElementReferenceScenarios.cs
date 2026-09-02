namespace Jazor.CLR.Test;

internal static class ClrRuntimeElementReferenceScenarios
{
    private const string ExtensionsModule = "Microsoft/AspNetCore/Components/ElementReferenceExtensionsModule.js";
    private const string FocusAsync = "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)";
    private const string FocusAsyncWithOptions = "static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)";

    // ElementReference is an HTMLElement carrier in the browser. A callable focus member keeps
    // the scenario on the same path as the generated helper without introducing a DOM dependency
    // into the CLR runtime host.
    private static readonly ClrRuntimeValue Element = ClrRuntimeValue.Record(
        ("focus", ClrRuntimeValue.Callable(ClrRuntimeCallableKind.Identity)));

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "element-reference.focus-async",
            FocusAsync,
            [Element],
            ClrRuntimeValue.Undefined()),
        Success(
            "element-reference.focus-async-with-options",
            FocusAsyncWithOptions,
            [Element, ClrRuntimeValue.Boolean(true)],
            ClrRuntimeValue.Undefined())
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, ExtensionsModule, arguments, expected);
}
