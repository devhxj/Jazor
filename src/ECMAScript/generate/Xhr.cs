namespace ECMAScript;

/// <summary>
/// ProgressEventInit
/// </summary>
[ECMAScript]
[Description("@#ProgressEventInit")]
public record ProgressEventInit(
    [property: Description("@#lengthComputable")]bool LengthComputable = false,
	[property: Description("@#loaded")]double Loaded = 0d,
	[property: Description("@#total")]double Total = 0d): EventInit;

/// <summary>
/// XMLHttpRequestEventTarget
/// </summary>
[ECMAScript]
[Description("@#XMLHttpRequestEventTarget")]
public class XMLHttpRequestEventTarget : EventTarget
{
    /// <summary>
    /// onloadstart
    /// </summary>
    [Description("@#onloadstart")]
    public extern EventHandler Onloadstart { get; set; }

    /// <summary>
    /// onprogress
    /// </summary>
    [Description("@#onprogress")]
    public extern EventHandler Onprogress { get; set; }

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onload
    /// </summary>
    [Description("@#onload")]
    public extern EventHandler Onload { get; set; }

    /// <summary>
    /// ontimeout
    /// </summary>
    [Description("@#ontimeout")]
    public extern EventHandler Ontimeout { get; set; }

    /// <summary>
    /// onloadend
    /// </summary>
    [Description("@#onloadend")]
    public extern EventHandler Onloadend { get; set; }
}

/// <summary>
/// XMLHttpRequestUpload
/// </summary>
[ECMAScript]
[Description("@#XMLHttpRequestUpload")]
public class XMLHttpRequestUpload : XMLHttpRequestEventTarget
{

}

/// <summary>
/// FormData
/// </summary>
[ECMAScript]
[Description("@#FormData")]
public class FormData : IEnumerable<(string, FormDataEntryValue)>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="form">form</param>
    /// <param name="submitter">submitter</param>
    public extern FormData(HTMLFormElement form, HTMLElement? submitter);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#append")]
    public extern void Append(string name, string value);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="blobValue">blobValue</param>
    /// <param name="filename">filename</param>
    [Description("@#append")]
    public extern void Append(string name, Blob blobValue, string filename);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#delete")]
    public extern void Delete(string name);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#get")]
    public extern FormDataEntryValue? Get(string name);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getAll")]
    public extern FormDataEntryValue[] GetAll(string name);

    /// <summary>
    /// has
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#has")]
    public extern bool Has(string name);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#set")]
    public extern void Set(string name, string value);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="blobValue">blobValue</param>
    /// <param name="filename">filename</param>
    [Description("@#set")]
    public extern void Set(string name, Blob blobValue, string filename);

    extern IEnumerator<(string, FormDataEntryValue)> IEnumerable<(string, FormDataEntryValue)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// ProgressEvent
/// </summary>
[ECMAScript]
[Description("@#ProgressEvent")]
public class ProgressEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ProgressEvent(string type, ProgressEventInit eventInitDict);

    /// <summary>
    /// lengthComputable
    /// </summary>
    [Description("@#lengthComputable")]
    public extern bool LengthComputable { get; }

    /// <summary>
    /// loaded
    /// </summary>
    [Description("@#loaded")]
    public extern double Loaded { get; }

    /// <summary>
    /// total
    /// </summary>
    [Description("@#total")]
    public extern double Total { get; }
}