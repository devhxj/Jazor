# 目标

每个子目录对应项目中的一个核心模块，记录其**为什么存在**、**解决什么问题**、**大致的实现思路**。

## 两条技术线路

| 线路 | 模式 | 文档目录 | 对应源码 |
|------|------|---------|---------|
| **RazorVue** | 库模式（Source Generator，不使用 .vue SFC） | `razorvue/` | `src/Jazor.RazorVue/` |
| **Jolt** | 全功能模式（类似 Vite，支持 .jazor + .vue SFC） | `jolt/` | `src/Jolt/` |

## 按项目结构索引

| 目录 | 对应源码 | 内容 |
|------|---------|------|
| `ecmascript/` | `src/ECMAScript/` | JS 运行时对齐规则、空值语义、WebIDL 清单 |
| `compiler/` | `src/Jazor.Compiler/` | 编译器架构、转换管线、白名单 |
| `compiler/semantic-walker/` | `src/Jazor.Compiler/` SemanticWalker | 各操作类型的 C#→JS 转换规范（14 个文件） |
| `compiler/sourcemap/` | 跨模块 SourceMap | SourceMap 设计决策与约束 |
| `compiler/emit/` | `src/Jazor.Emit/` | 发射管线、打包物化、SourceMap 输出 |
| `clr/` | `src/Jazor.CLR/` + Generator | CLR 运行时支持、白名单声明与自动生成 |
| `analyzer/` | `src/Jazor.Analyzer/` | 静态代码分析、白名单编译时验证 |
| `webidl/` | `src/ECMAScript.WebIDL.Generator/` | WebIDL 规范到 C# 绑定的自动生成 |
| `razor/` | `src/Jazor.Razor/` + `src/Jazor.Compiler.Razor/` | Razor 语法到 Jazor 编译管线的桥接层 |
| `razorvue/` | `src/Jazor.RazorVue/` + Analysis + Vuetify | **库模式**：Source Generator、Vuetify 组件库 |
| `razorvue/design/` | `src/Jazor.RazorVue/` | RazorVue 设计决策与约束（11 个文件） |
| `jolt/` | `src/Jolt/` | **全功能模式**：LSP + DevServer + HMR + Debug + Build |
| `common/` | `src/Jazor.Common/` + `src/Jazor.Name/` | 跨项目共享契约与命名工具 |
| `tools/` | `src/Jazor/` + VSCode + Test | NuGet 打包、VS Code 扩展、集成测试 |

## 保留在源码旁的文档

| 位置 | 说明 |
|------|------|
| `src/Jazor.CLR/doc/*.md` | 47 个 CLR 模块文档（与源码强关联，保留原处） |
