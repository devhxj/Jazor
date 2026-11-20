namespace Jolt.Syntax;

public static class JoltTree
{
    public static IEnumerable<JoltElement> Elements(JoltDocument document)
        => Elements(document.Nodes.Concat(document.InlineTemplates.SelectMany(template => template.Children)));

    public static IEnumerable<JoltElement> Elements(IEnumerable<JoltNode> nodes)
        => Descendants(nodes).OfType<JoltElement>();

    public static IEnumerable<JoltNode> Descendants(IEnumerable<JoltNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var descendant in Descendants(Children(node)))
                yield return descendant;
        }
    }

    private static IEnumerable<JoltNode> Children(JoltNode node) => node switch
    {
        JoltElement element => element.Children,
        JoltIf conditional => conditional.Then.Concat(conditional.Else),
        JoltForEach loop => loop.Children,
        JoltFor loop => loop.Children,
        JoltSwitch selection => selection.Sections,
        JoltSwitchSection section => section.Children,
        JoltBlock block => block.Children,
        _ => [],
    };
}
