# VueHost P1 实施计划

## 已确认事实

- 终局蓝图以 `docs/architecture/vuehost-capabilities.md` 为准：三 Lane、Deno Worker、ProjectionMap/Source Map 分层、单进程 LSP + DevServer。
- 当前实施约束以 `docs/architecture/jazor-vuehost-single-project.md` 为准：`Jazor.VueHost` 是唯一宿主边界，迁移先于删除旧项目，P1 文档范围是 `didOpen/didChange/didClose/completion/hover/definition/diagnostics aggregation`。
- 现状代码尚未达到该约束：
  - `src/Jazor.VueHost/Services/VueHostService.cs` 和 `src/Jazor.VueHost/Jazor/Projection/JazorProjectionService.cs` 仍直接依赖 `Jazor.Vue` / `Jazor.VueContracts`。
  - `src/Jazor.VueHost/VirtualDocuments/Mapping/ProjectionMap.cs` 仅有 `CreateWholeDocument(...)`，尚无段级/字符级映射能力。
  - `src/Jazor.VueHost/Lsp/Routing/DocumentProjectionResolver.cs` 仅按区域粗分 lane，未进行位置重映射。
  - `src/Jazor.VueHost/Lsp/Lanes/FrontendLaneService.cs` 当前大量依赖正则、磁盘扫描和 fallback，本地启发式承担了真实能力。
  - `src/Jazor.VueHost/Lsp/LspSession.cs` 当前对外已暴露 references / rename / codeAction，但这不作为 P1 承诺范围。
- 当前测试已经覆盖部分“附近 `.vue` 发现”和 LSP 行为，但这些行为尚未建立在真实 ProjectionMap + Deno/Volar 主路径上。

## 用户已拍板的约束

- 这次聚焦 **P1 落地范围澄清**，最终产出是 **实施计划**。
- P1 口径 **以文档为准**，不以当前多暴露的协议能力为准。
- 旧资产策略：**先补替代层，再删旧项目**；删除要尽快，但必须以后继能力接管为前提。
- P1 ProjectionMap 要求：**直接做到字符级映射**。
- P1 `.jazor` 能力要求：**要包含跨文件 `.vue` 导航**。
- FrontendLane：**去掉 fallback**，不能再把启发式磁盘/正则兜底当正式路径。
- 质量取向：**接受首个可交付周期更长，优先减少技术债**。
- Roslyn 接入：**P1 先走虚拟 `.cs` 投影**。
- 验收方式：**自动化测试优先**。

## P1 范围定义（锁定）

### 功能范围

P1 只承诺以下能力：

1. `textDocument/didOpen`
2. `textDocument/didChange`
3. `textDocument/didClose`
4. `.jazor` completion
5. `.jazor` hover
6. `.jazor` definition
7. diagnostics aggregation

其中 `.jazor` 范围细化为：

- `@code` 区域：通过 **虚拟 `.cs` + RoslynLane** 提供 completion / hover / definition / diagnostics。
- 标记区（Razor markup / template-facing region）：通过 **虚拟 `.vue` + Deno Worker(Volar + TSServer)** 提供 completion / hover / definition / diagnostics。
- 跨文件 `.vue` 导航：P1 内必须可用，至少覆盖 `.jazor` 标记中的组件标签到实际 `.vue` 文件的 definition/hover 解析。
- 所有 lane 输出必须先映射回 `.jazor`，再由 LSP 层统一返回/发布。

### 明确不承诺

以下能力即使代码中已有入口，也不计入 P1 验收：

- references
- rename
- codeAction
- semanticTokens
- DevServer/HMR/生产构建/Source Map/DAP

处理策略：

- 计划中应把这些能力标记为 **非 P1 / 可暂时关闭或降为未声明**。
- 如果当前协议已暴露，应评估是否先隐藏 capability 宣告，避免“对外承诺 > 实际保证”。

## 核心实施原则

1. **先建真实主路径，再清理兼容层**：先打通 `.jazor -> virtual .vue/.cs -> lane -> aggregate -> map back`，再删除旧依赖与 fallback。
2. **ProjectionMap 先于高级能力**：字符级双向映射是 P1 首要前置，不完成则 completion/hover/definition/diagnostics 的可靠性都不成立。
3. **Lane 输出不得直达客户端**：JazorLane/聚合层负责统一映射与发布。
4. **Deno/Volar 必须成为 FrontendLane 正式实现**：本地启发式 fallback 仅可用于迁移期内部过渡，不可作为 P1 交付路径。
5. **迁移先于删除，但删除要进入本轮计划尾部**：一旦新路径覆盖并有测试兜底，即移除 `Jazor.VueHost -> Jazor.Vue* / Jazor.Vite` 的依赖。
6. **测试矩阵驱动交付**：每个阶段均以新增/修正测试作为门禁。

## 差距清单（当前代码 vs P1）

### A. 架构边界差距

- `Jazor.VueHost` 仍依赖旧项目实现，不满足单项目宿主边界。
- FrontendLane 仍混合“正式路径 + fallback heuristics”，不符合 Deno 唯一路径。
- LSP capability 宣告超出 P1 范围，存在承诺膨胀。

### B. 映射与虚拟文档差距

- ProjectionMap 只有 whole-document 占位。
- 缺少 source->projected / projected->source 的位置、range、edit 双向映射 API。
- `DocumentProjectionResolver` 只做 lane 粗分，不做位置精确路由。
- 聚合器未重映射 projected spans。

### C. Lane 能力差距

- RoslynLane 尚未建立“虚拟 `.cs` 投影 + 工作区同步 + 基于映射返回结果”的可靠链路。
- FrontendLane 尚未建立“虚拟 `.vue` 投影 + Deno Worker + Volar/TSServer”的主路径。
- JazorLane 目前更多是简单宿主服务，不是设计中的协调中枢。

### D. 生命周期差距

- `didOpen/didChange/didClose` 虚拟文档生命周期与 lane 同步尚不完备。
- open/change/close 后的重投影、虚拟文档注册/移除、lane 同步缺少严格一致性设计。

### E. 测试差距

- 现���测试多验证启发式行为，不足以验证真实投影链路。
- 尚缺字符级 ProjectionMap 测试、投影重映射测试、lane 聚合测试、真实 Deno 路径测试。

## 实施阶段

## 阶段 0：收敛 P1 对外承诺与测试基线

### 目标

先把“文档 P1”和“当前公开行为”对齐，避免边实现边继续扩大承诺面。

### 工作项

1. 审查 `LspSession` capability 宣告：
   - 评估是否在 P1 期间暂时关闭 `referencesProvider` / `renameProvider` / `codeActionProvider`。
   - 若暂不关闭，也要在计划中列为非验收项并补 TODO/测试隔离策略。
2. 盘点现有 VueHost 相关测试，区分：
   - 可保留为 P1 基线
   - 依赖 fallback 的测试
   - 应迁移/重写为真实 lane 路径的测试
3. 明确 P1 自动化测试矩阵骨架。

### 退出标准

- P1 测试清单确定。
- 对外 capability 与 P1 承诺的偏差已明确并进入执行清单。

## 阶段 1：建立字符级 ProjectionMap 核心

### 目标

把 ProjectionMap 从 whole-document 占位升级为可支撑 `.jazor <-> virtual .vue/.cs` 的字符级双向映射基础设施。

### 工作项

1. 扩展 `ProjectionSegment` / `ProjectionMap`：
   - 支持 original/projected 的 position/range 映射。
   - 支持 edit span 投影。
   - 支持查询未映射区、边界点、跨 segment 行为。
2. 引入映射 API：
   - `TryMapToProjectedPosition/Range`
   - `TryMapToOriginalPosition/Range`
   - `MapProjectedEditsToOriginalEdits`
3. 设计 segment 生成规则：
   - `.jazor` 标记区到虚拟 `.vue`
   - `@code` 块到虚拟 `.cs`
   - 对插入的桥接/样板内容标记“不可逆”或“宿主插入段”
4. 完成字符级测试：
   - 单段、多段、跨行、多区域混合
   - 前向/逆向映射一致性
   - 边界字符与换行行为
   - 不可逆段处理

### 关键决策

- P1 不接受只做段级粗映射；必须直接落到字符级。
- 对非源码直映射的桥接文本，需要有明确的“不可直接回写”规则。

### 退出标准

- ProjectionMap 具备字符级双向映射 API。
- 对 `.jazor -> virtual .vue/.cs` 的典型片段有稳定测试覆盖。

## 阶段 2：重写 JazorProjectionService，生成真实虚拟 `.vue` / `.cs`

### 目标

从当前 `Jazor.Vue` 编译结果 whole-document 打包，升级为“带字符级映射元数据的虚拟文档生成器”。

### 工作项

1. 重构 `JazorProjectionService`：
   - 输出 `VirtualDocument(VirtualDocumentIdentity, Text, ProjectionMap, Version)`
   - 不再使用 `ProjectionMap.CreateWholeDocument(...)`
2. 明确虚拟 `.vue` 生成边界：
   - 标记区如何投影
   - 非模板区如何占位/剥离
3. 明确虚拟 `.cs` 生成策略：
   - 以虚拟 `.cs` 投影为主路径
   - 保留最小必要上下文，让 Roslyn 有效分析 `@code`
4. 评估旧 `Jazor.Vue` 编译器可复用的最小部分：
   - 可内聚迁入 `src/Jazor.VueHost/Jazor/*`
   - 不可继续直接跨项目依赖
5. 更新虚拟文档注册表与版本同步逻辑。

### 风险

- 虚拟 `.cs` 上下文不足会导致 Roslyn 结果漂移。
- 虚拟 `.vue` 生成若混入过多桥接代码，会显著提升映射复杂度。

### 退出标准

- `ProjectAsync` 返回的两个虚拟文档都带字符级 ProjectionMap。
- `didOpen/didChange` 后可稳定注册和更新虚拟文档。

## 阶段 3：打通 RoslynLane 正式链路（虚拟 `.cs`）

### 目标

让 `@code` completion/hover/definition/diagnostics 走“虚拟 `.cs` → Roslyn → 重映射回 `.jazor`”正式路径。

### 工作项

1. 建立 Roslyn 工作区宿主：
   - 同步虚拟 `.cs` 文档 open/change/close
   - 管理版本和文本更新
2. 重构/补完 `RoslynLaneService`：
   - 输入使用 projected document path + mapped position
   - 输出 diagnostics / hover / completion / definition 时返回 projected 结果
3. 在聚合层将 Roslyn projected 结果映射回 `.jazor`。
4. 新增测试：
   - `@code` completion
   - `@code` hover
   - `@code` definition
   - Roslyn diagnostics 映射回原文
   - didChange 增量更新后结果同步

### 退出标准

- `.jazor` 中 `@code` 的 P1 能力不再依赖旧启发式。
- RoslynLane 的结果可稳定映射回源文件位置。

## 阶段 4：打通 FrontendLane 正式链路（Deno Worker + Volar/TSServer）

### 目标

让标记区能力和跨 `.vue` 导航走真实 Deno/Volar 主路径，并移除当前 fallback 依赖。

### 工作项

1. 明确 Deno Worker 协议：
   - open/change/close projected `.vue`
   - completion / hover / definition / diagnostics 请求
2. 将 `FrontendLaneService` 改为：
   - 主路径仅调用 Deno host
   - 不再以正则/磁盘扫描结果作为正式返回
3. 重新设计“跨文件 `.vue` 导航”来源：
   - 由虚拟 `.vue` 中的真实组件引用关系驱动
   - 而不是 `Regex + EnumerateNearbyVueComponents(...)`
4. 保留的仅有启动/运行失败处理：
   - 这是降级/故障策略，不是功能 fallback
   - 失败时 FrontendLane unavailable，但不返回伪造智能结果
5. 新增/改造测试：
   - Deno Worker 正常时 completion/hover/definition/diagnostics
   - `.jazor` 组件标签到 `.vue` 的 definition
   - Deno 不可用时不返回误导性 heuristics 结果
   - didOpen/didChange 后 projected `.vue` 正确同步到 worker

### 关键约束

- 用户已明确要求 **去掉 fallback**。因此当前 `CreateFilesystemBackedCompletionItems` / `CreateFilesystemBackedHover` / `CreateFilesystemBackedDefinitions` / `CreateFilesystemBackedReferences` 路径必须下线或严格隔离为非正式调试代码。

### 退出标准

- FrontendLane P1 能力全部由 Deno/Volar 主路径提供。
- 不再把本地 heuristics 当产品能力。

## 阶段 5：实现聚合与重映射闭环

### 目标

让 JazorLane/聚合层真正承担“统一映射回 `.jazor`”职责。

### 工作项

1. 增强 `DocumentProjectionResolver`：
   - 返回 lane + projected document uri + projected range/position + mapping identity
   - 不再只返回 lane 粗分类
2. 增强 `LspResultAggregator`：
   - diagnostics：projected -> original
   - completion：必要时补上下文/去重
   - definition：location projected -> user-visible uri/range
3. 调整 `LspSession`：
   - lane 请求传 projected 坐标
   - 最终只发布/返回 `.jazor` 可见结果
4. 明确 lane 输出模型：
   - lane 输出 projected result
   - 聚合层做统一 reproject

### 退出标准

- `completion/hover/definition/diagnostics` 全部经过投影和重映射链路。
- 没有 lane 直接拿原始 `.jazor` 文本位置做伪分析后直接返回客户端。

## 阶段 6：移除旧依赖，收拢到单项目边界

### 目标

在新主路径通过测试后，解除 `Jazor.VueHost` 对旧项目的直接依赖，并准备物理删除旧资产。

### 工作项

1. 删除 `src/Jazor.VueHost` 内对以下项目的直接依赖：
   - `Jazor.Vue`
   - `Jazor.VueContracts`
   - 任何 `Jazor.Vue.Analysis*` 旧边界残留
2. 将仍需要的 parser/projection/contracts 最小集合迁入：
   - `src/Jazor.VueHost/Jazor/*`
   - `src/Jazor.VueHost/Protocol/*`
3. 清理 capability 宣告和过渡代码。
4. 评估并执行旧项目删除：
   - 先删 project reference
   - 再删 solution layout
   - 最后物理移除目录

### 删除门禁

只有同时满足以下条件，才允许删旧项目：

- P1 自动化测试通过
- `Jazor.VueHost` 已不直接依赖旧项目
- `.jazor` 的 `didOpen/didChange/didClose/completion/hover/definition/diagnostics aggregation` 均由新路径提供

## 测试计划

### 1. ProjectionMap 单元测试

必须新增：

- 字符级前向映射
- 字符级逆向映射
- 多 segment 映射
- 跨行/换行映射
- 不可逆桥接段行为
- projected edit 回映射

### 2. 虚拟文档生成测试

- `.jazor` -> virtual `.vue` 文本正确性
- `.jazor` -> virtual `.cs` 文本正确性
- ProjectionMap 与虚拟文档内容一致

### 3. Lane 单元/集成测试

RoslynLane：

- completion
- hover
- definition
- diagnostics

FrontendLane：

- completion
- hover
- definition
- diagnostics
- 跨 `.vue` definition
- Deno 不可用时不返回伪结果

### 4. LSP 端到端测试

覆盖：

- didOpen 发布 diagnostics
- didChange 更新 diagnostics
- didClose 清空 diagnostics
- `.jazor` 标记区 completion
- `.jazor` 标记区 hover
- `.jazor` 标记区 definition 到 `.vue`
- `.jazor` `@code` completion/hover/definition
- lane 混合结果统一映射回 `.jazor`

### 5. 回归策略

- 将现有依赖 fallback 的测试重写为真实投影链路测试。
- references/rename/codeAction 现有测试若保留，应从 P1 门禁中排除，避免误导阶段目标。

## 建议的执行顺序（严格门禁）

1. 锁 P1 范围与 capability 表述
2. 先完成字符级 ProjectionMap
3. 再完成真实虚拟 `.vue` / `.cs` 投影
4. 先打通 RoslynLane
5. 再打通 FrontendLane(Deno/Volar)
6. 再做统一聚合与重映射收口
7. 最后删除旧依赖与旧项目

原因：

- 不先做 ProjectionMap，后续 lane 接入都会返工。
- 先做 RoslynLane 风险更可控，可先验证虚拟文档与映射模型。
- FrontendLane 去掉 fallback 后，必须有真实 Deno 路径才能维持 P1 体验。
- 旧项目删除必须放到最后，否则中途失去回归参照。

## 需要在实施中持续校验的高风险点

1. **虚拟 `.cs` 上下文设计是否足够支撑 Roslyn definition**
2. **虚拟 `.vue` 中桥接代码占比是否导致映射复杂度失控**
3. **Deno Worker 与 Volar/TSServer 的文档同步协议是否支持 `.jazor` 投影场景**
4. **去掉 fallback 后，Deno 未就绪时 P1 体验是否可接受**
5. **跨文件 `.vue` 导航是否真实由 frontend semantics 提供，而非邻近搜索误判**

## 交付物

P1 完成时应交付：

- 新的字符级 ProjectionMap 基础设施
- 真实的虚拟 `.vue` / `.cs` 投影服务
- RoslynLane 正式链路
- FrontendLane(Deno/Volar) 正式链路
- 聚合与重映射闭环
- 调整后的 LSP capability/测试矩阵
- `Jazor.VueHost` 对旧项目依赖解除
- 旧项目删除或具备可立即删除的条件

## 不建议本轮并行推进的事项

以下内容会分散 P1 目标，不应与本轮主线并行：

- DevServer/HMR 重写
- Source Map/DAP
- references/rename/codeAction 正式化
- 生产 bundler/esbuild
- `.css/.html/.json` 扩展能力

## 最终判断

这是一个 **高质量、非最短路径的 P1**：

- 它不是“先把接口搭起来”，而是要直接建立真实投影和 lane 主路径。
- 它的关键不是多做几个 LSP handler，而是先把 **字符级 ProjectionMap + 虚拟 `.vue`/`.cs` + 聚合回写** 建稳。
- 只要这一层建稳，P2 的 references/rename/codeAction 才不会建立在错误基础上。
