using System.Text;

var output = Path.GetFullPath(args[0]);
var components = new[]
{
    (Name: "Select", Slots: typeof(DefaultSlot), Emits: ""),
    (Name: "Tabs", Slots: typeof(DefaultSlot), Emits: ""),
    (Name: "DropdownMenu", Slots: typeof(DefaultSlot), Emits: ""),
    (Name: "DropdownItem", Slots: typeof(DefaultSlot), Emits: ""),
    (Name: "Dropdown", Slots: typeof(DropdownSlots), Emits: ", " + nameof(DropdownEmit))
};

// Preserve the upstream component props and publish the slots used by Jero.
// Element Plus's emitted constructor signatures omit those slot contracts.
var declaration = new StringBuilder();
declaration.Append("import type { ")
    .AppendJoin(", ", components.Select(component => "El" + component.Name))
    .AppendLine(" } from 'element-plus'");
declaration.AppendLine("import type { VNodeChild } from 'vue'").AppendLine();

WriteSlots<DefaultSlot>();
WriteSlots<DropdownSlots>();

declaration.Append("type ").Append(nameof(DropdownEmit)).AppendLine(" = {");
foreach (var listener in typeof(DropdownEmit).GetMethods().OrderBy(method => method.Name, StringComparer.Ordinal))
{
    declaration.Append("    (event: '").Append(VueName(listener.Name)).Append('\'');
    foreach (var parameter in listener.GetParameters())
    {
        var type = parameter.ParameterType == typeof(string)
            ? "string"
            : throw new InvalidOperationException($"Unsupported Vue event payload: {parameter.ParameterType}");
        declaration.Append(", ").Append(parameter.Name).Append(": ").Append(type);
    }
    declaration.AppendLine("): void");
}
declaration.AppendLine("}");
declaration.AppendLine("""
    type Component = abstract new (...args: never[]) => { $props: unknown, $emit: unknown }
    type WithSlots<C extends Component, Slots, Emit = InstanceType<C>['$emit']> =
        new () => { $props: InstanceType<C>['$props'], $slots: Slots, $emit: Emit }
    """);

foreach (var component in components)
{
    declaration.AppendLine(
        $"export declare const Project{component.Name}: WithSlots<typeof El{component.Name}, {component.Slots.Name}{component.Emits}>");
}

var content = declaration.ToString();
if (!File.Exists(output) || File.ReadAllText(output) != content)
{
    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
    File.WriteAllText(output, content);
}

static string VueName(string name) => char.ToLowerInvariant(name[0]) + name[1..];

void WriteSlots<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
    System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)] T>()
{
    declaration.Append("type ").Append(typeof(T).Name).AppendLine(" = {");
    foreach (var slot in typeof(T).GetProperties().OrderBy(property => property.Name, StringComparer.Ordinal))
        declaration.Append("    ").Append(VueName(slot.Name)).AppendLine("?(): VNodeChild");
    declaration.AppendLine("}");
}

record DefaultSlot(Func<VNodeChild>? Default);
record DropdownSlots(Func<VNodeChild>? Default, Func<VNodeChild>? Dropdown) : DefaultSlot(Default);

interface DropdownEmit
{
    void Click();
    void Command(string command);
}

sealed class VNodeChild;
