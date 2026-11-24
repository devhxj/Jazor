#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using System.Collections;
using System.Reflection;
using Jazor.Compiler;
using Jazor.NodeHost;
using Jolt.Compiler;
using Jazor.Npm;
using Jolt.Npm;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var root = Path.GetFullPath(".");
var project = Path.GetFullPath(args is [var existing] ? existing :
    Path.Combine("scripts", "csharp", "fixtures", "jolt-binding-projection"));
var directory = Directory.CreateDirectory(Path.Combine(project, $".i18n-plugin-{Guid.NewGuid():N}"));
try
{
    var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(root, ".tmp", "nodeapi-store"));
    File.WriteAllText(Path.Combine(directory.FullName, "package.json"), """{"private":true,"type":"module"}""");
    var shaped = Directory.CreateDirectory(Path.Combine(directory.FullName, "node_modules", "locale-shape-fixture"));
    File.WriteAllText(Path.Combine(shaped.FullName, "package.json"),
        """{"name":"locale-shape-fixture","version":"1.0.0","type":"module","exports":"./index.js"}""");
    File.WriteAllText(Path.Combine(shaped.FullName, "index.js"), "export const inspect = () => ({});");
    File.WriteAllText(Path.Combine(shaped.FullName, "index.d.ts"), """
        import type { ComputedRef } from 'vue';
        type IsNever<T> = [T] extends [never] ? true : false;
        type IsEmpty<T> = [keyof T] extends [never] ? true : false;
        type RegionKeys<T, K = keyof T> = K extends string ? K : never;
        interface Inventory<T extends Record<string, any>, Faults extends Record<string, number>> {
            readonly entries: ComputedRef<{ [K in keyof T]: T[K] }>;
            readonly counts: ComputedRef<{ [K in keyof Faults]: Faults[K] }>;
            byRegion<S extends Record<string, any> = never, R extends string = string,
                K extends RegionKeys<NonNullable<T>> = RegionKeys<NonNullable<T>>, V = IsNever<S> extends true
                    ? IsEmpty<T> extends true ? {} : NonNullable<T>[K]
                    : S>(region: R | K): V;
        }
        export declare function inspect(): Inventory<Record<string, Record<string, unknown>>, Record<string, number>>;
        type Preferences = string | string[] | {[key: string]: string[]} | false;
        export declare function preferences(): { choice: Preferences };
        export declare function savePreferences(choice: Preferences): void;
        type Path<T> = Extract<keyof T, string>;
        type Empty<T> = keyof T extends never ? true : false;
        type Read<T, K> = K extends keyof T ? T[K] : never;
        interface DynamicInventory<M> {
            lookup<K extends string, P extends Path<M> = Path<M>, L extends Path<NonNullable<M>> = Path<NonNullable<M>>,
                T = Empty<M> extends false ? NonNullable<M>[L] : {}, R = P extends Path<T> ? Read<T, P> : Record<string, unknown>>(key: K | P): R;
            competing(key: 'reserved'): number;
            competing<K extends string, P extends Path<M> = Path<M>, L extends Path<NonNullable<M>> = Path<NonNullable<M>>,
                T = Empty<M> extends false ? NonNullable<M>[L] : {}, R = P extends Path<T> ? Read<T, P> : Record<string, unknown>>(key: K | P): R;
        }
        export declare function dynamicInventory(): DynamicInventory<Record<string, Record<string, unknown>>>;
        """);
    var synthetic = await provider.ReadValueAsync(Path.Combine(directory.FullName, "Probe.jazor"), "locale-shape-fixture", "inspect");
    var members = synthetic.Signatures.Single().ReturnType.Members!;
    if (members.SingleOrDefault(member => member.Name == "entries")?.Type.ElementType is not
            { Kind: "readonlyDictionary", ElementType.Kind: "plainRecord" } ||
        members.Any(member => member.Name == "counts" && member.Type.ElementType?.ElementType?.Kind == "plainRecord"))
        throw new InvalidOperationException("The locale map must use the receiver slot indexed by the resource getter. " +
            string.Join(", ", members.Select(member => $"{member.Name}={member.Type.Kind}")));
    var values = await provider.ReadValuesAsync(Path.Combine(project, "Probe.jazor"), "vue-i18n", ["createI18n", "useI18n"]);
    var preferenceValues = await provider.ReadValuesAsync(Path.Combine(directory.FullName, "Probe.jazor"), "locale-shape-fixture",
        ["preferences", "savePreferences"]);
    var preferences = preferenceValues.Single(value => value.ExportName == "preferences");
    var dynamicInventory = await provider.ReadValueAsync(Path.Combine(directory.FullName, "Probe.jazor"),
        "locale-shape-fixture", "dynamicInventory");
    var dynamicMembers = dynamicInventory.Signatures.Single().ReturnType.Members!;
    if (dynamicMembers.Single(member => member.Name == "lookup").Type.Signatures?.Single().ReturnType.Kind != "unknownValue" ||
        dynamicMembers.Any(member => member.Name == "competing" &&
            member.Type.Signatures!.Any(signature => signature.ReturnType.Kind == "unknownValue")))
        throw new InvalidOperationException("Renamed dynamic lookups must establish their resource relationship without a competing overload.");
    if (preferences.Signatures.Single().ReturnType.Members?.Single().Type is not
        { Kind: "stringArrayMapUnion", Name: "Preferences", TypeIdentity: not null })
        throw new InvalidOperationException("Expected a closed string, array, sparse map and false domain for the renamed property.");
    if (values.Count != 2 || values.Any(value => value.Version != "11.4.12"))
        throw new InvalidOperationException("Expected the locked Vue I18n plugin and composable.");
    var source = File.ReadAllText(Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-binding-projection", "I18nPlugin.jazor"));
    var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], new() { NpmValues = values });
    if (result.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
    var component = JoltProjectCompiler.Analyze([new("App.jazor", source)], new() { NpmValues = values })
        .Documents.Single().SemanticModel.Compilation;
    var projection = component.SyntaxTrees.Single(tree => tree.FilePath == "__JoltNpmValues.g.cs");
    var bindingSource = NpmBindings.GenerateSource([], [..values, ..preferenceValues]);
    var cachedPreferences = await provider.ReadValueAsync(Path.Combine(directory.FullName, "Probe.jazor"),
        "locale-shape-fixture", "preferences");
    var choiceName = System.Text.RegularExpressions.Regex.Match(bindingSource, @"union (Preferences_\w+)\(").Groups[1].Value;
    if (bindingSource != NpmBindings.GenerateSource([], [..preferenceValues.Reverse(), ..values.Reverse()]) ||
        !bindingSource.Contains("union FallbackLocales_", StringComparison.Ordinal) ||
        choiceName.Length == 0 || !NpmBindings.GenerateSource([], [cachedPreferences])
            .Contains($"union {choiceName}(", StringComparison.Ordinal))
        throw new InvalidOperationException("Closed unions must retain stable declaration names regardless of export order.");
    var publicTree = CSharpSyntaxTree.ParseText(bindingSource, (CSharpParseOptions)projection.Options);
    var bindings = CSharpCompilation.Create("I18nBindings", [publicTree], component.References,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    using var assembly = new MemoryStream();
    var emitted = bindings.Emit(assembly, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(metadataOnly: true));
    if (!emitted.Success)
        throw new InvalidOperationException(string.Join(Environment.NewLine, emitted.Diagnostics));
    assembly.Position = 0;
    var roundTrip = CSharpSyntaxTree.ParseText("""
        using Fixture = Npm.LocaleShapeFixture.Exports;
        public static class Consumer
        {
            public static void Copy() => Fixture.SavePreferences(Fixture.Preferences().Choice);
        }
        """, (CSharpParseOptions)projection.Options);
    var consumer = component.RemoveSyntaxTrees(projection).AddSyntaxTrees(roundTrip).AddReferences(MetadataReference.CreateFromStream(assembly));
    var publicErrors = consumer.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
    if (publicErrors.Length != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, publicErrors.AsEnumerable()));
    var invalidFallback = source.Replace("FallbackLocale = \"en\"", "FallbackLocale = true", StringComparison.Ordinal);
    var rejectedFallback = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalidFallback)], new() { NpmValues = values });
    if (!rejectedFallback.Diagnostics.Any(diagnostic => diagnostic.ToString().Contains("CS0029", StringComparison.Ordinal)))
        throw new InvalidOperationException("Expected a C# type error for true as a fallback locale.");
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
        ("composer.Te(\"onlyFrench\", composer.Locale.Value)", "composer.Te(\"onlyFrench\", false)", "CS1503"),
        ("composer.Rt(\"Resolved {'message'}\")", "composer.Rt(321)", "CS1503"),
        ("var listed = composer.Messages.Value[\"fr\"];", "composer.Messages.Value[\"fr\"] = merged; var listed = merged;", "CS0200"),
        ("var datetime = composer.DatetimeFormats.Value[\"en\"];", "var datetime = composer.GetDateTimeFormat(\"en\"); composer.DatetimeFormats.Value[\"en\"] = datetime;", "CS0200"),
        ("var number = composer.NumberFormats.Value[\"en\"];", "var number = composer.GetNumberFormat(\"en\"); composer.NumberFormats.Value[\"en\"] = number;", "CS0200"),
        ("global.Locale.Value = \"en\"", "global.Locale = \"en\"", "CS0029"),
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
        import { createI18n } from 'vue-i18n';
        import { renderToString } from 'vue/server-renderer';
        import App, { CompositionGlobal, DynamicMessages, Fallback, FormatMaps, Install, LegacyGlobal, MessageTree } from './App.mjs';
        const dynamic = { plain: 'text', nested: { leaf: 'leaf' }, number: 7, callable: () => 'function' };
        assert.equal(DynamicMessages(dynamic, false), true);
        assert.equal(DynamicMessages(dynamic, true), true);
        assert.equal(Fallback(), 'en/English/fr/French/en/English/target');
        assert.equal(CompositionGlobal(), 'fr/Bonjour/en/Hello');
        const mixed = { nested: { original: 'Original', zero: 0, enabled: false, values: ['one', 2, false] } };
        const tree = MessageTree(mixed, 'missing-locale');
        assert.equal(tree.original, 'Original');
        assert.equal(tree.merged, 'Merged');
        assert.equal(tree.resolved, 'Resolved message');
        assert.deepEqual(tree.replacement, { nested: { replacement: 'Replaced' } });
        assert.deepEqual(tree.roundTrip, { nested: { original: 'Original', zero: 0, enabled: false,
            values: ['one', 2, false], added: 'Merged' } });
        assert.deepEqual(tree.requested, {});
        assert.equal(tree.listed, tree.roundTrip);
        assert.equal(tree.missingFromMap, true);
        const formats = FormatMaps('missing-locale');
        assert.deepEqual(formats.datetime, { short: { year: 'numeric' } });
        assert.deepEqual(formats.number, { currency: { style: 'currency', currency: 'USD' } });
        assert.equal(formats.datetime, formats.datetimeFromGetter);
        assert.equal(formats.number, formats.numberFromGetter);
        assert.equal(formats.missingDatetime, true);
        assert.equal(formats.missingNumber, true);
        assert.equal(LegacyGlobal(), 'en/Hello/fr/Salut/en');
        assert.equal(CompositionGlobal(), 'fr/Bonjour/en/Hello');
        const first = createSSRApp(App);
        const second = createSSRApp(App);
        first.config.errorHandler = second.config.errorHandler = error => { throw error; };
        const disposeFirst = Install(first, 'Bonjour {guest}');
        const disposeSecond = Install(second, 'Salut {guest}');
        const third = createSSRApp(App);
        third.use(createI18n({
            legacy: false, locale: 'fr', messages: { fr: { welcome: 'Bonjour {guest}' } },
            messageResolver: (messages, key) => key === 'resolver-key' ? 'resolver value' : messages[key] ?? null,
        }));
        assert.match(await renderToString(third), /<span id="resolver-key">resolver value<\/span>/);
        const number = new Intl.NumberFormat('fr').format(321);
        const date = new Intl.DateTimeFormat('fr').format(new Date('2026-11-21T12:00:00Z'));
        assert.deepEqual(await Promise.all([renderToString(first), renderToString(second)]), [
            `<p>Bonjour Ada / Bonjour Lin / ${number} / ${date} / Hello<span id="known-key">absent/missing</span><span id="resolver-key"></span></p>`,
            `<p>Salut Ada / Salut Lin / ${number} / ${date} / Hello<span id="known-key">absent/missing</span><span id="resolver-key"></span></p>`,
        ]);
        disposeFirst();
        disposeSecond();
        console.log('C# created and installed the Vue I18n plugin and rendered its messages on the server.');
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify));
    await VerifyStaticI18n(provider);
}
finally
{
    directory.Delete(recursive: true);
}

async Task VerifyStaticI18n(NpmVueMetadataProvider provider)
{
    var package = Directory.CreateDirectory(Path.Combine(directory.FullName, "static-i18n"));
    File.WriteAllText(Path.Combine(package.FullName, "package.json"), """
        {"name":"static-i18n-fixture","version":"1.0.0","type":"module","exports":{"types":"./index.d.mts","import":"./index.mjs"}}
        """);
    File.WriteAllText(Path.Combine(package.FullName, "index.d.mts"), """
        import type { Composer } from 'vue-i18n';
        type SpanishPanelTitle = 'Hola {guest}' | 'Buenas {guest}';
        type SpanishPanelScore = 0 | 0.5;
        export declare const messages: {
            readonly en: {
                readonly welcome: 'Hello {guest}';
                readonly shared: '{guest}';
                readonly status: 'Count {count}, active {active}';
                readonly checkout: { readonly title: 'Checkout' };
                readonly fallback: { readonly title: 'English only' };
                readonly 欢迎: '你好';
                readonly config: { readonly enabled: true; readonly caption: string; readonly array: readonly string[] };
                readonly widened: string;
                readonly plain: 'Plain text';
                readonly escaped: "Contact {'@'} support";
                readonly linked: '@:plain';
                readonly plural: 'None | One | Many';
                readonly indexed: 'Item {0}';
                readonly value: '{0} / {0}';
                readonly namedPlural: '{guest} has none | {guest} has {n} | {guest} has {count}';
                readonly listPlural: 'None | {0} has one | {0} has many';
                readonly sparse: 'Item {2}';
                readonly mixed: '{guest} {0}';
                readonly malformed: 'Unclosed {name';
                readonly orderTotal: 'Flat';
                readonly order: { readonly total: 'Nested' };
            };
            readonly fr: {
                readonly welcome: 'Bonjour {guest}';
                readonly status: 'Total {count}, actif {active}';
                readonly value: '{1} / {0} / {1}';
            };
            readonly es: {
                readonly panel: {
                    readonly title: SpanishPanelTitle;
                    readonly caption: string;
                    readonly enabled: false;
                    readonly score: SpanishPanelScore;
                };
                readonly nullable?: { readonly first: string; readonly second: string } | null;
                readonly extra?: { readonly first: string; readonly second: string };
            };
            readonly pt?: { readonly onlyOptional: 'Opcional'; readonly shared: '{guest} {place}' };
        };
        export declare function openComposer(fallback: boolean): Composer<typeof messages>;
        export declare function acceptScore(options: { score: SpanishPanelScore }): number;
        export declare function openDynamicComposer(): Composer;
        export declare function openLookalike(): Pick<Composer<typeof messages>, 'messages' | 't' | 'tm' | 'getLocaleMessage'>;
        """);
    File.WriteAllText(Path.Combine(package.FullName, "index.mjs"), """
        import { createI18n } from 'vue-i18n';
        export const messages = {
            en: { welcome: 'Hello {guest}', shared: '{guest}', status: 'Count {count}, active {active}', checkout: { title: 'Checkout' },
                fallback: { title: 'English only' }, config: { enabled: true, caption: 'Original', array: ['retained'] }, 欢迎: '你好',
                widened: 'Dynamic {value}', plain: 'Plain text', escaped: "Contact {'@'} support",
                linked: '@:plain', plural: 'None | One | Many', indexed: 'Item {0}',
                value: '{0} / {0}',
                namedPlural: '{guest} has none | {guest} has {n} | {guest} has {count}',
                listPlural: 'None | {0} has one | {0} has many',
                sparse: 'Item {2}', mixed: '{guest} {0}',
                malformed: 'Unclosed {name', orderTotal: 'Flat', order: { total: 'Nested' } },
            fr: { welcome: 'Bonjour {guest}', status: 'Total {count}, actif {active}', value: '{1} / {0} / {1}' },
            es: { panel: { title: 'Hola {guest}', caption: 'Inicial', enabled: false, score: 0 }, nullable: null },
        };
        export function acceptScore(options) { return options.score; }
        export function openComposer(fallback) {
            const composer = createI18n({
                legacy: false, locale: 'fr', fallbackLocale: fallback ? 'en' : false,
                missingWarn: false, fallbackWarn: false, messages: structuredClone(messages),
            }).global;
            const translate = composer.t;
            composer.t = function (...args) {
                if (this !== composer) throw new Error('Translation lost the Composer receiver.');
                return translate(...args);
            };
            return composer;
        }
        export function openDynamicComposer() {
            return createI18n({
                legacy: false, locale: 'fr', messages,
                messageResolver: (resource, key) => key === 'resolver-key' ? 'resolver value' : resource[key] ?? null,
            }).global;
        }
        export function openLookalike() {
            return openComposer(true);
        }
        """);
    var link = Path.Combine(directory.FullName, "link-i18n.mjs");
    File.WriteAllText(link, """
        import { symlinkSync } from 'node:fs';
        import { fileURLToPath } from 'node:url';
        symlinkSync(fileURLToPath(new URL('./static-i18n', import.meta.url)),
            fileURLToPath(new URL('./node_modules/static-i18n-fixture', import.meta.url)),
            process.platform === 'win32' ? 'junction' : 'dir');
        """);
    await NodeModuleRunner.RunAsync(project, link);
    try
    {
        await VerifyStaticI18nConsumer(provider);
    }
    finally
    {
        Directory.Delete(Path.Combine(directory.FullName, "node_modules", "static-i18n-fixture"));
    }
}

async Task VerifyStaticI18nConsumer(NpmVueMetadataProvider provider)
{
    const string componentPath = "StaticI18n.jazor";
    var values = await provider.ReadValuesAsync(Path.Combine(directory.FullName, componentPath),
        "static-i18n-fixture", ["openComposer", "openDynamicComposer", "openLookalike", "acceptScore"]);
    var staticMembers = values.Single(value => value.ExportName == "openComposer").Signatures.Single().ReturnType.Members!;
    var staticKeys = staticMembers.Where(member => member.JsName == "t" && member.StaticKey is not null).Select(member => member.StaticKey).ToArray();
    var bindingSource = NpmBindings.GenerateSource([], values);
    var replacementPayload = staticMembers.Single(member => member.Name == "setLocaleMessageEs")
        .Type.Signatures!.Single().Parameters.Single().Type;
    var patch = staticMembers.Single(member => member.Name == "mergeLocaleMessageEs")
        .Type.Signatures!.Single().Parameters.Single().Type;
    var englishPatch = staticMembers.Single(member => member.Name == "mergeLocaleMessageEn")
        .Type.Signatures!.Single().Parameters.Single().Type;
    var englishConfig = englishPatch.Members!.Single(member => member.Name == "config").Type.Members!;
    if (staticMembers.Any(member => member.Name == "setLocaleMessageEn") ||
        !englishConfig.Any(member => member.Name == "caption") || englishConfig.Any(member => member.Name == "array"))
        throw new InvalidOperationException("A required array prevents full replacement and leaves supported fields patchable.");
    var panel = replacementPayload.Members!.Single(member => member.Name == "panel");
    var nullable = patch.Members!.Single(member => member.Name == "nullable");
    if (!nullable.Optional || nullable.Type.Kind != "nullable" ||
        nullable.Type.ElementType!.Members!.Any(member => member.Optional))
        throw new InvalidOperationException("A nullable child can be cleared or replaced with a complete object.");
    var extra = replacementPayload.Members!.Single(member => member.Name == "extra");
    if (panel.Optional || panel.Type.Members!.Any(member => member.Optional) || !extra.Optional ||
        !patch.Members!.Single(member => member.Name == "panel").Optional ||
        patch.Members!.Single(member => member.Name == "panel").Type.Members!.Any(member => !member.Optional) ||
        !patch.Members!.Single(member => member.Name == "extra").Optional ||
        patch.Members!.Single(member => member.Name == "extra").Type.Members!.Any(member => member.Optional))
        throw new InvalidOperationException("Replacement and patch must preserve required fields and avoid creating incomplete optional objects.");
    if (!staticKeys.Contains("欢迎", StringComparer.Ordinal) ||
        staticMembers.Any(member => member.Name is "setLocaleMessage" or "mergeLocaleMessage"))
        throw new InvalidOperationException("Concrete composers must expose the Chinese message and protect typed resources from broad writes.");
    var dynamicMembers = values.Single(value => value.ExportName == "openDynamicComposer").Signatures.Single().ReturnType.Members!;
    if (new[] { "setLocaleMessage", "mergeLocaleMessage" }.Any(name => !dynamicMembers.Any(member => member.Name == name)))
        throw new InvalidOperationException("Open composers must retain both locale writers.");
    if (!staticKeys.Contains("welcome", StringComparer.Ordinal) || staticKeys.Any(key => key is "widened" or "order.total" or "orderTotal"))
        throw new InvalidOperationException($"Unexpected static translation keys after widening and collisions. {string.Join(", ", staticKeys)}");
    if (!new[] { "plain", "escaped", "plural", "indexed", "value", "namedPlural", "listPlural" }
            .All(key => staticKeys.Contains(key, StringComparer.Ordinal)) ||
        staticKeys.Any(key => key is "linked" or "sparse" or "mixed" or "malformed"))
        throw new InvalidOperationException($"Unexpected static translation keys for the message grammar. {string.Join(", ", staticKeys)}");
    var shared = staticMembers.Single(member => member.StaticKey == "shared").Type.Signatures!.Single();
    var sharedNames = shared.Parameters.Single().Type.Members!;
    if (staticKeys.Contains("onlyOptional", StringComparer.Ordinal) ||
        !sharedNames.Select(member => member.Name).ToHashSet(StringComparer.Ordinal).SetEquals(["guest", "place"]))
        throw new InvalidOperationException("Optional locales contribute arguments to required keys without introducing new shortcuts.");
    var lookalike = values.Single(value => value.ExportName == "openLookalike").Signatures.Single().ReturnType.Members!;
    if (lookalike.Any(member => member.StaticKey is not null))
        throw new InvalidOperationException("A structural Composer lookalike must have no static translation keys.");
    var source = File.ReadAllText(Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-binding-projection", componentPath));
    var options = new JoltCompileOptions { NpmValues = values };
    var result = await JoltProjectCompiler.CompileAsync([new(componentPath, source)], options);
    if (result.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
    File.WriteAllText(Path.Combine(directory.FullName, "StaticI18n.mjs"), result.Modules.Single().Content);
    foreach (var (original, replacement, code) in new[]
    {
        ("TWelcome", "TWelcomme", "CS1061"),
        ("new() { Guest = \"Ada\" }", "new()", "CS9035"),
        ("Guest = \"Ada\"", "Guest = new int[1]", "CS0029"),
        ("Guest = \"Ada\"", "Name = \"Ada\"", "CS0117"),
        ("TIndexed(\"a\")", "TIndexed()", "CS7036"),
        ("TValue(0d, false)", "TValue(0d)", "CS7036"),
        ("TPlural(2d)", "TPlural(false)", "CS1503"),
        ("TNamedPlural(new() { Guest = \"Ada\" }, 2d)", "TNamedPlural(new() { Guest = \"Ada\" })", "CS7036"),
        ("TListPlural(false, 2d)", "TListPlural(false, false)", "CS1503"),
        ("I18n.OpenComposer(true);", "I18n.OpenDynamicComposer();", "CS1061"),
        (".Value.En!.Config.Enabled", ".Value.Fr!.Config.Enabled", "CS1061"),
        (".Value.En!.Config.Enabled", ".Value.De!.Config.Enabled", "CS1061"),
        ("Caption = \"Reemplazo\", Enabled = ECMAScript.False.Value", "Enabled = ECMAScript.False.Value", "CS9035"),
        ("Title = SpanishTitle.HolaGuest", "Title = \"Hola {name}\"", "CS0029"),
        ("Title = SpanishTitle.BuenasGuest", "Title = \"Adios {guest}\"", "CS0029"),
        ("Panel = new() { Title = SpanishTitle.BuenasGuest }", "Welcome = \"Bonjour\"", "CS0117"),
        ("Nullable = new() { First = \"uno\", Second = \"dos\" }", "Nullable = new() { First = \"uno\" }", "CS9035"),
        ("Extra = new() { First = \"uno\", Second = \"dos\" }", "Extra = new() { First = \"uno\" }", "CS9035"),
    })
    {
        var rejected = await JoltProjectCompiler.CompileAsync(
            [new(componentPath, source.Replace(original, replacement, StringComparison.Ordinal))], options);
        if (!rejected.Diagnostics.Any(diagnostic => diagnostic.Id == code))
            throw new InvalidOperationException($"Expected {code} for {replacement}. {string.Join(", ", rejected.Diagnostics)}");
    }

    var component = JoltProjectCompiler.Analyze([new(componentPath, source)], options).Documents.Single().SemanticModel.Compilation;
    var parseOptions = (CSharpParseOptions)component.SyntaxTrees.First().Options;
    var bindingTree = CSharpSyntaxTree.ParseText(bindingSource, parseOptions);
    var bindings = CSharpCompilation.Create("StaticI18nBindings", [bindingTree], component.References,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    using var bindingDll = new MemoryStream();
    var emitted = bindings.Emit(bindingDll, options: new Microsoft.CodeAnalysis.Emit.EmitOptions(metadataOnly: true));
    if (!emitted.Success)
        throw new InvalidOperationException(string.Join(Environment.NewLine, emitted.Diagnostics));
    Console.WriteLine("Verified widened messages, Composer lookalikes and colliding names in public bindings.");
    bindingDll.Position = 0;
    var consumerTree = CSharpSyntaxTree.ParseText("""
        using I18n = Npm.StaticI18nFixture.Exports;
        using SpanishTitle = Jolt.Generated.Npm.Exports.SpanishPanelTitle_98FDFB990253;
        using SpanishScore = Jolt.Generated.Npm.Exports.SpanishPanelScore_1A28DA94C57C;
        [ECMAScript.ECMAScriptModule("static-i18n-consumer.mjs")]
        public static class StaticI18nConsumer
        {
            public static string Translate()
            {
                var composer = I18n.OpenComposer(true);
                return composer.TWelcome(new() { Guest = "Lin" }) + " / " + composer.TCheckoutTitle()
                    + " / " + composer.TIndexed("b") + " / " + composer.TValue(0d, false)
                    + " / " + composer.TPlural(2d) + " / " + composer.TNamedPlural(new() { Guest = "Lin" }, 2d)
                    + " / " + composer.TListPlural(false, 2d);
            }

            public static string Write()
            {
                var composer = I18n.OpenComposer(false);
                composer.Locale.Value = I18n.StringChoiceOfEnOrEsOrFrOrPt.Es;
                composer.SetLocaleMessageEs(new() {
                    Panel = new() { Title = SpanishTitle.HolaGuest, Caption = "DLL", Enabled = ECMAScript.False.Value, Score = SpanishScore.Value0 },
                });
                composer.MergeLocaleMessageEs(new() { Panel = new() { Title = SpanishTitle.BuenasGuest, Enabled = ECMAScript.False.Value } });
                return composer.TPanelTitle(new() { Guest = "Lin" })
                    + " / " + composer.Tm("panel.caption").AsString
                    + " / " + (composer.Tm("panel.enabled").AsBoolean == false ? "false" : "wrong");
            }

            public static bool WriteScore()
            {
                var composer = I18n.OpenComposer(false);
                composer.Locale.Value = I18n.StringChoiceOfEnOrEsOrFrOrPt.Es;
                var score = SpanishScore.Value0_5;
                composer.SetLocaleMessageEs(new() {
                    Panel = new() { Title = SpanishTitle.HolaGuest, Caption = "DLL", Enabled = ECMAScript.False.Value, Score = score },
                });
                var accepted = I18n.AcceptScore(new() { Score = score });
                composer.MergeLocaleMessageEs(new() { Panel = new() { Score = SpanishScore.Value0 } });
                return accepted == 0.5 && composer.Tm("panel.score").AsNumber == 0;
            }
        }
        """, parseOptions);
    var consumer = CSharpCompilation.Create("StaticI18nConsumer", [consumerTree],
        [..component.References, MetadataReference.CreateFromStream(bindingDll)],
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    GeneratorDriver driver = CSharpGeneratorDriver.Create([new ESGenerator().AsSourceGenerator()], parseOptions: parseOptions);
    driver.RunGeneratorsAndUpdateCompilation(consumer, out var generated, out var diagnostics);
    using var consumerDll = new MemoryStream();
    emitted = generated.Emit(consumerDll);
    if (!emitted.Success || diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        throw new InvalidOperationException(string.Join(Environment.NewLine, diagnostics.Concat(emitted.Diagnostics)));
    var catalog = Assembly.Load(consumerDll.ToArray()).GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var modules = (IEnumerable)catalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    var module = modules.Cast<object>().Single();
    File.WriteAllText(Path.Combine(directory.FullName, "static-i18n-consumer.mjs"),
        (string)module.GetType().GetProperty("Content")!.GetValue(module)!);

    var verify = Path.Combine(directory.FullName, "verify-static-i18n.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        import { createSSRApp, h } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import App, { ChineseMessage, Fallback, Grammar, Missing, RawMessages, Resolve, ScalarArguments, TypedResources, WriteEnglish, WriteNullable, WriteSpanish } from './StaticI18n.mjs';
        import { Translate, Write, WriteScore } from './static-i18n-consumer.mjs';
        assert.equal(await renderToString(createSSRApp(App)),
            '<p>Bonjour Ada / Checkout / Item a / false / 0 / false / None / One / Many / Ada has none / Ada has 1 / Ada has 2 / None / 0 has one / false has many</p>');
        assert.equal(Translate(), 'Bonjour Lin / Checkout / Item b / false / 0 / false / Many / Lin has 2 / false has many');
        assert.equal(ScalarArguments(), 'Total 0, actif false');
        assert.equal(Grammar(), 'Plain text / Contact @ support / Dynamic text');
        assert.equal(RawMessages(), 'Item {2} / {guest} {0} / Plain text / Unclosed {name');
        assert.equal(TypedResources(), true);
        assert.equal(ChineseMessage(), '你好');
        assert.equal(Fallback(), 'English only');
        assert.equal(Missing(), 'missing');
        assert.equal(Resolve(), 'resolver value');
        assert.equal(await renderToString(createSSRApp({ render: () => h('p', WriteSpanish()) })),
            '<p>Hola Ada / Buenas Ada / Reemplazo / false / uno / dos</p>');
        const nullable = WriteNullable();
        assert.equal(nullable.created, "uno / dos");
        assert.equal(Object.hasOwn(nullable.messages, "nullable"), true);
        assert.equal(nullable.messages.nullable, null);
        assert.deepEqual(WriteEnglish(), { enabled: true, caption: "Updated", array: ["retained"] });
        assert.equal(Write(), 'Buenas Lin / DLL / false');
        assert.equal(WriteScore(), true);
        console.log('Verified static Composer projection through a Jolt component and an independent binding DLL.');
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify));
}
