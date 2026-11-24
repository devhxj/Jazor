# 架构说明

本目录记录当前产品的稳定结构与模块边界；实施过程、历史测试数量和过期方案归档于历史资料，代码文件级说明以对应项目的源码旁文档为准。

| 文档 | 说明 |
| --- | --- |
| [Jolt 作者模型、目标特性与语法审计](./jolt-authoring-comparison.md) | Razor 指令与模板、Vue/TSX 组件契约、npm 与本地 SFC 混编的目标写法，以及当前实现对照 |
| [编译器](./compiler.md) | C# 语义降低、白名单和确定性输出的边界 |
| [框架集成层](./framework-integrations.md) | 核心平台与框架特定产品方向的分层约束 |
| [Jazor 与 Jolt 的职责设定](./jazor-jolt-boundaries.md) | 已确定的目标分工。覆盖通用 npm 投影、独立 ES module、Vue 开发会话和宿主交接；实施进度见配套调整方案 |
| [产物管线](./artifact-pipeline.md) | 两类 carrier、模块、源映射、物化输出、打包和 SSR 的归属 |
| [类库产物与引用契约](./library-artifact-contract.md) | 直接引用、工具资产隔离、manifest/ModuleCatalog 传播和统一 Emit 物化 |
| [平台与绑定](./platform-and-bindings.md) | ECMAScript/Vue 生态绑定和宿主契约 |
| [npm 类型投影与值传递规范](./npm-projection-rules.md) | TypeScript 泛型关系、参数与回调方向、C# 表示、重载选择和结构稳定的投影缓存 |

建议先阅读[系统架构](../01-overview/system-architecture.md)，再按当前问题进入相应的专题文档。
