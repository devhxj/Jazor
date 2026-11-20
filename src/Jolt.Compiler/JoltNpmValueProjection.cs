using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jolt.Compiler;

internal sealed class JoltNpmValueProjection
{
    private const string FilePath = "__JoltNpmValues.g.cs";
    private const string Runtime = "[global::ECMAScript.ECMAScript]";
    private readonly StringBuilder _code = new("#nullable enable\n");
    private readonly Dictionary<int, (NpmValueExport? Export, NpmDefinition? Definition)> _declarations = [];

    internal JoltNpmValueProjection(IEnumerable<string> namespaces, IReadOnlyList<NpmValueExport> exports, CSharpParseOptions parseOptions)
    {
        var modules = exports.GroupBy(export => NpmNames.NamespaceFor(export.ModuleSpecifier))
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        foreach (var ns in namespaces.Concat(modules.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            _code.AppendLine($"namespace {ns} {{ {Runtime} public static class Exports {{");
            var shapes = new ShapeDeclarations();
            foreach (var export in (modules.GetValueOrDefault(ns) ?? []).OrderBy(value => value.ExportName, StringComparer.Ordinal))
            {
                var name = NpmNames.ValueNameFor(export.ExportName);
                if (export.ConstantType is { } constant)
                    Declare($"public static extern {Type(constant, name + "Result", shapes)} {Identifier(name)} {{ get; }}", export, export.Definition);
                foreach (var signature in export.Signatures)
                {
                    var result = Type(signature.ReturnType, name + "Result", shapes);
                    var parameters = Parameters(signature, name, shapes);
                    Declare($"public static extern {result} {Identifier(name)}({parameters});", export, signature.Definition ?? export.Definition);
                }
            }

            while (shapes.TryDequeue(out var declaration))
            {
                var (name, shape, input) = declaration;
                if (shape.Kind == "function")
                {
                    var signature = shape.Signatures!.Single();
                    _code.AppendLine($"{Runtime} public delegate {Type(signature.ReturnType, name + "Result", shapes)} {name}({Parameters(signature, name, shapes)});");
                    continue;
                }

                _code.AppendLine(input ? $"public sealed record {name} {{" : $"{Runtime} public sealed class {name} {{ private {name}() {{ }}");
                foreach (var member in shape.Members!)
                {
                    var property = NpmNames.ValueNameFor(member.Name);
                    var type = Type(member.Type, name + property + (input ? "Input" : "Member"), shapes);
                    if (member.Optional)
                        type += "?";
                    var accessors = input ? "get; init;" : member.ReadOnly ? "get;" : "get; set;";
                    var modifier = input ? member.Optional ? "" : "required " : "extern ";
                    var attribute = $"[global::System.ComponentModel.Description({SymbolDisplay.FormatLiteral("@#" + member.Name, true)})]";
                    _code.AppendLine(attribute);
                    Declare($"public {modifier}{type} {Identifier(property)} {{ {accessors} }}", null, member.Definition);
                }
                _code.AppendLine("}");
            }
            _code.AppendLine("} }");
        }
        Tree = CSharpSyntaxTree.ParseText(_code.ToString(), parseOptions, FilePath);
    }

    internal SyntaxTree Tree { get; }

    internal JoltNpmValueLowering Bind(CSharpCompilation compilation)
    {
        var model = compilation.GetSemanticModel(Tree);
        var bindings = new Dictionary<ISymbol, NpmValueExport>(SymbolEqualityComparer.Default);
        var definitions = new Dictionary<ISymbol, NpmDefinition>(SymbolEqualityComparer.Default);
        foreach (var member in Tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            // Property attributes are emitted on the preceding line. Bind the declaration
            // from its first modifier so metadata stays independent of Roslyn display strings.
            int start = member switch
            {
                MethodDeclarationSyntax method => method.Modifiers[0].SpanStart,
                PropertyDeclarationSyntax property => property.Modifiers[0].SpanStart,
                _ => -1,
            };
            if (!_declarations.TryGetValue(start, out var metadata) || model.GetDeclaredSymbol(member) is not { } symbol)
                continue;
            if (metadata.Export is { } export)
                bindings.Add(symbol, export);
            if (metadata.Definition is { } definition)
                definitions.Add(symbol, definition);
        }
        return new(bindings, definitions);
    }

    internal static bool IsProjected(ITypeSymbol type) =>
        type.DeclaringSyntaxReferences.Any(reference => reference.SyntaxTree.FilePath == FilePath);

    internal static bool ContainsVueRef(ITypeSymbol type)
    {
        static bool Matches(ITypeSymbol candidate, string name) =>
            candidate is INamedTypeSymbol { Arity: 1, ContainingType: { Name: "Vue" } parent } named &&
            parent.ContainingNamespace.ToDisplayString() == "ECMAScript" && named.Name == name;

        if (Matches(type, "IVueRef") || type.AllInterfaces.Any(candidate => Matches(candidate, "IVueRef")))
            return true;
        for (ITypeSymbol? candidate = type; candidate is not null; candidate = candidate.BaseType)
            if (Matches(candidate, "VueReadonlyRef"))
                return true;
        return IsProjected(type) && type.GetMembers().OfType<IPropertySymbol>().Any(property => ContainsVueRef(property.Type));
    }

    private void Declare(string declaration, NpmValueExport? export, NpmDefinition? definition)
    {
        _declarations.Add(_code.Length, (export, definition));
        _code.AppendLine(declaration);
    }

    private static string Parameters(NpmValueSignature signature, string name, ShapeDeclarations shapes) =>
        string.Join(", ", signature.Parameters.Select(parameter =>
            $"{Type(parameter.Type, name + NpmNames.ValueNameFor(parameter.Name) + "Input", shapes)} {Identifier(parameter.Name)}{(parameter.Optional ? " = default" : "")}"));

    private static string Type(NpmValueType type, string name, ShapeDeclarations shapes)
    {
        switch (type.Kind)
        {
            case "string": return "string";
            case "number": return "double";
            case "boolean": return "bool";
            case "void": return "void";
            case "ref":
                return $"global::ECMAScript.Vue.{(type.ReadOnly ? "VueReadonlyRef" : "IVueRef")}<{Type(type.ElementType!, name, shapes)}>";
            case "object":
            case "function":
                return shapes.Add(name, type);
            default:
                throw new InvalidOperationException($"Unsupported npm type projection '{type.Kind}'.");
        }
    }

    private static string Identifier(string name) => "@" + name;

    private sealed class ShapeDeclarations
    {
        private readonly Dictionary<string, NpmValueType> _names = new(StringComparer.Ordinal);
        private readonly Queue<(string Name, NpmValueType Type, bool Input)> _pending = new();

        public string Add(string name, NpmValueType type)
        {
            var candidate = name;
            for (int suffix = 2; _names.TryGetValue(candidate, out var previous); suffix++)
            {
                // Several overloads may use the same options parameter, but its TypeScript
                // shape may differ. Reuse the C# type only for equivalent projections.
                if (SameType(previous, type))
                    return candidate;
                candidate = name + "_" + suffix;
            }
            _names.Add(candidate, type);
            _pending.Enqueue((candidate, type, name.EndsWith("Input", StringComparison.Ordinal)));
            return candidate;
        }

        public bool TryDequeue(out (string Name, NpmValueType Type, bool Input) declaration) => _pending.TryDequeue(out declaration);

        private static bool SameType(NpmValueType first, NpmValueType second)
        {
            if (first.Kind != second.Kind || first.ReadOnly != second.ReadOnly)
                return false;
            return first.Kind switch
            {
                "ref" => SameType(first.ElementType!, second.ElementType!),
                "object" => SameSequence(first.Members!, second.Members!, SameMember),
                "function" => SameSequence(first.Signatures!, second.Signatures!, SameSignature),
                _ => true,
            };
        }

        private static bool SameMember(NpmValueMember first, NpmValueMember second) =>
            first.Name == second.Name && first.Optional == second.Optional && first.ReadOnly == second.ReadOnly &&
            SameType(first.Type, second.Type);

        private static bool SameSignature(NpmValueSignature first, NpmValueSignature second) =>
            SameType(first.ReturnType, second.ReturnType) && SameSequence(first.Parameters, second.Parameters, SameParameter);

        private static bool SameParameter(NpmValueParameter first, NpmValueParameter second) =>
            first.Name == second.Name && first.Optional == second.Optional && SameType(first.Type, second.Type);

        private static bool SameSequence<T>(IReadOnlyList<T> first, IReadOnlyList<T> second, Func<T, T, bool> same) =>
            first.Count == second.Count && first.Zip(second, same).All(equal => equal);
    }
}

internal sealed class JoltNpmValueLowering(
    IReadOnlyDictionary<ISymbol, NpmValueExport> exports,
    IReadOnlyDictionary<ISymbol, NpmDefinition> definitions) : SemanticWalkerHost
{
    internal IReadOnlyDictionary<ISymbol, NpmDefinition> Definitions => definitions;

    public override Expression? RewriteInvocationArgumentPreorder(
        IInvocationOperation operation, IArgumentOperation argumentOperation, int argumentIndex, SenseArgument argument)
    {
        // Core omits trailing ECMAScript defaults and retains gaps when a later named
        // parameter is present. JS interprets the retained gap as the package's default.
        return argumentOperation.ArgumentKind == ArgumentKind.DefaultValue &&
            (exports.ContainsKey(operation.TargetMethod.OriginalDefinition) ||
             operation.TargetMethod.MethodKind == MethodKind.DelegateInvoke &&
             JoltNpmValueProjection.IsProjected(operation.TargetMethod.ContainingType))
            ? new NonUpdateUnaryExpression(Operator.Void, new NumericLiteral(0, "0"))
            : null;
    }

    public override Expression? RewriteInvocation(IInvocationOperation operation, SenseArgument argument, Expression? instance, IReadOnlyList<Expression> arguments) =>
        exports.TryGetValue(operation.TargetMethod.OriginalDefinition, out var export)
            ? new CallExpression(Import(export, argument), NodeList.From(arguments), optional: false)
            : null;

    public override Expression? RewriteMethodReferencePreorder(IMethodReferenceOperation operation, SenseArgument argument) =>
        exports.TryGetValue(operation.Method.OriginalDefinition, out var export) ? Import(export, argument) : null;

    public override Expression? RewritePropertyReference(IPropertyReferenceOperation operation, SenseArgument argument, Expression? instance, IReadOnlyList<Expression> arguments) =>
        exports.TryGetValue(operation.Property.OriginalDefinition, out var export) ? Import(export, argument) : null;

    private static Identifier Import(NpmValueExport export, SenseArgument argument) =>
        argument.BindExternalImportSpecifier(export.ModuleSpecifier, export.ExportName, forceAlias: true);
}
