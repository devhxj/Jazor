# External Documents

以下类型默认排除出项目主文档体系：
- `.dotnet/.nuget/packages/**/*.md`
- `src/**/node_modules/**/*.md`
- `.tmp/**/*.md`
- `.claude/worktrees/**/*.md`

这些文件可以被搜索到，但不应被当作 Jazor 项目文档的主入口或权威规范。
