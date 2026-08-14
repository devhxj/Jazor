using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ECMAScript.VueDevtoolsTest;

[TestClass]
public sealed class EcmaScriptVueDevtoolsImportTests
{
    [TestMethod]
    public async Task Analyze_OpaqueInlineFactoryFromMetadata_IsAcceptedAsRecordProxy()
    {
        var compilation = DevtoolsTestCompiler.CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.VueDevtools;

            namespace Demo
            {
                [ECMAScriptModule("devtools/analyzer.mjs")]
                public static class DevtoolsAnalyzerModule
                {
                    public static void Configure()
                    {
                        var setting = BooleanPluginSetting.Create(new BooleanPluginSettingOptions
                        {
                            Label = "Verbose",
                            DefaultValue = true
                        });
                    }
                }
            }
            """);

        var diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new Jazor.Analyzer.Analyzer()))
            .GetAnalyzerDiagnosticsAsync();

        Assert.IsFalse(
            diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));
    }

    [TestMethod]
    public async Task Convert_PluginSetupInspectorAndTimeline_GeneratesOfficialDevtoolsCalls()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.VueDevtools;

            namespace Demo
            {
                [ECMAScript]
                [Description("@#")]
                public sealed record DevtoolsSettings : Vue.VueProps
                {
                    public bool Verbose { get; init; }

                    public string Theme { get; init; } = "";
                }

                [ECMAScript]
                [Description("@#")]
                public sealed record TimelineData : Vue.VueProps
                {
                    public string Name { get; init; } = "";
                }

                [ECMAScript]
                [Description("@#")]
                public sealed record TimelineMeta : Vue.VueProps
                {
                    public int Sequence { get; init; }
                }

                [ECMAScriptModule("devtools/plugin.mjs")]
                public static class DevtoolsPluginModule
                {
                    public static void Register(Vue.VueApp app)
                    {
                        SetupPlugin<DevtoolsSettings>(new PluginDescriptor<DevtoolsSettings>
                        {
                            Id = "demo.devtools",
                            Label = "Demo Devtools",
                            App = app,
                            EnableEarlyProxy = true,
                            Settings = new PluginSettings
                            {
                                { "verbose", BooleanPluginSetting.Create(new BooleanPluginSettingOptions { Label = "Verbose", DefaultValue = true }) },
                                { "theme", TextPluginSetting.Create(new TextPluginSettingOptions { Label = "Theme", DefaultValue = "system" }) }
                            }
                        }, api =>
                        {
                            api.AddInspector(new InspectorOptions
                            {
                                Id = "demo.inspector",
                                Label = "Demo Inspector"
                            });
                            api.AddTimelineLayer<TimelineData, TimelineMeta>(new TimelineLayerOptions<TimelineData, TimelineMeta>
                            {
                                Id = "demo.timeline",
                                Label = "Demo Timeline",
                                Color = 65280
                            });
                            api.AddTimelineEvent<TimelineData, TimelineMeta>(new TimelineEventOptions<TimelineData, TimelineMeta>
                            {
                                LayerId = "demo.timeline",
                                Event = new TimelineEvent<TimelineData, TimelineMeta>
                                {
                                    Time = api.Now(),
                                    Data = new TimelineData { Name = "opened" },
                                    Meta = new TimelineMeta { Sequence = 1 },
                                    Title = "Opened"
                                }
                            });
                            api.On.GetInspectorTree(payload =>
                            {
                                payload.RootNodes = new Array<InspectorNode>(new InspectorNode
                                {
                                    Id = "root",
                                    Label = "Root"
                                });
                            });
                            api.On.GetInspectorState(payload =>
                            {
                                payload.State = new InspectorState
                                {
                                    { "settings", new[] { new InspectorStateEntry { Key = "theme", Value = "system", Editable = true } } }
                                };
                            });
                            api.On.VisitComponentTree(payload =>
                            {
                                payload.TreeNode.Tags.Push(new InspectorNodeTag
                                {
                                    Label = "tracked",
                                    TextColor = 0,
                                    BackgroundColor = 16777215
                                });
                            });
                            api.On.InspectComponent(payload =>
                            {
                                payload.InstanceData.State.Push(new ComponentStateEntry
                                {
                                    Key = "theme",
                                    Type = "devtools",
                                    Value = "system"
                                });
                            });
                            api.On.EditInspectorState(payload =>
                            {
                                payload.Set(new DevtoolsSettings { Theme = "system" }, payload.Path, "dark");
                            });
                            api.On.InspectTimelineEvent<TimelineData, TimelineMeta>(payload =>
                            {
                                payload.Data = new TimelineData { Name = "inspected" };
                            });
                            api.On.SetPluginSettings<DevtoolsSettings>(payload =>
                            {
                                var current = payload.Settings.Theme;
                            });
                            api.GetSettings();
                            api.GetSettings("other.plugin");
                            api.GetSettings<DevtoolsSettings>();
                            api.GetSettings<DevtoolsSettings>("other.plugin");
                        });
                    }
                }
            }
            """;

        var script = await DevtoolsTestCompiler.ConvertModuleAsync(code, "DevtoolsPluginModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "import { setupDevToolsPlugin } from \"@vue/devtools-api\";");
        StringAssert.Contains(script, "setupDevToolsPlugin({");
        StringAssert.Contains(script, "id: \"demo.devtools\"");
        StringAssert.Contains(script, "enableEarlyProxy: true");
        StringAssert.Contains(script, "Object.assign({ type: \"boolean\" }, { label: \"Verbose\", defaultValue: true })");
        StringAssert.Contains(script, "Object.assign({ type: \"text\" }, { label: \"Theme\", defaultValue: \"system\" })");
        StringAssert.Contains(script, "api.addInspector({ id: \"demo.inspector\", label: \"Demo Inspector\" });");
        StringAssert.Contains(script, "api.addTimelineLayer({");
        StringAssert.Contains(script, "id: \"demo.timeline\"");
        StringAssert.Contains(script, "color: 65280");
        StringAssert.Contains(script, "api.addTimelineEvent({ layerId: \"demo.timeline\"");
        StringAssert.Contains(script, "api.on.getInspectorTree");
        StringAssert.Contains(script, "api.on.getInspectorState");
        StringAssert.Contains(script, "api.on.visitComponentTree");
        StringAssert.Contains(script, "api.on.inspectComponent");
        StringAssert.Contains(script, "api.on.editInspectorState");
        StringAssert.Contains(script, "api.on.inspectTimelineEvent");
        StringAssert.Contains(script, "api.on.setPluginSettings");
        StringAssert.Contains(script, "payload.set({ Theme: \"system\" }, payload.path, \"dark\");");
        StringAssert.Contains(script, "api.getSettings();");
        StringAssert.Contains(script, "api.getSettings(\"other.plugin\");");
    }

    [TestMethod]
    public async Task Convert_CustomTabsCommandsAndConnectionCallbacks_GenerateDevtoolsImports()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.VueDevtools;

            namespace Demo
            {
                [ECMAScriptModule("devtools/shell.mjs")]
                public static class DevtoolsShellModule
                {
                    public static void Configure()
                    {
                        AddCustomTab(new CustomTab
                        {
                            Name = "demo.tab",
                            Title = "Demo tab",
                            Category = TabCategory.Pinned,
                            View = IframeView.Create(new IframeViewOptions
                            {
                                Src = "https://example.test/devtools",
                                Persistent = true
                            })
                        });
                        AddCustomCommand(new CustomCommand
                        {
                            Id = "demo.command",
                            Title = "Open demo",
                            Action = CustomCommandUrlAction.Create(new CustomCommandUrlActionOptions
                            {
                                Src = "https://example.test/open"
                            }),
                            Children = new[]
                            {
                                new CustomCommandChild
                                {
                                    Id = "demo.command.child",
                                    Title = "Open child",
                                    Action = CustomCommandUrlAction.Create(new CustomCommandUrlActionOptions
                                    {
                                        Src = "https://example.test/child"
                                    })
                                }
                            }
                        });
                        RemoveCustomCommand("demo.command");
                        OnDevToolsConnected(() => { });
                        OnDevToolsClientConnected(() => { });
                    }
                }
            }
            """;

        var script = await DevtoolsTestCompiler.ConvertModuleAsync(code, "DevtoolsShellModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "import { addCustomCommand, addCustomTab, onDevToolsClientConnected, onDevToolsConnected, removeCustomCommand } from \"@vue/devtools-api\";");
        StringAssert.Contains(script, "addCustomTab({");
        StringAssert.Contains(script, "name: \"demo.tab\"");
        StringAssert.Contains(script, "Object.assign({ type: \"iframe\" }, { src: \"https://example.test/devtools\", persistent: true })");
        StringAssert.Contains(script, "category: \"pinned\"");
        StringAssert.Contains(script, "addCustomCommand({");
        StringAssert.Contains(script, "id: \"demo.command\"");
        StringAssert.Contains(script, "Object.assign({ type: \"url\" }, { src: \"https://example.test/open\" })");
        StringAssert.Contains(script, "children: [{");
        StringAssert.Contains(script, "id: \"demo.command.child\"");
        StringAssert.Contains(script, "removeCustomCommand(\"demo.command\");");
        StringAssert.Contains(script, "onDevToolsConnected(() => {");
        StringAssert.Contains(script, "onDevToolsClientConnected(() => {");
    }
}
