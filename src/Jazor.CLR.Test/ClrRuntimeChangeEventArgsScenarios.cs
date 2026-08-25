namespace Jazor.CLR.Test;

internal static class ClrRuntimeChangeEventArgsScenarios
{
    private const string ModulePath = "Microsoft/AspNetCore/Components/ChangeEventArgsModule.js";
    private const string Capture = "Microsoft.AspNetCore.Components.ChangeEventArgs.captureChangeEvent";
    private const string GetValue = "Microsoft.AspNetCore.Components.ChangeEventArgs.Value.get";

    public static IReadOnlyList<ClrRuntimeScenario> All
        =>
        [
            new(
                "change-event.capture-null-rejected",
                Capture,
                ModulePath,
                [ClrRuntimeValue.Null()],
                ExpectedValue: null,
                ExpectedErrorContains: "ArgumentNullException"),
            new(
                "change-event.value-null-rejected",
                GetValue,
                ModulePath,
                [ClrRuntimeValue.Null()],
                ExpectedValue: null,
                ExpectedErrorContains: "ArgumentNullException")
        ];
}
