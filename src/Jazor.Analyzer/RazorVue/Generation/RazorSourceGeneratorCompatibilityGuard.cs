using System;
using System.Collections.Immutable;
using System.Linq;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorCompatibilityGuard
{
    internal const string ExpectedInitializeMethodIlSha256 = "FDC307519FAC1C8FB29D94CE5477D6B5FDEB136ED3757D1236BB12E28C2F6B32";

    private static readonly ImmutableArray<string> ExpectedDeclaredMethodNames =
    [
        "ComputeRazorSourceGeneratorOptions",
        "Initialize"
    ];

    public static RazorSourceGeneratorCompatibilityValidationResult Validate(RazorSourceGeneratorCompatibilityProbeResult probeResult)
    {
        if (probeResult is null)
            throw new ArgumentNullException(nameof(probeResult));

        if (!probeResult.Success || probeResult.Shape is null)
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                probeResult.Failure ?? "The Razor source generator compatibility probe did not return a shape snapshot.");
        }

        return Validate(probeResult.Shape);
    }

    public static RazorSourceGeneratorCompatibilityValidationResult Validate(RazorSourceGeneratorCompatibilityShape shape)
    {
        if (shape is null)
            throw new ArgumentNullException(nameof(shape));

        if (!string.Equals(shape.InitializeMethodIlSha256, ExpectedInitializeMethodIlSha256, StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "Initialize(...) IL SHA-256 mismatch. Expected '" +
                ExpectedInitializeMethodIlSha256 +
                "' but found '" +
                shape.InitializeMethodIlSha256 +
                "'.");
        }

        if (!shape.DeclaredMethodNames.SequenceEqual(ExpectedDeclaredMethodNames, StringComparer.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "Declared method surface mismatch. Expected [" +
                string.Join(", ", ExpectedDeclaredMethodNames) +
                "] but found [" +
                string.Join(", ", shape.DeclaredMethodNames) +
                "].");
        }

        return RazorSourceGeneratorCompatibilityValidationResult.Succeed(shape);
    }
}

internal sealed record RazorSourceGeneratorCompatibilityValidationResult(
    bool Success,
    RazorSourceGeneratorCompatibilityShape? Shape,
    string? Failure)
{
    public static RazorSourceGeneratorCompatibilityValidationResult Succeed(RazorSourceGeneratorCompatibilityShape shape)
        => new(true, shape, null);

    public static RazorSourceGeneratorCompatibilityValidationResult Fail(string failure)
        => new(false, null, failure);
}
