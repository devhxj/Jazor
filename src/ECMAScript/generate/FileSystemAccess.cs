namespace ECMAScript;

/// <summary>
/// FileSystemPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#FileSystemPermissionDescriptor")]
public record FileSystemPermissionDescriptor(
    [property: Description("@#handle")]FileSystemHandle? Handle = default,
	[property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read): PermissionDescriptor;

/// <summary>
/// FileSystemHandlePermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#FileSystemHandlePermissionDescriptor")]
public record FileSystemHandlePermissionDescriptor(
    [property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read);

/// <summary>
/// FilePickerAcceptType
/// </summary>
[ECMAScript]
[Description("@#FilePickerAcceptType")]
public record FilePickerAcceptType(
    [property: Description("@#description")]string? Description = default,
	[property: Description("@#accept")]Dictionary<string, Either<string, string[]>>? Accept = default);

/// <summary>
/// FilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#FilePickerOptions")]
public record FilePickerOptions(
    [property: Description("@#types")]FilePickerAcceptType[]? Types = default,
	[property: Description("@#excludeAcceptAllOption")]bool ExcludeAcceptAllOption = false,
	[property: Description("@#id")]string? Id = default,
	[property: Description("@#startIn")]StartInDirectory? StartIn = default);

/// <summary>
/// OpenFilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#OpenFilePickerOptions")]
public record OpenFilePickerOptions(
    [property: Description("@#multiple")]bool Multiple = false): FilePickerOptions;

/// <summary>
/// SaveFilePickerOptions
/// </summary>
[ECMAScript]
[Description("@#SaveFilePickerOptions")]
public record SaveFilePickerOptions(
    [property: Description("@#suggestedName")]string? SuggestedName = default): FilePickerOptions;

/// <summary>
/// DirectoryPickerOptions
/// </summary>
[ECMAScript]
[Description("@#DirectoryPickerOptions")]
public record DirectoryPickerOptions(
    [property: Description("@#id")]string? Id = default,
	[property: Description("@#startIn")]StartInDirectory? StartIn = default,
	[property: Description("@#mode")]FileSystemPermissionMode Mode = FileSystemPermissionMode.Read);

/// <summary>
/// FileSystemHandle
/// </summary>
[ECMAScript]
[Description("@#FileSystemHandle")]
public partial class FileSystemHandle
{
    /// <summary>
    /// queryPermission
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#queryPermission")]
    public extern PromiseResult<PermissionState> QueryPermission(FileSystemHandlePermissionDescriptor? descriptor = default);

    /// <summary>
    /// requestPermission
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#requestPermission")]
    public extern PromiseResult<PermissionState> RequestPermission(FileSystemHandlePermissionDescriptor? descriptor = default);

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern FileSystemHandleKind Kind { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// isSameEntry
    /// </summary>
    /// <param name="other">other</param>
    [Description("@#isSameEntry")]
    public extern PromiseResult<bool> IsSameEntry(FileSystemHandle other);
}