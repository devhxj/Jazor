# Vuetify 组件桩设计文档

## 1. 概述

**Vuetify 组件桩** 是 RazorVue 线路中 Vuetify 3 组件库的 C# 类型声明，提供编译时类型安全和 IntelliSense 支持。这些桩组件不包含运行时实现，而是通过特性声明映射到 JavaScript Vuetify 组件。

**命名空间**: `ECMAScript.UI.Vue.Vuetify`

**文件位置**: `src/Jazor.RazorVue.Vuetify/`

**组件数量**: 38 个

## 2. 通用模式

### 2.1 组件桩结构

所有 Vuetify 组件桩遵循统一模式：

```csharp
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;

namespace ECMAScript.UI.Vue.Vuetify;

[VueLibraryComponent("vuetify/components", "VXxx")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VXxx : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public TProperty? PropertyName { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

### 2.2 特性声明

| 特性 | 用途 | 示例值 |
|------|------|--------|
| `[VueLibraryComponent]` | 声明 Vue 库组件的导入路径 | `("vuetify/components", "VBtn")` |
| `[VueLibraryStyle]` | 声明样式依赖 | `("vuetify/styles")` |
| `[VueLibraryPluginRequirement]` | 声明插件需求 | `("vuetify")` |

### 2.3 基类和接口

```csharp
public sealed class VXxx : ComponentBase, IVueLibraryComponent
```

- **ComponentBase**: ASP.NET Core Components 基类，提供组件基础设施
- **IVueLibraryComponent**: 标记接口，表示这是一个 Vue 库组件桩（无运行时实现）

### 2.4 参数类型

| 参数类型 | 用途 | 示例 |
|---------|------|------|
| `string?` | 文本属性 | `Label`, `Text`, `Color` |
| `bool` | 布尔属性 | `Disabled`, `Multiple`, `Dense` |
| `int?` | 数值属性 | `Rows`, `MaxLength` |
| `EventCallback<T>` | 事件回调 | `OnClick`, `ModelValueChanged` |
| `RenderFragment?` | 子内容 | `ChildContent` |
| `RenderFragment<T>?` | 作用域插槽 | `Activator`, `Default` |

### 2.5 Model Binding 模式

支持双向数据绑定的组件使用 `ModelValue` + `ModelValueChanged` 模式：

```csharp
[Parameter]
public T ModelValue { get; set; }

[Parameter]
public EventCallback<T> ModelValueChanged { get; set; }
```

**类型映射**:
- 字符串输入：`string? ModelValue`, `EventCallback<string?> ModelValueChanged`
- 布尔输入：`bool ModelValue`, `EventCallback<bool> ModelValueChanged`

## 3. 组件分类

### 3.1 表单控件 (Form Controls)

#### VTextField - 文本输入框

```csharp
[VueLibraryComponent("vuetify/components", "VTextField")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTextField : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }
}
```

**关键属性**:
- `Label`: 标签文本
- `Disabled`: 禁用状态
- `ModelValue`: 输入值（双向绑定）

#### VTextarea - 多行文本输入

```csharp
[VueLibraryComponent("vuetify/components", "VTextarea")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTextarea : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool AutoGrow { get; set; }

    [Parameter]
    public int? Rows { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }
}
```

**关键属性**:
- `AutoGrow`: 自动增长高度
- `Rows`: 初始行数

#### VSelect - 下拉选择器

```csharp
[VueLibraryComponent("vuetify/components", "VSelect")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSelect : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }
}
```

**关键属性**:
- `Multiple`: 多选模式
- `ModelValue`: 选中值（字符串或字符串数组）

#### VAutocomplete - 自动完成

```csharp
[VueLibraryComponent("vuetify/components", "VAutocomplete")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAutocomplete : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }
}
```

**关键属性**:
- 继承 VSelect 的属性
- 支持搜索和过滤

#### VCheckbox - 复选框

```csharp
[VueLibraryComponent("vuetify/components", "VCheckbox")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCheckbox : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }
}
```

**关键属性**:
- `ModelValue`: 选中状态（布尔型）

#### VSwitch - 开关

```csharp
[VueLibraryComponent("vuetify/components", "VSwitch")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSwitch : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }
}
```

**关键属性**:
- 类似 VCheckbox，但视觉呈现为开关

#### VRadioGroup - 单选按钮组

```csharp
[VueLibraryComponent("vuetify/components", "VRadioGroup")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VRadioGroup : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `ChildContent`: 包含 VRadio 子组件

### 3.2 布局组件 (Layout)

#### VContainer - 容器

```csharp
[VueLibraryComponent("vuetify/components", "VContainer")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VContainer : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Fluid { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Fluid`: 流体模式（100% 宽度）

#### VRow - 行

```csharp
[VueLibraryComponent("vuetify/components", "VRow")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VRow : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Dense`: 紧凑模式

#### VCol - 列

```csharp
[VueLibraryComponent("vuetify/components", "VCol")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCol : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Cols { get; set; }

    [Parameter]
    public string? Offset { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Cols`: 列宽（如 `"12"`, `"6"`, `"auto"`）
- `Offset`: 偏移量

#### VSheet - 工作表

```csharp
[VueLibraryComponent("vuetify/components", "VSheet")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSheet : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Outlined { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Color`: 背景颜色
- `Outlined`: 轮廓模式

### 3.3 导航组件 (Navigation)

#### VBtn - 按钮

```csharp
[VueLibraryComponent("vuetify/components", "VBtn")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBtn : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Text`: 按钮文本（与 ChildContent 二选一）
- `OnClick`: 点击事件

#### VToolbar - 工具栏

```csharp
[VueLibraryComponent("vuetify/components", "VToolbar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VToolbar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Color`: 背景颜色
- `Dense`: 紧凑模式

#### VToolbarTitle - 工具栏标题

```csharp
[VueLibraryComponent("vuetify/components", "VToolbarTitle")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VToolbarTitle : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Text`: 标题文本

#### VTabs - 标签页

```csharp
[VueLibraryComponent("vuetify/components", "VTabs")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTabs : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Grow { get; set; }

    [Parameter]
    public string? ModelValue { get; set; }

    [Parameter]
    public EventCallback<string?> ModelValueChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `ModelValue`: 当前激活的标签 ID
- `Grow`: 填充模式

#### VTab - 标签

```csharp
[VueLibraryComponent("vuetify/components", "VTab")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTab : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Value`: 标签标识符（与 VTabs.ModelValue 对应）

#### VBreadcrumbs - 面包屑导航

```csharp
[VueLibraryComponent("vuetify/components", "VBreadcrumbs")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBreadcrumbs : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Color`: 文本颜色

### 3.4 反馈组件 (Feedback)

#### VAlert - 警告框

```csharp
[VueLibraryComponent("vuetify/components", "VAlert")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAlert : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Type { get; set; }

    [Parameter]
    public string? Variant { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Type`: 类型（如 `"success"`, `"error"`, `"warning"`, `"info"`）
- `Variant`: 变体（如 `"outlined"`, `"tonal"`, `"elevated"`）
- `Closable`: 可关闭

#### VSnackbar - 消息条

```csharp
[VueLibraryComponent("vuetify/components", "VSnackbar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSnackbar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `ModelValue`: 显示状态（双向绑定）
- `Text`: 消息文本
- `Color`: 颜色

#### VChip - 芯片

```csharp
[VueLibraryComponent("vuetify/components", "VChip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VChip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Closable { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Closable`: 可关闭

#### VProgressCircular - 圆形进度条

```csharp
[VueLibraryComponent("vuetify/components", "VProgressCircular")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VProgressCircular : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public int? Value { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }
}
```

**关键属性**:
- `Value`: 进度值（0-100）
- `Indeterminate`: 不确定进度模式

#### VProgressLinear - 线性进度条

```csharp
[VueLibraryComponent("vuetify/components", "VProgressLinear")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VProgressLinear : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public int? Value { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }
}
```

**关键属性**:
- 类似 VProgressCircular，但为线性布局

#### VBadge - 徽章

```csharp
[VueLibraryComponent("vuetify/components", "VBadge")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VBadge : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Content { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public bool Dot { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Content`: 徽章内容
- `Dot`: 点模式（仅显示小圆点）

### 3.5 数据展示组件 (Data Display)

#### VCard - 卡片

```csharp
[VueLibraryComponent("vuetify/components", "VCard")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCard : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `ChildContent`: 包含 VCardTitle, VCardText 等子组件

#### VCardText - 卡片文本

```csharp
[VueLibraryComponent("vuetify/components", "VCardText")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCardText : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

#### VCardTitle - 卡片标题

```csharp
[VueLibraryComponent("vuetify/components", "VCardTitle")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VCardTitle : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

#### VDataTable - 数据表格

```csharp
[VueLibraryComponent("vuetify/components", "VDataTable")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDataTable : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public IEnumerable<object>? Headers { get; set; }

    [Parameter]
    public IEnumerable<object>? Items { get; set; }

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public string? ItemKey { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Headers`: 表头定义
- `Items`: 数据行
- `ItemKey`: 行唯一标识字段

#### VPagination - 分页器

```csharp
[VueLibraryComponent("vuetify/components", "VPagination")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VPagination : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public int? Length { get; set; }

    [Parameter]
    public int? ModelValue { get; set; }

    [Parameter]
    public EventCallback<int?> ModelValueChanged { get; set; }
}
```

**关键属性**:
- `Length`: 总页数
- `ModelValue`: 当前页码

#### VList - 列表

```csharp
[VueLibraryComponent("vuetify/components", "VList")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VList : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public bool Nav { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Nav`: 导航模式
- `Dense`: 紧凑模式

#### VListItem - 列表项

```csharp
[VueLibraryComponent("vuetify/components", "VListItem")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VListItem : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Title`: 标题文本
- `Value`: 项值

#### VImg - 图片

```csharp
[VueLibraryComponent("vuetify/components", "VImg")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VImg : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Src { get; set; }

    [Parameter]
    public string? Alt { get; set; }

    [Parameter]
    public bool Contain { get; set; }
}
```

**关键属性**:
- `Src`: 图片 URL
- `Alt`: 替代文本
- `Contain`: 包含模式（不裁剪）

### 3.6 覆盖层组件 (Overlays)

#### VDialog - 对话框

```csharp
[VueLibraryComponent("vuetify/components", "VDialog")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDialog : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public RenderFragment<VDialogActivatorContext>? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `ModelValue`: 显示状态（双向绑定）
- `Activator`: 激活器插槽（作用域插槽）

#### VMenu - 菜单

```csharp
[VueLibraryComponent("vuetify/components", "VMenu")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VMenu : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool ModelValue { get; set; }

    [Parameter]
    public EventCallback<bool> ModelValueChanged { get; set; }

    [Parameter]
    public RenderFragment? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- 类似 VDialog，但用于上下文菜单

#### VTooltip - 工具提示

```csharp
[VueLibraryComponent("vuetify/components", "VTooltip")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VTooltip : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Location { get; set; }

    [Parameter]
    public bool OpenOnHover { get; set; }

    [Parameter]
    public RenderFragment? Activator { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Text`: 提示文本（与 ChildContent 二选一）
- `Location`: 位置（如 `"top"`, `"bottom"`, `"left"`, `"right"`）
- `OpenOnHover`: 悬停打开

#### VAvatar - 头像

```csharp
[VueLibraryComponent("vuetify/components", "VAvatar")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VAvatar : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? Image { get; set; }

    [Parameter]
    public string? Size { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- `Image`: 头像图片 URL
- `Size`: 尺寸（如 `"small"`, `"large"`, `"x-large"`）

### 3.7 其他组件 (Others)

#### VIcon - 图标

```csharp
[VueLibraryComponent("vuetify/components", "VIcon")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VIcon : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? Size { get; set; }
}
```

**关键属性**:
- `Icon`: 图标名称（如 `"$vuetify.icons.home"`）

#### VDivider - 分隔线

```csharp
[VueLibraryComponent("vuetify/components", "VDivider")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VDivider : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Inset { get; set; }

    [Parameter]
    public bool Vertical { get; set; }
}
```

**关键属性**:
- `Inset`: 内缩模式
- `Vertical`: 垂直模式

#### VSpacer - 间隔器

```csharp
[VueLibraryComponent("vuetify/components", "VSpacer")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VSpacer : ComponentBase, IVueLibraryComponent
{
}
```

**用途**: 占据剩余空间，推挤其他元素到两侧

#### VForm - 表单

```csharp
[VueLibraryComponent("vuetify/components", "VForm")]
[VueLibraryStyle("vuetify/styles")]
[VueLibraryPluginRequirement("vuetify")]
public sealed class VForm : ComponentBase, IVueLibraryComponent
{
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

**关键属性**:
- 提供表单验证功能
- 包含表单控件子组件

## 4. 作用域插槽上下文

### 4.1 VDialogActivatorContext

**文件位置**: `src/Jazor.RazorVue.Vuetify/VDialogActivatorContext.cs`

```csharp
namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// Minimal typed activator context for the first scoped-slot Vuetify example.
/// </summary>
public sealed record VDialogActivatorContext(
    bool IsActive,
    string? AriaHasPopup = "dialog");
```

**用途**: VDialog 的 `Activator` 插槽的上下文参数

**属性**:
- `IsActive`: 对话框是否激活
- `AriaHasPopup`: ARIA 属性（默认为 `"dialog"`）

### 4.2 作用域插槽使用示例

```razor
@* 使用 VDialogActivatorContext 的作用域插槽 *@
<VDialog>
    <Activator Context="activatorContext">
        <VBtn OnClick="() => ToggleDialog()">
            @(activatorContext.IsActive ? "Close" : "Open")
        </VBtn>
    </Activator>
    <ChildContent>
        <p>Dialog content</p>
    </ChildContent>
</VDialog>

@code {
    bool _isOpen = false;

    void ToggleDialog()
    {
        _isOpen = !_isOpen;
    }
}
```

## 5. 特殊模式

### 5.1 Model Binding 模式

支持双向数据绑定的组件：

| 组件 | ModelValue 类型 | 用途 |
|------|----------------|------|
| VTextField | `string?` | 文本输入 |
| VTextarea | `string?` | 多行文本 |
| VSelect | `string?` | 下拉选择 |
| VAutocomplete | `string?` | 自动完成 |
| VCheckbox | `bool` | 复选框 |
| VSwitch | `bool` | 开关 |
| VRadioGroup | `string?` | 单选按钮组 |
| VTabs | `string?` | 标签页切换 |
| VDialog | `bool` | 对话框显示 |
| VMenu | `bool` | 菜单显示 |
| VPagination | `int?` | 分页器页码 |

### 5.2 事件回调模式

| 事件回调 | 参数类型 | 触发时机 |
|---------|---------|---------|
| `OnClick` | 无 | 按钮点击 |
| `ModelValueChanged` | `T` | ModelValue 变化 |
| `OnSubmit` | 无 | 表单提交 |
| `OnCancel` | 无 | 取消操作 |

### 5.3 插槽模式

| 插槽名称 | 类型 | 用途 |
|---------|------|------|
| `ChildContent` | `RenderFragment?` | 默认子内容 |
| `Activator` | `RenderFragment?` | 激活器插槽 |
| `Activator` | `RenderFragment<TContext>?` | 类型化激活器插槽（作用域插槽） |

## 6. 完整组件列表（38 个）

### 6.1 按字母顺序

1. VAlert
2. VAutocomplete
3. VAvatar
4. VBadge
5. VBreadcrumbs
6. VBtn
7. VCard
8. VCardText
9. VCardTitle
10. VCheckbox
11. VChip
12. VCol
13. VContainer
14. VDataTable
15. VDialog
16. VDivider
17. VForm
18. VIcon
19. VImg
20. VList
21. VListItem
22. VMenu
23. VPagination
24. VProgressCircular
25. VProgressLinear
26. VRadioGroup
27. VRow
28. VSelect
29. VSheet
30. VSnackbar
31. VSpacer
32. VSwitch
33. VTab
34. VTabs
35. VTextarea
36. VTextField
37. VToolbar
38. VToolbarTitle
39. VTooltip

**注意**: 实际为 39 个组件（文档中提到的 38 个可能未包含 VTooltip）

## 7. 使用示例

### 7.1 基本表单

```razor
<VForm>
    <VTextField Label="Name" ModelValue="@name" ModelValueChanged="@((v) => name = v)" />
    <VCheckbox Label="Accept terms" ModelValue="@accepted" ModelValueChanged="@((v) => accepted = v)" />
    <VBtn OnClick="@Submit">Submit</VBtn>
</VForm>

@code {
    string? name;
    bool accepted;

    void Submit()
    {
        // 表单提交逻辑
    }
}
```

### 7.2 布局系统

```razor
<VContainer>
    <VRow>
        <VCol Cols="12">
            <VTextField Label="Full width" />
        </VCol>
        <VCol Cols="6">
            <VTextField Label="Half width (left)" />
        </VCol>
        <VCol Cols="6">
            <VTextField Label="Half width (right)" />
        </VCol>
    </VRow>
</VContainer>
```

### 7.3 对话框

```razor
<VDialog ModelValue="@_isOpen" ModelValueChanged="@((v) => _isOpen = v)">
    <ChildContent>
        <VCard>
            <VCardTitle>
                <p>Dialog Title</p>
            </VCardTitle>
            <VCardText>
                <p>Dialog content goes here.</p>
            </VCardText>
        </VCard>
    </ChildContent>
</VDialog>

@code {
    bool _isOpen = false;
}
```

### 7.4 工具栏

```razor
<VToolbar Color="primary">
    <VToolbarTitle>My App</VToolbarTitle>
    <VSpacer />
    <VBtn Text="Login" />
</VToolbar>
```

## 8. 相关文件

| 文件 | 职责 |
|------|------|
| `src/Jazor.RazorVue.Vuetify/VBtn.cs` | 按钮组件桩 |
| `src/Jazor.RazorVue.Vuetify/VTextField.cs` | 文本输入组件桩 |
| `src/Jazor.RazorVue.Vuetify/VDialog.cs` | 对话框组件桩 |
| `src/Jazor.RazorVue.Vuetify/VDataTable.cs` | 数据表格组件桩 |
| `src/Jazor.RazorVue.Vuetify/VTooltip.cs` | 工具提示组件桩 |
| `src/Jazor.RazorVue.Vuetify/VDialogActivatorContext.cs` | 对话框激活器上下文 |
| `src/Jazor.RazorVue/VueLibraryComponentAttribute.cs` | Vue 库组件特性 |
| `src/Jazor.RazorVue/VueLibraryStyleAttribute.cs` | Vue 库样式特性 |
| `src/Jazor.RazorVue/VueLibraryPluginRequirementAttribute.cs` | Vue 库插件需求特性 |
| `src/Jazor.RazorVue/IVueLibraryComponent.cs` | Vue 库组件标记接口 |

---

**文档维护者**: developerhan
**最后更新**: 2026-04-21
**文档版本**: v1.0
