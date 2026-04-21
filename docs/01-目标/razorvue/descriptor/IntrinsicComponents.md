# Vue 内置组件（Intrinsic Components）

## 为什么需要

Vue 内置组件是 Vue 框架提供的特殊组件，它们具有独特的语义和编译时行为。RazorVue 通过内置组件描述符系统，将这些 Vue 原生组件引入 RazorVue 编译时环境，使得开发者可以在 Razor 组件中直接使用它们。

内置组件系统解决了以下问题：

1. **框架集成**：无缝使用 Vue 内置组件（Teleport、Transition、KeepAlive、Suspense）
2. **类型安全**：提供编译时类型检查和智能提示
3. **名称保留**：防止用户组件与内置组件名称冲突
4. **自动导入**：从 "vue" 包自动导入，无需手动声明
5. **元数据统一**：使用统一的描述符系统管理所有组件

## 实现思路

### 内置组件列表

RazorVue 支持 4 个 Vue 内置组件，位于 `src/Jazor.RazorVue/Descriptor/VueIntrinsicComponentDescriptors.cs`：

```csharp
internal static class VueIntrinsicComponentDescriptors
{
    public static ImmutableArray<VueComponentDescriptor> All { get; } =
    [
        Create("Teleport"),
        Create("Transition"),
        Create("KeepAlive"),
        Create("Suspense")
    ];
}
```

### 内置组件特征

#### 1. Teleport

**用途**：将组件内容渲染到 DOM 的其他位置

```csharp
// Vue 模板
<Teleport to="body">
  <div>This will be rendered to body</div>
</Teleport>
```

**RazorVue 使用**：

```razor
<Teleport To="body">
    <div>This will be rendered to body</div>
</Teleport>
```

**描述符**：

```csharp
new VueComponentDescriptor(
    Name: "Teleport",
    FullName: "ECMAScript.UI.Vue.Teleport",
    SourceKind: VueComponentSourceKind.Intrinsic,
    ResolutionNamespace: "ECMAScript.UI.Vue",
    ImportSpecifier: "vue",
    ExportName: "Teleport",
    Props: [],
    Emits: [],
    Slots: [],
    StyleDependencies: [],
    PluginRequirements: [],
    Flags: VueComponentFlags.None)
```

#### 2. Transition

**用途**：为元素和组件添加进入/离开过渡效果

```csharp
// Vue 模板
<Transition name="fade">
  <div v-if="show">Hello</div>
</Transition>
```

**RazorVue 使用**：

```razor
<Transition Name="fade">
    @if (show)
    {
        <div>Hello</div>
    }
</Transition>
```

**描述符**：

```csharp
new VueComponentDescriptor(
    Name: "Transition",
    FullName: "ECMAScript.UI.Vue.Transition",
    SourceKind: VueComponentSourceKind.Intrinsic,
    ResolutionNamespace: "ECMAScript.UI.Vue",
    ImportSpecifier: "vue",
    ExportName: "Transition",
    Props: [],
    Emits: [],
    Slots: [],
    StyleDependencies: [],
    PluginRequirements: [],
    Flags: VueComponentFlags.None)
```

#### 3. KeepAlive

**用途**：缓存组件实例以提升性能

```csharp
// Vue 模板
<KeepAlive>
  <component :is="currentComponent"></component>
</KeepAlive>
```

**RazorVue 使用**：

```razor
<KeepAlive>
    <DynamicComponent Type="@currentComponent" />
</KeepAlive>
```

**描述符**：

```csharp
new VueComponentDescriptor(
    Name: "KeepAlive",
    FullName: "ECMAScript.UI.Vue.KeepAlive",
    SourceKind: VueComponentSourceKind.Intrinsic,
    ResolutionNamespace: "ECMAScript.UI.Vue",
    ImportSpecifier: "vue",
    ExportName: "KeepAlive",
    Props: [],
    Emits: [],
    Slots: [],
    StyleDependencies: [],
    PluginRequirements: [],
    Flags: VueComponentFlags.None)
```

#### 4. Suspense

**用途**：处理异步组件依赖

```csharp
// Vue 模板
<Suspense>
  <template #default>
    <AsyncComponent />
  </template>
  <template #fallback>
    <div>Loading...</div>
  </template>
</Suspense>
```

**RazorVue 使用**：

```razor
<Suspense>
    <Default>
        <AsyncComponent />
    </Default>
    <Fallback>
        <div>Loading...</div>
    </Fallback>
</Suspense>
```

**描述符**：

```csharp
new VueComponentDescriptor(
    Name: "Suspense",
    FullName: "ECMAScript.UI.Vue.Suspense",
    SourceKind: VueComponentSourceKind.Intrinsic,
    ResolutionNamespace: "ECMAScript.UI.Vue",
    ImportSpecifier: "vue",
    ExportName: "Suspense",
    Props: [],
    Emits: [],
    Slots: [],
    StyleDependencies: [],
    PluginRequirements: [],
    Flags: VueComponentFlags.None)
```

### 创建工厂方法

```csharp
private static VueComponentDescriptor Create(string name)
    => new(
        Name: name,
        FullName: $"ECMAScript.UI.Vue.{name}",
        SourceKind: VueComponentSourceKind.Intrinsic,
        ResolutionNamespace: "ECMAScript.UI.Vue",
        ImportSpecifier: "vue",
        ExportName: name,
        Props: [],
        Emits: [],
        Slots: [],
        StyleDependencies: [],
        PluginRequirements: [],
        Flags: VueComponentFlags.None);
```

**特点**：
- 所有内置组件从 `"vue"` 包导入
- 使用虚拟命名空间 `ECMAScript.UI.Vue` 避免冲突
- 无 props/emits/slots（Vue 框架内部处理）
- 无样式依赖和插件需求
- 无特殊标志

### 名称保留机制

#### 冲突检测

内置组件名称是保留字，当用户组件或库组件与内置组件同名时，解析器会报告冲突：

```csharp
// 用户组件
public class Teleport : ComponentBase
{
    // ...
}

// 解析结果
var result = registry.Resolve("Teleport", context);

// 状态
result.Status  // ReservedIntrinsicName
result.Issues[0].Code  // RazorVueIssueCode.ReservedIntrinsicNameCollision
result.Issues[0].Message  // "Component name 'Teleport' collides with a reserved intrinsic Vue component name."
```

#### 冲突解决

用户必须使用完全限定名来引用非内置组件：

```razor
<!-- 错误：歧义引用 -->
<Teleport />

<!-- 正确：引用内置组件 -->
<Teleport />

<!-- 正确：引用用户组件（使用完全限定名））
<App.Components.Teleport />
```

### 注册表集成

内置组件在注册表创建时自动添加，并具有最高优先级：

```csharp
public static VueComponentRegistry Create(
    ImmutableArray<VueComponentDescriptor> userComponents,
    ImmutableArray<VueComponentDescriptor> libraryComponents = default)
{
    var allComponents = ImmutableArray.CreateBuilder<VueComponentDescriptor>();

    // 1. 优先添加内置组件
    AddRange(allComponents, IntrinsicComponents);

    // 2. 添加用户组件
    AddRange(allComponents, userComponents);

    // 3. 添加库组件
    AddRange(allComponents, libraryComponents);

    // ... 构建索引
}
```

### 解析优先级

#### 完全限定名解析

```csharp
// ECMAScript.UI.Vue.Teleport（内置）
var result = registry.Resolve("ECMAScript.UI.Vue.Teleport", context);
result.Status  // Resolved
result.Descriptor.SourceKind  // Intrinsic

// App.Components.Teleport（用户）
var result = registry.Resolve("App.Components.Teleport", context);
result.Status  // Resolved
result.Descriptor.SourceKind  // UserComponent
```

#### 短名称解析（无冲突）

```csharp
// 上下文：无用户 Teleport 组件
var context = new VueComponentResolutionContext("App.Components", []);

var result = registry.Resolve("Teleport", context);
result.Status  // Resolved
result.Descriptor.SourceKind  // Intrinsic
```

#### 短名称解析（有冲突）

```csharp
// 上下文：有 App.Components.Teleport 用户组件
var context = new VueComponentResolutionContext("App.Components", []);

var result = registry.Resolve("Teleport", context);
result.Status  // ReservedIntrinsicName
result.Candidates.Length  // 2（内置 + 用户）
result.Issues.Length  // 1（冲突诊断）
```

### 生成代码示例

#### Teleport 使用

```razor
@* MyComponent.razor *@

<Teleport To="body">
    <div class="modal">
        <h1>Modal Title</h1>
        <p>Modal content</p>
    </div>
</Teleport>
```

**生成的 JavaScript**：

```javascript
// MyComponent.mjs
import { Teleport } from "vue";

export default {
  components: { Teleport },
  setup() {
    return {};
  },
  template: `
    <Teleport to="body">
      <div class="modal">
        <h1>Modal Title</h1>
        <p>Modal content</p>
      </div>
    </Teleport>
  `
};
```

#### Transition 使用

```razor
@* FadeIn.razor *@

<Transition Name="fade" @OnBeforeEnter="OnBeforeEnter" @OnAfterEnter="OnAfterEnter">
    @if (show)
    {
        <div class="content">
            Hello, World!
        </div>
    }
</Transition>

@code {
    private bool show = true;

    private void OnBeforeEnter() => Console.WriteLine("Before enter");
    private void OnAfterEnter() => Console.WriteLine("After enter");
}
```

**生成的 JavaScript**：

```javascript
// FadeIn.mjs
import { Transition, ref } from "vue";

export default {
  components: { Transition },
  setup() {
    const show = ref(true);

    const onBeforeEnter = () => console.log("Before enter");
    const onAfterEnter = () => console.log("After enter");

    return { show, onBeforeEnter, onAfterEnter };
  },
  template: `
    <Transition name="fade" @before-enter="onBeforeEnter" @after-enter="onAfterEnter">
      <div v-if="show" class="content">
        Hello, World!
      </div>
    </Transition>
  `
};
```

## 设计权衡

### 为什么使用虚拟命名空间

使用 `ECMAScript.UI.Vue` 作为内置组件的命名空间，而不是实际的 Vue 包命名空间：

1. **避免冲突**：防止与真实的 Vue 类型冲突
2. **统一管理**：所有内置组件使用相同的命名空间前缀
3. **清晰标识**：通过命名空间明确区分内置组件和用户组件
4. **简化逻辑**：解析器可以快速识别内置组件

### 为什么不声明 Props/Slots

内置组件的 props 和 slots 由 Vue 框架内部处理，编译时无需声明：

1. **框架责任**：Vue 运行时负责验证内置组件的属性
2. **简化元数据**：减少描述符的复杂度
3. **动态特性**：某些内置组件的 API 可能在不同 Vue 版本中变化
4. **编译时优化**：无需在编译时处理内置组件的特殊逻辑

## 文件位置

- **内置组件描述符**：`src/Jazor.RazorVue/Descriptor/VueIntrinsicComponentDescriptors.cs`

## 相关文档

- **组件描述符**：`docs/01-目标/razorvue/descriptor/ComponentDescriptor.md`
- **组件注册表**：`docs/01-目标/razorvue/descriptor/ComponentRegistry.md`
- **描述符工厂**：`docs/01-目标/razorvue/descriptor/DescriptorFactory.md`
- **编译问题**：`docs/01-目标/razorvue/descriptor/CompilationIssues.md`

## 参考资料

- **Vue Teleport 官方文档**：https://vuejs.org/guide/built-ins/teleport.html
- **Vue Transition 官方文档**：https://vuejs.org/guide/built-ins/transition.html
- **Vue KeepAlive 官方文档**：https://vuejs.org/guide/built-ins/keep-alive.html
- **Vue Suspense 官方文档**：https://vuejs.org/guide/built-ins/suspense.html

---

**维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
