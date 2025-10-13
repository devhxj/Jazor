namespace ECMAScript;

/// <summary>
/// BluetoothLEScanOptions
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanOptions")]
public record BluetoothLEScanOptions(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
	[property: Description("@#keepRepeatedDevices")]bool KeepRepeatedDevices = false,
	[property: Description("@#acceptAllAdvertisements")]bool AcceptAllAdvertisements = false);

/// <summary>
/// BluetoothLEScanPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanPermissionDescriptor")]
public record BluetoothLEScanPermissionDescriptor(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
	[property: Description("@#keepRepeatedDevices")]bool KeepRepeatedDevices = false,
	[property: Description("@#acceptAllAdvertisements")]bool AcceptAllAdvertisements = false): PermissionDescriptor;

/// <summary>
/// Bluetooth
/// </summary>
[ECMAScript]
[Description("@#Bluetooth")]
public partial class Bluetooth : EventTarget
{
    /// <summary>
    /// requestLEScan
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestLEScan")]
    public extern PromiseResult<BluetoothLEScan> RequestLEScan(BluetoothLEScanOptions? options = default);

    /// <summary>
    /// getAvailability
    /// </summary>
    [Description("@#getAvailability")]
    public extern PromiseResult<bool> GetAvailability();

    /// <summary>
    /// onavailabilitychanged
    /// </summary>
    [Description("@#onavailabilitychanged")]
    public extern EventHandler Onavailabilitychanged { get; set; }

    /// <summary>
    /// referringDevice
    /// </summary>
    [Description("@#referringDevice")]
    public extern BluetoothDevice? ReferringDevice { get; }

    /// <summary>
    /// getDevices
    /// </summary>
    [Description("@#getDevices")]
    public extern PromiseResult<BluetoothDevice[]> GetDevices();

    /// <summary>
    /// requestDevice
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestDevice")]
    public extern PromiseResult<BluetoothDevice> RequestDevice(RequestDeviceOptions? options = default);

    #region mixin BluetoothDeviceEventHandlers
    /// <summary>
    /// onadvertisementreceived
    /// </summary>
    [Description("@#onadvertisementreceived")]
    public extern EventHandler Onadvertisementreceived { get; set; }

    /// <summary>
    /// ongattserverdisconnected
    /// </summary>
    [Description("@#ongattserverdisconnected")]
    public extern EventHandler Ongattserverdisconnected { get; set; }
    #endregion

    #region mixin CharacteristicEventHandlers
    /// <summary>
    /// oncharacteristicvaluechanged
    /// </summary>
    [Description("@#oncharacteristicvaluechanged")]
    public extern EventHandler Oncharacteristicvaluechanged { get; set; }
    #endregion

    #region mixin ServiceEventHandlers
    /// <summary>
    /// onserviceadded
    /// </summary>
    [Description("@#onserviceadded")]
    public extern EventHandler Onserviceadded { get; set; }

    /// <summary>
    /// onservicechanged
    /// </summary>
    [Description("@#onservicechanged")]
    public extern EventHandler Onservicechanged { get; set; }

    /// <summary>
    /// onserviceremoved
    /// </summary>
    [Description("@#onserviceremoved")]
    public extern EventHandler Onserviceremoved { get; set; }
    #endregion
}

/// <summary>
/// BluetoothDataFilter
/// </summary>
[ECMAScript]
[Description("@#BluetoothDataFilter")]
public class BluetoothDataFilter
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern BluetoothDataFilter(BluetoothDataFilterInit init);

    /// <summary>
    /// dataPrefix
    /// </summary>
    [Description("@#dataPrefix")]
    public extern ArrayBuffer DataPrefix { get; }

    /// <summary>
    /// mask
    /// </summary>
    [Description("@#mask")]
    public extern ArrayBuffer Mask { get; }
}

/// <summary>
/// BluetoothManufacturerDataFilter
/// </summary>
[ECMAScript]
[Description("@#BluetoothManufacturerDataFilter")]
public class BluetoothManufacturerDataFilter : IDictionary<ushort, BluetoothDataFilter>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern BluetoothManufacturerDataFilter(object init);

    #region Dictionary
    extern BluetoothDataFilter IDictionary<ushort, BluetoothDataFilter>.this[ushort key] { get; set; }
    extern ICollection<ushort> IDictionary<ushort, BluetoothDataFilter>.Keys { get; }
    extern ICollection<BluetoothDataFilter> IDictionary<ushort, BluetoothDataFilter>.Values { get; }
    extern int ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.Count { get; }
    extern bool ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.IsReadOnly { get; }
    extern void IDictionary<ushort, BluetoothDataFilter>.Add(ushort key, BluetoothDataFilter value);
    extern void ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.Add(KeyValuePair<ushort, BluetoothDataFilter> item);
    extern void ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.Clear();
    extern bool ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.Contains(KeyValuePair<ushort, BluetoothDataFilter> item);
    extern bool IDictionary<ushort, BluetoothDataFilter>.ContainsKey(ushort key);
    extern void ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.CopyTo(KeyValuePair<ushort, BluetoothDataFilter>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<ushort, BluetoothDataFilter>> IEnumerable<KeyValuePair<ushort, BluetoothDataFilter>>.GetEnumerator();
    extern bool IDictionary<ushort, BluetoothDataFilter>.Remove(ushort key);
    extern bool ICollection<KeyValuePair<ushort, BluetoothDataFilter>>.Remove(KeyValuePair<ushort, BluetoothDataFilter> item);
    extern bool IDictionary<ushort, BluetoothDataFilter>.TryGetValue(ushort key, [MaybeNullWhen(false)] out BluetoothDataFilter value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// BluetoothServiceDataFilter
/// </summary>
[ECMAScript]
[Description("@#BluetoothServiceDataFilter")]
public class BluetoothServiceDataFilter : IDictionary<UUID, BluetoothDataFilter>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern BluetoothServiceDataFilter(object init);

    #region Dictionary
    extern BluetoothDataFilter IDictionary<UUID, BluetoothDataFilter>.this[UUID key] { get; set; }
    extern ICollection<UUID> IDictionary<UUID, BluetoothDataFilter>.Keys { get; }
    extern ICollection<BluetoothDataFilter> IDictionary<UUID, BluetoothDataFilter>.Values { get; }
    extern int ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.Count { get; }
    extern bool ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.IsReadOnly { get; }
    extern void IDictionary<UUID, BluetoothDataFilter>.Add(UUID key, BluetoothDataFilter value);
    extern void ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.Add(KeyValuePair<UUID, BluetoothDataFilter> item);
    extern void ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.Clear();
    extern bool ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.Contains(KeyValuePair<UUID, BluetoothDataFilter> item);
    extern bool IDictionary<UUID, BluetoothDataFilter>.ContainsKey(UUID key);
    extern void ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.CopyTo(KeyValuePair<UUID, BluetoothDataFilter>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<UUID, BluetoothDataFilter>> IEnumerable<KeyValuePair<UUID, BluetoothDataFilter>>.GetEnumerator();
    extern bool IDictionary<UUID, BluetoothDataFilter>.Remove(UUID key);
    extern bool ICollection<KeyValuePair<UUID, BluetoothDataFilter>>.Remove(KeyValuePair<UUID, BluetoothDataFilter> item);
    extern bool IDictionary<UUID, BluetoothDataFilter>.TryGetValue(UUID key, [MaybeNullWhen(false)] out BluetoothDataFilter value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// BluetoothLEScanFilter
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanFilter")]
public class BluetoothLEScanFilter
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern BluetoothLEScanFilter(BluetoothLEScanFilterInit init);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string? Name { get; }

    /// <summary>
    /// namePrefix
    /// </summary>
    [Description("@#namePrefix")]
    public extern string? NamePrefix { get; }

    /// <summary>
    /// services
    /// </summary>
    [Description("@#services")]
    public extern FrozenSet<UUID> Services { get; }

    /// <summary>
    /// manufacturerData
    /// </summary>
    [Description("@#manufacturerData")]
    public extern BluetoothManufacturerDataFilter ManufacturerData { get; }

    /// <summary>
    /// serviceData
    /// </summary>
    [Description("@#serviceData")]
    public extern BluetoothServiceDataFilter ServiceData { get; }
}

/// <summary>
/// BluetoothLEScan
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScan")]
public class BluetoothLEScan
{
    /// <summary>
    /// filters
    /// </summary>
    [Description("@#filters")]
    public extern FrozenSet<BluetoothLEScanFilter> Filters { get; }

    /// <summary>
    /// keepRepeatedDevices
    /// </summary>
    [Description("@#keepRepeatedDevices")]
    public extern bool KeepRepeatedDevices { get; }

    /// <summary>
    /// acceptAllAdvertisements
    /// </summary>
    [Description("@#acceptAllAdvertisements")]
    public extern bool AcceptAllAdvertisements { get; }

    /// <summary>
    /// active
    /// </summary>
    [Description("@#active")]
    public extern bool Active { get; }

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// BluetoothLEScanPermissionResult
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanPermissionResult")]
public class BluetoothLEScanPermissionResult : PermissionStatus
{
    /// <summary>
    /// scans
    /// </summary>
    [Description("@#scans")]
    public extern FrozenSet<BluetoothLEScan> Scans { get; set; }
}