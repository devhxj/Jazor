# Jazor SourceMap 注意事项与易踩坑清单

## 1. 文档定位

本文档补充 [SourceMap.Design.md](./SourceMap.Design.md) 与 [SourceMap.ImplementationChecklist.md](../../../02-计划/compiler/SourceMap.ImplementationChecklist.md) 中没有展开的实施细节风险。

它只回答一个问题：

后续继续扩展 sourcemap 覆盖面与稳定性时，哪些点最容易做错。

## 2. 总原则

先记住三条：

1. sourcemap 不是字符串后处理问题
2. sourcemap 不能反过来污染 lowering 设计
3. synthetic 节点不能主导调试体验

后面大多数坑，本质上都违反了这三条之一。

## 3. 最容易犯的设计错误

### 3.1 过早扩张覆盖面

表现：

- 在 lowering 仍不稳定的语法域上直接承诺高精度 sourcemap
- 每修一个语法点都连带修 mapping 和结构断言

后果：

- 维护成本急剧上升
- 很难分辨 bug 是 lowering 问题还是 sourcemap 问题

规避：

- 先稳住对应 lowering 行为
- 再按既定清单扩展 sourcemap 覆盖

### 3.2 在 `SemanticWalker` 里直接拼 `mappings`

表现：

- 在 `VisitXxx(...)` 过程中直接记录 segment 或 VLQ

后果：

- `SemanticWalker` 同时承担语义 lowering 和输出编码职责
- 代码结构迅速失控
- 后续换 writer 或调整格式时会大面积返工

规避：

- `SemanticWalker` 只负责挂 `SourceOrigin`
- builder/writer 才负责生成 map

### 3.3 在 emit 阶段反推 source map

表现：

- 只拿最终 `.mjs` 字符串，靠文本位置猜测源位置

后果：

- tuple / pattern / deconstruct / with 这类 lowering 无法正确回溯
- 一旦输出格式调整，映射整体漂移

规避：

- 必须在 AST 级别保留源来源

## 4. AST 级别的典型坑

### 4.1 把所有节点都一股脑挂 origin

表现：

- 每个新节点都 `WithOrigin(...)`
- 不区分真实语义节点和 synthetic 节点

后果：

- DevTools 会频繁停在临时变量、缓存赋值、lowering glue 上

规避：

- 区分真实语义节点与 synthetic 节点
- temp 变量、缓存赋值、辅助 sequence 片段优先标 synthetic

### 4.2 外层节点覆盖了子节点更细的来源

表现：

- projection、assignment、return、expression wrapper 全都强制覆盖 `UserData`

后果：

- 细粒度来源丢失
- 调试只能回到整句，无法定位到关键子表达式

规避：

- 区分 `WithOrigin(...)` 与 `WithOriginIfMissing(...)`
- 只有根节点需要强绑定时才覆盖

### 4.3 `UserData` 被其他逻辑占用

表现：

- sourcemap 实现默认 `UserData` 一定只存 `SourceOrigin`

后果：

- 如果未来其他逻辑也往 `UserData` 放数据，容易冲突

规避：

- sourcemap 读取时明确做类型判断
- 只认 `SourceOrigin`
- 其他对象一律忽略

## 5. lowering 相关的典型坑

### 5.1 tuple swap / deconstruct 的 temp 节点污染调试

表现：

- `(a, b) = (b, a)` 单步经常停在 `v$0 = ...`

后果：

- 调试体验明显偏离源代码

规避：

- `v$0/v$1` 以及缓存赋值标记为 synthetic
- 真实赋值节点保留到左右 tuple element 的映射

### 5.2 tuple projection 只映射外层，不映射元素

表现：

- remap 后整个 projection 只有一个粗粒度来源

后果：

- 无法细分哪个生成片段对应哪个 tuple element

规避：

- 外层 projection 保留父级来源
- 内部 element value 尽量保留各自来源

### 5.3 pattern lowering 完全按 JS 结构映射

表现：

- 调试停在一堆 lowered comparison / guard 节点

后果：

- 开发者看到的是编译器内部结构，不是原 pattern

规避：

- 优先把关键映射指回原 pattern 子表达式
- lowering glue 视作 synthetic

## 6. writer / builder 层的典型坑

### 6.1 一开始就追求 token 级 mapping

表现：

- 为每个 punctuation、括号、逗号生成 mapping

后果：

- 实现复杂度暴涨
- 产出收益很低

规避：

- 第一阶段只做节点开始位置映射
- 先把主调试链路跑通

### 6.2 生成位置统计受换行风格影响

表现：

- 有的地方按 `\n` 计列
- 有的地方按 `\r\n` 计列

后果：

- 列号偏移
- 某些断点看似能跳回源码，但位置总是错一两列

规避：

- writer 内部统一换行模型
- 所有生成位置都按同一规则推进

### 6.3 重复 segment 不去重

表现：

- 同一个 generated 列反复记录多个相同映射

后果：

- map 体积膨胀
- 调试器行为可能出现不稳定

规避：

- builder 记录前先判断是否与上一条完全相同

## 7. emit 层的典型坑

### 7.1 `.mjs` hash 与 `.map` hash 混在一起

表现：

- 只维护一个总 hash

后果：

- 以后只改 map 也会导致 JS 写出策略混乱

规避：

- 明确区分 `JsHash` 与 `MapHash`

### 7.2 `sourceMappingURL` 写绝对路径

表现：

- 把本机路径直接写进 `.mjs`

后果：

- 产物不可移植
- 泄露本地路径信息

规避：

- 只写相对文件名或相对路径

### 7.3 `sources` 直接暴露本机绝对路径

表现：

- `C:\...` / `/Users/...` 直接进入 map

后果：

- 环境耦合
- 产物不干净

规避：

- 最终输出阶段统一规范化为 repo-relative 或逻辑路径

## 8. bundler 相关的典型坑

### 8.1 第一阶段就尝试做 map chaining

表现：

- 模块级 map 还没稳定，就开始处理 bundle map 合并

后果：

- 同时面对 compiler map、import rewrite map、bundle map 三层复杂度

规避：

- 第一阶段明确只做模块级 map
- bundle map 作为后续独立议题

### 8.2 把 bundle 入口文件也当真实源码映射

表现：

- `__jazor_bundle_entry__.mjs` 参与主要 mapping

后果：

- 调试栈出现用户根本没写过的入口文件

规避：

- bundle 入口视为工具产物，不进入主调试映射

## 9. 测试层的典型坑

### 9.1 只测 `.map` 文件存在，不测内容

表现：

- 测试只断言文件被写出来

后果：

- 很容易出现“有文件但映射不可用”的假通过

规避：

- 至少断言：
  - JSON 结构合法
  - `sources` 正确
  - `mappings` 非空
  - 至少一个关键节点映射有效

### 9.2 把所有测试都做成精确字符串断言

表现：

- 对整份 `mappings` 做硬编码文本比对

后果：

- writer 稍微调整就大面积脆断

规避：

- 结构性断言优先
- 只在少量关键回归测试里做精确断言

## 10. 实施时的自检问题

每完成一层后，都建议问自己这几个问题：

1. 这个改动是在补“来源信息”，还是已经越界去做“编码/落盘”了？
2. synthetic 节点会不会在 DevTools 里抢走主要断点？
3. 一个 tuple / pattern / deconstruct 场景里，调试体验更像源代码，还是更像 lowered JS？
4. 这份测试是在验证“可用调试体验”，还是只是验证“文件存在”？

## 11. 结论

Jazor 的 sourcemap 真正难的不是编码格式，而是：

1. 在 lowering 密集的编译器里保持清晰职责边界
2. 不让 synthetic 节点污染调试体验
3. 在不改变 lowering 语义的前提下，提供足够好的源级映射

只要这三点守住，source map v3 本身只是实现细节。
