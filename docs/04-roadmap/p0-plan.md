# RazorVue P0 执行计划

> 本文是 RazorVue 下一阶段 P0 的实施指导文档。它把“开发者可以自然地写、失败时知道怎么改、发布时有证据、规模变大后仍可接受”拆成可独立验收的交付切片。计划不扩大 RazorVue 的支持边界；任何新能力仍须经过实现、测试、文档和真实消费者验证。

## 目标

P0 的结果不是增加一组零散语法，而是让一个没有阅读内部实现的开发者能够完成一条可复制的 RazorVue 开发闭环：

```text
新建项目
  -> 使用推荐模板写组件
  -> 看到清楚的 IDE/构建诊断
  -> 在 Debug/HMR 中迭代
  -> 用 Release/SSR consumer 验证
  -> 得到稳定、可追踪的浏览器产物
```

P0 包含四条相互衔接的工作线：

1. **Golden Path 与真实消费者**：以 TDesign 管理页面为主样本，证明组件、表单、表格、路由、状态和 API client 可以自然组合。
2. **诊断与修改反馈**：让错误从 `.razor`/`.razor.cs` 位置可读地指出边界、原因和最小替代写法。
3. **增量构建与性能基线**：先建立可重复的首次构建、增量构建、HMR 和产物体积测量，再实施有证据的优化。
4. **绑定生成与版本漂移门禁**：让 Vuetify、Element Plus、TDesign 及后续组件库共享同一套元数据、文档和升级检查规则。

## 交付顺序

顺序固定为“样本定义 → 诊断闭环 → 性能测量 → 生成规范”。后一个切片不能用来掩盖前一个切片的失败。

| 切片 | 交付物 | 主要责任层 | 退出条件 |
| --- | --- | --- | --- |
| P0-A Golden Path | 独立 authoring fixture、TDesign CRUD 页面、SPA/SSR Release consumer、浏览器 smoke | `samples/`、`Jazor.RazorVue`、`Jazor.Emit`、`Jazor.AspNetCore` | 新项目按指南完成页面；SG、模块运行时、Release package 和浏览器验证全部通过 |
| P0-B 诊断闭环 | 诊断矩阵、HelpLink 锚点、最小替代写法、诊断报告命令和无 partial artifact 回归 | `Jazor.RazorVue`、作者指南、scripts | 常见 Reject/Guidance 均有稳定 ID、源位置、原因和替代路径；错误构建不留下 catalog/module/bundle |
| P0-C 增量性能 | 固定输入和测量协议、首次/增量/HMR/Release 指标、优化前后对比记录 | `Jazor.RazorVue`、`Jazor.Compiler`、`Jazor.Emit` | 同一机器和参数可重复测量；优化保持求值顺序、source map、导入稳定性和输出语义 |
| P0-D 绑定门禁 | 统一 snapshot、文档、runtime export、contract diff 和 coverage 检查 | `ECMAScript.Vue.Generator`、各 binding、CI scripts | 上游升级能在生成或 CI 阶段报告组件/prop/event/slot/文档漂移；无 silent fallback |

## P0-A：Golden Path

### 推荐样本

样本使用当前已稳定的 typed TDesign contract，页面至少包含：

- 应用 layout 和两个同源 route；
- Pinia 或等价 typed browser state；
- 查询表格、loading/empty/error 状态、分页和行操作；
- 新增/编辑表单、校验规则、提交失败后保留草稿、成功后刷新；
- named slot、default slot、typed event callback、`@bind` 和一个 union prop；
- typed API client 注入；数据访问只经过 endpoint，不把 server-only service 带入组件；
- Debug/HMR、SPA Release 和 SSR Release 的同一页面验证。

### 完成步骤

1. 固定页面需求和交互断言，先写 authoring fixture，再实现页面。
2. 用官方 Razor Source Generator 编译 fixture，保存生成 C# 作为诊断和 source map 的输入证据。
3. 在 Deno/module runner 验证初始渲染、参数更新、表单提交和错误路径。
4. 在隔离 package consumer 中验证 Release 资源闭包、PathBase 和刷新路由。
5. 在真实 HTTP-origin browser 中验证 HMR、SPA 和适用 SSR/hydration 行为。
6. 将页面写法、支持约束和失败替代路径加入作者指南；不把样本中的临时 workaround 提升为公共 API。

### 验收记录

每次变更记录以下信息：commit、SDK/Node/浏览器版本、测试命令、退出码、生成物入口、关键交互断言和已知边界。样本通过后才能把对应写法从 Guidance 提升为 Support。

## P0-B：诊断闭环

### 诊断契约

每个 RazorVue 自有诊断必须包含：

- 稳定 ID 和 severity；
- 映射回 `.razor` 或作者 `.razor.cs` 的 primary location；
- 失败所属域（direct render、component logic、compiler、binding、module）；
- 触发的最小源码形状；
- 一句原因；
- 一条不扩大边界的替代写法；
- 对应作者指南 HelpLink。

Razor SDK/Roslyn 的 `RZ****`/`CS****` 仍由 SDK 报告，RazorVue 不复制同一检查。最终 Compilation 失败时不得生成部分 `ModuleCatalog`、`.mjs`、`.mjs.map` 或 bundle。

### 交付内容

1. 维护诊断矩阵：ID、触发 operation、源位置策略、消息模板、HelpLink、替代示例和回归测试。
2. 为高频失败补充短示例：动态组件类型、frame 外 metadata、未知 RenderFragment、server-only 注入、未映射 external member、unsupported constructor activation。
3. 扩展 `inspect-razorvue-chain.cs`，支持人读文本和 JSON 两种输出，并在链路断裂时以非零退出码结束。
4. 增加 package consumer 诊断回归，确保源码项目和独立消费者的错误分类一致。
5. 将错误构建的清理行为纳入测试，而不是只检查异常字符串。

### 验收阈值

- 正常 Golden Path 无 RazorVue 额外 warning；
- 诊断测试覆盖所有公开 `JAZORVGA`/`JAZORVCA` 条目和至少一个相邻失败形状；
- 每个诊断都有稳定 source span 和 HelpLink；
- 失败后目标输出目录不包含本轮生成的 partial artifact；
- 同一输入重复构建时诊断顺序和文本稳定。

## P0-C：增量构建与性能

### 测量协议

性能工作只能从基线开始。固定：

- 仓库 commit、SDK、Node、浏览器和操作系统；
- 样本页面及依赖版本；
- clean/restore/build 参数；
- warm-up 次数、测量轮数和并发度；
- 首次构建、单文件增量、依赖组件增量、HMR 更新和 Release 打包指标；
- `.mjs`、source map、manifest、bundle 和 gzip 体积。

测量脚本必须使用 `scripts/csharp/` 下的单文件 C# 入口。结果至少输出中位数、离散程度、产物体积和失败退出码；不能用一次运行的最好值作为结论。

### 优先检查点

1. 未变化的组件是否被重复执行 final Compilation、member closure 和 module framing。
2. 单组件变更是否只重建受影响 module 和依赖闭包。
3. `ModuleCatalog`、source map catalog 和 manifest 是否可安全复用且保持确定性。
4. HMR 是否只通知受影响的模块，同时保持组件状态和错误 overlay 语义。
5. 优化是否改变求值顺序、副作用次数、导入 alias、source origin 或错误传播。

### 优化门槛

只有在固定基线显示存在稳定瓶颈时才改实现。优化必须同时通过 compiler regression、Razor SG、Emit、浏览器 smoke 和 source-map/稳定性检查；如果收益低于预设阈值或引入新的协议 fallback，则保留基线并记录原因。

## P0-D：绑定生成与版本漂移

### 统一输入

每个 binding library 至少锁定：

- npm 包名和版本；
- runtime export/component 清单；
- props、events、slots 类型快照；
- 原始文档来源及校验方式；
- license 和资源 manifest；
- 生成器版本与生成命令。

有结构化 `web-types.json` 时，组件、prop、event、slot 注释必须保留上游原文；没有结构化来源时必须明确记录限制，不得把手写摘要标成原始注释。

### 漂移检查

上游升级必须能报告以下变化：

- runtime export 被删除、重命名或改变模块；
- required/optional prop 变化；
- prop 类型、union 分支或事件 payload 变化；
- slot 名称或 payload 变化；
- 原始文档缺失、变化或无法解析；
- manifest 资源、依赖、license 或 hash 变化。

contract diff 默认阻止生成发布产物；只有明确记录迁移说明并更新对应 consumer 证据后，才允许接受破坏性变化。

## 非目标

P0 不实现以下内容：

- 完整 Blazor Server/WebAssembly 或完整 CLR parity；
- `IJSRuntime` 字符串互操作和任意动态 JavaScript fallback；
- Microsoft Blazor 内置 UI 组件兼容层；
- 未经协议设计的认证状态、PersistentComponentState 或 enhanced form handoff；
- 为了“看起来像 JS”而弱化 C# public API；
- 只增加组件数量而没有真实页面、文档和发布证据。

## Definition of Done

P0 只有在以下条件全部满足时完成：

1. Golden Path 页面可由独立 package consumer 在 Debug、HMR、SPA Release 和适用 SSR profile 中复现。
2. 常见诊断具有稳定 ID、源位置、HelpLink、替代写法和无 partial artifact 回归。
3. 增量构建/运行时基线可重复，至少一个有证据的优化已经完成或被明确否决。
4. Vuetify、Element Plus、TDesign 的生成器都具备 snapshot、原始文档来源和 contract drift 检查。
5. 作者指南、当前状态、路线图和 CHANGELOG 与实现和测试同步。
6. 全量门禁通过：compiler、CLR、Razor SG、Emit、绑定 coverage、Release consumer 和真实浏览器 smoke。

未满足任何一项时，P0 保持 active，不把未完成条目写入“已交付能力”。
