using System.Text.Json;
using Jolt.Volar.Deno.Hosting;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDenoWorkerProcessConcurrencyTests
{
    [TestMethod]
    public async Task Jolt_DenoWorkerProcess_ConcurrentRequests_CorrelateOutOfOrderResponses()
    {
        var tempDirectory = CreateTemporaryDirectory();
        var scriptPath = Path.Combine(tempDirectory, "out-of-order-worker.ts");
        await File.WriteAllTextAsync(scriptPath, CreateWorkerScript());

        var executablePath = DenoRuntimeAssetResolver.ResolveBundledExecutablePath();
        if (!File.Exists(executablePath))
        {
            Assert.Inconclusive($"Bundled Deno runtime was not found at '{executablePath}'.");
        }

        var process = CreateWorkerProcess(
            executablePath,
            scriptPath,
            Path.Combine(tempDirectory, "cache"),
            Path.Combine(tempDirectory, "workspace"));

        try
        {
            await process.StartAsync(CancellationToken.None);
            var warmupResult = await process.SendRequestAsync<JsonElement>(
                "echo",
                new
                {
                    value = "warmup",
                    delayMs = 0
                },
                CancellationToken.None);
            Assert.AreEqual("warmup", warmupResult.GetProperty("value").GetString());

            var slowRequest = process.SendRequestAsync<JsonElement>(
                "echo",
                new
                {
                    value = "slow",
                    delayMs = 5000
                },
                CancellationToken.None).AsTask();
            var fastRequest = process.SendRequestAsync<JsonElement>(
                "echo",
                new
                {
                    value = "fast",
                    delayMs = 50
                },
                CancellationToken.None).AsTask();

            var fastResult = await fastRequest.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.AreEqual("fast", fastResult.GetProperty("value").GetString());
            Assert.IsFalse(slowRequest.IsCompleted, "Expected the slow request to remain in flight while the fast response completed.");

            var slowResult = await slowRequest;
            Assert.AreEqual("slow", slowResult.GetProperty("value").GetString());
        }
        finally
        {
            await process.StopAsync(CancellationToken.None);
            DeleteDirectoryIfExists(tempDirectory);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jolt-deno-worker-concurrency-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static DenoWorkerProcess CreateWorkerProcess(
        string executablePath,
        string scriptPath,
        string cacheDirectory,
        string workingDirectory)
        => new(
            new DenoVolarHostOptions
            {
                Enabled = true,
                ExecutablePath = executablePath,
                HasExplicitExecutableOverride = true,
                WorkerScriptPath = scriptPath,
                CacheDirectory = cacheDirectory,
                Arguments =
                [
                    "run",
                    "--quiet",
                    scriptPath
                ],
                WorkingDirectory = workingDirectory,
                IgnoreStartupFailure = false
            });

    private static string CreateWorkerScript()
        =>
            """
            const decoder = new TextDecoder();
            const encoder = new TextEncoder();
            let buffered = "";
            let writeChain = Promise.resolve();
            const inFlight = new Set();

            for await (const chunk of Deno.stdin.readable) {
              buffered += decoder.decode(chunk, { stream: true });
              let newlineIndex = buffered.indexOf("\n");
              while (newlineIndex >= 0) {
                const line = buffered.slice(0, newlineIndex).trim();
                buffered = buffered.slice(newlineIndex + 1);
                if (line.length > 0) {
                  const task = (async () => {
                    const request = JSON.parse(line);
                    const response = await handleRequest(request);
                    writeChain = writeChain.then(() =>
                      Deno.stdout.write(encoder.encode(JSON.stringify(response) + "\n"))
                    );
                    await writeChain;
                  })().finally(() => {
                    inFlight.delete(task);
                  });
                  inFlight.add(task);
                }

                newlineIndex = buffered.indexOf("\n");
              }
            }

            await Promise.allSettled(Array.from(inFlight));
            await writeChain;

            async function handleRequest(request) {
              switch (request.method) {
                case "echo": {
                  const delayMs = request.payload?.delayMs ?? 0;
                  await new Promise((resolve) => setTimeout(resolve, delayMs));
                  return {
                    id: request.id,
                    success: true,
                    result: {
                      value: request.payload?.value ?? null,
                    },
                  };
                }
                default:
                  return {
                    id: request.id,
                    success: false,
                    error: `Unsupported method '${request.method}'.`,
                  };
              }
            }
            """;
}
