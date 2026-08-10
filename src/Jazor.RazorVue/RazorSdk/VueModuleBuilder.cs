using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Common.SourceMaps;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>
/// Builds the final Vue render module from official Razor SG C# and compiler-owned lowering.
/// Framing stays here; C# expression semantics stay in <see cref="AstConverter"/>.
/// </summary>
internal static class VueModuleBuilder
{
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;
    private static readonly ImmutableHashSet<string> FramingReservedNames =
    new[]
    {
        "defineComponent",
        "h",
        "Fragment",
        "createStaticVNode",
        "onMounted",
        "onUnmounted",
        "onUpdated",
        "reactive",
        "watch",
        "props",
        "slots",
        "state",
        "scope",
        "invalidate",
        "pendingInvalidations",
        "disposed",
        "hasRendered",
        "cachedVNode",
        "stateHasChanged",
        "invokeAsync",
        "parametersSetAsyncGen",
        "parametersSetAsyncTail",
        "runOnParametersSetAsync"
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
            cancellationToken).ConfigureAwait(false);
        var directRender = BuildOperationDirectRender(binding, component, declaredNames, injectRegistry);

        // Compiler and component imports are discovered after lowering. Re-run only when an
        // authored member would shadow one of those bindings inside the setup scope.
        var importLocalNames = CollectImportLocalNames(compilerOutput.Module, directRender);
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
                cancellationToken).ConfigureAwait(false);
            directRender = BuildOperationDirectRender(binding, component, declaredNames, injectRegistry);
        }

        var moduleBuild = BuildModuleText(
            component,
            closure,
            directRender,
            compilerOutput.Module,
            compilerOutput.Layout?.NodePositions,
            relativePath,
            declaredNames);
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
            BuildHmrMetadata(component, closure, relativePath));
    }

    private static async Task<CompilerOutput> BuildCompilerOutputAsync(
        GeneratedCSharpBinding binding,
        BoundComponent component,
        MemberClosure closure,
        SemanticModel semanticModel,
        string relativePath,
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        CancellationToken cancellationToken)
    {
        var converter = new AstConverter(
            component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions(
                declaredNames: declaredNames,
                propertyReferenceRewriter: CreateDirectRenderSlotParameterPropertyReferenceRewriter(closure)));
        var module = await converter.Convert(cancellationToken).ConfigureAwait(false);
        module = AppendFlattenedRuntimeClasses(module, converter, component, closure, cancellationToken);
        var layout = module is null
            ? null
            : module.ToKnRECMAScriptWithSourceMapAndNodePositions(
                generatedFileName: relativePath,
                includeSourcesContent: false,
                sourceRootPath: TryGetCompilationSourceRoot(binding.Compilation, component.Document),
                readSourceContent: null);
        return new CompilerOutput(module, layout);
    }

    private static Module? AppendFlattenedRuntimeClasses(
        Module? module,
        AstConverter converter,
        BoundComponent component,
        MemberClosure closure,
        CancellationToken cancellationToken)
    {
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
        IReadOnlyDictionary<ISymbol, string> declaredNames,
        VueInjectRegistry injectRegistry)
    {
        if (TryBuildOperationDirectRender(
                binding,
                component,
                declaredNames,
                injectRegistry,
                out var directRender,
                out var directRenderFailure))
        {
            return directRender;
        }

        throw new InvalidOperationException(
            "RazorVue direct render lowering failed for '" +
            component.ComponentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) +
            "': " +
            (string.IsNullOrWhiteSpace(directRenderFailure) ? "No failure detail was provided." : directRenderFailure));
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
        DirectRenderBuildResult directRender)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (compilerModule is not null)
        {
            foreach (var import in compilerModule.Body.OfType<ImportDeclaration>())
                AddImportLocalNames(import, names);
        }

        foreach (var import in directRender.ImportDeclarations)
            AddImportLocalNames(import, names);
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
        string relativePath,
        IReadOnlyDictionary<ISymbol, string>? declaredNames)
    {
        var parts = BuildCompilerModuleParts(compilerModule, compilerNodePositions, closure);
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

        var setupFactoryName = "create" + SanitizeJavaScriptIdentifierPart(componentSymbol.Name, "Component") + "SetupScope";
        var returnedMembers = GetReturnedMembers(closure, declaredNames);
        var lifecycleMembers = ComponentLifecycleRuntimeMembers.Create(closure, declaredNames);
        returnedMembers = returnedMembers
            .RemoveAll(member => string.Equals(member, buildRenderTreeMemberName, StringComparison.Ordinal))
            .Add(directRender.MemberName);
        parts = parts with
        {
            SetupStatements = RemoveDirectRenderOnlyFunctions(
                parts.SetupStatements,
                directRender,
                parts.StateSlots,
                returnedMembers)
        };

        var usesInvokeAsync = ReferencesIdentifier(parts.SetupStatements, "invokeAsync");

        var usesFactorySlots = directRender.UsesSlots ||
                               ReferencesIdentifier(parts.SetupStatements, "slots") ||
                               parts.StateSlots.Any(static slot =>
                                   slot.Initializer is not null &&
                                   AstReferenceAnalysis.ReferencesIdentifier(slot.Initializer, "slots"));
        var usesSlots = usesFactorySlots;
        var usesFactoryProps = ReferencesIdentifier(parts.SetupStatements, "props") || directRender.UsesProps;
        var usesSetupProps = usesFactoryProps ||
                             usesSlots ||
                             lifecycleMembers.HasOnParametersSet ||
                             lifecycleMembers.HasOnParametersSetAsync;
        var usesState = parts.StateSlots.Length > 0;
        var usesStateHasChanged = lifecycleMembers.HasOnInitializedAsync ||
                                  lifecycleMembers.HasOnParametersSetAsync ||
                                  ReferencesIdentifier(parts.SetupStatements, "stateHasChanged");
        var features = new VueModuleFeatures(
            lifecycleMembers,
            usesSlots,
            usesFactorySlots,
            usesFactoryProps,
            usesSetupProps,
            usesState,
            usesStateHasChanged,
            usesInvokeAsync);
        var moduleStatements = new List<Statement>();
        var assets = ImmutableArray.CreateBuilder<VueAsset>();
        // Every production import is created by ImportDeclarationFactory and therefore owns at
        // least one local binding. Local bindings are the module-scope uniqueness contract: the
        // same declaration repeats the same locals, while a conflicting declaration must not
        // shadow an already emitted compiler binding.
        var emittedImportLocals = new HashSet<string>(StringComparer.Ordinal);

        moduleStatements.Add(BuildVueImportDeclaration(
            features.UsesMounted,
            features.UsesUnmounted,
            features.UsesUpdated,
            features.UsesReactive,
            features.UsesWatch,
            directRender.UsesFragment,
            directRender.UsesStaticVNode));

        foreach (var importDeclaration in parts.ImportDeclarations)
        {
            if (IsVueFramingImport(importDeclaration))
                continue;
            if (!IsCompilerImportReferenced(importDeclaration, directRender, parts))
                continue;

            var rebasedImport = RebaseImportDeclaration(importDeclaration, relativePath);
            if (HasAnyImportLocalName(rebasedImport, emittedImportLocals))
                continue;
            moduleStatements.Add(rebasedImport);
            AddImportLocalNames(rebasedImport, emittedImportLocals);
            if (TryCreateVueSfcAsset(rebasedImport, relativePath, out var asset))
                assets.Add(asset);
        }

        foreach (var importDeclaration in directRender.ImportDeclarations)
        {
            var rebasedImport = RebaseImportDeclaration(importDeclaration, relativePath);
            if (HasAnyImportLocalName(rebasedImport, emittedImportLocals))
                continue;
            moduleStatements.Add(rebasedImport);
            AddImportLocalNames(rebasedImport, emittedImportLocals);
        }

        moduleStatements.Add(BuildSetupFactoryDeclaration(
            setupFactoryName,
            returnedMembers,
            parts,
            directRender,
            features));
        moduleStatements.Add(BuildVueComponentExport(
            closure,
            setupFactoryName,
            directRender,
            features));

        var vueModule = new Module(NodeList.From(moduleStatements));
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
            statements.Add(BuildStateDeclaration(parts.StateSlots));

        statements.AddRange(parts.SetupStatements
            .Where(static item => item.Statement is not ClassDeclaration)
            .Select(static item => item.Statement));
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

    private static ObjectExpression BuildReturnedMembersExpression(ImmutableArray<string> returnedMembers)
        => new(NodeList.From<Node>(returnedMembers.Select(static name =>
            (Node)new ObjectProperty(
                PropertyKind.Init,
                new Identifier(name),
                new Identifier(name),
                computed: false,
                shorthand: true,
                method: false))));

    private static ExportDefaultDeclaration BuildVueComponentExport(
        MemberClosure closure,
        string setupFactoryName,
        DirectRenderBuildResult directRender,
        VueModuleFeatures features)
    {
        var componentOptions = new List<Node>();
        var propNames = GetVuePropNames(closure);
        if (propNames.Length > 0)
            componentOptions.Add(CreateObjectProperty("props", CreateStringArray(propNames)));

        var setupFunction = new FunctionExpression(
            id: null,
            parameters: NodeList.From<Node>(BuildVueSetupParameters(features)),
            body: CreateFunctionBody(BuildVueSetupStatements(
                setupFactoryName,
                directRender,
                features)),
            generator: false,
            async: false);
        componentOptions.Add(new ObjectProperty(
            PropertyKind.Init,
            new Identifier("setup"),
            setupFunction,
            computed: false,
            shorthand: false,
            method: true));

        return new ExportDefaultDeclaration(CreateCall(
            "defineComponent",
            new ObjectExpression(NodeList.From(componentOptions))));
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
    }

    private static IEnumerable<Expression> BuildSetupFactoryArguments(VueModuleFeatures features)
    {
        if (features.UsesFactoryProps)
            yield return new Identifier("props");
        if (features.UsesFactorySlots)
            yield return new Identifier("slots");
        if (features.UsesStateHasChanged)
            yield return new Identifier("stateHasChanged");
        if (features.UsesInvokeAsync)
            yield return new Identifier("invokeAsync");
    }

    private static IEnumerable<Node> BuildVueSetupParameters(VueModuleFeatures features)
    {
        if (features.UsesSlots)
        {
            yield return new Identifier("props");
            var slots = new Identifier("slots");
            yield return new ObjectPattern(NodeList.From<Node>(new AssignmentProperty(
                slots,
                new Identifier("slots"),
                computed: false,
                shorthand: true)));
            yield break;
        }

        if (features.UsesSetupProps)
            yield return new Identifier("props");
    }

    private static List<Statement> BuildVueSetupStatements(
        string setupFactoryName,
        DirectRenderBuildResult directRender,
        VueModuleFeatures features)
    {
        var statements = new List<Statement>();

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

        statements.Add(CreateVariableDeclaration(
            VariableDeclarationKind.Const,
            "scope",
            CreateCall(
                setupFactoryName,
                BuildSetupFactoryArguments(features))));

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

        if (features.OnInitialized is { } onInitialized)
            statements.Add(CreateExpressionStatement(CreateScopeCall(onInitialized)));

        if (features.OnInitializedAsync is { } onInitializedAsync)
        {
            statements.Add(CreateExpressionStatement(CreateCallMember(
                CreateCallMember(new Identifier("Promise"), "resolve", CreateScopeCall(onInitializedAsync)),
                "then",
                CreateStateHasChangedCallback(),
                CreateStateHasChangedCallback())));
        }

        if (features.OnParametersSet is { } onParametersSet)
        {
            statements.Add(CreateExpressionStatement(CreateScopeCall(onParametersSet)));
            statements.Add(CreateWatchStatement(onParametersSet));
        }

        if (features.OnParametersSetAsync is { } onParametersSetAsync)
            statements.AddRange(BuildOnParametersSetAsyncStatements(onParametersSetAsync));

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

    private static VariableDeclaration BuildStateHasChangedDeclaration(bool usesUnmounted)
    {
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

    private static Statement CreateWatchStatement(string scopeMethod)
        => CreateExpressionStatement(CreateCall(
            "watch",
            CreateArrowExpression(new Identifier("props")),
            CreateArrowFunction(
                [],
                [CreateExpressionStatement(CreateScopeCall(scopeMethod))]),
            BuildDeepWatchOptions()));

    private static IEnumerable<Statement> BuildOnParametersSetAsyncStatements(string scopeMethod)
    {
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
        yield return CreateExpressionStatement(CreateCall(
            "watch",
            CreateArrowExpression(new Identifier("props")),
            CreateArrowFunction(
                [],
                [CreateExpressionStatement(CreateCall("runOnParametersSetAsync"))]),
            BuildDeepWatchOptions()));
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

    private static ObjectExpression BuildDeepWatchOptions()
        => new(NodeList.From<Node>(CreateObjectProperty("deep", BooleanLiteral(true))));

    private static Statement CreateLifecycleRegistration(
        string vueLifecycleMethod,
        string scopeMethod,
        BooleanLiteral firstRender,
        bool discardResult)
    {
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

    private static ImmutableArray<CompiledLineMapping> BuildCompiledLineMappings(
        IReadOnlyDictionary<Node, GeneratedNodePosition> generatedNodePositions,
        CompilerModuleParts parts)
    {
        var mappings = new HashSet<CompiledLineMapping>();
        foreach (var pair in generatedNodePositions)
        {
            if (pair.Key.UserData is SourceOrigin { IsSynthetic: false } origin &&
                parts.CompilerOriginPositions.TryGetValue(origin, out var compiledPosition))
            {
                mappings.Add(new CompiledLineMapping(
                    pair.Value.Line,
                    pair.Value.Column,
                    compiledPosition.Line,
                    compiledPosition.Column));
            }
        }

        foreach (var statement in parts.SetupStatements)
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
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        VueInjectRegistry injectRegistry,
        out DirectRenderBuildResult result,
        out string? failure)
    {
        result = default!;
        failure = null;
        if (!RenderEmitter.TryEmit(
                binding.Compilation,
                component.ComponentSymbol,
                component.BuildRenderTreeMethod,
                component.BuildRenderTreeBody,
                declaredNames,
                injectRegistry,
                out var operationResult,
                out failure))
        {
            return false;
        }

        result = new DirectRenderBuildResult(
            operationResult.RenderExpression,
            "$renderDirect",
            operationResult.PreludeStatements,
            operationResult.UsesFragment,
            operationResult.UsesStaticVNode,
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
                    break;
                case INamedTypeSymbol type when IsRuntimeMemberClass(type):
                    declaredNames[type.OriginalDefinition] = ChooseModuleDeclaredName(type, usedDeclaredNames, directLocalNames);
                    break;
            }
        }

        return declaredNames;
    }

    private static HashSet<string> CollectDirectRenderLocalNames(IBlockOperation buildRenderTreeBody)
    {
        var collector = new DirectRenderLocalNameCollector();
        collector.Visit(buildRenderTreeBody);
        return collector.Names;
    }

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
        ImmutableArray<string> returnedMembers)
    {
        // Direct lowering may inline a BuildRenderTree helper entirely. Only remove function
        // declarations that are unreachable from the surviving Vue module roots; declarations
        // are side-effect free, while every other top-level statement remains intact.
        var roots = new List<Node> { directRender.RenderExpression };
        roots.AddRange(directRender.PreludeStatements);
        roots.AddRange(stateSlots
            .Where(static slot => slot.Initializer is not null)
            .Select(static slot => slot.Initializer!));

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
        => declaration.Source.Value is "vue" or "@jazor/vue-runtime/render-context.mjs";

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

        var artifactPath = ResolveImportArtifactPath(specifier.Substring(0, specifier.Length - ".mjs".Length), importerRelativePath);
        asset = new VueAsset(
            SourcePath: artifactPath,
            ArtifactPath: artifactPath,
            Kind: "vue-sfc",
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

    private static bool HasAnyImportLocalName(
        ImportDeclaration declaration,
        HashSet<string> localNames)
    {
        foreach (var specifier in declaration.Specifiers)
        {
            var localName = specifier switch
            {
                ImportSpecifier named => named.Local.Name,
                ImportDefaultSpecifier defaultSpecifier => defaultSpecifier.Local.Name,
                ImportNamespaceSpecifier namespaceSpecifier => namespaceSpecifier.Local.Name,
                _ => string.Empty
            };
            if (!string.IsNullOrWhiteSpace(localName) && localNames.Contains(localName))
                return true;
        }

        return false;
    }

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
                ReferencesIdentifier(parts.SetupStatements, localName) ||
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
        bool usesStaticVNode)
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
    {
        var imports = ImmutableArray.CreateBuilder<ImportDeclaration>();
        var setupStatements = ImmutableArray.CreateBuilder<CompilerStatement>();
        var stateSlots = BuildStateSlots(closure);
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
            setupStatements.ToImmutable(),
            stateSlots.ToImmutableArray(),
            compilerOriginPositions);

        void AddDeclaration(Declaration declaration)
        {
            var declarationPosition = GetPosition(declaration);
            if (declaration is not VariableDeclaration variableDeclaration)
            {
                AddSetupStatement(declaration, declarationPosition);
                return;
            }

            var retainedDeclarators = ImmutableArray.CreateBuilder<VariableDeclarator>();
            foreach (var declarator in variableDeclaration.Declarations)
            {
                if (declarator.Id is not Identifier identifier)
                {
                    retainedDeclarators.Add(declarator);
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

                if (!discardedDeclarationNames.Contains(identifier.Name))
                    retainedDeclarators.Add(declarator);
            }

            if (retainedDeclarators.Count > 0)
            {
                var retainedDeclaration = retainedDeclarators.Count == variableDeclaration.Declarations.Count
                    ? variableDeclaration
                    : new VariableDeclaration(variableDeclaration.Kind, NodeList.From(retainedDeclarators));
                AddSetupStatement(retainedDeclaration, declarationPosition);
            }
        }

        void AddSetupStatement(Statement statement, GeneratedNodePosition position)
            => setupStatements.Add(new CompilerStatement(statement, position.Line, position.Column));

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
            slots.Add(new StateSlot(field, name, name, field.Type, null));
        }

        foreach (var property in closure.StateProperties)
        {
            slots.Add(new StateSlot(
                property,
                Util.GetConfigOrSymbolName(property),
                GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                property.Type,
                null));
        }

        return slots;
    }

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
        foreach (var field in componentSymbol.GetMembers().OfType<IFieldSymbol>())
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
                    original.ContainingNamespace?.ToDisplayString(),
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
        IMethodSymbol method,
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
                    attribute.AttributeClass?.ToDisplayString(),
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

        var assemblyName = componentSymbol.ContainingAssembly?.Name ?? "Jazor.Assembly";
        var namespaceName = componentSymbol.ContainingNamespace?.IsGlobalNamespace == true
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

        if (builder.Length == 0 || !IsJavaScriptIdentifierStart(builder[0]))
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
        var projectedCompilerMap = ProjectCompilerSourceMap(relativePath, compilerMap, compiledLineMappings);
        if (projectedCompilerMap.Segments.Count == 0)
            return false;

        var moduleMaps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
        ImmutableArray<CompiledLineMapping> compiledLineMappings)
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

        return new SourceMapDocument(
            relativePath,
            compilerMap.Sources,
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
            var sourcePath = NormalizeSourcePath(first.OriginalSpan.FilePath ?? document.SourcePath);
            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, sourcePath, null);
            segments.Add(new SourceMapSegment(
                0,
                0,
                sourceIndex,
                Math.Max(0, first.OriginalSpan.LineIndex),
                Math.Max(0, first.OriginalSpan.CharacterIndex)));
        }

        foreach (var mapping in orderedMappings)
        {
            var sourcePath = NormalizeSourcePath(mapping.OriginalSpan.FilePath ?? document.SourcePath);
            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, sourcePath, null);
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

            var sourceIndex = GetOrAddSourceIndex(sources, sourceIndexByPath, mapped.SourcePath, null);
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

        mapped = new MappedSourcePosition(
            NormalizeSourcePath(candidate.OriginalSpan.FilePath ?? document.SourcePath),
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
        var sourcePath = NormalizeSourcePath(sourceSpan.FilePath ?? component.Document.SourcePath);
        var sourceLine = Math.Max(0, sourceSpan.LineIndex);
        var sourceColumn = Math.Max(0, sourceSpan.CharacterIndex);
        var generatedLine = FindGeneratedLine(moduleText, "scope.$renderDirect()");
        var document = new SourceMapDocument(
            relativePath,
            [new SourceMapSource(sourcePath, null)],
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

    private sealed record ComponentLifecycleRuntimeMembers(
        string? OnInitialized,
        string? OnInitializedAsync,
        string? OnParametersSet,
        string? OnParametersSetAsync,
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

    private sealed record VueModuleFeatures(
        ComponentLifecycleRuntimeMembers LifecycleMembers,
        bool UsesSlots,
        bool UsesFactorySlots,
        bool UsesFactoryProps,
        bool UsesSetupProps,
        bool UsesState,
        bool UsesStateHasChanged,
        bool UsesInvokeAsync)
    {
        public string? OnInitialized => LifecycleMembers.OnInitialized;

        public string? OnInitializedAsync => LifecycleMembers.OnInitializedAsync;

        public string? OnParametersSet => LifecycleMembers.OnParametersSet;

        public string? OnParametersSetAsync => LifecycleMembers.OnParametersSetAsync;

        public string? OnAfterRender => LifecycleMembers.OnAfterRender;

        public string? OnAfterRenderAsync => LifecycleMembers.OnAfterRenderAsync;

        public string? ShouldRender => LifecycleMembers.ShouldRender;

        public bool HasShouldRender => LifecycleMembers.HasShouldRender;

        public ImmutableArray<string> DisposeMemberNames => LifecycleMembers.DisposeMemberNames;

        public ImmutableArray<string> DisposeAsyncMemberNames => LifecycleMembers.DisposeAsyncMemberNames;

        public bool UsesWatch => LifecycleMembers.HasOnParametersSet || LifecycleMembers.HasOnParametersSetAsync;

        public bool UsesMounted => LifecycleMembers.HasOnAfterRender || LifecycleMembers.HasOnAfterRenderAsync;

        public bool UsesUpdated => LifecycleMembers.HasOnAfterRender || LifecycleMembers.HasOnAfterRenderAsync;

        public bool HasDispose => !DisposeMemberNames.IsDefaultOrEmpty;

        public bool HasDisposeAsync => !DisposeAsyncMemberNames.IsDefaultOrEmpty;

        public bool UsesUnmounted => HasDispose || HasDisposeAsync;

        public bool UsesReactive => UsesState || UsesStateHasChanged;
    }

    private sealed record ModuleTextBuildResult(
        string ModuleText,
        ImmutableArray<CompiledLineMapping> CompiledLineMappings,
        ImmutableArray<string> PackageImports,
        ImmutableArray<VueAsset> Assets);

    private readonly record struct MappedSourcePosition(
        string SourcePath,
        int SourceLine,
        int SourceColumn);

    private readonly record struct CompiledLineMapping(
        int GeneratedLine,
        int GeneratedColumn,
        int CompiledLine,
        int CompiledColumn);

    private sealed record CompilerStatement(
        Statement Statement,
        int CompiledLine,
        int CompiledColumn);

    private sealed record CompilerOutput(
        Module? Module,
        GeneratedJavaScriptLayout? Layout);

    private sealed record CompilerModuleParts(
        ImmutableArray<ImportDeclaration> ImportDeclarations,
        ImmutableArray<CompilerStatement> SetupStatements,
        ImmutableArray<StateSlot> StateSlots,
        IReadOnlyDictionary<SourceOrigin, GeneratedNodePosition> CompilerOriginPositions);

    private sealed record DirectRenderBuildResult(
        Expression RenderExpression,
        string MemberName,
        ImmutableArray<Statement> PreludeStatements,
        bool UsesFragment,
        bool UsesStaticVNode,
        bool UsesProps,
        bool UsesSlots,
        ImmutableArray<ImportDeclaration> ImportDeclarations,
        ImmutableArray<ISymbol> ReferenceCaptureStateMembers);

    private sealed record StateSlot(
        ISymbol Member,
        string RuntimeName,
        string? DeclarationName,
        ITypeSymbol Type,
        Expression? Initializer,
        int? InitializerCompiledLine = null,
        int? InitializerCompiledColumn = null);
}

/// <summary>One generated Vue module and the source assets it imports.</summary>
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

/// <summary>One source asset copied beside the generated module.</summary>
internal sealed record VueAsset(
    string SourcePath,
    string ArtifactPath,
    string Kind,
    string ContentHash);
