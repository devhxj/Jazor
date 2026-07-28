using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Common.SourceMaps;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorSgVueComponentModuleBuilder
{
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string ParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.ParameterAttribute";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private const string VuePropAttributeMetadataName = "ECMAScript.VueContract.VuePropAttribute";
    private const string VueLibraryEmitAttributeMetadataName = "ECMAScript.VueContract.VueLibraryEmitAttribute";
    private const string VueSlotAttributeMetadataName = "ECMAScript.VueContract.VueSlotAttribute";
    private static readonly Regex VariableDeclarationPattern = new(
        @"^\s*(?:export\s+)?(?:let|const|var)\s+([A-Za-z_$][A-Za-z0-9_$]*)\s*(?:=\s*(.*))?;\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex VariableDeclarationStartPattern = new(
        @"^\s*(?:export\s+)?(?:let|const|var)\s+([A-Za-z_$][A-Za-z0-9_$]*)\s*(?:=\s*(.*))?\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex ImportFromPattern = new(
        @"^(?<prefix>\s*import\s+.+?\s+from\s+)(?<quote>[""'])(?<path>\./[^""']+)(\k<quote>)(?<suffix>\s*;?\s*)$",
        RegexOptions.CultureInvariant);
    private static readonly Regex RelativeImportFromPattern = new(
        @"^\s*import\s+.+?\s+from\s+(?<quote>[""'])(?<path>(?:\./|\.\./)[^""']+)(\k<quote>)\s*;?\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex BuildRenderTreeFunctionPattern = new(
        @"^\s*function\s+buildRenderTree\s*\(\s*(?<builder>[A-Za-z_$][A-Za-z0-9_$]*)\s*\)\s*\{\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex DirectFunctionPattern = new(
        @"^\s*function\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\(\s*(?<parameter>[A-Za-z_$][A-Za-z0-9_$]*)\s*\)\s*\{\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex DirectArrowFunctionPattern = new(
        @"^\s*(?:let|const|var)\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*\(?\s*(?<parameter>[A-Za-z_$][A-Za-z0-9_$]*)\s*\)?\s*=>\s*\{\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex DirectReturnArrowFunctionPattern = new(
        @"^\s*return\s+\(?\s*(?<parameter>[A-Za-z_$][A-Za-z0-9_$]*)\s*\)?\s*=>\s*\{\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex BuilderCallPattern = new(
        @"^\s*(?<builder>[A-Za-z_$][A-Za-z0-9_$]*)\.(?<method>[A-Za-z_$][A-Za-z0-9_$]*)\((?<arguments>.*)\);\s*$",
        RegexOptions.CultureInvariant);
    private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

    public static async Task<RazorSgVueComponentModuleArtifact> BuildAsync(
        RazorSgGeneratedCSharpBinding binding,
        RazorSgBoundComponent component,
        RazorSgComponentMemberClosure closure,
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

        var syntaxTree = component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree;
        var semanticModel = binding.Compilation.GetSemanticModel(syntaxTree);
        var declaredNames = BuildDirectRenderDeclaredNames(component, closure);
        var converter = new AstConverter(
            component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions(declaredNames: declaredNames));
        var relativePath = GetRelativePath(component.ComponentSymbol);
        var module = await converter.Convert(cancellationToken).ConfigureAwait(false);
        var compiledArtifact = module is null
            ? null
            : module.ToKnRECMAScriptWithSourceMap(
                generatedFileName: relativePath,
                includeSourcesContent: false,
                sourceRootPath: TryGetCompilationSourceRoot(binding.Compilation, component.Document),
                readSourceContent: null);
        var compiledScript = compiledArtifact is null
            ? string.Empty
            : Util.NormalizeLineEndingsToLf(compiledArtifact.Content).Trim();
        var moduleBuild = BuildModuleText(binding, component, closure, declaredNames, compiledScript, relativePath);
        var moduleText = moduleBuild.ModuleText;
        var sourceMapRelativePath = relativePath + ".map";
        var sourceMapContent = BuildSourceMapContent(
            component,
            relativePath,
            moduleText,
            compiledArtifact?.SourceMapContent,
            moduleBuild.CompiledLineMappings);

        return new RazorSgVueComponentModuleArtifact(
            component.ComponentSymbol.ToDisplayString(),
            relativePath,
            moduleText,
            ComputeContentHash(moduleText),
            sourceMapRelativePath,
            sourceMapContent,
            ComputeContentHash(sourceMapContent),
            moduleBuild.FrontendAssets);
    }

    private static ModuleTextBuildResult BuildModuleText(
        RazorSgGeneratedCSharpBinding binding,
        RazorSgBoundComponent component,
        RazorSgComponentMemberClosure closure,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        string compiledScript,
        string relativePath)
    {
        var parts = SplitCompiledScript(compiledScript, closure);
        var componentSymbol = component.ComponentSymbol;
        var directRender = TryBuildOperationDirectRender(binding, component, declaredNames, parts.SetupBodyLines, out var operationDirectRender)
            ? operationDirectRender
            : null;
        if (directRender is not null)
            parts = parts with { SetupBodyLines = directRender.SetupBodyLines };

        var usesInvokeAsync = parts.SetupBodyLines.Any(static line =>
            line.Text.Contains("invokeAsync(", StringComparison.Ordinal));
        var setupFactoryName = "create" + SanitizeJavaScriptIdentifierPart(componentSymbol.Name, "Component") + "SetupScope";
        var returnedMembers = GetReturnedMembers(closure);
        if (directRender is not null)
            returnedMembers = returnedMembers
                .RemoveAll(static member => string.Equals(member, "buildRenderTree", StringComparison.Ordinal))
                .Add(directRender.MemberName);

        var hasOnInitialized = returnedMembers.Contains("onInitialized", StringComparer.Ordinal);
        var hasOnInitializedAsync = returnedMembers.Contains("onInitializedAsync", StringComparer.Ordinal);
        var hasOnParametersSet = returnedMembers.Contains("onParametersSet", StringComparer.Ordinal);
        var hasOnParametersSetAsync = returnedMembers.Contains("onParametersSetAsync", StringComparer.Ordinal);
        var hasOnAfterRender = returnedMembers.Contains("onAfterRender", StringComparer.Ordinal);
        var hasOnAfterRenderAsync = returnedMembers.Contains("onAfterRenderAsync", StringComparer.Ordinal);
        var hasShouldRender = returnedMembers.Contains("shouldRender", StringComparer.Ordinal);
        var hasDispose = returnedMembers.Contains("dispose", StringComparer.Ordinal);
        var hasDisposeAsync = returnedMembers.Contains("disposeAsync", StringComparer.Ordinal);
        var usesSlots = HasSlotParameterBridges(closure);
        var usesFactorySlots = directRender?.UsesSlots == true;
        var usesFactoryProps = parts.SetupBodyLines.Any(static line =>
            line.Text.Contains("props.", StringComparison.Ordinal)) ||
            directRender?.UsesProps == true;
        var usesWatch = hasOnParametersSet || hasOnParametersSetAsync;
        var usesSetupProps = usesFactoryProps || usesSlots || usesWatch;
        var usesMounted = hasOnAfterRender || hasOnAfterRenderAsync;
        var usesUpdated = hasOnAfterRender || hasOnAfterRenderAsync;
        var usesUnmounted = hasDispose || hasDisposeAsync;
        var usesState = parts.StateSlots.Length > 0;
        var usesStateHasChanged = hasOnInitializedAsync ||
                                  hasOnParametersSetAsync ||
                                  parts.SetupBodyLines.Any(static line =>
                                      line.Text.Contains("stateHasChanged(", StringComparison.Ordinal));
        var usesReactive = usesState || usesStateHasChanged;
        var builder = new StringBuilder();
        var frontendAssets = ImmutableArray.CreateBuilder<RazorSgFrontendAsset>();
        var lineMappings = ImmutableArray.CreateBuilder<CompiledLineMapping>();
        var emittedImports = new HashSet<string>(StringComparer.Ordinal);
        var emittedImportLocals = new HashSet<string>(StringComparer.Ordinal);
        var line = 0;

        void AppendLine(string value = "")
        {
            builder.AppendLine(value);
            line++;
        }

        AppendLine(BuildVueImportLine(
            usesMounted,
            usesUnmounted,
            usesUpdated,
            usesReactive,
            usesWatch,
            directRender?.UsesFragment == true,
            directRender?.UsesStaticVNode == true));
        if (directRender is null)
            AppendLine("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";");

        foreach (var importLine in parts.ImportLines)
        {
            if (!string.Equals(importLine.Text, "import { defineComponent, h, onMounted, onUnmounted, onUpdated, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, onMounted, onUpdated, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, reactive } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal))
            {
                var rebasedImportLine = RebaseRootRelativeImportLine(importLine.Text, relativePath);
                if (HasAnyImportLocalName(rebasedImportLine, emittedImportLocals))
                    continue;
                if (!emittedImports.Add(rebasedImportLine))
                    continue;

                AppendLine(rebasedImportLine);
                AddImportLocalNames(rebasedImportLine, emittedImportLocals);
                if (TryCreateVueSfcAsset(rebasedImportLine, relativePath, out var asset))
                    frontendAssets.Add(asset);
            }
        }

        if (directRender is not null)
        {
            foreach (var importLine in directRender.ImportLines)
            {
                var rebasedImportLine = RebaseRootRelativeImportLine(importLine, relativePath);
                if (HasAnyImportLocalName(rebasedImportLine, emittedImportLocals))
                    continue;
                if (!emittedImports.Add(rebasedImportLine))
                    continue;

                AppendLine(rebasedImportLine);
                AddImportLocalNames(rebasedImportLine, emittedImportLocals);
            }
        }

        AppendLine();
        AppendLine("function " + setupFactoryName + "(" + BuildSetupFactoryParameterList(usesFactoryProps, usesFactorySlots, usesStateHasChanged, usesInvokeAsync) + ") {");
        if (usesState)
            AppendStateDeclaration(builder, parts.StateSlots, lineMappings, ref line);
        if (parts.SetupBodyLines.Length > 0)
        {
            AppendLine();
            AppendIndentedLines(builder, parts.SetupBodyLines, lineMappings, ref line);
        }

        if (directRender is not null)
        {
            AppendLine();
            AppendLine("  function " + directRender.MemberName + "() {");
            foreach (var preludeLine in directRender.PreludeLines)
                AppendLine("    " + preludeLine);
            foreach (var mapping in directRender.SourceMappings)
            {
                lineMappings.Add(new CompiledLineMapping(
                    GeneratedLine: line,
                    GeneratedColumn: "    return ".Length + mapping.RenderColumn,
                    CompiledLine: mapping.CompiledLine,
                    CompiledColumn: mapping.CompiledColumn));
            }

            AppendLine("    return " + directRender.RenderExpression + ";");
            AppendLine("  }");
        }

        AppendLine();
        if (returnedMembers.Length == 0)
        {
            AppendLine("  return {};");
        }
        else if (returnedMembers.Length == 1)
        {
            AppendLine("  return { " + returnedMembers[0] + " };");
        }
        else
        {
            AppendLine("  return {");
            for (var index = 0; index < returnedMembers.Length; index++)
            {
                var suffix = index + 1 == returnedMembers.Length ? string.Empty : ",";
                AppendLine("    " + returnedMembers[index] + suffix);
            }

            AppendLine("  };");
        }

        AppendLine("}");
        AppendLine();
        AppendLine("export default defineComponent({");
        AppendPropsDeclaration(builder, closure, ref line);
        AppendEmitsDeclaration(builder, closure, ref line);
        AppendLine(usesSlots
            ? "  setup(props, { slots }) {"
            : usesSetupProps
                ? "  setup(props) {"
                : "  setup() {");
        AppendSlotParameterBridges(builder, closure, ref line);
        if (usesUnmounted)
            AppendLine("    let disposed = false;");
        if (usesStateHasChanged)
        {
            AppendLine("    let invalidate = null;");
            AppendLine("    let pendingInvalidations = 0;");
            AppendLine("    const stateHasChanged = () => {");
            if (usesUnmounted)
            {
                AppendLine("      if (disposed) {");
                AppendLine("        throw new Error(\"RazorVue component is disposed; StateHasChanged cannot run after unmount.\");");
                AppendLine("      }");
            }
            AppendLine("      if (invalidate === null) {");
            AppendLine("        pendingInvalidations++;");
            AppendLine("        return;");
            AppendLine("      }");
            AppendLine("      invalidate.tick++;");
            AppendLine("    };");
        }

        if (usesInvokeAsync)
        {
            AppendLine("    const invokeAsync = (workItem) => {");
            if (usesUnmounted)
            {
                AppendLine("      if (disposed) {");
                AppendLine("        return Promise.reject(new Error(\"RazorVue component is disposed; InvokeAsync cannot run after unmount.\"));");
                AppendLine("      }");
            }
            AppendLine("      try {");
            AppendLine("        return Promise.resolve(workItem());");
            AppendLine("      } catch (error) {");
            AppendLine("        return Promise.reject(error);");
            AppendLine("      }");
            AppendLine("    };");
        }

        AppendLine("    const scope = " + setupFactoryName + "(" + BuildSetupFactoryArgumentList(usesFactoryProps, usesFactorySlots, usesStateHasChanged, usesInvokeAsync) + ");");
        if (usesStateHasChanged)
            AppendLine("    invalidate = reactive({ tick: pendingInvalidations });");
        if (hasOnInitialized)
        {
            AppendLine("    scope.onInitialized();");
        }

        if (hasOnInitializedAsync)
        {
            AppendLine("    Promise.resolve(scope.onInitializedAsync()).then(");
            AppendLine("      () => {");
            AppendLine("        stateHasChanged();");
            AppendLine("      },");
            AppendLine("      () => {");
            AppendLine("        stateHasChanged();");
            AppendLine("      }");
            AppendLine("    );");
        }

        if (hasOnParametersSet)
        {
            AppendLine("    scope.onParametersSet();");
            AppendLine("    watch(");
            AppendLine("      () => props,");
            AppendLine("      () => {");
            AppendLine("        scope.onParametersSet();");
            AppendLine("      },");
            AppendLine("      { deep: true }");
            AppendLine("    );");
        }

        if (hasOnParametersSetAsync)
        {
            AppendLine("    let parametersSetAsyncGen = 0;");
            AppendLine("    let parametersSetAsyncTail = Promise.resolve();");
            AppendLine("    const runOnParametersSetAsync = () => {");
            AppendLine("      const gen = ++parametersSetAsyncGen;");
            AppendLine("      parametersSetAsyncTail = parametersSetAsyncTail");
            AppendLine("        .catch(() => {})");
            AppendLine("        .then(() => {");
            AppendLine("          if (gen !== parametersSetAsyncGen) {");
            AppendLine("            return;");
            AppendLine("          }");
            AppendLine("          return Promise.resolve(scope.onParametersSetAsync()).then(");
            AppendLine("            () => {");
            AppendLine("              if (gen === parametersSetAsyncGen) {");
            AppendLine("                stateHasChanged();");
            AppendLine("              }");
            AppendLine("            },");
            AppendLine("            () => {");
            AppendLine("              if (gen === parametersSetAsyncGen) {");
            AppendLine("                stateHasChanged();");
            AppendLine("              }");
            AppendLine("            }");
            AppendLine("          );");
            AppendLine("        });");
            AppendLine("    };");
            AppendLine("    runOnParametersSetAsync();");
            AppendLine("    watch(");
            AppendLine("      () => props,");
            AppendLine("      () => {");
            AppendLine("        runOnParametersSetAsync();");
            AppendLine("      },");
            AppendLine("      { deep: true }");
            AppendLine("    );");
        }

        if (hasOnAfterRender)
        {
            AppendLine("    onMounted(() => {");
            AppendLine("      scope.onAfterRender(true);");
            AppendLine("    });");
            AppendLine("    onUpdated(() => {");
            AppendLine("      scope.onAfterRender(false);");
            AppendLine("    });");
        }

        if (hasOnAfterRenderAsync)
        {
            AppendLine("    onMounted(() => {");
            AppendLine("      void Promise.resolve(scope.onAfterRenderAsync(true));");
            AppendLine("    });");
            AppendLine("    onUpdated(() => {");
            AppendLine("      void Promise.resolve(scope.onAfterRenderAsync(false));");
            AppendLine("    });");
        }

        if (hasDispose || hasDisposeAsync)
        {
            AppendLine("    onUnmounted(() => {");
            if (hasDispose)
                AppendLine("      scope.dispose();");
            if (hasDisposeAsync)
                AppendLine("      void scope.disposeAsync();");
            AppendLine("      disposed = true;");
            AppendLine("    });");
        }

        if (hasShouldRender)
        {
            AppendLine("    let hasRendered = false;");
            AppendLine("    let cachedVNode = null;");
        }

        AppendLine();
        AppendLine("    return () => {");
        if (usesStateHasChanged)
            AppendLine("      invalidate.tick;");
        if (hasShouldRender)
        {
            AppendLine("      if (hasRendered && !scope.shouldRender()) {");
            AppendLine("        return cachedVNode;");
            AppendLine("      }");
            AppendLine("      hasRendered = true;");
        }

        if (directRender is null)
        {
            AppendLine("      const builder = createRenderContext(h);");
            AppendLine("      scope.buildRenderTree(builder);");
        }

        if (hasShouldRender)
        {
            AppendLine(directRender is null
                ? "      cachedVNode = builder.finish();"
                : "      cachedVNode = scope." + directRender.MemberName + "();");
            AppendLine("      return cachedVNode;");
        }
        else
        {
            AppendLine(directRender is null
                ? "      return builder.finish();"
                : "      return scope." + directRender.MemberName + "();");
        }

        AppendLine("    };");
        AppendLine("  }");
        AppendLine("});");

        return new ModuleTextBuildResult(
            Util.NormalizeLineEndingsToLf(builder.ToString()),
            lineMappings.ToImmutable(),
            frontendAssets
                .GroupBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .Select(static group => group.First())
                .OrderBy(static asset => asset.ArtifactPath, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static asset => asset.SourcePath, StringComparer.Ordinal)
                .ToImmutableArray());
    }

    private static bool TryBuildOperationDirectRender(
        RazorSgGeneratedCSharpBinding binding,
        RazorSgBoundComponent component,
        IReadOnlyDictionary<ISymbol, string>? declaredNames,
        ImmutableArray<SetupBodyLine> setupBodyLines,
        out DirectRenderBuildResult result)
    {
        result = default!;
        if (!RazorSgDirectRenderOperationEmitter.TryEmit(
                binding.Compilation,
                component.ComponentSymbol,
                component.BuildRenderTreeMethod,
                component.BuildRenderTreeBody,
                declaredNames,
                out var operationResult,
                out _))
        {
            return false;
        }

        result = new DirectRenderBuildResult(
            operationResult.RenderExpression,
            "$renderDirect",
            operationResult.PreludeLines,
            operationResult.UsesFragment,
            operationResult.UsesStaticVNode,
            operationResult.UsesProps,
            operationResult.UsesSlots,
            ImmutableArray<DirectSourceMapping>.Empty,
            RemoveBuildRenderTreeFunction(setupBodyLines),
            operationResult.ImportLines);
        return true;
    }

    private static IReadOnlyDictionary<ISymbol, string>? BuildDirectRenderDeclaredNames(
        RazorSgBoundComponent component,
        RazorSgComponentMemberClosure closure)
    {
        var directLocalNames = CollectDirectRenderLocalNames(component.BuildRenderTreeBody);
        if (directLocalNames.Count == 0)
            return null;

        var hasCollision = closure.ReachableMethods
            .Where(method => IsDeclaredOnComponentHierarchy(component.ComponentSymbol, method.ContainingType))
            .Any(method =>
                directLocalNames.Contains(GetPreferredModuleDeclaredName(method))) ||
            closure.ComputedProperties
            .Where(property => IsDeclaredOnComponentHierarchy(component.ComponentSymbol, property.ContainingType))
            .Any(property =>
                property.GetMethod is not null &&
                directLocalNames.Contains(GetPreferredModuleDeclaredName(property.GetMethod)));
        if (!hasCollision)
            return null;

        var declaredNames = new Dictionary<ISymbol, string>(SymbolComparer);
        var usedDeclaredNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in closure.OrderedMembers)
        {
            if (member is not INamedTypeSymbol &&
                !IsDeclaredOnComponentHierarchy(component.ComponentSymbol, member.ContainingType))
            {
                continue;
            }

            switch (member)
            {
                case IFieldSymbol field:
                    declaredNames[field.OriginalDefinition] = ChooseModuleDeclaredName(field, usedDeclaredNames, directLocalNames);
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
            if (!string.IsNullOrWhiteSpace(operation.Symbol.Name))
                Names.Add(operation.Symbol.Name);

            base.VisitVariableDeclarator(operation);
        }

        public override void VisitForEachLoop(IForEachLoopOperation operation)
        {
            if (TryGetLoopControlVariable(operation.LoopControlVariable, out var local) &&
                !string.IsNullOrWhiteSpace(local.Name))
            {
                Names.Add(local.Name);
            }

            base.VisitForEachLoop(operation);
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

        var sourceName = GetSourceDeclaredNameCandidate(symbol);
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
        => symbol switch
        {
            IFieldSymbol field => GetPreferredModuleFieldDeclaredName(field),
            IMethodSymbol
            {
                MethodKind: MethodKind.PropertyGet,
                AssociatedSymbol: IPropertySymbol property
            } => Util.GetConfigOrSymbolName(property),
            IMethodSymbol method => Util.GetConfigOrSymbolName(method),
            INamedTypeSymbol type => Util.GetConfigOrSymbolName(type),
            _ => Util.GetConfigOrSymbolName(symbol)
        };

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

    private static string GetPreferredModuleFieldDeclaredName(IFieldSymbol symbol)
    {
        if (symbol.AssociatedSymbol is IPropertySymbol && symbol.IsImplicitlyDeclared)
            return Format.HashName(symbol.AssociatedSymbol!.OriginalDefinition.ToDisplayString(Format.NameFormat));

        return Util.GetConfigOrSymbolName(symbol);
    }

    private static bool ShouldReserveModuleMethodName(IMethodSymbol method)
    {
        if (method.MethodKind == MethodKind.SharedConstructor && method.IsImplicitlyDeclared)
            return false;

        if (method.IsInitOnly)
            return false;

        return method.MethodKind is MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.SharedConstructor;
    }

    private static bool IsRuntimeMemberClass(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class && !type.IsRecord;

    private static bool IsDeclaredOnComponentHierarchy(
        INamedTypeSymbol componentType,
        INamedTypeSymbol? containingType)
    {
        if (containingType is null)
            return false;

        for (var current = componentType; current is not null; current = current.BaseType)
        {
            if (SymbolComparer.Equals(containingType.OriginalDefinition, current.OriginalDefinition))
                return true;
        }

        return false;
    }

    private static bool IsParameterProperty(IPropertySymbol? property)
        => property is not null &&
           property.GetAttributes().Any(static attribute =>
               string.Equals(
                   attribute.AttributeClass?.ToDisplayString(),
                   ParameterAttributeMetadataName,
                   StringComparison.Ordinal));

    private static ImmutableArray<SetupBodyLine> RemoveBuildRenderTreeFunction(
        ImmutableArray<SetupBodyLine> setupBodyLines)
    {
        var functionStart = -1;
        for (var index = 0; index < setupBodyLines.Length; index++)
        {
            if (BuildRenderTreeFunctionPattern.Match(setupBodyLines[index].Text).Success)
            {
                functionStart = index;
                break;
            }
        }

        if (functionStart < 0)
            return setupBodyLines;

        var depth = 0;
        var functionEnd = -1;
        for (var index = functionStart; index < setupBodyLines.Length; index++)
        {
            depth += CountJavaScriptBraceDelta(setupBodyLines[index].Text);
            if (index > functionStart && depth == 0)
            {
                functionEnd = index;
                break;
            }
        }

        if (functionEnd <= functionStart)
            return setupBodyLines;

        return setupBodyLines
            .Where((_, index) => index < functionStart || index > functionEnd)
            .ToImmutableArray();
    }

    private static bool TryCreateVueSfcAsset(
        string importLine,
        string importerRelativePath,
        out RazorSgFrontendAsset asset)
    {
        asset = default!;
        var match = RelativeImportFromPattern.Match(importLine);
        if (!match.Success)
            return false;

        var specifier = match.Groups["path"].Value;
        if (!specifier.EndsWith(".vue.mjs", StringComparison.OrdinalIgnoreCase))
            return false;

        var artifactPath = ResolveImportArtifactPath(specifier.Substring(0, specifier.Length - ".mjs".Length), importerRelativePath);
        asset = new RazorSgFrontendAsset(
            SourcePath: artifactPath,
            ArtifactPath: artifactPath,
            Kind: "vue-sfc",
            ContentHash: string.Empty);
        return true;
    }

    private static void AddImportLocalNames(
        string importLine,
        HashSet<string> localNames)
    {
        if (!TryParseImportDeclaration(importLine, out var declaration))
            return;

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
        string importLine,
        HashSet<string> localNames)
    {
        if (!TryParseImportDeclaration(importLine, out var declaration))
            return false;

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

    private static bool TryGetSingleImportLocalName(
        string importLine,
        out string localName)
    {
        localName = string.Empty;
        if (!TryParseImportDeclaration(importLine, out var declaration) ||
            declaration.Specifiers.Count != 1)
        {
            return false;
        }

        localName = declaration.Specifiers[0] switch
        {
            ImportSpecifier named => named.Local.Name,
            ImportDefaultSpecifier defaultSpecifier => defaultSpecifier.Local.Name,
            ImportNamespaceSpecifier namespaceSpecifier => namespaceSpecifier.Local.Name,
            _ => string.Empty
        };
        return !string.IsNullOrWhiteSpace(localName);
    }

    private static bool TryParseImportDeclaration(
        string importLine,
        out ImportDeclaration declaration)
    {
        declaration = null!;
        try
        {
            if (new Parser().ParseModule(importLine).Body.SingleOrDefault() is ImportDeclaration parsed)
            {
                declaration = parsed;
                return true;
            }
        }
        catch (ParseErrorException)
        {
        }

        return false;
    }

    private static bool TryBuildDirectRender(
        ImmutableArray<SetupBodyLine> setupBodyLines,
        out DirectRenderBuildResult result)
    {
        result = default!;

        var functionStart = -1;
        var functionEnd = -1;
        var builderName = string.Empty;
        for (var index = 0; index < setupBodyLines.Length; index++)
        {
            var match = BuildRenderTreeFunctionPattern.Match(setupBodyLines[index].Text);
            if (!match.Success)
                continue;

            if (functionStart >= 0)
                return false;

            functionStart = index;
            builderName = match.Groups["builder"].Value;
            break;
        }

        if (functionStart < 0)
            return false;

        var depth = 0;
        for (var index = functionStart; index < setupBodyLines.Length; index++)
        {
            depth += CountJavaScriptBraceDelta(setupBodyLines[index].Text);
            if (index > functionStart && depth == 0)
            {
                functionEnd = index;
                break;
            }
        }

        if (functionEnd <= functionStart)
            return false;

        var bodyLines = setupBodyLines
            .Skip(functionStart + 1)
            .Take(functionEnd - functionStart - 1)
            .ToImmutableArray();
        var slotFunctions = BuildDirectSlotFunctionMap(setupBodyLines, functionStart, functionEnd);
        if (!TryBuildDirectRenderExpression(
                bodyLines,
                builderName,
                slotFunctions,
                out var renderExpression,
                out var usesFragment,
                out var usesStaticVNode,
                out var usedSlotFunctionRanges))
        {
            return false;
        }

        var retainedLines = ImmutableArray.CreateBuilder<SetupBodyLine>();
        for (var index = 0; index < setupBodyLines.Length; index++)
        {
            if ((index >= functionStart && index <= functionEnd) ||
                usedSlotFunctionRanges.Any(range => index >= range.Start && index <= range.End))
            {
                continue;
            }

            retainedLines.Add(setupBodyLines[index]);
        }

        result = new DirectRenderBuildResult(
            renderExpression,
            "$renderDirect",
            ImmutableArray<string>.Empty,
            usesFragment,
            usesStaticVNode,
            renderExpression.Contains("props.", StringComparison.Ordinal),
            UsesSlots: false,
            CollectDirectSourceMappings(bodyLines, renderExpression),
            retainedLines.ToImmutable(),
            ImmutableArray<string>.Empty);
        return true;
    }

    private static ImmutableDictionary<string, DirectSlotFunction> BuildDirectSlotFunctionMap(
        ImmutableArray<SetupBodyLine> setupBodyLines,
        int buildRenderTreeStart,
        int buildRenderTreeEnd)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, DirectSlotFunction>(StringComparer.Ordinal);
        for (var index = 0; index < setupBodyLines.Length; index++)
        {
            if (index >= buildRenderTreeStart && index <= buildRenderTreeEnd)
                continue;

            if (!TryMatchDirectSlotFunctionHeader(setupBodyLines[index].Text, out var functionName, out var parameterName) ||
                string.Equals(functionName, "buildRenderTree", StringComparison.Ordinal))
            {
                continue;
            }

            var depth = 0;
            var end = -1;
            for (var functionIndex = index; functionIndex < setupBodyLines.Length; functionIndex++)
            {
                depth += CountJavaScriptBraceDelta(setupBodyLines[functionIndex].Text);
                if (functionIndex > index && depth == 0)
                {
                    end = functionIndex;
                    break;
                }
            }

            if (end <= index)
                continue;

            var functionBody = setupBodyLines
                .Skip(index + 1)
                .Take(end - index - 1)
                .ToImmutableArray();
            if (TryBuildDirectRenderExpression(
                    functionBody,
                    parameterName,
                    ImmutableDictionary<string, DirectSlotFunction>.Empty,
                    out var renderExpression,
                    out var usesFragment,
                    out var usesStaticVNode,
                    out _))
            {
                builder[functionName] = new DirectSlotFunction(
                    renderExpression,
                    ScopedParameterName: null,
                    usesFragment,
                    usesStaticVNode,
                    new DirectFunctionRange(index, end));
                continue;
            }

            if (TryBuildDirectScopedSlotFunction(
                    functionBody,
                    parameterName,
                    out var scopedRenderExpression,
                    out var scopedParameterName,
                    out var scopedUsesFragment,
                    out var scopedUsesStaticVNode))
            {
                builder[functionName] = new DirectSlotFunction(
                    scopedRenderExpression,
                    scopedParameterName,
                    scopedUsesFragment,
                    scopedUsesStaticVNode,
                    new DirectFunctionRange(index, end));
            }
        }

        return builder.ToImmutable();
    }

    private static bool TryBuildDirectScopedSlotFunction(
        ImmutableArray<SetupBodyLine> functionBody,
        string outerParameterName,
        out string renderExpression,
        out string scopedParameterName,
        out bool usesFragment,
        out bool usesStaticVNode)
    {
        renderExpression = string.Empty;
        scopedParameterName = string.Empty;
        usesFragment = false;
        usesStaticVNode = false;
        if (functionBody.Length < 2)
            return false;

        var header = DirectReturnArrowFunctionPattern.Match(functionBody[0].Text);
        if (!header.Success || !string.Equals(functionBody[functionBody.Length - 1].Text.Trim(), "};", StringComparison.Ordinal))
            return false;

        var builderName = header.Groups["parameter"].Value;
        var builderBody = functionBody
            .Skip(1)
            .Take(functionBody.Length - 2)
            .ToImmutableArray();
        if (!TryBuildDirectRenderExpression(
                builderBody,
                builderName,
                ImmutableDictionary<string, DirectSlotFunction>.Empty,
                out renderExpression,
                out usesFragment,
                out usesStaticVNode,
                out _))
        {
            return false;
        }

        scopedParameterName = outerParameterName;
        return true;
    }

    private static bool TryMatchDirectSlotFunctionHeader(
        string line,
        out string functionName,
        out string parameterName)
    {
        var match = DirectFunctionPattern.Match(line);
        if (!match.Success)
            match = DirectArrowFunctionPattern.Match(line);

        if (!match.Success)
        {
            functionName = string.Empty;
            parameterName = string.Empty;
            return false;
        }

        functionName = match.Groups["name"].Value;
        parameterName = match.Groups["parameter"].Value;
        return true;
    }

    private static ImmutableArray<DirectSourceMapping> CollectDirectSourceMappings(
        ImmutableArray<SetupBodyLine> bodyLines,
        string renderExpression)
    {
        var mappings = ImmutableArray.CreateBuilder<DirectSourceMapping>();
        var searchIndex = 0;
        foreach (var line in bodyLines)
        {
            var text = line.Text.Trim();
            var match = BuilderCallPattern.Match(text);
            if (!match.Success)
                continue;

            var method = match.Groups["method"].Value;
            if (!string.Equals(method, "addContent", StringComparison.Ordinal) &&
                !string.Equals(method, "addAttribute", StringComparison.Ordinal) &&
                !string.Equals(method, "addComponentParameter", StringComparison.Ordinal) &&
                !string.Equals(method, "addMarkupContent", StringComparison.Ordinal))
            {
                continue;
            }

            var arguments = SplitJavaScriptArguments(match.Groups["arguments"].Value);
            var expression = method switch
            {
                "addContent" or "addMarkupContent" => arguments.Length == 1 ? arguments[0] : null,
                "addAttribute" or "addComponentParameter" => arguments.Length == 2 ? arguments[1] : null,
                _ => null
            };
            if (string.IsNullOrWhiteSpace(expression))
                continue;

            var expressionIndex = renderExpression.IndexOf(expression!, searchIndex, StringComparison.Ordinal);
            if (expressionIndex < 0)
                continue;

            searchIndex = expressionIndex + expression!.Length;
            var compiledColumn = line.CompiledColumn + Math.Max(0, line.Text.IndexOf(expression!, StringComparison.Ordinal));
            mappings.Add(new DirectSourceMapping(
                expression!,
                expressionIndex,
                line.CompiledLine,
                compiledColumn));
        }

        return mappings.ToImmutable();
    }

    private static bool TryBuildDirectRenderExpression(
        ImmutableArray<SetupBodyLine> bodyLines,
        string builderName,
        ImmutableDictionary<string, DirectSlotFunction> slotFunctions,
        out string renderExpression,
        out bool usesFragment,
        out bool usesStaticVNode,
        out ImmutableArray<DirectFunctionRange> usedSlotFunctionRanges)
    {
        renderExpression = string.Empty;
        usesFragment = false;
        usesStaticVNode = false;
        usedSlotFunctionRanges = ImmutableArray<DirectFunctionRange>.Empty;
        var usedSlots = ImmutableArray.CreateBuilder<DirectFunctionRange>();
        var localSlotFunctions = slotFunctions.ToBuilder();
        bodyLines = CollapseMultilineBuilderCalls(bodyLines, builderName);
        bodyLines = ExtractInlineDirectSlotFunctions(bodyLines, localSlotFunctions);

        var roots = new List<string>();
        var stack = new Stack<DirectRenderFrame>();
        foreach (var line in bodyLines)
        {
            var text = line.Text.Trim();
            if (text.Length == 0)
                continue;
            if (string.Equals(text, "return;", StringComparison.Ordinal))
                continue;

            var match = BuilderCallPattern.Match(text);
            if (!match.Success ||
                !string.Equals(match.Groups["builder"].Value, builderName, StringComparison.Ordinal))
            {
                return false;
            }

            var method = match.Groups["method"].Value;
            var arguments = SplitJavaScriptArguments(match.Groups["arguments"].Value);
            switch (method)
            {
                case "openElement":
                    if (arguments.Length != 1 || !TryReadJavaScriptStringLiteral(arguments[0], out var tagName))
                        return false;

                    if (stack.Count > 0)
                        stack.Peek().ChildrenStarted = true;
                    stack.Push(new DirectElementFrame(arguments[0], tagName));
                    break;

                case "openRegion":
                    if (arguments.Length != 0)
                        return false;

                    if (stack.Count > 0)
                        stack.Peek().ChildrenStarted = true;
                    stack.Push(new DirectRegionFrame());
                    break;

                case "openComponent":
                    if (arguments.Length is < 1 or > 2)
                        return false;

                    var parameterNameMap = ImmutableDictionary<string, string>.Empty;
                    if (arguments.Length == 2 &&
                        !TryReadDirectComponentParameterMap(arguments[1], out parameterNameMap))
                    {
                        return false;
                    }

                    if (stack.Count > 0)
                        stack.Peek().ChildrenStarted = true;
                    stack.Push(new DirectComponentFrame(
                        arguments[0],
                        arguments.Length == 2 ? parameterNameMap : ImmutableDictionary<string, string>.Empty));
                    break;

                case "closeElement":
                    if (arguments.Length != 0 || stack.Count == 0 || stack.Peek() is not DirectElementFrame)
                        return false;

                    AddDirectRenderChild(roots, stack, stack.Pop().ToRenderExpression());
                    break;

                case "closeRegion":
                    if (arguments.Length != 0 || stack.Count == 0 || stack.Peek() is not DirectRegionFrame)
                        return false;

                    var region = stack.Pop();
                    usesFragment = usesFragment || region.UsesFragment;
                    AddDirectRenderChild(roots, stack, region.ToRenderExpression());
                    break;

                case "closeComponent":
                    if (arguments.Length != 0 || stack.Count == 0 || stack.Peek() is not DirectComponentFrame)
                        return false;

                    AddDirectRenderChild(roots, stack, stack.Pop().ToRenderExpression());
                    break;

                case "addAttribute":
                    if (arguments.Length != 2 || stack.Count == 0 || !IsDirectPropFrame(stack.Peek()))
                        return false;

                    var frame = stack.Peek();
                    if (frame.ChildrenStarted || !TryReadJavaScriptStringLiteral(arguments[0], out var attributeName))
                        return false;

                    frame.AddAttribute(new DirectAttribute(
                        frame.NormalizeAttributeName(attributeName),
                        arguments[1]));
                    break;

                case "addMultipleAttributes":
                    if (arguments.Length != 1 || stack.Count == 0 || !IsDirectPropFrame(stack.Peek()))
                        return false;

                    var multipleFrame = stack.Peek();
                    if (multipleFrame.ChildrenStarted ||
                        !TryReadDirectMultipleAttributes(arguments[0], multipleFrame, out var multipleAttributes))
                    {
                        return false;
                    }

                    foreach (var attribute in multipleAttributes)
                        multipleFrame.AddAttribute(attribute);
                    break;

                case "setKey":
                    if (arguments.Length != 1 || stack.Count == 0 || !IsDirectPropFrame(stack.Peek()))
                        return false;

                    var keyFrame = stack.Peek();
                    if (keyFrame.ChildrenStarted)
                        return false;

                    keyFrame.AddAttribute(new DirectAttribute("key", arguments[0]));
                    break;

                case "setAttributeValue":
                    if (arguments.Length != 1 || stack.Count == 0 || !IsDirectPropFrame(stack.Peek()))
                        return false;

                    var valueFrame = stack.Peek();
                    if (valueFrame.ChildrenStarted || !valueFrame.TrySetLastAttributeValue(arguments[0]))
                        return false;

                    break;

                case "setUpdatesAttributeName":
                    if (arguments.Length != 1 || stack.Count == 0 || stack.Peek() is not DirectElementFrame updatesFrame)
                        return false;

                    if (updatesFrame.ChildrenStarted || !TryReadJavaScriptStringLiteral(arguments[0], out var updatesAttributeName))
                        return false;

                    updatesFrame.SetUpdatesAttributeName(updatesAttributeName);
                    break;

                case "addEventPreventDefaultAttribute":
                    if (arguments.Length != 2 || stack.Count == 0 || stack.Peek() is not DirectElementFrame preventFrame)
                        return false;

                    if (preventFrame.ChildrenStarted || !TryReadJavaScriptStringLiteral(arguments[0], out var preventEventName))
                        return false;

                    if (!string.Equals(arguments[1], "false", StringComparison.Ordinal))
                        preventFrame.SetEventModifier(preventEventName, preventDefault: true, stopPropagation: false);
                    break;

                case "addEventStopPropagationAttribute":
                    if (arguments.Length != 2 || stack.Count == 0 || stack.Peek() is not DirectElementFrame stopFrame)
                        return false;

                    if (stopFrame.ChildrenStarted || !TryReadJavaScriptStringLiteral(arguments[0], out var stopEventName))
                        return false;

                    if (!string.Equals(arguments[1], "false", StringComparison.Ordinal))
                        stopFrame.SetEventModifier(stopEventName, preventDefault: false, stopPropagation: true);
                    break;

                case "addNamedEvent":
                    if (arguments.Length != 2 || stack.Count == 0 || stack.Peek() is not DirectElementFrame namedEventFrame)
                        return false;

                    if (namedEventFrame.ChildrenStarted ||
                        !TryReadJavaScriptStringLiteral(arguments[0], out var namedEventType) ||
                        !TryReadJavaScriptStringLiteral(arguments[1], out var assignedEventName) ||
                        string.IsNullOrWhiteSpace(namedEventType) ||
                        string.IsNullOrWhiteSpace(assignedEventName))
                    {
                        return false;
                    }

                    break;

                case "addElementReferenceCapture":
                    if (arguments.Length != 1 || stack.Count == 0 || stack.Peek() is not DirectElementFrame referenceElementFrame)
                        return false;

                    if (referenceElementFrame.ChildrenStarted)
                        return false;

                    referenceElementFrame.AddReferenceCapture(arguments[0]);
                    break;

                case "addComponentReferenceCapture":
                    if (arguments.Length != 1 || stack.Count == 0 || stack.Peek() is not DirectComponentFrame referenceComponentFrame)
                        return false;

                    if (referenceComponentFrame.ChildrenStarted)
                        return false;

                    referenceComponentFrame.AddReferenceCapture(arguments[0]);
                    break;

                case "addComponentParameter":
                    if (arguments.Length != 2 || stack.Count == 0 || stack.Peek() is not DirectComponentFrame componentFrame)
                        return false;

                    if (componentFrame.ChildrenStarted ||
                        !TryReadJavaScriptStringLiteral(arguments[0], out var parameterName) ||
                        string.Equals(parameterName, "ChildContent", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    componentFrame.AddAttribute(new DirectAttribute(
                        componentFrame.NormalizeAttributeName(parameterName),
                        arguments[1]));
                    break;

                case "addComponentSlot":
                    if (arguments.Length != 2 ||
                        stack.Count == 0 ||
                        stack.Peek() is not DirectComponentFrame slotComponentFrame ||
                        slotComponentFrame.ChildrenStarted ||
                        slotComponentFrame.Children.Count != 0 ||
                        !TryReadJavaScriptStringLiteral(arguments[0], out var slotName) ||
                        !localSlotFunctions.TryGetValue(arguments[1], out var slotFunction))
                    {
                        return false;
                    }

                    if (slotFunction.ScopedParameterName is not null)
                        return false;

                    slotComponentFrame.Slots.Add(new DirectSlot(
                        slotComponentFrame.NormalizeSlotName(slotName),
                        ParameterName: null,
                        slotFunction.RenderExpression));
                    usesFragment = usesFragment || slotFunction.UsesFragment;
                    usesStaticVNode = usesStaticVNode || slotFunction.UsesStaticVNode;
                    usedSlots.Add(slotFunction.Range);
                    break;

                case "addComponentScopedSlot":
                    if (arguments.Length != 2 ||
                        stack.Count == 0 ||
                        stack.Peek() is not DirectComponentFrame scopedSlotComponentFrame ||
                        scopedSlotComponentFrame.ChildrenStarted ||
                        scopedSlotComponentFrame.Children.Count != 0 ||
                        !TryReadJavaScriptStringLiteral(arguments[0], out var scopedSlotName) ||
                        !localSlotFunctions.TryGetValue(arguments[1], out var scopedSlotFunction) ||
                        scopedSlotFunction.ScopedParameterName is null)
                    {
                        return false;
                    }

                    scopedSlotComponentFrame.Slots.Add(new DirectSlot(
                        scopedSlotComponentFrame.NormalizeSlotName(scopedSlotName),
                        scopedSlotFunction.ScopedParameterName,
                        scopedSlotFunction.RenderExpression));
                    usesFragment = usesFragment || scopedSlotFunction.UsesFragment;
                    usesStaticVNode = usesStaticVNode || scopedSlotFunction.UsesStaticVNode;
                    usedSlots.Add(scopedSlotFunction.Range);
                    break;

                case "addContent":
                    if (arguments.Length != 1)
                        return false;

                    AddDirectRenderChild(roots, stack, arguments[0]);
                    break;

                case "addMarkupContent":
                    if (arguments.Length != 1)
                        return false;

                    usesStaticVNode = true;
                    AddDirectRenderChild(roots, stack, "createStaticVNode(" + arguments[0] + ", 1)");
                    break;

                default:
                    return false;
            }
        }

        if (stack.Count != 0)
            return false;

        usedSlotFunctionRanges = usedSlots.ToImmutable();

        if (roots.Count == 0)
        {
            renderExpression = "null";
            return true;
        }

        if (roots.Count == 1)
        {
            renderExpression = roots[0];
            return true;
        }

        usesFragment = true;
        renderExpression = "h(Fragment, null, [" + string.Join(", ", roots) + "])";
        return true;
    }

    private static bool IsDirectPropFrame(DirectRenderFrame frame)
    {
        return frame is DirectElementFrame or DirectComponentFrame;
    }

    private static ImmutableArray<SetupBodyLine> CollapseMultilineBuilderCalls(
        ImmutableArray<SetupBodyLine> bodyLines,
        string builderName)
    {
        var collapsed = ImmutableArray.CreateBuilder<SetupBodyLine>();
        for (var index = 0; index < bodyLines.Length; index++)
        {
            var line = bodyLines[index];
            var trimmed = line.Text.TrimStart();
            if (!trimmed.StartsWith(builderName + ".", StringComparison.Ordinal) ||
                BuilderCallPattern.Match(line.Text.Trim()).Success)
            {
                collapsed.Add(line);
                continue;
            }

            var pieces = new List<SetupBodyLine> { line };
            var text = line.Text.Trim();
            var matched = BuilderCallPattern.Match(text).Success;
            for (var next = index + 1; next < bodyLines.Length && !matched; next++)
            {
                pieces.Add(bodyLines[next]);
                text += " " + bodyLines[next].Text.Trim();
                matched = BuilderCallPattern.Match(text).Success;
                index = next;
            }

            if (!matched)
            {
                foreach (var piece in pieces)
                    collapsed.Add(piece);
                continue;
            }

            collapsed.Add(new SetupBodyLine(
                text,
                line.CompiledLine,
                line.CompiledColumn));
        }

        return collapsed.ToImmutable();
    }

    private static ImmutableArray<SetupBodyLine> ExtractInlineDirectSlotFunctions(
        ImmutableArray<SetupBodyLine> bodyLines,
        ImmutableDictionary<string, DirectSlotFunction>.Builder slotFunctions)
    {
        var retained = ImmutableArray.CreateBuilder<SetupBodyLine>();
        for (var index = 0; index < bodyLines.Length; index++)
        {
            if (!TryMatchDirectSlotFunctionHeader(bodyLines[index].Text, out var functionName, out var parameterName))
            {
                retained.Add(bodyLines[index]);
                continue;
            }

            var depth = 0;
            var end = -1;
            for (var functionIndex = index; functionIndex < bodyLines.Length; functionIndex++)
            {
                depth += CountJavaScriptBraceDelta(bodyLines[functionIndex].Text);
                if (functionIndex > index && depth == 0)
                {
                    end = functionIndex;
                    break;
                }
            }

            if (end <= index)
            {
                retained.Add(bodyLines[index]);
                continue;
            }

            var functionBody = bodyLines
                .Skip(index + 1)
                .Take(end - index - 1)
                .ToImmutableArray();
            if (TryBuildDirectRenderExpression(
                    functionBody,
                    parameterName,
                    ImmutableDictionary<string, DirectSlotFunction>.Empty,
                    out var renderExpression,
                    out var usesFragment,
                    out var usesStaticVNode,
                    out _))
            {
                slotFunctions[functionName] = new DirectSlotFunction(
                    renderExpression,
                    ScopedParameterName: null,
                    usesFragment,
                    usesStaticVNode,
                    new DirectFunctionRange(-1, -1));
                index = end;
                continue;
            }

            if (TryBuildDirectScopedSlotFunction(
                    functionBody,
                    parameterName,
                    out var scopedRenderExpression,
                    out var scopedParameterName,
                    out var scopedUsesFragment,
                    out var scopedUsesStaticVNode))
            {
                slotFunctions[functionName] = new DirectSlotFunction(
                    scopedRenderExpression,
                    scopedParameterName,
                    scopedUsesFragment,
                    scopedUsesStaticVNode,
                    new DirectFunctionRange(-1, -1));
                index = end;
                continue;
            }

            for (var retainedIndex = index; retainedIndex <= end; retainedIndex++)
                retained.Add(bodyLines[retainedIndex]);
            index = end;
        }

        return retained.ToImmutable();
    }

    private static void AddDirectRenderChild(
        List<string> roots,
        Stack<DirectRenderFrame> stack,
        string expression)
    {
        if (string.Equals(expression, "null", StringComparison.Ordinal))
            return;

        if (stack.Count == 0)
        {
            roots.Add(expression);
            return;
        }

        var frame = stack.Peek();
        frame.ChildrenStarted = true;
        frame.Children.Add(expression);
    }

    private static int CountJavaScriptBraceDelta(string text)
    {
        var delta = 0;
        var inString = false;
        var quote = '\0';
        var escaped = false;
        foreach (var item in text)
        {
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (item == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (item == quote)
                    inString = false;
                continue;
            }

            if (item == '"' || item == '\'' || item == '`')
            {
                inString = true;
                quote = item;
                continue;
            }

            if (item == '{')
                delta++;
            else if (item == '}')
                delta--;
        }

        return delta;
    }

    private static ImmutableArray<string> SplitJavaScriptArguments(string argumentsText)
    {
        var arguments = ImmutableArray.CreateBuilder<string>();
        var start = 0;
        var depth = 0;
        var inString = false;
        var quote = '\0';
        var escaped = false;
        for (var index = 0; index < argumentsText.Length; index++)
        {
            var item = argumentsText[index];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (item == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (item == quote)
                    inString = false;
                continue;
            }

            if (item == '"' || item == '\'' || item == '`')
            {
                inString = true;
                quote = item;
                continue;
            }

            if (item == '(' || item == '[' || item == '{')
            {
                depth++;
                continue;
            }

            if (item == ')' || item == ']' || item == '}')
            {
                depth--;
                continue;
            }

            if (item == ',' && depth == 0)
            {
                arguments.Add(argumentsText.Substring(start, index - start).Trim());
                start = index + 1;
            }
        }

        var tail = argumentsText.Substring(start).Trim();
        if (tail.Length > 0)
            arguments.Add(tail);

        return arguments.ToImmutable();
    }

    private static bool TryReadJavaScriptStringLiteral(string expression, out string value)
    {
        value = string.Empty;
        expression = expression.Trim();
        if (expression.Length < 2 || expression[0] != '"' || expression[expression.Length - 1] != '"')
            return false;

        var builder = new StringBuilder();
        for (var index = 1; index < expression.Length - 1; index++)
        {
            var item = expression[index];
            if (item != '\\')
            {
                builder.Append(item);
                continue;
            }

            if (index + 1 >= expression.Length - 1)
                return false;

            var escaped = expression[++index];
            builder.Append(escaped switch
            {
                '"' => '"',
                '\\' => '\\',
                '/' => '/',
                'b' => '\b',
                'f' => '\f',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                _ => escaped
            });
        }

        value = builder.ToString();
        return true;
    }

    private static string NormalizeDirectElementAttributeName(string name)
    {
        var normalized = name;
        if (normalized.StartsWith("@", StringComparison.Ordinal))
            normalized = normalized.Substring(1);

        return normalized.Length >= 3 &&
               normalized.StartsWith("on", StringComparison.Ordinal) &&
               char.IsLower(normalized[2])
            ? "on" + char.ToUpperInvariant(normalized[2]) + normalized.Substring(3)
            : normalized;
    }

    private static bool IsDirectEventAttributeName(string name)
    {
        return name.Length > 2 &&
               name.StartsWith("on", StringComparison.Ordinal) &&
               char.IsUpper(name[2]);
    }

    private static string BuildDirectDomBindHandler(string handlerExpression, string attributeName)
    {
        var escapedAttributeName = "\"" + EscapeJavaScriptString(attributeName) + "\"";
        return "(eventOrValue, ...args) => { const value = eventOrValue !== null && eventOrValue !== undefined && typeof eventOrValue === \"object\" && eventOrValue.target !== null && eventOrValue.target !== undefined && " +
               escapedAttributeName +
               " in eventOrValue.target ? eventOrValue.target[" +
               escapedAttributeName +
               "] : eventOrValue; return (" +
               handlerExpression +
               ")(value, ...args); }";
    }

    private static string BuildDirectEventModifierHandler(string handlerExpression, DirectEventModifier modifier)
    {
        var statements = new List<string>();
        if (modifier.PreventDefault)
            statements.Add("event?.preventDefault?.();");
        if (modifier.StopPropagation)
            statements.Add("event?.stopPropagation?.();");
        statements.Add("return (" + handlerExpression + ")(event, ...args);");
        return "(event, ...args) => { " + string.Join(" ", statements) + " }";
    }

    private static string NormalizeDirectComponentParameterName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static bool TryReadDirectComponentParameterMap(
        string expression,
        out ImmutableDictionary<string, string> parameterNameMap)
    {
        parameterNameMap = ImmutableDictionary<string, string>.Empty;
        expression = expression.Trim();
        if (expression.Length < 2 || expression[0] != '{' || expression[expression.Length - 1] != '}')
            return false;

        var entries = SplitJavaScriptArguments(expression.Substring(1, expression.Length - 2));
        var builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            var parts = SplitObjectProperty(entry);
            if (parts.Length != 2 ||
                !TryReadJavaScriptStringLiteral(parts[0], out var key) ||
                !TryReadJavaScriptStringLiteral(parts[1], out var value))
            {
                return false;
            }

            builder[key] = value;
        }

        parameterNameMap = builder.ToImmutable();
        return true;
    }

    private static bool TryReadDirectMultipleAttributes(
        string expression,
        DirectRenderFrame frame,
        out ImmutableArray<DirectAttribute> attributes)
    {
        attributes = ImmutableArray<DirectAttribute>.Empty;
        expression = expression.Trim();
        if (string.Equals(expression, "null", StringComparison.Ordinal) ||
            string.Equals(expression, "undefined", StringComparison.Ordinal))
        {
            return true;
        }

        var builder = ImmutableArray.CreateBuilder<DirectAttribute>();
        if (TryReadDirectObjectAttributes(expression, frame, builder) ||
            TryReadDirectMapAttributes(expression, frame, builder) ||
            TryReadDirectEntryArrayAttributes(expression, frame, builder))
        {
            attributes = builder.ToImmutable();
            return true;
        }

        return false;
    }

    private static bool TryReadDirectObjectAttributes(
        string expression,
        DirectRenderFrame frame,
        ImmutableArray<DirectAttribute>.Builder attributes)
    {
        expression = expression.Trim();
        if (expression.Length < 2 || expression[0] != '{' || expression[expression.Length - 1] != '}')
            return false;

        var entries = SplitJavaScriptArguments(expression.Substring(1, expression.Length - 2));
        foreach (var entry in entries)
        {
            var parts = SplitObjectProperty(entry);
            if (parts.Length != 2 || !TryReadDirectAttributeName(parts[0], out var name))
                return false;

            attributes.Add(new DirectAttribute(frame.NormalizeAttributeName(name), parts[1]));
        }

        return true;
    }

    private static bool TryReadDirectMapAttributes(
        string expression,
        DirectRenderFrame frame,
        ImmutableArray<DirectAttribute>.Builder attributes)
    {
        const string prefix = "new Map(";
        expression = expression.Trim();
        if (!expression.StartsWith(prefix, StringComparison.Ordinal) || expression[expression.Length - 1] != ')')
            return false;

        var arguments = SplitJavaScriptArguments(expression.Substring(prefix.Length, expression.Length - prefix.Length - 1));
        if (arguments.Length == 0)
            return true;

        return arguments.Length == 1 &&
               TryReadDirectEntryArrayAttributes(arguments[0], frame, attributes);
    }

    private static bool TryReadDirectEntryArrayAttributes(
        string expression,
        DirectRenderFrame frame,
        ImmutableArray<DirectAttribute>.Builder attributes)
    {
        expression = expression.Trim();
        if (expression.Length < 2 || expression[0] != '[' || expression[expression.Length - 1] != ']')
            return false;

        var entries = SplitJavaScriptArguments(expression.Substring(1, expression.Length - 2));
        foreach (var entry in entries)
        {
            if (!TryReadDirectAttributeEntry(entry, frame, out var attribute))
                return false;

            attributes.Add(attribute);
        }

        return true;
    }

    private static bool TryReadDirectAttributeEntry(
        string expression,
        DirectRenderFrame frame,
        out DirectAttribute attribute)
    {
        attribute = default!;
        expression = expression.Trim();
        if (expression.Length < 2 || expression[0] != '[' || expression[expression.Length - 1] != ']')
            return false;

        var parts = SplitJavaScriptArguments(expression.Substring(1, expression.Length - 2));
        if (parts.Length != 2 || !TryReadJavaScriptStringLiteral(parts[0], out var name))
            return false;

        attribute = new DirectAttribute(frame.NormalizeAttributeName(name), parts[1]);
        return true;
    }

    private static bool TryReadDirectAttributeName(string expression, out string name)
    {
        expression = expression.Trim();
        if (TryReadJavaScriptStringLiteral(expression, out name))
            return true;

        if (!IsJavaScriptIdentifier(expression))
            return false;

        name = expression;
        return true;
    }

    private static ImmutableArray<string> SplitObjectProperty(string propertyText)
    {
        var depth = 0;
        var inString = false;
        var quote = '\0';
        var escaped = false;
        for (var index = 0; index < propertyText.Length; index++)
        {
            var item = propertyText[index];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (item == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (item == quote)
                    inString = false;
                continue;
            }

            if (item == '"' || item == '\'' || item == '`')
            {
                inString = true;
                quote = item;
                continue;
            }

            if (item == '(' || item == '[' || item == '{')
            {
                depth++;
                continue;
            }

            if (item == ')' || item == ']' || item == '}')
            {
                depth--;
                continue;
            }

            if (item == ':' && depth == 0)
            {
                return ImmutableArray.Create(
                    propertyText.Substring(0, index).Trim(),
                    propertyText.Substring(index + 1).Trim());
            }
        }

        return ImmutableArray<string>.Empty;
    }

    private static string FormatJavaScriptPropertyName(string name)
    {
        if (IsJavaScriptIdentifier(name))
            return name;

        return "\"" + EscapeJavaScriptString(name) + "\"";
    }

    private static bool IsJavaScriptIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!IsJavaScriptIdentifierStart(value[0]))
            return false;

        for (var index = 1; index < value.Length; index++)
        {
            if (!IsJavaScriptIdentifierPart(value[index]))
                return false;
        }

        return true;
    }

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

    private static void AppendStateDeclaration(
        StringBuilder builder,
        ImmutableArray<StateSlot> stateSlots,
        ImmutableArray<CompiledLineMapping>.Builder lineMappings,
        ref int line)
    {
        if (stateSlots.Length == 0)
        {
            builder.AppendLine("  const state = reactive({});");
            line++;
            return;
        }

        builder.AppendLine("  const state = reactive({");
        line++;
        for (var index = 0; index < stateSlots.Length; index++)
        {
            var slot = stateSlots[index];
            var suffix = index + 1 == stateSlots.Length ? string.Empty : ",";
            var initializer = slot.Initializer ?? CurrentComponentStateDefaultInitializer.Format(slot.Type);
            var lineText = "    " + slot.RuntimeName + ": " + initializer + suffix;
            if (slot.InitializerCompiledLine is int compiledLine)
            {
                lineMappings.Add(new CompiledLineMapping(
                    GeneratedLine: line,
                    GeneratedColumn: 4 + slot.RuntimeName.Length + ": ".Length,
                    CompiledLine: compiledLine,
                    CompiledColumn: Math.Max(slot.InitializerCompiledColumn ?? 0, 0)));
            }

            builder.AppendLine(lineText);
            line++;
        }

        builder.AppendLine("  });");
        line++;
    }

    private static string BuildVueImportLine(
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

        return "import { " + string.Join(", ", imports) + " } from \"vue\";";
    }

    private static string BuildSetupFactoryParameterList(
        bool usesProps,
        bool usesSlots,
        bool usesStateHasChanged,
        bool usesInvokeAsync)
    {
        var parameters = new List<string>();
        if (usesProps)
            parameters.Add("props");
        if (usesSlots)
            parameters.Add("slots");
        if (usesStateHasChanged)
            parameters.Add("stateHasChanged");
        if (usesInvokeAsync)
            parameters.Add("invokeAsync");

        return string.Join(", ", parameters);
    }

    private static string BuildSetupFactoryArgumentList(
        bool usesProps,
        bool usesSlots,
        bool usesStateHasChanged,
        bool usesInvokeAsync)
    {
        var arguments = new List<string>();
        if (usesProps)
            arguments.Add("props");
        if (usesSlots)
            arguments.Add("slots");
        if (usesStateHasChanged)
            arguments.Add("stateHasChanged");
        if (usesInvokeAsync)
            arguments.Add("invokeAsync");

        return string.Join(", ", arguments);
    }

    private static void AppendIndentedLines(
        StringBuilder builder,
        ImmutableArray<SetupBodyLine> lines,
        ImmutableArray<CompiledLineMapping>.Builder lineMappings,
        ref int line)
    {
        foreach (var item in lines)
        {
            if (item.Text.Length == 0)
            {
                builder.AppendLine();
                line++;
            }
            else
            {
                var leadingWhitespace = CountLeadingWhitespace(item.Text);
                lineMappings.Add(new CompiledLineMapping(
                    GeneratedLine: line,
                    GeneratedColumn: 2 + leadingWhitespace,
                    CompiledLine: item.CompiledLine,
                    CompiledColumn: Math.Max(item.CompiledColumn, 0)));
                builder.Append("  ").AppendLine(item.Text);
                line++;
            }
        }
    }

    private static CompilerScriptParts SplitCompiledScript(
        string compiledScript,
        RazorSgComponentMemberClosure closure)
    {
        var imports = ImmutableArray.CreateBuilder<CompiledSourceLine>();
        var setupBodyLines = ImmutableArray.CreateBuilder<SetupBodyLine>();
        var stateSlots = BuildStateSlots(closure);
        var stateSlotByDeclarationName = new Dictionary<string, int>(StringComparer.Ordinal);
        var discardedDeclarationNames = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < stateSlots.Count; index++)
        {
            var declarationName = stateSlots[index].DeclarationName;
            if (!string.IsNullOrWhiteSpace(declarationName))
                stateSlotByDeclarationName[declarationName!] = index;
        }

        foreach (var declarationName in GetDiscardedPropertyBackingFieldNames(closure))
        {
            discardedDeclarationNames.Add(declarationName);
        }

        var compiledLines = compiledScript.Split('\n');
        for (var lineIndex = 0; lineIndex < compiledLines.Length; lineIndex++)
        {
            var rawLine = compiledLines[lineIndex];
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.TrimStart();
            if (trimmed.Length == 0)
            {
                setupBodyLines.Add(new SetupBodyLine(string.Empty, lineIndex, 0));
                continue;
            }

            if (trimmed.StartsWith("import ", StringComparison.Ordinal))
            {
                imports.Add(new CompiledSourceLine(line, lineIndex));
                continue;
            }

            if (TryReadVariableDeclaration(line, out var declaredName, out var initializer, out var initializerColumn))
            {
                if (stateSlotByDeclarationName.TryGetValue(declaredName, out var stateSlotIndex))
                {
                    var stateInitializer = string.IsNullOrWhiteSpace(initializer)
                        ? stateSlots[stateSlotIndex].Initializer
                        : initializer!;
                    stateSlots[stateSlotIndex] = stateSlots[stateSlotIndex] with
                    {
                        Initializer = stateInitializer,
                        InitializerCompiledLine = string.IsNullOrWhiteSpace(stateInitializer) ? null : lineIndex,
                        InitializerCompiledColumn = string.IsNullOrWhiteSpace(stateInitializer) ? null : initializerColumn
                    };
                    continue;
                }

                if (discardedDeclarationNames.Contains(declaredName))
                    continue;
            }
            else if (TryReadVariableDeclarationBlock(
                         compiledLines,
                         lineIndex,
                         out declaredName,
                         out initializer,
                         out initializerColumn,
                         out var endLineIndex))
            {
                if (stateSlotByDeclarationName.TryGetValue(declaredName, out var stateSlotIndex))
                {
                    stateSlots[stateSlotIndex] = stateSlots[stateSlotIndex] with
                    {
                        Initializer = initializer,
                        InitializerCompiledLine = lineIndex,
                        InitializerCompiledColumn = initializerColumn
                    };
                    lineIndex = endLineIndex;
                    continue;
                }

                if (discardedDeclarationNames.Contains(declaredName))
                {
                    lineIndex = endLineIndex;
                    continue;
                }
            }

            if (trimmed.StartsWith("export {", StringComparison.Ordinal))
                continue;

            var stripped = StripExportModifier(line, out var compiledColumn);
            setupBodyLines.Add(new SetupBodyLine(stripped, lineIndex, compiledColumn));
        }

        return new CompilerScriptParts(
            imports.ToImmutable(),
            setupBodyLines.ToImmutable(),
            stateSlots.ToImmutableArray());
    }

    private static List<StateSlot> BuildStateSlots(RazorSgComponentMemberClosure closure)
    {
        var slots = new List<StateSlot>();
        foreach (var field in closure.StateFields)
        {
            var name = Util.GetConfigOrSymbolName(field);
            slots.Add(new StateSlot(name, name, field.Type, null));
        }

        foreach (var property in closure.StateProperties)
        {
            slots.Add(new StateSlot(
                Util.GetConfigOrSymbolName(property),
                GetPropertyBackingFieldName(closure.ComponentSymbol, property),
                property.Type,
                null));
        }

        return slots;
    }

    private static IEnumerable<string> GetDiscardedPropertyBackingFieldNames(RazorSgComponentMemberClosure closure)
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

    private static bool TryReadVariableDeclaration(
        string line,
        out string name,
        out string? initializer,
        out int initializerColumn)
    {
        var match = VariableDeclarationPattern.Match(line);
        if (!match.Success)
        {
            name = string.Empty;
            initializer = null;
            initializerColumn = 0;
            return false;
        }

        name = match.Groups[1].Value;
        initializer = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;
        initializerColumn = match.Groups[2].Success ? match.Groups[2].Index : 0;
        return true;
    }

    private static bool TryReadVariableDeclarationBlock(
        string[] lines,
        int startLineIndex,
        out string name,
        out string? initializer,
        out int initializerColumn,
        out int endLineIndex)
    {
        name = string.Empty;
        initializer = null;
        initializerColumn = 0;
        endLineIndex = startLineIndex;
        if (startLineIndex < 0 || startLineIndex >= lines.Length)
            return false;

        var firstLine = lines[startLineIndex].TrimEnd('\r');
        var match = VariableDeclarationStartPattern.Match(firstLine);
        if (!match.Success || !match.Groups[2].Success)
            return false;

        name = match.Groups[1].Value;
        initializerColumn = match.Groups[2].Index;
        var builder = new StringBuilder();
        var firstInitializer = match.Groups[2].Value.TrimEnd();
        builder.Append(firstInitializer);
        if (TryFindVariableDeclarationBlockEnd(firstInitializer, out var firstLineEnd))
        {
            initializer = firstInitializer.Substring(0, firstLineEnd).Trim();
            return !string.IsNullOrWhiteSpace(initializer);
        }

        for (var index = startLineIndex + 1; index < lines.Length; index++)
        {
            var line = lines[index].TrimEnd('\r');
            builder.Append('\n').Append(line);
            if (!TryFindVariableDeclarationBlockEnd(builder.ToString(), out var end))
                continue;

            endLineIndex = index;
            initializer = builder.ToString().Substring(0, end).Trim();
            return !string.IsNullOrWhiteSpace(initializer);
        }

        return false;
    }

    private static bool TryFindVariableDeclarationBlockEnd(string text, out int endIndex)
    {
        endIndex = 0;
        var stack = new Stack<char>();
        char? quote = null;
        var escaped = false;
        for (var index = 0; index < text.Length; index++)
        {
            var ch = text[index];
            if (quote is char currentQuote)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == currentQuote)
                    quote = null;

                continue;
            }

            if (ch == '"' || ch == '\'' || ch == '`')
            {
                quote = ch;
                continue;
            }

            switch (ch)
            {
                case '(':
                    stack.Push(')');
                    break;
                case '[':
                    stack.Push(']');
                    break;
                case '{':
                    stack.Push('}');
                    break;
                case ')' or ']' or '}':
                    if (stack.Count == 0 || stack.Pop() != ch)
                        return false;
                    break;
                case ';' when stack.Count == 0:
                    endIndex = index;
                    return true;
            }
        }

        return false;
    }

    private static string StripExportModifier(string line, out int compiledColumn)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith("export ", StringComparison.Ordinal))
        {
            compiledColumn = CountLeadingWhitespace(line);
            return line;
        }

        var leadingWhitespaceLength = line.Length - trimmed.Length;
        compiledColumn = leadingWhitespaceLength + "export ".Length;
        return line.Substring(0, leadingWhitespaceLength) + trimmed.Substring("export ".Length);
    }

    private static void AppendPropsDeclaration(
        StringBuilder builder,
        RazorSgComponentMemberClosure closure,
        ref int line)
    {
        // RenderFragment parameters are slot contracts, not Vue props. Keep them out of props: [...].
        var propNames = GetComponentParameterProperties(closure.ComponentSymbol)
            .Where(static property => !IsAnyRenderFragmentType(property.Type))
            .Select(property => GetVueParameterPropName(closure.ComponentSymbol, property))
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
        if (propNames.Length == 0)
            return;

        builder.AppendLine("  props: [");
        line++;
        for (var index = 0; index < propNames.Length; index++)
        {
            var suffix = index + 1 == propNames.Length ? string.Empty : ",";
            builder.AppendLine("    \"" + EscapeJavaScriptString(propNames[index]) + "\"" + suffix);
            line++;
        }

        builder.AppendLine("  ],");
        line++;
    }

    private static void AppendEmitsDeclaration(
        StringBuilder builder,
        RazorSgComponentMemberClosure closure,
        ref int line)
    {
        var emitNames = GetComponentParameterProperties(closure.ComponentSymbol)
            .Where(static property => IsEventCallbackType(property.Type))
            .Select(property => GetVueParameterEmitName(closure.ComponentSymbol, property))
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();
        if (emitNames.Length == 0)
            return;

        builder.AppendLine("  emits: [");
        line++;
        for (var index = 0; index < emitNames.Length; index++)
        {
            var suffix = index + 1 == emitNames.Length ? string.Empty : ",";
            builder.AppendLine("    \"" + EscapeJavaScriptString(emitNames[index]) + "\"" + suffix);
            line++;
        }

        builder.AppendLine("  ],");
        line++;
    }

    private static void AppendSlotParameterBridges(
        StringBuilder builder,
        RazorSgComponentMemberClosure closure,
        ref int line)
    {
        var slotParameters = closure.ParameterProperties
            .Where(static property => IsAnyRenderFragmentType(property.Type))
            .Select(static property => new SlotParameterBridge(
                property.Name,
                Util.GetConfigOrSymbolName(property),
                GetVueSlotName(property),
                IsGenericRenderFragmentType(property.Type)))
            .Where(static item => !string.IsNullOrWhiteSpace(item.RuntimePropName) &&
                                  !string.IsNullOrWhiteSpace(item.VueSlotName))
            .Distinct()
            .OrderBy(static item => item.VueSlotName, StringComparer.Ordinal)
            .ToArray();
        foreach (var item in slotParameters)
        {
            builder.AppendLine("    if (typeof slots." + item.VueSlotName + " === \"function\") {");
            line++;
            builder.AppendLine(item.IsScoped
                ? "      props." + item.RuntimePropName + " = (value) => (builder) => {"
                : "      props." + item.RuntimePropName + " = (builder) => {");
            line++;
            builder.AppendLine(item.IsScoped
                ? "        const content = slots." + item.VueSlotName + "(value);"
                : "        const content = slots." + item.VueSlotName + "();");
            line++;
            builder.AppendLine("        if (content !== null && content !== undefined) {");
            line++;
            builder.AppendLine("          builder.addContent(content);");
            line++;
            builder.AppendLine("        }");
            line++;
            builder.AppendLine("      };");
            line++;
            builder.AppendLine("    }");
            line++;
        }
    }

    private static bool HasSlotParameterBridges(RazorSgComponentMemberClosure closure)
        => closure.ParameterProperties.Any(static property =>
            IsAnyRenderFragmentType(property.Type) &&
            !string.IsNullOrWhiteSpace(property.Name) &&
            !string.IsNullOrWhiteSpace(Util.GetConfigOrSymbolName(property)) &&
            !string.IsNullOrWhiteSpace(GetVueSlotName(property)));

    private static string GetVueSlotName(IPropertySymbol property)
        => TryGetClassSlotDescriptorName(property, out var descriptorName)
            ? descriptorName
            : IsChildContentParameter(property)
            ? "default"
            : Util.GetConfigOrSymbolName(property);

    private static bool TryGetClassSlotDescriptorName(
        IPropertySymbol property,
        out string name)
    {
        if (property.ContainingType is not INamedTypeSymbol componentSymbol)
        {
            name = string.Empty;
            return false;
        }

        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    VueSlotAttributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not string publicName ||
                !string.Equals(publicName, property.Name, StringComparison.Ordinal) ||
                GetNamedBoolean(attribute, "PatternOnly") == true)
            {
                continue;
            }

            if (GetNamedBoolean(attribute, "IsDefault") == true)
            {
                name = "default";
                return true;
            }

            var descriptorName = GetNamedString(attribute, "Name");
            if (!string.IsNullOrWhiteSpace(descriptorName))
            {
                name = descriptorName!;
                return true;
            }
        }

        name = string.Empty;
        return false;
    }

    private static IEnumerable<IPropertySymbol> GetComponentParameterProperties(INamedTypeSymbol componentSymbol)
    {
        for (INamedTypeSymbol? current = componentSymbol; current is not null; current = current.BaseType)
        {
            foreach (var property in current
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(static property => property.GetAttributes().Any(static attribute =>
                    string.Equals(
                        attribute.AttributeClass?.ToDisplayString(),
                        ParameterAttributeMetadataName,
                        StringComparison.Ordinal))))
            {
                yield return property;
            }
        }
    }

    private static string GetVueParameterPropName(INamedTypeSymbol componentSymbol, IPropertySymbol property)
        => TryGetClassDescriptorName(
            componentSymbol,
            VuePropAttributeMetadataName,
            property.Name,
            out var descriptorName)
            ? descriptorName
            : Util.GetConfigOrSymbolName(property);

    private static string GetVueParameterEmitName(INamedTypeSymbol componentSymbol, IPropertySymbol property)
        => TryGetClassDescriptorName(
            componentSymbol,
            VueLibraryEmitAttributeMetadataName,
            property.Name,
            out var descriptorName)
            ? descriptorName
            : GetVueEmitName(Util.GetConfigOrSymbolName(property));

    private static bool TryGetClassDescriptorName(
        INamedTypeSymbol componentSymbol,
        string attributeMetadataName,
        string publicName,
        out string name)
    {
        foreach (var attribute in componentSymbol.GetAttributes())
        {
            if (!string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    attributeMetadataName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not string attributePublicName ||
                !string.Equals(attributePublicName, publicName, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var argument in attribute.NamedArguments)
            {
                if (string.Equals(argument.Key, "Name", StringComparison.Ordinal) &&
                    argument.Value.Value is string descriptorName &&
                    !string.IsNullOrWhiteSpace(descriptorName))
                {
                    name = descriptorName.Trim();
                    return true;
                }
            }
        }

        name = string.Empty;
        return false;
    }

    private static string? GetNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is string value &&
                !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static bool? GetNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal) &&
                argument.Value.Value is bool value)
            {
                return value;
            }
        }

        return null;
    }

    private static string GetVueEmitName(string runtimePropName)
    {
        if (string.IsNullOrWhiteSpace(runtimePropName))
            return string.Empty;

        if (runtimePropName.Length > 2 &&
            runtimePropName.StartsWith("on", StringComparison.Ordinal) &&
            char.IsUpper(runtimePropName[2]))
        {
            var eventName = runtimePropName.Substring(2);
            return char.ToLowerInvariant(eventName[0]) + eventName.Substring(1);
        }

        return runtimePropName;
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

    private static string RebaseRootRelativeImportLine(string importLine, string importerRelativePath)
    {
        var match = ImportFromPattern.Match(importLine);
        if (!match.Success)
            return importLine;

        var rebasedPath = RebaseRootRelativeModuleSpecifier(
            match.Groups["path"].Value,
            importerRelativePath);
        return match.Groups["prefix"].Value +
               match.Groups["quote"].Value +
               rebasedPath +
               match.Groups["quote"].Value +
               match.Groups["suffix"].Value;
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

    private sealed record SlotParameterBridge(
        string SourceName,
        string RuntimePropName,
        string VueSlotName,
        bool IsScoped);

    private static bool IsChildContentParameter(IPropertySymbol property)
        => string.Equals(property.Name, "ChildContent", StringComparison.Ordinal) &&
           IsRenderFragmentType(property.Type);

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

    private static string EscapeJavaScriptString(string value)
        => (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

    private static ImmutableArray<string> GetReturnedMembers(RazorSgComponentMemberClosure closure)
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
            var name = Util.GetConfigOrSymbolName(method);
            if (!names.Contains(name, StringComparer.Ordinal))
                names.Add(name);
        }
    }

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
        RazorSgBoundComponent component,
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
        RazorSgBoundComponent component,
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
            foreach (var path in component.Document.SourceMappings.Select(static mapping => mapping.GeneratedSpan.FilePath))
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
        RazorSgGeneratedDocument document,
        SourceMapDocument compilerMap)
    {
        var sources = new List<SourceMapSource>();
        var sourceIndexByPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var segments = new List<SourceMapSegment>();

        var orderedMappings = document.SourceMappings
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
        RazorSgGeneratedDocument document,
        IReadOnlyList<RazorSgSourceMapping> orderedMappings,
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

        RazorSgSourceMapping candidate = default;
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
        RazorSgBoundComponent component,
        string relativePath,
        string moduleText)
    {
        var sourceSpan = component.Document.SourceMappings
            .OrderBy(static mapping => mapping.GeneratedSpan.LineIndex)
            .ThenBy(static mapping => mapping.GeneratedSpan.CharacterIndex)
            .Select(static mapping => mapping.OriginalSpan)
            .FirstOrDefault();
        var sourcePath = NormalizeSourcePath(sourceSpan.FilePath ?? component.Document.SourcePath);
        var sourceLine = Math.Max(0, sourceSpan.LineIndex);
        var sourceColumn = Math.Max(0, sourceSpan.CharacterIndex);
        var generatedLine = FindGeneratedLine(moduleText, "scope.buildRenderTree(builder);");
        var document = new SourceMapDocument(
            relativePath,
            [new SourceMapSource(sourcePath, null)],
            [new SourceMapSegment(generatedLine, 0, 0, sourceLine, sourceColumn)]);

        return Util.NormalizeLineEndingsToLf(new SourceMapWriter().Write(document));
    }

    private static string? TryGetCompilationSourceRoot(Compilation compilation, RazorSgGeneratedDocument document)
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

    private static int CountLeadingWhitespace(string value)
    {
        var index = 0;
        while (index < value.Length && char.IsWhiteSpace(value[index]))
            index++;

        return index;
    }

    private sealed record ModuleTextBuildResult(
        string ModuleText,
        ImmutableArray<CompiledLineMapping> CompiledLineMappings,
        ImmutableArray<RazorSgFrontendAsset> FrontendAssets);

    private readonly record struct MappedSourcePosition(
        string SourcePath,
        int SourceLine,
        int SourceColumn);

    private readonly record struct CompiledLineMapping(
        int GeneratedLine,
        int GeneratedColumn,
        int CompiledLine,
        int CompiledColumn);

    private sealed record CompiledSourceLine(string Text, int CompiledLine);

    private sealed record SetupBodyLine(string Text, int CompiledLine, int CompiledColumn);

    private sealed record CompilerScriptParts(
        ImmutableArray<CompiledSourceLine> ImportLines,
        ImmutableArray<SetupBodyLine> SetupBodyLines,
        ImmutableArray<StateSlot> StateSlots);

    private sealed record DirectRenderBuildResult(
        string RenderExpression,
        string MemberName,
        ImmutableArray<string> PreludeLines,
        bool UsesFragment,
        bool UsesStaticVNode,
        bool UsesProps,
        bool UsesSlots,
        ImmutableArray<DirectSourceMapping> SourceMappings,
        ImmutableArray<SetupBodyLine> SetupBodyLines,
        ImmutableArray<string> ImportLines);

    private sealed record DirectSourceMapping(
        string Expression,
        int RenderColumn,
        int CompiledLine,
        int CompiledColumn);

    private abstract class DirectRenderFrame
    {
        public bool ChildrenStarted { get; set; }

        public List<DirectAttribute> Attributes { get; } = new();

        public List<string> ReferenceCaptures { get; } = new();

        public List<string> Children { get; } = new();

        public virtual bool UsesFragment => false;

        public abstract string NormalizeAttributeName(string name);

        public abstract string ToRenderExpression();

        private string? LastAttributeName { get; set; }

        public void AddAttribute(DirectAttribute attribute)
        {
            for (var index = Attributes.Count - 1; index >= 0; index--)
            {
                if (string.Equals(Attributes[index].Name, attribute.Name, StringComparison.Ordinal))
                {
                    Attributes[index] = attribute;
                    LastAttributeName = attribute.Name;
                    return;
                }
            }

            Attributes.Add(attribute);
            LastAttributeName = attribute.Name;
        }

        public bool TrySetLastAttributeValue(string valueExpression)
        {
            if (LastAttributeName is null)
                return false;

            for (var index = Attributes.Count - 1; index >= 0; index--)
            {
                if (string.Equals(Attributes[index].Name, LastAttributeName, StringComparison.Ordinal))
                {
                    Attributes[index] = Attributes[index] with { ValueExpression = valueExpression };
                    return true;
                }
            }

            return false;
        }

        public void AddReferenceCapture(string actionExpression)
        {
            actionExpression = actionExpression.Trim();
            if (string.Equals(actionExpression, "null", StringComparison.Ordinal) ||
                string.Equals(actionExpression, "undefined", StringComparison.Ordinal))
            {
                return;
            }

            ReferenceCaptures.Add(actionExpression);
        }

        protected string FormatPropsExpression()
        {
            if (Attributes.Count == 0 && ReferenceCaptures.Count == 0)
                return "null";

            var props = Attributes
                .Select(attribute => FormatJavaScriptPropertyName(attribute.Name) + ": " + FormatAttributeValueExpression(attribute))
                .ToList();
            if (ReferenceCaptures.Count > 0)
                props.Add("ref: " + FormatReferenceCaptureExpression());

            return "{ " + string.Join(", ", props) + " }";
        }

        private string FormatReferenceCaptureExpression()
        {
            return ReferenceCaptures.Count == 1
                ? ReferenceCaptures[0]
                : "(value) => { " + string.Join(" ", ReferenceCaptures.Select(static capture => "(" + capture + ")(value);")) + " }";
        }

        protected virtual string FormatAttributeValueExpression(DirectAttribute attribute)
        {
            return attribute.ValueExpression;
        }

        protected string FormatChildrenArrayExpression()
        {
            return Children.Count == 0
                ? "null"
                : "[" + string.Join(", ", Children) + "]";
        }
    }

    private sealed class DirectElementFrame : DirectRenderFrame
    {
        private readonly Dictionary<string, DirectEventModifier> _eventModifiers = new(StringComparer.Ordinal);
        private string? _updatesAttributeName;
        private string? _updatesEventName;

        public DirectElementFrame(string tagExpression, string tagName)
        {
            TagExpression = tagExpression;
            TagName = tagName;
        }

        public string TagExpression { get; }

        public string TagName { get; }

        public override string NormalizeAttributeName(string name)
        {
            return NormalizeDirectElementAttributeName(name);
        }

        public override string ToRenderExpression()
        {
            return "h(" + TagExpression + ", " + FormatPropsExpression() + ", " + FormatChildrenArrayExpression() + ")";
        }

        public void SetUpdatesAttributeName(string name)
        {
            _updatesAttributeName = name;
            var lastEventName = Attributes
                .LastOrDefault(static attribute => IsDirectEventAttributeName(attribute.Name))
                ?.Name;
            _updatesEventName = lastEventName;
        }

        public void SetEventModifier(string eventName, bool preventDefault, bool stopPropagation)
        {
            var runtimeName = NormalizeDirectElementAttributeName(eventName);
            _eventModifiers.TryGetValue(runtimeName, out var existing);
            _eventModifiers[runtimeName] = new DirectEventModifier(
                existing.PreventDefault || preventDefault,
                existing.StopPropagation || stopPropagation);
        }

        protected override string FormatAttributeValueExpression(DirectAttribute attribute)
        {
            var value = attribute.ValueExpression;
            if (_updatesAttributeName is not null &&
                string.Equals(attribute.Name, _updatesEventName, StringComparison.Ordinal))
            {
                value = BuildDirectDomBindHandler(value, _updatesAttributeName);
            }

            if (_eventModifiers.TryGetValue(attribute.Name, out var modifier))
                value = BuildDirectEventModifierHandler(value, modifier);

            return value;
        }
    }

    private sealed class DirectComponentFrame : DirectRenderFrame
    {
        private readonly ImmutableDictionary<string, string> _parameterNameMap;

        public DirectComponentFrame(string componentExpression, ImmutableDictionary<string, string> parameterNameMap)
        {
            ComponentExpression = componentExpression;
            _parameterNameMap = parameterNameMap;
        }

        public string ComponentExpression { get; }

        public List<DirectSlot> Slots { get; } = new();

        public override string NormalizeAttributeName(string name)
        {
            return _parameterNameMap.TryGetValue(name, out var mapped)
                ? mapped
                : NormalizeDirectComponentParameterName(name);
        }

        public string NormalizeSlotName(string name)
        {
            if (_parameterNameMap.TryGetValue(name, out var mapped))
                return mapped;

            return string.Equals(name, "ChildContent", StringComparison.Ordinal)
                ? "default"
                : NormalizeDirectComponentParameterName(name);
        }

        public override string ToRenderExpression()
        {
            var props = FormatPropsExpression();
            if (Slots.Count > 0)
            {
                var slots = "{ " + string.Join(", ", Slots.Select(static slot =>
                    FormatJavaScriptPropertyName(slot.Name) + ": " + FormatSlotFunctionExpression(slot))) + " }";
                return "h(" + ComponentExpression + ", " + props + ", " + slots + ")";
            }

            if (Children.Count == 0)
                return "h(" + ComponentExpression + ", " + props + ")";

            var children = Children.Count == 1
                ? Children[0]
                : "[" + string.Join(", ", Children) + "]";
            return "h(" + ComponentExpression + ", " + props + ", " + children + ")";
        }

        private static string FormatSlotFunctionExpression(DirectSlot slot)
        {
            return slot.ParameterName is null
                ? "() => " + slot.RenderExpression
                : "(" + slot.ParameterName + ") => " + slot.RenderExpression;
        }
    }

    private sealed class DirectRegionFrame : DirectRenderFrame
    {
        public override bool UsesFragment => Children.Count > 1;

        public override string NormalizeAttributeName(string name)
        {
            return name;
        }

        public override string ToRenderExpression()
        {
            return Children.Count switch
            {
                0 => "null",
                1 => Children[0],
                _ => "h(Fragment, null, [" + string.Join(", ", Children) + "])"
            };
        }
    }

    private sealed record DirectAttribute(
        string Name,
        string ValueExpression);

    private sealed record DirectSlot(
        string Name,
        string? ParameterName,
        string RenderExpression);

    private readonly record struct DirectEventModifier(
        bool PreventDefault,
        bool StopPropagation);

    private sealed record DirectSlotFunction(
        string RenderExpression,
        string? ScopedParameterName,
        bool UsesFragment,
        bool UsesStaticVNode,
        DirectFunctionRange Range);

    private readonly record struct DirectFunctionRange(
        int Start,
        int End);

    private sealed record StateSlot(
        string RuntimeName,
        string? DeclarationName,
        ITypeSymbol Type,
        string? Initializer,
        int? InitializerCompiledLine = null,
        int? InitializerCompiledColumn = null);
}

internal sealed record RazorSgVueComponentModuleArtifact(
    string ComponentId,
    string RelativePath,
    string ModuleText,
    string ContentHash,
    string SourceMapRelativePath,
    string SourceMapContent,
    string MapHash,
    ImmutableArray<RazorSgFrontendAsset> FrontendAssets);

internal sealed record RazorSgFrontendAsset(
    string SourcePath,
    string ArtifactPath,
    string Kind,
    string ContentHash);
