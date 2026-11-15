# 类型、宿主标注与迁移

普通源码类型通过编译入口和源码依赖闭包参与转换。`[ECMAScript]` 表达 JavaScript 宿主含义；`[ECMAScriptModule]` 表达生成模块的身份和路径。具体操作能否转换，最终由核心编译器在使用点判断。

## 源码与外部依赖

| 输入 | 当前契约 |
| --- | --- |
| 编译入口可达的普通源码 class、helper 和成员 | 生成受支持的实现，无需逐类型添加宿主标注 |
| 已编译的 Jazor 类库 | 通过程序集中的 `Jazor.Generated.ModuleCatalog`、导出与依赖交付实现 |
| 已声明的 JavaScript 资源库 | 通过 binding、npm/JSR identity、标准 ESM specifier 和资源闭包接入 |
| 普通 NuGet / CLR DLL | 按实际使用点自动恢复已解析实现程序集，恢复并编译可达的受支持 class 成员；其余外部运行时操作仍需映射或已有产物，见[类库导入](../04-roadmap/ordinary-nuget-compilation-research.md) |

取消标注许可不意味着自动反编译或转换任意 DLL。普通类库按需恢复的支持范围见[类库导入](../04-roadmap/ordinary-nuget-compilation-research.md)。跨项目引用和两类产物 carrier 见[类库契约](../02-architecture/library-artifact-contract.md)。

## class 与 record

| 声明 | 运行时表示与边界 |
| --- | --- |
| 普通源码 class | 保留受支持的构造、字段初始化、实例成员和原型分派；不因空名称或宿主标注变成对象字面量 |
| 数据 record | 创建为结构化对象，`with` 使用对象复制；使用位置参数或受支持的属性初始化 |
| readonly record struct | 使用已支持的结构化表示；不保留完整 CLR runtime identity |
| 可变 record struct | 缺少隐式值复制协议，明确拒绝 |
| 带可执行自定义构造函数的 record | 明确拒绝丢弃构造行为；改用位置参数数据初始化，或使用 class 承载行为 |
| 宿主 class | 使用已声明的外部 JavaScript 类型、构造器和映射；缺失运行时身份时明确诊断 |

配置对象可以直接使用普通 record：

```csharp
public sealed record JsOptions(string Title)
{
    public bool Enabled { get; init; } = true;
}

var options = new JsOptions("release");
```

需要 Vue 深层响应式跟踪的数据仍可保留可写属性，写法见 [TodoList 样例](../../samples/RazorVue.TodoList/README.md)。这不扩展为任意 CLR 结构体复制或 record 构造协议。

## 宿主标注

| 标注 | 职责 |
| --- | --- |
| `[ECMAScript]` | 环境 JavaScript 宿主或明确的特殊值语义 |
| `[ECMAScript("specifier")]` | 外部 ESM 导入来源；成员声明优先于类型声明 |
| `[ECMAScriptModule("path")]` | 编译根与模块输出身份；普通辅助类型沿源码闭包参与转换 |
| `[ECMAScriptName]`、`[Description("@#...")]` | 名称和属性键解析，不改变 class/record 的对象模型 |
| `[Style]`、`[ECMAScriptInline]` | 样式依赖和已声明表达式模板 |

宿主 union 的擦除与普通源码准入是独立规则。优先使用分支互不赋值的原生 `union`；分支可赋值重叠、需要精确投影时使用已声明的 tagged fallback。保留 `Value`、`AsX` 和接收者单次求值契约，不能把任意 union 当作原始 JavaScript 值。

`ECMAScript.Number` / `BigInt` 的模式检查描述 JavaScript 值域。CLR 数值装箱后的类型检查只有在类型身份可证明时折叠；共享标量 carrier 上的模糊 CLR 身份明确拒绝。

## 擦除位置与诊断

泛型实参和数组元素类型通常是擦除的编译期信息。Compiler 不会仅因 `List<Unsupported>` 或 `Unsupported[]` 的类型形状而拒绝转换；具体构造、默认值、成员访问和运行时类型检查仍须有可用实现。

Analyzer 可以在封闭的具体外部类型出现在签名、局部推断或集合目标类型时更早诊断。泛型参数 `T` 本身允许使用，不追踪其来源或数据流。这种更早的诊断不改变 Compiler 在实际运行时使用点验证的职责。

## 迁移旧标注与对象模型

1. 移除仅为编译许可添加在普通业务类型上的 `[ECMAScript]`；保留真实环境宿主、外部 ESM、extern 成员与特殊宿主 union 的标注。
2. 原来通过标注和空名称生成对象字面量的 options class 改为数据 record，同时审查继承、初始化与属性命名。
3. record 的构造行为改为位置参数或属性初始化；需要执行逻辑时使用 class。可变 record struct 不作为共享引用对象使用。
4. 保留模块入口、路径、导出和资源声明；在源码项目与隔离 package consumer 中验证实际调用。

1.2.0 的 class→record 调整按维护者批准的版本例外交付，迁移背景见 [CHANGELOG](../../CHANGELOG.md)。1.3.0 的[逐项语法修复记录](../04-roadmap/compiler-syntax-bug-fixes.md)区分运行行为修正与明确的不支持诊断。

底层扩展决策以 [Compiler 实现原则](../../src/Jazor.Compiler/ImplementationPrinciples.md)为准。
