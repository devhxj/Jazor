using System.Text.RegularExpressions;
using Jolt.Build;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltBuildOptimizationTests
{
    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ForMinifiedVueEntry_AppliesProductionOptimizations()
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
                /* minify-js-comment */
                import App from "./App.vue";
                import { usedTreeShakingValue, formatMessage } from "./feature.js";
                console.log( App, formatMessage( usedTreeShakingValue ) );
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "feature.js"),
                """
                export const usedTreeShakingValue = "used-tree-shaking-marker";
                export const unusedTreeShakingValue = "unused-tree-shaking-marker";
                export function formatMessage( value ) {
                  const suffix = "!";
                  return value + suffix;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "App.vue"),
                """
                <template>
                  <div class="app">hello</div>
                </template>
                <script>
                export default {
                  name: "App"
                };
                </script>
                <style scoped>
                .app {
                  color: red;
                }
                </style>
                """);

            var result = await BuildAsync(tempDir, outDir: "dist-prod-optimized", minify: true);

            Assert.IsTrue(result.Success, FormatDiagnostics(result));

            var entryChunkContent = await ReadEntryChunkAsync(tempDir, result);
            Assert.IsFalse(entryChunkContent.Contains("import.meta.hot", StringComparison.Ordinal));
            Assert.IsFalse(entryChunkContent.Contains("__jazorHmrId", StringComparison.Ordinal));
            Assert.IsFalse(entryChunkContent.Contains("__JAZOR_HMR__", StringComparison.Ordinal));
            Assert.IsTrue(entryChunkContent.Contains("__scopeId", StringComparison.Ordinal));
            Assert.IsTrue(entryChunkContent.Contains("used-tree-shaking-marker", StringComparison.Ordinal));
            Assert.IsFalse(entryChunkContent.Contains("unused-tree-shaking-marker", StringComparison.Ordinal));
            Assert.IsFalse(entryChunkContent.Contains("minify-js-comment", StringComparison.Ordinal), "Expected minification to strip comments.");
            Assert.IsFalse(entryChunkContent.Contains("function formatMessage( value )", StringComparison.Ordinal), "Expected minification to collapse author formatting.");
            Assert.IsFalse(entryChunkContent.Contains("console.log( App, formatMessage( usedTreeShakingValue ) );", StringComparison.Ordinal), "Expected minification to collapse author call-site formatting.");
            Assert.IsFalse(entryChunkContent.Contains("  ", StringComparison.Ordinal), "Expected minification to remove repeated author whitespace.");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public async Task BuildOrchestrator_BuildAsync_ForVueAndStandaloneCssModules_EmitsScopedCssModuleMappings()
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
                import App from "./App.vue";
                import standaloneStyles from "./standalone.module.css";
                console.log(App, standaloneStyles.panel);
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "App.vue"),
                """
                <template>
                  <div :class="$style.hero">
                    <span :class="styles.accent">hello</span>
                  </div>
                </template>
                <script setup>
                import styles from "./app.module.css";
                </script>
                <style module>
                .hero {
                  color: red;
                }
                </style>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "app.module.css"),
                """
                .accent {
                  color: blue;
                }
                """);
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, "standalone.module.css"),
                """
                .panel {
                  color: green;
                }
                """);

            var result = await BuildAsync(tempDir, outDir: "dist-combined-css-modules", minify: false);

            Assert.IsTrue(result.Success, FormatDiagnostics(result));
            Assert.AreEqual(1, result.CssAssets.Count, "Expected a single extracted CSS asset for the combined CSS module scenario.");

            var entryChunkContent = await ReadEntryChunkAsync(tempDir, result);
            Assert.IsFalse(entryChunkContent.Contains("./app.module.css", StringComparison.Ordinal));
            Assert.IsFalse(entryChunkContent.Contains("./standalone.module.css", StringComparison.Ordinal));
            StringAssert.Contains(entryChunkContent, "__cssModules");

            var cssAssetContent = await ReadFileAsync(tempDir, result.CssAssets[0].FilePath);
            var localClassMatch = Regex.Match(
                cssAssetContent,
                @"\.((?<class>jz_App_hero_[A-Za-z0-9_]+))\b",
                RegexOptions.CultureInvariant);
            var importedClassMatch = Regex.Match(
                cssAssetContent,
                @"\.((?<class>jz_app_module_accent_[A-Za-z0-9_]+))\b",
                RegexOptions.CultureInvariant);
            var standaloneClassMatch = Regex.Match(
                cssAssetContent,
                @"\.((?<class>jz_standalone_module_panel_[A-Za-z0-9_]+))\b",
                RegexOptions.CultureInvariant);

            Assert.IsTrue(localClassMatch.Success, "Expected the extracted CSS asset to contain a hashed SFC CSS Modules class.");
            Assert.IsTrue(importedClassMatch.Success, "Expected the extracted CSS asset to contain a hashed Vue-script CSS Modules class.");
            Assert.IsTrue(standaloneClassMatch.Success, "Expected the extracted CSS asset to contain a hashed standalone CSS Modules class.");

            var localClassName = localClassMatch.Groups["class"].Value;
            var importedClassName = importedClassMatch.Groups["class"].Value;
            var standaloneClassName = standaloneClassMatch.Groups["class"].Value;

            StringAssert.Contains(cssAssetContent, "." + localClassName);
            StringAssert.Contains(cssAssetContent, "." + importedClassName);
            StringAssert.Contains(cssAssetContent, "." + standaloneClassName);
            StringAssert.Contains(entryChunkContent, localClassName);
            StringAssert.Contains(entryChunkContent, importedClassName);
            StringAssert.Contains(entryChunkContent, standaloneClassName);
            Assert.IsFalse(cssAssetContent.Contains(".hero{", StringComparison.Ordinal));
            Assert.IsFalse(cssAssetContent.Contains(".accent{", StringComparison.Ordinal));
            Assert.IsFalse(cssAssetContent.Contains(".panel{", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static async Task<BuildResult> BuildAsync(
        string rootDirectory,
        string outDir,
        bool minify)
    {
        var orchestrator = new BuildOrchestrator();
        return await orchestrator.BuildAsync(
            new BuildOptions
            {
                RootDirectory = rootDirectory,
                OutDir = outDir,
                SourceMap = SourceMapOption.None,
                Minify = minify,
                CodeSplitting = false
            },
            CancellationToken.None);
    }

    private static async Task<string> ReadEntryChunkAsync(string rootDirectory, BuildResult result)
    {
        var entryChunk = result.Chunks.Single(static chunk => chunk.IsEntry);
        return await ReadFileAsync(rootDirectory, entryChunk.FilePath);
    }

    private static async Task<string> ReadFileAsync(string rootDirectory, string relativePath)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return await File.ReadAllTextAsync(fullPath);
    }

    private static int CountLines(string text)
        => text.Split(["\r\n", "\n"], StringSplitOptions.None).Length;

    private static string FormatDiagnostics(BuildResult result)
        => string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.Message));

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "jazor-build-opt-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(path);
        return path;
    }
}
