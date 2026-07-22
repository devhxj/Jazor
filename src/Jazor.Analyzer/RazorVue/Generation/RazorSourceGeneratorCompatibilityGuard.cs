using System;
using System.Linq;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorCompatibilityGuard
{
    internal const string RazorSourceGeneratorTypeName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator";
    internal const string IncrementalGeneratorInitializationContextTypeName = "Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext";
    internal const string VoidTypeName = "System.Void";
    // The hook overwrites Initialize, so a shape-only check is not enough to make patching safe.
    internal const string SupportedRazorCompilerAssemblyVersion = "10.4.0.0";
    internal const int SupportedInitializeMethodIlLength = 1375;
    internal const string SupportedInitializeMethodIlSha256 = "D26899291FF7BD908E842A14E3AFE5DBB1CE7376AB667C7733EBF25820FBBDC6";

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

        if (!string.Equals(shape.TypeFullName, RazorSourceGeneratorTypeName, StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "Razor source generator type mismatch. Expected '" +
                RazorSourceGeneratorTypeName +
                "' but found '" +
                shape.TypeFullName +
                "'.");
        }

        if (!shape.ImplementsIncrementalGenerator)
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                RazorSourceGeneratorTypeName + " no longer implements Microsoft.CodeAnalysis.IIncrementalGenerator.");
        }

        if (!string.Equals(shape.InitializeMethodName, "Initialize", StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize method mismatch. Expected 'Initialize' but found '" +
                shape.InitializeMethodName +
                "'.");
        }

        if (!string.Equals(shape.InitializeContextParameterType, IncrementalGeneratorInitializationContextTypeName, StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize parameter mismatch. Expected '" +
                IncrementalGeneratorInitializationContextTypeName +
                "' but found '" +
                shape.InitializeContextParameterType +
                "'.");
        }

        if (!string.Equals(shape.InitializeMethodReturnType, VoidTypeName, StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize return type mismatch. Expected '" +
                VoidTypeName +
                "' but found '" +
                shape.InitializeMethodReturnType +
                "'.");
        }

        if (!shape.InitializeMethodIsPublic)
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize is no longer public.");
        }

        if (shape.InitializeMethodIsStatic)
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize is static; the RazorVue bootstrap patch expects an instance method.");
        }

        if (!string.Equals(shape.AssemblyVersion, SupportedRazorCompilerAssemblyVersion, StringComparison.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "Razor source generator assembly version mismatch. Expected '" +
                SupportedRazorCompilerAssemblyVersion +
                "' but found '" +
                shape.AssemblyVersion +
                "'.");
        }

        if (!shape.InitializeMethodIlFingerprintAvailable)
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize fingerprint was unavailable. " +
                "Expected IL length " +
                SupportedInitializeMethodIlLength.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                " and SHA-256 '" +
                SupportedInitializeMethodIlSha256 +
                "'.");
        }

        if (shape.InitializeMethodIlLength != SupportedInitializeMethodIlLength ||
            !string.Equals(shape.InitializeMethodIlSha256, SupportedInitializeMethodIlSha256, StringComparison.OrdinalIgnoreCase))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "RazorSourceGenerator.Initialize fingerprint mismatch. Expected IL length " +
                SupportedInitializeMethodIlLength.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                " and SHA-256 '" +
                SupportedInitializeMethodIlSha256 +
                "', but found IL length " +
                shape.InitializeMethodIlLength.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                " and SHA-256 '" +
                shape.InitializeMethodIlSha256 +
                "'.");
        }

        if (!shape.DeclaredMethodNames.Contains("Initialize", StringComparer.Ordinal))
        {
            return RazorSourceGeneratorCompatibilityValidationResult.Fail(
                "Declared method surface no longer contains Initialize. Found [" +
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
