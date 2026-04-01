using System.Text.Json;

namespace ECMAScript.WebIDL.Generator;

internal sealed record WebIdlInventory(
    int SchemaVersion,
    DateTimeOffset GeneratedAt,
    WebIdlSourceInfo Source,
    IReadOnlyList<WebIdlFileInventory> Files,
    IReadOnlyList<InterfaceEventMap> InterfaceEvents,
    WebIdlStats Stats);

internal sealed record WebIdlSourceInfo(
    string Parser,
    string WebrefIdl,
    string WebrefCss,
    string WebrefEvents);

internal sealed record WebIdlFileInventory(
    string FileName,
    string? Namespace,
    IReadOnlyList<WebIdlDeclarationInventory> Declarations);

internal sealed record WebIdlDeclarationInventory(
    string Kind,
    string? Name,
    bool? Partial,
    string? Inheritance,
    string? Target,
    string? Includes,
    int? MemberCount,
    JsonElement Payload);

internal sealed record InterfaceEventMap(
    string InterfaceName,
    IReadOnlyList<InterfaceEventInventory> Events);

internal sealed record InterfaceEventInventory(
    string EventType,
    string InterfaceName);

internal sealed record WebIdlStats(
    int FileCount,
    int DeclarationCount,
    int InterfaceEventTargetCount,
    IReadOnlyDictionary<string, int> DeclarationsByKind);
