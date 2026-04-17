using System.Text.Json;
using Jazor.VueHost.Build;

namespace Jazor.CompilerTest;

[TestClass]
[DoNotParallelize]
public sealed class JazorVueHostBuildManifestTests
{
    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForSingleChunkBuild()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "public"));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head>
                  <link rel="icon" href="/favicon.svg">
                </head>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """console.log("manifest-single");""");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "favicon.svg"),
                "<svg></svg>");

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;

            var entry = root.GetProperty("Entry").GetString();
            Assert.AreEqual(GetHtmlRelativePath(tempDir, "dist", result.Chunks.Single().FilePath), entry);
            Assert.AreEqual(result.TotalSize, root.GetProperty("TotalSize").GetInt64());

            var chunks = root.GetProperty("Chunks");
            Assert.AreEqual(1, chunks.GetArrayLength());
            Assert.AreEqual(entry, chunks[0].GetProperty("File").GetString());
            Assert.IsTrue(chunks[0].GetProperty("IsEntry").GetBoolean());

            var css = root.GetProperty("Css");
            Assert.AreEqual(0, css.GetArrayLength());

            var staticAssets = root.GetProperty("StaticAssets");
            Assert.AreEqual(1, staticAssets.GetArrayLength());
            Assert.AreEqual("/favicon.svg", staticAssets[0].GetProperty("OriginalPath").GetString());
            Assert.AreEqual(
                GetHtmlRelativePath(tempDir, "dist", result.StaticAssets.Single().FilePath),
                staticAssets[0].GetProperty("File").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForExtractedCssWithSourceTreeUrlAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "styles"));
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
                import "./styles/app.css";
                console.log("manifest-css-asset");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "app.css"),
                """
                .app {
                  background-image: url("./logo.png?v=9");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "logo.png"),
                "fake-png-data");

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());
            Assert.AreEqual(result.TotalSize, root.GetProperty("TotalSize").GetInt64());

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var css = root.GetProperty("Css");
            CollectionAssert.Contains(
                css.EnumerateArray().Select(static item => item.GetString()).ToArray(),
                GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath));

            var imageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/styles/logo.png");
            var staticAsset = root.GetProperty("StaticAssets").EnumerateArray()
                .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/styles/logo.png", StringComparison.Ordinal));
            Assert.AreEqual(GetHtmlRelativePath(tempDir, "dist", imageAsset.FilePath), staticAsset.GetProperty("File").GetString());
            StringAssert.EndsWith(staticAsset.GetProperty("File").GetString(), ".png");
            Assert.AreNotEqual("styles/logo.png", staticAsset.GetProperty("File").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForCodeSplittingBuild()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                console.log("main");
                await import("./feature.js");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.js"),
                """
                export const featureMessage = "manifest-split";
                console.log(featureMessage);
                """);

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;
            var chunks = root.GetProperty("Chunks");
            Assert.IsTrue(chunks.GetArrayLength() >= 2);

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var splitChunk = result.Chunks.Single(static chunk => !chunk.IsEntry);
            var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            var splitChunkPath = GetHtmlRelativePath(tempDir, "dist", splitChunk.FilePath);

            Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());

            var manifestEntryChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), entryChunkPath, StringComparison.Ordinal));
            Assert.IsTrue(manifestEntryChunk.GetProperty("IsEntry").GetBoolean());
            CollectionAssert.Contains(
                manifestEntryChunk.GetProperty("Imports").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                splitChunkPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForCodeSplitChunkOwnedCss()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                import "./entry.css";
                console.log("main");
                globalThis.__loadFeature = () => import("./feature.js");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "entry.css"),
                """
                .entry-style-marker {
                  color: red;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.js"),
                """
                import "./feature.css";
                export const featureMessage = "manifest-split-css";
                console.log(featureMessage);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.css"),
                """
                .lazy-style-marker {
                  color: blue;
                }
                """);

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;
            var chunks = root.GetProperty("Chunks");
            Assert.IsTrue(chunks.GetArrayLength() >= 2);

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var splitChunk = result.Chunks.Single(static chunk => !chunk.IsEntry);
            var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            var splitChunkPath = GetHtmlRelativePath(tempDir, "dist", splitChunk.FilePath);
            var entryCssAsset = result.CssAssets.Single(asset => string.Equals(asset.OwnerChunkFilePath, entryChunk.FilePath, StringComparison.Ordinal));
            var lazyCssAsset = result.CssAssets.Single(asset => string.Equals(asset.OwnerChunkFilePath, splitChunk.FilePath, StringComparison.Ordinal));
            var entryCssPath = GetHtmlRelativePath(tempDir, "dist", entryCssAsset.FilePath);
            var lazyCssPath = GetHtmlRelativePath(tempDir, "dist", lazyCssAsset.FilePath);

            Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());

            var manifestEntryChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), entryChunkPath, StringComparison.Ordinal));
            CollectionAssert.Contains(
                manifestEntryChunk.GetProperty("Imports").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                splitChunkPath);
            CollectionAssert.Contains(
                manifestEntryChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                entryCssPath);
            CollectionAssert.DoesNotContain(
                manifestEntryChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                lazyCssPath);

            var manifestSplitChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), splitChunkPath, StringComparison.Ordinal));
            CollectionAssert.Contains(
                manifestSplitChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                lazyCssPath);
            CollectionAssert.DoesNotContain(
                manifestSplitChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                entryCssPath);

            var css = root.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(css, entryCssPath);
            CollectionAssert.DoesNotContain(css, lazyCssPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForTwoLazyChunksWithDistinctOwnedCss()
    {
        const string entryMarker = "entry-style-marker";
        const string lazyAMarker = "lazy-a-style-marker";
        const string lazyBMarker = "lazy-b-style-marker";
        const string lazyAChunkMarker = "lazy-a-payload";
        const string lazyBChunkMarker = "lazy-b-payload";

        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                import "./entry.css";
                await Promise.all([
                  import("./feature-a.js"),
                  import("./feature-b.js")
                ]);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "entry.css"),
                $$"""
                .{{entryMarker}} {
                  color: red;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-a.js"),
                $$"""
                import "./feature-a.css";
                export const featureMessageA = "{{lazyAChunkMarker}}";
                console.log(featureMessageA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-a.css"),
                $$"""
                .{{lazyAMarker}} {
                  color: blue;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                $$"""
                import "./feature-b.css";
                export const featureMessageB = "{{lazyBChunkMarker}}";
                console.log(featureMessageB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.css"),
                $$"""
                .{{lazyBMarker}} {
                  color: green;
                }
                """);

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCssAsset = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal)).Asset;
            var lazyACssAsset = cssOutputs.Single(output => output.Content.Contains(lazyAMarker, StringComparison.Ordinal)).Asset;
            var lazyBCssAsset = cssOutputs.Single(output => output.Content.Contains(lazyBMarker, StringComparison.Ordinal)).Asset;

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;
            var chunks = root.GetProperty("Chunks");
            Assert.IsTrue(chunks.GetArrayLength() >= 3);

            var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            var lazyAChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyAChunk.FilePath);
            var lazyBChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyBChunk.FilePath);
            var entryCssPath = GetHtmlRelativePath(tempDir, "dist", entryCssAsset.FilePath);
            var lazyACssPath = GetHtmlRelativePath(tempDir, "dist", lazyACssAsset.FilePath);
            var lazyBCssPath = GetHtmlRelativePath(tempDir, "dist", lazyBCssAsset.FilePath);

            Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());

            var manifestEntryChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), entryChunkPath, StringComparison.Ordinal));
            var manifestEntryImports = manifestEntryChunk.GetProperty("Imports").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(manifestEntryImports, lazyAChunkPath);
            CollectionAssert.Contains(manifestEntryImports, lazyBChunkPath);
            var manifestEntryCss = manifestEntryChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(manifestEntryCss, entryCssPath);
            CollectionAssert.DoesNotContain(manifestEntryCss, lazyACssPath);
            CollectionAssert.DoesNotContain(manifestEntryCss, lazyBCssPath);

            var manifestLazyAChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), lazyAChunkPath, StringComparison.Ordinal));
            var manifestLazyACss = manifestLazyAChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(manifestLazyACss, lazyACssPath);
            CollectionAssert.DoesNotContain(manifestLazyACss, entryCssPath);
            CollectionAssert.DoesNotContain(manifestLazyACss, lazyBCssPath);

            var manifestLazyBChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), lazyBChunkPath, StringComparison.Ordinal));
            var manifestLazyBCss = manifestLazyBChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(manifestLazyBCss, lazyBCssPath);
            CollectionAssert.DoesNotContain(manifestLazyBCss, entryCssPath);
            CollectionAssert.DoesNotContain(manifestLazyBCss, lazyACssPath);

            var css = root.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(css, entryCssPath);
            CollectionAssert.DoesNotContain(css, lazyACssPath);
            CollectionAssert.DoesNotContain(css, lazyBCssPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WritesManifest_ForLazyChunksThatShareStaticChunkCssClosure()
    {
        const string entryMarker = "entry-style-marker";
        const string sharedMarker = "shared-static-style-marker";
        const string sharedChunkMarker = "shared-static-payload";
        const string lazyAChunkMarker = "lazy-a-closure";
        const string lazyBChunkMarker = "lazy-b-closure";

        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                import "./entry.css";
                await Promise.all([
                  import("./feature-a.js"),
                  import("./feature-b.js")
                ]);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "entry.css"),
                $$"""
                .{{entryMarker}} {
                  color: red;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-a.js"),
                $$"""
                import { sharedA } from "./shared.js";
                export const featureMessageA = "{{lazyAChunkMarker}}";
                console.log(featureMessageA, sharedA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                $$"""
                import { sharedB } from "./shared.js";
                export const featureMessageB = "{{lazyBChunkMarker}}";
                console.log(featureMessageB, sharedB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared.js"),
                $$"""
                import "./shared.css";
                export const sharedA = "{{sharedChunkMarker}}";
                export const sharedB = "{{sharedChunkMarker}}";
                console.log(sharedA, sharedB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared.css"),
                $$"""
                .{{sharedMarker}} {
                  color: blue;
                }
                """);

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            Console.WriteLine("DEBUG ManifestSharedStatic chunkOutputs="
                + string.Join(", ", chunkOutputs.Select(output =>
                    $"{output.Chunk.FilePath}|entry={output.Chunk.IsEntry}|shared={output.Content.Contains(sharedChunkMarker, StringComparison.Ordinal)}|lazyA={output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)}|lazyB={output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)}")));
            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var sharedChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(sharedChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCssAsset = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal)).Asset;
            var sharedCssAsset = cssOutputs.Single(output => output.Content.Contains(sharedMarker, StringComparison.Ordinal)).Asset;

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!));
            var root = document.RootElement;
            var chunks = root.GetProperty("Chunks");

            var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            var sharedChunkPath = GetHtmlRelativePath(tempDir, "dist", sharedChunk.FilePath);
            var lazyAChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyAChunk.FilePath);
            var lazyBChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyBChunk.FilePath);
            var entryCssPath = GetHtmlRelativePath(tempDir, "dist", entryCssAsset.FilePath);
            var sharedCssPath = GetHtmlRelativePath(tempDir, "dist", sharedCssAsset.FilePath);

            var manifestEntryChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), entryChunkPath, StringComparison.Ordinal));
            var manifestEntryCss = manifestEntryChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(manifestEntryCss, entryCssPath);
            CollectionAssert.DoesNotContain(manifestEntryCss, sharedCssPath);

            var manifestSharedChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), sharedChunkPath, StringComparison.Ordinal));
            CollectionAssert.Contains(
                manifestSharedChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                sharedCssPath);

            var manifestLazyAChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), lazyAChunkPath, StringComparison.Ordinal));
            CollectionAssert.Contains(
                manifestLazyAChunk.GetProperty("Imports").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                sharedChunkPath);
            CollectionAssert.Contains(
                manifestLazyAChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                sharedCssPath);

            var manifestLazyBChunk = chunks.EnumerateArray()
                .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), lazyBChunkPath, StringComparison.Ordinal));
            CollectionAssert.Contains(
                manifestLazyBChunk.GetProperty("Imports").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                sharedChunkPath);
            CollectionAssert.Contains(
                manifestLazyBChunk.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray(),
                sharedCssPath);

            var css = root.GetProperty("Css").EnumerateArray().Select(static item => item.GetString()).ToArray();
            CollectionAssert.Contains(css, entryCssPath);
            CollectionAssert.DoesNotContain(css, sharedCssPath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-manifest-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetHtmlRelativePath(string rootDirectory, string outDirName, string rootRelativePath)
    {
        var outDirectory = Path.Combine(rootDirectory, outDirName);
        var absolutePath = Path.Combine(rootDirectory, rootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetRelativePath(outDirectory, absolutePath).Replace('\\', '/');
    }
}
