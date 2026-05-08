# RazorVue Razor SG Tail Injection Guidance

> Status: Accepted implementation guidance
> Date: 2026-05-08
> Scope: RazorVue 正式 SG 接入阶段，如何把 RazorVue carrier 生成逻辑挂到当前 SDK Razor Source Generator 主线末尾。

本文记录当前源码阅读与 focused tests 后锁定的最优实现方向。

结论是：**官方 Razor SG 原样运行，通过受控 IL 尾部注入复用官方增量数据流，额外注册 RazorVue carrier source output。**

这不是 wrapper，不是私跑第二遍 Razor SG，也不是在 `HostOutput` 回调里直接产源码。

## 1. 决策摘要

采用受控 IL 尾部注入作为 RazorVue 正式 SG 接入主方案：

1. 保留官方 `RazorSourceGenerator` 的 `.razor.g.cs` 生成逻辑。
2. 保留官方 `HostOutputs -> RazorGeneratorResult` 发布逻辑。
3. 在官方 Razor SG 已经形成最终 Razor/C# document 的增量数据流之后，额外注册一个 RazorVue source output。
4. 新增 source output 只负责生成 RazorVue IR carrier partial。
5. RazorVue 后续消费侧继续保持 carrier-only，不回读 `.razor` 原文。

概念形态如下：

```csharp
official RazorSourceGenerator.Initialize(context)
{
    var csharpDocuments = ...;

    context.RegisterImplementationSourceOutput(csharpDocuments, officialRazorEmit);

    // RazorVue injected output. It reuses the official SG data flow and only adds carrier source.
    context.RegisterImplementationSourceOutput(csharpDocuments, razorVueCarrierEmit);

    context.RegisterHostOutput(hostOutputs, officialHostOutput);
}
```

## 2. 为什么这是当前最优解

相比 wrapper / fork / nested run，这条路线对用户项目的 Razor 编译行为侵入最小：

1. 不替换 SDK Razor SG 主线。
2. 不重复执行 Razor SG。
3. 不改变官方 `.razor.g.cs` 的生成内容、hint name、诊断、缓存和设计时行为。
4. 不要求用户显式改写 Razor 文件或额外声明中间产物。
5. 不把 `.razor` 原文读取、`AdditionalText.GetText()` 或 path 回读升级为生产契约。

相比只读 `HostOutput`，这条路线能真正把 carrier 放进当前 compilation：

1. `HostOutput` 是宿主输出，不是源码输出。
2. `HostOutput` 适合作为 IR 结果锚点和验证点。
3. carrier partial 必须通过 `SourceProductionContext.AddSource(...)` 所在的 source output 通道产出。
4. 因此正式实现应注入并列 source output，而不是试图在 `HostOutput` 回调内完成源码生成。

## 3. 已验证事实

当前仓库已通过 focused tests 证明以下事实：

1. SDK `RazorSourceGenerator` 单轮运行可以同时产出标准 Razor generated source 和 `HostOutputs`。
2. `HostOutputs` 中存在 internal `RazorGeneratorResult`。
3. 可以通过受控 bridge 从 `RazorGeneratorResult.GetCodeDocument(string physicalPath)` 取回对应 `RazorCodeDocument`。
4. Roslyn 同一轮 generator 之间不能看到彼此新生成的 partial / attribute。
5. 第二轮 compilation 才能看到上一轮 generator 输出。

对应测试：

1. `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorHostOutputTests.cs`
2. `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorCarrierBridgeTests.cs`
3. `src/Jazor.RazorVue.RazorIr.Test/RoslynGeneratorVisibilityTests.cs`

这些事实共同排除了 companion generator 同轮消费官方 Razor SG 输出的方案，也排除了 production nested run。

## 4. HostOutput 的定位

`HostOutput` 在本方案中的定位是：

1. 证明官方 Razor SG 末端已经拥有完整 Razor generation result。
2. 作为定位末端数据流和验证 bridge 的锚点。
3. 作为调试和 future host integration 的观察通道。

`HostOutput` 不是：

1. carrier partial 的源码产出通道。
2. generator 之间共享数据的正式通道。
3. 绕过 source output 的替代编译输入。

实现时可以利用 `RazorGeneratorResult` / `RazorCodeDocument` 的 shape 作为数据确认点，但 carrier 进入 compilation 必须走 source output。

## 5. IL 注入边界

IL 注入必须保持窄边界：

1. 只 patch `Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator.Initialize(...)` 的末端注册逻辑。
2. 只新增一个 RazorVue carrier source output 注册。
3. 不改写官方 Razor source output delegate。
4. 不改写官方 `HostOutput` delegate。
5. 不修改官方 generated C# 文本。
6. 不修改 Razor parser、Razor engine passes、tag helper discovery 或 document classifier。

注入点优先选择已经形成最终 `csharpDocuments` 的位置。

如果当前 SDK 版本下无法稳定拿到 `csharpDocuments` 数据流，则该 SDK 版本应 fail-fast，而不是退回 nested run、classic codegen 或 `.razor` 原文重建。

## 6. Carrier 生成职责

注入的 RazorVue source output 只做以下事情：

1. 读取官方 Razor SG 已产生的 document / IR 结果。
2. 提取 RazorVue carrier 所需的最小稳定数据。
3. 生成组件 partial 上的 `RazorVueRazorIrCarrierAttribute`。
4. 生成明确、稳定、可诊断的 hint name。
5. 对无法识别或不支持的 Razor IR shape 产出诊断。

它不得做以下事情：

1. 重新读取 `.razor` 文件作为语义输入。
2. 重新运行 `RazorProjectEngine.Process(...)` 作为生产主线。
3. 从 `BuildRenderTree` 反推 Razor 组件模板。
4. 静默降级到空 carrier 或伪 carrier。
5. 改变官方 Razor component class 的 public surface。

## 7. 版本与失败策略

由于这是 internal / IL 层面的尾部注入，必须把技术风险显式收口：

1. 绑定目标 Razor compiler assembly 版本、MVID 或 IL 指纹。
2. 校验目标方法签名和关键局部数据流 shape。
3. 校验失败时，在 RazorVue 启用场景下给出明确诊断并停止 carrier 生成。
4. RazorVue 未启用时，不注入、不影响普通 Razor 项目。
5. SDK 升级必须先更新指纹和 focused tests，再允许进入生产路径。

失败时禁止自动回退到：

1. `.razor` 原文回读。
2. `AdditionalText.GetText()` 重建文档。
3. `BuildRenderTree` 反推 Razor 组件。
4. classic Razor codegen。
5. production nested Razor SG run。

## 8. 打包与启用原则

用户侧默认风险控制原则：

1. 只有 RazorVue 显式启用时才激活尾部注入。
2. 不启用 RazorVue 的项目应看到与原 SDK Razor SG 一致的行为。
3. NuGet 包应把注入逻辑收口在 `Jazor.Analyzer` analyzer/generator 载体内。
4. 不向用户暴露额外 build task 作为主入口。
5. 不要求用户手动引用 Razor compiler internals。

如果实现需要随包携带辅助 IL patcher 或 bridge assembly，它们必须只服务于 `Jazor.Analyzer` 的 RazorVue SG 接入，不扩大为通用 Razor 替换层。

## 9. 验收标准

正式实现完成前必须满足：

1. 官方 `.razor.g.cs` 仍正常生成。
2. RazorVue carrier partial 在同一 generator run 中生成并进入 final compilation。
3. RazorVue 消费侧能从 carrier-only 模型完成模板前端。
4. 普通 Razor 项目未启用 RazorVue 时不产生额外 source、diagnostic 或 build 行为变化。
5. SDK 指纹不匹配时 diagnostic 清晰，可定位到 RazorVue SG injection 版本不匹配。
6. focused tests 覆盖成功注入、未启用 no-op、指纹不匹配 fail-fast、carrier 产出和官方 generated source parity。

## 10. 后续执行顺序

1. 建立 Razor SG `Initialize(...)` IL shape 探针，输出目标 SDK 的关键指纹。
2. 设计最小 `RazorVueCarrierSourceOutput` delegate，先只产最小 carrier partial。
3. 在测试环境中 patch Razor SG 并验证官方 `.razor.g.cs` 与 carrier 同轮产出。
4. 接入 RazorVue carrier-only 消费侧，跑 RazorVue focused suites。
5. 补 package / analyzer 装载验证，确保普通 Razor 项目 no-op。
6. 扩展 IR carrier 数据面到 imports、document identity 和后续模板所需结构。

## 11. 一句话结论

当前最优解是：**HostOutput 用作 Razor SG 末端 IR 结果锚点，受控 IL 尾部注入新增并列 source output，用同一条官方 Razor SG 数据流生成 RazorVue carrier。**
