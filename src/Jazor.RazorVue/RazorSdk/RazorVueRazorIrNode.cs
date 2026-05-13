using System.Collections.Immutable;

namespace Jazor.RazorVue.RazorSdk;

internal enum RazorVueRazorIrNodeKind
{
    Unknown,
    Document,
    NamespaceDeclaration,
    ClassDeclaration,
    MethodDeclaration,
    MarkupElement,
    Component,
    HtmlContent,
    CSharpExpression,
    MarkupBlock,
    TagHelperBody,
    CSharpCode,
    FieldDeclaration,
    PropertyDeclaration,
    UsingDirective,
    Directive,
    MalformedDirective,
    Extension,
    TagHelper,
    HtmlAttribute,
    ComponentAttribute,
    Splat,
    CSharpExpressionAttributeValue,
    CSharpCodeAttributeValue,
    HtmlAttributeValue,
    IntermediateToken
}

internal sealed record RazorVueRazorIrToken(
    string Content,
    RazorVueRazorSourceSpan? Source);

internal sealed record RazorVueRazorIrNode(
    RazorVueRazorIrNodeKind Kind,
    string RuntimeTypeName,
    ImmutableArray<RazorVueRazorIrNode> Children,
    ImmutableArray<RazorVueRazorIrToken> Tokens,
    RazorVueRazorSourceSpan? Source,
    string? TagName = null,
    string? TypeName = null,
    string? MethodName = null,
    string? AttributeName = null,
    string? ParameterName = null,
    bool IsParameterized = false,
    bool IsDesignTimePropertyAccessHelper = false,
    bool IsSynthesized = false,
    bool HasAttributeNameExpression = false,
    string? Content = null,
    RazorVueRazorSourceSpan? StartTagSpan = null,
    ImmutableArray<RazorVueRazorIrNode> Attributes = default,
    ImmutableArray<RazorVueRazorIrNode> Body = default,
    ImmutableArray<RazorVueRazorIrNode> Splats = default,
    ImmutableArray<RazorVueRazorIrNode> ChildContents = default,
    ImmutableArray<RazorVueRazorIrNode> Captures = default,
    ImmutableArray<RazorVueRazorIrNode> SetKeys = default,
    string? Prefix = null,
    string? Suffix = null)
{
    public ImmutableArray<RazorVueRazorIrNode> AttributesOrEmpty
        => Attributes.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : Attributes;

    public ImmutableArray<RazorVueRazorIrNode> BodyOrEmpty
        => Body.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : Body;

    public ImmutableArray<RazorVueRazorIrNode> SplatsOrEmpty
        => Splats.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : Splats;

    public ImmutableArray<RazorVueRazorIrNode> ChildContentsOrEmpty
        => ChildContents.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : ChildContents;

    public ImmutableArray<RazorVueRazorIrNode> CapturesOrEmpty
        => Captures.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : Captures;

    public ImmutableArray<RazorVueRazorIrNode> SetKeysOrEmpty
        => SetKeys.IsDefault ? ImmutableArray<RazorVueRazorIrNode>.Empty : SetKeys;
}
