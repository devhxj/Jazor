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
    UnsupportedLifecycleLowering,
    UnsupportedSetupLogicLowering,
    InvalidLibraryComponentDeclaration,
    InvalidLibraryStyleDependencyDeclaration,
    InvalidLibraryPluginRequirementDeclaration,
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
