# 架构说明

本目录记录当前产品已经稳定下来的结构与模块边界。它不承载实施过程、历史测试数量或过期方案；细到代码文件的说明，仍以对应项目的源码旁文档为准。

| 文档 | 说明 |
| --- | --- |
| [编译器](./compiler.md) | C# 语义降低、白名单和确定性输出的边界 |
| [框架集成层](./framework-integrations.md) | 核心平台与框架特定产品方向的分层约束 |
| [Razor-to-Vue](./razor-to-vue.md) | 当前 Razor SG 到 Vue render-function 的具体集成契约 |
| [RazorVue 开发范式](./razorvue-paradigm.md) | Razor/C# JSX-like 作者模型、支持边界和后续完善工作 |
| [产物管线](./artifact-pipeline.md) | 两类 carrier、模块、源映射、物化输出、打包和 SSR 的归属 |
| [类库产物与引用契约](./library-artifact-contract.md) | 直接引用、工具资产隔离、manifest/ModuleCatalog 传播和统一 Emit 物化 |
| [平台与绑定](./platform-and-bindings.md) | ECMAScript/Vue 生态绑定和宿主契约 |
| [管理壳](./admin-shell.md) | `Jazor.Admin` 库与示例应用的职责分离 |

建议先阅读[系统架构](../01-overview/system-architecture.md)，再按当前问题进入相应的专题文档。
