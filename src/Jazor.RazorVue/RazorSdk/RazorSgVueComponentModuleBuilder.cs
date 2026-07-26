using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Jazor.Common.SourceMaps;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.RazorSdk;

internal static class RazorSgVueComponentModuleBuilder
{
    private const string ECMAScriptModuleAttributeMetadataName = "ECMAScript.ECMAScriptModuleAttribute";
    private const string EventCallbackMetadataName = "Microsoft.AspNetCore.Components.EventCallback";
    private const string EventCallbackOfTMetadataName = "Microsoft.AspNetCore.Components.EventCallback`1";
    private static readonly Regex VariableDeclarationPattern = new(
        @"^\s*(?:export\s+)?(?:let|const|var)\s+([A-Za-z_$][A-Za-z0-9_$]*)\s*(?:=\s*(.*))?;\s*$",
        RegexOptions.CultureInvariant);
    private static readonly Regex ImportFromPattern = new(
        @"^(?<prefix>\s*import\s+.+?\s+from\s+)(?<quote>[""'])(?<path>\./[^""']+)(\k<quote>)(?<suffix>\s*;?\s*)$",
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
        var converter = new AstConverter(
            component.ComponentSymbol,
            semanticModel,
            closure.CreateAstConverterOptions());
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
        var moduleBuild = BuildModuleText(component.ComponentSymbol, closure, compiledScript, relativePath);
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
            ComputeContentHash(sourceMapContent));
    }

    private static ModuleTextBuildResult BuildModuleText(
        INamedTypeSymbol componentSymbol,
        RazorSgComponentMemberClosure closure,
        string compiledScript,
        string relativePath)
    {
        var parts = SplitCompiledScript(compiledScript, closure);
        var usesInvokeAsync = parts.SetupBodyLines.Any(static line =>
            line.Text.Contains("invokeAsync(", StringComparison.Ordinal));
        var setupFactoryName = "create" + SanitizeJavaScriptIdentifierPart(componentSymbol.Name, "Component") + "SetupScope";
        var returnedMembers = GetReturnedMembers(closure);
        var builder = new StringBuilder();
        var lineMappings = ImmutableArray.CreateBuilder<CompiledLineMapping>();
        var line = 0;

        void AppendLine(string value = "")
        {
            builder.AppendLine(value);
            line++;
        }

        AppendLine("import { defineComponent, h, onMounted, onUnmounted, onUpdated, reactive, watch } from \"vue\";");
        AppendLine("import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";");
        foreach (var importLine in parts.ImportLines)
        {
            if (!string.Equals(importLine.Text, "import { defineComponent, h, onMounted, onUnmounted, onUpdated, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, onMounted, onUpdated, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, reactive, watch } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { defineComponent, h, reactive } from \"vue\";", StringComparison.Ordinal) &&
                !string.Equals(importLine.Text, "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";", StringComparison.Ordinal))
            {
                AppendLine(RebaseRootRelativeImportLine(importLine.Text, relativePath));
            }
        }

        AppendLine();
        AppendLine(usesInvokeAsync
            ? "function " + setupFactoryName + "(props, stateHasChanged, invokeAsync) {"
            : "function " + setupFactoryName + "(props, stateHasChanged) {");
        AppendStateDeclaration(builder, parts.StateSlots, lineMappings, ref line);
        if (parts.SetupBodyLines.Length > 0)
        {
            AppendLine();
            AppendIndentedLines(builder, parts.SetupBodyLines, lineMappings, ref line);
        }

        AppendLine();
        AppendLine("  return {");
        for (var index = 0; index < returnedMembers.Length; index++)
        {
            var suffix = index + 1 == returnedMembers.Length ? string.Empty : ",";
            AppendLine("    " + returnedMembers[index] + suffix);
        }

        AppendLine("  };");
        AppendLine("}");
        AppendLine();
        AppendLine("export default defineComponent({");
        AppendPropsDeclaration(builder, closure, ref line);
        AppendEmitsDeclaration(builder, closure, ref line);
        AppendLine("  setup(props, { slots }) {");
        AppendSlotParameterBridges(builder, closure, ref line);
        // Minimal lifecycle/invalidation v1:
        // - state initialization belongs to the setup factory and must run before
        //   the render invalidation slot is materialized
        // - tick is a reactive dependency read by every render
        // - StateHasChanged lowers to stateHasChanged() which bumps tick
        AppendLine("    let invalidate = null;");
        AppendLine("    let pendingInvalidations = 0;");
        AppendLine("    const stateHasChanged = () => {");
        AppendLine("      if (invalidate === null) {");
        AppendLine("        pendingInvalidations++;");
        AppendLine("        return;");
        AppendLine("      }");
        AppendLine("      invalidate.tick++;");
        AppendLine("    };");
        if (usesInvokeAsync)
        {
            AppendLine("    const invokeAsync = (workItem) => {");
            AppendLine("      try {");
            AppendLine("        return Promise.resolve(workItem());");
            AppendLine("      } catch (error) {");
            AppendLine("        return Promise.reject(error);");
            AppendLine("      }");
            AppendLine("    };");
        }

        AppendLine(usesInvokeAsync
            ? "    const scope = " + setupFactoryName + "(props, stateHasChanged, invokeAsync);"
            : "    const scope = " + setupFactoryName + "(props, stateHasChanged);");
        AppendLine("    invalidate = reactive({ tick: pendingInvalidations });");
        // Minimal lifecycle v1: run OnInitialized once before first render when present.
        AppendLine("    if (typeof scope.onInitialized === \"function\") {");
        AppendLine("      scope.onInitialized();");
        AppendLine("    }");
        // OnInitializedAsync: fire-and-forget; does not block first render.
        // Completion schedules at most one invalidation via stateHasChanged().
        AppendLine("    if (typeof scope.onInitializedAsync === \"function\") {");
        AppendLine("      Promise.resolve(scope.onInitializedAsync()).then(");
        AppendLine("        () => {");
        AppendLine("          stateHasChanged();");
        AppendLine("        },");
        AppendLine("        () => {");
        AppendLine("          stateHasChanged();");
        AppendLine("        }");
        AppendLine("      );");
        AppendLine("    }");
        // OnParametersSet: once after parameters are applied, then on subsequent prop updates.
        AppendLine("    if (typeof scope.onParametersSet === \"function\") {");
        AppendLine("      scope.onParametersSet();");
        AppendLine("      watch(");
        AppendLine("        () => props,");
        AppendLine("        () => {");
        AppendLine("          scope.onParametersSet();");
        AppendLine("        },");
        AppendLine("        { deep: true }");
        AppendLine("      );");
        AppendLine("    }");
        // OnParametersSetAsync: serialized async watch.
        // - immediate first run + deep prop watch
        // - chain serializes execution
        // - generation token drops stale completions so old tasks cannot overwrite newer state
        AppendLine("    if (typeof scope.onParametersSetAsync === \"function\") {");
        AppendLine("      let parametersSetAsyncGen = 0;");
        AppendLine("      let parametersSetAsyncTail = Promise.resolve();");
        AppendLine("      const runOnParametersSetAsync = () => {");
        AppendLine("        const gen = ++parametersSetAsyncGen;");
        AppendLine("        parametersSetAsyncTail = parametersSetAsyncTail");
        AppendLine("          .catch(() => {})");
        AppendLine("          .then(() => {");
        AppendLine("            if (gen !== parametersSetAsyncGen) {");
        AppendLine("              return;");
        AppendLine("            }");
        AppendLine("            return Promise.resolve(scope.onParametersSetAsync()).then(");
        AppendLine("              () => {");
        AppendLine("                if (gen === parametersSetAsyncGen) {");
        AppendLine("                  stateHasChanged();");
        AppendLine("                }");
        AppendLine("              },");
        AppendLine("              () => {");
        AppendLine("                if (gen === parametersSetAsyncGen) {");
        AppendLine("                  stateHasChanged();");
        AppendLine("                }");
        AppendLine("              }");
        AppendLine("            );");
        AppendLine("          });");
        AppendLine("      };");
        AppendLine("      runOnParametersSetAsync();");
        AppendLine("      watch(");
        AppendLine("        () => props,");
        AppendLine("        () => {");
        AppendLine("          runOnParametersSetAsync();");
        AppendLine("        },");
        AppendLine("        { deep: true }");
        AppendLine("      );");
        AppendLine("    }");
        // OnAfterRender: firstRender=true on mount, false on subsequent updates.
        AppendLine("    if (typeof scope.onAfterRender === \"function\") {");
        AppendLine("      onMounted(() => {");
        AppendLine("        scope.onAfterRender(true);");
        AppendLine("      });");
        AppendLine("      onUpdated(() => {");
        AppendLine("        scope.onAfterRender(false);");
        AppendLine("      });");
        AppendLine("    }");
        // OnAfterRenderAsync: post-flush fire-and-forget.
        // Intentionally do NOT treat task completion as a render request.
        AppendLine("    if (typeof scope.onAfterRenderAsync === \"function\") {");
        AppendLine("      onMounted(() => {");
        AppendLine("        void Promise.resolve(scope.onAfterRenderAsync(true));");
        AppendLine("      });");
        AppendLine("      onUpdated(() => {");
        AppendLine("        void Promise.resolve(scope.onAfterRenderAsync(false));");
        AppendLine("      });");
        AppendLine("    }");
        // IDisposable / IAsyncDisposable: cleanup once on unmount.
        // Prefer dispose then disposeAsync when both exist; async completion is fire-and-forget.
        AppendLine("    if (typeof scope.dispose === \"function\" || typeof scope.disposeAsync === \"function\") {");
        AppendLine("      onUnmounted(() => {");
        AppendLine("        if (typeof scope.dispose === \"function\") {");
        AppendLine("          scope.dispose();");
        AppendLine("        }");
        AppendLine("        if (typeof scope.disposeAsync === \"function\") {");
        AppendLine("          void scope.disposeAsync();");
        AppendLine("        }");
        AppendLine("      });");
        AppendLine("    }");
        // ShouldRender: first render always executes; later renders may reuse cached VNode.
        // Epoch (invalidate.tick) is still read so StateHasChanged remains a reactive dependency.
        AppendLine("    let hasRendered = false;");
        AppendLine("    let cachedVNode = null;");
        AppendLine();
        AppendLine("    return () => {");
        AppendLine("      invalidate.tick;");
        AppendLine("      if (hasRendered && typeof scope.shouldRender === \"function\" && !scope.shouldRender()) {");
        AppendLine("        return cachedVNode;");
        AppendLine("      }");
        AppendLine("      hasRendered = true;");
        AppendLine("      const builder = createRenderContext(h);");
        AppendLine("      scope.buildRenderTree(builder);");
        AppendLine("      cachedVNode = builder.finish();");
        AppendLine("      return cachedVNode;");
        AppendLine("    };");
        AppendLine("  }");
        AppendLine("});");

        return new ModuleTextBuildResult(
            Util.NormalizeLineEndingsToLf(builder.ToString()),
            lineMappings.ToImmutable());
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
        var propNames = closure.ParameterProperties
            .Where(static property => !IsRenderFragmentType(property.Type))
            .Select(static property => Util.GetConfigOrSymbolName(property))
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
        var emitNames = closure.ParameterProperties
            .Where(static property => IsEventCallbackType(property.Type))
            .Select(static property => GetVueEmitName(Util.GetConfigOrSymbolName(property)))
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
            .Where(static property => IsRenderFragmentType(property.Type))
            .Select(static property => new SlotParameterBridge(
                property.Name,
                Util.GetConfigOrSymbolName(property),
                GetVueSlotName(property)))
            .Where(static item => !string.IsNullOrWhiteSpace(item.RuntimePropName) &&
                                  !string.IsNullOrWhiteSpace(item.VueSlotName))
            .Distinct()
            .OrderBy(static item => item.VueSlotName, StringComparer.Ordinal)
            .ToArray();
        foreach (var item in slotParameters)
        {
            builder.AppendLine("    if (typeof slots." + item.VueSlotName + " === \"function\") {");
            line++;
            builder.AppendLine("      props." + item.RuntimePropName + " = (builder) => {");
            line++;
            builder.AppendLine("        const content = slots." + item.VueSlotName + "();");
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

    private static string GetVueSlotName(IPropertySymbol property)
        => IsChildContentParameter(property)
            ? "default"
            : Util.GetConfigOrSymbolName(property);

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
        string VueSlotName);

    private static bool IsChildContentParameter(IPropertySymbol property)
        => string.Equals(property.Name, "ChildContent", StringComparison.Ordinal) &&
           IsRenderFragmentType(property.Type);

    private static bool IsRenderFragmentType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.IsGenericType)
            return false;

        return string.Equals(
            named.OriginalDefinition.ToDisplayString(),
            "Microsoft.AspNetCore.Components.RenderFragment",
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

        foreach (var method in closure.ReachableMethods)
        {
            AddName(method);
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
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        var segments = normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0)
            throw new InvalidOperationException("ECMAScriptModule import path cannot be empty.");

        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException("ECMAScriptModule import path cannot escape the output directory.");

        normalized = string.Join("/", segments);
        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        return normalized;
    }

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
        ImmutableArray<CompiledLineMapping> CompiledLineMappings);

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
    string MapHash);
