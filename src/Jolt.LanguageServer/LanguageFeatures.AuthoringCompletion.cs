using Jolt.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures
{
    private static readonly string[] GlobalHtmlAttributes = ["class", "id", "role", "style", "tabindex", "title"];
    private static readonly string[] CommonDomEvents =
        ["blur", "change", "click", "dblclick", "focus", "input", "keydown", "keyup", "mousedown", "mouseup"];

    private (object[] Items, bool IncludeCSharp)? AuthoringCompletions(int offset)
    {
        var start = offset;
        while (start > 0 && IsMarkupNameCharacter(_text[start - 1]))
            start--;
        var end = offset;
        while (end < _text.Length && IsMarkupNameCharacter(_text[end]))
            end++;
        var transition = start > 0 && _text[start - 1] == '@';
        if (transition)
            start--;

        var element = JoltTree.Elements(projection.Document)
            .Where(element => element.Span.Start <= start && start <= element.Span.End)
            .MinBy(element => element.Span.Length);
        var span = TextSpan.FromBounds(start, end);
        var prefix = _text.ToString(TextSpan.FromBounds(start, offset));
        if (element is { Name.Length: > 0 } && start > element.NameSpan.End)
        {
            var previous = element.Attributes.Where(attribute => attribute.Span.End < start)
                .Select(attribute => attribute.Span.End).DefaultIfEmpty(element.NameSpan.End).Max();
            var between = _text.ToString(TextSpan.FromBounds(previous, start));
            if (between.Length > 0 && between.All(char.IsWhiteSpace))
            {
                var attribute = element.Attributes.FirstOrDefault(item => item.NameSpan.Start == start);
                if (!char.IsLower(element.Name[0]) || element.Name == "slot" || element.Name == "template")
                    return null;

                // Razor can project an unfinished @ attribute as C#. The opening
                // tag and the attribute name span identify the authoring context.
                return (HtmlAttributeCompletions(element, attribute, span, offset, prefix).ToArray(), false);
            }
            if (element.Attributes.Any(attribute => attribute.Span.Start <= start && start <= attribute.Span.End))
                return null;
        }

        var lineStart = _text.Lines.GetLineFromPosition(start).Start;
        var atLineStart = _text.ToString(TextSpan.FromBounds(lineStart, start)).All(character => char.IsWhiteSpace(character) || character == '\uFEFF');
        if (!transition && (!atLineStart || start != offset || end != offset) ||
            transition && start > 0 && (_text[start - 1] == '@' || IsMarkupNameCharacter(_text[start - 1])) ||
            projection.Spans.Any(mapping => mapping.Source.Contains(start)) ||
            projection.Document.Nodes.Any(node => node is JoltScopedStyle && node.Span.Contains(start)))
            return null;

        // Parse recovery may omit a directive until it has its header. Match only
        // the transitions Jolt projects, and retain Roslyn's expression candidates.
        var items = new List<object>();
        if (element is null && atLineStart &&
            !projection.Document.Nodes.Any(node => node is (JoltIf or JoltFor or JoltForEach or JoltSwitch or JoltBlock) && node.Span.Contains(start)))
        {
            Add("@code", "Declare component members");
            Add("@using", "Import a C# namespace or alias");
            Add("@typeparam", "Declare a component type parameter");
            if (workspace.Options.Router)
            {
                Add("@page", "Declare a Vue Router path");
                Add("@layout", "Select a layout component");
            }
        }
        Add("@if", "Render markup conditionally");
        Add("@foreach", "Render markup for each item");
        Add("@for", "Render markup in a loop");
        Add("@switch", "Select markup by value or pattern");
        return items.Count == 0 ? null : (items.ToArray(), transition);

        void Add(string name, string detail)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
                items.Add(Completion(name, span, detail, 14));
        }
    }

    private IEnumerable<object> HtmlAttributeCompletions(JoltElement element, JoltAttribute? attribute, TextSpan span, int offset, string prefix)
    {
        if (!prefix.StartsWith('@'))
        {
            string[] specific = element.Name switch
            {
                "a" => ["href", "rel", "target"],
                "button" => ["disabled", "name", "type", "value"],
                "form" => ["action", "method"],
                "img" => ["alt", "height", "src", "width"],
                "input" => ["autocomplete", "checked", "disabled", "name", "placeholder", "readonly", "required", "type", "value"],
                "label" => ["for"],
                "select" => ["disabled", "multiple", "name", "required"],
                "textarea" => ["cols", "disabled", "name", "placeholder", "readonly", "required", "rows"],
                _ => [],
            };
            foreach (var name in GlobalHtmlAttributes.Concat(specific))
                if (Available(name))
                    yield return Completion(name, span, "HTML attribute", 10);
        }
        foreach (var item in DirectiveCompletions(element, attribute, null, null, span, offset))
            yield return item;
        foreach (var name in CommonDomEvents)
        {
            var directive = "@on" + name;
            if (Available(directive))
                yield return Completion(directive, span, "DOM event", 23);
        }
        if (element.Name == "form" && Available("@onsubmit"))
            yield return Completion("@onsubmit", span, "DOM event", 23);

        bool Available(string name) => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            !element.Attributes.Any(other => !ReferenceEquals(other, attribute) && other.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
