namespace ECMAScript;

/// <summary>
/// URLSearchParams
/// </summary>
[ECMAScript]
[Description("@#URLSearchParams")]
public class URLSearchParams : IEnumerable<(string, string)>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern URLSearchParams(Either<string[][], Dictionary<string, string>, string> init);

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }

    /// <summary>
    /// append
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#append")]
    public extern void Append(string name, string value);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#delete")]
    public extern void Delete(string name, string value);

    /// <summary>
    /// get
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#get")]
    public extern string? Get(string name);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getAll")]
    public extern string[] GetAll(string name);

    /// <summary>
    /// has
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#has")]
    public extern bool Has(string name, string value);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    [Description("@#set")]
    public extern void Set(string name, string value);

    /// <summary>
    /// sort
    /// </summary>
    [Description("@#sort")]
    public extern void Sort();

    extern IEnumerator<(string, string)> IEnumerable<(string, string)>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();
}