# 编译器架构桥接

## 入口顺序
1. [Compiler 文档索引](./README.md)
2. [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
3. [ArchitectureOverview.md](./ArchitectureOverview.md)
4. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
5. [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

## 长期有效规范
- [AstConverter.md](./AstConverter.md)
- [semantic-walker/SemanticWalker.md](./semantic-walker/SemanticWalker.md)
- [WhiteList.md](./WhiteList.md)
- [OpCompileSpec.md](./OpCompileSpec.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

## 专题入口
- [sourcemap/SourceMap.Overview.md](./sourcemap/SourceMap.Overview.md)
- [emit/Emit.Pipeline.Overview.md](./emit/Emit.Pipeline.Overview.md)
- [StableUniqueNameAllocation.md](./StableUniqueNameAllocation.md)

## 说明
这页只做仓库级桥接，不复制正文。

仓库中与 compiler 相关的两类核心材料分别是：

- `docs/01-目标/compiler/`：仓库级架构、专题说明与阅读路径；
- `src/Jazor.Compiler/ImplementationPrinciples.md`：项目内实现路线、边界、价值排序与扩展判据。

如果需要一个稳定入口，优先从 [README.md](./README.md) 开始。
