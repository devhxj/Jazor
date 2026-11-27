using Jolt.Syntax;
using Microsoft.CodeAnalysis;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    internal string? GetSlotMemberType(JoltAttribute attribute, SemanticModel? model) => attribute.Value switch
    {
        [JoltExpression expression] => model is null ? null : GetExpressionType(expression, model),
        [] => "bool",
        _ => "string",
    };

    internal void CheckSlotOutlets(SemanticModel model, List<Diagnostic> validationDiagnostics)
    {
        foreach (var group in ComponentContract.SlotOutlets(Document))
        {
            var first = ComponentContract.SlotMembers(group.First()).ToLookup(attribute => attribute.Name, StringComparer.Ordinal);
            foreach (var outlet in group.Skip(1))
            {
                var members = ComponentContract.SlotMembers(outlet).ToLookup(attribute => attribute.Name, StringComparer.Ordinal);
                foreach (var missing in first.Where(member => !members.Contains(member.Key)))
                    ReportValidation(validationDiagnostics, "JOLT1037", outlet.NameSpan, $"Slot '{group.Key}' must supply field '{missing.Key}' at each outlet.");
                foreach (var member in members)
                {
                    var current = member.First();
                    if (!first.Contains(member.Key))
                    {
                        ReportValidation(validationDiagnostics, "JOLT1037", current.NameSpan, $"Slot '{group.Key}' must supply field '{member.Key}' at each outlet.");
                        continue;
                    }

                    var expected = GetSlotMemberType(first[member.Key].First(), model);
                    var actual = GetSlotMemberType(current, model);
                    if (expected != actual)
                        ReportValidation(validationDiagnostics, "JOLT1037", current.Value.FirstOrDefault()?.Span ?? current.NameSpan,
                            $"Slot '{group.Key}' field '{member.Key}' must have a consistent type at each outlet, but was '{expected ?? "untyped"}' and '{actual ?? "untyped"}'.");
                }
            }
        }
    }
}
