# ECMAScript.Vue.Devtools.Plugin

> 定位：独立 Devtools binding 的最小 package-consumer sample，展示 typed settings、custom inspector 与 timeline，不保存或修改任何 tracked `jazor/` fixture。

## 结构

- `Devtools.Plugin.Host`：编译出 `devtools/plugin.mjs`，由实际 Vue host 在创建 app 后调用 `DevtoolsPluginModule.Install(app)`。
- `build-local.cs`：将当前工作区的 `Jazor` 和 `ECMAScript.Vue.Devtools` 打包到隔离目录，再以 NuGet consumer 方式构建 host。

## 构建

在仓库根目录执行：

```bash
dotnet run --file samples/ECMAScript.Vue.Devtools.Plugin/build-local.cs
```

脚本将生成模块、包缓存和本地 nupkg 写入 `.tmp/`，不会改动示例目录中的固定产物。生成结果展示：

- `setupDevToolsPlugin()` 的 descriptor 与 `enableEarlyProxy`。
- `BooleanPluginSetting.Create(...)` / `TextPluginSetting.Create(...)` 的稳定 `type` discriminator。
- inspector tree/state hook、typed settings update 与 `sendInspectorState()`。
- typed timeline layer/event 和 Devtools 高精度时间戳。

## 相关文档

- [ECMAScript.Vue.Devtools](../../src/ECMAScript.Vue.Devtools/README.md)
- [示例总览](../../docs/03-guides/examples.md)
