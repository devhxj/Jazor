# Transformation Closure Checklist

> Status: 活跃计划
> Positioning: 编译器转换收口清单，面向当前实现 lane 的执行闭环项。
> Note: 本页是执行清单，不代表专题已经收束；阶段性进展应写入正文或状态页，而不是借 `Status` 表达完成度。

## 1. 目标

本文档把现有规范进一步落到“可执行闭环项”。判断标准不是功能多少，而是：

- 是否打通主链路
- 是否减少错误 AST 风险
- 是否降低未来语法扩展成本
- 是否能被测试与文档共同约束

## 2. 优先级原则

### P0

主链路未闭环，导致“有 AST、无产物”或“有能力、无出口”。

### P1

会直接生成错误语义，或会让复杂语法扩展继续建立在脆弱基础上。

### P2

不立即破坏主流程，但会造成维护成本、测试噪音或未来扩展阻塞。

## 3. P0 清单

### P0-1 明确 compiler catalog 与 emit 物化边界

现状：

- compiler 侧已经稳定产出 AST、模块文本和 `ModuleCatalog`
- `Jazor.Emit` 已负责 `.mjs` / `.mjs.map` 物化
- 但旧文档仍会把“compiler 产 catalog”与“emit 写文件”混写成一层

目标：

- 明确 compiler 的终点是 catalog / carriers，emit 的终点是文件物化
- 把职责边界写进文档、测试和术语中

完成标准：

- 文档、测试、生成物三者对齐
- 不再把“catalog”与“直接模块文件输出”混写

### P0-2 稳定 `Op.Import` 的模块头输出行为

现状：

- `SemanticWalker` 能收集 import specifier
- `SenseArgument` 能保存导入分组
- `AstConverter` 已能输出去重后的 `ImportDeclaration`

目标：

- 保证 import 绑定、去重、别名策略稳定
- 用专门测试锁定模块头输出形态

完成标准：

- `Op.Import` 模块头输出有稳定断言
- 重复导入、别名冲突、跨方法收集都有回归覆盖

### P0-3 明确“模块类输入契约”并在代码中强制执行

现状：

- 测试和文档默认模块类是 `public static class`
- `AstConverter` 只显式检查 `public` 和顶层

目标：

- 明确并强制模块入口必须符合统一契约

完成标准：

- 入口约束写入代码、测试、文档三处
- 错误消息明确指出违反了哪条约束

## 4. P1 清单

### P1-1 稳定 `Op.Compile` 已接入主分发后的 contract

现状：

- Generator 已生成 `Compile_*` 接口和装配字典
- `SemanticWalker` 主流程已经优先调用 `_whiteListCompiles`
- 当前 `Compile(handler, args)` contract 仍偏窄，只适合表达式级钩子

目标：

- 锁定“已接入主分发”的真实边界
- 不把需要 temp/import/source-origin 的 lowering 误塞进当前 contract

完成标准：

- `GetWhiteListExpression` 继续先尝试 `Compile`
- `Compile` 的返回语义和 fallback 语义被测试锁定
- 至少迁移 1 个真实表达式级条目
- 需要更强上下文的场景被显式留给下一阶段 contract 扩展

补充约束：

- `handler` 单独承载实例宿主
- `args` 只承载真实参数
- `throw` 不能静默 fallback
- 详细实施顺序见 [OpCompileImplementationChecklist.md](./OpCompileImplementationChecklist.md)

### P1-2 约束 `Inline` AST 模板使用边界

现状：

- `Inline` 已升级为“模板 AST + 占位符替换 + 缓存复用”
- 当前主要风险不再是实现机制，而是边界误用

目标：

- 保持 AST 模板实现稳定，并把它限制为简单纯表达式模板

完成标准：

- 旧字符串替换逻辑不再回流
- 文档中定义禁区
- 高风险条目迁移到 `Compile` 或保留 `Import`
- 有专项测试覆盖对象字面量、逗号表达式、tuple 参数

### P1-3 修正 `SemanticWalker` 中已确认的语义错位点

建议优先处理：

1. 方法引用绑定对象错误
2. 嵌套类型全名构造错误
3. `ImplicitIndexerReference` 对 `Index` 方向的误判
4. 字段命名路径未统一走配置名

完成标准：

- 对应失败测试补齐或修正
- 文档与实现重新一致

### P1-4 建立“失败路径测试”基线

现状：

- 大量测试覆盖 happy path
- 失败路径分布不均，某些拒绝语义缺少稳定断言

目标：

- 把“显式失败”纳入回归集

完成标准：

- 每个大语法域至少有一类失败路径测试
- 不支持语法必须明确抛出受控异常

## 5. P2 清单

### P2-1 统一旧文档与当前实现状态

现状：

- 部分 README / 旧版文档仍写“已完成”“全部通过”
- 与当前测试结果和源码状态不一致

目标：

- 让历史文档不再误导维护者

完成标准：

- 把“现状”与“计划”显式分开
- 对过时结论做失效标注或回写

### P2-2 为 `Optimizer` 定义主链路接入策略

现状：

- `Optimizer.cs` 存在
- 主生成链路未见稳定接入点

目标：

- 明确它是测试工具、可选后处理，还是正式产物的一部分

完成标准：

- 定义接入位置
- 定义“允许做什么优化，不允许做什么优化”

### P2-3 清理名称、访问级别、partial 模型等规则分散问题

重点包括：

- `public/internal` 导出约定统一
- `protected` 等访问级别的策略显式化
- partial 类型的 `SemanticModel` 获取策略
- backing field、重载、自动属性命名规则统一

### P2-4 建立语法域级回归清单

目标：

- 每新增一个语法点，都能快速定位必须补哪些测试和文档

建议按分域维护：

- `Pattern`
- `Reference`
- `Creation`
- `Tuple`
- `Switch`
- `TryCatch`
- `Ordinary`

## 6. 建议执行顺序

### 第一阶段：先压实主链路与稳定契约

1. compiler/emit 边界与物化契约
2. `Op.Import` 导入稳定性
3. 模块类输入契约强制化

### 第二阶段：先拆脆弱点

1. 稳定 `Op.Compile` contract
2. 收紧 `Inline`
3. 修复已确认语义错位点

### 第三阶段：再做治理

1. 失败路径测试基线
2. 文档状态统一
3. `Optimizer` 接入策略
4. 分域回归清单

## 7. 每项任务的统一验收模板

每个闭环项完成时，必须同时回答：

1. 改动落在 Analyzer、AstConverter、SemanticWalker、WhiteList、Generator 的哪一层
2. 是否影响命名规则
3. 是否影响导入或变量声明收集
4. 是否新增了失败路径
5. 是否补了测试
6. 是否补了文档

## 8. 最终判断标准

当以下条件同时成立时，才可以说“转化链路基本闭环”：

- compiler catalog 能被 emit 确定性物化成 JavaScript 文件
- 白名单的 `Alias` / `Inline` / `Import` / `Compile` 都有清晰边界
- 复杂宿主映射不再依赖脆弱字符串模板
- `AstConverter` 与 `SemanticWalker` 的职责边界稳定
- 新语法加入时有固定落地流程，而不是靠临时特判

补充说明：

- `Import` 的“是否能输出模块头”在当前实现里已经不是待打通问题，而是待稳定问题
- enum/interface 擦除、成员类继承子集、成员类构造函数 dispatcher 这些能力当前已经进入现状契约，不应再在闭环清单里被表述成“尚未开始”

---

**建议用法**

- 方案设计阶段：先读 `SyntaxTransformationPipeline.md`
- 改模块层代码时：对照 `ModuleConversionSpec.md`
- 改 `SemanticWalker` 时：对照 `WalkerExtensionSpec.md`
- 排实现优先级时：以本文档为执行顺序基线
