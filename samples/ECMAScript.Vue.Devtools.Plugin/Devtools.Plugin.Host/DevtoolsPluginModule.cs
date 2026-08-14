using System.ComponentModel;
using ECMAScript;
using static ECMAScript.VueDevtools;

namespace Devtools.Plugin.Host;

[ECMAScript]
[Description("@#")]
public sealed record DiagnosticsSettings : Vue.VueProps
{
    public bool Verbose { get; init; }

    public string Theme { get; init; } = "system";
}

[ECMAScript]
[Description("@#")]
public sealed record DiagnosticsTimelineData : Vue.VueProps
{
    public string Message { get; init; } = "";
}

[ECMAScript]
[Description("@#")]
public sealed record DiagnosticsTimelineMeta : Vue.VueProps
{
    public int Sequence { get; init; }
}

/// <summary>
/// Sample plugin module. 它演示 settings、custom inspector 和 timeline 的最小组合，
/// 业务宿主应在创建同一个 Vue app 后调用 <see cref="Install"/>。
/// </summary>
[ECMAScriptModule("devtools/plugin.mjs")]
public static class DevtoolsPluginModule
{
    public static void Install(Vue.VueApp app)
    {
        SetupPlugin<DiagnosticsSettings>(new PluginDescriptor<DiagnosticsSettings>
        {
            Id = "jazor.sample.diagnostics",
            Label = "Jazor diagnostics",
            App = app,
            EnableEarlyProxy = true,
            Settings = new PluginSettings
            {
                {
                    "verbose",
                    BooleanPluginSetting.Create(new BooleanPluginSettingOptions
                    {
                        Label = "Verbose timeline",
                        DefaultValue = false
                    })
                },
                {
                    "theme",
                    TextPluginSetting.Create(new TextPluginSettingOptions
                    {
                        Label = "Theme",
                        DefaultValue = "system"
                    })
                }
            }
        }, api =>
        {
            api.AddInspector(new InspectorOptions
            {
                Id = "jazor.sample.diagnostics.inspector",
                Label = "Diagnostics"
            });
            api.AddTimelineLayer<DiagnosticsTimelineData, DiagnosticsTimelineMeta>(
                new TimelineLayerOptions<DiagnosticsTimelineData, DiagnosticsTimelineMeta>
                {
                    Id = "jazor.sample.diagnostics.timeline",
                    Label = "Diagnostics",
                    Color = 0x4F8BFF
                });

            // API 生命周期由 Devtools 管理；keep callback work scoped to the provided handle.
            api.On.GetInspectorTree(payload =>
            {
                payload.RootNodes = new Array<InspectorNode>(new InspectorNode
                {
                    Id = "summary",
                    Label = "Diagnostics"
                });
            });
            api.On.GetInspectorState(payload =>
            {
                payload.State = new InspectorState
                {
                    {
                        "settings",
                        new[]
                        {
                            new InspectorStateEntry
                            {
                                Key = "theme",
                                Value = api.GetSettings().Theme,
                                Editable = true
                            }
                        }
                    }
                };
            });
            api.On.SetPluginSettings<DiagnosticsSettings>(_ =>
            {
                api.SendInspectorState("jazor.sample.diagnostics.inspector");
            });

            api.AddTimelineEvent<DiagnosticsTimelineData, DiagnosticsTimelineMeta>(
                new TimelineEventOptions<DiagnosticsTimelineData, DiagnosticsTimelineMeta>
                {
                    LayerId = "jazor.sample.diagnostics.timeline",
                    Event = new TimelineEvent<DiagnosticsTimelineData, DiagnosticsTimelineMeta>
                    {
                        Time = api.Now(),
                        Title = "Plugin installed",
                        Data = new DiagnosticsTimelineData { Message = "ready" },
                        Meta = new DiagnosticsTimelineMeta { Sequence = 1 }
                    }
                });
        });
    }
}
