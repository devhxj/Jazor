# npm/JSR 包绑定指南

> 面向：为上游 JavaScript/JSR 库创建 `ECMAScript.<Name>` 强类型绑定的仓库维护者。绑定结果进入标准 `jazor/` 项目，由 Deno 2.9.7、NetPack 和 DenoHost 直接消费。

## 绑定交付模型

一个绑定包包含三部分相互对应的内容：

- C# 强类型 authoring contract；
- npm 或 JSR 的精确 package identity；
- `[ECMAScript]` 保存的标准 ESM specifier、export 形式和组件/普通导入形式。

运行时包由 npm 或 JSR 提供。绑定包中的 manifest、inventory、许可证和上游快照记录来源、版本、完整性与导出证据；这些资料帮助生成器和测试校验契约，项目运行时解析使用恢复后的标准 `package.json`。

ECMAScript 自有 MJS 属于源码库输入。`src/ECMAScript/clr/**` 写入 `jazor/clr/**`，其他 `ECMAScriptModule` 写入其声明的项目源码路径。源码模块使用相对 import，并与应用生成模块共同组成项目源码树。

## 上游锁定

生成器按固定版本读取 npm 或 JSR 包，并保存以下证据：

- package name、registry source、版本和 integrity；
- 上游 `package.json` 的 `exports`、conditions、`sideEffects`、dependencies 和 peerDependencies；
- 公开 ESM 入口与实际 export name；
- 许可证、上游文档来源、采集时间和 fingerprint；
- 组件样式、worker、字体、图片或 wasm 的标准 import/URL 关系。

生成器可以从 registry tarball 或本地缓存复现上游快照。快照用于契约检查与升级审阅；运行时依赖仍由项目根的 Deno restore 提供。

上游包形态决定入口选择：

| 上游形态 | 绑定入口 | 构建结果 |
| --- | --- | --- |
| 公开的细粒度 ESM 子路径 | 直接使用子路径 | NetPack 沿入口可达图裁剪 export、模块和资源 |
| 正式根入口承载完整运行时 | 使用根入口 | 入口的内部依赖和初始化顺序完整保留 |
| 条件导出提供 browser/deno/development/production | 保存公开 specifier | Deno 与 NetPack 按各自条件选择上游目标 |
| 高耦合组件库 | 使用正式组件或根入口 | 组件闭包随真实 import 保留 |

VueDraggable 等底层库可以直接绑定正式根入口。组件内部依赖由上游 import 表达，绑定维护者只需验证运行语义和入口 identity。

## 入口与映射

`[ECMAScript("<specifier>")]` 是生成 import 的真源。specifier 使用恢复后 package 可以解析的公开路径：

```csharp
[ECMAScript("tdesign-vue-next/es/button/index.mjs", Transform.Component, "Button")]
public sealed class TButton : IUIComponent
{
}

[ECMAScript("date-fns/addDays")]
public static class AddDays
{
}
```

映射规则如下：

| 声明 | 作用 |
| --- | --- |
| 类型级 `[ECMAScript]` | 为宿主类型提供默认入口 |
| 成员级 `[ECMAScript]` | 为需要拆分的函数、组件或常量提供更细入口，并优先于类型级入口 |
| `Transform.Component` | 生成 Vue 组件导入 |
| 普通导入形式 | 生成函数、composable、常量或模块导入 |
| `ExportName` | 指定 named export 或 default export |

绑定维护者优先选择上游公开的最细 ESM 入口。例如 TDesign Button 使用 `tdesign-vue-next/es/button/index.mjs`；该入口内部导入组件实现、共享 helper、Vue 和样式，标准构建会沿这些边保留所需闭包。上游只公开根入口时，绑定使用根入口并验证其完整运行时。

同一个 package key 贯穿 binding metadata、生成 import 和根 `package.json`：

| 来源 | 根 `package.json` | 生成 import |
| --- | --- | --- |
| npm | 精确 npm 版本，例如 `"date-fns": "4.4.0"` | `date-fns/addDays` |
| JSR | 精确 `jsr:` identity，例如 `"@std/path": "jsr:@std/path@..."` | `@std/path/...` |
| ECMAScript 源码 | 不增加 package dependency | `./` 或 `../` 相对路径 |

一个 dependency key 只对应一个 identity。冲突在 Deno restore 前由 Emit 报告，便于定位绑定包或项目引用。

## 样式和其他资源

上游模块已经 import CSS 时，绑定保留该标准边，NetPack 从包内 ESM 图继续解析。上游要求调用方显式导入样式时，在 binding metadata 中记录与入口对应的 CSS specifier，Emit 生成普通 side-effect import。

worker、字体、图片和 wasm 通过上游 ESM import、`new URL(..., import.meta.url)` 或源码项目的相对 URL 进入构建图。绑定记录入口和测试证据，NetPack 根据可达图输出资源。全局 stylesheet 由上游入口的公开 side-effect 关系保留；组件专属 stylesheet 随使用的组件入口进入输出。

## C# 契约

绑定类型位于对应的 `ECMAScript.<Name>` 命名空间，保持强类型的参数、返回值、事件、props、slots、enum 和 union。

- 函数库使用静态宿主类型和 named export；
- composable 使用泛型或专用宿主类型表达值域；
- 组件代理表达 props、events、slots 和透传属性；
- 开放的 JavaScript 值域使用有说明的宿主类型或泛型；
- 导出名差异通过显式 `[Description("@#...")]`、`[ECMAScriptName]` 或 `[ECMAScript]` metadata 表达。

组件数量较少时可以手写代理和 props/slots descriptor；组件数量较多时使用生成器从上游 contracts 或 web-types 生成代理和导出目录。两种形式都把同一个标准 ESM specifier 交给 Emit。

组件内部依赖属于上游运行时语义。代理类只承担 C# authoring contract，组件实现、共享 helper、Vue peer 和样式由上游 package 及标准项目图提供。

## Emit 接线

MSBuild 执行 Emit 时，绑定包参与以下步骤：

1. 提供实际使用的 package imports、dependency identities 与必要的 CSS side-effect specifier；
2. 让 Emit 把标准 bare import 写入项目源码；
3. 让 Emit 生成 `entry.mjs`、可选的 `ssr-entry.mjs` 和根 `package.json`；
4. 由 Deno 2.9.7 恢复 `node_modules`、生成或校验 `deno.lock` 并执行 frozen check；
5. 由 NetPack 从同一 `jazor/` 根读取上游 `exports`、conditions、`sideEffects` 和 ESM 资源图。

最终宿主统一生成项目入口、恢复依赖并构建 bundle，使源码 ProjectReference、NuGet consumer、浏览器构建与 SSR 共享同一标准项目。

## 生成器工作流

建议的单文件 C# 生成器位于 `scripts/csharp/`：

1. 接收 `--version <v>` 和可复现的本地上游缓存路径；
2. 校验 tarball integrity、package identity、exports 和实际 export；
3. 生成或更新 C# contract、binding metadata、inventory 和许可证；
4. 为每个入口写入精确标准 specifier，并记录必要的显式 CSS specifier；
5. 输出统计和 contract drift 报告；
6. 使用 `--check` 确认生成文件、上游快照、metadata 与 C# contract 一致。

生成器维护上游事实与 C# contract 的对应关系，Emit 维护最终项目的源码落位和依赖恢复。

## 测试与门禁

每个绑定包应覆盖以下证据：

1. package identity、版本、integrity、exports、conditions 和 peer dependency 与上游快照一致；
2. 每个 `[ECMAScript]` specifier 与上游公开入口和 export name 一致；
3. 生成源码中的 bare import 都能在 `jazor/package.json` 找到 dependency key；
4. 项目源码的相对 import、source map 和本地资源路径可解析；
5. Deno 2.9.7 restore、`deno.lock` frozen check 和无网络检查通过；
6. NetPack metafile 证明单组件/单函数入口只保留可达 JS、CSS、worker 和静态资源；
7. 组件内部依赖、共享模块、初始化顺序和必要 side effects 在真实 consumer 中保持；
8. SSR 入口复用同一 `node_modules`、`deno.lock` 和 package identity。

测试入口遵循仓库脚本策略：

```text
dotnet run --file scripts/csharp/test-dotnet.cs -- --project <binding-lane>
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

真实 RazorVue consumer 和适用的浏览器/SSR smoke 为绑定进入 Support 矩阵的最终证据。

## 包接线

绑定 NuGet 包携带：

- C# assembly 与 XML documentation；
- binding metadata、上游 inventory、许可证和生成器输入快照；
- `buildTransitive` locator，使最终宿主能够发现 binding declaration；
- README 与版本升级说明。

项目构建输出由最终宿主的 Emit 写入 `jazor/`。绑定包的版本与 Jazor 其他包保持 lockstep。

## 交付检查表

- 上游版本、来源、integrity、exports、sideEffects 和许可证已经冻结；
- 每个 public C# API 都有对应的标准 ESM specifier 和 export 证据；
- npm/JSR dependency identity 能合并到根 `package.json`；
- ECMAScript 源码 carrier 能按相对路径写入项目；
- Deno restore/check、NetPack tree shaking、CSS/worker/static 和 SSR consumer 均有测试；
- 文档、生成器、metadata、测试和 CHANGELOG 使用相同的 package identity 与入口。
