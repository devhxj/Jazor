using System.Threading;
using System.Runtime.CompilerServices;

namespace Jazor.Analyzer.RazorVue.Generation;

internal static class RazorSourceGeneratorBootstrapState
{
    private static int _attempted;
    private static int _installed;
    private static int _razorAssemblyObserved;
    private static int _patchAttempted;
    private static int _generatorTypeFound;
    private static int _initializeMethodFound;
    private static int _postfixMethodFound;
    private static int _patchSucceeded;
    private static int _patchFailed;
    private static int _patchUnavailable;
    private static string? _razorSourceGeneratorAssemblyVersion;
    private static string? _razorSourceGeneratorModuleVersionId;
    private static int _razorSourceGeneratorInitializeMethodIlLength;
    private static string? _razorSourceGeneratorInitializeMethodIlSha256;
    private static int _postfixInvoked;
    private static int _implementationSourceOutputHookInstalled;
    private static int _implementationSourceOutputObserved;
    private static int _tailOutputRegistered;
    private static int _tailOutputRegistrationVersion;
    private static int _testHookObserved;
    private static string? _failure;
    private static string? _tailOutputRegistrationKind;
    private static readonly object ContextSync = new();
    private static ConditionalWeakTable<object, TailOutputRegistration> _tailOutputRegisteredContexts = new();

    internal static void MarkAttempted()
        => Interlocked.Exchange(ref _attempted, 1);

    internal static bool HasAttempted()
        => Volatile.Read(ref _attempted) != 0;

    internal static void MarkInstalled()
        => Interlocked.Exchange(ref _installed, 1);

    internal static bool IsInstalled()
        => Volatile.Read(ref _installed) != 0;

    internal static void MarkRazorAssemblyObserved()
        => Interlocked.Exchange(ref _razorAssemblyObserved, 1);

    internal static void MarkPatchAttempted()
        => Interlocked.Exchange(ref _patchAttempted, 1);

    internal static void MarkGeneratorTypeFound()
        => Interlocked.Exchange(ref _generatorTypeFound, 1);

    internal static void MarkInitializeMethodFound()
        => Interlocked.Exchange(ref _initializeMethodFound, 1);

    internal static void MarkPostfixMethodFound()
        => Interlocked.Exchange(ref _postfixMethodFound, 1);

    internal static void MarkPatchSucceeded()
        => Interlocked.Exchange(ref _patchSucceeded, 1);

    internal static void MarkCompatibilityValidated(RazorSourceGeneratorCompatibilityShape shape)
    {
        if (shape is null)
            throw new ArgumentNullException(nameof(shape));

        _razorSourceGeneratorAssemblyVersion = shape.AssemblyVersion;
        _razorSourceGeneratorModuleVersionId = shape.ModuleVersionId;
        _razorSourceGeneratorInitializeMethodIlSha256 = shape.InitializeMethodIlSha256;
        Interlocked.Exchange(ref _razorSourceGeneratorInitializeMethodIlLength, shape.InitializeMethodIlLength);
    }

    internal static void MarkPatchFailed(string failure)
    {
        _failure = failure;
        Interlocked.Exchange(ref _patchFailed, 1);
    }

    internal static void MarkPatchUnavailable(string failure)
    {
        _failure = failure;
        Interlocked.Exchange(ref _patchUnavailable, 1);
    }

    internal static void MarkPostfixInvoked()
        => Interlocked.Exchange(ref _postfixInvoked, 1);

    internal static void MarkImplementationSourceOutputHookInstalled()
        => Interlocked.Exchange(ref _implementationSourceOutputHookInstalled, 1);

    internal static void MarkImplementationSourceOutputObserved()
        => Interlocked.Exchange(ref _implementationSourceOutputObserved, 1);

    internal static void MarkTailOutputRegistered(object? contextKey = null, string? registrationKind = null)
    {
        Interlocked.Exchange(ref _tailOutputRegistered, 1);
        Interlocked.Increment(ref _tailOutputRegistrationVersion);
        if (!string.IsNullOrWhiteSpace(registrationKind))
            _tailOutputRegistrationKind = registrationKind;

        if (contextKey is null)
            return;

        lock (ContextSync)
        {
            _tailOutputRegisteredContexts.GetOrCreateValue(contextKey).MarkRegistered(registrationKind);
        }
    }

    internal static void MarkTestHookObserved()
        => Interlocked.Exchange(ref _testHookObserved, 1);

    internal static bool WasTestHookObserved()
        => Volatile.Read(ref _testHookObserved) != 0;

    internal static int GetTailOutputRegistrationVersion()
        => Volatile.Read(ref _tailOutputRegistrationVersion);

    internal static RazorSourceGeneratorBootstrapTrace CreateTrace(object? contextKey = null)
        => CreateTrace(contextKey, tailOutputRegistrationVersionBeforeInitialize: null);

    internal static RazorSourceGeneratorBootstrapTrace CreateTrace(
        object? contextKey,
        int? tailOutputRegistrationVersionBeforeInitialize)
        => new(
            HasAttempted: Volatile.Read(ref _attempted) != 0,
            IsInstalled: Volatile.Read(ref _installed) != 0,
            RazorAssemblyObserved: Volatile.Read(ref _razorAssemblyObserved) != 0,
            PatchAttempted: Volatile.Read(ref _patchAttempted) != 0,
            GeneratorTypeFound: Volatile.Read(ref _generatorTypeFound) != 0,
            InitializeMethodFound: Volatile.Read(ref _initializeMethodFound) != 0,
            PostfixMethodFound: Volatile.Read(ref _postfixMethodFound) != 0,
            PatchSucceeded: Volatile.Read(ref _patchSucceeded) != 0,
            PatchFailed: Volatile.Read(ref _patchFailed) != 0,
            PatchUnavailable: Volatile.Read(ref _patchUnavailable) != 0,
            RazorSourceGeneratorAssemblyVersion: _razorSourceGeneratorAssemblyVersion ?? string.Empty,
            RazorSourceGeneratorModuleVersionId: _razorSourceGeneratorModuleVersionId ?? string.Empty,
            RazorSourceGeneratorInitializeMethodIlLength: Volatile.Read(ref _razorSourceGeneratorInitializeMethodIlLength),
            RazorSourceGeneratorInitializeMethodIlSha256: _razorSourceGeneratorInitializeMethodIlSha256 ?? string.Empty,
            PostfixInvoked: Volatile.Read(ref _postfixInvoked) != 0,
            ImplementationSourceOutputHookInstalled: Volatile.Read(ref _implementationSourceOutputHookInstalled) != 0,
            ImplementationSourceOutputObserved: Volatile.Read(ref _implementationSourceOutputObserved) != 0,
            TailOutputRegistered: Volatile.Read(ref _tailOutputRegistered) != 0,
            CurrentContextKeyAvailable: contextKey is not null,
            TailOutputRegisteredForCurrentContext: IsTailOutputRegisteredForContext(
                contextKey,
                tailOutputRegistrationVersionBeforeInitialize),
            TailOutputRegistrationKind: GetTailOutputRegistrationKind(contextKey),
            TestHookObserved: Volatile.Read(ref _testHookObserved) != 0,
            Failure: _failure);

    internal static bool HasTailOutputRegistrationAfter(int version)
        => Volatile.Read(ref _tailOutputRegistrationVersion) > version;

    private static bool IsTailOutputRegisteredForContext(
        object? contextKey,
        int? tailOutputRegistrationVersionBeforeInitialize)
    {
        if (tailOutputRegistrationVersionBeforeInitialize is int version)
            return HasTailOutputRegistrationAfter(version);

        if (contextKey is null)
            return false;

        lock (ContextSync)
        {
            return _tailOutputRegisteredContexts.TryGetValue(contextKey, out var registration) &&
                   registration.IsRegistered;
        }
    }

    private static string GetTailOutputRegistrationKind(object? contextKey)
    {
        if (contextKey is null)
            return _tailOutputRegistrationKind ?? string.Empty;

        lock (ContextSync)
        {
            if (_tailOutputRegisteredContexts.TryGetValue(contextKey, out var registration))
                return registration.Kind;
        }

        return string.Empty;
    }

    internal static void ResetForTests()
    {
        Interlocked.Exchange(ref _attempted, 0);
        Interlocked.Exchange(ref _installed, 0);
        Interlocked.Exchange(ref _razorAssemblyObserved, 0);
        Interlocked.Exchange(ref _patchAttempted, 0);
        Interlocked.Exchange(ref _generatorTypeFound, 0);
        Interlocked.Exchange(ref _initializeMethodFound, 0);
        Interlocked.Exchange(ref _postfixMethodFound, 0);
        Interlocked.Exchange(ref _patchSucceeded, 0);
        Interlocked.Exchange(ref _patchFailed, 0);
        Interlocked.Exchange(ref _patchUnavailable, 0);
        Interlocked.Exchange(ref _razorSourceGeneratorInitializeMethodIlLength, 0);
        Interlocked.Exchange(ref _postfixInvoked, 0);
        Interlocked.Exchange(ref _implementationSourceOutputHookInstalled, 0);
        Interlocked.Exchange(ref _implementationSourceOutputObserved, 0);
        Interlocked.Exchange(ref _tailOutputRegistered, 0);
        Interlocked.Exchange(ref _tailOutputRegistrationVersion, 0);
        Interlocked.Exchange(ref _testHookObserved, 0);
        lock (ContextSync)
        {
            _tailOutputRegisteredContexts = new ConditionalWeakTable<object, TailOutputRegistration>();
        }

        _failure = null;
        _tailOutputRegistrationKind = null;
        _razorSourceGeneratorAssemblyVersion = null;
        _razorSourceGeneratorModuleVersionId = null;
        _razorSourceGeneratorInitializeMethodIlSha256 = null;
    }

    private sealed class TailOutputRegistration
    {
        private int _registered;
        private string? _kind;

        public bool IsRegistered => Volatile.Read(ref _registered) != 0;

        public string Kind => _kind ?? string.Empty;

        public void MarkRegistered(string? kind)
        {
            if (!string.IsNullOrWhiteSpace(kind))
                _kind = kind;

            Interlocked.Exchange(ref _registered, 1);
        }
    }
}

internal sealed record RazorSourceGeneratorBootstrapTrace(
    bool HasAttempted,
    bool IsInstalled,
    bool RazorAssemblyObserved,
    bool PatchAttempted,
    bool GeneratorTypeFound,
    bool InitializeMethodFound,
    bool PostfixMethodFound,
    bool PatchSucceeded,
    bool PatchFailed,
    bool PatchUnavailable,
    string RazorSourceGeneratorAssemblyVersion,
    string RazorSourceGeneratorModuleVersionId,
    int RazorSourceGeneratorInitializeMethodIlLength,
    string RazorSourceGeneratorInitializeMethodIlSha256,
    bool PostfixInvoked,
    bool ImplementationSourceOutputHookInstalled,
    bool ImplementationSourceOutputObserved,
    bool TailOutputRegistered,
    bool CurrentContextKeyAvailable,
    bool TailOutputRegisteredForCurrentContext,
    string TailOutputRegistrationKind,
    bool TestHookObserved,
    string? Failure);
