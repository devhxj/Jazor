# Jazor.Analyzer

> 定位：ECMAScript 白名单诊断与 Razor Source Generator hook 的 Roslyn analyzer 入口。

`Jazor.Analyzer` 承载 `ECMAScript` / `Jazor.Compiler` 主线的静态诊断，以及 Razor-to-Vue 所需的 Razor SG hook/bootstrap。它不承载 Razor IR、SFC 语义模型或宿主 RPC。

## 职责

- 对进入 ECMAScript 编译域的类型和成员执行白名单诊断；class 会分析声明与方法体，interface 和 delegate 会分析其声明签名。
- 在泛型实参、数组元素、局部推断与集合表达式目标等 erased positions 提前报告不支持的具体类型。
- 在 <c>is</c>、pattern、<c>switch</c> pattern 与 <c>catch</c> 的 runtime type filtering 位置检查具体类型，保留 alias 歧义的独立诊断。
- 注册 Razor SG hook/bootstrap，并在最终文档输入不可用时提供可定位诊断。

## 边界

- Analyzer 可以比 `Jazor.Compiler` 更严格，但 compiler 仍是 runtime-sensitive lowering 的最终验证层。
- `ECMAScript.Contract` 提供最小声明契约，`Jazor.Common` 提供 `Format` 与 `SourceMaps` 等共享实现。
- 外部组件包装类型通过中性 `LibraryComponentAttribute` 进入分析域；Analyzer 不依赖 Vue、React 或其他框架的专属 attribute。具体 import 与渲染协议仍由对应适配层处理。
- `Jazor.RazorVue` 持有 Razor SDK final-document 绑定边界；本程序集只提供 hook 所需的 analyzer 入口。

## 代码结构

- `Analyzer.cs`：ECMAScript 主线静态分析器。
- `RazorVue/Generation/*.cs`：Razor SG hook/bootstrap 与 final-document 输入诊断。
- `AnalyzerReleases.*.md`：诊断规则发布说明。

## 诊断范围

- `JAZOR001`：不支持的类型或成员进入 ECMAScript 编译域。
- `JAZOR002`：共享 runtime alias 造成的类型过滤歧义。
- `JAZOR003`：`SpreadAttribute` 用法不符合 structural record 约束。
- `JAZOR004`：`SpreadAttribute` 与显式 JavaScript 属性名同时使用。
- `JAZOR005`：同一符号上的 `Description("@#...")` 与 `ECMAScriptName` 提供了不同的具体 JavaScript 名称。
- `JAZOR006`：同一实际发射作用域内的成员解析到重复 JavaScript 名称。模块检查只覆盖 public/internal 导出；runtime member class 分开检查 static/instance，属性 getter/setter 视为一个逻辑成员，record 只检查直接结构化属性。

## 构建与验证

```bash
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
```

## 相关文档

- [Jazor.Compiler](../Jazor.Compiler/README.md)
- [Jazor.RazorVue](../Jazor.RazorVue/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
