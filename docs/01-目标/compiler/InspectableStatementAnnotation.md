# Inspectable Statement Annotation

## 目录

- [1. 这份文档解决什么问题](#1-这份文档解决什么问题)
- [2. 最终决议](#2-最终决议)
- [3. 为什么不是 token 级后缀注释](#3-为什么不是-token-级后缀注释)
- [4. 开发视角评审](#4-开发视角评审)
- [5. 项目经理视角评审](#5-项目经理视角评审)
- [6. 核心规则](#6-核心规则)
- [7. 显示规则](#7-显示规则)
- [8. 字段、属性与 synthetic 节点规则](#8-字段属性与-synthetic-节点规则)
- [9. 抽象与实现边界](#9-抽象与实现边界)
- [10. 与 Sourcemap 的关系](#10-与-sourcemap-的关系)
- [11. 配置](#11-配置)
- [12. 测试策略](#12-测试策略)
- [13. 实施顺序](#13-实施顺序)
- [14. 验收标准](#14-验收标准)
- [15. 一句话结论](#15-一句话结论)

## 1. 这份文档解决什么问题

Jazor 在把 C# 降为 JavaScript 时，已经依赖稳定生成名来保证：

- 重载在 JS 侧的稳定区分
- `import` 重名冲突后的稳定规避
- 若干宿主映射和运行时对齐规则

这解决的是“正确性”问题，但会降低生成 JS 的人工可读性。  
开发者在检查输出时，往往很难直接判断某个稳定生成名对应哪个源声明。

这份文档的目标不是重新设计命名系统，而是给生成结果增加一层只服务人工检查的 annotation，并把范围、边界和实现方式一次性定死。

## 2. 最终决议

### 2.1 要做，但只做一个小范围版本

该能力值得做，但只值得做成“开发期可检查性增强”，不值得为它重构 writer 或 AST 命名体系。

它的正确定位是：

> 对模块级最终声明增加一层可丢弃、不可依赖、只服务人工检查的源符号注释。

### 2.2 第一阶段只做模块级最终 statement

第一阶段只覆盖模块级最终 statement：

- 模块级方法声明
- 模块级字段/变量声明
- 模块级类声明
- 模块级枚举声明
- 上述声明对应的最终 `export` statement

第一阶段明确不做：

- 类成员
- 函数体内部局部声明
- 普通调用点
- `import` 原生语法内部
- 对象字面量 key
- token 级标识符后缀注释
- sourcemap 联动

### 2.3 第一阶段统一采用前置块注释

第一阶段统一输出：

```js
/* MyType.Foo(int,string) */
export function Foo_ab12cd(...) {}
```

第一阶段明确不追求：

```js
export function Foo_ab12cd/* MyType.Foo(int,string) */(...) {}
```

原因不是“后缀形式没有价值”，而是当前 Jazor + Acornima 的结构更适合在声明级别插入 annotation，不适合在 identifier token 上附着注释。

### 2.4 annotation 不参与任何语义

annotation 只负责帮助人识别“这个声明是谁”，不参与：

- 命名
- 绑定
- 重载分发
- lowering
- 运行时行为
- 打包结果
- sourcemap

annotation 被删除后，生成结果的行为必须完全等价。

## 3. 为什么不是 token 级后缀注释

从工程实现上看，当前不适合优先做 token 级方案，原因有三点：

1. Acornima 支持 extension node 和自定义写出，但不原生提供“注释附着到 identifier token”的常规能力。
2. Jazor 当前在多数输出点已经把 symbol 信息收敛成名字字符串；如果强行做 writer 级 suffix comment，需要把 metadata 继续携带到 token 输出层，影响面过大。
3. 当前需求的核心是“声明检查”，不是“引用位或调用位调试”。声明级 annotation 已经足以覆盖主要收益。

因此，第一阶段应该做“statement 级 annotation”，而不是“identifier 级 annotation”。

## 4. 开发视角评审

### 4.1 第一轮开发评审：支持点

从开发视角，做一个小范围版本是合理的：

- 当前生成结果的可读性确实偏低，尤其是 hash / format name 出现后
- 模块级声明是最稳定、最容易插入 annotation 的位置
- symbol 在 `AstConverter` 里仍然可得，不需要倒推
- statement 级包装对现有 lowering 影响最小
- 该能力可以显著降低人工检查输出的成本

### 4.2 第一轮开发评审：反对点

从开发视角，如果范围不收敛，这个需求会迅速失控：

- 一旦试图进入 writer/token 层，metadata 传播会扩散到整个 emit 链
- 一旦覆盖调用点，输出噪音和体积会显著增加
- 一旦让 annotation 解释编译过程，它就会变成第二套调试元数据系统
- 一旦进入类成员、局部声明、`import` 原生结构内部，抽象边界会开始模糊

### 4.3 第二轮开发评审：收敛后的最终判断

在明确只做模块级最终 statement、只做前置注释、默认关闭之后，这个需求的技术风险是可控的。

因此开发侧最终结论是：

- 可以实现
- 但必须限制为 declaration-only
- 不应演变为通用 AST 注释能力
- 不应为此改写稳定命名链路

## 5. 项目经理视角评审

### 5.1 第一轮项目评审：价值判断

这个能力有明确价值，但价值属于“开发效率提升”，不是“用户能力提升”。

它带来的收益是：

- 更快检查生成结果是否符合预期
- 更快定位稳定生成名对应的源声明
- 降低编译器开发期的排查成本

它不直接带来：

- 更强的运行时能力
- 更完整的编译语义
- 更好的终端用户体验

### 5.2 第一轮项目评审：风险判断

如果对它的定位不清晰，会出现三类项目风险：

1. 需求漂移  
   团队可能把它当作 sourcemap 的替代物，继续往里面塞过程信息和定位能力。

2. 范围失控  
   团队可能从模块级声明一路扩到调用点、helper、局部声明，导致收益不再匹配成本。

3. 维护价值不足  
   如果默认开启并污染现有测试，维护成本会超过它带来的开发收益。

### 5.3 第二轮项目评审：收敛后的最终判断

这个能力可以作为一个小需求立项，但必须满足：

- 默认关闭
- 不阻塞主线编译器工作
- 不绑定大规模测试
- 不承诺自然扩展到 token 级注释系统

因此项目侧最终结论是：

- 值得做
- 但只能作为低优先级、低范围、低风险的开发辅助增强项

## 6. 核心规则

### 6.1 annotation 只描述结果映射

允许：

```js
/* MyType.Foo(int,string) */
export function Foo_ab12cd(...) {}
```

不允许：

```js
/* overload selected #2 after remap */
export function Foo_ab12cd(...) {}
```

annotation 只回答“这是什么声明”，不回答“编译器是怎么做出这个结果的”。

### 6.2 annotation 只跟随最终声明单元

annotation 应该跟随最终 statement，而不是内部 declaration 片段。

例如，对公开方法，annotation 应包最终 `ExportNamedDeclaration`，而不是先包内部 `FunctionDeclaration` 再外层 export。

### 6.3 annotation 必须可被完全删除

删除所有 annotation 后，生成结果的语义、绑定、运行时行为和打包结果都必须保持不变。

## 7. 显示规则

### 7.1 统一 formatter

annotation 文本必须通过统一 formatter 生成，不允许在多个 emitter 位置手工拼接。

推荐统一入口：

- `FormatInspectableSymbol(ISymbol symbol)`

允许按符号种类分少量分支，但显示规则必须保持全局一致。

### 7.2 默认显示短、稳定、可预测的签名

推荐显示格式：

- 方法: `Type.Method(T1,T2)`
- 构造: `Type..ctor(T1,T2)`
- 属性 getter: `Type.Name.get`
- 属性 setter: `Type.Name.set(T)`
- 字段: `Type.Field`
- 类: `Type`
- 枚举: `Type`

默认使用短类型名：

- `System.Int32 -> int`
- `System.String -> string`
- `System.Boolean -> bool`
- `System.Object -> object`
- `System.Void -> void`

默认不带完整命名空间。

### 7.3 当短名不够区分时，允许更具体显示

默认短名显示不是强约束。  
当短名不能稳定区分不同符号时，formatter 允许升级到更具体的显示。

这类升级只影响可读性，不影响任何语义。

## 8. 字段、属性与 synthetic 节点规则

这一部分最容易反复，必须单独定规则。

### 8.1 源里真实存在的成员使用 identity annotation

对源代码中真实存在的成员，annotation 直接展示源层身份：

- 显式字段: `Type.Field`
- 属性 getter: `Type.Name.get`
- 属性 setter: `Type.Name.set(T)`
- 显式方法: `Type.Method(T1,T2)`
- 类: `Type`
- 枚举: `Type`

### 8.2 为语义落地生成的存储/辅助声明必须使用 synthetic annotation

如果某个声明不是源代码中的直接成员，而是为了属性或其他语义落地生成的存储/辅助节点，则必须明确标 synthetic，不能伪装成源成员本体。

例如：

```js
/* storage for MyType.Name */
const _name_x1 = ...
```

不允许把这类节点写成：

```js
/* MyType.Name */
const _name_x1 = ...
```

因为这会误导读者，以为源里存在一个同名字段声明。

### 8.3 不直接暴露 Roslyn 内部名

annotation 不能直接暴露这类内部实现名：

- `<Name>k__BackingField`

这类名字对 inspect 的帮助很低，只会把平台内部细节泄漏到输出里。

## 9. 抽象与实现边界

### 9.1 第一阶段建议使用 `AnnotatedStatement`

第一阶段建议使用一个 statement 级包装节点，例如：

- `AnnotatedStatement`

它的职责仅为：

- 输出 annotation
- 输出原始 statement

它不负责：

- 理解 statement 内部语义
- 推导符号关系
- 参与 lowering
- 成为任意 AST 节点的通用注释容器

### 9.2 只接受模块级最终 statement

第一阶段的包装对象应限制为模块级最终 statement，而不是任意 `Node`。

这样可以避免后续误用到：

- 局部声明
- 调用表达式
- 控制流展开节点
- 类成员定义

### 9.3 不进入 `import` 原生结构内部

原生 `ImportDeclaration` 不在第一阶段注释范围内。  
只有在未来真的引入“额外本地绑定 statement”时，才考虑对该本地绑定加 annotation。

## 10. 与 Sourcemap 的关系

annotation 与 sourcemap 必须严格分层：

- annotation 面向人工阅读
- sourcemap 面向工具定位

因此：

1. sourcemap 不得依赖 annotation 存在
2. annotation 开关变化不应改变 mapping 策略
3. 不得通过增加 annotation 来弥补 sourcemap 的设计缺口

annotation 只能帮助人看懂“这是什么声明”，不能承担“源级定位协议”职责。

## 11. 配置

第一阶段只保留最小配置：

- `None`
- `Declaration`

含义：

- `None`: 不输出 annotation
- `Declaration`: 在模块级最终声明前输出 annotation

默认值建议为 `None`。

不建议在第一阶段加入：

- `Full`
- 调用点开关
- 复杂显示层级

## 12. 测试策略

annotation 不应污染现有语义测试。

建议测试策略：

1. 现有语义测试默认在 `None` 模式下运行
2. 增加少量专门的 annotation 输出测试
3. 测试重点放在插入点和文本稳定性
4. 不让大规模快照绑定 annotation 文本细节

只有这样，该能力才能保持“展示层增强”而不是“核心输出协议”。

## 13. 实施顺序

后续如果实现，按这个顺序：

1. 定义 annotation 配置
2. 实现统一 `FormatInspectableSymbol(...)`
3. 实现 `AnnotatedStatement`
4. 在模块级最终 statement 接入：
   - 方法
   - 字段/变量
   - 类
   - 枚举
5. 补少量 annotation 专项测试
6. 保持默认关闭

明确不在第一阶段实施：

1. identifier 后缀注释
2. 类成员 annotation
3. 调用点 annotation
4. sourcemap 联动

## 14. 验收标准

只有同时满足以下条件，第一阶段才算完成：

1. annotation 开关关闭时，输出与当前完全一致
2. annotation 开关开启时，只在模块级最终声明前增加注释
3. 注释删除后，行为完全等价
4. 同一符号的 annotation 文本稳定一致
5. 不污染现有核心语义测试
6. 不影响后续 sourcemap 设计边界

## 15. 一句话结论

Jazor 的 inspect annotation 应该实现为“模块级最终声明前置注释”，服务人工检查稳定生成名对应的源声明；它是一个默认关闭、范围受控、不可依赖的开发辅助能力，而不是 token 级注释系统、调试元数据系统或 sourcemap 的替代方案。
