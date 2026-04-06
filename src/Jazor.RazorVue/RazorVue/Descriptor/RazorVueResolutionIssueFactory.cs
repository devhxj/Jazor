using System.Collections.Immutable;
using System.Linq;

namespace Jazor.RazorVue.Descriptor;

internal static class RazorVueResolutionIssueFactory
{
    public static ImmutableArray<RazorVueCompilationIssue> Create(
        VueComponentResolutionStatus status,
        string componentName,
        ImmutableArray<VueComponentDescriptor> candidates)
    {
        switch (status)
        {
            case VueComponentResolutionStatus.NotFound:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.ComponentNotFound,
                        RazorVueIssueSeverity.Error,
                        $"Component '{componentName}' is not visible in the current RazorVue resolution scope.",
                        [])
                ];

            case VueComponentResolutionStatus.Ambiguous:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.AmbiguousComponentName,
                        RazorVueIssueSeverity.Error,
                        $"Component name '{componentName}' is ambiguous. Use a fully-qualified component name.",
                        GetRelatedComponentNames(candidates))
                ];

            case VueComponentResolutionStatus.ReservedIntrinsicName:
                return
                [
                    new RazorVueCompilationIssue(
                        RazorVueIssueCode.ReservedIntrinsicNameCollision,
                        RazorVueIssueSeverity.Error,
                        $"Component name '{componentName}' collides with a reserved intrinsic Vue component name.",
                        GetRelatedComponentNames(candidates))
                ];

            default:
                return [];
        }
    }

    private static ImmutableArray<string> GetRelatedComponentNames(ImmutableArray<VueComponentDescriptor> candidates)
        => candidates.IsDefaultOrEmpty
            ? []
            : candidates.Select(static candidate => candidate.FullName).Distinct(StringComparer.Ordinal).ToImmutableArray();
}

