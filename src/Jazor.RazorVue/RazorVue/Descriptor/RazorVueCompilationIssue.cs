using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

public sealed record RazorVueCompilationIssue(
    RazorVueIssueCode Code,
    RazorVueIssueSeverity Severity,
    string Message,
    ImmutableArray<string> RelatedComponentNames);

public enum RazorVueIssueCode
{
    ComponentNotFound,
    AmbiguousComponentName,
    ReservedIntrinsicNameCollision,
    UnsupportedLifecycleLowering,
    UnsupportedSetupLogicLowering,
    UnknownParameter,
    InvalidBindTarget,
    UnknownSlot,
    SlotContextMisuse
}

public enum RazorVueIssueSeverity
{
    Error
}

