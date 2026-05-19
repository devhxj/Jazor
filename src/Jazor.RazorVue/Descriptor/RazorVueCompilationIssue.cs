using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal sealed record RazorVueCompilationIssue(
    RazorVueIssueCode Code,
    RazorVueIssueSeverity Severity,
    string Message,
    ImmutableArray<string> RelatedComponentNames);

internal enum RazorVueIssueCode
{
    ComponentNotFound,
    AmbiguousComponentName,
    ReservedIntrinsicNameCollision,
    UnsupportedImperativeRenderLowering,
    UnsupportedLifecycleLowering,
    UnsupportedSetupLogicLowering,
    CanonicalizationFailed,
    UnsupportedTemplateEncoding,
    InvalidComponentDeclaration,
    InvalidLibraryComponentDeclaration,
    InvalidLibraryStyleDependencyDeclaration,
    InvalidLibraryPluginRequirementDeclaration,
    InvalidContainerInjectDeclaration,
    UnknownParameter,
    InvalidBindTarget,
    UnknownSlot,
    SlotContextMisuse,
    DuplicateSlotValue,
    MissingSlotValue
}

internal enum RazorVueIssueSeverity
{
    Error
}
