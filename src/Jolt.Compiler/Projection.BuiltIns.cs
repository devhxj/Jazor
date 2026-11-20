using Jolt.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    private void ReportTeleportSlots(JoltElement element)
    {
        foreach (var child in element.Children.OfType<JoltElement>().Where(child => child.Name == "template"))
            foreach (var attribute in child.Attributes.Where(attribute => attribute.Name.StartsWith('#')))
                Report("JOLT1007", attribute.NameSpan, "Teleport renders direct children and has no named slots.");
    }

    private JoltNode[] TransitionChildren(IEnumerable<JoltNode> nodes, TextSpan span)
    {
        // Vue transitions one element/component per render. Formatting whitespace
        // would become text VNodes and compete with the actual transition target.
        var content = nodes.Where(node => node is not JoltText text || !string.IsNullOrWhiteSpace(text.Text)).ToArray();
        if (content is [JoltIf conditional])
        {
            return [conditional with
            {
                Then = [.. TransitionChildren(conditional.Then, conditional.Span)],
                Else = [.. TransitionChildren(conditional.Else, conditional.Span)],
            }];
        }
        if (content is not ([] or [JoltElement { Name: not "slot" }]))
            Report("JOLT1039", span, "Transition expects one element or component per branch.");
        return content;
    }
}
