using System.Text.Json.Serialization;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Keeps the browser reload protocol usable from trimmed and single-file hosts.
/// </summary>
[JsonSerializable(typeof(DevelopmentReloadNotificationEnvelope))]
[JsonSerializable(typeof(DevelopmentReloadClientMessage))]
internal sealed partial class JazorDevelopmentReloadJsonSerializerContext : JsonSerializerContext;
