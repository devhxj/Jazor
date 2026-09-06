# RazorVue 范式调试

RazorVue 的调试链路以作者源文件为入口，不要求阅读内部 AST。生成 Debug 产物后，可以用仓库脚本检查 `.razor`、官方 Razor Source Generator 的 `.razor.g.cs`、render-function `.mjs` 和 `.mjs.map` 是否仍然连通：

```text
dotnet run --file scripts/csharp/inspect-razorvue-chain.cs -- \
  --source Pages/Counter.razor \
  --generated obj/Debug/net11.0/generated/Pages/Counter.razor.g.cs \
  --artifact bin/Debug/net11.0/jazor/Pages/Counter.mjs \
  --map bin/Debug/net11.0/jazor/Pages/Counter.mjs.map \
  --json
```

脚本输出四段稳定信息：生成 C# 是否包含 `BuildRenderTree`、模块是否包含 `sourceMappingURL`、source map 的 source 列表以及 `.razor` 是否被映射。提供 `--map` 时，缺少源映射或模块引用会以非零退出码失败；这表示 Debug 交付不完整，应修复产物管线，而不是在应用侧改写路径。

脚本只读取文件，不改变生成目录。CI 或本地诊断应保存 JSON 输出、提交 SHA、SDK 版本和实际产物路径，便于把作者源位置与浏览器错误对应起来。Release bundle 没有逐模块 source map 时，继续使用 Debug profile 定位 lowering 问题，再用 Release consumer 门禁验证最终交付。

相关入口：

- [RazorVue 开发范式](../02-architecture/razorvue-paradigm.md)
- [当前状态与质量门槛](../04-roadmap/current-status.md)
- `scripts/csharp/verify-development-hmr.cs`
- `scripts/csharp/verify-windows-spa-release.cs`
- `scripts/csharp/verify-windows-ssr-release.cs`
