namespace Jolt.Npm;

public sealed record NpmValueExport(
    string ModuleSpecifier,
    string ExportName,
    string PackageName,
    string Version,
    string ModulePath,
    string DeclarationPath,
    NpmDefinition? Definition,
    IReadOnlyList<NpmValueSignature> Signatures,
    NpmValueType? ConstantType);

public sealed record NpmValueSignature(
    IReadOnlyList<NpmValueParameter> Parameters,
    NpmValueType ReturnType,
    NpmDefinition? Definition = null);

public sealed record NpmValueParameter(string Name, NpmValueType Type, bool Optional);

public sealed record NpmValueType(
    string Kind,
    NpmValueType? ElementType = null,
    IReadOnlyList<NpmValueMember>? Members = null,
    IReadOnlyList<NpmValueSignature>? Signatures = null,
    bool ReadOnly = false);

public sealed record NpmValueMember(
    string Name,
    NpmValueType Type,
    bool ReadOnly,
    NpmDefinition? Definition = null,
    bool Optional = false);
