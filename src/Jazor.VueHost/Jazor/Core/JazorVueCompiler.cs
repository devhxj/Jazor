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
        @"\[Computed\]\s*public\s+(?<type>[\w\.\?\<\>]+)\s+(?<name>\w+)\s*=>",
        RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex MethodPattern = new Regex(
        @"public\s+(?:async\s+)?(?:[\w\.\?\<\>]+)\s+(?<name>\w+)\s*\((?<parameters>[^\)]*)\)",
        RegexOptions.Multiline | RegexOptions.Compiled);

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
        var builder = new StringBuilder();

        builder.AppendLine("<script setup>");
        if (computeds.Count > 0)
            builder.AppendLine("import { computed } from \"vue\";");

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
        }

        if (states.Count > 0)
        {
            builder.AppendLine();
            foreach (var state in states)
            {
                builder.Append("let ")
                    .Append(state.RuntimeName)
                    .Append(" = ")
                    .Append(state.Initializer ?? "undefined")
                    .AppendLine(";");
            }
        }

        if (computeds.Count > 0)
        {
            builder.AppendLine();
            foreach (var computed in computeds)
            {
                builder.Append("const ")
                    .Append(computed.RuntimeName)
                    .Append(" = computed(() => {")
                    .AppendLine()
                    .Append("  // TODO: lower C# computed member ")
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
                builder.Append("function ")
                    .Append(method.RuntimeName)
                    .Append("(")
                    .Append(string.Join(", ", method.Parameters))
                    .AppendLine(") {")
                    .Append("  // TODO: lower C# method ")
                    .Append(method.SourceName)
                    .AppendLine(".")
                    .AppendLine("}");
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
            results.Add(new ComputedDescriptor(sourceName, JazorVueNaming.ToCamelCase(sourceName)));
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
            var parameters = string.IsNullOrWhiteSpace(parameterBlock)
                ? Array.Empty<string>()
                : parameterBlock.Split(',')
                    .Select(static parameter => parameter.Trim())
                    .Where(static parameter => parameter.Length > 0)
                    .Select(static parameter =>
                    {
                        var parts = parameter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        return parts[parts.Length - 1];
                    })
                    .ToArray();
            results.Add(new MethodDescriptor(sourceName, JazorVueNaming.ToCamelCase(sourceName), parameters));
        }

        return results;
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
        public ComputedDescriptor(string sourceName, string runtimeName)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }
    }

    private sealed class MethodDescriptor
    {
        public MethodDescriptor(string sourceName, string runtimeName, IReadOnlyList<string> parameters)
        {
            SourceName = sourceName;
            RuntimeName = runtimeName;
            Parameters = parameters;
        }

        public string SourceName { get; }

        public string RuntimeName { get; }

        public IReadOnlyList<string> Parameters { get; }
    }
}
