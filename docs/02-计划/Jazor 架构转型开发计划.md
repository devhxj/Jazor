# Jazor 架构转型开发计划（WBS）v3

> **项目**：[Jazor](https://github.com/devhxj/jazor) - .NET toolchain for authoring JavaScript and Vue applications from C# and Razor
>
> **目标**：在独立转型分支上建立一条新的 Razor-to-Vue 主线：继续使用受控 Razor Source Generator tail hook，但只消费官方 SG 的最终生成文档（generated C#），并复用 hook 回调的 Roslyn compilation 语义上下文，不再消费 Razor DR/IR；将生成的 `BuildRenderTree` 语义编译为 Vue render-function `.mjs`；通过统一工具链完成 Deno 生产构建、开发服务器、HMR 与单向手写 SFC 互操作。
>
> **日期**：2026-07-22
>
> **状态**：Draft v3 - G0 已通过，Task 0.5 待执行

本文中的 `DR/IR` 特指 Razor intermediate representation（包括 `DocumentIntermediateNode` 及其相关节点），不是 Roslyn `IOperation` 或官方 SG 最终生成的 C# 文档。本文中的 `hook compilation` 指 source-output callback 可见的 Roslyn compilation 快照，不等同于外层 `GeneratorDriver` 应用全部 generator output 后返回的 final updated compilation。

---

## 一、计划定位与分支政策

### 1.1 新旧线路关系

本计划描述的是一条独立的新技术分支，不是对旧 RazorVue/Jolt 主线的兼容升级。

- 旧线路由原分支或基线 tag 保留，可继续用于维护、对照和历史追溯。
- 转型分支不维护 SFC 与 render-function 双输出，不承担旧 Jolt 协议兼容。
- 旧实现可以作为行为盘点和测试 fixture 参考，但是否复用代码由新架构边界决定。
- 默认策略是“按边界复用”，不是“按目录迁移”：只有不携带旧 IR/SFC/Jolt 合同的通用 Roslyn、compiler、Emit 或路径工具通过审查后才能复用；旧 frontend、DTO、协议和状态机一律按新合同重写。
- 转型分支删除旧代码时直接依赖 Git 历史，不把整份源码移动到 `docs/05-遗弃/`。
- G0 失败时停止转型分支，不影响旧线路继续存在。

建议在开始实现前记录：

```text
baseline branch/tag: main (at baseline commit below)
transformation branch: feature/razor-sg-render-function
baseline commit: d68aecbb00b23aa35735c9a269b2e987c7815b05
```

### 1.2 与 Jazor Component Runtime 计划的关系

本计划仍以 Vue 3 `h()` / VNode 为浏览器运行时，不实现独立的 Blazor DOM renderer，也不移植完整 `RenderTreeDiffBuilder`。

```text
本计划：Razor SG -> BuildRenderTree IOperation -> Vue render context -> Vue VNode
另一计划：Razor SG -> BuildRenderTree -> Jazor Component Runtime -> 自有 DOM diff
```

两条探索线不得在同一实现中混用 runtime、生命周期或 DOM ownership。

---

## 二、架构决策

| # | 决策 | 理由与后果 |
|---|---|---|
| D1 | **保留受控 Razor SG tail hook** | 当前 Roslyn/Razor SG 没有可直接满足该链路的稳定 companion-generator 排序合同。新线接受窄 hook 及其 SDK compatibility guard 成本。 |
| D2 | **只消费官方 SG 最终生成文档，不消费 DR/IR** | hook 读取 SG 生成结果中的 document identity、generated C#、source mapping 等必要数据；不调用 `GetDocumentNode()`，不转换 `DocumentIntermediateNode`，不建立 `RazorVueRazorIrNode`。这里的“最终”限定 SG document 数据流，不暗示 callback 已拿到 output-applied final compilation。 |
| D3 | **复用 hook compilation 派生链，以 generated C# + Roslyn IOperation 作为唯一组件语义输入** | G0 直接接收 callback compilation。若它已包含本轮全部精确匹配的 generated trees，则复用同一实例；若有缺失，则只在该实例上批量 `AddSyntaxTrees(...)` 一次取得不可变派生 compilation。两者都复用原 assembly identity、references、options 与 source trees，不重新创建 compilation、不双跑 SG。随后定位组件符号和 `BuildRenderTree`，由 `SemanticModel.GetOperation(...)` 提供 lowering 输入。G0 要求使用同一 compilation 派生链，不要求对象引用恒等。 |
| D4 | **单一 render-context lowering** | `Jazor.Compiler` 将 RenderTreeBuilder 调用映射为稳定的 Vue render-context 协议。首版不并行维护“运行时回放”和“直接嵌套 h()”两套 lowering。 |
| D5 | **render-function `.mjs` 是唯一 Razor 产物** | 不再从生成后的 builder 语义反推 Vue template/SFC。`.mjs` 是最终 Vue component module，不是待后续解释的 wrapper；`.mjs`、`.mjs.map` 与 manifest 构成稳定 artifact contract。 |
| D6 | **手写 SFC 只做单向互操作** | `.razor` 可通过 `DynamicVueComponent` 引用静态可发现的 `.vue`；`.vue` 不反向引用 `.razor`。 |
| D7 | **工具链消费统一 artifact manifest** | Deno 与 Netpack 只在工具链内部决定 SFC/bundle 实现，不向上层暴露 `CompileSfcAsync` 之类的实现差异。 |
| D8 | **Deno 是默认生产实现，Netpack 是显式实验实现** | 显式选择某实现时不可静默 fallback；只有 `Auto` 模式允许按明确顺序选择可用实现。 |
| D9 | **Jolt 在转型分支退役** | `.jazor`、Jolt LSP、DAP/debug 和跨 lane workspace coordination 不属于新线；Razor 与 `.vue` 分别使用原生语言服务。 |
| D10 | **简洁性和效率必须量化** | G2 前建立生成体积、编译耗时、render throughput 与 retained allocation 基线；未达阈值不得以“后续优化”通过 Gate。 |
| D11 | **SG 到 Emit 只走版本化 generated carrier** | tail output 只通过 `AddSource` 发出 data-only catalog 和 diagnostic，不直接写磁盘、启动进程或访问网络；`Jazor.Emit` 在成功 build 后读取 catalog 并物化真实 artifact。 |
| D12 | **组件按可达成员闭包编译，不复制 CLR/ComponentBase 对象模型** | lowering 根为 `BuildRenderTree`、已支持 lifecycle 和其可达 handler/member；不编译无关成员，不把完整 Blazor renderer 或外部基类继承搬到浏览器。 |
| D13 | **G0 只保留一个 production tail source** | 当前 SDK 以 implementation `SourceOutputNode` 作为正式输入；`HostOutput` 只用于 compatibility probe/diagnostic，不作为第二 lowering/fallback 路径。首选 source 不存在时 G0 fail-fast，除非另有 ADR 接受额外生产分支。 |
| D14 | **canonical artifact 不含时间/机器状态** | module、map、carrier 和 manifest 的 hash/内容不写入 wall-clock、当前目录、绝对路径或随机 ID；需要观测的 build metadata 放在明确的非 canonical sidecar。 |

这些决策在 G0 通过后应写入正式 ADR，并明确 supersede 转型分支上的旧 RazorVue/Jolt 架构说明。

---

## 三、目标架构

```text
.razor / .razor.cs
       |
       v
official Razor Source Generator
       |                                      Roslyn GeneratorDriver input
       | 受控 Initialize tail hook                         |
       v                                                   v
official SG final document stream             hook callback Compilation
  - document identity                                      |
  - generated C# text                                      |
  - source mappings                                        |
       |                                                   |
       +--------------------+------------------------------+
                            |
                            | 不读取 Razor DR/IR
                            v
generated-compilation binder
  - exact current trees 已存在：复用 callback Compilation
  - 否则：在同一 Compilation 派生链上一次性 AddSyntaxTrees(missingTrees)
                            |
                            v
bound Roslyn Compilation
       |
       v
component symbol + BuildRenderTree IOperation
       |
       v
Jazor.Compiler / SemanticWalker
  - C# expression/member/function semantics
  - RenderTreeBuilder -> Vue render-context Compile hooks
  - stable imports and temps
       |
       v
versioned Jazor.Generated.VueRenderCatalog
  - emitted only through AddSource
  - module/map/dependency/hash payload
       |
       v
compiled consumer assembly
       |
       v
Jazor.Emit
  - component .mjs
  - .mjs.map
  - artifact manifest
  - @jazor/vue-runtime assets
       |
       v
IFrontendToolchain
  |-- DenoToolchain (default, production)
  |     `-- esbuild + @vue/compiler-sfc
  `-- NetpackToolchain (experimental, explicit opt-in)
       |
       v
browser + Vue 3 runtime
```

手写 SFC 在工具链边界汇入：

```text
.razor --static Src--> DynamicVueComponent --manifest--> .vue source graph
                                                        |
                                                        v
                                                 IFrontendToolchain
```

### 3.1 SG-result 输入合同

hook 允许依赖 SDK internal shape，但必须把依赖限制在一个适配层内。G0 记录 implementation source-output 的 SDK/type/fingerprint；HostOutput 不进入 production lowering。适配层输出 Jazor 自有中立模型：

```csharp
internal sealed record RazorSgGeneratedDocument(
    string HintName,
    string SourcePath,
    SourceText GeneratedCSharp,
    ImmutableArray<RazorSgSourceMapping> SourceMappings);

internal sealed record RazorSgTailBatch(
    Compilation HookCompilation,
    ImmutableArray<RazorSgGeneratedDocument> Documents);
```

#### Compilation 复用边界

Razor SG 不拥有一个可在 source-output callback 中直接交给 Jazor 的“已应用本轮 generated sources 的 final compilation”。callback 内可读取的是 Roslyn driver 传入的 compilation 快照；官方 `.razor.g.cs` 在 callback 返回后才由外层 driver 合并到 updated compilation。最终 updated compilation 只有调用 `RunGeneratorsAndUpdateCompilation(...)` 的宿主能够直接取得，不能假设 tail callback 已经拿到它。

G0 因此定义两种合法 binding mode，并在 gate evidence 中记录当前 SDK/host 的实际模式：

| Binding mode | 条件 | 行为 |
|---|---|---|
| `ReusedHookCompilation` | callback compilation 已包含本轮所有 Razor generated tree，且 document identity 与 content hash 全部精确匹配 | 直接使用同一个 `Compilation` 实例取得 symbol、semantic model 与 `IOperation` |
| `DerivedHookCompilation` | callback compilation 缺少本轮一个或多个 generated trees，且不存在 identity/hash 冲突 | 使用 callback compilation 的 parse options，在稳定排序和去重后只补入 missing trees，一次性调用 `AddSyntaxTrees(...)` |

两种模式都是对官方 SG compilation 语义上下文的复用。`Compilation` 不可变，`AddSyntaxTrees(...)` 产生共享原有 assembly identity、references、options 和 syntax trees 的派生视图，不等于重新创建项目 compilation，更不等于 nested-run Razor SG。

binding 必须以本轮完整 document batch 为单位。部分 exact hit 可以复用，其余 missing trees 必须同批一次性补入；相同 document identity 对应不同 content hash，或无法证明现有 tree 属于当前官方 SG 结果时 fail-fast，首版不猜测替换 stale tree。不得按 generated text 相等性跨 document 去重，去重键至少包含稳定 document identity、hint name 与 content hash。

硬性规则：

- 不向 lowering 暴露 `RazorCodeDocument`、`RazorCSharpDocument` 或其他 SDK internal 类型。
- `Jazor.Analyzer` 不携带/发布 `Microsoft.CodeAnalysis.Razor.Compiler` 的第二份副本；adapter 通过受 fingerprint 保护的 object-shape reflection 读取官方对象，避免 load-context type identity split。
- 不读取 `RazorCodeDocument.GetDocumentNode()`。
- 不将 Razor IR node kind、tokens、attributes 或 body 结构投影到 Jazor DTO。
- 不重新解析 `.razor` 原文作为生产 fallback。
- 不在生产路径 nested-run 第二次 Razor SG。
- 不调用 `CSharpCompilation.Create(...)` 从零重建项目 compilation，也不在 callback 内调用 `RunGeneratorsAndUpdateCompilation(...)` 获取所谓 final compilation。
- hook compilation 不可读、SG result shape 不兼容、缺文档、tree set 冲突或 generated C# 不可绑定时 fail-fast，并报告 SDK/version/fingerprint/binding-mode 诊断。

### 3.2 单一 render-context 合同

首版输出形态固定为：

```javascript
export default defineComponent({
  setup(rawProps, vueContext) {
    const component = createComponentState(rawProps, vueContext)

    return () => {
      const builder = createRenderContext(h)
      buildRenderTree(component, builder)
      return builder.finish()
    }
  }
})
```

`Jazor.Compiler` 负责把 C# `BuildRenderTree` 方法及其调用语义编译为 JavaScript；runtime 只拥有 frame stack、VNode materialization 和 Vue-specific event/slot framing。组件表达式、成员访问、控制流、evaluation order、side-effect count 和 import 收集仍由 `SemanticWalker` 负责。

render-context 只存在于最终 module 的 runtime bridge，不序列化为 wrapper marker，不作为 slot/template transport 协议向工具链暴露；slot 在同一 lowering 中直接物化为 Vue slot function/VNode。

首版禁止另建“静态场景直接生成嵌套 `h()`”的第二路径。若性能 Gate 未通过，优先优化 render-context 的数据结构、重用策略和调用形状；只有单独 ADR 证明收益明显且语义不分叉时，才评估 fast path。

### 3.3 SG-to-Emit carrier 合同

Source Generator 的受支持输出边界是 generated source。新线保留这一事实，但不复用旧 SFC catalog shape：tail output 将 compiler 结果编码为版本化、data-only 的 `Jazor.Generated.VueRenderCatalog`；成功 build 后，`Jazor.Emit` 只读取该 catalog，再写 `.mjs`、map、runtime assets 和 manifest。

硬性规则：

- generator 进程不得直接写项目目录或 output 目录，不启动 Deno/Netpack，不访问网络。
- carrier 只包含 schema version、component/module identity、module text、source-map payload、dependency、content/hash 与 HMR boundary metadata、diagnostics metadata；不得携带 Razor DR/IR、`.razor` 原文或旧 SFC DTO。
- payload 必须按 artifact 分片，避免单个 generated method/metadata blob 随项目规模无限增长。
- `Jazor.Emit` 遇到未知 schema/runtime protocol、重复 identity、hash 不匹配或不完整 payload 时 fail-fast；不得猜测旧 catalog shape。
- carrier 是构建内部传输格式，不是用户可消费 API，也不是最终 artifact contract。

### 3.4 artifact 交接合同

`Jazor.Emit` 与工具链之间通过版本化 manifest 交接，不允许工具链扫描目录猜测入口，也不允许原地改写 compiler-owned `.mjs`：

```json
{
  "schemaVersion": 1,
  "runtimeProtocolVersion": 1,
  "entries": ["components/App.mjs"],
  "modules": [
    {
      "path": "components/App.mjs",
      "sourceMap": "components/App.mjs.map",
      "componentId": "Sample.App",
      "contentHash": "sha256:<hex>",
      "dependencies": ["@jazor/vue-runtime"],
      "hmrBoundary": "logic-safe"
    }
  ],
  "sfcEntries": [
    {
      "moduleId": "jazor-sfc:Components/HelloSfc.vue",
      "sourcePath": "Components/HelloSfc.vue"
    }
  ],
  "assets": []
}
```

`FrontendBuildRequest` / `FrontendDevServerRequest` 显式提供 `artifactRoot` 与 `sourceRoot`。`modules[*].path` 相对 artifact root，`sfcEntries[*].sourcePath` 相对 source root；manifest 自身不持久化机器绝对路径。所有 path 必须规范化且不能通过 `..`、符号链接或大小写别名逃逸所属 root。

canonical manifest 不包含 `GeneratedAtUtc`、当前工作目录、随机 session id 或未排序集合；诊断/运行观测若需要时间戳，写入单独的 non-canonical log/sidecar，不能改变 artifact hash。

---

## 四、WBS 总览与依赖

| Phase | 目标 | 估时 | Gate |
|---|---|---:|---|
| Phase 0 | SG-result hook G0、ADR、转型分支清理 | 3-5d | G0 |
| Phase 1 | Counter 最小纵向链路 | 5-7d | G1 |
| Phase 2 | render 语义、响应式、生命周期与性能 | 12-18d | G2 |
| Phase 3 | 工具链正式化与 DynamicVueComponent | 6-9d | G3 |
| Phase 4 | DevServer/HMR 与 Netpack 实验实现 | 6-10d | G4 |
| Phase 5 | 样例、NuGet、跨平台、文档与发布 | 4-6d | G5 |
| **合计** | G0 后重新估算 | **36-55d** | |

估时是单人基线，不包含未知 SDK hook 返工。G0 与 G2 后必须重新估算，不能把 32 天下界当作承诺日期。

```text
Phase 0 / G0
    |
    v
Phase 1 / G1: Razor -> .mjs -> minimal Deno build -> browser Counter
    |
    v
Phase 2 / G2: complete render/state/lifecycle contract + performance
    |
    v
Phase 3 / G3: stable toolchain contract + handwritten SFC interop
    |
    v
Phase 4 / G4: dev loop + HMR
    |\
    | `--> Netpack experimental implementation
    v
Phase 5 / G5: package + samples + release
```

Netpack 可在 3.1 接口冻结后与 Deno 的后续工作并行，但不阻塞 G3/G4/G5。文档和 package work 可在 G3 后并行，正式发布仍依赖 G5。

---

## 五、Phase 0：SG-result G0 与分支固化（3-5d）

### Task 0.1：记录分支边界与 ADR 草案

**内容**：记录旧线路 baseline、转型分支名称、D1-D14、明确退出的产品能力和恢复方式。

**验收标准**：

- [x] 旧线路有可定位的 branch/tag/commit，不复制源码到文档目录。
- [x] ADR 明确接受 tail hook，明确拒绝 DR/IR 消费和双 lowering。
- [x] `.jazor`、Jolt LSP/DAP、Razor-to-SFC 被列为转型分支 breaking changes。

**验证**：人工架构评审通过后再进入 0.2。

### Task 0.2：建立 SG-result-only hook adapter

**内容**：保留现有 native `Initialize` hook、IL fingerprint 和 compatibility guard；将 tail 输入收窄为官方 SG 最终生成文档，同时读取 source-output callback 的 hook compilation，输出单个 `RazorSgTailBatch`。adapter 不承诺该 compilation 已包含本轮 `.razor.g.cs`，只传递实际对象并记录 tree-presence probe。

**验收标准**：

- [x] 外部 `.razor` consumer 的 official SG 原样运行一次。
- [x] adapter 可读取 hint name、source identity、generated C# 与 source mappings。
- [x] adapter 可取得 callback compilation；读取失败属于 SDK compatibility failure，不另建 compilation 掩盖问题。
- [x] trace/evidence 记录本轮 generated document count、callback compilation tree count、exact tree hit count 和 binding mode；canonical output 不写入绝对路径。
- [x] implementation `SourceOutputNode` 被确定为唯一 production source；HostOutput 仅用于 probe/diagnostic，不注册第二个 lowering provider。
- [x] production call graph 不包含 `GetDocumentNode`、`ConvertNode`、`RazorVueRazorIrNode`。
- [x] SDK shape 不匹配时给出明确 diagnostic，不静默 no-op。

**验证**：新增 focused tests 覆盖正常、多文档、suppressed/empty result、compilation unavailable、shape mismatch 和 fingerprint mismatch；真实 package consumer probe 必须确认当前 SDK 的 callback compilation 是否已经包含 current generated trees。

**依赖**：0.1。

### Task 0.3：绑定 generated C# 并提取 IOperation

**内容**：以 `RazorSgTailBatch.HookCompilation` 为唯一 base compilation，先按 document identity、hint name 与 content hash 对本轮全部 generated C# 做 exact reconciliation。全部 trees 已存在时直接复用同一个实例；存在 missing trees 且没有 identity/hash 冲突时，按稳定顺序只解析 missing trees 并一次性 `AddSyntaxTrees(...)` 得到派生 compilation。随后定位组件 partial class 与 `BuildRenderTree(RenderTreeBuilder)` 方法并取得 `IBlockOperation`。不得按单文档即时 lowering，避免跨组件/共享 generated helper 符号缺失。

**验收标准**：

- [x] `ReusedHookCompilation` 与 `DerivedHookCompilation` 都有 focused test；真实 SDK/host 实际采用的 mode 被写入 G0 evidence。
- [x] 复用模式保持 `ReferenceEquals(boundCompilation, hookCompilation)`；派生模式保持同一 assembly identity/references/options，且不调用 `CSharpCompilation.Create(...)`。
- [x] current generated trees 在 bound compilation 中恰好各一份；部分 exact hit 被保留并只补 missing trees，identity/hash 冲突和 stale tree 均 fail-fast，不猜测替换。
- [x] 同一 `.razor` 与 `.razor.cs` partial 被绑定为同一组件符号。
- [x] `BuildRenderTree` 参数、方法体、泛型组件和 source origin 可稳定定位。
- [x] 同一轮的多文档、共享 generated helper 和 partial source 在一个 bound compilation 中可互相解析。
- [x] candidate identity 由 SG document 映射确定；手写 `.cs` `BuildRenderTree` 组件不会被 tail hook 重复 claim，另有明确 analyzer/compiler route 或 diagnostic。
- [x] 重复 hint、重复 component identity、编译错误和缺失 method 均 fail-fast。
- [x] 不通过磁盘回读 generated `.g.cs` 建立生产主线。
- [x] binding path 不启动 `GeneratorDriver`，不执行第二次 Razor SG，也不重新解析项目的普通 C# source trees。

**验证**：使用纯 markup、Counter、子组件、条件/循环四类 generated result fixture，并对每类运行“trees 已存在”和“trees 缺失”两种 compilation fixture；再由 0.4 外部构建锁定真实 host mode。

**依赖**：0.2。

### Task 0.4：独立 consumer G0 smoke

**内容**：通过本地 NuGet 包创建独立临时 Razor consumer，证明包外真实构建可以同时拿到 SG 最终生成文档、hook compilation 和 `BuildRenderTree` IOperation。

**验收标准**：

- [x] clean restore/build 可重复通过。
- [x] 首次和增量构建都只生成一份目标组件输入。
- [x] 修改 `.razor` 后 generated C# hash 与 operation inventory 更新。
- [x] 构建日志记录 hook compilation 与 binding mode，证明未消费 DR/IR、未从零创建 compilation、未 nested-run Razor SG。

**验证**：在当前支持的 .NET 11 preview SDK 上运行 package consumer smoke；SDK matrix 由 ADR 固定。

**依赖**：0.3。

### Gate G0

G0 必须同时满足：

- hook 稳定拿到 official SG 最终生成文档与 callback compilation；
- production 只注册一个已 fingerprint 的 implementation source-output；HostOutput 不作为隐藏 fallback；
- G0 evidence 明确当前 SDK/host 的 binding mode；若 generated trees 已在 callback compilation 中则直接复用，若不在则只允许基于同一 compilation 派生链批量派生一次；
- bound compilation 中每个 current generated tree 恰好一份，assembly identity/references/options 与 hook compilation 一致；
- `BuildRenderTree` IOperation 可供 compiler 消费；
- production path 不消费 DR/IR、不回读 `.razor`、不从零创建 compilation、不双跑 Razor SG；
- 独立 package consumer 首次/增量构建通过。

**结果（2026-07-22）：通过。** 当前支持 SDK 为
`11.0.100-preview.5.26302.115`；package consumer 实测为
`DerivedHookCompilation`（`Reused=0`、`Derived=1`）。Schema 2 evidence 记录
generated-C# content hash、`BuildRenderTree` operation inventory/hash 与禁止路径
标志；consumer 在 clean restore、首次 build、无改动 incremental build 及 Razor
结构修改后的 build 中均保持一个 generated input，修改后两个 inventory 均更新。
验证命令见 [Razor SG Final-Document G0 决策记录](./RazorSgFinalDocument.G0.DecisionRecord.md)。

任一条件不满足，停止转型分支并重新评估输入架构，不进入 runtime、toolchain 或清理工作。

### Task 0.5：G0 通过后的转型分支清理

**内容**：只有在 Gate G0 通过并记录 gate evidence 后，删除转型分支中的 Razor-to-SFC、Razor DR/IR frontend、Jolt/Jolt.Test/Jolt.VSCodeExtension 及其专用脚本/文档入口；保留并重命名 SG-result hook 测试资产。

**验收标准**：

- [ ] solution 不再包含 Jolt 和 Razor IR test project。
- [ ] 新建或重命名为 SG-result focused test project，测试职责不再包含 IR。
- [ ] 不存在 SFC output mode 分支、DR/IR DTO 或 Jolt protocol orphan。
- [ ] 测试不是 skip：旧合同测试删除，新合同测试在同一提交加入。
- [ ] 清理提交引用 G0 evidence；若 G0 失败，Task 0.5 不执行。

**验证**：`dotnet build Jazor.slnx` 与 G0 focused suite 通过，并检查 solution/project graph 没有 orphan reference。

**依赖**：Gate G0。

### Phase 0 出口检查点

- [x] G0 evidence、SDK fingerprint、baseline commit 和 ADR 草案已入转型分支。
- [ ] Task 0.5 清理完成，且新线在没有旧 IR/Jolt 依赖的情况下可 clean build。
- [ ] 下一阶段只从 Phase 0 出口开始；不得在 G0 未通过时并行实现 runtime 或 toolchain。

---

## 六、Phase 1：Counter 最小纵向链路（5-7d）

### Task 1.1：冻结 render-context v1 协议

**内容**：定义最小 runtime surface，仅覆盖 Counter 所需能力。

```javascript
const RENDER_CONTEXT_PROTOCOL_VERSION = 1

createRenderContext(h)
  .openElement(name)
  .addAttribute(name, value)
  .addContent(value)
  .closeElement()
  .finish()
```

sequence 参数由 compiler 接收并擦除，但仍验证调用形状；runtime 不承担 C# expression lowering。

**验收标准**：协议有版本、错误行为、frame balance 和空/单/多 root 规则（空 root 为 `null`，单 root 直接返回，多个 root 使用 Vue `Fragment`）；失配 close 必须抛出可定位错误。

**验证**：runtime unit tests 覆盖 frame balance、attribute/content 顺序和空节点规范化。

**依赖**：Phase 0 出口检查点。

### Task 1.2：RenderTreeBuilder Compile hooks

**内容**：在 `Jazor.Compiler`/`SemanticWalkerHost` 中识别最小 RenderTreeBuilder surface，并生成 render-context 调用。组件普通 C# 表达式继续走现有 compiler mainline。

**验收标准**：

- [ ] builder receiver 只求值一次。
- [ ] attribute/content value 保持 C# evaluation order 与 side-effect count。
- [ ] unsupported overload 明确列出 method signature 并 fail-fast。
- [ ] import、temp name 和 emitted call order 稳定。

**验证**：`Jazor.CompilerTest` 覆盖直接调用、条件分支、event lambda 与 unsupported overload。

**依赖**：1.1。

### Task 1.3：可达组件成员闭包、state 与 event

**内容**：以 `BuildRenderTree`、已支持 lifecycle、RenderFragment 和已捕获 event handler 为根，建立确定性的 current-component 可达成员闭包。只编译闭包中的字段、属性、方法、initializer 和 lambda；不复制完整 `ComponentBase`/CLR 对象模型。首版使用单一 reactive state object；参数读取与本地可变 state 使用不同 rewrite，不把所有字段机械变成独立 `ref()`。

**验收标准**：

- [ ] member closure、import 和 emitted member order 在重复构建中稳定；未被根可达的成员不进入产物。
- [ ] Counter 字段/initializer 只执行一次，render 中读取可被 Vue 追踪。
- [ ] click handler 修改 state 后只触发一次有效更新，handler identity 不因每次 render 无故漂移。
- [ ] `[Parameter]` 读取来自 props，写入参数明确拒绝或按已定义合同处理。
- [ ] current-component method 的 `this` 绑定稳定；无法解析的动态 dispatch 给出 source-origin diagnostic。

**验证**：member-closure inventory、state/member rewrite compiler tests、event identity tests + runtime reactive unit tests。

**依赖**：1.2。

### Task 1.4：generated carrier、`.mjs`、source map 与 manifest

**内容**：compiler/SG tail 通过 `AddSource` 产生版本化 data-only carrier；`Jazor.Emit` 在成功 build 后读取 carrier，物化 `defineComponent + setup + render function`、runtime import、`.mjs.map` 和 schema-v1 manifest。generator 不直接写磁盘。

**验收标准**：同一输入重复构建 byte-for-byte 一致；manifest path 不可逃逸；source map 能从 generated JS 定位回 `.razor`/`.razor.cs` source mapping；失败或删除的 component 不会留下仍被 manifest 宣称有效的旧 `.mjs`。

**验证**：carrier schema/version/hash tests、`Jazor.EmitTest` snapshot、path validation 和 source-map segment tests；测试 stale carrier、重复 component identity 与未知 schema 都 fail-fast。

**依赖**：1.1、1.2、1.3。

### Task 1.5：最小 Deno build harness 与 browser Counter

**内容**：在正式工具链接口前先建立最小、可删除的 G1 harness，负责解析固定版本的 `vue`、bundle entry 并启动静态测试服务器。它只服务 G1，不承诺 HMR 或 SFC；harness 必须复用同一 manifest/request core，不得形成第二套 bundler。

**验收标准**：Counter 首次显示 `0`，连续点击更新为 `1/2/3`；handler 不重复注册；控制台无异常。

**验证**：自动化真实浏览器测试，不以手工截图代替。

**依赖**：1.4。

### Gate G1

`Counter.razor -> official SG 最终生成文档 + hook compilation -> IOperation -> .mjs -> Deno bundle -> browser` 全链自动化通过。G1 失败必须按输入、compiler、runtime、emit、toolchain harness 五层定位，不能默认归因于 lowering。

---

## 七、Phase 2：核心组件语义与性能（12-18d）

### Task 2.1：完整 RenderTreeBuilder v1 surface

覆盖：

- `OpenElement` / `CloseElement`
- `OpenComponent<T>` / `CloseComponent`
- `AddContent` / `AddMarkupContent`
- `AddAttribute` / `AddComponentParameter` / `AddMultipleAttributes`
- `OpenRegion` / `CloseRegion`
- `SetKey`
- element/component reference capture
- `RenderFragment` / `RenderFragment<T>`

映射必须先固定，不允许用“包一层 `<div>`”掩盖 Vue 与 RenderTree 的结构差异：静态 `AddMarkupContent` 采用 `createStaticVNode`（预先记录 root count），动态 raw HTML 在 v1 直接 diagnostic；重复 attribute/boolean attribute 规则、region/fragment 边界、key scope、reference capture 时机都要有明确的 Vue primitive 或明确 diagnostic。

首版明确不承诺 cascading value/parameter、动态 `IComponent` type、custom RenderTree frame、SSR/render mode；这些形状在 compiler/analyzer 处给出 diagnostic，不通过“当作普通 prop”静默降级。

**验收标准**：每个 accepted generated-code call shape 都有 compiler emission test 和 runtime behavior test；多 root raw markup 不隐式插入 wrapper；未列入 surface 的 API fail-fast。

**验证**：`Jazor.CompilerTest` 对每个 accepted call shape 做 AST/emission assertion；runtime/browser fixtures 覆盖 raw markup、duplicate attributes、fragment/key 和 reference capture。

**依赖**：G1。

### Task 2.2：组件 descriptor、props、emits、slots 与 bind

**内容**：组件 type/import、prop public name、EventCallback runtime event name、slot metadata 和 bind pair 均来自强类型 descriptor，不按字符串猜测。

**验收标准**：

- [ ] DOM event 与 component emit 走不同规范化路径。
- [ ] `@bind-X` 使用 X 对应 prop/changed-event descriptor，不一律写成 `modelValue`。
- [ ] default、named、typed slot 的首版支持范围有显式矩阵。
- [ ] duplicate/unknown slot、required prop 和类型不匹配有诊断。
- [ ] sync/async `EventCallback` 的 await、error propagation 和 render invalidation 次数有统一规则。

**验证**：descriptor metadata tests + parent/child browser fixtures，逐项断言 prop/emit/slot/bind 的最终 VNode 与事件名。

**依赖**：2.1。

### Task 2.3：state 与生命周期合同

先冻结下表，再实现：

| C# 语义 | Vue 映射 | 首版要求 |
|---|---|---|
| field/property initializer | `setup` 内 state 初始化 | first render 前只执行一次 |
| `OnInitialized` | setup 初始化阶段 | first render 前 |
| `OnInitializedAsync` | setup 内启动 async task + invalidation scheduler | 未完成 task 不阻塞首屏；完成后最多合并为一次更新；错误进入统一 error handler |
| `OnParametersSet` | immediate props watch | 首次和后续参数更新均执行 |
| `OnParametersSetAsync` | serialized async watch | 新 props 到来时旧 completion 不得覆盖新状态；取消/错误规则写入合同 |
| `OnAfterRender` | `onMounted` + `onUpdated` | 正确维护 `firstRender` |
| `OnAfterRenderAsync` | post-flush task | 不把 task completion 静默当成 render request |
| `StateHasChanged` | component render-epoch invalidation | 同一 tick 合并；render function 在 gate 前读取 epoch |
| `InvokeAsync` | scheduler dispatch | 明确同步/异步 callback 与 dispose 后调用行为 |
| `IDisposable` / `IAsyncDisposable` | `onUnmounted` | handler/resource 清理一次；async dispose 有顺序和错误合同 |
| `ShouldRender` | render-epoch + cached VNode gate | 首次 render 必过；拒绝时不执行 render body，但仍保留 epoch dependency |

这是新线的明确组件合同，不宣称完整 `ComponentBase`/Blazor renderer parity；无法诚实实现的 lifecycle shape 必须诊断，不做近似映射。

**验收标准**：每一行都有同步/异步日志 fixture 和 browser ordering assertion。

**验证**：lifecycle test harness 记录 monotonic sequence、firstRender、cancel/error 和 unmount cleanup；同一 fixture 在 cold mount、prop update、rapid update 三种路径运行。

**依赖**：2.1、2.2。

### Task 2.4：source-map、diagnostic 与 deterministic emission

**内容**：将 SG source mappings、compiler origin、runtime framing 和 bundle map 串成可验证链；定义 Jazor-specific diagnostics catalog。

**验收标准**：浏览器异常栈可回到 `.razor` 或 `.razor.cs`；同输入的 module/import/temp/manifest 顺序稳定；unsupported 不产生半成品 artifact。

**验证**：source-map consumer test、双次 clean build hash comparison、diagnostic snapshot 和半成品目录检查。

**依赖**：2.1-2.3。

### Task 2.5：复杂度与性能 Gate

**内容**：建立三组等价 fixture：纯文本、Counter、100-item keyed list；同时与手写 Vue `h()` baseline 和 1.1 节记录的旧线路 baseline 比较。旧线只在固定 commit 的独立 worktree 运行，不把旧代码合并进转型分支。先提交 benchmark protocol 和采样脚本，再运行优化，避免用测量结果倒推阈值。

指标：

- compiler cold/incremental time；
- generated `.mjs` 与 runtime gzip size；
- 10,000 次 render/update throughput；
- mount/unmount 后 retained handler/object；
- runtime protocol 与 compiler special-case 数量；单一路径和 carrier 层数。

最低验收预算先固定为：同 fixture 的 generated module+protocol gzip 不超过手写 `h()` 的 2 倍（共享 Vue 依赖单独报告）；render/update throughput 不低于手写 baseline 的 70%；incremental compiler p95 不超过旧线 baseline 的 1.5 倍；100 次 mount/unmount 后 retained handler/object 不得呈持续增长。完整阈值、机器规格和统计方法必须在首次调优前写入 ADR；失败后不得无记录放宽。超过阈值时先优化单一 runtime path，不直接增加第二 lowering。

**验收标准**：三组 fixture 都有同机 baseline、重复采样、阈值判定和报告；任何指标未达标都阻断 G2，不能用功能测试通过替代性能证据。

**验证**：使用仓库内单文件 C# benchmark runner 固定输入、SDK、浏览器和采样次数，输出可审阅 JSON/Markdown 报告；retained measurement 使用统一 heap-snapshot/GC 方案；报告同时列出手写与旧线 baseline。

**依赖**：2.1-2.4。

### Gate G2

- Counter、父子 props/callback、fragment/slot、conditional/list、lifecycle fixture 全部通过。
- diagnostics、source maps、deterministic output 通过。
- 性能指标已记录并达到 ADR 阈值。
- compiler 中不存在 Razor template/SFC 字符串拼接或第二套 direct-`h()` lowering。

---

## 八、Phase 3：工具链正式化与单向 SFC 互操作（6-9d）

### Task 3.1：冻结 `IFrontendToolchain` 与 manifest contract

```csharp
public enum FrontendToolchainKind
{
    Deno,
    Netpack
}

public interface IFrontendToolchain
{
    FrontendToolchainKind Kind { get; }

    ValueTask<ToolchainProbeResult> ProbeAsync(
        ToolchainProbeRequest request,
        CancellationToken cancellationToken = default);

    Task<FrontendBuildResult> BuildAsync(
        FrontendBuildRequest request,
        CancellationToken cancellationToken = default);

    Task<IFrontendDevServer> StartDevServerAsync(
        FrontendDevServerRequest request,
        CancellationToken cancellationToken = default);
}
```

接口不暴露 `CompileSfcAsync`。`Auto` 是 resolver 的选择模式，不是某个 toolchain implementation 的 `Kind`。build/dev/probe request 必须包含 manifest path、artifact root、source root、output root、mode、source-map/minify 配置和显式 environment；result 必须返回稳定排序的 diagnostics 与产物清单。

预期的 capability/配置错误统一作为 typed result 返回；只有 cancellation、进程崩溃或编程错误抛异常。显式选择的实现不可由接口内部静默替换。

`ToolchainProbeRequest` 还必须携带所需 capability 集合和版本约束；实现不得从当前工作目录、全局 npm cache 或未声明的环境变量推断输入。

**验收标准**：Deno/Netpack 无需上层分支即可消费同一 request；request 的两个 root 通过同一 path validator；dev-server handle 的进程 ownership、停止、取消和日志合同明确。

**验证**：contract tests 以同一 manifest 分别驱动 Deno 与 Netpack fake/real implementation，断言 request/result schema、diagnostic order、capability probe 和 cancellation 行为。

**依赖**：G2。

### Task 3.2：DenoToolchain production build

**内容**：使用固定版本 DenoHost、esbuild 和 `@vue/compiler-sfc`。SFC 通过 bundler plugin/on-load 处理，不预先原地改写 `.mjs` import；plugin 从 request 的 `sourceRoot` 解析 manifest 中的 source-relative entry。

SFC 必须作为 source graph 处理：

- `<script>` / `<script setup>`；
- template + binding metadata；
- scoped style、CSS module、多个 style block；
- nested `.vue` import；
- asset URL 与 `<style src>`；
- JS/CSS source maps；
- compiler diagnostics。

**验收标准**：依赖版本和完整性写入 lockfile；clean/offline-cache build 可重复；同一 manifest 生成稳定 bundle/css/assets；nested import、asset 和 style URL 不能逃出 source root，source graph/hash 顺序稳定。

**验证**：Deno consumer test 分别运行 cold-cache、warm-cache 和断网模式；比较 bundle/css/assets/source-map hash 与结构化 diagnostics。

**依赖**：3.1。

### Task 3.3：`DynamicVueComponent` authoring contract

首版组件（避免 Razor SG 无法验证的 `object?` catch-all）：

```csharp
public sealed class DynamicVueComponent<TProps> : ComponentBase
    where TProps : class
{
    [Parameter, EditorRequired]
    public string Src { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public TProps Props { get; set; } = default!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}

public sealed record HelloSfcProps(
    [property: VueProp("message")] string Message,
    [property: VueEvent("confirm")] EventCallback OnConfirm);
```

authoring 示例：

```razor
<DynamicVueComponent TProps="HelloSfcProps"
                     Src="./HelloSfc.vue"
                     Props="@(new HelloSfcProps(message, EventCallback.Factory.Create(this, HandleConfirm)))">
    <ChildContent>
        <span>@message</span>
    </ChildContent>
</DynamicVueComponent>
```

合同：

- `Src` 必须是编译期字符串常量、相对于 `.razor` source path。
- 路径必须位于 request 指定的 source root 内，文件必须存在；manifest 保存 source-root-relative path。
- 同一 normalized path 生成一个稳定 module binding。
- 生成 `defineAsyncComponent(() => import("jazor-sfc:<source-relative>"))`；virtual specifier 只是 bundler module-resolution seam，不是 render/slot wrapper protocol。manifest plugin 静态解析它，不把机器绝对路径或 artifact layout 猜测塞进 compiler。
- `TProps` 必须是可静态分析的 closed C# 类型；普通 prop 使用强类型属性并通过 `[VueProp]` descriptor 固定 public name，事件成员使用 `[VueEvent]` descriptor，禁止按 CLR casing 或 `OnXxx` 字符串猜测。
- 若 prop value domain 需要 union，优先使用当前 SDK/C# 支持的 native `union`；只有分支可赋值关系破坏精确投影时才使用带 `[System.Runtime.CompilerServices.Union]` 的 tagged fallback，绝不退回 `object`。
- `ChildContent` 只映射 default slot；named/scoped slot 不在首版范围。
- `VueEventAttribute` 与 props value contracts（包含需要的 named union）必须位于可被官方 Razor SG 引用的公共 contract assembly；生成的 `.razor.g.cs` 必须在 SDK Razor SG 下直接编译通过。

动态 `Src`、匿名 object props、`object?` catch-all、路径逃逸和未知 event shape 必须 diagnostic。

**验收标准**：generic component 的 `TProps` 绑定、closed props record、`[VueProp]`/`[VueEvent]` descriptors、default slot 和静态 `Src` 均有官方 Razor SG consumer test；任何 `object`/匿名 props 方案在 analyzer 和 compiler 两端都 fail-fast。

**验证**：独立 package consumer 编译生成的 `.razor.g.cs`，检查其 `OpenComponent<DynamicVueComponent<TProps>>` 与参数调用形状，再运行 mixed browser fixture。

**依赖**：2.2、3.1。

### Task 3.4：Deno mixed browser E2E

使用 `.razor` parent + handwritten `.vue` child，覆盖 props、event、default slot、nested SFC、scoped CSS 和 asset URL。

**验收标准**：Deno build 后真实浏览器正确渲染；JS/CSS source map 可定位；不存在 `.vue.mjs` 原地重写协议。

**验证**：Playwright/browser suite 断言 props、event、default slot、nested graph、scoped CSS、asset 和 map origin；检查 artifact root 中没有 rewrite 临时协议残留。

**依赖**：3.2、3.3。

### Gate G3

- DenoToolchain 使用统一 manifest 完成纯 `.mjs` 与 mixed `.mjs + .vue` production build。
- DynamicVueComponent 的路径、props、event、slot、CSS、asset、source-map browser tests 通过。
- Deno 是 G3 必过项；Netpack 不参与 G3。

---

## 九、Phase 4：DevServer/HMR 与 Netpack（6-10d）

### Task 4.1：CLI 与 MSBuild 集成

新增 `Jazor.Toolchain.Cli`：

```text
jazor-toolchain build --manifest <path> --toolchain Deno
jazor-toolchain serve --manifest <path> --toolchain Deno
```

MSBuild 属性：

| 属性 | 默认值 | 说明 |
|---|---|---|
| `JazorToolchain` | `Deno` | `Deno` / `Netpack` / `Auto` |
| `JazorBundle` | `false` | 是否在 build 后执行 production build |
| `JazorBundleOut` | `$(OutDir)jazor` | bundle output root，保留明确输出合同 |
| `JazorDevServerHost` | `127.0.0.1` | dev server host |
| `JazorDevServerPort` | `0` | `0` 表示由 OS 分配端口 |

`JazorServe` 是长运行 target/CLI，handle 留在 CLI 进程内并响应 Ctrl+C/cancellation；MSBuild 不尝试跨进程持有 `IDevServerHandle`。

**验收标准**：CLI/MSBuild 传递同一 typed request；manifest、artifact root、source root 和 output root 先做 path validation；explicit/Auto 选择、退出码、diagnostics 和 Ctrl+C 行为稳定。

**验证**：CLI integration tests 使用唯一临时 root/port，覆盖 build、serve、取消、已占用端口和 child-process cleanup；MSBuild consumer smoke 不依赖当前仓库 bin 目录。

**依赖**：3.1、3.2。

### Task 4.2：Deno DevServer 与 HMR 协议

必须实现：

- artifact/source dependency graph；
- `.razor` build output change coalescing；
- `.vue` script/template/style change；
- browser HMR client；
- CSS-only update；
- component boundary update；
- boundary 不稳定时 full reload；
- process stop、watcher dispose、port ownership。

generated module 的 HMR boundary、component identity 和 template/logic/style hash 必须来自 manifest/carrier，不由 browser client 从 JS 文本猜测；只有标记为 logic-safe 的更新允许保留 component state。

HMR 验收不以“浏览器刷新后看到新内容”代替。测试必须区分 state-preserving update、CSS update 和 full reload。

**验收标准**：每类变更都有 protocol-level event、browser assertion 和 declared fallback；watcher、server、browser client 在取消或编译失败后都能收敛到可观测状态。

**验证**：Deno dev-server/browser suite 分别执行 state-preserving component update、CSS-only update、unsafe-boundary full reload、rapid edits 和 shutdown cleanup。

**依赖**：4.1。

### Task 4.3：NetpackToolchain experimental implementation

**内容**：使用项目本地、固定版本的 Netpack CLI 消费同一 manifest，实现 build/serve。禁止通过裸 `npx netpack` 临时下载最新版本。

**验收标准**：

- [ ] `JazorToolchain=Netpack` 不可用时明确失败，不 fallback。
- [ ] `JazorToolchain=Auto` 才允许按固定 `Deno -> Netpack` 顺序 fallback，并在日志/manifest 记录实际实现。
- [ ] capability 不足时返回结构化 diagnostic。
- [ ] experimental test 可在独立 CI lane 运行；失败不阻塞 Deno release，但不得伪报成功。

**验证**：Netpack lane 运行同一 manifest contract tests，并断言 explicit failure 不会调用 Deno；Auto lane 记录 selected implementation 和 fallback reason。

**依赖**：3.1；可与 4.2 并行。

### Task 4.4：完整开发循环 E2E

自动化验证：

```text
edit .razor
  -> dotnet watch/build
  -> .mjs + manifest update
  -> dev server invalidation
  -> HMR or declared full reload
  -> browser assertion

edit .vue style/template/script
  -> toolchain rebuild
  -> CSS/component HMR
  -> browser assertion
```

**验收标准**：`.razor`、`.vue` script/template/style 三类编辑都能得到 HMR 或明确 full reload；任何失败都保留原始 diagnostic，不伪报浏览器已更新。

**验证**：single-scenario browser E2E 覆盖首次启动、连续编辑、编译失败后恢复、端口释放和进程退出；Netpack 仅作为独立可选 lane。

**依赖**：4.2；Netpack lane 可选。

### Gate G4

- Deno build/serve/HMR 必须通过自动化浏览器测试。
- watcher、server、child process 可确定性停止，无残留端口和文件锁。
- explicit/Auto toolchain selection 行为确定且可观测。
- Netpack 状态可以是 passed 或 documented experimental gap，但不能改变 Deno Gate。

---

## 十、Phase 5：样例、包、文档与发布（4-6d）

### Task 5.1：迁移真实样例

迁移 `samples/RazorVue.TodoList` 为新线样例，至少包含：

- Razor Counter/list/form interaction；
- Razor parent/child props 与 callback；
- `DynamicVueComponent` 手写 SFC；
- Vuetify `VBtn`、`VTextField` 与 bind；
- production build 与 dev/HMR 配置。

样例不得依赖转型分支已删除的 Jolt/SFC output artifacts。

**验收标准**：样例从独立 consumer clean build 产出 `.mjs`/manifest；Razor state、父子 callback、强类型 `DynamicVueComponent<TProps>`、Vuetify bind 和 Deno dev/HMR 都由真实源码触发。

**验证**：sample-specific browser E2E + package-consumer smoke；检查 project graph 不含 Jolt 或旧 SFC catalog。

**依赖**：G4、3.3。

### Task 5.2：NuGet 与 runtime asset packaging

将以下内容纳入 `Jazor` package：

- SG-result hook/analyzer dependencies；
- compiler/runtime contracts；
- published `Jazor.Emit`；
- published `Jazor.Toolchain.Cli` 及 `.deps.json` / `.runtimeconfig.json`；
- `@jazor/vue-runtime` JS assets；
  - Deno runtime assets 与固定依赖配置；
- `buildTransitive/Jazor.props` / `Jazor.targets`。

**验收标准**：独立临时 consumer 只引用本地 NuGet 包即可 build、bundle、serve；不得回退到仓库 `bin/Debug` 工具。

**验证**：构建 package 后在仓库外临时目录执行 clean restore/build/bundle/serve，并记录包内文件清单与工具版本。

**依赖**：G4、5.1。

### Task 5.3：平台与可复现性验证

明确首发支持的 OS/RID。每个宣称支持的平台验证：

- clean restore；
- warm-cache/offline build；
- production bundle；
- dev-server startup/shutdown；
- path separator/case behavior；
- source map path。

未验证的平台不得写成“生产支持”。

**验收标准**：每个宣称支持的 RID 都有同一套结果记录；失败平台被标成 unsupported/experimental，而不是被静默跳过。

**验证**：平台矩阵 CI lane + offline-cache smoke；比较 manifest、map 和 bundle hash 的确定性。

**依赖**：5.2。

### Task 5.4：文档与 breaking release

更新：

- 根 `README.md` / `README_CN.md`；
- `src/Jazor/README.md`；
- 转型分支的架构/目标/状态文档；
- MSBuild property 表；
- 从旧 SFC/Jolt 线路迁移到新分支的说明；
- `.jazor`、Jolt debug/LSP、反向 SFC interop 的明确退出说明。

版本更新位于 `Directory.Build.props` / MinVer release flow，不在 `Jazor.props` 中维护版本号。

**验收标准**：文档明确新旧分支边界、breaking capability、支持矩阵、toolchain 选择和诊断；旧线链接可定位到 branch/tag/commit。

**验证**：文档链接检查、示例命令 dry run 和一次人工 release review。

**依赖**：G4、5.1。

### Task 5.5：发布前回归

```text
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
dotnet test src/Jazor.Toolchain.Test/Jazor.Toolchain.Test.csproj
```

同时运行新建 SG-result、toolchain 和 browser suites；`scripts/csharp/test-dotnet.cs` 的项目枚举必须同步移除 Jolt/旧 IR lane。旧线路测试在旧 branch/tag 维护，不进入转型分支完成分母。

**验收标准**：所有命令在 clean checkout/clean package consumer 上通过；失败项有明确 owner、diagnostic 和是否阻断发布的记录。

**验证**：按上列命令执行并保存 CI artifact、browser trace、性能报告和 package consumer 日志。

**依赖**：5.1、5.2、5.3、5.4。

### Gate G5

- TodoList mixed sample production/dev 流程通过。
- package consumer 不依赖 repo-local output。
- 支持平台矩阵通过。
- full solution、新 focused suites、browser suites 全绿。
- release notes 明确 breaking scope、性能数据和已知限制。

---

## 十一、目标代码结构

### 转型分支保留并调整

| 路径 | 新职责 |
|---|---|
| `src/Jazor.Compiler/` | C#/IOperation lowering + RenderTreeBuilder Compile hooks |
| `src/Jazor.Analyzer/` | Razor SG tail hook、SG-result adapter、component candidate generation |
| `src/Jazor.RazorVue/` | Vue component descriptor、DynamicVueComponent、render artifact framing；不再拥有 Razor IR/SFC producer |
| `src/Jazor.Emit/` | `.mjs`/map/manifest/runtime asset materialization |
| `src/Jazor.Common/` | stable contracts、source-map carriers、path validation |
| `src/ECMAScript.Vue3/` | Vue 3 host bindings |
| `src/ECMAScript.VueContract/` | prop/event/slot descriptors and `VueEvent` authoring contract |
| `src/ECMAScript.Vuetify/` | Vuetify descriptors and authoring wrappers |

### 转型分支新增

| 路径 | 用途 |
|---|---|
| `src/Jazor.RazorVue/Runtime/` | `@jazor/vue-runtime` render-context source/assets |
| `src/Jazor.Toolchain.Abstractions/` | manifest、request/result、toolchain/dev-server contracts |
| `src/Jazor.Toolchain.Deno/` | production Deno implementation |
| `src/Jazor.Toolchain.Netpack/` | experimental Netpack implementation |
| `src/Jazor.Toolchain.Cli/` | build/serve CLI and process lifecycle |
| `src/Jazor.RazorVue.Sg.Test/` | SG-result hook/binding/package consumer tests |
| `src/Jazor.Toolchain.Test/` | build/dev/HMR/toolchain-selection tests |

### 转型分支移除

| 路径/能力 | 处理 |
|---|---|
| Razor-to-SFC artifact factory/catalog/emit | 删除，不归档源码副本 |
| Razor DR/IR reader/frontend/DTO/tests | G0 通过后删除 |
| `JazorRazorVueOutputMode` 双模式 | 删除；新分支只有 `.mjs` |
| `src/Jolt/`、`src/Jolt.Test/` | 删除；旧线路由原 branch/tag 保留 |
| `src/Jolt.VSCodeExtension/` | 删除或移出转型分支发布面 |
| Jolt-only protocol/scripts/docs/wiki routes | 清理或改写为历史链接 |

---

## 十二、Gate 汇总

| Gate | 必须证明 | 阻断条件 |
|---|---|---|
| G0 | hook 消费 official SG 最终生成文档，复用 callback compilation 派生链并绑定 generated C#/IOperation，不消费 DR/IR | 生成文档/compilation 不稳定、tree reconciliation 冲突、需从零建 compilation、需双跑 SG、需回读 Razor/IR |
| G1 | Counter 从 `.razor` 到浏览器交互链路 | 任一层只有手工 fixture、无真实浏览器验证 |
| G2 | 组件语义、生命周期、source map、determinism、性能达标 | 两套 lowering、近似生命周期、无性能基线 |
| G3 | Deno production build + DynamicVueComponent mixed SFC | `.vue` graph/CSS/assets/map 不完整 |
| G4 | Deno dev/HMR、确定性 lifecycle；Netpack 可选 | 只做 reload 却声明 HMR、进程/端口泄漏 |
| G5 | package consumer、样例、平台矩阵、全量回归 | 依赖 repo-local bin、版本未锁、breaking scope 未文档化 |

---

## 十三、风险登记

| # | 风险 | 概率 | 影响 | 应对 |
|---|---|---:|---:|---|
| R1 | Razor SG internal result shape/IL 或 callback compilation access 在 SDK 更新时变化 | 高 | 高 | G0 fingerprint + object-shape adapter + SDK matrix + fail-fast |
| R2 | generated C#/source mapping 不足以恢复所需 component identity | 中 | 高 | G0 多文档/partial/generic fixture；不退回 DR/IR |
| R3 | render-context 每次 render 的额外 frame/array 成本过高 | 中 | 高 | G2 benchmark；优化单一路径数据结构，不先分叉 lowering |
| R4 | reactive state rewrite 与 C# member/identity 语义漂移 | 中 | 高 | 明确 state/props rewrite 表 + browser behavior fixtures |
| R5 | async lifecycle 与 Vue setup/watch 时序不等价 | 高 | 高 | 支持矩阵 + ordering tests；无法诚实映射时 diagnostic |
| R6 | handwritten SFC graph 涉及 scoped CSS/assets/source-map 链 | 中 | 高 | bundler plugin 统一处理，不做单文件预编译/路径替换 |
| R7 | Deno/npm 依赖在离线或升级后不稳定 | 中 | 高 | 精确版本、lockfile、integrity、warm-cache offline smoke |
| R8 | HMR 被低估为 file watch + reload | 高 | 中 | 独立 HMR protocol/browser Gate，明确 full-reload boundary |
| R9 | Netpack capability 或维护状态不足 | 中 | 低 | experimental lane，显式选择 fail-fast，不阻塞 Deno |
| R10 | 长期转型分支与旧线路漂移 | 中 | 中 | baseline 固定；新线按 Gate 原子提交，不做双向 cherry-pick 承诺 |
| R11 | 不同 Roslyn host 暴露的 callback compilation tree set 不同 | 中 | 高 | G0 记录 binding mode；复用 exact hit 并只补 missing trees，stale/conflict fail-fast；package consumer 覆盖首次与增量构建 |

---

## 十四、明确排除项

| 排除项 | 原因 |
|---|---|
| Razor DR/IR 消费 | 新输入合同只认 SG 最终生成文档 + hook compilation + generated C#/IOperation |
| Razor-to-template/SFC 生成 | 已由 D5 排除，避免逆向恢复模板语义 |
| SFC -> Razor 反向引用 | 单向互操作边界 |
| 自有 Blazor DOM diff runtime | 属于另一条 Jazor Component Runtime 计划 |
| `.jazor` authoring、Jolt LSP/DAP/debug | 转型分支明确退役 |
| DynamicVueComponent 动态 `Src` | bundler 无法可靠静态发现，首版只允许常量路径 |
| DynamicVueComponent named/scoped slots | 首版只做 default slot |
| Cascading value/parameter、动态 `IComponent` type、custom RenderTree frame | 首版不做隐式 runtime 模拟，直接 diagnostic |
| SSR/hydration | 首版只验收 browser client render |
| `Jazor.AspNetCore*` 新集成 | 后续独立评估，不阻塞 browser toolchain |
| Vite/webpack/rspack 实现 | 初版仅 Deno + experimental Netpack |
| 显式 toolchain 选择后的静默 fallback | 破坏确定性；仅 `Auto` 允许 fallback |

---

## 十五、完成定义

本计划完成必须同时满足：

- official Razor SG 原样运行，受控 hook 只消费其最终生成文档，并复用 callback compilation 派生链；
- production call graph 不包含 Razor DR/IR 读取、Razor 原文重建或 nested Razor SG；
- `.razor` 组件只生成 render-function `.mjs`、map 和 manifest；
- C# expression/member/function semantics 通过 `Jazor.Compiler`/`SemanticWalker`；
- RenderTreeBuilder 只走一套 versioned render-context lowering；
- Deno production build、dev server、HMR 和 mixed handwritten SFC browser flow 通过；
- package consumer 不依赖仓库本地工具输出；
- 性能、determinism、source-map 和支持平台有可复现报告；
- unsupported 与 breaking capabilities 有明确诊断或文档，不静默降级；
- 旧线路仍可通过记录的 branch/tag/commit 获取，不在转型分支保留双实现负担。

---

*文档结束*
