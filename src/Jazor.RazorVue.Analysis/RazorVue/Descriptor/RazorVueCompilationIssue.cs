using System.Collections.Immutable;

namespace Jazor.RazorVue.Analysis.Descriptor;

public sealed record RazorVueCompilationIssue(
    RazorVueIssueCode Code,
    RazorVueIssueSeverity Severity,
    string Message,
    ImmutableArray<string> RelatedComponentNames);

public enum RazorVueIssueCode
{
    ComponentNotFound,
    AmbiguousComponentName,
    ReservedIntrinsicNameCollision
}

public enum RazorVueIssueSeverity
{
    Error
}

