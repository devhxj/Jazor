namespace ECMAScript;

/// <summary>
/// FileSystemFlags
/// </summary>
[ECMAScript]
[Description("@#FileSystemFlags")]
public record FileSystemFlags(
    [property: Description("@#create")]bool Create = false,
	[property: Description("@#exclusive")]bool Exclusive = false);

/// <summary>
/// File
/// </summary>
[ECMAScript]
[Description("@#File")]
public partial class File(BlobPart[] blobParts, BlobPropertyBag options) : Blob(blobParts, options)
{
    /// <summary>
    /// webkitRelativePath
    /// </summary>
    [Description("@#webkitRelativePath")]
    public extern string WebkitRelativePath { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="fileBits">fileBits</param>
    /// <param name="fileName">fileName</param>
    /// <param name="options">options</param>
    public extern File(BlobPart[] fileBits, string fileName, FilePropertyBag options);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// lastModified
    /// </summary>
    [Description("@#lastModified")]
    public extern long LastModified { get; }
}

/// <summary>
/// HTMLInputElement
/// </summary>
[ECMAScript]
[Description("@#HTMLInputElement")]
public partial class HTMLInputElement : HTMLElement
{
    /// <summary>
    /// webkitdirectory
    /// </summary>
    [Description("@#webkitdirectory")]
    public extern bool Webkitdirectory { get; set; }

    /// <summary>
    /// webkitEntries
    /// </summary>
    [Description("@#webkitEntries")]
    public extern FrozenSet<FileSystemEntry> WebkitEntries { get; }

    /// <summary>
    /// capture
    /// </summary>
    [Description("@#capture")]
    public extern string Capture { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLInputElement();

    /// <summary>
    /// accept
    /// </summary>
    [Description("@#accept")]
    public extern string Accept { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern bool Alpha { get; set; }

    /// <summary>
    /// alt
    /// </summary>
    [Description("@#alt")]
    public extern string Alt { get; set; }

    /// <summary>
    /// autocomplete
    /// </summary>
    [Description("@#autocomplete")]
    public extern string Autocomplete { get; set; }

    /// <summary>
    /// defaultChecked
    /// </summary>
    [Description("@#defaultChecked")]
    public extern bool DefaultChecked { get; set; }

    /// <summary>
    /// checked
    /// </summary>
    [Description("@#checked")]
    public extern bool Checked { get; set; }

    /// <summary>
    /// colorSpace
    /// </summary>
    [Description("@#colorSpace")]
    public extern string ColorSpace { get; set; }

    /// <summary>
    /// dirName
    /// </summary>
    [Description("@#dirName")]
    public extern string DirName { get; set; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }

    /// <summary>
    /// form
    /// </summary>
    [Description("@#form")]
    public extern HTMLFormElement? Form { get; }

    /// <summary>
    /// files
    /// </summary>
    [Description("@#files")]
    public extern FileList? Files { get; set; }

    /// <summary>
    /// formAction
    /// </summary>
    [Description("@#formAction")]
    public extern string FormAction { get; set; }

    /// <summary>
    /// formEnctype
    /// </summary>
    [Description("@#formEnctype")]
    public extern string FormEnctype { get; set; }

    /// <summary>
    /// formMethod
    /// </summary>
    [Description("@#formMethod")]
    public extern string FormMethod { get; set; }

    /// <summary>
    /// formNoValidate
    /// </summary>
    [Description("@#formNoValidate")]
    public extern bool FormNoValidate { get; set; }

    /// <summary>
    /// formTarget
    /// </summary>
    [Description("@#formTarget")]
    public extern string FormTarget { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; set; }

    /// <summary>
    /// indeterminate
    /// </summary>
    [Description("@#indeterminate")]
    public extern bool Indeterminate { get; set; }

    /// <summary>
    /// list
    /// </summary>
    [Description("@#list")]
    public extern HTMLDataListElement? List { get; }

    /// <summary>
    /// max
    /// </summary>
    [Description("@#max")]
    public extern string Max { get; set; }

    /// <summary>
    /// maxLength
    /// </summary>
    [Description("@#maxLength")]
    public extern int MaxLength { get; set; }

    /// <summary>
    /// min
    /// </summary>
    [Description("@#min")]
    public extern string Min { get; set; }

    /// <summary>
    /// minLength
    /// </summary>
    [Description("@#minLength")]
    public extern int MinLength { get; set; }

    /// <summary>
    /// multiple
    /// </summary>
    [Description("@#multiple")]
    public extern bool Multiple { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// pattern
    /// </summary>
    [Description("@#pattern")]
    public extern string Pattern { get; set; }

    /// <summary>
    /// placeholder
    /// </summary>
    [Description("@#placeholder")]
    public extern string Placeholder { get; set; }

    /// <summary>
    /// readOnly
    /// </summary>
    [Description("@#readOnly")]
    public extern bool ReadOnly { get; set; }

    /// <summary>
    /// required
    /// </summary>
    [Description("@#required")]
    public extern bool Required { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; set; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// step
    /// </summary>
    [Description("@#step")]
    public extern string Step { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// defaultValue
    /// </summary>
    [Description("@#defaultValue")]
    public extern string DefaultValue { get; set; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }

    /// <summary>
    /// valueAsDate
    /// </summary>
    [Description("@#valueAsDate")]
    public extern object? ValueAsDate { get; set; }

    /// <summary>
    /// valueAsNumber
    /// </summary>
    [Description("@#valueAsNumber")]
    public extern double ValueAsNumber { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; set; }

    /// <summary>
    /// stepUp
    /// </summary>
    /// <param name="n">n</param>
    [Description("@#stepUp")]
    public extern void StepUp(int n = 1);

    /// <summary>
    /// stepDown
    /// </summary>
    /// <param name="n">n</param>
    [Description("@#stepDown")]
    public extern void StepDown(int n = 1);

    /// <summary>
    /// willValidate
    /// </summary>
    [Description("@#willValidate")]
    public extern bool WillValidate { get; }

    /// <summary>
    /// validity
    /// </summary>
    [Description("@#validity")]
    public extern ValidityState Validity { get; }

    /// <summary>
    /// validationMessage
    /// </summary>
    [Description("@#validationMessage")]
    public extern string ValidationMessage { get; }

    /// <summary>
    /// checkValidity
    /// </summary>
    [Description("@#checkValidity")]
    public extern bool CheckValidity();

    /// <summary>
    /// reportValidity
    /// </summary>
    [Description("@#reportValidity")]
    public extern bool ReportValidity();

    /// <summary>
    /// setCustomValidity
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#setCustomValidity")]
    public extern void SetCustomValidity(string error);

    /// <summary>
    /// labels
    /// </summary>
    [Description("@#labels")]
    public extern NodeList? Labels { get; }

    /// <summary>
    /// select
    /// </summary>
    [Description("@#select")]
    public extern void Select();

    /// <summary>
    /// selectionStart
    /// </summary>
    [Description("@#selectionStart")]
    public extern uint? SelectionStart { get; set; }

    /// <summary>
    /// selectionEnd
    /// </summary>
    [Description("@#selectionEnd")]
    public extern uint? SelectionEnd { get; set; }

    /// <summary>
    /// selectionDirection
    /// </summary>
    [Description("@#selectionDirection")]
    public extern string? SelectionDirection { get; set; }

    /// <summary>
    /// setRangeText
    /// </summary>
    /// <param name="replacement">replacement</param>
    [Description("@#setRangeText")]
    public extern void SetRangeText(string replacement);

    /// <summary>
    /// setRangeText
    /// </summary>
    /// <param name="replacement">replacement</param>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    /// <param name="selectionMode">selectionMode</param>
    [Description("@#setRangeText")]
    public extern void SetRangeText(string replacement, uint start, uint end, SelectionMode selectionMode = SelectionMode.Preserve);

    /// <summary>
    /// setSelectionRange
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    /// <param name="direction">direction</param>
    [Description("@#setSelectionRange")]
    public extern void SetSelectionRange(uint start, uint end, string direction);

    /// <summary>
    /// showPicker
    /// </summary>
    [Description("@#showPicker")]
    public extern void ShowPicker();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// useMap
    /// </summary>
    [Description("@#useMap")]
    public extern string UseMap { get; set; }

    #region mixin PopoverInvokerElement
    /// <summary>
    /// popoverTargetElement
    /// </summary>
    [Description("@#popoverTargetElement")]
    public extern Element? PopoverTargetElement { get; set; }

    /// <summary>
    /// popoverTargetAction
    /// </summary>
    [Description("@#popoverTargetAction")]
    public extern string PopoverTargetAction { get; set; }
    #endregion
}

/// <summary>
/// DataTransferItem
/// </summary>
[ECMAScript]
[Description("@#DataTransferItem")]
public partial class DataTransferItem
{
    /// <summary>
    /// webkitGetAsEntry
    /// </summary>
    [Description("@#webkitGetAsEntry")]
    public extern FileSystemEntry? WebkitGetAsEntry();

    /// <summary>
    /// getAsFileSystemHandle
    /// </summary>
    [Description("@#getAsFileSystemHandle")]
    public extern PromiseResult<FileSystemHandle?> GetAsFileSystemHandle();

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern string Kind { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// getAsString
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#getAsString")]
    public extern void GetAsString(FunctionStringCallback? callback);

    /// <summary>
    /// getAsFile
    /// </summary>
    [Description("@#getAsFile")]
    public extern File? GetAsFile();
}

/// <summary>
/// FileSystemEntry
/// </summary>
[ECMAScript]
[Description("@#FileSystemEntry")]
public class FileSystemEntry
{
    /// <summary>
    /// isFile
    /// </summary>
    [Description("@#isFile")]
    public extern bool IsFile { get; }

    /// <summary>
    /// isDirectory
    /// </summary>
    [Description("@#isDirectory")]
    public extern bool IsDirectory { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// fullPath
    /// </summary>
    [Description("@#fullPath")]
    public extern string FullPath { get; }

    /// <summary>
    /// filesystem
    /// </summary>
    [Description("@#filesystem")]
    public extern FileSystem Filesystem { get; }

    /// <summary>
    /// getParent
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#getParent")]
    public extern void GetParent(FileSystemEntryCallback successCallback, ErrorCallback errorCallback);
}

/// <summary>
/// FileSystemDirectoryEntry
/// </summary>
[ECMAScript]
[Description("@#FileSystemDirectoryEntry")]
public class FileSystemDirectoryEntry : FileSystemEntry
{
    /// <summary>
    /// createReader
    /// </summary>
    [Description("@#createReader")]
    public extern FileSystemDirectoryReader CreateReader();

    /// <summary>
    /// getFile
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="options">options</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#getFile")]
    public extern void GetFile(string? path, FileSystemFlags? options = default, FileSystemEntryCallback? successCallback = default, ErrorCallback? errorCallback = default);

    /// <summary>
    /// getDirectory
    /// </summary>
    /// <param name="path">path</param>
    /// <param name="options">options</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#getDirectory")]
    public extern void GetDirectory(string? path, FileSystemFlags? options = default, FileSystemEntryCallback? successCallback = default, ErrorCallback? errorCallback = default);
}

/// <summary>
/// FileSystemDirectoryReader
/// </summary>
[ECMAScript]
[Description("@#FileSystemDirectoryReader")]
public class FileSystemDirectoryReader
{
    /// <summary>
    /// readEntries
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#readEntries")]
    public extern void ReadEntries(FileSystemEntriesCallback successCallback, ErrorCallback errorCallback);
}

/// <summary>
/// FileSystemFileEntry
/// </summary>
[ECMAScript]
[Description("@#FileSystemFileEntry")]
public class FileSystemFileEntry : FileSystemEntry
{
    /// <summary>
    /// file
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="errorCallback">errorCallback</param>
    [Description("@#file")]
    public extern void File(FileCallback successCallback, ErrorCallback errorCallback);
}

/// <summary>
/// FileSystem
/// </summary>
[ECMAScript]
[Description("@#FileSystem")]
public class FileSystem
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// root
    /// </summary>
    [Description("@#root")]
    public extern FileSystemDirectoryEntry Root { get; }
}