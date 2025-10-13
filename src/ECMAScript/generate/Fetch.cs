namespace ECMAScript;

/// <summary>
/// ResponseInit
/// </summary>
[ECMAScript]
[Description("@#ResponseInit")]
public record ResponseInit(
    [property: Description("@#status")]ushort Status = 200,
	[property: Description("@#statusText")]byte[]? StatusText = default,
	[property: Description("@#headers")]HeadersInit? Headers = default);

/// <summary>
/// Headers
/// </summary>
[ECMAScript]
[Description("@#Headers")]
public class Headers : IEnumerable<(byte[], byte[])>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern Headers(HeadersInit init);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#append")]
    public extern void Append(byte[] name, byte[] value);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#delete")]
    public extern void Delete(byte[] name);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#get")]
    public extern byte[]? Get(byte[] name);

    /// <summary>
    /// getSetCookie
    /// </summary>
    [Description("@#getSetCookie")]
    public extern byte[][] GetSetCookie();

    /// <summary>
    /// has
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#has")]
    public extern bool Has(byte[] name);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#set")]
    public extern void Set(byte[] name, byte[] value);

    extern IEnumerator<(byte[], byte[])> IEnumerable<(byte[], byte[])>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}

/// <summary>
/// Request
/// </summary>
[ECMAScript]
[Description("@#Request")]
public partial class Request
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="init">init</param>
    public extern Request(RequestInfo input, RequestInit init);

    /// <summary>
    /// method
    /// </summary>
    [Description("@#method")]
    public extern byte[] Method { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// headers
    /// </summary>
    [Description("@#headers")]
    public extern Headers Headers { get; }

    /// <summary>
    /// destination
    /// </summary>
    [Description("@#destination")]
    public extern RequestDestination Destination { get; }

    /// <summary>
    /// referrer
    /// </summary>
    [Description("@#referrer")]
    public extern string Referrer { get; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern ReferrerPolicy ReferrerPolicy { get; }

    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern RequestMode Mode { get; }

    /// <summary>
    /// credentials
    /// </summary>
    [Description("@#credentials")]
    public extern RequestCredentials Credentials { get; }

    /// <summary>
    /// cache
    /// </summary>
    [Description("@#cache")]
    public extern RequestCache Cache { get; }

    /// <summary>
    /// redirect
    /// </summary>
    [Description("@#redirect")]
    public extern RequestRedirect Redirect { get; }

    /// <summary>
    /// integrity
    /// </summary>
    [Description("@#integrity")]
    public extern string Integrity { get; }

    /// <summary>
    /// keepalive
    /// </summary>
    [Description("@#keepalive")]
    public extern bool Keepalive { get; }

    /// <summary>
    /// isReloadNavigation
    /// </summary>
    [Description("@#isReloadNavigation")]
    public extern bool IsReloadNavigation { get; }

    /// <summary>
    /// isHistoryNavigation
    /// </summary>
    [Description("@#isHistoryNavigation")]
    public extern bool IsHistoryNavigation { get; }

    /// <summary>
    /// signal
    /// </summary>
    [Description("@#signal")]
    public extern AbortSignal Signal { get; }

    /// <summary>
    /// duplex
    /// </summary>
    [Description("@#duplex")]
    public extern RequestDuplex Duplex { get; }

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern Request Clone();

    /// <summary>
    /// targetAddressSpace
    /// </summary>
    [Description("@#targetAddressSpace")]
    public extern IPAddressSpace TargetAddressSpace { get; }

    #region mixin Body
    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern ReadableStream? Body_ { get; }

    /// <summary>
    /// bodyUsed
    /// </summary>
    [Description("@#bodyUsed")]
    public extern bool BodyUsed { get; }

    /// <summary>
    /// arrayBuffer
    /// </summary>
    [Description("@#arrayBuffer")]
    public extern PromiseResult<ArrayBuffer> ArrayBuffer();

    /// <summary>
    /// blob
    /// </summary>
    [Description("@#blob")]
    public extern PromiseResult<Blob> Blob();

    /// <summary>
    /// bytes
    /// </summary>
    [Description("@#bytes")]
    public extern PromiseResult<Uint8Array> Bytes();

    /// <summary>
    /// formData
    /// </summary>
    [Description("@#formData")]
    public extern PromiseResult<FormData> FormData();

    /// <summary>
    /// json
    /// </summary>
    [Description("@#json")]
    public extern PromiseResult<object> Json();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern PromiseResult<string> Text();
    #endregion
}

/// <summary>
/// Response
/// </summary>
[ECMAScript]
[Description("@#Response")]
public class Response
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="body">body</param>
    /// <param name="init">init</param>
    public extern Response(BodyInit? body, ResponseInit init);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern static Response Error();

    /// <summary>
    /// redirect
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="status">status</param>
    [Description("@#redirect")]
    public extern static Response Redirect(string url, ushort status = 302);

    /// <summary>
    /// json
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="init">init</param>
    [Description("@#json")]
    public extern static Response Json(object data, ResponseInit? init = default);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ResponseType Type { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// redirected
    /// </summary>
    [Description("@#redirected")]
    public extern bool Redirected { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern ushort Status { get; }

    /// <summary>
    /// ok
    /// </summary>
    [Description("@#ok")]
    public extern bool Ok { get; }

    /// <summary>
    /// statusText
    /// </summary>
    [Description("@#statusText")]
    public extern byte[] StatusText { get; }

    /// <summary>
    /// headers
    /// </summary>
    [Description("@#headers")]
    public extern Headers Headers { get; }

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern Response Clone();

    #region mixin Body
    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern ReadableStream? Body_ { get; }

    /// <summary>
    /// bodyUsed
    /// </summary>
    [Description("@#bodyUsed")]
    public extern bool BodyUsed { get; }

    /// <summary>
    /// arrayBuffer
    /// </summary>
    [Description("@#arrayBuffer")]
    public extern PromiseResult<ArrayBuffer> ArrayBuffer();

    /// <summary>
    /// blob
    /// </summary>
    [Description("@#blob")]
    public extern PromiseResult<Blob> Blob();

    /// <summary>
    /// bytes
    /// </summary>
    [Description("@#bytes")]
    public extern PromiseResult<Uint8Array> Bytes();

    /// <summary>
    /// formData
    /// </summary>
    [Description("@#formData")]
    public extern PromiseResult<FormData> FormData();

    /// <summary>
    /// json
    /// </summary>
    [Description("@#json")]
    public extern PromiseResult<object> Json();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern PromiseResult<string> Text();
    #endregion
}