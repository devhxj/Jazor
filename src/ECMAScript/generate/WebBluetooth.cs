namespace ECMAScript;

/// <summary>
/// BluetoothDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothDataFilterInit")]
public record BluetoothDataFilterInit(
    [property: Description("@#dataPrefix")]IBufferSource? DataPrefix = default,
	[property: Description("@#mask")]IBufferSource? Mask = default);

/// <summary>
/// BluetoothManufacturerDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothManufacturerDataFilterInit")]
public record BluetoothManufacturerDataFilterInit(
    [property: Description("@#companyIdentifier")]ushort CompanyIdentifier = default): BluetoothDataFilterInit;

/// <summary>
/// BluetoothServiceDataFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothServiceDataFilterInit")]
public record BluetoothServiceDataFilterInit(
    [property: Description("@#service")]BluetoothServiceUUID? Service = default): BluetoothDataFilterInit;

/// <summary>
/// BluetoothLEScanFilterInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothLEScanFilterInit")]
public record BluetoothLEScanFilterInit(
    [property: Description("@#services")]BluetoothServiceUUID[]? Services = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#namePrefix")]string? NamePrefix = default,
	[property: Description("@#manufacturerData")]BluetoothManufacturerDataFilterInit[]? ManufacturerData = default,
	[property: Description("@#serviceData")]BluetoothServiceDataFilterInit[]? ServiceData = default);

/// <summary>
/// RequestDeviceOptions
/// </summary>
[ECMAScript]
[Description("@#RequestDeviceOptions")]
public record RequestDeviceOptions(
    [property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
	[property: Description("@#exclusionFilters")]BluetoothLEScanFilterInit[]? ExclusionFilters = default,
	[property: Description("@#optionalServices")]BluetoothServiceUUID[]? OptionalServices = default,
	[property: Description("@#optionalManufacturerData")]ushort[]? OptionalManufacturerData = default,
	[property: Description("@#acceptAllDevices")]bool AcceptAllDevices = false);

/// <summary>
/// BluetoothPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#BluetoothPermissionDescriptor")]
public record BluetoothPermissionDescriptor(
    [property: Description("@#deviceId")]string? DeviceId = default,
	[property: Description("@#filters")]BluetoothLEScanFilterInit[]? Filters = default,
	[property: Description("@#optionalServices")]BluetoothServiceUUID[]? OptionalServices = default,
	[property: Description("@#optionalManufacturerData")]ushort[]? OptionalManufacturerData = default,
	[property: Description("@#acceptAllDevices")]bool AcceptAllDevices = false): PermissionDescriptor;

/// <summary>
/// AllowedBluetoothDevice
/// </summary>
[ECMAScript]
[Description("@#AllowedBluetoothDevice")]
public record AllowedBluetoothDevice(
    [property: Description("@#deviceId")]string? DeviceId = default,
	[property: Description("@#mayUseGATT")]bool MayUseGATT = default,
	[property: Description("@#allowedServices")]Either<string, UUID[]>? AllowedServices = default,
	[property: Description("@#allowedManufacturerData")]ushort[]? AllowedManufacturerData = default);

/// <summary>
/// BluetoothPermissionStorage
/// </summary>
[ECMAScript]
[Description("@#BluetoothPermissionStorage")]
public record BluetoothPermissionStorage(
    [property: Description("@#allowedDevices")]AllowedBluetoothDevice[]? AllowedDevices = default);

/// <summary>
/// ValueEventInit
/// </summary>
[ECMAScript]
[Description("@#ValueEventInit")]
public record ValueEventInit(
    [property: Description("@#value")]object? Value = default): EventInit;

/// <summary>
/// WatchAdvertisementsOptions
/// </summary>
[ECMAScript]
[Description("@#WatchAdvertisementsOptions")]
public record WatchAdvertisementsOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// BluetoothAdvertisingEventInit
/// </summary>
[ECMAScript]
[Description("@#BluetoothAdvertisingEventInit")]
public record BluetoothAdvertisingEventInit(
    [property: Description("@#device")]BluetoothDevice? Device = default,
	[property: Description("@#uuids")]Either<string, uint>[]? UUIDs = default,
	[property: Description("@#name")]string? Name = default,
	[property: Description("@#appearance")]ushort Appearance = default,
	[property: Description("@#txPower")]sbyte TxPower = default,
	[property: Description("@#rssi")]sbyte Rssi = default,
	[property: Description("@#manufacturerData")]BluetoothManufacturerDataMap? ManufacturerData = default,
	[property: Description("@#serviceData")]BluetoothServiceDataMap? ServiceData = default): EventInit;

/// <summary>
/// BluetoothPermissionResult
/// </summary>
[ECMAScript]
[Description("@#BluetoothPermissionResult")]
public class BluetoothPermissionResult : PermissionStatus
{
    /// <summary>
    /// devices
    /// </summary>
    [Description("@#devices")]
    public extern FrozenSet<BluetoothDevice> Devices { get; set; }
}

/// <summary>
/// ValueEvent
/// </summary>
[ECMAScript]
[Description("@#ValueEvent")]
public class ValueEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="initDict">initDict</param>
    public extern ValueEvent(string type, ValueEventInit initDict);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern object Value { get; }
}

/// <summary>
/// BluetoothDevice
/// </summary>
[ECMAScript]
[Description("@#BluetoothDevice")]
public class BluetoothDevice : EventTarget
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string? Name { get; }

    /// <summary>
    /// gatt
    /// </summary>
    [Description("@#gatt")]
    public extern BluetoothRemoteGATTServer? Gatt { get; }

    /// <summary>
    /// forget
    /// </summary>
    [Description("@#forget")]
    public extern PromiseResult<object> Forget();

    /// <summary>
    /// watchAdvertisements
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#watchAdvertisements")]
    public extern PromiseResult<object> WatchAdvertisements(WatchAdvertisementsOptions? options = default);

    /// <summary>
    /// watchingAdvertisements
    /// </summary>
    [Description("@#watchingAdvertisements")]
    public extern bool WatchingAdvertisements { get; }

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
/// BluetoothManufacturerDataMap
/// </summary>
[ECMAScript]
[Description("@#BluetoothManufacturerDataMap")]
public class BluetoothManufacturerDataMap : IDictionary<ushort, DataView>
{
    #region Dictionary
    extern DataView IDictionary<ushort, DataView>.this[ushort key] { get; set; }
    extern ICollection<ushort> IDictionary<ushort, DataView>.Keys { get; }
    extern ICollection<DataView> IDictionary<ushort, DataView>.Values { get; }
    extern int ICollection<KeyValuePair<ushort, DataView>>.Count { get; }
    extern bool ICollection<KeyValuePair<ushort, DataView>>.IsReadOnly { get; }
    extern void IDictionary<ushort, DataView>.Add(ushort key, DataView value);
    extern void ICollection<KeyValuePair<ushort, DataView>>.Add(KeyValuePair<ushort, DataView> item);
    extern void ICollection<KeyValuePair<ushort, DataView>>.Clear();
    extern bool ICollection<KeyValuePair<ushort, DataView>>.Contains(KeyValuePair<ushort, DataView> item);
    extern bool IDictionary<ushort, DataView>.ContainsKey(ushort key);
    extern void ICollection<KeyValuePair<ushort, DataView>>.CopyTo(KeyValuePair<ushort, DataView>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<ushort, DataView>> IEnumerable<KeyValuePair<ushort, DataView>>.GetEnumerator();
    extern bool IDictionary<ushort, DataView>.Remove(ushort key);
    extern bool ICollection<KeyValuePair<ushort, DataView>>.Remove(KeyValuePair<ushort, DataView> item);
    extern bool IDictionary<ushort, DataView>.TryGetValue(ushort key, [MaybeNullWhen(false)] out DataView value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// BluetoothServiceDataMap
/// </summary>
[ECMAScript]
[Description("@#BluetoothServiceDataMap")]
public class BluetoothServiceDataMap : IDictionary<UUID, DataView>
{
    #region Dictionary
    extern DataView IDictionary<UUID, DataView>.this[UUID key] { get; set; }
    extern ICollection<UUID> IDictionary<UUID, DataView>.Keys { get; }
    extern ICollection<DataView> IDictionary<UUID, DataView>.Values { get; }
    extern int ICollection<KeyValuePair<UUID, DataView>>.Count { get; }
    extern bool ICollection<KeyValuePair<UUID, DataView>>.IsReadOnly { get; }
    extern void IDictionary<UUID, DataView>.Add(UUID key, DataView value);
    extern void ICollection<KeyValuePair<UUID, DataView>>.Add(KeyValuePair<UUID, DataView> item);
    extern void ICollection<KeyValuePair<UUID, DataView>>.Clear();
    extern bool ICollection<KeyValuePair<UUID, DataView>>.Contains(KeyValuePair<UUID, DataView> item);
    extern bool IDictionary<UUID, DataView>.ContainsKey(UUID key);
    extern void ICollection<KeyValuePair<UUID, DataView>>.CopyTo(KeyValuePair<UUID, DataView>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<UUID, DataView>> IEnumerable<KeyValuePair<UUID, DataView>>.GetEnumerator();
    extern bool IDictionary<UUID, DataView>.Remove(UUID key);
    extern bool ICollection<KeyValuePair<UUID, DataView>>.Remove(KeyValuePair<UUID, DataView> item);
    extern bool IDictionary<UUID, DataView>.TryGetValue(UUID key, [MaybeNullWhen(false)] out DataView value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// BluetoothAdvertisingEvent
/// </summary>
[ECMAScript]
[Description("@#BluetoothAdvertisingEvent")]
public class BluetoothAdvertisingEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="init">init</param>
    public extern BluetoothAdvertisingEvent(string type, BluetoothAdvertisingEventInit init);

    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern BluetoothDevice Device { get; }

    /// <summary>
    /// uuids
    /// </summary>
    [Description("@#uuids")]
    public extern FrozenSet<UUID> UUIDs { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string? Name { get; }

    /// <summary>
    /// appearance
    /// </summary>
    [Description("@#appearance")]
    public extern ushort? Appearance { get; }

    /// <summary>
    /// txPower
    /// </summary>
    [Description("@#txPower")]
    public extern sbyte? TxPower { get; }

    /// <summary>
    /// rssi
    /// </summary>
    [Description("@#rssi")]
    public extern sbyte? Rssi { get; }

    /// <summary>
    /// manufacturerData
    /// </summary>
    [Description("@#manufacturerData")]
    public extern BluetoothManufacturerDataMap ManufacturerData { get; }

    /// <summary>
    /// serviceData
    /// </summary>
    [Description("@#serviceData")]
    public extern BluetoothServiceDataMap ServiceData { get; }
}

/// <summary>
/// BluetoothRemoteGATTServer
/// </summary>
[ECMAScript]
[Description("@#BluetoothRemoteGATTServer")]
public class BluetoothRemoteGATTServer
{
    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern BluetoothDevice Device { get; }

    /// <summary>
    /// connected
    /// </summary>
    [Description("@#connected")]
    public extern bool Connected { get; }

    /// <summary>
    /// connect
    /// </summary>
    [Description("@#connect")]
    public extern PromiseResult<BluetoothRemoteGATTServer> Connect();

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// getPrimaryService
    /// </summary>
    /// <param name="service">service</param>
    [Description("@#getPrimaryService")]
    public extern PromiseResult<BluetoothRemoteGATTService> GetPrimaryService(BluetoothServiceUUID service);

    /// <summary>
    /// getPrimaryServices
    /// </summary>
    /// <param name="service">service</param>
    [Description("@#getPrimaryServices")]
    public extern PromiseResult<BluetoothRemoteGATTService[]> GetPrimaryServices(BluetoothServiceUUID service);
}

/// <summary>
/// BluetoothRemoteGATTService
/// </summary>
[ECMAScript]
[Description("@#BluetoothRemoteGATTService")]
public class BluetoothRemoteGATTService : EventTarget
{
    /// <summary>
    /// device
    /// </summary>
    [Description("@#device")]
    public extern BluetoothDevice Device { get; }

    /// <summary>
    /// uuid
    /// </summary>
    [Description("@#uuid")]
    public extern UUID UUID { get; }

    /// <summary>
    /// isPrimary
    /// </summary>
    [Description("@#isPrimary")]
    public extern bool IsPrimary { get; }

    /// <summary>
    /// getCharacteristic
    /// </summary>
    /// <param name="characteristic">characteristic</param>
    [Description("@#getCharacteristic")]
    public extern PromiseResult<BluetoothRemoteGATTCharacteristic> GetCharacteristic(BluetoothCharacteristicUUID characteristic);

    /// <summary>
    /// getCharacteristics
    /// </summary>
    /// <param name="characteristic">characteristic</param>
    [Description("@#getCharacteristics")]
    public extern PromiseResult<BluetoothRemoteGATTCharacteristic[]> GetCharacteristics(BluetoothCharacteristicUUID characteristic);

    /// <summary>
    /// getIncludedService
    /// </summary>
    /// <param name="service">service</param>
    [Description("@#getIncludedService")]
    public extern PromiseResult<BluetoothRemoteGATTService> GetIncludedService(BluetoothServiceUUID service);

    /// <summary>
    /// getIncludedServices
    /// </summary>
    /// <param name="service">service</param>
    [Description("@#getIncludedServices")]
    public extern PromiseResult<BluetoothRemoteGATTService[]> GetIncludedServices(BluetoothServiceUUID service);

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
/// BluetoothRemoteGATTCharacteristic
/// </summary>
[ECMAScript]
[Description("@#BluetoothRemoteGATTCharacteristic")]
public class BluetoothRemoteGATTCharacteristic : EventTarget
{
    /// <summary>
    /// service
    /// </summary>
    [Description("@#service")]
    public extern BluetoothRemoteGATTService Service { get; }

    /// <summary>
    /// uuid
    /// </summary>
    [Description("@#uuid")]
    public extern UUID UUID { get; }

    /// <summary>
    /// properties
    /// </summary>
    [Description("@#properties")]
    public extern BluetoothCharacteristicProperties Properties { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern DataView? Value { get; }

    /// <summary>
    /// getDescriptor
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#getDescriptor")]
    public extern PromiseResult<BluetoothRemoteGATTDescriptor> GetDescriptor(BluetoothDescriptorUUID descriptor);

    /// <summary>
    /// getDescriptors
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    [Description("@#getDescriptors")]
    public extern PromiseResult<BluetoothRemoteGATTDescriptor[]> GetDescriptors(BluetoothDescriptorUUID descriptor);

    /// <summary>
    /// readValue
    /// </summary>
    [Description("@#readValue")]
    public extern PromiseResult<DataView> ReadValue();

    /// <summary>
    /// writeValue
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#writeValue")]
    public extern PromiseResult<object> WriteValue(IBufferSource value);

    /// <summary>
    /// writeValueWithResponse
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#writeValueWithResponse")]
    public extern PromiseResult<object> WriteValueWithResponse(IBufferSource value);

    /// <summary>
    /// writeValueWithoutResponse
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#writeValueWithoutResponse")]
    public extern PromiseResult<object> WriteValueWithoutResponse(IBufferSource value);

    /// <summary>
    /// startNotifications
    /// </summary>
    [Description("@#startNotifications")]
    public extern PromiseResult<BluetoothRemoteGATTCharacteristic> StartNotifications();

    /// <summary>
    /// stopNotifications
    /// </summary>
    [Description("@#stopNotifications")]
    public extern PromiseResult<BluetoothRemoteGATTCharacteristic> StopNotifications();

    #region mixin CharacteristicEventHandlers
    /// <summary>
    /// oncharacteristicvaluechanged
    /// </summary>
    [Description("@#oncharacteristicvaluechanged")]
    public extern EventHandler Oncharacteristicvaluechanged { get; set; }
    #endregion
}

/// <summary>
/// BluetoothCharacteristicProperties
/// </summary>
[ECMAScript]
[Description("@#BluetoothCharacteristicProperties")]
public class BluetoothCharacteristicProperties
{
    /// <summary>
    /// broadcast
    /// </summary>
    [Description("@#broadcast")]
    public extern bool Broadcast { get; }

    /// <summary>
    /// read
    /// </summary>
    [Description("@#read")]
    public extern bool Read { get; }

    /// <summary>
    /// writeWithoutResponse
    /// </summary>
    [Description("@#writeWithoutResponse")]
    public extern bool WriteWithoutResponse { get; }

    /// <summary>
    /// write
    /// </summary>
    [Description("@#write")]
    public extern bool Write { get; }

    /// <summary>
    /// notify
    /// </summary>
    [Description("@#notify")]
    public extern bool Notify { get; }

    /// <summary>
    /// indicate
    /// </summary>
    [Description("@#indicate")]
    public extern bool Indicate { get; }

    /// <summary>
    /// authenticatedSignedWrites
    /// </summary>
    [Description("@#authenticatedSignedWrites")]
    public extern bool AuthenticatedSignedWrites { get; }

    /// <summary>
    /// reliableWrite
    /// </summary>
    [Description("@#reliableWrite")]
    public extern bool ReliableWrite { get; }

    /// <summary>
    /// writableAuxiliaries
    /// </summary>
    [Description("@#writableAuxiliaries")]
    public extern bool WritableAuxiliaries { get; }
}

/// <summary>
/// BluetoothRemoteGATTDescriptor
/// </summary>
[ECMAScript]
[Description("@#BluetoothRemoteGATTDescriptor")]
public class BluetoothRemoteGATTDescriptor
{
    /// <summary>
    /// characteristic
    /// </summary>
    [Description("@#characteristic")]
    public extern BluetoothRemoteGATTCharacteristic Characteristic { get; }

    /// <summary>
    /// uuid
    /// </summary>
    [Description("@#uuid")]
    public extern UUID UUID { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern DataView? Value { get; }

    /// <summary>
    /// readValue
    /// </summary>
    [Description("@#readValue")]
    public extern PromiseResult<DataView> ReadValue();

    /// <summary>
    /// writeValue
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#writeValue")]
    public extern PromiseResult<object> WriteValue(IBufferSource value);
}

/// <summary>
/// BluetoothUUID
/// </summary>
[ECMAScript]
[Description("@#BluetoothUUID")]
public class BluetoothUUID
{
    /// <summary>
    /// getService
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getService")]
    public extern static UUID GetService(Either<string, uint> name);

    /// <summary>
    /// getService
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getService")]
    public extern static UUID GetService(string name);

    /// <summary>
    /// getService
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getService")]
    public extern static UUID GetService(uint name);

    /// <summary>
    /// getCharacteristic
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getCharacteristic")]
    public extern static UUID GetCharacteristic(Either<string, uint> name);

    /// <summary>
    /// getCharacteristic
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getCharacteristic")]
    public extern static UUID GetCharacteristic(string name);

    /// <summary>
    /// getCharacteristic
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getCharacteristic")]
    public extern static UUID GetCharacteristic(uint name);

    /// <summary>
    /// getDescriptor
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getDescriptor")]
    public extern static UUID GetDescriptor(Either<string, uint> name);

    /// <summary>
    /// getDescriptor
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getDescriptor")]
    public extern static UUID GetDescriptor(string name);

    /// <summary>
    /// getDescriptor
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#getDescriptor")]
    public extern static UUID GetDescriptor(uint name);

    /// <summary>
    /// canonicalUUID
    /// </summary>
    /// <param name="alias">alias</param>
    [Description("@#canonicalUUID")]
    public extern static UUID CanonicalUUID(uint alias);
}