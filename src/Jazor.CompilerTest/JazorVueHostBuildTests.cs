using System.Text.Json;
using System.Text;
using Jazor.VueHost.Build;
using Jazor.VueHost.DevServer;

namespace Jazor.CompilerTest;

[TestClass]
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
            ChunkSizeWarningLimit = 100_000
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
            Build = new JazorBuildConfig
            {
                OutDir = "site",
                SourceMap = "inline",
                Minify = false
            }
        };

        var options = BuildCommandOptionsResolver.ResolveBuildOptions([], "/project", config);

        Assert.AreEqual("/project", options.RootDirectory);
        Assert.AreEqual("site", options.OutDir);
        Assert.AreEqual(SourceMapOption.Inline, options.SourceMap);
        Assert.IsFalse(options.Minify);
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
                Minify = false
            }
        };

        var options = BuildCommandOptionsResolver.ResolveBuildOptions(
            ["--out-dir=preview-dist", "--sourcemap=false", "--minify=true"],
            "/project",
            config);

        Assert.AreEqual("preview-dist", options.OutDir);
        Assert.AreEqual(SourceMapOption.None, options.SourceMap);
        Assert.IsTrue(options.Minify);
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
    public void EsbuildPackageResolver_ResolvePackageDirectory_ReturnsLocalNodeModulesPackage()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var packageDirectory = Path.Combine(tempDir, "node_modules", "esbuild");
            Directory.CreateDirectory(packageDirectory);
            File.WriteAllText(Path.Combine(packageDirectory, "package.json"), """{ "name": "esbuild", "main": "index.js" }""");

            var resolved = EsbuildPackageResolver.ResolvePackageDirectory(tempDir);

            Assert.AreEqual(Path.GetFullPath(packageDirectory), resolved);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task EsbuildRunner_RunAsync_UsesResolvedEntryPointAndLocalEsbuildPackage()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            Directory.CreateDirectory(Path.Combine(tempDir, "src"));
            File.WriteAllText(
                Path.Combine(tempDir, "index.html"),
                """
                <html>
                <body>
                  <script type="module" src="/src/custom-entry.ts"></script>
                </body>
                </html>
                """);
            File.WriteAllText(Path.Combine(tempDir, "src", "custom-entry.ts"), "console.log('custom entry');");

            var esbuildPackageDirectory = Path.Combine(tempDir, "node_modules", "esbuild");
            Directory.CreateDirectory(esbuildPackageDirectory);
            File.WriteAllText(Path.Combine(esbuildPackageDirectory, "package.json"), """{ "name": "esbuild", "main": "index.js" }""");
            File.WriteAllText(
                Path.Combine(esbuildPackageDirectory, "index.js"),
                """
                const path = require('node:path');

                module.exports.build = async function build(options) {
                  if (!Array.isArray(options.entryPoints) || options.entryPoints[0] !== 'src/custom-entry.ts') {
                    throw new Error(`Unexpected entry point: ${JSON.stringify(options.entryPoints)}`);
                  }

                  if (options.entryNames !== 'assets/[name]-[hash]') {
                    throw new Error(`Unexpected entryNames: ${options.entryNames}`);
                  }

                  if (!Array.isArray(options.plugins) || options.plugins.length !== 1 || options.plugins[0].name !== 'noop-plugin') {
                    throw new Error('Expected the generated config to load the provided plugin.');
                  }

                  return {
                    metafile: {
                      inputs: {
                        'src/custom-entry.ts': { bytes: 32 }
                      },
                      outputs: {
                        [path.posix.join(options.outdir, 'assets/custom-entry-abc123.js')]: {
                          bytes: 64,
                          inputs: {
                            'src/custom-entry.ts': { bytesInOutput: 64 }
                          },
                          imports: [],
                          exports: []
                        }
                      }
                    }
                  };
                };
                """);

            var pluginPath = Path.Combine(tempDir, ".jazor", "noop-plugin.mjs");
            Directory.CreateDirectory(Path.GetDirectoryName(pluginPath)!);
            await File.WriteAllTextAsync(
                pluginPath,
                """
                export default {
                  name: 'noop-plugin',
                  setup() {}
                };
                """);

            var options = new BuildOptions { RootDirectory = tempDir, OutDir = "dist" };
            using var context = new BuildContext(options);
            var runner = new EsbuildRunner(context);

            var result = await runner.RunAsync(pluginPath, CancellationToken.None);

            Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Errors.Select(static error => error.Message)));
            Assert.IsNotNull(result.MetafileJson);
            StringAssert.Contains(result.MetafileJson, "src/custom-entry.ts");
            StringAssert.Contains(result.MetafileJson, "dist/assets/custom-entry-abc123.js");
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
    public void EsbuildPluginGenerator_GeneratesValidPlugin()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var options = new BuildOptions { RootDirectory = tempDir };
            var context = new BuildContext(options);
            context.BuildServerPort = 12345;

            var generator = new EsbuildPluginGenerator(context);
            var pluginPath = generator.GenerateAsync().GetAwaiter().GetResult();

            Assert.IsTrue(File.Exists(pluginPath));
            Assert.IsTrue(pluginPath.EndsWith("build-plugin.mjs"));

            var content = File.ReadAllText(pluginPath);

            // Verify key parts of the generated plugin
            Assert.IsTrue(content.Contains("http://localhost:12345"), "Should contain build server URL");
            Assert.IsTrue(content.Contains(".jazor"), "Should intercept .jazor files");
            Assert.IsTrue(content.Contains(".vue"), "Should intercept .vue files");
            Assert.IsTrue(content.Contains("export default"), "Should export default");
            Assert.IsTrue(content.Contains("onLoad"), "Should use onLoad");
            Assert.IsTrue(content.Contains("/compile"), "Should call /compile endpoint");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void BuildContext_Initializes_Correctly()
    {
        var options = new BuildOptions { RootDirectory = "/tmp/test" };
        using var context = new BuildContext(options);

        Assert.AreEqual("/tmp/test", context.RootDirectory);
        Assert.IsTrue(context.OutDirectory.EndsWith("dist"), $"OutDirectory should end with 'dist', got: {context.OutDirectory}");
        Assert.IsNotNull(context.CompilationCache);
        Assert.IsNotNull(context.DependencyGraph);
        Assert.IsNotNull(context.Diagnostics);
        Assert.AreEqual(0, context.Diagnostics.Count);
    }

    [TestMethod]
    public void BuildDiagnostic_Severity_Values()
    {
        Assert.AreEqual(0, (int)DiagnosticSeverity.Error);
        Assert.AreEqual(1, (int)DiagnosticSeverity.Warning);
        Assert.AreEqual(2, (int)DiagnosticSeverity.Info);
    }

    [TestMethod]
    public void AssetProcessor_ProcessAsync_ParsesMetafile()
    {
        var tempDir = CreateTemporaryDirectory();
        try
        {
            var options = new BuildOptions { RootDirectory = tempDir };
            using var context = new BuildContext(options);
            var processor = new AssetProcessor(context);

            var metafileJson = """
                {
                    "inputs": {},
                    "outputs": {
                        "dist/assets/index-abc123.js": {
                            "bytes": 1024,
                            "inputs": { "src/main.js": { "bytesInOutput": 512 } },
                            "imports": [{ "path": "dist/assets/vendor-def456.js" }],
                            "exports": []
                        },
                        "dist/assets/vendor-def456.js": {
                            "bytes": 2048,
                            "inputs": { "node_modules/vue/dist/vue.runtime.esm-bundler.js": { "bytesInOutput": 2048 } },
                            "imports": [],
                            "exports": []
                        },
                        "dist/assets/index-abc123.css": {
                            "bytes": 256,
                            "inputs": {},
                            "imports": [],
                            "exports": []
                        }
                    }
                }
                """;

            var esbuildResult = new EsbuildResult
            {
                Success = true,
                MetafileJson = metafileJson
            };

            var assets = processor.ProcessAsync(esbuildResult, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(2, assets.Chunks.Count, "Should have 2 JS chunks");
            Assert.AreEqual(1, assets.CssAssets.Count, "Should have 1 CSS asset");
            Assert.AreEqual(1024 + 2048 + 256, assets.TotalSize, "Total size should sum all outputs");

            var entryChunk = assets.Chunks.FirstOrDefault(c => c.IsEntry);
            Assert.IsNotNull(entryChunk, "Should detect entry chunk from src/ inputs");
            Assert.AreEqual("index-abc123.js", entryChunk!.FileName);
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

            // Verify files were copied
            var distFiles = Directory.GetFiles(context.OutDirectory, "*", SearchOption.AllDirectories);
            Assert.AreEqual(2, distFiles.Length, "Should have 2 files in dist/");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}
