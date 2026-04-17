using System.Text.Json;

namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionManifest
{
    public string? Assembly { get; init; }

    public string? Type { get; init; }

    public Dictionary<string, JsonElement>? Settings { get; init; }
}
