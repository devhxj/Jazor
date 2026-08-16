using System.Collections.Immutable;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Generation;

/// <summary>
/// Identifies the RazorVue final-compilation boundary that rejected an authoring shape.
/// 分类是跨 pipeline 的稳定协议，不能由异常文本或生成 C# 片段反推。
/// </summary>
internal enum RazorVueDiagnosticCategory
{
    Internal = 0,
    ComponentBinding,
    MemberClosure,
    DirectRender,
    CompilerBridge,
    VueInject,
    VueModule
}

/// <summary>Describes which source identity a final diagnostic can safely expose to the author.</summary>
internal enum RazorVueDiagnosticSourceKind
{
    None = 0,
    MappedRazor,
    AuthoredCSharp,
    GeneratedCSharp
}

/// <summary>
/// Typed diagnostic carrier used between RazorVue final-compilation stages.
/// It deliberately preserves category, source and component identity before Roslyn reporting
/// turns it into a <see cref="Diagnostic"/>.
/// 在 hook 之前不能压缩为 string；否则多个组件和 mapped Razor 位置都会丢失。
/// </summary>
internal sealed record RazorVueDiagnosticInfo(
    RazorVueDiagnosticCategory Category,
    string MessageKey,
    ImmutableArray<string> MessageArguments,
    DiagnosticSeverity Severity,
    Location PrimaryLocation,
    ImmutableArray<Location> AdditionalLocations,
    RazorVueDiagnosticSourceKind SourceKind,
    string? ComponentId,
    string? Subject,
    string? HelpLinkKey,
    bool IsAuthorReachable)
{
    /// <summary>Current descriptors carry one stable detail argument; keep formatting outside transport.</summary>
    public string Message => MessageArguments.IsDefaultOrEmpty
        ? string.Empty
        : MessageArguments[0];

    public RazorVueDiagnosticInfo WithComponent(INamedTypeSymbol? component)
    {
        if (component is null)
            return this;

        // Component fallback can turn Location.None into a generated or mapped Razor location.
        // SourceKind must describe the resolved location, not the pre-enrichment record value.
        // 补齐 component 位置后必须同步重算来源类型，否则诊断导航和分类会发生漂移。
        // A mapped Razor location is an external Location carrier (`IsInSource == false`). It is
        // already author-facing, so only resolve a component fallback when the diagnostic truly
        // has no location. Re-normalizing it would discard the mapped path.
        var resolvedLocation = PrimaryLocation == Location.None
            ? RazorVueDiagnosticFactory.PreferLocation(
                PrimaryLocation,
                RazorVueDiagnosticFactory.GetSymbolLocation(component))
            : PrimaryLocation;
        return this with
        {
            ComponentId = string.IsNullOrEmpty(ComponentId)
                ? component.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : ComponentId,
            PrimaryLocation = resolvedLocation,
            SourceKind = RazorVueDiagnosticFactory.GetSourceKind(resolvedLocation)
        };
    }
}

/// <summary>
/// Converts source-bound failures into final-pipeline diagnostics without parsing exception text
/// or <see cref="Exception.Data"/>. The legacy Data values stay inside Jazor.Compiler only for
/// compatibility with existing consumers.
/// </summary>
internal static class RazorVueDiagnosticFactory
{
    internal static RazorVueDiagnosticInfo Create(
        RazorVueDiagnosticCategory category,
        string message,
        Location? primaryLocation = null,
        INamedTypeSymbol? component = null,
        ISymbol? subject = null,
        ImmutableArray<Location> additionalLocations = default,
        bool isAuthorReachable = true,
        string? helpLinkKey = null)
    {
        var normalizedPrimary = ToAuthorLocation(primaryLocation);
        var normalizedAdditional = NormalizeLocations(additionalLocations);
        if (normalizedPrimary == Location.None && subject is not null)
            normalizedPrimary = GetSymbolLocation(subject);
        if (normalizedPrimary == Location.None && component is not null)
            normalizedPrimary = GetSymbolLocation(component);

        return new RazorVueDiagnosticInfo(
            category,
            MessageKey: GetMessageKey(category),
            MessageArguments: ImmutableArray.Create(message),
            Severity: DiagnosticSeverity.Error,
            PrimaryLocation: normalizedPrimary,
            AdditionalLocations: normalizedAdditional,
            SourceKind: GetSourceKind(normalizedPrimary),
            ComponentId: component?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Subject: subject?.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            HelpLinkKey: helpLinkKey ?? GetHelpLinkKey(category),
            IsAuthorReachable: isAuthorReachable);
    }

    internal static RazorVueDiagnosticInfo FromException(
        Exception exception,
        RazorVueDiagnosticCategory fallbackCategory,
        INamedTypeSymbol? component = null,
        ISymbol? subject = null)
    {
        if (exception is RazorVueDiagnosticException diagnosticException)
            return diagnosticException.Diagnostic.WithComponent(component);

        var location = exception switch
        {
            OperationTransformationException operation => operation.SourceLocation,
            SyntaxNodeTransformationException syntax => syntax.SourceLocation,
            SymbolTransformationException symbol => symbol.SourceLocation,
            _ => Location.None
        };
        var category = exception is OperationTransformationException or
            SyntaxNodeTransformationException or
            SymbolTransformationException
            ? RazorVueDiagnosticCategory.CompilerBridge
            : fallbackCategory;
        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? "No diagnostic detail was provided."
            : exception.Message;
        return Create(
            category,
            message,
            location,
            component,
            subject,
            isAuthorReachable: category != RazorVueDiagnosticCategory.Internal);
    }

    internal static Location GetSymbolLocation(ISymbol? symbol)
    {
        if (symbol is null)
            return Location.None;

        var location = symbol.Locations
            .Where(static candidate => candidate.IsInSource)
            .OrderBy(static candidate => candidate.SourceTree?.FilePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static candidate => candidate.SourceSpan.Start)
            .FirstOrDefault();
        return ToAuthorLocation(location);
    }

    internal static Location PreferLocation(Location? primary, Location? fallback)
    {
        var normalizedPrimary = ToAuthorLocation(primary);
        return normalizedPrimary != Location.None
            ? normalizedPrimary
            : ToAuthorLocation(fallback);
    }

    internal static Location ToAuthorLocation(Location? location)
    {
        if (location is null || !location.IsInSource)
            return Location.None;

        var mapped = location.GetMappedLineSpan();
        if (!mapped.HasMappedPath || string.IsNullOrWhiteSpace(mapped.Path))
            return location;

        // An external location deliberately stores Razor's mapped line/column rather than the
        // generated-tree offset. This makes compiler diagnostics and direct test inspection use
        // the same author-facing span even when the generated tree is not retained by the host.
        // SourceSpan is an opaque carrier here; user-facing navigation uses LinePositionSpan.
        return Location.Create(
            mapped.Path,
            location.SourceSpan,
            new LinePositionSpan(mapped.StartLinePosition, mapped.EndLinePosition));
    }

    private static ImmutableArray<Location> NormalizeLocations(ImmutableArray<Location> locations)
    {
        if (locations.IsDefaultOrEmpty)
            return ImmutableArray<Location>.Empty;

        return locations
            .Select(ToAuthorLocation)
            .Where(static location => location != Location.None)
            .Distinct(LocationComparer.Instance)
            .ToImmutableArray();
    }

    internal static RazorVueDiagnosticSourceKind GetSourceKind(Location location)
    {
        if (location == Location.None)
            return RazorVueDiagnosticSourceKind.None;

        var path = location.GetLineSpan().Path ?? location.SourceTree?.FilePath ?? string.Empty;
        if (path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            return RazorVueDiagnosticSourceKind.MappedRazor;
        if (path.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase))
        {
            return RazorVueDiagnosticSourceKind.GeneratedCSharp;
        }

        return RazorVueDiagnosticSourceKind.AuthoredCSharp;
    }

    private static string GetMessageKey(RazorVueDiagnosticCategory category)
        => "RazorVue." + category;

    private static string GetHelpLinkKey(RazorVueDiagnosticCategory category)
        => category switch
        {
            RazorVueDiagnosticCategory.DirectRender => "direct-render",
            RazorVueDiagnosticCategory.CompilerBridge => "compiler-boundary",
            RazorVueDiagnosticCategory.ComponentBinding => "component-binding",
            RazorVueDiagnosticCategory.MemberClosure => "member-closure",
            RazorVueDiagnosticCategory.VueInject => "vue-inject",
            RazorVueDiagnosticCategory.VueModule => "vue-module",
            _ => "final-compilation"
        };

    private sealed class LocationComparer : IEqualityComparer<Location>
    {
        internal static LocationComparer Instance { get; } = new();

        public bool Equals(Location? x, Location? y)
            => ReferenceEquals(x, y) ||
               (x is not null &&
                y is not null &&
                x.SourceSpan.Equals(y.SourceSpan) &&
                string.Equals(
                    x.GetLineSpan().Path ?? x.SourceTree?.FilePath,
                    y.GetLineSpan().Path ?? y.SourceTree?.FilePath,
                    StringComparison.OrdinalIgnoreCase) &&
                x.GetLineSpan().Span.Equals(y.GetLineSpan().Span));

        public int GetHashCode(Location location)
        {
            var span = location.GetLineSpan();
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(
                    span.Path ?? location.SourceTree?.FilePath ?? string.Empty);
                hash = (hash * 397) ^ location.SourceSpan.GetHashCode();
                return (hash * 397) ^ span.Span.GetHashCode();
            }
        }
    }
}

/// <summary>Preserves a classified RazorVue failure while it crosses exception-based APIs.</summary>
internal sealed class RazorVueDiagnosticException : InvalidOperationException
{
    internal RazorVueDiagnosticException(RazorVueDiagnosticInfo diagnostic)
        : base(diagnostic.Message)
    {
        Diagnostic = diagnostic;
    }

    internal RazorVueDiagnosticException(RazorVueDiagnosticInfo diagnostic, Exception innerException)
        : base(diagnostic.Message, innerException)
    {
        Diagnostic = diagnostic;
    }

    internal RazorVueDiagnosticInfo Diagnostic { get; }
}
