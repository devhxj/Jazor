# Module Conversion Spec

## 1. 适用范围

本文档只约束模块级转换，也就是：

- 输入：带 `[ECMAScriptModule]` 的类型
- 入口：`ESGenerator`
- 主转换器：`AstConverter`
- 输出目标：ES module 顶层声明集合

凡是方法体、表达式、语句、模式匹配、局部作用域等问题，不在本文档内，统一下沉到 `SemanticWalker` 规范。

## 2. 输入契约

当前实现与测试共同定义的事实契约：

- 模块入口类型必须是顶层类型
- 当前 `AstConverter` 显式要求类型为 `public`
- 当前测试和设计语义均以“模块类 = `public static class`”为主
- 嵌套模块类当前直接拒绝

建议后续统一补强为显式契约：

1. 必须是 `public static class`
2. 不允许实例字段、实例属性、实例方法
3. 不允许析构函数、事件、实例构造函数
4. partial 类型必须使用对应语法树的 `SemanticModel`

## 3. 成员分类与映射

`AstConverter` 当前按成员种类做分派：

| C# 成员 | 模块输出 | 说明 |
|---------|----------|------|
| 静态字段 | `let` / `const` | 常量与 `init` 倾向于 `const` |
| 静态属性 | `get_Name` / `set_Name` 函数 | 自动属性用 backing field |
| 静态方法 | `function Name(...) {}` | 方法体由 `SemanticWalker` 生成 |
| 嵌套类 | `class` | 当前仅支持作为成员类输出 |
| 枚举 | `const X = Object.freeze({...})` | 枚举成员转对象字面量 |

未识别成员当前一般抛 `NotSupportedException`，不应静默忽略。

## 4. 可见性与导出规则

当前实现规则：

- `public`、`internal` -> 导出
- 其他访问级别 -> 模块私有

当前规则是工程约定，不是 C# 原生可见性的严格镜像。后续若要扩展 `protected`、`protected internal`，必须先定义 JS 模块级语义，再改实现与测试。

## 5. 字段与属性规范

### 5.1 字段

- `const` 字段应生成 `const`
- 普通静态字段应生成 `let`
- 非字面量初始化器应优先通过 `SemanticWalker` 处理，而不是在 `AstConverter` 中继续堆语法特判

### 5.2 自动属性

自动属性当前通过哈希后的 backing field 表示。

要求：

- backing field 名称必须稳定
- getter/setter 必须和 backing field 保持一一对应
- `init` only 属性若被当作模块级只写初始化，应优先落成不可变绑定

### 5.3 名称来源

模块成员名称必须统一走：

1. `ECMAScriptNameAttribute`
2. `Description("@#name")`
3. 默认符号名 / 哈希名

不允许在字段、属性、方法之间混用不同命名策略。

## 6. 方法输出规范

### 6.1 方法体来源

方法可来源于：

- 块体 `Body`
- 表达式体 `ExpressionBody`
- 自动属性 accessor

块体和表达式体都应先取 `IOperation`，再交给 `SemanticWalker`。

### 6.2 参数规则

- 默认参数应转换为 JS 默认参数
- `ref/out` 参数目前需要与 `SemanticWalker` 的调用约定保持一致
- 重载方法必须在命名阶段稳定区分

### 6.3 未来规则

后续如要支持：

- `async`
- `generator`
- `ECMAScriptInline`

必须先把方法级契约写清楚，再调整 `FunctionDeclaration` 的生成位。

## 7. 嵌套类型规范

当前模块层允许成员类和枚举，但存在明显边界：

- 模块类自身不支持再作为嵌套模块类处理
- 多层嵌套类的命名和访问路径尚不稳定
- 嵌套类内部再出现复杂成员时，仍依赖 `AstConverter` / `SemanticWalker` 的递归一致性

建议未来统一原则：

1. 模块入口类不嵌套
2. 普通成员类允许嵌套一层
3. 超过一层前，先补命名、访问和测试矩阵

## 8. Import 规范

当前状态：

- `SemanticWalker` 可通过白名单 `Op.Import` 收集导入规格
- `SenseArgument` 里保存了导入分组
- `AstConverter` 的 `_imports` 还没有真正消费这些分组

所以目前导入机制只完成了“发现”与“收集”，还没有完成“模块头输出”。

未来落地顺序应为：

1. `SenseArgument` 增加导入 flush 能力
2. `AstConverter` 在模块头生成 `ImportDeclaration`
3. 合并同路径导入并去重
4. 为 `Import` 场景补充测试

## 9. 错误与拒绝策略

模块层应坚持以下原则：

- 输入前提不满足 -> 立即失败
- 成员类型未知 -> 立即失败
- 复杂初始化器不会写 -> 下沉到 `SemanticWalker`
- 导入未闭环 -> 不伪造错误的 `import`

也就是说，模块层的鲁棒性来自“边界清晰”，而不是“兜底吞掉不支持成员”。

## 10. 模块层扩展步骤

新增一种模块成员支持时，固定按以下步骤：

1. 明确它是模块层问题，而不是语义层问题
2. 定义成员到 ES module 顶层声明的映射
3. 定义导出/私有规则
4. 定义命名规则
5. 补 `AstConverterTests`
6. 只在需要时才下沉到 `SemanticWalker`

## 11. 复核清单

1. 新成员是否应由 `AstConverter` 负责，而不是 `SemanticWalker`
2. 成员命名是否统一走 `Util.GetConfigOrSymbolName`
3. 导出规则是否与现有 `public/internal` 约定一致
4. 是否错误复用了单一 `SemanticModel`
5. 是否补了对应的 `AstConverterTests`
6. 若涉及导入，是否同步补了模块头输出链路
