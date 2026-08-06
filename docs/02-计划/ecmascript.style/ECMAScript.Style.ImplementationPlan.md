# ECMAScript.Style 实现计划

> 状态：已实施，等待发布验收
>
> 更新：2026-07-29
>
> 目标：完成独立、强类型、确定且可跨浏览器与服务端环境运行的 C# CSS-in-JS 包

## 1. 交付范围

`ECMAScript.Style` 采用结构化 C# 模型与普通 ECMAScript 模块，不复制浏览器 CSS 解析器，也不建立编译器特例。交付范围由五个层次组成：

1. 小写静态门面 `css`，同时支持限定调用与显式静态导入；
2. 基于 Webref 语法数据、C# 原生 union 和名义值的强类型 CSS 值系统；
3. 类规则、关键帧、全局规则、现代嵌套规则与声明块 at-rule；
4. 默认、Shadow DOM 与 detached 上下文，以及提取和幂等水合；
5. 标准 Jazor catalog、source map、debug 物化、release Bundle 与独立 NuGet 消费。

正式链路为：

```text
typed values + CssRule / CssAtRule
    -> Roslyn IOperation
    -> Jazor.Compiler 通用 lowering
    -> style.mjs
    -> CssContext registry
    -> DOM / ShadowRoot / CssSnapshot
```

## 2. 不变量

以下合同在实现与发布中不得改变：

- `ECMAScript.Style` 是独立 opt-in 包，并精确依赖同版本 `Jazor`；
- 不增加 CSS 专用 Hook、RazorVue 分支、analyzer 例外或 MSBuild 属性；
- `style(...)` 与 `keyframes(...)` 返回普通 `string`；
- `ecmascript-style:v1`、类名、关键帧名、默认 StyleId 与 DOM 条目帧保持稳定；
- release 产物是包含运行时逻辑的 Bundle，不新增静态 CSS 构建管线；
- 未建模语法通过显式 `raw(...)` 进入，不以隐式字符串放宽全部属性。

## 3. 工作分解

### A. 公共 API

| 工作 | 交付物 | 验收 |
| --- | --- | --- |
| 建立唯一门面 | `ECMAScript.Style.css` | 所有公共方法和值 lower camel case |
| 支持静态导入 | `using static ECMAScript.Style.css` | `px(...)`、`style(...)` 可直接调用 |
| 统一上下文重载 | 默认与显式上下文使用同名方法 | ECMAScript 导出通过稳定名称消除重载冲突 |
| 保持模型命名 | `CssRule`、`CssContext`、`CssOptions` 等 | 类型采用标准 PascalCase |

### B. 强类型值系统

| 工作 | 交付物 | 验收 |
| --- | --- | --- |
| 锁定规范数据 | `CssProperties.Webref.json` | 来源为 `@webref/css@6.12.7`，确定排序 |
| 生成属性值域 | 705 个 `CssDeclarations` 属性 | `--check` 可复现，属性不再使用 `string?` |
| 建立名义值 | 长度、百分比、混合值、角度、时间、颜色等 | token 无公共构造函数 |
| 建立 union | 属性专用 `Css*Value` 原生 union | 分支互不继承，运行时按值擦除 |
| 提供便利 API | 单位、变量、颜色、网格、变换和数学组合 | 工厂输出合法、稳定的 CSS 文本 |
| 保留演进能力 | `raw(...)` | 新语法显式可用，普通字符串赋值失败 |
| 保持运算域准确 | `CssLengthPercentage` | 混合运算不进入纯长度属性 |

### C. 规则与确定性

| 工作 | 交付物 | 验收 |
| --- | --- | --- |
| 基础规则 | style、keyframes、global、extract | 固定哈希向量与正文稳定 |
| 有序声明 | `Additional` 与 `!important` | 保留 fallback 和级联顺序 |
| 现代嵌套 | selector、media、supports、container、layer、scope、starting-style | 递归与 sibling 顺序稳定 |
| 声明块规则 | `CssAtRule` | 支持 font-face、property、counter-style、page 等 |
| 选择器组合 | 引号、转义、括号、属性选择器感知 | selector list 笛卡尔积正确 |
| 冲突处理 | 双向名称与正文索引 | 哈希或所有权冲突明确失败 |

### D. 上下文、DOM 与水合

| 工作 | 交付物 | 验收 |
| --- | --- | --- |
| 默认上下文 | document.head 注入与 configure | 首次注册后禁止重配 |
| Shadow DOM | `CssOptions.Target` | 每个 target/StyleId 只管理一个 style 节点 |
| 隔离渲染 | `Detached=true` | 无 DOM 访问，注册表请求级隔离 |
| 快照 | `CssSnapshot` | 同时输出纯 CSS 与可接管文本 |
| HMR 与水合 | 所有权头、UTF-16 长度帧 | 重载和 hydration 不重写、不重复 |
| CSP | nonce 写入与接管校验 | DOM 模拟和真实浏览器行为一致 |

### E. 编译、物化与发布

| 工作 | 交付物 | 验收 |
| --- | --- | --- |
| 通用内联运算 | `[ECMAScriptInline]` 支持用户运算符 | `calc(...)` 无 Style 专用分支 |
| 模块目录 | `style.mjs` 与 source map catalog | 路径、hash、map file 一致 |
| RazorVue | 普通模块导入与 string 类名 | class prop 无适配层 |
| debug | 根目录物化模块与 manifest | `style.mjs` 可直接导入 |
| release | Deno/Netpack Bundle | 无未解析 Style runtime import |
| NuGet | 独立包与精确 Jazor 依赖 | 外部临时项目可 debug/release 构建 |

## 4. 测试矩阵

| 层级 | 必须覆盖的行为 |
| --- | --- |
| C# 编译 | 合法值域、跨域拒绝、字符串拒绝、`raw(...)` 接纳、静态导入 |
| 生成器 | 规范版本、705 属性、输出确定性、代表性类型映射 |
| Compiler | source-defined `[ECMAScriptInline]` 运算符、稳定 import、无专用分支 |
| Deno | 工厂值、组合值、规则正文、hash、错误传播、context、snapshot |
| DOM 模拟 | 所有权、nonce、冲突、owner document、HMR 接管 |
| 浏览器 | computed style、ShadowRoot、Unicode framing、hydration |
| RazorVue | `style.mjs` 导入、值工厂调用、普通 class 字符串 |
| Emit | catalog、source map、manifest、debug 物化 |
| Bundle | release 仅保留 bundle 与 map，运行时完整可执行 |
| NuGet | 包内容、精确依赖、无 CSS targets、公开包消费 |

## 5. 完成门槛

发布前必须同时满足：

1. 属性生成器 `--check` 通过，语法来源与 inventory 版本一致；
2. `ECMAScript.Style.Test`、Compiler 专用回归、RazorVue 与 Emit 聚焦测试全部通过；
3. 真实浏览器验证通过 document、Shadow DOM、HMR 与 hydration；
4. 本地 NuGet 消费项目在 `debug` 下物化 `style.mjs`，在 `release` 下仅生成 Bundle；
5. 完整解决方案构建和仓库测试脚本通过；
6. 包 README、根中英文 README、目标、状态和发布说明使用同一公共合同；
7. 提交、推送、打 tag 后，CI 发布成功，并从公开源完成一次全新消费验证。

## 6. 非交付项

- 原始 CSS parser、tagged template 与任意 CSS block；
- `styled(Component)` 或框架组件包装；
- CSS Typed OM 的完整对象模型；
- 构建期静态提取、独立 `.css`、PostCSS、autoprefixer 与 CSS Modules；
- 动态 statement at-rule；
- 自动引用计数、规则回收或 LRU；
- Style 专用配置、Hook 或编译器 lowering。

这些内容属于明确的产品边界，不作为兼容 fallback 或隐藏待办保留。
