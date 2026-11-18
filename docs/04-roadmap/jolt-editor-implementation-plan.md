# Jolt 编辑器与组件能力实施计划

以 Visual Studio 原生 Razor 的日常编辑体验为参照，优先解决打开项目、连续输入、跨文件导航和安全改名。以 Vue SFC 为参照补齐 `.jazor` 的组件样式隔离，以及本地组件和已发布 npm Vue 组件的强类型用法。计划跟踪 [Jolt #138](https://github.com/devhxj/Jazor/issues/138)。

## 实施基线

| 基线 | 已有能力与证据 |
| --- | --- |
| 已发布的 Jolt 预览 | `2.1.0-preview.4` 提供 CLI 和 VS Code VSIX。本地组件、npm ESM 与公开 `.d.ts`、Vite 开发与打包、作者源码映射已有实现。 [发行记录](https://github.com/devhxj/Jazor/releases/tag/v2.1.0-preview.4) |
| `feature/jolt-visual-studio` 工作树 | 已有 Visual Studio VSIX、Roslyn 语义着色、类型化参数和插槽补全、同项目组件自动导入。Visual Studio 实测候选显示，VS Code 实测应用补全编辑后插入 `@using`。发布能力按实际发行记录认定。 [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md) · [语言服务验证](../../src/Jolt.LanguageServer/verify.cs) |
| 模块契约 | 每个 `.jazor` 投影为 `public static` C# 模块类并生成一个 `.mjs`。公开静态方法、字段和常量沿用 `AstConverter` 的具名导出规则。Vue 组件使用默认导出，`setup()` 创建各自的响应式状态，状态类留在模块内。 [Jolt 作者指南](../03-guides/jolt-authoring.md) · [编译器实现原则](../../src/Jazor.Compiler/ImplementationPrinciples.md) |

## 交付顺序

| 阶段 | 改动范围 | 通过后可以验收的场景 |
| --- | --- | --- |
| 1. 编辑响应与依赖初始化 | `Jolt.LanguageServer`、`Jolt.Project`、`Jolt.Npm`，按复现涉及 `Jazor.NodeHost` | 首次还原期间 C# 和本地组件仍可补全。npm 类型就绪后，同一个编辑窗口恢复候选和声明跳转。 |
| 2. 查找引用与跨文件改名 | `Jolt.Compiler`、`Jolt.LanguageServer`、两个编辑器客户端 | 从本地组件参数跳到声明、查找引用并改名，打开和关闭的调用文件都正确更新。 |
| 3. 格式化、快捷修复与 Visual Studio 工作流 | `Jolt.LanguageServer`、`Jolt.Syntax`、Visual Studio VSIX | 格式化保留渲染文本。未知本地标签可以一键添加 `@using`。从实际解决方案打开文件时也能验证同一语言服务。 |
| 4. 强类型组件与样式隔离 | `Jolt.Npm`、`Jolt.Syntax`、`Jolt.Compiler`、`Jolt.Vite` | 逐项验证 Jolt 泛型和 npm 泛型消费。组件样式在开发和生产资源中都能隔离并正确热更新。 |

阶段 2 的跨文件编辑须基于阶段 1 的一致项目快照。阶段 3 可先完成纯格式化，依赖跨文件绑定的快捷修复接在阶段 2 后。阶段 4 的类型与样式分别验收，各自对照样例的编译、编辑和浏览器表现。

## 1. 编辑响应与 npm 首次初始化

目前诊断和编辑请求各自调用项目加载与分析。npm 元数据读取可以触发依赖还原，独立语言服务测试在首次还原时曾超时，已还原的项目通过了补全与声明导航。先使用同一发布候选 apphost 和同一锁文件分别复现 CLI 构建与 stdio 编辑，记录等待发生在项目扫描、TypeScript 工具准备、pnpm 还原、声明读取或 Roslyn 投影的哪个阶段。 [项目加载器](../../src/Jolt.Project/JoltProjectLoader.cs) · [npm 元数据](../../src/Jolt.Npm/README.md)

1. 将本地源码投影和 npm 元数据就绪分开调度。一个项目的已打开文档共用与文件版本对应的快照，诊断、语义高亮、补全和导航取同一版本的文本与投影。
2. 将 npm 还原和声明更新以项目依赖输入为单位合并，在后台沿用 NodeApi、LibNode 和固定 pnpm API。等待期间先返回本地结果，对受影响的 npm 导入提供就绪状态。声明就绪后重算关联文档的诊断和分类，下次补全读取新结果。CLI 构建仍等待完整依赖和类型检查。
3. 接上文件新建、删除、改名、保存以及 `package.json`、锁文件和实际声明变化的失效规则。过期请求只取消自己的等待，其他文档仍可使用同一后台任务。只发布与当前文档版本匹配的结果。
4. 在固定样例上记录冷启动首次本地补全、npm 首次可用时间以及热态补全和语义着色的 p50/p95。实现前后使用同一 SDK、RID、编辑器版本、项目副本和依赖状态，同时记录是否发生重复还原或重复投影。

**验收**。在无 `node_modules` 的独立副本打开项目，在还原结束前完成本地成员补全和定义跳转。完成还原后，npm 组件参数、语义高亮和 `.d.ts` 跳转在当前会话可用。连续输入、撤销和依赖升级后验证结果版本、诊断刷新以及 CLI 构建结果。使用可写临时目录和独立 pnpm store，分别覆盖目录访问失败与依赖还原失败。

## 2. 按绑定结果查找引用与改名

1. 延续现有 Roslyn 投影和作者位置映射。C# 支持 `@code` 和标记表达式里的源码符号。组件标签按编译器绑定到的组件路径定位，参数和事件按声明定位，具名插槽按所属组件和插槽声明定位。开闭标签和声明位置均使用 `.jazor` 的 UTF-16 原文范围。
2. 先实现 `textDocument/references`，让定义、引用与改名使用同一份绑定结果。再实现 `prepareRename` 和 `rename`，首个可交付切片覆盖本地 C# 符号及本地组件的参数、事件和具名插槽。已发布 npm 声明继续提供导航，其编辑权限由声明所有者决定。
3. 组件名来自文件名，组件改名作为同阶段第二个切片。编辑器支持 `RenameFile` 时，在一个工作区编辑里更新文件和引用它的开闭标签。发现同命名空间冲突或不能完整表达文件操作时，通过编辑器的失败反馈说明原因。跨目录移动会改变命名空间和 ESM 路径，单独定义行为后再验收。
4. 将 edits 应用到测试副本后重新编译，验证同名组件、同名参数、字符串和注释各自保持正确。涉及公开静态符号时，在改名预览中提示具名导出名变化，并用实际 ESM 消费者验证影响。

**验收**。两个命名空间各有一个同名组件。改名只更新绑定到目标的调用。测试覆盖未保存文档、关闭的文件、CRLF、非 ASCII 标识符、开闭标签、参数与插槽，最后在真实编辑器确认改名预览、一次撤销和重新构建。

## 3. 格式化、快捷修复与 Visual Studio 工作流

- **格式化**。以已有 Razor 语言层的源码范围划分 HTML 和 C#。先实现文档格式化，再按编辑器实际用法扩展选区。使用 Roslyn 格式化投影的 C# 片段，按作者范围映射有效编辑。标记缩进依据结构，保留可见文本、行内空白、HTML 实体、字符串、`<pre>` 等保留空白的区域，以及文件原有换行风格。用格式化幂等、格式化前后渲染结果相同和源映射有效作为验收。
- **快捷修复**。为未知本地组件复用自动导入的候选和命名空间冲突检查。先交付插入 `@using`。缺少必填参数时，只有存在合法的同名同类型候选才提供赋值修复。验证修复预览、应用后诊断消失和撤销。
- **Visual Studio**。先用同一候选的 VSIX 和 CLI 验证 Open Folder，再验证从解决方案或项目项打开 `.jazor` 时的项目根定位、文件类型、补全接收、跨文件编辑与格式化。检查文本词汇候选掩盖类型化候选的复现，在标签、参数和插槽位置让合法候选可直接选中。验证接收自动导入时实际写入 `@using` 并触发参数补全。若解决方案场景需要新的项目集成，在独立消费者中验证后再定公开入口。分别在两个编辑器安装各自的 VSIX，记录版本与实际操作结果。 [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md) · [VS Code 扩展](../../editors/jolt/README.md)

## 4. 强类型组件与样式隔离

### 泛型组件

先选择一个可复现的高频消费场景，例如从 `Items` 推断元素类型，并将同一个类型带入回调与插槽。分别做一份本地 `.jazor` 组件和一份已发布 npm Vue 组件探针，按各自证据交付。

- 本地组件由 `Jolt.Compiler` 投影并经 Roslyn 推断或检查类型参数。设计作者语法时同时验证 Razor 解析、未完成输入、显式实参与推断失败时的源码诊断。
- npm 组件由 `Jolt.Npm` 从公开声明提取泛型参数、约束及 props、事件、插槽之间的类型关系，保持与构建所用 ESM 入口对应。先覆盖样例确实使用的类型关系，并在 C# 表达面提供精确的参数和诊断。
- 验收同时检查正确类型、错误参数、错误回调、插槽成员补全与浏览器运行。保留实际用法要求的类型关系，避免用 `object` 或无约束泛型填补信息缺口。

### `.jazor` 组件样式

以同文件顶层 `<style scoped>` 为候选作者面。先做一个只含 scoped CSS 的探针，确认样式块的解析范围、scope ID、DOM 标记和 CSS 选择器能对齐。组件投影和 Vite 通过明确的 CSS 资源交付，CSS 改写优先复用 Vue 已有转换，仍通过进程内工具链。 [Vue SFC CSS 功能](https://vuejs.org/api/sfc-css-features.html) · [Jolt Counter 浏览器验证](../../samples/Jolt.Counter/README.md)

验收用两个组件声明相同 class，验证样式互不影响。另测父子组件根节点、插槽、开发样式热更新、生产 CSS，以及改名或移动后的资源替换。每项按样例实际覆盖的 CSS 语义登记支持范围。

## 回归与交付证据

| 验证 | 现有入口与扩展位置 |
| --- | --- |
| 语法、绑定与 ESM | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt`。新增行为覆盖公开静态成员具名导出、组件默认导出、私有状态、跨文件源码位置与正常编译。 |
| 编辑协议 | `dotnet run --file src/Jolt.LanguageServer/verify.cs -- --editor-only <CLI-apphost>`。在同一协议测试中增加快照版本、查找引用、改名、格式化和快捷修复。省略 `--editor-only` 运行 npm 路径，分别保留首次还原和已还原结果。 [协议验证入口](../../src/Jolt.LanguageServer/verify.cs) |
| 两个编辑器 | Visual Studio 用 `dotnet build editors/jolt-visual-studio/Jolt.VisualStudio.csproj -c Release` 构建 VSIX。VS Code 用现有 VSIX 打包脚本和隔离扩展测试宿主。安装相同候选的 CLI 后打开同一份项目副本。 [VSIX 打包](../../scripts/csharp/pack-jolt-vsix.cs) · [扩展测试](../../editors/jolt/README.md) |
| npm 消费与浏览器 | `dotnet run --file src/Jolt.Npm/verify.cs`、`dotnet run --file scripts/csharp/verify-jolt-package.cs` 和 Jolt Counter 浏览器验证。新增运行时能力要覆盖实际模块导入、开发热更新与生产打包。 [浏览器命令](../../samples/Jolt.Counter/README.md) |

每阶段用可复现样例和一条回归入口结项。记录提交、平台、SDK、CLI 与 VSIX 版本，并把已验证的能力同步到作者指南。实验边界按实际消费、编辑与运行证据更新。 [当前状态](./current-status.md) · [开发与测试](../03-guides/development-and-testing.md)

## 候选验收

2026-11-19 在 Windows x64 和 .NET SDK 11.0.100 上验证，分支为 `feature/jolt-visual-studio`。本地包版本为 `2.1.0-preview.4.local.20261119.3`，Visual Studio VSIX 版本为 `0.1.3`，VS Code VSIX 版本为 `2.1.0-preview.4`。

| 阶段 | 验收结果 |
| --- | --- |
| 冷启动与项目快照 | 修正 Windows 同步 stdin 管道与进程内还原同时运行时的阻塞。空项目、空 pnpm store 的一次运行中，本地定义 47 ms 返回，npm 声明 37.3 s 就绪。使用已有下载缓存和新项目目录时，本地定义与补全分别在 96 ms 和 195 ms 返回，声明 6.54 s 就绪。20 次热态请求的 npm 补全 p50/p95 为 28.7/44.9 ms，语义高亮为 17.3/25.0 ms。协议回归覆盖过期编辑、文件失效、声明就绪刷新及安装失败后保留本地跳转。 [协议验证](../../src/Jolt.LanguageServer/verify.cs) |
| 引用与改名 | 协议回归应用版本化编辑后重新绑定，覆盖同名组件、开闭标签、未保存缓冲区、关闭文件、CRLF、非 ASCII 参数、具名插槽和 ESM 导出提示。VS Code 实测组件文件和两处标签一起改名。Visual Studio 从参数使用点改名后，调用方、声明和组件内引用同步更新，一次撤销恢复两个文件。 [编辑器宿主验证](../../editors/jolt/verify.cjs) |
| 格式化与快捷修复 | 协议回归验证 CRLF、幂等、标记空白及生效后的诊断。VS Code 1.144.1 通过安装版扩展的补全、引用、改名、撤销、格式化和添加 `@using`。Visual Studio 2026 Insiders 18.13.12412.241 验证了文件夹和解决方案的定义跳转，在解决方案中实测格式化、参数改名及快捷修复添加 `@using`。 [Visual Studio 扩展](../../editors/jolt-visual-studio/README.md) |
| 泛型与 scoped CSS | 本地数组泛型和 Reka UI `ListboxVirtualizer` 的回调、插槽、错误诊断和成员补全通过回归。Chrome 157 实测开发和生产资源均可运行，覆盖同名选择器隔离、子组件根节点、`:slotted()`、保留组件状态的热更新和 ESM 具名导入。安装包验收验证了改名后旧 CSS 和模块被替换。 [浏览器样例](../../samples/Jolt.Counter/README.md) · [工具包验证](../../scripts/csharp/verify-jolt-package.cs) |

Jolt 测试通道通过 20 项语法和编译用例。泛型交付范围为单类型参数的集合、回调与插槽场景。组件文件改名限定在同目录，跨目录移动涉及命名空间和模块路径变化，留待单独定义。Visual Studio 的实测改名范围是参数跨文件编辑，文件操作改名的实测客户端是 VS Code。
