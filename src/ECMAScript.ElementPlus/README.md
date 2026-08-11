# ECMAScript.ElementPlus

> 定位：`element-plus` 的强类型 C# host binding 与 Razor-to-Vue authoring 接口。

该包提供经过验证的 Element Plus runtime 导入、组件代理和 props/slot 契约。它只覆盖当前发布的稳定 authoring surface，不以 `object` 或弱类型选择器补齐未建模 API。

## 当前支持范围

- 根插件与 runtime host：`ElementPlus`。
- 管理壳常用组件：config provider、container layout、menu、button、card、link、space 与 divider。
- 遵循 Element Plus 命名的公开类型，例如 `ElButtonType`、`ElUploadFile` 与 `ElComponents`；根 host 保留 `ElementPlus` 名称。

## 边界

Razor Source Generator 集成、render-function lowering 与产物物化分别属于 `Jazor.Vue`、`Jazor.RazorVue` 与 `Jazor.Emit`。本包只定义 host binding 和组件契约。

## 相关文档

- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
- [Razor-to-Vue](../../docs/02-architecture/razor-to-vue.md)
