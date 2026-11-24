# 平台与绑定

> 适用范围：ECMAScript 宿主契约、Vue 生态绑定与强类型 authoring surface。

## 宿主契约

`ECMAScript` 与 `ECMAScript.Contract` 定义可被编译器理解的 JavaScript 宿主类型和标注。公共 API 应优先以明确的 C# 参数、返回类型、重载或闭合 union 表达作者可用的值域；JavaScript 的动态性并不构成以 `object` 或无约束泛型弱化 C# 契约的理由。

泛型参数、集合元素与数组元素通常只作为擦除的类型注解。只有具体类型被实例化、执行运行时类型检查或直接访问成员时，编译器才要求它具备可用的运行时映射。

## Vue 生态

Jazor 持续维护并随 SDK 发布 Vue、Pinia、Vue Router、Pinia Testing 和 Vue Devtools 的手写 C# 绑定。Jolt 应用在 `package.json` 声明其他 npm 依赖，从锁定版本的公开 TypeScript 声明推导 C#，生成的 ESM 直接导入原包。DOM/WebIDL 和 JavaScript 内建类型属于宿主契约，`ECMAScript.Style` 提供 Jazor 自有的样式功能。

| 包 | 用途 |
| --- | --- |
| `ECMAScript.Vue` | Vue 3 核心类型、响应式 API、组件与 render-function authoring |
| `ECMAScript.VueRoute` | Vue Router 类型绑定 |
| `ECMAScript.Pinia` | Pinia 状态管理绑定 |
| `ECMAScript.Pinia.Testing` | `@pinia/testing` 绑定：测试期 Pinia root、spy 与 initial-state 的 authoring contract |
| `ECMAScript.Vue.Devtools` | Vue Devtools Plugin API 绑定：custom inspector、timeline、component hook、tab 与 command |
| `ECMAScript.Style` | 强类型、确定性的 CSS-in-JS |

Emit 依据手写绑定的包元数据把依赖写入根 `package.json`，pnpm API 恢复 `node_modules`。Pinia Testing 复用 Pinia 的实例契约，Vue Devtools 的插件描述和回调复用 Vue 应用契约。七个已发布应用级绑定的源码和测试为自动投影提供业务用例。 [npm 投影验收计划](../04-roadmap/p3-vue-application-bindings-plan.md)

## 名称与作者契约

作者声明的 C# 符号保持原有名称。JavaScript ABI 所需的名称差异通过成员级 `ECMAScriptName` 或约定的元数据显式声明，并以 metadata 解析。

Jolt 从 npm 包的公开 TypeScript 声明绑定组件参数、事件和插槽，再用 Roslyn 检查 `.jazor` 中的 C# 表达式。

## npm 值如何跨调用传递

映射算法统一使用公开声明、类型参数的绑定作用域和输入输出方向，规则覆盖组件、编辑器和普通类库。设计与实施批次见 [npm 类型投影与值传递规范](./npm-projection-rules.md)。

推导一个导出时，先由 TypeScript checker 确定声明身份、泛型实参和每个值的流向。函数实参由 C# 传给 npm，返回值由 npm 交给 C#，回调的形参和返回值再次交换方向。可写属性和 ref 同时涉及读和写。投影器据此选择已有宿主类型、可构造的输入记录、只读结果、闭合 union 或重载。Roslyn 检查具体调用，Jazor 根据绑定的符号发射 ESM。[声明推导](../../src/Jazor.Npm/metadata.mjs) · [C# 投影](../../src/Jazor.Npm/NpmValueProjection.cs) · [编译器语义](../../src/Jazor.Compiler/ImplementationPrinciples.md)

| 作者需要传递的值 | 投影和发射规则 |
| --- | --- |
| npm 返回的 DOM 实例传给另一个函数 | 从 ECMAScript 程序集的宿主标注取得 JavaScript 名和 C# 名。由 checker 匹配对应的标准 DOM 实例声明。返回的 `HTMLInputElement` 能沿已有继承关系传给 `Element` 参数，JavaScript 收到原实例。包内同名接口按自己的声明处理。 |
| 宿主数值放进混合查询键 | TS `number` 投影为 `double`。含该分支的 union 为 `ECMAScript.Number` 提供直接隐式转换，因此可以写 `QueryKey = ["page", page]`。C# 检查元素类型，JS 收到原数值。 |
| 从表单上下文读取值 | `UseFieldValue<double>("quantity").Value` 的类型是 `double?`。只读 ref 把缺失值读成 `null`，已有的零和假值保持原值。这个 API 的类型实参由调用方指定。 |
| 从部分记录中读取嵌套字段 | `PartialRecord<T>` 的属性选择器由 Roslyn 逐段绑定到数据属性。Jazor 用各段的宿主名称读取同一接收者，路径任一段缺失时得到 `null`。 |
| 用选项确定返回类型 | 插件工厂的条件返回直接索引输入泛型的布尔属性时，checker 用两个字面量分别解析调用。可区分的结果生成专用 C# 方法，两者仍指向同一个 ESM 导出。固定选项存入生成属性的程序集元数据，普通类库和组件都能发射正确的对象字面量。 |
| 将一个分页游标传给下一次请求 | 分别追踪页类型与游标类型在配置、异步回调、Vue ref 和结果数组中的位置。允许结束分页的 `null` 由声明决定。结果数组的元素取自返回声明，不能从输入游标反推 `unknown[]` 的元素类型。 |
| 省略可选参数，或明确传值 | 生成的参数携带缺省值标注，可跨程序集供 Jazor 读取。尾部省略时减少 JS 实参数量，具名后参之前的空位传 `undefined`，显式传入的 `null`、`0` 保持原值。 |
| 读取 `number \| undefined` | 公开函数、导出的常量和只读结果属性都能用 `double?` 接收。直接调用函数或把它赋给 `Func<bool, double?>` 后再调用，都只执行一次；C# 的判空能识别缺值，结果为 `0` 时保留数值。 |

复用同一声明的返回类型时还要核对实例化参数、可写性和取值域。两个同名数值枚举只要成员或数值不同，就需要独立的 C# 类型。对于共享声明的选项与结果，生成器可以增加接受结果的重载，让 `FormatDuration(IntervalToDuration(interval))` 直接传递返回的对象。[类型身份回归](../../src/Jolt.Compiler.Test/NpmValueTests.cs) · [跨程序集消费验收](../../scripts/csharp/verify-jolt-public-projection.cs)

## 用现有绑定检验自动投影

Jero 的实际消费调用决定优先验证哪些声明形状。现有七个应用级绑定用于回归。每个缺口先核对锁定版本的声明，再用改名同构夹具检验类型推导能否复用。凡涉及 Vue ref、DOM 或插件注册等宿主语义，须从声明身份和实际协议证明其行为。

手写绑定提供需要支持的业务用法，上游声明决定当前版本的参数、返回类型和可写性。VueUse 的 `isClient` 在上游是布尔常量，C# 直接读取 `VueUse.IsClient`。改变旧名称、签名或类型身份时，验收使用新的作者写法。

检验一项能力时分别记录以下结果。

1. 包的公开入口是否存在，实际导出是函数、常量还是命名空间。
2. 常用 C# 调用能否通过 Roslyn，能写入什么值，返回值保留了哪些可继续使用的成员。可选参数和泛型重载应逐个调用形式检验。
3. 发射的 ESM 是否以正确的实参、接收者和导出名调用锁定版本的包，实际返回值能否继续交给下一个 API 使用。

通用类型规则属于 Jazor 的 npm 元数据和 C# 投影层。浏览器对象、JavaScript 日期和 Vue 响应式值复用已有宿主类型。组件挂载、请求级状态、CSS 和 worker 入口由应用或专门的集成负责。普通 C# 类库在构建时根据 `using Npm…` 取得声明，公开成员携带 ESM 入口与导出名。第二个项目引用编译后的类型即可产生对应的 JavaScript 导入。[跨程序集运行验证](../../scripts/csharp/verify-jolt-public-projection.cs)

项目级 API 和示例位于各绑定项目的 README；包选择与配置见 [安装与配置](../03-guides/installation-and-configuration.md)。
