using Jazor.Npm;

namespace Jolt.Npm;

public sealed record NpmVueProp(
    string Type,
    bool Required,
    IReadOnlyList<string> LiteralValues,
    NpmDefinition? Definition)
{
    public string? InputType { get; init; }
    public IReadOnlyDictionary<string, string>? InputRecordFields { get; init; }
    public bool InputNullable { get; init; }
}

public sealed record NpmVueParameter(string Name, string Type, IReadOnlyDictionary<string, string>? Properties = null)
{
    public string? WebType { get; init; }
    public string? ValueDomain { get; init; }
    public IReadOnlyDictionary<string, NpmDefinition>? PropertyDefinitions { get; init; }
}

public sealed record NpmVueEvent(
    IReadOnlyList<IReadOnlyList<NpmVueParameter>> Signatures,
    NpmDefinition? Definition);

public sealed record NpmVueSlot(
    bool Required,
    IReadOnlyList<IReadOnlyList<NpmVueParameter>> Signatures,
    NpmDefinition? Definition);

public sealed record NpmVueTypeParameter(string Name, string? Constraint, IReadOnlyList<string> PrimitiveKinds);

public sealed record NpmVueComponent(
    string ModuleSpecifier,
    string ExportName,
    string PackageName,
    string Version,
    string ModulePath,
    string DeclarationPath,
    NpmDefinition Definition,
    IReadOnlyDictionary<string, NpmVueProp> Props,
    IReadOnlyDictionary<string, NpmVueEvent> Events,
    IReadOnlyDictionary<string, NpmVueSlot> Slots)
{
    public NpmVueTypeParameter? TypeParameter { get; init; }
    public string? LocalPath { get; init; }
    public string? LocalNamespace { get; init; }
    public string DisplayName => LocalPath is { } path ? $"Vue component '{path}'" : $"npm component '{ModuleSpecifier}/{ExportName}'";
}
