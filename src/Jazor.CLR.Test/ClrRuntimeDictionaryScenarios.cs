namespace Jazor.CLR.Test;

internal static class ClrRuntimeDictionaryScenarios
{
    private const string DictionaryModulePath = "System/Collections/Generic/DictionaryT2Module.js";
    private const string InterfaceModulePath = "System/Collections/Generic/IDictionaryT2Module.js";
    private const string ReadOnlyModulePath = "System/Collections/ObjectModel/ReadOnlyDictionaryT2Module.js";
    private const string WeakTableModulePath = "System/Runtime/CompilerServices/ConditionalWeakTableT2Module.js";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success("dictionary.indexer.get-existing", "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("release")], Number(42)),
        Failure("dictionary.indexer.rejects-missing-key", "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", DictionaryModulePath, [Map(), Text("release")], "KeyNotFoundException"),
        Failure("dictionary.indexer.rejects-null-instance", "System.Collections.Generic.Dictionary<TKey, TValue>.this[TKey].get", DictionaryModulePath, [Null(), Text("release")], "NullReferenceException"),
        Mutation("dictionary.add.new-key", "System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("owner"), Text("platform")], [Map((Text("release"), Number(42)), (Text("owner"), Text("platform"))), Text("owner"), Text("platform")]),
        Failure("dictionary.add.duplicate-key", "System.Collections.Generic.Dictionary<TKey, TValue>.Add(TKey, TValue)", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("release"), Number(99)], "ArgumentException"),
        SuccessMutation("dictionary.remove.out-existing", "System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("release")], Array(Bool(true), Number(42)), [Map(), Text("release")]),
        Success("dictionary.remove.out-missing", "System.Collections.Generic.Dictionary<TKey, TValue>.Remove(TKey, out TValue)", DictionaryModulePath, [Map(), Text("release")], Array(Bool(false), Null())),
        SuccessMutation("dictionary.try-add.duplicate-key", "System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("release"), Number(99)], Bool(false), [Map((Text("release"), Number(42))), Text("release"), Number(99)]),
        SuccessMutation("dictionary.try-add.new-key", "System.Collections.Generic.Dictionary<TKey, TValue>.TryAdd(TKey, TValue)", DictionaryModulePath, [Map(), Text("release"), Number(42)], Bool(true), [Map((Text("release"), Number(42))), Text("release"), Number(42)]),
        Success("dictionary.try-get.missing-key", "System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", DictionaryModulePath, [Map(), Text("release")], Array(Bool(false), Null())),
        Success("dictionary.try-get.existing-key", "System.Collections.Generic.Dictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", DictionaryModulePath, [Map((Text("release"), Number(42))), Text("release")], Array(Bool(true), Number(42))),

        Success("idictionary.indexer.get-existing", "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get", InterfaceModulePath, [Map((Text("release"), Number(42))), Text("release")], Number(42)),
        Failure("idictionary.indexer.rejects-missing-key", "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].get", InterfaceModulePath, [Map(), Text("release")], "KeyNotFoundException"),
        Mutation("idictionary.indexer.set-overwrites-existing", "System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey].set", InterfaceModulePath, [Map((Text("release"), Number(42))), Text("release"), Number(99)], [Map((Text("release"), Number(99))), Text("release"), Number(99)]),
        Success("idictionary.try-get.missing-key", "System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", InterfaceModulePath, [Map(), Text("release")], Array(Bool(false), Null())),
        Success("idictionary.try-get.existing-key", "System.Collections.Generic.IDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", InterfaceModulePath, [Map((Text("release"), Number(42))), Text("release")], Array(Bool(true), Number(42))),

        Success("read-only-dictionary.constructor.snapshots-source", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", ReadOnlyModulePath, [Map((Text("release"), Number(42)))], Map((Text("release"), Number(42)))),
        Failure("read-only-dictionary.constructor.rejects-null-source", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", ReadOnlyModulePath, [Null()], "ArgumentNullException"),
        Success("read-only-dictionary.constructor.snapshot-is-readable", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get", ReadOnlyModulePath, [Invoke("System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.ReadOnlyDictionary(System.Collections.Generic.IDictionary<TKey, TValue>)", Map((Text("release"), Number(42)))), Text("release")], Number(42)),
        Success("read-only-dictionary.try-get.missing-key", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", ReadOnlyModulePath, [Map(), Text("release"), Null()], Array(Bool(false), Null())),
        Success("read-only-dictionary.try-get.existing-key", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey, out TValue)", ReadOnlyModulePath, [Map((Text("release"), Number(42))), Text("release"), Null()], Array(Bool(true), Number(42))),
        Failure("read-only-dictionary.indexer.rejects-missing-key", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get", ReadOnlyModulePath, [Map(), Text("release")], "KeyNotFoundException"),
        Failure("read-only-dictionary.indexer.rejects-null-instance", "System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.this[TKey].get", ReadOnlyModulePath, [Null(), Text("release")], "NullReferenceException"),
        Success("read-only-dictionary.empty", "static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>.Empty.get", ReadOnlyModulePath, [], Map()),

        Success("weak-table.try-get.existing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)", WeakTableModulePath, [WeakMap((Key("try-get"), Text("cached"))), Key("try-get"), Null()], Array(Bool(true), Text("cached"))),
        Success("weak-table.try-get.missing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)", WeakTableModulePath, [WeakMap(), Key("try-get-missing"), Null()], Array(Bool(false), Null())),
        Mutation("weak-table.add.new-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)", WeakTableModulePath, [WeakMap(), Key("add"), Text("cached")], [WeakMap((Record(("id", Text("add"))), Text("cached"))), Record(("id", Text("add"))), Text("cached")]),
        Failure("weak-table.add.rejects-duplicate-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Add(TKey, TValue)", WeakTableModulePath, [WeakMap((Key("add-duplicate"), Text("cached"))), Key("add-duplicate"), Text("replacement")], "ArgumentException"),
        SuccessMutation("weak-table.try-add.existing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)", WeakTableModulePath, [WeakMap((Key("try-add"), Text("cached"))), Key("try-add"), Text("replacement")], Bool(false), [WeakMap((Record(("id", Text("try-add"))), Text("cached"))), Record(("id", Text("try-add"))), Text("replacement")]),
        SuccessMutation("weak-table.try-add.new-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryAdd(TKey, TValue)", WeakTableModulePath, [WeakMap(), Key("try-add-new"), Text("cached")], Bool(true), [WeakMap((Record(("id", Text("try-add-new"))), Text("cached"))), Record(("id", Text("try-add-new"))), Text("cached")]),
        Mutation("weak-table.add-or-update.replaces-value", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.AddOrUpdate(TKey, TValue)", WeakTableModulePath, [WeakMap((Key("update"), Text("old"))), Key("update"), Text("new")], [WeakMap((Record(("id", Text("update"))), Text("new"))), Record(("id", Text("update"))), Text("new")]),
        SuccessMutation("weak-table.remove.existing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)", WeakTableModulePath, [WeakMap((Key("remove"), Text("cached"))), Key("remove")], Bool(true), [WeakMap(), Record(("id", Text("remove")))]),
        SuccessMutation("weak-table.remove.missing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey)", WeakTableModulePath, [WeakMap(), Key("remove-missing")], Bool(false), [WeakMap(), Record(("id", Text("remove-missing")))]),
        SuccessMutation("weak-table.remove-out.existing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)", WeakTableModulePath, [WeakMap((Key("remove-out"), Text("cached"))), Key("remove-out"), Null()], Array(Bool(true), Text("cached")), [WeakMap(), Record(("id", Text("remove-out"))), Null()]),
        Success("weak-table.remove-out.missing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Remove(TKey, out TValue)", WeakTableModulePath, [WeakMap(), Key("remove-out-missing"), Null()], Array(Bool(false), Null())),
        SuccessMutation("weak-table.get-or-add.existing-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)", WeakTableModulePath, [WeakMap((Key("get-or-add"), Text("cached"))), Key("get-or-add"), Text("replacement")], Text("cached"), [WeakMap((Record(("id", Text("get-or-add"))), Text("cached"))), Record(("id", Text("get-or-add"))), Text("replacement")]),
        SuccessMutation("weak-table.get-or-add.new-reference-key", "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, TValue)", WeakTableModulePath, [WeakMap(), Key("get-or-add-new"), Text("cached")], Text("cached"), [WeakMap((Record(("id", Text("get-or-add-new"))), Text("cached"))), Record(("id", Text("get-or-add-new"))), Text("cached")])
    ];

    private static ClrRuntimeScenario Success(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessMutation(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, ClrRuntimeValue expected, IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Mutation(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Failure(string id, string member, string modulePath, IReadOnlyList<ClrRuntimeValue> arguments, string error)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: error);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments) => ClrRuntimeValue.Invoke(member, arguments);
    private static ClrRuntimeValue Key(string id) => ClrRuntimeValue.Reference(id, Record(("id", Text(id))));
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Map(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.Map(entries);
    private static ClrRuntimeValue WeakMap(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.WeakMap(entries);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.Record(entries);
}
