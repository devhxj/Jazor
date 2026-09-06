# 架构演进记录

> 本页解释当前分层为何形成；它不作为当前实现或路线图的依据。

## 核心方向的确立

Jazor 始终围绕一项核心能力展开：将受支持的 C# 语义转换为 ECMAScript。Roslyn 语义模型、`Jazor.Compiler` 的 ESTree lowering、宿主白名单和 `Jazor.Emit` 的产物交付共同构成这条平台主线。当前文档以它为第一层，避免把任一前端框架当作 Jazor 的定义。

## Razor-to-Vue 的位置

Razor-to-Vue 被确立为建立在核心平台之上的框架集成方向。其稳定边界是官方 Razor Source Generator 完成后的最终 `Compilation`：`Jazor.RazorVue` 负责组件绑定和 Vue artifact framing，所有 C# 语义继续由 Jazor 核心翻译。

这一决定排除了 Razor IR、生成 SFC、二次解析生成 C# 和 wrapper-JS marker 协议等生产路径。它们不再作为当前实现的兼容或回退方案。

## 已退役的 Jolt 路线

Jolt 曾覆盖开发宿主、语言服务、调试、DevServer 和相关协议探索。当前项目图中已不存在 `src/Jolt` 和对应测试工程，Jolt 不再指导产品实现、构建或验证。保留这一事实是为了避免从旧路径、旧命令或旧审计结论反推当前架构。

## 未落地的 CSX 提案

CSX 曾提出 `.jazor` TSX-like 作者格式，经 shadow C# 绑定后生成 `.jsx` 的独立前端路线。仓库当前没有 `Jazor.CSX` 项目或该路线的生产实现；它不是脚本能力，也不是当前产品范围。若未来重新评估，应按框架集成层的规则提出新的设计与验收，而不是恢复旧计划文档。

## 路线图的收口

早期路线图曾按模块、阶段和单个消费者分别记录 Artifact Graph、CLR runtime、RazorVue authoring、性能、宿主交付与 JazorAdmin 的实施过程。随着这些工作中的稳定结论进入架构文档、作者指南和当前状态页面，继续保留相互引用的独立计划会让读者难以分辨已交付能力与下一步方向。

因此，路线图已收束为“当前状态”和“下一阶段”两份文档：前者只陈述已经成立的产品契约与验证入口，后者只保留仍需推进的优先级、边界和完成条件。历史方案、阶段清单和当时的测试快照由 Git 历史保存；它们不再作为当前设计或支持范围的依据。

## 历史资料的处理

旧计划、完成清单、测试报告、审计整改、Jolt 细分设计和过期状态快照不再留在活动文档中。Git 历史保存原始材料；当前文档只保留上述对理解架构边界有必要的结论。

## 2026-09-05 RazorVue 审查与修复

本轮审查保持官方 Razor SG -> final Compilation -> Compiler hooks -> Vue render-function `.mjs` 的生产边界。审查发现和验收范围如下，当前行为以作者指南与回归测试为准。

| 优先级 | 审查发现 | 修复与验收范围 |
| --- | --- | --- |
| P1 | 同步 `OnAfterRender` 的 pending 标志立即复位，状态修改产生的 Vue update 可再次进入同一回调。 | pending 保持到对应 Vue flush 完成；真实 Vue renderer 验证不会自循环，后续外部更新仍能进入回调。 |
| P1 | 普通 `OnInitializedAsync` 尚未完成即进入参数生命周期，且缺少普通异步初始化的 SSR 等待。 | 初始参数阶段等待初始化，SSR 等待初始生命周期任务；覆盖初始化失败、参数顺序与首屏 HTML。 |
| P2 | `SetParametersAsync` 仅在入队时检查 disposed，已排队任务可在卸载后启动。 | 出队执行前检查实例生命周期；已启动任务允许完成，卸载后不启动新的作者任务。 |
| P2 | `DisposeAsync` 的 Promise 被 `void` 丢弃，异步失败脱离 Vue 错误处理。 | 卸载 hook 返回可观察的任务，验证异步释放失败到达 Vue error handler。 |
| P2 | 序号参数省略规则接受属性 getter、复杂字段 receiver 和可抛错运算。 | 只接受可证明能够省略的序号表达式，其余以 direct-render 诊断拒绝；覆盖 getter、receiver、转换、除零与无副作用序号。 |

测试应区分表达式运行时验证与 Vue 调度验证：轻量 stub 继续用于 VNode 形状和表达式结果，生命周期、依赖跟踪、卸载与 SSR 使用真实 Vue runtime。不能用手动调用 hook 的通过结果代替真实调度证明。

后续优化候选包括限制组件发现的程序集扫描范围、按职责拆分大型 lowering 文件，以及补齐原生 SG hook 的 SDK/平台验证。性能优化应先使用现有 benchmark 建立基线；这些候选不与本轮正确性修复混合实施。

## 2026-09-05 Jazor.Compiler 审查与修复

本轮审查以 `Roslyn IOperation -> Acornima ESTree -> JavaScript/source map/ModuleCatalog` 为边界，先记录问题，再实施可验证的最小修复。

| 优先级 | 审查发现 | 处理结果 |
| --- | --- | --- |
| P1 | `file://` URI 形式的编译输入会在 source map 中转换为本地路径，但 `ESGenerator` 的源码内容索引把 URI 当作普通字符串，导致 `includeSourcesContent` 静默缺失。 | 源码内容索引统一把 file URI 转换为本地路径后再匹配；新增 URI compilation 回归测试。 |
| P2 | 模块目录和 source map `sources` 使用仅忽略大小写的比较器。当前输入去重会规避大多数相等项，但比较器没有表达完整的稳定全序，后续边界扩展可能重新引入输入顺序依赖。 | 增加 `Ordinal` 二级比较，保持现有大小写不敏感排序语义并明确确定性契约。 |
| P2 | 单个模块 lowering 失败时，其他成功模块仍进入 `ModuleCatalog`；source map 失败则保留 JavaScript 并告警。 | 保持现有渐进式交付行为，并由现有场景测试锁定；这属于需要消费者明确接受的交付策略，不在本轮改成全量失败。 |

后续候选包括扩大白名单结构匹配的边界测试、统一诊断的源位置锚点、建立 optimizer 基准后再评估纯表达式覆盖，以及继续拆分大型 lowering 文件。它们需要独立的行为契约，不能仅以“看起来更完整”作为修改依据。
