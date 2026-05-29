using Jazor.Analyzer.RazorVue.Generation;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Jazor.RazorVue.Analysis;

// Thin host rule: this generator owns Roslyn wiring and diagnostics only; RazorVue semantics live in Jazor.RazorVue.
[Generator]
public sealed class RazorVueGenerator : IIncrementalGenerator
{
    private const string RazorVueOutputModePropertyName = "build_property.JazorRazorVueOutputMode";
    private const string RazorVueEnableRazorSgIntegrationPropertyName = "build_property.JazorRazorVueEnableRazorSgIntegration";
    private readonly Func<RazorVuePipeline> _legacyPipelineFactory;
    private readonly Func<RazorVueSfcPipeline> _sfcPipelineFactory;
    private readonly Func<RazorSourceGeneratorCompatibilityProbeResult> _compatibilityProbeFactory;
    private readonly Func<object?, RazorSourceGeneratorBootstrapTrace> _bootstrapTraceFactory;

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

    private static readonly DiagnosticDescriptor RazorVueMissingSlotValue = new(
        id: "JAZORVGA015",
        title: "RazorVue child content parameter value is missing",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidOutputMode = new(
        id: "JAZORVGA016",
        title: "RazorVue output mode is invalid",
        messageFormat: "Unsupported RazorVue output mode '{0}'. Supported values: legacy, sfc.",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidComponentDeclaration = new(
        id: "JAZORVGA017",
        title: "RazorVue component declaration is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueInvalidContainerInjectDeclaration = new(
        id: "JAZORVGA021",
        title: "RazorVue container inject declaration is invalid",
        messageFormat: "{0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueRazorSgIntegrationNotActive = new(
        id: "JAZORVGA018",
        title: "RazorVue Razor SG integration is not active",
        messageFormat: "RazorVue Razor SG integration is enabled, but no RazorVue tail output was produced for Razor component '{0}'. The official Razor source generator did not run with RazorVue tail injection.",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RazorVueRazorSgIntegrationIncompatible = new(
        id: "JAZORVGA019",
        title: "RazorVue Razor SG integration is incompatible with the current SDK",
        messageFormat: "RazorVue Razor SG integration is enabled, but the current Razor source generator shape is unsupported: {0}",
        category: "Jazor.RazorVue.Analysis",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public RazorVueGenerator()
        : this(
            static () => new RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, RazorVueLegacyIrFirstTemplateFrontend.Instance),
            static () => new RazorVueSfcPipeline(
                RazorVueRazorDocumentSemanticFrontend.Instance,
                new RazorVueBaselineFirstTemplateFrontend(
                    BuildRenderTreeTemplateFrontend.Instance,
                    new RazorVueRazorIrTemplateFrontend())),
            static () => RazorSourceGeneratorCompatibilityProbe.CollectCurrent(),
            static contextKey => RazorSourceGeneratorBootstrapState.CreateTrace(contextKey))
    {
    }

    internal RazorVueGenerator(
        Func<RazorVuePipeline> legacyPipelineFactory,
        Func<RazorVueSfcPipeline> sfcPipelineFactory,
        Func<RazorSourceGeneratorCompatibilityProbeResult> compatibilityProbeFactory)
        : this(
            legacyPipelineFactory,
            sfcPipelineFactory,
            compatibilityProbeFactory,
            static contextKey => RazorSourceGeneratorBootstrapState.CreateTrace(contextKey))
    {
    }

    internal RazorVueGenerator(
        Func<RazorVuePipeline> legacyPipelineFactory,
        Func<RazorVueSfcPipeline> sfcPipelineFactory,
        Func<RazorSourceGeneratorCompatibilityProbeResult> compatibilityProbeFactory,
        Func<object?, RazorSourceGeneratorBootstrapTrace> bootstrapTraceFactory)
    {
        _legacyPipelineFactory = legacyPipelineFactory ?? throw new ArgumentNullException(nameof(legacyPipelineFactory));
        _sfcPipelineFactory = sfcPipelineFactory ?? throw new ArgumentNullException(nameof(sfcPipelineFactory));
        _compatibilityProbeFactory = compatibilityProbeFactory ?? throw new ArgumentNullException(nameof(compatibilityProbeFactory));
        _bootstrapTraceFactory = bootstrapTraceFactory ?? throw new ArgumentNullException(nameof(bootstrapTraceFactory));
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        RazorSourceGeneratorBootstrap.Initialize();
        var tailOutputRegistrationVersionBeforeInitialize = RazorSourceGeneratorBootstrapState.GetTailOutputRegistrationVersion();
        RazorSourceGeneratorFallbackOutput.Register(context);
        var contextKey = RazorSourceGeneratorInitializationContextState.GetContextKey(context);
        var bootstrapTraceFactory = _bootstrapTraceFactory;

        var testHookEnabled = context.AnalyzerConfigOptionsProvider.Select(
            static (optionsProvider, _) => RazorSourceGeneratorHostOutputHookOptions.IsTestHookEnabled(optionsProvider));
        var bootstrapTrace = context.CompilationProvider.Select(
            (_, _) =>
            {
                var trace = bootstrapTraceFactory(contextKey);
                if (!RazorSourceGeneratorBootstrapState.HasTailOutputRegistrationAfter(tailOutputRegistrationVersionBeforeInitialize))
                    return trace;

                return trace with
                {
                    TailOutputRegistered = true,
                    TailOutputRegisteredForCurrentContext = true
                };
            });
        context.RegisterSourceOutput(
            testHookEnabled.Combine(bootstrapTrace),
            static (outputContext, input) =>
        {
            var (enabled, trace) = input;
            if (!enabled)
                return;

            outputContext.AddSource(
                "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs",
                BuildRazorSgBootstrapTraceSource(trace));
        });

        var componentCandidates = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "ECMAScript.ECMAScriptModuleAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (syntaxContext, _) => CreateCandidate(syntaxContext))
            .Where(static candidate => candidate is not null);

        var generatorOptions = context.AnalyzerConfigOptionsProvider
            .Select(static (optionsProvider, _) => RazorVueGeneratorOptions.Create(optionsProvider.GlobalOptions));

        var combined = context.CompilationProvider
            .Combine(componentCandidates.Collect())
            .Combine(generatorOptions);
        var combinedWithBootstrap = combined.Combine(bootstrapTrace);

        var legacyPipelineFactory = _legacyPipelineFactory;
        var sfcPipelineFactory = _sfcPipelineFactory;
        var compatibilityProbeFactory = _compatibilityProbeFactory;

        context.RegisterSourceOutput(combinedWithBootstrap, (outputContext, source) =>
        {
            var (((compilation, candidates), generatorOptions), trace) = source;
            EmitRazorVueCatalog(
                outputContext,
                compilation,
                candidates,
                generatorOptions,
                legacyPipelineFactory,
                sfcPipelineFactory,
                compatibilityProbeFactory,
                trace);
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
        ImmutableArray<ModuleCandidate?> candidates,
        RazorVueGeneratorOptions generatorOptions,
        Func<RazorVuePipeline> legacyPipelineFactory,
        Func<RazorVueSfcPipeline> sfcPipelineFactory,
        Func<RazorSourceGeneratorCompatibilityProbeResult> compatibilityProbeFactory,
        RazorSourceGeneratorBootstrapTrace bootstrapTrace)
    {
        if (!candidates.Any(static candidate => candidate is not null))
            return;

        var razorVueContext = RazorVueCompilationContext.TryCreate(compilation);
        if (razorVueContext is null)
            return;

        var candidate = candidates.FirstOrDefault(static candidate => candidate is not null);

        try
        {
            var normalGeneratorSnapshots = GetNormalGeneratorSnapshots(
                razorVueContext,
                generatorOptions.EnableRazorSgIntegration,
                ResolveOutputMode(generatorOptions.OutputModeText));

            if (generatorOptions.EnableRazorSgIntegration &&
                normalGeneratorSnapshots.IsDefaultOrEmpty &&
                TryCreateRazorSgIntegrationDiagnostic(
                    razorVueContext,
                    candidate,
                    compatibilityProbeFactory,
                    bootstrapTrace,
                    out var integrationDiagnostic))
            {
                context.ReportDiagnostic(integrationDiagnostic);
                return;
            }

            if (normalGeneratorSnapshots.IsDefaultOrEmpty)
                return;

            // Keep generator diagnostics aligned with the analyzer by validating
            // descriptor-only library stubs before any consuming component resolves them.
            _ = razorVueContext.DiscoverLibraryComponents();
            switch (ResolveOutputMode(generatorOptions.OutputModeText))
            {
                case RazorVueGeneratorOutputMode.Legacy:
                {
                    var catalog = legacyPipelineFactory().Execute(razorVueContext, normalGeneratorSnapshots);
                    if (catalog.Artifacts.IsDefaultOrEmpty)
                        return;

                    context.AddSource("Jazor.Generated.RazorVueCatalog.g.cs", BuildRazorVueCatalogSource(catalog));
                    return;
                }
                case RazorVueGeneratorOutputMode.Sfc:
                {
                    var catalog = generatorOptions.EnableRazorSgIntegration
                        ? sfcPipelineFactory().Execute(razorVueContext, normalGeneratorSnapshots)
                        : sfcPipelineFactory().Execute(razorVueContext);
                    if (catalog.Artifacts.IsDefaultOrEmpty)
                        return;

                    foreach (var artifact in catalog.Artifacts)
                    {
                        context.AddSource(
                            CreateRazorVueSfcArtifactHintName(artifact),
                            BuildRazorVueSfcArtifactSource(artifact));
                    }

                    context.AddSource("Jazor.Generated.RazorVueCatalog.g.cs", BuildRazorVueSfcCatalogSource(catalog));
                    return;
                }
                default:
                    throw new InvalidOperationException("Unhandled RazorVue output mode.");
            }
        }
        catch (RazorVueCompilationIssueException issueException)
        {
            context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException, candidate));
        }
        catch (InvalidRazorVueOutputModeException outputModeException)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                RazorVueInvalidOutputMode,
                candidate?.Location ?? Location.None,
                outputModeException.Mode));
        }
        catch (NotSupportedException ex) when (TryCreateUnsupportedSetupLogicIssueException(ex, candidate, out var issueException))
        {
            context.ReportDiagnostic(CreateCompilationIssueDiagnostic(issueException!, candidate));
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

    private static bool TryCreateRazorSgIntegrationDiagnostic(
        RazorVueCompilationContext context,
        ModuleCandidate? candidate,
        Func<RazorSourceGeneratorCompatibilityProbeResult> compatibilityProbeFactory,
        RazorSourceGeneratorBootstrapTrace bootstrapTrace,
        out Diagnostic diagnostic)
    {
        diagnostic = default!;
        var snapshots = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context);
        var tailRequiredSnapshots = snapshots
            .Where(IsRazorSgTailRequired)
            .ToImmutableArray();

        if (tailRequiredSnapshots.IsDefaultOrEmpty)
        {
            return false;
        }

        if (bootstrapTrace.PatchFailed || bootstrapTrace.PatchUnavailable)
        {
            var failure = string.IsNullOrWhiteSpace(bootstrapTrace.Failure)
                ? (bootstrapTrace.PatchUnavailable
                    ? "RazorVue Razor SG hook backend is unavailable on the current platform."
                    : "RazorVue Razor SG bootstrap patch failed.")
                : bootstrapTrace.Failure!;
            if (RazorSourceGeneratorFallbackOutput.IsFallbackRequiredFailure(failure))
            {
                failure += " RazorVue does not run a private Razor source generator fallback inside the analyzer; .razor component SFC generation requires official Razor SG tail output after Razor IR and generated C# are available.";
            }
            diagnostic = Diagnostic.Create(
                RazorVueRazorSgIntegrationIncompatible,
                candidate?.Location ?? Location.None,
                failure);
            return true;
        }

        if (!bootstrapTrace.CurrentContextKeyAvailable)
        {
            diagnostic = Diagnostic.Create(
                RazorVueRazorSgIntegrationIncompatible,
                candidate?.Location ?? Location.None,
                "Roslyn IncrementalGeneratorInitializationContext output-node state could not be read; RazorVue cannot prove that the Razor SG tail output was registered for the current generator context.");
            return true;
        }

        if (bootstrapTrace.TailOutputRegisteredForCurrentContext ||
            bootstrapTrace.TailOutputRegistered)
            return false;

        var compatibilityProbe = compatibilityProbeFactory();
        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(
            compatibilityProbe ?? throw new InvalidOperationException("Razor source generator compatibility probe was not loaded."));
        if (!validation.Success)
        {
            diagnostic = Diagnostic.Create(
                RazorVueRazorSgIntegrationIncompatible,
                candidate?.Location ?? Location.None,
                validation.Failure ?? "Unknown Razor source generator compatibility failure.");
            return true;
        }

        diagnostic = Diagnostic.Create(
            RazorVueRazorSgIntegrationNotActive,
            candidate?.Location ?? Location.None,
            DescribeTailRequiredComponents(tailRequiredSnapshots));
        return true;
    }

    private static string DescribeTailRequiredComponents(ImmutableArray<RazorVueSemanticSnapshot> snapshots)
        => snapshots.IsDefaultOrEmpty
            ? "<unknown>"
            : string.Join(
                ", ",
                snapshots
                    .Select(static snapshot => snapshot.Descriptor.FullName)
                    .OrderBy(static name => name, StringComparer.Ordinal));

    internal static bool IsRazorSgTailRequired(RazorVueSemanticSnapshot snapshot)
        => snapshot.RazorIrCarrier is null &&
           snapshot.RazorSourceGeneratorDocument is null &&
           (snapshot.BuildRenderTreeMethod is null ||
            !RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(snapshot)) &&
           IsLikelyRazorAuthoredComponentSnapshot(snapshot);

    private static bool IsLikelyRazorAuthoredComponentSnapshot(RazorVueSemanticSnapshot snapshot)
        => HasRazorSourceIdentity(snapshot.ComponentSymbol) ||
           (snapshot.BuildRenderTreeMethod is not null &&
            HasGeneratedRazorBuildRenderTreeSource(snapshot.BuildRenderTreeMethod));

    private static bool HasGeneratedRazorBuildRenderTreeSource(IMethodSymbol buildRenderTreeMethod)
    {
        foreach (var syntaxReference in buildRenderTreeMethod.DeclaringSyntaxReferences)
        {
            if (HasRazorSourcePath(syntaxReference.SyntaxTree.FilePath))
                return true;
        }

        foreach (var location in buildRenderTreeMethod.Locations)
        {
            if (!location.IsInSource)
                continue;

            var lineSpan = location.GetLineSpan();
            if (HasRazorSourcePath(lineSpan.Path))
                return true;

            var mappedLineSpan = location.GetMappedLineSpan();
            if (mappedLineSpan.HasMappedPath && HasRazorSourcePath(mappedLineSpan.Path))
                return true;
        }

        return false;
    }

    private static bool HasRazorSourceIdentity(INamedTypeSymbol componentSymbol)
    {
        foreach (var syntaxReference in componentSymbol.DeclaringSyntaxReferences)
        {
            if (HasRazorSourcePath(syntaxReference.SyntaxTree.FilePath))
                return true;
        }

        foreach (var location in componentSymbol.Locations)
        {
            if (!location.IsInSource)
                continue;

            var lineSpan = location.GetLineSpan();
            if (HasRazorSourcePath(lineSpan.Path))
                return true;

            var mappedLineSpan = location.GetMappedLineSpan();
            if (mappedLineSpan.HasMappedPath && HasRazorSourcePath(mappedLineSpan.Path))
                return true;
        }

        return false;
    }

    private static bool HasRazorSourcePath(string? path)
        => !string.IsNullOrWhiteSpace(path) &&
           (path!.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase));

    private static ImmutableArray<RazorVueSemanticSnapshot> GetNormalGeneratorSnapshots(
        RazorVueCompilationContext context,
        bool enableRazorSgIntegration,
        RazorVueGeneratorOutputMode outputMode)
    {
        var snapshots = RazorVueRazorDocumentSemanticFrontend.Instance.CreateSemanticSnapshots(context);
        if (snapshots.IsDefaultOrEmpty)
            return ImmutableArray<RazorVueSemanticSnapshot>.Empty;

        var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();
        foreach (var snapshot in snapshots)
        {
            if (snapshot.RazorSourceGeneratorDocument is not null)
                continue;

            if (snapshot.RazorIrCarrier is not null)
            {
                if (!enableRazorSgIntegration &&
                    outputMode == RazorVueGeneratorOutputMode.Legacy)
                {
                    builder.Add(snapshot);
                }

                continue;
            }

            if (IsRazorSgTailRequired(snapshot))
                continue;

            if (!enableRazorSgIntegration || RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(snapshot))
                builder.Add(snapshot);
        }

        return builder.ToImmutable();
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
            RazorVueIssueCode.InvalidComponentDeclaration => RazorVueInvalidComponentDeclaration,
            RazorVueIssueCode.InvalidLibraryComponentDeclaration => RazorVueInvalidLibraryComponentDeclaration,
            RazorVueIssueCode.InvalidLibraryStyleDependencyDeclaration => RazorVueInvalidLibraryStyleDependencyDeclaration,
            RazorVueIssueCode.InvalidLibraryPluginRequirementDeclaration => RazorVueInvalidLibraryPluginRequirementDeclaration,
            RazorVueIssueCode.InvalidContainerInjectDeclaration => RazorVueInvalidContainerInjectDeclaration,
            RazorVueIssueCode.UnknownParameter => RazorVueUnknownParameter,
            RazorVueIssueCode.InvalidBindTarget => RazorVueInvalidBindTarget,
            RazorVueIssueCode.UnknownSlot => RazorVueUnknownSlot,
            RazorVueIssueCode.SlotContextMisuse => RazorVueSlotContextMisuse,
            RazorVueIssueCode.DuplicateSlotValue => RazorVueDuplicateSlotValue,
            RazorVueIssueCode.MissingSlotValue => RazorVueMissingSlotValue,
            RazorVueIssueCode.CanonicalizationFailed => RazorVueGenerationFailed,
            RazorVueIssueCode.UnsupportedTemplateEncoding => RazorVueGenerationFailed,
            _ => RazorVueGenerationFailed
        };
        var location = TryCreateLocation(issueException.Origin) ?? candidate?.Location ?? Location.None;
        if (descriptor == RazorVueGenerationFailed)
        {
            var owner = string.IsNullOrWhiteSpace(issueException.OwnerComponentFullName)
                ? candidate?.ClassSymbol.ToDisplayString() ?? "RazorVue component"
                : issueException.OwnerComponentFullName;
            return Diagnostic.Create(descriptor, location, owner, issueException.Issue.Message);
        }

        return Diagnostic.Create(descriptor, location, issueException.Issue.Message);
    }

    private static RazorVueGeneratorOutputMode ResolveOutputMode(string? modeText)
    {
        if (string.IsNullOrWhiteSpace(modeText))
        {
            return RazorVueGeneratorOutputMode.Sfc;
        }

        if (string.Equals(modeText, "legacy", StringComparison.OrdinalIgnoreCase))
            return RazorVueGeneratorOutputMode.Legacy;
        if (string.Equals(modeText, "sfc", StringComparison.OrdinalIgnoreCase))
            return RazorVueGeneratorOutputMode.Sfc;

        throw new InvalidRazorVueOutputModeException(modeText ?? string.Empty);
    }

    private static bool TryCreateUnsupportedSetupLogicIssueException(
        NotSupportedException exception,
        ModuleCandidate? candidate,
        out RazorVueCompilationIssueException? issueException)
    {
        issueException = null;
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

    private static Location GetComponentLocation(INamedTypeSymbol symbol)
        => symbol.Locations.FirstOrDefault(static location => location.IsInSource) ?? Location.None;

    private static string BuildRazorVueCatalogSource(RazorVueCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static partial class RazorVueCatalog");
        builder.AppendLine("    {");
        builder.Append("        internal static string AssemblyName { get; } = ");
        builder.Append(EscapeCSharpString(catalog.AssemblyName));
        builder.AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("        internal static global::System.Collections.IEnumerable GetArtifacts()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _artifacts;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedArtifact");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedArtifact(string componentName, string relativeModulePath, string moduleCode, string[] routeTemplates, string[] imports, string[] styles, string[] pluginRequirements, GeneratedIdentity identity, GeneratedHints hints, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                ComponentName = componentName;");
        builder.AppendLine("                RelativeModulePath = relativeModulePath;");
        builder.AppendLine("                ModuleCode = moduleCode;");
        builder.AppendLine("                RouteTemplates = routeTemplates;");
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
        builder.AppendLine("            public string[] RouteTemplates { get; }");
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
            builder.Append("                routeTemplates: ").Append(BuildStringArrayLiteral(artifact.RouteTemplates)).AppendLine(",");
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

    internal static string BuildRazorVueSfcArtifactSource(VueSfcArtifact artifact)
    {
        var builder = new StringBuilder();
        var methodName = CreateRazorVueSfcArtifactMethodName(artifact);

        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    internal static partial class RazorVueCatalog");
        builder.AppendLine("    {");
        builder.Append("        private static GeneratedArtifact ").Append(methodName).AppendLine("()");
        builder.AppendLine("        {");
        builder.AppendLine("            return new GeneratedArtifact(");
        builder.Append("                componentName: ").Append(EscapeCSharpString(artifact.ComponentName)).AppendLine(",");
        builder.Append("                relativeSfcPath: ").Append(EscapeCSharpString(artifact.RelativeSfcPath)).AppendLine(",");
        builder.Append("                sfcText: ").Append(EscapeCSharpString(artifact.SfcText)).AppendLine(",");
        builder.Append("                templateBlock: ").Append(BuildTemplateBlockLiteral(artifact.TemplateBlock)).AppendLine(",");
        builder.Append("                scriptSetupBlock: ").Append(BuildScriptSetupBlockLiteral(artifact.ScriptSetupBlock)).AppendLine(",");
        builder.Append("                scriptBlock: ").Append(BuildScriptBlockLiteral(artifact.ScriptBlock)).AppendLine(",");
        builder.Append("                renderMode: GeneratedRenderMode.").Append(artifact.RenderMode).AppendLine(",");
        builder.Append("                styleBlocks: ").Append(BuildStyleBlocksArrayLiteral(artifact.StyleBlocks)).AppendLine(",");
        builder.Append("                customBlocks: ").Append(BuildCustomBlocksArrayLiteral(artifact.CustomBlocks)).AppendLine(",");
        builder.Append("                routeTemplates: ").Append(BuildStringArrayLiteral(artifact.RouteTemplates)).AppendLine(",");
        builder.Append("                imports: ").Append(BuildStringArrayLiteral(artifact.Imports)).AppendLine(",");
        builder.Append("                styles: ").Append(BuildStringArrayLiteral(artifact.Styles)).AppendLine(",");
        builder.Append("                pluginRequirements: ").Append(BuildStringArrayLiteral(artifact.PluginRequirements)).AppendLine(",");
        builder.AppendLine("                identity: new GeneratedIdentity(");
        builder.Append("                    componentId: ").Append(EscapeCSharpString(artifact.Identity.ComponentId)).AppendLine(",");
        builder.Append("                    moduleId: ").Append(EscapeCSharpString(artifact.Identity.ModuleId)).AppendLine(",");
        builder.Append("                    descriptorHash: ").Append(EscapeCSharpString(artifact.Identity.DescriptorHash)).AppendLine(",");
        builder.Append("                    templateHash: ").Append(EscapeCSharpString(artifact.Identity.TemplateHash)).AppendLine(",");
        builder.Append("                    logicHash: ").Append(EscapeCSharpString(artifact.Identity.LogicHash)).AppendLine(",");
        builder.Append("                    styleHash: ").Append(EscapeCSharpString(artifact.Identity.StyleHash)).AppendLine(",");
        builder.Append("                    hmrBoundaryKind: GeneratedHmrBoundaryKind.").Append(artifact.Identity.HmrBoundaryKind).AppendLine("),");
        builder.AppendLine("                hints: new GeneratedHints(");
        builder.Append("                    requiresVueRuntime: ").Append(ToCSharpBool(artifact.Hints.RequiresVueRuntime)).AppendLine(",");
        builder.Append("                    requiresHydration: ").Append(ToCSharpBool(artifact.Hints.RequiresHydration)).AppendLine(",");
        builder.Append("                    supportsSsr: ").Append(ToCSharpBool(artifact.Hints.SupportsSsr)).AppendLine(",");
        builder.Append("                    usesTeleport: ").Append(ToCSharpBool(artifact.Hints.UsesTeleport)).AppendLine(",");
        builder.Append("                    usesSuspense: ").Append(ToCSharpBool(artifact.Hints.UsesSuspense)).AppendLine(",");
        builder.Append("                    usesKeepAlive: ").Append(ToCSharpBool(artifact.Hints.UsesKeepAlive)).AppendLine("),");
        builder.Append("                sourceOrigins: ").Append(BuildSfcOriginsArrayLiteral(artifact.SourceOrigins)).AppendLine(");");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    internal static string BuildRazorVueSfcCatalogSource(RazorVueSfcCatalog catalog)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static partial class RazorVueCatalog");
        builder.AppendLine("    {");
        builder.Append("        internal static string AssemblyName { get; } = ");
        builder.Append(EscapeCSharpString(catalog.AssemblyName));
        builder.AppendLine(";");
        builder.AppendLine();
        builder.AppendLine("        internal static global::System.Collections.IEnumerable GetArtifacts()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _artifacts;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedArtifact");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedArtifact(string componentName, string relativeSfcPath, string sfcText, GeneratedTemplateBlock templateBlock, GeneratedScriptSetupBlock scriptSetupBlock, GeneratedScriptBlock scriptBlock, GeneratedRenderMode renderMode, GeneratedStyleBlock[] styleBlocks, GeneratedCustomBlock[] customBlocks, string[] routeTemplates, string[] imports, string[] styles, string[] pluginRequirements, GeneratedIdentity identity, GeneratedHints hints, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                ComponentName = componentName;");
        builder.AppendLine("                RelativeSfcPath = relativeSfcPath;");
        builder.AppendLine("                SfcText = sfcText;");
        builder.AppendLine("                TemplateBlock = templateBlock;");
        builder.AppendLine("                ScriptSetupBlock = scriptSetupBlock;");
        builder.AppendLine("                ScriptBlock = scriptBlock;");
        builder.AppendLine("                RenderMode = renderMode;");
        builder.AppendLine("                StyleBlocks = styleBlocks;");
        builder.AppendLine("                CustomBlocks = customBlocks;");
        builder.AppendLine("                RouteTemplates = routeTemplates;");
        builder.AppendLine("                Imports = imports;");
        builder.AppendLine("                Styles = styles;");
        builder.AppendLine("                PluginRequirements = pluginRequirements;");
        builder.AppendLine("                Identity = identity;");
        builder.AppendLine("                Hints = hints;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string ComponentName { get; }");
        builder.AppendLine("            public string RelativeSfcPath { get; }");
        builder.AppendLine("            public string SfcText { get; }");
        builder.AppendLine("            public GeneratedTemplateBlock TemplateBlock { get; }");
        builder.AppendLine("            public GeneratedScriptSetupBlock ScriptSetupBlock { get; }");
        builder.AppendLine("            public GeneratedScriptBlock ScriptBlock { get; }");
        builder.AppendLine("            public GeneratedRenderMode RenderMode { get; }");
        builder.AppendLine("            public GeneratedStyleBlock[] StyleBlocks { get; }");
        builder.AppendLine("            public GeneratedCustomBlock[] CustomBlocks { get; }");
        builder.AppendLine("            public string[] RouteTemplates { get; }");
        builder.AppendLine("            public string[] Imports { get; }");
        builder.AppendLine("            public string[] Styles { get; }");
        builder.AppendLine("            public string[] PluginRequirements { get; }");
        builder.AppendLine("            public GeneratedIdentity Identity { get; }");
        builder.AppendLine("            public GeneratedHints Hints { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedTemplateBlock");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedTemplateBlock(string text, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                Text = text;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Text { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedScriptSetupBlock");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedScriptSetupBlock(string text, string? language, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                Text = text;");
        builder.AppendLine("                Language = language;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Text { get; }");
        builder.AppendLine("            public string? Language { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedScriptBlock");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedScriptBlock(string text, string? language, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                Text = text;");
        builder.AppendLine("                Language = language;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Text { get; }");
        builder.AppendLine("            public string? Language { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedStyleBlock");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedStyleBlock(string text, bool isScoped, string? moduleName, string? language, string? sourceFilePath, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                Text = text;");
        builder.AppendLine("                IsScoped = isScoped;");
        builder.AppendLine("                ModuleName = moduleName;");
        builder.AppendLine("                Language = language;");
        builder.AppendLine("                SourceFilePath = sourceFilePath;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Text { get; }");
        builder.AppendLine("            public bool IsScoped { get; }");
        builder.AppendLine("            public string? ModuleName { get; }");
        builder.AppendLine("            public string? Language { get; }");
        builder.AppendLine("            public string? SourceFilePath { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedCustomBlock");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedCustomBlock(string name, string text, string? language, GeneratedAttribute[] attributes, string? sourceFilePath, GeneratedOrigin[] sourceOrigins)");
        builder.AppendLine("            {");
        builder.AppendLine("                Name = name;");
        builder.AppendLine("                Text = text;");
        builder.AppendLine("                Language = language;");
        builder.AppendLine("                Attributes = attributes;");
        builder.AppendLine("                SourceFilePath = sourceFilePath;");
        builder.AppendLine("                SourceOrigins = sourceOrigins;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Name { get; }");
        builder.AppendLine("            public string Text { get; }");
        builder.AppendLine("            public string? Language { get; }");
        builder.AppendLine("            public GeneratedAttribute[] Attributes { get; }");
        builder.AppendLine("            public string? SourceFilePath { get; }");
        builder.AppendLine("            public GeneratedOrigin[] SourceOrigins { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedAttribute");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedAttribute(string name, string? value)");
        builder.AppendLine("            {");
        builder.AppendLine("                Name = name;");
        builder.AppendLine("                Value = value;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string Name { get; }");
        builder.AppendLine("            public string? Value { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedIdentity");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedIdentity(string componentId, string moduleId, string descriptorHash, string templateHash, string logicHash, string styleHash, GeneratedHmrBoundaryKind hmrBoundaryKind)");
        builder.AppendLine("            {");
        builder.AppendLine("                ComponentId = componentId;");
        builder.AppendLine("                ModuleId = moduleId;");
        builder.AppendLine("                DescriptorHash = descriptorHash;");
        builder.AppendLine("                TemplateHash = templateHash;");
        builder.AppendLine("                LogicHash = logicHash;");
        builder.AppendLine("                StyleHash = styleHash;");
        builder.AppendLine("                HmrBoundaryKind = hmrBoundaryKind;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string ComponentId { get; }");
        builder.AppendLine("            public string ModuleId { get; }");
        builder.AppendLine("            public string DescriptorHash { get; }");
        builder.AppendLine("            public string TemplateHash { get; }");
        builder.AppendLine("            public string LogicHash { get; }");
        builder.AppendLine("            public string StyleHash { get; }");
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
        builder.AppendLine("            public GeneratedOrigin(GeneratedOriginKind originKind, string sourceFilePath, int sourceSpanStart, int sourceSpanLength, string? generatedFilePath, int? generatedSpanStart, int? generatedSpanLength, int startLine, int startColumn, GeneratedMappingQuality mappingQuality, GeneratedOriginProvenance provenance)");
        builder.AppendLine("            {");
        builder.AppendLine("                OriginKind = originKind;");
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
        builder.AppendLine("            public GeneratedOriginKind OriginKind { get; }");
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
        builder.AppendLine("        private enum GeneratedOriginKind");
        builder.AppendLine("        {");
        builder.AppendLine("            Component,");
        builder.AppendLine("            Descriptor,");
        builder.AppendLine("            Template,");
        builder.AppendLine("            Logic,");
        builder.AppendLine("            GeneratedRender,");
        builder.AppendLine("            Style,");
        builder.AppendLine("            CustomBlock");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private enum GeneratedMappingQuality");
        builder.AppendLine("        {");
        builder.AppendLine("            ExactSource,");
        builder.AppendLine("            MappedFromGenerated,");
        builder.AppendLine("            GeneratedOnly");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private enum GeneratedRenderMode");
        builder.AppendLine("        {");
        builder.AppendLine("            Template,");
        builder.AppendLine("            RenderFunction");
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
        for (var index = 0; index < catalog.Artifacts.Length; index++)
        {
            var artifact = catalog.Artifacts[index];
            builder.Append("            ").Append(CreateRazorVueSfcArtifactMethodName(artifact)).AppendLine("(),");
        }
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string BuildTemplateBlockLiteral(VueSfcTemplateBlock block)
        => "new GeneratedTemplateBlock(" + EscapeCSharpString(block.Text) + ", " + BuildSfcOriginsArrayLiteral(block.SourceOrigins) + ")";

    private static string BuildScriptSetupBlockLiteral(VueSfcScriptSetupBlock block)
        => "new GeneratedScriptSetupBlock(" + EscapeCSharpString(block.Text) + ", " + EscapeNullableCSharpString(block.Language) + ", " + BuildSfcOriginsArrayLiteral(block.SourceOrigins) + ")";

    private static string BuildScriptBlockLiteral(VueSfcScriptBlock block)
        => "new GeneratedScriptBlock(" + EscapeCSharpString(block.Text) + ", " + EscapeNullableCSharpString(block.Language) + ", " + BuildSfcOriginsArrayLiteral(block.SourceOrigins) + ")";

    private static string BuildStyleBlocksArrayLiteral(ImmutableArray<VueSfcStyleBlock> blocks)
    {
        if (blocks.IsDefaultOrEmpty)
            return "new GeneratedStyleBlock[0]";

        var builder = new StringBuilder();
        builder.AppendLine("new GeneratedStyleBlock[]");
        builder.AppendLine("                {");
        foreach (var block in blocks)
        {
            builder.Append("                    new GeneratedStyleBlock(")
                .Append(EscapeCSharpString(block.Text)).Append(", ")
                .Append(ToCSharpBool(block.IsScoped)).Append(", ")
                .Append(EscapeNullableCSharpString(block.ModuleName)).Append(", ")
                .Append(EscapeNullableCSharpString(block.Language)).Append(", ")
                .Append(EscapeNullableCSharpString(block.SourceFilePath)).Append(", ")
                .Append(BuildSfcOriginsArrayLiteral(block.SourceOrigins))
                .AppendLine("),");
        }

        builder.Append("                }");
        return builder.ToString();
    }

    private static string BuildCustomBlocksArrayLiteral(ImmutableArray<VueSfcCustomBlock> blocks)
    {
        if (blocks.IsDefaultOrEmpty)
            return "new GeneratedCustomBlock[0]";

        var builder = new StringBuilder();
        builder.AppendLine("new GeneratedCustomBlock[]");
        builder.AppendLine("                {");
        foreach (var block in blocks)
        {
            builder.Append("                    new GeneratedCustomBlock(")
                .Append(EscapeCSharpString(block.Name)).Append(", ")
                .Append(EscapeCSharpString(block.Text)).Append(", ")
                .Append(EscapeNullableCSharpString(block.Language)).Append(", ")
                .Append(BuildAttributesArrayLiteral(block.Attributes)).Append(", ")
                .Append(EscapeNullableCSharpString(block.SourceFilePath)).Append(", ")
                .Append(BuildSfcOriginsArrayLiteral(block.SourceOrigins))
                .AppendLine("),");
        }

        builder.Append("                }");
        return builder.ToString();
    }

    private static string BuildAttributesArrayLiteral(ImmutableArray<VueSfcAttribute> attributes)
    {
        if (attributes.IsDefaultOrEmpty)
            return "new GeneratedAttribute[0]";

        var builder = new StringBuilder("new GeneratedAttribute[] { ");
        for (var i = 0; i < attributes.Length; i++)
        {
            if (i > 0)
                builder.Append(", ");
            builder.Append("new GeneratedAttribute(")
                .Append(EscapeCSharpString(attributes[i].Name))
                .Append(", ")
                .Append(EscapeNullableCSharpString(attributes[i].Value))
                .Append(")");
        }

        builder.Append(" }");
        return builder.ToString();
    }

    private static string BuildSfcOriginsArrayLiteral(ImmutableArray<RazorVueSourceOrigin> origins)
    {
        if (origins.IsDefaultOrEmpty)
            return "new GeneratedOrigin[0]";

        var builder = new StringBuilder();
        builder.AppendLine("new GeneratedOrigin[]");
        builder.AppendLine("                {");
        foreach (var origin in origins)
        {
            builder.AppendLine("                    new GeneratedOrigin(");
            builder.Append("                        originKind: GeneratedOriginKind.").Append(origin.OriginKind).AppendLine(",");
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

    internal static string CreateRazorVueSfcArtifactHintName(VueSfcArtifact artifact)
        => "Jazor.Generated.RazorVue.Artifact_" + CreateRazorVueSfcArtifactKey(artifact) + ".g.cs";

    private static string CreateRazorVueSfcArtifactMethodName(VueSfcArtifact artifact)
        => "GetArtifact_" + CreateRazorVueSfcArtifactKey(artifact);

    private static string CreateRazorVueSfcArtifactKey(VueSfcArtifact artifact)
    {
        var baseName = string.IsNullOrWhiteSpace(artifact.ComponentName)
            ? artifact.Identity.ComponentId
            : artifact.ComponentName;
        var safeBaseName = SanitizeIdentifier(baseName);
        var hash = ComputeSha256Hex(artifact.Identity.ComponentId + "|" + artifact.Identity.ModuleId);
        return safeBaseName + "_" + hash.Substring(0, Math.Min(hash.Length, 12));
    }

    private static string SanitizeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Artifact";

        var builder = new StringBuilder(value.Length + 8);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        var sanitized = builder.ToString().Trim('_');
        if (string.IsNullOrWhiteSpace(sanitized))
            return "Artifact";

        if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            sanitized = "Artifact_" + sanitized;

        return sanitized;
    }

    private static string ToCSharpBool(bool value)
        => value ? "true" : "false";

    private static string BuildRazorSgBootstrapTraceSource(RazorSourceGeneratorBootstrapTrace trace)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace Jazor.Generated");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static class RazorSgBootstrapTrace");
        builder.AppendLine("    {");
        builder.Append("        internal const bool HasAttempted = ").Append(ToCSharpBool(trace.HasAttempted)).AppendLine(";");
        builder.Append("        internal const bool IsInstalled = ").Append(ToCSharpBool(trace.IsInstalled)).AppendLine(";");
        builder.Append("        internal const bool RazorAssemblyObserved = ").Append(ToCSharpBool(trace.RazorAssemblyObserved)).AppendLine(";");
        builder.Append("        internal const bool PatchAttempted = ").Append(ToCSharpBool(trace.PatchAttempted)).AppendLine(";");
        builder.Append("        internal const bool GeneratorTypeFound = ").Append(ToCSharpBool(trace.GeneratorTypeFound)).AppendLine(";");
        builder.Append("        internal const bool InitializeMethodFound = ").Append(ToCSharpBool(trace.InitializeMethodFound)).AppendLine(";");
        builder.Append("        internal const bool PostfixMethodFound = ").Append(ToCSharpBool(trace.PostfixMethodFound)).AppendLine(";");
        builder.Append("        internal const bool PatchSucceeded = ").Append(ToCSharpBool(trace.PatchSucceeded)).AppendLine(";");
        builder.Append("        internal const bool PatchFailed = ").Append(ToCSharpBool(trace.PatchFailed)).AppendLine(";");
        builder.Append("        internal const bool PatchUnavailable = ").Append(ToCSharpBool(trace.PatchUnavailable)).AppendLine(";");
        builder.Append("        internal const bool PostfixInvoked = ").Append(ToCSharpBool(trace.PostfixInvoked)).AppendLine(";");
        builder.Append("        internal const bool HostOutputHookInstalled = ").Append(ToCSharpBool(trace.HostOutputHookInstalled)).AppendLine(";");
        builder.Append("        internal const bool HostOutputObserved = ").Append(ToCSharpBool(trace.HostOutputObserved)).AppendLine(";");
        builder.Append("        internal const bool TailOutputRegistered = ").Append(ToCSharpBool(trace.TailOutputRegistered)).AppendLine(";");
        builder.Append("        internal const bool CurrentContextKeyAvailable = ").Append(ToCSharpBool(trace.CurrentContextKeyAvailable)).AppendLine(";");
        builder.Append("        internal const bool TailOutputRegisteredForCurrentContext = ").Append(ToCSharpBool(trace.TailOutputRegisteredForCurrentContext)).AppendLine(";");
        builder.Append("        internal const string TailOutputRegistrationKind = ").Append(EscapeCSharpString(trace.TailOutputRegistrationKind)).AppendLine(";");
        builder.Append("        internal const bool TestHookObserved = ").Append(ToCSharpBool(trace.TestHookObserved)).AppendLine(";");
        builder.Append("        internal const string? Failure = ").Append(EscapeNullableCSharpString(trace.Failure)).AppendLine(";");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? string.Empty));
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var item in bytes)
            builder.Append(item.ToString("X2", CultureInfo.InvariantCulture));
        return builder.ToString();
    }

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

    private readonly record struct RazorVueGeneratorOptions(
        string? OutputModeText,
        bool EnableRazorSgIntegration)
    {
        public static RazorVueGeneratorOptions Create(AnalyzerConfigOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            options.TryGetValue(RazorVueOutputModePropertyName, out var outputModeText);
            return new RazorVueGeneratorOptions(
                outputModeText,
                TryGetBooleanOption(options, RazorVueEnableRazorSgIntegrationPropertyName));
        }

        private static bool TryGetBooleanOption(AnalyzerConfigOptions options, string key)
            => options.TryGetValue(key, out var value) &&
               bool.TryParse(value, out var parsed) &&
               parsed;
    }

    private enum RazorVueGeneratorOutputMode
    {
        Legacy,
        Sfc
    }

    private sealed class InvalidRazorVueOutputModeException(string mode)
        : Exception($"Unsupported RazorVue output mode '{mode}'.")
    {
        public string Mode { get; } = mode;
    }
}
