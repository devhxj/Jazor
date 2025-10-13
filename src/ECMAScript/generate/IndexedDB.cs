namespace ECMAScript;

/// <summary>
/// IDBVersionChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#IDBVersionChangeEventInit")]
public record IDBVersionChangeEventInit(
    [property: Description("@#oldVersion")]ulong OldVersion = 0,
	[property: Description("@#newVersion")]ulong? NewVersion = null): EventInit;

/// <summary>
/// IDBDatabaseInfo
/// </summary>
[ECMAScript]
[Description("@#IDBDatabaseInfo")]
public record IDBDatabaseInfo(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#version")]ulong Version = default);

/// <summary>
/// IDBTransactionOptions
/// </summary>
[ECMAScript]
[Description("@#IDBTransactionOptions")]
public record IDBTransactionOptions(
    [property: Description("@#durability")]IDBTransactionDurability Durability = IDBTransactionDurability.Default);

/// <summary>
/// IDBObjectStoreParameters
/// </summary>
[ECMAScript]
[Description("@#IDBObjectStoreParameters")]
public record IDBObjectStoreParameters(
    [property: Description("@#keyPath")]Either<string, string[]>? KeyPath = default,
	[property: Description("@#autoIncrement")]bool AutoIncrement = false);

/// <summary>
/// IDBIndexParameters
/// </summary>
[ECMAScript]
[Description("@#IDBIndexParameters")]
public record IDBIndexParameters(
    [property: Description("@#unique")]bool Unique = false,
	[property: Description("@#multiEntry")]bool MultiEntry = false);

/// <summary>
/// IDBRequest
/// </summary>
[ECMAScript]
[Description("@#IDBRequest")]
public class IDBRequest : EventTarget
{
    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern object Result { get; }

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern DOMException? Error { get; }

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Either<IDBObjectStore, IDBIndex, IDBCursor>? Source { get; }

    /// <summary>
    /// transaction
    /// </summary>
    [Description("@#transaction")]
    public extern IDBTransaction? Transaction { get; }

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern IDBRequestReadyState ReadyState { get; }

    /// <summary>
    /// onsuccess
    /// </summary>
    [Description("@#onsuccess")]
    public extern EventHandler Onsuccess { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
}

/// <summary>
/// IDBOpenDBRequest
/// </summary>
[ECMAScript]
[Description("@#IDBOpenDBRequest")]
public class IDBOpenDBRequest : IDBRequest
{
    /// <summary>
    /// onblocked
    /// </summary>
    [Description("@#onblocked")]
    public extern EventHandler Onblocked { get; set; }

    /// <summary>
    /// onupgradeneeded
    /// </summary>
    [Description("@#onupgradeneeded")]
    public extern EventHandler Onupgradeneeded { get; set; }
}

/// <summary>
/// IDBVersionChangeEvent
/// </summary>
[ECMAScript]
[Description("@#IDBVersionChangeEvent")]
public class IDBVersionChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern IDBVersionChangeEvent(string type, IDBVersionChangeEventInit eventInitDict);

    /// <summary>
    /// oldVersion
    /// </summary>
    [Description("@#oldVersion")]
    public extern ulong OldVersion { get; }

    /// <summary>
    /// newVersion
    /// </summary>
    [Description("@#newVersion")]
    public extern ulong? NewVersion { get; }
}

/// <summary>
/// IDBFactory
/// </summary>
[ECMAScript]
[Description("@#IDBFactory")]
public class IDBFactory
{
    /// <summary>
    /// open
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="version">version</param>
    [Description("@#open")]
    public extern IDBOpenDBRequest Open(string name, ulong version);

    /// <summary>
    /// deleteDatabase
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#deleteDatabase")]
    public extern IDBOpenDBRequest DeleteDatabase(string name);

    /// <summary>
    /// databases
    /// </summary>
    [Description("@#databases")]
    public extern PromiseResult<IDBDatabaseInfo[]> Databases();

    /// <summary>
    /// cmp
    /// </summary>
    /// <param name="first">first</param>
    /// <param name="second">second</param>
    [Description("@#cmp")]
    public extern short Cmp(object first, object second);
}

/// <summary>
/// IDBDatabase
/// </summary>
[ECMAScript]
[Description("@#IDBDatabase")]
public class IDBDatabase : EventTarget
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// version
    /// </summary>
    [Description("@#version")]
    public extern ulong Version { get; }

    /// <summary>
    /// objectStoreNames
    /// </summary>
    [Description("@#objectStoreNames")]
    public extern DOMStringList ObjectStoreNames { get; }

    /// <summary>
    /// transaction
    /// </summary>
    /// <param name="storeNames">storeNames</param>
    /// <param name="mode">mode</param>
    /// <param name="options">options</param>
    [Description("@#transaction")]
    public extern IDBTransaction Transaction(Either<string, string[]> storeNames, IDBTransactionMode mode = IDBTransactionMode.Readonly, IDBTransactionOptions? options = default);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// createObjectStore
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="options">options</param>
    [Description("@#createObjectStore")]
    public extern IDBObjectStore CreateObjectStore(string name, IDBObjectStoreParameters? options = default);

    /// <summary>
    /// deleteObjectStore
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#deleteObjectStore")]
    public extern void DeleteObjectStore(string name);

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onversionchange
    /// </summary>
    [Description("@#onversionchange")]
    public extern EventHandler Onversionchange { get; set; }
}

/// <summary>
/// IDBObjectStore
/// </summary>
[ECMAScript]
[Description("@#IDBObjectStore")]
public class IDBObjectStore
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// keyPath
    /// </summary>
    [Description("@#keyPath")]
    public extern object KeyPath { get; }

    /// <summary>
    /// indexNames
    /// </summary>
    [Description("@#indexNames")]
    public extern DOMStringList IndexNames { get; }

    /// <summary>
    /// transaction
    /// </summary>
    [Description("@#transaction")]
    public extern IDBTransaction Transaction { get; }

    /// <summary>
    /// autoIncrement
    /// </summary>
    [Description("@#autoIncrement")]
    public extern bool AutoIncrement { get; }

    /// <summary>
    /// put
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="key">key</param>
    [Description("@#put")]
    public extern IDBRequest Put(object value, object key);

    /// <summary>
    /// add
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="key">key</param>
    [Description("@#add")]
    public extern IDBRequest Add(object value, object key);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#delete")]
    public extern IDBRequest Delete(object query);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern IDBRequest Clear();

    /// <summary>
    /// get
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#get")]
    public extern IDBRequest Get(object query);

    /// <summary>
    /// getKey
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#getKey")]
    public extern IDBRequest GetKey(object query);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="count">count</param>
    [Description("@#getAll")]
    public extern IDBRequest GetAll(object query, uint count);

    /// <summary>
    /// getAllKeys
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="count">count</param>
    [Description("@#getAllKeys")]
    public extern IDBRequest GetAllKeys(object query, uint count);

    /// <summary>
    /// count
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#count")]
    public extern IDBRequest Count(object query);

    /// <summary>
    /// openCursor
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="direction">direction</param>
    [Description("@#openCursor")]
    public extern IDBRequest OpenCursor(object query, IDBCursorDirection direction = IDBCursorDirection.Next);

    /// <summary>
    /// openKeyCursor
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="direction">direction</param>
    [Description("@#openKeyCursor")]
    public extern IDBRequest OpenKeyCursor(object query, IDBCursorDirection direction = IDBCursorDirection.Next);

    /// <summary>
    /// index
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#index")]
    public extern IDBIndex Index(string name);

    /// <summary>
    /// createIndex
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="keyPath">keyPath</param>
    /// <param name="options">options</param>
    [Description("@#createIndex")]
    public extern IDBIndex CreateIndex(string name, Either<string, string[]> keyPath, IDBIndexParameters? options = default);

    /// <summary>
    /// deleteIndex
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#deleteIndex")]
    public extern void DeleteIndex(string name);
}

/// <summary>
/// IDBIndex
/// </summary>
[ECMAScript]
[Description("@#IDBIndex")]
public class IDBIndex
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// objectStore
    /// </summary>
    [Description("@#objectStore")]
    public extern IDBObjectStore ObjectStore { get; }

    /// <summary>
    /// keyPath
    /// </summary>
    [Description("@#keyPath")]
    public extern object KeyPath { get; }

    /// <summary>
    /// multiEntry
    /// </summary>
    [Description("@#multiEntry")]
    public extern bool MultiEntry { get; }

    /// <summary>
    /// unique
    /// </summary>
    [Description("@#unique")]
    public extern bool Unique { get; }

    /// <summary>
    /// get
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#get")]
    public extern IDBRequest Get(object query);

    /// <summary>
    /// getKey
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#getKey")]
    public extern IDBRequest GetKey(object query);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="count">count</param>
    [Description("@#getAll")]
    public extern IDBRequest GetAll(object query, uint count);

    /// <summary>
    /// getAllKeys
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="count">count</param>
    [Description("@#getAllKeys")]
    public extern IDBRequest GetAllKeys(object query, uint count);

    /// <summary>
    /// count
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#count")]
    public extern IDBRequest Count(object query);

    /// <summary>
    /// openCursor
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="direction">direction</param>
    [Description("@#openCursor")]
    public extern IDBRequest OpenCursor(object query, IDBCursorDirection direction = IDBCursorDirection.Next);

    /// <summary>
    /// openKeyCursor
    /// </summary>
    /// <param name="query">query</param>
    /// <param name="direction">direction</param>
    [Description("@#openKeyCursor")]
    public extern IDBRequest OpenKeyCursor(object query, IDBCursorDirection direction = IDBCursorDirection.Next);
}

/// <summary>
/// IDBKeyRange
/// </summary>
[ECMAScript]
[Description("@#IDBKeyRange")]
public class IDBKeyRange
{
    /// <summary>
    /// lower
    /// </summary>
    [Description("@#lower")]
    public extern object Lower { get; }

    /// <summary>
    /// upper
    /// </summary>
    [Description("@#upper")]
    public extern object Upper { get; }

    /// <summary>
    /// lowerOpen
    /// </summary>
    [Description("@#lowerOpen")]
    public extern bool LowerOpen { get; }

    /// <summary>
    /// upperOpen
    /// </summary>
    [Description("@#upperOpen")]
    public extern bool UpperOpen { get; }

    /// <summary>
    /// only
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#only")]
    public extern static IDBKeyRange Only(object value);

    /// <summary>
    /// lowerBound
    /// </summary>
    /// <param name="lower">lower</param>
    /// <param name="open">open</param>
    [Description("@#lowerBound")]
    public extern static IDBKeyRange LowerBound(object lower, bool open = false);

    /// <summary>
    /// upperBound
    /// </summary>
    /// <param name="upper">upper</param>
    /// <param name="open">open</param>
    [Description("@#upperBound")]
    public extern static IDBKeyRange UpperBound(object upper, bool open = false);

    /// <summary>
    /// bound
    /// </summary>
    /// <param name="lower">lower</param>
    /// <param name="upper">upper</param>
    /// <param name="lowerOpen">lowerOpen</param>
    /// <param name="upperOpen">upperOpen</param>
    [Description("@#bound")]
    public extern static IDBKeyRange Bound(object lower, object upper, bool lowerOpen = false, bool upperOpen = false);

    /// <summary>
    /// includes
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#includes")]
    public extern bool Includes(object key);
}

/// <summary>
/// IDBCursor
/// </summary>
[ECMAScript]
[Description("@#IDBCursor")]
public class IDBCursor
{
    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern Either<IDBObjectStore, IDBIndex> Source { get; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern IDBCursorDirection Direction { get; }

    /// <summary>
    /// key
    /// </summary>
    [Description("@#key")]
    public extern object Key { get; }

    /// <summary>
    /// primaryKey
    /// </summary>
    [Description("@#primaryKey")]
    public extern object PrimaryKey { get; }

    /// <summary>
    /// request
    /// </summary>
    [Description("@#request")]
    public extern IDBRequest Request { get; }

    /// <summary>
    /// advance
    /// </summary>
    /// <param name="count">count</param>
    [Description("@#advance")]
    public extern void Advance(uint count);

    /// <summary>
    /// continue
    /// </summary>
    /// <param name="key">key</param>
    [Description("@#continue")]
    public extern void Continue(object key);

    /// <summary>
    /// continuePrimaryKey
    /// </summary>
    /// <param name="key">key</param>
    /// <param name="primaryKey">primaryKey</param>
    [Description("@#continuePrimaryKey")]
    public extern void ContinuePrimaryKey(object key, object primaryKey);

    /// <summary>
    /// update
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#update")]
    public extern IDBRequest Update(object value);

    /// <summary>
    /// delete
    /// </summary>
    [Description("@#delete")]
    public extern IDBRequest Delete();
}

/// <summary>
/// IDBCursorWithValue
/// </summary>
[ECMAScript]
[Description("@#IDBCursorWithValue")]
public class IDBCursorWithValue : IDBCursor
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern object Value { get; }
}

/// <summary>
/// IDBTransaction
/// </summary>
[ECMAScript]
[Description("@#IDBTransaction")]
public class IDBTransaction : EventTarget
{
    /// <summary>
    /// objectStoreNames
    /// </summary>
    [Description("@#objectStoreNames")]
    public extern DOMStringList ObjectStoreNames { get; }

    /// <summary>
    /// mode
    /// </summary>
    [Description("@#mode")]
    public extern IDBTransactionMode Mode { get; }

    /// <summary>
    /// durability
    /// </summary>
    [Description("@#durability")]
    public extern IDBTransactionDurability Durability { get; }

    /// <summary>
    /// db
    /// </summary>
    [Description("@#db")]
    public extern IDBDatabase Db { get; }

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern DOMException? Error { get; }

    /// <summary>
    /// objectStore
    /// </summary>
    /// <param name="name">name</param>
    [Description("@#objectStore")]
    public extern IDBObjectStore ObjectStore(string name);

    /// <summary>
    /// commit
    /// </summary>
    [Description("@#commit")]
    public extern void Commit();

    /// <summary>
    /// abort
    /// </summary>
    [Description("@#abort")]
    public extern void Abort();

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// oncomplete
    /// </summary>
    [Description("@#oncomplete")]
    public extern EventHandler Oncomplete { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
}