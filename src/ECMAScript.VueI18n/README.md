# ECMAScript.VueI18n

vue-i18n 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游多包版本，
`manifest.json`（schema 2）和 `inventory.json` 记录 npm package、入口、完整性与许可证元数据。

Strongly typed C# bindings for vue-i18n, shipped as a Jazor JS resource library with locked upstream
package versions. Runtime files remain in npm; metadata describes the dependency graph for the
generated `jazor` project.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `vue-i18n` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 闭包包 | `vue-i18n`、`@intlify/core-base`、`@intlify/shared`、`@intlify/message-compiler` |
| 许可证 | MIT（`licenses/*-LICENSE`） |
| peer 依赖 | `vue` 与 `@vue/devtools-api`（均由 `ECMAScript.Vue` 资源库提供） |

各包版本分别锁定并记入 `inventory.json`；`manifest.json` 的 `version` 是作者入口包 `vue-i18n` 的版本。

## 首期范围 First slice

- `createI18n`：创建 i18n 根实例（全局 Composer、`dispose`、作为 Vue 插件安装）
- `useI18n`：组件内 Composer，支持 `global`/`local`/`parent` 作用域
- 消息查找：`t`（4 个重载：纯 key、复数、命名插值、完整选项）、`rt`、`te`、`tm`
- 格式化：`d`（`Date`/`Number`）、`n`
- 资源管理：`getLocaleMessage`/`setLocaleMessage`/`mergeLocaleMessage`，以及日期时间/数字格式的 get/set/merge

未绑定：`Translation`、`NumberFormat`、`DatetimeFormat`、`I18nT`、`I18nN`、`I18nD` 组件与 `vTDirective` 指令；`d`/`n` 只接受格式名或格式 key 字符串，具名格式对象（`d(value, { key: 'short' })`）不在首期内。

Not bound: the `Translation`, `NumberFormat`, `DatetimeFormat`, `I18nT`, `I18nN`, and `I18nD` components and the `vTDirective` directive; `d`/`n` accept a format name or key string only, and named format objects are outside the first slice.

## SSR 边界 SSR boundary

`createI18n` 可以在服务端创建并用于 SSR 渲染；`useI18n` 与 Composer 的读写依赖组件实例，只能在组件 setup 作用域内调用。本包只负责 Vue 侧的 locale/message runtime，不取代 ASP.NET 的 request culture。

## 使用示例 Authoring

```csharp
using static ECMAScript.VueI18n;

var i18n = CreateI18n(new VueI18nCreateOptions
{
    Legacy = false,
    Locale = "en-US",
    FallbackLocale = "en-US",
    Messages = new Vue.VueDictionary
    {
        ["en-US"] = new Vue.VueDictionary { ["hello"] = "Hello {name}" },
        ["zh-CN"] = new Vue.VueDictionary { ["hello"] = "你好 {name}" }
    }
});

// i18n 作为 Vue 插件安装；组件内：
var composer = UseI18n();
var text = composer.T("hello", new Vue.VueDictionary { ["name"] = "Jazor" });
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-vue-i18n.cs -- --version <version>
dotnet run --file scripts/csharp/generate-vue-i18n.cs -- --source .tmp/p3b/node_modules
```

## 测试 Tests

`src/ECMAScript.VueI18n.Test` 覆盖 import/manifest/inventory 元数据、npm integrity、上游导出 drift 与编译器 emission；`Jazor.EmitTest` 覆盖真实 package graph materialization。
