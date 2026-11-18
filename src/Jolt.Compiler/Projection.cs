using System.Globalization;
using System.Text;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal enum IntrinsicKind { Array, Append, Text, Element, Prop, Slot, Outlet }
internal sealed record Intrinsic(IntrinsicKind Kind, TextSpan Span, string? Name = null, ComponentContract? Component = null);

internal sealed class Projection(JoltDocument document, string name, string componentNamespace, List<Diagnostic> diagnostics)
{
    internal const string Render = "__JoltRender";
    internal const string IntrinsicPrefix = "__JoltIntrinsic";
    internal string Name { get; } = name;
    internal string Namespace { get; } = componentNamespace;
    internal string StateName => "__State" + Name;
    internal JoltTypeParameter? TypeParameter { get; } = document.Nodes.OfType<JoltTypeParameter>().FirstOrDefault();
    internal string StateMetadataName => TypeParameter is null ? StateName : StateName + "`1";
    private string StateType => TypeParameter is null ? StateName : $"{StateName}<{TypeParameter.Name}>";
    internal string ModuleName => "__Module" + Name;
    internal JoltDocument Document { get; } = document;
    internal SourceText Source { get; } = SourceText.From(document.Source);
    internal SourceText? GeneratedText { get; private set; }
    internal List<JoltProjectionSpan> Spans { get; } = [];
    internal List<JoltMarkupBinding> MarkupBindings { get; } = [];
    internal Dictionary<string, Intrinsic> Intrinsics { get; } = new(StringComparer.Ordinal);
    internal IEnumerable<ComponentContract> Dependencies => Intrinsics.Values.Select(intrinsic => intrinsic.Component)
        .OfType<ComponentContract>().Distinct();
    private readonly StringBuilder _code = new();
    private readonly List<string> _methods = [];
    private readonly List<string> _scopes = [];
    private readonly List<(TextSpan Generated, TextSpan Source)> _anchors = [];
    private readonly List<(TextSpan Generated, JoltExpression Source, ComponentProperty Property)> _literalChecks = [];
    private readonly List<(TextSpan Generated, JoltElement Element, ComponentContract Component)> _typeProbes = [];
    private IReadOnlyList<ComponentContract> _components = [];
    private IReadOnlyDictionary<int, string?>? _typeArguments;
    private int _nextLocal;

    internal SyntaxTree Build(IReadOnlyList<ComponentContract>? components, CSharpParseOptions parseOptions,
        IReadOnlyDictionary<int, string?>? typeArguments = null)
    {
        Spans.Clear();
        MarkupBindings.Clear();
        _code.Clear();
        Intrinsics.Clear();
        _methods.Clear();
        _scopes.Clear();
        _anchors.Clear();
        _literalChecks.Clear();
        _typeProbes.Clear();
        _nextLocal = 0;
        _components = components ?? [];
        _typeArguments = typeArguments;
        Write("#nullable enable\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing JoltAuthoring;\n");
        foreach (var import in Document.Nodes.OfType<JoltUsing>().Where(import => !import.Code.Trim().StartsWith("Npm.", StringComparison.Ordinal)))
        {
            Write("using ");
            Copy(import.CodeSpan);
            if (!import.Code.TrimEnd().EndsWith(';'))
                Write(";");
            Write("\n");
        }
        Write($"namespace {Namespace};\npublic static class {ModuleName} {{\n");
        EmitAuthorCode(parseOptions);

        if (components is not null)
        {
            Write($"public object {Render}() {{\n");
            var content = Document.Nodes.Where(node => node is not (JoltUsing or JoltCode or JoltTypeParameter or JoltScopedStyle))
                .Where(node => node is not JoltText text || !string.IsNullOrWhiteSpace(text.Text)).ToArray();
            if (content is [JoltElement { Name.Length: > 0 } element])
            {
                // Vue applies the parent's scope ID to a child root VNode. Author
                // whitespace framing the document must not turn it into a fragment.
                Write("return ");
                EmitElement(element);
                Write(";\n");
            }
            else
            {
                var nodes = BeginNodes();
                EmitNodes(Document.Nodes, nodes);
                Write($"return {nodes};\n");
            }
            Write("}\n");
            foreach (var method in _methods)
                Write(method);
            foreach (var scope in _scopes)
                Write(scope);
        }

        Write("}\n}\n");
        GeneratedText = SourceText.From(_code.ToString());
        return CSharpSyntaxTree.ParseText(GeneratedText, parseOptions, Document.Path + ".g.cs");
    }

    private void EmitAuthorCode(CSharpParseOptions parseOptions)
    {
        var instanceSpans = new List<TextSpan>();
        foreach (var code in Document.Nodes.OfType<JoltCode>())
        {
            const string prefix = "class Component {\n";
            var parsed = SyntaxFactory.ParseCompilationUnit(prefix + code.Code + "\n}", options: parseOptions);
            var declaration = (ClassDeclarationSyntax)parsed.Members[0];
            int cursor = code.CodeSpan.Start;
            foreach (var member in declaration.Members.Where(IsModuleMember))
            {
                // 外围条件编译指令留在原块以维持配对。Roslyn 已选择当前生效的声明。
                var lastDirective = member.GetLeadingTrivia().LastOrDefault(trivia => trivia.IsDirective);
                var nextDirective = member.GetTrailingTrivia().FirstOrDefault(trivia => trivia.IsDirective);
                int start = code.CodeSpan.Start + (lastDirective.IsDirective ? lastDirective.FullSpan.End : member.FullSpan.Start) - prefix.Length;
                int end = Math.Min(code.CodeSpan.Start + (nextDirective.IsDirective ? nextDirective.FullSpan.Start : member.FullSpan.End) - prefix.Length,
                    code.CodeSpan.End);
                if (start > cursor)
                    instanceSpans.Add(TextSpan.FromBounds(cursor, start));
                Copy(TextSpan.FromBounds(start, end));
                Write("\n");
                cursor = end;
            }
            if (cursor <= code.CodeSpan.End)
                instanceSpans.Add(TextSpan.FromBounds(cursor, code.CodeSpan.End));
        }
        // 将模块成员交给 AstConverter 生成具名导出，组件实例仍持有自己的状态。
        Write($"private sealed class {StateName}");
        if (TypeParameter is { } parameter)
        {
            Write("<");
            Copy(parameter.NameSpan);
            Write(">");
        }
        Write(" {\n");
        foreach (var span in instanceSpans)
        {
            Copy(span);
            Write("\n");
        }
    }

    private static bool IsModuleMember(MemberDeclarationSyntax member) =>
        member is MethodDeclarationSyntax or FieldDeclarationSyntax &&
        member.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword) || modifier.IsKind(SyntaxKind.ConstKeyword));

    internal string? GetExpressionType(JoltExpression expression, SemanticModel model)
    {
        var span = Spans.FirstOrDefault(entry => entry.Source == expression.CodeSpan);
        if (span is null)
            return null;
        var syntax = model.SyntaxTree.GetRoot().FindNode(span.Generated, getInnermostNodeForTie: true) as ExpressionSyntax;
        var type = syntax is null ? null : model.GetTypeInfo(syntax).Type;
        return type is { TypeKind: not TypeKind.Error, IsAnonymousType: false }
            ? type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            : null;
    }

    internal void CheckLiterals(SemanticModel model)
    {
        var root = model.SyntaxTree.GetRoot();
        foreach (var check in _literalChecks)
        {
            var expression = root.FindNode(check.Generated, getInnermostNodeForTie: true);
            var value = model.GetConstantValue(expression);
            if (value is { HasValue: true, Value: string literal } &&
                check.Property.Literals!.Contains(literal, StringComparer.Ordinal))
                continue;
            Report("JOLT1006", check.Source.CodeSpan, $"Prop '{check.Property.Name}' expects a constant from {string.Join(", ", check.Property.Literals!)}.");
        }
    }

    internal IReadOnlyDictionary<int, string?> BindTypeArguments(SemanticModel model)
    {
        var root = model.SyntaxTree.GetRoot();
        return _typeProbes.ToDictionary(probe => probe.Element.NameSpan.Start, probe =>
        {
            var call = root.FindNode(probe.Generated, getInnermostNodeForTie: true);
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            var argument = method?.TypeArguments.Single();
            if (argument is null || argument.TypeKind == TypeKind.Error || argument.IsAnonymousType)
                return null;
            var constraint = probe.Component.TypeParameter!.NpmConstraint;
            if (constraint is not null && !constraint.Contains(argument.SpecialType switch
                {
                    SpecialType.System_String => "string",
                    SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16
                        or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Single or SpecialType.System_Double => "number",
                    SpecialType.System_Boolean => "boolean",
                    _ => "",
                }, StringComparer.Ordinal))
                return null;
            return argument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        });
    }

    internal Location Location(TextSpan span) => Microsoft.CodeAnalysis.Location.Create(Document.Path, span, Source.Lines.GetLinePositionSpan(span));

    internal TextSpan? Map(TextSpan span)
    {
        foreach (var entry in Spans)
        {
            if (span.Start >= entry.Generated.Start && span.Start <= entry.Generated.End)
            {
                int start = entry.Source.Start + span.Start - entry.Generated.Start;
                return new TextSpan(start, Math.Min(span.Length, entry.Source.End - start));
            }
        }
        return _anchors.Where(entry => entry.Generated.Contains(span.Start))
            .OrderBy(entry => entry.Generated.Length).Select(entry => (TextSpan?)entry.Source).FirstOrDefault();
    }

    private void EmitNodes(IEnumerable<JoltNode> nodes, string target)
    {
        foreach (var node in nodes)
        {
            switch (node)
            {
                case JoltText text when text.Text.Length != 0:
                    Append(target, node, () => Write(Literal(text.Text)));
                    break;
                case JoltExpression expression:
                    Append(target, node, () =>
                    {
                        Call(new(IntrinsicKind.Text, expression.CodeSpan), "string", "object? value");
                        Copy(expression.CodeSpan);
                        Write(")");
                    });
                    break;
                case JoltIf conditional:
                    Write("if (");
                    Copy(conditional.ConditionSpan);
                    Write(") {\n");
                    EmitNodes(conditional.Then, target);
                    Write("} else {\n");
                    EmitNodes(conditional.Else, target);
                    Write("}\n");
                    break;
                case JoltForEach loop:
                    Write("foreach (");
                    Copy(loop.HeaderSpan);
                    Write(") {\n");
                    EmitNodes(loop.Children, target);
                    Write("}\n");
                    break;
                case JoltElement { Name.Length: 0 } incomplete:
                    // Razor can produce an unnamed element while the author has typed only '<'.
                    Report("JOLT1017", incomplete.NameSpan, "Expected an element name.");
                    break;
                case JoltElement element:
                    Append(target, node, () => EmitElement(element));
                    break;
            }
        }
    }

    private void EmitElement(JoltElement element)
    {
        if (element.Name == "slot")
        {
            var nameAttribute = element.Attributes.FirstOrDefault(attribute => attribute.Name == "name");
            string name = ComponentContract.Text(nameAttribute) ?? "default";
            if (nameAttribute?.Value is [JoltText outlet] && outlet.Span.Length > 0)
                MarkupBindings.Add(new(new(Document.Path, JoltMarkupKind.Slot, name), outlet.Span, IsDeclaration: true));
            Call(new(IntrinsicKind.Outlet, element.Span, name), "object", $"{StateType} state, object[] props, global::System.Func<object> fallback");
            Write("this, ");
            EmitProperties(element.Attributes.Where(attribute => attribute.Name != "name"), null, element);
            Write(", ");
            EmitSlotBody(element.Children);
            Write(")");
            return;
        }

        var candidates = _components.Where(component => component.Name == element.Name).ToArray();
        var component = candidates.Length == 1 ? candidates[0] : null;
        if (candidates.Length > 1)
            Report("JOLT1002", element.NameSpan, $"Component '{element.Name}' is ambiguous.");
        else if (component is null && char.IsUpper(element.Name[0]))
            Report("JOLT1001", element.NameSpan, $"Component '{element.Name}' was not found.");

        if (component is { Npm: null })
        {
            var symbol = new JoltMarkupSymbol(component.Path, JoltMarkupKind.Component);
            MarkupBindings.Add(new(symbol, element.NameSpan));
            if (element.ClosingNameSpan is { } closing)
                MarkupBindings.Add(new(symbol, closing));
        }

        var bound = component;
        var attributes = element.Attributes.AsEnumerable();
        bool typeProbe = false;
        if (component?.TypeParameter is { } parameter)
        {
            var explicitType = element.Attributes.FirstOrDefault(attribute => attribute.Name == parameter.Name);
            attributes = attributes.Where(attribute => attribute != explicitType);
            if (_typeArguments is null)
                typeProbe = EmitTypeProbe(element, component, parameter, explicitType);
            var argument = _typeArguments?.GetValueOrDefault(element.NameSpan.Start);
            if (_typeArguments is not null && argument is null)
                Report("JOLT1021", explicitType?.NameSpan ?? element.NameSpan,
                    $"Cannot bind type parameter '{parameter.Name}' of '{element.Name}'. Supply a supported type or a typed collection.");
            // The probe only binds input expressions. A failed final binding already
            // has a type-parameter diagnostic at its author span.
            bound = component.Specialize(argument ?? "object");
        }
        Call(new(IntrinsicKind.Element, element.NameSpan, element.Name, component), "object", "object[] props, object children");
        EmitProperties(attributes, bound, element);
        Write(", ");
        if (component is null)
            EmitSlotBody(element.Children, invoke: true);
        else
            EmitSlots(bound!, element);
        Write(typeProbe ? "))" : ")");
    }

    private bool EmitTypeProbe(JoltElement element, ComponentContract component, ComponentTypeParameter parameter, JoltAttribute? explicitType)
    {
        var method = IntrinsicPrefix + "Infer" + _typeProbes.Count;
        var witness = component.Properties.FirstOrDefault(property => property.Type is { } type &&
            (type == parameter.Name + "[]" || type == $"global::System.Collections.Generic.IReadOnlyList<{parameter.Name}>"));
        var attribute = element.Attributes.FirstOrDefault(attribute => witness is not null &&
            string.Equals(attribute.Name, witness.Name, StringComparison.OrdinalIgnoreCase));
        bool explicitArgument = explicitType?.Value is [JoltText explicitText] &&
            SyntaxFactory.ParseTypeName(explicitText.Text) is { ContainsDiagnostics: false } parsed && parsed.FullSpan.Length == explicitText.Text.Length;
        if (explicitType is not null && !explicitArgument || !explicitArgument && attribute?.Value is not [JoltExpression])
            return false;
        // The probe is a separate expression in a well-formed call. Only its
        // constructed Roslyn symbol is read, and no probe reaches lowering.
        if (_typeProbes.Count == 0)
            _methods.Add("private static object __JoltBindType(object witness, object element) => element;\n");
        Write("__JoltBindType(");
        var start = _code.Length;
        Write(method);
        if (explicitArgument)
        {
            Write("<");
            Copy(((JoltText)explicitType!.Value[0]).Span);
            Write(">");
        }
        Write("(");
        if (!explicitArgument)
            Copy(((JoltExpression)attribute!.Value[0]).CodeSpan);
        Write(")");
        _typeProbes.Add((TextSpan.FromBounds(start, _code.Length), element, component));
        Write(", ");
        _methods.Add($"private static object {method}<{parameter.Name}>({(explicitArgument ? "" : witness!.Type + " value")}) => throw new global::System.NotSupportedException();\n");
        return true;
    }

    private void EmitProperties(IEnumerable<JoltAttribute> attributes, ComponentContract? component, JoltElement element)
    {
        Write("new object[] {");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attribute in attributes)
        {
            var domEvent = attribute.Name.Length > 3 && attribute.Name.StartsWith("@on", StringComparison.Ordinal);
            if (attribute.Name == "@on")
                Report("JOLT1016", attribute.NameSpan, "The event attribute requires an event name, such as @onclick.");
            string input = domEvent ? ComponentContract.EventProperty(attribute.Name[3..]) : attribute.Name;
            var property = component?.Properties.FirstOrDefault(prop => prop.Name == input) ??
                component?.Properties.FirstOrDefault(prop => string.Equals(prop.Name, input, StringComparison.OrdinalIgnoreCase));
            var name = property?.Name ?? input;
            if (!seen.Add(name))
                Report("JOLT1003", attribute.NameSpan, $"Attribute '{attribute.Name}' is specified more than once.");
            if (component is not null && property is null)
                Report("JOLT1004", attribute.NameSpan, $"Component '{component.Name}' has no prop or event '{attribute.Name}'.");
            if (component?.Npm is { } npm && property is { Type: null })
                Report("JOLT1014", attribute.NameSpan,
                    $"npm component '{npm.ModuleSpecifier}/{npm.ExportName}' member '{property.Name}' has an unsupported type ({property.Unsupported}).");
            if (component is { Npm: null } && property is not null)
                MarkupBindings.Add(new(new(component.Path, JoltMarkupKind.Property, property.Name), attribute.NameSpan));

            var span = attribute.Value.OfType<JoltExpression>().FirstOrDefault()?.CodeSpan ??
                attribute.Value.OfType<JoltText>().FirstOrDefault()?.Span ?? attribute.NameSpan;
            int generatedStart = _code.Length;
            Call(new(IntrinsicKind.Prop, span, name), "object", $"{property?.Type ?? (domEvent && property is null ? "global::System.Action" : "object?")} value");
            EmitAttributeValue(attribute, property, domEvent);
            _anchors.Add((TextSpan.FromBounds(generatedStart, _code.Length), span));
            Write("), ");
        }
        if (component is not null)
        {
            foreach (var property in component.Properties.Where(prop => prop.Required && !seen.Contains(prop.Name)))
                Report("JOLT1005", element.NameSpan, $"Component '{component.Name}' requires prop '{property.Name}'.");
        }
        Write("}");
    }

    private void EmitAttributeValue(JoltAttribute attribute, ComponentProperty? property, bool domEvent)
    {
        if (attribute.Value.Length == 0)
        {
            Write("true");
            return;
        }
        if (attribute.Value is [JoltExpression expression])
        {
            int generatedStart = _code.Length;
            Copy(expression.CodeSpan);
            if (property?.Literals is { Count: > 0 })
                _literalChecks.Add((TextSpan.FromBounds(generatedStart, _code.Length), expression, property));
            return;
        }
        if (attribute.Value is [JoltText text])
        {
            string value = text.Text;
            if (property?.Literals is { Count: > 0 } literals && !literals.Contains(value, StringComparer.Ordinal))
                Report("JOLT1006", text.Span, $"Prop '{property.Name}' expects one of {string.Join(", ", literals)}.");
            if (domEvent)
            {
                Copy(text.Span);
                return;
            }
            if (property?.Type is "bool" or "bool?" && bool.TryParse(value, out bool boolean))
                Write(boolean ? "true" : "false");
            else if (property?.Type is "long" or "long?" &&
                long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                Write(integer.ToString(CultureInfo.InvariantCulture) + "L");
            else if (property?.Type is "double" or "double?" or "int" or "float" &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number) && double.IsFinite(number))
                Write(number.ToString("R", CultureInfo.InvariantCulture) + (property.Type == "float" ? "f" : ""));
            else
                Write(Literal(value));
            return;
        }
        Write("\"\"");
        foreach (var part in attribute.Value)
        {
            Write(" + ");
            if (part is JoltExpression fragment)
            {
                Write("(");
                Copy(fragment.CodeSpan);
                Write(")");
            }
            else if (part is JoltText literal)
                Write(Literal(literal.Text));
        }
    }

    private void EmitSlots(ComponentContract component, JoltElement element)
    {
        Write("new object[] {");
        var named = element.Children.OfType<JoltElement>()
            .Where(child => child.Name == "template" && child.Attributes.Any(attribute => attribute.Name.StartsWith('#')))
            .ToArray();
        var defaultNodes = element.Children.Except(named).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var template in named)
        {
            var directive = template.Attributes.First(attribute => attribute.Name.StartsWith('#'));
            string slot = directive.Name[1..];
            if (component.Npm is null && component.Slots.Any(candidate => candidate.Name == slot))
                MarkupBindings.Add(new(new(component.Path, JoltMarkupKind.Slot, slot),
                    TextSpan.FromBounds(directive.NameSpan.Start + 1, directive.NameSpan.End)));
            var context = template.Attributes.FirstOrDefault(attribute => attribute.Name == "context");
            EmitSlot(template.Children, slot, directive.NameSpan, context, component, seen);
        }
        if (defaultNodes.Any(node => node is not JoltText text || !string.IsNullOrWhiteSpace(text.Text)))
            EmitSlot(defaultNodes, "default", element.NameSpan, null, component, seen);
        foreach (var slot in component.Slots.Where(slot => slot.Required && !seen.Contains(slot.Name)))
            Report("JOLT1008", element.NameSpan, $"Component '{component.Name}' requires slot '{slot.Name}'.");
        Write("}");
    }

    private void EmitSlot(IEnumerable<JoltNode> nodes, string name, TextSpan span, JoltAttribute? context, ComponentContract component, HashSet<string> seen)
    {
        var slot = component.Slots.FirstOrDefault(candidate => candidate.Name == name);
        if (slot is null)
            Report("JOLT1007", span, $"Component '{component.Name}' has no slot '{name}'.");
        if (!seen.Add(name))
            Report("JOLT1009", span, $"Slot '{name}' is specified more than once.");

        string signature = "global::System.Func<object>";
        string? parameter = ComponentContract.Text(context);
        if (parameter is not null && SyntaxFacts.IsValidIdentifier(parameter))
        {
            if (component.Npm is { } npm && slot?.Unsupported is { } unsupported)
                Report("JOLT1015", span, $"npm component '{npm.ModuleSpecifier}/{npm.ExportName}' slot '{name}' has an unsupported scope ({unsupported}).");
            var scopeType = "__JoltScope" + _scopes.Count;
            var members = slot?.Scope ?? [];
            _scopes.Add($"public sealed class {scopeType} {{\n" + string.Join("\n", members.Where(member => SyntaxFacts.IsValidIdentifier(member.Name))
                .Select(member => ScopeMember(component, name, member))) + "\n}\n");
            signature = $"global::System.Func<{scopeType}, object>";
        }
        else if (context is not null)
            Report("JOLT1010", context.Span, "Slot context must be a C# identifier.");

        Call(new(IntrinsicKind.Slot, span, name), "object", $"{signature} body");
        EmitSlotBody(nodes, parameter: parameter is not null && SyntaxFacts.IsValidIdentifier(parameter) ? parameter : null);
        Write("), ");
    }

    private static string ScopeMember(ComponentContract component, string slot, ComponentProperty member)
    {
        string attribute = member.Type is null && component.Npm is { } npm
            ? $"[global::System.Obsolete({Literal($"npm component '{npm.ModuleSpecifier}/{npm.ExportName}' slot '{slot}' member '{member.Name}' has an unsupported type ({member.Unsupported}).")}, true)]\n"
            : "";
        return attribute + $"public {member.Type ?? "object"} {member.Name} {{ get; set; }} = default!;";
    }

    private void EmitSlotBody(IEnumerable<JoltNode> children, bool invoke = false, string? parameter = null)
    {
        if (invoke)
        {
            // Evaluate normal HTML children immediately, with the same statement scope as slots.
            Write("((global::System.Func<object>)(() => {\n");
        }
        else
            Write(parameter is null ? "() => {\n" : $"{parameter} => {{\n");
        var nodes = BeginNodes();
        EmitNodes(children, nodes);
        Write($"return {nodes};\n}}");
        if (invoke)
            Write("))()");
    }

    private string BeginNodes()
    {
        string name = "__joltNodes" + _nextLocal++;
        Write($"var {name} = ");
        Call(new(IntrinsicKind.Array, default), "object[]", "");
        Write(");\n");
        return name;
    }

    private void Append(string target, JoltNode node, Action value)
    {
        Call(new(IntrinsicKind.Append, node.Span), "void", "object[] target, object? node");
        Write(target + ", ");
        value();
        Write(");\n");
    }

    private void Call(Intrinsic intrinsic, string result, string parameters)
    {
        string name = IntrinsicPrefix + Intrinsics.Count;
        Intrinsics.Add(name, intrinsic);
        _methods.Add($"private static {result} {name}({parameters}) => throw new global::System.NotImplementedException();\n");
        if (intrinsic.Kind != IntrinsicKind.Array)
            _anchors.Add((new TextSpan(_code.Length, name.Length), intrinsic.Span));
        Write(name + "(");
    }

    private void Copy(TextSpan span)
    {
        Spans.Add(new JoltProjectionSpan(span, new TextSpan(_code.Length, span.Length)));
        Write(Source.ToString(span));
    }

    private void Write(string text) => _code.Append(text);
    private static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);
    private void Report(string id, TextSpan span, string message)
        => diagnostics.Add(JoltProjectCompiler.Error(id, Location(span), message));
}
