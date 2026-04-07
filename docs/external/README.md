# External Documents

以下类型默认排除在项目主文档体系外头：
- `.dotnet/.nuget/packages/**/*.md`
- `src/**/node_modules/**/*.md`
- `.tmp/**/*.md`
- `.claude/worktrees/**/*.md`

这些文件可以被搜索到，但不该拿来当 Jazor 项目文档的主入口或权威规范。要查外部依赖可以看，莫把它们和仓库自有文档整混了。
