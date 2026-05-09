# ECMAScript Nullish 语义

## 目标

公共 C# 投影应该对普通用户代码隐藏 JavaScript 的 `undefined` 值。
在 C# 层，`null` 是"无值"的唯一暴露表示。

这尽可能保持 C# 和 JavaScript 之间的语义差距小，而不改变底层 JavaScript runtime 行为。

## 规则

1. 公共 C# API 不应引入 `Undefined` host 值、wrapper 类型或公共常量。
2. 当 JavaScript API 返回 `undefined` 意味着"无值"时，C# 投影通常应将其表面化为 `null`。
3. 对于可空返回的文档应该说明 JavaScript 可能产生 `undefined`，C# 投影将该 absence 映射到 `null`。
4. 内部编译器和 runtime 层可能在 JavaScript 语义需要的地方发出或测试真实 JavaScript `undefined`。

## `undefined` 必须保持内部的位置

`undefined` 在生成的 JavaScript 中仍然需要用于以下情况：

- 省略的参数必须触发 JavaScript 默认参数行为
- 丢弃或省略值的内部占位符
- JavaScript 区分 `undefined` 和省略绑定的存在检查
- 必须精确保留 JavaScript runtime 真值的桥接代码

这是实现细节，不应成为公共 C# host 概念。

## API 设计指导

- 对于"缺失"结果是 `undefined` 的 JavaScript APIs，优先使用 `T?`、`string?`、`object?` 或可空 host 对象。
- 当使用可空来吸收 JavaScript `undefined` 时，保持注释显式。
- 不要将 `undefined` 建模为与 `null` 并列的第二个公共 nullish 状态。
- 当已有兼容性表面必须保持不可空索引器时，记录该索引器镜像直接 JavaScript 属性访问，并应优先使用 `At()` 等 API 做 absence-aware 读取。

## Presence-Sensitive APIs

一些 JavaScript APIs 通过 `undefined` 编码"缺失"，但也允许可以与投影的 `null` 混淆的存储值。

示例：

- `Map.get`
- `WeakMap.get`
- 属性查找和存在检查

对于这些 API，调用者应该将值读取与显式存在检查（如 `Has`）配对。
投影应该青睐 JavaScript 的 host 形状，文档解释 C# null-projection 行为及其限制。

## 仓库策略

- 公共 ECMAScript host 映射仅暴露 `null` 作为 C# 无值表面。
- 编译器和 CLR 桥接内部可能仍使用真实 JavaScript `undefined`。
- 如果未来的 API 需要比 `null` 更强的区分能力，优先使用配套协议（如 `Has`）而不是暴露公共 `undefined` 值。
