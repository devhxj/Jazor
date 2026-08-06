# ECMAScript.ElementPlus 状态（2026-05-16）

> Status: 当前状态快照
> Positioning: `src/ECMAScript.ElementPlus/` 外部库绑定线的仓库级状态快照
> Scope: 元数据驱动生成、公开契约收敛、共享 Vue 类型复用、聚焦回归基线

## 总结

`ECMAScript.ElementPlus` 当前已经完成从“元数据驱动生成已落地”到“首批高风险弱面已系统收口”的阶段推进：

- 生成入口已统一为 `src/ECMAScript.Vue.Generator` 的 `elementplus` 命令；
- 生成数据源已固定为本地官方元数据：
  - `src/ECMAScript.Vue.Generator/upstream/element-plus/2.9.8/web-types.json`
  - `src/ECMAScript.Vue.Generator/upstream/element-plus/2.9.8/attributes.json`
  - `src/ECMAScript.Vue.Generator/upstream/element-plus/2.9.8/es/component.mjs`
  - 相关组件 `.d.ts`
- `ElementPlusComponentExports` / `ElementPlusComponentRegistry` / `ElementPlus.Components.generated.cs` 已由同一生成链产出；
- authoring export 与 runtime export 已支持分离映射，当前已覆盖 `ElVirtualizedSelect -> ElSelectV2`；
- canonical `modelValue` 即使在元数据遗漏 `update:modelValue` 时，也会生成成员级通用名称和标准 `*Changed` 回调；旧 `VuePropKind` / `AcceptsBinding` 元数据已退役；
- 公共 CSS / style / 常见联合值已优先复用 `ECMAScript.Vue3`，不再在 `ECMAScript.ElementPlus` 内重复制造一批近似类型；
- 截至本轮，之前列出的 **29 个 `VueValue?` / `EventCallback<VueValue?>` 弱面已全部从生成组件 authoring surface 清零**。

当前更准确的状态是：**ElementPlus 已达到“生成结果不再残留这批高风险 `VueValue?` 弱面”的标准，但整体还没有到“完全无需显式 override、所有动态函数面都已最终建模完成”的终态。**

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

本轮结束后，之前文档里那组 “仍回退到 `VueValue?` 的剩余 29 项” 已经不再成立。当前主要缺口应更新为：

1. 仍有不少官方函数面保留 `Delegate?`

- 这不再是本轮要处理的 `VueValue?` 弱面问题；
- 但仍代表一批 authoring contract 还没完全细化到显式命名 delegate；
- 当前已优先把最容易形成高风险弱面的 table-v2 getter/handler 面收紧到命名 delegate，其他面后续继续按官方 `.d.ts` 分批推进。

2. 生成器仍依赖显式 override 驱动一批关键高价值 contract

- 目前这条路线是有意选择的“高置信、可审计、避免误推断”的实现；
- 但这意味着离“绝大多数官方类型形状都能自动解析”还有距离；
- 因此当前状态可以称为“达到生产可用的第一阶段标准”，还不是“生成器自动化完成态”。

3. 少量动态内部字段仍保留 `VueValue` 作为命名动态边界

- 例如某些官方本身就是 `any` / `Record<string, any>` 的深层载荷；
- 当前策略是不再把它们直接暴露成组件 prop 的裸 `VueValue?`，而是收进命名 record / union / dictionary 合同；
- 后续只有在本地官方元数据能提供更高置信细节时，才继续细化。

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

## 2026-05-16 第三批补充完成

这一批继续收口了 `DatePicker` / `DatePickerPanel` / `TimePicker` 家族里仍然残留的弱类型回退，重点是把官方 `SingleOrRange<T>` 语义稳定落到共享 Vue authoring contract 上，而不是继续保留 `VueValue?`。

### 1. 共享 Vue 日期区间契约已正式补齐

本轮在 `ECMAScript.Vue3` 新增并公开：

- `VueDatePair`
- `VueDateSingleOrRangeValue`

这组类型与上一批的字符串区间契约保持同一设计原则：

- `VueDatePair` 精确要求两个 `Date` 项，不接受任意长度数组；
- `VueDateSingleOrRangeValue` 表达 `Date | [Date, Date]`；
- 保留集合表达式 / 数组到区间值的 authoring 入口。

### 2. 生成器已支持官方 `SingleOrRange<T>` 常见元数据形状

此前 `web-types.json` / `attributes.json` 里如果把类型写成：

- `Date | [Date, Date]`
- `string | [string, string]`

生成器会把这类 prop 退回成 `VueValue?`。

当前已补上受控解析规则：

- `Date | [Date, Date] -> VueDateSingleOrRangeValue`
- `string | [string, string] -> VueStringSingleOrRangeValue`

这不是只为 Element Plus 打点状补丁，而是把这类 Vue 生态高频 `SingleOrRange<T>` 形状收口成稳定的共享解析能力。

### 3. `string see` 这类官方文档噪音已做归一化

`web-types.json` 中存在少量文档噪音写法，例如：

- `string see`

此前这类类型会被解析器当成未知类型，再次退回 `VueValue?`。

当前已做受控归一化：

- `string see -> string`
- `number see -> number`
- `boolean see -> boolean`

这样 `DatePickerPanel` 这类组件上的 `dateFormat` / `timeFormat` / `valueFormat` 可以回到正确的标量 authoring contract。

### 4. `DatePickerPanel` 与 `TimePicker` 的官方缺口已收口

本轮已确认并收口：

- `ElDatePickerPanel.DefaultValue -> VueDateSingleOrRangeValue?`
- `ElDatePickerPanel.DefaultTime -> VueDateSingleOrRangeValue?`
- `ElDatePickerPanel.ValueFormat -> string`
- `ElDatePickerPanel.DateFormat -> string`
- `ElDatePickerPanel.TimeFormat -> string`
- `ElTimePicker.DefaultTime -> VueDateSingleOrRangeValue?`

其中 `ElTimePicker.DefaultTime` 不是现有 `web-types` 稳定暴露出来的 prop，当前已通过本地官方 `.d.ts` 补充元数据接回生成链。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests" -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `23/23` 通过。
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

## 2026-05-16 第四批继续收口

这一批继续处理两类此前仍停留在弱回退面的官方契约：

- Vue Router 已有正式宿主类型，但 Element Plus 组件面仍回退成 `VueValue?`
- 官方 `.d.ts` 已使用 `SingleOrRange<string>` / `Partial<Options>` / `Placement[]` / `StringConstructor`，但生成结果仍是 `VueValue?` / `VueValue[]?`

### 1. `RouteLocationRaw` 已进入 Element Plus 正式公开契约

本地官方路由相关 `.d.ts` 明确给出：

- `breadcrumb-item.d.ts`
  - `to?: RouteLocationRaw`
- `menu-item.d.ts`
  - `route?: RouteLocationRaw`

当前已经完成：

- 生成器将 `RouteLocationRaw` 视为正式可解析宿主类型，而不是继续回退为 `VueValue?`
- `ECMAScript.ElementPlus` 项目已显式引用 `ECMAScript.VueRoute`
- 组件公开面已收敛为：
  - `ElBreadcrumbItem.To -> RouteLocationRaw?`
  - `ElMenuItem.Route -> RouteLocationRaw?`

这一步意味着 `ElementPlus` 对 Vue Router 的 authoring surface 依赖已经从“弱值兼容”升级为“显式类型依赖”，后续不应再退回到 `VueValue?`。

### 2. `SingleOrRange<string>` 已提升为共享 Vue authoring contract

本地官方 `time-picker` / `date-picker` `.d.ts` 都把 `id` / `name` 定义为：

- `SingleOrRange<string>`

此前若直接收窄成 `string`，会丢掉区间 authoring 语义；
若退回成 `string[]`，又会放宽为任意长度数组。

当前已在 `ECMAScript.Vue3` 新增共享契约：

- `VueStringPair`
- `VueStringSingleOrRangeValue`

其中：

- `VueStringPair` 保持“精确两个字符串”的区间分支；
- `VueStringSingleOrRangeValue` 表示“单个字符串或双值字符串区间”；
- 该契约放在 `Vue3` 而不是 `ElementPlus`，避免后续其他前端库再次裂出同构专用类型。

### 3. `DatePicker` / `TimePicker` 同族公开面已同步收敛

按本地官方 `.d.ts`，当前已收敛以下 props：

- `ElTimePicker`
  - `Format -> string`
  - `ValueFormat -> string`
  - `DateFormat -> string`
  - `TimeFormat -> string`
  - `PopperOptions -> VueDictionary`
  - `FallbackPlacements -> string[]`
  - `Placement -> string`
  - `Id -> VueStringSingleOrRangeValue?`
  - `Name -> VueStringSingleOrRangeValue?`
- `ElDatePicker`
  - `Format -> string`
  - `ValueFormat -> string`
  - `DateFormat -> string`
  - `TimeFormat -> string`
  - `PopperOptions -> VueDictionary`
  - `FallbackPlacements -> string[]`
  - `Placement -> string`
  - `Id -> VueStringSingleOrRangeValue?`
  - `Name -> VueStringSingleOrRangeValue?`

此外，本轮还处理了一个官方元数据组合缺口：

- `dateFormat`
- `timeFormat`

虽然在本地官方 `.d.ts` 中存在，但原始 `web-types` / `attributes` 组合未稳定暴露。
当前已通过受控 supplemental prop 补入生成链，避免组件面继续漏出这两个官方 prop。

### 4. 最新聚焦验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `23/23` 通过。

## 2026-05-16 第五批继续收口

这一批继续处理几类“本地官方 `.d.ts` 已给出明确结构，但生成结果仍退回 `VueValue?`”的剩余高价值公开契约。

### 1. `ElCalendar` 已切回官方日期契约

本地官方 `calendar.d.ts` 明确给出：

- `modelValue?: Date`
- `range?: [Date, Date]`
- `update:modelValue` payload 为 `Date`

当前已完成：

- 生成器正式识别 `Date`
- 元组 `[Date, Date]` 正式收敛到共享 `VueDatePair`
- 组件公开面已收敛为：
  - `ElCalendar.ModelValue -> Date`
  - `ElCalendar.Range -> VueDatePair?`
  - `ElCalendar.ModelValueChanged -> EventCallback<Date>`

这一步把 `Calendar` 家族从弱 `VueValue?` / `EventCallback<VueValue?>` 收回到正式日期 authoring contract。

### 2. `ElCol` 的响应式尺寸已落成命名结构

本地官方 `col.d.ts` 明确给出：

- `ColSizeObject`
  - `span?: number`
  - `offset?: number`
  - `pull?: number`
  - `push?: number`
- `ColSize = number | ColSizeObject`

当前已在 `ECMAScript.ElementPlus` 新增并接入：

- `ElementPlusColSizeProps`
- `ElementPlusColSizeValue`

并已收敛到：

- `ElCol.Xs -> ElementPlusColSizeValue?`
- `ElCol.Sm -> ElementPlusColSizeValue?`
- `ElCol.Md -> ElementPlusColSizeValue?`
- `ElCol.Lg -> ElementPlusColSizeValue?`
- `ElCol.Xl -> ElementPlusColSizeValue?`

这避免了把“数字或四字段结构对象”继续模糊压成 `VueValue?`。

### 3. `ElTable` 的默认排序与树配置已接入命名值对象

本地官方 `table/defaults.d.ts` 明确给出：

- `Sort`
  - `prop: string`
  - `order: 'ascending' | 'descending'`
  - `init?: any`
  - `silent?: any`
- `TreeProps`
  - `hasChildren?: string`
  - `children?: string`
  - `checkStrictly?: boolean`

当前已新增并接入：

- `ElementPlusTableSortOrder`
- `ElementPlusTableSort`
- `ElementPlusTableTreeProps`

并已收敛到：

- `ElTable.DefaultSort -> ElementPlusTableSort`
- `ElTable.TreeProps -> ElementPlusTableTreeProps`

其中 `init` / `silent` 仍保留 `VueValue?`，这是因为本地官方定义本身就是 `any`，当前不额外伪造并不存在的强类型。

### 4. `ElPagination.PagerCount` 已从弱值退回数值契约

本地官方 `pagination.ts` / `pagination.d.ts` 明确给出：

- `pagerCount` 的公开 prop 类型是 `number`
- 运行时 validator 约束为奇数，且范围在 `(4, 22)` 内

当前已先把公开 authoring contract 收回到：

- `ElPagination.PagerCount -> Number?`

这一轮没有再人为引入 awkward 的奇数枚举/离散常量类型；后续如果确实需要 authoring 期精确 odd-domain 约束，再基于官方 validator 做更严格闭合集合建模。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `24/24` 通过。

## 2026-05-16 第六批继续收口

这一批继续处理“本地官方 `.d.ts` 已经给出稳定联合/对象结构，但生成公开面仍退回弱 `VueValue?` / `VueValue[]?`”的剩余高价值合同，重点收口 `ElMention` 与 `ElSpace`。

### 1. `ElMention` 已切回官方 options / prefix / popperOptions 合同

本地官方 `mention.d.ts` / `types.d.ts` 明确给出：

- `options?: MentionOption[]`
- `prefix?: string | string[]`
- `popperOptions?: Partial<Options>`
- `MentionOption`
  - `value?: string`
  - `label?: string`
  - `disabled?: boolean`
  - `[key: string]: any`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增命名值对象：
  - `ElementPlusMentionOption : VueDictionary`
- 组件公开面已收敛为：
  - `ElMention.Options -> ElementPlusMentionOption[]`
  - `ElMention.Prefix -> VueStringOrStringsValue?`
  - `ElMention.PopperOptions -> VueDictionary`

这一步把 `Mention` 家族从“选项数组 / 前缀联合 / Popper 对象都走弱值兜底”收回到正式 authoring contract。

### 2. `ElSpace` 已切回官方 alignment / spacer / size 合同

本地官方 `space.d.ts` 明确给出：

- `alignment?: string`（更精确地来自 CSS `align-items` 字符串域）
- `spacer?: string | number | VNode`
- `size?: ComponentSize | number | [number, number]`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享基础合同：
  - `VueStringOrStringsValue`
  - `VueNumberPair`
  - `VueStringNumberVNodeValue`
- 在 `ECMAScript.ElementPlus` 新增命名值类型：
  - `ElementPlusSpaceSizeValue`
- 组件公开面已收敛为：
  - `ElSpace.Alignment -> string`
  - `ElSpace.Spacer -> VueStringNumberVNodeValue?`
  - `ElSpace.Size -> ElementPlusSpaceSizeValue?`

这里没有把 `[number, number]` 退化成任意长度数组，也没有把 `VNode` 分支继续抹平成 `VueValue?`，从而保持了官方 authoring 边界。

### 3. 共享 `[number, number]` 合同已沉到 `Vue3`

本轮新增的 `VueNumberPair` 不只用于 `ElSpace.Size`。生成器现在会把明确的 `[number, number]` 官方元组收敛为共享精确 pair，而不再退回 `Number[]?`。

这已经顺带提升了本轮生成结果中的其他公开面，例如：

- `ElScrollbar.Offset -> VueNumberPair?`
- `ElTour.Gap -> VueNumberPair?`
- `ElTour.Offset -> VueNumberPair?`

这属于“共享 authoring contract 下沉到 `Vue3`”而不是继续在 `ElementPlus` 内部分裂一次性数组语义。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `27/27` 通过。

## 2026-05-16 第七批继续收口

这一批继续处理几个“官方 `.d.ts` 已经明确定义为标量或小联合，但生成公开面仍保留弱 `VueValue?`”的剩余高置信度合同。

### 1. `ElDivider.BorderStyle` 已退回正式字符串合同

本地官方 `divider.d.ts` 明确给出：

- `borderStyle?: BorderStyle`
- 其公开 prop 最终是字符串域

当前已完成：

- `ElDivider.BorderStyle -> string`

这里没有继续保留 `VueValue?`，因为本地官方 authoring 面并不存在更宽的对象或数组分支。

### 2. `ElInputTag.Delimiter` 已切回字符串/正则联合

本地官方 `input-tag.d.ts` 明确给出：

- `delimiter?: string | RegExp`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享联合：
  - `VueStringRegExpValue`
- `ElInputTag.Delimiter -> VueStringRegExpValue?`

这一步避免了把“字符串或正则”继续模糊压回 `VueValue?`，并且把该 authoring contract 下沉为可复用的 Vue 共享类型，而不是只在 `ElementPlus` 内部裂一份。

### 3. `ElWatermark.Content` 已切回字符串/字符串数组联合

本地官方 `watermark.d.ts` 明确给出：

- `content?: string | string[]`

当前已完成：

- `ElWatermark.Content -> VueStringOrStringsValue?`

这里直接复用了前一批已经下沉到 `Vue3` 的共享字符串联合，没有再在 `ElementPlus` 里分裂重复合同。

### 4. `RegExp` token 解析已补到生成器基础层

这一步不仅服务 `InputTag.Delimiter`。生成器当前已经正式识别：

- `RegExp`

后续如果官方 `.d.ts` 再出现同类“字符串或正则”公开 authoring 合同，就不需要再次靠局部临时修补。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `28/28` 通过。

## 2026-05-16 第八批收口

这一批继续处理“本地官方 `.d.ts` 已明确给出标准字符串数组或标准 Vue `StyleValue`，但生成公开面仍保留弱 `VueValue[]?` / `VueValue?`”的剩余高置信度合同。

### 1. 剩余 `fallbackPlacements` 已统一回官方 `Placement[]` 对应的 `string[]`

本地官方声明已明确给出以下公开面：

- `cascader.d.ts`：`fallbackPlacements?: Placement[]`
- `select.d.ts`：`fallbackPlacements: Placement[]`
- `tooltip`/`tree-select`/`select-v2` 相关 `.d.ts`：同样是 `Placement[]`

当前已完成：

- `ElCascader.FallbackPlacements -> string[]`
- `ElSelect.FallbackPlacements -> string[]`
- `ElTooltip.FallbackPlacements -> string[]`
- `ElTreeSelect.FallbackPlacements -> string[]`
- `ElVirtualizedSelect.FallbackPlacements -> string[]`

这里没有继续保留 `VueValue[]?`，因为这些公开 authoring 合同并不接受任意数组项，而是稳定的 Popper placement 字符串数组。

### 2. `ElScrollbar.WrapStyle/ViewStyle` 已切回共享 `VueStyleValue`

本地官方 `scrollbar.d.ts` 明确给出：

- `wrapStyle?: StyleValue`
- `viewStyle?: StyleValue`

当前已完成：

- `ElScrollbar.WrapStyle -> VueStyleValue?`
- `ElScrollbar.ViewStyle -> VueStyleValue?`

这里直接复用了已有共享 `VueStyleValue`，不再把标准 Vue `StyleValue` authoring contract 压回 `VueValue?`。

### 3. 这一步属于“统一收口”，不是局部手工修补

本轮不是直接手改生成结果，而是补齐了生成器中的显式公开面覆盖，让同类官方 placement/style 合同回到稳定生成路径：

- `el-cascader.fallbackPlacements`
- `el-select.fallbackPlacements`
- `el-tooltip.fallbackPlacements`
- `el-tree-select.fallbackPlacements`
- `el-virtualized-select.fallbackPlacements`
- `el-scrollbar.wrapStyle`
- `el-scrollbar.viewStyle`

这保证后续重新生成时不会回退。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `29/29` 通过。

## 2026-05-16 第九批收口

这一批继续处理“官方宿主合同已经明确，但生成公开面仍保留 `VueValue?` 弱兜底”的几个高置信度属性，重点是布尔可见性、宿主 DOM 入口、节流配置与上传请求头。

### 1. `ElPopover.Visible` 已切回正式布尔可见性合同

本地官方来源：

- `popover.d.ts`：`visible?: ElTooltipContentProps['visible']`
- `tooltip/src/content.d.ts`：`visible?: boolean | null`
- 对外组件 prop 最终公开面是受控布尔可见性

当前已完成：

- `ElPopover.Visible -> bool?`

这里不再保留 `VueValue?`，因为官方 authoring contract 并不是“任意值”，而是明确的布尔可见性分支。

### 2. `ElImage.ScrollContainer` 已切回字符串/HTML 元素联合

本地官方 `image.d.ts` 明确给出：

- `scrollContainer?: string | HTMLElement`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享宿主联合：
  - `VueStringHtmlElementValue`
- `ElImage.ScrollContainer -> VueStringHtmlElementValue?`

这一步把“CSS 选择器字符串或现有 DOM 元素”收回正式 authoring contract，没有继续抹平成 `VueValue?`。

### 3. `ElSkeleton.Throttle` 已切回数字/配置对象联合

本地官方 `skeleton.d.ts` 与 `use-throttle-render/index.d.ts` 明确给出：

- `throttle?: ThrottleType`
- `ThrottleType = number | { leading?: number; trailing?: number; initVal?: boolean }`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增命名配置类型：
  - `ElementPlusThrottleRenderOptions`
  - `ElementPlusThrottleValue`
- `ElSkeleton.Throttle -> ElementPlusThrottleValue?`

这里没有用 `VueValue?` 混过去，也没有退化成 `Number?`，而是保留了官方对象分支。

### 4. `ElUpload.Headers` 已切回 `Headers | Record<string, any>`

本地官方 `upload.d.ts` 明确给出：

- `headers?: Headers | Record<string, any>`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享宿主联合：
  - `VueHeadersValue`
- `ElUpload.Headers -> VueHeadersValue?`

这里直接复用了仓库已有的 `Headers` 与 `VueDictionary` 宿主面，没有再用 `VueValue?` 掩盖 Fetch Headers 与对象字典的正式边界。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `30/30` 通过。

## 2026-05-16 第十批收口

这一批继续处理“本地官方 d.ts 已经给出精确公开合同，但当前生成 authoring surface 仍保留弱兜底”的三处高置信度缺口，重点放在表格排序/过滤公开面与 slider 的 `modelValue`。

### 1. `ElTableColumn.SortOrders` 已切回正式排序顺序数组

本地官方来源：

- `es/components/table/src/table/defaults.d.ts`：`type TableSortOrder = 'ascending' | 'descending'`
- `es/components/table/src/table-column/defaults.d.ts`：`sortOrders: (TableSortOrder | null)[]`

当前已完成：

- `ElTableColumn.SortOrders -> ElementPlusTableSortOrder?[]?`

这里不再继续保留 `VueValue[]?`，因为官方 authoring contract 不是“任意数组”，而是稳定的排序顺序枚举数组，并且显式允许 `null` 参与循环切换。

### 2. `ElTableColumn.Filters` 已切回命名过滤项数组

本地官方 `table-column/defaults.d.ts` 明确给出：

- `type Filters = { text: string; value: string; }[]`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增命名类型：
  - `ElementPlusTableFilterItem`
- `ElTableColumn.Filters -> ElementPlusTableFilterItem[]`

这里没有退回 `VueValue[]?` 或 `VueDictionary[]?`，而是保留官方公开面的字段语义，避免 authoring 时丢失 `text/value` 结构边界。

### 3. `ElSlider.ModelValue` 已提升为共享 Vue 数值/数值数组联合

本地官方来源：

- `es/components/slider/src/slider.d.ts`：`modelValue: Arrayable<number>`
- `es/utils/typescript.d.ts`：`Arrayable<T> = T | T[]`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享联合：
  - `VueNumberOrNumbersValue`
- `ElSlider.ModelValue -> VueNumberOrNumbersValue?`
- `ElSlider.ModelValueChanged -> EventCallback<VueNumberOrNumbersValue?>`

这里没有把 `number | number[]` 留在 Element Plus 私有层，因为它属于跨 Vue 生态可复用的 authoring contract。这样后续其他 slider/range/value surface 也能复用同一共享类型。

### 4. 本轮仍然坚持“生成器收口”，不是手改生成产物

本轮通过显式公开面覆盖与共享类型补齐，稳定修正了以下生成路径：

- `el-table-column.sortOrders`
- `el-table-column.filters`
- `el-slider.modelValue`

因此重新生成后，属性类型与 `update:modelValue` emit payload 会同步收口，不会出现“属性精确了但 emit 元数据仍旧弱化”的半成品。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `30/30` 通过。

## 2026-05-16 第十一批收口

这一批继续处理 checkbox / dropdown / input-number 这一组“官方公开合同已经明确，但生成 surface 仍保留 `VueValue?` 弱面”的高置信属性，优先收口标量或轻量对象联合，不把复杂泛型数据结构混入同一批。

### 1. `ElCheckbox` / `ElCheckboxButton` 的 `value` / `label` 已切回布尔/字符串/数字/对象联合

本地官方来源：

- `checkbox.d.ts`：
  - `label?: string | boolean | number | object`
  - `value?: string | boolean | number | object`
- `checkbox-button.vue.d.ts` 公开面同样继承这一合同

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享联合：
  - `VueBooleanStringNumberObjectValue`
- `ElCheckbox.Value -> VueBooleanStringNumberObjectValue?`
- `ElCheckbox.Label -> VueBooleanStringNumberObjectValue?`
- `ElCheckboxButton.Value -> VueBooleanStringNumberObjectValue?`
- `ElCheckboxButton.Label -> VueBooleanStringNumberObjectValue?`

这里没有继续保留 `VueValue?`，也没有把 object 分支拆成 Element Plus 私有类型。公开 authoring contract 的重点是“允许对象值”，因此这部分被提升为共享 Vue authoring union。

### 2. `ElDropdownItem.Command` 已切回字符串/数字/对象联合

本地官方来源：

- `dropdown.d.ts`：
  - `command: ObjectConstructor | StringConstructor | NumberConstructor`

当前已完成：

- 在 `ECMAScript.Vue3` 新增共享联合：
  - `VueStringNumberObjectValue`
- `ElDropdownItem.Command -> VueStringNumberObjectValue?`

这里不再继续用 `VueValue?` 掩盖 command 的实际合同边界。

### 3. `ElCheckboxGroup.ModelValue` 已切回字符串/数字数组

本地官方来源：

- `checkbox-group.d.ts`：
  - `type CheckboxGroupValueType = Exclude<CheckboxValueType, boolean>[]`
  - `CheckboxValueType = string | number | boolean`

因此对外 authoring 合同就是：

- `modelValue?: (string | number)[]`

当前已完成：

- `ElCheckboxGroup.ModelValue -> VueStringNumberValue[]`
- `ElCheckboxGroup.ModelValueChanged -> EventCallback<VueStringNumberValue[]?>`

这里保持了“数组项只能是字符串或数字”的边界，不再退回 `VueValue?`。

### 4. `ElInputNumber.ModelValue` 已切回正式数值 model

本地官方来源：

- `input-number.d.ts`：
  - `modelValue?: number | null`
  - `update:modelValue: (val: number | undefined) => boolean`

当前已完成：

- `ElInputNumber.ModelValue -> Number?`
- `ElInputNumber.ModelValueChanged -> EventCallback<Number?>`

这里没有继续保留 `VueValue?`，因为官方公开合同并不是任意值，而是严格数值 model。

### 5. 本轮仍然是“共享类型 + 生成器覆盖”收口

本轮通过共享 Vue union 与显式覆盖，让以下公开面回到稳定生成路径：

- `el-checkbox.value`
- `el-checkbox.label`
- `el-checkbox-button.value`
- `el-checkbox-button.label`
- `el-checkbox-group.modelValue`
- `el-dropdown-item.command`
- `el-input-number.modelValue`

这样重新生成时，属性与 `update:modelValue` emit payload 会保持同步，不会出现属性与 emit 合同分裂。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `30/30` 通过。

## 2026-05-16 第十二批收口

这一批继续处理“官方 `.d.ts` 已给出稳定结构，但当前公开 authoring surface 仍回退成 `VueValue[]?`”的高置信数组面，优先选择本地元数据证据完整、不会引入宽泛 `any[]` 猜测的合同。

### 1. `TreeKey[]` 已在 `ElTree` / `ElTreeV2` / `ElTreeSelect` 上统一收口

本地官方来源：

- `tree.type.d.ts`：
  - `type TreeKey = string | number`
  - `defaultCheckedKeys?: TreeKey[]`
  - `defaultExpandedKeys?: TreeKey[]`
- `tree-v2/src/types.d.ts`：
  - `type TreeKey = string | number`
  - `defaultCheckedKeys?: TreeKey[]`
  - `defaultExpandedKeys?: TreeKey[]`
- `tree-select.vue.d.ts`：
  - `defaultCheckedKeys: PropType<TreeKey[]>`
  - `defaultExpandedKeys: PropType<TreeKey[]>`

当前已完成：

- `ElTree.DefaultExpandedKeys -> VueStringNumberValue[]`
- `ElTree.DefaultCheckedKeys -> VueStringNumberValue[]`
- `ElTreeV2.DefaultExpandedKeys -> VueStringNumberValue[]`
- `ElTreeV2.DefaultCheckedKeys -> VueStringNumberValue[]`
- `ElTreeSelect.DefaultExpandedKeys -> VueStringNumberValue[]`
- `ElTreeSelect.DefaultCheckedKeys -> VueStringNumberValue[]`

这里直接复用现有共享 Vue authoring contract，而不是继续保留 `VueValue[]?`。原因很明确：本地官方合同不是“任意数组”，而是稳定的 `string | number` key 集合。

### 2. `ElUpload.FileList` 已切回官方命名对象数组

本地官方来源：

- `upload.d.ts`：
  - `fileList?: UploadUserFile[]`
  - `type UploadUserFile = Omit<UploadFile, 'status' | 'uid'> & Partial<Pick<UploadFile, 'status' | 'uid'>>`
  - `interface UploadFile { name; percentage?; status; size?; response?; uid; url?; raw? }`
  - `interface UploadRawFile extends File { uid: number; isDirectory?: boolean }`
  - `type UploadStatus = 'ready' | 'uploading' | 'success' | 'fail'`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusUploadStatus`
  - `ElementPlusUploadRawFile`
  - `ElementPlusUploadUserFile`
- `ElUpload.FileList -> ElementPlusUploadUserFile[]`

其中当前命名合同边界为：

- `Name -> string`
- `Percentage -> Number?`
- `Status -> ElementPlusUploadStatus?`
- `Size -> Number?`
- `Response -> VueValue?`
- `Uid -> Number?`
- `Url -> string`
- `Raw -> ElementPlusUploadRawFile`

`raw` 这里没有退回 `VueValue?`，而是保留成独立 Element Plus 命名值对象，以便后续如果继续收口 upload hooks / request contract，可以在同一命名面上扩展而不重做公开 API。

### 3. 本轮仍然通过“显式覆盖 + 聚焦测试”落地

本轮生成器新增显式覆盖：

- `el-tree.defaultExpandedKeys`
- `el-tree.defaultCheckedKeys`
- `el-tree-v2.defaultExpandedKeys`
- `el-tree-v2.defaultCheckedKeys`
- `el-tree-select.defaultExpandedKeys`
- `el-tree-select.defaultCheckedKeys`
- `el-upload.fileList`

这样重新生成后，树组件的 key 数组面和 upload 文件列表面都不会再被通用数组推断错误压回 `VueValue[]?`。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `32/32` 通过。

## 2026-05-16 第十三批收口

这一批继续处理“本地官方 `.d.ts` 已明确给出字面量值域，但当前公开 authoring surface 仍保留 `string?`”的高置信属性。范围刻意控制在一组纯字面量合同上，不把 `Dropdown.Trigger` 这种还带数组化和更大兼容面的 prop 混入同一批。

### 1. `ElImage` / `ElUpload` 的 `crossorigin` 已切回官方命名值域

本地官方来源：

- `image.d.ts`：
  - `type ImageCrossorigin = 'anonymous' | 'use-credentials' | ''`
- `upload.d.ts`：
  - `type Crossorigin = 'anonymous' | 'use-credentials' | ''`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusCrossorigin`
- `ElImage.Crossorigin -> ElementPlusCrossorigin?`
- `ElUpload.Crossorigin -> ElementPlusCrossorigin?`

这里不再继续用宽泛 `string?`，因为本地官方合同已经明确只允许空值、匿名和携带凭据三种 authoring 值域。

### 2. `ElImage.Fit` / `ElImage.Loading` 已切回官方字面量域

本地官方来源：

- `image.d.ts`：
  - `type ImageFitType = '' | 'contain' | 'cover' | 'fill' | 'none' | 'scale-down'`
  - `loading?: 'eager' | 'lazy'`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusImageFitType`
  - `ElementPlusImageLoadingType`
- `ElImage.Fit -> ElementPlusImageFitType?`
- `ElImage.Loading -> ElementPlusImageLoadingType?`

这样 `fit` 与 `loading` 不会再被 authoring surface 错误放大成任意字符串。

### 3. `ElUpload.ListType` 已切回官方命名值域

本地官方来源：

- `upload.d.ts`：
  - `type ListType = 'text' | 'picture' | 'picture-card'`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusUploadListType`
- `ElUpload.ListType -> ElementPlusUploadListType?`

这里保持 Element Plus 局部命名值域，而不是回退成 `string?`，因为这不是通用 Vue 生态合同，而是 upload 列表展示方式的组件级枚举面。

### 4. `ElCarousel.Trigger` / `ElMenu.MenuTrigger` 已切回 hover-click 联合域

本地官方来源：

- `carousel.d.ts`：
  - `trigger?: 'hover' | 'click'`
- `menu.d.ts`：
  - `menuTrigger: EpPropFinalized<StringConstructor, "click" | "hover", ...>`

当前已完成：

- 在 `ECMAScript.ElementPlus` 新增并公开：
  - `ElementPlusHoverClickTrigger`
- `ElCarousel.Trigger -> ElementPlusHoverClickTrigger?`
- `ElMenu.MenuTrigger -> ElementPlusHoverClickTrigger?`

这一批没有把 `ElDropdown.Trigger` 一起收进来，原因也明确：

- `Dropdown.Trigger` 本地官方合同是 `Arrayable<'click' | 'hover' | 'contextmenu'>`
- 它不仅包含额外 `contextmenu`，还包含单值/数组双 authoring 面
- 把它混在这一批会让范围从“纯标量字面量域”扩大成“值域 + 容器形状”双问题，不利于单批稳态验证

### 5. 本轮仍然通过“显式覆盖 + 聚焦测试”落地

本轮生成器新增显式覆盖：

- `el-image.fit`
- `el-image.loading`
- `el-image.crossorigin`
- `el-upload.crossorigin`
- `el-upload.listType`
- `el-carousel.trigger`
- `el-menu.menuTrigger`

这样重生成后，这一组 prop 不会再被默认推断回 `string?`。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `32/32` 通过。

## 2026-05-16 第十四批收口

这一批单独处理上一批刻意延后的 `ElDropdown.Trigger`。原因很明确：它不是单纯的标量字面量域，而是本地官方 `.d.ts` 已明确给出的 `Arrayable<'click' | 'hover' | 'contextmenu'>`，同时包含值域和单值/数组双 authoring 形状。

### 1. `ElDropdown.Trigger` 保留公开 union 名称，但内部已切回官方命名值域

本地官方来源：

- `es/components/dropdown/src/dropdown.d.ts`
- `es/components/dropdown/src/dropdown.vue.d.ts`

官方合同明确为：

- `trigger: Arrayable<'click' | 'hover' | 'contextmenu'>`

当前已完成：

- 新增并公开：
  - `ElementPlusDropdownTriggerType`
- 保留现有公开 union 名称：
  - `ElementPlusDropdownTriggerValue`
- 但其内部合同已从弱分支：
  - `string | string[]`
- 收紧为官方命名分支：
  - `ElementPlusDropdownTriggerType | ElementPlusDropdownTriggerType[]`

这样 `ElDropdown.Trigger` 仍保持稳定的公开 prop 类型名：

- `ElDropdown.Trigger -> ElementPlusDropdownTriggerValue?`

但 authoring surface 已不再默许任意字符串或任意字符串数组，而是严格落回官方 `click / hover / contextmenu` 值域。

### 2. 这一步没有扩大生成器范围，只做了值域收紧

这一批没有再引入新的广义推断规则，也没有改动 `ElDropdown.Trigger` 的生成覆盖入口：

- `el-dropdown.trigger -> ElementPlusDropdownTriggerValue`

生成器层保持稳定，变化集中在公开 union 内部的命名分支收紧。这可以避免把 `Dropdown.Trigger` 再次误并入 tooltip/menu/carousel 那类不同语义面的 trigger 合同。

### 3. 契约测试已补足分支级守卫

本轮不仅继续守护：

- `ElDropdown.Trigger -> ElementPlusDropdownTriggerValue?`

还补充了 union 内部分支断言：

- `AsSingle -> ElementPlusDropdownTriggerType?`
- `AsMultiple -> ElementPlusDropdownTriggerType[]`

这保证后续不会出现“prop 类型名还在，但 union 内部分支又悄悄回退成 `string` / `string[]`”的漂移。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `32/32` 通过。

## 2026-05-16 第十五批收口

这一批继续处理官方本地 `.d.ts` 已经给出高置信 `Arrayable` 合同、但当前生成结果仍保留 `VueValue?` 的 `modelValue` 面。范围刻意收在一组“单值或同域数组”的公共 authoring contract 上，不把 `DatePicker` / `TimePicker` 那类更宽值域一并混进来。

### 1. 共享 Vue `Arrayable` 合同已继续沉淀到 `ECMAScript.Vue3`

本轮新增并公开：

- `VueStringNumberArrayableValue`
- `VueBooleanStringNumberObjectArrayableValue`

设计原则保持一致：

- 如果官方合同是“标量或同域数组”，优先沉淀到 `ECMAScript.Vue3`；
- `ElementPlus` 只消费共享 contract，不重复制造仅库内可见的近似 wrapper；
- 数组分支保持同域数组，不退回弱化的混合 `VueValue[]`。

### 2. `Collapse` / `Cascader` / `Select` / `TreeSelect` 的 `modelValue` 已切回共享强类型

本地官方来源：

- `es/components/collapse/src/collapse.d.ts`
  - `CollapseModelValue = Arrayable<CollapseActiveName>`
- `es/components/cascader/src/cascader.d.ts`
- `es/components/cascader-panel/src/config.d.ts`
  - `CascaderValue = string | number | Record<string, any> | ...同域数组`
- `es/components/select/src/select.d.ts`
- `es/components/tree-select/src/tree-select.vue.d.ts`
  - 标量或数组 over `boolean | string | number | object`

当前已完成：

- `ElCollapse.ModelValue -> VueStringNumberArrayableValue?`
- `ElCascader.ModelValue -> VueBooleanStringNumberObjectArrayableValue?`
- `ElCascaderPanel.ModelValue -> VueBooleanStringNumberObjectArrayableValue?`
- `ElSelect.ModelValue -> VueBooleanStringNumberObjectArrayableValue?`
- `ElTreeSelect.ModelValue -> VueBooleanStringNumberObjectArrayableValue?`

对应的：

- `ModelValueChanged`
- `VueLibraryEmit(... PayloadTypeName = ...)`

也已同步切到同一 canonical contract，不再残留 `VueValue?`。

### 3. 这一步继续通过显式覆盖落地，不扩大弱推断面

本轮生成器显式覆盖新增：

- `el-collapse.modelValue`
- `el-cascader.modelValue`
- `el-cascader-panel.modelValue`
- `el-select.modelValue`
- `el-tree-select.modelValue`

策略仍然是：

- 先补测试守卫；
- 再加高置信显式覆盖；
- 不引入更宽泛、容易误判的通用推断规则。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `32/32` 通过。

## 2026-05-16 第十六批收口

这一批继续处理 `DatePicker` / `DatePickerPanel` / `TimePicker` 仍保留 `VueValue?` 的 `modelValue`。本地官方 `.d.ts` 已经把这一组合同稳定写成：

- `string | number | Date | string[] | number[] | Date[]`

这不是“任意值”，而是一个非常明确的“标量三元域 + 同域数组”合同，因此应当落到共享 Vue authoring contract，而不是继续用 `VueValue?`。

### 1. 共享日期/时间 `modelValue` 合同已沉淀到 `ECMAScript.Vue3`

本轮新增并公开：

- `VueStringNumberDateValue`
- `VueStringNumberDateArrayableValue`

这组类型专门表达官方 `DateModelType` / `ModelValueType` 语义：

- 标量域：`string | number | Date`
- 数组域：`string[] | number[] | Date[]`

这里有意不把数组放宽成“混合元素数组”，因为本地官方合同本身就是同域数组而不是任意混合数组。

### 2. `DatePicker` / `DatePickerPanel` / `TimePicker` 已切回共享强类型 `modelValue`

本地官方来源：

- `es/components/time-picker/src/common/props.d.ts`
  - `type DateModelType = number | string | Date`
  - `type ModelValueType = DateModelType | number[] | string[] | Date[]`
- `es/components/date-picker/src/date-picker.d.ts`
- `es/components/date-picker-panel/src/date-picker-panel.d.ts`
- `es/components/time-picker/src/time-picker.d.ts`

当前已完成：

- `ElDatePicker.ModelValue -> VueStringNumberDateArrayableValue?`
- `ElDatePickerPanel.ModelValue -> VueStringNumberDateArrayableValue?`
- `ElTimePicker.ModelValue -> VueStringNumberDateArrayableValue?`

对应的：

- `ModelValueChanged`
- `VueLibraryEmit(... PayloadTypeName = ...)`

也已同步切回同一 canonical contract。

### 3. 生成链的 nullability 告警已一起清理

在补这批共享 union 后，`dotnet run --file scripts/csharp/generate-elementplus-bindings.cs` 一度重新暴露了
`VueBooleanStringNumberObjectArrayableValue` 上两条 `CS8619`。

当前已一起修正为更稳定的显式局部强类型转换写法，使：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.Vue3/ECMAScript.Vue3.csproj -v minimal`

都回到无这两条警告的状态。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.Vue3/ECMAScript.Vue3.csproj -v minimal`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `32/32` 通过。

## 2026-05-16 第十七批收口

这一批专门清理此前已经确认的 **29 个剩余 `VueValue?` / `EventCallback<VueValue?>` 弱面**。目标不是单纯把类型名换掉，而是让每一处都落到“共享 Vue 合同”或“Element Plus 命名动态合同”。

### 1. 表单规则、输入修饰符、选项值已切回明确 contract

本轮已完成：

- `ElForm.Rules -> ElementPlusFormRules`
- `ElFormItem.Prop -> VueStringOrStringsValue?`
- `ElFormItem.Rules -> ElementPlusFormItemRules?`
- `ElInput.ModelModifiers -> VueModelModifierBag?`
- `ElOption.Value -> VueBooleanStringNumberObjectValue?`

其中：

- `VueModelModifierBag` 是本轮补到 `ECMAScript.Vue3` 的共享写入侧 authoring contract；
- 它与现有 `VueModelModifiers` 的读侧抽象包分离，避免把不可构造的读侧类型误复用到组件 prop。

### 2. `Rate` / `Slider` 已从弱回退改为命名动态合同

本地官方来源：

- `es/components/rate/src/rate.d.ts`
- `es/components/slider/src/slider.d.ts`
- `es/components/slider/src/marker.d.ts`

本轮已完成：

- `ElRate.Colors -> ElementPlusRateColorsValue?`
- `ElRate.Icons -> ElementPlusRateIconsValue?`
- `ElSlider.Marks -> ElementPlusSliderMarks`

这里没有伪造官方没有给出的“固定 3 项数组强约束”或 `label` 精细类型，而是把官方仍然动态的部分收进命名 map / marker contract，保持公开面稳定且不继续裸露 `VueValue?`。

### 3. `TableV2` 全部剩余弱面已收口，并补上显式 getter/handler 合同

本地官方来源：

- `es/components/table-v2/src/table-v2.d.ts`
- `es/components/table-v2/src/table.d.ts`
- `es/components/table-v2/src/types.d.ts`
- `es/components/table-v2/src/row.d.ts`

本轮已完成：

- `HeaderClass / RowClass -> ElementPlusTableV2ClassValue?`
- `HeaderProps / HeaderCellProps / RowProps / CellProps -> ElementPlusTableV2DynamicPropsValue?`
- `HeaderHeight -> ElementPlusTableV2HeaderHeightValue?`
- `RowKey -> ElementPlusTableV2KeyValue?`
- `Columns -> ElementPlusTableV2Column[]`
- `Data / FixedData -> ElementPlusTableV2DataItem[]`
- `DataGetter -> ElementPlusTableV2DataGetter`
- `ExpandedRowKeys / DefaultExpandedRowKeys -> ElementPlusTableV2KeyValue[]`
- `SortBy -> ElementPlusTableV2SortBy`
- `SortState -> ElementPlusTableV2SortState`
- `RowEventHandlers -> ElementPlusTableV2RowEventHandlers`

这一批不仅把 prop 类型从 `VueValue?` 换成命名 union / record，还额外补了：

- `ElementPlusTableV2ClassGetter`
- `ElementPlusTableV2DynamicPropsGetter`
- `ElementPlusTableV2DataGetterContext`
- `ElementPlusTableV2RowEventHandlerContext`

这样 table-v2 的“字符串或 getter”“对象或 getter”面不再只是名义上摆脱 `VueValue?`，而是真正具备稳定的 lambda 目标类型。

### 4. `Transfer` / `VirtualizedSelect` 剩余弱面已切到命名 contract

本地官方来源：

- `es/components/transfer/src/transfer.d.ts`
- `es/components/select-v2/src/defaults.d.ts`
- `es/components/select-v2/src/select.types.d.ts`

本轮已完成：

- `ElTransfer.RenderContent -> ElementPlusTransferRenderContent`
- `ElVirtualizedSelect.ModelValue -> ElementPlusSelectV2ModelValue?`
- `ElVirtualizedSelect.Options -> ElementPlusSelectV2OptionValue[]`
- `ElVirtualizedSelect.ModelValueChanged -> EventCallback<ElementPlusSelectV2ModelValue?>`

其中：

- `Transfer.renderContent` 官方是 `(h, option) => VNode | VNode[]`；
- 当前已显式建模为 `ElementPlusTransferRenderContentResult(IVNode, IVNode[])` + `ElementPlusTransferRenderContent`；
- `SelectV2.modelValue` 官方本地元数据仍是 `any`，因此本轮没有伪造成旧 `Select` 同款标量/数组合同，而是收进命名 escape-hatch：`ElementPlusSelectV2ModelValue`。

### 5. 本轮结果

本轮完成后：

- `rg -n 'public VueValue\\?|EventCallback<VueValue\\?>' src/ECMAScript.ElementPlus/ElementPlus.Components.generated.cs`
- 结果已为空；
- 这意味着此前确认的 29 个生成组件弱面已全部清零。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.Vue3/ECMAScript.Vue3.csproj -v minimal -p:UseSharedCompilation=false`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal -p:UseSharedCompilation=false`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusAuthoringSurfaceTests|FullyQualifiedName~ElementPlusSharedContractTests' -v minimal -p:UseSharedCompilation=false`

当前聚焦 `ElementPlusAuthoringSurfaceTests` + `ElementPlusSharedContractTests` 共 `33/33` 通过。

## 2026-05-16 第十八批收口

这一批继续处理上一轮之后仍然残留的 **函数型 prop `Delegate` fallback**。目标不是局部替换生成结果，而是从公开契约、生成器 override、supplemental metadata、以及 fallback 策略四层一起收口，确保后续重新生成时不会静默回退。

### 1. 生成组件 authoring surface 已清零 `System.Delegate` fallback

本轮已把此前仍然落成 `Delegate?` 的函数型 prop 全部切回命名 callback / union / 共享 Vue contract，包括但不限于：

- `ElAutoResizer.OnResize`
- `ElAutocomplete.FetchSuggestions`
- `ElCalendar.Formatter`
- `ElCascader.FilterMethod` / `BeforeFilter`
- `ElCollapse.BeforeCollapse`
- `ElDatePicker* DisabledDate` / `CellClassName`
- `ElDialog.BeforeClose` / `ElDrawer.BeforeClose`
- `ElInput*` / `ElInputNumber*` / `ElInputOtp*`
- `ElMention.*`
- `ElProgress.Color` / `Format`
- `ElSelect*` / `ElVirtualizedSelect*`
- `ElSlider.*`
- `ElSwitch.BeforeChange`
- `ElTable.*`
- `ElTableColumn.*`
- `ElTabs.BeforeLeave`
- `ElTimePicker.*`
- `ElTransfer.FilterMethod`
- `ElTree*` / `ElTreeSelect*` / `ElTreeV2.*`
- `ElUpload.*`

结果上，生成后的 `ElementPlus.Components.generated.cs` 已不再出现组件参数级 `Delegate` / `Delegate?`。

### 2. 关键公开 callback 契约已进一步补强

本轮新增或完善的代表性 contract 包括：

- `ElementPlusCascaderBeforeFilterCallback`
- `ElementPlusTabsBeforeLeaveCallback`
- `ElementPlusTransferFilterMethod`
- `ElementPlusUploadFile`
- `ElementPlusUploadBeforeRemoveCallback`
- `ElementPlusUploadBeforeUploadCallback`

同时做了几项重要的精化：

- `Autocomplete.fetchSuggestions` 的 async 返回从宽泛 `IPromise<VueValue?>` 收紧到 `IPromise<ElementPlusAutocompleteSuggestionItem[]?>`
- `Tabs.beforeLeave` 明确收敛到 `bool | IPromise<bool?>`，并通过包装 delegate 表达官方 `Awaitable<void | boolean>` 的可取消语义
- `Upload` hooks 正式区分：
  - `UploadFile`：官方运行时文件钩子面
  - `UploadUserFile`：用户输入 `fileList` / `onExceed` 面
- `Upload.data` async factory 返回收紧到 `IPromise<ElementPlusUploadData>`
- `Upload.beforeUpload` 公开 callback 返回允许 nullable wrapper，以覆盖官方 `void | undefined | null | boolean | File | Blob`

### 3. 生成器已从“静默回退 Delegate”切到“未覆盖即失败”

本轮不仅补了显式 `ExplicitPropTypeOverrides`，还同时移除了生成器里对函数型 prop 的 `Delegate` fallback：

- 删除 `RuntimeTypeMap["Delegate"]`
- `ElAutoResizer` supplemental metadata 不再手写 `Delegate?`
- 对 `ContainsTopLevelArrow(...)` / `Function` token：
  - 不再返回 `Delegate?`
  - 改为抛出 `InvalidOperationException`

这意味着后续如果官方新增新的函数型 prop，而本地生成链还没有为其建立命名 contract，生成器会直接失败并指出：

- tag name
- runtime prop name
- 原始 type expression

从而阻止新的弱面无声进入公开 authoring surface。

### 4. 新增 focused callback contract 测试

本轮新增：

- `src/Jazor.RazorVue.Test/ElementPlusCallbackContractTests.cs`

守护点包括：

- 组件参数面不再出现 `System.Delegate`
- 代表性函数 prop 已落到预期命名 contract
- `Tabs` / `Cascader` / `Upload` / `Autocomplete` 等复杂 callback 的参数与返回形状保持稳定

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal -p:UseSharedCompilation=false`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusSharedContractTests|FullyQualifiedName~ElementPlusCallbackContractTests' -v minimal -p:UseSharedCompilation=false`

当前聚焦：

- `ElementPlusSharedContractTests`
- `ElementPlusCallbackContractTests`

共 `25/25` 通过。

## 2026-05-16 第十九批收口

这一批转向处理 **高频稳定字面量域仍然裸露为 `string?` 的 authoring surface**。目标不是把每个组件各自改成一组零散 enum，而是把 Element Plus 已经稳定复用的公开字符串域收敛为共享 contract，并把组件专属但稳定的字面量域收敛为命名 contract。

### 1. 共享字面量域已开始统一，不再让高频 prop 到处落成 `string?`

本轮新增并接入了一组共享 literal-domain contract，包括：

- `ElementPlusPopperPlacement`
- `ElementPlusPopperPlacementSide`
- `ElementPlusButtonType`
- `ElementPlusButtonNativeType`
- `ElementPlusDirection`
- `ElementPlusTagType`
- `ElementPlusTagEffect`

已切换到这些共享 contract 的代表性公开面包括：

- `ElButton.Type` / `NativeType`
- `ElButtonGroup.Type` / `Direction`
- `ElDropdown.Type` / `Placement` / `Effect`
- `ElPopover.Placement` / `Effect`
- `ElSelect.Placement` / `Effect` / `TagType` / `TagEffect`
- `ElTabs.Type` / `TabPosition`
- `ElTag.Type` / `Effect`
- `ElTooltip.Placement`
- `ElVirtualizedSelect.Effect` / `Placement` / `TagType` / `TagEffect`
- `ElTreeSelect.Effect` / `Placement` / `TagType` / `TagEffect`

同步地，相关共享 value/config contract 也已一起收紧：

- `ElementPlusButtonConfig.Type`
- `ElementPlusLinkConfig.Type`
- `ElementPlusTagTooltipProps.Placement` / `FallbackPlacements`
- `ElementPlusButtonProps.Type` / `NativeType`

### 2. 组件专属但稳定的字面量域已切回命名 contract

本轮还补了几组组件专属 literal-domain contract：

- `ElementPlusAvatarShape`
- `ElementPlusCalendarControllerType`
- `ElementPlusCollapseIconPosition`
- `ElementPlusContentPosition`
- `ElementPlusFormItemValidateStatus`
- `ElementPlusProgressType`
- `ElementPlusProgressStatus`
- `ElementPlusStepStatus`

已切换到这些 contract 的公开面包括：

- `ElAvatar.Shape`
- `ElAvatarGroup.Shape`
- `ElCalendar.ControllerType`
- `ElCollapse.ExpandIconPosition`
- `ElDivider.ContentPosition`
- `ElFormItem.ValidateStatus`
- `ElProgress.Type` / `Status`
- `ElStep.Status`
- `ElSteps.Direction` / `FinishStatus` / `ProcessStatus`

### 3. 修复过程中补了一条重要生成架构结论

这一批顺手暴露出一个容易误判的点：

- `ElTreeSelect` 不是从自己的一份独立 prop metadata 直接生成；
- 它是 supplemental component，由 `ElSelect + ElTree` 合并得到；
- 因此像 `Effect` / `Placement` / `TagType` / `TagEffect` 这类共享面，正确修复入口是先收紧 `ElSelect` 的公开 contract，再由 `ElTreeSelect` 继承；
- 仅给 `el-tree-select` 自己补 override 并不能覆盖来自源组件继承的 prop。

这个规则已经通过本轮修复路径得到验证，后续处理 supplemental component 时必须先判断 prop 的真正来源，不要只盯最终 tag。

### 4. 新增 focused literal-domain contract 测试

本轮新增：

- `src/Jazor.RazorVue.Test/ElementPlusLiteralDomainContractTests.cs`

守护点包括：

- 组件参数面的高频 `type/effect/placement/status/direction/...` 不再回退为裸 `string?`
- 共享 value/config contract 与组件参数面保持一致
- `ElTreeSelect` 这类 supplemental component 的共享字面量域确实通过源组件继承链被正确收紧

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal -p:UseSharedCompilation=false`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusSharedContractTests|FullyQualifiedName~ElementPlusCallbackContractTests|FullyQualifiedName~ElementPlusLiteralDomainContractTests' -v minimal -p:UseSharedCompilation=false`

当前聚焦：

- `ElementPlusSharedContractTests`
- `ElementPlusCallbackContractTests`
- `ElementPlusLiteralDomainContractTests`

共 `27/27` 通过。

## 2026-05-16 第二十批收口

这一批继续处理 **官方已经给出稳定 union、但 authoring surface 仍残留 `string?` 的布局/位置类字面量域**。本轮重点不是继续堆零散组件私有枚举，而是把能够稳定复用的域收成共享 contract，把确实组件专属的稳定域收成命名 contract。

### 1. `horizontal/vertical` 已继续统一到共享方向 contract

本轮确认并收紧到 `ElementPlusDirection` 的公开面包括：

- `ElCarousel.Direction`
- `ElContainer.Direction`
- `ElDescriptions.Direction`
- `ElSegmented.Direction`
- `ElSpace.Direction`

这意味着 Element Plus 内部多处复用的方向域不再各自裸露为 `string?`，后续 authoring 和反射守护都可以按一个共享 contract 收敛。

### 2. `top/bottom` 位置域已抽成共享 contract

本轮新增共享 contract：

- `ElementPlusTopBottomPlacement`

已切换到该 contract 的公开面包括：

- `ElAffix.Position`
- `ElTimelineItem.Placement`

这里没有错误复用到完整 popper placement，因为官方域只有 `top | bottom`，不是完整的 twelve-way placement。收紧时保持了原始语义边界。

### 3. 组件专属稳定域收成命名 contract，而不是继续裸 `string?`

本轮新增并接入：

- `ElementPlusCarouselType`
- `ElementPlusTimelineMode`
- `ElementPlusSemanticType`

已切换的公开面包括：

- `ElCarousel.Type`
- `ElTimeline.Mode`
- `ElText.Type`
- `ElTimelineItem.Type`

其中：

- `ElementPlusCarouselType` 对应 `'' | 'card'`
- `ElementPlusTimelineMode` 对应 `start | end | alternate | alternate-reverse`
- `ElementPlusSemanticType` 对应 `'' | primary | success | warning | info | danger`

这批没有把 `Alert/Badge/Text/TimelineItem` 所有 `type` 粗暴合并到一个过宽 contract；而是只在 **官方值域真正一致** 的面上复用 `ElementPlusSemanticType`，避免为了减少类型数而扩大可选域。

### 4. focused literal-domain 守护继续扩展

本轮继续扩展：

- `src/Jazor.RazorVue.Test/ElementPlusLiteralDomainContractTests.cs`

新增守护点覆盖：

- `ElAffix`
- `ElCarousel`
- `ElContainer`
- `ElDescriptions`
- `ElSegmented`
- `ElSpace`
- `ElText`
- `ElTimeline`
- `ElTimelineItem`

这样后续如果生成器回退到裸 `string?`，或者共享 contract 被意外拆散，focused 反射测试会直接报出具体组件与 prop。

## 本轮验证

本轮已通过：

- `dotnet run --file scripts/csharp/generate-elementplus-bindings.cs`
- `dotnet build src/ECMAScript.ElementPlus/ECMAScript.ElementPlus.csproj -v minimal -p:UseSharedCompilation=false`
- `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter 'FullyQualifiedName~ElementPlusSharedContractTests|FullyQualifiedName~ElementPlusCallbackContractTests|FullyQualifiedName~ElementPlusLiteralDomainContractTests' -v minimal -p:UseSharedCompilation=false`

当前聚焦：

- `ElementPlusSharedContractTests`
- `ElementPlusCallbackContractTests`
- `ElementPlusLiteralDomainContractTests`

共 `27/27` 通过。

## 参考

- [src/ECMAScript.ElementPlus](../../../src/ECMAScript.ElementPlus)
- [scripts/csharp/generate-elementplus-bindings.cs](../../../scripts/csharp/generate-elementplus-bindings.cs)
- [src/Jazor.RazorVue.Test/ElementPlusAuthoringSurfaceTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusAuthoringSurfaceTests.cs)
- [src/Jazor.RazorVue.Test/ElementPlusSharedContractTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusSharedContractTests.cs)
- [src/Jazor.RazorVue.Test/ElementPlusCallbackContractTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusCallbackContractTests.cs)
- [src/Jazor.RazorVue.Test/ElementPlusLiteralDomainContractTests.cs](../../../src/Jazor.RazorVue.Test/ElementPlusLiteralDomainContractTests.cs)
