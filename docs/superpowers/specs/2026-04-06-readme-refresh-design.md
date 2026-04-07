# README Refresh Design

- 日期：2026-04-06
- 范围：在不大改仓库文档体系的前提下，修订仓库主 `README.md`，使其成为准确、可持续维护、对外友好的入口页。
- 目标：保留现有主结构与英文主文档定位，修正过时信息与错误表述，并用更清晰的状态分层呈现当前仓库能力与工作流。

## 1. 背景与问题

当前主 `README.md` 已具备仓库入口作用，但存在以下问题：

1. 部分内容口径滞后于当前状态文档。
2. “已具备能力 / 正在执行 / 未来保留”三类信息混写，容易让读者误判成熟度。
3. 首页存在容易过时的静态快照与占位信息。
4. 对新访客、潜在使用者、贡献者、维护者四类读者都想兼顾，但当前信息层次不够清晰。

本轮目标不是重写仓库文档体系，而是把主 README 收口为一个准确的 repo-level landing page。

## 2. 设计目标

本次 README 修订应满足以下目标：

1. **对外入口优先**：让第一次进入仓库的读者能快速理解项目是什么、当前处于什么阶段、接下来该看哪里。
2. **状态表达准确**：与当前状态页保持一致，不把已进入执行阶段的工作流继续写成纯 future，也不把实验性方向写成稳定能力。
3. **保守修订**：尽量保留现有 README 的章节骨架，只重写易误导、已过时、对外可读性弱的部分。
4. **混合读者可用**：既服务 GitHub 新访客，也保留贡献者/维护者常用的文档导航与构建测试入口。
5. **降低维护成本**：减少首页中的静态统计和过深细节，把动态内容交给状态页与子模块文档承接。

## 3. 非目标

本轮明确不做：

- 不把主 README 改造成完整状态报表。
- 不全面重写 `README_CN.md`。
- 不顺带统一改写所有子模块 README。
- 不把所有类型映射、语法支持矩阵继续放在首页做长表维护。
- 不新增与当前仓库状态不一致的能力承诺。

## 4. 读者与定位

主 README 的定位是：

- **主文档语言**：英文
- **文档角色**：仓库对外入口页
- **读者范围**：新访客、潜在使用者、贡献者、维护者的混合读者

因此它需要先回答：

1. Jazor 是什么？
2. 当前项目处于什么阶段？
3. 哪些区域是稳定参考？
4. 哪些方向正在推进？
5. 想深入时应该看哪些文档？
6. 如何开始构建和测试？

## 5. 信息分层设计

### 5.1 首页状态分层

README 首页不再仅使用“Key Features / Planned Features”二分法，而改成更贴近当前仓库现实的四层表达：

1. **What Jazor focuses on today**
2. **Stable reference areas**
3. **Active workstreams**
4. **Evolving / future-facing areas**

这样可以更准确地区分：

- compiler 主线作为相对成熟的稳定核心；
- RazorVue、emit / host materialization、sourcemap-related lanes 作为当前活跃工作流；
- 更长期或边界尚未收口的方向作为 future-facing areas。

### 5.2 文档导航分层

`Documentation Map` 保留，但按读者意图重新组织，而不是直接堆叠链接。建议分组：

- **Start here**
- **Current status**
- **Architecture**
- **Subsystem deep dives**
- **Planning and execution material**

这样既保留 repo-level bridge，又提升第一次阅读时的可理解性。

## 6. 章节改写方案

### 6.1 顶部简介

保留标题、badge 和 experimental 提示，但正文开头改成更保守、准确的口径：

- 强调 Jazor 是基于 Roslyn 的 C# → JavaScript compiler project；
- 明确当前仓库是“compiler core + active adjacent workstreams”的结构；
- 避免“高性能、完整支持、全面完成”这类当前难以稳定证明的措辞。

### 6.2 项目状态表达

将现有 `Key Features` 与 `Planned Features` 改写为状态分层节，核心原则：

- 已进入执行中的能力，不再写成纯 planned；
- 尚在探索或边界仍不稳定的方向，不写成已具备；
- 首页只给概要，细节状态统一跳转到 `docs/status/*`。

### 6.3 Documentation Map

保留当前文档枢纽作用，但重构顺序，让用户先看到：

1. 仓库级总入口
2. 当前状态入口
3. 执行/路线入口
4. 架构入口
5. 子系统深入入口

### 6.4 Project Structure

保留现有目录树，但仅作为仓库结构概览，不承担能力承诺。

### 6.5 Core Components

保留主要组件简介，但每个组件只保留 2~4 行说明，突出其职责边界。对容易失真的静态数字、模块完成度统计、阶段性快照不再保留在首页。

### 6.6 Capability Snapshot

把当前过长的：

- `Supported C# Types and Type Mapping`
- `Supported C# Syntax`

压缩为一个更短的 **Current capability snapshot**，只表达：

- 当前编译器主线覆盖的代表性能力；
- 项目强调语义保持与 AST lowering；
- 详细支持范围与边界应参考子系统文档；
- 对仍在推进中的方向保持保守措辞。

### 6.7 Usage / Build / Test

保留对贡献者和维护者最有价值的命令入口，但修正错误与歧义：

- clone URL 改成真实仓库地址；
- prerequisite 口径更谨慎；
- 保留 `scripts/test-dotnet.ps1` 和 `dotnet test` 的常用命令。

### 6.8 Contributing / License / Contact

保留现有结构，仅做轻量清理，避免与仓库事实冲突。

## 7. 已确认需修正的问题

本轮至少修正以下已知问题：

1. `README.md` 中将部分已进入执行阶段的工作流继续写作纯 `Planned Features`。
2. `README.md` 中 `Jazor.CLR` 的静态模块统计缺少当前同步依据，容易过时。
3. clone URL 仍为占位地址，应改为 `https://github.com/devhxj/Jazor.git`。
4. 首页若干能力描述过满，与当前仓库状态页“稳定核心 + 多工作流推进中”的口径不完全一致。
5. README 当前没有显式区分 stable reference、active execution、future-facing materials。

## 8. 校验依据

本轮 README 修订需要优先对齐以下来源：

- `docs/status/2026-04-06-project-workstream-dashboard.md`
- `docs/status/2026-04-06-compiler-mainline-status.md`
- 仓库当前目录结构与现有脚本路径
- 主仓库 remote：`https://github.com/devhxj/Jazor.git`

若 README 旧内容与以上来源冲突，应以当前状态文档和当前仓库实际结构为准。

## 9. 验收标准

完成后，主 README 应满足：

1. 新访客能在首屏快速理解项目定位与当前阶段。
2. 首页不再混淆稳定主线、活跃工作流和未来方向。
3. 文档导航比当前更清晰，但仍保留 repo-level bridge 作用。
4. 不再包含明显错误或无依据的首页静态快照。
5. 贡献者仍能直接找到构建、测试和继续阅读的入口。

## 10. 一句话结论

这一轮不是把主 README 写成功能大全或状态报表，而是把它收口成一个**准确、保守、对外友好的仓库入口页**：先讲清项目是什么和现在处于哪里，再把动态细节交给状态页与子系统文档承接。
