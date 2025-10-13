namespace ECMAScript;

/// <summary>
/// BlobPropertyBag
/// </summary>
[ECMAScript]
[Description("@#BlobPropertyBag")]
public record BlobPropertyBag(
    [property: Description("@#type")]string? Type = default,
	[property: Description("@#endings")]EndingType Endings = EndingType.Transparent);

/// <summary>
/// FilePropertyBag
/// </summary>
[ECMAScript]
[Description("@#FilePropertyBag")]
public record FilePropertyBag(
    [property: Description("@#lastModified")]long LastModified = default): BlobPropertyBag;

/// <summary>
/// Blob
/// </summary>
[ECMAScript]
[Description("@#Blob")]
public class Blob
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="blobParts">blobParts</param>
    /// <param name="options">options</param>
    public extern Blob(BlobPart[] blobParts, BlobPropertyBag options);

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern ulong Size { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// slice
    /// </summary>
    /// <param name="start">start</param>
    /// <param name="end">end</param>
    /// <param name="contentType">contentType</param>
    [Description("@#slice")]
    public extern Blob Slice(long start, long end, string contentType);

    /// <summary>
    /// stream
    /// </summary>
    [Description("@#stream")]
    public extern ReadableStream Stream();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern PromiseResult<string> Text();

    /// <summary>
    /// arrayBuffer
    /// </summary>
    [Description("@#arrayBuffer")]
    public extern PromiseResult<ArrayBuffer> ArrayBuffer();

    /// <summary>
    /// bytes
    /// </summary>
    [Description("@#bytes")]
    public extern PromiseResult<Uint8Array> Bytes();
}

/// <summary>
/// FileList
/// </summary>
[ECMAScript]
[Description("@#FileList")]
public class FileList
{
    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern File? GetItem(uint index);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// FileReader
/// </summary>
[ECMAScript]
[Description("@#FileReader")]
public class FileReader : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern FileReader();

    /// <summary>
    /// readAsArrayBuffer
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsArrayBuffer")]
    public extern void ReadAsArrayBuffer(Blob blob);

    /// <summary>
    /// readAsBinaryString
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsBinaryString")]
    public extern void ReadAsBinaryString(Blob blob);

    /// <summary>
    /// readAsText
    /// </summary>
    /// <param name="blob">blob</param>
    /// <param name="encoding">encoding</param>
    [Description("@#readAsText")]
    public extern void ReadAsText(Blob blob, string encoding);

    /// <summary>
    /// readAsDataURL
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsDataURL")]
    public extern void ReadAsDataURL(Blob blob);

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern void Abort();

    /// <summary>
    /// EMPTY
    /// </summary>
    [Description("@#EMPTY")]
    public const ushort EMPTY = 0;

    /// <summary>
    /// LOADING
    /// </summary>
    [Description("@#LOADING")]
    public const ushort LOADING = 1;

    /// <summary>
    /// DONE
    /// </summary>
    [Description("@#DONE")]
    public const ushort DONE = 2;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern Either<string, ArrayBuffer>? Result { get; }

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern DOMException? Error { get; }

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
    /// onload
    /// </summary>
    [Description("@#onload")]
    public extern EventHandler Onload { get; set; }

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
    /// onloadend
    /// </summary>
    [Description("@#onloadend")]
    public extern EventHandler Onloadend { get; set; }
}

/// <summary>
/// FileReaderSync
/// </summary>
[ECMAScript]
[Description("@#FileReaderSync")]
public class FileReaderSync
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern FileReaderSync();

    /// <summary>
    /// readAsArrayBuffer
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsArrayBuffer")]
    public extern ArrayBuffer ReadAsArrayBuffer(Blob blob);

    /// <summary>
    /// readAsBinaryString
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsBinaryString")]
    public extern string ReadAsBinaryString(Blob blob);

    /// <summary>
    /// readAsText
    /// </summary>
    /// <param name="blob">blob</param>
    /// <param name="encoding">encoding</param>
    [Description("@#readAsText")]
    public extern string ReadAsText(Blob blob, string encoding);

    /// <summary>
    /// readAsDataURL
    /// </summary>
    /// <param name="blob">blob</param>
    [Description("@#readAsDataURL")]
    public extern string ReadAsDataURL(Blob blob);
}

/// <summary>
/// URL
/// </summary>
[ECMAScript]
[Description("@#URL")]
public partial class URL
{
    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern static string CreateObjectURL(Either<Blob, MediaSource> obj);

    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern static string CreateObjectURL(Blob obj);

    /// <summary>
    /// createObjectURL
    /// </summary>
    /// <param name="obj">obj</param>
    [Description("@#createObjectURL")]
    public extern static string CreateObjectURL(MediaSource obj);

    /// <summary>
    /// revokeObjectURL
    /// </summary>
    /// <param name="url">url</param>
    [Description("@#revokeObjectURL")]
    public extern static void RevokeObjectURL(string url);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="base">base</param>
    public extern URL(string url, string @base);

    /// <summary>
    /// parse
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="base">base</param>
    [Description("@#parse")]
    public extern static URL? Parse(string url, string @base);

    /// <summary>
    /// canParse
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="base">base</param>
    [Description("@#canParse")]
    public extern static bool CanParse(string url, string @base);

    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; set; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; set; }

    /// <summary>
    /// username
    /// </summary>
    [Description("@#username")]
    public extern string Username { get; set; }

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; set; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; set; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; set; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; set; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; set; }

    /// <summary>
    /// searchParams
    /// </summary>
    [Description("@#searchParams")]
    public extern URLSearchParams SearchParams { get; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; set; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern string ToJSON();
}