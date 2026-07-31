# Jazor.CompilerTest

`Jazor.CompilerTest` 是 `src/Jazor.Compiler` 的主回归测试项目。它验证的不只是“能不能生成 JS”，而是更具体的三类契约：

- 语义级 lowering 是否保持使用点可观察行为
- 模块级输出、命名、导入与类声明协议是否稳定
- source-origin / sourcemap / catalog 相关输出是否保持确定性

## 依赖与运行环境

- 测试框架：`MSTest.Sdk 4.1.0`
- 目标框架：`net11.0`
- 主要项目引用：
  - `src/Jazor.Compiler/Jazor.Compiler.csproj`
  - `src/ECMAScript/ECMAScript.csproj`
- 主要包依赖：
  - `Microsoft.CodeAnalysis.CSharp`
  - `Basic.Reference.Assemblies.Net110`
  - `coverlet.collector`

## 当前测试分层

### 1. 模块级转换

- `AstConverterTests.cs`
- `AstConverterUniqueNameTests.cs`

关注点：

- 模块入口约束
- 字段/属性/方法的顶层展开
- 成员类、继承、构造函数重载协议
- 稳定命名、导出规则、导入头输出

### 2. 语义级 lowering

- `SemanticWalkerBoundaryTest.cs`
- `SemanticWalkerCollectionExpressionCarrierTest.cs`
- `SemanticWalkerCreationTest.cs`
- `SemanticWalkerDeclarationTest.cs`
- `SemanticWalkerLoopTest.cs`
- `SemanticWalkerOrdinaryTest.cs`
- `SemanticWalkerPatternTest.cs`
- `SemanticWalkerReferenceTest.cs`
- `SemanticWalkerStringTest.cs`
- `SemanticWalkerSwitchTest.cs`
- `SemanticWalkerTryCatchTest.cs`
- `SemanticWalkerTupleTest.cs`
- `SemanticWalkerInvalidTest.cs`

关注点：

- 表达式、语句、控制流
- tuple / `ref` / `out` / 解构 / 模式匹配
- 宿主映射、静态宿主修正、白名单消费
- 创建表达式、初始化器、集合 carrier
- 显式失败路径与边界拒绝

### 3. 输出、命名与映射基础设施

- `UniqueNameAllocatorTests.cs`
- `WhiteListLookupCompatibilityTests.cs`
- `OptimizerTest.cs`

关注点：

- 临时名、helper 名和 overload 名的稳定性
- 白名单查找兼容性
- writer / optimizer 输出形态

### 4. SourceOrigin / SourceMap / catalog

- `SemanticWalkerSourceOriginTest.cs`
- `SemanticWalkerSourceMapEmissionTest.cs`
- `ESGeneratorSourceMapCatalogTest.cs`

关注点：

- lowering 后的来源锚点是否保留
- writer 输出的 map 内容是否稳定
- `ESGenerator` 收集的 catalog / source map carriers 是否一致

## 辅助文件

- `SourceMapTestHelpers.cs`：SourceMap 断言与读取辅助
- `MSTestSettings.cs`：测试设置
- `coverlet.runsettings`：覆盖率配置

## 运行方式

运行整个编译器测试项目：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
```

运行单个测试类：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter SemanticWalkerReferenceTest
```

运行单个测试方法：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter FullyQualifiedName~SemanticWalkerTupleTest.Visit_TupleLiteral_NamedTuple_UsesCurrentViewNames
```

采集原始覆盖率报告：

```powershell
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --settings src/Jazor.CompilerTest/coverlet.runsettings --collect:"XPlat Code Coverage"
```

执行正式 coverage gate（至少 8,000 个通过测试、95% 行覆盖、90% 分支覆盖）：

```powershell
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

`coverlet.runsettings` 只定义采集范围；验收必须走 coverage gate，不能把“生成了报告”当成“达到阈值”。

仓库级快捷入口：

```powershell
dotnet run --file ./scripts/csharp/test-dotnet.cs
```

## 编写测试时的约束

新增或修改测试时，优先遵守这些原则：

1. 先锁定行为契约，再断言具体输出形态。
2. 对 tuple、`ref/out`、导入、source-origin、稳定命名这类高风险语义，同时覆盖 happy path 和失败路径。
3. 若行为依赖 `AstConverter` 与 `SemanticWalker` 协作，优先分别补模块级测试和语义级测试，不要只在一侧断言。
4. 若改动影响 source map、catalog 或输出文本，补对应的 `SourceOrigin` / `SourceMap` / `ESGenerator` 测试。
5. 若新增 helper、dispatcher 或 synthetic temp，必须证明名称稳定，不受遍历顺序影响。

## 关注重点

这个项目不维护“编译器支持全部 C# 语法”的清单。更准确的理解方式是：

- 已进入主线的能力，必须由测试锁定当前契约
- 明确不支持或仍保留扩展点的能力，必须由测试或断言证明“受控失败”
- 文档、实现、测试三者不一致时，应优先修正测试与文档，使其回到当前源码契约
