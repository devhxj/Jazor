using System;
using Jazor.RazorVue.Artifacts;

namespace Jazor.RazorVue.Descriptor;

public sealed class RazorVueCompilationIssueException : Exception
{
    public RazorVueCompilationIssueException(
        RazorVueCompilationIssue issue,
        string ownerComponentFullName,
        RazorVueSourceOrigin? origin)
        : base(issue?.Message)
    {
        Issue = issue ?? throw new ArgumentNullException(nameof(issue));
        OwnerComponentFullName = ownerComponentFullName ?? string.Empty;
        Origin = origin;
    }

    public RazorVueCompilationIssue Issue { get; }

    public string OwnerComponentFullName { get; }

    public RazorVueSourceOrigin? Origin { get; }
}
