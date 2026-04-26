# RazorVue 组件描述符规范

## 1. 目的

本文档定义 RazorVue 使用的组件契约模型。

其目的是修复：

1. RazorVue 组件如何向编译器描述自己
2. 如何验证组件调用站点
3. 如何发现插槽、发出和可绑定通道
4. 内置组件和库组件如何适应同一契约系统

本文档有意比 `RazorVue.Design.md` 更窄。
它只关注组件契约形状和解析行为。

相关文档：

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](../../../02-计划/jolt/razorvue-implementation/RazorVue.ImplementationChecklist.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)

## 2. 设计目标

RazorVue 不得在降低模板时临时推断组件契约。

RazorVue 管道使用的每个组件必须具有显式描述符形状，至少可以回答：

- 此组件称为什么
- 从哪里导入
- 接受什么 props
- 暴露什么 emits
- 支持什么插槽
- 是否支持模型风格绑定
- 是否携带样式/运行时提示

## 3. 描述符范围

描述符模型适用于三种组件：

1. 用户组件
2. Vue 内置组件
3. 库组件

示例：

- 用户组件：`Counter`
- 内置：`Teleport`
- 库组件：`VBtn`

编译器应该通过一个描述符契约而不是单独的临时路径来消费所有三种组件。

## 4. 顶级描述符模型

推荐结构：

```csharp
public sealed record VueComponentDescriptor(
    string Name,
    string FullName,
    VueComponentSourceKind SourceKind,
    string ResolutionNamespace,
    string ImportSpecifier,
    string ExportName,
    ImmutableArray<VuePropDescriptor> Props,
    ImmutableArray<VueEmitDescriptor> Emits,
    ImmutableArray<VueSlotDescriptor> Slots,
    ImmutableArray<string> StyleDependencies,
    VueComponentFlags Flags);
```

### 4.1 `Name`

用于模板解析的短组件名称。

示例：

- `Counter`
- `MyDialog`
- `VBtn`
- `Teleport`

### 4.2 `FullName`

用于诊断和跨程序集查找的稳定完整标识。

示例：

- `Demo.Components.Counter`
- `ECMAScript.Vue.Components.Teleport`
- `ECMAScript.Vue.Vuetify.VBtn`

### 4.3 `SourceKind`

推荐枚举：

```csharp
public enum VueComponentSourceKind
{
    UserComponent,
    Intrinsic,
    LibraryComponent
}
```

### 4.4 `ImportSpecifier`

将此组件降低为 Vue 输出时使用的 ESM 导入源。

示例：

- `./Counter.mjs`
- `vue`
- `vuetify/components`

### 4.5 `ResolutionNamespace`

组件通过其对 RazorVue 组件解析可见的命名空间。

示例：

- 用户组件：`Demo.Components`
- 内置：`ECMAScript.UI.Vue`
- 库组件：`ECMAScript.UI.Vue.Vuetify`

此字段的存在是为了支持 `using` 驱动的组件可见性，而无需在每个 Razor 组件上引入额外的目标属性。

### 4.6 `ExportName`

从模块使用的导出符号名称。

第一阶段推荐：

- 用户组件默认为 `default`
- 内置/库组件使用其运行时导出名称

## 5. Prop 描述符

推荐结构：

```csharp
public sealed record VuePropDescriptor(
    string Name,
    string PublicName,
    string TypeName,
    bool Required,
    bool AcceptsBinding,
    string? DefaultExpression,
    VuePropKind Kind);
```

推荐枚举：

```csharp
public enum VuePropKind
{
    Normal,
    Model,
    HtmlLike,
    LibrarySpecific
}
```

### 5.1 `Name`

发送到最终组件调用的运行时 Vue prop 名称。

示例：

- `title`
- `visible`
- `modelValue`

### 5.2 `PublicName`

向组件创作者暴露的 Razor/C# 表面名称。

示例：

- `Title`
- `Visible`
- `Value`

### 5.3 `AcceptsBinding`

指示 prop 参与 `@bind-*` 降低。

### 5.4 `DefaultExpression`

在可用时存储组件侧默认元数据。

描述符应将此保留为契约元数据。
实际运行时实现仍可能在 setup 或 prop 选项中发生。

## 6. Emit 描述符

推荐结构：

```csharp
public sealed record VueEmitDescriptor(
    string Name,
    string PayloadTypeName,
    string? RazorAlias,
    VueEmitKind Kind);
```

推荐枚举：

```csharp
public enum VueEmitKind
{
    Normal,
    ModelUpdate,
    LifecycleLike,
    LibrarySpecific
}
```

### 6.1 `Name`

运行时 Vue emit 名称。

示例：

- `save`
- `close`
- `update:modelValue`
- `update:visible`

### 6.2 `RazorAlias`

可选的 Razor/C# 面向糖别名。

示例：

- `OnSave`
- `OnClose`
- `ValueChanged`

### 6.3 `PayloadTypeName`

提取时最佳已知有效负载类型名称。

第一阶段不需要为每个显式 `Emit("...")` 路径进行完美推断，
但该字段必须存在。

## 7. 插槽描述符

推荐结构：

```csharp
public sealed record VueSlotDescriptor(
    string Name,
    bool IsDefault,
    ImmutableArray<VueSlotParameterDescriptor> Parameters,
    bool Required);
```

```csharp
public sealed record VueSlotParameterDescriptor(
    string Name,
    string TypeName);
```

### 7.1 默认插槽

`ChildContent` 降低为：

- `Name = "default"`
- `IsDefault = true`

### 7.2 命名插槽

命名 `RenderFragment` 降低为：

- `Name = lowerCamelCase(parameterName)`

示例：

- `Header` -> `header`

### 7.3 作用域插槽

`RenderFragment<T>` 降低为：

- 命名插槽
- 一个或多个插槽参数描述符

第一阶段只需要支持模板子集所需的最小作用域插槽参数模型。

## 8. 组件标志

推荐枚举：

```csharp
[Flags]
public enum VueComponentFlags
{
    None = 0,
    SupportsModelValue = 1,
    SupportsMultipleModels = 2,
    RequiresExplicitChildren = 4,
    IsDynamicSafe = 8,
    IsFormControl = 16
}
```

第一阶段应该保持标志有意较小。
只包含实质影响的标志：

- 模板验证
- `@bind` 降低
- 运行时导入/布局提示

## 9. 用户组件的描述符生成规则

### 9.1 普通 `[Parameter]`

映射为：

- 一个 `VuePropDescriptor`

### 9.2 `EventCallback` 和 `EventCallback<T>`

映射为：

- 一个 `VueEmitDescriptor`

推荐别名规则：

- `OnSave` -> `save`
- `OnClose` -> `close`

### 9.3 `RenderFragment`

映射为：

- 如果参数是 `ChildContent` 则为默认插槽
- 否则为命名插槽

### 9.4 `RenderFragment<T>`

映射为：

- 作用域插槽描述符

### 9.5 `Foo + FooChanged`

映射为：

- prop 标记为可绑定
- 相应的 `update:*` 风格 emit 元数据
- 相关的模型支持标志

### 9.6 显式 `Emit(...)`

在尚未被 `EventCallback` 覆盖时可能增加 emit 元数据。

## 10. 内置组件的描述符规则

内置组件使用相同的描述符模型。

示例：

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

它们仅在以下方面不同：

- `SourceKind = Intrinsic`
- 导入源
- prop/插槽定义

它们不得在没有描述符条目的情况下通过仅编译器名称猜测来处理。

## 11. 库组件的描述符规则

库组件也使用相同的描述符模型。

示例：

- `VBtn`
- `VDialog`
- `VTextField`

它们仅在以下方面不同：

- `SourceKind = LibraryComponent`
- 导入源
- 样式/运行时依赖

第一阶段推荐：

- 库包通过注册表/提供者机制提供描述符
- 编译器核心不硬编码每个第三方组件

## 12. 组件注册表模型

推荐聚合注册表：

```csharp
public sealed class VueComponentRegistry
{
    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByName { get; }
    public ImmutableDictionary<string, VueComponentDescriptor> ComponentsByFullName { get; }
    public ImmutableDictionary<string, ImmutableArray<VueComponentDescriptor>> ComponentsByResolutionNamespace { get; }
}
```

推荐的描述符源：

1. 内置内置描述符
2. 当前项目用户组件描述符
3. 引用项目用户组件描述符
4. 库注册表提供者

## 12.1 解析上下文

组件解析不得仅为全局名称。

每个 RazorVue 文件需要从以下内容构建的解析上下文：

- 当前组件命名空间
- 作用域内 `using` 指令
- 引用的用户组件描述符
- 内置描述符注册表
- 库描述符注册表

如果编译器无法解释为什么组件名称在一个文件中可见而在另一个文件中不可见，
解析模型就不完整。

## 13. 解析规则

当编译器看到大写类组件样式的标签时：

1. 首先解析显式完全限定组件匹配
2. 保留内置组件名称并解析内置匹配
3. 从当前命名空间和导入的命名空间解析可见的用户组件
4. 从导入的命名空间解析可见的库组件
5. 如果剩余多个可见候选，报告歧义诊断
6. 否则报告错误

当编译器看到组件体下的子节点时：

1. 首先针对父描述符解析插槽名称匹配
2. 如果匹配，视为插槽内容
3. 否则解析为普通组件

## 13.1 内置名称保留

第一阶段应该将内置组件名称视为保留。

示例：

- `Teleport`
- `Transition`
- `KeepAlive`
- `Suspense`

用户组件或库组件不得静默隐藏这些名称。

推荐行为：

- 精确内置名称冲突 -> 诊断
- 无静默隐藏
- 如果稍后添加转义语法，必须显式

## 13.2 `using` 驱动的可见性

库组件仅在其 `ResolutionNamespace` 导入到当前 Razor 文件或通过周围编译模型带入作用域时才可见。

示例：

- `using ECMAScript.UI.Vue;`
- `using ECMAScript.UI.Vue.Vuetify;`

这是保持 UI 库采用轻量级的机制。
不要将每个组件目标属性作为默认可见性模型添加。

## 13.3 歧义行为

如果两个或多个具有相同短名称的非内置组件同时可见，
第一阶段必须报告诊断而不是启发式选择一个。

示例：

- 两个库都导出 `VBtn`
- 项目组件 `Dialog` 与导入的库 `Dialog` 冲突

推荐行为：

- 完全限定用法解决歧义
- 简单名称回退不解决

## 13.4 第一阶段歧义解析的创作语法

第一阶段应该保持组件创作语法有意窄。

支持的解析形式：

- 不明确时的简单组件名称
- 需要消歧时的完全限定组件名称

示例：

- `<Dialog />`
- `<Demo.Components.Dialog />`

第一阶段不应该为此问题需要自定义目标属性或目标选择器语法。

第一阶段也不应该承诺别名限定组件标签，除非编译器可以在生成的 Razor 输出中证明它们稳定的降低形式。

推荐管理规则：

- 简单名称是默认创作形式
- 完全限定组件名称是第一阶段唯一需要的歧义转义
- 基于别名的组件标签解析推迟到它具有稳定的语义提取路径

## 14. 严格性规则

对于组件调用站点：

- prop 匹配是严格的
- 未知 props 是诊断
- 未知事件别名是诊断
- 未解析的插槽名称不被静默接受为普通 props
- 歧义的组件名称是诊断

对于 HTML 元素：

- 第一阶段在适当的地方可能保持更宽松

## 15. 描述符标识和 HMR/Sourcemap

描述符必须参与工件标识和未来的 HMR 决策。

至少：

- 描述符内容必须可哈希
- 描述符更改必须与仅模板和仅逻辑更改可区分

描述符还应保留足够的源可链接标识以进行诊断和后续工具，
尽管第一阶段不需要完整的描述符级 sourcemap 行为。

## 16. 第一阶段范围

第一阶段需要：

- 描述符结构
- 用户组件描述符提取
- 内置描述符注册
- `using` 感知的可见性规则
- 歧义诊断
- 严格的基于描述符的组件验证

第一阶段不需要：

- 全面的第三方生态系统覆盖
- 自动 npm/jsr 包内省
- 高级指令元数据系统

## 17. 结论

RazorVue 必须将组件契约视为显式编译器拥有的元数据。

`VueComponentDescriptor` 不是可选的便利结构。
它是保持以下内容稳定的边界：

- 模板验证
- 插槽解析
- `@bind` 降低
- 生态系统集成
- 面向主机的元数据
