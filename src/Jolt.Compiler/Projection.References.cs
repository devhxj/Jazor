using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    private readonly List<(TextSpan Generated, TextSpan Source)> _referenceChecks = [];
    internal Dictionary<int, string> ReferenceTargets { get; } = [];
    private int _referenceLoopDepth;
    private readonly List<(string Name, TextSpan Signature, TextSpan Identifier)> _exposedDeclarations = [];
    private readonly List<TextSpan> _invalidExposedDeclarations = [];
    internal string? ReferenceType => _exposedDeclarations.Count == 0 ? null : $"global::{Namespace}.{Name}Ref";

    private void CollectExposedDeclarations(ClassDeclarationSyntax declaration, int offset)
    {
        foreach (var method in declaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (!method.AttributeLists.SelectMany(list => list.Attributes).Any(attribute =>
                attribute.Name.ToString() is "Expose" or "ExposeAttribute" or "JoltAuthoring.Expose" or "JoltAuthoring.ExposeAttribute"
                    or "global::JoltAuthoring.Expose" or "global::JoltAuthoring.ExposeAttribute"))
                continue;
            var identifier = new TextSpan(offset + method.Identifier.SpanStart, method.Identifier.Span.Length);
            if (!method.Modifiers.Any(SyntaxKind.PublicKeyword) || method.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                method.ExplicitInterfaceSpecifier is not null)
            {
                _invalidExposedDeclarations.Add(identifier);
                continue;
            }
            int end = method.ConstraintClauses.LastOrDefault()?.Span.End ?? method.ParameterList.Span.End;
            _exposedDeclarations.Add((method.Identifier.ValueText,
                TextSpan.FromBounds(offset + method.ReturnType.SpanStart, offset + end), identifier));
        }
    }

    private void EmitReferenceContract()
    {
        if (_exposedDeclarations.Count == 0)
            return;
        Write($"public interface {Name}Ref");
        if (TypeParameters.Length > 0)
        {
            Write($"<{string.Join(", ", TypeParameters.Select(parameter => parameter.Name))}>");
            foreach (var parameter in TypeParameters)
                if (parameter.ConstraintSpan is { } constraint)
                {
                    Write(" ");
                    Copy(constraint);
                }
        }
        Write(" {\n");
        foreach (var declaration in _exposedDeclarations)
        {
            Copy(declaration.Signature);
            Write(";\n");
        }
        Write("}\n");
    }

    internal IReadOnlyList<IMethodSymbol> ExposedMethods(INamedTypeSymbol state) => _exposedDeclarations.Count == 0 ? [] :
        state.GetMembers().OfType<IMethodSymbol>()
            .Where(method => _exposedDeclarations.Any(declaration => declaration.Name == method.Name) && method.GetAttributes().Any(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "JoltAuthoring.ExposeAttribute")).ToArray();

    private void EmitReference(JoltAttribute attribute, ComponentContract? component, JoltElement element, HashSet<string> seen)
    {
        if (_referenceLoopDepth > 0)
        {
            Report("JOLT1028", attribute.NameSpan, "@ref captures one instance. Move it outside @for or @foreach, or capture it in a child component.");
            return;
        }
        if (!seen.Add("ref"))
            Report("JOLT1003", attribute.NameSpan, $"Attribute '{attribute.Name}' is specified more than once.");
        if (attribute.Value is not [JoltExpression { Code.Length: > 0 } expression])
        {
            Report("JOLT1028", attribute.NameSpan, "@ref requires a writable nullable component member.");
            return;
        }

        string type;
        if (component is null)
            type = "global::ECMAScript." + NativeReferenceType(element.Name);
        else if (component.ReferenceType is { } referenceType)
        {
            type = referenceType;
            if (component.TypeParameters.Count > 0)
            {
                // Generic inference probes bind input props first. The final projection
                // can then bind the reference to the constructed interface.
                if (_typeArguments?.GetValueOrDefault((element.NameSpan.Start, _dynamicComponentPath))?.Arguments is not { } arguments)
                    return;
                type += $"<{string.Join(", ", arguments)}>";
            }
        }
        else
        {
            Report("JOLT1028", attribute.NameSpan, $"Component '{element.Name}' has no typed reference contract. Declare a public [Expose] method on a local component.");
            return;
        }

        Call(new(IntrinsicKind.Ref, attribute.NameSpan, _referenceChecks.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            "object", $"{StateType} state, global::System.Func<object?> get, global::System.Action<{type}?> set");
        Write("this, () => ");
        int start = _code.Length;
        Copy(expression.CodeSpan);
        _referenceChecks.Add((TextSpan.FromBounds(start, _code.Length), expression.CodeSpan));
        Write(", __joltRefValue => { if (!global::System.Object.ReferenceEquals(");
        Copy(expression.CodeSpan);
        Write(", __joltRefValue)) ");
        Copy(expression.CodeSpan);
        Write(" = __joltRefValue; }), ");
    }

    internal void CheckReferences(SemanticModel model, INamedTypeSymbol state, List<Diagnostic> validationDiagnostics, bool prepareLowering)
    {
        foreach (var identifier in _invalidExposedDeclarations)
            ReportValidation(validationDiagnostics, "JOLT1028", identifier, "[Expose] requires a public instance method.");
        var exposed = ExposedMethods(state);
        foreach (var (name, _, identifier) in _exposedDeclarations)
        {
            if (!exposed.Any(method => method.Name == name))
                ReportValidation(validationDiagnostics, "JOLT1028", identifier, "Use JoltAuthoring.ExposeAttribute to declare a component reference method.");
            else if (state.GetMembers(name).OfType<IMethodSymbol>().Skip(1).Any())
                ReportValidation(validationDiagnostics, "JOLT1028", identifier, "[Expose] requires a unique method name.");
        }

        var root = model.SyntaxTree.GetRoot();
        for (int index = 0; index < _referenceChecks.Count; index++)
        {
            var (generated, source) = _referenceChecks[index];
            var target = root.FindNode(generated, getInnermostNodeForTie: true);
            var operation = model.GetOperation(target);
            var member = operation switch
            {
                IFieldReferenceOperation { Instance: IInstanceReferenceOperation } field => (ISymbol)field.Field,
                IPropertyReferenceOperation { Instance: IInstanceReferenceOperation } property => property.Property,
                _ => null,
            };
            if (member is null && operation is null or IInvalidOperation)
                continue; // Roslyn already reports an invalid assignment or missing target.
            bool valid = member switch
            {
                IFieldSymbol { IsReadOnly: false, NullableAnnotation: NullableAnnotation.Annotated } => true,
                IPropertySymbol { GetMethod: not null, SetMethod.IsInitOnly: false, NullableAnnotation: NullableAnnotation.Annotated } property
                    when property.GetAttributes().All(attribute => attribute.AttributeClass?.Name != "ParameterAttribute") => true,
                _ => false,
            };
            if (!valid || !SymbolEqualityComparer.Default.Equals(member?.ContainingType, state))
                ReportValidation(validationDiagnostics, "JOLT1028", source, "@ref requires a writable nullable instance field or property on this component.");
            else if (prepareLowering)
                ReferenceTargets.Add(index, member!.Name);
        }
    }

    private static string NativeReferenceType(string name) => name.ToLowerInvariant() switch
    {
        "a" => "HTMLAnchorElement",
        "blockquote" or "q" => "HTMLQuoteElement",
        "caption" => "HTMLTableCaptionElement",
        "col" or "colgroup" => "HTMLTableColElement",
        "del" or "ins" => "HTMLModElement",
        "fieldset" => "HTMLFieldSetElement",
        "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => "HTMLHeadingElement",
        "img" => "HTMLImageElement",
        "td" or "th" => "HTMLTableCellElement",
        "thead" or "tbody" or "tfoot" => "HTMLTableSectionElement",
        "tr" => "HTMLTableRowElement",
        "textarea" => "HTMLTextAreaElement",
        "iframe" => "HTMLIFrameElement",
        "datalist" => "HTMLDataListElement",
        "optgroup" => "HTMLOptGroupElement",
        "br" or "dl" or "hr" or "li" or "ol" or "ul" => "HTML" + name.ToUpperInvariant() + "Element",
        "area" or "audio" or "base" or "body" or "button" or "canvas" or "data" or "details" or "dialog" or
        "div" or "embed" or "form" or "head" or "html" or "input" or "label" or "legend" or "link" or "map" or
        "menu" or "meta" or "meter" or "object" or "option" or "output" or "picture" or "pre" or "progress" or
        "script" or "select" or "source" or "span" or "style" or "table" or "template" or "time" or "title" or
        "track" or "video" => "HTML" + char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant() + "Element",
        "p" => "HTMLParagraphElement",
        "abbr" or "address" or "article" or "aside" or "b" or "bdi" or "bdo" or "cite" or "code" or
        "dd" or "dfn" or "dt" or "em" or "figcaption" or "figure" or "footer" or "header" or "hgroup" or
        "i" or "kbd" or "main" or "mark" or "nav" or "noscript" or "rp" or "rt" or "ruby" or "s" or
        "samp" or "search" or "section" or "small" or "strong" or "sub" or "summary" or "sup" or "u" or
        "var" or "wbr" => "HTMLElement",
        // Element is also a valid base for custom elements and SVG descendants.
        _ => "Element",
    };
}
