using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RenderTree;
using Jazor.RazorVue;

namespace Jazor.RazorVue.Canonical;

internal sealed record RazorVueCanonicalHComponentModel(
    string ComponentName,
    string ComponentFullName,
    string RelativeComponentPath,
    VueComponentDescriptor Descriptor,
    ImmutableArray<string> Imports,
    ImmutableArray<RazorVueCompilerImportBinding> CompilerImports,
    ImmutableArray<string> Styles,
    ImmutableArray<string> PluginRequirements,
    VueRuntimeHints Hints,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins,
    RazorVueCanonicalTemplateFragment Template,
    RazorVueCanonicalSetupModel Setup);

internal sealed record RazorVueCanonicalSetupModel(
    ImmutableArray<VueLogicFieldDescriptor> Fields,
    ImmutableArray<VueLogicMethodDescriptor> Methods,
    ImmutableArray<VueLogicFieldDescriptor> RequiredFields,
    ImmutableArray<VueLogicMethodDescriptor> RequiredMethods,
    VueLifecycleDescriptor Lifecycle);

internal sealed record RazorVueCanonicalTemplateFragment(
    ImmutableArray<RazorVueCanonicalTemplateNode> Children)
{
    public static RazorVueCanonicalTemplateFragment Empty { get; } =
        new(ImmutableArray<RazorVueCanonicalTemplateNode>.Empty);
}

internal abstract record RazorVueCanonicalTemplateNode(
    RazorVueCanonicalNodeKind NodeKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueCanonicalElementNode(
    string TagName,
    ImmutableArray<RazorVueCanonicalAttributeEntry> Attributes,
    RazorVueCanonicalTemplateFragment Children,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Element, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    VueComponentDescriptor? ResolvedDescriptor,
    ImmutableArray<RazorVueCanonicalAttributeEntry> Attributes,
    ImmutableArray<RazorVueCanonicalSlotBinding> Slots,
    RazorVueCanonicalTemplateFragment Children,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Component, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalTextNode(
    string Text,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Text, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalInterpolationNode(
    string ExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Interpolation, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalConditionalNode(
    string ConditionExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueCanonicalTemplateFragment WhenTrue,
    RazorVueCanonicalTemplateFragment WhenFalse,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Conditional, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalForEachNode(
    string ItemName,
    string SourceExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueCanonicalTemplateFragment Body,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.ForEach, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalForNode(
    string VariableName,
    string InitialValueExpressionText,
    RazorVueExpressionBindingKind InitialValueBindingKind,
    RazorVueForConditionKind ConditionKind,
    string LimitValueExpressionText,
    RazorVueExpressionBindingKind LimitValueBindingKind,
    RazorVueForStepKind StepKind,
    string? StepValueExpressionText,
    RazorVueExpressionBindingKind StepValueBindingKind,
    RazorVueCanonicalTemplateFragment Body,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.For, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalSlotOutletNode(
    string SlotName,
    string? ArgumentExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.SlotOutlet, TemplateEncodability, SideEffectClassification, SourceOrigins);

internal abstract record RazorVueCanonicalAttributeEntry(
    RazorVueTemplateEncodability TemplateEncodability,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueCanonicalAttributeBinding(
    string Name,
    string? ExpressionText,
    RazorVueLiteralValueKind LiteralValueKind,
    RazorVueCanonicalAttributeKind AttributeKind,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalAttributeEntry(TemplateEncodability, SourceOrigins);

internal sealed record RazorVueCanonicalAttributeSpreadBinding(
    string ExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalAttributeEntry(TemplateEncodability, SourceOrigins);

internal sealed record RazorVueCanonicalSlotBinding(
    string SlotName,
    bool IsDefault,
    string? ParameterName,
    RazorVueCanonicalSlotValueKind ValueKind,
    string? ValueExpressionText,
    string? ForwardedSlotName,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueCanonicalTemplateFragment Children,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal enum RazorVueCanonicalSlotValueKind
{
    None,
    ValueExpression,
    ForwardedSlot
}

internal enum RazorVueCanonicalNodeKind
{
    Element,
    Component,
    Text,
    Interpolation,
    Conditional,
    ForEach,
    For,
    SlotOutlet
}

internal enum RazorVueTemplateEncodability
{
    DirectTemplate,
    TemplateViaSetupBinding,
    NotTemplateEncodable
}

internal enum RazorVueSideEffectClassification
{
    None,
    SingleEvaluationRequired,
    RepeatedEvaluationRisk
}

internal enum RazorVueExpressionBindingKind
{
    None,
    Literal,
    PropsReference,
    SlotReference,
    SetupReference,
    LocalReference,
    RuntimeExpression
}

internal enum RazorVueLiteralValueKind
{
    None,
    String,
    Boolean,
    Number,
    Null,
    Other
}

internal enum RazorVueCanonicalAttributeKind
{
    HtmlAttribute,
    ComponentProp,
    ComponentEvent,
    ComponentFallthroughAttribute
}
