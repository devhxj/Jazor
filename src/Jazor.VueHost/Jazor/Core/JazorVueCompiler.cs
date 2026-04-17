using System.Text;
using System.Text.RegularExpressions;
using Jazor.Emit;
using Jazor.Emit.SourceMaps;

namespace Jazor.Vue;

public sealed partial class JazorVueCompiler
{
    private static readonly Regex PropPattern = new Regex(
        @"\[Prop\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*\{",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex StatePattern = new Regex(
        @"\[State\]\s*private\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*(=\s*(?<initializer>[^;]+))?;",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ComputedPattern = new Regex(
        @"\[Computed\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*=>\s*(?<expression>[^;]+);",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex MethodPattern = new Regex(
        @"public\s+(?<async>async\s+)?(?<return>[\w\.\?\<\>\[\]]+)\s+(?<name>\w+)\s*\((?<parameters>[^\)]*)\)\s*\{",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private const string LocalTypePattern = @"(?:var|[A-Za-z_][A-Za-z0-9_\.\?\<\>\[\],]*)";
    private static readonly Regex LocalDeclarationWithInitializerPattern = new(
        @"^(?<indent>\s*)(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expression>.+);\s*$",
        RegexOptions.Compiled);
    private static readonly Regex LocalDeclarationPattern = new(
        @"^(?<indent>\s*)(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*;\s*$",
        RegexOptions.Compiled);
    private static readonly Regex ForLoopPattern = new(
        @"^(?<indent>\s*)for\s*\((?<initializer>.*?);(?<condition>.*?);(?<iterator>.*?)\)(?<suffix>\s*\{?\s*)$",
        RegexOptions.Compiled);
    private static readonly Regex ForInitializerDeclarationPattern = new(
        @"^(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expression>.+)$",
        RegexOptions.Compiled);
    private static readonly Regex ForeachLoopPattern = new(
        @"^(?<indent>\s*)foreach\s*\(\s*(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s+in\s+(?<expression>.+?)\s*\)(?<suffix>\s*\{?\s*)$",
        RegexOptions.Compiled);
    private static readonly Regex TypedCatchPattern = new(
        @"^(?<indent>\s*)catch\s*\(\s*(?<type>" + LocalTypePattern + @")\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\)(?<suffix>\s*\{?\s*)$",
        RegexOptions.Compiled);
    private static readonly Regex ExceptionConstructorPattern = new(
        @"\bnew\s+(?<type>(?:[A-Za-z_][A-Za-z0-9_]*\.)*[A-Za-z_][A-Za-z0-9_]*)\s*\(",
        RegexOptions.Compiled);
    private static readonly Regex SourceMapAnchorTokenPattern = new(
        @"[A-Za-z_][A-Za-z0-9_]*|\d+",
        RegexOptions.Compiled);
    private static readonly ISet<string> EmptyShadowedNames = new HashSet<string>(StringComparer.Ordinal);

    public JazorVueCompilationResult Compile(JazorVueDocument document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var diagnostics = new List<string>();
        var externalSymbols = VirtualExternalSymbolTable.FromImports(document.Imports);
        var generatedExternalDeclarationsText = JazorVueExternalDeclarationEmitter.Emit(
            externalSymbols,
            JazorVueExternalDeclarationEmitter.DefaultNamespace,
            JazorVueExternalDeclarationEmitter.CreateContainerName(document.FilePath));
        var imports = BuildImportStatements(document.Imports);
        var importSourceLines = GetImportSourceLines(document);
        var props = ExtractProps(document);
        var states = ExtractStates(document);
        var computeds = ExtractComputeds(document);
        var methods = ExtractMethods(document);
        var loweringContext = LoweringContext.Create(props, states, computeds, methods);
        var builder = new StringBuilder();
        var generatedLines = new List<GeneratedVueLine>();
        var scriptAnchorSourceLine = GetScriptAnchorSourceLine(document, importSourceLines, props, states, computeds, methods);

        AppendGeneratedLine(builder, generatedLines, "<script setup>", scriptAnchorSourceLine);
        var vueHelpers = GetVueHelpers(props, states, computeds);
        if (vueHelpers.Count > 0)
        {
            AppendGeneratedLine(
                builder,
                generatedLines,
                $"import {{ {string.Join(", ", vueHelpers)} }} from \"vue\";",
                scriptAnchorSourceLine);
        }

        for (var index = 0; index < imports.Count; index++)
        {
            AppendGeneratedLine(
                builder,
                generatedLines,
                imports[index],
                index < importSourceLines.Count ? importSourceLines[index] : scriptAnchorSourceLine);
        }

        if (props.Count > 0)
        {
            AppendGeneratedLine(builder, generatedLines, string.Empty, props[0].SourceLine);
            AppendGeneratedLine(builder, generatedLines, "const props = defineProps({", props[0].SourceLine);
            foreach (var prop in props)
            {
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"  {prop.RuntimeName}: {prop.VueTypeExpression},",
                    prop.SourceLine);
            }

            AppendGeneratedLine(builder, generatedLines, "});", props[^1].SourceLine);

            foreach (var prop in props)
            {
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"const {prop.RuntimeName} = toRef(props, \"{prop.RuntimeName}\");",
                    prop.SourceLine);
            }
        }

        if (states.Count > 0)
        {
            AppendGeneratedLine(builder, generatedLines, string.Empty, states[0].SourceLine);
            foreach (var state in states)
            {
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"const {state.RuntimeName} = ref({LowerExpression(state.Initializer ?? "undefined", loweringContext, EmptyShadowedNames)});",
                    state.SourceLine);
            }
        }

        if (computeds.Count > 0)
        {
            AppendGeneratedLine(builder, generatedLines, string.Empty, computeds[0].SourceLine);
            foreach (var computed in computeds)
            {
                if (TryLowerComputed(computed, loweringContext, out var loweredExpression))
                {
                    AppendGeneratedLine(
                        builder,
                        generatedLines,
                        $"const {computed.RuntimeName} = computed(() => {loweredExpression});",
                        computed.SourceLine);
                    continue;
                }

                diagnostics.Add($"Computed member '{computed.SourceName}' could not be lowered by the local fallback compiler.");
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"const {computed.RuntimeName} = computed(() => {{",
                    computed.SourceLine);
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"  // Fallback compiler could not lower computed member {computed.SourceName}.",
                    computed.SourceLine);
                AppendGeneratedLine(builder, generatedLines, "  return undefined;", computed.SourceLine);
                AppendGeneratedLine(builder, generatedLines, "});", computed.SourceLine);
            }
        }

        if (methods.Count > 0)
        {
            AppendGeneratedLine(builder, generatedLines, string.Empty, methods[0].SourceLine);
            foreach (var method in methods)
            {
                AppendGeneratedLine(
                    builder,
                    generatedLines,
                    $"{(method.IsAsync ? "async function " : "function ")}{method.RuntimeName}({string.Join(", ", method.Parameters)}) {{",
                    method.SourceLine);

                if (TryLowerMethodBody(method, loweringContext, out var loweredBody))
                {
                    foreach (var line in loweredBody)
                    {
                        AppendGeneratedLine(
                            builder,
                            generatedLines,
                            "  " + line.Line,
                            line.SourceLine);
                    }
                }
                else
                {
                    diagnostics.Add($"Method '{method.SourceName}' could not be lowered by the local fallback compiler.");
                    AppendGeneratedLine(
                        builder,
                        generatedLines,
                        $"  // Fallback compiler could not lower method {method.SourceName}.",
                        method.SourceLine);
                }

                AppendGeneratedLine(builder, generatedLines, "}", method.SourceLine);
            }
        }

        if (!string.IsNullOrWhiteSpace(document.Code))
        {
            var codeStartSourceLine = GetCodeStartSourceLine(document) ?? scriptAnchorSourceLine;
            AppendGeneratedLine(builder, generatedLines, string.Empty, codeStartSourceLine);
            AppendGeneratedLine(builder, generatedLines, "/*", codeStartSourceLine);
            AppendGeneratedLine(
                builder,
                generatedLines,
                " Original @code block retained for bridge diagnostics:",
                codeStartSourceLine);
            AppendGeneratedTextLines(
                builder,
                generatedLines,
                document.Code.Replace("*/", "* /", StringComparison.Ordinal),
                codeStartSourceLine);
            AppendGeneratedLine(builder, generatedLines, "*/", GetCodeEndSourceLine(document) ?? codeStartSourceLine);
        }

        var templateAnchorSourceLine = GetTemplateStartSourceLine(document) ?? scriptAnchorSourceLine;
        AppendGeneratedLine(builder, generatedLines, "</script>", scriptAnchorSourceLine);
        AppendGeneratedLine(builder, generatedLines, string.Empty, templateAnchorSourceLine);
        AppendGeneratedLine(builder, generatedLines, "<template>", templateAnchorSourceLine);
        if (string.IsNullOrWhiteSpace(document.Template))
        {
            AppendGeneratedLine(builder, generatedLines, "<div />", templateAnchorSourceLine);
        }
        else
        {
            AppendGeneratedTextLines(builder, generatedLines, document.Template, templateAnchorSourceLine);
        }

        AppendGeneratedLine(builder, generatedLines, "</template>", GetTemplateEndSourceLine(document) ?? templateAnchorSourceLine);

        if (methods.Count == 0 && document.Code.Length > 0)
            diagnostics.Add("No public methods were lowered. The current bridge compiler emits method stubs only for public instance methods.");

        var generatedVueText = builder.ToString();
        var generatedVueSourceMap = CreateGeneratedVueSourceMap(document, generatedLines);

        return new JazorVueCompilationResult(
            document,
            externalSymbols,
            generatedVueText,
            generatedExternalDeclarationsText,
            diagnostics,
            hotReload: null,
            generatedVueSourceMap);
    }

    private static void AppendGeneratedLine(
        StringBuilder builder,
        List<GeneratedVueLine> generatedLines,
        string line,
        int? sourceLine)
    {
        builder.AppendLine(line);
        generatedLines.Add(new GeneratedVueLine(line, sourceLine));
    }

    private static void AppendGeneratedTextLines(
        StringBuilder builder,
        List<GeneratedVueLine> generatedLines,
        string text,
        int? sourceStartLine)
    {
        var lines = NormalizeLineEndings(text).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            AppendGeneratedLine(
                builder,
                generatedLines,
                lines[index],
                sourceStartLine.HasValue ? sourceStartLine.Value + index : null);
        }
    }

    private static string? CreateGeneratedVueSourceMap(
        JazorVueDocument document,
        IReadOnlyList<GeneratedVueLine> generatedLines)
    {
        var sourceLines = NormalizeLineEndings(document.SourceText).Split('\n');
        var segments = new List<SourceMapSegment>(generatedLines.Count * 2);
        for (var generatedLine = 0; generatedLine < generatedLines.Count; generatedLine++)
        {
            var generatedLineMapping = generatedLines[generatedLine];
            if (!generatedLineMapping.SourceLine.HasValue)
            {
                continue;
            }

            segments.Add(new SourceMapSegment(
                GeneratedLine: generatedLine,
                GeneratedColumn: 0,
                SourceIndex: 0,
                SourceLine: generatedLineMapping.SourceLine.Value,
                SourceColumn: 0));

            if (TryCreateColumnPreciseSegment(
                generatedLine,
                generatedLineMapping.Text,
                GetSourceLineText(sourceLines, generatedLineMapping.SourceLine.Value),
                generatedLineMapping.SourceLine.Value,
                out var preciseSegment)
                && preciseSegment is not null)
            {
                segments.Add(preciseSegment);
            }
        }

        if (segments.Count == 0)
        {
            return null;
        }

        var fileName = Path.GetFileName(document.FilePath);
        var sourceMap = new SourceMapDocument(
            fileName,
            [new SourceMapSource(fileName, document.SourceText)],
            segments);
        return new SourceMapWriter().Write(sourceMap);
    }

    private static bool TryCreateColumnPreciseSegment(
        int generatedLine,
        string generatedText,
        string sourceText,
        int sourceLine,
        out SourceMapSegment? segment)
    {
        if (TryFindSharedTokenAnchor(generatedText, sourceText, out var generatedColumn, out var sourceColumn))
        {
            segment = new SourceMapSegment(
                GeneratedLine: generatedLine,
                GeneratedColumn: generatedColumn,
                SourceIndex: 0,
                SourceLine: sourceLine,
                SourceColumn: sourceColumn);
            return true;
        }

        generatedColumn = GetFirstNonWhitespaceColumn(generatedText);
        sourceColumn = GetFirstNonWhitespaceColumn(sourceText);
        if (generatedColumn > 0 && sourceColumn > 0)
        {
            segment = new SourceMapSegment(
                GeneratedLine: generatedLine,
                GeneratedColumn: generatedColumn,
                SourceIndex: 0,
                SourceLine: sourceLine,
                SourceColumn: sourceColumn);
            return true;
        }

        segment = default;
        return false;
    }

    private static bool TryFindSharedTokenAnchor(
        string generatedText,
        string sourceText,
        out int generatedColumn,
        out int sourceColumn)
    {
        generatedColumn = 0;
        sourceColumn = 0;

        var fallbackGeneratedColumn = -1;
        var fallbackSourceColumn = -1;
        foreach (Match match in SourceMapAnchorTokenPattern.Matches(generatedText))
        {
            if (match.Index <= 0)
            {
                continue;
            }

            var candidate = match.Value;
            if (candidate.Length == 0)
            {
                continue;
            }

            var sourceIndex = sourceText.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (sourceIndex < 0)
            {
                continue;
            }

            if (sourceIndex > 0)
            {
                generatedColumn = match.Index;
                sourceColumn = sourceIndex;
                return true;
            }

            if (fallbackGeneratedColumn < 0)
            {
                fallbackGeneratedColumn = match.Index;
                fallbackSourceColumn = sourceIndex;
            }
        }

        if (fallbackGeneratedColumn > 0)
        {
            generatedColumn = fallbackGeneratedColumn;
            sourceColumn = fallbackSourceColumn;
            return true;
        }

        return false;
    }

    private static string GetSourceLineText(IReadOnlyList<string> sourceLines, int sourceLine)
        => sourceLine >= 0 && sourceLine < sourceLines.Count
            ? sourceLines[sourceLine]
            : string.Empty;

    private static int GetFirstNonWhitespaceColumn(string text)
    {
        for (var index = 0; index < text.Length; index++)
        {
            if (!char.IsWhiteSpace(text[index]))
            {
                return index;
            }
        }

        return 0;
    }

    private static IReadOnlyList<int?> GetImportSourceLines(JazorVueDocument document)
    {
        var sourceLines = new List<int?>(document.Imports.Count);
        var searchStart = 0;
        foreach (var import in document.Imports)
        {
            int? sourceLine = null;
            if (!string.IsNullOrWhiteSpace(import.RawText))
            {
                var index = document.SourceText.IndexOf(import.RawText, searchStart, StringComparison.Ordinal);
                if (index < 0)
                {
                    index = document.SourceText.IndexOf(import.RawText, StringComparison.Ordinal);
                }

                if (index >= 0)
                {
                    sourceLine = CountNewLines(document.SourceText, 0, index);
                    searchStart = index + import.RawText.Length;
                }
            }

            sourceLines.Add(sourceLine ?? GetTemplateStartSourceLine(document) ?? GetCodeStartSourceLine(document));
        }

        return sourceLines;
    }

    private static int GetScriptAnchorSourceLine(
        JazorVueDocument document,
        IReadOnlyList<int?> importSourceLines,
        IReadOnlyList<PropDescriptor> props,
        IReadOnlyList<StateDescriptor> states,
        IReadOnlyList<ComputedDescriptor> computeds,
        IReadOnlyList<MethodDescriptor> methods)
        => importSourceLines.FirstOrDefault(static line => line.HasValue)
            ?? props.FirstOrDefault()?.SourceLine
            ?? states.FirstOrDefault()?.SourceLine
            ?? computeds.FirstOrDefault()?.SourceLine
            ?? methods.FirstOrDefault()?.SourceLine
            ?? GetCodeStartSourceLine(document)
            ?? GetTemplateStartSourceLine(document)
            ?? 0;

    private static int GetCodeSourceLine(JazorVueDocument document, int relativeCodeOffset)
    {
        if (document.CodeStartIndex < 0)
        {
            return GetTemplateStartSourceLine(document) ?? 0;
        }

        var absoluteOffset = Math.Clamp(document.CodeStartIndex + Math.Max(relativeCodeOffset, 0), 0, document.SourceText.Length);
        return CountNewLines(document.SourceText, 0, absoluteOffset);
    }

    private static int? GetCodeStartSourceLine(JazorVueDocument document)
        => document.CodeStartIndex >= 0
            ? CountNewLines(document.SourceText, 0, Math.Min(document.CodeStartIndex, document.SourceText.Length))
            : null;

    private static int? GetCodeEndSourceLine(JazorVueDocument document)
        => document.CodeStartIndex >= 0
            ? CountNewLines(
                document.SourceText,
                0,
                Math.Min(document.CodeStartIndex + Math.Max(document.CodeLength - 1, 0), document.SourceText.Length))
            : null;

    private static int? GetTemplateStartSourceLine(JazorVueDocument document)
        => document.TemplateStartIndex >= 0
            ? CountNewLines(document.SourceText, 0, Math.Min(document.TemplateStartIndex, document.SourceText.Length))
            : null;

    private static int? GetTemplateEndSourceLine(JazorVueDocument document)
        => document.TemplateStartIndex >= 0
            ? CountNewLines(
                document.SourceText,
                0,
                Math.Min(document.TemplateStartIndex + Math.Max(document.TemplateLength - 1, 0), document.SourceText.Length))
            : null;

    private static int CountNewLines(string text, int startIndex, int length)
    {
        var count = 0;
        var endIndex = Math.Min(text.Length, startIndex + length);
        for (var index = Math.Max(0, startIndex); index < endIndex; index++)
        {
            if (text[index] == '\n')
            {
                count++;
            }
        }

        return count;
    }

    private static string NormalizeLineEndings(string text)
        => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static IReadOnlyList<string> GetVueHelpers(
        IReadOnlyList<PropDescriptor> props,
        IReadOnlyList<StateDescriptor> states,
        IReadOnlyList<ComputedDescriptor> computeds)
    {
        var helpers = new List<string>();
        if (computeds.Count > 0)
        {
            helpers.Add("computed");
        }

        if (states.Count > 0)
        {
            helpers.Add("ref");
        }

        if (props.Count > 0)
        {
            helpers.Add("toRef");
        }

        return helpers;
    }

    private static IReadOnlyList<string> BuildImportStatements(IReadOnlyList<JazorImportDirective> imports)
    {
        var statements = new List<string>();
        foreach (var import in imports)
        {
            var defaultBinding = import.Bindings.FirstOrDefault(static binding => binding.BindingKind == JazorImportBindingKind.Default);
            var namespaceBinding = import.Bindings.FirstOrDefault(static binding => binding.BindingKind == JazorImportBindingKind.Namespace);
            var namedBindings = import.Bindings.Where(static binding => binding.BindingKind == JazorImportBindingKind.Named).ToArray();
            string clause;
            if (defaultBinding is not null && namespaceBinding is null && namedBindings.Length == 0)
            {
                clause = defaultBinding.LocalName;
            }
            else if (defaultBinding is null && namespaceBinding is not null && namedBindings.Length == 0)
            {
                clause = "* as " + namespaceBinding.LocalName;
            }
            else
            {
                var segments = new List<string>();
                if (defaultBinding is not null)
                    segments.Add(defaultBinding.LocalName);

                if (namespaceBinding is not null)
                {
                    segments.Add("* as " + namespaceBinding.LocalName);
                }
                else if (namedBindings.Length > 0)
                {
                    var named = namedBindings.Select(static binding =>
                        binding.ImportedName is not null && !string.Equals(binding.ImportedName, binding.LocalName, StringComparison.Ordinal)
                            ? binding.ImportedName + " as " + binding.LocalName
                            : binding.LocalName);
                    segments.Add("{ " + string.Join(", ", named) + " }");
                }

                clause = string.Join(", ", segments);
            }

            statements.Add($"import {clause} from \"{import.Source}\";");
        }

        return statements;
    }

    private static IReadOnlyList<PropDescriptor> ExtractProps(JazorVueDocument document)
    {
        var results = new List<PropDescriptor>();
        var code = document.Code;
        foreach (Match match in PropPattern.Matches(code))
        {
            var typeName = match.Groups["type"].Value.Trim();
            var sourceName = match.Groups["name"].Value.Trim();
            results.Add(new PropDescriptor(
                sourceName,
                JazorVueNaming.ToCamelCase(sourceName),
                MapVueType(typeName),
                GetCodeSourceLine(document, match.Index)));
        }

        return results;
    }

    private static IReadOnlyList<StateDescriptor> ExtractStates(JazorVueDocument document)
    {
        var results = new List<StateDescriptor>();
        var code = document.Code;
        foreach (Match match in StatePattern.Matches(code))
        {
            var typeName = match.Groups["type"].Value.Trim();
            var sourceName = match.Groups["name"].Value.Trim();
            var initializer = match.Groups["initializer"].Success
                ? match.Groups["initializer"].Value.Trim()
                : null;
            results.Add(new StateDescriptor(
                sourceName,
                JazorVueNaming.ToCamelCase(sourceName),
                typeName,
                initializer,
                GetCodeSourceLine(document, match.Index)));
        }

        return results;
    }

    private static IReadOnlyList<ComputedDescriptor> ExtractComputeds(JazorVueDocument document)
    {
        var results = new List<ComputedDescriptor>();
        var code = document.Code;
        foreach (Match match in ComputedPattern.Matches(code))
        {
            var typeName = match.Groups["type"].Value.Trim();
            var sourceName = match.Groups["name"].Value.Trim();
            var expression = match.Groups["expression"].Value.Trim();
            results.Add(new ComputedDescriptor(
                sourceName,
                JazorVueNaming.ToCamelCase(sourceName),
                typeName,
                expression,
                GetCodeSourceLine(document, match.Index)));
        }

        return results;
    }

    private static IReadOnlyList<MethodDescriptor> ExtractMethods(JazorVueDocument document)
    {
        var results = new List<MethodDescriptor>();
        var code = document.Code;
        foreach (Match match in MethodPattern.Matches(code))
        {
            var isAsync = match.Groups["async"].Success;
            var returnType = match.Groups["return"].Value.Trim();
            var sourceName = match.Groups["name"].Value.Trim();
            var parameterBlock = match.Groups["parameters"].Value.Trim();
            var bodyStart = match.Index + match.Length;
            var body = ExtractBlockBody(code, bodyStart, out _, out var bodyStartOffset);
            var parameters = string.IsNullOrWhiteSpace(parameterBlock)
                ? Array.Empty<string>()
                : parameterBlock.Split(',')
                    .Select(static parameter => parameter.Trim())
                    .Where(static parameter => parameter.Length > 0)
                    .Select(static parameter =>
                    {
                        var sanitized = parameter.Split('=')[0].Trim();
                        var parts = sanitized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        return parts[^1];
                    })
                    .ToArray();
            results.Add(new MethodDescriptor(
                sourceName,
                JazorVueNaming.ToCamelCase(sourceName),
                CreateMethodSignature(isAsync, returnType, sourceName, parameterBlock),
                parameters,
                body,
                isAsync,
                GetCodeSourceLine(document, match.Index),
                GetCodeSourceLine(document, bodyStartOffset)));
        }

        return results;
    }

    private static string CreateMethodSignature(
        bool isAsync,
        string returnType,
        string sourceName,
        string parameterBlock)
        => (isAsync ? "async " : string.Empty)
            + returnType.Trim()
            + " "
            + sourceName.Trim()
            + "("
            + NormalizeWhitespace(parameterBlock)
            + ")";

    private static string ExtractBlockBody(string code, int bodyStart, out int nextIndex, out int bodyStartOffset)
    {
        var depth = 1;
        for (var index = bodyStart; index < code.Length; index++)
        {
            switch (code[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        var body = code[bodyStart..index];
                        var trimmedBody = body.Trim();
                        var trimOffset = trimmedBody.Length == 0
                            ? 0
                            : body.IndexOf(trimmedBody, StringComparison.Ordinal);
                        nextIndex = index + 1;
                        bodyStartOffset = bodyStart + Math.Max(trimOffset, 0);
                        return trimmedBody;
                    }

                    break;
            }
        }

        nextIndex = code.Length;
        var tail = code[bodyStart..];
        var trimmedTail = tail.Trim();
        var tailTrimOffset = trimmedTail.Length == 0
            ? 0
            : tail.IndexOf(trimmedTail, StringComparison.Ordinal);
        bodyStartOffset = bodyStart + Math.Max(tailTrimOffset, 0);
        return trimmedTail;
    }

    private static bool TryLowerComputed(
        ComputedDescriptor computed,
        LoweringContext loweringContext,
        out string loweredExpression)
    {
        if (string.IsNullOrWhiteSpace(computed.Expression))
        {
            loweredExpression = "undefined";
            return false;
        }

        loweredExpression = LowerExpression(computed.Expression, loweringContext, EmptyShadowedNames);
        return true;
    }

    private static bool TryLowerMethodBody(
        MethodDescriptor method,
        LoweringContext loweringContext,
        out IReadOnlyList<LoweredMethodLine> loweredLines)
    {
        if (string.IsNullOrWhiteSpace(method.Body))
        {
            loweredLines = Array.Empty<LoweredMethodLine>();
            return true;
        }

        var scopeStack = new Stack<HashSet<string>>();
        scopeStack.Push(new HashSet<string>(method.Parameters, StringComparer.Ordinal));
        string[] pendingBlockScopedNames = [];
        var results = new List<LoweredMethodLine>();
        var sourceLineOffset = 0;
        foreach (var rawLine in method.Body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.Trim();
            var sourceLine = method.BodyStartLine + sourceLineOffset++;
            if (line.Length == 0)
            {
                results.Add(new LoweredMethodLine(string.Empty, sourceLine));
                continue;
            }

            for (var closingBraceIndex = 0;
                 closingBraceIndex < CountLeadingCharacters(line, '}') && scopeStack.Count > 1;
                 closingBraceIndex++)
            {
                scopeStack.Pop();
            }

            var shadowedNames = GetVisibleShadowedNames(scopeStack);
            var loweredStatement = LowerStatementLine(line, loweringContext, shadowedNames);
            results.Add(new LoweredMethodLine(loweredStatement.Line, sourceLine));

            var openingBraceCount = CountCharacters(line, '{');
            if (openingBraceCount > 0)
            {
                var blockScopedNames = pendingBlockScopedNames;
                if (blockScopedNames.Length == 0
                    && loweredStatement.DeclaredNameLifetime == DeclaredNameLifetime.NextBlockScope)
                {
                    blockScopedNames = loweredStatement.DeclaredNames;
                }

                for (var openingBraceIndex = 0; openingBraceIndex < openingBraceCount; openingBraceIndex++)
                {
                    scopeStack.Push(openingBraceIndex == 0
                        ? new HashSet<string>(blockScopedNames, StringComparer.Ordinal)
                        : new HashSet<string>(StringComparer.Ordinal));
                }

                pendingBlockScopedNames = [];

                if (loweredStatement.DeclaredNameLifetime == DeclaredNameLifetime.CurrentScope)
                {
                    AddNamesToCurrentScope(scopeStack, loweredStatement.DeclaredNames);
                }

                continue;
            }

            switch (loweredStatement.DeclaredNameLifetime)
            {
                case DeclaredNameLifetime.CurrentScope:
                    AddNamesToCurrentScope(scopeStack, loweredStatement.DeclaredNames);
                    pendingBlockScopedNames = [];
                    break;
                case DeclaredNameLifetime.NextBlockScope:
                    pendingBlockScopedNames = loweredStatement.DeclaredNames;
                    break;
                default:
                    pendingBlockScopedNames = [];
                    break;
            }
        }

        loweredLines = results;
        return true;
    }

    private static LoweredStatement LowerStatementLine(
        string line,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
    {
        if (TryLowerForLoop(line, loweringContext, shadowedNames, out var loweredForLoop, out var forDeclaredNames))
        {
            return new LoweredStatement(loweredForLoop, forDeclaredNames, DeclaredNameLifetime.NextBlockScope);
        }

        if (TryLowerForeachLoop(line, loweringContext, shadowedNames, out var loweredForeachLoop, out var foreachDeclaredNames))
        {
            return new LoweredStatement(loweredForeachLoop, foreachDeclaredNames, DeclaredNameLifetime.NextBlockScope);
        }

        if (TryLowerTypedCatch(line, out var loweredCatch, out var catchDeclaredNames))
        {
            return new LoweredStatement(loweredCatch, catchDeclaredNames, DeclaredNameLifetime.NextBlockScope);
        }

        var declarationWithInitializerMatch = LocalDeclarationWithInitializerPattern.Match(line);
        if (declarationWithInitializerMatch.Success
            && IsSupportedLocalTypeToken(declarationWithInitializerMatch.Groups["type"].Value))
        {
            var variableName = declarationWithInitializerMatch.Groups["name"].Value;
            var expression = declarationWithInitializerMatch.Groups["expression"].Value.Trim();
            var loweredDeclaration = $"let {variableName} = {LowerExpression(expression, loweringContext, shadowedNames)};";
            return new LoweredStatement(loweredDeclaration, [variableName], DeclaredNameLifetime.CurrentScope);
        }

        var declarationMatch = LocalDeclarationPattern.Match(line);
        if (declarationMatch.Success
            && IsSupportedLocalTypeToken(declarationMatch.Groups["type"].Value))
        {
            var variableName = declarationMatch.Groups["name"].Value;
            return new LoweredStatement($"let {variableName};", [variableName], DeclaredNameLifetime.CurrentScope);
        }

        return new LoweredStatement(LowerExpression(line, loweringContext, shadowedNames), [], DeclaredNameLifetime.None);
    }

    private static bool TryLowerForLoop(
        string line,
        LoweringContext loweringContext,
        ISet<string> shadowedNames,
        out string loweredLine,
        out string[] declaredNames)
    {
        var match = ForLoopPattern.Match(line);
        if (!match.Success)
        {
            loweredLine = string.Empty;
            declaredNames = [];
            return false;
        }

        var loopShadowedNames = new HashSet<string>(shadowedNames, StringComparer.Ordinal);
        var initializer = LowerForInitializer(match.Groups["initializer"].Value.Trim(), loweringContext, loopShadowedNames, out var declaredName);
        var condition = LowerOptionalExpression(match.Groups["condition"].Value.Trim(), loweringContext, loopShadowedNames);
        var iterator = LowerOptionalExpression(match.Groups["iterator"].Value.Trim(), loweringContext, loopShadowedNames);
        declaredNames = declaredName is null ? [] : [declaredName];
        loweredLine = $"for ({initializer}; {condition}; {iterator}){match.Groups["suffix"].Value}";
        return true;
    }

    private static bool TryLowerForeachLoop(
        string line,
        LoweringContext loweringContext,
        ISet<string> shadowedNames,
        out string loweredLine,
        out string[] declaredNames)
    {
        var match = ForeachLoopPattern.Match(line);
        if (!match.Success)
        {
            loweredLine = string.Empty;
            declaredNames = [];
            return false;
        }

        var variableName = match.Groups["name"].Value;
        var expression = LowerExpression(match.Groups["expression"].Value.Trim(), loweringContext, shadowedNames);
        loweredLine = $"for (const {variableName} of {expression}){match.Groups["suffix"].Value}";
        declaredNames = [variableName];
        return true;
    }

    private static bool TryLowerTypedCatch(
        string line,
        out string loweredLine,
        out string[] declaredNames)
    {
        var match = TypedCatchPattern.Match(line);
        if (!match.Success)
        {
            loweredLine = string.Empty;
            declaredNames = [];
            return false;
        }

        var variableName = match.Groups["name"].Value;
        loweredLine = $"catch ({variableName}){match.Groups["suffix"].Value}";
        declaredNames = [variableName];
        return true;
    }

    private static string LowerForInitializer(
        string initializer,
        LoweringContext loweringContext,
        ISet<string> shadowedNames,
        out string? declaredName)
    {
        if (initializer.Length == 0)
        {
            declaredName = null;
            return string.Empty;
        }

        var declarationMatch = ForInitializerDeclarationPattern.Match(initializer);
        if (declarationMatch.Success
            && IsSupportedLocalTypeToken(declarationMatch.Groups["type"].Value))
        {
            var variableName = declarationMatch.Groups["name"].Value;
            var expression = declarationMatch.Groups["expression"].Value.Trim();
            var loweredDeclaration = $"let {variableName} = {LowerExpression(expression, loweringContext, shadowedNames)}";
            shadowedNames.Add(variableName);
            declaredName = variableName;
            return loweredDeclaration;
        }

        declaredName = null;
        return LowerExpression(initializer, loweringContext, shadowedNames);
    }

    private static string LowerOptionalExpression(
        string expression,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
        => expression.Length == 0
            ? string.Empty
            : LowerExpression(expression, loweringContext, shadowedNames);

    private static string LowerExpression(
        string expression,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
    {
        var rewritten = expression.Replace("this.", string.Empty, StringComparison.Ordinal)
            .Replace("string.Empty", "\"\"", StringComparison.Ordinal)
            .Replace("Task.CompletedTask", "Promise.resolve()", StringComparison.Ordinal);
        rewritten = ExceptionConstructorPattern.Replace(
            rewritten,
            static match =>
            {
                var typeName = match.Groups["type"].Value;
                var simpleTypeName = typeName[(typeName.LastIndexOf('.') + 1)..];
                return simpleTypeName.EndsWith("Exception", StringComparison.Ordinal)
                    ? "new Error("
                    : match.Value;
            });
        rewritten = RewriteInterpolatedStrings(rewritten, loweringContext, shadowedNames);
        return RewriteIdentifiers(rewritten, loweringContext, shadowedNames);
    }

    private static string RewriteInterpolatedStrings(
        string value,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
    {
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '$' || index + 1 >= value.Length || value[index + 1] != '"')
            {
                builder.Append(value[index]);
                continue;
            }

            index += 2;
            var content = new StringBuilder();
            for (; index < value.Length; index++)
            {
                if (value[index] == '"' && value[index - 1] != '\\')
                {
                    break;
                }

                content.Append(value[index]);
            }

            builder.Append(ConvertInterpolatedStringContent(content.ToString(), loweringContext, shadowedNames));
        }

        return builder.ToString();
    }

    private static string ConvertInterpolatedStringContent(
        string content,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
    {
        var builder = new StringBuilder(content.Length + 4);
        builder.Append('`');
        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];
            if (character == '{')
            {
                if (index + 1 < content.Length && content[index + 1] == '{')
                {
                    builder.Append('{');
                    index++;
                    continue;
                }

                var endIndex = FindInterpolationEnd(content, index + 1);
                var expression = content[(index + 1)..endIndex];
                var expressionWithoutFormat = expression.Split(':')[0].Trim();
                builder.Append("${")
                    .Append(LowerExpression(expressionWithoutFormat, loweringContext, shadowedNames))
                    .Append('}');
                index = endIndex;
                continue;
            }

            if (character == '}' && index + 1 < content.Length && content[index + 1] == '}')
            {
                builder.Append('}');
                index++;
                continue;
            }

            if (character == '`')
            {
                builder.Append("\\`");
                continue;
            }

            builder.Append(character);
        }

        builder.Append('`');
        return builder.ToString();
    }

    private static int FindInterpolationEnd(string content, int startIndex)
    {
        var depth = 0;
        for (var index = startIndex; index < content.Length; index++)
        {
            switch (content[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    if (depth == 0)
                    {
                        return index;
                    }

                    depth--;
                    break;
            }
        }

        return content.Length - 1;
    }

    private static string RewriteIdentifiers(
        string value,
        LoweringContext loweringContext,
        ISet<string> shadowedNames)
    {
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length;)
        {
            var character = value[index];
            if (character is '"' or '\'' or '`')
            {
                AppendStringLiteral(builder, value, ref index, character);
                continue;
            }

            if (IsIdentifierStart(character))
            {
                var start = index;
                index++;
                while (index < value.Length && IsIdentifierPart(value[index]))
                {
                    index++;
                }

                var identifier = value[start..index];
                if (!IsMemberAccessIdentifier(value, start)
                    && !shadowedNames.Contains(identifier)
                    && loweringContext.Replacements.TryGetValue(identifier, out var replacement))
                {
                    builder.Append(replacement);
                }
                else
                {
                    builder.Append(identifier);
                }

                continue;
            }

            builder.Append(character);
            index++;
        }

        return builder.ToString();
    }

    private static bool IsMemberAccessIdentifier(string value, int startIndex)
    {
        for (var index = startIndex - 1; index >= 0; index--)
        {
            if (char.IsWhiteSpace(value[index]))
            {
                continue;
            }

            return value[index] == '.';
        }

        return false;
    }

    private static void AppendStringLiteral(StringBuilder builder, string value, ref int index, char delimiter)
    {
        builder.Append(delimiter);
        index++;
        while (index < value.Length)
        {
            builder.Append(value[index]);
            if (value[index] == delimiter && value[index - 1] != '\\')
            {
                index++;
                return;
            }

            index++;
        }
    }

    private static bool IsIdentifierStart(char character)
        => char.IsLetter(character) || character == '_';

    private static bool IsIdentifierPart(char character)
        => char.IsLetterOrDigit(character) || character == '_';

    private static bool IsSupportedLocalTypeToken(string typeToken)
        => !string.IsNullOrWhiteSpace(typeToken)
            && !string.Equals(typeToken, "return", StringComparison.Ordinal)
            && !string.Equals(typeToken, "if", StringComparison.Ordinal)
            && !string.Equals(typeToken, "else", StringComparison.Ordinal)
            && !string.Equals(typeToken, "for", StringComparison.Ordinal)
            && !string.Equals(typeToken, "foreach", StringComparison.Ordinal)
            && !string.Equals(typeToken, "while", StringComparison.Ordinal)
            && !string.Equals(typeToken, "switch", StringComparison.Ordinal)
            && !string.Equals(typeToken, "await", StringComparison.Ordinal);

    private static HashSet<string> GetVisibleShadowedNames(IEnumerable<HashSet<string>> scopeStack)
    {
        var shadowedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var scope in scopeStack)
        {
            shadowedNames.UnionWith(scope);
        }

        return shadowedNames;
    }

    private static void AddNamesToCurrentScope(Stack<HashSet<string>> scopeStack, IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            scopeStack.Peek().Add(name);
        }
    }

    private static int CountLeadingCharacters(string value, char character)
    {
        var count = 0;
        while (count < value.Length && value[count] == character)
        {
            count++;
        }

        return count;
    }

    private static int CountCharacters(string value, char character)
    {
        var count = 0;
        foreach (var currentCharacter in value)
        {
            if (currentCharacter == character)
            {
                count++;
            }
        }

        return count;
    }

    private static string NormalizeWhitespace(string value)
        => string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string MapVueType(string typeName)
        => typeName switch
        {
            "string" or "String" => "String",
            "bool" or "Boolean" => "Boolean",
            "int" or "long" or "short" or "float" or "double" or "decimal" or "byte" => "Number",
            _ => "null"
        };

    private sealed class PropDescriptor
    {
        public PropDescriptor(string sourceName, string runtimeName, string vueTypeExpression, int sourceLine)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            VueTypeExpression = vueTypeExpression;
            SourceLine = sourceLine;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string VueTypeExpression { get; }

        public int SourceLine { get; }
    }

    private sealed class StateDescriptor
    {
        public StateDescriptor(string sourceName, string runtimeName, string typeName, string? initializer, int sourceLine)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            TypeName = typeName;
            Initializer = initializer;
            SourceLine = sourceLine;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string TypeName { get; }

        public string? Initializer { get; }

        public int SourceLine { get; }
    }

    private sealed class ComputedDescriptor
    {
        public ComputedDescriptor(string sourceName, string runtimeName, string typeName, string expression, int sourceLine)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            TypeName = typeName;
            Expression = expression;
            SourceLine = sourceLine;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string TypeName { get; }

        public string Expression { get; }

        public int SourceLine { get; }
    }

    private sealed class MethodDescriptor
    {
        public MethodDescriptor(
            string sourceName,
            string runtimeName,
            string signature,
            IReadOnlyList<string> parameters,
            string body,
            bool isAsync,
            int sourceLine,
            int bodyStartLine)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            Signature = signature;
            Parameters = parameters;
            Body = body;
            IsAsync = isAsync;
            SourceLine = sourceLine;
            BodyStartLine = bodyStartLine;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string Signature { get; }

        public IReadOnlyList<string> Parameters { get; }

        public string Body { get; }

        public bool IsAsync { get; }

        public int SourceLine { get; }

        public int BodyStartLine { get; }
    }

    private sealed class LoweringContext
    {
        private LoweringContext(IReadOnlyDictionary<string, string> replacements)
        {
            Replacements = replacements;
        }

        public IReadOnlyDictionary<string, string> Replacements { get; }

        public static LoweringContext Create(
            IReadOnlyList<PropDescriptor> props,
            IReadOnlyList<StateDescriptor> states,
            IReadOnlyList<ComputedDescriptor> computeds,
            IReadOnlyList<MethodDescriptor> methods)
        {
            var replacements = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var prop in props)
            {
                replacements[prop.SourceName] = prop.RuntimeName + ".value";
                replacements[prop.RuntimeName] = prop.RuntimeName + ".value";
            }

            foreach (var state in states)
            {
                replacements[state.SourceName] = state.RuntimeName + ".value";
                replacements[state.RuntimeName] = state.RuntimeName + ".value";
            }

            foreach (var computed in computeds)
            {
                replacements[computed.SourceName] = computed.RuntimeName + ".value";
                replacements[computed.RuntimeName] = computed.RuntimeName + ".value";
            }

            foreach (var method in methods)
            {
                replacements[method.SourceName] = method.RuntimeName;
            }

            return new LoweringContext(replacements);
        }
    }

    private enum DeclaredNameLifetime
    {
        None,
        CurrentScope,
        NextBlockScope
    }

    private readonly record struct LoweredStatement(
        string Line,
        string[] DeclaredNames,
        DeclaredNameLifetime DeclaredNameLifetime);

    private readonly record struct GeneratedVueLine(
        string Text,
        int? SourceLine);

    private readonly record struct LoweredMethodLine(
        string Line,
        int SourceLine);
}
