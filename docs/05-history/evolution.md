# 架构演进记录

> 本页解释当前分层为何形成；当前实现与路线图以现行架构和状态文档为依据。

## 2026-11-19 Jolt 编辑器与组件能力

Jolt 完成后台 npm 声明初始化、跨文件引用和改名、格式化及快捷修复。候选包在 Windows x64 上验证 VS Code 和 Visual Studio 的编辑流程，并在 Chrome 验证本地泛型组件、Reka UI `ListboxVirtualizer`、scoped CSS、热更新和 ES 模块导出。Visual Studio 的实测改名场景覆盖跨文件参数，组件文件改名在 VS Code 中验证。旧实施计划和候选性能数字见 [Git 历史](https://github.com/devhxj/Jazor/blob/v2.2.0-preview.1/docs/04-roadmap/jolt-editor-implementation-plan.md)。发布能力见 [当前状态](../04-roadmap/current-status.md#jolt-作者面)。

## 2026-11-19 自建 LibNode 归档

微软发布 LibNode 26 原生包后，Jazor 改用微软的 LibNode 和 NodeApi。自建 LibNode 的源码与发布记录保存在 GitHub 归档仓库。 [Libnode](https://github.com/devhxj/Libnode)

## 2026-11-15 1.3.0 语法修复与发布

11 月 14 日的语法审计以 `9f0bb79c` 为基线，检查 C# lowering，并创建 133 个独立英文 issue：35 个 bug、82 个 enhancement 和 16 个 documentation。探针包括核心 C#、隔离 render-body 与选定程序的 CLR/NodeApi 对照；它们不代表所有 C# 组合或生产 profile 均已验证。

1.3.0 将 11 项 bug 修正为受支持的运行行为，将其余 24 项收口为源码位置明确的不支持诊断。修复涵盖整数宽度、溢出和零除、默认初始化、表达式包装、模式 switch 控制流，以及动态、引用、接口、枚举器和 awaiter 的协议边界。宿主 Number/BigInt 按 JavaScript 值域判定，CLR 装箱身份仍需可证明。逐项结果保留在[修复记录](../04-roadmap/compiler-syntax-bug-fixes.md)。

本轮本地 Compiler 为 10,876/10,876，行/分支覆盖率 99.38%/97.02%；CLR 为 5,092/5,092。Emit 完整套件为 292 项，主测试 lane 按既有分类运行其中 244 项；独立发布消费者继续验证其余相关交付路径。Release build、API、binding、样例与 SPA/SSR 包消费者门禁通过。候选 API 比较为基线 78,224、当前 78,241，新增 20、删除 3；删除均为已审阅的 TaskName 签名，未批准删除为 0。

[官方 v1.3.0 工作流](https://github.com/devhxj/Jazor/actions/runs/43657840445)完成质量检查、23 包上传与 GitHub Release；35 个 bug 均以独立英文说明关闭。上传成功与 NuGet 公共索引/下载可用性分别核实，不将服务端后续处理状态写成永久产品能力。

## 2026-11-14 普通 NuGet 实现恢复探针

本地研究分别恢复了无宿主标注 DLL 的计算/实例方法，以及真实 `Fastenshtein 1.1.1` 的一个字符串算法。后者手工裁剪无关成员后，9 组输入与原 DLL 结果一致，使用 `ICSharpCode.Decompiler 11.2.0.10050`。这只证明所选方法和输入的可行性，不覆盖 Span 重载、整个包或自动消费者集成。

探针确认原 metadata 符号与重新绑定的源码符号不能仅靠同名程序集身份视为相等，生产路线仍需成员依赖计算、原始/恢复符号对应和共享模块归属。scratch 与日志位于忽略目录，没有作为正式测试分发；不将它们列为可复现发布门禁。后续建议与验收保留在[普通类库研究](../04-roadmap/ordinary-nuget-compilation-research.md)。

## 2026-11-13 1.2.0 类型与标注职责调整

普通源码类型改由编译根与依赖闭包决定实现归属，不再逐类型要求 `[ECMAScript]`。标注保留环境宿主、外部 ESM 与特殊宿主值语义。class 保留类行为，record 表达结构化对象；原 options/callback literal class 的迁移需审查继承和初始化。维护者批准本轮使用 MINOR 作为版本规则例外，破坏性影响仍记入 CHANGELOG。

源码、Analyzer 与生成模板的符号绑定同步调整。当前规则与迁移步骤保留在[类型指南](../03-guides/type-semantics.md)，原实施表与探针通过 Git 历史追溯。

## 2026-11-12 NodeApi 工具链迁移

1.1.0 将维护中的依赖恢复、WebIDL、Vite 和 SSR 迁至进程内 NodeApi + LibNode + 固定 pnpm API，移除 DenoHost/CLI fallback。固定组合、Windows CET 条件、pnpm 布局、取消等待和原生 addon 生命周期由真实工具与发布闭包验证。

四平台组合 CI、Windows SPA/SSR 包消费者和既有 Emit 消费者通过。Vite/SSR TaskName、旧 runtime 路径、WebIDL 参数与锁文件迁移按维护者批准的版本例外交付。稳定运行条件和升级步骤保留在[工具链指南](../03-guides/toolchain-migration.md)，候选版本比较、选型背景、试点和阶段 WBS 通过 Git 历史追溯。

## 2026-10-10 preview.9 作者体验与发布准备

本轮将入口分包、最终编译阶段计时和 N4 数值/union 作者体验整理到 `1.0.0-preview.9`。通用 lowering 使用 Roslyn 分支与返回类型；绑定维护生成器按声明提供 1,665 条参数提示和 22 个嵌套 union 的直接标量投影。跨库矩阵与消费者测试验证这些边界。

本地完整主线通过，最终编译器回归为 10,740/10,740，CLR 为 5,092/5,092，Emit 为 239/239。编译器行覆盖率 17,789/17,895（99.41%）、分支覆盖率 6,974/7,189（97.01%）。Release solution build 为 0 警告、0 错误；Vue binding coverage、绑定 XML 文档、绑定 contract 和发布说明门禁通过。

Release API 快照包含 78,224 个去重签名（9,018 个顶层声明、69,206 个成员声明），相对已审查的 frontend 候选基线 76,566 项新增 1,658 个数值隐式转换签名、删除 0。该候选基线已经包含本轮此前审查的 Number 分页回调与字符串日期组件；它不代表相对公开 preview.8 没有破坏性变化，迁移以 CHANGELOG 为准。刷新机器快照后再比较新增 0、删除 0；上游 binding inventory 未漂移。本地日志与报告位于忽略目录 `.tmp/preview9-release/`、`artifacts/preview9/`，正式发布 ref 由官方 tag workflow 继续验证。

Zero/Jero 的 `.8.12` 本地候选完成 33 个作者文件迁移、0 警告/错误构建、N4 浏览器 4/4 与插件回归 5/5，pageErrors 为 0；业务 API、auth 与 WebSocket 使用 mock。真实后端保存及插件启用后业务仍待联调。本轮不把隔离首次构建、阶段计时或历史 `.8.11` 的 Release 首屏数据当作新的 HMR 或首屏性能样本。

## 2026-10-09 preview.8 开发者反馈验收

Zero/Jero 的迁移反馈推动了组件诊断、wrapper 默认值、lifecycle/fragment、Element Plus typed interaction、原生事件与 payload、Startup 宿主、RID 选择、子路径身份及发布闭包修正。反馈 G1–G34 的逐项复现和早期验收表通过 Git 历史追溯；当前边界由作者指南、Browser Interop 与各 binding README 定义。

当时完整主线为 Compiler 10,728、CLR 5,092、Emit 221，Compiler 覆盖率 99.41%/97.02%。API 基线为 76,564 项，相对前候选新增 445、改变 14；刷新基线后的 0 diff 只说明该次比较。23 包、绑定/XML、typed bootstrap、诊断、参考应用和 Deno 时代的 SPA/SSR 消费者通过，不外推为当前 NodeApi 的证明。

DOM 仿真使用真实 Vue 与 Happy DOM，部分 FileList/TouchList 字段由夹具补齐；真实 Chrome 消费者证明各自目标场景。Element Plus command/sort/service 的部分成功路径使用 stub，不把仿真结果表述为每个事件或服务均已完成真实浏览器验收。

## 2026-09-14 P0/P1/P2 与标准前端项目收口

P0 建立 Golden Path、诊断矩阵、构建/runtime benchmark、binding drift 与完整候选门禁；复杂表格列、typed callback 和组合 slot 由 Authoring 的 source/package/browser 场景验证。clean/incremental/HMR 的早期单轮与多轮计时受预览 SDK 和机器负载影响，只作为当时测量记录，不作为当前性能承诺。

P1 逐步交付版本化 SSR state envelope、typed 认证状态、单一显式构造函数的普通引用服务注入，以及 history handler 的取消、竞态和释放切片。P2 固定内置 UI、IJSRuntime 和 StreamRendering 的 Reject 边界，localization 与复杂 validation 保持 Guidance。完成这些切片不意味着完整 Blazor 或 CLR parity。

标准前端项目收口统一了 `jazor/`、npm/JSR identity、标准 ESM specifier、pnpm lock、Vite/Vue SSR 与细粒度 JS/CSS tree shaking。当前交付规则保留在[类库产物契约](../02-architecture/library-artifact-contract.md)，后续投入统一列在[下一阶段](../04-roadmap/next-development.md)。

## 核心方向的确立

Jazor 始终围绕一项核心能力展开：受支持的 C# 语义转换为 ECMAScript。Roslyn 语义模型、`Jazor.Compiler` 的 ESTree lowering、宿主白名单和 `Jazor.Emit` 的产物交付共同构成这条平台主线。当前文档以它为第一层，并以核心语义定义 Jazor。

## Jolt 的早期探索

早期 Jolt 覆盖开发宿主、语言服务、调试、DevServer 和相关协议探索。自 2.1.0-preview.6 起，Jolt 成为 Jazor 的应用开发主线。

## 未落地的 CSX 提案

CSX 曾提出 `.jazor` TSX-like 作者格式，经 shadow C# 绑定后生成 `.jsx`。该提案已归档，历史详情见 Git 提交记录。

## 路线图的收口

早期路线图曾按模块、阶段和单个消费者分别记录 Artifact Graph、CLR runtime、性能与宿主交付 的实施过程。随着这些工作中的稳定结论进入架构文档、作者指南和当前状态页面，继续保留相互引用的独立计划会让读者难以分辨已交付能力与下一步方向。

因此，路线图收束为“当前状态”和“下一阶段”两份文档：前者陈述已成立的产品契约与验证入口，后者记录待推进的优先级、边界和完成条件。历史方案、阶段清单和当时的测试快照由 Git 历史保存；当前设计以现行架构和指南为依据。

## 历史资料的处理

旧计划、完成清单、测试报告、审计整改、Jolt 细分设计和过期状态快照归档于 Git 历史。当前文档记录理解架构边界所需的结论。

## 2026-08-24 CLR 审查与修复

本轮审查收口了整型解析、零值符号判断、只读集合变更拦截、数值边界与字符串拆分语义，并补齐 `Queue<T>` / `Stack<T>` 核心成员。映射源、生成白名单和回归测试同步更新；Trim 的宿主空白字符集合及浮点换底舍入边界由模块源码说明。原始审计与复评报告由 Git 历史保存，当前支持面以 `Jazor.CLR/module/`、模块参考和自动化测试为准。

## 2026-09-05 Jazor.Compiler 审查与修复

本轮审查以 `Roslyn IOperation -> Acornima ESTree -> JavaScript/source map/ModuleCatalog` 为边界，先记录问题，再实施可验证的最小修复。

| 优先级 | 审查发现 | 处理结果 |
| --- | --- | --- |
| P1 | `file://` URI 形式的编译输入会在 source map 中转换为本地路径，但 `ESGenerator` 的源码内容索引按普通字符串处理 URI，导致 `includeSourcesContent` 静默缺失。 | 源码内容索引对 file URI 统一转换为本地路径后再匹配；新增 URI compilation 回归测试。 |
| P2 | 模块目录和 source map `sources` 使用仅忽略大小写的比较器。当前输入去重覆盖多数相等项，比较器需要完整稳定全序以保持输入顺序确定性。 | 增加 `Ordinal` 二级比较，保持现有大小写不敏感排序语义并明确确定性契约。 |
| P2 | 单个模块 lowering 失败时，其他成功模块仍进入 `ModuleCatalog`；source map 失败则保留 JavaScript 并告警。 | 保持现有渐进式交付行为，并由现有场景测试锁定；这属于需要消费者明确接受的交付策略，不在本轮改成全量失败。 |

后续候选包括扩大白名单结构匹配的边界测试、统一诊断的源位置锚点、建立 optimizer 基准后再评估纯表达式覆盖，以及继续拆分大型 lowering 文件。每项候选以独立的行为契约作为修改依据。

## 2026-09-28 API 与发布接线复核

本轮复核了 P3/Monaco 发布接线、测试 lane 和 API 基线，并重新生成 Release 程序集快照。`compare-public-api.cs` 按 ordinal 去重统计 `- ` 顶层声明与 `  - ` 成员声明：基线和当前均为 76,105 项，其中顶层 8,948 项、成员 67,157 项，新增和删除均为 0。比较器现在会在报告中直接给出这两个分类计数；本节保留该次兼容性结论，逐行报告、当前快照和原始日志保存在本地 `artifacts/quality/review-2026-09-28/`。

`test-dotnet.cs` 的默认集合和项目选择器实际包含 10 个 P3 lane。复核时全部通过，共 115/115 个测试；先前的 8-lane 摘要漏列 `vue-i18n` 与 `monaco`，并非它们未接线。Release solution build 通过（0 warning、0 error），Emit SSR consumer 定向测试 1/1、Vue binding contract gate 和 `git diff --check` 也通过。逐项命令输出与完整当前 API 快照保留在本地忽略目录 `artifacts/quality/review-2026-09-28/`；这些测试结果不替代 P3/Monaco 进入 Support 矩阵所需的 真实浏览器和独立包消费者验证。

绑定 XML 文档门禁随后纳入 `quality-gates.yml` 与独立 Release Candidate stage；Release 输出中的 26 个绑定库均通过，报告为 0 failures。该门禁验证公开声明、枚举值、XML 产物、上游快照和 nuspec 文档文件，并使用仓库内的临时目录与缓存。
