namespace ECMAScript;

/// <summary>
/// FileSystemCreateWritableOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemCreateWritableOptions")]
public record FileSystemCreateWritableOptions(
    [property: Description("@#keepExistingData")]bool KeepExistingData = false);

/// <summary>
/// FileSystemGetFileOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemGetFileOptions")]
public record FileSystemGetFileOptions(
    [property: Description("@#create")]bool Create = false);

/// <summary>
/// FileSystemGetDirectoryOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemGetDirectoryOptions")]
public record FileSystemGetDirectoryOptions(
    [property: Description("@#create")]bool Create = false);

/// <summary>
/// FileSystemRemoveOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemRemoveOptions")]
public record FileSystemRemoveOptions(
    [property: Description("@#recursive")]bool Recursive = false);

/// <summary>
/// WriteParams
/// </summary>
[ECMAScript]
[Description("@#WriteParams")]
public record WriteParams(
    [property: Description("@#type")]WriteCommandType? Type = default,
	[property: Description("@#size")]ulong Size = default,
	[property: Description("@#position")]ulong Position = default,
	[property: Description("@#data")]Either<IBufferSource, Blob, string>? Data = default);

/// <summary>
/// FileSystemReadWriteOptions
/// </summary>
[ECMAScript]
[Description("@#FileSystemReadWriteOptions")]
public record FileSystemReadWriteOptions(
    [property: Description("@#at")]ulong At = default);

/// <summary>
/// FileSystemFileHandle
/// </summary>
[ECMAScript]
[Description("@#FileSystemFileHandle")]
public class FileSystemFileHandle : FileSystemHandle
{
    /// <summary>
    /// getFile
    /// </summary>
    [Description("@#getFile")]
    public extern PromiseResult<File> GetFile();

    /// <summary>
    /// createWritable
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createWritable")]
    public extern PromiseResult<FileSystemWritableFileStream> CreateWritable(FileSystemCreateWritableOptions? options = default);

    /// <summary>
    /// createSyncAccessHandle
    /// </summary>
    [Description("@#createSyncAccessHandle")]
    public extern PromiseResult<FileSystemSyncAccessHandle> CreateSyncAccessHandle();
}

/// <summary>
/// FileSystemDirectoryHandle
/// </summary>
[ECMAScript]
[Description("@#FileSystemDirectoryHandle")]
public class FileSystemDirectoryHandle : FileSystemHandle, IEnumerable<(string, FileSystemHandle)>
{
    extern IEnumerator<(string, FileSystemHandle)> IEnumerable<(string, FileSystemHandle)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// getFileHandle
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#getFileHandle")]
    public extern PromiseResult<FileSystemFileHandle> GetFileHandle(string name, FileSystemGetFileOptions? options = default);

    /// <summary>
    /// getDirectoryHandle
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#getDirectoryHandle")]
    public extern PromiseResult<FileSystemDirectoryHandle> GetDirectoryHandle(string name, FileSystemGetDirectoryOptions? options = default);

    /// <summary>
    /// removeEntry
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#removeEntry")]
    public extern PromiseResult<object> RemoveEntry(string name, FileSystemRemoveOptions? options = default);

    /// <summary>
    /// resolve
    /// </summary>
    /// <param name="possibleDescendant">possibleDescendant</param>
    [Description("@#resolve")]
    public extern PromiseResult<string[]?> Resolve(FileSystemHandle possibleDescendant);
}

/// <summary>
/// FileSystemWritableFileStream
/// </summary>
[ECMAScript]
[Description("@#FileSystemWritableFileStream")]
public class FileSystemWritableFileStream(object underlyingSink, QueuingStrategy strategy) : WritableStream(underlyingSink, strategy)
{
    /// <summary>
    /// write
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#write")]
    public extern PromiseResult<object> Write(FileSystemWriteChunkType data);

    /// <summary>
    /// seek
    /// </summary>
    /// <param name="position">position</param>
    [Description("@#seek")]
    public extern PromiseResult<object> Seek(ulong position);

    /// <summary>
    /// truncate
    /// </summary>
    /// <param name="size">size</param>
    [Description("@#truncate")]
    public extern PromiseResult<object> Truncate(ulong size);
}

/// <summary>
/// FileSystemSyncAccessHandle
/// </summary>
[ECMAScript]
[Description("@#FileSystemSyncAccessHandle")]
public class FileSystemSyncAccessHandle
{
    /// <summary>
    /// read
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="options">options</param>
    [Description("@#read")]
    public extern ulong Read(IAllowSharedBufferSource buffer, FileSystemReadWriteOptions? options = default);

    /// <summary>
    /// write
    /// </summary>
    /// <param name="buffer">buffer</param>
    /// <param name="options">options</param>
    [Description("@#write")]
    public extern ulong Write(IAllowSharedBufferSource buffer, FileSystemReadWriteOptions? options = default);

    /// <summary>
    /// truncate
    /// </summary>
    /// <param name="newSize">newSize</param>
    [Description("@#truncate")]
    public extern void Truncate(ulong newSize);

    /// <summary>
    /// getSize
    /// </summary>
    [Description("@#getSize")]
    public extern ulong GetSize();

    /// <summary>
    /// flush
    /// </summary>
    [Description("@#flush")]
    public extern void Flush();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();
}

/// <summary>
/// StorageManager
/// </summary>
[ECMAScript]
[Description("@#StorageManager")]
public partial class StorageManager
{
    /// <summary>
    /// getDirectory
    /// </summary>
    [Description("@#getDirectory")]
    public extern PromiseResult<FileSystemDirectoryHandle> GetDirectory();

    /// <summary>
    /// persisted
    /// </summary>
    [Description("@#persisted")]
    public extern PromiseResult<bool> Persisted();

    /// <summary>
    /// persist
    /// </summary>
    [Description("@#persist")]
    public extern PromiseResult<bool> Persist();

    /// <summary>
    /// estimate
    /// </summary>
    [Description("@#estimate")]
    public extern PromiseResult<StorageEstimate> Estimate();
}