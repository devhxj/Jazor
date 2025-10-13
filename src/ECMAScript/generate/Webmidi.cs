namespace ECMAScript;

/// <summary>
/// MidiPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#MidiPermissionDescriptor")]
public record MidiPermissionDescriptor(
    [property: Description("@#sysex")]bool Sysex = false): PermissionDescriptor;

/// <summary>
/// MIDIOptions
/// </summary>
[ECMAScript]
[Description("@#MIDIOptions")]
public record MIDIOptions(
    [property: Description("@#sysex")]bool Sysex = default,
	[property: Description("@#software")]bool Software = default);

/// <summary>
/// MIDIMessageEventInit
/// </summary>
[ECMAScript]
[Description("@#MIDIMessageEventInit")]
public record MIDIMessageEventInit(
    [property: Description("@#data")]Uint8Array? Data = default): EventInit;

/// <summary>
/// MIDIConnectionEventInit
/// </summary>
[ECMAScript]
[Description("@#MIDIConnectionEventInit")]
public record MIDIConnectionEventInit(
    [property: Description("@#port")]MIDIPort? Port = default): EventInit;

/// <summary>
/// MIDIInputMap
/// </summary>
[ECMAScript]
[Description("@#MIDIInputMap")]
public class MIDIInputMap : IDictionary<string, MIDIInput>
{
    #region Dictionary
    extern MIDIInput IDictionary<string, MIDIInput>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, MIDIInput>.Keys { get; }
    extern ICollection<MIDIInput> IDictionary<string, MIDIInput>.Values { get; }
    extern int ICollection<KeyValuePair<string, MIDIInput>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, MIDIInput>>.IsReadOnly { get; }
    extern void IDictionary<string, MIDIInput>.Add(string key, MIDIInput value);
    extern void ICollection<KeyValuePair<string, MIDIInput>>.Add(KeyValuePair<string, MIDIInput> item);
    extern void ICollection<KeyValuePair<string, MIDIInput>>.Clear();
    extern bool ICollection<KeyValuePair<string, MIDIInput>>.Contains(KeyValuePair<string, MIDIInput> item);
    extern bool IDictionary<string, MIDIInput>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, MIDIInput>>.CopyTo(KeyValuePair<string, MIDIInput>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, MIDIInput>> IEnumerable<KeyValuePair<string, MIDIInput>>.GetEnumerator();
    extern bool IDictionary<string, MIDIInput>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, MIDIInput>>.Remove(KeyValuePair<string, MIDIInput> item);
    extern bool IDictionary<string, MIDIInput>.TryGetValue(string key, [MaybeNullWhen(false)] out MIDIInput value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// MIDIOutputMap
/// </summary>
[ECMAScript]
[Description("@#MIDIOutputMap")]
public class MIDIOutputMap : IDictionary<string, MIDIOutput>
{
    #region Dictionary
    extern MIDIOutput IDictionary<string, MIDIOutput>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, MIDIOutput>.Keys { get; }
    extern ICollection<MIDIOutput> IDictionary<string, MIDIOutput>.Values { get; }
    extern int ICollection<KeyValuePair<string, MIDIOutput>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, MIDIOutput>>.IsReadOnly { get; }
    extern void IDictionary<string, MIDIOutput>.Add(string key, MIDIOutput value);
    extern void ICollection<KeyValuePair<string, MIDIOutput>>.Add(KeyValuePair<string, MIDIOutput> item);
    extern void ICollection<KeyValuePair<string, MIDIOutput>>.Clear();
    extern bool ICollection<KeyValuePair<string, MIDIOutput>>.Contains(KeyValuePair<string, MIDIOutput> item);
    extern bool IDictionary<string, MIDIOutput>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, MIDIOutput>>.CopyTo(KeyValuePair<string, MIDIOutput>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, MIDIOutput>> IEnumerable<KeyValuePair<string, MIDIOutput>>.GetEnumerator();
    extern bool IDictionary<string, MIDIOutput>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, MIDIOutput>>.Remove(KeyValuePair<string, MIDIOutput> item);
    extern bool IDictionary<string, MIDIOutput>.TryGetValue(string key, [MaybeNullWhen(false)] out MIDIOutput value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// MIDIAccess
/// </summary>
[ECMAScript]
[Description("@#MIDIAccess")]
public class MIDIAccess : EventTarget
{
    /// <summary>
    /// inputs
    /// </summary>
    [Description("@#inputs")]
    public extern MIDIInputMap Inputs { get; }

    /// <summary>
    /// outputs
    /// </summary>
    [Description("@#outputs")]
    public extern MIDIOutputMap Outputs { get; }

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    /// <summary>
    /// sysexEnabled
    /// </summary>
    [Description("@#sysexEnabled")]
    public extern bool SysexEnabled { get; }
}

/// <summary>
/// MIDIPort
/// </summary>
[ECMAScript]
[Description("@#MIDIPort")]
public class MIDIPort : EventTarget
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// manufacturer
    /// </summary>
    [Description("@#manufacturer")]
    public extern string? Manufacturer { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string? Name { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern MIDIPortType Type { get; }

    /// <summary>
    /// version
    /// </summary>
    [Description("@#version")]
    public extern string? Version { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern MIDIPortDeviceState State { get; }

    /// <summary>
    /// connection
    /// </summary>
    [Description("@#connection")]
    public extern MIDIPortConnectionState Connection { get; }

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    /// <summary>
    /// open
    /// </summary>
    [Description("@#open")]
    public extern PromiseResult<MIDIPort> Open();

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern PromiseResult<MIDIPort> Close();
}

/// <summary>
/// MIDIInput
/// </summary>
[ECMAScript]
[Description("@#MIDIInput")]
public class MIDIInput : MIDIPort
{
    /// <summary>
    /// onmidimessage
    /// </summary>
    [Description("@#onmidimessage")]
    public extern EventHandler Onmidimessage { get; set; }
}

/// <summary>
/// MIDIOutput
/// </summary>
[ECMAScript]
[Description("@#MIDIOutput")]
public class MIDIOutput : MIDIPort
{
    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    /// <param name="timestamp">timestamp</param>
    [Description("@#send")]
    public extern void Send(byte[] data, double timestamp = 0d);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// MIDIMessageEvent
/// </summary>
[ECMAScript]
[Description("@#MIDIMessageEvent")]
public class MIDIMessageEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MIDIMessageEvent(string type, MIDIMessageEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern Uint8Array? Data { get; }
}

/// <summary>
/// MIDIConnectionEvent
/// </summary>
[ECMAScript]
[Description("@#MIDIConnectionEvent")]
public class MIDIConnectionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MIDIConnectionEvent(string type, MIDIConnectionEventInit eventInitDict);

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern MIDIPort? Port { get; }
}