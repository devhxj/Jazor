# 目标

每个子目录对应仓库中的一个核心模块或一条核心工作线，记录它**为什么存在**、**解决什么问题**、以及**当前推荐的实现边界**。

## 两条技术线路

| 线路 | 模式 | 文档目录 | 当前物理落点 |
|------|------|---------|-------------|
| **RazorVue** | 库模式（Source Generator，不以 `.vue` 为 authoring 格式） | `razorvue/` | `src/Jazor.Common/RazorVue/` + `src/Jazor.Analyzer/RazorVue/` + `src/ECMAScript.Vuetify/` |
| **Jolt** | 全功能模式（`.jazor` 开发时宿主） | `jolt/` | `src/Jolt/` |

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `compiler/` | `src/Jazor.Compiler/` | 编译器架构、转换管线、主线约束 |
| `compiler/semantic-walker/` | `src/Jazor.Compiler/` | 各操作类型的 lowering 规则 |
| `compiler/sourcemap/` | `src/Jazor.Common/SourceMaps/` + `src/Jazor.Emit/SourceMaps/` | SourceMap 设计与物化约束 |
| `compiler/emit/` | `src/Jazor.Emit/` | 发射、物化、打包与差分输出 |
| `clr/` | `src/Jazor.CLR/` + Generator | CLR 映射声明与白名单生成链路 |
| `analyzer/` | `src/Jazor.Analyzer/` | 静态代码分析与 RazorVue 编译时分析宿主 |
| `razor/` | `src/Jazor.Razor/` + `src/Jazor.Compiler.Razor/` | Razor 基础标记与编译器侧语义前端 |
| `ecmascript/` | `src/ECMAScript/` | ECMAScript host、Vue3 平衡式目标与映射细节、host/nullish 边界 |
| `razorvue/` | `src/Jazor.Common/RazorVue/` + `src/Jazor.Analyzer/RazorVue/` + `src/ECMAScript.Vuetify/` | 库模式 Razor-to-JS、组件描述与 Vuetify authoring |
| `jolt/` | `src/Jolt/` | `.jazor` LSP、DevServer、Build、Debug、Deno/Volar 集成 |
| `common/` | `src/ECMAScript.Contract/` + `src/Jazor.Common/` | 最小契约层与共享实现层 |
| `webidl/` | `src/ECMAScript.WebIDL.Generator/` | WebIDL 到 C# 绑定生成 |
| `tools/` | `src/Jazor/` + `src/Jolt.VSCodeExtension/` | NuGet 打包、VS Code 集成与工具侧边界 |

## 保留在源码旁的文档

| 位置 | 说明 |
|------|------|
| `src/Jazor.CLR/doc/*.md` | CLR 模块签名与映射参考，和源码紧耦合，保留在模块旁 |
