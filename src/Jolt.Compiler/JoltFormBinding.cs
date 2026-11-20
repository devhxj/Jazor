using Jolt.Syntax;

namespace Jolt.Compiler;

public enum JoltFormBinding
{
    Text,
    Checkbox,
    Radio,
    Select,
    SelectMultiple,
    Number,
}

public static class JoltFormBindings
{
    internal static bool HasBinding(JoltElement element) => element.Attributes.Any(attribute =>
        attribute is { Target: null, Kind: JoltAttributeKind.Bind, Modifier: null or "get" });

    public static JoltFormBinding? Get(JoltElement element)
    {
        var typeAttribute = element.Attributes.FirstOrDefault(attribute => string.Equals(attribute.Name, "type", StringComparison.OrdinalIgnoreCase));
        var inputType = ComponentContract.Text(typeAttribute)?.ToLowerInvariant();
        return element.Name switch
        {
            "textarea" => JoltFormBinding.Text,
            "input" when typeAttribute is null || inputType is "text" or "search" or "email" or "url" or "tel" or "password" => JoltFormBinding.Text,
            "input" when inputType == "checkbox" => JoltFormBinding.Checkbox,
            "input" when inputType == "radio" => JoltFormBinding.Radio,
            "input" when inputType == "number" => JoltFormBinding.Number,
            "select" when !element.Attributes.Any(attribute => attribute.Name.Equals("multiple", StringComparison.OrdinalIgnoreCase)) => JoltFormBinding.Select,
            "select" when element.Attributes.Any(attribute => attribute.Name.Equals("multiple", StringComparison.OrdinalIgnoreCase) &&
                (attribute.Value.IsEmpty || attribute.Value is [JoltText])) => JoltFormBinding.SelectMultiple,
            _ => null,
        };
    }
}
