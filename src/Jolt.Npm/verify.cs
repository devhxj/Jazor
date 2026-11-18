#:property CETCompat=false
#:property PublishAot=false
#:project ../Jolt.Compiler/Jolt.Compiler.csproj

using Jolt.Compiler;
using Jolt.Npm;

var root = Path.GetFullPath("samples/JazorComponentProbe/Npm");
var provider = new NpmVueMetadataProvider(Path.GetFullPath(".tmp/nodeapi-store"));
var current = Path.Combine(root, "current", "components", "Counter.jazor");
var previous = Path.Combine(root, "previous", "Counter.jazor");
var exports = await provider.ListExportsAsync(current, "reka-ui");
Check(exports.Contains("CollapsibleRoot"), "component export discovery");
Check(!exports.Contains("useForwardProps"), "non-component omission");
Check(NpmNames.NamespaceFor("@scope/my-package/widget") == "Npm.Scope.MyPackage.Widget", "scoped subpath namespace");
Check(NpmNames.TagFor("@scope/my-package/widget", "default") == "Widget", "default export tag");
var collapsible = await provider.ReadAsync(current, "reka-ui", "CollapsibleRoot");
Check(collapsible.Props["open"] is { Type: "boolean", Required: false }, "boolean prop");
Check(collapsible.Events["update:open"].Signatures is [[{ Name: "value", Type: "boolean" }]], "update event tuple payload");
Check(collapsible.Slots["default"].Signatures[0][0].Type.Contains("open: boolean"), "scoped slot");
Check(collapsible.Slots["default"].Signatures[0][0].Properties?["open"] == "boolean", "structured slot payload");
Check(collapsible.Props["open"].Definition is { Length: > 0 }, "prop definition");
Check(ReferenceEquals(collapsible, await provider.ReadAsync(current, "reka-ui", "CollapsibleRoot")), "cached metadata");
var generic = await provider.ReadAsync(current, "reka-ui", "SwitchRoot");
Check(generic.Props["disabled"].Type == "boolean" && generic.Events.ContainsKey("update:modelValue")
    && generic.Slots.ContainsKey("default"), "generic Vue callable component metadata");
var virtualizer = await provider.ReadAsync(current, "reka-ui", "ListboxVirtualizer");
Check(virtualizer.TypeParameter is { Name: "T" } typeParameter && typeParameter.PrimitiveKinds.Contains("string") &&
    virtualizer.Props["options"] is { Type: "T[]", Required: true } &&
    virtualizer.Props["textContent"].Type == "(option: T) => string" &&
    virtualizer.Slots["default"].Signatures[0][0].Properties?["option"] == "T",
    "published generic collection, callback and slot preserve the declared type parameter");
const string virtualizerSource = """
    @using Npm.RekaUi
    <ListboxVirtualizer Options="@Names" TextContent="@Label">
        <template #default context="row"><span>@row.option.Length</span></template>
    </ListboxVirtualizer>
    @code {
        private string[] Names = ["One"];
        private string Label(string item) => item;
    }
    """;
var virtualizerOptions = new JoltCompileOptions(NpmComponents: [virtualizer]);
var typed = await JoltProjectCompiler.CompileAsync([new("App.jazor", virtualizerSource)], virtualizerOptions);
Check(typed.Diagnostics.Count == 0 && typed.Modules.Single().Content.Contains("row[\"option\"].length"), "generic npm lowering");
foreach (var (invalid, diagnostic) in new[]
{
    (virtualizerSource.Replace("Label(string item) => item", "Label(bool item) => \"bad\""), "CS1503"),
    (virtualizerSource.Replace("row.option.Length", "row.option.Missing"), "CS1061"),
    (virtualizerSource.Replace("string[] Names = [\"One\"]", "bool[] Names = [true]"), "JOLT1021"),
})
{
    var invalidOutput = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalid)], virtualizerOptions);
    Check(invalidOutput.Modules.Count == 0 && invalidOutput.Diagnostics.Any(item => item.Id == diagnostic), $"generic npm diagnostic {diagnostic}");
}
var number = await provider.ReadAsync(current, "reka-ui", "NumberFieldRoot");
Check(number.Version == "2.11.0" && number.Props.ContainsKey("allowInvalid"), "new package declaration");
var oldNumber = await provider.ReadAsync(previous, "reka-ui", "NumberFieldRoot");
Check(oldNumber.Version == "2.10.1" && !oldNumber.Props.ContainsKey("allowInvalid"), "previous package declaration");

var temporary = Directory.CreateTempSubdirectory("jolt-npm-");
try
{
    var project = temporary.FullName;
    var package = Directory.CreateDirectory(Path.Combine(project, "fixture")).FullName;
    await File.WriteAllTextAsync(Path.Combine(project, "package.json"), """
        {"private":true,"dependencies":{"component-fixture":"file:./fixture","vue":"3.5.42"}}
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "package.json"), """
        {"name":"component-fixture","version":"1.0.0","type":"module","exports":{".":{"types":"./index.d.ts","import":"./index.js"},"./widgets":{"types":"./index.d.ts","import":"./index.js"}}}
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "index.js"), "export default { render() {} };");
    await File.WriteAllTextAsync(Path.Combine(package, "index.d.ts"), """
        import type { DefineComponent } from 'vue';
        interface WidgetProps { tone: 'quiet' | 'strong'; }
        declare const Widget: DefineComponent<WidgetProps>;
        export default Widget;
        """);
    var document = Path.Combine(project, "views", "Widget.jazor");
    Check((await provider.ListSpecifiersAsync(document, "component-fixture")).SequenceEqual(["component-fixture", "component-fixture/widgets"]),
        "fresh install with explicit subpaths");
    var first = await provider.ReadAsync(document, "component-fixture/widgets", "default");
    Check(first.PackageName == "component-fixture", "subpath package identity");
    Check(first.Props["tone"].Required && first.Props["tone"].LiteralValues.SequenceEqual(["quiet", "strong"]), "required literal-union prop");
    Check(first.Definition is { Length: > 0 } && File.Exists(first.Definition.Path), "export definition");
    File.AppendAllText(first.DeclarationPath, "\n");
    var second = await provider.ReadAsync(document, "component-fixture/widgets", "default");
    Check(!ReferenceEquals(first, second), "installed declaration content invalidation");

    var declared = Path.Combine(project, "package.json");
    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"reka-ui":"2.10.1","vue":"3.5.42"}}""");
    var beforeUpgrade = await provider.ReadAsync(document, "reka-ui", "NumberFieldRoot");
    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"reka-ui":"2.11.0","vue":"3.5.42"}}""");
    var afterUpgrade = await provider.ReadAsync(document, "reka-ui", "NumberFieldRoot");
    Check(beforeUpgrade.Version == "2.10.1" && !beforeUpgrade.Props.ContainsKey("allowInvalid") &&
        afterUpgrade.Version == "2.11.0" && afterUpgrade.Props.ContainsKey("allowInvalid"), "in-place dependency upgrade invalidates metadata");
}
finally
{
    temporary.Delete(recursive: true);
}
Console.WriteLine("Verified Reka UI and default/subpath Vue component metadata.");

static void Check(bool value, string description)
{
    if (!value)
        throw new InvalidOperationException($"Verification failed for {description}.");
}
