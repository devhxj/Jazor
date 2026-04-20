# `ESGenerator`

## 定位

`ESGenerator` 是当前的 Roslyn 增量源生成器入口。

对应代码：

- `src/Jazor.Compiler/ESGenerator.cs`

它当前不是直接往磁盘生成 `.mjs` 文件，而是：

- 收集 ECMAScript 模块候选类型
- 调用 `AstConverter`
- 把转换结果写进生成的 C# 模块目录表

## 当前职责

### 1. 发现模块候选

`Initialize(...)` 使用：

- `ForAttributeWithMetadataName("ECMAScript.ECMAScriptModuleAttribute", ...)`

筛出带 `[ECMAScriptModule]` 的 `ClassDeclarationSyntax`。

当前 `CreateCandidate(...)` 会保留：

- `INamedTypeSymbol`
- `SemanticModel`
- `Location`

这为后续错误报告和 AST 转换提供了完整上下文。

### 2. 组合编译与候选集

当前增量管道会把：

- `CompilationProvider`
- 收集后的模块候选数组

组合后送入 `EmitCatalog(...)`。

这说明生成逻辑不是按单个节点零散落地，而是以“当前编译单元的模块清单”为中心。

### 3. 调用 `AstConverter`

`EmitCatalog(...)` 会对每个去重后的模块类型：

1. 构造 `AstConverter`
2. 执行 `Convert()`
3. 优先走 `ToKnRECMAScriptWithSourceMap(...)` 产出 `JS + map`
4. map 失败时降级为 `ToKnRECMAScript()`（仅 JS）
5. 生成 `GeneratedModuleInfo`

当前生成的信息包括：

- `AssemblyName`
- `TypeName`
- `Id`
- `RelativePath`
- `Content`
- `Hash`
- `SourceMapRelativePath`（可空）
- `SourceMapContent`（可空）
- `MapHash`（可空）

### 4. 生成模块目录表

当前真正写出的 source 只有一个：

- `Jazor.Generated.ModuleCatalog.g.cs`

当模块有 sourcemap 内容时，还会额外生成：

- `Jazor.Generated.ModuleSourceMapCatalog.g.cs`

其中包含：

- `ModuleCatalog.AssemblyName`
- `GetModules()`
- 私有 `GeneratedModule` 类型
- `_modules` 静态数组

也就是说，当前 ESGenerator 的产物是：

- C# 内部可访问的模块描述目录表

而不是旧文档里说的：

- 直接输出占位 `.mjs` 源码文件

### 5. 路径与哈希管理

`GetRelativePath(...)` 当前优先读取：

- `[ECMAScriptModule("...")]`

里的路径参数。

如果没有显式路径，则按：

- `AssemblyName/Namespace/TypeName.mjs`

构造默认相对路径。

同时：

- 非 `.js` / `.mjs` 后缀会自动补 `.mjs`
- `..` 路径逃逸会直接报错
- 内容还会计算 SHA-256 十六进制哈希

## 当前关键规则

### 1. 生成器现在产出的是 C# catalog，不是 `.mjs` 文件

这是当前实现与旧文档最大的偏差。

当前 `context.AddSource(...)` 添加的是：

- `Jazor.Generated.ModuleCatalog.g.cs`

不是：

- `{TypeName}.mjs`

### 2. 候选按类型名去重

`EmitCatalog(...)` 内会用 `HashSet<string>` 按 `typeName` 去重。

这样可避免同一模块候选在增量管道里重复生成。

### 3. 失败会报告 `JAZORG001`

`AstConverter` 或路径规范化等步骤抛异常时，当前不会让整个生成器静默失败，而是：

- 通过 `ModuleGenerationFailed`
- 报告诊断 `JAZORG001`

### 4. sourcemap 失败降级为 warning（`JAZORG002`）

如果 JS AST 已经成功生成，但 sourcemap 组装失败：

- 报告 `JAZORG002`（Warning）
- 保留 JS 输出（`ModuleCatalog` 仍生成）
- 跳过该模块的 sourcemap catalog 记录

也就是说，sourcemap 失败不再把整个模块生成打成 Error。

### 5. 最终目录表会排序

生成前会按：

1. `RelativePath`
2. `TypeName`

排序。

这保证生成结果稳定，减少无意义 diff。

## 现状与典型结果

当前生成的 C# 目录表会包含类似结构：

```csharp
public static partial class ModuleCatalog
{
    public static string AssemblyName { get; } = "...";

    public static System.Collections.IEnumerable GetModules()
    {
        return _modules;
    }
}
```

内部每个模块记录：

- 程序集名
- 类型名
- 相对路径
- JS 内容
- 内容哈希

## 当前边界

这部分当前已经解决的是：

- 模块候选发现
- `AstConverter` 接入
- 相对路径标准化
- JS 文本和哈希收集
- 生成稳定排序的模块目录表

它没有试图做这些事情：

- 直接向项目输出物目录写 `.mjs`
- 直接发出独立磁盘文件
- 建立运行时模块加载器
- 在生成器层做更深的 AST 优化

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/ESGeneratorTests.cs`

当前已有测试重点验证：

- 引用外部模块程序集时不会产生类型冲突告警
- 生成的 `ModuleCatalog` 结构正确
- 内部 `GeneratedModule` 不泄漏为公共顶层类型

## 推荐阅读

建议按这个顺序看：

1. [AstConverter.md](./AstConverter.md)
2. [ESGenerator.md](./ESGenerator.md)
3. [SemanticWalker.md](./SemanticWalker.md)

## 相关文档

- [AstConverter.md](./AstConverter.md)
- [SemanticWalker.md](./SemanticWalker.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
