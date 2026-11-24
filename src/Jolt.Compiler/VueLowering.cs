using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Acornima;
using Acornima.Ast;
using Jazor.Compiler;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jolt.Compiler;

internal sealed partial class VueLowering(Projection projection, ComponentContract contract) : SemanticWalkerHost
{
    internal string VueSourceRootRelative { get; init; } = ".";
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
        return ReadLocalProp(instance, props, property.Name, read, fallback);
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
            IntrinsicKind.Text => operation.Arguments[0].Value is IConversionOperation { Operand.Type: { Name: "RenderedFragment" } type }
                && type.ContainingNamespace.ToDisplayString() == "JoltAuthoring"
                ? arguments[0] : Call(Id("$joltDisplay"), arguments[0]),
            IntrinsicKind.Fragment => arguments[0],
            IntrinsicKind.Element => LowerElement(intrinsic, arguments),
            IntrinsicKind.ConditionalBranch => Call(Id("$joltH"), Id("$joltFragment"),
                Object(Property("key", String(intrinsic.Name!))), arguments[0]),
            IntrinsicKind.Bind => LowerNativeBinding(intrinsic.Binding!, arguments),
            IntrinsicKind.ModelUpdate => LowerModelUpdate(arguments),
            IntrinsicKind.Attributes => Array(arguments[0]),
            IntrinsicKind.DomEvent => LowerDomEvent(intrinsic.Name!, operation, arguments),
            IntrinsicKind.Ref => LowerReference(intrinsic.Name!, arguments),
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

    internal JoltModule Emit(Module compilerModule, INamedTypeSymbol moduleSymbol, string renderName, IReadOnlyList<IMethodSymbol> exposed,
        IMethodSymbol? setupMethod)
    {
        string path = ModulePath(contract.Path);
        var statements = new List<Statement>();
        var styles = projection.Document.Nodes.OfType<JoltScopedStyle>().ToArray();
        var scope = styles.Any(style => style.Scoped) ? ScopedStyleId(path) : null;
        if (styles.Length != 0)
            statements.Add(ImportDeclarationFactory.CreateSideEffect("./" + Path.GetFileNameWithoutExtension(path) + ".jolt.css"));
        var vueImports = new List<ImportSpecifier>
        {
            Import("defineComponent", "$joltDefine"),
            Import("h", "$joltH"),
            Import("reactive", "$joltReactive"),
            Import("renderSlot", "$joltRenderSlot"),
            Import("toDisplayString", "$joltDisplay"),
        };
        AddModelImports(vueImports);
        if (_usesShallowState)
            vueImports.Add(Import("shallowReactive", "$joltShallowReactive"));
        if (projection.Intrinsics.Values.Any(intrinsic => intrinsic.Kind == IntrinsicKind.ConditionalBranch))
            vueImports.Add(Import("Fragment", "$joltFragment"));
        var hasDomRefs = projection.Intrinsics.Values.Any(intrinsic => intrinsic.Kind == IntrinsicKind.Ref);
        if (hasDomRefs)
            vueImports.Add(Import("customRef", "$joltCustomRef"));
        if (hasDomRefs || _usesShallowState)
            vueImports.Add(Import("toRaw", "$joltToRaw"));
        if (contract.LocalModels.Count > 0 || contract.VuePropertyAliases.Count > 0)
            vueImports.Add(Import("getCurrentInstance", "$joltGetCurrentInstance"));
        if (projection.Intrinsics.Values.Any(intrinsic => intrinsic.Kind == IntrinsicKind.Attributes))
            vueImports.Add(Import("mergeProps", "$joltMergeProps"));
        foreach (var (component, alias) in _aliases.Where(pair => pair.Key.IsVueBuiltIn))
            vueImports.Add(Import(component.Name, alias));
        statements.AddRange(ImportDeclarationFactory.Create("vue", vueImports));
        foreach (var (component, alias) in _aliases)
        {
            if (component.IsVueBuiltIn)
                continue;
            if (component.Npm is { LocalPath: null } npm)
                statements.AddRange(ImportDeclarationFactory.Create(npm.ModuleSpecifier,
                    [npm.ExportName == "default" ? new ImportDefaultSpecifier(Id(alias)) : Import(npm.ExportName, alias)]));
            else if (component.Path != contract.Path)
            {
                var directory = Path.GetDirectoryName(path) ?? "";
                var target = component.Npm?.LocalPath is { } vuePath
                    ? Path.Combine(VueSourceRootRelative, vuePath)
                    : ModulePath(component.Path);
                var specifier = Path.GetRelativePath(Path.GetFullPath(directory.Length == 0 ? "." : directory),
                    Path.GetFullPath(target)).Replace('\\', '/');
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
            .Single(declaration => declaration.Id?.Name == projection.StateName);
        var context = Id("$joltContext");
        var setupStatements = new List<Statement>();
        setupStatements.Add(new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(new VariableDeclarator(state,
            Call(Id("$joltReactive"), new NewExpression(Id(stateClass.Id!.Name), NodeList.Empty<Expression>()))))));
        setupStatements.Add(Assign(Member(state, "$joltProps"), Id("$joltProps")));
        setupStatements.Add(Assign(Member(state, "$joltSlots"), Member(context, "slots")));
        if (contract.LocalModels.Count > 0 || contract.VuePropertyAliases.Count > 0)
        {
            var vueInstance = Id("$joltVueInstance");
            setupStatements.Add(new VariableDeclaration(VariableDeclarationKind.Const, NodeList.From(
                new VariableDeclarator(vueInstance, Call(Id("$joltGetCurrentInstance"))))));
            setupStatements.Add(Assign(Member(state, "$joltReadVNodeProps"),
                new ArrowFunctionExpression(NodeList.Empty<Node>(), Member(Member(vueInstance, "vnode"), "props"),
                    expression: true, async: false)));
            if (contract.LocalModels.Count > 0)
                setupStatements.Add(Assign(Member(state, "$joltEmit"), Member(context, "emit")));
        }
        if (setupMethod is not null)
        {
            // Vue owns the active instance and effect scope during setup. Keep this call
            // synchronous so lifecycle registration and watcher disposal use that scope.
            setupStatements.Add(new NonSpecialExpressionStatement(Call(Member(state, Util.GetConfigOrSymbolName(setupMethod)))));
        }
        if (exposed.Count > 0)
        {
            // Bind to the reactive state so externally invoked methods observe and
            // update the same instance as the template.
            var publicMethods = exposed.Select(method =>
                Property(method.Name, Call(Member(Member(state, Util.GetConfigOrSymbolName(method)), "bind"), state)));
            setupStatements.Add(new NonSpecialExpressionStatement(Call(Member(context, "expose"), Object(publicMethods.ToArray()))));
        }
        setupStatements.Add(new ReturnStatement(Object(Property("joltState", state))));
        var setup = new ArrowFunctionExpression(
            NodeList.From<Node>(Id("$joltProps"), context),
            new FunctionBody(NodeList.From(setupStatements), strict: true),
            expression: false, async: false);

        var definitionProperties = new List<ObjectProperty>
        {
            Property("name", String(contract.Name)),
            Property("props", Array(contract.Properties.Select(property => property.Name)
                .Concat(contract.VuePropertyAliases.Values)
                .Select(property => (Expression)String(property)).ToArray())),
            Property("setup", setup),
            // Vue can replace this render function while retaining setup state. Invoke the
            // render method from this module's prototype to pick up the new template body.
            Property("render", new ArrowFunctionExpression(
                NodeList.From<Node>(Id("_ctx"), Id("_cache"), Id("_props"), Id("$setup")),
                Call(Member(Member(Member(Id(stateClass.Id!.Name), "prototype"), renderName), "call"),
                    Member(Id("$setup"), "joltState")),
                expression: true, async: false)),
        };
        if (contract.LocalModels.Count > 0)
            definitionProperties.Add(Property("emits", Array(contract.LocalModels.Select(model => (Expression)String(model.Event)).ToArray())));
        if (scope is not null)
            definitionProperties.Add(Property("__scopeId", String(scope)));
        var definition = Call(Id("$joltDefine"), Object(definitionProperties.ToArray()));
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
        var declaration = JoltTypeScriptDeclaration.Emit(contract, projection, moduleSymbol, StateType!, exposed);
        return new JoltModule(contract.Name, path, artifact.Content + "\n//# sourceMappingURL=" + Path.GetFileName(path) + ".map\n", artifact.SourceMapContent!)
        {
            ClrImports = clrImports.Count == 0 ? [] : clrImports.ToArray(),
            LocalVueImports = _aliases.Keys.Select(component => component.Npm?.LocalPath).OfType<string>()
                .Order(StringComparer.Ordinal).ToArray(),
            ScopedCss = styles.Length == 0 ? null : styles.All(style => style.Scoped)
                ? $"/* {scope} */\n" + string.Join("\n", styles.Select(style => NormalizeStyle(style.Css)))
                : "/* jolt-style-blocks */\n" + System.Text.Json.JsonSerializer.Serialize(
                    styles.Select(style => new { scope = style.Scoped ? scope : null, css = NormalizeStyle(style.Css) })),
            TypeScriptDeclaration = declaration.Content,
            TypeScriptSourceMap = declaration.SourceMap,
        };
    }

    internal static string ModulePath(string source) => Path.ChangeExtension(source.Replace('\\', '/'), ".mjs");

    private static string NormalizeStyle(string css) =>
        System.Text.RegularExpressions.Regex.Replace(css,
            """/\*[\s\S]*?(?:\*/|$)|"(?:\\[\s\S]|[^"\\])*"|'(?:\\[\s\S]|[^'\\])*'|\\[\s\S]|@@(?=[a-zA-Z-])""",
            match => match.Value == "@@" ? "@" : match.Value);

    private static string ScopedStyleId(string modulePath) =>
        "data-v-" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(modulePath)).AsSpan(0, 6));

    private void RemapOrigins(Node node)
    {
        if (node.UserData is SourceOrigin origin && origin.SourcePath == projection.GeneratedPath &&
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
