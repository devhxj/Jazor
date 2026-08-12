# 宿主生产就绪

> 范围：Windows x64 优先，将 SPA、SSR、HMR 与 Debug 交付从“可运行”推进到可重复验证的生产契约。

## 目标

宿主层的验收不能只依赖仓库内 `ProjectReference` 构建成功。每一条对外能力都需要在实际发布布局、实际浏览器和实际 NuGet 包消费者中证明其行为：

- SPA 在 release publish 后只加载 Netpack `bundle.js`，而不是残留 Debug 模块图。
- 首次 HTML、PathBase、静态资源、404 与 SPA 导航在发布目录中保持一致。
- `ECMAScript.Style` 和 `H()` 由真实 Wiki 页面使用，而不是孤立的示例或单元测试。
- 开发期 HMR/Debug 保持可检查模块、source map 与 reload transport；生产环境不暴露 reload transport。
- SSR 在独立阶段按同样标准覆盖服务器模块图、hydration 与发布包消费。

## 第一阶段：SPA Release Publish

状态：已实施，使用 `samples/Wiki` 作为真实消费者。

### 交付范围

1. Wiki 的 H 函数页面通过 `WikiStyleSheet` 使用 `ECMAScript.Style`，并在应用启动前加载样式模块。
2. Debug 和 Release 使用不同产物契约：Debug 使用 `main.mjs` + `jazor-manifest.json`；Release 使用 `bundle.js` + `bundle.js.map`。
3. 宿主根据当前 Debug manifest 优先选择 `main.mjs`，避免 watch 构建旁残留的旧 bundle 被加载。
4. 浏览器门禁在 Edge 中验证真实路由、`ecs-*` class、`#ecmascript-style`、生成 selector、source map 和生产环境无 HMR transport。
5. `verify-windows-spa-release.cs` 复制 Wiki 到临时目录，以 `WikiUsePackages=true` 从本地 `Jazor`、`Jazor.Vue`、`ECMAScript.Style` 包恢复、发布并运行浏览器验证。
6. Tag 发布工作流在上传和推送包之前消费刚打出的包运行该门禁。

### 验收命令

```bash
dotnet run --file scripts/csharp/wiki-verify-smoke.cs -- --publish --path-base /docs
dotnet run --file scripts/csharp/wiki-verify-browser.cs -- --publish --path-base /docs
dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs
```

第三条命令默认打包本地 Release 包。发布工作流传入 `--package-source artifacts/packages --skip-pack`，确保验证的是工作流刚生成的准确 nupkg。

## 后续阶段

### 第二阶段：开发与 HMR

- 用 `dotnet watch` 驱动真实 Wiki 的源码变化，并对模板热更新和全页刷新分别验证可见结果。
- 覆盖 watcher 重建期间的短暂文件替换、PathBase 下 websocket、错误恢复和浏览器连接重建。
- 将 HMR 事件协议、版本兼容性和失败诊断固定为可测试契约。

### 第三阶段：SSR 与 Hydration

- 建立一个使用 `JazorSSR=true` 的真实页面，验证 release `jazor/ssr/` 模块图、服务端 HTML、hydration 和交互恢复。
- 将 SSR 发布验证加入独立 NuGet 消费者，覆盖 Windows x64 DenoHost runtime 与部署根目录解析。
- 对 hydration mismatch、SSR 模块缺失和服务器渲染错误提供可操作诊断，而不回退为静默 CSR。

### 第四阶段：运行与发布质量

- 增加发布缓存、压缩、source-map 公开策略和健康检查的明确部署选项。
- 在 Windows 上验证并发发布、端口/文件锁清理和失败日志归档。
- 将最终 SPA、HMR、SSR 门禁纳入版本发布前的统一质量报告。
