using System.Globalization;
using System.Text;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal enum IntrinsicKind { Array, Append, Text, Element, Prop, Slot, Outlet, Bind, DomEvent, Attributes, Ref, Fragment, ModelUpdate, ConditionalBranch }
internal sealed record Intrinsic(IntrinsicKind Kind, TextSpan Span, string? Name = null, ComponentContract? Component = null, ModelBinding? Binding = null);

internal sealed partial class Projection(JoltDocument document, string name, string componentNamespace,
    IReadOnlyList<JoltImport> imports, List<Diagnostic> diagnostics)
{
    internal const string Render = "__JoltRender";
    internal const string IntrinsicPrefix = "__JoltIntrinsic";
    internal string Name { get; } = name;
    internal string Namespace { get; } = componentNamespace;
    internal string StateName => "__State" + Name;
    internal JoltTypeParameter[] TypeParameters { get; } = document.Nodes.OfType<JoltTypeParameter>().DistinctBy(parameter => parameter.Name).ToArray();
    internal string StateMetadataName => TypeParameters.Length == 0 ? StateName : StateName + "`" + TypeParameters.Length;
    private string StateType => TypeParameters.Length == 0 ? StateName : $"{StateName}<{string.Join(", ", TypeParameters.Select(parameter => parameter.Name))}>";
    internal string ModuleName => "__Module" + Name;
    internal JoltDocument Document { get; } = document;
    internal SourceText Source { get; } = SourceText.From(document.Source);
    internal SourceText? GeneratedText { get; private set; }
    internal List<JoltProjectionSpan> Spans { get; } = [];
    internal IReadOnlyList<JoltImport> Imports { get; } = imports;
    private readonly List<(TextSpan Generated, JoltImport Import)> _inheritedImports = [];
    internal List<JoltMarkupBinding> MarkupBindings { get; } = [];
    internal Dictionary<string, Intrinsic> Intrinsics { get; } = new(StringComparer.Ordinal);
    internal IEnumerable<ComponentContract> Dependencies => Intrinsics.Values.Select(intrinsic => intrinsic.Component)
        .OfType<ComponentContract>().Distinct();
    private readonly StringBuilder _code = new();
    private readonly List<string> _methods = [];
    private readonly List<string> _scopes = [];
    internal Dictionary<(string Scope, string Member), (string Path, TextSpan Span)> SlotMemberOrigins { get; } = [];
    internal Dictionary<(string Scope, string Member), Jolt.Npm.NpmDefinition> ExternalSlotMemberOrigins { get; } = [];
    private readonly List<(TextSpan Generated, TextSpan Source)> _anchors = [];
    private readonly List<(TextSpan Generated, JoltExpression Source, ComponentProperty Property)> _literalChecks = [];
    private readonly List<(TextSpan Generated, (int Source, string Path) Site, ComponentContract Component, JoltAttribute?[] Explicit)> _typeProbes = [];
    private IReadOnlyList<ComponentContract> _components = [];
    private bool _componentsAvailable;
    private IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>? _typeArguments;
    private bool _probeTypeArguments;
    private int _nextLocal;

    internal SyntaxTree Build(IReadOnlyList<ComponentContract>? components, CSharpParseOptions parseOptions,
        IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding>? typeArguments = null, bool probeTypeArguments = false)
    {
        Spans.Clear();
        _inheritedImports.Clear();
        MarkupBindings.Clear();
        _code.Clear();
        Intrinsics.Clear();
        _methods.Clear();
        _scopes.Clear();
        SlotMemberOrigins.Clear();
        ExternalSlotMemberOrigins.Clear();
        _anchors.Clear();
        _literalChecks.Clear();
        _bindingChecks.Clear();
        _insideBoundSelect = false;
        _referenceChecks.Clear();
        ReferenceTargets.Clear();
        _referenceLoopDepth = 0;
        _exposedDeclarations.Clear();
        _invalidExposedDeclarations.Clear();
        _typeProbes.Clear();
        _nextLocal = 0;
        _components = components ?? [];
        _componentsAvailable = components is not null;
        _typeArguments = typeArguments;
        _probeTypeArguments = probeTypeArguments;
        Write("#nullable enable\nusing System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing JoltAuthoring;\n");
        foreach (var import in Imports.Where(import => !import.IsNpm || import.IsNpmValue))
        {
            var start = _code.Length;
            Write("using ");
            if (import.Document == Document)
                Copy(import.Directive.CodeSpan);
            else
                Write(import.Directive.Code);
            if (!import.Directive.Code.TrimEnd().EndsWith(';'))
                Write(";");
            Write("\n");
            if (import.Document != Document)
                _inheritedImports.Add((TextSpan.FromBounds(start, _code.Length), import));
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
        }
        foreach (var method in _methods)
            Write(method);
        foreach (var scope in _scopes)
            Write(scope);

        Write("}\n}\n");
        EmitReferenceContract();
        GeneratedText = SourceText.From(_code.ToString());
        return CSharpSyntaxTree.ParseText(GeneratedText, parseOptions, Document.Path + ".g.cs");
    }

    private void EmitAuthorCode(CSharpParseOptions parseOptions)
    {
        var instanceSpans = new List<TextSpan>();
        foreach (var code in Document.Nodes.OfType<JoltCode>())
        {
            const string prefix = "class Component {\n";
            var parsed = SyntaxFactory.ParseCompilationUnit(prefix + MaskInlineTemplates(code) + "\n}", options: parseOptions);
            var declaration = (ClassDeclarationSyntax)parsed.Members[0];
            CollectExposedDeclarations(declaration, code.CodeSpan.Start - prefix.Length);
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
        if (TypeParameters.Length != 0)
        {
            Write("<");
            for (int index = 0; index < TypeParameters.Length; index++)
            {
                if (index != 0)
                    Write(", ");
                Copy(TypeParameters[index].NameSpan);
            }
            Write(">");
            foreach (var parameter in TypeParameters)
            {
                if (parameter.ConstraintSpan is not { } constraint)
                    continue;
                Write(" ");
                Copy(constraint);
            }
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

    internal string? GetExpressionType(JoltExpression expression, SemanticModel model) =>
        GetExpressionTypeSymbol(expression, model)?.ToDisplayString(ComponentContract.TypeDisplay);

    internal ITypeSymbol? GetExpressionTypeSymbol(JoltExpression expression, SemanticModel model)
    {
        var span = Spans.FirstOrDefault(entry => entry.Source == expression.CodeSpan);
        if (span is null)
            return null;
        var syntax = model.SyntaxTree.GetRoot().FindNode(span.Generated, getInnermostNodeForTie: true) as ExpressionSyntax;
        var type = syntax is null ? null : model.GetTypeInfo(syntax).Type;
        return type is { TypeKind: not TypeKind.Error, IsAnonymousType: false }
            ? type
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

    internal IReadOnlyDictionary<(int Source, string Path), ComponentTypeBinding> BindTypeArguments(SemanticModel model)
    {
        var root = model.SyntaxTree.GetRoot();
        var result = new Dictionary<(int Source, string Path), ComponentTypeBinding>();
        foreach (var probe in _typeProbes)
        {
            var call = root.FindNode(probe.Generated, getInnermostNodeForTie: true);
            var method = model.GetSymbolInfo(call).Symbol as IMethodSymbol;
            var errors = model.GetDiagnostics(probe.Generated).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error ||
                diagnostic.Id is "CS8714" or "CS8634").ToArray();
            if (errors.Length != 0)
            {
                // C# reports a failed constraint on the generic call name. Its message
                // identifies the declaration parameter so an explicit argument can own the diagnostic.
                var error = errors[0];
                string message = error.GetMessage(CultureInfo.InvariantCulture);
                if (method is null || error.Id != "CS1503")
                {
                    TextSpan? source = Map(error.Location.SourceSpan);
                    for (int index = 0; index < probe.Explicit.Length; index++)
                    {
                        if (probe.Explicit[index] is { } attribute && message.Contains($"parameter '{probe.Component.TypeParameters[index].Name}'", StringComparison.Ordinal))
                        {
                            source = attribute.Value[0].Span;
                            break;
                        }
                    }
                    result.Add(probe.Site, new(null, source, message));
                    continue;
                }
            }
            if (method is null || method.TypeArguments.Any(argument => argument.TypeKind == TypeKind.Error || argument.IsAnonymousType))
                continue;
            var constraint = probe.Component.TypeParameters[0].NpmConstraint;
            if (constraint is not null && !constraint.Contains(method.TypeArguments[0].SpecialType switch
                {
                    SpecialType.System_String => "string",
                    SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16
                        or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Single or SpecialType.System_Double => "number",
                    SpecialType.System_Boolean => "boolean",
                    _ => "",
                }, StringComparer.Ordinal))
                continue;
            result.Add(probe.Site, new(method.TypeArguments
                .Select(argument => argument.ToDisplayString(ComponentContract.TypeDisplay)).ToArray()));
        }
        return result;
    }

    internal Location Location(TextSpan span) => Microsoft.CodeAnalysis.Location.Create(Document.Path, span, Source.Lines.GetLinePositionSpan(span));

    internal Location MapLocation(TextSpan span)
    {
        foreach (var (generated, import) in _inheritedImports)
        {
            if (!generated.Contains(span.Start))
                continue;
            // Preserve the import file's coordinates without adding foreign spans to editor projections.
            var code = import.Directive.CodeSpan;
            var offset = Math.Clamp(span.Start - generated.Start - "using ".Length, 0, code.Length);
            return import.Location(new TextSpan(code.Start + offset, Math.Min(span.Length, code.Length - offset)));
        }
        return Location(Map(span) ?? default);
    }

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
                    EmitConditionalBranch(conditional, conditional.Then, target, "then");
                    Write("} else {\n");
                    EmitConditionalBranch(conditional, conditional.Else, target, "else");
                    Write("}\n");
                    break;
                case JoltForEach loop:
                    Write("foreach (");
                    Copy(loop.HeaderSpan);
                    Write(") {\n");
                    // Indentation around the one keyed row belongs to the template.
                    // Interleaved unkeyed text changes which DOM row Vue moves on reorder.
                    // Only trim the structural trivia around this explicit keyed form.
                    _referenceLoopDepth++;
                    var row = SingleKeyedRow(loop);
                    if (row is null)
                        EmitNodes(loop.Children, target);
                    else
                        Append(target, row, () => EmitElement(row));
                    _referenceLoopDepth--;
                    Write("}\n");
                    break;
                case JoltFor loop:
                    Write("for (");
                    Copy(loop.HeaderSpan);
                    Write(") {\n");
                    _referenceLoopDepth++;
                    EmitNodes(loop.Children, target);
                    _referenceLoopDepth--;
                    Write("}\n");
                    break;
                case JoltSwitch selection:
                    Write("switch (");
                    Copy(selection.ExpressionSpan);
                    Write(") {\n");
                    foreach (var section in selection.Sections)
                    {
                        foreach (var label in section.Labels)
                        {
                            Copy(label.CodeSpan);
                            Write("\n");
                        }
                        EmitNodes(section.Children, target);
                    }
                    Write("}\n");
                    break;
                case JoltStatement statement:
                    Copy(statement.CodeSpan);
                    Write("\n");
                    break;
                case JoltBlock block:
                    Write("{\n");
                    EmitNodes(block.Children, target);
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

    private void EmitConditionalBranch(JoltIf conditional, IEnumerable<JoltNode> children, string target, string branch)
    {
        if (_referenceLoopDepth > 0)
        {
            // Keep authored row keys visible to Vue across iterations.
            EmitNodes(children, target);
            return;
        }
        // Vue keeps the Fragment's children array by reference. Append the
        // Fragment before emitting its contents so break/continue retain nodes
        // added before the transfer. Each branch occupies one sibling position.
        var nodes = BeginNodes();
        Append(target, conditional, () =>
        {
            Call(new(IntrinsicKind.ConditionalBranch, conditional.Span, $"$joltIf{conditional.Span.Start}:{branch}"),
                "object", "object[] children");
            Write(nodes + ")");
        });
        EmitNodes(children, nodes);
    }

    private JoltElement? SingleKeyedRow(JoltForEach loop)
    {
        JoltElement? row = null;
        foreach (var node in loop.Children)
        {
            if (node is JoltText text && string.IsNullOrWhiteSpace(text.Text) &&
                string.IsNullOrWhiteSpace(Source.ToString(text.Span)))
                continue;
            if (row is not null || node is not JoltElement element ||
                !element.Attributes.Any(attribute => attribute.Kind == JoltAttributeKind.Key))
                return null;
            row = element;
        }
        return row;
    }

    private void EmitElement(JoltElement element)
    {
        if (element.Name == "component")
        {
            EmitDynamicComponent(element);
            return;
        }

        if (element.Name == "slot")
        {
            ReportSlotDirectives(element);
            var nameAttribute = element.Attributes.FirstOrDefault(attribute => attribute.Name == "name");
            string name = ComponentContract.Text(nameAttribute) ?? "default";
            if (nameAttribute?.Value is [JoltText outlet] && outlet.Span.Length > 0)
                MarkupBindings.Add(new(new(Document.Path, JoltMarkupKind.Slot, name), outlet.Span, IsDeclaration: true));
            Call(new(IntrinsicKind.Outlet, element.Span, name), "object", $"{StateType} state, object[] props, global::System.Func<object> fallback");
            Write("this, ");
            EmitProperties(element.Attributes.Where(attribute => attribute.Name != "name" && attribute.Kind == JoltAttributeKind.Markup), null, element);
            Write(", ");
            EmitSlotBody(element.Children);
            Write(")");
            return;
        }

        var candidates = _components.Where(component => component.Name == element.Name).ToArray();
        var component = candidates.Length switch
        {
            0 => VueBuiltIns.Find(element.Name),
            1 => candidates[0],
            _ => null,
        };
        if (candidates.Length > 1)
            Report("JOLT1002", element.NameSpan, $"Component '{element.Name}' is ambiguous.");
        else if (component is null && char.IsUpper(element.Name[0]))
            Report("JOLT1001", element.NameSpan, $"Component '{element.Name}' was not found.");

        if (component is { Npm: null, IsVueBuiltIn: false })
        {
            var symbol = new JoltMarkupSymbol(component.Path, JoltMarkupKind.Component);
            MarkupBindings.Add(new(symbol, element.NameSpan));
            if (element.ClosingNameSpan is { } closing)
                MarkupBindings.Add(new(symbol, closing));
        }

        var bound = component;
        var attributes = element.Attributes.AsEnumerable();
        bool typeProbe = false;
        if (component is { TypeParameters.Count: > 0 })
        {
            var explicitTypes = component.TypeParameters.Select(parameter => element.Attributes.FirstOrDefault(attribute =>
                attribute.Kind == JoltAttributeKind.Markup && attribute.Name == parameter.Name)).ToArray();
            attributes = attributes.Where(attribute => !explicitTypes.Contains(attribute));
            if (_probeTypeArguments)
                typeProbe = EmitTypeProbe(element, component, explicitTypes);
            var typeBinding = _typeArguments?.GetValueOrDefault((element.NameSpan.Start, _dynamicComponentPath));
            var argument = typeBinding?.Arguments;
            if (!_probeTypeArguments && _typeArguments is not null && argument is null)
            {
                var explicitType = explicitTypes.FirstOrDefault(attribute => attribute is not null);
                Report("JOLT1021", typeBinding?.ErrorSource ?? explicitType?.Value.FirstOrDefault()?.Span ?? explicitType?.NameSpan ?? element.NameSpan,
                    typeBinding?.Error ?? $"Cannot bind type parameters {string.Join(", ", component.TypeParameters.Select(parameter => parameter.Name))} of '{element.Name}'. Supply all type arguments or typed props that satisfy the declared constraints.");
            }
            // The probe only binds input expressions. A failed final binding already
            // has a type-parameter diagnostic at its author span.
            bound = component.Specialize(argument ?? component.TypeParameters.Select(_ => "object").ToArray());
        }
        Call(new(IntrinsicKind.Element, element.NameSpan, element.Name, component), "object", "object[] props, object children");
        EmitProperties(attributes, bound, element);
        Write(", ");
        if (component is null || component == VueBuiltIns.Teleport)
        {
            if (component == VueBuiltIns.Teleport)
                ReportTeleportSlots(element);
            bool wasBoundSelect = _insideBoundSelect;
            if (element.Name == "select")
                _insideBoundSelect = JoltFormBindings.HasBinding(element);
            EmitSlotBody(element.Children, invoke: true);
            _insideBoundSelect = wasBoundSelect;
        }
        else
            EmitSlots(bound!, element);
        Write(typeProbe ? "))" : ")");
    }

    private bool EmitTypeProbe(JoltElement element, ComponentContract component, JoltAttribute?[] explicitTypes)
    {
        var method = IntrinsicPrefix + "Infer" + _typeProbes.Count;
        bool hasExplicit = explicitTypes.Any(attribute => attribute is not null);
        var witnesses = new List<(string Type, JoltAttribute Attribute)>();
        if (hasExplicit)
        {
            if (explicitTypes.Any(attribute => attribute?.Value is not [JoltText explicitText] ||
                SyntaxFactory.ParseTypeName(explicitText.Text) is not { ContainsDiagnostics: false } parsed ||
                parsed.FullSpan.Length != explicitText.Text.Length))
                return false;
        }
        else
        {
            foreach (var property in component.Properties)
            {
                if (property.Type is not { } type)
                    continue;
                if (component.Npm is not null)
                {
                    var name = component.TypeParameters[0].Name;
                    if (type != name + "[]" && type != $"global::System.Collections.Generic.IReadOnlyList<{name}>")
                        continue;
                }
                else if (!SyntaxFactory.ParseTypeName(type).DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
                    .Any(identifier => component.TypeParameters.Any(parameter => parameter.Name == identifier.Identifier.ValueText) &&
                        identifier.Parent is not QualifiedNameSyntax and not AliasQualifiedNameSyntax))
                    continue;
                var attribute = element.Attributes.FirstOrDefault(candidate => !explicitTypes.Contains(candidate) &&
                    (candidate.Kind == JoltAttributeKind.Markup &&
                        string.Equals(candidate.Name, property.Name, StringComparison.OrdinalIgnoreCase) ||
                     candidate is { Kind: JoltAttributeKind.Bind, Target: {} model, Modifier: null or "get" } &&
                        component.FindModel(model)?.Local?.Property == property.Name));
                if (attribute is not null && (component.Npm is null || attribute.Value is [JoltExpression]))
                    witnesses.Add((type, attribute));
                if (component.Npm is not null)
                    break;
            }
        }
        if (!hasExplicit && witnesses.Count == 0)
            return false;
        // The probe is a separate expression in a well-formed call. Only its
        // constructed Roslyn symbol is read, and no probe reaches lowering.
        if (_typeProbes.Count == 0)
            _methods.Add("private static object __JoltBindType(object witness, object element) => element;\n");
        Write("__JoltBindType(");
        var start = _code.Length;
        Write(method);
        if (hasExplicit)
        {
            Write("<");
            for (int index = 0; index < explicitTypes.Length; index++)
            {
                if (index != 0)
                    Write(", ");
                Copy(((JoltText)explicitTypes[index]!.Value[0]).Span);
            }
            Write(">");
        }
        Write("(");
        for (int index = 0; index < witnesses.Count; index++)
        {
            if (index != 0)
                Write(", ");
            EmitAttributeValue(witnesses[index].Attribute, null, false);
        }
        Write(")");
        _typeProbes.Add((TextSpan.FromBounds(start, _code.Length), (element.NameSpan.Start, _dynamicComponentPath), component, explicitTypes));
        Write(", ");
        var parameters = string.Join(", ", component.TypeParameters.Select(parameter => parameter.Name));
        var constraints = string.Concat(component.TypeParameters.Select(parameter => parameter.Constraint));
        var signature = string.Join(", ", witnesses.Select((witness, index) => $"{witness.Type} value{index}"));
        _methods.Add($"private static object {method}<{parameters}>({signature}){constraints} => throw new global::System.NotSupportedException();\n");
        return true;
    }

    private void EmitProperties(IEnumerable<JoltAttribute> attributes, ComponentContract? component, JoltElement element)
    {
        Write("new object[] {");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var form = component is null && JoltFormBindings.HasBinding(element) ? JoltFormBindings.Get(element) : null;
        foreach (var attribute in attributes)
        {
            var kind = attribute.Kind;
            var eventName = attribute.Target;
            if (kind == JoltAttributeKind.EventModifier && component is not null &&
                component.Properties.Any(property => property.Name == ComponentContract.EventProperty(attribute.Name[3..])))
            {
                // A Vue component can declare a colon in its public event name.
                kind = JoltAttributeKind.Event;
                eventName = attribute.Name[3..];
            }
            switch (kind)
            {
                case JoltAttributeKind.UnsupportedDirective:
                    continue; // Syntax has already reported the directive at its authored name.
                case JoltAttributeKind.Bind:
                case JoltAttributeKind.BindEvent:
                    EmitBinding(attribute, component, element, seen);
                    continue;
                case JoltAttributeKind.Key:
                    EmitKey(attribute, seen);
                    continue;
                case JoltAttributeKind.Attributes:
                    EmitAttributeSplat(attribute);
                    continue;
                case JoltAttributeKind.Ref:
                    EmitReference(attribute, component, element, seen);
                    continue;
                case JoltAttributeKind.EventModifier:
                    if (component is null)
                        EmitStandaloneEventModifier(attribute, element, seen);
                    else
                        Report("JOLT1024", attribute.NameSpan, "DOM event modifiers apply to native elements.");
                    continue;
            }

            var domEvent = kind == JoltAttributeKind.Event;
            string input = domEvent ? ComponentContract.EventProperty(eventName!) : attribute.Name;
            var property = component?.Properties.FirstOrDefault(prop => prop.Name == input) ??
                component?.Properties.FirstOrDefault(prop => string.Equals(prop.Name, input, StringComparison.OrdinalIgnoreCase));
            var name = property?.Name ?? input;
            if (!seen.Add(name))
                Report("JOLT1003", attribute.NameSpan, $"Attribute '{attribute.Name}' is specified more than once.");
            if (domEvent && component is null)
            {
                EmitDomEvent(attribute, name, element);
                continue;
            }
            if (component is not null && property is null)
                Report("JOLT1004", attribute.NameSpan, $"Component '{component.Name}' has no prop or event '{attribute.Name}'.");
            if (component?.Npm is { } npm && property is { Type: null })
                Report("JOLT1014", attribute.NameSpan,
                    $"{npm.DisplayName} member '{property.Name}' has an unsupported type ({property.Unsupported}).");
            if (component is { Npm: null, IsVueBuiltIn: false } && property is not null)
                MarkupBindings.Add(new(new(component.Path, JoltMarkupKind.Property, property.Name), attribute.NameSpan));

            var span = attribute.Value.OfType<JoltExpression>().FirstOrDefault()?.CodeSpan ??
                attribute.Value.OfType<JoltText>().FirstOrDefault()?.Span ?? attribute.NameSpan;
            int generatedStart = _code.Length;
            bool stringValue = component is null && name.Equals("value", StringComparison.OrdinalIgnoreCase) &&
                (form == JoltFormBinding.Radio || (_insideBoundSelect && element.Name == "option"));
            bool multiple = component is null && name.Equals("multiple", StringComparison.OrdinalIgnoreCase) &&
                form == JoltFormBinding.SelectMultiple;
            string propName = stringValue ? "value" : multiple ? "multiple" : name;
            Call(new(IntrinsicKind.Prop, span, propName), "object", $"{property?.Type ?? (stringValue ? "string" : "object?")} value");
            if (multiple)
                Write("true");
            else
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

    private void EmitKey(JoltAttribute attribute, HashSet<string> seen)
    {
        if (!seen.Add("key"))
            Report("JOLT1003", attribute.NameSpan, "The VNode key is specified more than once.");
        if (attribute.Value is not [JoltExpression expression] || string.IsNullOrWhiteSpace(expression.Code))
        {
            Report("JOLT1025", attribute.NameSpan, "@key requires a C# key expression.");
            return;
        }
        // The key belongs to Vue's VNode, including when the tag resolves to a component.
        Call(new(IntrinsicKind.Prop, expression.CodeSpan, "key"), "object", "global::ECMAScript.PropertyKeyRef value");
        Copy(expression.CodeSpan);
        Write("), ");
    }

    private void ReportSlotDirectives(JoltElement element)
    {
        foreach (var attribute in element.Attributes.Where(attribute =>
            attribute.Kind is not (JoltAttributeKind.Markup or JoltAttributeKind.UnsupportedDirective)))
            Report("JOLT1022", attribute.NameSpan, $"Use '{attribute.Name}' on a rendered element or component inside the slot.");
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
            if (property?.LiteralExpressions is { } expressions)
            {
                if (expressions.TryGetValue(value, out var mapped))
                    Write(mapped);
                else
                {
                    Report("JOLT1006", text.Span, $"Prop '{property.Name}' expects one of {string.Join(", ", expressions.Keys)}.");
                    Write("default");
                }
                return;
            }
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
        var defaultNodes = new List<JoltNode>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in element.Children)
        {
            if (node is not JoltElement template)
            {
                defaultNodes.Add(node);
                continue;
            }

            var directive = template.Name == "template"
                ? template.Attributes.FirstOrDefault(attribute => attribute.Name.StartsWith('#'))
                : null;
            var slot = directive?.Name[1..] ?? FindNamedChildSlot(component, template);
            if (slot is null)
            {
                defaultNodes.Add(node);
                continue;
            }

            var span = directive?.NameSpan ?? template.NameSpan;
            if (directive is null && _components.Any(candidate => candidate.Name == template.Name))
            {
                Report("JOLT1007", span, $"'{template.Name}' names both a child component and a slot. Use <template #{slot}> to select the slot or place the component inside <ChildContent>.");
                continue;
            }
            ReportSlotDirectives(template);
            if (component is { Npm: null, IsVueBuiltIn: false } && component.Slots.Any(candidate => candidate.Name == slot))
            {
                var symbol = new JoltMarkupSymbol(component.Path, JoltMarkupKind.Slot, slot);
                MarkupBindings.Add(new(symbol, directive is null ? span : TextSpan.FromBounds(span.Start + 1, span.End)));
                if (directive is null && template.ClosingNameSpan is { } closing)
                    MarkupBindings.Add(new(symbol, closing));
            }
            var context = template.Attributes.FirstOrDefault(attribute => attribute.Name == (directive is null ? "Context" : "context"));
            if (directive is null)
            {
                foreach (var attribute in template.Attributes.Where(attribute =>
                    attribute.Kind == JoltAttributeKind.Markup && attribute != context))
                    Report("JOLT1007", attribute.NameSpan, "A named slot child accepts only its Context attribute.");
            }
            EmitSlot(template.Children, slot, span, context, component, seen);
        }
        if (defaultNodes.Any(node => node is not JoltText text || !string.IsNullOrWhiteSpace(text.Text)))
            EmitSlot(defaultNodes, "default", element.NameSpan, null, component, seen);
        foreach (var slot in component.Slots.Where(slot => slot.Required && !seen.Contains(slot.Name)))
            Report("JOLT1008", element.NameSpan, $"Component '{component.Name}' requires slot '{slot.Name}'.");
        Write("}");
    }

    private static string? FindNamedChildSlot(ComponentContract component, JoltElement child)
    {
        if (!char.IsUpper(child.Name[0]))
            return null;
        var name = child.Name;
        if (name == "ChildContent" && component.Slots.Any(slot => slot.Name == "default"))
            return "default";
        if (component.Slots.Any(slot => slot.Name == name))
            return name;
        name = char.ToLowerInvariant(name[0]) + name[1..];
        return component.Slots.Any(slot => slot.Name == name) ? name : null;
    }

    private void EmitSlot(IEnumerable<JoltNode> nodes, string name, TextSpan span, JoltAttribute? context, ComponentContract component, HashSet<string> seen)
    {
        if (component == VueBuiltIns.Transition && name == "default")
            nodes = TransitionChildren(nodes, span);
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
                Report("JOLT1015", span, $"{npm.DisplayName} slot '{name}' has an unsupported scope ({unsupported}).");
            var scopeType = "__JoltScope" + _scopes.Count;
            var members = slot?.Scope ?? [];
            // Scope properties live in the consumer projection but originate at the declaring outlet.
            foreach (var member in members)
                if (member.Source is { } origin)
                    SlotMemberOrigins.Add((scopeType, member.Name), (component.Path, origin));
            if (component.Npm?.Slots.GetValueOrDefault(name)?.Signatures is [[{ PropertyDefinitions: { } origins }]])
                foreach (var (member, origin) in origins)
                    ExternalSlotMemberOrigins.Add((scopeType, member), origin);
            _scopes.Add($"public sealed class {scopeType} {{\n" + string.Join("\n", members.Where(member => SyntaxFacts.IsValidIdentifier(member.Name))
                .Select(member => ScopeMember(component, name, member))) + "\n}\n");
            signature = $"global::System.Func<{scopeType}, object>";
        }
        else if (context is not null)
            Report("JOLT1010", context.Span, "Slot context must be a C# identifier.");

        Call(new(IntrinsicKind.Slot, span, name), "object", $"{signature} body");
        var parameterNode = parameter is not null && SyntaxFacts.IsValidIdentifier(parameter) ? (JoltText)context!.Value[0] : null;
        EmitSlotBody(nodes, parameter: parameterNode);
        Write("), ");
    }

    private static string ScopeMember(ComponentContract component, string slot, ComponentProperty member)
    {
        string attribute = member.Type is null && component.Npm is { } npm
            ? $"[global::System.Obsolete({Literal($"{npm.DisplayName} slot '{slot}' member '{member.Name}' has an unsupported type ({member.Unsupported}).")}, true)]\n"
            : "";
        return attribute + $"public {member.Type ?? "object"} {member.Name} {{ get; set; }} = default!;";
    }

    private void EmitSlotBody(IEnumerable<JoltNode> children, bool invoke = false, JoltText? parameter = null)
    {
        if (invoke)
        {
            // Evaluate normal HTML children immediately, with the same statement scope as slots.
            Write("((global::System.Func<object>)(() => {\n");
        }
        else if (parameter is not null)
        {
            Spans.Add(new JoltProjectionSpan(parameter.Span, new TextSpan(_code.Length, parameter.Text.Length)));
            Write(parameter.Text);
            Write(" => {\n");
        }
        else
            Write("() => {\n");
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
        var cursor = span.Start;
        foreach (var template in Document.InlineTemplates.Where(template => span.Contains(template.Span)))
        {
            if (template.Span.Start < cursor)
                continue;
            CopySource(TextSpan.FromBounds(cursor, template.Span.Start));
            Call(new(IntrinsicKind.Fragment, template.Span), "global::JoltAuthoring.RenderedFragment", "object children");
            if (_componentsAvailable)
                EmitSlotBody(template.Children, invoke: true);
            else
                Write("null!");
            Write(")");
            cursor = template.Span.End;
        }
        CopySource(TextSpan.FromBounds(cursor, span.End));
    }

    private void CopySource(TextSpan span)
    {
        Spans.Add(new JoltProjectionSpan(span, new TextSpan(_code.Length, span.Length)));
        Write(Source.ToString(span));
    }

    private string MaskInlineTemplates(JoltCode code)
    {
        if (!Document.InlineTemplates.Any(template => code.CodeSpan.Contains(template.Span)))
            return code.Code;
        var text = code.Code.ToCharArray();
        foreach (var template in Document.InlineTemplates.Where(template => code.CodeSpan.Contains(template.Span)))
        {
            var start = template.Span.Start - code.CodeSpan.Start;
            for (var index = start; index < start + template.Span.Length; index++)
                if (text[index] is not ('\r' or '\n'))
                    text[index] = ' ';
            "null".CopyTo(0, text, start, Math.Min(4, template.Span.Length));
        }
        return new string(text);
    }

    private void Write(string text) => _code.Append(text);
    private static string Literal(string text) => SymbolDisplay.FormatLiteral(text, true);
    private void Report(string id, TextSpan span, string message)
        => diagnostics.Add(JoltProjectCompiler.Error(id, Location(span), message));
}
