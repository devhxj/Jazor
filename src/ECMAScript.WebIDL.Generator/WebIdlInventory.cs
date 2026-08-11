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
    string WebrefEvents,
    string? WebrefXref = null);

internal sealed record WebIdlFileInventory(
    string FileName,
    string? Namespace,
    IReadOnlyList<WebIdlDeclarationInventory> Declarations,
    WebIdlSpecificationSource? Source = null);

internal sealed record WebIdlSpecificationSource(
    string Title,
    string Url,
    string? Shortname = null);

/// <summary>
/// Source-authored documentation associated with an IDL declaration, member, or argument.
/// The collector retains only prose that it can trace to the corresponding W3C/WHATWG
/// specification definition; it never invents descriptions from an IDL shape.
/// </summary>
internal sealed record WebIdlDocumentation(
    string Href,
    string SpecificationTitle,
    string? Heading,
    string? HeadingHref,
    string? Prose,
    string? Usage = null);

internal sealed record WebIdlArgumentDocumentation(
    int ArgumentIndex,
    WebIdlDocumentation Documentation);

internal sealed record WebIdlMemberDocumentation(
    int MemberIndex,
    WebIdlDocumentation? Documentation,
    IReadOnlyList<WebIdlArgumentDocumentation>? Arguments = null);

internal sealed record WebIdlDeclarationInventory(
    string Kind,
    string? Name,
    bool? Partial,
    string? Inheritance,
    string? Target,
    string? Includes,
    int? MemberCount,
    JsonElement Payload,
    WebIdlDocumentation? Documentation = null,
    IReadOnlyList<WebIdlMemberDocumentation>? MemberDocumentation = null);

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
