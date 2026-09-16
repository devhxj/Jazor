# Jazor.Analyzer

> 定位：ECMAScript 编译域的 Roslyn 预诊断分析器。

`Jazor.Analyzer` 承载 `ECMAScript` / `Jazor.Compiler` 主线的静态诊断。直接引用 `Jazor` 时，包的 MSBuild targets 注册 analyzer，设计时构建也保留该注册，使 IDE 和构建均可提前报告问题。Razor Source Generator 接入与 Vue 作者面兼容性诊断由 `Jazor.RazorVue` 负责。

## 职责

- 对进入 ECMAScript 编译域的类型和成员执行白名单诊断；class 与 structural record（含 record struct）分析声明与方法体，interface 和 delegate 分析声明签名。
- 在泛型实参、数组元素、局部推断与集合表达式目标等 erased positions 提前报告不支持的具体类型。
- 在 `is`、`as`、pattern、`switch` pattern 与 `catch` 的 runtime type filtering 位置检查具体类型，保留 alias 歧义的独立诊断。
- 根据实际读写行为检查属性 accessor 映射，并检查绑定到自定义运算符的使用点。

## 边界

- Analyzer 可以比 `Jazor.Compiler` 更严格，但 compiler 仍是 runtime-sensitive lowering 的最终验证层。
- 白名单诊断只分析进入 `[ECMAScript]` / `[ECMAScriptModule]` 编译域的源码；`Spread` 用法诊断随该特性检查。生成代码不参与本分析器诊断。没有 analyzer 报错不等于所有 lowering 均能成功，依赖具体发射上下文的限制仍由 compiler 裁决。
- 泛型参数 `T` 本身允许通过，不追踪它的来源或数据流；闭合的具体类型会提前诊断。这不要求 compiler 拒绝相同的 erased positions。
- `ECMAScript.Contract` 提供最小声明契约，`Jazor.Common` 提供 `Format` 与 `SourceMaps` 等共享实现。
- 外部组件包装类型通过统一 `ECMAScript(import, Transform.Component, exportName)` 进入分析域；Analyzer 不依赖 Vue、React 或其他框架的专属 attribute。具体 import 与渲染协议仍由对应适配层处理，旧组件 Attribute 仅作迁移期兼容。
- `Jazor.RazorVue` 持有 Razor SDK final-document 绑定、Vue 作者面兼容性诊断与 lowering 边界。

## 代码结构

- `Analyzer.cs`：ECMAScript 主线静态分析器。
- `AnalyzerReleases.*.md`：诊断规则发布说明。

## 诊断范围

- `JAZOR001`：不支持的类型或成员进入 ECMAScript 编译域。
- `JAZOR002`：共享 runtime alias 造成的类型过滤歧义。
- `JAZOR003`：`SpreadAttribute` 用法不符合 structural record 约束。
- `JAZOR004`：`SpreadAttribute` 与显式 JavaScript 属性名同时使用。
- `JAZOR005`：同一符号上的 `Description("@#...")` 与 `ECMAScriptName` 提供了不同的具体 JavaScript 名称。
- `JAZOR006`：同一实际发射作用域内的成员解析到重复 JavaScript 名称。模块检查只覆盖 public/internal 导出；runtime member class 分开检查 static/instance，属性 getter/setter 视为一个逻辑成员，record 只检查直接结构化属性。

### 预诊断覆盖

| 使用位置 | 提前检查的内容 |
| --- | --- |
| 字段、属性、参数、返回值、约束、record struct 签名 | 不支持的具体类型及闭合泛型实参 |
| 普通局部变量、`out var`、解构、`foreach` | 在变量声明处检查推断出的具体类型 |
| 方法调用、方法组、静态泛型成员、嵌套泛型类型 | 方法实参及各层包含类型的泛型实参，例如 `Box<FileInfo>.Ignore()` |
| 元数据中声明的 host 方法、字段与属性 | 使用结果的具体类型，即使返回值被丢弃或擦除为 `object` |
| 属性读取、赋值、解构赋值、`+=`、`++`、`??=` | 区分 getter/setter；只映射 getter 的事件参数不能写入 |
| 自定义二元、一元、递增/递减、转换及复合赋值运算符 | 检查绑定的成员映射；复合赋值同时检查输入/输出转换 |
| `is`、`as`、pattern、`switch`、`catch` | 具体类型支持与共享 runtime alias 的类型过滤歧义 |

## 构建与验证

```bash
dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "FullyQualifiedName~RazorVueAnalyzerScopeTests|FullyQualifiedName~RazorVueCompatibilityAnalyzerTests"
```

## 相关文档

- [Jazor.Compiler](../Jazor.Compiler/README.md)
- [Jazor.RazorVue](../Jazor.RazorVue/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
