# Testing Archive

> Historical archive only.
> The files in this directory preserve point-in-time audit results, test counts, project names, file paths, and patch-tracking notes.
> They are useful for tracing why a fix happened, but they must not be read as the current repository state.

- [2026-01-27 Compiler Test Audit Summary](./2026-01-27-compiler-test-audit-summary.md)
- [原始分析报告](./TEST_ANALYSIS_REPORT.md)
- [原始进度报告](./TEST_PROGRESS_REPORT.md)
- [原始补充总结](./ADDITIONAL_TESTS_SUMMARY.md)

本目录保存历史测试审计材料，主要拿来追溯，不代表当前仓库的实时状态。

尤其要注意：

- 文中出现的测试总数、通过率、项目名、文件路径，都可能只对应当时快照；
- “已完成 / 已修复 / 全部通过”这类结论，默认只对该文档记录时点成立；
- 判断今天的 compiler 状态，不应以本目录里的历史报告直接代替当前测试运行结果。

要看现在的测试面貌，还是回到当前的 status 和 plans 入口更稳当。

当前入口建议：

- [Compiler 当前状态](../status.md)
- [Compiler 文档索引](../../../01-目标/compiler/README.md)
- [ImplementationPrinciples.md](../../../../src/Jazor.Compiler/ImplementationPrinciples.md)
- [Compiler 计划清单](../../../02-计划/compiler/TransformationRoadmap.md)
