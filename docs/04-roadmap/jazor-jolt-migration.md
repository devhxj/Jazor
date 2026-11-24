# Jazor 投影迁出与 Jolt 前端开发调整方案

实施依据是[Jazor 与 Jolt 的职责设定](../02-architecture/jazor-jolt-boundaries.md)。截至 2026-11-24，各阶段按实际消费者的退出条件记录如下。

| 阶段 | 状态 | 消费者验收 |
| --- | --- | --- |
| 1. 独立通用投影 | 已通过 | 纯 Jazor 类库从空缓存生成声明，通过 NodeApi 执行真实 npm 调用；Vue ref 保留读写语义。 |
| 2. 普通 C# 构建与编辑器 | 已通过 | 独立 SDK 与跨 DLL 类型身份已验证。普通 C# 成员 F12 定位到生成声明。真实 VS Code 和装有合并 VSIX 的桌面 VS 均从悬停打开本项目的 `addDays.d.ts`，定位到第 48 行、第 25 列。桌面另验了未保存补全、参数帮助和类型诊断。 |
| 3. Jolt 消费通用投影 | 已通过 | 安装包已移除 `jolt bindings`。普通 C# 和组件对相同重载、回调生成等价导入并执行成功。 |
| 4. 模块交付 | 已通过 | 同一个 NuGet 类库在独立宿主及 Vue 应用中运行。工程引用另行验证热更新、改名清理及 source map 行号。 |
| 5. Vue 开发会话 | 已通过 | 桌面 Visual Studio 用 F5 启动 ASP.NET 与 Jolt，浏览器经代理热更新并保留计数。Shift+F5 后两个服务退出。强制结束附接宿主时，外部 Jolt 继续服务。SPA 和 SSR 均从发布目录运行。 |

## 从哪些代码开始改

| 现有位置与行为 | 调整动作 |
| --- | --- |
| [SDK 构建入口](../../src/Jazor/build/Jazor.Npm.targets) 自动提供当前编译的声明；[Jolt CLI](../../src/Jolt.Cli/Program.cs) 移除了独立绑定命令 | 旧项目去掉专用预构建步骤，保留业务门面。分别验证普通项目与组件项目 |
| [通用元数据读取](../../src/Jazor.Npm/NpmValueMetadataProvider.cs) 解析 npm 值；[Jolt 组件解析](../../src/Jolt.Npm/NpmVueMetadataProvider.cs) 提取 Vue 组件与 SFC 契约 | 普通 npm 值使用锁定的 TypeScript checker。Vue 组件工具由 Jolt 安装，静态国际化消息语法按需加载独立解析器 |
| [值投影](../../src/Jazor.Npm/NpmValueProjection.cs) 与[公开声明](../../src/Jazor.Npm/NpmBindings.cs) 共用签名规则 | 继续审查不透明记录的读写表示及跨程序集类型名。普通字符串索引输入与稀疏输出已有强类型投影用例 |
| [JoltProjectCompiler](../../src/Jolt.Compiler/JoltProjectCompiler.cs) 生成声明树并对组件做多轮 Roslyn 绑定；[语言服务](../../src/Jolt.LanguageServer/LanguageFeatures.cs) 也按成员前缀请求候选 | 保留组件请求发现、模板跨度和 Vue authoring 树。改为读取 Jazor 交付的声明、位置与诊断。逐项核对现有组件专用调用 lowering，已有宿主标注能表达的值调用统一走 Jazor |
| [ESGenerator](../../src/Jazor.Compiler/ESGenerator.cs) 用已绑定的 Roslyn 语义模型生成 catalog；[Jazor 包](../../src/Jazor/Jazor.csproj) 注册 analyzer 和工具依赖 | 将 npm 投影作为编译前输入，覆盖普通构建、设计时项目和公开 DLL 消费。分析器仅报告使用处问题，缓存及生成由同一套 Jazor 投影实现负责 |
| [Emit](../../src/Jazor.Emit/EmitPipeline.cs) 同时物化模块、准备宿主项目、恢复依赖和进行生产构建 | 提取或直接复用通用模块写入路径，供既有独立宿主及 Vue 前端消费。写入范围由消费者拥有；沿用 catalog、map、资源、依赖解析和增量清理 |
| [Jolt.Vite](../../src/Jolt.Vite/tooling/jolt-vite.cjs) 拥有 Vue 插件、组件 HMR、样式和 SPA/SSR 构建脚本；[NodeHost](../../src/Jazor.NodeHost/tooling/toolchain.cjs) 提供通用 Vite 装载与服务生命周期 | 独立模块使用原有通用入口。Vue 开发由 Jolt 发起，ASP.NET 使用已有代理、路径和启动选项 |
| [Jolt SSR 托管](../../src/Jazor.AspNetCore/JoltSsrExtensions.cs) 从应用分发目录读取 Vue 客户端模板与服务端入口 | 保留部署产物的读写约定。通用运行时只提供模块执行；Vue 请求和 hydration 归 Vue 集成。若涉及公开扩展方法或现有输出布局，先审计迁移方式 |

## 实施顺序与退出条件

### 1. 形成独立的通用投影

元数据、声明解析、C# 名称和宿主映射、生成器、定义索引与缓存已经归入内部工程 `Jazor.Npm`，随现有 Jazor 工具交付。普通 `.cs` 与组件使用同一生成实现，分别选择公开宿主标注及组件位置映射。[生成缓存](../../src/Jazor.Npm/NpmProjectionCache.cs)

用 `date-fns` 具名导出 `addDays` 的日期传递、一个本地包的普通记录、字典和泛型回调做首批独立验收。另用声明返回真实 `Ref<T>` 的包证明可选择 Vue 宿主类型。检查生成源码、公开 DLL 的程序集引用、工具实际加载的程序集及 npm 工具依赖。**退出条件**是纯模块消费者无需安装 Jolt 或 Vue，集合、对象和回调能真实执行且保留 JS 身份。Vue 分支继续通过原有读写语义用例。

### 2. 将普通 C# 的使用点接入构建和编辑器

普通 C# 编辑器随 Jazor 的直接包引用加载 Roslyn 补全组件和设计时生成器。VS Code C# 语言服务已验证项目打开后添加和移除普通 npm 依赖。在 Balanced 和 Automatic 下，新增包名、`Exports` 和成员均可在保存 C# 之前补全。Balanced 下普通保存会刷新导入诊断和新成员签名，Automatic 在编辑时刷新。在 Automatic 下，修改已有 `link:` 包中被重导出的 `.d.ts`，打开的 C# 调用就能获得新签名和新的悬停链接位置。`file:` 依赖跟踪已安装副本。要持续跟踪新引入的本地声明文件，仍需项目重新求值以更新文件列表。F12 的目标仍是生成的 C# 声明。[编辑器实现](../../src/Jazor.Npm.Editor/) · [真实语言服务验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

桌面 Visual Studio 2026 Insiders 已用候选 SDK `2.6.0-migration.20261124.12` 打开普通类库。Automatic 下，从空 C# 文件输入 `using Dates = Npm.DateFns.Exports;` 和实际调用，磁盘文件哈希不变。`Dates.` 补全包含 `AddDays`，悬停显示 `Date Dates.AddDays(Date date, double amount)` 及另外两个重载。成员 F12 直接打开 `Jazor.Npm.Editor.g.cs` 对应方法。默认 Balanced 下，保存一次后参数帮助可查看三条重载，同一成员 F12 也可打开。生成声明的 XML 链接指向当前安装的 `date-fns/addDays.d.ts#L48,25`。桌面原生 Quick Info 显示此链接，但该版本的 VS 只处理 HTTP(S) 目标，点击本机文件不会打开声明。[桌面复验步骤](../03-guides/jolt-authoring.md#在桌面-visual-studio-验收普通-c-编辑反馈)

编辑器现在使用项目 SDK 的 `NetCoreRoot` 启动投影工具。将真实 Roslyn 语言服务切到桌面 VS 自带的 .NET 10 运行时后，旧候选 `.9` 无法补全首次使用的 npm 成员；`.12` 在相同条件下通过了冷补全、类型检查、F12 和重导出悬停位置刷新，工具实际使用项目的 .NET 11。[语言服务对照](../../scripts/csharp/verify-jazor-npm-editor.cs) · [宿主启动实现](../../src/Jazor.Npm.Editor/NpmHostProcess.cs)

在隔离的 Windows VS Code 配置中，使用 C# 扩展 11.7.18 和同一候选 SDK `.12`，把鼠标移到编辑器中已绑定的 `AddDays` 调用上。点击实际渲染的 `Open TypeScript declaration` 后，编辑器打开本项目 `node_modules/date-fns/addDays.d.ts` 标签页，状态栏显示第 48 行、第 25 列。

在桌面 Visual Studio 2026 Insiders 的独立配置中，仅安装正式组合包 `Jolt.VisualStudio.vsix`（原扩展 ID `Jazor.Jolt.VisualStudio`，版本 `0.1.9`）。点击同一普通 C# 调用处悬停里的 `Open addDays.d.ts at line 48`，VS 打开项目实际安装的 TypeScript 文件，状态栏显示“行: 48，字符: 25”。Jazor 的导航组件与原有 Jolt 语言客户端是同一包内分别注册的两个 MEF 组件。[安装和复验步骤](../../editors/jolt-visual-studio/README.md) · [扩展清单](../../editors/jolt-visual-studio/source.extension.vsixmanifest)

再次启动桌面 VS 时，只给该进程指定空的 `DOTNET_CLI_HOME`，确认其工具目录没有 `jolt.cmd`，普通类库中也没有本地工具清单。悬停点击仍打开第 48 行、第 25 列，随后从同一调用按 F12 进入 `Jazor.Npm.Editor.g.cs` 的 `AddDays`。本次会话没有启动新的 Jolt 进程。

同一组合包另行打开真实样例 `Counter.jazor`。隔离的工具目录指向候选 Jolt `.11`。Visual Studio 在打开组件后启动 `jolt lsp`，关闭该实例后语言服务器退出。[Visual Studio 客户端](../../editors/jolt-visual-studio/JoltLanguageClient.cs)

在桌面 Balanced 中，保存正确调用后把第二个参数从 `2` 改为 `true` 并保持未保存，Error List 在 `Calendar.cs` 调用行报告活动诊断 `CS1503`，说明该参数要求 `double`。磁盘中仍是原先的有效调用。

已用候选包 `2.6.0-migration.20261124.9` 验证普通 C# 子项目依赖同一 pnpm 工作区的兄弟包。SDK 从共享根锁构建并安装 `workspace:*` 链接。真实 Roslyn Automatic 在调用方 C# 缓冲区未保存时观察到兄弟包参数从字符串改成布尔值，报告 `CS1503`，恢复声明后错误清除。只从共享锁移除本项目的 importer 后，编辑器触发 pnpm 补回链接，悬停仍给出 `Inspect(string value)`。该编辑器验收在打开工程前执行了一次普通构建。[工作区回归](../../scripts/csharp/verify-jazor-npm-editor.cs)

另有一个从零开始的编辑器场景。类库打开时没有 npm 清单或锁文件，作者先输入 `using`，再在项目目录首次创建两个文件。候选包在设计时监听这两个确切路径。Roslyn Automatic 在原 C# 缓冲区仍未保存时清除命名空间错误并提供类型签名；Balanced 可在保存前补全包和成员，保存一次后获得新签名。在普通命令行构建中，没有清单的项目继续跳过投影目标。[首次创建清单验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

项目打开时若 `package.json` 只写了一个左花括号，候选 SDK `2.6.0-migration.20261124.5` 会在枚举设计时输入时失败，随后修复清单仍得不到包补全。`.6` 将该文件保留在 Roslyn 的监听列表。在同一个 C# 编辑会话中补全清单并写入锁文件后，Automatic 和 Balanced 都能在保存 C# 前补全包、`Exports` 和成员。Automatic 随即给出 `AddDays` 的参数类型，Balanced 在保存一次 C# 后给出签名。[修复清单的语言服务验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

子项目最初使用父目录的 npm 工作区时，项目内新建的清单会取得依赖解析权。已用实际 SDK 包和 Roslyn Automatic 验证三个连续状态：父清单提供 `Hot.Inspect(string)`，子清单生效后只提供 `Dates.AddDays`，删除子清单后原来的父包签名和补全恢复。整个过程中父包调用所在的 C# 文件保持未保存；独立的磁盘构建在相同状态下选到相同的依赖。[嵌套清单验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

跨文件验收打开两份 C# 文档。一份调用未限定的 `AddDays`，另一份加入 `global using static Npm.DateFns.Exports;`。Automatic 在两份文档均未保存时绑定到 date-fns 成员，Balanced 在保存导入文件后完成绑定。在两种模式下，从缓冲区删除导入后，调用处都会在保存该次删除之前重新报告 `CS0103`。[多文档编辑器验收](../../scripts/csharp/verify-jazor-npm-editor.cs)

当同一文件同时调用可投影的函数和返回 `symbol` 的函数时，构建和普通 C# 编辑器都能在失败成员处报告 `JAZOR`，消息包含失败的返回类型及原始声明的文件、行和列。编辑器仍可显示另一个函数的准确签名。候选包已在 Automatic 的未保存缓冲区及 Balanced 保存后验证移动调用时诊断跟随，删除失败调用后诊断清除；元数据夹具另行覆盖不支持的具名参数。[编辑器回归](../../scripts/csharp/verify-jazor-npm-editor.cs) · [签名与声明位置](../../scripts/csharp/verify-jolt-public-projection.cs)

1. 在 SDK 已求值的 `Compile` 输入上使用 Roslyn 读取 `using`、别名、`global using`、全限定引用以及成员使用。纳入各文件实际启用的条件编译分支，覆盖跨文件别名、重名本地变量和已有程序集提供的声明。以包的 exports 定位模块，先列名，按调用选择类型闭包。没有 npm 使用的项目在此短路，避免启动 JS 工具宿主。
2. 为普通构建选用一个明确的生成次序：发现使用点，生成或命中缓存，将声明文件加入本次 `Compile`，随后 Roslyn 与现有 `ESGenerator` 读取同一语义模型。新增、删除导出或更改锁文件后重新计算本次输入集，过期生成文件不再进入编译。冷构建可用仓库固定的 pnpm API 还原作者已锁定的依赖，恢复失败时保留原始诊断。
3. 设计时复用同一发现器和缓存，将未保存的文档版本作为覆盖输入。在受支持的 C# 编辑器中实测 `Dates.` 初次补全、参数帮助、错误恢复、跨文件 `global using` 和跳转到原始声明。首次补全从轻量导出索引提供候选，避免为所有候选展开签名。取消旧请求并用文档版本丢弃过时结果。若必须注册编辑器组件，列明 IDE、分发方式与用户操作后审计。
4. 明确 Roslyn 约束。普通增量生成器之间无法依赖注册顺序。仓库锁定的 Roslyn 提供 `RegisterPreCompilationSourceOutput`，可把声明加入初始 compilation，但其输入禁止依赖 compilation 或语法节点。它单独无法发现 `using`。基线路线使用上面的编译前输入，实验性 API 不作为安装前提。Roslyn 程序集与 NodeApi 工具宿主各自按实际 TFM/RID 打包。[锁定版本](../../Directory.Build.props) · [ESGenerator 输入](../../src/Jazor.Compiler/ESGenerator.cs)

**退出条件**是一份干净的普通 SDK 类库，只有项目、源码、包描述和锁文件，首次 `dotnet build` 即可完成；检查 DLL 的 catalog 并执行导入。第二个类库只引用第一份 DLL，能用公开契约调用相同 JS 导出。两个 DLL 各自公开同一包的不同导出时，覆盖相同记录参数跨库传递、来源冲突及重复定义错误。IDE 冷启动测试与无 IDE 的构建测试分别取证。

全局引用生产者时，消费者可直接调用生产者 DLL 中已有的 npm 导出。使用 `extern alias` 分别引用两份生产者 DLL 时，消费者还能为同一包生成本地投影。若业务 API 必须跨库接收同一个命名记录，两个生产者都引用定义投影的普通契约类库。已用本地候选包验证两个生产者和最终消费者的类型引用指向同一份契约 DLL，ESM 跨库传递后仍保留对象身份。独立编译且各有类型定义的二进制在强类型实参处报告 `CS1503`，迁移需重新编译生产者。[共享契约示例](../03-guides/jolt-authoring.md#在多个-dll-之间传递同一条-npm-记录) · [程序集元数据与实际 ESM 验收](../../scripts/csharp/verify-jazor-npm-library.cs)

同一消费者若全局引用两个都公开 `Npm.WireCells.Exports` 的生产者，候选 SDK `.12` 在实际调用这一类型的位置仅报告一条 `CS0433`，其中列出两个程序集。使用 `extern alias` 明确区分两者的消费者通过编译并执行了相应 npm 调用。[双全局引用与别名验收](../../scripts/csharp/verify-jazor-npm-library.cs)

### 3. Jolt 改为消费投影，删除公开命令

将组件构建、项目快照和语言服务的普通值请求切到阶段 1 的投影结果。`.jazor` 产生对模块、成员和源码跨度的请求即可，无需为虚拟 C# 文档启动一套普通项目扫描。Vue 的 props、emits、slots、本地 `.vue`、TSX 互操作与模板补全继续由 Jolt 校验。两端调用同一 npm 函数时，比对 ESM specifier、导出名、重载、回调约定和返回类型；不同模块包装可分别生成。

在同一交付切片中删除 `jolt bindings`、帮助和相关公开入口。把依赖该命令的脚本改为构建真正的普通 C# 消费者。旧项目迁移时先盘点手写扩展和显式命令导出的成员，保留作者拥有的代码；删除专属的预构建调用及其整份可再生输出，改用 `using` 后执行干净构建。过去专门生成绑定 DLL 并公开尚无本地调用的导出时，使用公开 C# 门面显式表达要交付的 API，并用下游项目核对类型身份和调用。若确实需要稳定公开某个外部类型而现有 C# 入口无法表达，单列公开契约审计。

**退出条件**是独立安装的 Jolt 工具帮助不含该命令，直接调用旧命令进入未知命令路径；普通 C# 和 Vue 项目均能从空缓存构建。同步作者指南、CLI README、打包验收、本机技能说明和对应版本的破坏性迁移记录。[公开投影夹具](../../scripts/csharp/verify-jolt-public-projection.cs) · [安装包验收](../../scripts/csharp/verify-jolt-package.cs)

已从隔离的本地包源安装 Jolt，验证帮助、旧命令退出、组件构建及打包。普通 SDK 和 Jolt 的调用方使用同一段 C#，选择本地包的两个同参数数量重载，其中一个通过回调推导泛型结果。两个实际 ESM 均从 `projection-fixture` 导入 `compareProjection` 并返回 `doubled/6`，Vue 服务端渲染输出同样的文本。七库对照覆盖 177 个导出名；246 个调用形状中有 3 个 VueUse 项在锁定版本中声明为布尔常量。[等价调用与运行](../../scripts/csharp/verify-jolt-public-projection.cs) · [参考库统计口径](../../scripts/csharp/verify-jolt-binding-projection.cs)

### 4. 让已有模块交付进入 Vue 应用的模块图

从最终消费者获取 .NET 已解析的实现程序集和资源清单。复用 Emit 的模块收集与写入逻辑，将 ESM、map、所需 CLR 模块和声明资源交付到应用构建拥有的可观察位置。以原始 specifier 传递第三方导入。对独立 Jazor 宿主验证 Emit 能从其既有资源或包元数据恢复依赖；对 Vue 应用验证 Vite 用作者的包描述和锁文件解析相同导入。若投影带来的 npm 包身份尚未随普通 DLL 到达消费端，优先复用已存在的 package metadata 和 manifest locator；任何 catalog schema 扩展单独审计。[资源契约](../02-architecture/library-artifact-contract.md) · [资源解析](../../src/Jazor.Emit/LibraryMaterializer.cs)

初版从显式已知的项目引用和实际 MSBuild 输出交付，覆盖首次构建及重建，不扫描相邻 `bin` 按文件名猜根。如果独立 `jolt dev` 只有一个前端目录，缺少关联 .NET 项目或已交付模块的信息，需要在审计中决定接入方式。该缺口不能通过修改类库 `JazorDir` 默认语义解决。

已验证两种交付。使用项目引用时，一次 `dotnet watch` 会话覆盖首次构建、类库源码保存和模块改名。保存后宿主自动更新前端子目录的模块与 map，Chromium 页面保留状态并显示新值；改名后旧模块和旧 map 均已清理。使用包引用时，两个独立宿主安装相同哈希的生产者包，执行投影的 `date-fns` 和 CLR 字符串调用。独立宿主还原生产者的 4.4.0，Jolt 应用锁定 4.1.0。两套依赖同时存在时，开发和生产页面均验证从 DLL 导出的 `addDays` 与前端直接导入的是同一个函数。夹具另行验证写入失败后重试、自定义目录、`none/debug/release` 及 Folder Publish。[工程布局](../03-guides/jolt-authoring.md#在-vue-应用中使用普通-jazor-类库的模块) · [项目引用热更新](../../scripts/csharp/verify-jolt-mixed.cs) · [包引用交付验收](../../scripts/csharp/verify-jazor-module-handoff.cs)

**退出条件**是同一个打包后的 C# 类库既可供独立 Jazor 宿主运行，也可供 Vue 应用导入。依赖模块、map 和 CLR 资源齐全；删除或重命名模块可清理自有过期文件，失败重试不会覆盖作者的入口、锁文件和无关资产。测试既有 MSBuild 的空／`none`、`debug`、`release`、自定义目录和 Folder Publish。

两种交付方式都检查了实际源码映射坐标。在项目引用中，首次构建及热更新后的 JS 字符串都映回 `Greeting.cs` 的返回行。安装类库包后，从生成模块的 `addDays` 调用位置解码其 map，可以定位到包内 `Calendar.cs` 的对应调用行。[项目引用映射](../../scripts/csharp/verify-jolt-mixed.cs) · [包引用映射](../../scripts/csharp/verify-jazor-module-handoff.cs)

### 5. 统一 Vue 开发会话，保留通用模块运行能力

Jolt 统一启动编译、Vite 与 Vue 插件。已有前端运行时核心留在 NodeApi 宿主。将通用模块的运行与错误报告从 Vue 插件的加载和模板更新判定中解开。ASP.NET Core 接入先验证“连接已有 Jolt 会话”，再验证“F5 启动同一 Jolt 会话”。两种方式共用就绪探测、代理和 WebSocket 转发。验收时从被代理的应用请求一个真实的 `.jazor` 组件并完成一次热更新，能返回入口页面的 HTTP 200 只能证明服务器可访问。连接者只释放自己持有的资源。控制台、IDE 和宿主的停止或取消均应能释放自己创建的服务。

让 `.jazor`、普通 `.cs` 和 SFC 的修改分别沿自身路径更新。Vite 可观察模块输出，Jolt 监听源输入、相关声明及依赖配置。当前 `.jazor` 临时目录排除规则不能一并吞掉最终 ESM。一次修改同时涉及共享 C# DTO 和 Vue 组件时，等受影响的产物都已就绪再向浏览器提供一致版本。生成文件写入和 pnpm 更新锁文件时，测量实际重编译次数，排除监听回环。

**退出条件**是独立 Jazor 模块开发无需 Vue；Vue CLI 与 F5 各自能启动、刷新和退出，宿主连接已有会话时没有第二个 Vite。分别修改模板、C# 类库、SFC prop 和 npm 声明，在真实浏览器观察 DOM、状态和错误恢复。验证子路径、懒加载、source map、Vue SPA 单次打包和 SSR 客户端／服务端各自的输出。宿主发布目录须能在开发服务器关闭后运行。[开发宿主回归](../../scripts/csharp/verify-development-hmr.cs) · [混编验收](../../scripts/csharp/verify-jolt-mixed.cs) · [SSR 验收](../../scripts/csharp/verify-jolt-ssr.cs)

Windows 上的真实宿主已分别连接外部 Jolt 和启动已安装的本地 Jolt 工具。Chromium 通过带路径前缀的 ASP.NET HTTP 与 WebSocket 代理更新组件，保留 Vue 父组件状态。关闭附接宿主后外部会话继续服务，正常停止会话所有者后可重新绑定 Vite 端口。[宿主示例](../../samples/Jolt.AspNetCore/) · [代理及端口验收](../../scripts/csharp/verify-development-hmr.cs)

独立 CLI 的控制台退出也已在 Windows 验证。在隔离控制台启动 `jolt dev` 并请求 `.jolt/Counter.mjs` 后，发送系统的 `CTRL_C_EVENT`。夹具的 Vite 插件分别在 HTTP 服务器的 `close` 事件与 `closeBundle` 钩子记录了该 CLI 的 PID。进程以退出码 0 结束后，夹具立即在原地址成功绑定同一端口。

在同一端口先运行返回 HTTP 503 的监听器，独立 CLI 和由 ASP.NET 启动的本地 Jolt 都报告 Vite 端口已占用。释放端口后，新启动的宿主用原地址成功提供页面并完成浏览器热更新。现有附接方式按配置的 URL 和 HTTP 就绪响应识别会话。[占用端口与重试验收](../../scripts/csharp/verify-development-hmr.cs)

桌面 Visual Studio 2026 Insiders 在隔离配置中用 F5 启动 ASP.NET 样例。宿主使用本地安装的候选工具 `2.6.0-migration.20261124.11` 并持有其标准输入。Chromium 经 ASP.NET 的 WebSocket 代理连接，点击计数器后保存 `Counter.jazor`。页面显示新标题、计数仍为 1，页面实例保持不变。按 Shift+F5 后，ASP.NET、Jolt 和 `dotnet tool run` 均已退出，两个端口均已释放。[桌面复验步骤](../../samples/Jolt.AspNetCore/README.md)

一次共享 C# 源码保存更新两个组件。在该夹具中 Vite 发出两条模块消息，已打开页面只观察到一次 DOM 变更，两处同时显示新值。项目目录外的 `link:` 包将重导出改指向新文件后，Jolt 会监听新声明，保存旧声明不触发编译。另一个独立包只通过本地 SFC 的 `defineProps` 引入类型。仅把该包的 `props.d.ts` 从字符串改成数值，调用组件的 `App.jazor` 就报告 `CS1503`，开发服务器保留上一次成功产物。恢复声明后，已打开的页面保留状态。Windows Chromium 整轮记录了 11 次成功编译和 6 次预期诊断，临时目录和旧声明的写入均未触发额外编译。[浏览器与编译次数](../../scripts/csharp/verify-jolt-dev-dependencies.cs)

在已安装的 Windows 候选工具 `2.6.0-migration.20261124.7` 上，项目外的链接包最初从 `initial.d.ts` 导出接收字符串的函数。保持调用处的 `.jazor` 缓冲区未保存，将包入口改为从 `updated.d.ts` 重导出接收数值的函数。LSP 主动发布 `CS1503`，另一个仍能绑定的调用按 F12 定位到新声明的对应行。随后仅改写 `updated.d.ts` 本身，诊断随参数类型变化而清除或重新出现，F12 也跟随声明移动到新行。恢复原重导出后，F12 回到原文件。整个过程只改写磁盘上的声明，编辑器没有发送外部文件变更通知。[外部声明与 F12 回归](../../scripts/csharp/verify-jolt-projection-cache.cs)

在同一候选版的发布 apphost 上，只破坏父目录的 `pnpm-lock.yaml`，运行中的 Jolt 就会在受影响组件报告 `JOLT1001`，给出锁文件路径。恢复根锁后组件自动编译成功，整个过程没有编辑组件。[共享工作区根锁验收](../../scripts/csharp/verify-jolt-projection-cache.cs)

候选工具 `.11` 从 Jolt bundle 到 .NET Folder Publish 的 Vite `buildStart` 计数为 SPA 一次，SSR 的客户端和服务端各一次。关闭开发服务后，Chromium 从生产宿主的 `/portal/frontend/index.html` 加载页面。点击前浏览器尚未请求懒加载模块，点击后从 `/portal/frontend/assets/` 请求带哈希的独立 JS 文件，HTTP 200，页面显示其导出文本并保留计数。发布目录的前端文件与 `frontend/dist` 一致。SSR 宿主另行验证了请求隔离、深链接、hydration 和预取失败后的恢复。[发布目录 SPA 验收](../../scripts/csharp/verify-development-hmr.cs) · [SSR 发布目录验收](../../scripts/csharp/verify-jolt-ssr.cs)

Monaco 在开发服务与生产静态包中运行同一组浏览器交互。停止开发服务后，Chromium 从独立静态宿主的 `/assets` 加载打包后的 Worker。服务器观察到请求的 `Sec-Fetch-Dest` 为 `worker`、响应为 HTTP 200，Worker 读回模型中写入的 `one line`。[生产 Worker 与模型同步](../../scripts/csharp/verify-jolt-npm-runtime.cs)

阶段 1 → 2 → 3 交付按需投影与命令退出；阶段 4 → 5 交付模块交接与开发会话。阶段 4 的程序集、资源与监听归属可与阶段 1 并行核对。Vue 混合应用的最终验收同时依赖两条路径。

## 验收矩阵

| 场景 | 需要看到的证据 | 可复用的仓库入口 |
| --- | --- | --- |
| 纯 Jazor 首次使用 npm | 空生成目录、没有 Jolt 或 Vue，普通对象和泛型回调类型准确；实际 JS 导入可执行，错误参数定位在调用点 | [普通项目冷构建与错误恢复](../../scripts/csharp/verify-jazor-npm-projection.cs) · [两份 DLL 传递对象](../../scripts/csharp/verify-jazor-npm-library.cs) |
| 完全没有 npm 的模块项目 | 实际安装 Jazor 的普通可执行项目采用 SDK 默认模式；无 npm 清单的冷构建、发布和启动成功。构建日志及目录核对投影宿主调用、请求文件、生成源码与 pnpm store。类库只交付 catalog | [无清单的可执行项目](../../scripts/csharp/verify-jazor-npm-projection.cs) · [SDK 配置与发布](../../src/Jazor.EmitTest/SdkFeedbackPublishTests.cs) · [类库消费者](../../src/Jazor.EmitTest/SdkIntegrationTests.cs) |
| 未保存源码与干净 CI | 未保存的 `using` 可补全、报错、撤销；CI 通过独立还原与构建得到已保存源码对应的签名。IDE 关闭或清空缓存后仍可构建 | [真实 C# 语言服务与候选包](../../scripts/csharp/verify-jazor-npm-editor.cs)；[现有 Jolt LSP 夹具](../../src/Jolt.LanguageServer/verify.cs) 对照另一入口 |
| 七库与等价本地声明 | date-fns 日期传递、Floating UI 元素、VueUse ref、Vue Query 的 `useQuery/useMutation`、VeeValidate 的 `useField`、Vue I18n 泛型和 Monaco 编辑器／模型／worker；核对完整调用与运行结果 | [七库对照](../../scripts/csharp/verify-jolt-binding-projection.cs) · [真实 npm 调用](../../scripts/csharp/verify-jolt-npm-runtime.cs) · [Monaco 浏览器夹具](../../scripts/csharp/fixtures/jolt-monaco-browser/) |
| 热缓存及失败恢复 | 使用点换序维持源码字节和时间戳；重导出声明的变化使元数据正确失效。冷、热和计算键期间取消后均可重试。破坏组件索引的 JSON 后，重建恢复原索引字节并保留源码时间戳，下一次热构建直接复用两者 | [代码缓存](../../scripts/csharp/verify-jolt-projection-cache.cs) · [声明缓存](../../scripts/csharp/verify-jolt-npm-cache.cs) |
| Vue Dev 与通用运行 | 一次模板更改只产生一次有效更新；修改 C# 后浏览器读到新值；启动失败可重试，关闭释放端口，附接者退出后外部会话可继续使用 | [Jolt 开发依赖](../../scripts/csharp/verify-jolt-dev-dependencies.cs) · [ASP.NET Core 前端测试](../../src/Jazor.EmitTest/JazorFrontendBuilderTests.cs) |
| 生产与分发 | 安装 Jazor 能独立运行；安装 Jolt 能编译 Vue；SPA、SSR 和路径前缀部署可加载，浏览器 hydration、延迟导入及 worker 成功；帮助与指南和实际安装包一致 | [打包](../../scripts/csharp/verify-jolt-package.cs) · [SSR 宿主](../../src/Jazor.EmitTest/JoltSsrHostingTests.cs) |

实施前后在同一机器、SDK、锁文件、fixture 及缓存条件下采样。分别记录首次构建耗时、无改动再次构建的 checker 启动次数与文件写入、首个补全耗时、编辑到诊断／DOM 更新的 p50 和 p95、实际运行的 Vite 实例数。性能比较沿用[现有混编基线](../../samples/Jolt.Mixed/EditFeedback-baseline.md)的方法。只有定位到具体重复工作或性能回退，才调整现有通用运行路径。

Windows 隔离源码计数显示，普通 C# 有实际 npm 调用时，首次构建执行 3 次 `ts.createProgram`，无改动重建为 0 次，生成投影的时间戳保持不变。相同清单和锁文件下移除源码中的 npm 调用，两次构建均为 0 次。混编应用从启动到五轮热编辑只创建了一个 Vite 开发服务。2.5.0 与候选版同机各十次热编辑的 `.jazor` 保存到 DOM 更新 p50 分别为 379 和 385 ms，p95 分别为 511 和 446 ms。[测试条件、源码计数位置及样本](../../samples/Jolt.Mixed/EditFeedback-baseline.md)

普通 C# 冷构建在同一台机器各测三次。2.5.0 显式生成绑定再构建的耗时为 8.09、7.76、11.95 秒。候选 SDK `.9` 只运行一次 `dotnet build`，耗时为 14.32、11.11、11.08 秒。新流程省去了作者手动生成的步骤，本次冷构建耗时中位数从 8.09 秒增加到 11.11 秒。[夹具和计时范围](../../samples/Jolt.Mixed/EditFeedback-baseline.md)

同机混编编辑器对照中，v2.5.0 与迁移候选版从打开文档到首次无错误诊断分别为 4705 和 4563 ms。随后首次请求类型为属性的 `Limit` 补全，响应分别为 1963 和 2003 ms，每版本各一次。四种热编辑各预热一轮后取十个样本。[编辑器采样条件与 p50/p95](../../samples/Jolt.Mixed/EditFeedback-baseline.md#v250-与迁移候选版的同夹具编辑器对照)

从仓库根验证。普通 C# 验收使用本次构建的 Jazor.Emit 输出。七库对照测试复用同一个声明引擎，Vue 开发与 SSR 测试使用对应的 Jolt apphost 和浏览器。

```text
dotnet build src/Jazor.Emit/Jazor.Emit.csproj
dotnet run --file scripts/csharp/verify-jazor-npm-projection.cs
dotnet run --file scripts/csharp/verify-jazor-npm-projection.cs -- <Jazor.nupkg>
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> Automatic --pnpm-workspace
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> Balanced
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> Automatic
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> Automatic --editor-runtime <Visual-Studio-私有-dotnet-根目录>
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> --first-manifest Automatic
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> --first-manifest Balanced
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> --repair-manifest Automatic
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> --repair-manifest Balanced
dotnet run --file scripts/csharp/verify-jazor-npm-editor.cs -- <Jazor.nupkg> --nested-manifest
dotnet run --file scripts/csharp/verify-jazor-npm-library.cs -- src/Jazor.Emit/bin/Debug/net11.0
dotnet run --file scripts/csharp/verify-jazor-npm-library.cs -- <Jazor.nupkg>
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~SdkFeedbackPublishTests|FullyQualifiedName~EmitOptionsTests"
dotnet run --file scripts/csharp/verify-jolt-binding-projection.cs
dotnet run --file scripts/csharp/verify-jolt-npm-cache.cs
dotnet run --file scripts/csharp/verify-jolt-package.cs -- --package-source <本地-Jolt-包源目录> --editor-features --browser <Chromium>
dotnet run --file scripts/csharp/verify-jolt-projection-cache.cs -- <Jolt-apphost>
dotnet run --file scripts/csharp/verify-jolt-projection-cache.cs -- <Jolt-apphost> --external-link
dotnet run --file scripts/csharp/verify-jolt-projection-cache.cs -- <Jolt-apphost> --workspace-lock
dotnet run --file scripts/csharp/verify-jolt-public-projection.cs -- <Jolt-apphost>
dotnet run --file scripts/csharp/verify-jolt-dev-dependencies.cs -- <Jolt-apphost> <Chromium>
dotnet run --file scripts/csharp/verify-jolt-mixed.cs -- <Jolt-apphost> <Chromium>
dotnet run --file scripts/csharp/verify-jolt-mixed.cs -- <Jolt-apphost> <Chromium> --module-handoff <Jazor.nupkg>
dotnet run --file scripts/csharp/verify-jazor-module-handoff.cs -- <Jazor.nupkg> <Jolt-apphost> <Chromium>
dotnet run --file scripts/csharp/verify-development-hmr.cs -- --jolt-proxy <Jolt-apphost> <Chromium> <本地-Jolt-包源目录> <工具版本>
dotnet run --file scripts/csharp/verify-jolt-ssr.cs -- <Jolt-apphost> <Chromium>
dotnet run --file scripts/csharp/verify-jolt-npm-runtime.cs -- <Jolt-apphost> --browser <Chromium>
```

### 单独复验宿主强制退出

`--jolt-force-stop` 是验收脚本的参数。已在 Windows 安装 `2.6.0-migration.20261124.11`，分别验证附接和自启。两次均先从 ASP.NET 代理请求编译后的 `Counter.mjs`，再仅强制结束宿主 PID。附接的外部 Jolt 继续返回该组件。自启的 Jolt 在标准输入关闭后触发 Vite 的 HTTP `close` 和插件 `closeBundle`，随后进程退出，两个端口可重绑。独立 CLI 另行对照了两种行为：传入 `--stop-on-stdin-close` 时 EOF 使进程以 0 退出，缺省时收到 EOF 后仍可请求组件。[宿主生命周期验收](../../scripts/csharp/verify-development-hmr.cs)

```text
dotnet run --file scripts/csharp/verify-development-hmr.cs -- --jolt-force-stop <本地-Jolt-包源目录> <工具版本>
```

## 实现前需审计的选择

| 待定事项 | 建议起点 | 审计时交付什么 |
| --- | --- | --- |
| 桌面 VS 的本机声明链接（沿用已有 VSIX） | Jazor C# 导航组件消费已有符号的声明位置，随现有 `Jolt.VisualStudio.vsix` 安装和升级。普通 C# 项目仍只引用 Jazor NuGet。VS Code 直接使用客户端的悬停链接 | 用真实组合包打开普通 C# 类库，验证点击后的本机文件、行列及 F12，并确认普通 C# 操作无需启动 Jolt |
| 普通 C# 的原始声明导航（已确认） | 普通 C# 的 F12 打开生成的 C# 声明，调用处悬停链接打开本项目的 TypeScript 原始声明。`.jazor` 中 F12 直接定位原声明 | 核对两个普通 C# 入口各自的位置、重导出和声明移动后的链接。[宿主扩展点与实测](../02-architecture/jazor-jolt-boundaries.md#作者体验与工程输入) |
| 纯 Jazor 公开类型及跨 DLL 身份 | 复用已有 ECMAScript 表示和属性。用两个生产者、一个消费者检验引用解析；遇到 `VueProps` 等通用形状先给出等价类型设计 | 新旧生成签名、是否改变既有公开类型、具体迁移示例；确需新公开类型时单独审查 |
| 独立 Vue 应用关联 .NET 库、F5 启动会话 | 先使用已有项目关系、程序集输出、ASP.NET Core 宿主和现有工具清单。比较 CLI 独立启动与已启动 .NET 宿主两条路径 | 两种工程布局的命令、所有者、输出位置、端口释放和观察结果。若现有契约无法表达，再提出最小公开改动 |
| 调试器强制结束会话所有者（已确认） | 启动会话的宿主传 `jolt dev --stop-on-stdin-close` 并持有 stdin 写端。外部独立启动的 Jolt 由原启动者管理 | 安装实际工具，验证正常关闭、仅杀宿主 PID 后 Vite 关闭并释放端口、附接外部会话继续服务，以及普通 CLI 收到 EOF 后继续运行 |
| Emit／Dev 对现有宿主的影响 | 固定当前默认值、直接 CLI 缺省行为和发布用例，比较迁移前后实际产物与启动路径 | `none/debug/release`、独立 CLI、SPA/SSR 对照及兼容性说明。改变公共默认值需单独决策 |

删除 `jolt bindings` 是已确定的破坏性调整，按仓库 MINOR 规则写迁移说明。其余公共改动在相应审计项获定后进入实现。各阶段只有通过所列消费者验收才更新支持状态。[版本规则](../03-guides/release-and-versioning.md)
