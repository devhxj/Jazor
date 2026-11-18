using System.Text.Json;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jolt.Compiler;

internal sealed class VueLowering(Projection projection, ComponentContract contract) : SemanticWalkerHost
{
    private readonly Dictionary<ComponentContract, string> _aliases = projection.Dependencies
        .Select((component, index) => (component, alias: "$joltComponent" + index))
        .ToDictionary(pair => pair.component, pair => pair.alias);

    public override Expression? RewritePropertyReference(
        IPropertyReferenceOperation operation, SenseArgument argument, Expression? instance, IReadOnlyList<Expression> arguments)
    {
        var property = operation.Property;
        if (instance is null)
            return null;
        if (property.ContainingType.Name.StartsWith("__JoltScope", StringComparison.Ordinal))
            return Member(instance, property.Name);
        if (property.ContainingType.Name != projection.StateName || !contract.Properties.Any(candidate => candidate.Name == property.Name))
            return null;
        // Vue keeps the props object live for the instance lifetime. A missing value falls
        // back to the C# initializer, while an explicitly supplied null remains observable.
        var props = Member(instance, "$joltProps");
        var read = new ChainExpression(new MemberExpression(props, String(property.Name), computed: true, optional: true));
        var fallback = Member(instance, Util.GetConfigOrSymbolName(property));
        return new ConditionalExpression(
            new NonLogicalBinaryExpression(Operator.StrictInequality, read, Id("undefined")),
            read,
            fallback);
    }

    public override Expression? RewriteInvocationIntrinsic(
        IInvocationOperation operation, Expression? instance, IReadOnlyList<Expression> arguments, SemanticInvocationLoweringContext context)
    {
        if (operation.TargetMethod.ContainingType.Name != projection.StateName ||
            !projection.Intrinsics.TryGetValue(operation.TargetMethod.Name, out var intrinsic))
            return null;

        Expression expression = intrinsic.Kind switch
        {
            IntrinsicKind.Array => Array(),
            IntrinsicKind.Append => Call(Member(arguments[0], "push"), arguments[1]),
            IntrinsicKind.Text => Call(Id("$joltDisplay"), arguments[0]),
            IntrinsicKind.Element => Call(Id("$joltH"), intrinsic.Component is null
                ? String(intrinsic.Name!)
                : Id(_aliases[intrinsic.Component]),
                Properties(arguments[0]), intrinsic.Component is null ? arguments[1] : Properties(arguments[1])),
            IntrinsicKind.Prop or IntrinsicKind.Slot => Array(String(intrinsic.Name!), arguments[0]),
            IntrinsicKind.Outlet => Call(Id("$joltRenderSlot"), Member(arguments[0], "$joltSlots"), String(intrinsic.Name!),
                Properties(arguments[1]), arguments[2]),
            _ => throw new InvalidOperationException($"Unknown Jolt intrinsic {intrinsic.Kind}."),
        };
        if (intrinsic.Kind != IntrinsicKind.Array)
        {
            var positions = projection.Source.Lines.GetLinePositionSpan(intrinsic.Span);
            expression.UserData = new SourceOrigin(projection.Document.Path, positions.Start.Line, positions.Start.Character,
                positions.End.Line, positions.End.Character, intrinsic.Name);
        }
        return expression;
    }

    internal JoltModule Emit(Module compilerModule, string renderName)
    {
        string path = ModulePath(contract.Path);
        var statements = new List<Statement>();
        statements.AddRange(ImportDeclarationFactory.Create("vue",
        [
            Import("defineComponent", "$joltDefine"),
            Import("h", "$joltH"),
            Import("reactive", "$joltReactive"),
            Import("renderSlot", "$joltRenderSlot"),
            Import("toDisplayString", "$joltDisplay"),
        ]));
        foreach (var (component, alias) in _aliases)
        {
            if (component.Npm is { } npm)
                statements.AddRange(ImportDeclarationFactory.Create(npm.ModuleSpecifier,
                    [npm.ExportName == "default" ? new ImportDefaultSpecifier(Id(alias)) : Import(npm.ExportName, alias)]));
            else if (component.Path != contract.Path)
            {
                var directory = Path.GetDirectoryName(path) ?? "";
                var specifier = Path.GetRelativePath(Path.GetFullPath(directory.Length == 0 ? "." : directory),
                    Path.GetFullPath(ModulePath(component.Path))).Replace('\\', '/');
                if (!specifier.StartsWith("../", StringComparison.Ordinal))
                    specifier = "./" + specifier;
                statements.AddRange(ImportDeclarationFactory.Create(specifier, [new ImportDefaultSpecifier(Id(alias))]));
            }
        }
        RemapOrigins(compilerModule);
        var clrImports = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var statement in compilerModule.Body)
        {
            if (statement is ImportDeclaration import && import.Source.Value.StartsWith("./clr/", StringComparison.Ordinal))
            {
                // Compiler CLR imports are root-relative in the output layout. Keep one
                // runtime copy shared by components in every directory.
                clrImports.Add(import.Source.Value[2..]);
                var depth = path.Count(character => character == '/');
                var specifier = depth == 0 ? import.Source.Value : string.Concat(Enumerable.Repeat("../", depth)) + import.Source.Value[2..];
                statements.Add(ImportDeclarationFactory.WithModulePath(import, specifier));
            }
            else
            {
                statements.Add(statement);
            }
        }
        var state = Id("$joltState");
        var stateClass = compilerModule.Body.OfType<ClassDeclaration>()
            .Concat(compilerModule.Body.OfType<ExportNamedDeclaration>().Select(declaration => declaration.Declaration).OfType<ClassDeclaration>())
            .Single(declaration => declaration.Id?.Name == projection.StateName);
        var setup = new ArrowFunctionExpression(
            NodeList.From<Node>(Id("$joltProps"), Id("$joltContext")),
            new FunctionBody(NodeList.From<Statement>(
                new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(new VariableDeclarator(state,
                    Call(Id("$joltReactive"), new NewExpression(Id(stateClass.Id!.Name), NodeList.Empty<Expression>()))))),
                Assign(Member(state, "$joltProps"), Id("$joltProps")),
                Assign(Member(state, "$joltSlots"), Member(Id("$joltContext"), "slots")),
                new ReturnStatement(new ArrowFunctionExpression(NodeList.Empty<Node>(), Call(Member(state, renderName)), expression: true, async: false))), strict: true),
            expression: false, async: false);

        var definition = Call(Id("$joltDefine"), Object(
            Property("name", String(contract.Name)),
            Property("props", Array(contract.Properties.Select(property => (Expression)String(property.Name)).ToArray())),
            Property("setup", setup)));
        var self = _aliases.FirstOrDefault(pair => pair.Key.Npm is null && pair.Key.Path == contract.Path).Value;
        if (self is null)
            statements.Add(new ExportDefaultDeclaration(definition));
        else
        {
            statements.Add(new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(new VariableDeclarator(Id(self), definition))));
            statements.Add(new ExportDefaultDeclaration(Id(self)));
        }

        var artifact = new Module(NodeList.From(statements)).ToKnRECMAScriptWithSourceMap(path,
            readSourceContent: _ => projection.Document.Source);
        return new JoltModule(contract.Name, path, artifact.Content + "\n//# sourceMappingURL=" + Path.GetFileName(path) + ".map\n", artifact.SourceMapContent!)
        {
            ClrImports = clrImports.Count == 0 ? [] : clrImports.ToArray(),
        };
    }

    private static string ModulePath(string source) => Path.ChangeExtension(source.Replace('\\', '/'), ".mjs");

    private void RemapOrigins(Node node)
    {
        if (node.UserData is SourceOrigin origin && origin.SourcePath == projection.Document.Path + ".g.cs" &&
            projection.GeneratedText is { } generated && origin.StartLine < generated.Lines.Count)
        {
            var offset = generated.Lines[origin.StartLine].Start + origin.StartColumn;
            if (projection.Map(new Microsoft.CodeAnalysis.Text.TextSpan(offset, 0)) is { } authored)
            {
                var position = projection.Source.Lines.GetLinePosition(authored.Start);
                node.UserData = origin with { SourcePath = projection.Document.Path, StartLine = position.Line,
                    StartColumn = position.Character, EndLine = position.Line, EndColumn = position.Character };
            }
            else
                node.UserData = null;
        }
        foreach (var child in node.ChildNodes)
            RemapOrigins(child);
    }

    private static Expression Properties(Expression entries)
    {
        if (entries is ArrayExpression array && array.Elements.All(member => member is ArrayExpression))
        {
            return Object(array.Elements.Cast<ArrayExpression>()
                .Select(entry => Property(((StringLiteral)entry.Elements[0]!).Value, entry.Elements[1]!)).ToArray());
        }
        return Call(Member(Id("Object"), "fromEntries"), entries);
    }

    private static ImportSpecifier Import(string name, string alias) => new(Id(name), Id(alias));
    private static Identifier Id(string name) => new(name);
    private static StringLiteral String(string text) => new(text, JsonSerializer.Serialize(text));
    private static MemberExpression Member(Expression instance, string name) => new(instance, String(name), computed: true, optional: false);
    private static CallExpression Call(Expression callee, params Expression[] arguments) => new(callee, NodeList.From(arguments), optional: false);
    private static ArrayExpression Array(params Expression[] entries) => new(NodeList.From<Expression?>(entries));
    private static ObjectExpression Object(params ObjectProperty[] properties) => new(NodeList.From<Node>(properties));
    private static ObjectProperty Property(string name, Expression value) => new(PropertyKind.Init, String(name), value,
        computed: name == "__proto__", shorthand: false, method: false);
    private static Statement Assign(Expression left, Expression right)
        => new NonSpecialExpressionStatement(new AssignmentExpression(Operator.Assignment, left, right));
}
