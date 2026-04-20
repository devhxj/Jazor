# RazorVue 项目职责

本文档修复 RazorVue 路线的最终项目级所有权边界。

它的存在是因为仓库已经达到"代码工作"不再足够的程度。
如果没有稳定的项目边界，下一个实现步骤将缓慢模糊：

- 编译器核心编排
- Razor 前端提取
- Vue 目标降低
- 面向创作的组件 API

因此本文档回答两个问题：

1. 哪个项目应该拥有每个职责
2. 哪些窄接口应该连接这些项目

相关文档：

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.Review.md](./RazorVue.Review.md)

## 1. 最终定位

仓库应该收敛于此公共项目拆分：

- `Jazor.Compiler`
  拥有编译器核心编排、共享契约、工件塑造、静态模块生成和扩展点
- `Jazor.Razor`
  拥有面向 Razor 的基础组件基质（`JazorComponent`）
- `Jazor.RazorVue`
  拥有面向 Vue 的创作基质（`VueComponent`）加上 RazorVue 核心语义车道：前端发现、生成的 Razor 分析、描述符提取、源出处、渲染树提取和 Vue 工件降低
- `Jazor.RazorVue.Analysis`
  拥有瘦 RazorVue 生成器/分析器面向主机入口：仅 Roslyn 连线和诊断投影
- `Jazor.Emit`
  拥有目录/物化工件读取和清单持久性

重要澄清是：

- Razor 基础类型不是编译器分析
- Vue 创作 API 不是 Roslyn/前端提取
- 编译器编排不同于 Razor 解析

## 2. 职责矩阵

| 能力 | Jazor.Compiler | Jazor.Razor | Jazor.RazorVue | Jazor.RazorVue.Analysis | Jazor.Emit |
|---|---|---|---|---|---|
| 增量生成器编排 | 拥有 | 否 | 否 | 拥有瘦主机入口 | 否 |
| 扩展点接口 | 拥有 | 否 | 否 | 消费 | 否 |
| 共享工件/目录契约 | 拥有 | 使用 | 使用 | 使用 | 消费 |
| HMR/源始核心契约 | 拥有 | 使用 | 生产/使用 | 投影到诊断 | 持久/消费 |
| Razor 入口检测 | 否 | 否 | 拥有 | 否 | 否 |
| 生成的 Razor 代码分析 | 否 | 否 | 拥有 | 否 | 否 |
| `BuildRenderTree` 提取 | 拥有共享原语 | 否 | 拥有 | 否 | 否 |
| Razor 源出处 | 拥有契约 | 否 | 拥有提取/映射 | 投影到诊断 | 否 |
| Vue 描述符塑造 | 共享边界 | 否 | 拥有 | 否 | 否 |
| Vue 渲染函数降低 | 拥有目标管道协调 | 否 | 拥有 | 否 | 否 |
| `JazorComponent` | 否 | 拥有 | 否 | 否 | 否 |
| `VueComponent` 和 Vue 创作糖 | 否 | 否 | 拥有 | 否 | 否 |
| 目录清单读取/写入 | 否 | 否 | 否 | 否 | 拥有 |

## 3. 硬规则

以下规则应在后续阶段中保持固定：

1. `Jazor.Compiler` 不得累积超过保持当前构建所必需的更多 Razor 特定提取逻辑。
2. `Jazor.Razor` 必须保持瘦运行时/基础库，而非 Roslyn 分析之家。
3. `Jazor.RazorVue` 拥有 RazorVue 核心语义车道，包括生成的 Razor 分析和 `BuildRenderTree` 解析/降低。
4. `Jazor.RazorVue.Analysis` 必须保持瘦主机入口，不得吸收通用编译器核心逻辑或复制 RazorVue 核心语义。
5. Vue 目标语义可能依赖于 Razor 前端输出，但 Razor 前端输出不得依赖于 Vue 创作 API。
6. 现在添加的任何扩展接缝必须足够窄，以便后续物理移动不需要另一个公共重新设计。

## 4. 最小接口方案

当前路线不需要广泛的通用 UI 框架抽象。

它只需要以下之间的窄编译器接缝：

1. Razor 语义前端提取
2. Vue 目标降低
3. 目录物化

推荐的最小接口是：

```csharp
public interface IRazorSemanticFrontend
{
    string Name { get; }
    bool CanHandle(Compilation compilation);
    RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol);
    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation);
}
```

```csharp
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);
    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}
```

为什么这保持有意窄：

- 它不发明假的多框架编译器模型
- 它允许 `Jazor.Compiler` 拥有编排而不永远拥有所有 Razor 提取
- 它保持 `Jazor.RazorVue` 作为单个 RazorVue 核心语义之家
- 它保持 `Jazor.RazorVue.Analysis` 瘦且可替换为主机入口
- 它保持 Vue 目标车道显式

## 5. 分阶段代码移动计划

因为仓库当前有目标框架拆分：

- `Jazor.Compiler` 是 `netstandard2.0`
- `Jazor.Razor`、`Jazor.RazorVue` 和当前 `Jazor.RazorVue.Analysis` 入口是 `net10.0`

项目不应强制不安全的反向引用以满足文件夹纯度。

实用的分阶段移动是：

### 阶段 1

- 在 `Jazor.Compiler` 中添加扩展接口
- 使 `RazorVuePipeline` 消费接口
- 将 RazorVue 核心语义实现保留在 `Jazor.RazorVue` 中
- 通过 `Jazor.RazorVue.Analysis` 公开瘦 Roslyn 主机入口
- 保持运行时/基础库免于 Roslyn 分析代码

### 阶段 2

- 将任何剩余的 RazorVue 语义逻辑从 `Jazor.Compiler` 迁移到 `Jazor.RazorVue`
- 将 `Jazor.RazorVue.Analysis` 限制为 Roslyn 连线和诊断投影
- 停止直接在 `Jazor.Compiler` 内增长 Razor 提取

### 阶段 3

- 一旦 `Jazor.Compiler` 对于 RazorVue 路线仅是编排，就退役过渡性默认实现

## 6. 审查第一轮

### 6.1 开发者审查

发现：

- 如果创作/运行时库与分析分离，架构方向会更好
- 现在进行大的物理移动仍会与当前目标框架分层斗争
- 现在引入窄接缝比等待更多提取逻辑落地更便宜

决定：

- 接受接口优先的分阶段移动
- 拒绝 bang 项目移动，并在当前迭代中保持 `Jazor.RazorVue` 作为单个 RazorVue 核心语义之家

### 6.2 项目经理审查

发现：

- 清晰的公共项目拆分降低了编译器、运行时和分析工作之间未来的协调成本
- 分阶段接缝让交付继续，而架构债务停止增长
- 将面向创作的类型移出编译器风格的项目名称立即提高产品清晰度

决定：

- 批准分阶段实现
- 要求文档明确解释为什么物理移动是分阶段的

## 7. 审查第二轮

### 7.1 开发者审查

挑战：

- 第一个接口草案仍可能漂移到过度抽象

解决方案：

- 保持接缝 Razor/Vue 路线特定
- 现在不要添加通用框架不可知类型系统
- 仅保留前端提取和降低接缝

决定：

- 窄接口形状可接受
- 在此阶段添加更多通用编译器抽象将过早

### 7.2 项目经理审查

挑战：

- 分阶段重构往往在第一阶段更改命名而非所有权时失败

解决方案：

- 要求此阶段中的真实代码移动：
  - 管道消费接口
  - 新的公共项目存在用于运行时/基础和瘦分析主机入口
  - 测试证明新的运行时名称和核心拥有的 RazorVue 路径

决定：

- 批准，条件是此阶段产生可执行的代码移动信号，而不仅仅是文档

## 8. 最终结果

仓库应该向以下方向移动：

- `Jazor.Compiler` 中的核心编排
- `Jazor.Razor` 中的 Razor 基础类型
- `Jazor.RazorVue` 中的 Vue 创作表面
- `Jazor.RazorVue.Analysis` 中的 RazorVue 生成器/分析入口

正确的实现策略是：

- 首先在窄接口后面分阶段移动
- 避免不安全的反向引用
- 在接缝附近保留注释，以便未来的贡献者理解为什么边界存在
