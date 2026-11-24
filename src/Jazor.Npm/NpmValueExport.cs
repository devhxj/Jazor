namespace Jazor.Npm;

public sealed record NpmValueProjections(IReadOnlyList<NpmValueExport> Values, IReadOnlyDictionary<string, string> Errors);

public sealed record NpmValueExport(
    string ModuleSpecifier,
    string ExportName,
    string PackageName,
    string Version,
    string ModulePath,
    string DeclarationPath,
    NpmDefinition? Definition,
    IReadOnlyList<NpmValueSignature> Signatures,
    NpmValueType? ConstantType,
    IReadOnlyList<NpmValueMember>? AttachedMembers = null)
{
    [System.Text.Json.Serialization.JsonInclude]
    internal string? AttachedMemberSuffix { get; init; }

    internal NpmValueType? AttachedMemberType =>
        AttachedMembers is { Count: > 0 } ? new("object", Members: AttachedMembers) : null;
}

public sealed record NpmValueSignature(
    IReadOnlyList<NpmValueParameter> Parameters,
    NpmValueType ReturnType,
    NpmDefinition? Definition = null,
    IReadOnlyList<string>? TypeParameters = null,
    IReadOnlyList<string>? ReferenceTypeParameters = null,
    string? ScalarResultTypeParameter = null,
    int? OverloadPriority = null,
    IReadOnlyList<string>? ValueTypeParameters = null,
    string? ManagedNameSuffix = null);

public sealed record NpmValueParameter(string Name, NpmValueType Type, bool Optional, bool ReadOnlyReference = false,
    bool? FixedBoolean = null, bool FixedUndefined = false, bool OptionalCallback = false, bool RestCallback = false);

public sealed record NpmValueType(
    string Kind,
    NpmValueType? ElementType = null,
    IReadOnlyList<NpmValueMember>? Members = null,
    IReadOnlyList<NpmValueSignature>? Signatures = null,
    bool ReadOnly = false,
    string? Name = null,
    NpmDefinition? TypeDefinition = null,
    IReadOnlyList<NpmValueType>? Variants = null,
    IReadOnlyList<string>? TypeParameters = null,
    IReadOnlyList<NpmValueType>? TypeArguments = null,
    IReadOnlyList<NpmEnumMember>? EnumMembers = null,
    IReadOnlyList<string>? StringLiterals = null,
    string? ProtocolKey = null,
    IReadOnlyList<string>? ValueTypeParameters = null,
    string? TypeIdentity = null,
    bool VeeValidatePath = false,
    NpmValueType? WriteType = null,
    bool RequirePlainRecord = false,
    bool ArrayIndices = false,
    bool RequireRecordView = false,
    IReadOnlyList<double>? NumberLiterals = null);

public sealed record NpmEnumMember(string Name, int Value);

public sealed record NpmValueMember(
    string Name,
    NpmValueType Type,
    bool ReadOnly,
    NpmDefinition? Definition = null,
    bool Optional = false,
    bool? FixedBoolean = null,
    string? JsName = null,
    string? StaticKey = null,
    int? StaticListArity = null);
