#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jolt.Npm;

var root = Path.GetFullPath(".");
var project = Path.GetFullPath(args is [var existing] ? existing :
    Path.Combine("scripts", "csharp", "fixtures", "jolt-binding-projection"));
var directory = Directory.CreateDirectory(Path.Combine(project, $".i18n-plugin-{Guid.NewGuid():N}"));
try
{
    var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(root, ".tmp", "nodeapi-store"));
    var values = await provider.ReadValuesAsync(Path.Combine(project, "Probe.jazor"), "vue-i18n", ["createI18n", "useI18n"]);
    if (values.Count != 2 || values.Any(value => value.Version != "11.4.12"))
        throw new InvalidOperationException("Expected the locked Vue I18n plugin and composable.");
    var source = File.ReadAllText(Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-binding-projection", "I18nPlugin.jazor"));
    var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = values });
    if (result.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
    var invalid = source.Replace("[\"guest\"] = \"Ada\"", "[\"guest\"] = false", StringComparison.Ordinal);
    var rejected = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalid)], new() { NpmValues = values });
    if (!rejected.Diagnostics.Any(diagnostic => diagnostic.ToString().Contains("CS0029", StringComparison.Ordinal)))
        throw new InvalidOperationException("Expected a C# type error for a boolean interpolation value.");
    var invalidFormat = source.Replace("composer.N(321)", "composer.N(false)", StringComparison.Ordinal);
    var rejectedFormat = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalidFormat)], new() { NpmValues = values });
    if (!rejectedFormat.Diagnostics.Any(diagnostic => diagnostic.ToString().Contains("CS1503", StringComparison.Ordinal)))
        throw new InvalidOperationException("Expected a C# type error for a boolean number format input.");
    var invalidMessages = source.Replace("[\"greeting\"] = \"Hello\"", "[\"greeting\"] = true", StringComparison.Ordinal);
    var rejectedMessages = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalidMessages)], new() { NpmValues = values });
    if (!rejectedMessages.Diagnostics.Any(diagnostic => diagnostic.ToString().Contains("CS0029", StringComparison.Ordinal)))
        throw new InvalidOperationException("Expected a C# type error for a boolean nested message.");
    foreach (var (original, replacement, code) in new[]
    {
        ("global.Locale.Value = \"en\"", "global.Locale = \"en\"", "CS0200"),
        ("global.Locale = \"fr\"", "global.Locale.Value = \"fr\"", "CS1061"),
    })
    {
        var wrongMode = await JoltProjectCompiler.CompileAsync([new("App.jazor",
            source.Replace(original, replacement, StringComparison.Ordinal))], new() { NpmValues = values });
        if (!wrongMode.Diagnostics.Any(diagnostic => diagnostic.Id == code))
            throw new InvalidOperationException($"Expected {code} for {replacement}. {string.Join(", ", wrongMode.Diagnostics)}");
    }
    File.WriteAllText(Path.Combine(directory.FullName, "App.mjs"), result.Modules.Single().Content);
    var verify = Path.Combine(directory.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        import { createSSRApp } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import App, { CompositionGlobal, Install, LegacyGlobal } from './App.mjs';
        assert.equal(CompositionGlobal(), 'fr/Bonjour/en/Hello');
        assert.equal(LegacyGlobal(), 'en/Hello/fr/Salut');
        assert.equal(CompositionGlobal(), 'fr/Bonjour/en/Hello');
        const first = createSSRApp(App);
        const second = createSSRApp(App);
        first.config.errorHandler = second.config.errorHandler = error => { throw error; };
        const disposeFirst = Install(first, 'Bonjour {guest}');
        const disposeSecond = Install(second, 'Salut {guest}');
        const number = new Intl.NumberFormat('fr').format(321);
        const date = new Intl.DateTimeFormat('fr').format(new Date('2026-11-21T12:00:00Z'));
        assert.deepEqual(await Promise.all([renderToString(first), renderToString(second)]), [
            `<p>Bonjour Ada / Bonjour Lin / ${number} / ${date} / Hello</p>`,
            `<p>Salut Ada / Salut Lin / ${number} / ${date} / Hello</p>`,
        ]);
        disposeFirst();
        disposeSecond();
        console.log('C# created and installed the Vue I18n plugin and rendered its messages on the server.');
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify));
}
finally
{
    directory.Delete(recursive: true);
}
