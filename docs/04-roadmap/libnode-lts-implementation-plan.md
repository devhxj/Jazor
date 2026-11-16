# Jazor.LibNode LTS 实施计划

> 状态：方案评估与实施计划，尚未开始原生移植，未发布 `Jazor.LibNode`。核查日期：2026-11-16。对应 [issue #139](https://github.com/devhxj/Jazor/issues/139)，顺序为普通类库导入 #4 → 本计划 → `.jazor` #138。当前产品仍使用微软 LibNode；支持声明以[当前状态](./current-status.md)为准。

## 结论与首个交付

建议推进。自行构建最新 LTS 的共享库，移植微软托管层依赖的 embedding C API，继续使用 `Microsoft.JavaScript.NodeApi` 和现有 `Jazor.NodeHost`。复用 Node 的构建系统、微软的托管测试和原生 embedding 测试，Jazor 只维护移植差异、构建输入、NuGet 包与验收编排。

首个交付是 **Windows x64 的 LTS 原生候选 + 复用上游测试结果 + 实际 NodeApi NuGet 消费者**。先证明该组合，再扩展四个平台和替换产品依赖。新 Node 主版本能解除旧 engine 限制，但不自动证明 embedding ABI、原生 addon、取消或 SSR 行为正确。

## 已核查的基线

| 项目 | 2026-11-16 核查结果 | 实施含义 |
| --- | --- | --- |
| 当前托管包 | `Microsoft.JavaScript.NodeApi 0.9.36`；本地 nuspec 指向提交 `0483aa01a76e88ccb8f9de0b3a89202b538ae41e` | 优先使用该提交的测试，不再把早期研究提交当作匹配基线 |
| 当前原生包 | `Microsoft.JavaScript.LibNode 20.1800.215`，总包依赖七个 RID 包 | 包名替换、运行时资产与工具资产需要同时迁移 |
| 当前实际运行 | Windows x64 进程内探针返回 Node `v20.1800.215`、V8 `11.3.244.8-node.23`、Node-API `9`、module ABI `115`、`shared_library`；JS 与 C# PID 相同 | 微软构建改写了版本标识，不能仅凭 package version 推断具体上游 Node tag |
| 首个 LTS 候选 | [官方索引](https://nodejs.org/dist/index.json)当前首个 LTS 为 `v26.13.1` / Lithium；tag 对应提交 `e9c277643b0249c50f6e6de5413be8a88ea11b6a` | L0 再核查后锁定，消费者构建不解析浮动 latest |
| C embedding API | [Node PR #54660](https://github.com/nodejs/node/pull/54660)仍为 closed/unmerged；官方 `v26.13.1` 没有 `src/node_embedding_api.h` | 需要移植，不能只取官方共享库便声称与 NodeApi 兼容 |
| 可复用原生来源 | [vmoroz/nodejs-node 的固定提交](https://github.com/vmoroz/nodejs-node/tree/3752ce263fab860cb144b7488ee2123fe92614c9)，包含 C API、C++ 辅助层和 embedding 测试 | 是待验证的移植来源，不是已证明与微软发布二进制完全对应的源码 |
| 原生来源缺口 | 微软 nuspec 标注官方 Node 提交 `53a57efd83a18efe7a84bf1b460acc789139939b`，该提交没有 embedding C API；上述 fork 比它多 15 个提交 | L0 必须记录实际采用的 patch 和差异，不能把 nuspec 的上游提交当作完整原生配方 |
| 现有平台证据 | `nodeapi-verify.yml` 验证 Windows x64、Linux x64 glibc、macOS x64、macOS arm64 | 四个平台是首轮验收目标；七个资产目录不等于七个平台都已验证 |

官方 `SHASUMS256.txt` 当前记录 `node-v26.13.1.tar.xz` 的 SHA256 为 `a7f6204ff63b6a2cd274d48903285abb86ed53a99070e532d4c2fd787457d9e6`。这是已读取的官方清单值；本次没有下载源码归档或完成构建，L0 下载后必须实际校验。固定 tag、源码提交、归档摘要与 patch 摘要共同定义候选输入。

## 责任与复用边界

| 层次 | 复用与责任 |
| --- | --- |
| Node、V8、libuv、标准模块、Node-API | 固定的官方 Node LTS 源码和原构建系统 |
| `node_embedding_*` | 从固定原生来源提取必要变更，按所选托管包的导入签名移植；维护可审阅 patch |
| 托管互操作 | 保留微软 NodeApi，不增加一套并行 P/Invoke 或重新封装公共 JS 值模型 |
| 线程、environment、pnpm、Vite、SSR | 继续使用 `Jazor.NodeHost`、Emit 和 ASP.NET Core 的现有所有权模型 |
| 原生包与构建编排 | `Jazor.LibNode` 总包、RID 包、单文件 C# 脚本和官方发版流程 |
| 测试 | 优先运行上游断言和夹具，仅为真实移植、包形状或 Jazor 集成缺口新增测试 |

官方 Node 的 [BUILDING.md](https://github.com/nodejs/node/blob/v26.13.1/BUILDING.md)和[共享库 CI](https://github.com/nodejs/node/blob/v26.13.1/.github/workflows/build-shared.yml)提供工具链依据；不原样引入整套 Nix/CLI CI。原生编译继续调用上游的 Python、Make、MSBuild/VC 构建入口，Jazor 的编排使用 C#。pnpm、Vite、SSR 和应用工具不引入 Node/npm/pnpm/Deno CLI 回退。

## 原生移植的范围

### ABI 契约

普通 `napi_*` 与额外的 `node_embedding_*` 分开验证。前者的 addon 兼容性不能替代后者的平台、runtime 与 event-loop 契约。

从匹配的 [`NodejsRuntime.Embedding.cs`](https://github.com/microsoft/node-api-dotnet/blob/0483aa01a76e88ccb8f9de0b3a89202b538ae41e/src/NodeApi/Runtime/NodejsRuntime.Embedding.cs)提取实际导入清单，与原生头文件和成品导出逐项比较，至少覆盖：

- `Cdecl`、整数宽度、opaque handles、状态码及枚举/flag 数值；原生来源当前声明 `NODE_EMBEDDING_VERSION = 1`，仍须核对托管端实际传入值。
- platform/runtime 创建与释放、配置、参数、错误传播、user data。
- preload/loading/loaded、linked module、task runner 等回调签名、线程及数据释放顺序。
- event-loop run/terminate/run-once/no-wait，以及 Node-API scope 的 open/close/run。

保留现有 loader 需要的 `libnode.dll` / `libnode.so` / `libnode.dylib` 文件名。导出检查通过后，必须用所选托管 NuGet 包实际调用，不能只验证“符号存在”。不把尚未合并的 embedding API 描述成 Node 官方稳定 ABI。

### Patch 提取

已检查的 fork 差异不止三个新增头文件/实现文件，还涉及 `src/api/embed_helpers.cc`、部分 Node-API/V8 句柄处理、`node.gyp`、`common.gypi` 等。L0 按“embedding 必需、构建必需、测试、无关修改”审阅 15 个提交；L1 只移植在新 LTS 上仍必要的部分。`node_embedding_api_cpp.h` 是当前实现的依赖，也需要一起核查。

不要直接复制整条旧 Node 20 fork，或把它作为新 LTS 源码。微软完整打包配方尚未确认；缺失时只补必要的确定性打包步骤，明确记录来源缺口。

## 平台与包设计

### RID 决策

| RID | 目标与约束 |
| --- | --- |
| `win-x64` | 首个开发候选；MSVC 共享库，先保留现有 `CETCompat=false` apphost 条件 |
| `linux-x64` | glibc；在明确的最低用户空间构建，检查 glibc/GLIBCXX、`libatomic` 和 addon 符号作用域 |
| `osx-x64` | 原生 Intel runner；检查最低部署版本、dylib install name、依赖闭包与 addon 架构 |
| `osx-arm64` | 原生 Apple Silicon runner；独立构建和运行，不用 x64/Rosetta 结果替代 |
| `win-arm64`、`linux-arm64`、`win-x86` | 现有包包含这些资产，首轮必须逐一评估能否在 LTS 构建和运行；未验收前不迁移成空目录或继续宣传支持 |
| musl / 其他 RID | 首期不新增；官方 Node 的平台支持不能直接外推为 Jazor embedding/addon 支持 |

Node `v26.13.1` 的 BUILDING.md 指出 macOS 最低基线为 13.5，Linux 官方二进制从 Node 25 起增加 `libatomic` 依赖。自建库仍需从实际链接结果确认这些约束，不能把 `ubuntu-latest` 上编译成功当成最低发行版兼容证明。Windows x86 没有出现在该版本的受支持平台表中，是单独的迁移决策点。

四个平台候选验收完成后，若其余三个 RID 无法保留，先写明资产与部署影响并按版本规则决定发布通道；不能在 MINOR 中静默删掉现有包承诺的运行资产。已有 `CreateLocalPackage_CarriesNodeApiRuntimeAndFrontendHost` 明确断言七个 build-host 资产，须随最终契约调整，而非直接降低断言来让测试通过。

### NuGet 与两个 RID 选择过程

拟采用微软现有分包形式，原生包不新增托管宿主程序集：

```text
Jazor.LibNode
  -> Jazor.LibNode.<rid>
       runtimes/<rid>/native/libnode.{dll|so|dylib}
       native-build.json + LICENSE + NOTICE
```

总包依赖完整的已发布 RID 集合。普通应用部署由 NuGet 根据目标 RID 选库；Jazor 包内 `tools/net11.0/runtimes/<rid>/native/` 则供**构建宿主**选择。Windows 上交叉发布 Linux 应用时，Emit 加载 Windows 库，发布目录包含 Linux 库，两者不能混用。

候选恢复图、包、工具目录和部署目录均检查同名 native 资产的所有者，避免 `Microsoft.JavaScript.LibNode.*` 与 `Jazor.LibNode.*` 同时贡献库文件。隔离消费者实际加载候选库后记录解析路径、文件哈希、Node/V8/Node-API/module ABI 及 RID。

NuGet 版本继续随 Jazor git tag lockstep；Node `26.13.1` 属于上游构建元数据，不作为第二套 NuGet 版本。保留上游 `process.version`，不要为了匹配 Jazor 包版本改写 Node 的运行时版本字符串。

## 实施切片与退出条件

| 切片 | 具体工作 | 交付与退出条件 |
| --- | --- | --- |
| L0：锁定与差异审阅 | 固定 LTS、托管包提交、原生 patch 来源和测试夹具；核对 ABI；确认原生源码/构建差异及七 RID 影响 | `source.lock.json`、ABI/平台矩阵、patch 清单和未决项；所有发布输入均有精确版本/摘要，不使用浮动分支 |
| L1：Windows x64 原生候选 | 用 Node 构建系统生成共享库与 embedding test harness；完成最小必要移植 | 成品能通过复用 C API 测试与真实托管调用；记录实际加载路径/哈希；没有 mainline 依赖替换 |
| L2：微软测试与四平台 | 跑匹配提交的 embedding/GC/适用互操作测试；实际 NodeApi NuGet 消费者；扩展 Linux/macOS | 每个 RID 提交测试发现清单、结果、适配差异和 runtime 身份；未发现测试、退出异常或缺失候选资产都不能成功 |
| L3：分包与产品集成 | 打总包/RID 包；验证 host/target 选择；更新 NodeHost/Jazor 原生引用、许可证和包断言 | 本地隔离包消费者通过；图内只有候选 native 实现；七 RID 迁移决策完成后才替换生产依赖 |
| L4：完整交付与维护 | 现有 NodeApi、CLR、RazorVue、pnpm/Vite、WebIDL、SPA/SSR/HMR 门禁；接入官方发布 | 全部对应门禁成功，公开包闭包完整，文档说明 Node 版本/平台/运行条件；下一个 LTS patch 可沿相同输入和命令更新 |

L1 是首个继续投入的判断点：若 Node 26 的内部 API 变化使当前 embedding 契约无法保持，提交最小失败场景，评估匹配的新 NodeApi 版本或上游修复；不要扩展为整个托管层重写，也不通过旧运行时回退掩盖失败。

## 复用微软测试的执行方案

优先保留上游 xUnit 项目与 fixture，在隔离 checkout 中应用小型适配 patch；不为统一测试框架把整套测试重写成 MSTest。基线为当前 NuGet 对应提交 `0483aa01a76e88ccb8f9de0b3a89202b538ae41e`。

| 来源 | 保留的验收内容 | 集成方式 |
| --- | --- | --- |
| [`NodejsEmbeddingTests.cs`](https://github.com/microsoft/node-api-dotnet/blob/0483aa01a76e88ccb8f9de0b3a89202b538ae41e/test/NodejsEmbeddingTests.cs) | platform/runtime、连续重建、CJS/ESM/包导入、函数调用、Promise/error、worker、托管对象 | 替换 native 包选择与 fixture 路径，保持断言；一进程一个 platform |
| [`GCTests.cs`](https://github.com/microsoft/node-api-dotnet/blob/0483aa01a76e88ccb8f9de0b3a89202b538ae41e/test/GCTests.cs) | 真实 JS/.NET GC、handle 和对象生命周期 | 保留 GC/事件循环等待及最终计数断言，不用一次 GC 的自写 smoke 替代 |
| `JSReferenceTests`、`JSValueScopeTests`、`TsfnCallGateTests` 等 | 引用、scope、线程调用相关回归 | 逐项确认实际使用原生 runtime 还是 mock；mock 不计入 native 兼容证据 |
| fork 的 `test/embedding/embedtest_c_api*.c`、公共头/夹具、`embedtest_main.cc` | C ABI、回调、scope、模块、preload、threading、退出状态 | 构建原生 harness，保留测试源和断言；记录每个测试模式与结果 |
| fork 的 `test/embedding/test-embedding.js` 等 | 现有 native harness 的输入、stdout/stderr/退出码断言 | 优先在候选嵌入环境执行并仅适配 harness/fixture 定位；由 C# 编排原生测试进程，不引入 Node CLI 作为产品工具链 |

上游托管测试项目还引用 `NodeApi.DotNetHost`、generator 和内部 API，不能机械删除全部 ProjectReference 后声称测试仍然等价。因此提供两个独立证据层：

1. **匹配源码的上游测试**：保留所需项目依赖，仅替换 native 资产，验证原有断言与候选兼容。
2. **实际 NuGet 二进制消费者**：引用 `Microsoft.JavaScript.NodeApi 0.9.36` 与本地打包的 `Jazor.LibNode`，不引用托管源码项目，验证公开 embedding/导入/错误/生命周期路径，确认它实际加载候选库。

原微软库与候选的对照运行各用独立进程，不在同一进程初始化两个 platform。所有 skip/exclude 给出具体原因和覆盖影响；保留上游版权、精确提交、适配 patch、测试发现数量及通过/失败/跳过清单。反向“Node CLI 中托管 .NET”的测试不是本包的默认门禁。

## 仓库改动位置

以下是实施时拟新增/修改的位置，**不是已经存在的实现或命令**：

| 位置 | 预期改动 |
| --- | --- |
| `native/libnode/source.lock.json`、`patches/`、`README.md` | 固定源输入、必要移植 patch、采用来源和原生构建说明；RID 配置从同一 manifest 读取 |
| `src/Jazor.LibNode/` | 总包项目，依赖验收通过的 RID 包；无另一套托管互操作层 |
| `src/Jazor.LibNode.Runtime/` | 复用打包工程为每个已声明 RID 生成 native 包，不重复维护多套打包逻辑 |
| `scripts/csharp/build-libnode.cs` | 源码准备、patch 应用、上游 native build 调用、导出与依赖收集；独立输出和缓存目录 |
| `scripts/csharp/verify-libnode.cs` | 上游 C/托管测试、实际 NuGet consumer、候选加载证据与测试报告编排 |
| 原生 CI 工作流 | 每 RID 构建和验证，归档成品、构建记录与必要符号；原生编译采用独立于普通两分钟回归的预算 |
| `Directory.Build.props`、`Jazor.NodeHost.csproj`、`src/Jazor/Jazor.csproj` | 替换原生依赖和 license/tool-native 资产收集，保留集中版本及现有 NodeApi；只有候选通过后修改 |
| `publish-nuget.cs`、包验证与 release workflow | 将总包/RID 包纳入正式 catalog，打包前消费同一源码/patch 对应的已验收 native 成品；缺一个 RID 就失败，不靠 `Exists` 跳过 |
| 现有 runtime/package 测试及文档 | 更新资产拥有者与平台矩阵，保持旧成功场景和调试/发布路径证据 |

原生 build manifest 至少记录 Node tag/commit/源码摘要、patch 摘要、NodeApi 包与测试提交、RID、工具链、构建参数、最低 OS/libc、动态依赖、导出清单与成品哈希。缓存按这些真实输入区分；consumer 不在安装时下载 Node 源码或自行编译。

CI 可跨工作流传递匹配提交的验收成品，最终 NuGet 上传仍只由 `.github/workflows/nuget-publish-ref.yml` 执行。不另建有发布凭据的入口，不要求本地 NuGet key；正式包来源不得是无法追溯的开发机 DLL。

## Jazor 集成与旧 workaround

| 现有行为 | 本轮处理 |
| --- | --- |
| 进程共享 platform、environment 独立线程、inspector flags | 先保持；验证并行创建/销毁、worker、异常后的资源释放，再讨论精简 |
| Windows `CETCompat=false` | 先保持；升级 Node 本身不证明 CET 问题消失 |
| Linux `RTLD_GLOBAL` 提升同一已加载库 | 先保持；用 pnpm/Rolldown addon 实际加载验证，不加载第二份 libnode |
| Rolldown 的 MSVC native-binding 显式路径 | 对当前固定 addon 重验；仅在无需该分支的回归成立后删除 |
| 固定 `lightningcss-wasm` | 先保持同一依赖/锁文件策略；新 Node 不会自动修复 addon 查找主 executable 导出的问题 |
| `Math.f16round` 的 .NET Half 补齐 | 核查新 runtime 原生实现及既有舍入/NaN/Infinity/有符号零回归，通过后才移除旧补齐 |
| 取消与退出 | 保留“JS 线程终止事件循环、等待退出再返回”；不额外承诺同步无限循环可中断或 pnpm 原生任务可强制取消 |

本轮不顺带升级 pnpm、Vite、Vue 或所有 addon。使用相同工具版本验证 runtime 变化，避免同时更换输入而无法判断回归来源。若 `.jazor` 选择 Verter，增加真实 native 包加载/调用证据；`engines >=22` 只能说明版本范围，不能证明可嵌入。

## 集成门禁与发布

可复用的现有命令与工作流：

```text
dotnet run --file scripts/csharp/verify-vite-nodeapi.cs
dotnet run --file scripts/csharp/verify-nodeapi-pipeline.cs
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter FullyQualifiedName~NodeModuleRunnerTests
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter FullyQualifiedName~CreateLocalPackage_CarriesNodeApiRuntimeAndFrontendHost
dotnet run --file scripts/csharp/test-dotnet.cs
```

扩展现有 [NodeApi 四平台 workflow](../../.github/workflows/nodeapi-verify.yml)，加入候选 native ABI、上游测试和包资产门禁。取消/SSR pool/开发服务关闭继续跑现有 `JazorViteProxyTests`、`JazorSsrHostingTests`、`EmitProgressTests`、`StandardPackageProjectTests`，再运行 Compiler/CLR/Style/RazorVue 的对应 runtime 与覆盖率回归。发布前通过隔离 Release 包、真实 Chromium、HMR/PathBase、SPA 和 SSR 消费者。

新增总包和 RID 包属于新增交付，不能放进 PATCH。若可保持既有公开契约与平台资产，使用 MINOR；若移除原有 RID、提高已承诺的最低 OS 或改变公开部署契约，按 [MAJOR 规则](../03-guides/release-and-versioning.md)交付并写迁移说明。计划不预定具体版本号，也不复用此前工具链迁移的单次例外。

完成条件：

- [ ] LTS/ABI/源码与测试输入全部固定，来源缺口有明确结论。
- [ ] 每个声明 RID 通过原生 C API、微软测试与实际 NuGet 二进制消费者。
- [ ] 工具 host RID 与部署 RID 正确，图和产物不包含竞争的微软原生库。
- [ ] 七 RID 的保留或迁移决定明确；最低系统、动态依赖和 addon 约束有证据。
- [ ] 当前 pnpm/Vite/WebIDL、运行时回归和 SPA/SSR/浏览器发布门禁通过。
- [ ] 所有 native 包在同一正式发布闭包内，附完整许可、上游版本与构建记录。
- [ ] 已验证下一次 LTS patch 的更新流程，文档区分新能力、当前限制和未支持平台。

## 资料

- [提案 #139](https://github.com/devhxj/Jazor/issues/139)与[下一阶段](./next-development.md)。
- [Node 26.13.1 固定源码](https://github.com/nodejs/node/tree/e9c277643b0249c50f6e6de5413be8a88ea11b6a)、[官方发行摘要](https://nodejs.org/dist/v26.13.1/SHASUMS256.txt)。
- [原生 embedding 来源](https://github.com/vmoroz/nodejs-node/tree/3752ce263fab860cb144b7488ee2123fe92614c9)、[相对微软 nuspec 所列上游提交的差异](https://github.com/vmoroz/nodejs-node/compare/53a57efd83a18efe7a84bf1b460acc789139939b...3752ce263fab860cb144b7488ee2123fe92614c9)。
- [NodeApi 0.9.36 对应源码与测试](https://github.com/microsoft/node-api-dotnet/tree/0483aa01a76e88ccb8f9de0b3a89202b538ae41e)、[微软嵌入指南](https://github.com/microsoft/node-api-dotnet/blob/main/docs/scenarios/dotnet-js.md)。
- [现有 NodeHost 契约](../../src/Jazor.NodeHost/README.md)、[工具链指南](../03-guides/toolchain-migration.md)、[发版规则](../03-guides/release-and-versioning.md)。
