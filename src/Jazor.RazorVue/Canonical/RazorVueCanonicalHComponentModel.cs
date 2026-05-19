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
    RazorVueCanonicalImperativeRootProgram? ImperativeRootProgram,
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

internal sealed record RazorVueCanonicalImperativeRootProgram(
    RazorVueImperativeBlockKind Kind,
    RazorVueRenderFragment RenderTree,
    bool IsRootOnly,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal abstract record RazorVueCanonicalTemplateNode(
    RazorVueCanonicalNodeKind NodeKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueCanonicalElementNode(
    string TagName,
    RazorVueCanonicalNodeKey? Key,
    ImmutableArray<RazorVueCanonicalAttributeEntry> Attributes,
    RazorVueCanonicalTemplateFragment Children,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Element, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    VueComponentDescriptor? ResolvedDescriptor,
    RazorVueCanonicalNodeKey? Key,
    ImmutableArray<RazorVueCanonicalAttributeEntry> Attributes,
    ImmutableArray<RazorVueCanonicalSlotBinding> Slots,
    RazorVueCanonicalTemplateFragment Children,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Component, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalTextNode(
    string Text,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Text, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalInterpolationNode(
    string ExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Interpolation, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalLocalDeclarationNode(
    string LocalName,
    string InitializerExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.LocalDeclaration, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalTemplateScopeNode(
    string ScopeName,
    string InitializerExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueCanonicalTemplateFragment Children,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.TemplateScope, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalConditionalNode(
    string ConditionExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueCanonicalTemplateFragment WhenTrue,
    RazorVueCanonicalTemplateFragment WhenFalse,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.Conditional, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalForEachNode(
    string ItemName,
    string SourceExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueCanonicalTemplateFragment Body,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.ForEach, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalForNode(
    string VariableName,
    string InitialValueExpressionText,
    RazorVueExpressionBindingKind InitialValueBindingKind,
    RazorVueTemplateExpressionSafety InitialValueTemplateExpressionSafety,
    RazorVueSideEffectClassification InitialValueSideEffectClassification,
    RazorVueForConditionKind ConditionKind,
    string LimitValueExpressionText,
    RazorVueExpressionBindingKind LimitValueBindingKind,
    RazorVueTemplateExpressionSafety LimitValueTemplateExpressionSafety,
    RazorVueSideEffectClassification LimitValueSideEffectClassification,
    RazorVueForStepKind StepKind,
    string? StepValueExpressionText,
    RazorVueExpressionBindingKind StepValueBindingKind,
    RazorVueTemplateExpressionSafety StepValueTemplateExpressionSafety,
    RazorVueSideEffectClassification StepValueSideEffectClassification,
    RazorVueCanonicalTemplateFragment Body,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.For, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalSlotOutletNode(
    string SlotName,
    string? ArgumentExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalTemplateNode(RazorVueCanonicalNodeKind.SlotOutlet, TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalNodeKey(
    string ExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal abstract record RazorVueCanonicalAttributeEntry(
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueCanonicalAttributeBinding(
    string Name,
    string? ExpressionText,
    RazorVueLiteralValueKind LiteralValueKind,
    RazorVueCanonicalAttributeKind AttributeKind,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalAttributeEntry(TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalAttributeSpreadBinding(
    string ExpressionText,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
    : RazorVueCanonicalAttributeEntry(TemplateEncodability, TemplateExpressionSafety, SideEffectClassification, SourceOrigins);

internal sealed record RazorVueCanonicalSlotBinding(
    string SlotName,
    bool IsDefault,
    string? ParameterName,
    RazorVueCanonicalSlotValueKind ValueKind,
    string? ValueExpressionText,
    string? ForwardedSlotName,
    RazorVueExpressionBindingKind BindingKind,
    RazorVueTemplateEncodability TemplateEncodability,
    RazorVueTemplateExpressionSafety TemplateExpressionSafety,
    RazorVueSideEffectClassification SideEffectClassification,
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
    LocalDeclaration,
    TemplateScope,
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

internal enum RazorVueTemplateExpressionSafety
{
    DirectTemplateSafe,
    RequiresSetupBinding,
    NotTemplateSafe
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
