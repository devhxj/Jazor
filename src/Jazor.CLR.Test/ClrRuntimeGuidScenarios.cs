namespace Jazor.CLR.Test;

internal static class ClrRuntimeGuidScenarios
{
    private const string ModulePath = "System/GuidModule.js";
    private const string CanonicalGuid = "00112233-4455-6677-8899-aabbccddeeff";
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        new(
            "guid.parse.uppercase-d-format",
            "static System.Guid.Parse(string)",
            ModulePath,
            [ClrRuntimeValue.Text("00112233-4455-6677-8899-AABBCCDDEEFF")],
            ClrRuntimeValue.Text(CanonicalGuid)),
        new(
            "guid.parse.compact-format",
            "static System.Guid.Parse(string)",
            ModulePath,
            [ClrRuntimeValue.Text("00112233445566778899AABBCCDDEEFF")],
            ClrRuntimeValue.Text(CanonicalGuid)),
        new(
            "guid.try-parse.valid-braced-format",
            "static System.Guid.TryParse(string, out System.Guid)",
            ModulePath,
            [ClrRuntimeValue.Text("{00112233-4455-6677-8899-AABBCCDDEEFF}"), ClrRuntimeValue.Null()],
            ClrRuntimeValue.Array(ClrRuntimeValue.Boolean(true), ClrRuntimeValue.Text(CanonicalGuid))),
        new(
            "guid.try-parse.invalid-text",
            "static System.Guid.TryParse(string, out System.Guid)",
            ModulePath,
            [ClrRuntimeValue.Text("not-a-guid"), ClrRuntimeValue.Null()],
            ClrRuntimeValue.Array(ClrRuntimeValue.Boolean(false), ClrRuntimeValue.Text(EmptyGuid))),
        new(
            "guid.format.lowercase-n-specifier",
            "System.Guid.ToString(string)",
            ModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid), ClrRuntimeValue.Text("n")],
            ClrRuntimeValue.Text("00112233445566778899aabbccddeeff")),
        new(
            "guid.equals.normalized-value",
            "System.Guid.Equals(System.Guid)",
            ModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid), ClrRuntimeValue.Text("00112233445566778899AABBCCDDEEFF")],
            ClrRuntimeValue.Boolean(true)),
        new(
            "guid.hash-code.known-value",
            "override System.Guid.GetHashCode()",
            ModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid)],
            ClrRuntimeValue.Number(572662306)),
        new(
            "guid.parse.invalid-text-throws-format",
            "static System.Guid.Parse(string)",
            ModulePath,
            [ClrRuntimeValue.Text("not-a-guid")],
            ExpectedValue: null,
            ExpectedErrorContains: "FormatException")
    ];
}
