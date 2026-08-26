# ECMAScript.Vue.Devtools

> 定位：`@vue/devtools-api` 8.1.5 的独立强类型 C# binding，用于 author custom Vue Devtools plugin，而不是浏览器扩展或 Devtools 内部实现的封装。

## 安装

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.23.0" />
  <PackageReference Include="Jazor.Vue" Version="0.23.0" />
  <PackageReference Include="ECMAScript.Vue.Devtools" Version="0.23.0" />
</ItemGroup>
```

所有 Jazor、`Jazor.Vue` 和 `ECMAScript.*` 包应使用相同版本。`Jazor.Vue` 提供 Vue、`@vue/devtools-api` 与 `perfect-debounce` 的本地 runtime closure；本包不会重复携带 manifest、browser ESM 或 Devtools extension internals。

## 使用

将 plugin 注册放在创建同一个 `VueApp` 的模块中。`PluginApi` 由 Devtools callback 提供，不能自行构造；`Id` 需要在 reload 之间保持稳定。

```csharp
using System.ComponentModel;
using ECMAScript;
using static ECMAScript.VueDevtools;

[ECMAScript]
[Description("@#")]
public sealed record DiagnosticsSettings : Vue.VueProps
{
    public bool Verbose { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record TimelineData : Vue.VueProps
{
    public string Message { get; init; } = "";
}

[ECMAScriptModule("devtools/diagnostics.mjs")]
public static class DiagnosticsDevtools
{
    public static void Install(Vue.VueApp app)
    {
        SetupPlugin<DiagnosticsSettings>(new PluginDescriptor<DiagnosticsSettings>
        {
            Id = "sample.diagnostics",
            Label = "Diagnostics",
            App = app,
            Settings = new PluginSettings
            {
                { "verbose", BooleanPluginSetting.Create(new BooleanPluginSettingOptions
                    { Label = "Verbose timeline", DefaultValue = false }) }
            }
        }, api =>
        {
            api.AddInspector(new InspectorOptions
            {
                Id = "sample.diagnostics.inspector",
                Label = "Diagnostics"
            });
            api.AddTimelineLayer<TimelineData, Vue.VueProps>(new TimelineLayerOptions<TimelineData, Vue.VueProps>
            {
                Id = "sample.diagnostics.timeline",
                Label = "Diagnostics",
                Color = 0x4F8BFF
            });
            api.On.GetInspectorTree(payload =>
            {
                payload.RootNodes = new Array<InspectorNode>(new InspectorNode
                {
                    Id = "root",
                    Label = "Application"
                });
            });
            api.AddTimelineEvent<TimelineData, Vue.VueProps>(new TimelineEventOptions<TimelineData, Vue.VueProps>
            {
                LayerId = "sample.diagnostics.timeline",
                Event = new TimelineEvent<TimelineData, Vue.VueProps>
                {
                    Time = api.Now(),
                    Title = "Plugin installed",
                    Data = new TimelineData { Message = "ready" }
                }
            });
        });
    }
}
```

`PluginSetting`、custom tab view 与 custom command action 是带 discriminator 的结果类型。请使用 `BooleanPluginSetting.Create(...)`、`IframeView.Create(...)`、`CustomCommandUrlAction.Create(...)` 等 factory，让 emitted object 始终带有官方 `type` literal；不要依赖 C# property default 初始化器写入 JavaScript structural object。

## 覆盖范围

- `setupDevToolsPlugin()`、plugin descriptor 与 typed settings。
- custom inspector、tree/state lifecycle、state editing 与 inspector action。
- component tree/state hooks、component query/highlight 与 update notification。
- timeline layer/event、timeline inspection、screenshot overlay 与 clear hook。
- custom tab、command palette、Devtools/Devtools-client connection callback。

## 边界

- 本包仅绑定 `@vue/devtools-api` 的 public plugin authoring surface；不暴露 `@vue/devtools-kit`、浏览器 extension、Vite/Electron RPC 或 private Devtools runtime shape。
- Pinia 4 在 `app.Use(pinia)` 时会自行注册其开发面板。不要为已有 Pinia panel 再手工调用 `SetupPlugin(...)`。
- Devtools 可以不存在于生产环境或用户浏览器。官方 API 会处理未连接状态；application business logic 不应依赖 plugin callback 一定执行。

## 示例与验证

- [ECMAScript.Vue.Devtools.Plugin sample](../../samples/ECMAScript.Vue.Devtools.Plugin/README.md)
- `dotnet run --file scripts/csharp/test-dotnet.cs -- --project devtools`
- `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs`

## 相关文档

- [ECMAScript.Vue](../ECMAScript.Vue/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [安装与配置](../../docs/03-guides/installation-and-configuration.md)
