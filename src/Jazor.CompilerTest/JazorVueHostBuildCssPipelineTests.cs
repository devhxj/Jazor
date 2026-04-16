using System.Text.Json;
using System.Reflection;
using Jazor.VueHost.Build;
using Jazor.VueHost.DevServer;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
[DoNotParallelize]
public sealed class JazorVueHostBuildCssPipelineTests
{
    [TestMethod]
    public void CssUrlRewriter_RewriteAssetReferences_RewritesAbsoluteAndRelativeUrls()
    {
        var css = """
            .hero {
              background-image: url("/images/logo.png?v=1");
              mask-image: url('../fonts/ui.woff2#hash');
              cursor: url("https://cdn.example.com/cursor.cur"), auto;
              border-image: url(data:image/png;base64,abc=);
            }
            """;

        var result = CssUrlRewriter.RewriteAssetReferences(
            css,
            "assets/app-1234.css",
            [
                new AssetInfo
                {
                    FileName = "logo-5678.png",
                    FilePath = "images/logo-5678.png",
                    OriginalPath = "/images/logo.png",
                    Size = 128
                },
                new AssetInfo
                {
                    FileName = "ui-90ab.woff2",
                    FilePath = "fonts/ui-90ab.woff2",
                    OriginalPath = "/fonts/ui.woff2",
                    Size = 256
                }
            ]);

        StringAssert.Contains(result, "url(\"../images/logo-5678.png?v=1\")");
        StringAssert.Contains(result, "url('../fonts/ui-90ab.woff2#hash')");
        StringAssert.Contains(result, "url(\"https://cdn.example.com/cursor.cur\")");
        StringAssert.Contains(result, "url(data:image/png;base64,abc=)");
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ExtractsImportedCssIntoBuildAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "public"));

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
                import "./app.css";
                console.log("imported css");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "app.css"),
                """
                .app {
                  background-image: url("/logo.png?v=1");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "logo.png"),
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
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(cssAsset.FileName.Contains('-', StringComparison.Ordinal));
            StringAssert.EndsWith(cssAsset.FileName, ".css");
            Assert.IsNotNull(cssAsset.SourceMapPath);

            var imageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/logo.png");
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));
            var sourceMapPath = Path.Combine(tempDir, cssAsset.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));

            var cssContent = await File.ReadAllTextAsync(cssPath);
            var expectedImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, imageAsset.FilePath);
            StringAssert.Contains(cssContent, $"url(\"{expectedImagePath}?v=1\")");
            StringAssert.Contains(cssContent, $"/*# sourceMappingURL={Path.GetFileName(sourceMapPath)} */");

            using (var sourceMap = JsonDocument.Parse(await File.ReadAllTextAsync(sourceMapPath)))
            {
                Assert.AreEqual(cssAsset.FileName, sourceMap.RootElement.GetProperty("file").GetString());
                StringAssert.EndsWith(sourceMap.RootElement.GetProperty("sources")[0].GetString(), "app.css");
                var mappedLines = DecodeGeneratedLineToSourceLine(sourceMap.RootElement);
                AssertGeneratedLineMapsToSourceLine(cssContent, "background-image:", await File.ReadAllTextAsync(Path.Combine(tempDir, "app.css")), "background-image:", mappedLines);
            }

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ExtractsVueStyleContentIntoBuildAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string uniqueMarker = "vue-inline-style-sourcemap-marker";
            const string source = """
                <template>
                  <div class="app">Hello</div>
                </template>
                <style>
                .app {
                  color: red;
                }

                .vue-inline-style-sourcemap-marker {
                  border-color: blue;
                }
                </style>
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
                import App from "./App.vue";
                console.log(App);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "App.vue"),
                source);

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
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));
            Assert.IsNotNull(cssAsset.SourceMapPath);

            var sourceMapPath = Path.Combine(tempDir, cssAsset.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));

            var cssContent = await File.ReadAllTextAsync(cssPath);
            StringAssert.Contains(cssContent, "color: red");
            StringAssert.Contains(cssContent, uniqueMarker);
            StringAssert.Contains(cssContent, $"/*# sourceMappingURL={Path.GetFileName(sourceMapPath)} */");

            using (var sourceMap = JsonDocument.Parse(await File.ReadAllTextAsync(sourceMapPath)))
            {
                Assert.AreEqual(cssAsset.FileName, sourceMap.RootElement.GetProperty("file").GetString());
                StringAssert.EndsWith(sourceMap.RootElement.GetProperty("sources")[0].GetString(), "App.vue");
                Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            }

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");

            var chunkPath = Path.Combine(tempDir, result.Chunks.Single().FilePath.Replace('/', Path.DirectorySeparatorChar));
            var chunkContent = await File.ReadAllTextAsync(chunkPath);
            Assert.IsFalse(chunkContent.Contains("__jazorStyleId", StringComparison.Ordinal), "Build output should not inline dev-style style injection.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ExtractsVueStyleSrcContentIntoBuildAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string uniqueMarker = "vue-style-src-marker";

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
                import App from "./App.vue";
                console.log(App);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "App.vue"),
                """
                <template>
                  <div class="app">Hello style src</div>
                </template>
                <style src="./app.css"></style>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "app.css"),
                $$"""
                .app {
                  color: green;
                }

                .{{uniqueMarker}} {
                  border-color: blue;
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
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));

            var cssContent = await File.ReadAllTextAsync(cssPath);
            StringAssert.Contains(cssContent, uniqueMarker);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");

            var chunkPath = Path.Combine(tempDir, result.Chunks.Single().FilePath.Replace('/', Path.DirectorySeparatorChar));
            var chunkContent = await File.ReadAllTextAsync(chunkPath);
            Assert.IsFalse(chunkContent.Contains("__jazorStyleId", StringComparison.Ordinal), "Build output should not inline dev-style style injection.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_RewritesVueStyleSrcCssUrlsRelativeToExtractedCssAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string uniqueMarker = "vue-style-src-cross-dir-marker";

            Directory.CreateDirectory(Path.Combine(tempDir, "components"));
            Directory.CreateDirectory(Path.Combine(tempDir, "styles"));
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "styles"));

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
                import App from "./components/App.vue";
                console.log(App);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "components", "App.vue"),
                """
                <template>
                  <div class="app">Hello cross-directory style src</div>
                </template>
                <style src="../styles/app.css"></style>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "app.css"),
                $$"""
                .{{uniqueMarker}} {
                  background-image: url("./logo.png?v=7");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "styles", "logo.png"),
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
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));

            var cssContent = await File.ReadAllTextAsync(cssPath);
            var outputImages = Directory.GetFiles(Path.Combine(tempDir, "dist"), "*.png", SearchOption.AllDirectories);
            Assert.AreEqual(1, outputImages.Length, "Expected the build to emit one copied png asset for the CSS url().");

            var outputImagePath = outputImages[0];
            var outputImageRootRelativePath = Path.GetRelativePath(tempDir, outputImagePath).Replace('\\', '/');
            var expectedImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, outputImageRootRelativePath);

            StringAssert.Contains(cssContent, uniqueMarker);
            StringAssert.Contains(cssContent, $"url(\"{expectedImagePath}?v=7\")");
            Assert.IsFalse(cssContent.Contains("url(\"./logo.png?v=7\")", StringComparison.Ordinal), "Build output should rewrite the original relative CSS asset URL.");
            Assert.IsFalse(cssContent.Contains("url(\"../styles/logo.png?v=7\")", StringComparison.Ordinal), "Build output should not resolve CSS URLs relative to the Vue component source path.");

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");

            var chunkPath = Path.Combine(tempDir, result.Chunks.Single().FilePath.Replace('/', Path.DirectorySeparatorChar));
            var chunkContent = await File.ReadAllTextAsync(chunkPath);
            Assert.IsFalse(chunkContent.Contains("__jazorStyleId", StringComparison.Ordinal), "Build output should not inline dev-style style injection.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_RewritesAppImportedSourceTreeCssUrlsToCopiedHashedAssets()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string uniqueMarker = "app-imported-source-tree-css-marker";

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
                console.log("source-tree css asset rewrite");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "app.css"),
                $$"""
                .{{uniqueMarker}} {
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
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));

            var outputImages = Directory.GetFiles(Path.Combine(tempDir, "dist"), "*.png", SearchOption.AllDirectories);
            Assert.AreEqual(1, outputImages.Length, "Expected the build to emit one copied png asset for the CSS url().");

            var outputImagePath = outputImages[0];
            Assert.AreNotEqual("logo.png", Path.GetFileName(outputImagePath), "Build output should emit a hashed png asset.");

            var outputImageRootRelativePath = Path.GetRelativePath(tempDir, outputImagePath).Replace('\\', '/');
            var cssContent = await File.ReadAllTextAsync(cssPath);
            var expectedImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, outputImageRootRelativePath);

            StringAssert.Contains(cssContent, uniqueMarker);
            StringAssert.Contains(cssContent, $"url(\"{expectedImagePath}?v=9\")");

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            Assert.IsTrue(File.Exists(distIndexHtmlPath));
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_EmitsChunkOwnedExtractedCssAssets()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string entryMarker = "entry-style-marker";
            const string lazyMarker = "lazy-style-marker";

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head><title>split css ownership</title></head>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """
                import "./entry.css";
                console.log("main");
                await import("./feature.js");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "entry.css"),
                $$"""
                .{{entryMarker}} {
                  color: red;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.js"),
                """
                import "./feature.css";
                export const featureMessage = "feature-css";
                console.log(featureMessage);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.css"),
                $$"""
                .{{lazyMarker}} {
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
            Assert.IsTrue(result.Chunks.Count >= 2, "Expected code splitting to produce multiple chunks.");
            Assert.IsTrue(result.CssAssets.Count >= 2, "Expected separate extracted CSS assets for entry and lazy chunks.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var splitChunk = result.Chunks.Single(static chunk => !chunk.IsEntry);
            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var lazyCss = cssOutputs.Single(output => output.Content.Contains(lazyMarker, StringComparison.Ordinal));

            StringAssert.StartsWith(entryCss.Asset.FileName, "styles-");
            StringAssert.Contains(lazyCss.Asset.FileName, "-styles-");
            Assert.IsFalse(entryCss.Content.Contains(lazyMarker, StringComparison.Ordinal), "Entry CSS asset should not contain lazy chunk styles.");
            Assert.IsFalse(lazyCss.Content.Contains(entryMarker, StringComparison.Ordinal), "Lazy CSS asset should not contain entry chunk styles.");
            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.AreEqual(splitChunk.FilePath, lazyCss.Asset.OwnerChunkFilePath);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", lazyCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject lazy chunk CSS.");

            var entryChunkPath = Path.Combine(tempDir, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var entryChunkContent = await File.ReadAllTextAsync(entryChunkPath);
            StringAssert.Contains(entryChunkContent, $"./{splitChunk.FileName}");
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", lazyCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_EmitsDistinctChunkOwnedCssAssets_ForTwoLazyChunks()
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
            Console.WriteLine("DEBUG DistinctLazyChunks result.Chunks="
                + string.Join(", ", result.Chunks.Select(static chunk => $"{chunk.FilePath}|entry={chunk.IsEntry}|imports={string.Join(";", chunk.Imports)}")));
            Assert.IsTrue(result.Chunks.Count >= 3, "Expected code splitting to produce an entry chunk and two lazy chunks.");
            Assert.IsTrue(result.CssAssets.Count >= 3, "Expected extracted CSS assets for the entry and both lazy chunks.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            Console.WriteLine("DEBUG DistinctLazyChunks chunkOutputs="
                + string.Join(", ", chunkOutputs.Select(output =>
                    $"{output.Chunk.FilePath}|entry={output.Chunk.IsEntry}|lazyA={output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)}|lazyB={output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)}")));
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var lazyACss = cssOutputs.Single(output => output.Content.Contains(lazyAMarker, StringComparison.Ordinal));
            var lazyBCss = cssOutputs.Single(output => output.Content.Contains(lazyBMarker, StringComparison.Ordinal));

            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.AreEqual(lazyAChunk.FilePath, lazyACss.Asset.OwnerChunkFilePath);
            Assert.AreEqual(lazyBChunk.FilePath, lazyBCss.Asset.OwnerChunkFilePath);

            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), lazyACss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), lazyBCss.Asset.FilePath);
            CollectionAssert.Contains(lazyAChunk.Css.ToArray(), lazyACss.Asset.FilePath);
            CollectionAssert.DoesNotContain(lazyAChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(lazyAChunk.Css.ToArray(), lazyBCss.Asset.FilePath);
            CollectionAssert.Contains(lazyBChunk.Css.ToArray(), lazyBCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(lazyBChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(lazyBChunk.Css.ToArray(), lazyACss.Asset.FilePath);

            Assert.IsFalse(entryCss.Content.Contains(lazyAMarker, StringComparison.Ordinal));
            Assert.IsFalse(entryCss.Content.Contains(lazyBMarker, StringComparison.Ordinal));
            Assert.IsFalse(lazyACss.Content.Contains(entryMarker, StringComparison.Ordinal));
            Assert.IsFalse(lazyACss.Content.Contains(lazyBMarker, StringComparison.Ordinal));
            Assert.IsFalse(lazyBCss.Content.Contains(entryMarker, StringComparison.Ordinal));
            Assert.IsFalse(lazyBCss.Content.Contains(lazyAMarker, StringComparison.Ordinal));

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", lazyACss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject lazy-a CSS.");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", lazyBCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject lazy-b CSS.");

            var entryChunkContent = chunkOutputs.Single(output => string.Equals(output.Chunk.FilePath, entryChunk.FilePath, StringComparison.Ordinal)).Content;
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", lazyACss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", lazyBCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_DeduplicatesMultiOwnerSharedCssAcrossTwoLazyChunks()
    {
        const string entryMarker = "entry-style-marker";
        const string sharedMarker = "shared-style-marker";
        const string lazyAChunkMarker = "lazy-a-multi-owner";
        const string lazyBChunkMarker = "lazy-b-multi-owner";

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
                import "./shared.css";
                export const featureMessageA = "{{lazyAChunkMarker}}";
                console.log(featureMessageA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                $$"""
                import "./shared.css";
                export const featureMessageB = "{{lazyBChunkMarker}}";
                console.log(featureMessageB);
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
            Assert.IsTrue(result.Chunks.Count >= 3, "Expected an entry chunk and two lazy chunks.");
            Assert.AreEqual(2, result.CssAssets.Count, "Expected one entry CSS asset plus one shared multi-owner CSS asset.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var sharedCss = cssOutputs.Single(output => output.Content.Contains(sharedMarker, StringComparison.Ordinal));

            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.IsNull(sharedCss.Asset.OwnerChunkFilePath, "Shared CSS should expose multi-owner metadata instead of collapsing to a single owner.");
            CollectionAssert.AreEquivalent(
                new[] { lazyAChunk.FilePath, lazyBChunk.FilePath },
                sharedCss.Asset.OwnerChunkFilePaths.ToArray());

            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyAChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyBChunk.Css.ToArray(), sharedCss.Asset.FilePath);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", sharedCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject shared lazy CSS.");

            var entryChunkContent = chunkOutputs.Single(output => string.Equals(output.Chunk.FilePath, entryChunk.FilePath, StringComparison.Ordinal)).Content;
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", sharedCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_LoadsSharedStaticChunkCssViaLazyChunkClosure()
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
            Console.WriteLine("DEBUG SharedStaticChunk result.Chunks="
                + string.Join(", ", result.Chunks.Select(static chunk => $"{chunk.FilePath}|entry={chunk.IsEntry}|imports={string.Join(";", chunk.Imports)}")));

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            Console.WriteLine("DEBUG SharedStaticChunk chunkOutputs="
                + string.Join(", ", chunkOutputs.Select(output =>
                    $"{output.Chunk.FilePath}|entry={output.Chunk.IsEntry}|shared={output.Content.Contains(sharedChunkMarker, StringComparison.Ordinal)}|lazyA={output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)}|lazyB={output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)}")));
            var sharedChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(sharedChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var sharedCss = cssOutputs.Single(output => output.Content.Contains(sharedMarker, StringComparison.Ordinal));

            Assert.AreEqual(sharedChunk.FilePath, sharedCss.Asset.OwnerChunkFilePath);
            CollectionAssert.Contains(sharedChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyAChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyBChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", sharedCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Shared static-chunk CSS should be lazy, not eagerly injected into HTML.");

            var entryChunkContent = chunkOutputs.Single(output => string.Equals(output.Chunk.FilePath, entryChunk.FilePath, StringComparison.Ordinal)).Content;
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", sharedCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_WithoutSourceMap_PreservesMultiOwnerCssClosureViaFallbackOwnership()
    {
        const string entryMarker = "entry-style-marker-no-sourcemap";
        const string sharedMarker = "shared-style-marker-no-sourcemap";
        const string lazyAChunkMarker = "lazy-a-no-sourcemap";
        const string lazyBChunkMarker = "lazy-b-no-sourcemap";

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
                import "./shared.css";
                export const featureMessageA = "{{lazyAChunkMarker}}";
                console.log(featureMessageA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                $$"""
                import "./shared.css";
                export const featureMessageB = "{{lazyBChunkMarker}}";
                console.log(featureMessageB);
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
                    SourceMap = SourceMapOption.None,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(result.Chunks.Count >= 3, "Expected an entry chunk and two lazy chunks.");
            Assert.AreEqual(2, result.CssAssets.Count, "Expected one entry CSS asset plus one shared multi-owner CSS asset.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var sharedCss = cssOutputs.Single(output => output.Content.Contains(sharedMarker, StringComparison.Ordinal));

            Assert.IsNull(entryCss.Asset.SourceMapPath);
            Assert.IsNull(sharedCss.Asset.SourceMapPath);
            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.IsNull(sharedCss.Asset.OwnerChunkFilePath, "Shared CSS should still expose multi-owner metadata when source maps are disabled.");
            CollectionAssert.AreEquivalent(
                new[] { lazyAChunk.FilePath, lazyBChunk.FilePath },
                sharedCss.Asset.OwnerChunkFilePaths.ToArray());

            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyAChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyBChunk.Css.ToArray(), sharedCss.Asset.FilePath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_RepeatedDynamicImports_DedupesCssRuntimeLoadGuards()
    {
        const string lazyMarker = "lazy-style-dedupe-marker";
        const string lazyChunkMarker = "lazy-dedupe-payload";

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
                await import("./feature.js");
                await import("./feature.js");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.js"),
                $$"""
                import "./feature.css";
                export const featureMessage = "{{lazyChunkMarker}}";
                console.log(featureMessage);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.css"),
                $$"""
                .{{lazyMarker}} {
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
            Assert.IsTrue(result.Chunks.Count >= 2, "Expected code splitting to produce entry and lazy chunks.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var lazyChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var lazyCss = cssOutputs.Single(output => output.Content.Contains(lazyMarker, StringComparison.Ordinal)).Asset;

            var entryChunkContent = chunkOutputs.Single(output => string.Equals(output.Chunk.FilePath, entryChunk.FilePath, StringComparison.Ordinal)).Content;
            var lazyChunkSpecifier = $"./{lazyChunk.FileName}";
            Assert.IsTrue(
                CountOccurrences(entryChunkContent, lazyChunkSpecifier) >= 2,
                $"Expected repeated dynamic imports to preserve two '{lazyChunkSpecifier}' import expressions.");

            var lazyCssHref = GetHtmlRelativePath(tempDir, "dist", lazyCss.FilePath);
            Assert.IsTrue(
                CountOccurrences(entryChunkContent, lazyCssHref) >= 2,
                $"Expected repeated lazy imports to carry duplicated CSS href '{lazyCssHref}' so runtime dedupe can apply.");
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
            StringAssert.Contains(entryChunkContent, "__jazorLoadedCss ??= new Set()");
            StringAssert.Contains(entryChunkContent, "registry.has(href)");
            StringAssert.Contains(entryChunkContent, "link[rel=\"stylesheet\"][href=\"'+href+'\"]");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_ReadNormalizedSourceMapSources_WithMalformedJson_ReturnsEmpty()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var mapDirectory = Path.Combine(tempDir, "dist", "assets");
            Directory.CreateDirectory(mapDirectory);
            var sourceMapPath = Path.Combine(mapDirectory, "broken.css.map");
            File.WriteAllText(sourceMapPath, "{ invalid-json");

            var readSourcesMethod = typeof(BuildOrchestrator).GetMethod(
                "ReadNormalizedSourceMapSources",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(readSourcesMethod, "Expected to locate BuildOrchestrator.ReadNormalizedSourceMapSources.");

            var resolvedSources = readSourcesMethod.Invoke(
                null,
                [
                    tempDir,
                    Path.GetRelativePath(tempDir, sourceMapPath).Replace('\\', '/'),
                    new ModuleResolver(tempDir)
                ]) as IReadOnlyList<string>;

            Assert.IsNotNull(resolvedSources);
            Assert.AreEqual(0, resolvedSources.Count);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_RewritesCopiedPublicCssUrlsToHashedStaticAssets()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "images"));
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "styles"));

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head>
                  <link rel="stylesheet" href="/styles/site.css">
                </head>
                <body>
                  <div id="app"></div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """console.log("build css rewrite");""");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "styles", "site.css"),
                """
                body {
                  background-image: url("../images/logo.png#hero");
                  mask-image: url("/images/logo.png?v=2");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "images", "logo.png"),
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

            var imageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/images/logo.png");
            var publicCssAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/styles/site.css");

            var publicCssPath = Path.Combine(tempDir, publicCssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(publicCssPath));

            var publicCssContent = await File.ReadAllTextAsync(publicCssPath);

            var publicExpectedImagePath = GetRelativeOutputPath(tempDir, publicCssAsset.FilePath, imageAsset.FilePath);
            StringAssert.Contains(publicCssContent, $"url(\"{publicExpectedImagePath}?v=2\")");
            StringAssert.Contains(publicCssContent, $"url(\"{publicExpectedImagePath}#hero\")");

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            Assert.IsTrue(File.Exists(distIndexHtmlPath));
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, "href=\"styles/site.css\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static int CountOccurrences(string text, string value)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
        {
            return 0;
        }

        var count = 0;
        var startIndex = 0;
        while (true)
        {
            var index = text.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            startIndex = index + value.Length;
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-css-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string GetHtmlRelativePath(string rootDirectory, string outDirName, string rootRelativePath)
    {
        var outDirectory = Path.Combine(rootDirectory, outDirName);
        var absolutePath = Path.Combine(rootDirectory, rootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetRelativePath(outDirectory, absolutePath).Replace('\\', '/');
    }

    private static string GetRelativeOutputPath(string rootDirectory, string fromRootRelativePath, string toRootRelativePath)
    {
        var fromPath = Path.Combine(rootDirectory, fromRootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var toPath = Path.Combine(rootDirectory, toRootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var relativePath = Path.GetRelativePath(Path.GetDirectoryName(fromPath)!, toPath).Replace('\\', '/');
        return relativePath.StartsWith("./", StringComparison.Ordinal)
            ? relativePath[2..]
            : relativePath;
    }
}
