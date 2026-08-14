using System.Runtime.CompilerServices;
using System.Text;
using DenoHost.Core;

namespace ECMAScript.VueDevtoolsTest;

[TestClass]
public sealed class EcmaScriptVueDevtoolsRuntimeTests
{
    [TestMethod]
    public async Task VueDevtoolsBinding_UsesOfficialBrowserBundleWithMinimalHookBridge()
    {
        var script = await DevtoolsTestCompiler.ConvertModuleAsync(RuntimeModuleSource, "DevtoolsRuntimeModule");
        Assert.IsNotNull(script);

        var root = Path.Combine(Path.GetTempPath(), "jazor-vue-devtools-runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await WriteUtf8Async(Path.Combine(root, "devtools-binding.mjs"), script!);
            await CopyRuntimeAssetAsync(root, "vue-devtools-api.esm-browser.js");
            await CopyRuntimeAssetAsync(root, "perfect-debounce.mjs");
            await WriteUtf8Async(
                Path.Combine(root, "deno.json"),
                """
                {
                  "imports": {
                    "@vue/devtools-api": "./vendor/vue-devtools-api.esm-browser.js",
                    "perfect-debounce": "./vendor/perfect-debounce.mjs"
                  }
                }
                """);
            await WriteUtf8Async(Path.Combine(root, "devtools-runtime.test.mjs"), RuntimeSmokeSource);

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Deno.Execute(
                new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--config", Path.Combine(root, "deno.json"), "--quiet", "--allow-read", Path.Combine(root, "devtools-runtime.test.mjs")],
                timeout.Token);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static async Task CopyRuntimeAssetAsync(string root, string assetName)
    {
        var repoRoot = FindRepositoryRoot();
        var source = Path.Combine(repoRoot, "src", "ECMAScript.Vue", "dist", "devtools-api", assetName);
        Assert.IsTrue(File.Exists(source), $"Vendored Vue Devtools asset was not found: {source}");

        var target = Path.Combine(root, "vendor", assetName);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await using var input = File.OpenRead(source);
        await using var output = File.Create(target);
        await input.CopyToAsync(output);
    }

    private static Task WriteUtf8Async(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string FindRepositoryRoot([CallerFilePath] string sourceFilePath = "")
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath)!);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate repository root containing Jazor.slnx.");
    }

    private const string RuntimeModuleSource = """
        using System.ComponentModel;
        using ECMAScript;
        using static ECMAScript.VueDevtools;

        namespace RuntimeDemo
        {
            [ECMAScript]
            [Description("@#")]
            public sealed record RuntimeTimelineData : Vue.VueProps
            {
                public string Name { get; init; } = "";
            }

            [ECMAScript]
            [Description("@#")]
            public sealed record RuntimeTimelineMeta : Vue.VueProps
            {
                public int Sequence { get; init; }
            }

            [ECMAScriptModule("devtools-binding.mjs")]
            public static class DevtoolsRuntimeModule
            {
                public static bool Connected;
                public static bool ClientConnected;

                public static void Register(Vue.VueApp app)
                {
                    SetupPlugin(new PluginDescriptor
                    {
                        Id = "runtime.plugin",
                        Label = "Runtime plugin",
                        App = app,
                        Settings = new PluginSettings
                        {
                            { "verbose", BooleanPluginSetting.Create(new BooleanPluginSettingOptions { Label = "Verbose", DefaultValue = true }) }
                        }
                    }, api =>
                    {
                        api.AddInspector(new InspectorOptions
                        {
                            Id = "runtime.inspector",
                            Label = "Runtime inspector"
                        });
                        api.AddTimelineLayer<RuntimeTimelineData, RuntimeTimelineMeta>(new TimelineLayerOptions<RuntimeTimelineData, RuntimeTimelineMeta>
                        {
                            Id = "runtime.timeline",
                            Label = "Runtime timeline",
                            Color = 3368601
                        });
                        api.AddTimelineEvent<RuntimeTimelineData, RuntimeTimelineMeta>(new TimelineEventOptions<RuntimeTimelineData, RuntimeTimelineMeta>
                        {
                            LayerId = "runtime.timeline",
                            Event = new TimelineEvent<RuntimeTimelineData, RuntimeTimelineMeta>
                            {
                                Time = api.Now(),
                                Data = new RuntimeTimelineData { Name = "registered" },
                                Meta = new RuntimeTimelineMeta { Sequence = 7 }
                            }
                        });
                        api.On.GetInspectorTree(payload =>
                        {
                            payload.RootNodes = new Array<InspectorNode>(new InspectorNode
                            {
                                Id = "runtime.root",
                                Label = "Runtime root"
                            });
                        });
                        api.On.GetInspectorState(payload =>
                        {
                            payload.State = new InspectorState
                            {
                                { "runtime", new[] { new InspectorStateEntry { Key = "ready", Value = true } } }
                            };
                        });
                    });

                    AddCustomTab(new CustomTab
                    {
                        Name = "runtime.tab",
                        Title = "Runtime tab",
                        View = IframeView.Create(new IframeViewOptions
                        {
                            Src = "https://example.test/runtime",
                            Persistent = true
                        })
                    });
                    AddCustomCommand(new CustomCommand
                    {
                        Id = "runtime.command",
                        Title = "Runtime command",
                        Action = CustomCommandUrlAction.Create(new CustomCommandUrlActionOptions
                        {
                            Src = "https://example.test/runtime-command"
                        })
                    });
                    OnDevToolsConnected(() => Connected = true);
                    OnDevToolsClientConnected(() => ClientConnected = true);
                }

                public static bool IsReady()
                    => Connected && ClientConnected;

                public static void RemoveCommand()
                    => RemoveCustomCommand("runtime.command");
            }
        }
        """;

    private const string RuntimeSmokeSource = """
        const storage = new Map();
        globalThis.localStorage = {
          getItem(key) {
            return storage.has(key) ? storage.get(key) : null;
          },
          setItem(key, value) {
            storage.set(key, String(value));
          }
        };
        globalThis.__VUE_DEVTOOLS_KIT_GLOBAL_STATE__ = {
          connected: true,
          clientConnected: true,
          highPerfModeEnabled: false,
          timelineLayersState: { "runtime.plugin": true }
        };

        const capture = {
          inspectors: [],
          layers: [],
          events: [],
          tree: null,
          state: null,
          descriptor: null
        };
        const api = {
          on: {
            getInspectorTree(handler) {
              const payload = { rootNodes: [] };
              handler(payload);
              capture.tree = payload.rootNodes;
            },
            getInspectorState(handler) {
              const payload = { state: null };
              handler(payload);
              capture.state = payload.state;
            }
          },
          addInspector(options) {
            capture.inspectors.push(options);
          },
          addTimelineLayer(options) {
            capture.layers.push(options);
          },
          addTimelineEvent(options) {
            capture.events.push(options);
          },
          now() {
            return 4242;
          }
        };
        globalThis.__VUE_DEVTOOLS_HOOK = {
          callHook(event, descriptor, setup) {
            if (event !== "devtools-plugin:setup")
              throw new Error(`Unexpected Devtools hook: ${event}`);
            capture.descriptor = descriptor;
            setup(api);
          }
        };

        Deno.test("generated Devtools binding calls the official browser API through its hook bridge", async () => {
          const binding = await import("./devtools-binding.mjs");
          binding.Register({ name: "runtime-app" });

          if (!binding.IsReady())
            throw new Error("connected callbacks were not run through @vue/devtools-api");
          if (capture.descriptor?.id !== "runtime.plugin" || capture.descriptor?.settings?.verbose?.type !== "boolean")
            throw new Error(`plugin descriptor was not preserved: ${JSON.stringify(capture.descriptor)}`);
          if (capture.inspectors.length !== 1 || capture.inspectors[0].id !== "runtime.inspector")
            throw new Error(`inspector registration was not forwarded: ${JSON.stringify(capture.inspectors)}`);
          if (capture.layers.length !== 1 || capture.layers[0].id !== "runtime.timeline")
            throw new Error(`timeline layer was not forwarded: ${JSON.stringify(capture.layers)}`);
          if (capture.events.length !== 1 || capture.events[0].event.time !== 4242 || capture.events[0].event.meta.Sequence !== 7)
            throw new Error(`timeline event was not forwarded: ${JSON.stringify(capture.events)}`);
          if (capture.tree?.[0]?.id !== "runtime.root" || capture.state?.runtime?.[0]?.value !== true)
            throw new Error(`inspector hook payload was not updated: ${JSON.stringify({ tree: capture.tree, state: capture.state })}`);

          const tabs = globalThis.__VUE_DEVTOOLS_KIT_CUSTOM_TABS__;
          const commands = globalThis.__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__;
          if (tabs?.[0]?.view?.type !== "iframe" || tabs[0].view.src !== "https://example.test/runtime")
            throw new Error(`custom tab was not registered: ${JSON.stringify(tabs)}`);
          if (commands?.[0]?.action?.type !== "url")
            throw new Error(`custom command was not registered: ${JSON.stringify(commands)}`);

          binding.RemoveCommand();
          if (globalThis.__VUE_DEVTOOLS_KIT_CUSTOM_COMMANDS__.length !== 0)
            throw new Error("custom command removal was not forwarded");
        });
        """;
}
