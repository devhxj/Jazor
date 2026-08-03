namespace Jazor.CLR.Test;

internal static class ClrRuntimeCollectionDiscardScenarios
{
    private const string DictionaryModulePath = "System/Collections/Generic/DictionaryT2Module.js";
    private const string HashSetModulePath = "System/Collections/Generic/HashSetT1Module.js";
    private const string DictionaryInterfaceModulePath = "System/Collections/Generic/IDictionaryT2Module.js";
    private const string GenericCollectionModulePath = "System/Collections/Generic/ICollectionT1Module.js";
    private const string GenericListModulePath = "System/Collections/Generic/IListT1Module.js";
    private const string ListInterfaceModulePath = "System/Collections/IListModule.js";
    private const string ReadOnlyCollectionModulePath = "System/Collections/ObjectModel/ReadOnlyCollectionT1Module.js";
    private const string WeakTableModulePath = "System/Runtime/CompilerServices/ConditionalWeakTableT2Module.js";
    private const string EqualityDefaultMember = "static System.Collections.Generic.EqualityComparer<T>.Default.get";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        Success(
            "generic-collection.is-read-only.recognizes-list-carrier",
            "System.Collections.Generic.ICollection<T>.IsReadOnly.get",
            GenericCollectionModulePath,
            [List(Number(1))],
            Bool(false)),
        Success(
            "generic-collection.is-read-only.treats-array-as-fixed-size",
            "System.Collections.Generic.ICollection<T>.IsReadOnly.get",
            GenericCollectionModulePath,
            [Array(Number(1))],
            Bool(true)),
        Mutation(
            "generic-collection.add.appends-to-list-carrier",
            "System.Collections.Generic.ICollection<T>.Add(T)",
            GenericCollectionModulePath,
            [List(Number(1)), Number(2)],
            [Array(Number(1), Number(2)), Number(2)]),
        Failure(
            "generic-collection.add.rejects-fixed-array",
            "System.Collections.Generic.ICollection<T>.Add(T)",
            GenericCollectionModulePath,
            [Array(Number(1)), Number(2)],
            "NotSupportedException: Collection has a fixed size."),
        Failure(
            "generic-collection.add.rejects-read-only-view",
            "System.Collections.Generic.ICollection<T>.Add(T)",
            GenericCollectionModulePath,
            [Invoke("System.Collections.ObjectModel.ReadOnlyCollection<T>.ReadOnlyCollection(System.Collections.Generic.IList<T>)", Array(Number(1))), Number(2)],
            "NotSupportedException: Collection is read-only."),
        Mutation(
            "generic-collection.clear.clears-list-carrier",
            "System.Collections.Generic.ICollection<T>.Clear()",
            GenericCollectionModulePath,
            [List(Number(1), Number(2))],
            [Array()]),
        SuccessMutation(
            "generic-collection.remove.removes-first-list-item",
            "System.Collections.Generic.ICollection<T>.Remove(T)",
            GenericCollectionModulePath,
            [List(Number(1), Number(2), Number(1)), Number(1)],
            Bool(true),
            [Array(Number(2), Number(1)), Number(1)]),

        Mutation(
            "generic-list.insert.inserts-into-list-carrier",
            "System.Collections.Generic.IList<T>.Insert(int, T)",
            GenericListModulePath,
            [List(Number(1), Number(3)), Number(1), Number(2)],
            [Array(Number(1), Number(2), Number(3)), Number(1), Number(2)]),
        Mutation(
            "generic-list.remove-at.removes-from-list-carrier",
            "System.Collections.Generic.IList<T>.RemoveAt(int)",
            GenericListModulePath,
            [List(Number(1), Number(2), Number(3)), Number(1)],
            [Array(Number(1), Number(3)), Number(1)]),

        Mutation(
            "list-interface.indexer-set.replaces-fixed-array-item",
            "System.Collections.IList.this[int].set",
            ListInterfaceModulePath,
            [Array(Number(1), Number(2)), Number(1), Number(3)],
            [Array(Number(1), Number(3)), Number(1), Number(3)]),
        SuccessMutation(
            "list-interface.add.appends-to-list-carrier",
            "System.Collections.IList.Add(object)",
            ListInterfaceModulePath,
            [List(Number(1)), Number(2)],
            Number(1),
            [Array(Number(1), Number(2)), Number(2)]),
        Mutation(
            "list-interface.clear.clears-list-carrier",
            "System.Collections.IList.Clear()",
            ListInterfaceModulePath,
            [List(Number(1), Number(2))],
            [Array()]),
        Success(
            "list-interface.is-read-only.recognizes-fixed-array",
            "System.Collections.IList.IsReadOnly.get",
            ListInterfaceModulePath,
            [Array(Number(1))],
            Bool(true)),
        Success(
            "list-interface.is-fixed-size.distinguishes-list-carrier",
            "System.Collections.IList.IsFixedSize.get",
            ListInterfaceModulePath,
            [List(Number(1))],
            Bool(false)),
        Mutation(
            "list-interface.insert.inserts-into-list-carrier",
            "System.Collections.IList.Insert(int, object)",
            ListInterfaceModulePath,
            [List(Number(1), Number(3)), Number(1), Number(2)],
            [Array(Number(1), Number(2), Number(3)), Number(1), Number(2)]),
        Mutation(
            "list-interface.remove.removes-first-list-item",
            "System.Collections.IList.Remove(object)",
            ListInterfaceModulePath,
            [List(Number(1), Number(2), Number(1)), Number(1)],
            [Array(Number(2), Number(1)), Number(1)]),
        Mutation(
            "list-interface.remove-at.removes-from-list-carrier",
            "System.Collections.IList.RemoveAt(int)",
            ListInterfaceModulePath,
            [List(Number(1), Number(2), Number(3)), Number(1)],
            [Array(Number(1), Number(3)), Number(1)]),

        Success(
            "dictionary.constructor.capacity.creates-empty-map",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int)",
            DictionaryModulePath,
            [Number(4)],
            Map()),
        Success(
            "dictionary.constructor.comparer.creates-empty-map",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)",
            DictionaryModulePath,
            [DefaultEquality()],
            Map()),
        Success(
            "dictionary.constructor.capacity-and-comparer.creates-empty-map",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(int, System.Collections.Generic.IEqualityComparer<TKey>)",
            DictionaryModulePath,
            [Number(4), DefaultEquality()],
            Map()),
        Success(
            "dictionary.constructor.source-and-comparer.copies-entries",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IDictionary<TKey, TValue>, System.Collections.Generic.IEqualityComparer<TKey>)",
            DictionaryModulePath,
            [Map((Text("release"), Number(1))), DefaultEquality()],
            Map((Text("release"), Number(1)))),
        Success(
            "dictionary.constructor.pairs.copies-entry-sequence",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>)",
            DictionaryModulePath,
            [Array(Array(Text("release"), Number(1)), Array(Text("owner"), Number(2)))],
            Map((Text("release"), Number(1)), (Text("owner"), Number(2)))),
        Failure(
            "dictionary.constructor.pairs.comparer-rejects-equivalent-duplicate-key",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.Generic.IEqualityComparer<TKey>)",
            DictionaryModulePath,
            [Array(Array(Number(1), Text("first")), Array(Number(3), Text("duplicate"))), ParityEquality()],
            "same key has already been added"),
        Success(
            "dictionary.comparer.returns-configured-comparer",
            "System.Collections.Generic.Dictionary<TKey, TValue>.Comparer.get",
            DictionaryModulePath,
            [Invoke(
                "System.Collections.Generic.Dictionary<TKey, TValue>.Dictionary(System.Collections.Generic.IEqualityComparer<TKey>)",
                CustomEquality())],
            CustomEquality()),
        Mutation(
            "dictionary.trim-excess.preserves-entries",
            "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess()",
            DictionaryModulePath,
            [Map((Text("release"), Number(1)))],
            [Map((Text("release"), Number(1)))]),
        Failure(
            "dictionary.trim-excess.capacity-rejects-below-count",
            "System.Collections.Generic.Dictionary<TKey, TValue>.TrimExcess(int)",
            DictionaryModulePath,
            [Map((Text("release"), Number(1))), Number(0)],
            "ArgumentOutOfRangeException"),

        Success(
            "hash-set.constructor.capacity.creates-empty-set",
            "System.Collections.Generic.HashSet<T>.HashSet(int)",
            HashSetModulePath,
            [Number(4)],
            Set()),
        Success(
            "hash-set.constructor.comparer.creates-empty-set",
            "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)",
            HashSetModulePath,
            [DefaultEquality()],
            Set()),
        Success(
            "hash-set.constructor.source-and-comparer.deduplicates-source",
            "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)",
            HashSetModulePath,
            [Array(Text("release"), Text("owner"), Text("release")), DefaultEquality()],
            Set(Text("release"), Text("owner"))),
        Success(
            "hash-set.constructor.capacity-and-comparer.creates-empty-set",
            "System.Collections.Generic.HashSet<T>.HashSet(int, System.Collections.Generic.IEqualityComparer<T>)",
            HashSetModulePath,
            [Number(4), DefaultEquality()],
            Set()),
        Success(
            "hash-set.try-get-value.returns-stored-representative",
            "System.Collections.Generic.HashSet<T>.TryGetValue(T, out T)",
            HashSetModulePath,
            [Set(Text("release")), Text("release")],
            Array(Bool(true), Text("release"))),
        Mutation(
            "hash-set.copy-to.copies-all-items",
            "System.Collections.Generic.HashSet<T>.CopyTo(T[])",
            HashSetModulePath,
            [Set(Number(1), Number(2)), Array(Number(0), Number(0))],
            [Set(Number(1), Number(2)), Array(Number(1), Number(2))]),
        Mutation(
            "hash-set.copy-to.offset-copies-all-items",
            "System.Collections.Generic.HashSet<T>.CopyTo(T[], int)",
            HashSetModulePath,
            [Set(Number(1), Number(2)), Array(Number(0), Number(0), Number(0)), Number(1)],
            [Set(Number(1), Number(2)), Array(Number(0), Number(1), Number(2)), Number(1)]),
        Mutation(
            "hash-set.copy-to.count-copies-prefix",
            "System.Collections.Generic.HashSet<T>.CopyTo(T[], int, int)",
            HashSetModulePath,
            [Set(Number(1), Number(2)), Array(Number(0), Number(0), Number(0)), Number(1), Number(1)],
            [Set(Number(1), Number(2)), Array(Number(0), Number(1), Number(0)), Number(1), Number(1)]),
        SuccessMutation(
            "hash-set.remove-where.removes-matching-items",
            "System.Collections.Generic.HashSet<T>.RemoveWhere(System.Predicate<T>)",
            HashSetModulePath,
            [Set(Number(1), Number(2), Number(3), Number(4)), Callable(ClrRuntimeCallableKind.IsEven)],
            Number(2),
            [Set(Number(1), Number(3)), Callable(ClrRuntimeCallableKind.IsEven)]),
        Success(
            "hash-set.comparer.returns-configured-comparer",
            "System.Collections.Generic.HashSet<T>.Comparer.get",
            HashSetModulePath,
            [Invoke(
                "System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEqualityComparer<T>)",
                CustomEquality())],
            CustomEquality()),
        Mutation(
            "hash-set.trim-excess.preserves-items",
            "System.Collections.Generic.HashSet<T>.TrimExcess()",
            HashSetModulePath,
            [Set(Number(1), Number(2))],
            [Set(Number(1), Number(2))]),
        Failure(
            "hash-set.trim-excess.capacity-rejects-below-count",
            "System.Collections.Generic.HashSet<T>.TrimExcess(int)",
            HashSetModulePath,
            [Set(Number(1), Number(2)), Number(1)],
            "ArgumentOutOfRangeException"),
        Success(
            "hash-set.create-set-comparer.uses-configured-element-equality",
            "System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)",
            "System/Collections/Generic/IEqualityComparerT1Module.js",
            [
                Invoke("static System.Collections.Generic.HashSet<T>.CreateSetComparer()"),
                Invoke("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)", Array(Number(1), Number(3), Number(2)), ParityEquality()),
                Invoke("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)", Array(Number(5), Number(4)), ParityEquality())
            ],
            Bool(true)),
        Success(
            "hash-set.create-set-comparer.hashes-elements-order-independently",
            "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)",
            "System/Collections/Generic/IEqualityComparerT1Module.js",
            [Invoke("static System.Collections.Generic.HashSet<T>.CreateSetComparer()"), Set(Number(1), Number(2))],
            Number(3)),
        Success(
            "hash-set.create-set-comparer.hashes-elements-with-default-hash",
            "System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)",
            "System/Collections/Generic/IEqualityComparerT1Module.js",
            [
                Invoke("static System.Collections.Generic.HashSet<T>.CreateSetComparer()"),
                Invoke("System.Collections.Generic.HashSet<T>.HashSet(System.Collections.Generic.IEnumerable<T>, System.Collections.Generic.IEqualityComparer<T>)", Array(Number(1), Number(2)), ParityEquality())
            ],
            Number(3)),

        Mutation(
            "idictionary.add.adds-unique-key",
            "System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey, TValue)",
            DictionaryInterfaceModulePath,
            [Map(), Text("release"), Number(1)],
            [Map((Text("release"), Number(1))), Text("release"), Number(1)]),
        SuccessMutation(
            "idictionary.remove.removes-existing-key",
            "System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey)",
            DictionaryInterfaceModulePath,
            [Map((Text("release"), Number(1))), Text("release")],
            Bool(true),
            [Map(), Text("release")]),

        Success(
            "weak-table.get-or-add.factory-creates-value",
            "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd(TKey, System.Func<TKey, TValue>)",
            WeakTableModulePath,
            [WeakMap(), Key("factory"), Callable(ClrRuntimeCallableKind.ReturnFactoryText)],
            Text("factory")),
        Success(
            "weak-table.get-or-add.argument-factory-uses-argument",
            "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetOrAdd<TArg>(TKey, System.Func<TKey, TArg, TValue>, TArg)",
            WeakTableModulePath,
            [WeakMap(), Key("factory-argument"), Callable(ClrRuntimeCallableKind.ReturnFactoryArgument), Text("release")],
            Text("release")),
        Success(
            "weak-table.get-value.callback-creates-value",
            "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.GetValue(TKey, System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.CreateValueCallback)",
            WeakTableModulePath,
            [WeakMap(), Key("callback"), Callable(ClrRuntimeCallableKind.ReturnFactoryText)],
            Text("factory")),
        Success(
            "weak-table.clear-detaches-prior-storage",
            "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.TryGetValue(TKey, out TValue)",
            WeakTableModulePath,
            [
                ClrRuntimeValue.Reference("clear-table", WeakMap((Key("clear-key"), Text("value")))),
                Key("clear-key"),
                Invoke(
                    "System.Runtime.CompilerServices.ConditionalWeakTable<TKey, TValue>.Clear()",
                    ClrRuntimeValue.Reference("clear-table", WeakMap()))
            ],
            Array(Bool(false), Null()))
    ];

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario SuccessMutation(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedErrorContains)
        => new(id, member, modulePath, arguments, ExpectedValue: null, ExpectedErrorContains: expectedErrorContains);

    private static ClrRuntimeScenario Mutation(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, ClrRuntimeValue.Undefined(), ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue DefaultEquality() => Invoke(EqualityDefaultMember);
    private static ClrRuntimeValue CustomEquality() => Record(
        ("equals", Callable(ClrRuntimeCallableKind.IsEven)),
        ("getHashCode", Callable(ClrRuntimeCallableKind.DoubleNumber)));
    private static ClrRuntimeValue ParityEquality() => Record(
        ("equals", Callable(ClrRuntimeCallableKind.SameParity)),
        ("getHashCode", Callable(ClrRuntimeCallableKind.ParityHash)));
    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments) => ClrRuntimeValue.Invoke(member, arguments);
    private static ClrRuntimeValue List(params ClrRuntimeValue[] values)
        => Invoke("System.Collections.Generic.List<T>.List(System.Collections.Generic.IEnumerable<T>)", Array(values));
    private static ClrRuntimeValue Key(string id) => ClrRuntimeValue.Reference(id, Record(("id", Text(id))));
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Text(string value) => ClrRuntimeValue.Text(value);
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
    private static ClrRuntimeValue Array(params ClrRuntimeValue[] values) => ClrRuntimeValue.Array(values);
    private static ClrRuntimeValue Set(params ClrRuntimeValue[] values) => ClrRuntimeValue.Set(values);
    private static ClrRuntimeValue Map(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.Map(entries);
    private static ClrRuntimeValue WeakMap(params (ClrRuntimeValue Key, ClrRuntimeValue Value)[] entries) => ClrRuntimeValue.WeakMap(entries);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] values) => ClrRuntimeValue.Record(values);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
}
