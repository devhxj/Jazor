using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Syntax;

public sealed record JoltDocument(string Source, string Path, ImmutableArray<JoltNode> Nodes, ImmutableArray<JoltDiagnostic> Diagnostics)
{
    public ImmutableArray<JoltInlineTemplate> InlineTemplates { get; init; } = [];
}
public sealed record JoltInlineTemplate(ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span);
public sealed record JoltDiagnostic(string Id, string Message, DiagnosticSeverity Severity, TextSpan Span);
public abstract record JoltNode(TextSpan Span);
public sealed record JoltElement(string Name, TextSpan NameSpan, ImmutableArray<JoltAttribute> Attributes, ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span)
{
    public TextSpan? ClosingNameSpan { get; init; }
}
public enum JoltAttributeKind
{
    Markup,
    Event,
    EventModifier,
    Bind,
    BindEvent,
    Key,
    Ref,
    UnsupportedDirective,
    Attributes,
}
public sealed record JoltAttribute(string Name, TextSpan NameSpan, ImmutableArray<JoltNode> Value, TextSpan Span) : JoltNode(Span)
{
    public JoltAttributeKind Kind { get; init; }
    public string? Target { get; init; }
    public string? Modifier { get; init; }
}
public sealed record JoltText(string Text, TextSpan Span) : JoltNode(Span);
public sealed record JoltExpression(string Code, TextSpan CodeSpan, TextSpan Span) : JoltNode(Span);
public sealed record JoltIf(string Condition, TextSpan ConditionSpan, ImmutableArray<JoltNode> Then, ImmutableArray<JoltNode> Else, TextSpan Span) : JoltNode(Span);
public sealed record JoltForEach(string Header, TextSpan HeaderSpan, ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span);
public sealed record JoltFor(string Header, TextSpan HeaderSpan, ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span);
public sealed record JoltSwitch(string Expression, TextSpan ExpressionSpan, ImmutableArray<JoltSwitchSection> Sections, TextSpan Span) : JoltNode(Span);
public sealed record JoltSwitchSection(ImmutableArray<JoltSwitchLabel> Labels, ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span);
public sealed record JoltSwitchLabel(string Code, TextSpan CodeSpan);
public sealed record JoltStatement(string Code, TextSpan CodeSpan, TextSpan Span) : JoltNode(Span);
public sealed record JoltBlock(ImmutableArray<JoltNode> Children, TextSpan Span) : JoltNode(Span);
public sealed record JoltUsing(string Code, TextSpan CodeSpan, TextSpan Span) : JoltNode(Span);
public sealed record JoltRouteDirective(string Name, TextSpan NameSpan, string Argument, TextSpan ArgumentSpan, TextSpan Span) : JoltNode(Span);
public sealed record JoltCode(string Code, TextSpan CodeSpan, TextSpan Span) : JoltNode(Span);
public sealed record JoltScopedStyle(string Css, TextSpan CssSpan, TextSpan Span) : JoltNode(Span)
{
    public bool Scoped { get; init; } = true;
}
public sealed record JoltTypeParameter(string Name, TextSpan NameSpan, TextSpan Span) : JoltNode(Span)
{
    public TextSpan? ConstraintSpan { get; init; }
}
