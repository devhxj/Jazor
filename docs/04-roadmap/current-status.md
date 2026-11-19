# 当前状态

> 当前版本：[2.0.0（2026-11-18）](https://github.com/devhxj/Jazor/releases/tag/v2.0.0)，使用 .NET 11 GA。包选择与升级见[安装与配置](../03-guides/installation-and-configuration.md)，版本变化见 [CHANGELOG](../../CHANGELOG.md)。

本页定义今天可以依赖的产品契约。测试与运行报告必须关联被测提交、SDK、平台和输入；历史计数只保留在[演进记录](../05-history/evolution.md)，不作为当前版本的能力清单。

## 已交付的核心能力

| 能力 | 当前范围 | 权威入口 |
| --- | --- | --- |
| C# 到 ECMAScript | Roslyn `IOperation` 经核心 Compiler 降低为 Acornima ESTree；导入、临时名、source origin 与 source map 保持确定性 | [编译器架构](../02-architecture/compiler.md)、[实现原则](../../src/Jazor.Compiler/ImplementationPrinciples.md) |
| 普通源码与类型语义 | 可达的普通源码 helper 无需逐类型添加 `[ECMAScript]`；class 保留受支持的类行为，数据 record 表达结构化对象 | [类型与宿主标注](../03-guides/type-semantics.md) |
| 普通 DLL 与 NuGet 导入 | ILSpy 恢复、独立 Roslyn 绑定、成员闭包、共享模块/状态、恢复源码映射；普通模块与 RazorVue 消费 | [类库导入](./ordinary-nuget-compilation-research.md) |
| CLR 与宿主映射 | 白名单、runtime helper、强类型 ECMAScript/WebIDL binding 定义受支持的运行时语义；未实现语义在使用点明确诊断 | [Compiler](../../src/Jazor.Compiler/README.md)、[CLR](../../src/Jazor.CLR/README.md) |
| 模块与资源交付 | `ModuleCatalog`、ECMAScript 源码和 schema 2 binding metadata 接入标准 `jazor/` 项目，支持 npm/JSR identity、ESM specifier 和细粒度 JS/CSS tree shaking | [类库与产物契约](../02-architecture/library-artifact-contract.md)、[产物管线](../02-architecture/artifact-pipeline.md) |
| 构建与 SSR 工具链 | 进程内 NodeApi + LibNode + 固定 pnpm API；Vite、Vue SSR 和依赖恢复共享显式资源闭包 | [工具链与迁移](../03-guides/toolchain-migration.md)、[NodeHost](../../src/Jazor.NodeHost/README.md) |

1.3.0 的语法修复包括整数行为、数组和模块默认值、表达式包装及模式 switch 控制流。35 个审计 bug 中，11 项修正运行行为，24 项以源码位置诊断替代错误代码或内部异常；这些诊断不表示完整运行时协议已实现。范围与替代写法见[逐项修复记录](./compiler-syntax-bug-fixes.md)。

## Jolt 作者面

Jolt 是应用主线，覆盖 .jazor 模板、C# 语义、npm 组件声明、VS Code 与 Visual Studio 编辑体验。已发布范围和示例见 [Jolt 作者指南](../03-guides/jolt-authoring.md)。RazorVue 的 .razor 作者面、SPA 与 SSR 验收由 [RazorVue 仓库](https://github.com/devhxj/RazorVue) 维护。

## 类型与产品边界

- 普通 NuGet / ProjectReference 按实际使用点自动导入已解析的本地实现程序集，恢复并编译可达的受支持 class 成员；无需生产者标注。缺少实现或超出支持切片的外部运行时操作继续拒绝，见[类库导入](./ordinary-nuget-compilation-research.md)。
- 泛型实参和数组元素等擦除位置与运行时构造、成员访问、默认值和类型检查分开验证。Analyzer 可以更早报告封闭具体外部类型，Compiler 保持使用点验证。
- Microsoft/Blazor 内置 UI 组件与 `IJSRuntime` 字符串互操作使用明确的 Reject 边界；应用通过自定义组件、typed binding 和显式 endpoint 协议实现相应功能。
- `StreamRendering` 使用 Reject；localization、复杂 validation、复杂 activation、完整 browser history、服务器 circuit 与完整 CLR/Blazor reference parity 不在当前已证明的支持范围内。
- 编译器优先保证求值顺序、副作用次数、结果、使用点语义和确定性。测试覆盖率不表示任意 C#、任意 NuGet 库或任意浏览器场景都已受支持。

## 生态与参考应用

Vue、Vue Router、Pinia、Vue Devtools 与 Style 提供各自 README 声明的绑定切片。P3 的 DateFns、VueUse、FloatingUi、VeeValidate、VueI18n、VueQuery、VueDraggable、FilePond、WangEditor、Monaco 已进入 lockstep 发布集合，但仍保持 Guidance，等待各自 browser smoke 与真实 RazorVue consumer 证据。五个 Razor 组件库见 [RazorVue](https://github.com/devhxj/RazorVue)。见 [P3 验收计划](./p3-vue-application-bindings-plan.md)。

[Jolt Counter](../../samples/Jolt.Counter/README.md) 验证 npm 组件、父子组件回调、开发服务和生产构建。

## 正式版与兼容性

1.0.0 已正式发布。公开作者 API 的冻结基线保持不变，每次候选从实际程序集重新生成快照并报告新增、删除和签名变化；不能把某次“0 新增/0 删除”当作永久状态。已批准删除仍在比较报告中保留，未审阅删除阻断发布。

新增能力使用 MINOR，修复使用 PATCH，正式版后的破坏性变更使用 MAJOR。1.1.0 工具链和 1.2.0 类型语义迁移有维护者批准的单次例外，迁移说明和证据要求继续适用。见[版本规则](../03-guides/release-and-versioning.md)和[API 兼容性](../03-guides/public-api-freeze.md)。

## 质量门槛与验证

| 门禁 | 当前最低要求 | 复现入口 |
| --- | --- | --- |
| Compiler | 10,000 个通过场景；行覆盖率 98%，分支覆盖率 97% | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Jolt | 语法和编译回归、隔离包消费、语言服务与浏览器交互 | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` |
| Vue binding | 每个目标公共 binding contract 审计覆盖率 90% | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 绑定一致性 | 共享绑定的公开 API 保留 XML 文档 | `verify-binding-documentation.cs` |
| 发布交付 | API 兼容性、包形状、隔离 SPA/SSR 消费者与真实浏览器通过 | `verify-release-candidate.cs`、官方 tag workflow |

完整主测试入口为 `dotnet run --file scripts/csharp/test-dotnet.cs`。覆盖率对应 Debug 基线，发布消费者验证 Release 产物。平台声明以 [NodeApi workflow](../../.github/workflows/nodeapi-verify.yml)的实际运行矩阵为准，不从 RID 文件存在推断支持。

[Quality Gates](../../.github/workflows/quality-gates.yml)与[正式发布工作流](../../.github/workflows/nuget-publish-ref.yml)保留版本对应的报告。GitHub Release、上传被接受与 NuGet 公共下载可用是不同交付状态；安装时核对所选版本的包可用性。具体命令、输出隔离和报告位置见[开发与测试](../03-guides/development-and-testing.md)。

下一轮投入、责任层与验收条件统一见[下一阶段](./next-development.md)。
