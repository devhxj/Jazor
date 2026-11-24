using System;
using System.ComponentModel;

namespace ECMAScript;

public static partial class Vue
{
    /// <summary>Opaque view of a JavaScript record whose ordinary properties may unwrap Vue refs.</summary>
    /// <typeparam name="TRecord">The original structural record used to select an authored property.</typeparam>
    [ECMAScript]
    public sealed class VueUnwrappedRecord<TRecord> where TRecord : class
    {
        private VueUnwrappedRecord() { }

        /// <summary>Views the original object at a direct, required record property.</summary>
        [ECMAScriptInline("__arg1[__arg2]")]
        public extern VueUnwrappedRecord<TChild> Select<TChild>(
            [ECMAScriptPropertyPath(true, IndirectRead = true, RequireRecordView = true)] Func<TRecord, TChild> key)
            where TChild : class;
    }

    // Vue's object-property overload links to the actual property and retains
    // the nested ref on both deep and shallow sources. Only scalar ref
    // payloads can be returned here without further unwrapping their shape.
    /// <summary>Links to a numeric ref at a direct record property.</summary>
    [Description("@#toRef")]
    public extern static IVueRef<double> ToRef<TRecord>(
        VueUnwrappedRecord<TRecord> source,
        [ECMAScriptPropertyPath(true, IndirectRead = true)] Func<TRecord, IVueRef<double>> key)
        where TRecord : class;

    /// <summary>Links to a string ref at a direct record property.</summary>
    [Description("@#toRef")]
    public extern static IVueRef<string> ToRef<TRecord>(
        VueUnwrappedRecord<TRecord> source,
        [ECMAScriptPropertyPath(true, IndirectRead = true)] Func<TRecord, IVueRef<string>> key)
        where TRecord : class;

    /// <summary>Links to a Boolean ref at a direct record property.</summary>
    [Description("@#toRef")]
    public extern static IVueRef<bool> ToRef<TRecord>(
        VueUnwrappedRecord<TRecord> source,
        [ECMAScriptPropertyPath(true, IndirectRead = true)] Func<TRecord, IVueRef<bool>> key)
        where TRecord : class;
}
