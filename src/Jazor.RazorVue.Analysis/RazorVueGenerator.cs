using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Analysis;

// Thin host rule: this generator owns Roslyn wiring and diagnostics only; RazorVue semantics live in Jazor.RazorVue.
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor RazorVueGenerationFailed = new(
        id: "JAZORVGA001",
        title: "RazorVue catalog generation failed",
        messageFormat: "Failed to generate RazorVue catalog for '{0}': {1}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueComponentNotFound = new(
        id: "JAZORVGA002",
        title: "RazorVue component not found",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueAmbiguousComponentName = new(
        id: "JAZORVGA003",
        title: "RazorVue component name is ambiguous",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueReservedIntrinsicNameCollision = new(
        id: "JAZORVGA004",
        title: "RazorVue component name collides with intrinsic",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueUnsupportedLifecycleLowering = new(
        id: "JAZORVGA005",
        title: "RazorVue lifecycle lowering is unsupported",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueUnsupportedSetupLogicLowering = new(
        id: "JAZORVGA006",
        title: "RazorVue setup logic lowering is unsupported",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueUnknownParameter = new(
        id: "JAZORVGA007",
        title: "RazorVue parameter is unknown",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidBindTarget = new(
        id: "JAZORVGA008",
        title: "RazorVue bind target is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueUnknownSlot = new(
        id: "JAZORVGA009",
        title: "RazorVue child content parameter is unknown",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueSlotContextMisuse = new(
        id: "JAZORVGA010",
        title: "RazorVue child content parameter context is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueDuplicateSlotValue = new(
        id: "JAZORVGA011",
        title: "RazorVue child content parameter is assigned multiple times",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidLibraryComponentDeclaration = new(
        id: "JAZORVGA012",
        title: "RazorVue library component declaration is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidLibraryStyleDependencyDeclaration = new(
        id: "JAZORVGA013",
        title: "RazorVue library style dependency declaration is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidLibraryPluginRequirementDeclaration = new(
        id: "JAZORVGA014",
        title: "RazorVue library plugin requirement declaration is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var componentCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "ECMAScript.ECMAScriptModuleAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (syntaxContext, _) => CreateCandidate(syntaxContext))
            .Where(static candidate => candidate is not null);

        var combined = context.CompilationProvider.Combine(componentCandidates.Collect());
        context.RegisterSourceOutput(combined, static (outputContext, source) =>
        {
            var (compilation, candidates) = source;
            EmitRazorVueCatalog(outputContext, compilation, candidates);
        });
    }

    private static ModuleCandidate? CreateCandidate(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ClassDeclarationSyntax)
            return null;

        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        return new ModuleCandidate(classSymbol, context.TargetNode.GetLocation());
    }

    private static void EmitRazorVueCatalog(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<ModuleCandidate?> candidates)
    {
        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        if (razorVueContext is null)
            return;

        if (!candidates.Any(static candidate => candidate is not null))
            return;

        var candidate = candidates.FirstOrDefault(static candidate => candidate is not null);

        try
        {
            // Keep generator diagnostics aligned with the analyzer by validating
            // descriptor-only library stubs before any consuming component resolves them.
            _ = razorVueContext.DiscoverLibraryComponents();
            var catalog = new RazorVuePipeline().Execute(compilation);
            if (catalog.Artifacts.IsDefaultOrEmpty)
                return;

            context.AddSource("Jazor.Generated.RazorVueCatalog.g.cs", BuildRazorVueCatalogSource(catalog));
        }
        catch (RazorVueCompilationIssueException issueException)
        {
            context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException, candidate));
        }
        catch (NotSupportedException ex) when (TryCreateUnsupportedSetupLogicIssueException(ex, candidate, out var issueException))
        {
            context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException, candidate));
        }
        catch (global::System.Exception ex)
        {
            var location = candidate?.Location ?? Location.None;
            var typeName = candidate?.ClassSymbol.ToDisplayString() ?? (compilation.AssemblyName ?? "Jazor.Assembly");
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueGenerationFailed,
                location,
                typeName,
                ex.Message));
        }
    }

    private static Diagnostic CreateCompilationIssueDiagnostic(
        RazorVueCompilationIssueException issueException,
        ModuleCandidate? candidate)
    {
        var descriptor = issueException.Issue.Code switch
        {
            RazorVueIssueCode.ComponentNotFound => RazorVueComponentNotFound,
            RazorVueIssueCode.AmbiguousComponentName => RazorVueAmbiguousComponentName,
            RazorVueIssueCode.ReservedIntrinsicNameCollision => RazorVueReservedIntrinsicNameCollision,
            RazorVueIssueCode.UnsupportedLifecycleLowering => RazorVueUnsupportedLifecycleLowering,
            RazorVueIssueCode.UnsupportedSetupLogicLowering => RazorVueUnsupportedSetupLogicLowering,
            RazorVueIssueCode.InvalidLibraryComponentDeclaration => RazorVueInvalidLibraryComponentDeclaration,
            RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration => RazorVueInvalidLibraryStyleDependencyDeclaration,
            RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration => RazorVueInvalidLibraryPluginRequirementDeclaration,
            RazorVueIssueCode.UnknownParameter => RazorVueUnknownParameter,
            RazorVueIssueCode.InvalidBindTarget => RazorVueInvalidBindTarget,
            RazorVueIssueCode.UnknownSlot => RazorVueUnknownSlot,
            RazorVueIssueCode.SlotContextMisuse => RazorVueSlotContextMisuse,
            RazorVueIssueCode.DuplicateSlotValue => RazorVueDuplicateSlotValue,
            _ => RazorVueGenerationFailed
        };
        var location = TryCreateLocation(issueException.Origin) ?? candidate?.Location ?? Location.None;
        return Diagnostic.Create(descriptor, location, issueException.Issue.Message);
    }

    private static bool TryCreateUnsupportedSetupLogicIssueException(
        NotSupportedException exception,
        ModuleCandidate? candidate,
        out RazorVueCompilationIssueException issueException)
    {
        issueException = null!;
        if (candidate?.ClassSymbol is null)
            return false;

        var message = exception.Message;
        if (string.IsNullOrWhiteSpace(message) || !message.Contains("component method", StringComparison.Ordinal))
            return false;

        var methodName = ExtractQuotedIdentifier(message);
        if (string.IsNullOrWhiteSpace(methodName))
            return false;

        var method = candidate.ClassSymbol.GetMembers(methodName!)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(static member => !member.IsStatic);
        if (method is null)
            return false;

        var originLocation = method.Locations.FirstOrDefault(static location => location.IsInSource);
        var origin = originLocation is null
            ? null
            : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedSetupLogicLowering,
            RazorVueIssueSeverity.Error,
            $"RazorVue setup lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.",
            ImmutableArray<string>.Empty);
        issueException = new RazorVueCompilationIssueException(issue, method.ContainingType.ToDisplayString(), origin);
        return true;
    }

    private static string? ExtractQuotedIdentifier(string message)
    {
        var start = message.IndexOf("'", StringComparison.Ordinal);
        if (start < 0)
            return null;

        var end = message.IndexOf("'", start + 1, StringComparison.Ordinal);
        if (end <= start)
            return null;

        return message.Substring(start + 1, end - start - 1);
    }

    private static Location? TryCreateLocation(RazorVueSourceOrigin? origin)
    {
        if (origin is null || string.IsNullOrWhiteSpace(origin.SourceFilePath))
            return null;

        var startLine = Math.Max(origin.StartLine - 1, 0);
        var startColumn = Math.Max(origin.StartColumn - 1, 0);
        var start = new LinePosition(startLine, startColumn);
        var end = new LinePosition(startLine, startColumn + Math.Max(origin.SourceSpanLength, 1));
        return Location.Create(
            origin.SourceFilePath,
            new TextSpan(Math.Max(origin.SourceSpanStart, 0), Math.Max(origin.SourceSpanLength, 0)),
            new LinePositionSpan(start, end));
    }

    private static string BuildRazorVueCatalogSource(RazorVueCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    public static partial class RazorVueCatalog");
        builder.AppendLine("    {");
        builder.Append("        public static string AssemblyName { get; } = ");
        builder.Append(EscapeCSharpString(catalog.AssemblyName));
        builder.AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("        public static global::System.Collections.IEnumerable GetArtifacts()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _artifacts;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedArtifact");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedArtifact(string componentName, string relativeModulePath, string moduleCode, string[] imports, string[] styles, string[] pluginRequirements, GeneratedIdentity identity, GeneratedHints hints, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                ComponentName = componentName;");
        builder.AppendLine("                RelativeModulePath = relativeModulePath;");
        builder.AppendLine("                ModuleCode = moduleCode;");
        builder.AppendLine("                Imports = imports;");
        builder.AppendLine("                Styles = styles;");
        builder.AppendLine("                PluginRequirements = pluginRequirements;");
        builder.AppendLine("                Identity = identity;");
        builder.AppendLine("                Hints = hints;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string ComponentName { get; }");
        builder.AppendLine("            public string RelativeModulePath { get; }");
        builder.AppendLine("            public string ModuleCode { get; }");
        builder.AppendLine("            public string[] Imports { get; }");
        builder.AppendLine("            public string[] Styles { get; }");
        builder.AppendLine("            public string[] PluginRequirements { get; }");
        builder.AppendLine("            public GeneratedIdentity Identity { get; }");
        builder.AppendLine("            public GeneratedHints Hints { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedIdentity");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedIdentity(string componentId, string moduleId, string descriptorHash, string templateHash, string logicHash, GeneratedHmrBoundaryKind hmrBoundaryKind)");
        builder.AppendLine("            {");
        builder.AppendLine("                ComponentId = componentId;");
        builder.AppendLine("                ModuleId = moduleId;");
        builder.AppendLine("                DescriptorHash = descriptorHash;");
        builder.AppendLine("                TemplateHash = templateHash;");
        builder.AppendLine("                LogicHash = logicHash;");
        builder.AppendLine("                HmrBoundaryKind = hmrBoundaryKind;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string ComponentId { get; }");
        builder.AppendLine("            public string ModuleId { get; }");
        builder.AppendLine("            public string DescriptorHash { get; }");
        builder.AppendLine("            public string TemplateHash { get; }");
        builder.AppendLine("            public string LogicHash { get; }");
        builder.AppendLine("            public GeneratedHmrBoundaryKind HmrBoundaryKind { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedHints");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedHints(bool requiresVueRuntime, bool requiresHydration, bool supportsSsr, bool usesTeleport, bool usesSuspense, bool usesKeepAlive)");
        builder.AppendLine("            {");
        builder.AppendLine("                RequiresVueRuntime = requiresVueRuntime;");
        builder.AppendLine("                RequiresHydration = requiresHydration;");
        builder.AppendLine("                SupportsSsr = supportsSsr;");
        builder.AppendLine("                UsesTeleport = usesTeleport;");
        builder.AppendLine("                UsesSuspense = usesSuspense;");
        builder.AppendLine("                UsesKeepAlive = usesKeepAlive;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public bool RequiresVueRuntime { get; }");
        builder.AppendLine("            public bool RequiresHydration { get; }");
        builder.AppendLine("            public bool SupportsSsr { get; }");
        builder.AppendLine("            public bool UsesTeleport { get; }");
        builder.AppendLine("            public bool UsesSuspense { get; }");
        builder.AppendLine("            public bool UsesKeepAlive { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedOrigin");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedOrigin(string sourceFilePath, int sourceSpanStart, int sourceSpanLength, string? generatedFilePath, int? generatedSpanStart, int? generatedSpanLength, int startLine, int startColumn, GeneratedMappingQuality mappingQuality, GeneratedOriginProvenance provenance)");
        builder.AppendLine("            {");
        builder.AppendLine("                SourceFilePath = sourceFilePath;");
        builder.AppendLine("                SourceSpanStart = sourceSpanStart;");
        builder.AppendLine("                SourceSpanLength = sourceSpanLength;");
        builder.AppendLine("                GeneratedFilePath = generatedFilePath;");
        builder.AppendLine("                GeneratedSpanStart = generatedSpanStart;");
        builder.AppendLine("                GeneratedSpanLength = generatedSpanLength;");
        builder.AppendLine("                StartLine = startLine;");
        builder.AppendLine("                StartColumn = startColumn;");
        builder.AppendLine("                MappingQuality = mappingQuality;");
        builder.AppendLine("                Provenance = provenance;");
        builder.AppendLine("            }");
        builder.AppendLine("            public string SourceFilePath { get; }");
        builder.AppendLine("            public int SourceSpanStart { get; }");
        builder.AppendLine("            public int SourceSpanLength { get; }");
        builder.AppendLine("            public string? GeneratedFilePath { get; }");
        builder.AppendLine("            public int? GeneratedSpanStart { get; }");
        builder.AppendLine("            public int? GeneratedSpanLength { get; }");
        builder.AppendLine("            public int StartLine { get; }");
        builder.AppendLine("            public int StartColumn { get; }");
        builder.AppendLine("            public GeneratedMappingQuality MappingQuality { get; }");
        builder.AppendLine("            public GeneratedOriginProvenance Provenance { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private enum GeneratedHmrBoundaryKind");
        builder.AppendLine("        {");
        builder.AppendLine("            Unknown,");
        builder.AppendLine("            TemplateOnly,");
        builder.AppendLine("            LogicSafe,");
        builder.AppendLine("            FullReloadRequired");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private enum GeneratedMappingQuality");
        builder.AppendLine("        {");
        builder.AppendLine("            ExactSource,");
        builder.AppendLine("            MappedFromGenerated,");
        builder.AppendLine("            GeneratedOnly");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private enum GeneratedOriginProvenance");
        builder.AppendLine("        {");
        builder.AppendLine("            RazorSourceMap,");
        builder.AppendLine("            GeneratedSyntaxLocation,");
        builder.AppendLine("            GeneratedFallback");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private static readonly GeneratedArtifact[] _artifacts = new GeneratedArtifact[]");
        builder.AppendLine("        {");

        foreach (var artifact in catalog.Artifacts)
        {
            builder.AppendLine("            new GeneratedArtifact(");
            builder.Append("                componentName: ").Append(EscapeCSharpString(artifact.ComponentName)).AppendLine(",");
            builder.Append("                relativeModulePath: ").Append(EscapeCSharpString(artifact.RelativeModulePath)).AppendLine(",");
            builder.Append("                moduleCode: ").Append(EscapeCSharpString(artifact.ModuleCode)).AppendLine(",");
            builder.Append("                imports: ").Append(BuildStringArrayLiteral(artifact.Imports)).AppendLine(",");
            builder.Append("                styles: ").Append(BuildStringArrayLiteral(artifact.Styles)).AppendLine(",");
            builder.Append("                pluginRequirements: ").Append(BuildStringArrayLiteral(artifact.PluginRequirements)).AppendLine(",");
            builder.AppendLine("                identity: new GeneratedIdentity(");
            builder.Append("                    componentId: ").Append(EscapeCSharpString(artifact.Identity.ComponentId)).AppendLine(",");
            builder.Append("                    moduleId: ").Append(EscapeCSharpString(artifact.Identity.ModuleId)).AppendLine(",");
            builder.Append("                    descriptorHash: ").Append(EscapeCSharpString(artifact.Identity.DescriptorHash)).AppendLine(",");
            builder.Append("                    templateHash: ").Append(EscapeCSharpString(artifact.Identity.TemplateHash)).AppendLine(",");
            builder.Append("                    logicHash: ").Append(EscapeCSharpString(artifact.Identity.LogicHash)).AppendLine(",");
            builder.Append("                    hmrBoundaryKind: GeneratedHmrBoundaryKind.").Append(artifact.Identity.HmrBoundaryKind).AppendLine("),");
            builder.AppendLine("                hints: new GeneratedHints(");
            builder.Append("                    requiresVueRuntime: ").Append(ToCSharpBool(artifact.Hints.RequiresVueRuntime)).AppendLine(",");
            builder.Append("                    requiresHydration: ").Append(ToCSharpBool(artifact.Hints.RequiresHydration)).AppendLine(",");
            builder.Append("                    supportsSsr: ").Append(ToCSharpBool(artifact.Hints.SupportsSsr)).AppendLine(",");
            builder.Append("                    usesTeleport: ").Append(ToCSharpBool(artifact.Hints.UsesTeleport)).AppendLine(",");
            builder.Append("                    usesSuspense: ").Append(ToCSharpBool(artifact.Hints.UsesSuspense)).AppendLine(",");
            builder.Append("                    usesKeepAlive: ").Append(ToCSharpBool(artifact.Hints.UsesKeepAlive)).AppendLine("),");
            builder.Append("                sourceOrigins: ").Append(BuildOriginsArrayLiteral(artifact.SourceOrigins)).AppendLine("),");
        }

        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string BuildOriginsArrayLiteral(ImmutableArray<RazorVueSourceOrigin> origins)
    {
        if (origins.IsDefaultOrEmpty)
            return "new GeneratedOrigin[0]";

        var builder = new StringBuilder();
        builder.AppendLine("new GeneratedOrigin[]");
        builder.AppendLine("                {");
        foreach (var origin in origins)
        {
            builder.AppendLine("                    new GeneratedOrigin(");
            builder.Append("                        sourceFilePath: ").Append(EscapeCSharpString(origin.SourceFilePath)).AppendLine(",");
            builder.Append("                        sourceSpanStart: ").Append(origin.SourceSpanStart).AppendLine(",");
            builder.Append("                        sourceSpanLength: ").Append(origin.SourceSpanLength).AppendLine(",");
            builder.Append("                        generatedFilePath: ").Append(EscapeNullableCSharpString(origin.GeneratedFilePath)).AppendLine(",");
            builder.Append("                        generatedSpanStart: ").Append(ToNullableCSharpInt(origin.GeneratedSpanStart)).AppendLine(",");
            builder.Append("                        generatedSpanLength: ").Append(ToNullableCSharpInt(origin.GeneratedSpanLength)).AppendLine(",");
            builder.Append("                        startLine: ").Append(origin.StartLine).AppendLine(",");
            builder.Append("                        startColumn: ").Append(origin.StartColumn).AppendLine(",");
            builder.Append("                        mappingQuality: GeneratedMappingQuality.").Append(origin.MappingQuality).AppendLine(",");
            builder.Append("                        provenance: GeneratedOriginProvenance.").Append(origin.Provenance).AppendLine("),");
        }

        builder.Append("                }");
        return builder.ToString();
    }

    private static string BuildStringArrayLiteral(ImmutableArray<string> values)
    {
        if (values.IsDefaultOrEmpty)
            return "new string[0]";

        var builder = new StringBuilder("new string[] { ");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
                builder.Append(", ");
            builder.Append(EscapeCSharpString(values[i]));
        }

        builder.Append(" }");
        return builder.ToString();
    }

    private static string ToCSharpBool(bool value)
        => value ? "true" : "false";

    private static string ToNullableCSharpInt(int? value)
        => value?.ToString() ?? "null";

    private static string EscapeNullableCSharpString(string? value)
        => value is null ? "null" : EscapeCSharpString(value);

    private static string EscapeCSharpString(string value)
    {
        var builder = new StringBuilder((value ?? string.Empty).Length + 2);
        builder.Append('"');
        foreach (var ch in value ?? string.Empty)
        {
            builder.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ => ch.ToString()
            });
        }

        builder.Append('"');
        return builder.ToString();
    }

    private sealed record ModuleCandidate(
        INamedTypeSymbol ClassSymbol,
        Location Location);
}

