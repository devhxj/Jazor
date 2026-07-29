namespace Jazor.CLR.Test;

internal enum ClrRuntimeValueKind
{
    Null,
    String,
    Number,
    Boolean,
    BigInt,
    Array,
    Undefined
}

internal sealed record ClrRuntimeValue(
    ClrRuntimeValueKind Kind,
    string? Scalar = null,
    IReadOnlyList<ClrRuntimeValue>? Items = null)
{
    public static ClrRuntimeValue Null() => new(ClrRuntimeValueKind.Null);

    public static ClrRuntimeValue Text(string value) => new(ClrRuntimeValueKind.String, value);

    public static ClrRuntimeValue Number(double value)
        => new(ClrRuntimeValueKind.Number, value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue BigInt(long value)
        => new(ClrRuntimeValueKind.BigInt, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static ClrRuntimeValue Boolean(bool value)
        => new(ClrRuntimeValueKind.Boolean, value ? "true" : "false");

    public static ClrRuntimeValue Array(params ClrRuntimeValue[] values)
        => new(ClrRuntimeValueKind.Array, Items: values);

    public static ClrRuntimeValue Undefined() => new(ClrRuntimeValueKind.Undefined);
}

internal sealed record ClrRuntimeScenario(
    string Id,
    string Member,
    string ModulePath,
    IReadOnlyList<ClrRuntimeValue> Arguments,
    ClrRuntimeValue? ExpectedValue,
    string? ExpectedErrorContains = null);

internal static class ClrRuntimeScenarioCatalog
{
    private const string GuidModulePath = "System/GuidModule.js";
    private const string CanonicalGuid = "00112233-4455-6677-8899-aabbccddeeff";
    private const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        new(
            "guid.parse.uppercase-d-format",
            "static System.Guid.Parse(string)",
            GuidModulePath,
            [ClrRuntimeValue.Text("00112233-4455-6677-8899-AABBCCDDEEFF")],
            ClrRuntimeValue.Text(CanonicalGuid)),
        new(
            "guid.parse.compact-format",
            "static System.Guid.Parse(string)",
            GuidModulePath,
            [ClrRuntimeValue.Text("00112233445566778899AABBCCDDEEFF")],
            ClrRuntimeValue.Text(CanonicalGuid)),
        new(
            "guid.try-parse.valid-braced-format",
            "static System.Guid.TryParse(string, out System.Guid)",
            GuidModulePath,
            [ClrRuntimeValue.Text("{00112233-4455-6677-8899-AABBCCDDEEFF}"), ClrRuntimeValue.Null()],
            ClrRuntimeValue.Array(ClrRuntimeValue.Boolean(true), ClrRuntimeValue.Text(CanonicalGuid))),
        new(
            "guid.try-parse.invalid-text",
            "static System.Guid.TryParse(string, out System.Guid)",
            GuidModulePath,
            [ClrRuntimeValue.Text("not-a-guid"), ClrRuntimeValue.Null()],
            ClrRuntimeValue.Array(ClrRuntimeValue.Boolean(false), ClrRuntimeValue.Text(EmptyGuid))),
        new(
            "guid.format.lowercase-n-specifier",
            "System.Guid.ToString(string)",
            GuidModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid), ClrRuntimeValue.Text("n")],
            ClrRuntimeValue.Text("00112233445566778899aabbccddeeff")),
        new(
            "guid.equals.normalized-value",
            "System.Guid.Equals(System.Guid)",
            GuidModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid), ClrRuntimeValue.Text("00112233445566778899AABBCCDDEEFF")],
            ClrRuntimeValue.Boolean(true)),
        new(
            "guid.hash-code.known-value",
            "override System.Guid.GetHashCode()",
            GuidModulePath,
            [ClrRuntimeValue.Text(CanonicalGuid)],
            ClrRuntimeValue.Number(572662306)),
        new(
            "guid.parse.invalid-text-throws-format",
            "static System.Guid.Parse(string)",
            GuidModulePath,
            [ClrRuntimeValue.Text("not-a-guid")],
            ExpectedValue: null,
            ExpectedErrorContains: "FormatException")
    ];

    public static ClrRuntimeScenario Get(string id)
        => All.Single(scenario => string.Equals(scenario.Id, id, StringComparison.Ordinal));
}
