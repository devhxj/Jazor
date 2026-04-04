using System.Text.Json;
using DenoHost.Core;

namespace ECMAScript.WebIDL.Generator;

internal sealed class DenoWebIdlCollector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly GeneratorOptions _options;

    public DenoWebIdlCollector(GeneratorOptions options)
    {
        _options = options;
    }

    public async Task<WebIdlInventory> CollectAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.WorkerPath))
        {
            throw new FileNotFoundException("The Deno worker entrypoint was not found.", _options.WorkerPath);
        }

        if (!File.Exists(_options.DenoConfigPath))
        {
            throw new FileNotFoundException("The Deno config file was not found.", _options.DenoConfigPath);
        }

        Directory.CreateDirectory(_options.OutputDirectory);
        var tempFile = Path.Combine(_options.OutputDirectory, $"{Path.GetRandomFileName()}.json");

        try
        {
            var workerDirectory = Path.GetDirectoryName(_options.WorkerPath)
                ?? throw new InvalidOperationException("Could not determine the Deno worker directory.");
            var executeOptions = new DenoExecuteBaseOptions
            {
                WorkingDirectory = workerDirectory,
            };

            var args = new[]
            {
                "run",
                "--config",
                _options.DenoConfigPath,
                "--quiet",
                "--allow-read",
                "--allow-write",
                "--allow-net",
                _options.WorkerPath,
                "--out",
                tempFile,
            };

            await Deno.Execute(executeOptions, args, cancellationToken);

            await using var stream = File.OpenRead(tempFile);
            var inventory = await JsonSerializer.DeserializeAsync<WebIdlInventory>(stream, JsonOptions, cancellationToken);
            return inventory ?? throw new InvalidOperationException("The Deno worker completed but did not produce a valid WebIDL inventory.");
        }
        catch (Exception ex) when (ex is InvalidOperationException or FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to execute the Deno WebIDL collector. Ensure DenoHost runtime assets are available for the current RID and restore the generator project before running.",
                ex);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
