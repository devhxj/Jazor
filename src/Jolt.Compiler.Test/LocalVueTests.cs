using Jolt.Npm;
using Jolt.Project;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class LocalVueTests
{
    [TestMethod]
    public async Task LoadAndCompile_BindsVueContractAndUsesUnsavedDeclarations()
    {
        var root = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-local-vue-" + Guid.NewGuid()));
        try
        {
            var widgets = root.CreateSubdirectory("Widgets");
            var vueFile = Path.Combine(widgets.FullName, "Panel.vue");
            var typeFile = Path.Combine(widgets.FullName, "contract.d.cts");
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "package.json"), """{"private":true,"dependencies":{"vue":"3.5.30"}}""");
            await File.WriteAllTextAsync(typeFile, "export interface Contract { caption: number }");
            const string vue = """
                <script setup lang="ts">
                import type { Contract } from './contract.cjs'
                defineProps<Contract>()
                defineEmits<{ select: [label: string] }>()
                defineSlots<{ detail(props: { label: string }): void }>()
                const model = defineModel<string>({ required: true })
                </script>
                <template><div>{{ caption }} {{ model }}<slot name="detail" label="detail" /></div></template>
                """;
            const string page = """
                @using App.Widgets
                <Panel Caption="hello" @bind-ModelValue="model" @onselect="Select">
                    <Detail Context="item">@item.label.Length</Detail>
                </Panel>
                @code { private string model = ""; private void Select(string label) {} }
                """;
            await File.WriteAllTextAsync(Path.Combine(root.FullName, "Page.jazor"), page);
            const string valid = "export interface Contract { caption: string }";
            var overlays = new Dictionary<string, string> { [vueFile] = vue, [typeFile] = valid };
            var engineCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jolt", "pnpm-store");
            var provider = new NpmVueMetadataProvider(Path.Combine(root.FullName, ".pnpm-store"), engineCache);
            var workspace = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.IsEmpty(workspace.Diagnostics, string.Join(Environment.NewLine, workspace.Diagnostics));
            var component = workspace.Options.VueComponents.Single();
            Assert.AreEqual("Widgets/Panel.vue", component.LocalPath);
            Assert.AreEqual("string", component.Props["caption"].Type);
            var definition = component.Props["caption"].Definition!;
            Assert.AreEqual(typeFile, definition.Path);
            Assert.AreEqual("caption", valid.Substring(definition.Start, definition.Length));
            var slot = component.Slots["detail"].Signatures.Single().Single().PropertyDefinitions!["label"];
            Assert.AreEqual(vueFile, slot.Path);
            Assert.AreEqual("label", vue.Substring(slot.Start, slot.Length));
            var model = component.Props["modelValue"].Definition!;
            Assert.AreEqual(vueFile, model.Path);
            Assert.AreEqual("model", vue.Substring(model.Start, model.Length));
            var compilation = await JoltProjectCompiler.CompileAsync(workspace.Sources, workspace.Options with { VueSourceRootRelative = ".." });
            Assert.IsEmpty(compilation.Diagnostics, string.Join(Environment.NewLine, compilation.Diagnostics));
            var js = compilation.Modules.Single().Content;
            StringAssert.Contains(js, "../Widgets/Panel.vue");
            StringAssert.Contains(js, "onUpdate:modelValue");
            StringAssert.Contains(js, "onSelect");

            var cached = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.AreSame(component.Props, cached.Options.VueComponents.Single().Props);

            var shiftedVue = "<!-- shifted -->\n" + vue;
            overlays[vueFile] = shiftedVue;
            var shifted = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            var shiftedSlot = shifted.Options.VueComponents.Single().Slots["detail"].Signatures.Single().Single()
                .PropertyDefinitions!["label"];
            Assert.AreEqual(vueFile, shiftedSlot.Path);
            Assert.AreEqual("label", shiftedVue.Substring(shiftedSlot.Start, shiftedSlot.Length));

            overlays.Remove(typeFile);
            var changed = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            var invalid = JoltProjectCompiler.Analyze(changed.Sources, changed.Options);
            var diagnostic = invalid.Diagnostics.Single();
            Assert.AreEqual("Page.jazor", diagnostic.Location.GetLineSpan().Path);
            Assert.AreEqual("hello", page.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));

            const string imported = """
                <script setup lang="ts">
                import Contract from '../Contract.jazor'
                defineProps<{ caption: NonNullable<InstanceType<typeof Contract>['$props']['Caption']> }>()
                </script>
                <template><div /></template>
                """;
            overlays[vueFile] = imported;
            var source = Path.Combine(root.FullName, "Contract.jazor");
            await File.WriteAllTextAsync(source, "<div />\n@code { [Parameter] public double Caption { get; set; } }");
            var clean = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.IsEmpty(clean.Diagnostics, string.Join(Environment.NewLine, clean.Diagnostics));
            Assert.AreEqual("number", clean.Options.VueComponents.Single().Props["caption"].Type);
            Assert.IsFalse(Directory.Exists(Path.Combine(root.FullName, ".jolt")));
            overlays[source] = "<div />\n@code { [Parameter] public string Caption { get; set; } = \"\"; }";
            var resolved = await JoltProjectLoader.LoadAsync(root.FullName, provider, overlays);
            Assert.AreEqual("string", resolved.Options.VueComponents.Single().Props["caption"].Type);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
