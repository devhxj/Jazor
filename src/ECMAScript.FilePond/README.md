# ECMAScript.FilePond

FilePond 与其官方 Vue 适配器的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游多包版本、`manifest.json`（schema 2）、`dist/` 资源、默认样式、许可证与 inventory。

Strongly typed C# bindings for FilePond and its official Vue adapter, shipped as a Jazor JS resource library with locked upstream package versions, package-local `manifest.json` (schema 2), `dist/` runtime assets, the default stylesheet, licenses, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `vue-filepond` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 闭包包 | `vue-filepond`（适配器）、`filepond`（核心，锁定版本） |
| 许可证 | MIT（`licenses/*-LICENSE`） |
| 样式 | `dist/filepond/filepond.css`（manifest `styles` 声明） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

## 首期范围 First slice

- 核心命令式 API（`filepond` 入口）：`create`/`destroy`/`find`/`getOptions`/`setOptions`/`supported`
- 实例 API（`FilePondInstance`）：`SetOptions`、`AddFile`、`GetFiles`/`GetFile`、`RemoveFile`/`RemoveFiles`、`ProcessFile`/`ProcessFiles`、`Browse`、`Destroy`
- 选项（`FilePondOptions`）：服务端配置、多文件、拖拽/浏览/粘贴/移除/回退/重排开关、`MaxFiles`、`AcceptedFileTypes`、`LabelIdle` 等
- 回调：`OnAddFile`、`OnProcessFile`、`OnProcessFileError`、`OnProcessFileProgress`、`OnRemoveFile`、`OnUpdateFiles`、`OnError`、`OnWarning`、`BeforeAddFile`
- 组件代理：`VueFilePond`（按上游**默认导出工厂**绑定；`Options` 承载选项/回调，其余 HTML 属性透传）

未绑定：`registerPlugin`（插件是各 `@pqina/*` 包的默认导出，尚无强类型契约；用 `object` 兜底违反绑定库强类型规则）、`prepareFile`/`moveFile`/`sort` 等次要实例方法。

## SSR 边界 SSR boundary

上传依赖真实 DOM、文件 API 与表单提交。`create`、`FilePondInstance` 与全部回调必须在浏览器生命周期内使用；SSR 期间不得创建实例。

## 使用示例 Authoring

```csharp
var pond = FilePond.Create(element, new FilePondOptions
{
    Server = "/api/upload",
    AllowMultiple = true,
    OnProcessFile = (error, file) => { /* 处理完成 */ }
});
```

```razor
<VueFilePond Options="@Options" />
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-file-pond.cs -- --version <adapter-version>
dotnet run --file scripts/csharp/generate-file-pond.cs -- --source .tmp/p3c --version <adapter-version>
```

核心版本按上游 peer 区间锁定于生成器内的常量；生成器同样执行浏览器安全闸门（拒绝顶层 `process.env`）。

## 测试 Tests

`src/ECMAScript.FilePond.Test` 覆盖 manifest/inventory 元数据、vendored 哈希、样式资源、上游导出 drift 与编译器 emission。
