using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Common.SourceMaps;
using Jazor.Compiler;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Builds the final Vue render module from official Razor SG C# and compiler-owned lowering.
/// Framing stays here; C# expression semantics stay in <see cref="AstConverter"/>.
/// 该边界负责模块、HMR 与 sourcemap 的 artifact framing，不以手工 JavaScript 替代 C# lowering。
/// </summary>
internal static class VueModuleBuilder
{
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private const string HmrComponentVariableName = "__jazorComponent";
    private const string CascadingMissingLocalName = "__jazor$cascade$missing";
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;
    private static readonly ImmutableHashSet<string> FramingReservedNames =
    new[]
    {
        "defineComponent",
        "h",
        "Fragment",
        "createStaticVNode",
        VueRawMarkup.CreateRawMarkupName,
        "openBlock",
        "createElementBlock",
        "createBlock",
        "createTextVNode",
        "renderList",
        "withCtx",
        "createSlots",
        "mergeProps",
        "onMounted",
        "onUnmounted",
        "onUpdated",
        "reactive",
        "watch",
        "inject",
        "unref",
        CascadingMissingLocalName,
        "props",
        "slots",
        "attrs",
        "parameterProps",
        "state",
        "scope",
        "invalidate",
        "pendingInvalidations",
        "disposed",
        "hasRendered",
        "cachedVNode",
        "__jazor$handlerCache",
        "stateHasChanged",
        "invokeAsync",
        "parametersSetAsyncGen",
        "parametersSetAsyncTail",
        "runOnParametersSetAsync",
        "parameterWatchSource",
        "parameterAdapter",
        "parameterSnapshot",
        "parameterUpdateTail",
        "runSetParametersAsync",
        "initialized",
        HmrComponentVariableName
    }.ToImmutableHashSet(StringComparer.Ordinal);

    public static async Task<VueModuleArtifact> BuildAsync(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        MemberClosure closure,
        CancellationToken cancellationToken = default)
    {
        if (binding is null)
            throw new ArgumentNullException(nameof(binding));
        if (component is null)
            throw new ArgumentNullException(nameof(component));
        if (closure is null)
            throw new ArgumentNullException(nameof(closure));
        if (!SymbolComparer.Equals(component.ComponentSymbol.OriginalDefinition, closure.ComponentSymbol.OriginalDefinition))
            throw new ArgumentException("The RazorVue component module closure does not belong to the requested component.", nameof(closure));

        cancellationToken.ThrowIfCancellationRequested();

        var injectRegistry = VueInjectRegistry.ForCompilation(binding.Compilation);

        var syntaxTree = component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var semanticModel = binding.Compilation.GetSemanticModel(syntaxTree);
        var relativePath = GetRelativePath(component.ComponentSymbol);
        var declaredNames = BuildDirectRenderDeclaredNames(component, closure);
        var compilerOutput = await BuildCompilerOutputAsync(
            binding,
            component,
            closure,
            semanticModel,
            relativePath,
            declaredNames,
            injectRegistry,
            cancellationToken).ConfigureAwait(false);
        var directRender = BuildOperationDirectRender(
            binding,
            component,
            closure,
            declaredNames,
            CollectCompilerImportLocalNames(
                compilerOutput.Module,
                compilerOutput.Initialization.ImportDeclarations),
            injectRegistry);

        // Compiler and component imports are discovered after lowering. Re-run only when an
        // authored member would shadow one of those bindings inside the setup scope.
        var importLocalNames = CollectImportLocalNames(
            compilerOutput.Module,
            directRender,
            compilerOutput.Initialization.ImportDeclarations);
        if (HasImportNameCollision(declaredNames, importLocalNames))
        {
            declaredNames = BuildDirectRenderDeclaredNames(component, closure, importLocalNames);
            compilerOutput = await BuildCompilerOutputAsync(
                binding,
                component,
                closure,
                semanticModel,
                relativePath,
                declaredNames,
                injectRegistry,
                cancellationToken).ConfigureAwait(false);
            directRender = BuildOperationDirectRender(
                binding,
                component,
                closure,
                declaredNames,
                CollectCompilerImportLocalNames(
                    compilerOutput.Module,
                    compilerOutput.Initialization.ImportDeclarations),
                injectRegistry);
        }

        var hmr = BuildHmrMetadata(component, closure, relativePath);
        var moduleBuild = BuildModuleText(
            component,
            closure,
            directRender,
            compilerOutput.Module,
            compilerOutput.Layout?.NodePositions,
            compilerOutput.Initialization,
            compilerOutput.OrdinaryRenderFeatures,
            relativePath,
            declaredNames,
            hmr.ModuleId);
        var moduleText = moduleBuild.ModuleText;
        var sourceMapRelativePath = relativePath + ".map";
        var sourceMapContent = BuildSourceMapContent(
            component,
            relativePath,
            moduleText,
            compilerOutput.Layout?.Artifact.SourceMapContent,
            moduleBuild.CompiledLineMappings);

        return new VueModuleArtifact(
            component.ComponentSymbol.ToDisplayString(),
            relativePath,
            moduleText,
            ComputeContentHash(moduleText),
            sourceMapRelativePath,
            sourceMapContent,
            ComputeContentHash(sourceMapContent),
            moduleBuild.PackageImports,
            moduleBuild.Assets,
            hmr);
    }

    private static async Task<CompilerOutput> BuildCompilerOutputAsync(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        MemberClosure closure,
        SemanticModel semanticModel,
        string relativePath,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        VueInjectRegistry injectRegistry,
        CancellationToken cancellationToken)
    {
        // Delegate all C# semantics, import discovery, and source origins to AstConverter. This
        // builder only frames its module output into Vue setup/render conventions.
        // C# lowering 绝不能在此手工重建；本层只消费 compiler 的 AST 结果。
        var ordinaryRenderFeatures = new VueRenderRuntimeFeatures();
        var converter = new AstConverter(
            component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions(
                declaredNames: declaredNames,
                propertyReferenceRewriter: CreateDirectRenderSlotParameterPropertyReferenceRewriter(closure),
                compilation: binding.Compilation,
                injectRegistry: injectRegistry,
                ordinaryRenderFeatures: ordinaryRenderFeatures));
        var module = await converter.Convert(cancellationToken).ConfigureAwait(false);
        module = AppendFlattenedRuntimeClasses(module, converter, component, closure, cancellationToken);
        var initialization = ComponentInitializationLowerer.Build(
            binding.Compilation,
            closure,
            declaredNames,
            module?.Body.OfType<ImportDeclaration>() ?? Enumerable.Empty<ImportDeclaration>(),
            FramingReservedNames,
            cancellationToken);
        var layout = module is null
            ? null
            : module.ToKnRECMAScriptWithSourceMapAndNodePositions(
                generatedFileName: relativePath,
                includeSourcesContent: false,
                sourceRootPath: TryGetCompilationSourceRoot(binding.Compilation, component.Document),
                readSourceContent: null);
        return new CompilerOutput(module, layout, initialization, ordinaryRenderFeatures);
    }

    private static Module? AppendFlattenedRuntimeClasses(
        Module? module,
        AstConverter converter,
        BoundComponent component,
        MemberClosure closure,
        CancellationToken cancellationToken)
    {
        // Nested member runtime classes may be referenced by field initializers before setup
        // state exists. Emit them first, in stable containment order, to avoid JS TDZ failures.
        // 内嵌运行时类必须先于 state 声明，避免类声明 temporal dead zone 改变 C# 初始化时序。
        var flattenedClasses = GetFlattenedRuntimeClasses(component.ComponentSymbol, closure);
        if (flattenedClasses.Length == 0)
            return module;

        var members = new List<Statement>();
        foreach (var runtimeClass in flattenedClasses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            members.Add(converter.ConvertRuntimeClass(runtimeClass, cancellationToken));
        }

        if (module is not null)
        {
            members.AddRange(module.Body.Where(static statement => statement is not ImportDeclaration));
        }

        var imports = converter.FlushImportDeclarations(members);
        return new Module(NodeList.From<Statement>(imports.Concat(members)));
    }

    private static ImmutableArray<INamedTypeSymbol> GetFlattenedRuntimeClasses(
        INamedTypeSymbol componentType,
        MemberClosure closure)
    {
        var candidates = ImmutableHashSet.CreateRange<INamedTypeSymbol>(
            SymbolComparer,
            closure.OrderedMembers
                .OfType<INamedTypeSymbol>()
                .Where(type => IsFlattenedRuntimeClass(componentType, type)));
        if (candidates.Count == 0)
            return [];

        var ordered = ImmutableArray.CreateBuilder<INamedTypeSymbol>(candidates.Count);
        foreach (var runtimeClass in candidates
                     .Where(type => !candidates.Contains(type.ContainingType!))
                     .OrderBy(static type => GetStableSymbolSortKey(type), StringComparer.Ordinal))
        {
            AppendFlattenedRuntimeClass(runtimeClass, candidates, ordered);
        }

        return ordered.ToImmutable();
    }

    private static void AppendFlattenedRuntimeClass(
        INamedTypeSymbol runtimeClass,
        ImmutableHashSet<INamedTypeSymbol> candidates,
        ImmutableArray<INamedTypeSymbol>.Builder ordered)
    {
        foreach (var nestedClass in candidates
                     .Where(candidate => SymbolComparer.Equals(candidate.ContainingType, runtimeClass))
                     .OrderBy(static type => GetStableSymbolSortKey(type), StringComparer.Ordinal))
        {
            AppendFlattenedRuntimeClass(nestedClass, candidates, ordered);
        }

        ordered.Add(runtimeClass);
    }

    private static bool IsFlattenedRuntimeClass(INamedTypeSymbol componentType, INamedTypeSymbol type)
    {
        if (!IsRuntimeMemberClass(type) ||
            type.ContainingType is not INamedTypeSymbol containingRuntimeClass ||
            ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(componentType, containingRuntimeClass))
        {
            return false;
        }

        for (var current = containingRuntimeClass; current is not null; current = current.ContainingType)
        {
            if (ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(componentType, current))
                return true;

            if (!IsRuntimeMemberClass(current))
                return false;
        }

        return false;
    }

    private static string GetStableSymbolSortKey(ISymbol symbol)
        => symbol.Locations.FirstOrDefault(static location => location.IsInSource) is { } location
            ? location.SourceTree!.FilePath.Replace('\\', '/') + ":" + location.SourceSpan.Start.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static DirectRenderBuildResult BuildOperationDirectRender(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        IEnumerable<string> reservedImportNames,
        VueInjectRegistry injectRegistry)
    {
        if (TryBuildOperationDirectRender(
                binding,
                component,
                closure,
                declaredNames,
                reservedImportNames,
                injectRegistry,
                out var directRender,
                out var directRenderDiagnostic))
        {
            return directRender;
        }

        throw new RazorVueDiagnosticException(
            directRenderDiagnostic!);
    }

    private static VueHmrMetadata BuildHmrMetadata(
        BoundComponent component,
        MemberClosure closure,
        string relativePath)
    {
        // The three fingerprints intentionally come from distinct compiler-owned inputs.
        // They are not inferred by diffing the final framed module text, because framing can
        // change without making a Razor template update safe to apply at runtime.
        var assemblyName = component.ComponentSymbol.ContainingAssembly.Name;
        var moduleId = assemblyName + ":" + relativePath;
        return new VueHmrMetadata(
            moduleId,
            ComputeDescriptorHash(closure),
            ComputeTemplateHash(component),
            ComputeLogicHash(component, closure),
            VueHmrBoundaryKind.TemplateOnly);
    }

    private static string ComputeDescriptorHash(MemberClosure closure)
    {
        var descriptorEntries = new List<string>();
        foreach (var property in LibraryComponentConventions.GetEffectiveParameterProperties(closure.ComponentSymbol))
        {
            var typeName = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (IsAnyRenderFragmentType(property.Type))
            {
                descriptorEntries.Add(
                    "slot|" + property.Name + "|" +
                    LibraryComponentConventions.GetSlotRuntimeName(closure.ComponentSymbol, property) + "|" + typeName);
                continue;
            }

            if (IsEventCallbackType(property.Type))
            {
                descriptorEntries.Add(
                    "event|" + property.Name + "|" +
                    LibraryComponentConventions.GetEventListenerRuntimeName(closure.ComponentSymbol, property) + "|" + typeName);
                continue;
            }

            descriptorEntries.Add(
                "prop|" + property.Name + "|" +
                LibraryComponentConventions.GetPropRuntimeName(property) + "|" + typeName);
        }

        var fingerprint = new StringBuilder();
        AppendFingerprintPart(
            fingerprint,
            closure.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        foreach (var entry in descriptorEntries.OrderBy(static entry => entry, StringComparer.Ordinal))
            AppendFingerprintPart(fingerprint, entry);
        return ComputeContentHash(fingerprint.ToString());
    }

    private static string ComputeTemplateHash(BoundComponent component)
    {
        var declaration = component.BuildRenderTreeMethod.DeclaringSyntaxReferences
            .Single()
            .GetSyntax();
        return ComputeContentHash(GetCanonicalSyntaxText(declaration));
    }

    private static string ComputeLogicHash(BoundComponent component, MemberClosure closure)
    {
        var fingerprint = new StringBuilder();
        foreach (var member in closure.OrderedMembers
                     .Where(member => !SymbolComparer.Equals(
                         member.OriginalDefinition,
                         component.BuildRenderTreeMethod.OriginalDefinition))
                     .OrderBy(GetStableSymbolSortKey, StringComparer.Ordinal))
        {
            AppendFingerprintPart(fingerprint, member.Kind.ToString());
            AppendFingerprintPart(
                fingerprint,
                member.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            foreach (var declaration in member.DeclaringSyntaxReferences
                         .Select(static reference => GetCanonicalSyntaxText(reference.GetSyntax()))
                         .OrderBy(static declaration => declaration, StringComparer.Ordinal))
            {
                AppendFingerprintPart(fingerprint, declaration);
            }
        }

        return ComputeContentHash(fingerprint.ToString());
    }

    private static string GetCanonicalSyntaxText(SyntaxNode syntax)
        => Util.NormalizeLineEndingsToLf(syntax.WithoutTrivia().ToFullString());

    private static void AppendFingerprintPart(StringBuilder builder, string value)
    {
        builder.Append(value.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.Append(':');
        builder.Append(value);
        builder.Append('\n');
    }

    private static HashSet<string> CollectImportLocalNames(
        Module? compilerModule,
        DirectRenderBuildResult directRender,
        IEnumerable<ImportDeclaration>? initializationImports = null)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (compilerModule is not null)
        {
            foreach (var import in compilerModule.Body.OfType<ImportDeclaration>())
                AddImportLocalNames(import, names);
        }

        foreach (var import in directRender.ImportDeclarations)
            AddImportLocalNames(import, names);
        if (initializationImports is not null)
        {
            foreach (var import in initializationImports)
                AddImportLocalNames(import, names);
        }
        return names;
    }

    private static HashSet<string> CollectCompilerImportLocalNames(
        Module? compilerModule,
        IEnumerable<ImportDeclaration>? initializationImports = null)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (compilerModule is not null)
        {
            foreach (var import in compilerModule.Body.OfType<ImportDeclaration>())
                AddImportLocalNames(import, names);
        }
        if (initializationImports is not null)
        {
            foreach (var import in initializationImports)
                AddImportLocalNames(import, names);
        }
        return names;
    }

    private static bool HasImportNameCollision(
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        HashSet<string> importLocalNames)
        => declaredNames.Values.Any(importLocalNames.Contains);

    private static ModuleTextBuildResult BuildModuleText(
        BoundComponent component,
        MemberClosure closure,
        DirectRenderBuildResult directRender,
        Module? compilerModule,
        IReadOnlyDictionary<Node, GeneratedNodePosition>? compilerNodePositions,
        ComponentInitializationBuildResult initialization,
        VueRenderRuntimeFeatures ordinaryRenderFeatures,
        string relativePath,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        string hmrModuleId)
    {
        // Partition compiler output before adding Vue framing. Imports/declarations/setup state
        // have different ordering and source-map responsibilities in the final artifact.
        // 先拆分 compiler 输出，保证 import、state、setup 以确定顺序进入 artifact。
        var parts = BuildCompilerModuleParts(compilerModule, compilerNodePositions, closure, declaredNames);
        parts = parts with
        {
            ImportDeclarations = parts.ImportDeclarations.AddRange(initialization.ImportDeclarations),
            InitializationPhases = initialization.Phases
        };
        var componentSymbol = component.ComponentSymbol;
        var buildRenderTreeMemberName = GetRuntimeMemberName(
            component.BuildRenderTreeMethod,
            declaredNames);
        // A Vue ref callback receives its value only during mount. Its direct C# assignment
        // identifies an otherwise opaque component storage slot whose pre-mount state is empty.
        parts = parts with
        {
            SetupStatements = RemoveBuildRenderTreeFunction(
                parts.SetupStatements,
                buildRenderTreeMemberName),
            StateSlots = ApplyReferenceCaptureStateInitializers(
                parts.StateSlots,
                directRender.ReferenceCaptureStateMembers)
        };

        // BuildCompilerModuleParts has already separated component/module lifetime from setup
        // lifetime. Static initializers must see their static helpers and runtime classes before
        // any Vue instance exists, while instance members keep closing over props/state here.
        // module/setup 生命周期在 compiler AST 分区时一次确定，不能在最终文本阶段再猜声明归属。
        var setupParts = parts;

        var setupFactoryName = "create" + SanitizeJavaScriptIdentifierPart(componentSymbol.Name, "Component") + "SetupScope";
        var returnedMembers = GetReturnedMembers(closure, declaredNames);
        var lifecycleMembers = ComponentLifecycleRuntimeMembers.Create(closure, declaredNames);
        returnedMembers = returnedMembers
            .RemoveAll(member => string.Equals(member, buildRenderTreeMemberName, StringComparison.Ordinal))
            .Add(directRender.MemberName);
        if (lifecycleMembers.HasSetParametersAsync &&
            !returnedMembers.Contains("parameterAdapter", StringComparer.Ordinal))
        {
            // The adapter is setup-instance state. Expose only its snapshot seam to the outer
            // Vue setup closure through the existing scope object; never reference the local
            // adapter binding from module-level framing code.
            // 适配器属于 setup 实例，外层只通过 scope 访问，避免跨 lexical scope 的自由变量。
            returnedMembers = returnedMembers.Add("parameterAdapter");
        }
        setupParts = setupParts with
        {
            SetupStatements = RemoveDirectRenderOnlyFunctions(
                setupParts.SetupStatements,
                directRender,
                setupParts.StateSlots,
                setupParts.InitializationPhases,
                returnedMembers)
        };

        var usesInvokeAsync = ReferencesIdentifier(setupParts.SetupStatements, "invokeAsync") ||
                              ReferencesIdentifier(setupParts.InitializationPhases, "invokeAsync");

        var parameterBindings = BuildParameterBindings(closure);
        var injectBindings = BuildInjectBindings(closure);
        var cascadingBindings = BuildCascadingBindings(closure);
        var usesUnmatchedAttributes = parameterBindings.Any(static binding => binding.CapturesUnmatchedValues);
        var usesParameterViewSlots = lifecycleMembers.HasSetParametersAsync &&
                                     parameterBindings.Any(static binding => binding.IsSlot);
        var usesFactorySlots = directRender.UsesSlots ||
                               ordinaryRenderFeatures.UsesSlots ||
                               usesParameterViewSlots ||
                                ReferencesIdentifier(setupParts.SetupStatements, "slots") ||
                                ReferencesIdentifier(setupParts.InitializationPhases, "slots") ||
                                setupParts.StateSlots.Any(static slot =>
                                    slot.Initializer is not null &&
                                    AstReferenceAnalysis.ReferencesIdentifier(slot.Initializer, "slots"));
        var usesSlots = usesFactorySlots;
        var usesFactoryProps = ReferencesIdentifier(setupParts.SetupStatements, "props") ||
                               ReferencesIdentifier(setupParts.InitializationPhases, "props") ||
                               directRender.UsesProps ||
                               ordinaryRenderFeatures.UsesProps ||
                               lifecycleMembers.HasSetParametersAsync ||
                               usesUnmatchedAttributes;
        var usesSetupProps = usesFactoryProps ||
                             usesSlots ||
                             lifecycleMembers.HasOnParametersSet ||
                             lifecycleMembers.HasOnParametersSetAsync;
        var usesState = setupParts.StateSlots.Length > 0;
        var usesStateHasChanged = lifecycleMembers.HasOnInitializedAsync ||
                                   lifecycleMembers.HasOnParametersSetAsync ||
                                   lifecycleMembers.HasSetParametersAsync ||
                                   !cascadingBindings.IsDefaultOrEmpty ||
                                   ReferencesIdentifier(setupParts.SetupStatements, "stateHasChanged") ||
                                   ReferencesIdentifier(setupParts.InitializationPhases, "stateHasChanged");
        var propNames = GetVuePropNames(closure);
        var features = new VueModuleFeatures(
            lifecycleMembers,
            usesSlots,
            usesFactorySlots,
            usesFactoryProps,
            usesSetupProps,
            usesState,
            usesStateHasChanged,
            usesInvokeAsync,
            propNames,
            parameterBindings,
            usesParameterViewSlots,
            usesUnmatchedAttributes,
            injectBindings,
            cascadingBindings,
            component.ComponentSymbol.ToDisplayString(Format.NameFormat));
        var moduleStatements = new List<Statement>();
        var assets = ImmutableArray.CreateBuilder<VueAsset>();
        var usesMergeProps = directRender.UsesMergeProps || ordinaryRenderFeatures.UsesMergeProps;
        // Local bindings are the module-scope uniqueness contract. Compiler lowering and direct
        // render lowering can contribute different specifiers for one module, so dedupe must be
        // performed per specifier instead of dropping the whole import declaration on one match.
        var emittedImportBindings = new Dictionary<string, ImportBinding>(StringComparer.Ordinal);

        moduleStatements.Add(BuildVueImportDeclaration(
            features.UsesMounted,
            features.UsesUnmounted,
            features.UsesUpdated,
            features.UsesReactive,
            features.UsesWatch,
            directRender.UsesFragment || ordinaryRenderFeatures.UsesFragment,
            directRender.UsesStaticVNode,
            directRender.UsesRawMarkupRuntime || ordinaryRenderFeatures.UsesRawMarkupRuntime,
            directRender.UsesBlockTree || ordinaryRenderFeatures.UsesBlockTree,
            directRender.UsesTextVNode || ordinaryRenderFeatures.UsesTextVNode,
            directRender.UsesRenderList || ordinaryRenderFeatures.UsesRenderList,
            directRender.UsesWithCtx || ordinaryRenderFeatures.UsesWithCtx,
            directRender.UsesCreateSlots || ordinaryRenderFeatures.UsesCreateSlots,
            usesMergeProps,
            usesInject: features.UsesInject,
            usesCascading: !cascadingBindings.IsDefaultOrEmpty));

        if (directRender.UsesRawMarkupRuntime || ordinaryRenderFeatures.UsesRawMarkupRuntime)
        {
            moduleStatements.AddRange(ImportDeclarationFactory.Create(
                VueRawMarkup.RuntimeModuleSpecifier,
                [new ImportSpecifier(
                    new Identifier(VueRawMarkup.RuntimeExportName),
                    new Identifier(VueRawMarkup.CreateRawMarkupName))]));
        }

        foreach (var importDeclaration in parts.ImportDeclarations)
        {
            if (IsVueFramingImport(importDeclaration))
                continue;
            if (!IsCompilerImportReferenced(importDeclaration, directRender, parts))
                continue;

            var rebasedImport = RebaseImportDeclaration(importDeclaration, relativePath);
            var importToEmit = FilterEmittedImportSpecifiers(rebasedImport, emittedImportBindings);
            if (importToEmit is null)
                continue;
            moduleStatements.Add(importToEmit);
            if (TryCreateVueSfcAsset(importToEmit, relativePath, out var asset))
                assets.Add(asset);
        }

        foreach (var importDeclaration in directRender.ImportDeclarations)
        {
            // Vue helpers are framed above as one deterministic import. RenderEmitter's direct
            // collector contributes mergeProps for the same module, so do not emit a duplicate
            // local binding after ordinary-member fragments have joined the feature set.
            if (IsVueFramingImport(importDeclaration))
                continue;
            var rebasedImport = RebaseImportDeclaration(importDeclaration, relativePath);
            var importToEmit = FilterEmittedImportSpecifiers(rebasedImport, emittedImportBindings);
            if (importToEmit is null)
                continue;
            moduleStatements.Add(importToEmit);
            // Component references discovered by RenderEmitter bypass compiler module
            // statements, but their .vue source still has to reach the artifact catalog.
            // direct render import 也必须登记 asset，否则 SFC 只会出现在 import 而不会被 Emit 复制。
            if (TryCreateVueSfcAsset(importToEmit, relativePath, out var asset))
                assets.Add(asset);
        }

        // Runtime classes precede every eager module initializer: JavaScript class declarations
        // have a temporal dead zone, unlike function declarations. Preserve compiler order within
        // each partition so base-before-derived class ordering remains deterministic.
        // runtime class 必须先于 static field initializer，避免 `new Nested()` 命中 TDZ。
        moduleStatements.AddRange(parts.ModuleStatements
            .Where(static statement => statement.Statement is ClassDeclaration)
            .Select(static statement => statement.Statement));

        // Vue helper imports must precede hoist initializers. Hoists themselves remain module
        // constants, while handler caches stay in setup so component instances never share one.
        // 静态 props/VNode 可跨实例复用；事件闭包只能由 setup instance 持有。
        moduleStatements.AddRange(directRender.ModuleHoists.Select(static hoist =>
            CreateVariableDeclaration(VariableDeclarationKind.Const, hoist.Name, hoist.Initializer)));

        moduleStatements.AddRange(parts.ModuleStatements
            .Where(static statement => statement.Statement is not ClassDeclaration)
            .Select(static statement => statement.Statement));

        moduleStatements.Add(BuildSetupFactoryDeclaration(
            setupFactoryName,
            returnedMembers,
            setupParts,
            directRender,
            ordinaryRenderFeatures,
            features));
        moduleStatements.Add(BuildVueComponentDeclaration(
            closure,
            setupFactoryName,
            directRender,
            features,
            propNames));
        moduleStatements.Add(BuildVueHmrRegistration(hmrModuleId));
        moduleStatements.Add(new ExportDefaultDeclaration(new Identifier(HmrComponentVariableName)));

        var vueModule = new Module(NodeList.From(moduleStatements));
        // Validate the complete AST after compiler/direct-render/framing composition. This is
        // intentionally before text serialization so property keys, labels, and declarations
        // retain their ESTree meaning and future helper/import regressions fail as JAZORVGA026.
        // 组合完成后立即检查自由标识符，禁止生成能编译却在浏览器留下 undefined 的模块。
        VueModuleIntegrityValidator.Validate(vueModule);
        var moduleLayout = vueModule.ToKnRECMAScriptWithNodePositions();
        var moduleText = Util.NormalizeLineEndingsToLf(moduleLayout.Content);
        var lineMappings = BuildCompiledLineMappings(moduleLayout.NodePositions, parts);
        var packageImports = moduleStatements
            .OfType<ImportDeclaration>()
            .Select(static declaration => declaration.Source.Value)
            .Where(ECMAScriptModulePath.IsPackageSpecifier)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToImmutableArray();

        return new ModuleTextBuildResult(
            moduleText,
            lineMappings,
            packageImports,
            assets
                .GroupBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
                .ToImmutableArray());
    }

    private static FunctionDeclaration BuildSetupFactoryDeclaration(
        string setupFactoryName,
        ImmutableArray<string> returnedMembers,
        CompilerModuleParts parts,
        DirectRenderBuildResult directRender,
        VueRenderRuntimeFeatures ordinaryRenderFeatures,
        VueModuleFeatures features)
    {
        var statements = new List<Statement>();
        // JavaScript class declarations are in the temporal dead zone until evaluated.
        // C# field initializers may materialize a nested member class, so preserve the
        // compiler's class order while placing those declarations before reactive state.
        statements.AddRange(parts.SetupStatements
            .Where(static item => item.Statement is ClassDeclaration)
            .Select(static item => item.Statement));
        if (features.UsesState)
        {
            statements.Add(parts.InitializationPhases.Any(static phase => phase.ConstructorStatement is not null)
                ? BuildStateDefaultsDeclaration(parts.StateSlots)
                : BuildStateDeclaration(parts.StateSlots));
        }

        // Component construction occurs after every instance slot has its CLR default, then
        // applies source initializers and constructor bodies base-to-derived. This is the closest
        // setup-time analogue of CLR allocation without materializing a ComponentBase instance.
        // 所有 slot 先获得 CLR default，再按基类到派生类执行 initializer/constructor。
        statements.AddRange(BuildComponentInitializationStatements(
            parts.StateSlots,
            parts.InitializationPhases));

        // Blazor resolves [Inject] properties after construction/field initializers and before
        // OnInitialized/parameter callbacks. Assign the resolved Vue service into the same
        // reactive state carrier used by normal component members so authored getters, methods,
        // and later renders all observe one value.
        // Blazor 的注入时序位于构造完成之后、生命周期之前；不能把服务塞进构造函数或 props。
        foreach (var binding in features.InjectBindings)
        {
            statements.Add(CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                CreateMemberAccess(new Identifier("state"), binding.StateName),
                new Identifier(binding.LocalName))));
        }

        // Cascading values arrive as Vue refs from the provider adapter. Unwrap once for the
        // authored CLR property, then keep the same state slot synchronized as the nearest
        // provider changes. A unique sentinel preserves the distinction between “no provider”
        // (Blazor leaves the default value intact) and an explicitly supplied null/undefined.
        // 级联值由 provider 以 ref 提供；这里映射回普通 C# 属性并监听后续更新。
        foreach (var binding in features.CascadingBindings)
        {
            var resolved = new Identifier(binding.LocalName);
            var missing = new Identifier(CascadingMissingLocalName);
            if (!features.UsesParameterViewState)
            {
                statements.Add(new IfStatement(
                    new NonLogicalBinaryExpression(
                        Operator.StrictInequality,
                        resolved,
                        missing),
                    CreateBlock(CreateExpressionStatement(new AssignmentExpression(
                        Operator.Assignment,
                        CreateMemberAccess(new Identifier("state"), binding.StateName),
                        CreateCall("unref", resolved)))),
                    null));
            }

            var cascadeUpdateStatements = new List<Statement>();
            if (!features.UsesParameterViewState)
            {
                cascadeUpdateStatements.Add(CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("state"), binding.StateName),
                    new Identifier("value"))));
            }
            cascadeUpdateStatements.Add(CreateExpressionStatement(CreateCall("cascadeChanged")));

            statements.Add(CreateExpressionStatement(CreateCall(
                "watch",
                CreateArrowExpression(CreateCall("unref", new Identifier(binding.LocalName))),
                CreateArrowFunction(
                    ["value"],
                    [
                        new IfStatement(
                            new NonLogicalBinaryExpression(
                                Operator.StrictInequality,
                                new Identifier("value"),
                                missing),
                            CreateBlock(cascadeUpdateStatements.ToArray()),
                            null)
                    ]))));
        }

        if (features.UsesParameterViewState)
        {
            // This object lives beside reactive state and lifecycle functions, so the lowered
            // authored SetParametersAsync body can call it through its normal compiler host seam.
            // 参数适配器必须与 state 同一 setup factory 实例，不能成为模块共享对象。
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Let,
                "initialized",
                BooleanLiteral(false)));
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "parameterAdapter",
                BuildParameterAdapterExpression(features)));
        }
        statements.AddRange(parts.SetupStatements
            .Where(static item => item.Statement is not ClassDeclaration)
            .Select(static item => item.Statement));
        if (directRender.UsesHandlerCache || ordinaryRenderFeatures.UsesHandlerCache)
        {
            // Vue's compiler cache is instance-owned: a module-level array would retain the
            // first component's state/props closure and break instance isolation.
            // handler cache 必须在 setup factory 内创建，不能提升成模块全局数组。
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "__jazor$handlerCache",
                new ArrayExpression(NodeList.Empty<Expression?>())));
        }
        var renderStatements = directRender.PreludeStatements.ToList();
        renderStatements.Add(new ReturnStatement(directRender.RenderExpression));
        statements.Add(new FunctionDeclaration(
            new Identifier(directRender.MemberName),
            NodeList.Empty<Node>(),
            CreateFunctionBody(renderStatements),
            generator: false,
            async: false));

        statements.Add(new ReturnStatement(BuildReturnedMembersExpression(returnedMembers)));
        return new FunctionDeclaration(
            new Identifier(setupFactoryName),
            NodeList.From<Node>(BuildSetupFactoryIdentifiers(features)),
            CreateFunctionBody(statements),
            generator: false,
            async: false);
    }

    private static VariableDeclaration BuildStateDeclaration(ImmutableArray<StateSlot> stateSlots)
    {
        var properties = stateSlots.Select(slot => (Node)CreateObjectProperty(
            slot.RuntimeName,
            slot.Initializer ?? CurrentComponentStateDefaultInitializer.CreateExpression(slot.Type)));
        var state = CreateCall(
            "reactive",
            new ObjectExpression(NodeList.From(properties)));
        return CreateVariableDeclaration(VariableDeclarationKind.Const, "state", state);
    }

    private static VariableDeclaration BuildStateDefaultsDeclaration(ImmutableArray<StateSlot> stateSlots)
    {
        var properties = stateSlots.Select(slot => (Node)CreateObjectProperty(
            slot.RuntimeName,
            CurrentComponentStateDefaultInitializer.CreateExpression(slot.Type)));
        var state = CreateCall(
            "reactive",
            new ObjectExpression(NodeList.From(properties)));
        return CreateVariableDeclaration(VariableDeclarationKind.Const, "state", state);
    }

    private static IEnumerable<Statement> BuildComponentInitializationStatements(
        ImmutableArray<StateSlot> stateSlots,
        ImmutableArray<ComponentInitializationPhaseBuild> phases)
    {
        foreach (var phase in phases)
        {
            foreach (var slot in stateSlots
                         .Where(slot =>
                             SymbolComparer.Equals(
                                 slot.Member.ContainingType?.OriginalDefinition,
                                 phase.ComponentType.OriginalDefinition) &&
                             slot.HasExplicitInitializer)
                         .OrderBy(static slot => GetStableSourceOrder(slot.Member), StringComparer.Ordinal))
            {
                var initializer = slot.Initializer ??
                    CurrentComponentStateDefaultInitializer.CreateExpression(slot.Type);
                yield return CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("state"), slot.RuntimeName),
                    initializer));
            }

            if (phase.ConstructorStatement is not null)
                yield return phase.ConstructorStatement;
        }
    }

    private static string GetStableSourceOrder(ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            if (!location.IsInSource)
                continue;

            var lineSpan = location.GetLineSpan();
            return (lineSpan.Path ?? string.Empty).Replace('\\', '/') + "|" +
                   location.SourceSpan.Start.ToString("D10", System.Globalization.CultureInfo.InvariantCulture) + "|" +
                   symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return "~|" + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static ObjectExpression BuildReturnedMembersExpression(ImmutableArray<string> returnedMembers)
        => new(NodeList.From<Node>(returnedMembers.Select(static name =>
            (Node)new ObjectProperty(
                PropertyKind.Init,
                new Identifier(name),
                new Identifier(name),
                computed: false,
                shorthand: true,
                method: false))));

    private static VariableDeclaration BuildVueComponentDeclaration(
        MemberClosure closure,
        string setupFactoryName,
        DirectRenderBuildResult directRender,
        VueModuleFeatures features,
        ImmutableArray<string> propNames)
    {
        var componentOptions = new List<Node>();
        if (propNames.Length > 0)
            componentOptions.Add(CreateObjectProperty("props", CreateStringArray(propNames)));

        var setupFunction = new FunctionExpression(
            id: null,
            parameters: NodeList.From<Node>(BuildVueSetupParameters(features)),
            body: CreateFunctionBody(BuildVueSetupStatements(
                setupFactoryName,
                directRender,
                features,
                propNames)),
            generator: false,
            async: false);
        componentOptions.Add(new ObjectProperty(
            PropertyKind.Init,
            new Identifier("setup"),
            setupFunction,
            computed: false,
            shorthand: false,
            method: true));

        return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            HmrComponentVariableName,
            CreateCall(
                "defineComponent",
                new ObjectExpression(NodeList.From(componentOptions))));
    }

    private static IfStatement BuildVueHmrRegistration(string moduleId)
    {
        // The module only frames a component identity here. Vue owns the actual instance
        // update protocol, while the development client decides whether a change is safe.
        Expression CreateHmrAccess()
            => CreateMemberAccess(new Identifier("globalThis"), "JazorHmr");

        var registrationAccess = CreateMemberAccess(CreateHmrAccess(), "registerVueComponent");
        var condition = new LogicalExpression(
            Operator.LogicalAnd,
            CreateHmrAccess(),
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new NonUpdateUnaryExpression(Operator.TypeOf, registrationAccess),
                StringLiteral("function")));
        var registration = CreateCallMember(
            CreateHmrAccess(),
            "registerVueComponent",
            StringLiteral(moduleId),
            new Identifier(HmrComponentVariableName));
        return new IfStatement(
            condition,
            CreateBlock(CreateExpressionStatement(registration)),
            null);
    }

    private static IEnumerable<Identifier> BuildSetupFactoryIdentifiers(VueModuleFeatures features)
    {
        if (features.UsesFactoryProps)
            yield return new Identifier("props");
        if (features.UsesFactorySlots)
            yield return new Identifier("slots");
        if (features.UsesStateHasChanged)
            yield return new Identifier("stateHasChanged");
        if (features.UsesInvokeAsync)
            yield return new Identifier("invokeAsync");
        foreach (var binding in features.InjectResolutions)
            yield return new Identifier(binding.LocalName);
        foreach (var binding in features.CascadingResolutions)
            yield return new Identifier(binding.LocalName);
        if (!features.CascadingBindings.IsDefaultOrEmpty)
            yield return new Identifier(CascadingMissingLocalName);
        if (!features.CascadingBindings.IsDefaultOrEmpty)
            yield return new Identifier("cascadeChanged");
    }

    private static IEnumerable<Expression> BuildSetupFactoryArguments(VueModuleFeatures features)
    {
        if (features.UsesFactoryProps)
            yield return new Identifier(features.UsesUnmatchedAttributes ? "parameterProps" : "props");
        if (features.UsesFactorySlots)
            yield return new Identifier("slots");
        if (features.UsesStateHasChanged)
            yield return new Identifier("stateHasChanged");
        if (features.UsesInvokeAsync)
            yield return new Identifier("invokeAsync");
        foreach (var binding in features.InjectResolutions)
            yield return new Identifier(binding.LocalName);
        foreach (var binding in features.CascadingResolutions)
            yield return new Identifier(binding.LocalName);
        if (!features.CascadingBindings.IsDefaultOrEmpty)
            yield return new Identifier(CascadingMissingLocalName);
        if (!features.CascadingBindings.IsDefaultOrEmpty)
            yield return new Identifier("cascadeChanged");
    }

    private static IEnumerable<Node> BuildVueSetupParameters(VueModuleFeatures features)
    {
        if (features.UsesSlots || features.UsesUnmatchedAttributes)
        {
            yield return new Identifier("props");
            var contextProperties = new List<Node>();
            if (features.UsesSlots)
            {
                var slots = new Identifier("slots");
                contextProperties.Add(new AssignmentProperty(
                    slots,
                    new Identifier("slots"),
                    computed: false,
                    shorthand: true));
            }

            if (features.UsesUnmatchedAttributes)
            {
                var attrs = new Identifier("attrs");
                contextProperties.Add(new AssignmentProperty(
                    attrs,
                    new Identifier("attrs"),
                    computed: false,
                    shorthand: true));
            }

            yield return new ObjectPattern(NodeList.From<Node>(contextProperties));
            yield break;
        }

        if (features.UsesSetupProps)
            yield return new Identifier("props");
    }

    private static List<Statement> BuildVueSetupStatements(
        string setupFactoryName,
        DirectRenderBuildResult directRender,
        VueModuleFeatures features,
        ImmutableArray<string> propNames)
    {
        var statements = new List<Statement>();

        // Setup owns the bridge from Vue lifecycle to component methods. Keep all mutable
        // protocol state in this closure so separate component instances never share it.
        // 生命周期与失效状态必须是 setup-instance scoped，不能泄漏为 module 全局状态。
        if (features.UsesUnmounted)
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "disposed", BooleanLiteral(false)));

        if (features.UsesStateHasChanged)
        {
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "invalidate", NullLiteral()));
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "pendingInvalidations", NumericLiteral(0)));
            statements.Add(BuildStateHasChangedDeclaration(features.UsesUnmounted));
        }

        if (features.UsesInvokeAsync)
            statements.Add(BuildInvokeAsyncDeclaration(features.UsesUnmounted));

        foreach (var binding in features.InjectBindings)
        {
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                binding.LocalName,
                CreateCall("inject", StringLiteral(binding.ServiceKey))));
            statements.Add(new IfStatement(
                new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new NonUpdateUnaryExpression(
                        Operator.TypeOf,
                        new Identifier(binding.LocalName)),
                    StringLiteral("undefined")),
                CreateBlock(new ThrowStatement(new NewExpression(
                    new Identifier("Error"),
                    NodeList.From<Expression>(StringLiteral(
                        "RazorVue could not resolve injected service '" +
                        binding.ServiceTypeDisplay +
                        "' for component '" +
                        features.ComponentDisplayName +
                        "'. Register it with the Vue app provider key '" +
                        binding.ServiceKey + "'."))))),
                null));
        }

        if (!features.CascadingBindings.IsDefaultOrEmpty)
        {
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                CascadingMissingLocalName,
                new ObjectExpression(NodeList.Empty<Node>())));
            foreach (var binding in features.CascadingResolutions)
            {
                statements.Add(CreateVariableDeclaration(
                    VariableDeclarationKind.Const,
                    binding.LocalName,
                    CreateCall(
                        "inject",
                        StringLiteral(binding.ServiceKey),
                        new Identifier(CascadingMissingLocalName))));
            }
        }

        if (features.UsesUnmatchedAttributes)
        {
            // Vue keeps undeclared component attributes in setup context.attrs. A Blazor
            // CaptureUnmatchedValues parameter must see those values through the ordinary
            // authored property, while declared props remain on Vue's reactive props proxy.
            // 未匹配属性只在 framing 层合并，作者仍然只接触标准 AdditionalAttributes 属性。
            statements.AddRange(BuildUnmatchedAttributeSetupStatements(features));
        }

        if (features.UsesParameterViewState && !features.CascadingBindings.IsDefaultOrEmpty)
        {
            // ParameterView needs the current cascade values as sparse entries, but the
            // custom SetParametersAsync implementation remains responsible for applying them.
            // ParameterView 以稀疏项携带当前级联值；何时写入属性仍由作者的 SetParametersAsync 决定。
            statements.Add(BuildCascadingParameterSnapshotDeclaration(features.CascadingBindings));
        }

        if (!features.CascadingBindings.IsDefaultOrEmpty)
        {
            // The cascade provider installs its watcher while the setup factory is created.
            // Keep the callback instance-scoped and let it observe the completed scope after
            // setup assignment, so a provider update runs the same parameter lifecycle as a
            // normal Blazor parameter update instead of only invalidating the DOM.
            // 级联 watcher 在 factory 内注册；callback 通过 let scope 延迟绑定，确保更新时复用
            // 标准参数生命周期，而不是只调用 StateHasChanged。
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Let,
                "scope",
                NullLiteral()));
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "cascadeChanged",
                CreateArrowFunction(
                    [],
                    BuildCascadeChangedStatements(features))));
            statements.Add(CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("scope"),
                CreateCall(
                    setupFactoryName,
                    BuildSetupFactoryArguments(features)))));
        }
        else
        {
            statements.Add(CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "scope",
                CreateCall(
                    setupFactoryName,
                    BuildSetupFactoryArguments(features))));
        }

        if (features.UsesStateHasChanged)
        {
            statements.Add(CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("invalidate"),
                CreateCall(
                    "reactive",
                    new ObjectExpression(NodeList.From<Node>(CreateObjectProperty(
                        "tick",
                        new Identifier("pendingInvalidations"))))))));
        }

        if (!features.UsesParameterViewState && features.OnInitialized is { } onInitialized)
            statements.Add(CreateExpressionStatement(CreateScopeCall(onInitialized)));

        if (!features.UsesParameterViewState && features.OnInitializedAsync is { } onInitializedAsync)
        {
            statements.Add(CreateExpressionStatement(CreateCallMember(
                CreateCallMember(new Identifier("Promise"), "resolve", CreateScopeCall(onInitializedAsync)),
                "then",
                CreateStateHasChangedCallback(),
                CreateStateHasChangedCallback())));
        }

        if (!features.UsesParameterViewState && features.OnParametersSet is { } onParametersSet)
        {
            statements.Add(CreateExpressionStatement(CreateScopeCall(onParametersSet)));
            statements.AddRange(CreateWatchStatements(
                onParametersSet,
                propNames,
                features.UsesUnmatchedAttributes));
        }

        if (!features.UsesParameterViewState && features.OnParametersSetAsync is { } onParametersSetAsync)
            statements.AddRange(BuildOnParametersSetAsyncStatements(
                onParametersSetAsync,
                propNames,
                features.UsesUnmatchedAttributes));

        if (features.UsesParameterViewState)
        {
            // setup cannot await a ComponentBase.SetParametersAsync Task. Keep a rejected task
            // observable by the next render so Vue's regular error boundary/application error
            // path receives the authored exception instead of a swallowed queue rejection.
            // setup 无法 await 参数 Task；下一次 render 必须重新抛出作者异常，不能吞掉队列 rejection。
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "hasParameterFailure", BooleanLiteral(false)));
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "parameterFailure", NullLiteral()));
            statements.AddRange(BuildParameterViewSetupStatements(
                features.ParameterBindings,
                features.UsesParameterViewSlots,
                features.UsesUnmatchedAttributes,
                !features.CascadingBindings.IsDefaultOrEmpty));
        }

        if (features.OnAfterRender is { } onAfterRender)
        {
            statements.Add(CreateLifecycleRegistration("onMounted", onAfterRender, BooleanLiteral(true), discardResult: false));
            statements.Add(CreateLifecycleRegistration("onUpdated", onAfterRender, BooleanLiteral(false), discardResult: false));
        }

        if (features.OnAfterRenderAsync is { } onAfterRenderAsync)
        {
            statements.Add(CreateLifecycleRegistration("onMounted", onAfterRenderAsync, BooleanLiteral(true), discardResult: true));
            statements.Add(CreateLifecycleRegistration("onUpdated", onAfterRenderAsync, BooleanLiteral(false), discardResult: true));
        }

        if (features.UsesUnmounted)
            statements.Add(BuildUnmountedRegistration(features.DisposeMemberNames, features.DisposeAsyncMemberNames));

        if (features.HasShouldRender)
        {
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "hasRendered", BooleanLiteral(false)));
            statements.Add(CreateVariableDeclaration(VariableDeclarationKind.Let, "cachedVNode", NullLiteral()));
        }

        statements.Add(new ReturnStatement(BuildRenderClosure(directRender, features)));
        return statements;
    }

    private static IEnumerable<Statement> BuildUnmatchedAttributeSetupStatements(
        VueModuleFeatures features)
    {
        var capture = features.ParameterBindings
            .Where(static binding => binding.CapturesUnmatchedValues)
            .ToArray();
        if (capture.Length == 0)
            yield break;
        if (capture.Length != 1)
        {
            throw new InvalidOperationException(
                "A Razor component can declare at most one CaptureUnmatchedValues parameter.");
        }

        var binding = capture[0];
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "parameterProps",
            CreateCallMember(
                new Identifier("Object"),
                "create",
                new Identifier("props")));

        var capturedValue = CreateMemberAccess(new Identifier("props"), binding.RuntimeName);
        var attrsValue = new Identifier("attrs");
        var merged = CreateCallMember(
            new Identifier("Object"),
            "assign",
            new ObjectExpression(NodeList.Empty<Node>()),
            new LogicalExpression(
                Operator.LogicalOr,
                capturedValue,
                new ObjectExpression(NodeList.Empty<Node>())),
            new LogicalExpression(
                Operator.LogicalOr,
                attrsValue,
                new ObjectExpression(NodeList.Empty<Node>())));
        var descriptor = new ObjectExpression(NodeList.From<Node>(
            CreateObjectProperty("enumerable", BooleanLiteral(true)),
            CreateObjectProperty("configurable", BooleanLiteral(true)),
            CreateObjectProperty("get", CreateArrowExpression(merged))));

        // Keep the wrapper stable so compiler-generated `props.OtherParameter` reads remain
        // reactive through the prototype, while the capture getter always sees current attrs.
        // wrapper 保持稳定，普通 props 仍走 Vue proxy；capture getter 每次读取最新 attrs。
        yield return CreateExpressionStatement(CreateCallMember(
            new Identifier("Object"),
            "defineProperty",
            new Identifier("parameterProps"),
            StringLiteral(binding.RuntimeName),
            descriptor));

        // Vue's attrs object is reactive but intentionally shallow. Deep watch is scoped to
        // this adapter source so ordinary Blazor parameters retain their reference semantics.
        // 只有 unmatched attrs 使用 deep watch，避免普通参数因嵌套变化误触生命周期。
    }

    private static IEnumerable<Statement> BuildCascadeChangedStatements(VueModuleFeatures features)
    {
        // A provider watcher can be registered before the outer setup scope is assigned. The
        // guard is only for eager test/runtime adapters; Vue's normal watch callback runs after
        // setup has completed. Once available, preserve Blazor's parameter-update ordering:
        // apply the new cascading value, run OnParametersSet* and then invalidate the render.
        yield return new IfStatement(
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new Identifier("scope"),
                NullLiteral()),
            CreateBlock(new ReturnStatement(null)),
            null);

        if (features.UsesParameterViewState)
        {
            // A custom SetParametersAsync controls when its properties are applied. Feed the
            // changed cascade through its normal serialized ParameterView queue so code before
            // base.SetParametersAsync still observes the old value, just as it does in Blazor.
            // 自定义 SetParametersAsync 必须经 ParameterView 队列；不能提前写 state 绕过作者逻辑。
            yield return CreateExpressionStatement(new CallExpression(
                new Identifier("runSetParametersAsync"),
                NodeList.From<Expression>(CreateParameterViewSnapshotExpression(
                    features.UsesParameterViewSlots,
                    features.UsesUnmatchedAttributes,
                    hasCascadingParameters: true)),
                optional: false));
            yield break;
        }

        if (features.OnParametersSet is { } onParametersSet)
        {
            yield return CreateExpressionStatement(CreateScopeCall(onParametersSet));
            if (features.OnParametersSetAsync is null)
                yield return CreateExpressionStatement(CreateCall("stateHasChanged"));
        }

        if (features.OnParametersSetAsync is not null)
        {
            // `runOnParametersSetAsync` is declared later in setup, but the callback is invoked
            // only after the factory returns, so the lexical binding is initialized by then.
            yield return CreateExpressionStatement(CreateCall("runOnParametersSetAsync"));
        }
        else if (features.OnParametersSet is null)
        {
            yield return CreateExpressionStatement(CreateCall("stateHasChanged"));
        }
    }

    private static VariableDeclaration BuildCascadingParameterSnapshotDeclaration(
        ImmutableArray<CascadingBinding> cascadingBindings)
    {
        var statements = new List<Statement>
        {
            CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "cascadeParameters",
                new ObjectExpression(NodeList.Empty<Node>()))
        };

        foreach (var binding in cascadingBindings)
        {
            statements.Add(new IfStatement(
                new NonLogicalBinaryExpression(
                    Operator.StrictInequality,
                    new Identifier(binding.LocalName),
                    new Identifier(CascadingMissingLocalName)),
                CreateBlock(CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("cascadeParameters"), binding.Property.Name),
                    CreateCall("unref", new Identifier(binding.LocalName))))),
                null));
        }

        statements.Add(new ReturnStatement(new Identifier("cascadeParameters")));
        return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "cascadingParameterSnapshot",
            CreateArrowFunction([], statements));
    }

    private static ObjectExpression BuildParameterAdapterExpression(VueModuleFeatures features)
    {
        var properties = new List<Node>();
        properties.Add(CreateObjectProperty(
            "applyParameterProperties",
            CreateArrowFunction(
                ["parameters"],
                BuildParameterOverlayStatements(
                    features.ParameterBindings,
                    features.CascadingBindings))));
        properties.Add(CreateObjectProperty(
            "createSnapshot",
            CreateArrowFunction(
                ["parameters", "slotValues", "rawParameters", "cascadingParameters"],
                BuildParameterSnapshotStatements(features.ParameterBindings))));
        properties.Add(CreateObjectProperty(
            "applyComponentBaseParameters",
            CreateArrowFunction(
                ["parameters"],
                BuildComponentBaseParameterStatements(features))));
        return new ObjectExpression(NodeList.From(properties));
    }

    private static List<Statement> BuildParameterOverlayStatements(
        ImmutableArray<ParameterBinding> parameterBindings,
        ImmutableArray<CascadingBinding> cascadingBindings)
    {
        var statements = new List<Statement>();
        foreach (var binding in parameterBindings)
        {
            // Presence, rather than value, is the ParameterView contract: an explicitly supplied
            // undefined must overwrite the old value, while an omitted parameter must not.
            // 必须区分“显式 undefined”和“参数缺失”，否则会破坏 Blazor 的旧值保留语义。
            var hasParameter = CreateHasOwnExpression(new Identifier("parameters"), binding.SourceName);
            statements.Add(new IfStatement(
                hasParameter,
                CreateBlock(CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("state"), binding.StateName),
                    CreateMemberAccess(new Identifier("parameters"), binding.SourceName)))),
                null));
        }

        foreach (var binding in cascadingBindings)
        {
            var sourceName = binding.Property.Name;
            statements.Add(new IfStatement(
                CreateHasOwnExpression(new Identifier("parameters"), sourceName),
                CreateBlock(CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("state"), binding.StateName),
                    CreateMemberAccess(new Identifier("parameters"), sourceName)))),
                null));
        }

        return statements;
    }

    private static List<Statement> BuildParameterSnapshotStatements(
        ImmutableArray<ParameterBinding> parameterBindings)
    {
        var statements = new List<Statement>
        {
            CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "snapshot",
                CreateCallMember(
                    new Identifier("Object"),
                    "assign",
                    new ObjectExpression(NodeList.Empty<Node>()),
                    new LogicalExpression(
                        Operator.LogicalOr,
                        new Identifier("rawParameters"),
                        new ObjectExpression(NodeList.Empty<Node>())),
                    new Identifier("parameters"),
                    new LogicalExpression(
                        Operator.LogicalOr,
                        new Identifier("cascadingParameters"),
                        new ObjectExpression(NodeList.Empty<Node>()))))
        };

        foreach (var binding in parameterBindings)
        {
            var source = binding.IsSlot
                ? (Expression)new LogicalExpression(
                    Operator.LogicalAnd,
                    new NonLogicalBinaryExpression(
                        Operator.StrictInequality,
                        new Identifier("slotValues"),
                        NullLiteral()),
                    CreateHasOwnExpression(new Identifier("slotValues"), binding.RuntimeName))
                : CreateHasOwnExpression(
                    binding.CapturesUnmatchedValues
                        ? new Identifier("parameters")
                        : new LogicalExpression(
                            Operator.LogicalOr,
                            new Identifier("rawParameters"),
                            new Identifier("parameters")),
                    binding.RuntimeName);
            var value = binding.IsSlot
                ? CreateMemberAccess(new Identifier("slotValues"), binding.RuntimeName)
                : CreateMemberAccess(
                    binding.CapturesUnmatchedValues
                        ? new Identifier("parameters")
                        : new LogicalExpression(
                            Operator.LogicalOr,
                            new Identifier("rawParameters"),
                            new Identifier("parameters")),
                    binding.RuntimeName);
            statements.Add(new IfStatement(
                source,
                CreateBlock(CreateExpressionStatement(new AssignmentExpression(
                    Operator.Assignment,
                    CreateMemberAccess(new Identifier("snapshot"), binding.SourceName),
                    value))),
                null));
        }

        statements.Add(new ReturnStatement(new Identifier("snapshot")));
        return statements;
    }

    private static CallExpression CreateHasOwnExpression(Expression source, string name)
        => new(
            CreateMemberAccess(
                CreateMemberAccess(
                    CreateMemberAccess(new Identifier("Object"), "prototype"),
                    "hasOwnProperty"),
                "call"),
            NodeList.From<Expression>(source, StringLiteral(name)),
            optional: false);

    private static List<Statement> BuildComponentBaseParameterStatements(VueModuleFeatures features)
    {
        var statements = new List<Statement>
        {
            CreateExpressionStatement(CreateCallMember(
                new Identifier("parameterAdapter"),
                "applyParameterProperties",
                new Identifier("parameters")))
        };

        var firstInitialization = new List<Statement>
        {
            CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("initialized"),
                BooleanLiteral(true)))
        };
        if (features.OnInitialized is { } onInitialized)
            firstInitialization.Add(CreateExpressionStatement(CreateCall(onInitialized)));

        Expression initializationTask = CreateCallMember(
            new Identifier("Promise"),
            "resolve");
        if (features.OnInitializedAsync is { } onInitializedAsync)
        {
            initializationTask = CreateCallMember(
                new Identifier("Promise"),
                "resolve",
                CreateCall(onInitializedAsync));
        }

        var parameterPhaseStatements = new List<Statement>();
        if (features.OnParametersSet is { } onParametersSet)
            parameterPhaseStatements.Add(CreateExpressionStatement(CreateCall(onParametersSet)));
        parameterPhaseStatements.Add(new ReturnStatement(BuildParameterAsyncResult(features)));

        var continueWithParameters = CreateCallMember(
            initializationTask,
            "then",
            CreateArrowFunction([], parameterPhaseStatements));
        firstInitialization.Add(new ReturnStatement(continueWithParameters));

        var subsequent = new List<Statement>();
        if (features.OnParametersSet is { } subsequentOnParametersSet)
            subsequent.Add(CreateExpressionStatement(CreateCall(subsequentOnParametersSet)));
        subsequent.Add(new ReturnStatement(BuildParameterAsyncResult(features)));

        statements.Add(new IfStatement(
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new Identifier("initialized"),
                BooleanLiteral(false)),
            CreateBlock(firstInitialization.ToArray()),
            CreateBlock(subsequent.ToArray())));
        return statements;
    }

    private static IEnumerable<Statement> BuildParameterViewSetupStatements(
        ImmutableArray<ParameterBinding> parameterBindings,
        bool usesSlots,
        bool usesUnmatchedAttributes,
        bool hasCascadingParameters)
    {
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "parameterSnapshot",
            CreateParameterViewSnapshotExpression(
                usesSlots,
                usesUnmatchedAttributes,
                hasCascadingParameters));
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Let,
            "parameterUpdateTail",
            CreateCallMember(new Identifier("Promise"), "resolve"));

        var task = new Identifier("task");
        var queueBody = new List<Statement>
        {
            CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("hasParameterFailure"),
                BooleanLiteral(false))),
            CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "task",
                CreateCallMember(
                    new Identifier("parameterUpdateTail"),
                    "then",
                    CreateArrowFunction(
                        [],
                        [new ReturnStatement(CreateScopeCall("SetParametersAsync", new Identifier("parameters")))]))),
            CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("parameterUpdateTail"),
                CreateCallMember(
                    task,
                    "then",
                    CreateArrowFunction([], []),
                    CreateArrowFunction(
                        ["error"],
                        [
                            CreateExpressionStatement(new AssignmentExpression(
                                Operator.Assignment,
                                new Identifier("hasParameterFailure"),
                                BooleanLiteral(true))),
                            CreateExpressionStatement(new AssignmentExpression(
                                Operator.Assignment,
                                new Identifier("parameterFailure"),
                                new Identifier("error"))),
                            CreateExpressionStatement(CreateCall("stateHasChanged"))
                        ])))),
            new ReturnStatement(task)
        };
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "runSetParametersAsync",
            CreateArrowFunction(["parameters"], queueBody));
        yield return CreateExpressionStatement(new CallExpression(
            new Identifier("runSetParametersAsync"),
            NodeList.From<Expression>(new Identifier("parameterSnapshot")),
            optional: false));
        if (!parameterBindings.IsDefaultOrEmpty)
        {
            yield return CreateExpressionStatement(CreateCall(
                "watch",
                 CreateParameterWatchSource(parameterBindings),
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(new CallExpression(
                        new Identifier("runSetParametersAsync"),
                        NodeList.From<Expression>(CreateParameterViewSnapshotExpression(
                            usesSlots,
                            usesUnmatchedAttributes,
                            hasCascadingParameters)),
                        optional: false))])));
        }

        if (usesUnmatchedAttributes)
        {
            // Vue exposes undeclared attrs through a reactive setup-context object. Keep this
            // watcher separate from the shallow declared-prop watcher so nested ordinary props
            // do not accidentally become Blazor parameter assignments.
            yield return CreateUnmatchedAttributesWatchStatement(
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(new CallExpression(
                        new Identifier("runSetParametersAsync"),
                        NodeList.From<Expression>(CreateParameterViewSnapshotExpression(
                            usesSlots,
                            usesUnmatchedAttributes,
                            hasCascadingParameters)),
                        optional: false))]));
        }
    }

    private static CallExpression CreateParameterViewSnapshotExpression(
        bool usesSlots,
        bool usesUnmatchedAttributes,
        bool hasCascadingParameters)
        => CreateCallMember(
            CreateMemberAccess(new Identifier("scope"), "parameterAdapter"),
            "createSnapshot",
            new Identifier(usesUnmatchedAttributes ? "parameterProps" : "props"),
            usesSlots ? new Identifier("slots") : NullLiteral(),
            new Identifier("props"),
            hasCascadingParameters
                ? CreateCall("cascadingParameterSnapshot")
                : NullLiteral());

    private static Expression BuildParameterAsyncResult(VueModuleFeatures features)
        => features.OnParametersSetAsync is { } onParametersSetAsync
            ? CreateCallMember(
                new Identifier("Promise"),
                "resolve",
                CreateCall(onParametersSetAsync))
            : new Identifier("undefined");

    private static VariableDeclaration BuildStateHasChangedDeclaration(bool usesUnmounted)
    {
        // Calls before reactive invalidation is ready are counted, then folded into its initial
        // tick. This preserves lifecycle calls made while setup is still being constructed.
        // setup 早期 StateHasChanged 不能丢失，先计数并在 invalidate 建立后一次性体现。
        var body = new List<Statement>();
        if (usesUnmounted)
        {
            body.Add(new IfStatement(
                new Identifier("disposed"),
                CreateBlock(new ThrowStatement(new NewExpression(
                    new Identifier("Error"),
                    NodeList.From<Expression>(StringLiteral(
                        "RazorVue component is disposed; StateHasChanged cannot run after unmount."))))),
                null));
        }

        body.Add(new IfStatement(
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new Identifier("invalidate"),
                NullLiteral()),
            CreateBlock(
                CreateExpressionStatement(new UpdateExpression(
                    Operator.Increment,
                    new Identifier("pendingInvalidations"),
                    prefix: false)),
                new ReturnStatement(null)),
            null));
        body.Add(CreateExpressionStatement(new UpdateExpression(
            Operator.Increment,
            CreateMemberAccess(new Identifier("invalidate"), "tick"),
            prefix: false)));
        return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "stateHasChanged",
            CreateArrowFunction([], body));
    }

    private static VariableDeclaration BuildInvokeAsyncDeclaration(bool usesUnmounted)
    {
        var body = new List<Statement>();
        if (usesUnmounted)
        {
            body.Add(new IfStatement(
                new Identifier("disposed"),
                CreateBlock(new ReturnStatement(CreateCallMember(
                    new Identifier("Promise"),
                    "reject",
                    new NewExpression(
                        new Identifier("Error"),
                        NodeList.From<Expression>(StringLiteral(
                            "RazorVue component is disposed; InvokeAsync cannot run after unmount.")))))),
                null));
        }

        var tryBody = CreateBlock(new ReturnStatement(CreateCallMember(
            new Identifier("Promise"),
            "resolve",
            CreateCall("workItem"))));
        var error = new Identifier("error");
        var catchClause = new CatchClause(
            error,
            CreateBlock(new ReturnStatement(CreateCallMember(
                new Identifier("Promise"),
                "reject",
                new Identifier("error")))));
        body.Add(new TryStatement(tryBody, catchClause, null));
        return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "invokeAsync",
            CreateArrowFunction(["workItem"], body));
    }

    private static ArrowFunctionExpression CreateStateHasChangedCallback()
        => CreateArrowFunction(
            [],
            [CreateExpressionStatement(CreateCall("stateHasChanged"))]);

    private static IEnumerable<Statement> CreateWatchStatements(
        string scopeMethod,
        ImmutableArray<string> propNames,
        bool usesUnmatchedAttributes)
    {
        // Do not register an empty array source. Vue correctly treats it as stable, but test
        // adapters and custom schedulers may still invoke its callback and create a spurious
        // OnParametersSet pass for cascade-only components.
        // 无普通参数时不注册空 watcher，避免级联组件出现额外的 OnParametersSet。
        if (!propNames.IsDefaultOrEmpty)
        {
            yield return CreateExpressionStatement(CreateCall(
                "watch",
                CreateParameterWatchSource(propNames),
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(CreateScopeCall(scopeMethod))])));
        }

        if (usesUnmatchedAttributes)
        {
            yield return CreateUnmatchedAttributesWatchStatement(
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(CreateScopeCall(scopeMethod))]));
        }
    }

    private static Statement CreateUnmatchedAttributesWatchStatement(
        ArrowFunctionExpression callback)
        => CreateExpressionStatement(CreateCall(
            "watch",
            CreateArrowExpression(new Identifier("attrs")),
            callback,
            new ObjectExpression(NodeList.From<Node>(
                CreateObjectProperty("deep", BooleanLiteral(true))))));

    private static IEnumerable<Statement> BuildOnParametersSetAsyncStatements(
        string scopeMethod,
        ImmutableArray<string> propNames,
        bool usesUnmatchedAttributes)
    {
        // Serialize parameter callbacks and version each run. Vue may publish newer props while
        // an earlier Task is pending; only the newest completion may request a rerender.
        // 异步 ParametersSet 必须串行且淘汰旧完成回调，避免旧任务覆盖最新 props 状态。
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Let,
            "parametersSetAsyncGen",
            NumericLiteral(0));
        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Let,
            "parametersSetAsyncTail",
            CreateCallMember(new Identifier("Promise"), "resolve"));

        var generation = new Identifier("gen");
        var runBody = new List<Statement>
        {
            CreateVariableDeclaration(
                VariableDeclarationKind.Const,
                "gen",
                new UpdateExpression(
                    Operator.Increment,
                    new Identifier("parametersSetAsyncGen"),
                    prefix: true))
        };
        var skipStaleGeneration = new IfStatement(
            new NonLogicalBinaryExpression(
                Operator.StrictInequality,
                new Identifier("gen"),
                new Identifier("parametersSetAsyncGen")),
            CreateBlock(new ReturnStatement(null)),
            null);
        var invokeCurrentGeneration = new ReturnStatement(CreateCallMember(
            CreateCallMember(
                new Identifier("Promise"),
                "resolve",
                CreateScopeCall(scopeMethod)),
            "then",
            CreateParametersSetCompletionCallback(),
            CreateParametersSetCompletionCallback()));
        var thenCallback = CreateArrowFunction(
            [],
            [skipStaleGeneration, invokeCurrentGeneration]);
        var chainedTail = CreateCallMember(
            CreateCallMember(
                new Identifier("parametersSetAsyncTail"),
                "catch",
                CreateArrowFunction([], [])),
            "then",
            thenCallback);
        runBody.Add(CreateExpressionStatement(new AssignmentExpression(
            Operator.Assignment,
            new Identifier("parametersSetAsyncTail"),
            chainedTail)));

        yield return CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "runOnParametersSetAsync",
            CreateArrowFunction([], runBody));
        yield return CreateExpressionStatement(CreateCall("runOnParametersSetAsync"));
        if (!propNames.IsDefaultOrEmpty)
        {
            yield return CreateExpressionStatement(CreateCall(
                "watch",
                CreateParameterWatchSource(propNames),
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(CreateCall("runOnParametersSetAsync"))])));
        }

        if (usesUnmatchedAttributes)
        {
            yield return CreateUnmatchedAttributesWatchStatement(
                CreateArrowFunction(
                    [],
                    [CreateExpressionStatement(CreateCall("runOnParametersSetAsync"))]));
        }
    }

    private static ArrowFunctionExpression CreateParametersSetCompletionCallback()
        => CreateArrowFunction(
            [],
            [new IfStatement(
                new NonLogicalBinaryExpression(
                    Operator.StrictEquality,
                    new Identifier("gen"),
                    new Identifier("parametersSetAsyncGen")),
                CreateBlock(CreateExpressionStatement(CreateCall("stateHasChanged"))),
                null)]);

    private static ArrowFunctionExpression CreateParameterWatchSource(
        ImmutableArray<ParameterBinding> parameterBindings)
    {
        // ComponentBase parameter lifecycle is keyed by the parent's supplied parameter values.
        // Vue mutates the stable props proxy in place, so watching `props` itself cannot observe
        // replacement. Project declared props into a shallow array: value/reference replacement
        // triggers once, while nested mutation of the same object is explicitly not a new parameter set.
        // 只观察声明参数的浅层 value/reference；同一引用内部变更不属于参数重新赋值。
        var values = parameterBindings.SelectMany(binding =>
        {
            var source = new Identifier(binding.IsSlot ? "slots" : "props");
            return new Expression?[]
            {
                CreateHasOwnExpression(source, binding.RuntimeName),
                CreateMemberAccess(source, binding.RuntimeName)
            };
        });
        return CreateArrowExpression(new ArrayExpression(NodeList.From(values)));
    }

    private static ArrowFunctionExpression CreateParameterWatchSource(
        ImmutableArray<string> propNames)
    {
        var values = propNames.Select(static name => (Expression?)CreateMemberAccess(
            new Identifier("props"),
            name));
        return CreateArrowExpression(new ArrayExpression(NodeList.From(values)));
    }

    private static Statement CreateLifecycleRegistration(
        string vueLifecycleMethod,
        string scopeMethod,
        BooleanLiteral firstRender,
        bool discardResult)
    {
        // Vue does not await lifecycle callbacks. Async ComponentBase methods are deliberately
        // observed through Promise.resolve while the component update protocol remains explicit.
        // Vue hook 不等待 Task；这里确保 rejected/fulfilled 都不会改变已定义的 render 调度边界。
        Expression invocation = CreateScopeCall(scopeMethod, firstRender);
        if (discardResult)
        {
            invocation = new NonUpdateUnaryExpression(
                Operator.Void,
                CreateCallMember(new Identifier("Promise"), "resolve", invocation));
        }

        return CreateExpressionStatement(CreateCall(
            vueLifecycleMethod,
            CreateArrowFunction([], [CreateExpressionStatement(invocation)])));
    }

    private static Statement BuildUnmountedRegistration(
        ImmutableArray<string> disposeMemberNames,
        ImmutableArray<string> disposeAsyncMemberNames)
    {
        var body = new List<Statement>();
        foreach (var disposeMemberName in disposeMemberNames)
            body.Add(CreateExpressionStatement(CreateScopeCall(disposeMemberName)));

        foreach (var disposeAsyncMemberName in disposeAsyncMemberNames)
        {
            body.Add(CreateExpressionStatement(new NonUpdateUnaryExpression(
                Operator.Void,
                CreateScopeCall(disposeAsyncMemberName))));
        }

        body.Add(CreateExpressionStatement(new AssignmentExpression(
            Operator.Assignment,
            new Identifier("disposed"),
            BooleanLiteral(true))));
        return CreateExpressionStatement(CreateCall(
            "onUnmounted",
            CreateArrowFunction([], body)));
    }

    private static ArrowFunctionExpression BuildRenderClosure(
        DirectRenderBuildResult directRender,
        VueModuleFeatures features)
    {
        var body = new List<Statement>();

        if (features.UsesStateHasChanged)
        {
            body.Add(CreateExpressionStatement(CreateMemberAccess(
                new Identifier("invalidate"),
                "tick")));
        }

        if (features.UsesParameterViewState)
        {
            body.Add(new IfStatement(
                new Identifier("hasParameterFailure"),
                CreateBlock(new ThrowStatement(new Identifier("parameterFailure"))),
                null));
        }

        if (features.ShouldRender is { } shouldRender)
        {
            body.Add(new IfStatement(
                new LogicalExpression(
                    Operator.LogicalAnd,
                    new Identifier("hasRendered"),
                    new NonUpdateUnaryExpression(
                        Operator.LogicalNot,
                        CreateScopeCall(shouldRender))),
                CreateBlock(new ReturnStatement(new Identifier("cachedVNode"))),
                null));
            body.Add(CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("hasRendered"),
                BooleanLiteral(true))));
        }

        var renderExpression = CreateCallMember(new Identifier("scope"), directRender.MemberName);

        if (features.HasShouldRender)
        {
            body.Add(CreateExpressionStatement(new AssignmentExpression(
                Operator.Assignment,
                new Identifier("cachedVNode"),
                renderExpression)));
            body.Add(new ReturnStatement(new Identifier("cachedVNode")));
        }
        else
        {
            body.Add(new ReturnStatement(renderExpression));
        }

        return CreateArrowFunction([], body);
    }

    private static ImmutableArray<string> GetVuePropNames(MemberClosure closure)
        => LibraryComponentConventions.GetEffectiveParameterProperties(closure.ComponentSymbol)
            .Where(static property => !IsAnyRenderFragmentType(property.Type))
            .Select(property => IsEventCallbackType(property.Type)
                ? LibraryComponentConventions.GetEventListenerRuntimeName(
                    closure.ComponentSymbol,
                    property)
                : LibraryComponentConventions.GetPropRuntimeName(property))
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToImmutableArray();

    private static ImmutableArray<ParameterBinding> BuildParameterBindings(MemberClosure closure)
        => closure.ParameterProperties
            .Select(property =>
            {
                var isSlot = IsAnyRenderFragmentType(property.Type);
                var runtimeName = isSlot
                    ? LibraryComponentConventions.GetSlotRuntimeName(closure.ComponentSymbol, property)
                    : IsEventCallbackType(property.Type)
                        ? LibraryComponentConventions.GetEventListenerRuntimeName(closure.ComponentSymbol, property)
                        : LibraryComponentConventions.GetPropRuntimeName(property);
                return new ParameterBinding(
                    property.Name,
                    runtimeName,
                    Util.GetConfigOrSymbolName(property),
                    isSlot,
                    LibraryComponentConventions.CapturesUnmatchedValues(property));
            })
            .ToImmutableArray();

    private static ImmutableArray<InjectBinding> BuildInjectBindings(MemberClosure closure)
    {
        var bindings = ImmutableArray.CreateBuilder<InjectBinding>();
        var localsByServiceKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in closure.InjectProperties)
        {
            var serviceKey = LibraryComponentConventions.GetInjectServiceKey(property);
            if (!localsByServiceKey.TryGetValue(serviceKey, out var localName))
            {
                localName = GetInjectLocalName(serviceKey);
                localsByServiceKey.Add(serviceKey, localName);
                bindings.Add(new InjectBinding(
                    property,
                    serviceKey,
                    localName,
                    Util.GetConfigOrSymbolName(property),
                    property.Type.ToDisplayString(Format.NameFormat)));
                continue;
            }

            // Multiple [Inject] properties of the same service type share one Vue inject call,
            // matching a DI provider lookup while keeping setup deterministic and inexpensive.
            // 同类型服务只解析一次，多个属性仍各自写入自己的 state slot。
            bindings.Add(new InjectBinding(
                property,
                serviceKey,
                localName,
                Util.GetConfigOrSymbolName(property),
                property.Type.ToDisplayString(Format.NameFormat)));
        }

        return bindings.ToImmutable();
    }

    private static ImmutableArray<CascadingBinding> BuildCascadingBindings(MemberClosure closure)
    {
        var bindings = ImmutableArray.CreateBuilder<CascadingBinding>();
        var localsByServiceKey = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in closure.CascadingParameterProperties)
        {
            var serviceKey = LibraryComponentConventions.GetCascadingServiceKey(property);
            if (!localsByServiceKey.TryGetValue(serviceKey, out var localName))
            {
                localName = GetCascadingLocalName(serviceKey);
                localsByServiceKey.Add(serviceKey, localName);
            }

            bindings.Add(new CascadingBinding(
                property,
                serviceKey,
                localName,
                Util.GetConfigOrSymbolName(property)));
        }

        return bindings.ToImmutable();
    }

    private static string GetInjectLocalName(string serviceKey)
        => "__jazor$inject$" + Format.HashName(serviceKey).TrimStart('_');

    private static string GetCascadingLocalName(string serviceKey)
        => "__jazor$cascade$" + Format.HashName(serviceKey).TrimStart('_');

    private static ImmutableArray<CompiledLineMapping> BuildCompiledLineMappings(
        IReadOnlyDictionary<Node, GeneratedNodePosition> generatedNodePositions,
        CompilerModuleParts parts)
    {
        var mappings = new HashSet<CompiledLineMapping>();
        foreach (var pair in generatedNodePositions)
        {
            if (pair.Key.UserData is not SourceOrigin { IsSynthetic: false } origin)
                continue;

            // Direct RenderTree lowering creates Vue AST nodes after the generic compiler has
            // emitted its setup module. Those nodes still carry their C# origin, but naturally
            // have no entry in CompilerOriginPositions. Preserve that origin so the chained
            // Razor map retains each authored fragment instead of collapsing to BuildRenderTree.
            // 直接 h(...) 节点不经过 compiler module；缺少 compiler position 时仍应保留其真实 C# 坐标。
            var hasCompilerPosition = parts.CompilerOriginPositions.TryGetValue(origin, out var position);
            var compiledPosition = hasCompilerPosition
                ? position
                : new GeneratedNodePosition(origin.StartLine, origin.StartColumn);

            mappings.Add(new CompiledLineMapping(
                pair.Value.Line,
                pair.Value.Column,
                compiledPosition.Line,
                compiledPosition.Column,
                IsDirectRenderOrigin: !hasCompilerPosition));
        }

        foreach (var statement in parts.ModuleStatements.Concat(parts.SetupStatements))
        {
            if (generatedNodePositions.TryGetValue(statement.Statement, out var generatedPosition))
            {
                mappings.Add(new CompiledLineMapping(
                    generatedPosition.Line,
                    generatedPosition.Column,
                    statement.CompiledLine,
                    statement.CompiledColumn));
            }
        }

        foreach (var slot in parts.StateSlots)
        {
            if (slot.Initializer is not null &&
                slot.InitializerCompiledLine is int compiledLine &&
                generatedNodePositions.TryGetValue(slot.Initializer, out var generatedPosition))
            {
                mappings.Add(new CompiledLineMapping(
                    generatedPosition.Line,
                    generatedPosition.Column,
                    compiledLine,
                    slot.InitializerCompiledColumn ?? 0));
            }
        }

        return mappings
            .OrderBy(static mapping => mapping.GeneratedLine)
            .ThenBy(static mapping => mapping.GeneratedColumn)
            .ThenBy(static mapping => mapping.CompiledLine)
            .ThenBy(static mapping => mapping.CompiledColumn)
            .ToImmutableArray();
    }

    private static FunctionBody CreateFunctionBody(IEnumerable<Statement> statements)
        => new(NodeList.From(statements), strict: true);

    private static NestedBlockStatement CreateBlock(params Statement[] statements)
        => new(NodeList.From(statements));

    private static VariableDeclaration CreateVariableDeclaration(
        VariableDeclarationKind kind,
        string name,
        Expression? initializer)
        => new(
            kind,
            NodeList.From(new VariableDeclarator(new Identifier(name), initializer)));

    private static NonSpecialExpressionStatement CreateExpressionStatement(Expression expression)
        => new(expression);

    private static ArrowFunctionExpression CreateArrowFunction(
        IEnumerable<string> parameterNames,
        IEnumerable<Statement> statements)
        => new(
            NodeList.From<Node>(parameterNames.Select(static name => (Node)new Identifier(name))),
            CreateFunctionBody(statements),
            expression: false,
            async: false);

    private static ArrowFunctionExpression CreateArrowExpression(
        Expression expression,
        params string[] parameterNames)
        => new(
            NodeList.From<Node>(parameterNames.Select(static name => (Node)new Identifier(name))),
            expression,
            expression: true,
            async: false);

    private static CallExpression CreateCall(string name, params Expression[] arguments)
        => CreateCall(new Identifier(name), arguments);

    private static CallExpression CreateCall(
        string name,
        IEnumerable<Expression> arguments)
        => CreateCall(new Identifier(name), arguments);

    private static CallExpression CreateCall(
        Expression callee,
        IEnumerable<Expression> arguments)
        => new(callee, NodeList.From(arguments), optional: false);

    private static CallExpression CreateCallMember(
        Expression receiver,
        string memberName,
        params Expression[] arguments)
        => CreateCall(CreateMemberAccess(receiver, memberName), arguments);

    private static CallExpression CreateScopeCall(
        string memberName,
        params Expression[] arguments)
        => CreateCallMember(new Identifier("scope"), memberName, arguments);

    private static MemberExpression CreateMemberAccess(Expression receiver, string memberName)
        => JavaScriptAstFactory.IsJavaScriptIdentifierName(memberName)
            ? new MemberExpression(
                receiver,
                new Identifier(memberName),
                computed: false,
                optional: false)
            : new MemberExpression(
                receiver,
                StringLiteral(memberName),
                computed: true,
                optional: false);

    private static ObjectProperty CreateObjectProperty(string name, Node value)
        => new(
            PropertyKind.Init,
            JavaScriptAstFactory.IsJavaScriptIdentifierName(name)
                ? new Identifier(name)
                : StringLiteral(name),
            value,
            computed: false,
            shorthand: false,
            method: false);

    private static ArrayExpression CreateStringArray(IEnumerable<string> values)
        => new(NodeList.From<Expression?>(values.Select(static value => (Expression?)StringLiteral(value))));

    private static NullLiteral NullLiteral()
        => new("null");

    private static BooleanLiteral BooleanLiteral(bool value)
        => new(value, value ? "true" : "false");

    private static NumericLiteral NumericLiteral(int value)
        => new(value, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static StringLiteral StringLiteral(string value)
        => JavaScriptAstFactory.CreateStringLiteral(value);

    private static bool TryBuildOperationDirectRender(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        IEnumerable<string>? reservedImportNames,
        VueInjectRegistry injectRegistry,
        out DirectRenderBuildResult result,
        out RazorVueDiagnosticInfo? diagnostic)
    {
        result = default!;
        diagnostic = null;
        if (!RenderEmitter.TryEmitWithDiagnostic(
                binding.Compilation,
                component.ComponentSymbol,
                component.BuildRenderTreeMethod,
                component.BuildRenderTreeBody,
                declaredNames,
                reservedImportNames,
                injectRegistry,
                out var operationResult,
                out diagnostic,
                parameterPropertiesUseState: closure.UsesParameterViewState))
        {
            return false;
        }

        result = new DirectRenderBuildResult(
            operationResult.RenderExpression,
            "$renderDirect",
            operationResult.PreludeStatements,
            operationResult.ModuleHoists,
            operationResult.UsesFragment,
            operationResult.UsesStaticVNode,
            operationResult.UsesRawMarkupRuntime,
            operationResult.UsesBlockTree,
            operationResult.UsesTextVNode,
            operationResult.UsesRenderList,
            operationResult.UsesWithCtx,
            operationResult.UsesCreateSlots,
            operationResult.UsesMergeProps,
            operationResult.UsesHandlerCache,
            operationResult.UsesProps,
            operationResult.UsesSlots,
            operationResult.ImportDeclarations,
            operationResult.ReferenceCaptureStateMembers);
        return true;
    }

    private static IReadOnlyDictionary<ISymbol, string> BuildDirectRenderDeclaredNames(
        BoundComponent component,
        MemberClosure closure,
        IEnumerable<string>? importLocalNames = null)
    {
        var directLocalNames = CollectDirectRenderLocalNames(component.BuildRenderTreeBody);
        directLocalNames.UnionWith(FramingReservedNames);
        directLocalNames.UnionWith(
            BuildInjectBindings(closure).Select(static binding => binding.LocalName));
        if (importLocalNames is not null)
            directLocalNames.UnionWith(importLocalNames);
        var declaredNames = new Dictionary<ISymbol, string>(SymbolComparer);
        var usedDeclaredNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in closure.OrderedMembers)
        {
            if (member is not INamedTypeSymbol &&
                !ComponentSymbolPolicy.IsDeclaredOnComponentHierarchy(component.ComponentSymbol, member.ContainingType))
            {
                continue;
            }

            switch (member)
            {
                case IFieldSymbol field when field.IsStatic &&
                                             field.AssociatedSymbol is null:
                    declaredNames[field.OriginalDefinition] = ChooseModuleDeclaredName(
                        field,
                        usedDeclaredNames,
                        directLocalNames);
                    break;
                case IMethodSymbol method when ShouldReserveModuleMethodName(method) &&
                                               !IsParameterProperty(method.AssociatedSymbol as IPropertySymbol):
                    declaredNames[method.OriginalDefinition] = ChooseModuleDeclaredName(method, usedDeclaredNames, directLocalNames);
                    break;
                case IPropertySymbol property when !IsParameterProperty(property):
                    if (property.GetMethod is not null && ShouldReserveModuleMethodName(property.GetMethod))
                    {
                        var getterName = ChooseModuleDeclaredName(property.GetMethod, usedDeclaredNames, directLocalNames);
                        declaredNames[property.GetMethod.OriginalDefinition] = getterName;
                        declaredNames[property.OriginalDefinition] = getterName;
                    }

                    if (property.SetMethod is not null && ShouldReserveModuleMethodName(property.SetMethod))
                        declaredNames[property.SetMethod.OriginalDefinition] = ChooseModuleDeclaredName(property.SetMethod, usedDeclaredNames, directLocalNames);

                    // Auto-property accessors are intentionally erased by the Vue state
                    // projection. Static auto-properties still need their compiler backing
                    // field name so direct expressions can bind to the module lexical slot.
                    // 静态 auto-property 不进入 reactive state，必须把隐式 backing field 纳入同一命名表。
                    if (property.IsStatic && IsAutoProperty(property) &&
                        GetBackingField(property) is { } backingField &&
                        !declaredNames.ContainsKey(backingField.OriginalDefinition))
                    {
                        declaredNames[backingField.OriginalDefinition] = ChooseModuleDeclaredName(
                            backingField,
                            usedDeclaredNames,
                            directLocalNames);
                    }
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type):
                    declaredNames[type.OriginalDefinition] = ChooseModuleDeclaredName(type, usedDeclaredNames, directLocalNames);
                    break;
            }
        }

        return declaredNames;
    }

    private static Dictionary<string, ITypeSymbol> BuildStaticFieldTypeMap(
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
    {
        var staticTypes = new Dictionary<string, ITypeSymbol>(StringComparer.Ordinal);
        foreach (var field in closure.StaticFields)
            staticTypes[GetRuntimeMemberName(field, declaredNames)] = field.Type;

        foreach (var property in closure.StaticAutoProperties)
        {
            if (GetBackingField(property) is { } backingField)
                staticTypes[GetRuntimeMemberName(backingField, declaredNames)] = property.Type;
        }

        return staticTypes;
    }

    private static HashSet<string> BuildModuleLifetimeFunctionNames(
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in closure.OrderedMembers.OfType<IMethodSymbol>().Where(static method => method.IsStatic))
            names.Add(GetRuntimeMemberName(method, declaredNames));

        foreach (var property in closure.ComputedProperties.Where(static property => property.IsStatic))
        {
            if (property.GetMethod is { } getter)
                names.Add(GetRuntimeMemberName(getter, declaredNames));
            if (property.SetMethod is { } setter)
                names.Add(GetRuntimeMemberName(setter, declaredNames));
        }

        return names;
    }

    private static HashSet<string> BuildRuntimeClassNames(
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
        => new(
            closure.OrderedMembers
                .OfType<INamedTypeSymbol>()
                .Where(IsRuntimeMemberClass)
                .Select(type => GetRuntimeMemberName(type, declaredNames)),
            StringComparer.Ordinal);

    private static VariableDeclarator ProjectStaticFieldDeclarator(
        VariableDeclarator declarator,
        ITypeSymbol type)
    {
        if (declarator.Init is not null)
            return declarator;

        // Module fields still obey CLR allocation defaults. JavaScript `undefined` is not
        // a valid substitute for value-type zero/false and would corrupt the first update.
        // 模块 static slot 与实例 state 使用同一 CLR default 契约。
        return new VariableDeclarator(
            declarator.Id,
            CurrentComponentStateDefaultInitializer.CreateExpression(type))
        {
            UserData = declarator.UserData
        };
    }

    private static HashSet<string> CollectDirectRenderLocalNames(IBlockOperation buildRenderTreeBody)
    {
        var collector = new DirectRenderLocalNameCollector();
        collector.Visit(buildRenderTreeBody);
        return collector.Names;
    }

    /// <summary>Collects direct-render local names before setup bindings are allocated.</summary>
    private sealed class DirectRenderLocalNameCollector : OperationWalker
    {
        public HashSet<string> Names { get; } = new(StringComparer.Ordinal);

        public override void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
        {
            Add(operation.Symbol);

            base.VisitVariableDeclarator(operation);
        }

        public override void VisitDeclarationPattern(IDeclarationPatternOperation operation)
        {
            Add(operation.DeclaredSymbol);
            base.VisitDeclarationPattern(operation);
        }

        public override void VisitRecursivePattern(IRecursivePatternOperation operation)
        {
            Add(operation.DeclaredSymbol);
            base.VisitRecursivePattern(operation);
        }

        public override void VisitListPattern(IListPatternOperation operation)
        {
            Add(operation.DeclaredSymbol);
            base.VisitListPattern(operation);
        }

        public override void VisitForEachLoop(IForEachLoopOperation operation)
        {
            if (TryGetLoopControlVariable(operation.LoopControlVariable, out var local))
                Add(local);

            base.VisitForEachLoop(operation);
        }

        private void Add(ISymbol? symbol)
        {
            var name = symbol?.Name;
            if (!string.IsNullOrWhiteSpace(name))
                Names.Add(name!);
        }
    }

    private static bool TryGetLoopControlVariable(IOperation operation, out ILocalSymbol local)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;

        switch (operation)
        {
            case ILocalReferenceOperation localReference:
                local = localReference.Local;
                return true;
            case IVariableDeclaratorOperation declarator:
                local = declarator.Symbol;
                return true;
            default:
                local = null!;
                return false;
        }
    }

    private static string ChooseModuleDeclaredName(
        ISymbol symbol,
        HashSet<string> usedDeclaredNames,
        HashSet<string> localNames)
    {
        var preferredName = GetPreferredModuleDeclaredName(symbol);
        if (!localNames.Contains(preferredName) && usedDeclaredNames.Add(preferredName))
            return preferredName;

        var sourceName = symbol is IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }
            ? null
            : GetSourceDeclaredNameCandidate(symbol);
        if (!string.IsNullOrEmpty(sourceName) &&
            !localNames.Contains(sourceName!) &&
            usedDeclaredNames.Add(sourceName!))
        {
            return sourceName!;
        }

        var displayString = symbol.OriginalDefinition.ToDisplayString(Format.NameFormat);
        var alias = "m$" + Format.HashName(displayString).TrimStart('_');
        var suffix = 0;
        while (localNames.Contains(alias) || !usedDeclaredNames.Add(alias))
        {
            suffix++;
            alias = "m$" + Format.HashName(displayString).TrimStart('_') + "$" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return alias;
    }

    private static string GetPreferredModuleDeclaredName(ISymbol symbol)
    {
        if (symbol is IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation } explicitMethod)
        {
            // C# explicit-interface names contain dots and are not JavaScript identifiers.
            // A stable alias keeps all explicit implementation dispatch inside the normal AST path.
            return "m$" + Format.HashName(explicitMethod.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_');
        }

        return symbol switch
        {
            IMethodSymbol
            {
                MethodKind: MethodKind.PropertyGet,
                AssociatedSymbol: IPropertySymbol property
            } => Util.GetConfigOrSymbolName(property),
            IMethodSymbol method => Util.GetConfigOrSymbolName(method),
            INamedTypeSymbol type => Util.GetConfigOrSymbolName(type),
            _ => Util.GetConfigOrSymbolName(symbol)
        };
    }

    private static string? GetSourceDeclaredNameCandidate(ISymbol symbol)
        => symbol switch
        {
            IFieldSymbol field when field.AssociatedSymbol is IPropertySymbol property && !field.IsImplicitlyDeclared => property.Name,
            IFieldSymbol field when field.IsImplicitlyDeclared => null,
            IFieldSymbol field => field.Name,
            IMethodSymbol method when method.AssociatedSymbol is IPropertySymbol property => property.Name,
            IMethodSymbol method => method.Name,
            INamedTypeSymbol type => type.Name,
            _ => symbol.Name
        };

    private static bool ShouldReserveModuleMethodName(IMethodSymbol method)
    {
        if (method.MethodKind == MethodKind.SharedConstructor && method.IsImplicitlyDeclared)
            return false;

        if (method.IsInitOnly)
            return false;

        return method.MethodKind is MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or
            MethodKind.SharedConstructor or MethodKind.ExplicitInterfaceImplementation;
    }

    private static bool IsRuntimeMemberClass(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class && !type.IsRecord;

    private static bool IsParameterProperty(IPropertySymbol? property)
        => property is not null && LibraryComponentConventions.IsParameterProperty(property);

    private static ImmutableArray<CompilerStatement> RemoveBuildRenderTreeFunction(
        ImmutableArray<CompilerStatement> setupStatements,
        string buildRenderTreeMemberName)
        => setupStatements
            .Where(item =>
                item.Statement is not FunctionDeclaration { Id.Name: var name } ||
                !string.Equals(name, buildRenderTreeMemberName, StringComparison.Ordinal))
            .ToImmutableArray();

    private static ImmutableArray<CompilerStatement> RemoveDirectRenderOnlyFunctions(
        ImmutableArray<CompilerStatement> setupStatements,
        DirectRenderBuildResult directRender,
        ImmutableArray<StateSlot> stateSlots,
        ImmutableArray<ComponentInitializationPhaseBuild> initializationPhases,
        ImmutableArray<string> returnedMembers)
    {
        // Direct lowering may inline a BuildRenderTree helper entirely. Only remove function
        // declarations that are unreachable from the surviving Vue module roots; declarations
        // are side-effect free, while every other top-level statement remains intact.
        var roots = new List<Node> { directRender.RenderExpression };
        roots.AddRange(directRender.PreludeStatements);
        roots.AddRange(directRender.ModuleHoists.Select(static hoist => hoist.Initializer));
        roots.AddRange(stateSlots
            .Where(static slot => slot.Initializer is not null)
            .Select(static slot => slot.Initializer!));
        roots.AddRange(initializationPhases
            .Where(static phase => phase.ConstructorStatement is not null)
            .Select(static phase => (Node)phase.ConstructorStatement!));

        var referencedNames = AstReferenceAnalysis.CollectIdentifiers(roots);
        foreach (var returnedMember in returnedMembers)
            referencedNames.Add(returnedMember);

        foreach (var statement in setupStatements)
        {
            if (statement.Statement is not FunctionDeclaration)
                referencedNames.UnionWith(AstReferenceAnalysis.CollectIdentifiers([statement.Statement]));
        }

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var statement in setupStatements)
            {
                if (statement.Statement is not FunctionDeclaration function ||
                    function.Id is not { } functionId ||
                    !referencedNames.Contains(functionId.Name))
                {
                    continue;
                }

                var count = referencedNames.Count;
                referencedNames.UnionWith(AstReferenceAnalysis.CollectIdentifiers([function]));
                changed = changed || referencedNames.Count != count;
            }
        }

        return setupStatements
            .Where(statement =>
                statement.Statement is not FunctionDeclaration function ||
                function.Id is not { } functionId ||
                referencedNames.Contains(functionId.Name))
            .ToImmutableArray();
    }

    private static bool IsVueFramingImport(ImportDeclaration declaration)
        => declaration.Source.Value is "vue";

    private static bool TryCreateVueSfcAsset(
        ImportDeclaration declaration,
        string importerRelativePath,
        out VueAsset asset)
    {
        asset = default!;
        var specifier = declaration.Source.Value;
        if ((!specifier.StartsWith("./", StringComparison.Ordinal) &&
             !specifier.StartsWith("../", StringComparison.Ordinal)) ||
            !specifier.EndsWith(".vue.mjs", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var importPath = ResolveImportArtifactPath(specifier, importerRelativePath);
        var artifactPath = ResolveImportArtifactPath(specifier.Substring(0, specifier.Length - ".mjs".Length), importerRelativePath);
        asset = new VueAsset(
            SourcePath: artifactPath,
            ArtifactPath: artifactPath,
            Kind: "module-source",
            ImportPath: importPath,
            ContentHash: string.Empty);
        return true;
    }

    private static void AddImportLocalNames(
        ImportDeclaration declaration,
        HashSet<string> localNames)
    {
        foreach (var specifier in declaration.Specifiers)
        {
            switch (specifier)
            {
                case ImportSpecifier named:
                    localNames.Add(named.Local.Name);
                    break;
                case ImportDefaultSpecifier defaultSpecifier:
                    localNames.Add(defaultSpecifier.Local.Name);
                    break;
                case ImportNamespaceSpecifier namespaceSpecifier:
                    localNames.Add(namespaceSpecifier.Local.Name);
                    break;
            }
        }
    }

    private static ImportDeclaration? FilterEmittedImportSpecifiers(
        ImportDeclaration declaration,
        Dictionary<string, ImportBinding> emittedBindings)
    {
        // A bare import has module-evaluation semantics. It has no local binding to dedupe and
        // therefore must remain intact even when another declaration uses the same source.
        if (declaration.Specifiers.Count == 0)
            return declaration;

        var retainedSpecifiers = new List<ImportDeclarationSpecifier>(declaration.Specifiers.Count);
        var newBindings = new List<KeyValuePair<string, ImportBinding>>(declaration.Specifiers.Count);
        foreach (var specifier in declaration.Specifiers)
        {
            var localName = GetImportLocalName(specifier);
            if (string.IsNullOrWhiteSpace(localName))
            {
                retainedSpecifiers.Add(specifier);
                continue;
            }

            var binding = GetImportBinding(declaration, specifier);
            if (emittedBindings.TryGetValue(localName, out var emittedBinding))
            {
                if (emittedBinding == binding)
                    continue;

                throw new InvalidOperationException(
                    "RazorVue generated import local '" + localName + "' resolves to both " +
                    DescribeImportBinding(emittedBinding) + " and " + DescribeImportBinding(binding) +
                    ". Import aliases must be unique within one generated Vue module.");
            }

            retainedSpecifiers.Add(specifier);
            newBindings.Add(new KeyValuePair<string, ImportBinding>(localName, binding));
        }

        foreach (var pair in newBindings)
            emittedBindings.Add(pair.Key, pair.Value);

        if (retainedSpecifiers.Count == 0)
            return null;

        return new ImportDeclaration(
            NodeList.From<ImportDeclarationSpecifier>(retainedSpecifiers),
            declaration.Source,
            declaration.Attributes,
            declaration.Phase);
    }

    private static ImportBinding GetImportBinding(
        ImportDeclaration declaration,
        ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportSpecifier named => new(
                declaration.Source.Value,
                "named",
                GetImportedName(named.Imported)),
            ImportDefaultSpecifier => new(declaration.Source.Value, "default", "default"),
            ImportNamespaceSpecifier => new(declaration.Source.Value, "namespace", "*"),
            _ => throw new NotSupportedException("Unsupported ECMAScript import specifier: " + specifier.Type)
        };

    private static string GetImportedName(Expression imported)
        => imported switch
        {
            Identifier identifier => identifier.Name,
            StringLiteral literal => literal.Value,
            _ => throw new NotSupportedException("Unsupported ECMAScript named import key: " + imported.Type)
        };

    private static string DescribeImportBinding(ImportBinding binding)
        => binding.Kind + " import '" + binding.ImportedName + "' from '" + binding.ModulePath + "'";

    private static bool IsCompilerImportReferenced(
        ImportDeclaration declaration,
        DirectRenderBuildResult directRender,
        CompilerModuleParts parts)
    {
        if (declaration.Specifiers.Count == 0)
            return true;

        foreach (var specifier in declaration.Specifiers)
        {
            var localName = GetImportLocalName(specifier);
            if (AstReferenceAnalysis.ReferencesIdentifier(directRender.RenderExpression, localName) ||
                directRender.PreludeStatements.Any(statement => AstReferenceAnalysis.ReferencesIdentifier(statement, localName)) ||
                directRender.ModuleHoists.Any(hoist => AstReferenceAnalysis.ReferencesIdentifier(hoist.Initializer, localName)) ||
                ReferencesIdentifier(parts.ModuleStatements, localName) ||
                ReferencesIdentifier(parts.SetupStatements, localName) ||
                ReferencesIdentifier(parts.InitializationPhases, localName) ||
                parts.StateSlots.Any(slot =>
                    slot.Initializer is not null &&
                    AstReferenceAnalysis.ReferencesIdentifier(slot.Initializer, localName)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ReferencesIdentifier(
        ImmutableArray<CompilerStatement> statements,
        string name)
        => statements.Any(item => AstReferenceAnalysis.ReferencesIdentifier(item.Statement, name));

    private static bool ReferencesIdentifier(
        ImmutableArray<ComponentInitializationPhaseBuild> phases,
        string name)
        => phases.Any(phase =>
            phase.ConstructorStatement is not null &&
            AstReferenceAnalysis.ReferencesIdentifier(phase.ConstructorStatement, name));

    private static Func<IPropertyReferenceOperation, SenseArgument, Expression?>
        CreateDirectRenderSlotParameterPropertyReferenceRewriter(MemberClosure closure)
    {
        var slotNames = ImmutableDictionary.CreateBuilder<IPropertySymbol, string>(SymbolComparer);
        foreach (var property in LibraryComponentConventions
                     .GetEffectiveParameterProperties(closure.ComponentSymbol)
                     .Where(static property => IsAnyRenderFragmentType(property.Type)))
        {
            slotNames[(IPropertySymbol)property.OriginalDefinition] =
                LibraryComponentConventions.GetSlotRuntimeName(
                closure.ComponentSymbol,
                property);
        }

        return (operation, _) =>
            slotNames.TryGetValue((IPropertySymbol)operation.Property.OriginalDefinition, out var slotName)
                ? BuildDirectRenderSlotValueExpression(slotName)
                : null;
    }

    private static Expression BuildDirectRenderSlotValueExpression(string slotName)
    {
        var access = JavaScriptAstFactory.IsJavaScriptIdentifierName(slotName)
            ? (Expression)new MemberExpression(
                new Identifier("slots"),
                new Identifier(slotName),
                computed: false,
                optional: false)
            : new MemberExpression(
                new Identifier("slots"),
                StringLiteral(slotName),
                computed: true,
                optional: false);
        return new ConditionalExpression(
            new NonLogicalBinaryExpression(
                Operator.StrictEquality,
                new NonUpdateUnaryExpression(Operator.TypeOf, access),
                StringLiteral("function")),
            access,
            NullLiteral());
    }

    private static string GetImportLocalName(ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportSpecifier named => named.Local.Name,
            ImportDefaultSpecifier defaultSpecifier => defaultSpecifier.Local.Name,
            ImportNamespaceSpecifier namespaceSpecifier => namespaceSpecifier.Local.Name,
            _ => string.Empty
        };

    private static string ResolveImportArtifactPath(string importSpecifier, string importerRelativePath)
    {
        var importer = NormalizeGeneratedSourcePath(importerRelativePath);
        var importerDirectory = Path.GetDirectoryName(importer)?.Replace('\\', '/') ?? string.Empty;
        var segments = new List<string>();
        foreach (var segment in SplitPathSegments(importerDirectory))
            segments.Add(segment);

        foreach (var segment in SplitPathSegments(importSpecifier))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (segments.Count == 0)
                    throw new InvalidOperationException("Vue SFC import path cannot escape the output directory.");

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(segment);
        }

        if (segments.Count == 0)
            throw new InvalidOperationException("Vue SFC import path cannot be empty.");

        return string.Join("/", segments);
    }

    private static ImportDeclaration BuildVueImportDeclaration(
        bool usesMounted,
        bool usesUnmounted,
        bool usesUpdated,
        bool usesReactive,
        bool usesWatch,
        bool usesFragment,
        bool usesStaticVNode,
        bool usesRawMarkupRuntime,
        bool usesBlockTree,
        bool usesTextVNode,
        bool usesRenderList,
        bool usesWithCtx,
        bool usesCreateSlots,
        bool usesMergeProps,
        bool usesInject,
        bool usesCascading)
    {
        var imports = new List<string>
        {
            "defineComponent",
            "h"
        };

        if (usesFragment)
            imports.Add("Fragment");
        if (usesStaticVNode)
            imports.Add("createStaticVNode");
        if (usesBlockTree)
        {
            imports.Add("openBlock");
            imports.Add("createElementBlock");
            imports.Add("createBlock");
        }
        if (usesTextVNode)
            imports.Add("createTextVNode");
        if (usesRenderList)
            imports.Add("renderList");
        if (usesWithCtx)
            imports.Add("withCtx");
        if (usesCreateSlots)
            imports.Add("createSlots");
        if (usesMergeProps)
            imports.Add("mergeProps");
        if (usesInject || usesCascading)
            imports.Add("inject");
        if (usesCascading)
            imports.Add("unref");

        if (usesMounted)
            imports.Add("onMounted");
        if (usesUnmounted)
            imports.Add("onUnmounted");
        if (usesUpdated)
            imports.Add("onUpdated");

        if (usesReactive)
            imports.Add("reactive");
        if (usesWatch)
            imports.Add("watch");

        return CreateNamedImportDeclaration("vue", imports);
    }

    private static ImportDeclaration CreateNamedImportDeclaration(
        string modulePath,
        IEnumerable<string> importedNames)
        => ImportDeclarationFactory.Create(
                modulePath,
                importedNames.Select(static name =>
                    (ImportDeclarationSpecifier)new ImportSpecifier(new Identifier(name))))
            .Single();

    private static CompilerModuleParts BuildCompilerModuleParts(
        Module? module,
        IReadOnlyDictionary<Node, GeneratedNodePosition>? nodePositions,
        MemberClosure closure)
        => BuildCompilerModuleParts(module, nodePositions, closure, declaredNames: null);

    private static CompilerModuleParts BuildCompilerModuleParts(
        Module? module,
        IReadOnlyDictionary<Node, GeneratedNodePosition>? nodePositions,
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
    {
        var imports = ImmutableArray.CreateBuilder<ImportDeclaration>();
        var moduleStatements = ImmutableArray.CreateBuilder<CompilerStatement>();
        var setupStatements = ImmutableArray.CreateBuilder<CompilerStatement>();
        var stateSlots = BuildStateSlots(closure);
        var staticFieldTypes = BuildStaticFieldTypeMap(closure, declaredNames);
        var moduleFunctionNames = BuildModuleLifetimeFunctionNames(closure, declaredNames);
        var runtimeClassNames = BuildRuntimeClassNames(closure, declaredNames);
        var stateSlotByDeclarationName = new Dictionary<string, int>(StringComparer.Ordinal);
        var discardedDeclarationNames = new HashSet<string>(
            GetDiscardedPropertyBackingFieldNames(closure),
            StringComparer.Ordinal);

        for (var index = 0; index < stateSlots.Count; index++)
        {
            var declarationName = stateSlots[index].DeclarationName;
            if (!string.IsNullOrWhiteSpace(declarationName))
                stateSlotByDeclarationName[declarationName!] = index;
        }

        if (module is not null)
        {
            if (nodePositions is null)
                throw new InvalidOperationException("Compiler AST node positions are required when a compiler module is present.");

            foreach (var statement in module.Body)
            {
                switch (statement)
                {
                    case ImportDeclaration importDeclaration:
                        imports.Add(importDeclaration);
                        break;
                    case ExportNamedDeclaration { Declaration: { } declaration }:
                        AddDeclaration(declaration);
                        break;
                    case ExportNamedDeclaration:
                        break;
                    case Declaration declaration:
                        AddDeclaration(declaration);
                        break;
                    default:
                        AddSetupStatement(statement, GetPosition(statement));
                        break;
                }
            }
        }

        var compilerOriginPositions = new Dictionary<SourceOrigin, GeneratedNodePosition>();
        if (nodePositions is not null)
        {
            foreach (var pair in nodePositions
                         .Where(static pair => pair.Key.UserData is SourceOrigin { IsSynthetic: false })
                         .OrderBy(static pair => pair.Value.Line)
                         .ThenBy(static pair => pair.Value.Column))
            {
                var origin = (SourceOrigin)pair.Key.UserData!;
                if (!compilerOriginPositions.ContainsKey(origin))
                    compilerOriginPositions.Add(origin, pair.Value);
            }
        }

        return new CompilerModuleParts(
            imports.ToImmutable(),
            moduleStatements.ToImmutable(),
            setupStatements.ToImmutable(),
            stateSlots.ToImmutableArray(),
            ImmutableArray<ComponentInitializationPhaseBuild>.Empty,
            compilerOriginPositions);

        void AddDeclaration(Declaration declaration)
        {
            var declarationPosition = GetPosition(declaration);
            if (declaration is FunctionDeclaration { Id: Identifier functionName } &&
                moduleFunctionNames.Contains(functionName.Name) ||
                declaration is ClassDeclaration { Id: Identifier className } &&
                runtimeClassNames.Contains(className.Name))
            {
                AddModuleStatement(declaration, declarationPosition);
                return;
            }

            if (declaration is not VariableDeclaration variableDeclaration)
            {
                AddSetupStatement(declaration, declarationPosition);
                return;
            }

            var moduleDeclarators = ImmutableArray.CreateBuilder<VariableDeclarator>();
            var setupDeclarators = ImmutableArray.CreateBuilder<VariableDeclarator>();
            foreach (var declarator in variableDeclaration.Declarations)
            {
                if (declarator.Id is not Identifier identifier)
                {
                    setupDeclarators.Add(declarator);
                    continue;
                }

                if (stateSlotByDeclarationName.TryGetValue(identifier.Name, out var stateSlotIndex))
                {
                    var initializer = declarator.Init;
                    GeneratedNodePosition? initializerPosition = initializer is null
                        ? null
                        : GetPosition(initializer);
                    stateSlots[stateSlotIndex] = stateSlots[stateSlotIndex] with
                    {
                        Initializer = initializer,
                        InitializerCompiledLine = initializerPosition?.Line,
                        InitializerCompiledColumn = initializerPosition?.Column
                    };
                    continue;
                }

                if (staticFieldTypes.TryGetValue(identifier.Name, out var staticFieldType))
                {
                    moduleDeclarators.Add(ProjectStaticFieldDeclarator(declarator, staticFieldType));
                    continue;
                }

                if (!discardedDeclarationNames.Contains(identifier.Name))
                    setupDeclarators.Add(declarator);
            }

            if (moduleDeclarators.Count > 0)
            {
                AddModuleStatement(
                    CopyVariableDeclaration(variableDeclaration, moduleDeclarators),
                    declarationPosition);
            }

            if (setupDeclarators.Count > 0)
            {
                AddSetupStatement(
                    CopyVariableDeclaration(variableDeclaration, setupDeclarators),
                    declarationPosition);
            }
        }

        void AddModuleStatement(Statement statement, GeneratedNodePosition position)
            => moduleStatements.Add(new CompilerStatement(statement, position.Line, position.Column));

        void AddSetupStatement(Statement statement, GeneratedNodePosition position)
            => setupStatements.Add(new CompilerStatement(statement, position.Line, position.Column));

        static VariableDeclaration CopyVariableDeclaration(
            VariableDeclaration declaration,
            IEnumerable<VariableDeclarator> declarators)
            => new(declaration.Kind, NodeList.From(declarators))
            {
                UserData = declaration.UserData
            };

        GeneratedNodePosition GetPosition(Node node)
        {
            if (nodePositions!.TryGetValue(node, out var position))
                return position;

            throw new InvalidOperationException(
                "Compiler writer did not report a generated position for AST node '" + node.Type + "'.");
        }
    }

    private static List<StateSlot> BuildStateSlots(MemberClosure closure)
    {
        var slots = new List<StateSlot>();
        foreach (var field in closure.StateFields)
        {
            var name = Util.GetConfigOrSymbolName(field);
            slots.Add(new StateSlot(
                field,
                name,
                name,
                field.Type,
                null,
                HasExplicitStateInitializer(field)));
        }

        foreach (var property in closure.StateProperties)
        {
            slots.Add(new StateSlot(
                property,
                Util.GetConfigOrSymbolName(property),
                GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                property.Type,
                null,
                HasExplicitStateInitializer(property)));
        }

        foreach (var property in closure.InjectProperties)
        {
            if (slots.Any(slot => SymbolComparer.Equals(slot.Member, property)))
                continue;

            slots.Add(new StateSlot(
                property,
                Util.GetConfigOrSymbolName(property),
                GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                property.Type,
                null,
                HasExplicitStateInitializer(property)));
        }

        foreach (var property in closure.CascadingParameterProperties)
        {
            if (slots.Any(slot => SymbolComparer.Equals(slot.Member, property)))
                continue;

            slots.Add(new StateSlot(
                property,
                Util.GetConfigOrSymbolName(property),
                GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                property.Type,
                null,
                HasExplicitStateInitializer(property)));
        }

        if (closure.UsesParameterViewState)
        {
            foreach (var property in closure.ParameterProperties)
            {
                if (slots.Any(slot => SymbolComparer.Equals(slot.Member, property)))
                    continue;

                slots.Add(new StateSlot(
                    property,
                    Util.GetConfigOrSymbolName(property),
                    GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                    property.Type,
                    null,
                    HasExplicitStateInitializer(property)));
            }
        }

        return slots;
    }

    private static IFieldSymbol? GetBackingField(IPropertySymbol property)
        => property.ContainingType?
            .GetMembers($"<{property.Name}>k__BackingField")
            .OfType<IFieldSymbol>()
            .FirstOrDefault();

    private static bool IsAutoProperty(IPropertySymbol property)
    {
        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax declaration)
                continue;

            if (declaration.ExpressionBody is not null || declaration.AccessorList is null)
                return false;

            if (declaration.AccessorList.Accessors.Any(accessor =>
                    accessor.Body is not null || accessor.ExpressionBody is not null))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private static bool HasExplicitStateInitializer(ISymbol member)
        => member.DeclaringSyntaxReferences.Any(reference =>
            reference.GetSyntax() switch
            {
                Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax { Initializer: not null } => true,
                Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax { Initializer: not null } => true,
                _ => false
            });

    private static ImmutableArray<StateSlot> ApplyReferenceCaptureStateInitializers(
        ImmutableArray<StateSlot> stateSlots,
        ImmutableArray<ISymbol> referenceCaptureStateMembers)
    {
        if (referenceCaptureStateMembers.IsDefaultOrEmpty)
            return stateSlots;

        var capturedMembers = new HashSet<ISymbol>(referenceCaptureStateMembers, SymbolComparer);
        return stateSlots
            .Select(slot =>
                slot.Initializer is null && capturedMembers.Contains(slot.Member)
                    ? slot with { Initializer = new NullLiteral("null") }
                    : slot)
            .ToImmutableArray();
    }

    private static IEnumerable<string> GetDiscardedPropertyBackingFieldNames(MemberClosure closure)
    {
        foreach (var property in closure.ParameterProperties)
        {
            var backingFieldName = GetPropertyBackingFieldName(closure.ComponentSymbol, property);
            if (!string.IsNullOrWhiteSpace(backingFieldName))
                yield return backingFieldName!;
        }
    }

    private static string? GetPropertyBackingFieldName(
        INamedTypeSymbol componentSymbol,
        IPropertySymbol property)
    {
        // Auto-property storage belongs to the declaring source type. Looking only at the most
        // derived component loses base-class properties and leaves their backing declaration in
        // setup instead of reactive state.
        // 属性后备字段应从声明类型查找，不能只查最派生 component。
        var declaringType = property.ContainingType;
        foreach (var field in declaringType.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.AssociatedSymbol is IPropertySymbol associatedProperty &&
                SymbolComparer.Equals(associatedProperty.OriginalDefinition, property.OriginalDefinition))
            {
                return Util.GetConfigOrSymbolName(field);
            }
        }

        return null;
    }

    private static bool IsEventCallbackType(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var original = namedType.OriginalDefinition;
        var display = original.ToDisplayString(Jazor.Common.Format.NameFormat);
        return string.Equals(display, EventCallbackMetadataName, StringComparison.Ordinal) ||
               string.Equals(display, EventCallbackOfTMetadataName, StringComparison.Ordinal) ||
               string.Equals(display, "Microsoft.AspNetCore.Components.EventCallback<TValue>", StringComparison.Ordinal) ||
               (string.Equals(original.Name, "EventCallback", StringComparison.Ordinal) &&
                string.Equals(
                    original.ContainingNamespace!.ToDisplayString(),
                    "Microsoft.AspNetCore.Components",
                    StringComparison.Ordinal));
    }

    private static ImportDeclaration RebaseImportDeclaration(
        ImportDeclaration declaration,
        string importerRelativePath)
    {
        var modulePath = declaration.Source.Value;
        if (!modulePath.StartsWith("./", StringComparison.Ordinal))
            return declaration;

        var rebasedPath = RebaseRootRelativeModuleSpecifier(modulePath, importerRelativePath);
        return ImportDeclarationFactory.WithModulePath(declaration, rebasedPath);
    }

    private static string RebaseRootRelativeModuleSpecifier(
        string rootRelativeSpecifier,
        string importerRelativePath)
    {
        var target = NormalizeGeneratedSourcePath(rootRelativeSpecifier);
        var importer = NormalizeGeneratedSourcePath(importerRelativePath);
        var importerDirectory = Path.GetDirectoryName(importer)?.Replace('\\', '/') ?? string.Empty;
        var targetSegments = SplitPathSegments(target);
        var importerSegments = SplitPathSegments(importerDirectory);
        var commonLength = 0;
        while (commonLength < targetSegments.Length &&
               commonLength < importerSegments.Length &&
               string.Equals(targetSegments[commonLength], importerSegments[commonLength], StringComparison.Ordinal))
        {
            commonLength++;
        }

        var relativeSegments = Enumerable
            .Repeat("..", importerSegments.Length - commonLength)
            .Concat(targetSegments.Skip(commonLength))
            .ToArray();
        var relative = string.Join("/", relativeSegments);
        if (string.IsNullOrWhiteSpace(relative))
            relative = Path.GetFileName(target).Replace('\\', '/');

        return relative.StartsWith(".", StringComparison.Ordinal)
            ? relative
            : "./" + relative;
    }

    private static string[] SplitPathSegments(string path)
        => NormalizeGeneratedSourcePath(path)
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

    private static bool IsAnyRenderFragmentType(ITypeSymbol type)
        => IsRenderFragmentType(type) || IsGenericRenderFragmentType(type);

    private static bool IsRenderFragmentType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.IsGenericType)
            return false;

        return string.Equals(
            named.OriginalDefinition.ToDisplayString(),
            "Microsoft.AspNetCore.Components.RenderFragment",
            StringComparison.Ordinal);
    }

    private static bool IsGenericRenderFragmentType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || !named.IsGenericType)
            return false;

        return string.Equals(
            named.OriginalDefinition.ToDisplayString(),
            "Microsoft.AspNetCore.Components.RenderFragment<TValue>",
            StringComparison.Ordinal);
    }

    private static ImmutableArray<string> GetReturnedMembers(
        MemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
    {
        var names = ImmutableArray.CreateBuilder<string>();
        AddName(closure.BuildRenderTreeMethod);
        foreach (var lifecycleRoot in closure.LifecycleRoots)
        {
            AddName(lifecycleRoot);
        }

        return names.ToImmutable();

        void AddName(IMethodSymbol method)
        {
            var name = GetRuntimeMemberName(method, declaredNames);
            if (!names.Contains(name, StringComparer.Ordinal))
                names.Add(name);
        }
    }

    private static string GetRuntimeMemberName(
        ISymbol method,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
        => declaredNames is not null &&
           declaredNames.TryGetValue(method.OriginalDefinition, out var declaredName) &&
           !string.IsNullOrWhiteSpace(declaredName)
            ? declaredName
            : Util.GetConfigOrSymbolName(method);

    private static bool IsSynchronousDisposeMethod(IMethodSymbol method)
        => string.Equals(method.Name, "Dispose", StringComparison.Ordinal) ||
           ImplementsExplicitInterfaceMethod(method, "System.IDisposable", "Dispose");

    private static bool IsAsynchronousDisposeMethod(IMethodSymbol method)
        => string.Equals(method.Name, "DisposeAsync", StringComparison.Ordinal) ||
           ImplementsExplicitInterfaceMethod(method, "System.IAsyncDisposable", "DisposeAsync");

    private static bool ImplementsExplicitInterfaceMethod(
        IMethodSymbol method,
        string interfaceTypeName,
        string methodName)
        => method.ExplicitInterfaceImplementations.Any(implementation =>
            string.Equals(implementation.Name, methodName, StringComparison.Ordinal) &&
            string.Equals(
                implementation.ContainingType.OriginalDefinition.ToDisplayString(),
                interfaceTypeName,
                StringComparison.Ordinal));

    private static string GetRelativePath(INamedTypeSymbol componentSymbol)
    {
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass!.ToDisplayString(),
                    ECMAScriptModuleAttributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 1 &&
                attribute.ConstructorArguments[0].Value is string importPath &&
                !string.IsNullOrWhiteSpace(importPath))
            {
                return NormalizeRelativePath(importPath);
            }
        }

        // BoundComponent is created from the active compilation, so both owners are present.
        // Keep this path deterministic instead of inventing a fallback assembly for invalid symbols.
        var assemblyName = componentSymbol.ContainingAssembly!.Name;
        var namespaceName = componentSymbol.ContainingNamespace!.IsGlobalNamespace
            ? string.Empty
            : componentSymbol.ContainingNamespace!.ToDisplayString().Replace('.', '/');
        var fileName = componentSymbol.Name + ".mjs";

        return string.IsNullOrEmpty(namespaceName)
            ? assemblyName + "/" + fileName
            : assemblyName + "/" + namespaceName + "/" + fileName;
    }

    private static string NormalizeRelativePath(string path)
        => ECMAScriptModulePath.NormalizeRelativePath(path);

    private static string SanitizeJavaScriptIdentifierPart(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(IsJavaScriptIdentifierPart(character) ? character : '_');
        }

        if (!IsJavaScriptIdentifierStart(builder[0]))
            builder.Insert(0, fallback);

        return builder.ToString();
    }

    private static bool IsJavaScriptIdentifierStart(char value)
        => value == '$' || value == '_' || char.IsLetter(value);

    private static bool IsJavaScriptIdentifierPart(char value)
        => value == '$' || value == '_' || char.IsLetterOrDigit(value);

    private static string ComputeContentHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        var builder = new StringBuilder(hash.Length * 2);
        foreach (var value in hash)
        {
            builder.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return "sha256:" + builder;
    }

    private static string BuildSourceMapContent(
        BoundComponent component,
        string relativePath,
        string moduleText,
        string? compilerSourceMapContent,
        ImmutableArray<CompiledLineMapping> compiledLineMappings)
    {
        // Prefer the precise chained map back to authored Razor. A malformed/partial compiler map
        // must not fail generation; the coarse map keeps DevTools source navigation available.
        // 高精度 chain 只是增强项，失败时退回 coarse map 仍保证 artifact 可生成并指向 Razor。
        if (!string.IsNullOrWhiteSpace(compilerSourceMapContent) &&
            compiledLineMappings.Length > 0 &&
            TryBuildChainedSourceMapContent(
                component,
                relativePath,
                compilerSourceMapContent!,
                compiledLineMappings,
                out var chainedSourceMapContent))
        {
            return chainedSourceMapContent!;
        }

        return BuildCoarseSourceMapContent(component, relativePath, moduleText);
    }

    private static bool TryBuildChainedSourceMapContent(
        BoundComponent component,
        string relativePath,
        string compilerSourceMapContent,
        ImmutableArray<CompiledLineMapping> compiledLineMappings,
        out string? sourceMapContent)
    {
        sourceMapContent = null;

        var writer = new SourceMapWriter();
        var compilerMap = new SourceMapReader().Read(compilerSourceMapContent);
        var projectedCompilerMap = ProjectCompilerSourceMap(
            relativePath,
            compilerMap,
            compiledLineMappings,
            component.Document.HintName);
        if (projectedCompilerMap.Segments.Count == 0)
            return false;

        var moduleMaps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // The compiler map starts at generated C#. Join it with Razor #line mappings here so
        // downstream consumers only need the final .mjs.map and never load SDK-generated files.
        // 将 compiler -> .g.cs map 与 .g.cs -> .razor map 在此链接，最终产物只暴露作者源文件。
        var generatedCSharpMap = BuildGeneratedCSharpSourceMap(component.Document, compilerMap);
        if (generatedCSharpMap.Segments.Count > 0)
        {
            var generatedCSharpMapContent = writer.Write(generatedCSharpMap);
            AddModuleMapAlias(moduleMaps, component.Document.HintName, generatedCSharpMapContent);
            foreach (var path in component.Document.RazorSourceMaps.Select(static mapping => mapping.GeneratedSpan.FilePath))
                AddModuleMapAlias(moduleMaps, path, generatedCSharpMapContent);
        }

        SourceMapDocument chained;
        try
        {
            chained = new SourceMapChainBuilder().Chain(writer.Write(projectedCompilerMap), moduleMaps);
        }
        catch
        {
            return false;
        }

        var pruned = PruneIntermediateSources(chained, relativePath);
        if (pruned.Segments.Count == 0 ||
            !pruned.Sources.Any(static source => source.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        sourceMapContent = Util.NormalizeLineEndingsToLf(writer.Write(pruned));
        return true;
    }

    private static SourceMapDocument ProjectCompilerSourceMap(
        string relativePath,
        SourceMapDocument compilerMap,
        ImmutableArray<CompiledLineMapping> compiledLineMappings,
        string generatedCSharpHintName)
    {
        var mappingsByCompiledLine = compiledLineMappings
            .GroupBy(static mapping => mapping.CompiledLine)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .OrderBy(static mapping => mapping.CompiledColumn)
                    .ToArray());
        var segments = new List<SourceMapSegment>();

        foreach (var segment in compilerMap.Segments)
        {
            if (segment.SourceIndex < 0 || segment.SourceIndex >= compilerMap.Sources.Count)
                continue;

            if (!mappingsByCompiledLine.TryGetValue(segment.GeneratedLine, out var lineMappings))
                continue;

            var lineMapping = lineMappings[0];
            foreach (var candidate in lineMappings)
            {
                if (candidate.CompiledColumn <= segment.GeneratedColumn)
                    lineMapping = candidate;
                else
                    break;
            }

            var generatedColumn = lineMapping.GeneratedColumn +
                                  Math.Max(0, segment.GeneratedColumn - lineMapping.CompiledColumn);
            segments.Add(new SourceMapSegment(
                lineMapping.GeneratedLine,
                generatedColumn,
                segment.SourceIndex,
                segment.SourceLine,
                segment.SourceColumn));
        }

        // RenderEmitter nodes are created after BuildRenderTree has been intentionally removed
        // from the generic compiler module. Their SourceOrigin coordinates are nevertheless in
        // the official generated C# document, so add them as an explicit first hop before the
        // existing generated-C# -> Razor map is chained below.
        // direct h(...) 不存在 compiler map 段，必须显式接回 .g.cs，随后复用 Razor 链式 map。
        var sources = compilerMap.Sources.ToList();
        var generatedCSharpSourceIndex = -1;
        foreach (var mapping in compiledLineMappings
                     .Where(static mapping => mapping.IsDirectRenderOrigin)
                     .OrderBy(static mapping => mapping.GeneratedLine)
                     .ThenBy(static mapping => mapping.GeneratedColumn))
        {
            if (generatedCSharpSourceIndex < 0)
            {
                generatedCSharpSourceIndex = sources.FindIndex(source =>
                    IsGeneratedCSharpSourcePath(source.Path, generatedCSharpHintName));
                if (generatedCSharpSourceIndex < 0)
                {
                    generatedCSharpSourceIndex = sources.Count;
                    sources.Add(new SourceMapSource(
                        NormalizeGeneratedSourcePath(generatedCSharpHintName),
                        null));
                }
            }

            segments.Add(new SourceMapSegment(
                mapping.GeneratedLine,
                mapping.GeneratedColumn,
                generatedCSharpSourceIndex,
                mapping.CompiledLine,
                mapping.CompiledColumn));
        }

        return new SourceMapDocument(
            relativePath,
            sources,
            segments
                .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
                .Select(static group => group.First())
                .OrderBy(static segment => segment.GeneratedLine)
                .ThenBy(static segment => segment.GeneratedColumn)
                .ToArray());
    }

    private static SourceMapDocument BuildGeneratedCSharpSourceMap(
        GeneratedDocument document,
        SourceMapDocument compilerMap)
    {
        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var segments = new List<SourceMapSegment>();

        var orderedMappings = document.RazorSourceMaps
            .OrderBy(static mapping => mapping.GeneratedSpan.LineIndex)
            .ThenBy(static mapping => mapping.GeneratedSpan.CharacterIndex)
            .ToArray();
        if (orderedMappings.Length > 0)
        {
            var first = orderedMappings[0];
            var sourceFilePath = first.OriginalSpan.FilePath ?? document.SourcePath;
            var sourcePath = NormalizeSourcePath(sourceFilePath);
            var sourceIndex = GetOrAddSourceIndex(
                sources,
                sourceIndexByPath,
                sourcePath,
                TryGetSourceMapSourceContent(sourceFilePath));
            segments.Add(new SourceMapSegment(
                0,
                0,
                sourceIndex,
                Math.Max(0, first.OriginalSpan.LineIndex),
                Math.Max(0, first.OriginalSpan.CharacterIndex)));
        }

        foreach (var mapping in orderedMappings)
        {
            var sourceFilePath = mapping.OriginalSpan.FilePath ?? document.SourcePath;
            var sourcePath = NormalizeSourcePath(sourceFilePath);
            var sourceIndex = GetOrAddSourceIndex(
                sources,
                sourceIndexByPath,
                sourcePath,
                TryGetSourceMapSourceContent(sourceFilePath));
            segments.Add(new SourceMapSegment(
                Math.Max(0, mapping.GeneratedSpan.LineIndex),
                Math.Max(0, mapping.GeneratedSpan.CharacterIndex),
                sourceIndex,
                Math.Max(0, mapping.OriginalSpan.LineIndex),
                Math.Max(0, mapping.OriginalSpan.CharacterIndex)));
        }

        foreach (var compilerSegment in compilerMap.Segments)
        {
            if (compilerSegment.SourceIndex < 0 || compilerSegment.SourceIndex >= compilerMap.Sources.Count)
                continue;

            var compilerSource = compilerMap.Sources[compilerSegment.SourceIndex];
            if (!IsGeneratedCSharpSourcePath(compilerSource.Path, document.HintName))
                continue;

            if (!TryResolveOriginalSourcePosition(
                document,
                orderedMappings,
                compilerSegment.SourceLine,
                compilerSegment.SourceColumn,
                out var mapped))
            {
                continue;
            }

            var sourceIndex = GetOrAddSourceIndex(
                sources,
                sourceIndexByPath,
                mapped.SourcePath,
                mapped.SourceContent);
            segments.Add(new SourceMapSegment(
                compilerSegment.SourceLine,
                compilerSegment.SourceColumn,
                sourceIndex,
                mapped.SourceLine,
                mapped.SourceColumn));
        }

        return new SourceMapDocument(
            NormalizeGeneratedSourcePath(document.HintName),
            sources,
            segments
                .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
                .Select(static group => group.First())
                .OrderBy(static segment => segment.GeneratedLine)
                .ThenBy(static segment => segment.GeneratedColumn)
                .ToArray());
    }

    private static bool TryResolveOriginalSourcePosition(
        GeneratedDocument document,
        IReadOnlyList<RazorSourceMap> orderedMappings,
        int generatedLine,
        int generatedColumn,
        out MappedSourcePosition mapped)
    {
        mapped = default;
        if (orderedMappings.Count == 0 ||
            !TryGetAbsoluteIndex(document.GeneratedCSharp, generatedLine, generatedColumn, out var generatedAbsoluteIndex))
        {
            return false;
        }

        RazorSourceMap candidate = default;
        var hasCandidate = false;
        foreach (var mapping in orderedMappings)
        {
            var start = mapping.GeneratedSpan.AbsoluteIndex;
            var end = start + Math.Max(mapping.GeneratedSpan.Length, 0);
            if (generatedAbsoluteIndex >= start && generatedAbsoluteIndex <= end)
            {
                candidate = mapping;
                hasCandidate = true;
            }
            else if (generatedAbsoluteIndex >= start)
            {
                candidate = mapping;
                hasCandidate = true;
            }
            else
            {
                break;
            }
        }

        if (!hasCandidate)
            return false;

        var offset = Math.Max(0, generatedAbsoluteIndex - candidate.GeneratedSpan.AbsoluteIndex);
        if (candidate.OriginalSpan.Length > 0)
            offset = Math.Min(offset, candidate.OriginalSpan.Length - 1);

        var sourceFilePath = candidate.OriginalSpan.FilePath ?? document.SourcePath;
        mapped = new MappedSourcePosition(
            NormalizeSourcePath(sourceFilePath),
            TryGetSourceMapSourceContent(sourceFilePath),
            Math.Max(0, candidate.OriginalSpan.LineIndex),
            Math.Max(0, candidate.OriginalSpan.CharacterIndex + offset));
        return true;
    }

    private static bool TryGetAbsoluteIndex(
        Microsoft.CodeAnalysis.Text.SourceText text,
        int line,
        int column,
        out int absoluteIndex)
    {
        absoluteIndex = 0;
        if (line < 0 || line >= text.Lines.Count)
            return false;

        var textLine = text.Lines[line];
        var safeColumn = Math.Max(0, Math.Min(column, textLine.End - textLine.Start));
        absoluteIndex = textLine.Start + safeColumn;
        return true;
    }

    private static bool IsGeneratedCSharpSourcePath(string sourcePath, string hintName)
    {
        var normalizedSourcePath = NormalizeGeneratedSourcePath(sourcePath);
        var normalizedHintName = NormalizeGeneratedSourcePath(hintName);
        return string.Equals(normalizedSourcePath, normalizedHintName, StringComparison.OrdinalIgnoreCase) ||
               normalizedSourcePath.EndsWith("/" + normalizedHintName, StringComparison.OrdinalIgnoreCase) ||
               normalizedSourcePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
    }

    private static SourceMapDocument PruneIntermediateSources(SourceMapDocument document, string relativePath)
    {
        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var segments = new List<SourceMapSegment>();

        foreach (var segment in document.Segments)
        {
            if (segment.SourceIndex < 0 || segment.SourceIndex >= document.Sources.Count)
                continue;

            var source = document.Sources[segment.SourceIndex];
            if (IsIntermediateSource(source.Path, relativePath))
                continue;

            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, source.Path, source.Content);
            segments.Add(segment with { SourceIndex = sourceIndex });
        }

        return new SourceMapDocument(
            document.File,
            sources,
            segments
                .GroupBy(static segment => (segment.GeneratedLine, segment.GeneratedColumn, segment.SourceIndex, segment.SourceLine, segment.SourceColumn))
                .Select(static group => group.First())
                .OrderBy(static segment => segment.GeneratedLine)
                .ThenBy(static segment => segment.GeneratedColumn)
                .ToArray());
    }

    private static bool IsIntermediateSource(string sourcePath, string relativePath)
    {
        var normalized = NormalizeGeneratedSourcePath(sourcePath);
        return string.Equals(normalized, NormalizeGeneratedSourcePath(relativePath), StringComparison.OrdinalIgnoreCase) ||
               normalized.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetOrAddSourceIndex(
        List<SourceMapSource> sources,
        Dictionary<string, int> sourceIndexByPath,
        string path,
        string? content)
    {
        // A chained map can introduce an authored Razor path before the corresponding
        // segment is projected. Hydrate that existing source entry at the merge boundary.
        content ??= TryGetSourceMapSourceContent(path);
        var normalizedPath = NormalizeGeneratedSourcePath(path);
        if (sourceIndexByPath.TryGetValue(normalizedPath, out var existingIndex))
        {
            if (sources[existingIndex].Content is null && content is not null)
                sources[existingIndex] = sources[existingIndex] with { Content = content };

            return existingIndex;
        }

        var index = sources.Count;
        sources.Add(new SourceMapSource(path, content));
        sourceIndexByPath[normalizedPath] = index;
        return index;
    }

    private static string? TryGetSourceMapSourceContent(string? sourcePath)
        // Razor files are not public static assets. Capture generator-provided text so DevTools
        // can show the source that produced this module without a second HTTP route or analyzer I/O.
        => RazorSourceTextRegistry.TryGet(sourcePath);

    private static void AddModuleMapAlias(
        Dictionary<string, string> moduleMaps,
        string? path,
        string sourceMapContent)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        moduleMaps[NormalizeGeneratedSourcePath(path!)] = sourceMapContent;
    }

    private static string BuildCoarseSourceMapContent(
        BoundComponent component,
        string relativePath,
        string moduleText)
    {
        var sourceSpan = component.Document.RazorSourceMaps
            .OrderBy(static mapping => mapping.GeneratedSpan.LineIndex)
            .ThenBy(static mapping => mapping.GeneratedSpan.CharacterIndex)
            .Select(static mapping => mapping.OriginalSpan)
            .FirstOrDefault();
        var sourceFilePath = sourceSpan.FilePath ?? component.Document.SourcePath;
        var sourcePath = NormalizeSourcePath(sourceFilePath);
        var sourceLine = Math.Max(0, sourceSpan.LineIndex);
        var sourceColumn = Math.Max(0, sourceSpan.CharacterIndex);
        var generatedLine = FindGeneratedLine(moduleText, "scope.$renderDirect()");
        var document = new SourceMapDocument(
            relativePath,
            [new SourceMapSource(sourcePath, TryGetSourceMapSourceContent(sourceFilePath))],
            [new SourceMapSegment(generatedLine, 0, 0, sourceLine, sourceColumn)]);

        return Util.NormalizeLineEndingsToLf(new SourceMapWriter().Write(document));
    }

    private static string? TryGetCompilationSourceRoot(Compilation compilation, GeneratedDocument document)
    {
        var directories = new List<string>();
        AddDirectory(document.SourcePath);
        foreach (var tree in compilation.SyntaxTrees)
            AddDirectory(tree.FilePath);

        if (directories.Count == 0)
            return null;

        var current = Path.GetFullPath(directories[0]);
        while (!string.IsNullOrWhiteSpace(current))
        {
            var normalizedCurrent = EnsureDirectorySeparator(current);
            var containsAll = true;
            foreach (var directory in directories)
            {
                var normalizedDirectory = EnsureDirectorySeparator(Path.GetFullPath(directory));
                if (!normalizedDirectory.StartsWith(normalizedCurrent, StringComparison.OrdinalIgnoreCase))
                {
                    containsAll = false;
                    break;
                }
            }

            if (containsAll)
                return current;

            current = Path.GetDirectoryName(current);
        }

        return null;

        void AddDirectory(string? sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !Path.IsPathRooted(sourcePath))
                return;

            try
            {
                var fullPath = Path.GetFullPath(sourcePath);
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    directories.Add(directory);
            }
            catch
            {
                // Best effort only. Source map path normalization must not make component generation fail.
            }
        }
    }

    private static string EnsureDirectorySeparator(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? path
            : path + Path.DirectorySeparatorChar;
    }

    private static int FindGeneratedLine(string moduleText, string needle)
    {
        var line = 0;
        foreach (var item in moduleText.Split('\n'))
        {
            if (item.Contains(needle, StringComparison.Ordinal))
                return line;

            line++;
        }

        return 0;
    }

    private static string NormalizeSourcePath(string sourcePath)
    {
        var normalized = (sourcePath ?? string.Empty).Replace('\\', '/').Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return "component.razor";

        var pagesIndex = normalized.LastIndexOf("/Pages/", StringComparison.OrdinalIgnoreCase);
        if (pagesIndex >= 0)
            return normalized.Substring(pagesIndex + 1);

        if (!Path.IsPathRooted(normalized))
            return normalized.TrimStart('/');

        var fileName = Path.GetFileName(normalized);
        return string.IsNullOrWhiteSpace(fileName)
            ? "component.razor"
            : fileName;
    }

    private static string NormalizeGeneratedSourcePath(string sourcePath)
    {
        var normalized = (sourcePath ?? string.Empty).Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        return normalized.TrimStart('/');
    }

    /// <summary>Groups lifecycle symbols after they have been projected to runtime member names.</summary>
    private sealed record ComponentLifecycleRuntimeMembers(
        string? OnInitialized,
        string? OnInitializedAsync,
        string? OnParametersSet,
        string? OnParametersSetAsync,
        string? SetParametersAsync,
        string? OnAfterRender,
        string? OnAfterRenderAsync,
        string? ShouldRender,
        ImmutableArray<string> DisposeMemberNames,
        ImmutableArray<string> DisposeAsyncMemberNames)
    {
        public bool HasOnInitialized => OnInitialized is not null;

        public bool HasOnInitializedAsync => OnInitializedAsync is not null;

        public bool HasOnParametersSet => OnParametersSet is not null;

        public bool HasOnParametersSetAsync => OnParametersSetAsync is not null;

        public bool HasSetParametersAsync => SetParametersAsync is not null;

        public bool HasOnAfterRender => OnAfterRender is not null;

        public bool HasOnAfterRenderAsync => OnAfterRenderAsync is not null;

        public bool HasShouldRender => ShouldRender is not null;

        public static ComponentLifecycleRuntimeMembers Create(
            MemberClosure closure,
            IReadOnlyDictionary<ISymbol, string>? declaredNames)
        {
            string? onInitialized = null;
            string? onInitializedAsync = null;
            string? onParametersSet = null;
            string? onParametersSetAsync = null;
            string? setParametersAsync = null;
            string? onAfterRender = null;
            string? onAfterRenderAsync = null;
            string? shouldRender = null;
            var disposeMemberNames = ImmutableArray.CreateBuilder<string>();
            var disposeAsyncMemberNames = ImmutableArray.CreateBuilder<string>();

            foreach (var method in closure.LifecycleRoots)
            {
                var runtimeName = GetRuntimeMemberName(method, declaredNames);
                if (string.Equals(method.Name, "OnInitialized", StringComparison.Ordinal))
                {
                    onInitialized = runtimeName;
                }
                else if (string.Equals(method.Name, "OnInitializedAsync", StringComparison.Ordinal))
                {
                    onInitializedAsync = runtimeName;
                }
                else if (string.Equals(method.Name, "OnParametersSet", StringComparison.Ordinal))
                {
                    onParametersSet = runtimeName;
                }
                else if (string.Equals(method.Name, "OnParametersSetAsync", StringComparison.Ordinal))
                {
                    onParametersSetAsync = runtimeName;
                }
                else if (string.Equals(method.Name, "SetParametersAsync", StringComparison.Ordinal))
                {
                    setParametersAsync = runtimeName;
                }
                else if (string.Equals(method.Name, "OnAfterRender", StringComparison.Ordinal))
                {
                    onAfterRender = runtimeName;
                }
                else if (string.Equals(method.Name, "OnAfterRenderAsync", StringComparison.Ordinal))
                {
                    onAfterRenderAsync = runtimeName;
                }
                else if (string.Equals(method.Name, "ShouldRender", StringComparison.Ordinal))
                {
                    shouldRender = runtimeName;
                }
                else if (IsSynchronousDisposeMethod(method))
                {
                    AddDistinct(disposeMemberNames, runtimeName);
                }
                else if (IsAsynchronousDisposeMethod(method))
                {
                    AddDistinct(disposeAsyncMemberNames, runtimeName);
                }
            }

            return new ComponentLifecycleRuntimeMembers(
                onInitialized,
                onInitializedAsync,
                onParametersSet,
                onParametersSetAsync,
                setParametersAsync,
                onAfterRender,
                onAfterRenderAsync,
                shouldRender,
                disposeMemberNames.ToImmutable(),
                disposeAsyncMemberNames.ToImmutable());
        }

        private static void AddDistinct(ImmutableArray<string>.Builder names, string name)
        {
            if (!names.Contains(name, StringComparer.Ordinal))
                names.Add(name);
        }
    }

    /// <summary>Captures Vue helper requirements discovered during module framing.</summary>
    private sealed record VueModuleFeatures(
        ComponentLifecycleRuntimeMembers LifecycleMembers,
        bool UsesSlots,
        bool UsesFactorySlots,
        bool UsesFactoryProps,
        bool UsesSetupProps,
        bool UsesState,
        bool UsesStateHasChanged,
        bool UsesInvokeAsync,
        ImmutableArray<string> ParameterNames,
        ImmutableArray<ParameterBinding> ParameterBindings,
        bool UsesParameterViewSlots,
        bool UsesUnmatchedAttributes,
        ImmutableArray<InjectBinding> InjectBindings,
        ImmutableArray<CascadingBinding> CascadingBindings,
        string ComponentDisplayName)
    {
        public string? OnInitialized => LifecycleMembers.OnInitialized;

        public string? OnInitializedAsync => LifecycleMembers.OnInitializedAsync;

        public string? OnParametersSet => LifecycleMembers.OnParametersSet;

        public string? OnParametersSetAsync => LifecycleMembers.OnParametersSetAsync;

        public string? SetParametersAsync => LifecycleMembers.SetParametersAsync;

        public string? OnAfterRender => LifecycleMembers.OnAfterRender;

        public string? OnAfterRenderAsync => LifecycleMembers.OnAfterRenderAsync;

        public string? ShouldRender => LifecycleMembers.ShouldRender;

        public bool HasShouldRender => LifecycleMembers.HasShouldRender;

        public ImmutableArray<string> DisposeMemberNames => LifecycleMembers.DisposeMemberNames;

        public ImmutableArray<string> DisposeAsyncMemberNames => LifecycleMembers.DisposeAsyncMemberNames;

        public bool UsesWatch => LifecycleMembers.HasOnParametersSet ||
                                  LifecycleMembers.HasOnParametersSetAsync ||
                                  LifecycleMembers.HasSetParametersAsync ||
                                  UsesUnmatchedAttributes ||
                                  UsesCascading;

        public bool UsesParameterViewState => LifecycleMembers.HasSetParametersAsync;

        public bool UsesInject => !InjectBindings.IsDefaultOrEmpty;

        public bool UsesCascading => !CascadingBindings.IsDefaultOrEmpty;

        public ImmutableArray<InjectBinding> InjectResolutions
            => InjectBindings
                .GroupBy(static binding => binding.LocalName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToImmutableArray();

        public ImmutableArray<CascadingBinding> CascadingResolutions
            => CascadingBindings
                .GroupBy(static binding => binding.LocalName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToImmutableArray();

        public bool UsesMounted => LifecycleMembers.HasOnAfterRender || LifecycleMembers.HasOnAfterRenderAsync;

        public bool UsesUpdated => LifecycleMembers.HasOnAfterRender || LifecycleMembers.HasOnAfterRenderAsync;

        public bool HasDispose => !DisposeMemberNames.IsDefaultOrEmpty;

        public bool HasDisposeAsync => !DisposeAsyncMemberNames.IsDefaultOrEmpty;

        public bool UsesUnmounted => HasDispose || HasDisposeAsync;

        public bool UsesReactive => UsesState || UsesStateHasChanged;
    }

    /// <summary>Returns framed module text and compiler-to-artifact line correspondence.</summary>
    private sealed record ModuleTextBuildResult(
        string ModuleText,
        ImmutableArray<CompiledLineMapping> CompiledLineMappings,
        ImmutableArray<string> PackageImports,
        ImmutableArray<VueAsset> Assets);

    private readonly record struct MappedSourcePosition(
        string SourcePath,
        string? SourceContent,
        int SourceLine,
        int SourceColumn);

    private readonly record struct CompiledLineMapping(
        int GeneratedLine,
        int GeneratedColumn,
        int CompiledLine,
        int CompiledColumn,
        bool IsDirectRenderOrigin = false);

    /// <summary>Captures the identity of one module-scope local import binding.</summary>
    private readonly record struct ImportBinding(
        string ModulePath,
        string Kind,
        string ImportedName);

    /// <summary>Pairs one compiler statement with its source position for source-map merging.</summary>
    private sealed record CompilerStatement(
        Statement Statement,
        int CompiledLine,
        int CompiledColumn);

    /// <summary>Bundles compiler output and optional layout metadata for one component pass.</summary>
    private sealed record CompilerOutput(
        Module? Module,
        GeneratedJavaScriptLayout? Layout,
        ComponentInitializationBuildResult Initialization,
        VueRenderRuntimeFeatures OrdinaryRenderFeatures);

    /// <summary>Separates imports, declarations, and executable statements for deterministic framing.</summary>
    private sealed record CompilerModuleParts(
        ImmutableArray<ImportDeclaration> ImportDeclarations,
        ImmutableArray<CompilerStatement> ModuleStatements,
        ImmutableArray<CompilerStatement> SetupStatements,
        ImmutableArray<StateSlot> StateSlots,
        ImmutableArray<ComponentInitializationPhaseBuild> InitializationPhases,
        IReadOnlyDictionary<SourceOrigin, GeneratedNodePosition> CompilerOriginPositions);

    /// <summary>Stores direct RenderTree lowering plus the Vue helper features it requires.</summary>
    private sealed record DirectRenderBuildResult(
        Expression RenderExpression,
        string MemberName,
        ImmutableArray<Statement> PreludeStatements,
        ImmutableArray<RenderModuleHoist> ModuleHoists,
        bool UsesFragment,
        bool UsesStaticVNode,
        bool UsesRawMarkupRuntime,
        bool UsesBlockTree,
        bool UsesTextVNode,
        bool UsesRenderList,
        bool UsesWithCtx,
        bool UsesCreateSlots,
        bool UsesMergeProps,
        bool UsesHandlerCache,
        bool UsesProps,
        bool UsesSlots,
        ImmutableArray<ImportDeclaration> ImportDeclarations,
        ImmutableArray<ISymbol> ReferenceCaptureStateMembers);

    /// <summary>Maps one Blazor parameter name to its Vue carrier and component state slot.</summary>
    private sealed record ParameterBinding(
        string SourceName,
        string RuntimeName,
        string StateName,
        bool IsSlot,
        bool CapturesUnmatchedValues);

    /// <summary>One standard Blazor [Inject] property projected to a Vue provider lookup.</summary>
    private sealed record InjectBinding(
        IPropertySymbol Property,
        string ServiceKey,
        string LocalName,
        string StateName,
        string ServiceTypeDisplay);

    /// <summary>One [CascadingParameter] projected to a typed Vue provide/inject ref.</summary>
    private sealed record CascadingBinding(
        IPropertySymbol Property,
        string ServiceKey,
        string LocalName,
        string StateName);

    /// <summary>Represents one component field/property projected into Vue reactive state.</summary>
    private sealed record StateSlot(
        ISymbol Member,
        string RuntimeName,
        string? DeclarationName,
        ITypeSymbol Type,
        Expression? Initializer,
        bool HasExplicitInitializer,
        int? InitializerCompiledLine = null,
        int? InitializerCompiledColumn = null);
}

/// <summary>One generated Vue module and the source assets it imports. 是 emit pipeline 写入 .mjs、map 与附属资源的不可变载体。</summary>
internal sealed record VueModuleArtifact(
    string ComponentId,
    string RelativePath,
    string ModuleText,
    string ContentHash,
    string SourceMapRelativePath,
    string SourceMapContent,
    string MapHash,
    ImmutableArray<string> PackageImports,
    ImmutableArray<VueAsset> Assets,
    VueHmrMetadata Hmr);

/// <summary>One source asset copied beside the generated module. 记录 artifact 相对路径与内容哈希以支持确定性物化。</summary>
internal sealed record VueAsset(
    string SourcePath,
    string ArtifactPath,
    string Kind,
    string ImportPath,
    string ContentHash);
