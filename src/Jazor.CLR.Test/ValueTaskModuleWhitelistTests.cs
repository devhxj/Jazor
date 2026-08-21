using System.Reflection;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

[TestClass]
public sealed class ValueTaskModuleWhitelistTests
{
    [TestMethod]
    public void ValueTaskTypeAlias_SharesThePromiseCarrierWithTask()
    {
        var attribute = typeof(Jazor.CLR.ValueTaskModule).GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute);
        Assert.AreEqual(Op.Alias, attribute.Op);
        Assert.AreEqual("System.Threading.Tasks.ValueTask", attribute.Member);
        Assert.AreEqual("Promise", attribute.Value);
    }

    [TestMethod]
    public void ValueTaskCreationAndAwaitMembers_LowerToPromiseTemplates()
    {
        var mappings = GetMappings();

        AssertInline(mappings, "System.Threading.Tasks.ValueTask.ValueTask()", "Promise.resolve()");
        AssertInline(
            mappings,
            "System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Task)",
            "Promise.resolve(__arg1)");
        AssertInline(mappings, "static System.Threading.Tasks.ValueTask.CompletedTask.get", "Promise.resolve()");
        AssertInline(
            mappings,
            "static System.Threading.Tasks.ValueTask.FromException(System.Exception)",
            "Promise.reject(__arg1)");
        AssertInline(mappings, "System.Threading.Tasks.ValueTask.AsTask()", "Promise.resolve(__arg1)");
        AssertInline(mappings, "System.Threading.Tasks.ValueTask.Preserve()", "Promise.resolve(__arg1)");
        AssertInline(mappings, "System.Threading.Tasks.ValueTask.GetAwaiter()", "Promise.resolve(__arg1)");
        AssertInline(
            mappings,
            "System.Threading.Tasks.ValueTask.ConfigureAwait(bool)",
            "Promise.resolve(__arg1)");
    }

    [TestMethod]
    public void ValueTaskCancellation_UsesTheSharedTaskCanceledMarker()
    {
        // IsCanceled/Status recognition in TaskModule keys off this exact rejection reason, so the
        // ValueTask factory must not invent a second cancellation encoding.
        AssertInline(
            GetMappings(),
            "static System.Threading.Tasks.ValueTask.FromCanceled(System.Threading.CancellationToken)",
            "Promise.reject(new Error(\"TaskCanceledException\"))");
    }

    [TestMethod]
    public void ValueTaskLongTail_StaysUnsupported()
    {
        var mappings = GetMappings();

        // ValueTask<TResult> has no carrier mapping yet, so the generic factories would hand back a
        // value without a usable member face.
        AssertDiscard(mappings, "static System.Threading.Tasks.ValueTask.FromResult<TResult>(TResult)");
        AssertDiscard(mappings, "static System.Threading.Tasks.ValueTask.FromCanceled<TResult>(System.Threading.CancellationToken)");
        AssertDiscard(mappings, "static System.Threading.Tasks.ValueTask.FromException<TResult>(System.Exception)");

        // IValueTaskSource is a CLR pooling protocol without a browser carrier.
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.ValueTask(System.Threading.Tasks.Sources.IValueTaskSource, short)");

        // Equality erases to Promise reference identity, which does not match CLR value equality.
        AssertDiscard(mappings, "static System.Threading.Tasks.ValueTask.operator ==(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)");
        AssertDiscard(mappings, "static System.Threading.Tasks.ValueTask.operator !=(System.Threading.Tasks.ValueTask, System.Threading.Tasks.ValueTask)");
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.Equals(System.Threading.Tasks.ValueTask)");

        // State inspection depends on the TaskModule __jazorTaskStates protocol.
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.IsCompleted.get");
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.IsCompletedSuccessfully.get");
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.IsFaulted.get");
        AssertDiscard(mappings, "System.Threading.Tasks.ValueTask.IsCanceled.get");
    }

    private static IReadOnlyDictionary<string, JazorAttribute> GetMappings()
        => typeof(Jazor.CLR.ValueTaskModule)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(attribute => attribute.Member, StringComparer.Ordinal);

    private static void AssertInline(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        string expected)
    {
        Assert.IsTrue(mappings.TryGetValue(member, out var mapping), $"Missing ValueTask mapping: {member}");
        Assert.AreEqual(Op.Inline, mapping.Op, $"ValueTask mapping should be Inline: {member}");
        Assert.AreEqual(expected, mapping.Value, $"Unexpected ValueTask template: {member}");
    }

    private static void AssertDiscard(IReadOnlyDictionary<string, JazorAttribute> mappings, string member)
    {
        Assert.IsTrue(mappings.TryGetValue(member, out var mapping), $"Missing ValueTask mapping: {member}");
        Assert.AreEqual(Op.Discard, mapping.Op, $"ValueTask mapping should stay unsupported: {member}");
    }
}
