# 当前状态

> 当前版本：[2.4.0（2026-11-21）](https://github.com/devhxj/Jazor/releases/tag/v2.4.0)，使用 .NET 11 GA。包选择与升级见[安装与配置](../03-guides/installation-and-configuration.md)，版本变化见 [CHANGELOG](../../CHANGELOG.md)。

本页定义今天可以依赖的产品契约。测试与运行报告必须关联被测提交、SDK、平台和输入；历史计数只保留在[演进记录](../05-history/evolution.md)，不作为当前版本的能力清单。

## Jolt 作者面

| 范围 | 2.4.0 已发布能力 |
| --- | --- |
| 编译与构建 | `.jazor` 模板与 C#、本地组件、npm 公开 TypeScript 声明与 ESM、源码映射、Vite 开发与生产构建 |
| 组件契约 | 类型化参数、回调、插槽、受约束的泛型参数、本地和 npm 命名模型、原生表单绑定、DOM 事件、`@key`、`@ref` 与 `<style scoped>` |
| 混合项目与组件库 | 同一 Vite 项目双向引用 `.vue` 和 `.jazor`，通过 `jolt library` 分发 ESM、TypeScript 声明和样式 |
| npm 与应用组合 | 类型化 npm 函数和标量常量、每实例 `[Setup]`、Vue 内置组件、可选路由指令和 ASP.NET Core 生产 SSR |
| npm 声明投影 | `jolt bindings` 可为普通 .NET 类库生成公开 C# 声明，构建和编辑器可跨进程复用项目内的投影结果 |
| 编辑器 | VS Code 与 Visual Studio 扩展。语义高亮、补全、诊断、定义、引用、改名、格式化与快捷修复 |
| 模块导出 | Vue 组件为默认导出，公开静态方法和字段以及公开常量为具名导出 |

运行方式、类型边界及编辑器配置见 [Jolt 作者指南](../03-guides/jolt-authoring.md)。

## 已交付的核心能力

| 能力 | 当前范围 | 权威入口 |
| --- | --- | --- |
| C# 到 ECMAScript | Roslyn `IOperation` 经核心 Compiler 降低为 Acornima ESTree；导入、临时名、source origin 与 source map 保持确定性 | [编译器架构](../02-architecture/compiler.md)、[实现原则](../../src/Jazor.Compiler/ImplementationPrinciples.md) |
| 普通源码与类型语义 | 可达的普通源码 helper 无需逐类型添加 `[ECMAScript]`；class 保留受支持的类行为，数据 record 表达结构化对象 | [类型与宿主标注](../03-guides/type-semantics.md) |
| 普通 DLL 与 NuGet 导入 | ILSpy 恢复、独立 Roslyn 绑定、成员闭包、共享模块/状态、恢复源码映射 | [类库导入](./ordinary-nuget-compilation-research.md) |
| CLR 与宿主映射 | 白名单、runtime helper、强类型 ECMAScript/WebIDL binding 定义受支持的运行时语义；未实现语义在使用点明确诊断 | [Compiler](../../src/Jazor.Compiler/README.md)、[CLR](../../src/Jazor.CLR/README.md) |
| 模块与资源交付 | `ModuleCatalog`、ECMAScript 源码和 schema 2 binding metadata 接入标准 `jazor/` 项目，支持 npm/JSR identity、ESM specifier 和细粒度 JS/CSS tree shaking | [类库与产物契约](../02-architecture/library-artifact-contract.md)、[产物管线](../02-architecture/artifact-pipeline.md) |
| 构建与 SSR 工具链 | 进程内 NodeApi + LibNode + 固定 pnpm API；Vite、Vue SSR 和依赖恢复共享显式资源闭包 | [工具链与迁移](../03-guides/toolchain-migration.md)、[NodeHost](../../src/Jazor.NodeHost/README.md) |

1.3.0 的语法修复包括整数行为、数组和模块默认值、表达式包装及模式 switch 控制流。35 个审计 bug 中，11 项修正运行行为，24 项以源码位置诊断替代错误代码或内部异常；这些诊断不表示完整运行时协议已实现。范围与替代写法见[逐项修复记录](./compiler-syntax-bug-fixes.md)。

## 类型与产品边界

- 普通 NuGet / ProjectReference 按实际使用点自动导入已解析的本地实现程序集，恢复并编译可达的受支持 class 成员；无需生产者标注。缺少实现或超出支持切片的外部运行时操作继续拒绝，见[类库导入](./ordinary-nuget-compilation-research.md)。
- 泛型实参和数组元素等擦除位置与运行时构造、成员访问、默认值和类型检查分开验证。Analyzer 可以更早报告封闭具体外部类型，Compiler 保持使用点验证。
- Microsoft/Blazor 内置 UI 组件与 `IJSRuntime` 字符串互操作使用明确的 Reject 边界；应用通过自定义组件、typed binding 和显式 endpoint 协议实现相应功能。
- `StreamRendering` 使用 Reject；localization、复杂 validation、复杂 activation、完整 browser history、服务器 circuit 与完整 CLR/Blazor reference parity 不在当前已证明的支持范围内。
- 编译器优先保证求值顺序、副作用次数、结果、使用点语义和确定性。测试覆盖率不表示任意 C#、任意 NuGet 库或任意浏览器场景都已受支持。

## 生态与参考应用

构建和编辑器按项目复用磁盘上的 npm 声明快照、C# 投影及定义位置。普通 C# 项目可以用 `jolt bindings` 将公开投影写入类库，其他项目通过项目引用复用同一套类型。跨进程验收覆盖并发首次生成、重启后的命中、删除输出后恢复，以及上游声明修改后的重新诊断。[缓存与输出位置](../03-guides/jolt-authoring.md#复用构建生成的投影) · [缓存验收](../../scripts/csharp/verify-jolt-projection-cache.cs)

Vue、Vue Router、Pinia、Pinia Testing 和 Vue Devtools 的手写绑定随 SDK 发布，Style 提供 Jazor 的样式功能。七个应用级绑定已移出 2.4.0 的默认发布清单。已有项目仍可使用 2.3.0 的包。Jolt 的投影验收沿用这些绑定的业务用例，逐项记录真实调用、浏览器交互和适用的 SSR 或 worker。 [npm 投影验收计划](./p3-vue-application-bindings-plan.md)

已用 Vue SSR 验证 date-fns 的区间步长、VueUse 的三种媒体查询入参、Vue I18n 的复数翻译与语言切换、VeeValidate 的字段错误和 Vue Query 的缓存清理。date-fns 的 ISO 格式和 Floating UI 的 placement 均从字符串字面量推导出 C# 枚举。VueUse 在 JSDOM 中验证了键盘与鼠标专用回调、主动清理与卸载后停止监听。Vue I18n 的字符串字典命名插值已通过 SSR。Vue Query 的混合查询键可包含标量和应用记录，精确匹配、前缀失效及跨页交错重取都已检查缓存结果。VeeValidate 的 `UseField<Email>` 和 `UseField<double>` 保留验证回调、初值、可写 ref 和再次校验的类型关系，数值字段还验证了 Promise 返回的规则。表单也验证了无效提交读取原输入及字段错误，重新赋值后有效提交读取转换后的记录。Floating UI 的 `computePosition` 已在 JSDOM 的固定布局中接收 `offset` 与 `arrow`，并读回箭头坐标。`autoUpdate` 被释放后不再响应 resize。Vue 入口的 `UseFloating` 在锚点由空变为元素后开始定位，配合 `WhileElementsMounted` 换锚点后更新坐标并移除旧监听器，卸载后停止布局读取。 [SSR 与 DOM 定位](../../scripts/csharp/verify-jolt-npm-runtime.cs) · [Vue 元素 ref](../../scripts/csharp/verify-jolt-floating-vue.cs) · [翻译与缓存](../../scripts/csharp/verify-jolt-composable-runtime.cs)

Vue Query 的 `UseQuery` 把异步回调的结果类型传给数据 ref，`UseMutation` 把输入和返回类型传给 `MutateAsync`。Vue SSR 与 JSDOM 客户端均由 C# 注册 `VueQueryPlugin` 并执行查询和提交。三个活跃查询验证了精确失效、前缀失效和主动重取各自影响的缓存。无限查询用数字游标读取两页记录并停止，真实缓存保留 `[0, 1]`。Vue I18n 的 C# 工厂注册用例以字符串字典配置法语消息，并在 SSR 后释放实例。指定 `legacy` 字面量后，C# 可按模式读写全局 locale。嵌套表单记录的类型化选择器在缺失父字段时读到 `null`，保留已有标量的零和假值。 [查询与提交用例](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor) · [Vue I18n 注册](../../scripts/csharp/verify-jolt-i18n-plugin.cs)

Monaco 浏览器用例通过投影的 `CreateModel`、`Create` 和 `SetModel` 将同一模型装入编辑器。应用定义 worker 的 RPC 接口并提供 worker 入口。投影的 `WithSyncedResources` 同步该模型后，worker 读取镜像中的文本并返回给页面，随后组件释放 worker、编辑器和模型。Chrome 实测还能从模型变更事件读到版本号，解除订阅后再修改模型，回调次数保持不变。C# 还可写入带严重度、消息及范围的 marker 数组，按 owner 和模型读取，再清除一个 owner。Chromium 检查了另一 owner 的标记仍然存在，并从编辑器 DOM 读到 C# 主题指定的背景色。 [Monaco 用例](../../scripts/csharp/fixtures/jolt-monaco-browser/)

普通 C# 类库可用 `jolt bindings` 生成公开投影。独立消费项目已验证同一个 date-fns `Interval` 跨导出传递、es-toolkit 的 `retry` 与 `withTimeout` 传递应用记录、真实 ESM 调用、默认导出、标量常量、显式 null 与缺省字段。Vue I18n 的两个模式在只引用投影 DLL 的消费者中也已实际运行。七库盘点把组件消费用例再对着公开 DLL 编译一次，并列出手写重载和类型成员，按 Roslyn 在代表性用例中选中的具体方法、属性或枚举成员记录编译证据。 [独立类库验收](../../scripts/csharp/verify-jolt-public-projection.cs) · [七库对照](../../scripts/csharp/verify-jolt-binding-projection.cs)

[Jolt Counter](../../samples/Jolt.Counter/README.md) 验证 npm 组件、父子组件回调、开发服务和生产构建。

## 正式版与兼容性

1.0.0 已正式发布。公开作者 API 的冻结基线保持不变，每次候选从实际程序集重新生成快照并报告新增、删除和签名变化；不能把某次“0 新增/0 删除”当作永久状态。已批准删除仍在比较报告中保留，未审阅删除阻断发布。

新增能力使用 MINOR，修复使用 PATCH，正式版后的破坏性变更使用 MAJOR。1.1.0 工具链和 1.2.0 类型语义迁移有维护者批准的单次例外，迁移说明和证据要求继续适用。见[版本规则](../03-guides/release-and-versioning.md)和[API 兼容性](../03-guides/public-api-freeze.md)。

## 质量门槛与验证

| 门禁 | 当前最低要求 | 复现入口 |
| --- | --- | --- |
| Compiler | 10,000 个通过场景；行覆盖率 98%，分支覆盖率 97% | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Jolt | 语法与编译回归、CLI 隔离安装、语言服务、浏览器交互及两套 VSIX 打包 | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt`、[工具包验收](../../scripts/csharp/verify-jolt-package.cs)和[发布工作流](../../.github/workflows/nuget-publish-ref.yml) |
| Vue binding | 每个目标公共 binding contract 审计覆盖率 90% | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 绑定一致性 | 共享绑定的公开 API 保留 XML 文档 | `verify-binding-documentation.cs` |
| 发布交付 | API 兼容性、包形状、隔离 SPA/SSR 消费者与真实浏览器通过 | `verify-release-candidate.cs`、官方 tag workflow |

完整主测试入口为 `dotnet run --file scripts/csharp/test-dotnet.cs`。覆盖率对应 Debug 基线，发布消费者验证 Release 产物。平台声明以 [NodeApi workflow](../../.github/workflows/nodeapi-verify.yml)的实际运行矩阵为准，不从 RID 文件存在推断支持。

[Quality Gates](../../.github/workflows/quality-gates.yml)与[正式发布工作流](../../.github/workflows/nuget-publish-ref.yml)保留版本对应的报告。GitHub Release、上传被接受与 NuGet 公共下载可用是不同交付状态；安装时核对所选版本的包可用性。具体命令、输出隔离和报告位置见[开发与测试](../03-guides/development-and-testing.md)。

下一轮投入、责任层与验收条件统一见[下一阶段](./next-development.md)。
