# Module Conversion Spec

## 目录

- [1. 适用范围](#1-适用范围)
- [2. 输入契约](#2-输入契约)
- [3. 成员分类与映射](#3-成员分类与映射)
- [4. 可见性与导出规则](#4-可见性与导出规则)
- [5. 字段与属性规范](#5-字段与属性规范)
- [6. 方法输出规范](#6-方法输出规范)
- [7. 嵌套类型规范](#7-嵌套类型规范)
- [8. Import 规范](#8-import-规范)
- [9. 错误与拒绝策略](#9-错误与拒绝策略)
- [10. 模块层扩展步骤](#10-模块层扩展步骤)
- [11. 复核清单](#11-复核清单)

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
| 枚举 | 无独立 runtime 声明 | 定义仅保留在编译期；使用点在 `SemanticWalker` 常量化 |
| 接口 | 无独立 runtime 声明 | 只作为契约参与分析、投影和宿主查找 |

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

- 默认参数应转换为 JS 默认参数，或在构造函数 dispatcher 命中分支后按该 overload 自身的默认值补齐
- `ref/out` 参数目前需要与 `SemanticWalker` 的调用约定保持一致
- 普通方法重载必须在命名阶段稳定区分；当前通过 `Util.GetConfigOrSymbolName(...)` 仅在确有同名方法重载时追加稳定签名 hash
- 成员类构造函数重载不走“多个 JS 名字”路线，而走“单真实 `constructor` + `$ctor_<hash>` helper + `arguments.length` 分派”

### 6.3 未来规则

后续如要支持：

- `async`
- `generator`
- `ECMAScriptInline`

必须先把方法级契约写清楚，再调整 `FunctionDeclaration` 的生成位。

## 7. 嵌套类型规范

当前模块层允许一层成员类输入；成员枚举和成员接口则只保留编译期角色：

- 模块类自身不支持再作为嵌套模块类处理
- 成员类内部再次嵌套 `class` 当前不支持
- 嵌套类内部再出现复杂成员时，仍依赖 `AstConverter` / `SemanticWalker` 的递归一致性
- 枚举和接口都不发射模块级 runtime artifact

建议未来统一原则：

1. 模块入口类不嵌套
2. 普通成员类当前只允许嵌套一层
3. 超过一层前，先补命名、访问和测试矩阵

### 7.1 枚举路线

模块层枚举当前就应被视为“编译期值域类型声明”，而不是“值域对象声明”。

约束应是：

- 默认不输出 JS 枚举声明对象；
- 使用点把枚举值降级为底层标量常量或标量表达式；
- enum typed runtime value 统一按标量处理；
- `Flags` 只是 bitmask 值域，不引入额外 runtime 包装；
- 名字语义、`System.Enum` 家族 API、反射和格式化能力默认不保留；如需支持，必须走显式宿主缝或元数据映射；
- 对超出 JS `Number` 安全范围的底层值，必须升级到精确表示路线或显式拒绝。

这也意味着职责应逐步收敛为：

- `AstConverter` 不把 enum 当成模块级声明输出类型；
- `SemanticWalker` 负责 `E.A`、`default(E)`、比较、`switch`、位运算等使用点常量化；
- 若旧文档或历史实现仍出现 `Object.freeze({...})`，应视为已废弃的过渡态，而不是当前规范。

### 7.2 接口路线

接口一律只作为契约存在，不应发射 JS runtime artifact。

因此：

- nested interface declaration 当前直接擦除、不发射 runtime artifact；
- interface 参与 lowering 的唯一正当理由应是约束、投影或宿主查找；
- 不能把 interface 当作“以后补一个 JS 产物”的半成品。

### 7.3 继承路线

由于 JavaScript 天然支持 `class extends`，对语义足够接近的 class inheritance，模块层当前已经支持一条受控子集。

当前支持的子集是：

- 同模块成员类之间的单继承
- 模块输出必须先基类、后派生类，即使源码书写顺序相反
- 成员类支持 `extends`
- 显式 `: base(...)` 映射到 `super(...)`
- 派生类无显式构造函数时合成 `constructor() { super(); }`
- `base.Method(...)`、`base.Property`、`base.Property = value` 映射到 `super`
- `base.Method` 方法组引用通过局部 forwarder 保留“调用基类实现”的语义
- override 依赖 JS prototype dispatch

当前继续显式拒绝的路径是：

- `base.Field`
- `this(...)` 构造函数链
- 外部基类
- 需要额外协议设计的构造函数初始化器能力
- 任何仍依赖 CLR metadata identity 的继承语义

模块层不得生成丢失继承信息的 class declaration。

### 7.4 成员类构造函数重载路线

成员类构造函数重载不能照搬普通方法重载路线。  
原因不是签名 hash 不稳定，而是 JS class runtime shape 只允许一个真实 `constructor`。

当前路线固定为：

- 始终只发射一个真实 `constructor`
- 每个显式实例构造函数 body 下降为一个稳定命名的 `$ctor_<hash>` helper method
- `constructor` 内按 `arguments.length` 分派
- optional parameter 的默认值在命中分支后补齐
- 派生类每个分支各自先执行对应的 `super(...)`，再进入 helper
- dispatcher / helper 插入位置跟随第一处显式构造函数，不额外重排到 class 顶部

当前支持的 overload 集必须满足：

- 每个 overload 的可接受参数个数区间 `[requiredCount, totalCount]` 两两不重叠
- 同 arity 重载直接失败
- optional parameter 导致的区间重叠同样直接失败

当前继续显式拒绝：

- `this(...)`
- `ref/out/in/params` 参与的构造函数分派
- 需要按参数类型、命名参数或其他 CLR 规则进一步判别的 overload 集
- 外部基类上的构造函数协议模拟

## 8. Import 规范

当前主链已经接通：

- `SemanticWalker` 可通过白名单 `Op.Import` 收集导入规格
- `SenseArgument.FlushImportSpecifiers()` 负责把方法/表达式转换阶段收集到的导入分组上浮
- `AstConverter.MergeImports(...)` 负责按模块路径合并并按输出文本去重
- `AstConverter.BuildImportDeclarations()` 在模块头生成稳定排序的 `ImportDeclaration`

因此当前导入机制可以被描述为“发现 -> 收集 -> 合并 -> 模块头输出”的主链已接通，但稳定性仍需要继续靠约束与回归测试巩固。

当前约束应是：

1. 同路径导入必须合并
2. 重复 specifier 必须按输出文本去重
3. 导入顺序必须稳定，不能受访问遍历偶然性影响
4. 模块层不允许伪造与白名单/宿主规则不一致的 `import`

## 9. 错误与拒绝策略

模块层应坚持以下原则：

- 输入前提不满足 -> 立即失败
- 成员类型未知 -> 立即失败
- 复杂初始化器不会写 -> 下沉到 `SemanticWalker`
- 导入主链虽然已接通，但若白名单/宿主规则无法给出一致结果，仍应立即失败，不伪造错误的 `import`

换言之，模块层的鲁棒性来自“边界清晰”，而不是“兜底吞掉不支持成员”。

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
7. 若涉及普通方法重载，是否保持稳定签名 hash 规则
8. 若涉及构造函数重载，是否仍满足“单 `constructor` + 唯一 arity 分派”约束
