# ECMAScript.ElementPlus 状态（2026-05-15）

> Status: 当前状态快照
> Positioning: `src/ECMAScript.ElementPlus/` 外部库绑定线的仓库级状态快照
> Scope: 元数据驱动生成、公开契约收敛、共享 Vue 类型复用、聚焦回归基线

## 总结

`ECMAScript.ElementPlus` 当前已经完成第一轮“从手工散点到元数据驱动”的收口：

- 生成入口已统一为 `scripts/csharp/generate-elementplus-bindings.cs`；
- 生成数据源已固定为本地官方元数据：
  - `.tmp/elementplus-inspect/package/web-types.json`
  - `.tmp/elementplus-inspect/package/attributes.json`
  - `.tmp/elementplus-inspect/package/es/components/index.d.ts`
- `ElementPlusComponentExports` / `ElementPlusComponentRegistry` / `ElementPlus.Components.generated.cs` 已由同一生成链产出；
- `ElOwn` 已从公开 authoring surface 过滤，不再污染官方组件导出比对；
- authoring export 与 runtime export 已支持分离映射，当前已覆盖 `ElVirtualizedSelect -> ElSelectV2`；
- canonical `modelValue` 即使在元数据遗漏 `update:modelValue` 时，也会显式生成 `VuePropKind.Model`、`AcceptsBinding = true` 和 `*Changed` 回调；
- 公共 CSS / style / 常见联合值已优先复用 `ECMAScript.Vue3`，不再在 `ECMAScript.ElementPlus` 内重复制造一批近似类型。

当前更准确的状态是：**生成链和第一批公开契约守卫已成形，但仍有一批官方命名类型在生成结果里被回退成 `VueValue?`，需要继续收敛。**

## 当前已完成

### 1. 生成链已切到官方元数据驱动

- 组件清单来自 `web-types.json` + `es/components/index.d.ts`
- 属性/事件补完来自 `attributes.json`
- runtime export baseline 由 package `index.d.ts` 守卫
- 生成结果不再依赖散点手写列表同步

### 2. 第一批高频共享类型已收敛

当前已经明确复用共享 Vue authoring contract 的面包括：

- `CssClass -> VueClassValue`
- `CssStyle -> VueStyleValue`
- `TeleportTarget -> VueTeleportTarget`
- `string | number -> VueStringNumberValue`
- `bool | string -> VueBooleanStringValue`
- `bool | string | number -> VueBooleanStringNumberValue`
- `string | component -> VueStringComponentValue`

### 3. 首批聚焦缺口已修复

已完成并有回归守护的收口包括：

- `ElVirtualizedSelect` authoring/runtime export 别名分离
- canonical `modelValue` bindable contract 自动补齐
- 公共 namespace 不再重复制造 `ElementPlusBooleanStringValue` 等弱重复类型

## 当前主要缺口

本轮复核后，公开 API 仍有一批不应存在的弱类型回退：

1. `ElConfigProvider` 与 `ElementPlusInstallOptions` 契约分叉

- `ElementPlusInstallOptions` 已有部分命名配置类型
- `ElConfigProvider` 同一组配置面仍大量回退到 `VueValue?`
- 这会造成同一官方 `ConfigProviderContext` 在组件用法和插件安装用法上出现两套 authoring contract

2. 官方命名结构类型未被消费

本地官方 `.d.ts` 已明确给出：

- `Language`
- `TranslatePair`
- `DialogConfigContext`
- `DialogTransition`
- `TableConfigContext`
- `TableOverflowTooltipOptions`

但当前生成输出里仍存在：

- `ElConfigProvider.Locale -> VueValue?`
- `ElDialog.Transition -> VueValue?`
- `ElTable.ShowOverflowTooltip -> bool?`
- `ElTable.TooltipOptions -> VueValue?`

这说明生成器还没有把官方命名结构类型稳定映射到公开 C# contract。

## 当前收敛原则

后续 `ElementPlus` 绑定继续按以下原则推进：

1. 优先消费官方命名类型

- 如果官方 `.d.ts` 已存在稳定命名类型，不应继续退回 `VueValue?`
- 例如 `Language`、`DialogConfigContext`、`TableConfigContext`

2. 共享类型优先放在 `ECMAScript.Vue3`

- 如果某个 union/value shape 是 Vue 生态高频通用面，应优先沉淀到 `ECMAScript.Vue3`
- `ElementPlus` 不应重复制造仅名字不同、语义相同的本地类型

3. 组件与插件安装面必须收敛

- `ElConfigProvider` 与 `ElementPlusInstallOptions` 应尽量复用同一组值对象
- 不允许一个是强类型，一个是 `VueValue?` 的长期分叉

4. 生成器改动必须先有测试守卫

- 先补契约断言，再改生成器和值对象
- 不接受“生成后人工看起来差不多”的弱验证方式

## 当前验证基线

上一轮聚焦验证已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当时聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `14/14` 通过。

## 本轮进行中

2026-05-15 这一批正在继续收口：

- `ConfigProviderContext` 强类型收敛
- `Language` / `TranslatePair` 正式公开 contract
- `DialogTransition` / `ElDialog.Transition` 强类型收敛
- `TableOverflowTooltipOptions` / `showOverflowTooltip` 强类型收敛
- `ElConfigProvider` 与 `ElementPlusInstallOptions` 契约统一

## 2026-05-15 本轮已完成

本轮已完成并验证通过的收口如下：

- 在 `ECMAScript.Vue3` 新增共享 `VueTransitionValue(string, VueTransitionProps)`，供 Vue 生态组件复用；
- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusTranslatePair`
  - `ElementPlusTranslateValue`
  - `ElementPlusLanguage`
  - `ElementPlusValueOnClearValue`
  - `ElementPlusTableOverflowTooltipOptions`
  - `ElementPlusTableOverflowTooltipValue`
- `ElementPlusDialogConfig.Transition` 与 `ElDialog.Transition` 已收敛到 `VueTransitionValue?`；
- `ElementPlusInstallOptions` 已收敛到官方命名类型：
  - `Locale -> ElementPlusLanguage?`
  - `ValueOnClear -> ElementPlusValueOnClearValue?`
  - `Button` / `Card` / `Dialog` / `Link` / `Message` / `Table` 继续使用命名配置对象；
- 生成器已加入基于官方 `.d.ts` 的显式 prop 类型覆盖，不再把以下面错误回退为 `VueValue?`：
  - `el-config-provider.locale`
  - `el-config-provider.button`
  - `el-config-provider.card`
  - `el-config-provider.dialog`
  - `el-config-provider.link`
  - `el-config-provider.message`
  - `el-config-provider.emptyValues`
  - `el-config-provider.valueOnClear`
  - `el-config-provider.table`
  - `el-dialog.transition`
  - `el-table.showOverflowTooltip`
  - `el-table.tooltipOptions`
  - `el-table-column.showOverflowTooltip`
- 由于官方元数据组合缺口，生成器本轮还为 `el-config-provider` 补入了官方 `.d.ts` 已存在但原始元数据未稳定暴露的 props：
  - `a11y`
  - `card`
  - `keyboardNavigation`
- `el-table.tooltipEffect` 已对齐官方 `string`，不再错误收窄为 `ElementPlusPopperEffect`。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `16/16` 通过。

## 2026-05-15 第二批补充完成

这一批补充主要修正了“官方 installable baseline 与 `web-types` 不完全重合”以及“同一官方 prop 在不同元数据来源下退回成不同弱类型”的问题。

### 1. installable component baseline 已切到 `es/component.mjs`

此前生成器把组件存在性基线放在 `web-types.json` / `es/components/index.d.ts` 上，这对 Element Plus 不够稳定：

- `web-types.json` 并不覆盖全部 installable component；
- 某些组件可以安装和导出，但不会稳定出现在 `web-types` 的 `vue-components` 数组里；
- `ElTreeSelect` 就是这一类典型缺口。

当前已改为：

- installable component baseline 以 `.tmp/elementplus-inspect/package/es/component.mjs` 为准；
- `web-types.json` / `attributes.json` 继续负责 props / emits / slots 元数据；
- 对官方 installable、但 `web-types` 缺失的组件，允许受控地从本地官方 `.d.ts` / `.mjs` / `.map` 做补充。

### 2. 已补入缺失的 installable components

本轮已通过受控补充恢复以下 installable component 的生成面：

- `ElAutoResizer`
- `ElCollapseTransition`
- `ElPopper`
- `ElTreeSelect`

这批补充不是随意手写 authoring surface，而是基于本地官方包内容组合恢复。

### 3. `UseEmptyValues` 测试基线已修正为官方真实暴露面

此前测试错误地假设更多组件公开暴露了 `EmptyValues` / `ValueOnClear`。
本轮已按官方 `.d.ts` 修正守卫：

- 明确存在：
  - `ElCascader`
  - `ElColorPicker`
  - `ElDatePicker`
  - `ElSelect`
  - `ElTimePicker`
  - `ElTimeSelect`
  - `ElTreeSelect`
  - `ElVirtualizedSelect`
- 明确不存在或只存在部分：
  - `ElAutocomplete` 不存在 `EmptyValues` / `ValueOnClear`
  - `ElInputTag` 不存在 `ValueOnClear`
  - `ElInputNumber` 保留 `ValueOnClear`

## 2026-05-15 第二批继续收口

这一轮继续补了两类此前未完全收干净的元数据缺口：

### 1. `attributes.json` 的 bracket prop 噪音已做受控归一化

本地官方 `attributes.json` 并不总是把真实 prop 名稳定写成正常 kebab-case。
当前已确认存在少量 bracket 形式：

- `[tag-tooltip]`
- `[tooltip]`
- `[props]`
- `[input props]`
- `[input events]`

此前生成器把 bracket 名统一当作文档占位项处理，导致：

- `ElVirtualizedSelect.TagTooltip` 虽然在官方元数据里存在，但会在 prop 合并和最终跳过规则中被错误丢弃；
- 后续补充组件如果继承到了同名占位 prop，还会把显式补的强类型 prop 重新覆盖掉。

当前已做两层修正：

- `NormalizePropRuntimeName` 先对 bracket 名做受控去壳，再参与 runtime name 归一化；
- prop 合并时，如果当前项是 bracket 占位名、而后续项是正常真实 prop 名，则保留真实 prop 名；
- supplemental prop 去重时，优先保留“非跳过 / 非 `VueValue?` 弱回退”的项，避免强类型补充被占位项反向覆盖。

### 2. `Trigger` 推断已收窄为显式官方覆盖

此前为了让 `ElTooltip.Trigger` / `ElPopover.Trigger` 复用官方 `TooltipTriggerType`，生成器引入了通用字面量集合推断。
这个规则过宽，造成：

- `ElCarousel.Trigger` 被错误收窄为 `ElementPlusTooltipTriggerValue?`
- `ElMenu.MenuTrigger` 也被一并卷入同类推断

当前已改为：

- 移除过宽的通用 `TooltipTrigger` 字面量推断；
- 仅保留基于 tag + runtime prop 名的显式官方覆盖：
  - `el-tooltip.trigger -> ElementPlusTooltipTriggerValue`
  - `el-popover.trigger -> ElementPlusTooltipTriggerValue`
  - `el-tooltip.triggerKeys -> string[]`
  - `el-popover.triggerKeys -> string[]`

这样可以保证：

- `ElTooltip.Trigger` / `ElPopover.Trigger` 继续使用命名强类型；
- `ElCarousel.Trigger` 恢复为保守 `string?`；
- `ElMenu.MenuTrigger` 恢复为保守 `string?`，避免误把不同语义域合并成同一 union。

### 3. `TagTooltipProps` 组件面已补齐

当前已经稳定暴露：

- `ElVirtualizedSelect.TagTooltip -> ElementPlusTagTooltipProps?`
- `ElTreeSelect.TagTooltip -> ElementPlusTagTooltipProps?`

其中：

- `ElVirtualizedSelect` 来自 `attributes.json` 的 `[tag-tooltip]` 噪音修正；
- `ElTreeSelect` 来自 supplemental component 构造链补充，并通过去重策略保证不会再被继承链中的弱项覆盖。

### 4. 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `18/18` 通过。

### 4. 第二批公开契约继续收敛

在 installable baseline 修正后，这一批继续收口了几类仍然错误退回为 `VueValue?` 的 prop：

- 在 `ECMAScript.Vue3` 新增共享 `VueBooleanNumberValue(bool, double)`；
- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusInputAutoSizeOptions`
  - `ElementPlusInputAutoSize`
- `ElCascader.FitInputWidth` 与 `ElVirtualizedSelect.FitInputWidth` 已收敛到 `VueBooleanNumberValue?`；
- `ElTooltip.TriggerKeys` 与 `ElPopover.TriggerKeys` 已收敛到 `string[]`；
- `ElInput.Autosize` 已收敛到 `ElementPlusInputAutoSize?`；
- `ElInput.Max` / `Min` / `Step` 已收敛到 `VueStringNumberValue?`；
- `ElInput.InputStyle` 已收敛到 `VueStyleValue?`。

### 5. 当前聚焦验证基线

在 installable baseline 修正后，上一轮聚焦验证已提升到：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `18/18` 通过。

## 下一批建议

下一批仍建议继续按官方元数据驱动收敛以下面：

- `ExperimentalFeatures` 是否需要从 `VueProps` 升级为更明确的命名类型；
- `TableOverflowTooltipOptions` 内部字段是否继续细化为更强的共享/命名契约：
  - `placement`
  - `popperOptions`
  - `transition`
- 继续扫描 `ConfigProviderContext` 之外的官方命名结构类型，减少 `VueValue?` / `VueDictionary?` 的剩余回退面。

## 2026-05-16 第三批继续收口

这一批继续处理“官方本地元数据已经给出命名结构，但生成结果仍回退成弱类型”的剩余面。

### 1. bracket 官方 prop 与文档分节噪音已稳定区分

本地 `attributes.json` / `web-types.json` 中的 bracket 名并不都是同一语义：

- 真实 prop：
  - `[props]`
  - `[tag-tooltip]`
- 文档分节噪音：
  - `[input props]`
  - `[input events]`
  - `[input slots]`
  - `[image viewer slots]`
  - `[tooltip]`

当前生成器已经按“真实 bracket prop”和“文档章节占位名”分流处理：

- 真实 bracket prop 先去壳，再参与 runtime prop 名归一化；
- 包含空格的 bracket 名视为文档分节噪音，从 props / events / slots 合并面过滤；
- 因此不再泄漏出：
  - `ElMention.InputSlots`
  - `ElMention.OnInputEvents`
  - `ElImage.ImageViewerSlots`

同时：

- `ElSelect.Props`
- `ElVirtualizedSelect.Props`
- `ElVirtualizedSelect.TagTooltip`

这类真实官方 bracket prop 已能稳定落到命名强类型，而不会再被统一跳过策略误伤。

### 2. `Transfer` 与选项 alias 契约继续收敛

本轮已经按本地官方 `.d.ts` / `attributes.json` 收敛以下公开 contract：

- `ElTransfer`
  - `Data -> ElementPlusTransferDataItem[]`
  - `TargetOrder -> ElementPlusTransferTargetOrder?`
  - `Titles -> ElementPlusTransferTextPair?`
  - `ButtonTexts -> ElementPlusTransferTextPair?`
  - `Format -> ElementPlusTransferFormat`
  - `Props -> ElementPlusTransferPropsAlias`
- `ElSelect` / `ElVirtualizedSelect`
  - `Props -> ElementPlusSelectPropsAlias`
- `ElSegmented`
  - `Props -> ElementPlusSegmentedPropsAlias`
- `ElTree` / `ElTreeV2`
  - `Props -> ElementPlusTreeOptionProps`
- `ElCascader` / `ElCascaderPanel`
  - `Props -> ElementPlusCascaderProps`

### 3. 新增三组官方 option props alias

继续按本地官方 `.d.ts` 收口以下组件的 `props` 配置面：

- `checkbox-group.d.ts`
  - `CheckboxOptionProps`
- `mention.d.ts`
  - `MentionOptionProps`
- `radio-group.d.ts`
  - `radioOptionProp`

当前已新增并接入：

- `ElementPlusCheckboxOptionPropsAlias`
- `ElementPlusMentionOptionPropsAlias`
- `ElementPlusRadioOptionPropsAlias`

并已收口到生成组件面：

- `ElCheckboxGroup.Props -> ElementPlusCheckboxOptionPropsAlias`
- `ElMention.Props -> ElementPlusMentionOptionPropsAlias`
- `ElRadioGroup.Props -> ElementPlusRadioOptionPropsAlias`

这一步把三处此前仍残留的 `Props: VueValue?` 官方 option 映射回退面清掉了。

### 4. 最新聚焦验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `20/20` 通过。

## 2026-05-16 第三批补充收口

在 option props alias 收口之后，这一批继续处理“官方 `.d.ts` 或仓库现有 DOM/WebIDL 类型已经存在，但生成输出仍停留在 `VueValue?` / `VueDictionary?`”的公开契约。

### 1. `ElDropdown` 已接入官方 trigger / buttonProps 契约

本地官方 `dropdown.d.ts` 明确给出：

- `trigger: Arrayable<'click' | 'hover' | 'contextmenu'>`
- `buttonProps: Partial<ButtonProps>`

当前已新增并接入：

- `ElementPlusDropdownTriggerValue`
- `ElementPlusButtonProps`

并已收口到生成组件面：

- `ElDropdown.Trigger -> ElementPlusDropdownTriggerValue?`
- `ElDropdown.ButtonProps -> ElementPlusButtonProps`

这一步消除了：

- `Trigger: VueValue?`
- `ButtonProps: VueDictionary?`

两个弱回退点。

### 2. `ElForm.ScrollIntoViewOptions` 已复用仓库现有 DOM 类型

本地官方 `form.d.ts` 明确给出：

- `scrollIntoViewOptions?: ScrollIntoViewOptions | boolean`

仓库本身已经在 `src/ECMAScript/webidl/generate/` 生成了：

- `ScrollIntoViewOptions`
- `ScrollIntoViewArg(bool, ScrollIntoViewOptions)`

当前 `ElForm.ScrollIntoViewOptions` 已直接复用现有 DOM union：

- `ElForm.ScrollIntoViewOptions -> ScrollIntoViewArg?`

因此不再停留在 `VueValue?`。

### 3. 最新聚焦验证

本轮补充已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `21/21` 通过。

## 参考

- [src/ECMAScript.ElementPlus](../../../src/ECMAScript.ElementPlus)
- [scripts/csharp/generate-elementplus-bindings.cs](../../../scripts/csharp/generate-elementplus-bindings.cs)
- [src/Jazor.RazorVue.Test/ElementPlusAuthoringSurfaceTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusAuthoringSurfaceTests.cs)
- [src/Jazor.RazorVue.Test/ElementPlusSharedContractTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusSharedContractTests.cs)
