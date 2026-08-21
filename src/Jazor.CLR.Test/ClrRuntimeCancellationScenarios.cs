namespace Jazor.CLR.Test;

/// <summary>
/// CancellationToken / CancellationTokenSource / CancellationTokenRegistration 的运行时场景。
/// </summary>
/// <remarks>
/// 这三个类型的 carrier 分别是 AbortSignal、AbortController 和生成的 JCancellationTokenRegistration，
/// 都把状态放在原型 getter 或 #private 字段上，因此可观察面只有：
/// <list type="bullet">
/// <item>runner 显式编码的 <c>aborted</c> / <c>signal</c>（见 ClrRuntimeTestHost 的 encode 分支）；</item>
/// <item><c>Unregister()</c> 的布尔返回值；</item>
/// <item>回调写进 state 对象的痕迹（captured arguments）。</item>
/// </list>
/// 没有任何 Import 能把一个"尚未取消但可取消"的 signal 交出来（<c>Token.get</c> 是 Op.Alias），
/// 所以"取消真的会触发回调"只能通过 <c>new CancellationToken(true)</c> 这条已取消路径验证：
/// RegisterCancellationCallback 在 signal 已 abort 时同步调用回调。
/// <para>
/// 延迟取消一律用 -1（Timeout.Infinite）或极小值：runner 没有 Deno.exit，残留的长定时器会把整批
/// 场景拖到 30 秒超时。
/// </para>
/// </remarks>
internal static class ClrRuntimeCancellationScenarios
{
    private const string TokenModule = "System/Threading/CancellationTokenModule.js";
    private const string SourceModule = "System/Threading/CancellationTokenSourceModule.js";
    private const string RegistrationModule = "System/Threading/CancellationTokenRegistrationModule.js";

    private const string DefaultToken = "System.Threading.CancellationToken.CancellationToken()";
    private const string NoneToken = "static System.Threading.CancellationToken.None.get";
    private const string CanBeCanceled = "System.Threading.CancellationToken.CanBeCanceled.get";
    private const string CreateToken = "System.Threading.CancellationToken.CancellationToken(bool)";
    private const string ThrowIfCancellationRequested = "System.Threading.CancellationToken.ThrowIfCancellationRequested()";

    private const string Register = "System.Threading.CancellationToken.Register(System.Action)";
    private const string RegisterWithSynchronizationContext = "System.Threading.CancellationToken.Register(System.Action, bool)";
    private const string RegisterWithState = "System.Threading.CancellationToken.Register(System.Action<object>, object)";
    private const string RegisterWithStateAndToken = "System.Threading.CancellationToken.Register(System.Action<object, System.Threading.CancellationToken>, object)";
    private const string RegisterWithStateAndSynchronizationContext = "System.Threading.CancellationToken.Register(System.Action<object>, object, bool)";
    private const string UnsafeRegisterWithState = "System.Threading.CancellationToken.UnsafeRegister(System.Action<object>, object)";
    private const string UnsafeRegisterWithStateAndToken = "System.Threading.CancellationToken.UnsafeRegister(System.Action<object, System.Threading.CancellationToken>, object)";

    private const string RegistrationDispose = "System.Threading.CancellationTokenRegistration.Dispose()";
    private const string RegistrationDisposeAsync = "System.Threading.CancellationTokenRegistration.DisposeAsync()";
    private const string Unregister = "System.Threading.CancellationTokenRegistration.Unregister()";

    private const string SourceWithDelay = "System.Threading.CancellationTokenSource.CancellationTokenSource(System.TimeSpan)";
    private const string SourceWithMilliseconds = "System.Threading.CancellationTokenSource.CancellationTokenSource(int)";
    private const string CancelAfterDelay = "System.Threading.CancellationTokenSource.CancelAfter(System.TimeSpan)";
    private const string CancelAfter = "System.Threading.CancellationTokenSource.CancelAfter(int)";
    private const string SourceDispose = "System.Threading.CancellationTokenSource.Dispose()";
    private const string CreateLinked = "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken)";
    private const string CreateLinkedPair = "static System.Threading.CancellationTokenSource.CreateLinkedTokenSource(System.Threading.CancellationToken, System.Threading.CancellationToken)";

    private const string TimeSpanTicks = "System.TimeSpan.TimeSpan(long)";

    // Timeout.InfiniteTimeSpan：-1 毫秒 = -10000 tick，表示"永不自动取消"，不排定定时器。
    private const long InfiniteTicks = -10000;

    private const string RegistrationRef = "cancellation-registration";

    public static IReadOnlyList<ClrRuntimeScenario> All { get; } =
    [
        // CanBeCanceled 的语义是"这个 token 是否可能被取消"，擦除后等价于"不是那个永不取消的单例"，
        // 因此 default / None / new CancellationToken(false) 三条路径必须同为 false。
        Success("cancellation.token.default-cannot-be-canceled", CanBeCanceled, TokenModule, [Invoke(DefaultToken)], Bool(false)),
        Success("cancellation.token.none-cannot-be-canceled", CanBeCanceled, TokenModule, [Invoke(NoneToken)], Bool(false)),
        Success("cancellation.token.unset-flag-cannot-be-canceled", CanBeCanceled, TokenModule, [Invoke(CreateToken, Bool(false))], Bool(false)),
        Success("cancellation.token.canceled-can-be-canceled", CanBeCanceled, TokenModule, [Invoke(CreateToken, Bool(true))], Bool(true)),

        // 取消检查不走 signal.throwIfAborted()（那会抛 DOMException），而是运行时统一的失败格式。
        Success("cancellation.token.throw-if-requested.none", ThrowIfCancellationRequested, TokenModule, [Invoke(NoneToken)], Undefined()),
        Failure("cancellation.token.throw-if-requested.canceled", ThrowIfCancellationRequested, TokenModule, [Invoke(CreateToken, Bool(true))], "OperationCanceledException"),

        // 每个 register 重载都返回一枚 registration；registration 的 signal/handler 是 #private 字段，
        // 在编码面不可见，因此用 Unregister() 的返回值证明 listener 真的挂上去了（true = 已摘除且未取消）。
        Success("cancellation.token.register.unregister", Unregister, RegistrationModule, [Invoke(Register, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity))], Bool(true)),
        Success("cancellation.token.register-with-synchronization-context.unregister", Unregister, RegistrationModule, [Invoke(RegisterWithSynchronizationContext, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Bool(true))], Bool(true)),
        Success("cancellation.token.register-with-state.unregister", Unregister, RegistrationModule, [Invoke(RegisterWithState, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Null())], Bool(true)),
        Success("cancellation.token.register-with-state-and-token.unregister", Unregister, RegistrationModule, [Invoke(RegisterWithStateAndToken, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Null())], Bool(true)),
        Success("cancellation.token.register-with-state-and-synchronization-context.unregister", Unregister, RegistrationModule, [Invoke(RegisterWithStateAndSynchronizationContext, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Null(), Bool(false))], Bool(true)),
        Success("cancellation.token.unsafe-register-with-state.unregister", Unregister, RegistrationModule, [Invoke(UnsafeRegisterWithState, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Null())], Bool(true)),
        Success("cancellation.token.unsafe-register-with-state-and-token.unregister", Unregister, RegistrationModule, [Invoke(UnsafeRegisterWithStateAndToken, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity), Null())], Bool(true)),

        // 已取消的 token 上注册：回调同步执行，registration 不持有 handler，所以 Unregister() 是 false。
        Success("cancellation.token.register.canceled-token-has-no-handler", Unregister, RegistrationModule, [Invoke(Register, Invoke(CreateToken, Bool(true)), Callable(ClrRuntimeCallableKind.Identity))], Bool(false)),
        // 回调真的被调用，只能通过它写进 state 的痕迹观察；同时 arg1 的编码顺带证明了
        // new CancellationToken(true) 交出的是一个已 abort 的 signal。
        Captured(
            "cancellation.token.register-with-state.invokes-callback-on-canceled-token",
            RegisterWithState,
            TokenModule,
            [Invoke(CreateToken, Bool(true)), Callable(ClrRuntimeCallableKind.CaptureCancellationState), Record(("canceled", Bool(false)))],
            Registration(),
            [Signal(true), Callable(ClrRuntimeCallableKind.CaptureCancellationState), Record(("canceled", Bool(true)))]),
        // Register(Action<object, CancellationToken>) 的第二个实参是"触发本次取消的 token"，
        // 擦除后就是被注册的那个 signal 自身，因此回调看到的 aborted 必须是 true。
        Captured(
            "cancellation.token.register-with-state-and-token.passes-token",
            RegisterWithStateAndToken,
            TokenModule,
            [Invoke(CreateToken, Bool(true)), Callable(ClrRuntimeCallableKind.CaptureCancellationToken), Record(("tokenAborted", Bool(false)))],
            Registration(),
            [Signal(true), Callable(ClrRuntimeCallableKind.CaptureCancellationToken), Record(("tokenAborted", Bool(true)))]),
        // UnsafeRegister 只是不捕获 ExecutionContext，浏览器没有 ExecutionContext，行为必须与 Register 一致。
        Captured(
            "cancellation.token.unsafe-register-with-state.invokes-callback-on-canceled-token",
            UnsafeRegisterWithState,
            TokenModule,
            [Invoke(CreateToken, Bool(true)), Callable(ClrRuntimeCallableKind.CaptureCancellationState), Record(("canceled", Bool(false)))],
            Registration(),
            [Signal(true), Callable(ClrRuntimeCallableKind.CaptureCancellationState), Record(("canceled", Bool(true)))]),
        Captured(
            "cancellation.token.unsafe-register-with-state-and-token.passes-token",
            UnsafeRegisterWithStateAndToken,
            TokenModule,
            [Invoke(CreateToken, Bool(true)), Callable(ClrRuntimeCallableKind.CaptureCancellationToken), Record(("tokenAborted", Bool(false)))],
            Registration(),
            [Signal(true), Callable(ClrRuntimeCallableKind.CaptureCancellationToken), Record(("tokenAborted", Bool(true)))]),
        // Dispose() 丢掉 Unregister() 的布尔结果，所以只能看到 undefined；真正的摘除效果由后面
        // 那个 sequence 场景证明：dispose 之后再 Unregister() 已经没有 handler 可摘。
        Success("cancellation.registration.dispose", RegistrationDispose, RegistrationModule, [Invoke(Register, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity))], Undefined()),
        Success(
            "cancellation.registration.dispose-unregisters",
            Unregister,
            RegistrationModule,
            [Sequence(
                Reference(RegistrationRef, Invoke(Register, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity))),
                Invoke(RegistrationDispose, Reference(RegistrationRef, Null())),
                Reference(RegistrationRef, Null()))],
            Bool(false)),
        // DisposeAsync() 返回一枚已完成的 Promise，runner 会 await 它，因此编码结果是 undefined。
        Success("cancellation.registration.dispose-async", RegistrationDisposeAsync, RegistrationModule, [Invoke(Register, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity))], Undefined()),
        Success(
            "cancellation.registration.dispose-async-unregisters",
            Unregister,
            RegistrationModule,
            [Sequence(
                Reference(RegistrationRef, Invoke(Register, Invoke(NoneToken), Callable(ClrRuntimeCallableKind.Identity))),
                Invoke(RegistrationDisposeAsync, Reference(RegistrationRef, Null())),
                Reference(RegistrationRef, Null()))],
            Bool(false)),
        // 延迟构造只是"排定一次延迟取消"，token 本身仍未取消；-1 表示永不自动取消，不排定定时器。
        Success("cancellation.source.create-with-infinite-delay", SourceWithDelay, SourceModule, [Span(InfiniteTicks)], Source(false)),
        Success("cancellation.source.create-with-infinite-milliseconds", SourceWithMilliseconds, SourceModule, [Number(-1)], Source(false)),
        // setTimeout 会静默钳位越界延迟，把"永不自动取消"变成"下一 tick 就取消"，因此必须显式失败。
        Failure("cancellation.source.create-with-milliseconds-out-of-range", SourceWithMilliseconds, SourceModule, [Number(2147483648)], "ArgumentOutOfRangeException"),
        Failure("cancellation.source.create-with-delay-out-of-range", SourceWithDelay, SourceModule, [Span(-20000)], "ArgumentOutOfRangeException"),

        Success("cancellation.source.cancel-after-infinite", CancelAfter, SourceModule, [Invoke(SourceWithMilliseconds, Number(-1)), Number(-1)], Undefined()),
        Failure("cancellation.source.cancel-after-out-of-range", CancelAfter, SourceModule, [Invoke(SourceWithMilliseconds, Number(-1)), Number(-2)], "ArgumentOutOfRangeException"),
        Success("cancellation.source.cancel-after-delay-infinite", CancelAfterDelay, SourceModule, [Invoke(SourceWithMilliseconds, Number(-1)), Span(InfiniteTicks)], Undefined()),
        Failure("cancellation.source.cancel-after-delay-out-of-range", CancelAfterDelay, SourceModule, [Invoke(SourceWithMilliseconds, Number(-1)), Span(21474836480000)], "ArgumentOutOfRangeException"),

        // Dispose() 只清除待触发的延迟取消（走 WeakMap 的存在性探测 + clearTimeout），从不 abort。
        Success("cancellation.source.dispose-clears-pending-cancel", SourceDispose, SourceModule, [Invoke(SourceWithMilliseconds, Number(0))], Undefined()),

        // 链接后的 source 仍可独立 Cancel()，所以必须是一枚新 controller；输入已取消时 AbortSignal.any
        // 的结果也已 abort，此时 abort 事件不会再派发，运行时必须立刻把取消转发到新 controller 上。
        Success("cancellation.source.linked-from-none", CreateLinked, SourceModule, [Invoke(NoneToken)], Source(false)),
        Success("cancellation.source.linked-from-canceled-token", CreateLinked, SourceModule, [Invoke(CreateToken, Bool(true))], Source(true)),
        Success("cancellation.source.linked-pair-uncanceled", CreateLinkedPair, SourceModule, [Invoke(NoneToken), Invoke(DefaultToken)], Source(false)),
        Success("cancellation.source.linked-pair-with-canceled", CreateLinkedPair, SourceModule, [Invoke(NoneToken), Invoke(CreateToken, Bool(true))], Source(true))
    ];

    // AbortSignal / AbortController 的状态在原型 getter 上，由 runner 的 encode 分支显式投影。
    private static ClrRuntimeValue Signal(bool aborted)
        => Record(("aborted", Bool(aborted)));

    private static ClrRuntimeValue Source(bool aborted)
        => Record(("signal", Signal(aborted)));

    // 生成的 JCancellationTokenRegistration 把 signal/handler 放在 #private 字段上，
    // 因此没有可枚举的自有属性，编码结果是空记录。
    private static ClrRuntimeValue Registration() => Record();

    private static ClrRuntimeValue Span(long ticks) => Invoke(TimeSpanTicks, Big(ticks));

    private static ClrRuntimeScenario Success(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected)
        => new(id, member, modulePath, arguments, expected);

    private static ClrRuntimeScenario Failure(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        string expectedErrorContains)
        => new(id, member, modulePath, arguments, null, expectedErrorContains);

    private static ClrRuntimeScenario Captured(
        string id,
        string member,
        string modulePath,
        IReadOnlyList<ClrRuntimeValue> arguments,
        ClrRuntimeValue expected,
        IReadOnlyList<ClrRuntimeValue> expectedArguments)
        => new(id, member, modulePath, arguments, expected, ExpectedArguments: expectedArguments);

    private static ClrRuntimeValue Invoke(string member, params ClrRuntimeValue[] arguments)
        => ClrRuntimeValue.Invoke(member, arguments);

    private static ClrRuntimeValue Sequence(params ClrRuntimeValue[] steps) => ClrRuntimeValue.Sequence(steps);
    private static ClrRuntimeValue Reference(string id, ClrRuntimeValue value) => ClrRuntimeValue.Reference(id, value);
    private static ClrRuntimeValue Record(params (string Name, ClrRuntimeValue Value)[] properties) => ClrRuntimeValue.Record(properties);
    private static ClrRuntimeValue Callable(ClrRuntimeCallableKind kind) => ClrRuntimeValue.Callable(kind);
    private static ClrRuntimeValue Null() => ClrRuntimeValue.Null();
    private static ClrRuntimeValue Undefined() => ClrRuntimeValue.Undefined();
    private static ClrRuntimeValue Number(double value) => ClrRuntimeValue.Number(value);
    private static ClrRuntimeValue Big(long value) => ClrRuntimeValue.BigInt(value);
    private static ClrRuntimeValue Bool(bool value) => ClrRuntimeValue.Boolean(value);
}
