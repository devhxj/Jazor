using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Jazor.Vue;
using Jazor.VueHost.Build;
using Jazor.VueHost.DevServer;

namespace Jazor.CompilerTest;

[TestClass]
[DoNotParallelize]
public sealed class JazorVueHostBuildTests
{
    [TestMethod]
    public void BuildOptions_Defaults_AreCorrect()
    {
        var options = new BuildOptions { RootDirectory = "/tmp/project" };

        Assert.AreEqual("/tmp/project", options.RootDirectory);
        Assert.AreEqual("dist", options.OutDir);
        Assert.AreEqual(SourceMapOption.External, options.SourceMap);
        Assert.IsTrue(options.Minify);
        Assert.AreEqual("es2020", options.Target);
        Assert.IsTrue(options.CodeSplitting);
        Assert.AreEqual(500_000, options.ChunkSizeWarningLimit);
        Assert.AreEqual("assets", options.AssetsDir);
        Assert.AreEqual(8, options.AssetHashLength);
        Assert.AreEqual(0, options.ResolveAliases.Count);
        Assert.IsFalse(options.Incremental);
        Assert.IsTrue(options.GenerateSourceMap);
    }

    [TestMethod]
    public void BuildOptions_GenerateSourceMap_IsFalse_WhenNone()
    {
        var options = new BuildOptions
        {
            RootDirectory = "/tmp",
            SourceMap = SourceMapOption.None
        };

        Assert.IsFalse(options.GenerateSourceMap);
    }

    [TestMethod]
    public void BuildOptions_GenerateSourceMap_IsTrue_WhenInline()
    {
        var options = new BuildOptions
        {
            RootDirectory = "/tmp",
            SourceMap = SourceMapOption.Inline
        };

        Assert.IsTrue(options.GenerateSourceMap);
    }

    [TestMethod]
    public void JazorBuildConfig_ToBuildOptions_AppliesDefaults()
    {
        var config = new JazorBuildConfig();
        var options = config.ToBuildOptions("/project");

        Assert.AreEqual("/project", options.RootDirectory);
        Assert.AreEqual("dist", options.OutDir);
        Assert.AreEqual(SourceMapOption.External, options.SourceMap);
        Assert.IsTrue(options.Minify);
        Assert.AreEqual("es2020", options.Target);
        Assert.IsTrue(options.CodeSplitting);
        Assert.AreEqual("assets", options.AssetsDir);
        Assert.AreEqual(8, options.AssetHashLength);
        Assert.AreEqual(500_000, options.ChunkSizeWarningLimit);
        Assert.IsFalse(options.Incremental);
    }

    [TestMethod]
    public void JazorBuildConfig_ToBuildOptions_AppliesOverrides()
    {
        var config = new JazorBuildConfig
        {
            OutDir = "build",
            SourceMap = "inline",
            Minify = false,
            Target = "es2022",
            CodeSplitting = false,
            AssetsDir = "static",
            AssetHashLength = 12,
            ChunkSizeWarningLimit = 100_000,
            Incremental = true
        };

        var options = config.ToBuildOptions("/app");

        Assert.AreEqual("/app", options.RootDirectory);
        Assert.AreEqual("build", options.OutDir);
        Assert.AreEqual(SourceMapOption.Inline, options.SourceMap);
        Assert.IsFalse(options.Minify);
        Assert.AreEqual("es2022", options.Target);
        Assert.IsFalse(options.CodeSplitting);
        Assert.AreEqual("static", options.AssetsDir);
        Assert.AreEqual(12, options.AssetHashLength);
        Assert.AreEqual(100_000, options.ChunkSizeWarningLimit);
        Assert.IsTrue(options.Incremental);
    }

    [TestMethod]
    public void JazorBuildConfig_SourceMap_False_MapsToNone()
    {
        var config = new JazorBuildConfig { SourceMap = "false" };
        var options = config.ToBuildOptions("/tmp");
        Assert.AreEqual(SourceMapOption.None, options.SourceMap);
    }

    [TestMethod]
    public void JazorBuildConfig_SourceMap_Null_MapsToExternal()
    {
        var config = new JazorBuildConfig { SourceMap = null };
        var options = config.ToBuildOptions("/tmp");
        Assert.AreEqual(SourceMapOption.External, options.SourceMap);
    }

    [TestMethod]
    public void JazorBuildConfig_SourceMap_Unknown_MapsToExternal()
    {
        var config = new JazorBuildConfig { SourceMap = "yes" };
        var options = config.ToBuildOptions("/tmp");
        Assert.AreEqual(SourceMapOption.External, options.SourceMap);
    }

    [TestMethod]
    public void BuildCommandOptionsResolver_ResolveBuildOptions_UsesConfigDefaults()
    {
        var config = new JazorConfig
        {
            Resolve = new JazorResolveConfig
            {
                Alias = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["@"] = "/src"
                }
            },
            Build = new JazorBuildConfig
            {
                OutDir = "site",
                SourceMap = "inline",
                Minify = false,
                Incremental = true
            }
        };

        var options = BuildCommandOptionsResolver.ResolveBuildOptions([], "/project", config);

        Assert.AreEqual("/project", options.RootDirectory);
        Assert.AreEqual("site", options.OutDir);
        Assert.AreEqual(SourceMapOption.Inline, options.SourceMap);
        Assert.IsFalse(options.Minify);
        Assert.IsTrue(options.Incremental);
        Assert.AreEqual(1, options.ResolveAliases.Count);
        Assert.AreEqual("/src", options.ResolveAliases["@"]);
    }

    [TestMethod]
    public void BuildCommandOptionsResolver_ResolveBuildOptions_AllowsCliOverrides()
    {
        var config = new JazorConfig
        {
            Build = new JazorBuildConfig
            {
                OutDir = "site",
                SourceMap = "inline",
                Minify = false,
                Target = "es2020",
                CodeSplitting = true,
                AssetsDir = "assets",
                AssetHashLength = 8,
                ChunkSizeWarningLimit = 500_000,
                Incremental = false
            }
        };

        var options = BuildCommandOptionsResolver.ResolveBuildOptions(
            [
                "--out-dir=preview-dist",
                "--sourcemap=true",
                "--minify=true",
                "--target=es2022",
                "--code-splitting=false",
                "--assets-dir=static",
                "--asset-hash-length=12",
                "--chunk-size-warning-limit=100000",
                "--incremental=true"
            ],
            "/project",
            config);

        Assert.AreEqual("preview-dist", options.OutDir);
        Assert.AreEqual(SourceMapOption.External, options.SourceMap);
        Assert.IsTrue(options.Minify);
        Assert.AreEqual("es2022", options.Target);
        Assert.IsFalse(options.CodeSplitting);
        Assert.AreEqual("static", options.AssetsDir);
        Assert.AreEqual(12, options.AssetHashLength);
        Assert.AreEqual(100_000, options.ChunkSizeWarningLimit);
        Assert.IsTrue(options.Incremental);
    }

    [TestMethod]
    public void BuildCommandOptionsResolver_ResolveOutputDirectory_UsesResolvedOutDir()
    {
        var config = new JazorConfig
        {
            Build = new JazorBuildConfig
            {
                OutDir = "site"
            }
        };

        var outputDirectory = BuildCommandOptionsResolver.ResolveOutputDirectory(
            ["--out-dir=preview-dist"],
            @"C:\workspace\app",
            config);

        Assert.AreEqual(
            Path.GetFullPath(@"C:\workspace\app\preview-dist"),
            outputDirectory);
    }

    [TestMethod]
    public void JazorConfig_Deserialize_WithBuildConfig()
    {
        var json = """
            {
                "server": { "port": 3000 },
                "build": {
                    "outDir": "out",
                    "minify": false,
                    "target": "esnext"
                },
                "resolve": {
                    "alias": {
                        "@": "/src",
                        "@shared": "./shared"
                    }
                }
            }
            """;

        var config = JsonSerializer.Deserialize<JazorConfig>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(config);
        Assert.IsNotNull(config.Server);
        Assert.AreEqual(3000, config.Server.Port);
        Assert.IsNotNull(config.Build);
        Assert.AreEqual("out", config.Build.OutDir);
        Assert.IsFalse(config.Build.Minify);
        Assert.AreEqual("esnext", config.Build.Target);
        Assert.IsNotNull(config.Resolve);
        Assert.IsNotNull(config.Resolve.Alias);
        Assert.AreEqual("/src", config.Resolve.Alias["@"]);
        Assert.AreEqual("./shared", config.Resolve.Alias["@shared"]);
    }

    [TestMethod]
    public void JazorConfig_Deserialize_WithoutBuildConfig()
    {
        var json = """{ "server": { "port": 5173 } }""";

        var config = JsonSerializer.Deserialize<JazorConfig>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.IsNotNull(config);
        Assert.IsNull(config.Build);
    }

    [TestMethod]
    public void BuildEntryPointResolver_ResolveEntryPoint_UsesIndexHtmlModuleScript()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            var entryPoint = Path.Combine(tempDir, "src", "custom-entry.ts");
            File.WriteAllText(entryPoint, "console.log('hello');");
            File.WriteAllText(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/src/custom-entry.ts"></script>
                </body>
                </html>
                """);

            var resolved = BuildEntryPointResolver.ResolveEntryPoint(tempDir);

            Assert.AreEqual(Path.GetFullPath(entryPoint), resolved);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildEntryPointResolver_ResolveEntryPoint_PrefersModuleScriptAndIgnoresQueryString()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            var analyticsScript = Path.Combine(tempDir, "analytics.js");
            var entryPoint = Path.Combine(tempDir, "src", "main.ts");
            File.WriteAllText(analyticsScript, "console.log('analytics');");
            File.WriteAllText(entryPoint, "console.log('main');");
            File.WriteAllText(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script src="/analytics.js"></script>
                  <script type="module" src="/src/main.ts?v=42"></script>
                </body>
                </html>
                """);

            var resolved = BuildEntryPointResolver.ResolveEntryPoint(tempDir);

            Assert.AreEqual(Path.GetFullPath(entryPoint), resolved);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildEntryPointResolver_ResolveEntryPoint_FallsBackToStandardCandidates()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            var entryPoint = Path.Combine(tempDir, "src", "main.ts");
            File.WriteAllText(entryPoint, "console.log('hello');");

            var resolved = BuildEntryPointResolver.ResolveEntryPoint(tempDir);

            Assert.AreEqual(Path.GetFullPath(entryPoint), resolved);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task DenoBuildImportMapGenerator_GenerateAsync_IncludesVueFallback()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var importMapPath = await DenoBuildImportMapGenerator.GenerateAsync(tempDir, CancellationToken.None);

            Assert.IsTrue(File.Exists(importMapPath));
            var json = await File.ReadAllTextAsync(importMapPath);
            StringAssert.Contains(json, "\"vue\": \"npm:vue@3\"");
            StringAssert.Contains(json, "\"vue/\": \"npm:vue@3/\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task DenoBundleRunner_RunAsync_BundlesHttpEntryUsingBundledRuntime()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/src/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "main.js"),
                """
                import { message } from "./message.js";
                console.log(message);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "message.js"),
                """export const message = "hello from deno bundle";""");

            var port = GetAvailablePort();
            var devOptions = new DevServerOptions
            {
                RootDirectory = tempDir,
                Host = "127.0.0.1",
                Port = port,
                HmrEnabled = false,
                FrontendCompiler = "stub"
            };

            var moduleResolver = new ModuleResolver(tempDir);
            var compiler = new OnDemandCompiler(
                new JazorVueParser(),
                new JazorVueCompiler(),
                frontendCompiler: null,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);

            await using var server = new DevHttpServer(
                devOptions,
                compiler,
                moduleResolver,
                new HtmlTransformer(devOptions));
            await server.StartAsync(CancellationToken.None);

            var buildOptions = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = false
            };
            using var context = new BuildContext(buildOptions);
            var runner = new DenoBundleRunner(context);

            var entryPointPath = BuildEntryPointResolver.ResolveEntryPoint(tempDir);
            var entryRequestPath = "/" + Path.GetRelativePath(tempDir, entryPointPath).Replace('\\', '/');
            var entryUri = new Uri(server.ListeningUri!, entryRequestPath);

            var result = await runner.RunAsync(entryUri, CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.AreEqual(1, result.Chunks.Count);
            StringAssert.StartsWith(result.Chunks[0].FileName, "index-");
            StringAssert.EndsWith(result.Chunks[0].FileName, ".js");

            var chunkPath = Path.Combine(tempDir, result.Chunks[0].FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(chunkPath));
            var chunkContent = await File.ReadAllTextAsync(chunkPath);
            StringAssert.Contains(chunkContent, "hello from deno bundle");

            Assert.IsNotNull(result.Chunks[0].SourceMapPath);
            var sourceMapPath = Path.Combine(tempDir, result.Chunks[0].SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task DenoBundleRunner_RunAsync_CodeSplitting_RewritesChunkImportsAndSourceMaps()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "main.js"),
                """
                console.log("main");
                await import("./feature.js");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "feature.js"),
                """
                export const featureMessage = "hello from split chunk";
                console.log(featureMessage);
                """);

            var port = GetAvailablePort();
            var devOptions = new DevServerOptions
            {
                RootDirectory = tempDir,
                Host = "127.0.0.1",
                Port = port,
                HmrEnabled = false,
                FrontendCompiler = "stub"
            };

            var moduleResolver = new ModuleResolver(tempDir);
            var compiler = new OnDemandCompiler(
                new JazorVueParser(),
                new JazorVueCompiler(),
                frontendCompiler: null,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);

            await using var server = new DevHttpServer(
                devOptions,
                compiler,
                moduleResolver,
                new HtmlTransformer(devOptions));
            await server.StartAsync(CancellationToken.None);

            var buildOptions = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = true
            };
            using var context = new BuildContext(buildOptions);
            var runner = new DenoBundleRunner(context);

            var entryUri = new Uri(server.ListeningUri!, "/src/main.js");
            var result = await runner.RunAsync(entryUri, CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(result.Chunks.Count >= 2, "Expected code splitting to emit multiple chunks.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var splitChunk = result.Chunks.Single(static chunk => !chunk.IsEntry);

            StringAssert.StartsWith(entryChunk.FileName, "main-");
            StringAssert.StartsWith(splitChunk.FileName, "feature-");
            CollectionAssert.Contains(entryChunk.Imports.ToArray(), splitChunk.FilePath);

            var entryChunkPath = Path.Combine(tempDir, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(entryChunkPath));
            var entryChunkContent = await File.ReadAllTextAsync(entryChunkPath);
            StringAssert.Contains(entryChunkContent, $"./{splitChunk.FileName}");

            Assert.IsNotNull(entryChunk.SourceMapPath);
            Assert.IsNotNull(splitChunk.SourceMapPath);

            var entrySourceMapPath = Path.Combine(tempDir, entryChunk.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            var splitSourceMapPath = Path.Combine(tempDir, splitChunk.SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(entrySourceMapPath));
            Assert.IsTrue(File.Exists(splitSourceMapPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_GeneratesBundleHtmlAndStaticAssets()
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
                  <title>VueHost</title>
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
                """
                import { message } from "./message.js";
                console.log(message);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "message.js"),
                """export const message = "hello from orchestrator";""");
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
            Assert.IsNotNull(result.OutDirectory);
            Assert.AreEqual(Path.Combine(tempDir, "dist"), result.OutDirectory);
            Assert.AreEqual(1, result.Chunks.Count);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            Assert.IsTrue(File.Exists(distIndexHtmlPath));

            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            var entryChunkHtmlPath = GetHtmlRelativePath(tempDir, "dist", result.Chunks[0].FilePath);
            var faviconHtmlPath = GetHtmlRelativePath(tempDir, "dist", result.StaticAssets[0].FilePath);
            Assert.IsFalse(html.Contains("src=\"/main.js\""), "Original entry script should be removed from production HTML");
            StringAssert.Contains(html, $"src=\"{entryChunkHtmlPath}\"");
            Assert.IsFalse(html.Contains("href=\"/favicon.svg\"", StringComparison.Ordinal), "Original favicon path should be rewritten");
            StringAssert.Contains(html, $"href=\"{faviconHtmlPath}\"");

            var chunkPath = Path.Combine(tempDir, result.Chunks[0].FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(chunkPath));
            var chunkContent = await File.ReadAllTextAsync(chunkPath);
            StringAssert.Contains(chunkContent, "hello from orchestrator");

            Assert.IsNotNull(result.Chunks[0].SourceMapPath);
            var sourceMapPath = Path.Combine(tempDir, result.Chunks[0].SourceMapPath!.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourceMapPath));

            Assert.AreEqual(1, result.StaticAssets.Count);
            StringAssert.Contains(result.StaticAssets[0].FileName, "favicon");
            var faviconPath = Path.Combine(tempDir, result.StaticAssets[0].FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(faviconPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithLegacyImport_FailsWithUnsupportedDirectiveError()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/src/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "main.js"),
                """
                import "./Counter.jazor";
                console.log("legacy-import-build");
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "helper.js"),
                """export default 1;""");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "src", "Counter.jazor"),
                """
                @jsimport helper from "./helper.js"

                <template>
                  <div>{{ helper }}</div>
                </template>
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

            Assert.IsFalse(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var legacyDiagnostic = result.Diagnostics.SingleOrDefault(static diagnostic =>
                diagnostic.Severity == DiagnosticSeverity.Error
                && diagnostic.Message.Contains("@jsimport is unsupported. Use @module instead.", StringComparison.Ordinal));
            Assert.IsNotNull(legacyDiagnostic);
            Assert.IsTrue(string.Equals(
                legacyDiagnostic.File,
                Path.Combine(tempDir, "src", "Counter.jazor"),
                StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(legacyDiagnostic.Location.HasValue);
            Assert.AreEqual(1, legacyDiagnostic.Location!.Value.Line);
            Assert.AreEqual(1, legacyDiagnostic.Location!.Value.Column);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_RewritesMetaAndSrcSetAssetReferences()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "images"));
            Directory.CreateDirectory(Path.Combine(tempDir, "public", "social"));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head>
                  <title>VueHost social</title>
                  <link rel="icon" href="/favicon.svg?v=1">
                  <meta property="og:image" content="/social/card.png#preview">
                  <meta name="twitter:image" content="./images/logo.png?v=2">
                </head>
                <body>
                  <div id="app"></div>
                  <img srcset="/images/logo.png 1x, ./images/logo@2x.png?variant=wide 2x">
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """console.log("build html rewrite");""");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "favicon.svg"),
                "<svg></svg>");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "images", "logo.png"),
                "fake-png-data");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "images", "logo@2x.png"),
                "fake-png-data-2x");
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "public", "social", "card.png"),
                "fake-card-data");

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

            var faviconAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/favicon.svg");
            var logoAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/images/logo.png");
            var retinaAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/images/logo@2x.png");
            var cardAsset = result.StaticAssets.Single(static asset => asset.OriginalPath == "/social/card.png");

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            Assert.IsTrue(File.Exists(distIndexHtmlPath));

            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            var faviconHtmlPath = GetHtmlRelativePath(tempDir, "dist", faviconAsset.FilePath);
            var logoHtmlPath = GetHtmlRelativePath(tempDir, "dist", logoAsset.FilePath);
            var retinaHtmlPath = GetHtmlRelativePath(tempDir, "dist", retinaAsset.FilePath);
            var cardHtmlPath = GetHtmlRelativePath(tempDir, "dist", cardAsset.FilePath);

            StringAssert.Contains(html, $"href=\"{faviconHtmlPath}?v=1\"");
            StringAssert.Contains(html, $"content=\"{cardHtmlPath}#preview\"");
            StringAssert.Contains(html, $"content=\"{logoHtmlPath}?v=2\"");
            StringAssert.Contains(html, $"srcset=\"{logoHtmlPath} 1x, {retinaHtmlPath}?variant=wide 2x\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithoutResolvableEntryPoint_ReturnsFailureDiagnostic()
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
                </body>
                </html>
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

            Assert.IsFalse(result.Success);
            Assert.IsTrue(
                result.Diagnostics.Any(static diagnostic =>
                    diagnostic.Message.Contains("Unable to locate a frontend entry point", StringComparison.OrdinalIgnoreCase)),
                string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithOutputDirectoryEscapingRoot_ReturnsFailureDiagnostic()
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
                """console.log("build escape root");""");

            var orchestrator = new BuildOrchestrator();
            var result = await orchestrator.BuildAsync(
                new BuildOptions
                {
                    RootDirectory = tempDir,
                    OutDir = "..\\dist-outside-root",
                    SourceMap = SourceMapOption.External,
                    Minify = false,
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.IsTrue(
                result.Diagnostics.Any(static diagnostic =>
                    diagnostic.Message.Contains("must stay inside project root", StringComparison.OrdinalIgnoreCase)),
                string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplitting_InjectsEntryChunkIntoHtml()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <head><title>VueHost split</title></head>
                <body>
                  <div id="app"></div>
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
                export const message = "hello from build split";
                console.log(message);
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
            Assert.IsTrue(result.Chunks.Count >= 2, "Expected production build to emit multiple chunks.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var splitChunk = result.Chunks.Single(static chunk => !chunk.IsEntry);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            Assert.IsTrue(File.Exists(distIndexHtmlPath));

            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            var entryChunkHtmlPath = GetHtmlRelativePath(tempDir, "dist", entryChunk.FilePath);
            Assert.IsFalse(html.Contains("src=\"/main.js\""), "Original entry script should be removed from production HTML");
            StringAssert.Contains(html, $"src=\"{entryChunkHtmlPath}\"");

            var entryChunkPath = Path.Combine(tempDir, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(entryChunkPath));
            var entryChunkContent = await File.ReadAllTextAsync(entryChunkPath);
            StringAssert.Contains(entryChunkContent, $"./{splitChunk.FileName}");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCodeSplittingAndSourceMapNone_PreservesCssOwnership_WithoutEmittingSourceMaps()
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
                    SourceMap = SourceMapOption.None,
                    Minify = false,
                    CodeSplitting = true
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.IsTrue(result.Chunks.Count >= 3, "Expected code splitting to produce an entry chunk and two lazy chunks.");
            Assert.IsTrue(result.CssAssets.Count >= 3, "Expected extracted CSS assets for the entry and both lazy chunks.");
            Assert.IsTrue(result.Chunks.All(static chunk => chunk.SourceMapPath is null));
            Assert.IsTrue(result.CssAssets.All(static asset => asset.SourceMapPath is null));

            var mapFiles = Directory.GetFiles(Path.Combine(tempDir, "dist"), "*.map", SearchOption.AllDirectories);
            Assert.AreEqual(0, mapFiles.Length, "SourceMapOption.None should not emit any .map files.");

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

            Assert.IsFalse(entryCss.Content.Contains("sourceMappingURL=", StringComparison.Ordinal));
            Assert.IsFalse(lazyACss.Content.Contains("sourceMappingURL=", StringComparison.Ordinal));
            Assert.IsFalse(lazyBCss.Content.Contains("sourceMappingURL=", StringComparison.Ordinal));

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

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", lazyACss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject lazy-a CSS.");
            Assert.IsFalse(
                html.Contains($"href=\"{GetHtmlRelativePath(tempDir, "dist", lazyBCss.Asset.FilePath)}\"", StringComparison.Ordinal),
                "Production HTML should not eagerly inject lazy-b CSS.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithCrLfJavaScriptCssImports_StripsCssImportsForBundler()
    {
        const string entryMarker = "entry-crlf-style-marker";

        var tempDir = CreateTemporaryDirectory();
        try
        {
            const string crlf = "\r\n";
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
                string.Concat(
                    "import \"./entry.css\";", crlf,
                    "console.log(\"entry-crlf-payload\");", crlf));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "entry.css"),
                $$"""
                .{{entryMarker}} {
                  color: tomato;
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
                    CodeSplitting = false
                },
                CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            Assert.AreEqual(1, result.Chunks.Count, "Expected a single entry chunk when code splitting is disabled.");
            Assert.IsTrue(result.CssAssets.Count >= 1, "Expected extracted CSS assets from CRLF-authored JS imports.");

            var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
            var entryChunkPath = Path.Combine(tempDir, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var entryChunkContent = await File.ReadAllTextAsync(entryChunkPath);
            Assert.IsFalse(
                entryChunkContent.Contains("./entry.css", StringComparison.Ordinal),
                "Build-mode JS preprocessing should strip static CSS imports before bundling.");
            StringAssert.Contains(entryChunkContent, "entry-crlf-payload");

            var cssOutputs = await Task.WhenAll(result.CssAssets.Select(async asset => new
            {
                Asset = asset,
                Content = await File.ReadAllTextAsync(Path.Combine(tempDir, asset.FilePath.Replace('/', Path.DirectorySeparatorChar)))
            }));
            var entryCss = cssOutputs.Single(output => output.Content.Contains(entryMarker, StringComparison.Ordinal));
            Assert.AreEqual(entryChunk.FilePath, entryCss.Asset.OwnerChunkFilePath);

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var html = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(html, $"href=\"{GetHtmlRelativePath(tempDir, "dist", entryCss.Asset.FilePath)}\"");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithIncrementalEnabled_ReusesPreviousOutputsWhenInputsUnchanged()
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
                """console.log("incremental-cache-hit");""");

            var orchestrator = new BuildOrchestrator();
            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = false,
                Incremental = true
            };

            var first = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(first.Success, string.Join(Environment.NewLine, first.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var entryChunk = first.Chunks.Single(static chunk => chunk.IsEntry);
            var entryChunkPath = Path.Combine(tempDir, entryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var firstChunkWriteTime = File.GetLastWriteTimeUtc(entryChunkPath);

            await Task.Delay(TimeSpan.FromMilliseconds(1200));

            var second = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(second.Success, string.Join(Environment.NewLine, second.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            CollectionAssert.Contains(
                second.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                "Incremental build cache hit.");
            Assert.AreEqual(entryChunk.FilePath, second.Chunks.Single(static chunk => chunk.IsEntry).FilePath);
            Assert.AreEqual(firstChunkWriteTime, File.GetLastWriteTimeUtc(entryChunkPath));
            Assert.IsTrue(File.Exists(Path.Combine(tempDir, "dist", "jazor-build-state.json")));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithIncrementalEnabled_RebuildsWhenInputsChange()
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
                """console.log("incremental-before");""");

            var orchestrator = new BuildOrchestrator();
            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = false,
                Incremental = true
            };

            var first = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(first.Success, string.Join(Environment.NewLine, first.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var firstEntryChunk = first.Chunks.Single(static chunk => chunk.IsEntry);

            await Task.Delay(TimeSpan.FromMilliseconds(1200));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """console.log("incremental-after-change");""");

            var second = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(second.Success, string.Join(Environment.NewLine, second.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            CollectionAssert.DoesNotContain(
                second.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                "Incremental build cache hit.");

            var secondEntryChunk = second.Chunks.Single(static chunk => chunk.IsEntry);
            Assert.AreNotEqual(firstEntryChunk.FilePath, secondEntryChunk.FilePath);
            var secondChunkPath = Path.Combine(tempDir, secondEntryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var secondChunkContent = await File.ReadAllTextAsync(secondChunkPath);
            StringAssert.Contains(secondChunkContent, "incremental-after-change");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_WithIncrementalEnabled_RefreshesHtmlWithoutRebundling_WhenOnlyIndexChanges()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <div id="version">v1</div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "main.js"),
                """console.log("incremental-html-refresh");""");

            var orchestrator = new BuildOrchestrator();
            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist",
                SourceMap = SourceMapOption.External,
                Minify = false,
                CodeSplitting = false,
                Incremental = true
            };

            var first = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(first.Success, string.Join(Environment.NewLine, first.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            var firstEntryChunk = first.Chunks.Single(static chunk => chunk.IsEntry);
            var firstEntryChunkPath = Path.Combine(tempDir, firstEntryChunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
            var firstChunkWriteTime = File.GetLastWriteTimeUtc(firstEntryChunkPath);

            await Task.Delay(TimeSpan.FromMilliseconds(1200));
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <div id="version">v2</div>
                  <script type="module" src="/main.js"></script>
                </body>
                </html>
                """);

            var second = await orchestrator.BuildAsync(options, CancellationToken.None);
            Assert.IsTrue(second.Success, string.Join(Environment.NewLine, second.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            CollectionAssert.Contains(
                second.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                "Incremental build html refresh.");
            CollectionAssert.DoesNotContain(
                second.Diagnostics.Select(static diagnostic => diagnostic.Message).ToArray(),
                "Incremental build cache hit.");

            var secondEntryChunk = second.Chunks.Single(static chunk => chunk.IsEntry);
            Assert.AreEqual(firstEntryChunk.FilePath, secondEntryChunk.FilePath);
            Assert.AreEqual(firstChunkWriteTime, File.GetLastWriteTimeUtc(firstEntryChunkPath));

            var distIndexHtmlPath = Path.Combine(tempDir, "dist", "index.html");
            var refreshedHtml = await File.ReadAllTextAsync(distIndexHtmlPath);
            StringAssert.Contains(refreshedHtml, "id=\"version\">v2");
            Assert.IsFalse(
                refreshedHtml.Contains("id=\"version\">v1", StringComparison.Ordinal),
                "Expected refreshed html to replace stale markup.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void HtmlTransformer_InjectScript_BeforeBodyClose()
    {
        var html = "<html><head></head><body><div>app</div></body></html>";
        var result = HtmlTransformer.InjectScript(html, "/assets/index-abc123.js");

        Assert.IsTrue(result.Contains("<script type=\"module\" src=\"/assets/index-abc123.js\"></script>"));
        Assert.IsTrue(result.Contains("</body>"));
        var scriptIndex = result.IndexOf("<script", StringComparison.Ordinal);
        var bodyCloseIndex = result.IndexOf("</body>", StringComparison.Ordinal);
        Assert.IsTrue(scriptIndex < bodyCloseIndex, "Script should be injected before </body>");
    }

    [TestMethod]
    public void HtmlTransformer_InjectCss_BeforeHeadClose()
    {
        var html = "<html><head><title>App</title></head><body></body></html>";
        var result = HtmlTransformer.InjectCss(html, "/assets/index-abc123.css");

        Assert.IsTrue(result.Contains("<link rel=\"stylesheet\" href=\"/assets/index-abc123.css\">"));
        var linkIndex = result.IndexOf("<link", StringComparison.Ordinal);
        var headCloseIndex = result.IndexOf("</head>", StringComparison.Ordinal);
        Assert.IsTrue(linkIndex < headCloseIndex, "Link should be injected before </head>");
    }

    [TestMethod]
    public void HtmlTransformer_RemoveDevScriptRefs_RemovesSrcPaths()
    {
        var html = """
            <html>
            <head></head>
            <body>
                <script src="/src/main.js"></script>
                <script src="/src/App.jazor"></script>
                <div id="app"></div>
            </body>
            </html>
            """;

        var result = HtmlTransformer.RemoveDevScriptRefs(html);

        Assert.IsFalse(result.Contains("src=\"/src/main.js\""), "Dev script /src/main.js should be removed");
        Assert.IsFalse(result.Contains("src=\"/src/App.jazor\""), "Dev script /src/App.jazor should be removed");
        Assert.IsTrue(result.Contains("id=\"app\""), "Non-script content should be preserved");
    }

    [TestMethod]
    public void HtmlTransformer_RemoveDevScriptRefs_PreservesExternalScripts()
    {
        var html = """
            <html>
            <body>
                <script src="https://cdn.example.com/lib.js"></script>
                <script src="/src/main.js"></script>
            </body>
            </html>
            """;

        var result = HtmlTransformer.RemoveDevScriptRefs(html);

        Assert.IsTrue(result.Contains("https://cdn.example.com/lib.js"), "External scripts should be preserved");
        Assert.IsFalse(result.Contains("/src/main.js"), "Dev scripts should be removed");
    }

    [TestMethod]
    public void HtmlTransformer_RemoveScriptReference_RemovesMatchedEntryScript()
    {
        var html = """
            <html>
            <body>
                <script type="module" src="./main.js?v=1"></script>
                <script src="https://cdn.example.com/lib.js"></script>
            </body>
            </html>
            """;

        var result = HtmlTransformer.RemoveScriptReference(html, "/main.js");

        Assert.IsFalse(result.Contains("./main.js?v=1"), "Matched entry script should be removed");
        Assert.IsTrue(result.Contains("https://cdn.example.com/lib.js"), "External scripts should be preserved");
    }

    [TestMethod]
    public void BuildContext_Initializes_Correctly()
    {
        var options = new BuildOptions { RootDirectory = "/tmp/test" };
        using var context = new BuildContext(options);

        Assert.AreEqual("/tmp/test", context.RootDirectory);
        Assert.IsTrue(context.OutDirectory.EndsWith("dist"), $"OutDirectory should end with 'dist', got: {context.OutDirectory}");
        Assert.IsTrue(context.AssetsDirectory.EndsWith(Path.Combine("dist", "assets")), $"AssetsDirectory should end with 'dist{Path.DirectorySeparatorChar}assets', got: {context.AssetsDirectory}");
        Assert.IsNotNull(context.Diagnostics);
        Assert.AreEqual(0, context.Diagnostics.Count);
    }

    [TestMethod]
    public void BuildContext_Rejects_AssetsDirOutsideOutputDirectory()
    {
        var options = new BuildOptions
        {
            RootDirectory = "/tmp/test",
            AssetsDir = "../escape"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => new BuildContext(options));
        StringAssert.Contains(exception.Message, "assets directory");
    }

    [TestMethod]
    public void BuildContext_AddsWarning_WhenTargetIsIgnoredByBundledDeno()
    {
        var options = new BuildOptions
        {
            RootDirectory = "/tmp/test",
            Target = "es2022"
        };

        using var context = new BuildContext(options);

        Assert.AreEqual(1, context.Diagnostics.Count);
        Assert.AreEqual(DiagnosticSeverity.Warning, context.Diagnostics[0].Severity);
        StringAssert.Contains(context.Diagnostics[0].Message, "ignores target 'es2022'");
    }

    [TestMethod]
    public void BuildDiagnostic_Severity_Values()
    {
        CollectionAssert.AreEqual(
            new[] { DiagnosticSeverity.Error, DiagnosticSeverity.Warning, DiagnosticSeverity.Info },
            Enum.GetValues<DiagnosticSeverity>());
    }

    [TestMethod]
    public async Task DenoBuildImportMapGenerator_GenerateAsync_IncludesPackageDependencies()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "package.json"),
                """
                {
                  "dependencies": {
                    "pinia": "^2.3.1",
                    "local-lib": "file:../local-lib"
                  },
                  "devDependencies": {
                    "vue-router": "~4.5.1"
                  }
                }
                """);

            var importMapPath = await DenoBuildImportMapGenerator.GenerateAsync(tempDir, CancellationToken.None);
            var json = await File.ReadAllTextAsync(importMapPath);

            StringAssert.Contains(json, "\"pinia\": \"npm:pinia@^2.3.1\"");
            StringAssert.Contains(json, "\"pinia/\": \"npm:pinia@^2.3.1/\"");
            StringAssert.Contains(json, "\"vue-router\": \"npm:vue-router@~4.5.1\"");
            Assert.IsFalse(json.Contains("local-lib", StringComparison.Ordinal), "file: dependencies should be excluded from the Deno import map");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void StaticAssetHandler_CopiesFiles_WithHash()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var publicDir = Path.Combine(tempDir, "public");
            Directory.CreateDirectory(publicDir);
            Directory.CreateDirectory(Path.Combine(publicDir, "images"));

            // Create test files
            File.WriteAllText(Path.Combine(publicDir, "favicon.svg"), "<svg></svg>");
            File.WriteAllText(Path.Combine(publicDir, "images", "logo.png"), "fake-png-data");

            var options = new BuildOptions
            {
                RootDirectory = tempDir,
                OutDir = "dist"
            };
            using var context = new BuildContext(options);
            var handler = new StaticAssetHandler(context);

            var assets = handler.CopyPublicAssetsAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, assets.Count, "Should copy 2 files");
            CollectionAssert.AreEquivalent(
                new[] { "/favicon.svg", "/images/logo.png" },
                assets.Select(static asset => asset.OriginalPath).ToArray());

            // Verify files were copied
            var distFiles = Directory.GetFiles(context.OutDirectory, "*", SearchOption.AllDirectories);
            Assert.AreEqual(2, distFiles.Length, "Should have 2 files in dist/");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void HtmlTransformer_RewriteAssetReferences_RewritesPublicAssetUrlsAcrossAttributes()
    {
        var html = """
            <html>
            <head>
              <link rel="icon" href="/favicon.svg?v=1">
              <meta property="og:image" content="/social/card.png#preview">
              <meta name="twitter:image" content="./images/logo.png?v=2">
              <meta name="description" content="/images/logo.png">
            </head>
            <body>
              <img srcset="/images/logo.png 1x, ./images/logo@2x.png?variant=wide 2x, https://cdn.example.com/logo.png 3x">
              <img src="./images/logo.png#hero">
              <img src="https://cdn.example.com/logo.png">
            </body>
            </html>
            """;

        var result = HtmlTransformer.RewriteAssetReferences(
            html,
            [
                new AssetInfo
                {
                    FileName = "favicon-1234.svg",
                    FilePath = "dist-assets/favicon-1234.svg",
                    OriginalPath = "/favicon.svg",
                    Size = 128
                },
                new AssetInfo
                {
                    FileName = "logo-5678.png",
                    FilePath = "dist-assets/images/logo-5678.png",
                    OriginalPath = "/images/logo.png",
                    Size = 256
                },
                new AssetInfo
                {
                    FileName = "logo@2x-9abc.png",
                    FilePath = "dist-assets/images/logo@2x-9abc.png",
                    OriginalPath = "/images/logo@2x.png",
                    Size = 512
                },
                new AssetInfo
                {
                    FileName = "card-9012.png",
                    FilePath = "dist-assets/social/card-9012.png",
                    OriginalPath = "/social/card.png",
                    Size = 768
                }
            ]);

        StringAssert.Contains(result, "href=\"dist-assets/favicon-1234.svg?v=1\"");
        StringAssert.Contains(result, "content=\"dist-assets/social/card-9012.png#preview\"");
        StringAssert.Contains(result, "content=\"dist-assets/images/logo-5678.png?v=2\"");
        StringAssert.Contains(result, "srcset=\"dist-assets/images/logo-5678.png 1x, dist-assets/images/logo@2x-9abc.png?variant=wide 2x, https://cdn.example.com/logo.png 3x\"");
        StringAssert.Contains(result, "src=\"dist-assets/images/logo-5678.png#hero\"");
        StringAssert.Contains(result, "src=\"https://cdn.example.com/logo.png\"");
        StringAssert.Contains(result, "name=\"description\" content=\"/images/logo.png\"");
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string GetHtmlRelativePath(string rootDirectory, string outDirName, string rootRelativePath)
    {
        var outDirectory = Path.Combine(rootDirectory, outDirName);
        var absolutePath = Path.Combine(rootDirectory, rootRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetRelativePath(outDirectory, absolutePath).Replace('\\', '/');
    }
}
