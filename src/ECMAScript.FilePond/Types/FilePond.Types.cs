using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// FilePond 状态值域；字符串值与 FilePond <c>FileStatus</c> 一致。
/// The FilePond file status domain; values match FilePond's <c>FileStatus</c>.
/// </summary>
public enum FilePondFileStatus
{
    /// <summary>空闲，等待处理。Idle, waiting to be processed.</summary>
    Idle = 1,

    /// <summary>正在加载文件。The file is loading.</summary>
    Loading = 2,

    /// <summary>加载失败。Loading failed.</summary>
    LoadError = 3,

    /// <summary>加载完成。Loading finished.</summary>
    Loaded = 4,

    /// <summary>正在处理。Processing is in progress.</summary>
    Processing = 5,

    /// <summary>处理失败。Processing failed.</summary>
    ProcessingError = 6,

    /// <summary>正在回退。Reverting is in progress.</summary>
    Reverting = 7,

    /// <summary>回退失败。Revert failed.</summary>
    RevertError = 8,

    /// <summary>处理完成。Processing finished.</summary>
    Processed = 9
}

/// <summary>
/// FilePond 服务端配置：URL 字符串或多端点配置对象。
/// The FilePond server configuration: a URL string or a multi-endpoint configuration object.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union FilePondServer(string, FilePondServerOptions);

/// <summary>
/// FilePond 的详细服务端端点配置。
/// The detailed FilePond server endpoint configuration.
/// </summary>
[ECMAScript]
[Description("@#")]
public record FilePondServerOptions
{
    /// <summary>端点基础 URL。The base URL of the endpoints.</summary>
    [Description("@#url")]
    public string? Url { get; init; }

    /// <summary>处理上传的端点。The endpoint that processes uploads.</summary>
    [Description("@#process")]
    public string? Process { get; init; }

    /// <summary>回退（撤销）上传的端点。The endpoint that reverts an upload.</summary>
    [Description("@#revert")]
    public string? Revert { get; init; }

    /// <summary>读取已上传文件的端点。The endpoint that loads an existing file.</summary>
    [Description("@#load")]
    public string? Load { get; init; }

    /// <summary>恢复临时上传的端点。The endpoint that restores a temporary upload.</summary>
    [Description("@#restore")]
    public string? Restore { get; init; }

    /// <summary>拉取远程文件的端点。The endpoint that fetches a remote file.</summary>
    [Description("@#fetch")]
    public string? Fetch { get; init; }

    /// <summary>请求是否携带凭据。Whether requests include credentials.</summary>
    [Description("@#withCredentials")]
    public bool? WithCredentials { get; init; }
}

/// <summary>
/// FilePond 选项；对应核心 <c>FilePondOptions</c> 的常用子集（含服务端、多文件与回调）。
/// FilePond options; the frequently used subset of the core <c>FilePondOptions</c> (server, multiple files, and callbacks).
/// </summary>
[ECMAScript]
[Description("@#")]
public record FilePondOptions
{
    /// <summary>服务端配置。The server configuration.</summary>
    [Description("@#server")]
    public FilePondServer? Server { get; init; }

    /// <summary>初始文件列表（远程地址或本地文件）。The initial files (remote locations or local files).</summary>
    [Description("@#files")]
    public FilePondFileSource[]? Files { get; init; }

    /// <summary>是否允许选择多个文件。Whether multiple files can be selected.</summary>
    [Description("@#allowMultiple")]
    public bool? AllowMultiple { get; init; }

    /// <summary>是否允许拖拽。Whether dropping is allowed.</summary>
    [Description("@#allowDrop")]
    public bool? AllowDrop { get; init; }

    /// <summary>是否允许浏览（点击选择）。Whether browsing (click to select) is allowed.</summary>
    [Description("@#allowBrowse")]
    public bool? AllowBrowse { get; init; }

    /// <summary>是否允许粘贴。Whether pasting is allowed.</summary>
    [Description("@#allowPaste")]
    public bool? AllowPaste { get; init; }

    /// <summary>是否允许移除文件。Whether removing files is allowed.</summary>
    [Description("@#allowRemove")]
    public bool? AllowRemove { get; init; }

    /// <summary>是否允许回退已上传文件。Whether reverting uploaded files is allowed.</summary>
    [Description("@#allowRevert")]
    public bool? AllowRevert { get; init; }

    /// <summary>是否允许重排文件。Whether reordering files is allowed.</summary>
    [Description("@#allowReorder")]
    public bool? AllowReorder { get; init; }

    /// <summary>最大文件数。The maximum number of files.</summary>
    [Description("@#maxFiles")]
    public Number? MaxFiles { get; init; }

    /// <summary>文件字段名。The file field name.</summary>
    [Description("@#name")]
    public string? Name { get; init; }

    /// <summary>是否接受的文件类型。The accepted file types.</summary>
    [Description("@#acceptedFileTypes")]
    public string? AcceptedFileTypes { get; init; }

    /// <summary>空状态提示文本。The idle-state label text.</summary>
    [Description("@#labelIdle")]
    public string? LabelIdle { get; init; }

    /// <summary>是否禁用组件。Whether the component is disabled.</summary>
    [Description("@#disabled")]
    public bool? Disabled { get; init; }

    /// <summary>是否上传文件本身（而非仅路径）。Whether the file itself is stored instead of just a path.</summary>
    [Description("@#storeAsFile")]
    public bool? StoreAsFile { get; init; }

    /// <summary>是否强制在加载失败时也允许回退。Whether revert is forced even when loading failed.</summary>
    [Description("@#forceRevert")]
    public bool? ForceRevert { get; init; }

    /// <summary>文件添加完成后的回调。Callback invoked after a file is added.</summary>
    [Description("@#onaddfile")]
    public Action<FilePondError?, FilePondFile?>? OnAddFile { get; init; }

    /// <summary>文件处理完成后的回调。Callback invoked after a file is processed.</summary>
    [Description("@#onprocessfile")]
    public Action<FilePondError?, FilePondFile?>? OnProcessFile { get; init; }

    /// <summary>文件处理失败时的回调。Callback invoked when file processing fails.</summary>
    [Description("@#onprocessfileerror")]
    public Action<FilePondError?, FilePondFile?>? OnProcessFileError { get; init; }

    /// <summary>处理进度的回调。Callback invoked with processing progress.</summary>
    [Description("@#onprocessfileprogress")]
    public Action<FilePondFile?, Number?>? OnProcessFileProgress { get; init; }

    /// <summary>文件被移除后的回调。Callback invoked after a file is removed.</summary>
    [Description("@#onremovefile")]
    public Action<FilePondError?, FilePondFile?>? OnRemoveFile { get; init; }

    /// <summary>文件列表变化时的回调。Callback invoked whenever the file list changes.</summary>
    [Description("@#onupdatefiles")]
    public Action<FilePondFile[]?>? OnUpdateFiles { get; init; }

    /// <summary>发生错误时的回调。Callback invoked when an error occurs.</summary>
    [Description("@#onerror")]
    public Action<FilePondError?>? OnError { get; init; }

    /// <summary>发生警告时的回调。Callback invoked when a warning occurs.</summary>
    [Description("@#onwarning")]
    public Action<FilePondError?>? OnWarning { get; init; }

    /// <summary>文件添加前的预处理回调；返回 false 可拒绝该文件。Pre-add hook; returning false rejects the file.</summary>
    [Description("@#beforeAddFile")]
    public Func<FilePondFile, bool>? BeforeAddFile { get; init; }
}

/// <summary>
/// FilePond 的初始文件来源：远程地址或浏览器 <c>File</c> 对象。
/// A FilePond initial file source: a remote location or a browser <c>File</c> object.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union FilePondFileSource(string, Blob);

/// <summary>
/// FilePond 插件对象（不透明宿主值）；由各 <c>@pqina/*</c> 插件包的默认导出提供，在 Jazor 侧只作传递。
/// A FilePond plugin object (opaque host value) supplied by the default export of an individual
/// <c>@pqina/*</c> plugin package; Jazor only passes it through.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class FilePondPlugin
{
    private FilePondPlugin()
    {
    }
}

/// <summary>
/// FilePond 文件对象。
/// A FilePond file object.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class FilePondFile
{
    private FilePondFile()
    {
    }
}

/// <summary>
/// FilePond 错误对象；无错误时为 null。
/// A FilePond error object; null when there is no error.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class FilePondError
{
    private FilePondError()
    {
    }
}
