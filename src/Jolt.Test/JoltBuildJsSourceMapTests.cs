using System.Text.Json;
using Jolt.Build;
using Jazor.SourceMaps;
using Jolt.SourceMap;
using static Jolt.Test.SourceMapTestHelpers;

namespace Jolt.Test;

[TestClass]
public sealed class JoltBuildJsSourceMapTests
{
    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ForMixedAuthoredModules_EmitsJsSourceMapsChainedToOriginalAuthoringFiles()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string appSource = """
                <template>
                  <button @click="increment">template-sourcemap-marker {{ message }}</button>
                </template>

                <script setup>
                import { ref } from "vue";

                const message = ref("hello-vue-source-map");

                function increment() {
                  message.value = "updated-vue-source-map";
                }
                </script>
                """;
            const string counterSource = """
                <template>
                  <button @click="increment()">@Count</button>
                </template>

                @code {
                    [State] private int Count = 1;

                    public void Increment()
                    {
                        Count++;
                    }
                }
                """;
            const string lazySource = """
                <template>
                  <div class="lazy-card">lazy-sourcemap-marker {{ lazyLabel }}</div>
                </template>

                <script setup>
                const lazyLabel = "lazy-script-marker";
                </script>
                """;

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                import Counter from "./CounterEntry.jazor";
                import App from "./AppEntry.vue";

                console.log(Counter.render, App.render);

                globalThis.__loadLazyCard = async function () {
                  const module = await import("./LazyCard.vue");
                  return module.default;
                };
                """);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "AppEntry.vue"), appSource);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "CounterEntry.jazor"), counterSource);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "LazyCard.vue"), lazySource);

            var result = await BuildAsync(tempDir, codeSplitting: true);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(result.Chunks.Count >= 2, "Expected code splitting to emit lazy chunks.");

            var (entryChunk, entryChunkContent, entrySourceMapPath, entrySourceMapJson) = await ReadEntryChunkArtifactsAsync(tempDir, result);
            StringAssert.Contains(entryChunkContent, $"//# sourceMappingURL={Path.GetFileName(entrySourceMapPath)}");
            StringAssert.Contains(entryChunkContent, "message.value = \"updated-vue-source-map\";");
            StringAssert.Contains(entryChunkContent, "count.value++;");

            using var entrySourceMap = JsonDocument.Parse(entrySourceMapJson);
            Assert.AreEqual(entryChunk.FileName, entrySourceMap.RootElement.GetProperty("file").GetString());
            AssertSourceMapContainsOriginalSource(entrySourceMap.RootElement, "AppEntry.vue", appSource);
            AssertSourceMapContainsOriginalSource(entrySourceMap.RootElement, "CounterEntry.jazor", counterSource);

            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk =>
            {
                var chunkPath = Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
                return new
                {
                    Chunk = chunk,
                    Content = await File.ReadAllTextAsync(chunkPath)
                };
            }));
            var lazyChunkOutput = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains("lazy-sourcemap-marker", StringComparison.Ordinal));
            var (_, lazyChunkContent, _, lazySourceMapJson) = await ReadChunkArtifactsAsync(tempDir, lazyChunkOutput.Chunk);

            using var lazySourceMap = JsonDocument.Parse(lazySourceMapJson);
            AssertSourceMapContainsOriginalSource(lazySourceMap.RootElement, "LazyCard.vue", lazySource);

            var lazyMappedLocations = DecodeGeneratedLineToSourceLocation(lazySourceMap.RootElement);
            AssertGeneratedLineMapsToSource(
                lazyChunkContent,
                "lazy-sourcemap-marker",
                lazySourceMap.RootElement,
                "LazyCard.vue",
                lazySource,
                "lazy-sourcemap-marker {{ lazyLabel }}",
                lazyMappedLocations);
            AssertGeneratedLineMapsToSource(
                lazyChunkContent,
                "lazy-script-marker",
                lazySourceMap.RootElement,
                "LazyCard.vue",
                lazySource,
                "const lazyLabel = \"lazy-script-marker\";",
                lazyMappedLocations);

            AssertSourceMapReverseLookupToOriginalAuthoring(
                generatedPath: lazyChunkOutput.Chunk.FilePath,
                sourceMapJson: lazySourceMapJson,
                generatedText: lazyChunkContent,
                generatedNeedle: "lazy-script-marker",
                expectedSourcePath: "LazyCard.vue",
                sourceText: lazySource,
                sourceNeedle: "lazy-script-marker");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static async Task<BuildResult> BuildAsync(
        string rootDirectory,
        bool codeSplitting = false)
    {
        var orchestrator = new BuildOrchestrator();
        return await orchestrator.BuildAsync(
            new BuildOptions
            {
                RootDirectory = rootDirectory,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = codeSplitting
            },
            CancellationToken.None);
    }

    private static async Task<(ChunkInfo EntryChunk, string ChunkContent, string SourceMapPath, string SourceMapJson)> ReadEntryChunkArtifactsAsync(
        string rootDirectory,
        BuildResult result)
    {
        var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
        return await ReadChunkArtifactsAsync(rootDirectory, entryChunk);
    }

    private static async Task<(ChunkInfo Chunk, string ChunkContent, string SourceMapPath, string SourceMapJson)> ReadChunkArtifactsAsync(
        string rootDirectory,
        ChunkInfo chunk)
    {
        Assert.IsNotNull(chunk.SourceMapPath);

        var chunkPath = Path.Combine(rootDirectory, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
        var sourceMapPath = Path.Combine(rootDirectory, chunk.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));

        Assert.IsTrue(File.Exists(chunkPath), $"Expected emitted chunk at '{chunkPath}'.");
        Assert.IsTrue(File.Exists(sourceMapPath), $"Expected emitted chunk source map at '{sourceMapPath}'.");

        return (
            chunk,
            await File.ReadAllTextAsync(chunkPath),
            sourceMapPath,
            await File.ReadAllTextAsync(sourceMapPath));
    }

    private static void AssertSourceMapReverseLookupToOriginalAuthoring(
        string generatedPath,
        string sourceMapJson,
        string generatedText,
        string generatedNeedle,
        string expectedSourcePath,
        string sourceText,
        string sourceNeedle,
        int? minimumOriginalColumn = null,
        int? minimumGeneratedColumn = null)
    {
        var generatedPosition = GetLineColumnContaining(generatedText, generatedNeedle);
        var expectedSourcePosition = GetLineColumnContaining(sourceText, sourceNeedle);

        var service = new InMemorySourceMapService();
        service.Register(generatedPath, sourceMapJson);

        var original = service.OriginalPositionFor(generatedPath, generatedPosition.Line, generatedPosition.Column);
        Assert.IsNotNull(original, $"Expected reverse source-map lookup for '{generatedNeedle}' to return an authored location.");
        StringAssert.EndsWith(
            original.SourcePath.Replace('\\', '/'),
            expectedSourcePath.Replace('\\', '/'),
            $"Expected reverse source-map lookup to resolve into '{expectedSourcePath}'.");
        Assert.AreEqual(expectedSourcePosition.Line, original.Line, "Expected reverse source-map lookup to preserve authored line.");
        Assert.IsTrue(
            original.Column <= expectedSourcePosition.Column,
            $"Expected authored column <= token start column ({expectedSourcePosition.Column}), actual {original.Column}.");
        if (minimumOriginalColumn.HasValue)
        {
            Assert.IsTrue(
                original.Column >= minimumOriginalColumn.Value,
                $"Expected authored column >= {minimumOriginalColumn.Value}, actual {original.Column}.");
        }

        var generated = service.GeneratedPositionFor(expectedSourcePath, expectedSourcePosition.Line, expectedSourcePosition.Column);
        Assert.IsNotNull(generated, "Expected forward source-map lookup from authored location to return generated position.");
        Assert.AreEqual(generatedPath.Replace('\\', '/'), generated.GeneratedPath.Replace('\\', '/'));
        Assert.AreEqual(generatedPosition.Line, generated.Line, "Expected forward source-map lookup to preserve generated line.");
        Assert.IsTrue(
            generated.Column <= generatedPosition.Column,
            $"Expected generated column <= token start column ({generatedPosition.Column}), actual {generated.Column}.");
        if (minimumGeneratedColumn.HasValue)
        {
            Assert.IsTrue(
                generated.Column >= minimumGeneratedColumn.Value,
                $"Expected generated column >= {minimumGeneratedColumn.Value}, actual {generated.Column}.");
        }
    }

    private static void AssertSourceMapContainsOriginalSource(
        JsonElement sourceMap,
        string expectedSourcePath,
        string expectedSourceContent)
    {
        var sourceIndex = FindSourceIndexContaining(sourceMap, expectedSourcePath);
        var matchingSources = sourceMap
            .GetProperty("sources")
            .EnumerateArray()
            .Select(static source => source.GetString())
            .Where(static source => !string.IsNullOrWhiteSpace(source))
            .Count(source => source!.Replace('\\', '/').EndsWith(expectedSourcePath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual(
            1,
            matchingSources,
            $"Expected exactly one authored source entry for '{expectedSourcePath}'.");

        var sourcesContent = sourceMap.GetProperty("sourcesContent");
        Assert.IsTrue(
            sourceIndex < sourcesContent.GetArrayLength(),
            $"Expected a sourcesContent entry for '{expectedSourcePath}'.");
        Assert.AreEqual(expectedSourceContent, sourcesContent[sourceIndex].GetString());
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-js-sourcemap-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}
