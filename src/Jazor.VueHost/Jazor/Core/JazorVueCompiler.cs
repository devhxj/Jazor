using System.Text;
using System.Text.RegularExpressions;

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
        @"public\s+(?<async>async\s+)?(?:[\w\.\?\<\>\[\]]+)\s+(?<name>\w+)\s*\((?<parameters>[^\)]*)\)\s*\{",
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
        var props = ExtractProps(document.Code);
        var states = ExtractStates(document.Code);
        var computeds = ExtractComputeds(document.Code);
        var methods = ExtractMethods(document.Code);
        var loweringContext = LoweringContext.Create(props, states, computeds, methods);
        var builder = new StringBuilder();

        builder.AppendLine("<script setup>");
        var vueHelpers = GetVueHelpers(props, states, computeds);
        if (vueHelpers.Count > 0)
        {
            builder.Append("import { ")
                .Append(string.Join(", ", vueHelpers))
                .AppendLine(" } from \"vue\";");
        }

        foreach (var importStatement in imports)
            builder.AppendLine(importStatement);

        if (props.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("const props = defineProps({");
            foreach (var prop in props)
            {
                builder.Append("  ")
                    .Append(prop.RuntimeName)
                    .Append(": ")
                    .Append(prop.VueTypeExpression)
                    .AppendLine(",");
            }

            builder.AppendLine("});");

            foreach (var prop in props)
            {
                builder.Append("const ")
                    .Append(prop.RuntimeName)
                    .Append(" = toRef(props, \"")
                    .Append(prop.RuntimeName)
                    .AppendLine("\");");
            }
        }

        if (states.Count > 0)
        {
            builder.AppendLine();
            foreach (var state in states)
            {
                builder.Append("const ")
                    .Append(state.RuntimeName)
                    .Append(" = ref(")
                    .Append(LowerExpression(state.Initializer ?? "undefined", loweringContext, EmptyShadowedNames))
                    .AppendLine(");");
            }
        }

        if (computeds.Count > 0)
        {
            builder.AppendLine();
            foreach (var computed in computeds)
            {
                if (TryLowerComputed(computed, loweringContext, out var loweredExpression))
                {
                    builder.Append("const ")
                        .Append(computed.RuntimeName)
                        .Append(" = computed(() => ")
                        .Append(loweredExpression)
                        .AppendLine(");");
                    continue;
                }

                diagnostics.Add($"Computed member '{computed.SourceName}' could not be lowered by the local fallback compiler.");
                builder.Append("const ")
                    .Append(computed.RuntimeName)
                    .Append(" = computed(() => {")
                    .AppendLine()
                    .Append("  // Fallback compiler could not lower computed member ")
                    .Append(computed.SourceName)
                    .AppendLine(".")
                    .AppendLine("  return undefined;")
                    .AppendLine("});");
            }
        }

        if (methods.Count > 0)
        {
            builder.AppendLine();
            foreach (var method in methods)
            {
                builder.Append(method.IsAsync ? "async function " : "function ")
                    .Append(method.RuntimeName)
                    .Append("(")
                    .Append(string.Join(", ", method.Parameters))
                    .AppendLine(") {");

                if (TryLowerMethodBody(method, loweringContext, out var loweredBody))
                {
                    foreach (var line in loweredBody)
                    {
                        builder.Append("  ")
                            .AppendLine(line);
                    }
                }
                else
                {
                    diagnostics.Add($"Method '{method.SourceName}' could not be lowered by the local fallback compiler.");
                    builder.Append("  // Fallback compiler could not lower method ")
                        .Append(method.SourceName)
                        .AppendLine(".");
                }

                builder.AppendLine("}");
            }
        }

        if (!string.IsNullOrWhiteSpace(document.Code))
        {
            builder.AppendLine();
            builder.AppendLine("/*");
            builder.AppendLine(" Original @code block retained for bridge diagnostics:");
            builder.AppendLine(document.Code.Replace("*/", "* /"));
            builder.AppendLine("*/");
        }

        builder.AppendLine("</script>");
        builder.AppendLine();
        builder.AppendLine("<template>");
        builder.AppendLine(string.IsNullOrWhiteSpace(document.Template) ? "<div />" : document.Template);
        builder.AppendLine("</template>");

        if (methods.Count == 0 && document.Code.Length > 0)
            diagnostics.Add("No public methods were lowered. The current bridge compiler emits method stubs only for public instance methods.");

        return new JazorVueCompilationResult(document, externalSymbols, builder.ToString(), generatedExternalDeclarationsText, diagnostics);
    }

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

    private static IReadOnlyList<PropDescriptor> ExtractProps(string code)
    {
        var results = new List<PropDescriptor>();
        foreach (Match match in PropPattern.Matches(code))
        {
            var typeName = match.Groups["type"].Value.Trim();
            var sourceName = match.Groups["name"].Value.Trim();
            results.Add(new PropDescriptor(sourceName, JazorVueNaming.ToCamelCase(sourceName), MapVueType(typeName)));
        }

        return results;
    }

    private static IReadOnlyList<StateDescriptor> ExtractStates(string code)
    {
        var results = new List<StateDescriptor>();
        foreach (Match match in StatePattern.Matches(code))
        {
            var sourceName = match.Groups["name"].Value.Trim();
            var initializer = match.Groups["initializer"].Success
                ? match.Groups["initializer"].Value.Trim()
                : null;
            results.Add(new StateDescriptor(sourceName, JazorVueNaming.ToCamelCase(sourceName), initializer));
        }

        return results;
    }

    private static IReadOnlyList<ComputedDescriptor> ExtractComputeds(string code)
    {
        var results = new List<ComputedDescriptor>();
        foreach (Match match in ComputedPattern.Matches(code))
        {
            var sourceName = match.Groups["name"].Value.Trim();
            var expression = match.Groups["expression"].Value.Trim();
            results.Add(new ComputedDescriptor(sourceName, JazorVueNaming.ToCamelCase(sourceName), expression));
        }

        return results;
    }

    private static IReadOnlyList<MethodDescriptor> ExtractMethods(string code)
    {
        var results = new List<MethodDescriptor>();
        foreach (Match match in MethodPattern.Matches(code))
        {
            var sourceName = match.Groups["name"].Value.Trim();
            var parameterBlock = match.Groups["parameters"].Value.Trim();
            var bodyStart = match.Index + match.Length;
            var body = ExtractBlockBody(code, bodyStart, out _);
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
                parameters,
                body,
                match.Groups["async"].Success));
        }

        return results;
    }

    private static string ExtractBlockBody(string code, int bodyStart, out int nextIndex)
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
                        nextIndex = index + 1;
                        return code[bodyStart..index].Trim();
                    }

                    break;
            }
        }

        nextIndex = code.Length;
        return code[bodyStart..].Trim();
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
        out IReadOnlyList<string> loweredLines)
    {
        if (string.IsNullOrWhiteSpace(method.Body))
        {
            loweredLines = Array.Empty<string>();
            return true;
        }

        var scopeStack = new Stack<HashSet<string>>();
        scopeStack.Push(new HashSet<string>(method.Parameters, StringComparer.Ordinal));
        string[] pendingBlockScopedNames = [];
        var results = new List<string>();
        foreach (var rawLine in method.Body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                results.Add(string.Empty);
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
            results.Add(loweredStatement.Line);

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
        public PropDescriptor(string sourceName, string runtimeName, string vueTypeExpression)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            VueTypeExpression = vueTypeExpression;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string VueTypeExpression { get; }
    }

    private sealed class StateDescriptor
    {
        public StateDescriptor(string sourceName, string runtimeName, string? initializer)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            Initializer = initializer;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string? Initializer { get; }
    }

    private sealed class ComputedDescriptor
    {
        public ComputedDescriptor(string sourceName, string runtimeName, string expression)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            Expression = expression;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public string Expression { get; }
    }

    private sealed class MethodDescriptor
    {
        public MethodDescriptor(
            string sourceName,
            string runtimeName,
            IReadOnlyList<string> parameters,
            string body,
            bool isAsync)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            Parameters = parameters;
            Body = body;
            IsAsync = isAsync;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public IReadOnlyList<string> Parameters { get; }

        public string Body { get; }

        public bool IsAsync { get; }
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
}
