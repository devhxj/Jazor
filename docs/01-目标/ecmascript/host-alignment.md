# ECMAScript Host 对齐

本文档记录 `src/ECMAScript` 在生成的绑定之外使用的手动映射规则。

## 目标

公共 C# host 表面应尽可能接近 JavaScript runtime 形状。
当差异不可避免时，优先选择最小的 host-language escape hatch，而不是发明一个新的概念层。

实际上，这意味着：

- 优先使用 JavaScript runtime 名称和对象边界。
- 接受 C# 命名约定所需的大小写差异。
- 仅当 C# 名称解析否则会与现有类型或导入的符号冲突时，才使用尾随下划线 `_`。
- 对于 JavaScript 可迭代输入，优先使用 `IEnumerable<T>``，包括 locale 列表，如 `IEnumerable<string>`，除非 runtime 语义需要更特定的 host 形状。
- 对于基于条目的输入，如 `Object.fromEntries(...)` 或 `new Map(...)`，可以接受同时暴露 `IEnumerable<Array<object?>>` 和更广泛的 `IEnumerable<IEnumerable<object?>>` 重载，以便常见 C# 序列族仍然与 JavaScript 的 iterable-of-entry 模型对齐。
- 当 ECMA-402 API 接受 JavaScript 数学值而不仅仅是 IEEE double 输入时，优先使用显式联合，如 `Either<Number, BigInt, string>`，而不是过早地将公共表面缩小到 `Number`。
- 当 JavaScript API 允许省略前导参数但 C# 无法自然表达该省略时，优先使用直接重载，而不是强制调用者传递 CLR sentinel。一个常见情况是 Intl 构造函数允许省略 `locales` 同时仍然提供 `options`。
- 对实例方法（如 `toLocaleString(...)` 或 `localeCompare(...)`）应用相同的规则，当 JavaScript 允许省略前导 `locales` 参数但仍然提供后续选项时。
- 当 JavaScript APIs await 或采用 promise-like 输入时，在公共签名中保留类似 promise 的同化语义。

## Global Host

`ECMAScript.Global` 是 JavaScript `globalThis` 的 host 投影。

真正暴露在 `globalThis` 上的全局函数和值保持在那里，例如：

- `parseInt`
- `parseFloat`
- `isNaN`
- `isFinite`
- `queueMicrotask`
- `structuredClone`

与 C# 类型名称冲突的类构造函数全局函数使用尾随下划线：

- `Number_` -> JavaScript `Number`
- `String_` -> JavaScript `String`
- `Boolean_` -> JavaScript `Boolean`
- `BigInt_` -> JavaScript `BigInt`
- `Symbol_` -> JavaScript `Symbol`

这保持了 runtime 形状可识别，同时避免了 `global using static ECMAScript.Global` 之后的 Roslyn 歧义。

当 JavaScript 全局构造函数/函数接受任意 runtime 值时，C# 投影不应将其缩小到 CLR 特定的原始形状，除非 runtime 真的需要该更窄的形状。
例如，`Symbol_` 应该接受 `object?`，因为 JavaScript 在 runtime 将任何非 `undefined` 描述值字符串化。

## Object Host

`Object.*` 静态成员和 `Object.prototype.*` 实例成员通过以下方式投影：

- `Global.extension(object obj)`

这避免了创建额外的仅限 CLR 的 host，如 `JsObject`，这将增加 C# 和 JavaScript 之间的分裂。

`IObject` 仍然保留为 JavaScript 类对象动态属性访问的狭窄公共形状。
它有意不被纯 `object` 替换，因为 `object` 太宽泛，无法在公共 API 中传达"具有属性/索引访问的 JavaScript 对象"。

当 JavaScript 暴露遗留但真实的 `Object.prototype` 成员而 C# 可以直接拼写时，优先在原始 runtime 名称下暴露它们，而不是发明 CLR 别名。
示例包括：

- `__proto__`
- `__defineGetter__`
- `__defineSetter__`
- `__lookupGetter__`
- `__lookupSetter__`

隐藏协议桥接接口可能仍然存在于基于 JavaScript 符号的钩子，如 `@@match` 或 `@@replace`。
这些桥接是 runtime 对齐的实现细节，应该保持隐藏，除非直接暴露它们是唯一忠实的选项。

## Prototype And Inheritance

面向原型的操作仍然保持显式 JavaScript host 成员：

- `Object.GetPrototypeOf`
- `Object.SetPrototypeOf`
- `Object.Create`
- `Super(...)`

映射不会尝试将 JavaScript 原型继承重新解释为 CLR 继承。
当 JavaScript 语义是基于原型的时候，公共 API 应该直接说明这一点。

静态构造函数 hosts 可能也会直接暴露其 `prototype` 对象，当这有助于保持公共表面与 JavaScript runtime 结构对齐时。
这比强制调用者通过类似反射的 helper 层或完全省略 host 成员更可取。

## Promise-Like Inputs

当稳定的 JavaScript API 显式采用或 await promise-like 值时，C# 投影应该直接建模该形状。

- `Promise.resolve(...)` 应该通过 `IPromise` / `IPromise<T>` 重载保留 promise 同化，而不是将所有内容折叠到 `object`。
- `Array.fromAsync(...)` 应该为 promise-like 源项和异步映射回调暴露重载，因为 JavaScript await 输入项和映射器结果。
- 使用 `PromiseResult` 的仅桥接重载可能存在于 compiler-lowered 异步代码中，但它们对正常编辑器完成隐藏。

## Weak Reference Hosts

与弱引用相关的 API 遵循 JavaScript runtime 规则，而不是 CLR 引用类型规则。

- `WeakRef`、`WeakMap`、`WeakSet` 和 `FinalizationRegistry` 最终依赖于 JavaScript `CanBeHeldWeakly` 规则。
- C# 约束，如 `where T : class`，只是阻止明显非 JavaScript 形状（如值类型）的近似值。
- 最终有效性仍然属于 JavaScript runtime，包括允许非全局符号而普通 CLR 引用类型（如 `string`）仍然无效的情况。

## Nullish Policy

公共 C# 层仅暴露 `null` 作为无值表面。

- 公共 API 不将 `undefined` 建模为第二个可见状态。
- 当 JavaScript 返回 `undefined` 时，公共投影通常使用可空 C# 类型并将该 absence 映射到 `null`。
- 内部编译器/runtime 层可能仍然在语义保真度需要时使用真实 JavaScript `undefined`。
- 对于回调参数（如 `thisArg`），注释应该描述 JavaScript runtime 默认行为，而不暗示公共 C# 代码可以观察到单独的 `undefined` 值。

## Constructor Host `prototype`

- 当手写映射将具体 JavaScript 构造函数 host 建模为非泛型 C# 类型时，直接在该 host 上暴露其 `prototype` 对象。
- 使用带有 `[Description("@#prototype")]` 的 `Prototype` 成员，以便公共 API 仍然像 JavaScript runtime 一样读取，只受正常 C# 大小写规则约束。
- 不要将 `prototype` 强制到泛型 CLR 投影上，当这会错误暗示每个封闭泛型类型有单独的 runtime 构造函数时。

参见 [ECMAScript-nullish-semantics.md](./ECMAScript-nullish-semantics.md)。

## Intentional Omissions

一些 JavaScript 成员有意不投影，当 C# 无法足够忠实地表示它们时：

- `object` 上的 `Object.prototype.toString()`
  - C# 实例调度会在 `object` 上与 CLR `object.ToString()` 语义冲突。
- 可调用 `Object(...)`
  - JavaScript 可能返回 boxed wrapper 对象，其公共形状不能干净地映射到当前 C# host 模型。

在这些情况下，省略优于暴露误导性的 CLR-shaped API。
