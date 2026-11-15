# Jazor.Compiler

> 定位：受控 C# 编译域到 ECMAScript 的核心 lowering 层。

`Jazor.Compiler` 以 Roslyn `IOperation` 为语义输入，以 Acornima ESTree 为中间表示，输出确定的 JavaScript module text、source-origin 和 `Jazor.Generated.ModuleCatalog` 记录。它不直接物化文件，也不为任何框架产品内置特判。

## 职责

- `AstConverter` 负责模块、类型、导入、导出和运行时成员类的结构转换。
- `SemanticWalker` 负责表达式和语句 lowering，以及运行时敏感使用点的最终验证。
- `ESGenerator` 负责 JavaScript 文本、source map 和 `ModuleCatalog` carrier；Jazor 自有 `.js`、项目入口和 bundle 由 `Jazor.Emit` 物化。
- WhiteList 与 `Alias`、`Inline`、`Import`、`Compile` 映射连接 CLR/host API 和 compiler lowering。

## 稳定边界

- 普通源码类型无需 `[ECMAScript]` 许可；`SourceTypeClosure` 收集入口可达的普通辅助类型，`AstConverter` 发射实现并提供共享的 `DeclaredNames`。宿主表达式通过 `SourceDependencyFilter` 声明依赖范围，可与普通成员发射范围分开。
- 普通 DLL/NuGet 根据实际使用点自动恢复已解析的实现程序集。ILSpy 在进程内恢复 C#，独立 Roslyn compilation 保留程序集身份并结构匹配成员；导入计划计算可达闭包，`SemanticWalker` 复用已有 lowering/import 与使用点验证。每个实现输出共享模块，source map 标识恢复源码。支持范围、缓存与配置见[类库导入](../../docs/04-roadmap/ordinary-nuget-compilation-research.md)。
- class 保留类的默认构造与实例语义，record / readonly record struct 创建生成结构化对象。可变 record struct 的隐式复制和 record 的可执行自定义构造协议暂不支持，并在使用点明确拒绝。命名配置不会把 class 转换成对象字面量；配置对象应声明为 record。
- 目标是保持使用点可观察行为和确定性输出，不是复刻完整 CLR runtime identity。
- 支持范围由已绑定的 Roslyn 语义、白名单映射和测试共同定义；不支持的运行时敏感类型或成员必须显式失败。
- `Jazor.Analyzer` 可以在 erased positions 提前诊断，`SemanticWalker` 仍在真正 lowering 处做最终裁定。
- interface 只作为编译期契约，不发射 runtime declaration；enum、tuple、record、`ref/out` 等按照各自已实现的擦除或协议语义处理。
- temp 名、import alias、helper 名、声明排序和 source-origin 锚点都是长期输出契约。
- 内建整数算术遵守声明位宽：除法向零截断，零除和有符号除法溢出抛错，`checked` 检查范围，`unchecked` 回绕；显式整数转换、复合赋值和更新复用同一数值规则。浮点到整数先截断，非有限值转换显式抛错；这不扩展 decimal、Int128 或完整 CLR 异常类型体系。
- 长度分配数组和未初始化模块字段生成类型对应默认值；多维数组的维度只求值一次，结构化元素默认值独立创建。普通模块先为存储赋默认值，再执行会读取模块状态的非恒定初始化表达式。
- 含 `return`、`await`、`yield` 或跨循环转移的模式 switch 保留原函数控制流；case 测试用到的绑定先在 switch 外层作用域声明。
- `dynamic` 运算/转换、表达式树、指针/fixed、持久 ref 别名/ref 返回、不能证明位置不同的多 ref/out 调用、多播委托、泛型类静态存储、静态抽象/默认接口协议、自定义枚举器和 awaiter 不会静默降为不同的 JS 行为。数值装箱类型判定仅在 CLR 类型可证明时折叠，否则明确拒绝。

## 扩展位置

| 问题 | 归属 |
| --- | --- |
| 输入域的前置诊断 | `Jazor.Analyzer` |
| 外部 API 映射 | `Jazor.CLR` 或 `ECMAScript` 标注与 WhiteList 生成 |
| 模块级结构 | `AstConverter` |
| C# 表达式、语句和使用点验证 | `SemanticWalker` |
| 文件输出与 bundle | `Jazor.Emit` |

复杂且依赖 temp、import、source-origin 或语句级协议的 host 语义，必须通过正式 compiler extension seam 实现，不能绕过 compiler 手拼 JavaScript 或 AST。

## 代码结构

- `AstConverter.cs`：模块和声明转换。
- `core/SemanticWalker.cs.*`：语义 lowering 的分区实现。
- `ESGenerator*.cs`：JavaScript writer、source map 与 `ModuleCatalog` carrier。
- `WhiteList.cs.*`：宿主映射消费；`WhiteList.cs.Generate.cs` 为生成结果。
- `ImplementationPrinciples.md`：支持边界与设计理由的权威说明。

## 验证

```bash
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
```

## 相关文档

- [ImplementationPrinciples.md](./ImplementationPrinciples.md)
- [Jazor.CompilerTest](../Jazor.CompilerTest/README.md)
- [编译器架构](../../docs/02-architecture/compiler.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
