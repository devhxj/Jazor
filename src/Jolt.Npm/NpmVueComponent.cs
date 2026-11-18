namespace Jolt.Npm;

public sealed record NpmDefinition(string Path, int Start, int Length);

public sealed record NpmVueProp(
    string Type,
    bool Required,
    IReadOnlyList<string> LiteralValues,
    NpmDefinition? Definition);

public sealed record NpmVueParameter(string Name, string Type, IReadOnlyDictionary<string, string>? Properties = null);

public sealed record NpmVueEvent(
    IReadOnlyList<IReadOnlyList<NpmVueParameter>> Signatures,
    NpmDefinition? Definition);

public sealed record NpmVueSlot(
    bool Required,
    IReadOnlyList<IReadOnlyList<NpmVueParameter>> Signatures,
    NpmDefinition? Definition);

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
    IReadOnlyDictionary<string, NpmVueSlot> Slots);
