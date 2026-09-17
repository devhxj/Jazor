using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// FilePond 实例；由 <c>FilePond.Create()</c> 返回，提供文件增删与处理入口。
/// A FilePond instance returned by <c>FilePond.Create()</c>, exposing file add/remove and processing entries.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class FilePondInstance
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected FilePondInstance()
    {
    }

    /// <summary>更新实例选项。Updates the instance options.</summary>
    /// <param name="options">新选项。The new options.</param>
    [Description("@#setOptions")]
    public extern void SetOptions(FilePondOptions options);

    /// <summary>添加单个文件来源。Adds a single file source.</summary>
    /// <param name="source">远程地址或本地文件。A remote location or local file.</param>
    [Description("@#addFile")]
    public extern void AddFile(FilePondFileSource source);

    /// <summary>当前文件列表。The current file list.</summary>
    /// <returns>文件数组。The file array.</returns>
    [Description("@#getFiles")]
    public extern FilePondFile[] GetFiles();

    /// <summary>按 id 或索引取得文件。Gets a file by id or index.</summary>
    /// <param name="query">文件 id 或索引。The file id or index.</param>
    /// <returns>文件对象。The file object.</returns>
    [Description("@#getFile")]
    public extern FilePondFile GetFile(Number query);

    /// <summary>按 id 取得文件。Gets a file by id.</summary>
    /// <param name="query">文件 id。The file id.</param>
    /// <returns>文件对象。The file object.</returns>
    [Description("@#getFile")]
    public extern FilePondFile GetFile(string query);

    /// <summary>移除文件。Removes a file.</summary>
    /// <param name="query">文件 id 或索引。The file id or index.</param>
    [Description("@#removeFile")]
    public extern void RemoveFile(string query);

    /// <summary>移除文件。Removes a file.</summary>
    /// <param name="query">文件索引。The file index.</param>
    [Description("@#removeFile")]
    public extern void RemoveFile(Number query);

    /// <summary>移除全部文件。Removes every file.</summary>
    [Description("@#removeFiles")]
    public extern void RemoveFiles();

    /// <summary>处理（上传）指定文件。Processes (uploads) the given file.</summary>
    /// <param name="query">文件 id 或索引。The file id or index.</param>
    /// <returns>Promise 包装的处理结果。Promise-wrapped processing result.</returns>
    [Description("@#processFile")]
    public extern IPromise<FilePondFile> ProcessFile(string query);

    /// <summary>处理（上传）全部文件。Processes (uploads) every file.</summary>
    /// <returns>Promise 包装的处理结果。Promise-wrapped processing result.</returns>
    [Description("@#processFiles")]
    public extern IPromise<FilePondFile[]> ProcessFiles();

    /// <summary>打开文件浏览对话框。Opens the file browser dialog.</summary>
    [Description("@#browse")]
    public extern void Browse();

    /// <summary>销毁实例并释放 DOM。Destroys the instance and releases its DOM.</summary>
    [Description("@#destroy")]
    public extern void Destroy();
}
