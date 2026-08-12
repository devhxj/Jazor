using System.Text.Json.Serialization;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Keeps the browser reload protocol usable from trimmed and single-file hosts.
/// </summary>
[JsonSerializable(typeof(ReloadMessage))]
[JsonSerializable(typeof(ReloadClientMessage))]
internal sealed partial class ReloadJsonSerializerContext : JsonSerializerContext;
