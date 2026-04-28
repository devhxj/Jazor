# Jazor.CLR

`Jazor.CLR` 是 Jazor 的 CLR 运行时映射层。  
它负责把可支持的 .NET 类型/成员声明为白名单事实，并为 `Op.Import` 成员提供 JavaScript 语义实现（以 C# 语法编写）。

## 定位与边界

- `Jazor.CLR` 是 **producer 侧事实来源**：通过 `[Jazor]` 声明类型/成员映射规则。
- `Jazor.Analyzer` 消费白名单做前置诊断（可比编译器更严格）。
- `Jazor.Compiler` 在真正 lowering 时再次按使用点做最终验证与分发。
- `Jazor.Emit` 负责 `.mjs/.map` 等输出物化，不在 `Jazor.CLR` 内。

## 协作链路

```text
Jazor.CLR (module/*.cs 上的 [Jazor] 声明)
    -> Jazor.Compiler.Generator 扫描并生成
       src/Jazor.Compiler/WhiteList.cs.Generate.cs
    -> Jazor.Analyzer / Jazor.Compiler 消费白名单
    -> 编译结果按需导入 System/...Module.js
```

## 目录结构

```text
src/Jazor.CLR/
├── GlobalUsings.cs
├── Jazor.CLR.csproj
├── module/                  # CLR 模块实现（主工作目录）
└── doc/                     # 模块签名/哈希参考文档
```

说明：
- 模块与文档数量不在 README 中维护，请以 `module/*.cs` 与 `doc/*.md` 当前内容为准。
- 不维护“完成率/通过率”静态快照，这类信息以当前测试与 CI 为准。

## 声明模型

### 类型级声明

```csharp
[ECMAScriptModule("System/BooleanModule.js")]
[Jazor(Op.Alias, "bool", "Boolean")]
public static class BooleanModule
{
    // ...
}
```

- `[ECMAScriptModule]`：声明模块导入路径。
- 类型上的 `[Jazor(...)]`：声明 C# 类型到 JS 运行时对象的映射（通常 `Op.Alias`）。

### 成员级声明与 Op 语义

| Op | 是否可 `extern` | 是否需要方法体 | 典型场景 |
|---|---|---|---|
| `Discard` | 是 | 否 | 明确不支持 |
| `Allowed` | 是 | 否 | JS 原生语义可直接承接 |
| `Alias` | 是 | 否 | 名称映射（如 `Count -> length/size`） |
| `Inline` | 是 | 否 | 稳定单表达式模板 |
| `Import` | 否 | 是 | 复杂逻辑、校验、异常协议、helper 复用 |
| `Compile` | 是 | 否 | 编译器内部钩子（极窄场景） |

## 哈希命名约定

`Jazor.CLR` 成员方法名使用 `_xxxxxxxxxxxxxxxx` 形式（`_` + 16 位十六进制）。

- 生成规则来自 `Format.HashName(...)`：对成员签名做 SHA256，取前 8 字节。
- 实际签名文本应使用 Jazor.Name 统一格式（与白名单 lookup 一致）。
- 现有签名可参考 `src/Jazor.CLR/doc/*.md`，避免手写漂移。

## 关键实现约束

### Inline 与 Import 取舍

- `Inline` 保持短小、可读、单表达式。
- 需要分支、循环、异常协议、重复 guard 或跨成员复用时，优先改为 `Import`。
- 不要机械地把所有逻辑都塞进 `Import`；若编译器已保证某些语义，避免重复校验导致行为漂移。
- 同一 API 面尽量保持策略一致（避免 concrete/interface 路径实现风格分裂）。

### Comparer 家族一致性

新增 comparer 能力时，把 concrete 与 interface 看作同一契约面：

- `EqualityComparer<T>` 与 `IEqualityComparer<T>`（以及 `System.Collections.IEqualityComparer`）应成套覆盖可达路径。
- 语义共享时集中到同一核心 helper，避免 null/NaN/identity 处理漂移。
- 每次扩展都必须同时补：
  - `src/Jazor.CLR.Test`：白名单元数据断言（类型别名 + 成员 Op/路径）
  - `src/Jazor.CompilerTest`：编译产物断言（concrete 调用 + interface 调度）

### `out/ref` 返回协议

`Jazor.CLR` 使用数组模拟 `out/ref`：

```csharp
// [returnValue, out1, out2, ...]
public static Array<object?> _hash(...);
```

- 索引 `0` 是方法返回值（`void` 时按协议返回占位值）。
- 索引 `1..n` 按声明顺序返回 `out/ref` 参数值。

## 开发流程（新增/修改映射）

1. 在 `src/Jazor.CLR/module/*Module.cs` 修改类型或成员 `[Jazor]` 声明。
2. 按语义复杂度选择 `Allowed/Alias/Inline/Import/Compile`，`Import` 提供完整方法体。
3. 需要时更新 `src/Jazor.CLR/doc/*.md` 对应签名说明。
4. 运行白名单生成器，刷新 `src/Jazor.Compiler/WhiteList.cs.Generate.cs`。
5. 补齐 CLR 与 Compiler 两侧回归测试。

## 常用命令

在仓库根目录执行：

```powershell
# 构建 CLR 项目
dotnet build src/Jazor.CLR/Jazor.CLR.csproj --no-restore -v minimal

# 运行 CLR 白名单与特征测试
dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj

# 运行编译器侧回归（可按需加 --filter）
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj

# 刷新白名单生成文件（修改 CLR 映射后必做）
dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj

# 仓库级快捷入口（CLR 切片）
pwsh ./scripts/test-dotnet.ps1 -Project clr
```

## 参考文档

- `docs/01-目标/clr/README.md`
- `docs/01-目标/compiler/WhiteList.md`
- `src/Jazor.Compiler/README.md`
- `src/Jazor.Compiler/ImplementationPrinciples.md`
