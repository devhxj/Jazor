# GPT-Image-2 架构出图提示词

> Status: 活跃参考
> Positioning: 基于当前 Jazor 仓库结构，为 `gpt-image-2` 生成“方案架构彩图”和“关键节点逻辑图”的统一提示词。
> Scope: 以 `README.md`、`src/Jazor.Compiler/ImplementationPrinciples.md`、`src/Jolt/README.md`、`src/Jazor.RazorVue/README.md`、`src/Jazor.Emit/README.md`、`src/Jazor.Analyzer/README.md`、`src/Jazor.CLR/README.md` 为准；不把 `docs/03-完成/compiler/testing/` 视为当前事实源。

## 当前仓库必须表达的事实

- Jazor 当前有两条活跃技术线：
  - RazorVue：库模式，核心落点 `Jazor.RazorVue`、`Jazor.Analyzer`、`ECMAScript.Vuetify`
  - Jolt：全功能 `.jazor` 开发时宿主，核心落点 `Jolt`
- 共享基础设施包括：
  - `Jazor.Compiler`
  - `Jazor.CLR`
  - `Jazor.Analyzer`
  - `Jazor.Compiler.Generator`
  - `Jazor.CLR.Generator`
  - `Jazor.Emit`
  - `Jazor.Common`
  - `Jazor.Name`
  - `ECMAScript`
  - `ECMAScript.Contract`
  - `ECMAScript.Vue3`
  - `ECMAScript.Pinia`
  - `ECMAScript.VueRoute`
  - `ECMAScript.VueContract`
  - `ECMAScript.Vuetify`
- 编译主链不是“直接把 C# 翻译成 JS 文本”，而是：
  - `Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator/catalog/source-map carriers -> Jazor.Emit`
- `Jazor.Emit` 负责文件物化、manifest、source map、bundle，不拥有 lowering 语义。
- `Jazor.CLR` 是白名单事实来源，经 `Jazor.Compiler.Generator` 生成 `WhiteList.cs.Generate.cs`，再被 `Jazor.Analyzer` 和 `Jazor.Compiler` 消费。
- `Jolt` 是开发时宿主，不拥有编译器 lowering 规则；它通过 workspace、LSP、Roslyn、Razor、Volar/Deno、DevServer、HMR、Build、Debug 组织 authoring 体验。
- RazorVue/Jolt 共享文档与协议 DTO 位于 `Jazor.RazorVue`，不在 `Jolt` 内重复定义。
- `.jazor` 是 Jolt 的第一作者文档；RazorVue 不是 `.vue` SFC-first authoring 主线。

## 出图总规则

- 风格：现代技术架构信息图，清晰、克制、工程感强，接近高质量矢量海报，不是卡通，不是写实机房，不是企业咨询模板。
- 背景：浅色或浅灰蓝工程底图，允许轻微网格或 blueprint 纹理。
- 颜色分层：
  - 深蓝：Jolt 开发时宿主
  - 青绿：RazorVue 库模式
  - 橙色：`Jazor.Compiler` 编译核心
  - 绿色：CLR / WhiteList / Generator
  - 金色：Emit / Bundle / Output
  - 石墨灰：Common / Contract / ECMAScript AST / shared infrastructure
- 文本：尽量短，优先“中文功能名 + 原始项目名”，例如“编译器核心 / Jazor.Compiler”。
- 箭头：必须明确数据方向、调用方向和物化方向，避免装饰性连线。
- 节点数量：总览图控制在 18-28 个关键块；逻辑图控制在 8-16 个节点。
- 视觉重点：边界、层次、输入输出、共享基础设施、双主线分流与汇合。
- 禁止：
  - 不要把 `Jolt` 画成编译器核心
  - 不要把 `Jazor.Emit` 画成语义分析器
  - 不要把 RazorVue 描述成 `.vue` SFC-first 体系
  - 不要把 `docs/03-完成/compiler/testing/` 这类历史测试快照画成当前主结构
  - 不要使用云厂商 logo、人物插画、屏幕 mockup、代码截图拼贴

## Prompt A：方案架构彩图

```text
请生成一张“Jazor 当前方案架构彩图”，横向 16:9，高分辨率，现代技术架构信息图风格，清晰的矢量质感，轻微立体但以工程图表达为主。背景为浅灰蓝或浅米白 blueprint 风格，带极淡网格。整体不是写实海报，不是卡通，不是企业咨询 PPT。

标题：Jazor 当前方案架构图
副标题：C# to JavaScript Compiler + RazorVue + Jolt

必须准确表达下面这组仓库事实：

1. 顶层分成两条活跃技术线：
- 左侧或上侧：RazorVue，标注“库模式 / shared semantic lane”
- 右侧或上侧：Jolt，标注“全功能 .jazor 开发时宿主 / dev-time host”

2. RazorVue 线必须包含这些模块并表现它们的协作：
- Jazor.RazorVue
- Jazor.Analyzer
- ECMAScript.Vuetify
- Jazor.Emit
- 输出 artifact / catalog / .vue / .mjs / manifest / source map
说明：RazorVue 是编译时与共享语义线路，不是 .vue SFC-first authoring 主线

3. Jolt 线必须包含这些模块并表现它们的协作：
- Jolt.Workspace
- Jolt.Lsp
- Jolt.Rpc / Services
- Jolt.Jazor
- Roslyn/InProc
- Razor/InProc
- Volar/Deno
- DevServer
- HMR
- Build
- Debug / Preview
说明：.jazor 是第一作者文档，邻近 .cs / .ts / .js / .css / .html / .vue 共同构成 workspace graph

4. 两条线下方或中部必须有共享基础设施层：
- Jazor.Compiler
  - SemanticWalker
  - AstConverter
  - ESGenerator
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Common / Jazor.Name
- ECMAScript / ECMAScript.Contract
- ECMAScript.Vue3
- ECMAScript.Pinia
- ECMAScript.VueRoute
- ECMAScript.VueContract
- Jazor package

5. 必须画出关键方向：
- Jazor.CLR -> Jazor.Compiler.Generator -> WhiteList.cs.Generate.cs -> Jazor.Analyzer + Jazor.Compiler
- Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator -> catalog / source-map carriers -> Jazor.Emit
- Jazor.RazorVue 与 Jolt 都依赖共享语义或协议层，其中共享 DTO 位于 Jazor.RazorVue，不在 Jolt 内重复定义
- Jazor.Emit 负责真正写出 .mjs / .vue / manifest / .map，并可继续 bundle
- Jolt 负责开发时体验：LSP、workspace routing、preview、HMR、build、debug，不拥有 compiler lowering rules

6. 画面要有明显的分层标签：
- Authoring Inputs
- Shared Semantic / Compile Core
- Dev-Time Host
- Materialization / Outputs

7. 使用稳定的配色：
- 深蓝：Jolt
- 青绿：RazorVue
- 橙色：Compiler core
- 绿色：CLR / WhiteList / Generator
- 金色：Emit / Output
- 石墨灰：Common / Contract / ECMAScript base

8. 图中标签尽量使用“中文说明 + 原始项目名”，例如：
- 编译器核心 / Jazor.Compiler
- 开发时宿主 / Jolt
- 共享语义层 / Jazor.RazorVue
- 白名单事实来源 / Jazor.CLR

9. 右下角放一个小图例：
- 蓝色 = dev-time host
- 青绿色 = library/shared semantic lane
- 橙色 = compiler core
- 绿色 = whitelist/runtime mapping
- 金色 = file materialization/output

避免：
- 不要画成云原生部署图
- 不要出现 Kubernetes、Docker、AWS 图标
- 不要画人物、办公桌、电脑屏幕
- 不要把模块画成过于密集的小字表格
- 不要使用夸张霓虹赛博朋克风
```

## Prompt B：关键节点逻辑图（编译与物化主链）

```text
请生成一张“Jazor 编译与物化关键节点逻辑图”，横向 4:3，信息密度高但可读性强，风格为简洁技术流程图，彩色分层，白色或浅灰背景。

目标：突出从 C# authoring 到最终 .mjs / .map / manifest 的关键节点与职责边界。

图中必须包含并按主流程连接这些节点：

1. 输入作者代码
- C# modules with [ECMAScriptModule]
- 外部 host bindings: ECMAScript.Vue3 / Pinia / VueRoute / Vuetify
- CLR mapping declarations with [Jazor] in Jazor.CLR

2. 白名单生产与消费链
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Analyzer
- Jazor.Compiler
说明：Analyzer 可以更早更严格，Compiler 在 lowering 使用点做最终 runtime-sensitive 裁决

3. 编译主链
- Roslyn IOperation
- SemanticWalker
- ESTree AST
- AstConverter
- ESGenerator
- module catalog / source-map carriers

4. 文件物化链
- Jazor.Emit
- .mjs
- .mjs.map
- manifest
- bundle optional

5. 边界说明必须写在图里：
- Compiler does not directly write files
- Emit does not own lowering semantics
- Unsupported runtime-sensitive external usage should fail explicitly

6. 视觉要求：
- 使用编号 01 到 08 或 01 到 10 标记阶段
- 绿色表示 whitelist/runtime mapping
- 橙色表示 compile/lowering
- 金色表示 emit/output
- 灰色表示 inputs/contracts
- 箭头清晰，尽量单向，少交叉

7. 保留精确术语：
- Roslyn IOperation
- SemanticWalker
- AstConverter
- ESGenerator
- WhiteList.cs.Generate.cs
- Jazor.Emit

不要：
- 不要把流程画成 CI/CD pipeline
- 不要引入无关测试统计、百分比、历史里程碑
- 不要把 Jolt 作为这张图的主角色
```

## Prompt C：关键节点逻辑图（Jolt 开发时三 lane）

```text
请生成一张“Jolt 开发时关键节点逻辑图”，纵向或方形构图，适合展示 `.jazor` 开发体验的三 lane 架构。风格为高级技术说明图，结构清晰，颜色克制，避免卡通。

标题：Jolt 三 Lane 开发时架构
副标题：workspace graph + LSP + preview/build/debug

图中必须准确表达：

1. 输入与工作区
- `.jazor` 是第一作者文档
- 邻近 `.cs`, `.ts`, `.js`, `.css`, `.html`, `.vue` 一起组成 workspace graph

2. 入口节点
- Workspace Resolver
- LSP Session / Router
- RPC / Services

3. 三条 native semantic lanes
- Jazor lane
- Roslyn lane
- Volar / Deno lane

4. lane 下的关键实现
- Jolt.Jazor
- Razor/InProc
- Roslyn/InProc
- Volar/Deno worker

5. 聚合与输出
- diagnostics
- navigation
- preview
- DevServer
- HMR
- build
- debug

6. 共享边界必须体现：
- Jolt 不拥有 compiler lowering rules
- RazorVue / Jolt shared DTO and documents live in Jazor.RazorVue
- transport-based analysis is legacy compatibility path, not target architecture

7. 视觉组织建议：
- 顶部：authoring inputs
- 中部：workspace + routing
- 中下：three lanes 并行
- 底部：preview / HMR / build / debug / editor feedback
- 使用蓝色系作为主色，Jazor.RazorVue shared contracts 用青绿色辅助强调

8. 文本要求：
- 节点标签短而准
- 尽量保留原始模块名，例如 `Jolt/Lsp`, `Workspace`, `Roslyn/InProc`, `Razor/InProc`, `Volar/Deno`
- 对 `.jazor first-authoring` 做醒目标注

不要：
- 不要把 Jolt 画成单一 monolith 黑盒
- 不要把 `.vue` 画成主要 authoring 输入
- 不要把编译器内部 lowering 细节塞进这张图的中心
```

## 可选增强词

需要更像“架构彩图”时，在任一 prompt 末尾追加：

```text
使用高端技术蓝图海报风格，模块块面清晰，少量半等距透视，细致但不拥挤，强调层级、边界、流向与工程秩序。
```

需要更像“关键逻辑图”时，在任一 prompt 末尾追加：

```text
优先流程清晰度而不是装饰，所有箭头必须可追踪，节点文字必须大而准，减少背景噪声，突出步骤编号与职责分层。
```

## 手工修订建议

如果模型对中文小字控制不稳定，优先保留这些精确字符串，再在后期人工补字：

- `Jazor.Compiler`
- `SemanticWalker`
- `AstConverter`
- `ESGenerator`
- `Jazor.Emit`
- `Jazor.CLR`
- `WhiteList.cs.Generate.cs`
- `Jazor.RazorVue`
- `Jolt`
- `Roslyn/InProc`
- `Razor/InProc`
- `Volar/Deno`

## 极简直投版

### 极简 A：方案架构彩图

```text
生成一张 Jazor 当前方案架构彩图，16:9，现代技术架构信息图，浅灰蓝 blueprint 背景，清晰矢量质感，工程感强，不要卡通，不要企业 PPT，不要云原生部署图。

必须表达：
- 两条主线：RazorVue（库模式，共享语义线）与 Jolt（全功能 .jazor 开发时宿主）
- RazorVue 线包含：Jazor.RazorVue、Jazor.Analyzer、ECMAScript.Vuetify、Jazor.Emit、artifact / catalog / .vue / .mjs / manifest / source map
- Jolt 线包含：Workspace、Lsp、Rpc / Services、Jolt.Jazor、Roslyn/InProc、Razor/InProc、Volar/Deno、DevServer、HMR、Build、Debug / Preview
- 共享基础设施层包含：Jazor.Compiler（SemanticWalker、AstConverter、ESGenerator）、Jazor.CLR、Jazor.Compiler.Generator、WhiteList.cs.Generate.cs、Jazor.Common / Jazor.Name、ECMAScript / ECMAScript.Contract、ECMAScript.Vue3、ECMAScript.Pinia、ECMAScript.VueRoute、ECMAScript.VueContract
- 关键方向：Jazor.CLR -> Jazor.Compiler.Generator -> WhiteList.cs.Generate.cs -> Jazor.Analyzer + Jazor.Compiler；Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator -> catalog / source-map carriers -> Jazor.Emit
- 强调：Jolt 不拥有 compiler lowering rules；Jazor.Emit 负责文件物化，不拥有 lowering semantics；.jazor 是第一作者文档；RazorVue 不是 .vue SFC-first 体系

配色：
- 深蓝 = Jolt
- 青绿 = RazorVue
- 橙色 = Compiler core
- 绿色 = CLR / WhiteList / Generator
- 金色 = Emit / Output
- 石墨灰 = Common / Contract / ECMAScript base

图中文字尽量用“中文说明 + 原始项目名”。
```

### 极简 B：编译与物化逻辑图

```text
生成一张 Jazor 编译与物化关键节点逻辑图，4:3，白色或浅灰背景，简洁技术流程图风格，彩色分层，高可读性。

主流程必须包含：
- C# modules with [ECMAScriptModule]
- host bindings: ECMAScript.Vue3 / Pinia / VueRoute / Vuetify
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Analyzer
- Jazor.Compiler
- Roslyn IOperation
- SemanticWalker
- ESTree AST
- AstConverter
- ESGenerator
- module catalog / source-map carriers
- Jazor.Emit
- .mjs
- .mjs.map
- manifest
- bundle optional

必须标注：
- Compiler does not directly write files
- Emit does not own lowering semantics
- Unsupported runtime-sensitive external usage should fail explicitly

颜色：
- 绿色 = whitelist/runtime mapping
- 橙色 = compile/lowering
- 金色 = emit/output
- 灰色 = inputs/contracts

不要画成 CI/CD pipeline，不要加入历史统计或测试通过率。
```

### 极简 C：Jolt 三 Lane 逻辑图

```text
生成一张 Jolt 开发时三 lane 架构逻辑图，方形或纵向，高级技术说明图风格，蓝色系主色，结构清晰，避免卡通。

必须表达：
- `.jazor` 是第一作者文档
- 邻近 `.cs`, `.ts`, `.js`, `.css`, `.html`, `.vue` 组成 workspace graph
- 入口：Workspace Resolver、LSP Session / Router、RPC / Services
- 三条 lane：Jazor lane、Roslyn lane、Volar / Deno lane
- 关键实现：Jolt.Jazor、Razor/InProc、Roslyn/InProc、Volar/Deno worker
- 输出：diagnostics、navigation、preview、DevServer、HMR、build、debug
- 强调：Jolt 不拥有 compiler lowering rules；shared DTO and documents live in Jazor.RazorVue；transport-based analysis is legacy compatibility path

布局建议：
- 顶部是 authoring inputs
- 中部是 workspace + routing
- 中下是 three lanes 并行
- 底部是 preview / HMR / build / debug / editor feedback
```

## 一键可用中文终稿

### 中文终稿 A：方案架构彩图

```text
请生成一张“Jazor 当前方案架构图”，16:9 横向，高分辨率，现代技术架构信息图风格。整体应像高质量工程蓝图海报：浅灰蓝背景，轻微网格纹理，清晰矢量质感，结构分层明显，信息密度高但不拥挤。不要卡通，不要写实人物，不要办公场景，不要云原生部署图，不要企业咨询 PPT 风。

标题：Jazor 当前方案架构图
副标题：C# to JavaScript Compiler + RazorVue + Jolt

图中必须准确表达以下结构：

第一层，顶层两条主线：
- RazorVue：库模式，共享语义线
- Jolt：全功能 `.jazor` 开发时宿主

第二层，RazorVue 线包含：
- Jazor.RazorVue
- Jazor.Analyzer
- ECMAScript.Vuetify
- Jazor.Emit
- artifact / catalog / .vue / .mjs / manifest / source map
并明确说明：RazorVue 是编译时与共享语义线路，不是 `.vue` SFC-first authoring 主线

第三层，Jolt 线包含：
- Workspace
- Lsp
- Rpc / Services
- Jolt.Jazor
- Roslyn/InProc
- Razor/InProc
- Volar/Deno
- DevServer
- HMR
- Build
- Debug / Preview
并明确说明：`.jazor` 是第一作者文档，邻近 `.cs`、`.ts`、`.js`、`.css`、`.html`、`.vue` 共同组成 workspace graph

第四层，共享基础设施层包含：
- Jazor.Compiler
- SemanticWalker
- AstConverter
- ESGenerator
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Common / Jazor.Name
- ECMAScript / ECMAScript.Contract
- ECMAScript.Vue3
- ECMAScript.Pinia
- ECMAScript.VueRoute
- ECMAScript.VueContract

必须画出关键方向：
- Jazor.CLR -> Jazor.Compiler.Generator -> WhiteList.cs.Generate.cs -> Jazor.Analyzer + Jazor.Compiler
- Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator -> catalog / source-map carriers -> Jazor.Emit
- Jazor.RazorVue 与 Jolt 都依赖共享语义或协议层，其中共享 DTO 位于 Jazor.RazorVue，不在 Jolt 内重复定义
- Jazor.Emit 负责真正写出 `.mjs`、`.vue`、manifest、`.map`
- Jolt 负责开发时体验：LSP、workspace routing、preview、HMR、build、debug，不拥有 compiler lowering rules

请给画面添加清晰的层次标签：
- Authoring Inputs
- Shared Semantic / Compile Core
- Dev-Time Host
- Materialization / Outputs

请使用稳定配色：
- 深蓝：Jolt
- 青绿：RazorVue
- 橙色：Compiler core
- 绿色：CLR / WhiteList / Generator
- 金色：Emit / Output
- 石墨灰：Common / Contract / ECMAScript base

图中文字尽量使用“中文功能名 + 原始项目名”，例如：
- 编译器核心 / Jazor.Compiler
- 开发时宿主 / Jolt
- 共享语义层 / Jazor.RazorVue
- 白名单事实来源 / Jazor.CLR

右下角添加简洁图例：
- 蓝色 = dev-time host
- 青绿色 = library/shared semantic lane
- 橙色 = compiler core
- 绿色 = whitelist/runtime mapping
- 金色 = file materialization/output

不要：
- 不要把 Jolt 画成编译器核心
- 不要把 Jazor.Emit 画成语义分析器
- 不要把 RazorVue 画成 `.vue` SFC-first 体系
- 不要使用 Kubernetes、Docker、AWS 图标
- 不要使用屏幕截图拼贴
```

### 中文终稿 B：编译与物化逻辑图

```text
请生成一张“Jazor 编译与物化关键节点逻辑图”，4:3 横向，白色或浅灰背景，简洁技术流程图风格，彩色分层，信息清晰，箭头可追踪，节点文字大而准确。

目标：突出从 C# authoring 到 `.mjs` / `.mjs.map` / manifest 的关键节点与职责边界。

必须包含这些节点，并按主流程连接：
- C# modules with [ECMAScriptModule]
- host bindings: ECMAScript.Vue3 / Pinia / VueRoute / Vuetify
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Analyzer
- Jazor.Compiler
- Roslyn IOperation
- SemanticWalker
- ESTree AST
- AstConverter
- ESGenerator
- module catalog / source-map carriers
- Jazor.Emit
- .mjs
- .mjs.map
- manifest
- bundle optional

图中必须显式写出这些边界说明：
- Compiler does not directly write files
- Emit does not own lowering semantics
- Unsupported runtime-sensitive external usage should fail explicitly

视觉要求：
- 用 01 到 10 左右的阶段编号
- 绿色表示 whitelist/runtime mapping
- 橙色表示 compile/lowering
- 金色表示 emit/output
- 灰色表示 inputs/contracts
- 箭头尽量单向，避免交叉

必须保留这些精确术语：
- Roslyn IOperation
- SemanticWalker
- AstConverter
- ESGenerator
- WhiteList.cs.Generate.cs
- Jazor.Emit

不要：
- 不要画成 CI/CD pipeline
- 不要加入历史统计、测试通过率、里程碑数字
- 不要把 Jolt 作为这张图的主角色
```

### 中文终稿 C：Jolt 三 Lane 逻辑图

```text
请生成一张“Jolt 开发时三 Lane 架构逻辑图”，方形或纵向构图，高级技术说明图风格，蓝色系主色，结构清晰，层级分明，避免卡通和 UI mockup 风格。

标题：Jolt 三 Lane 开发时架构
副标题：workspace graph + LSP + preview/build/debug

必须准确表达：
- `.jazor` 是第一作者文档
- 邻近 `.cs`、`.ts`、`.js`、`.css`、`.html`、`.vue` 一起组成 workspace graph
- 入口节点：Workspace Resolver、LSP Session / Router、RPC / Services
- 三条 native semantic lanes：Jazor lane、Roslyn lane、Volar / Deno lane
- 关键实现：Jolt.Jazor、Razor/InProc、Roslyn/InProc、Volar/Deno worker
- 聚合输出：diagnostics、navigation、preview、DevServer、HMR、build、debug

必须强调这些边界：
- Jolt 不拥有 compiler lowering rules
- shared DTO and documents live in Jazor.RazorVue
- transport-based analysis is legacy compatibility path, not target architecture

布局建议：
- 顶部：authoring inputs
- 中部：workspace + routing
- 中下：three lanes 并行
- 底部：preview / HMR / build / debug / editor feedback

颜色建议：
- 深蓝和蓝灰色用于 Jolt 主体
- 青绿色用于 Jazor.RazorVue shared contracts
- 少量橙色用于辅助强调编译边界，但不要喧宾夺主

不要：
- 不要把 Jolt 画成单一黑盒
- 不要把 `.vue` 画成主要 authoring 输入
- 不要把编译器 lowering 细节放在这张图的中心
```

## One-Shot English Prompts

### English Final A: Architecture Poster

```text
Generate a high-resolution 16:9 architecture poster for the current Jazor solution. Use a modern technical infographic style with a light gray-blue blueprint background, subtle grid texture, crisp vector rendering, strong engineering structure, and clear layering. Avoid cartoon style, office scenes, people, cloud deployment diagrams, and management-slide aesthetics.

Title: Jazor Current Architecture
Subtitle: C# to JavaScript Compiler + RazorVue + Jolt

The image must accurately show these structural facts:

Top level: two active tracks
- RazorVue: library mode, shared semantic lane
- Jolt: full-featured `.jazor` dev-time host

RazorVue lane must include:
- Jazor.RazorVue
- Jazor.Analyzer
- ECMAScript.Vuetify
- Jazor.Emit
- artifact / catalog / .vue / .mjs / manifest / source map
Make it clear that RazorVue is a compile-time and shared-semantic lane, not a `.vue` SFC-first authoring system.

Jolt lane must include:
- Workspace
- Lsp
- Rpc / Services
- Jolt.Jazor
- Roslyn/InProc
- Razor/InProc
- Volar/Deno
- DevServer
- HMR
- Build
- Debug / Preview
Make it clear that `.jazor` is the primary authoring document and nearby `.cs`, `.ts`, `.js`, `.css`, `.html`, and `.vue` files form a workspace graph.

Shared infrastructure layer must include:
- Jazor.Compiler
- SemanticWalker
- AstConverter
- ESGenerator
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Common / Jazor.Name
- ECMAScript / ECMAScript.Contract
- ECMAScript.Vue3
- ECMAScript.Pinia
- ECMAScript.VueRoute
- ECMAScript.VueContract

Show these key directions clearly:
- Jazor.CLR -> Jazor.Compiler.Generator -> WhiteList.cs.Generate.cs -> Jazor.Analyzer + Jazor.Compiler
- Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator -> catalog / source-map carriers -> Jazor.Emit
- Both Jazor.RazorVue and Jolt depend on shared semantics or protocol layers, and shared DTOs live in Jazor.RazorVue rather than being redefined inside Jolt
- Jazor.Emit is responsible for writing `.mjs`, `.vue`, manifest, and `.map` outputs
- Jolt owns the dev-time experience: LSP, workspace routing, preview, HMR, build, debug, but does not own compiler lowering rules

Add clear layer labels:
- Authoring Inputs
- Shared Semantic / Compile Core
- Dev-Time Host
- Materialization / Outputs

Use stable colors:
- dark blue for Jolt
- teal for RazorVue
- orange for compiler core
- green for CLR / whitelist / generator
- gold for emit / output
- graphite gray for common contracts and ECMAScript base

Use labels in the format “Chinese description + original project name” where possible.

Do not:
- portray Jolt as the compiler core
- portray Jazor.Emit as a semantic analyzer
- portray RazorVue as a `.vue` SFC-first system
- use Kubernetes, Docker, AWS, or cloud icons
- use screenshot collage
```

### English Final B: Compile and Emit Logic Diagram

```text
Generate a 4:3 technical logic diagram for the Jazor compile-and-emit pipeline. Use a clean, high-readability engineering flowchart style with a white or light gray background, color-coded layers, clear arrows, and compact but readable node labels.

The goal is to show the path from C# authoring to `.mjs`, `.mjs.map`, and manifest outputs.

The diagram must include and connect these nodes in the main flow:
- C# modules with [ECMAScriptModule]
- host bindings: ECMAScript.Vue3 / Pinia / VueRoute / Vuetify
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Analyzer
- Jazor.Compiler
- Roslyn IOperation
- SemanticWalker
- ESTree AST
- AstConverter
- ESGenerator
- module catalog / source-map carriers
- Jazor.Emit
- .mjs
- .mjs.map
- manifest
- bundle optional

The diagram must explicitly state:
- Compiler does not directly write files
- Emit does not own lowering semantics
- Unsupported runtime-sensitive external usage should fail explicitly

Visual rules:
- use stage numbers roughly 01 to 10
- green for whitelist/runtime mapping
- orange for compile/lowering
- gold for emit/output
- gray for inputs/contracts
- mostly one-way arrows with minimal crossing

Preserve these exact terms:
- Roslyn IOperation
- SemanticWalker
- AstConverter
- ESGenerator
- WhiteList.cs.Generate.cs
- Jazor.Emit

Do not:
- turn it into a CI/CD pipeline
- add historical metrics, pass rates, or milestone numbers
- make Jolt the central subject of this diagram
```

### English Final C: Jolt Three-Lane Diagram

```text
Generate a square or vertical technical diagram for the Jolt three-lane dev-time architecture. Use a refined engineering illustration style, blue-centered palette, strong structure, clear hierarchy, and no cartoon or product-mockup aesthetics.

Title: Jolt Three-Lane Dev-Time Architecture
Subtitle: workspace graph + LSP + preview/build/debug

The diagram must accurately show:
- `.jazor` as the primary authoring document
- nearby `.cs`, `.ts`, `.js`, `.css`, `.html`, and `.vue` files forming a workspace graph
- entry nodes: Workspace Resolver, LSP Session / Router, RPC / Services
- three native semantic lanes: Jazor lane, Roslyn lane, Volar / Deno lane
- key implementations: Jolt.Jazor, Razor/InProc, Roslyn/InProc, Volar/Deno worker
- outputs: diagnostics, navigation, preview, DevServer, HMR, build, debug

It must emphasize these boundaries:
- Jolt does not own compiler lowering rules
- shared DTOs and documents live in Jazor.RazorVue
- transport-based analysis is a legacy compatibility path, not the target architecture

Suggested layout:
- top: authoring inputs
- middle: workspace + routing
- lower middle: three lanes in parallel
- bottom: preview / HMR / build / debug / editor feedback

Color guidance:
- deep blue and blue-gray for the Jolt body
- teal for Jazor.RazorVue shared contracts
- a small amount of orange only for compiler-boundary emphasis

Do not:
- draw Jolt as a single black-box monolith
- make `.vue` the main authoring input
- make compiler lowering details the center of this diagram
```

## 推荐使用顺序

如果你现在就要出图，按下面顺序直接复制：

1. 要一张总览架构彩图：
   - 首选 `中文终稿 A`
   - 如果模型对中文标签稳定性差，改用 `English Final A`

2. 要一张“编译器主链路”逻辑图：
   - 首选 `中文终稿 B`
   - 如果想要更硬的流程约束，改用 `Prompt B`

3. 要一张“Jolt 开发时三 lane”逻辑图：
   - 首选 `中文终稿 C`
   - 如果模型更适合英文技术术语，改用 `English Final C`

4. 只想快速试图，不想读长 prompt：
   - 直接用 `极简 A / 极简 B / 极简 C`

5. 第一张图如果不够准：
   - 先保留结构节点不变
   - 再追加“可选增强词”
   - 最后再改风格，不要先改结构事实

## 最小建议

- 第一次出图，优先用 `中文终稿 A`
- 第二次补链路图，优先用 `中文终稿 B`
- 如果要做一套完整方案图册，再补 `中文终稿 C`
- 如果模型把中文小字画糊了，用 `English Final A/B/C` 出底图，再人工补中文标签

## 可直接发送版

### 直接发送 1：总览架构彩图

```text
请生成一张“Jazor 当前方案架构图”，16:9 横向，高分辨率，现代技术架构信息图风格。整体应像高质量工程蓝图海报：浅灰蓝背景，轻微网格纹理，清晰矢量质感，结构分层明显，信息密度高但不拥挤。不要卡通，不要写实人物，不要办公场景，不要云原生部署图，不要企业咨询 PPT 风。

图中必须准确表达以下结构：

顶层两条主线：
- RazorVue：库模式，共享语义线
- Jolt：全功能 `.jazor` 开发时宿主

RazorVue 线包含：
- Jazor.RazorVue
- Jazor.Analyzer
- ECMAScript.Vuetify
- Jazor.Emit
- artifact / catalog / .vue / .mjs / manifest / source map
并明确说明：RazorVue 是编译时与共享语义线路，不是 `.vue` SFC-first authoring 主线

Jolt 线包含：
- Workspace
- Lsp
- Rpc / Services
- Jolt.Jazor
- Roslyn/InProc
- Razor/InProc
- Volar/Deno
- DevServer
- HMR
- Build
- Debug / Preview
并明确说明：`.jazor` 是第一作者文档，邻近 `.cs`、`.ts`、`.js`、`.css`、`.html`、`.vue` 共同组成 workspace graph

共享基础设施层包含：
- Jazor.Compiler
- SemanticWalker
- AstConverter
- ESGenerator
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Common / Jazor.Name
- ECMAScript / ECMAScript.Contract
- ECMAScript.Vue3
- ECMAScript.Pinia
- ECMAScript.VueRoute
- ECMAScript.VueContract

必须画出关键方向：
- Jazor.CLR -> Jazor.Compiler.Generator -> WhiteList.cs.Generate.cs -> Jazor.Analyzer + Jazor.Compiler
- Roslyn IOperation -> SemanticWalker -> ESTree -> AstConverter -> ESGenerator -> catalog / source-map carriers -> Jazor.Emit
- Jazor.RazorVue 与 Jolt 都依赖共享语义或协议层，其中共享 DTO 位于 Jazor.RazorVue，不在 Jolt 内重复定义
- Jazor.Emit 负责真正写出 `.mjs`、`.vue`、manifest、`.map`
- Jolt 负责开发时体验：LSP、workspace routing、preview、HMR、build、debug，不拥有 compiler lowering rules

请给画面添加清晰的层次标签：
- Authoring Inputs
- Shared Semantic / Compile Core
- Dev-Time Host
- Materialization / Outputs

请使用稳定配色：
- 深蓝：Jolt
- 青绿：RazorVue
- 橙色：Compiler core
- 绿色：CLR / WhiteList / Generator
- 金色：Emit / Output
- 石墨灰：Common / Contract / ECMAScript base

图中文字尽量使用“中文功能名 + 原始项目名”，例如：
- 编译器核心 / Jazor.Compiler
- 开发时宿主 / Jolt
- 共享语义层 / Jazor.RazorVue
- 白名单事实来源 / Jazor.CLR

右下角添加简洁图例：
- 蓝色 = dev-time host
- 青绿色 = library/shared semantic lane
- 橙色 = compiler core
- 绿色 = whitelist/runtime mapping
- 金色 = file materialization/output

不要：
- 不要把 Jolt 画成编译器核心
- 不要把 Jazor.Emit 画成语义分析器
- 不要把 RazorVue 画成 `.vue` SFC-first 体系
- 不要使用 Kubernetes、Docker、AWS 图标
- 不要使用屏幕截图拼贴
```

### 直接发送 2：编译与物化逻辑图

```text
请生成一张“Jazor 编译与物化关键节点逻辑图”，4:3 横向，白色或浅灰背景，简洁技术流程图风格，彩色分层，信息清晰，箭头可追踪，节点文字大而准确。

目标：突出从 C# authoring 到 `.mjs` / `.mjs.map` / manifest 的关键节点与职责边界。

必须包含这些节点，并按主流程连接：
- C# modules with [ECMAScriptModule]
- host bindings: ECMAScript.Vue3 / Pinia / VueRoute / Vuetify
- Jazor.CLR
- Jazor.Compiler.Generator
- WhiteList.cs.Generate.cs
- Jazor.Analyzer
- Jazor.Compiler
- Roslyn IOperation
- SemanticWalker
- ESTree AST
- AstConverter
- ESGenerator
- module catalog / source-map carriers
- Jazor.Emit
- .mjs
- .mjs.map
- manifest
- bundle optional

图中必须显式写出这些边界说明：
- Compiler does not directly write files
- Emit does not own lowering semantics
- Unsupported runtime-sensitive external usage should fail explicitly

视觉要求：
- 用 01 到 10 左右的阶段编号
- 绿色表示 whitelist/runtime mapping
- 橙色表示 compile/lowering
- 金色表示 emit/output
- 灰色表示 inputs/contracts
- 箭头尽量单向，避免交叉

必须保留这些精确术语：
- Roslyn IOperation
- SemanticWalker
- AstConverter
- ESGenerator
- WhiteList.cs.Generate.cs
- Jazor.Emit

不要：
- 不要画成 CI/CD pipeline
- 不要加入历史统计、测试通过率、里程碑数字
- 不要把 Jolt 作为这张图的主角色
```

### 直接发送 3：Jolt 三 Lane 逻辑图

```text
请生成一张“Jolt 开发时三 Lane 架构逻辑图”，方形或纵向构图，高级技术说明图风格，蓝色系主色，结构清晰，层级分明，避免卡通和 UI mockup 风格。

必须准确表达：
- `.jazor` 是第一作者文档
- 邻近 `.cs`、`.ts`、`.js`、`.css`、`.html`、`.vue` 一起组成 workspace graph
- 入口节点：Workspace Resolver、LSP Session / Router、RPC / Services
- 三条 native semantic lanes：Jazor lane、Roslyn lane、Volar / Deno lane
- 关键实现：Jolt.Jazor、Razor/InProc、Roslyn/InProc、Volar/Deno worker
- 聚合输出：diagnostics、navigation、preview、DevServer、HMR、build、debug

必须强调这些边界：
- Jolt 不拥有 compiler lowering rules
- shared DTO and documents live in Jazor.RazorVue
- transport-based analysis is legacy compatibility path, not target architecture

布局建议：
- 顶部：authoring inputs
- 中部：workspace + routing
- 中下：three lanes 并行
- 底部：preview / HMR / build / debug / editor feedback

颜色建议：
- 深蓝和蓝灰色用于 Jolt 主体
- 青绿色用于 Jazor.RazorVue shared contracts
- 少量橙色用于辅助强调编译边界，但不要喧宾夺主

不要：
- 不要把 Jolt 画成单一黑盒
- 不要把 `.vue` 画成主要 authoring 输入
- 不要把编译器 lowering 细节放在这张图的中心
```
