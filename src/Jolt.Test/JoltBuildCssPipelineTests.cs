using System.Text.Json;
using System.Reflection;
using Jolt.Build;
using Jolt.DevServer;
using static Jolt.Test.SourceMapTestHelpers;

namespace Jolt.Test;

[TestClass]
public sealed class JoltBuildCssPipelineTests
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
    public async Task BuildOrchestrator_BuildAsync_ExtractsImportedAndPublicCssAndRewritesReferencedAssets()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string sourceTreeMarker = "app-imported-source-tree-css-marker";

            Directory.CreateDirectory(Path.Combine(tempDir, "styles"));
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "images"));
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "styles"));

            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head>
                  <link rel="icon" href="/favicon.svg">
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
                """
                import "./styles/app.css";
                console.log("imported css");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "app.css"),
                $$"""
                .app {
                  background-image: url("/logo.png?v=1");
                }

                .{{sourceTreeMarker}} {
                  mask-image: url("./logo.png?v=9");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "styles", "site.css"),
                """
                body {
                  background-image: url("../images/logo.png#hero");
                  mask-image: url("/images/logo.png?v=2");
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "logo.png"),
                "fake-png-data");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "logo.png"),
                "fake-source-tree-png-data");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "images", "logo.png"),
                "fake-public-css-png-data");
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
            Assert.IsTrue(
                result.CssAssets.Any(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase)),
                "Expected an extracted styles-*.css asset.");

            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(cssAsset.FileName.Contains('-', StringComparison.Ordinal));
            StringAssert.EndsWith(cssAsset.FileName, ".css");
            Assert.IsNotNull(cssAsset.SourceMapPath);

            var rootImageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/logo.png");
            var sourceTreeImageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/styles/logo.png");
            var publicCssImageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/images/logo.png");
            var publicCssAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/styles/site.css");
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(cssPath));
            var sourceMapPath = Path.Combine(tempDir, cssAsset.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));
            Assert.AreNotEqual("logo.png", sourceTreeImageAsset.FileName, "Build output should emit a hashed png asset for source-tree CSS url() references.");

            var cssContent = await File.ReadAllTextAsync(cssPath);
            var expectedRootImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, rootImageAsset.FilePath);
            var expectedSourceTreeImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, sourceTreeImageAsset.FilePath);
            StringAssert.Contains(cssContent, sourceTreeMarker);
            StringAssert.Contains(cssContent, $"url(\"{expectedRootImagePath}?v=1\")");
            StringAssert.Contains(cssContent, $"url(\"{expectedSourceTreeImagePath}?v=9\")");
            StringAssert.Contains(cssContent, $"/*# sourceMappingURL={Path.GetFileName(sourceMapPath)} */");
            Assert.IsFalse(cssContent.Contains("url(\"/logo.png?v=1\")", StringComparison.Ordinal), "Build output should rewrite absolute public asset URLs referenced from imported CSS.");
            Assert.IsFalse(cssContent.Contains("url(\"./logo.png?v=9\")", StringComparison.Ordinal), "Build output should rewrite source-tree relative asset URLs referenced from imported CSS.");

            using (var sourceMap = JsonDocument.Parse(await File.ReadAllTextAsync(sourceMapPath)))
            {
                Assert.AreEqual(cssAsset.FileName, sourceMap.RootElement.GetProperty("file").GetString());
                StringAssert.EndsWith(sourceMap.RootElement.GetProperty("sources")[0].GetString(), "styles/app.css");
                var mappedLines = DecodeGeneratedLineToSourceLine(sourceMap.RootElement);
                AssertGeneratedLineMapsToSourceLine(cssContent, "background-image:", await File.ReadAllTextAsync(Path.Combine(tempDir, "styles", "app.css")), "background-image:", mappedLines);
            }

            var publicCssPath = Path.Combine(tempDir, publicCssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(publicCssPath));
            var publicCssContent = await File.ReadAllTextAsync(publicCssPath);
            var expectedPublicCssImagePath = GetRelativeOutputPath(tempDir, publicCssAsset.FilePath, publicCssImageAsset.FilePath);
            StringAssert.Contains(publicCssContent, $"url(\"{expectedPublicCssImagePath}?v=2\")");
            StringAssert.Contains(publicCssContent, $"url(\"{expectedPublicCssImagePath}#hero\")");

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath)}\"");
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", publicCssAsset.FilePath)}\"");

            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using (var manifestDocument = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!)))
            {
                var root = manifestDocument.RootElement;
                var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
                var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
                var entryCssPath = GetHtmlRelativePath(tempDir, "dist", cssAsset.FilePath);
                var faviconAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/favicon.svg");

                Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());
                Assert.AreEqual(result.TotalSize, root.GetProperty("TotalSize").GetInt64());

                var manifestChunks = root.GetProperty("Chunks");
                Assert.AreEqual(1, manifestChunks.GetArrayLength());
                var manifestEntryChunk = manifestChunks[0];
                Assert.AreEqual(entryChunkPath, manifestEntryChunk.GetProperty("File").GetString());
                Assert.IsTrue(manifestEntryChunk.GetProperty("IsEntry").GetBoolean());
                CollectionAssert.Contains(GetManifestStringArray(manifestEntryChunk, "Css"), entryCssPath);
                CollectionAssert.Contains(GetManifestStringArray(root, "Css"), entryCssPath);

                var manifestLogo = root.GetProperty("StaticAssets").EnumerateArray()
                    .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/logo.png", StringComparison.Ordinal));
                Assert.AreEqual(
                    GetHtmlRelativePath(tempDir, "dist", rootImageAsset.FilePath),
                    manifestLogo.GetProperty("File").GetString());

                var manifestFavicon = root.GetProperty("StaticAssets").EnumerateArray()
                    .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/favicon.svg", StringComparison.Ordinal));
                Assert.AreEqual(
                    GetHtmlRelativePath(tempDir, "dist", faviconAsset.FilePath),
                    manifestFavicon.GetProperty("File").GetString());

                var manifestSourceTreeLogo = root.GetProperty("StaticAssets").EnumerateArray()
                    .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/styles/logo.png", StringComparison.Ordinal));
                Assert.AreEqual(
                    GetHtmlRelativePath(tempDir, "dist", sourceTreeImageAsset.FilePath),
                    manifestSourceTreeLogo.GetProperty("File").GetString());

                var manifestPublicCssImage = root.GetProperty("StaticAssets").EnumerateArray()
                    .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/images/logo.png", StringComparison.Ordinal));
                Assert.AreEqual(
                    GetHtmlRelativePath(tempDir, "dist", publicCssImageAsset.FilePath),
                    manifestPublicCssImage.GetProperty("File").GetString());

                var manifestPublicCss = root.GetProperty("StaticAssets").EnumerateArray()
                    .Single(asset => string.Equals(asset.GetProperty("OriginalPath").GetString(), "/styles/site.css", StringComparison.Ordinal));
                Assert.AreEqual(
                    GetHtmlRelativePath(tempDir, "dist", publicCssAsset.FilePath),
                    manifestPublicCss.GetProperty("File").GetString());
            }
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_MinifyTrue_CompressesExtractedCss()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
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
                console.log("css minify test");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "app.css"),
                """
                .app {
                  color : red ;
                  padding : 4px 8px ;
                }
                """);

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "dist",
                    SourceMap = SourceMapOption.None,
                    Minify = true,
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var cssAsset = result.CssAssets.Single(static asset => asset.FileName.StartsWith("styles-", StringComparison.OrdinalIgnoreCase));
            var cssPath = Path.Combine(tempDir, cssAsset.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var cssContent = await File.ReadAllTextAsync(cssPath);

            Assert.IsFalse(cssContent.Contains('\n'), "Expected minified CSS to be emitted as compact text.");
            Assert.IsFalse(cssContent.Contains("  ", StringComparison.Ordinal), "Expected minified CSS to remove redundant spaces.");
            StringAssert.Contains(cssContent, ".app{color:red;padding:4px 8px}");
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ExtractsVueInlineAndStyleSrcCssIntoBuildAsset()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string inlineMarker = "vue-inline-style-sourcemap-marker";
            const string styleSrcMarker = "vue-style-src-cross-dir-marker";
            const string appSource = """
                <template>
                  <div class="app">Hello combined styles</div>
                </template>
                <style>
                .app {
                  color: red;
                }

                .vue-inline-style-sourcemap-marker {
                  border-color: blue;
                }
                </style>
                <style src="../styles/app.css"></style>
                """;
            const string cssSource = $$"""
                .{{styleSrcMarker}} {
                  background-image: url("./logo.png?v=7");
                }
                """;

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
                appSource);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "styles", "app.css"),
                cssSource);
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
            Assert.IsNotNull(cssAsset.SourceMapPath);

            var sourceMapPath = Path.Combine(tempDir, cssAsset.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));

            var imageAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/styles/logo.png");
            var cssContent = await File.ReadAllTextAsync(cssPath);
            var expectedImagePath = GetRelativeOutputPath(tempDir, cssAsset.FilePath, imageAsset.FilePath);

            StringAssert.Contains(cssContent, "color: red");
            StringAssert.Contains(cssContent, inlineMarker);
            StringAssert.Contains(cssContent, styleSrcMarker);
            StringAssert.Contains(cssContent, $"url(\"{expectedImagePath}?v=7\")");
            StringAssert.Contains(cssContent, $"/*# sourceMappingURL={Path.GetFileName(sourceMapPath)} */");
            Assert.IsFalse(cssContent.Contains("url(\"./logo.png?v=7\")", StringComparison.Ordinal), "Build output should rewrite the original relative CSS asset URL.");
            Assert.IsFalse(cssContent.Contains("url(\"../styles/logo.png?v=7\")", StringComparison.Ordinal), "Build output should not resolve CSS URLs relative to the Vue component source path.");

            using (var sourceMap = JsonDocument.Parse(await File.ReadAllTextAsync(sourceMapPath)))
            {
                Assert.AreEqual(cssAsset.FileName, sourceMap.RootElement.GetProperty("file").GetString());
                AssertSourceMapContainsSourceContent(sourceMap.RootElement, "components/App.vue", appSource);
                AssertSourceMapContainsSourceContent(sourceMap.RootElement, "styles/app.css", cssSource);
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
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
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
            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = true
            };
            var result = await BuildWithLazyChunkRetryAsync(orchestrator, options, CancellationToken.None);

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
            var splitChunkSpecifier = $"./{splitChunk.FileName}";
            Assert.IsTrue(
                CountOccurrences(entryChunkContent, splitChunkSpecifier) >= 2,
                $"Expected repeated dynamic imports to preserve two '{splitChunkSpecifier}' import expressions.");
            StringAssert.Contains(entryChunkContent, $"./{splitChunk.FileName}");
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", lazyCss.Asset.FilePath));
            Assert.IsTrue(
                CountOccurrences(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", lazyCss.Asset.FilePath)) >= 2,
                "Expected repeated lazy imports to preserve duplicated CSS hrefs so runtime dedupe can apply.");
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
            StringAssert.Contains(entryChunkContent, "__jazorLoadedCss ??= new Set()");
            StringAssert.Contains(entryChunkContent, "registry.has(href)");
            StringAssert.Contains(entryChunkContent, "link[rel=\"stylesheet\"][href=\"'+href+'\"]");
        }
        finally
        {
            DeleteDirectory(tempDir);
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
            Assert.IsTrue(result.Chunks.Count >= 3, "Expected code splitting to produce an entry chunk and two lazy chunks.");
            Assert.IsTrue(result.CssAssets.Count >= 3, "Expected extracted CSS assets for the entry and both lazy chunks.");

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

            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using (var manifestDocument = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!)))
            {
                var root = manifestDocument.RootElement;
                var manifestChunks = root.GetProperty("Chunks");
                var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
                var lazyAChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyAChunk.FilePath);
                var lazyBChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyBChunk.FilePath);
                var entryCssPath = GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath);
                var lazyACssPath = GetHtmlRelativePath(tempDir, "dist", lazyACss.Asset.FilePath);
                var lazyBCssPath = GetHtmlRelativePath(tempDir, "dist", lazyBCss.Asset.FilePath);

                Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());
                Assert.AreEqual(result.TotalSize, root.GetProperty("TotalSize").GetInt64());

                var manifestEntryChunk = GetManifestChunk(manifestChunks, entryChunkPath);
                var manifestEntryImports = GetManifestStringArray(manifestEntryChunk, "Imports");
                CollectionAssert.Contains(manifestEntryImports, lazyAChunkPath);
                CollectionAssert.Contains(manifestEntryImports, lazyBChunkPath);

                var manifestEntryCss = GetManifestStringArray(manifestEntryChunk, "Css");
                CollectionAssert.Contains(manifestEntryCss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestEntryCss, lazyACssPath);
                CollectionAssert.DoesNotContain(manifestEntryCss, lazyBCssPath);

                var manifestLazyAChunk = GetManifestChunk(manifestChunks, lazyAChunkPath);
                var manifestLazyACss = GetManifestStringArray(manifestLazyAChunk, "Css");
                CollectionAssert.Contains(manifestLazyACss, lazyACssPath);
                CollectionAssert.DoesNotContain(manifestLazyACss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestLazyACss, lazyBCssPath);

                var manifestLazyBChunk = GetManifestChunk(manifestChunks, lazyBChunkPath);
                var manifestLazyBCss = GetManifestStringArray(manifestLazyBChunk, "Css");
                CollectionAssert.Contains(manifestLazyBCss, lazyBCssPath);
                CollectionAssert.DoesNotContain(manifestLazyBCss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestLazyBCss, lazyACssPath);

                var manifestCss = GetManifestStringArray(root, "Css");
                CollectionAssert.Contains(manifestCss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestCss, lazyACssPath);
                CollectionAssert.DoesNotContain(manifestCss, lazyBCssPath);
            }
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_PreservesDirectSharedAndStaticChunkCssClosuresAcrossTwoLazyChunks()
    {
        const string entryMarker = "entry-style-marker";
        const string directSharedMarker = "shared-direct-style-marker";
        const string staticSharedMarker = "shared-static-style-marker";
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
                Path.Combine(tempDir, "feature-a-helper.js"),
                """
                export const helperA = "feature-a-helper";
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b-helper.js"),
                """
                export const helperB = "feature-b-helper";
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-a.js"),
                $$"""
                import { helperA } from "./feature-a-helper.js";
                import "./shared-direct.css";
                import { sharedA } from "./shared.js";
                export const featureMessageA = "{{lazyAChunkMarker}}";
                console.log(featureMessageA, helperA, sharedA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                $$"""
                import { helperB } from "./feature-b-helper.js";
                import "./shared-direct.css";
                import { sharedB } from "./shared.js";
                export const featureMessageB = "{{lazyBChunkMarker}}";
                console.log(featureMessageB, helperB, sharedB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared-direct.css"),
                $$"""
                .{{directSharedMarker}} {
                  color: blue;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared.js"),
                $$"""
                import "./shared-static.css";
                export const sharedA = "{{sharedChunkMarker}}";
                export const sharedB = "{{sharedChunkMarker}}";
                console.log(sharedA, sharedB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared-static.css"),
                $$"""
                .{{staticSharedMarker}} {
                  color: purple;
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
            Assert.IsTrue(result.Chunks.Any(static chunk => !chunk.IsEntry), "Expected at least one lazy chunk.");
            Assert.AreEqual(3, result.CssAssets.Count, "Expected entry CSS plus direct-shared and static-shared lazy CSS assets.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var sharedChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(sharedChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyAChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyAChunkMarker, StringComparison.Ordinal)).Chunk;
            var lazyBChunk = chunkOutputs.Single(output => !output.Chunk.IsEntry && output.Content.Contains(lazyBChunkMarker, StringComparison.Ordinal)).Chunk;

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var directSharedCss = cssOutputs.Single(output => output.Content.Contains(directSharedMarker, StringComparison.Ordinal));
            var staticSharedCss = cssOutputs.Single(output => output.Content.Contains(staticSharedMarker, StringComparison.Ordinal));
            var directSharedOwnerChunkPaths = directSharedCss.Asset.OwnerChunkFilePaths.Count > 0
                ? directSharedCss.Asset.OwnerChunkFilePaths
                : string.IsNullOrWhiteSpace(directSharedCss.Asset.OwnerChunkFilePath)
                    ? []
                    : [directSharedCss.Asset.OwnerChunkFilePath!];

            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.AreEqual(sharedChunk.FilePath, staticSharedCss.Asset.OwnerChunkFilePath);
            Assert.IsTrue(directSharedOwnerChunkPaths.Count > 0, "Expected direct shared CSS to keep at least one lazy owner chunk.");
            Assert.IsFalse(
                directSharedOwnerChunkPaths.Contains(entryChunk.FilePath, StringComparer.Ordinal),
                "Direct shared CSS owners should remain lazy and never collapse to the entry chunk.");

            foreach (var ownerChunkFilePath in directSharedOwnerChunkPaths)
            {
                var ownerChunk = result.Chunks.SingleOrDefault(chunk => string.Equals(chunk.FilePath, ownerChunkFilePath, StringComparison.Ordinal));
                Assert.IsNotNull(ownerChunk, $"Expected direct shared CSS owner chunk '{ownerChunkFilePath}' to exist in emitted chunks.");
                Assert.IsFalse(ownerChunk.IsEntry, "Direct shared CSS owner chunks must stay lazy.");
                Assert.AreNotEqual(sharedChunk.FilePath, ownerChunk.FilePath, "Direct shared CSS should stay attached to lazy owners, not the shared static chunk.");
                CollectionAssert.Contains(ownerChunk.Css.ToArray(), directSharedCss.Asset.FilePath);
            }

            if (directSharedOwnerChunkPaths.Count == 1)
            {
                Assert.AreEqual(
                    directSharedOwnerChunkPaths[0],
                    directSharedCss.Asset.OwnerChunkFilePath,
                    "When the bundler merges lazy modules, direct shared CSS should collapse to one lazy owner.");
            }
            else
            {
                Assert.IsNull(directSharedCss.Asset.OwnerChunkFilePath, "Direct shared CSS should expose multi-owner metadata instead of collapsing to a single owner.");
            }

            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), directSharedCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), staticSharedCss.Asset.FilePath);
            CollectionAssert.Contains(sharedChunk.Css.ToArray(), staticSharedCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(sharedChunk.Css.ToArray(), directSharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyAChunk.Css.ToArray(), staticSharedCss.Asset.FilePath);
            CollectionAssert.Contains(lazyBChunk.Css.ToArray(), staticSharedCss.Asset.FilePath);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", directSharedCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject direct shared lazy CSS.");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", staticSharedCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject static-chunk lazy CSS.");

            var entryChunkContent = chunkOutputs.Single(output => string.Equals(output.Chunk.FilePath, entryChunk.FilePath, StringComparison.Ordinal)).Content;
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", directSharedCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, GetHtmlRelativePath(tempDir, "dist", staticSharedCss.Asset.FilePath));
            StringAssert.Contains(entryChunkContent, "__jazorImportCss");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.ManifestPath));
            Assert.IsTrue(File.Exists(result.ManifestPath!));

            using (var manifestDocument = JsonDocument.Parse(await File.ReadAllTextAsync(result.ManifestPath!)))
            {
                var root = manifestDocument.RootElement;
                var manifestChunks = root.GetProperty("Chunks");
                var entryChunkPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
                var sharedChunkPath = GetHtmlRelativePath(tempDir, "dist", sharedChunk.FilePath);
                var lazyAChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyAChunk.FilePath);
                var lazyBChunkPath = GetHtmlRelativePath(tempDir, "dist", lazyBChunk.FilePath);
                var entryCssPath = GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath);
                var directSharedCssPath = GetHtmlRelativePath(tempDir, "dist", directSharedCss.Asset.FilePath);
                var staticSharedCssPath = GetHtmlRelativePath(tempDir, "dist", staticSharedCss.Asset.FilePath);

                Assert.AreEqual(entryChunkPath, root.GetProperty("Entry").GetString());
                Assert.AreEqual(result.TotalSize, root.GetProperty("TotalSize").GetInt64());

                var manifestEntryChunk = GetManifestChunk(manifestChunks, entryChunkPath);
                var manifestEntryCss = GetManifestStringArray(manifestEntryChunk, "Css");
                CollectionAssert.Contains(manifestEntryCss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestEntryCss, directSharedCssPath);
                CollectionAssert.DoesNotContain(manifestEntryCss, staticSharedCssPath);

                var manifestSharedChunk = GetManifestChunk(manifestChunks, sharedChunkPath);
                CollectionAssert.Contains(GetManifestStringArray(manifestSharedChunk, "Css"), staticSharedCssPath);
                CollectionAssert.DoesNotContain(GetManifestStringArray(manifestSharedChunk, "Css"), directSharedCssPath);

                var manifestLazyAChunk = GetManifestChunk(manifestChunks, lazyAChunkPath);
                CollectionAssert.Contains(GetManifestStringArray(manifestLazyAChunk, "Imports"), sharedChunkPath);
                CollectionAssert.Contains(GetManifestStringArray(manifestLazyAChunk, "Css"), staticSharedCssPath);
                if (directSharedOwnerChunkPaths.Contains(lazyAChunk.FilePath, StringComparer.Ordinal))
                {
                    CollectionAssert.Contains(GetManifestStringArray(manifestLazyAChunk, "Css"), directSharedCssPath);
                }

                var manifestLazyBChunk = GetManifestChunk(manifestChunks, lazyBChunkPath);
                CollectionAssert.Contains(GetManifestStringArray(manifestLazyBChunk, "Imports"), sharedChunkPath);
                CollectionAssert.Contains(GetManifestStringArray(manifestLazyBChunk, "Css"), staticSharedCssPath);
                if (directSharedOwnerChunkPaths.Contains(lazyBChunk.FilePath, StringComparer.Ordinal))
                {
                    CollectionAssert.Contains(GetManifestStringArray(manifestLazyBChunk, "Css"), directSharedCssPath);
                }

                var manifestCss = GetManifestStringArray(root, "Css");
                CollectionAssert.Contains(manifestCss, entryCssPath);
                CollectionAssert.DoesNotContain(manifestCss, directSharedCssPath);
                CollectionAssert.DoesNotContain(manifestCss, staticSharedCssPath);
            }
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_WithoutSourceMap_PreservesMultiOwnerCssClosureViaFallbackOwnership()
    {
        const string entryMarker = "entry-style-marker-no-sourcemap";
        const string sharedMarker = "shared-style-marker-no-sourcemap";

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
                Path.Combine(tempDir, "feature-a-helper.js"),
                """
                export const helperA = "feature-a-helper-no-sourcemap";
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b-helper.js"),
                """
                export const helperB = "feature-b-helper-no-sourcemap";
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-a.js"),
                """
                import { helperA } from "./feature-a-helper.js";
                import "./shared.css";
                console.log("feature-a-no-sourcemap", helperA);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature-b.js"),
                """
                import { helperB } from "./feature-b-helper.js";
                import "./shared.css";
                console.log("feature-b-no-sourcemap", helperB);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "shared.css"),
                $$"""
                .{{sharedMarker}} {
                  color: blue;
                }
                """);

            var orchestrator = new BuildOrchestrator();
            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.None,
                Minify = false,
                CodeSplitting = true
            };
            var result = await BuildWithLazyChunkRetryAsync(orchestrator, options, CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(result.Chunks.Any(static chunk => !chunk.IsEntry), "Expected at least one lazy chunk.");
            Assert.AreEqual(2, result.CssAssets.Count, "Expected one entry CSS asset plus one shared multi-owner CSS asset.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var chunkOutputs = await Task.WhenAll(result.Chunks.Select(async chunk => new
            {
                Chunk = chunk,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, chunk.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            var sharedCss = cssOutputs.Single(output => output.Content.Contains(sharedMarker, StringComparison.Ordinal));
            var sharedOwnerChunkPaths = sharedCss.Asset.OwnerChunkFilePaths.Count > 0
                ? sharedCss.Asset.OwnerChunkFilePaths
                : string.IsNullOrWhiteSpace(sharedCss.Asset.OwnerChunkFilePath)
                    ? []
                    : [sharedCss.Asset.OwnerChunkFilePath!];

            Assert.IsNull(entryCss.Asset.SourceMapPath);
            Assert.IsNull(sharedCss.Asset.SourceMapPath);
            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);
            Assert.IsTrue(sharedOwnerChunkPaths.Count > 0, "Expected shared CSS to keep at least one lazy owner chunk.");
            Assert.IsFalse(
                sharedOwnerChunkPaths.Contains(entryChunk.FilePath, StringComparer.Ordinal),
                "Shared CSS owners should remain lazy and never collapse to the entry chunk.");

            foreach (var ownerChunkFilePath in sharedOwnerChunkPaths)
            {
                var ownerChunk = result.Chunks.SingleOrDefault(chunk => string.Equals(chunk.FilePath, ownerChunkFilePath, StringComparison.Ordinal));
                Assert.IsNotNull(ownerChunk, $"Expected shared CSS owner chunk '{ownerChunkFilePath}' to exist in emitted chunks.");
                Assert.IsFalse(ownerChunk.IsEntry, "Shared CSS owner chunks must stay lazy.");
                CollectionAssert.Contains(ownerChunk.Css.ToArray(), sharedCss.Asset.FilePath);
            }

            if (sharedOwnerChunkPaths.Count == 1)
            {
                Assert.AreEqual(
                    sharedOwnerChunkPaths[0],
                    sharedCss.Asset.OwnerChunkFilePath,
                    "When source maps are disabled and lazy modules merge, shared CSS should collapse to one lazy owner.");
            }
            else
            {
                Assert.IsNull(sharedCss.Asset.OwnerChunkFilePath, "Shared CSS should still expose multi-owner metadata when source maps are disabled.");
            }

            CollectionAssert.Contains(entryChunk.Css.ToArray(), entryCss.Asset.FilePath);
            CollectionAssert.DoesNotContain(entryChunk.Css.ToArray(), sharedCss.Asset.FilePath);
        }
        finally
        {
            DeleteDirectory(tempDir);
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
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_ReadNormalizedSourceMapSources_WithInvalidSourcePath_ReturnsEmpty()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var mapDirectory = Path.Combine(tempDir, "dist", "assets");
            Directory.CreateDirectory(mapDirectory);
            var sourceMapPath = Path.Combine(mapDirectory, "invalid-source.css.map");
            File.WriteAllText(
                sourceMapPath,
                """{"version":3,"sources":["bad\u0000.css"],"names":[],"mappings":""}""");

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
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_ReadNormalizedSourceMapSources_WithPathEscapingRoot_ReturnsEmpty()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var sourceMapPath = Path.Combine(externalDir, "outside.css.map");
            File.WriteAllText(sourceMapPath, """{"version":3,"sources":["/main.css"],"names":[],"mappings":""}""");

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
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_ReadChunkSourceModules_WithPathEscapingRoot_ReturnsEmpty()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var sourceMapPath = Path.Combine(externalDir, "outside.js.map");
            File.WriteAllText(sourceMapPath, """{"version":3,"sources":["/main.js"],"names":[],"mappings":""}""");
            var chunk = new ChunkInfo
            {
                FileName = "main.js",
                FilePath = "dist/main.js",
                Size = 0,
                SourceMapPath = Path.GetRelativePath(tempDir, sourceMapPath).Replace('\\', '/')
            };

            var readModulesMethod = typeof(BuildOrchestrator).GetMethod(
                "ReadChunkSourceModules",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(readModulesMethod, "Expected to locate BuildOrchestrator.ReadChunkSourceModules.");

            var sourceModules = readModulesMethod.Invoke(
                null,
                [
                    tempDir,
                    chunk,
                    new Dictionary<string, CompilationResult>(StringComparer.OrdinalIgnoreCase),
                    new ModuleResolver(tempDir)
                ]) as ISet<string>;

            Assert.IsNotNull(sourceModules);
            Assert.AreEqual(0, sourceModules.Count);
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_ReadChunkSourceModules_WithExternalFileUriSource_ReturnsEmpty()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var externalModulePath = Path.Combine(externalDir, "external.js");
            File.WriteAllText(externalModulePath, """console.log("external");""");

            var mapDirectory = Path.Combine(tempDir, "dist", "assets");
            Directory.CreateDirectory(mapDirectory);
            var sourceMapPath = Path.Combine(mapDirectory, "main.js.map");
            var externalModuleUri = new Uri(externalModulePath).AbsoluteUri;
            File.WriteAllText(
                sourceMapPath,
                $$"""{"version":3,"sources":[{{JsonSerializer.Serialize(externalModuleUri)}}],"names":[],"mappings":""}""");
            var chunk = new ChunkInfo
            {
                FileName = "main.js",
                FilePath = "dist/assets/main.js",
                Size = 0,
                SourceMapPath = Path.GetRelativePath(tempDir, sourceMapPath).Replace('\\', '/')
            };
            var cachedResults = new Dictionary<string, CompilationResult>(StringComparer.OrdinalIgnoreCase)
            {
                [Path.GetFullPath(externalModulePath)] = new CompilationResult
                {
                    ContentType = "application/javascript",
                    Content = """console.log("external");"""
                }
            };

            var readModulesMethod = typeof(BuildOrchestrator).GetMethod(
                "ReadChunkSourceModules",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(readModulesMethod, "Expected to locate BuildOrchestrator.ReadChunkSourceModules.");

            var sourceModules = readModulesMethod.Invoke(
                null,
                [
                    tempDir,
                    chunk,
                    cachedResults,
                    new ModuleResolver(tempDir)
                ]) as ISet<string>;

            Assert.IsNotNull(sourceModules);
            Assert.AreEqual(0, sourceModules.Count);
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_TryReadTrustedSourceMapContent_WithInvalidSourcePath_ReturnsFalse()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var result = BuildOrchestrator.TryReadTrustedSourceMapContent(
                tempDir,
                "bad\0.css",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                out var trustedSourcePath,
                out var sourceContent);

            Assert.IsFalse(result);
            Assert.IsNull(trustedSourcePath);
            Assert.AreEqual(string.Empty, sourceContent);
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_TryReadTrustedSourceMapContent_WithSourceInsideRoot_ReturnsTrue()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var sourcePath = Path.Combine(tempDir, "src", "app.css");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, ".app { color: red; }");
            var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var result = BuildOrchestrator.TryReadTrustedSourceMapContent(
                tempDir,
                sourcePath,
                cache,
                out var trustedSourcePath,
                out var sourceContent);

            Assert.IsTrue(result);
            Assert.AreEqual(Path.GetFullPath(sourcePath), trustedSourcePath);
            Assert.AreEqual(".app { color: red; }", sourceContent);
            Assert.IsTrue(cache.ContainsKey(Path.GetFullPath(sourcePath)));
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_TryReadTrustedSourceMapContent_WithSourceOutsideRoot_ReturnsFalse()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var sourcePath = Path.Combine(externalDir, "external.css");
            File.WriteAllText(sourcePath, ".external { color: red; }");

            var result = BuildOrchestrator.TryReadTrustedSourceMapContent(
                tempDir,
                sourcePath,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                out var trustedSourcePath,
                out var sourceContent);

            Assert.IsFalse(result);
            Assert.IsNull(trustedSourcePath);
            Assert.AreEqual(string.Empty, sourceContent);
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_CreateCssDependencyFragmentAsync_WithExternalCssPath_ReturnsNull()
    {
        var tempDir = CreateTemporaryDirectory();
        var externalDir = CreateTemporaryDirectory();
        try
        {
            var externalCssPath = Path.Combine(externalDir, "external.css");
            await File.WriteAllTextAsync(externalCssPath, ".external { color: red; }");

            var method = typeof(BuildOrchestrator).GetMethod(
                "CreateCssDependencyFragmentAsync",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "Expected to locate BuildOrchestrator.CreateCssDependencyFragmentAsync.");

            var invocation = method.Invoke(
                null,
                [
                    externalCssPath,
                    Array.Empty<string>(),
                    new Dictionary<string, CompilationResult>(StringComparer.OrdinalIgnoreCase),
                    new ModuleResolver(tempDir, enforceTrustedProjectPaths: true),
                    CancellationToken.None
                ]);
            Assert.IsNotNull(invocation);

            var asTaskMethod = invocation.GetType().GetMethod("AsTask", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(asTaskMethod, "Expected ValueTask<T>.AsTask().");

            var task = asTaskMethod.Invoke(invocation, null) as Task;
            Assert.IsNotNull(task, "Expected ValueTask<T>.AsTask() to produce a Task instance.");
            await task;

            var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(resultProperty, "Expected Task<T>.Result.");
            var result = resultProperty.GetValue(task);

            Assert.IsNull(result);
        }
        finally
        {
            DeleteDirectory(tempDir);
            DeleteDirectory(externalDir);
        }
    }

    [TestMethod]
    public void BuildOrchestrator_TryReadSourceMapContent_WhenFileIsLocked_ReturnsFalseInsteadOfThrowing()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var sourcePath = Path.Combine(tempDir, "locked.css");
            File.WriteAllText(sourcePath, ".app { color: red; }");

            var method = typeof(BuildOrchestrator).GetMethod(
                "TryReadSourceMapContent",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "Expected to locate BuildOrchestrator.TryReadSourceMapContent.");

            using var lockHandle = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            object?[] args =
            [
                sourcePath,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                null
            ];

            var result = method.Invoke(null, args);

            Assert.IsInstanceOfType<bool>(result);
            Assert.IsFalse((bool)result);
            Assert.AreEqual(string.Empty, args[2] as string);
        }
        finally
        {
            DeleteDirectory(tempDir);
        }
    }

    private static async Task<BuildResult> BuildWithLazyChunkRetryAsync(
        BuildOrchestrator orchestrator,
        BuildOptions options,
        CancellationToken cancellationToken)
    {
        var result = await orchestrator.BuildAsync(options, cancellationToken);
        if (!result.Success || !options.CodeSplitting || result.Chunks.Any(static chunk => !chunk.IsEntry))
        {
            return result;
        }

        return await orchestrator.BuildAsync(options, cancellationToken);
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

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
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
    }

    private static void AssertSourceMapContainsSourceContent(
        JsonElement sourceMap,
        string expectedSourcePath,
        string expectedSourceContent)
    {
        var sourceIndex = FindSourceIndexContaining(sourceMap, expectedSourcePath);
        var sourcesContent = sourceMap.GetProperty("sourcesContent");
        Assert.IsTrue(
            sourceIndex < sourcesContent.GetArrayLength(),
            $"Expected a sourcesContent entry for '{expectedSourcePath}'.");
        Assert.AreEqual(expectedSourceContent, sourcesContent[sourceIndex].GetString());
    }

    private static JsonElement GetManifestChunk(JsonElement chunks, string chunkPath)
    {
        return chunks.EnumerateArray()
            .Single(chunk => string.Equals(chunk.GetProperty("File").GetString(), chunkPath, StringComparison.Ordinal));
    }

    private static string[] GetManifestStringArray(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).EnumerateArray()
            .Select(static item => item.GetString() ?? string.Empty)
            .ToArray();
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
