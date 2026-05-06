using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal sealed class VueComponentResolutionResult
{
    private VueComponentResolutionResult(
        VueComponentResolutionStatus status,
        string componentName,
        VueComponentDescriptor? descriptor,
        ImmutableArray<VueComponentDescriptor> candidates,
        ImmutableArray<RazorVueCompilationIssue> issues)
    {
        Status = status;
        ComponentName = componentName;
        Descriptor = descriptor;
        Candidates = candidates.IsDefault ? ImmutableArray<VueComponentDescriptor>.Empty : candidates;
        Issues = issues.IsDefault ? ImmutableArray<RazorVueCompilationIssue>.Empty : issues;
    }

    public VueComponentResolutionStatus Status { get; }

    public string ComponentName { get; }

    public VueComponentDescriptor? Descriptor { get; }

    public ImmutableArray<VueComponentDescriptor> Candidates { get; }

    public ImmutableArray<RazorVueCompilationIssue> Issues { get; }

    public static VueComponentResolutionResult Resolved(string componentName, VueComponentDescriptor descriptor)
        => new(
            VueComponentResolutionStatus.Resolved,
            componentName,
            descriptor,
            [descriptor],
            []);

    public static VueComponentResolutionResult NotFound(string componentName)
        => new(
            VueComponentResolutionStatus.NotFound,
            componentName,
            null,
            [],
            RazorVueResolutionIssueFactory.Create(
                VueComponentResolutionStatus.NotFound,
                componentName,
                []));

    public static VueComponentResolutionResult Ambiguous(string componentName, ImmutableArray<VueComponentDescriptor> candidates)
        => new(
            VueComponentResolutionStatus.Ambiguous,
            componentName,
            null,
            candidates,
            RazorVueResolutionIssueFactory.Create(
                VueComponentResolutionStatus.Ambiguous,
                componentName,
                candidates));

    public static VueComponentResolutionResult ReservedIntrinsicName(string componentName, ImmutableArray<VueComponentDescriptor> candidates)
        => new(
            VueComponentResolutionStatus.ReservedIntrinsicName,
            componentName,
            null,
            candidates,
            RazorVueResolutionIssueFactory.Create(
                VueComponentResolutionStatus.ReservedIntrinsicName,
                componentName,
                candidates));
}

internal enum VueComponentResolutionStatus
{
    Resolved,
    NotFound,
    Ambiguous,
    ReservedIntrinsicName
}
