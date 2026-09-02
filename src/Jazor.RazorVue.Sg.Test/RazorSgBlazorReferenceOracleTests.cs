using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Captures behavior from the ASP.NET Core reference assemblies that the RazorVue adapters
/// claim to preserve. These tests are an oracle only; they do not make the framework runtime a
/// RazorVue implementation dependency.
/// </summary>
[TestClass]
public sealed class RazorSgBlazorReferenceOracleTests
{
    [TestMethod]
    public void BlazorReferenceParameterView_SetsKnownValuesAndPreservesSparseDefaults()
    {
        var parameters = ParameterView.FromDictionary(
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Title"] = "next",
                ["Count"] = 3,
                ["Optional"] = null
            });
        var target = new ReferenceParameterTarget();

        parameters.SetParameterProperties(target);

        Assert.AreEqual("next", target.Title);
        Assert.AreEqual(3, target.Count);
        Assert.IsNull(target.Optional);
        Assert.AreEqual("default", target.Missing);
        Assert.IsTrue(parameters.TryGetValue<string>("Title", out var title));
        Assert.AreEqual("next", title);
        Assert.IsFalse(
            ParameterView.FromDictionary(new Dictionary<string, object?>(StringComparer.Ordinal))
                .TryGetValue<string>("Title", out _));
        CollectionAssert.AreEquivalent(
            new[] { "Title", "Count", "Optional" },
            parameters.ToDictionary().Keys.ToArray());
    }

    [TestMethod]
    public void BlazorReferenceParameterView_RejectsUnknownParameterNames()
    {
        var parameters = ParameterView.FromDictionary(
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["Unknown"] = 1
            });

        var error = Assert.ThrowsExactly<InvalidOperationException>(
            () => parameters.SetParameterProperties(new ReferenceParameterTarget()));

        StringAssert.Contains(error.Message, "Unknown", StringComparison.Ordinal);
        StringAssert.Contains(error.Message, "does not have a property", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BlazorReferenceParameterView_AppliesValuesBeforePropagatingAsyncLifecycleFailure()
    {
        var target = new ReferenceLifecycleTarget();

        var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => target.SetParametersAsync(ParameterView.FromDictionary(
                new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["Title"] = "updated"
                })));

        Assert.AreEqual("updated", target.Title);
        StringAssert.Contains(error.Message, "reference-lifecycle-failure", StringComparison.Ordinal);
    }

    [TestMethod]
    public void BlazorReferenceInjectActivator_ResolvesPropertyAndReportsMissingProvider()
    {
        var activator = GetReferencePropertyActivator(typeof(ReferenceInjectTarget));
        var service = new ReferenceService { Label = "reference" };
        var provider = new ServiceCollection()
            .AddSingleton(service)
            .BuildServiceProvider();
        var target = new ReferenceInjectTarget();

        activator(provider, target);

        Assert.AreSame(service, target.Service);

        var error = Assert.ThrowsExactly<InvalidOperationException>(
            () => activator(new ServiceCollection().BuildServiceProvider(), new ReferenceInjectTarget()));
        StringAssert.Contains(error.Message, "ReferenceInjectTarget", StringComparison.Ordinal);
        StringAssert.Contains(error.Message, "ReferenceService", StringComparison.Ordinal);
    }

    [TestMethod]
    public void BlazorReferenceInjectActivator_PreservesScopedServiceIdentityWithinScope()
    {
        var activator = GetReferencePropertyActivator(typeof(ReferenceInjectTarget));
        using var provider = new ServiceCollection()
            .AddScoped<ReferenceService>()
            .BuildServiceProvider();
        using var scope = provider.CreateScope();
        var first = new ReferenceInjectTarget();
        var second = new ReferenceInjectTarget();

        activator(scope.ServiceProvider, first);
        activator(scope.ServiceProvider, second);

        Assert.IsNotNull(first.Service);
        Assert.AreSame(first.Service, second.Service);
    }

    [TestMethod]
    public void BlazorReferenceCascadingValue_MatchesNameAndTypeAndPublishesCurrentValue()
    {
        // CascadingValue.SetParametersAsync needs a renderer handle because the reference
        // component notifies subscribers after every parameter update. For the supplier
        // contract itself, assigning the public parameter properties is the smallest faithful
        // fixture and avoids inventing a renderer just for the oracle.
#pragma warning disable BL0005
        var provider = new CascadingValue<string>
        {
            Value = "dark",
            Name = "theme",
            IsFixed = false
        };
#pragma warning restore BL0005

        var matching = CreateCascadingParameterInfo("theme", "Theme", typeof(string));
        var wrongName = CreateCascadingParameterInfo("other", "Theme", typeof(string));
        var wrongType = CreateCascadingParameterInfo("theme", "Theme", typeof(int));

        Assert.IsTrue(TryReadCascadingValue(provider, matching, out var value));
        Assert.AreEqual("dark", value);
        Assert.IsFalse(TryReadCascadingValue(provider, wrongName, out _));
        Assert.IsFalse(TryReadCascadingValue(provider, wrongType, out _));
    }

    [TestMethod]
    public void BlazorReferenceCascadingValue_UsesTypeWhenNameIsOmitted()
    {
#pragma warning disable BL0005
        var provider = new CascadingValue<string>
        {
            Value = "unnamed",
            Name = null,
            IsFixed = true
        };
#pragma warning restore BL0005

        var matching = CreateCascadingParameterInfo(null!, "Theme", typeof(string));
        var wrongType = CreateCascadingParameterInfo(null!, "Theme", typeof(int));

        Assert.IsTrue(TryReadCascadingValue(provider, matching, out var value));
        Assert.AreEqual("unnamed", value);
        Assert.IsFalse(TryReadCascadingValue(provider, wrongType, out _));
    }

    [TestMethod]
    public async Task BlazorReferenceNavigationManager_ReportsOptionsLocationEventsAndCancellation()
    {
        var navigation = new ReferenceNavigationManager();
        navigation.Start("https://example.test/app/", "https://example.test/app/start");

        Assert.AreEqual("https://example.test/app/", navigation.BaseUri);
        Assert.AreEqual("https://example.test/app/start", navigation.Uri);
        Assert.AreEqual("start", navigation.ToBaseRelativePath(navigation.Uri));
        Assert.AreEqual(
            "https://example.test/app/orders?id=1",
            navigation.ToAbsoluteUri("orders?id=1").ToString());

        var locationEvents = new List<LocationChangedEventArgs>();
        navigation.LocationChanged += (_, args) => locationEvents.Add(args);
        navigation.SetHistoryEntryState("initial");
        navigation.NotifyLocationChangedForTest(isNavigationIntercepted: true);

        Assert.HasCount(1, locationEvents);
        Assert.AreEqual("https://example.test/app/start", locationEvents[0].Location);
        Assert.IsTrue(locationEvents[0].IsNavigationIntercepted);
        Assert.AreEqual("initial", locationEvents[0].HistoryEntryState);

        navigation.NavigateTo("https://example.test/app/next", new NavigationOptions
        {
            ReplaceHistoryEntry = true,
            HistoryEntryState = "next-state"
        });
        Assert.AreEqual(
            "options:https://example.test/app/next|False|True|next-state",
            navigation.Calls[^1]);

        navigation.NavigateTo("https://example.test/app/force", forceLoad: true);
        Assert.AreEqual("bool:https://example.test/app/force|True", navigation.Calls[^1]);

        var observedTargets = new List<string>();
        navigation.AddHandler(context =>
        {
            observedTargets.Add(context.TargetLocation);
            if (context.TargetLocation.EndsWith("blocked", StringComparison.Ordinal))
                context.PreventNavigation();
            return ValueTask.CompletedTask;
        });

        Assert.IsTrue(await navigation.NotifyLocationChangingForTest(
            "https://example.test/app/allowed", "allowed-state", isNavigationIntercepted: false));
        Assert.IsFalse(await navigation.NotifyLocationChangingForTest(
            "https://example.test/app/blocked", "blocked-state", isNavigationIntercepted: false));
        CollectionAssert.AreEqual(
            new[]
            {
                "https://example.test/app/allowed",
                "https://example.test/app/blocked"
            },
            observedTargets);
    }

    [TestMethod]
    public async Task BlazorReferenceNavigationManager_SupersedesPendingLocationChangingDispatch()
    {
        var navigation = new ReferenceNavigationManager();
        navigation.Start("https://example.test/app/", "https://example.test/app/start");
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var canceled = new List<string>();

        navigation.AddHandler(context =>
        {
            firstStarted.TrySetResult(true);
            context.CancellationToken.Register(() =>
            {
                canceled.Add(context.TargetLocation);
                releaseFirst.TrySetResult(true);
            });
            return new ValueTask(releaseFirst.Task);
        });

        var first = navigation.NotifyLocationChangingForTest(
            "https://example.test/app/first", null, isNavigationIntercepted: false);
        await firstStarted.Task;
        var second = navigation.NotifyLocationChangingForTest(
            "https://example.test/app/second", null, isNavigationIntercepted: false);

        Assert.IsTrue(await second);
        Assert.IsFalse(await first);
        CollectionAssert.AreEqual(
            new[]
            {
                "https://example.test/app/first",
                "https://example.test/app/second"
            },
            canceled);
    }

    [TestMethod]
    public async Task BlazorReferenceNavigationManager_DisposesLocationChangingRegistration()
    {
        var navigation = new ReferenceNavigationManager();
        navigation.Start("https://example.test/app/", "https://example.test/app/start");
        var invocations = 0;
        var registration = navigation.AddHandler(_ =>
        {
            invocations++;
            return ValueTask.CompletedTask;
        });

        registration.Dispose();

        Assert.IsTrue(await navigation.NotifyLocationChangingForTest(
            "https://example.test/app/after-dispose", null, isNavigationIntercepted: false));
        Assert.AreEqual(0, invocations);
    }

    private static Action<IServiceProvider, IComponent> GetReferencePropertyActivator(Type componentType)
    {
        var activatorType = typeof(ComponentBase).Assembly.GetType(
            "Microsoft.AspNetCore.Components.DefaultComponentPropertyActivator",
            throwOnError: true)!;
        var activator = Activator.CreateInstance(activatorType, nonPublic: true)!;
        var getActivator = activatorType.GetMethod(
            "GetActivator",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        return (Action<IServiceProvider, IComponent>)getActivator.Invoke(activator, [componentType])!;
    }

    private static object CreateCascadingParameterInfo(string name, string propertyName, Type propertyType)
    {
        var infoType = typeof(CascadingParameterInfo);
        var constructor = infoType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            [typeof(CascadingParameterAttributeBase), typeof(string), typeof(Type)],
            modifiers: null)!;
        return constructor.Invoke(
            [new CascadingParameterAttribute { Name = name }, propertyName, propertyType]);
    }

    private static bool TryReadCascadingValue<T>(T provider, object parameterInfo, out object? value)
        where T : class
    {
        var supplierType = typeof(CascadingValue<>).Assembly.GetType(
            "Microsoft.AspNetCore.Components.ICascadingValueSupplier",
            throwOnError: true)!;
        var canSupply = supplierType.GetMethod(
            "CanSupplyValue",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        var getCurrentValue = supplierType.GetMethod(
            "GetCurrentValue",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        var args = new object?[] { parameterInfo };
        var result = (bool)canSupply.Invoke(provider, args)!;
        value = result ? getCurrentValue.Invoke(provider, [null, args[0]]) : null;
        return result;
    }

    private sealed class ReferenceParameterTarget
    {
        [Parameter]
        public string Title { get; set; } = "default";

        [Parameter]
        public int Count { get; set; } = 7;

        [Parameter]
        public string? Optional { get; set; } = "fallback";

        public string Missing { get; set; } = "default";
    }

    private sealed class ReferenceLifecycleTarget : ComponentBase
    {
        [Parameter]
        public string Title { get; set; } = "default";

        protected override Task OnParametersSetAsync()
            => Task.FromException(new InvalidOperationException("reference-lifecycle-failure"));
    }

    private sealed class ReferenceService
    {
        public string Label { get; set; } = string.Empty;
    }

    private sealed class ReferenceInjectTarget : ComponentBase
    {
        [Inject]
        public ReferenceService Service { get; set; } = null!;
    }

    private sealed class ReferenceNavigationManager : NavigationManager
    {
        public List<string> Calls { get; } = [];

        public void Start(string baseUri, string uri)
            => Initialize(baseUri, uri);

        public void SetHistoryEntryState(string? state)
            => HistoryEntryState = state;

        public void NotifyLocationChangedForTest(bool isNavigationIntercepted)
            => NotifyLocationChanged(isNavigationIntercepted);

        public ValueTask<bool> NotifyLocationChangingForTest(
            string targetLocation,
            string? historyEntryState,
            bool isNavigationIntercepted)
            => NotifyLocationChangingAsync(targetLocation, historyEntryState, isNavigationIntercepted);

        public IDisposable AddHandler(
            Func<LocationChangingContext, ValueTask> handler)
            => RegisterLocationChangingHandler(handler);

        protected override void NavigateToCore(string uri, bool forceLoad)
            => Calls.Add($"bool:{uri}|{forceLoad}");

        protected override void NavigateToCore(string uri, NavigationOptions options)
            => Calls.Add(
                $"options:{uri}|{options.ForceLoad}|{options.ReplaceHistoryEntry}|{options.HistoryEntryState ?? "null"}");

        protected override void SetNavigationLockState(bool value)
        {
        }
    }
}
