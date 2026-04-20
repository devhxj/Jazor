# Walker Extension Spec

## 1. 适用范围

本文档约束 `SemanticWalker` 的扩展方式，目标是让新增语法支持保持：

- 语义优先
- 分文件可维护
- 上下文显式传递
- 可测试
- 可拒绝

凡是模块入口类型、成员拆解、导出策略，不在本文档内。

## 2. 核心扩展原则

### 2.1 先判断是不是 `SemanticWalker` 的职责

只有当问题属于下面几类时，才应进入 `SemanticWalker`：

- 表达式求值语义
- 语句控制流
- 局部变量与作用域
- 模式匹配、解构、异常、循环
- 方法调用、字段/属性/索引/方法引用
- 对象/数组/匿名对象/元组等方法体内构造语义

### 2.2 不依赖向上遍历

新增语法时，默认禁止用 `operation.Parent` 推断关键语义。优先做法：

- 新增 `Sense`
- 扩展 `SenseArgument`
- 从调用方显式下传上下文

### 2.3 明确失败优于错误 AST

如果某种 C# 语义在 JS 中无法保持等价：

- 优先通过 Analyzer 拒绝
- 否则在 `SemanticWalker` 中显式失败

不要输出“看起来能跑、实际语义错”的 AST。

## 3. 分文件扩展规则

`SemanticWalker` 当前按语法域拆分。新增能力时优先落到现有分域：

| 分域 | 负责内容 |
|------|----------|
| `Pattern` | `is`、递归模式、列表模式、声明模式 |
| `Loop` | `for`、`foreach`、`while`、`do-while` |
| `Switch` | `switch` 语句/表达式 |
| `String` | 插值字符串 |
| `TryCatch` | `try/catch/finally/throw` |
| `Declaration` | 变量、参数、局部函数、声明表达式 |
| `Ordinary` | 赋值、算术、逻辑、条件、lambda、return |
| `Reference` | 字段/属性/方法/数组索引/接收者 |
| `Creation` | 对象/数组/匿名对象/初始化器/委托创建 |
| `Tuple` | 元组创建、比较、解构 |
| `NotSupport` | 显式拒绝的 Roslyn operation |

如果一个新语法横跨多个分域，应先确定“主语义归属”，剩余部分用公共辅助方法协同，避免复制逻辑。

## 4. `Sense` 设计规则

新增 `Sense` 时，只描述语义上下文，不描述纯语法外形。

好例子：

- `PropertyWrite`
- `PatternInput`
- `ObjectInitializer`

坏例子：

- `VisitIfCondition`
- `ParenthesizedExpression`

判断标准：

- 如果它能复用于多个语法结构，就是好的 `Sense`
- 如果它只对应单一语法节点名字，通常不该成为 `Sense`

## 5. 依赖收集规则

### 5.1 变量声明

需要临时变量时，必须通过 `SenseArgument.AddVarDeclarator` 收集，并在块级/函数级统一 flush。

不要在多个子表达式里直接手工插入重复 `let` 声明。

### 5.2 导入声明

外部宿主 API 若落到 `Op.Import`：

- `SemanticWalker` 只负责收集 import 规格
- 不应在方法体内部直接伪造 `import`
- 真正的 `ImportDeclaration` 输出属于模块层闭环

## 6. 白名单扩展规则

### 6.1 何时用 `Alias`

适合：

- 纯名称替换
- 不改变参数结构
- 不改变求值策略

例：

- `Console.WriteLine -> console.log`
- `Property.get -> length`

### 6.2 何时用 `Inline`

只适合简单纯表达式模板。

本项目已选定内部实现方向为：

- 模板预解析 AST
- 占位符 AST 替换

而不是字符串替换后再次 parse。

典型适用：

- 数值计算
- 简单比较
- 明确括号包裹的宿主表达式

详细约束见 [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)。

### 6.3 何时必须升到 `Compile`

以下任一情况应优先考虑 `Op.Compile`：

- 参数可能是对象字面量
- 参数可能是逗号表达式
- 参数位置涉及成员访问、索引、`new`、可选链
- 需要按参数 AST 形状做条件化改写
- 需要保证 tuple、pattern、collection 等复杂输出不被字符串模板破坏

## 7. 语法节点回退规则

当前 `SemanticWalker` 有局部语法节点回退能力，但它只能作为补丁手段。

允许回退的前提：

- Roslyn `IOperation` 无法表达所需细节
- 回退代码有明确边界
- 有独立测试覆盖

不允许把大块主语义长期建立在语法节点 switch 上，否则会和 `IOperation` 主模型分裂。

## 8. 不支持语法的处理方式

新增一种“不支持”的 C# 语义时，应先分类：

1. 理论上可支持，但当前未实现
2. JS 无法语义等价
3. Roslyn 内部 operation，非用户语义

对应策略：

- 类别 1：在对应分域留扩展点，并补 TODO 文档
- 类别 2：显式抛异常
- 类别 3：放入 `NotSupport` 分域统一拒绝

## 9. 测试规则

每次扩展 `SemanticWalker` 至少要补三类测试：

### 9.1 直接语义测试

验证该语法本身是否正确输出。

### 9.2 组合语义测试

验证它与以下至少一项组合后仍正确：

- 模式匹配
- tuple
- 对象初始化器
- 白名单调用
- ref/out
- 可选链 / null 语义

### 9.3 失败路径测试

验证不支持场景是否显式失败，而不是静默产生错误 AST。

## 10. 新语法落地模板

新增一个 `IOperation` 支持时，建议按以下顺序：

1. 确认 Analyzer 是否允许该输入进入
2. 确认主落点分域
3. 如需上下文，先补 `Sense` / `SenseArgument`
4. 实现 `Visit*`
5. 复用 `Translate<T>` / `HandleTransformationFailure`
6. 补测试
7. 补同名文档

## 11. 高风险信号

出现以下信号时，应暂停继续堆实现，先做重构：

- 同一语义在多个分文件重复拼 AST
- 新逻辑必须依赖 `ToKnRECMAScript()` 再 parse
- 只能靠 `operation.Parent` 判断核心语义
- 临时变量声明散落在多个分支，无法统一提升
- 一个新语法需要同时改十几个 Visit 方法且没有公共抽象

## 12. 复核清单

1. 这个改动是否真属于 `SemanticWalker`
2. 是否引入了新的 `Sense`，且命名描述的是语义上下文
3. 是否保持了变量声明提升和作用域隔离
4. 是否错误使用了字符串模板代替 AST 结构
5. 是否把“不支持”与“未实现”区分清楚
6. 是否同步补了对应 `SemanticWalker*Test`
7. 是否需要新增或更新对应 `doc/SemanticWalker.*.md`
