using System.Text.Json;

namespace Jolt.DevServer;

internal static class JoltConfigFile
{
    public const string FileName = "jolt.config.json";
}

internal static class JoltConfigLoader
{
    public static JazorConfig? Load(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        var configPath = Path.Combine(rootDirectory, JoltConfigFile.FileName);
        if (!File.Exists(configPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JazorConfig>(
                File.ReadAllText(configPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (IOException exception)
        {
            throw CreateLoadException(configPath, exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CreateLoadException(configPath, exception);
        }
        catch (JsonException exception)
        {
            throw CreateLoadException(configPath, exception);
        }
        catch (NotSupportedException exception)
        {
            throw CreateLoadException(configPath, exception);
        }
    }

    private static InvalidOperationException CreateLoadException(string configPath, Exception exception)
        => new($"Failed to load Jolt config '{configPath}': {exception.Message}", exception);
}
