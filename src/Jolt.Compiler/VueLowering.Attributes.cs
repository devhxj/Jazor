using System.Globalization;
using Acornima.Ast;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    private static Expression LowerElementProperties(ArrayExpression layout, Expression? capturedEntries = null)
    {
        // Projection emits one-field entries for splats and two-field entries
        // for explicit props. Native model entries additionally carry a directive.
        bool splats = layout.Elements.Any(entry => entry is ArrayExpression { Elements.Count: 1 });
        bool splitText = layout.Elements.Any(entry => entry is ArrayExpression { Elements.Count: 3 } tuple && tuple.Elements[2] is NullLiteral);
        if (!splats && !splitText)
            return Properties(capturedEntries ?? layout);

        var sources = new Expression[layout.Elements.Count];
        var properties = new List<ObjectProperty>();
        for (int index = 0; index < sources.Length; index++)
        {
            var entry = (ArrayExpression)layout.Elements[index]!;
            var captured = capturedEntries is null ? null : EntryAt(capturedEntries, index);
            if (entry.Elements.Count == 1)
                sources[index] = captured is null ? entry.Elements[0]! : EntryAt(captured, 0);
            else
            {
                var value = captured is null ? entry.Elements[1]! : EntryAt(captured, 1);
                if (entry.Elements.Count == 3 && entry.Elements[2] is NullLiteral)
                    value = EntryAt(value, 0);
                var property = Property(((StringLiteral)entry.Elements[0]!).Value, value);
                if (splats)
                    sources[index] = Object(property);
                else
                    properties.Add(property);
            }
        }
        // Vue combines class/style and event handlers, and lets later ordinary
        // keys win. Argument evaluation also preserves the template's source order.
        return splats ? Call(Id("$joltMergeProps"), sources) : Object(properties.ToArray());
    }

    private static MemberExpression EntryAt(Expression entries, int index) =>
        new(entries, new NumericLiteral(index, index.ToString(CultureInfo.InvariantCulture)), computed: true, optional: false);
}
