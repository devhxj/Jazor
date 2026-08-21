using System.Reflection;
using ECMAScript;
using ECMAScript.Contract;

namespace Jazor.CLR.Test;

/// <summary>
/// CancellationToken / CancellationTokenSource / CancellationTokenRegistration 的白名单元数据断言。
/// </summary>
/// <remarks>
/// 这三个类型共同构成一条取消链，carrier 关系（signal / controller / 内部 registration）是整条链的
/// 语义前提：token 是只读视图，source 持有取消权，registration 只是撤销凭据。因此别名、模板文本和
/// Import 导出名都是契约而不是实现细节，任何漂移都会静默改变调用点语义。
/// </remarks>
[TestClass]
public sealed class CancellationModuleWhitelistTests
{
    [TestMethod]
    public void CancellationTypeAliases_MapOntoTheAbortCarriers()
    {
        // token/source 的只读视图与取消权分工正好落在 AbortSignal/AbortController 上；
        // registration 没有宿主对等类型，因此别名到 Object，实际 carrier 是内部 JCancellationTokenRegistration。
        AssertAlias(typeof(CancellationTokenModule), "System.Threading.CancellationToken", "AbortSignal");
        AssertAlias(typeof(CancellationTokenSourceModule), "System.Threading.CancellationTokenSource", "AbortController");
        AssertAlias(typeof(CancellationTokenRegistrationModule), "System.Threading.CancellationTokenRegistration", "Object");

        AssertModulePath(typeof(CancellationTokenModule), "System/Threading/CancellationTokenModule.js");
        AssertModulePath(typeof(CancellationTokenSourceModule), "System/Threading/CancellationTokenSourceModule.js");
        AssertModulePath(typeof(CancellationTokenRegistrationModule), "System/Threading/CancellationTokenRegistrationModule.js");
    }

    [TestMethod]
    public void CancellationTokenIdentityMembers_ShareTheNeverAbortSingleton()
    {
        var mappings = GetMappings(typeof(CancellationTokenModule));

        // default(CancellationToken) == CancellationToken.None 靠同一个 signal 单例成立，
        // 因此这三个入口必须走同一个模块（Import），不能各自内联出一个新 signal。
        AssertImport(mappings, "System.Threading.CancellationToken.CancellationToken()", "createDefaultToken");
        AssertImport(mappings, "static System.Threading.CancellationToken.None.get", "getNone");
        AssertImport(mappings, "System.Threading.CancellationToken.CancellationToken(bool)", "createToken");

        // CanBeCanceled 的擦除语义是"不是那个永不取消的单例"，只能在模块内部与单例比较。
        AssertImport(mappings, "System.Threading.CancellationToken.CanBeCanceled.get", "getCanBeCanceled");

        AssertAliasMember(mappings, "System.Threading.CancellationToken.IsCancellationRequested.get", "aborted");
    }

    [TestMethod]
    public void CancellationTokenRegisterOverloads_RouteThroughTheSharedCallbackHelper()
    {
        var mappings = GetMappings(typeof(CancellationTokenModule));

        // 每个重载只负责把 CLR 的 state/token 形参适配成零参回调，注册/撤销语义集中在
        // RuntimeModule.RegisterCancellationCallback，因此整族必须是 Import 而不是各自的 inline。
        AssertImport(mappings, "System.Threading.CancellationToken.Register(System.Action)", "register");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.Register(System.Action, bool)",
            "registerWithSynchronizationContext");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.Register(System.Action<object>, object)",
            "registerWithState");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.Register(System.Action<object, System.Threading.CancellationToken>, object)",
            "registerWithStateAndToken");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.Register(System.Action<object>, object, bool)",
            "registerWithStateAndSynchronizationContext");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.UnsafeRegister(System.Action<object>, object)",
            "unsafeRegisterWithState");
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.UnsafeRegister(System.Action<object, System.Threading.CancellationToken>, object)",
            "unsafeRegisterWithStateAndToken");

        // signal.throwIfAborted() 抛的是 DOMException，与运行时统一的 Error("<Name>: <message>")
        // 失败协议不一致，因此取消检查必须走模块内部的显式 throw。
        AssertImport(
            mappings,
            "System.Threading.CancellationToken.ThrowIfCancellationRequested()",
            "throwIfCancellationRequested");
    }

    [TestMethod]
    public void CancellationTokenEquality_UsesSignalReferenceIdentity()
    {
        var mappings = GetMappings(typeof(CancellationTokenModule));

        // CLR 比较内部 source 引用；擦除后同一个 source 就是同一个 signal，所以引用相等即语义相等。
        AssertInline(
            mappings,
            "System.Threading.CancellationToken.Equals(System.Threading.CancellationToken)",
            "__arg1 === __arg2");
        AssertInline(mappings, "override System.Threading.CancellationToken.Equals(object)", "__arg1 === __arg2");

        // 运算符走默认 lowering（=== / !==），与上面的 Equals 同一套身份规则。
        AssertAllowed(
            mappings,
            "static System.Threading.CancellationToken.operator ==(System.Threading.CancellationToken, System.Threading.CancellationToken)");
        AssertAllowed(
            mappings,
            "static System.Threading.CancellationToken.operator !=(System.Threading.CancellationToken, System.Threading.CancellationToken)");

        // signal 没有稳定数值身份，任何近似 hash 都会破坏 Equals/GetHashCode 一致性。
        AssertDiscard(mappings, "override System.Threading.CancellationToken.GetHashCode()");
        // WaitHandle 是 CLR 内核同步对象，浏览器没有对等物。
        AssertDiscard(mappings, "System.Threading.CancellationToken.WaitHandle.get");
    }

    [TestMethod]
    public void CancellationTokenSourceMembers_MapOntoTheAbortControllerSurface()
    {
        var mappings = GetMappings(typeof(CancellationTokenSourceModule));

        // source/token 与 controller/signal 一一对应，因此这两个访问器是纯名称改写。
        AssertAliasMember(mappings, "System.Threading.CancellationTokenSource.Token.get", "signal");
        AssertAliasMember(mappings, "System.Threading.CancellationTokenSource.Cancel()", "abort");

        AssertInline(
            mappings,
            "System.Threading.CancellationTokenSource.IsCancellationRequested.get",
            "__arg1.signal.aborted");
        AssertInline(mappings, "System.Threading.CancellationTokenSource.CancellationTokenSource()", "new AbortController()");
        // throwOnFirstException 描述 CLR 如何聚合回调异常；abort 派发不聚合 listener 异常。
        AssertInline(mappings, "System.Threading.CancellationTokenSource.Cancel(bool)", "__arg1.abort()");
        // abort() 是同步的，CancelAsync 的唯一可观察差异是结果以 Task 形式返回。
        AssertInline(
            mappings,
            "System.Threading.CancellationTokenSource.CancelAsync()",
            "Promise.resolve(__arg1.abort())");
    }

    [TestMethod]
    public void CancellationTokenSourceDelayMembers_ShareTheScheduleCancelHelper()
    {
        var mappings = GetMappings(typeof(CancellationTokenSourceModule));

        // 延迟取消要能"替换上一次延迟"并在 Dispose 时清除，定时器 id 记录在模块级 WeakMap 上，
        // 因此延迟构造、CancelAfter 和 Dispose 必须共用同一个模块状态（Import）。
        AssertImport(
            mappings,
            "System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan)",
            "createWithDelay");
        AssertImport(
            mappings,
            "System.Threading.CancellationTokenSource.CancellationTokenSource(int)",
            "createWithMillisecondsDelay");
        AssertImport(mappings, "System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)", "cancelAfterDelay");
        AssertImport(mappings, "System.Threading.CancellationTokenSource.CancelAfter(int)", "cancelAfter");
        // Dispose 不取消，只清除尚未触发的延迟取消定时器。
        AssertImport(mappings, "System.Threading.CancellationTokenSource.Dispose()", "dispose");

        // AbortSignal.any 只产出 signal，而 CLR 要求返回一个仍可独立 Cancel 的 source。
        AssertImport(
            mappings,
            "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken)",
            "createLinkedTokenSource");
        AssertImport(
            mappings,
            "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken, System.Threading.CancellationToken)",
            "createLinkedTokenSourceFromPair");
    }

    [TestMethod]
    public void CancellationTokenSourceLongTail_StaysUnsupported()
    {
        var mappings = GetMappings(typeof(CancellationTokenSourceModule));

        // AbortController 的 abort 是单向终态，没有复位入口。
        AssertDiscard(mappings, "System.Threading.CancellationTokenSource.TryReset()");
        // TimeProvider 未映射；用宿主时钟替代会静默丢掉调用方自带的时间源。
        AssertDiscard(
            mappings,
            "System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan, System.TimeProvider)");
        // 常用路径是一到两个 token，params 形态等有明确需求时再支持。
        AssertDiscard(
            mappings,
            "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.Threading.CancellationToken[])");
        AssertDiscard(
            mappings,
            "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(params System.ReadOnlySpan<System.Threading.CancellationToken>)");
    }

    [TestMethod]
    public void CancellationTokenRegistrationMembers_ExposeTheUnregisterCredential()
    {
        var mappings = GetMappings(typeof(CancellationTokenRegistrationModule));

        // 撤销语义集中在 RuntimeModule.UnregisterCancellationCallback：Unregister 返回是否真的撤下了
        // 一个尚未执行的回调，Dispose 是它的忽略返回值版本，DisposeAsync 再包一层已完成 Promise。
        AssertImport(mappings, "System.Threading.CancellationTokenRegistration.Unregister()", "unregister");
        AssertImport(mappings, "System.Threading.CancellationTokenRegistration.Dispose()", "dispose");
        AssertImport(mappings, "System.Threading.CancellationTokenRegistration.DisposeAsync()", "disposeAsync");

        // carrier 的 signal 就是注册时的那个 token。
        AssertAliasMember(mappings, "System.Threading.CancellationTokenRegistration.Token.get", "signal");

        // 同一次 Register 得到同一个 carrier 对象，因此身份按引用比较。
        AssertInline(
            mappings,
            "System.Threading.CancellationTokenRegistration.Equals(System.Threading.CancellationTokenRegistration)",
            "__arg1 === __arg2");
        AssertInline(
            mappings,
            "override System.Threading.CancellationTokenRegistration.Equals(object)",
            "__arg1 === __arg2");
        AssertAllowed(
            mappings,
            "static System.Threading.CancellationTokenRegistration.operator ==(System.Threading.CancellationTokenRegistration, System.Threading.CancellationTokenRegistration)");
        AssertAllowed(
            mappings,
            "static System.Threading.CancellationTokenRegistration.operator !=(System.Threading.CancellationTokenRegistration, System.Threading.CancellationTokenRegistration)");

        // carrier 没有稳定数值身份；空注册会把 carrier 的 signal 变成可空，从而给撤销路径引入
        // 一个仅为占位值存在的分支。
        AssertDiscard(mappings, "override System.Threading.CancellationTokenRegistration.GetHashCode()");
        AssertDiscard(
            mappings,
            "System.Threading.CancellationTokenRegistration.CancellationTokenRegistration()");
    }

    private static IReadOnlyDictionary<string, JazorAttribute> GetMappings(Type module)
        => module
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<JazorAttribute>())
            .OfType<JazorAttribute>()
            .ToDictionary(attribute => attribute.Member, StringComparer.Ordinal);

    private static void AssertAlias(Type module, string member, string expected)
    {
        var attribute = module.GetCustomAttribute<JazorAttribute>();

        Assert.IsNotNull(attribute, $"Missing type mapping: {module.Name}");
        Assert.AreEqual(Op.Alias, attribute.Op, $"Type mapping should be Alias: {module.Name}");
        Assert.AreEqual(member, attribute.Member, $"Unexpected mapped type: {module.Name}");
        Assert.AreEqual(expected, attribute.Value, $"Unexpected carrier alias: {module.Name}");
    }

    private static void AssertModulePath(Type module, string expected)
    {
        // ECMAScriptModuleAttribute 标注为 browser-only，但这里只读取元数据，不会派发任何宿主 API，
        // 因此平台可用性诊断在测试侧不适用。
#pragma warning disable CA1416
        var attribute = module.GetCustomAttribute<ECMAScriptModuleAttribute>();

        Assert.IsNotNull(attribute, $"Missing module marker: {module.Name}");
        Assert.AreEqual(expected, attribute.Export, $"Unexpected module path: {module.Name}");
#pragma warning restore CA1416
    }

    private static void AssertImport(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        string expected)
        => AssertMapping(mappings, member, Op.Import, expected);

    private static void AssertInline(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        string expected)
        => AssertMapping(mappings, member, Op.Inline, expected);

    private static void AssertAliasMember(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        string expected)
        => AssertMapping(mappings, member, Op.Alias, expected);

    private static void AssertAllowed(IReadOnlyDictionary<string, JazorAttribute> mappings, string member)
        => AssertMapping(mappings, member, Op.Allowed, null);

    private static void AssertDiscard(IReadOnlyDictionary<string, JazorAttribute> mappings, string member)
        => AssertMapping(mappings, member, Op.Discard, null);

    private static void AssertMapping(
        IReadOnlyDictionary<string, JazorAttribute> mappings,
        string member,
        Op op,
        string? expected)
    {
        Assert.IsTrue(mappings.TryGetValue(member, out var mapping), $"Missing cancellation mapping: {member}");
        Assert.AreEqual(op, mapping.Op, $"Unexpected cancellation Op: {member}");

        if (expected is not null)
            Assert.AreEqual(expected, mapping.Value, $"Unexpected cancellation mapping value: {member}");
    }
}
