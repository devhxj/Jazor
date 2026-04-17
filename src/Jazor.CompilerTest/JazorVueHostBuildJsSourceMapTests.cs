using System.Text.Json;
using Jazor.VueHost.Build;
using Jazor.VueHost.SourceMap;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
[DoNotParallelize]
public sealed class JazorVueHostBuildJsSourceMapTests
{
    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ForJazorEntry_EmitsJsSourceMapChainedToOriginalAuthoringFile()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string source = """
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

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/CounterEntry.jazor"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "CounterEntry.jazor"),
                source);

            var result = await BuildAsync(tempDir);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));

            var (entryChunk, chunkContent, sourceMapPath, sourceMapJson) = await ReadEntryChunkArtifactsAsync(tempDir, result);
            StringAssert.Contains(chunkContent, $"//# sourceMappingURL={Path.GetFileName(sourceMapPath)}");
            StringAssert.Contains(chunkContent, "count.value++;");

            using var sourceMap = JsonDocument.Parse(sourceMapJson);
            Assert.AreEqual(entryChunk.FileName, sourceMap.RootElement.GetProperty("file").GetString());
            AssertSourceMapContainsOriginalSource(sourceMap.RootElement, "CounterEntry.jazor", source);

            var mappedLocations = DecodeGeneratedLineToSourceLocation(sourceMap.RootElement);
            AssertGeneratedLineMapsToSource(
                chunkContent,
                "const count = ref(1);",
                sourceMap.RootElement,
                "CounterEntry.jazor",
                source,
                "[State] private int Count = 1;",
                mappedLocations);
            AssertGeneratedLineMapsToSource(
                chunkContent,
                "count.value++;",
                sourceMap.RootElement,
                "CounterEntry.jazor",
                source,
                "Count++;",
                mappedLocations);

            AssertSourceMapReverseLookupToOriginalAuthoring(
                generatedPath: entryChunk.FilePath,
                sourceMapJson,
                chunkContent,
                "count.value++;",
                expectedSourcePath: "CounterEntry.jazor",
                source,
                "Count++;");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ForVueEntry_EmitsJsSourceMapChainedToOriginalAuthoringFile()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string source = """
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

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/AppEntry.vue"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "AppEntry.vue"),
                source);

            var result = await BuildAsync(tempDir);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));

            var (entryChunk, chunkContent, sourceMapPath, sourceMapJson) = await ReadEntryChunkArtifactsAsync(tempDir, result);
            StringAssert.Contains(chunkContent, $"//# sourceMappingURL={Path.GetFileName(sourceMapPath)}");
            StringAssert.Contains(chunkContent, "message.value = \"updated-vue-source-map\";");

            using var sourceMap = JsonDocument.Parse(sourceMapJson);
            Assert.AreEqual(entryChunk.FileName, sourceMap.RootElement.GetProperty("file").GetString());
            AssertSourceMapContainsOriginalSource(sourceMap.RootElement, "AppEntry.vue", source);

            var mappedLocations = DecodeGeneratedLineToSourceLocation(sourceMap.RootElement);
            AssertGeneratedLineMapsToSource(
                chunkContent,
                "message.value = \"updated-vue-source-map\";",
                sourceMap.RootElement,
                "AppEntry.vue",
                source,
                "message.value = \"updated-vue-source-map\";",
                mappedLocations);
            AssertGeneratedLineMapsToSource(
                chunkContent,
                "template-sourcemap-marker",
                sourceMap.RootElement,
                "AppEntry.vue",
                source,
                "template-sourcemap-marker {{ message }}",
                mappedLocations);

            AssertSourceMapReverseLookupToOriginalAuthoring(
                generatedPath: entryChunk.FilePath,
                sourceMapJson,
                chunkContent,
                "message.value = \"updated-vue-source-map\";",
                expectedSourcePath: "AppEntry.vue",
                source,
                "message.value = \"updated-vue-source-map\";");

            AssertSourceMapReverseLookupToOriginalAuthoring(
                generatedPath: entryChunk.FilePath,
                sourceMapJson,
                chunkContent,
                "template-sourcemap-marker",
                expectedSourcePath: "AppEntry.vue",
                source,
                "template-sourcemap-marker {{ message }}",
                minimumOriginalColumn: 1);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static async Task<BuildResult> BuildAsync(string rootDirectory)
    {
        var orchestrator = new BuildOrchestrator();
        return await orchestrator.BuildAsync(
            new BuildOptions
            {
                RootDirectory = rootDirectory,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = false
            },
            CancellationToken.None);
    }

    private static async Task<(ChunkInfo EntryChunk, string ChunkContent, string SourceMapPath, string SourceMapJson)> ReadEntryChunkArtifactsAsync(
        string rootDirectory,
        BuildResult result)
    {
        var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
        Assert.IsNotNull(entryChunk.SourceMapPath);

        var chunkPath = Path.Combine(rootDirectory, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
        var sourceMapPath = Path.Combine(rootDirectory, entryChunk.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));

        Assert.IsTrue(File.Exists(chunkPath), $"Expected emitted entry chunk at '{chunkPath}'.");
        Assert.IsTrue(File.Exists(sourceMapPath), $"Expected emitted entry source map at '{sourceMapPath}'.");

        return (
            entryChunk,
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
