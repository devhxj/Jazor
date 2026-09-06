# RazorVue 诊断矩阵

本文把 RazorVue 自有诊断按作者遇到的场景排列。Razor SDK/Roslyn 的 `RZ****`、`CS****` 诊断仍由 SDK 报告；下表只记录 RazorVue compatibility analyzer 和 final Compilation generation boundary 的稳定 ID。

## 组件和生成链

| ID | 触发场景 | 最小替代写法 | 进一步阅读 |
| --- | --- | --- | --- |
| `JAZORVGA020` | 未分类的 final Compilation 输出失败 | 保留完整构建日志和最小复现；不要继续消费旧 artifact | [Final Compilation](./razorvue-authoring.md#final-compilation) |
| `JAZORVGA021` | direct RenderTree frame、内置 Blazor UI 或不支持的 render shape | 使用普通 Razor 标记、已声明 binding 组件和完整 frame 结构 | [Direct render](./razorvue-authoring.md#direct-render) |
| `JAZORVGA022` | 表达式无法通过 `Jazor.Compiler`/whitelist lowering | 改用已映射的 ECMAScript/CLR contract 或在 endpoint 预先整理 DTO | [Compiler boundary](./razorvue-authoring.md#compiler-boundary) |
| `JAZORVGA023` | 组件参数、slot 或事件无法绑定 | 对照 binding 的具体参数类型、`XxxValue`/`XxxContent` 命名和 `EventCallback` 签名 | [Component binding](./razorvue-authoring.md#component-binding) |
| `JAZORVGA024` | 可达成员、constructor、lifecycle 或导出成员无法进入 module closure | 将逻辑放进受支持的 `.razor.cs` 成员，使用显式 import 和可激活的 writable property | [Member closure](./razorvue-authoring.md#member-closure) |
| `JAZORVGA025` | `[VueInject]` 声明缺少或重复 implementation contract | 修正 container、implementation 和 provider key；不要增加第二套注入协议 | [VueInject](./razorvue-authoring.md#vue-inject) |
| `JAZORVGA026` | module/export/import 或 Vue framing 无法生成 | 检查 `[ECMAScript]` 模块路径、export 名和 import 冲突 | [Vue module](./razorvue-authoring.md#vue-module) |

## 浏览器服务和生命周期

| ID | 触发场景 | 最小替代写法 | 进一步阅读 |
| --- | --- | --- | --- |
| `JAZORVCA001` | 注入 `DbContext` 或派生类型 | endpoint 访问数据库，组件注入强类型 browser client | [Browser services](./razorvue-authoring.md#browser-services) |
| `JAZORVCA002` | 注入 `HttpContext`、Identity manager 或其他 server-only service | 把 request/identity 操作移到 endpoint；为浏览器暴露明确 DTO | [Browser services](./razorvue-authoring.md#browser-services) |
| `JAZORVCA003` | 调用 `ParameterView.TryGetValue` | 声明具体 `[Parameter]` 属性并直接读取它 | [Parameter lifecycle](./razorvue-authoring.md#parameter-lifecycle) |
| `JAZORVCA004` | 枚举 `ParameterView` | 用已知的 typed 参数属性替代运行时参数包枚举 | [Parameter lifecycle](./razorvue-authoring.md#parameter-lifecycle) |
| `JAZORVCA005` | 调用 `ParameterView.ToDictionary` | 从已知 typed 值显式构造字典 | [Parameter lifecycle](./razorvue-authoring.md#parameter-lifecycle) |
| `JAZORVCA006` | `[Inject]` 属性不是 writable auto-property | 使用 `ServiceType Service { get; set; } = null!;` | [Browser services](./razorvue-authoring.md#browser-services) |
| `JAZORVCA007` | Blazor host service 没有 RazorVue browser adapter | 注册 typed adapter，或把操作移到 endpoint | [Browser services](./razorvue-authoring.md#browser-services) |
| `JAZORVCA008` | `[CascadingParameter]` 无法由 adapter 激活 | 使用 writable auto-property；保持标准 `CascadingValue` 写法 | [Cascading parameters](./razorvue-authoring.md#cascading-parameters) |
| `JAZORVCA009` | 使用未注册 route host 的 `@page` profile | 注册应用自有 route host；标准 `@page` 在推荐 profile 中无需额外 adapter | [Routing](./razorvue-authoring.md#routing) |
| `JAZORVCA010` | 使用 Microsoft/Blazor 内置 UI 组件 | 使用自定义 `ComponentBase + IVueComponent` 或已声明 UI binding | [Component adapters](./razorvue-authoring.md#component-adapters) |
| `JAZORVCA011` | 使用未定义协议的 `PersistentComponentState`、`[PersistentState]` 或 form handoff | 使用版本化 typed endpoint/bootstrap payload | [SSR state handoff](./razorvue-authoring.md#ssr-state-handoff) |
| `JAZORVCA012` | 使用 `[StreamRendering]`，但当前 profile 没有 renderer-owned streaming SSR 协议 | 使用显式 typed SSR/bootstrap contract，并自行表达加载状态 | [SSR state handoff](./razorvue-authoring.md#ssr-state-handoff) |

## 诊断验收规则

每条自有诊断必须稳定满足：

1. ID、severity 和 HelpLink 不随并行构建或消息细节变化；
2. primary location 优先映射回 `.razor`/`.razor.cs`，而不是只指向 generated `.razor.g.cs`；
3. 消息说明失败域、原因和一条不扩大产品边界的替代路径；
4. 构建失败后不生成本轮的 `ModuleCatalog`、`.mjs`、`.mjs.map` 或 bundle；
5. 同一输入重复构建时诊断顺序和文本稳定。

链路证据可用以下脚本检查：

```text
dotnet run --file scripts/csharp/inspect-razorvue-chain.cs -- --source Page.razor --generated Page.razor.g.cs --artifact page.mjs --map page.mjs.map --json
```

脚本在缺文件、source map 未包含源文件或 artifact 没有 source map 引用时返回非零退出码。
