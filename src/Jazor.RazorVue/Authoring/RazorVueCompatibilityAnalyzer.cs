using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Authoring;

/// <summary>
/// Reports browser-environment incompatibilities directly in RazorVue author input.
/// It deliberately does not inspect Razor SG output: final Compilation remains the
/// sole owner of render-tree, closure, and module protocol diagnostics.
/// 这里只检查作者可高置信判断的浏览器边界，不能借 generated C# 重复最终 lowering 诊断。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorVueCompatibilityAnalyzer : DiagnosticAnalyzer
{
    private const string InjectAttributeMetadataName = "Microsoft.AspNetCore.Components.InjectAttribute";
    private const string ComponentBaseMetadataName = "Microsoft.AspNetCore.Components.ComponentBase";
    private const string IComponentMetadataName = "Microsoft.AspNetCore.Components.IComponent";
    private const string ParameterViewMetadataName = "Microsoft.AspNetCore.Components.ParameterView";
    private const string CascadingParameterAttributeMetadataName = "Microsoft.AspNetCore.Components.CascadingParameterAttribute";
    private const string SupplyParameterFromFormAttributeMetadataName = "Microsoft.AspNetCore.Components.SupplyParameterFromFormAttribute";
    private const string PersistentStateAttributeMetadataName = "Microsoft.AspNetCore.Components.PersistentStateAttribute";
    private const string DbContextMetadataName = "Microsoft.EntityFrameworkCore.DbContext";
    private const string PersistentComponentStateMetadataName = "Microsoft.AspNetCore.Components.PersistentComponentState";
    private static readonly ImmutableArray<string> ServerOnlyServiceMetadataNames =
    [
        "Microsoft.AspNetCore.Http.HttpContext",
        "Microsoft.AspNetCore.Http.HttpRequest",
        "Microsoft.AspNetCore.Http.HttpResponse",
        "Microsoft.AspNetCore.Http.IHttpContextAccessor",
        "Microsoft.AspNetCore.Hosting.IWebHostEnvironment",
        "Microsoft.Extensions.Hosting.IHostEnvironment",
        "Microsoft.AspNetCore.Identity.UserManager`1",
        "Microsoft.AspNetCore.Identity.SignInManager`1",
        "Microsoft.AspNetCore.Identity.RoleManager`1"
    ];
    private static readonly ImmutableArray<string> BrowserAdapterServiceMetadataNames =
    [
        "Microsoft.AspNetCore.Components.NavigationManager",
        "Microsoft.JSInterop.IJSRuntime",
        "Microsoft.JSInterop.IJSObjectReference"
    ];
    // These are framework services whose normal Blazor implementation is tied to a
    // server circuit/renderer. They are intentionally explicit: arbitrary application
    // services remain valid authoring surface and are checked only for activation shape.
    // 这里只列出高置信的 host 服务，避免把普通 typed browser client 误报成 server-only。
    private static readonly ImmutableArray<string> BrowserUnavailableServiceMetadataNames =
    [
        "Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedBrowserStorage",
        "Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage",
        "Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedSessionStorage",
        "Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler",
        "Microsoft.AspNetCore.Components.Server.Circuits.CircuitOptions",
        "Microsoft.AspNetCore.Components.Server.Circuits.CircuitClientProxy",
        "Microsoft.AspNetCore.Components.Server.Circuits.RemoteRenderer",
        // Authentication state needs an explicit host/provider and serialized claims contract;
        // treating the abstract Blazor provider as a normal browser service would defer that
        // missing protocol to a runtime inject failure.
        // 认证状态必须由宿主显式提供并定义 claims handoff，不能把抽象 provider 静默当作浏览器服务。
        "Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider",
        "Microsoft.AspNetCore.Components.PersistentComponentState",
        "Microsoft.AspNetCore.Components.IComponentActivator",
        "Microsoft.AspNetCore.Components.IComponentContext",
        "Microsoft.AspNetCore.Components.IComponentRenderMode"
    ];
    private static readonly ImmutableArray<(string TagName, string MetadataName)> UnsupportedRazorComponentContracts =
    [
        ("AuthorizeView", "Microsoft.AspNetCore.Components.Authorization.AuthorizeView"),
        ("AuthorizeRouteView", "Microsoft.AspNetCore.Components.Authorization.AuthorizeRouteView"),
        ("CascadingAuthenticationState", "Microsoft.AspNetCore.Components.Authorization.CascadingAuthenticationState"),
        ("DynamicComponent", "Microsoft.AspNetCore.Components.DynamicComponent"),
        ("CacheView", "Microsoft.AspNetCore.Components.CacheView"),
        ("ConfigureBrowser", "Microsoft.AspNetCore.Components.ConfigureBrowser"),
        ("ImportMap", "Microsoft.AspNetCore.Components.ImportMap"),
        ("ResourcePreloader", "Microsoft.AspNetCore.Components.ResourcePreloader"),
        ("BasePath", "Microsoft.AspNetCore.Components.Endpoints.BasePath"),
        ("DataAnnotationsValidator", "Microsoft.AspNetCore.Components.Forms.DataAnnotationsValidator"),
        ("EditForm", "Microsoft.AspNetCore.Components.Forms.EditForm"),
        ("ErrorBoundary", "Microsoft.AspNetCore.Components.Web.ErrorBoundary"),
        ("HeadContent", "Microsoft.AspNetCore.Components.Web.HeadContent"),
        ("HeadOutlet", "Microsoft.AspNetCore.Components.Web.HeadOutlet"),
        ("InputBase", "Microsoft.AspNetCore.Components.Forms.InputBase`1"),
        ("InputCheckbox", "Microsoft.AspNetCore.Components.Forms.InputCheckbox"),
        ("InputDate", "Microsoft.AspNetCore.Components.Forms.InputDate`1"),
        ("InputFile", "Microsoft.AspNetCore.Components.Forms.InputFile"),
        ("InputNumber", "Microsoft.AspNetCore.Components.Forms.InputNumber`1"),
        ("InputRadio", "Microsoft.AspNetCore.Components.Forms.InputRadio`1"),
        ("InputRadioGroup", "Microsoft.AspNetCore.Components.Forms.InputRadioGroup`1"),
        ("InputSelect", "Microsoft.AspNetCore.Components.Forms.InputSelect`1"),
        ("InputText", "Microsoft.AspNetCore.Components.Forms.InputText"),
        ("InputTextArea", "Microsoft.AspNetCore.Components.Forms.InputTextArea"),
        ("AntiforgeryToken", "Microsoft.AspNetCore.Components.Forms.AntiforgeryToken"),
        ("DisplayName", "Microsoft.AspNetCore.Components.Forms.DisplayName`1"),
        ("FormMappingScope", "Microsoft.AspNetCore.Components.Forms.FormMappingScope"),
        ("InputHidden", "Microsoft.AspNetCore.Components.Forms.InputHidden"),
        ("Label", "Microsoft.AspNetCore.Components.Forms.Label`1"),
        ("LayoutView", "Microsoft.AspNetCore.Components.LayoutView"),
        ("NavLink", "Microsoft.AspNetCore.Components.Routing.NavLink"),
        ("NavigationLock", "Microsoft.AspNetCore.Components.Routing.NavigationLock"),
        ("FocusOnNavigate", "Microsoft.AspNetCore.Components.Routing.FocusOnNavigate"),
        ("RouteView", "Microsoft.AspNetCore.Components.RouteView"),
        ("Router", "Microsoft.AspNetCore.Components.Routing.Router"),
        ("PageTitle", "Microsoft.AspNetCore.Components.Web.PageTitle"),
        ("EnvironmentView", "Microsoft.AspNetCore.Components.Web.EnvironmentView"),
        ("SectionContent", "Microsoft.AspNetCore.Components.Sections.SectionContent"),
        ("SectionOutlet", "Microsoft.AspNetCore.Components.Sections.SectionOutlet"),
        ("ValidationMessage", "Microsoft.AspNetCore.Components.Forms.ValidationMessage`1"),
        ("ValidationSummary", "Microsoft.AspNetCore.Components.Forms.ValidationSummary"),
        ("Virtualize", "Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize`1"),
        ("QuickGrid", "Microsoft.AspNetCore.Components.QuickGrid.QuickGrid`1")
    ];
    internal static readonly DiagnosticDescriptor BrowserIneligibleDbContext = new(
        id: "JAZORVCA001",
        title: "Injected DbContext cannot run in a RazorVue browser component",
        messageFormat: "Injected service '{0}' derives from Microsoft.EntityFrameworkCore.DbContext and requires a server process. RazorVue does not materialize DbContext in the browser; inject a browser-capable typed endpoint client instead.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("browser-services"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor BrowserIneligibleServerService = new(
        id: "JAZORVCA002",
        title: "Injected server service cannot run in a RazorVue browser component",
        messageFormat: "Injected service '{0}' is server-only ({1}) and cannot be materialized in a RazorVue browser bundle. Inject a browser-capable typed endpoint client or host adapter instead.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("browser-services"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor ParameterViewTryGetValueUnsupported = new(
        id: "JAZORVCA003",
        title: "ParameterView.TryGetValue is not available in the RazorVue adapter",
        messageFormat: "ParameterView member '{0}' is not part of RazorVue's supported compatibility adapter. Use the component's typed [Parameter] properties or a supported SetParameterProperties call; arbitrary ParameterView inspection cannot be materialized in the browser.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("parameter-lifecycle"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor ParameterViewEnumerationUnsupported = new(
        id: "JAZORVCA004",
        title: "ParameterView enumeration is not available in the RazorVue adapter",
        messageFormat: "ParameterView enumeration is not part of RazorVue's supported compatibility adapter. Use typed component parameters instead of enumerating the runtime parameter bag.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("parameter-lifecycle"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor ParameterViewToDictionaryUnsupported = new(
        id: "JAZORVCA005",
        title: "ParameterView.ToDictionary is not available in the RazorVue adapter",
        messageFormat: "ParameterView.ToDictionary is not part of RazorVue's supported compatibility adapter. Use typed component parameters or construct a dictionary from known values.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("parameter-lifecycle"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor InjectPropertyMustBeWritableAutoProperty = new(
        id: "JAZORVCA006",
        title: "Injected service property must be a writable auto-property",
        messageFormat: "Injected service property '{0}' cannot be activated by the RazorVue browser adapter because it is not a writable auto-property. Declare it as a normal settable property, for example '{1} Service {{ get; set; }} = null!;'.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("browser-services"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor BrowserAdapterServiceUnavailable = new(
        id: "JAZORVCA007",
        title: "Injected Blazor service has no RazorVue browser adapter",
        messageFormat: "Injected service '{0}' is a Blazor host service ({1}) without a RazorVue browser adapter in this profile. Register a typed browser adapter or move the operation behind a typed endpoint; page code stays standard [Inject]/@inject.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("browser-services"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor SsrStateHandoffUnavailable = new(
        id: "JAZORVCA011",
        title: "SSR state handoff is not available in the RazorVue browser adapter",
        messageFormat: "Razor API '{0}' requires a versioned RazorVue SSR/hydration state contract that is not available in this profile. Use an explicit typed endpoint/bootstrap payload, or keep the state on the server; do not rely on PersistentComponentState or server form handoff in a browser component.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("ssr-state-handoff"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor CascadingParameterUnsupported = new(
        id: "JAZORVCA008",
        title: "CascadingParameter property must be writable",
        messageFormat: "Cascading parameter '{0}' cannot be activated because the RazorVue browser adapter requires a writable auto-property. Keep the standard [CascadingParameter] shape and declare '{0}' with get; set; so the nearest CascadingValue can update it.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("cascading-parameters"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor RouteDirectiveRequiresHostAdapter = new(
        id: "JAZORVCA009",
        title: "Razor page route requires a RazorVue route host",
        messageFormat: "Razor directive '@page {0}' needs a RazorVue route host/registration that is not available in the current browser profile. Keep the page as standard Blazor Razor and register the route host, or expose it through an explicit typed host entry.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("routing"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    internal static readonly DiagnosticDescriptor BlazorComponentAdapterUnavailable = new(
        id: "JAZORVCA010",
        title: "Blazor built-in UI component is outside the RazorVue component contract",
        messageFormat: "Blazor built-in UI component '{0}' is outside RazorVue's component contract. Use a ComponentBase + IVueComponent component with an explicit import, or a typed TDesign/Vuetify/Element Plus component; do not depend on a historical standard-component adapter.",
        category: "Jazor.RazorVue.Compatibility",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink("component-adapters"),
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            BrowserIneligibleDbContext,
            BrowserIneligibleServerService,
            ParameterViewTryGetValueUnsupported,
            ParameterViewEnumerationUnsupported,
            ParameterViewToDictionaryUnsupported,
            InjectPropertyMustBeWritableAutoProperty,
            BrowserAdapterServiceUnavailable,
            SsrStateHandoffUnavailable,
            CascadingParameterUnsupported,
            RouteDirectiveRequiresHostAdapter,
            BlazorComponentAdapterUnavailable
        ];

    public override void Initialize(AnalysisContext context)
    {
        // Razor SG generated C# is intentionally outside this analyzer. The final generator
        // owns that protocol and already provides mapped JAZORVGA diagnostics when needed.
        // Razor SG generated C# 不能作为提前诊断输入，避免和 final Compilation 重复归责。
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            var dbContext = startContext.Compilation.GetTypeByMetadataName(DbContextMetadataName);
            var serverOnlyServices = ServerOnlyServiceMetadataNames
                .Select(metadataName =>
                {
                    var symbol = startContext.Compilation.GetTypeByMetadataName(metadataName);
                    return symbol is null
                        ? (ServerOnlyService?)null
                        : new ServerOnlyService(metadataName, symbol);
                })
                .Where(static service => service is not null)
                .Select(static service => service!.Value)
                .ToImmutableArray();
            var browserAdapterServices = BrowserAdapterServiceMetadataNames
                .Select(metadataName =>
                {
                    var symbol = startContext.Compilation.GetTypeByMetadataName(metadataName);
                    return symbol is null
                        ? (KnownService?)null
                        : new KnownService(metadataName, symbol);
                })
                .Where(static service => service is not null)
                .Select(static service => service!.Value)
                .ToImmutableArray();
            var browserUnavailableServices = BrowserUnavailableServiceMetadataNames
                .Select(metadataName =>
                {
                    var symbol = startContext.Compilation.GetTypeByMetadataName(metadataName);
                    return symbol is null
                        ? (KnownService?)null
                        : new KnownService(metadataName, symbol);
                })
                .Where(static service => service is not null)
                .Select(static service => service!.Value)
                .ToImmutableArray();
            var injectAttribute = startContext.Compilation.GetTypeByMetadataName(InjectAttributeMetadataName);
            var componentBase = startContext.Compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
            var componentContract = startContext.Compilation.GetTypeByMetadataName(IComponentMetadataName);
            var cascadingParameterAttribute = startContext.Compilation.GetTypeByMetadataName(CascadingParameterAttributeMetadataName);
            var supplyParameterFromFormAttribute = startContext.Compilation.GetTypeByMetadataName(SupplyParameterFromFormAttributeMetadataName);
            var persistentStateAttribute = startContext.Compilation.GetTypeByMetadataName(PersistentStateAttributeMetadataName);
            var hasComponentContract = componentBase is not null || componentContract is not null;
            var hasInjectAnalysis = injectAttribute is not null && hasComponentContract;
            var hasCascadingAnalysis = cascadingParameterAttribute is not null && hasComponentContract;
            var hasSsrStateAnalysis = (supplyParameterFromFormAttribute is not null || persistentStateAttribute is not null) && hasComponentContract;
            var parameterView = startContext.Compilation.GetTypeByMetadataName(ParameterViewMetadataName);
            var hasParameterViewAnalysis = parameterView is not null && hasComponentContract;
            var hasRazorAuthoringAnalysis = startContext.Options.AdditionalFiles.Any(static file =>
                file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase));

            // Do not short-circuit on the optional server-only metadata. Browser-service
            // activation (JAZORVCA006) and ParameterView rules remain useful in a minimal
            // Blazor project that references no EF/ASP.NET host assemblies.
            // 不能因为没有可选的 server-only 类型就跳过独立的作者面规则。
            if (!hasInjectAnalysis && !hasCascadingAnalysis && !hasParameterViewAnalysis && !hasSsrStateAnalysis && !hasRazorAuthoringAnalysis)
                return;

            if (hasInjectAnalysis)
            {
                var resolvedInjectAttribute = injectAttribute!;
                startContext.RegisterSymbolAction(
                    symbolContext => AnalyzeInjectedProperty(
                        (IPropertySymbol)symbolContext.Symbol,
                        resolvedInjectAttribute,
                        componentBase,
                        componentContract,
                        dbContext,
                        serverOnlyServices,
                        browserAdapterServices,
                        browserUnavailableServices,
                        symbolContext),
                    SymbolKind.Property);
            }

            if (hasParameterViewAnalysis)
            {
                var resolvedParameterView = parameterView!;
                startContext.RegisterOperationAction(
                    operationContext => AnalyzeParameterViewInvocation(
                        operationContext,
                        resolvedParameterView,
                        componentBase,
                        componentContract),
                    OperationKind.Invocation);
                startContext.RegisterOperationAction(
                    operationContext => AnalyzeParameterViewEnumeration(
                        operationContext,
                        resolvedParameterView,
                        componentBase,
                        componentContract),
                    OperationKind.Loop);
            }

            if (hasCascadingAnalysis)
            {
                var resolvedCascadingParameterAttribute = cascadingParameterAttribute!;
                startContext.RegisterSymbolAction(
                    symbolContext => AnalyzeCascadingParameterProperty(
                        (IPropertySymbol)symbolContext.Symbol,
                        resolvedCascadingParameterAttribute,
                        componentBase,
                        componentContract,
                        symbolContext),
                    SymbolKind.Property);
            }

            if (hasSsrStateAnalysis)
            {
                var resolvedSupplyParameterFromFormAttribute = supplyParameterFromFormAttribute;
                var resolvedPersistentStateAttribute = persistentStateAttribute;
                startContext.RegisterSymbolAction(
                    symbolContext => AnalyzeSsrStateProperty(
                        (IPropertySymbol)symbolContext.Symbol,
                        resolvedSupplyParameterFromFormAttribute,
                        resolvedPersistentStateAttribute,
                        componentBase,
                        componentContract,
                        symbolContext),
                    SymbolKind.Property);
            }

            if (hasRazorAuthoringAnalysis)
            {
                startContext.RegisterCompilationEndAction(endContext =>
                    AnalyzeRazorAuthoringFiles(
                        endContext,
                        dbContext,
                        serverOnlyServices,
                        browserAdapterServices,
                        browserUnavailableServices));
            }
        });
    }

    private static void AnalyzeParameterViewInvocation(
        OperationAnalysisContext context,
        INamedTypeSymbol parameterView,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract)
    {
        if (context.Operation is not IInvocationOperation invocation ||
            !IsAuthoredSource(invocation.Syntax) ||
            !IsComponent(GetContainingType(context.ContainingSymbol), componentBase, componentContract))
        {
            return;
        }

        var target = invocation.TargetMethod;
        if (IsParameterViewMethod(target, parameterView, "TryGetValue"))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ParameterViewTryGetValueUnsupported,
                invocation.Syntax.GetLocation(),
                target.Name));
            return;
        }

        if (!IsParameterViewMethod(target, parameterView, "ToDictionary"))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            ParameterViewToDictionaryUnsupported,
            invocation.Syntax.GetLocation(),
            target.Name));
    }

    private static void AnalyzeParameterViewEnumeration(
        OperationAnalysisContext context,
        INamedTypeSymbol parameterView,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract)
    {
        if (context.Operation is not IForEachLoopOperation loop ||
            !IsAuthoredSource(loop.Syntax) ||
            !IsComponent(GetContainingType(context.ContainingSymbol), componentBase, componentContract) ||
            !ContainsParameterView(loop.Collection, parameterView))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            ParameterViewEnumerationUnsupported,
            loop.Collection.Syntax.GetLocation()));
    }

    private static bool IsParameterViewMethod(
        IMethodSymbol method,
        INamedTypeSymbol parameterView,
        string methodName)
        => string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
           method.ContainingType is not null &&
           SymbolEqualityComparer.Default.Equals(
               method.ContainingType.OriginalDefinition,
               parameterView.OriginalDefinition);

    private static bool ContainsParameterView(IOperation operation, INamedTypeSymbol parameterView)
    {
        IOperation? current = operation;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(
                    current.Type?.OriginalDefinition,
                    parameterView.OriginalDefinition))
            {
                return true;
            }

            current = current switch
            {
                IConversionOperation conversion => conversion.Operand,
                IParenthesizedOperation parenthesized => parenthesized.Operand,
                _ => null
            };
            if (current is null)
                break;
        }

        return false;
    }

    private static INamedTypeSymbol? GetContainingType(ISymbol? symbol)
    {
        for (var current = symbol; current is not null; current = current.ContainingSymbol)
        {
            if (current is INamedTypeSymbol type)
                return type;
        }

        return null;
    }

    private static void AnalyzeInjectedProperty(
        IPropertySymbol property,
        INamedTypeSymbol injectAttribute,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract,
        INamedTypeSymbol? dbContext,
        ImmutableArray<ServerOnlyService> serverOnlyServices,
        ImmutableArray<KnownService> browserAdapterServices,
        ImmutableArray<KnownService> browserUnavailableServices,
        SymbolAnalysisContext context)
    {
        var attribute = property.GetAttributes().FirstOrDefault(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, injectAttribute));
        if (attribute is null ||
            !IsAuthoredSource(property) ||
            !IsComponent(property.ContainingType, componentBase, componentContract))
        {
            return;
        }

        if (dbContext is not null && DerivesFrom(property.Type, dbContext))
        {
            var dbLocation = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ??
                             GetAuthoredLocation(property);
            context.ReportDiagnostic(Diagnostic.Create(
                BrowserIneligibleDbContext,
                dbLocation,
                FormatTypeName(property.Type)));
            return;
        }

        if (FindServerOnlyService(property.Type, serverOnlyServices) is not { } serverOnly)
        {
            // A service that has an explicit browser adapter is still subject to the
            // writable-property activation contract, but must never be classified as
            // an unavailable host service.
            if (FindKnownService(property.Type, browserAdapterServices) is not null)
            {
                if (IsWritableAutoProperty(property))
                    return;

                ReportInjectPropertyShape(property, injectAttribute, context);
                return;
            }

            if (FindKnownService(property.Type, browserUnavailableServices) is { } unavailable)
            {
                var unavailableLocation = attribute.ApplicationSyntaxReference?
                        .GetSyntax(context.CancellationToken).GetLocation() ??
                    GetAuthoredLocation(property);
                if (string.Equals(unavailable.MetadataName, PersistentComponentStateMetadataName, StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        SsrStateHandoffUnavailable,
                        unavailableLocation,
                        FormatTypeName(property.Type)));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        BrowserAdapterServiceUnavailable,
                        unavailableLocation,
                        FormatTypeName(property.Type),
                        unavailable.MetadataName));
                }
                return;
            }

            if (IsWritableAutoProperty(property))
                return;

            ReportInjectPropertyShape(property, injectAttribute, context);
            return;
        }

        var location = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ??
                       GetAuthoredLocation(property);
        context.ReportDiagnostic(Diagnostic.Create(
            BrowserIneligibleServerService,
            location,
            FormatTypeName(property.Type),
            serverOnly.MetadataName));
    }

    private static bool IsWritableAutoProperty(IPropertySymbol property)
    {
        if (property.IsStatic || property.SetMethod is null || property.SetMethod.IsInitOnly)
            return false;

        foreach (var reference in property.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not PropertyDeclarationSyntax
                {
                    ExpressionBody: null,
                    AccessorList: { } accessors
                })
            {
                continue;
            }

            var setter = accessors.Accessors.FirstOrDefault(accessor =>
                accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration));
            if (setter is not null && setter.Body is null && setter.ExpressionBody is null)
            {
                var getter = accessors.Accessors.FirstOrDefault(accessor =>
                    accessor.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration));
                if (getter is not null && getter.Body is null && getter.ExpressionBody is null)
                    return true;
            }
        }

        return false;
    }

    private static void ReportInjectPropertyShape(
        IPropertySymbol property,
        INamedTypeSymbol injectAttribute,
        SymbolAnalysisContext context)
    {
        var location = property.GetAttributes().FirstOrDefault(candidate =>
                SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, injectAttribute))?
            .ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation() ??
            GetAuthoredLocation(property);
        context.ReportDiagnostic(Diagnostic.Create(
            InjectPropertyMustBeWritableAutoProperty,
            location,
            property.Name,
            FormatTypeName(property.Type)));
    }

    private static void AnalyzeCascadingParameterProperty(
        IPropertySymbol property,
        INamedTypeSymbol cascadingParameterAttribute,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract,
        SymbolAnalysisContext context)
    {
        var attribute = property.GetAttributes().FirstOrDefault(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, cascadingParameterAttribute));
        if (attribute is null ||
            !IsAuthoredSource(property) ||
            !IsComponent(property.ContainingType, componentBase, componentContract))
        {
            return;
        }

        // The normal Blazor cascading contract is now provided by the browser adapter. Only
        // reject an authored shape that cannot receive the resolved value; valid properties
        // stay completely quiet so developers do not learn a RazorVue-specific protocol.
        // 标准级联形状已经有 adapter；这里只诊断无法写入的作者属性，避免 Direct Support 噪音。
        if (IsWritableAutoProperty(property))
            return;

        var location = attribute.ApplicationSyntaxReference?
                           .GetSyntax(context.CancellationToken).GetLocation() ??
                       GetAuthoredLocation(property);
        context.ReportDiagnostic(Diagnostic.Create(
            CascadingParameterUnsupported,
            location,
            property.Name));
    }

    private static void AnalyzeSsrStateProperty(
        IPropertySymbol property,
        INamedTypeSymbol? supplyParameterFromFormAttribute,
        INamedTypeSymbol? persistentStateAttribute,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract,
        SymbolAnalysisContext context)
    {
        if (!IsAuthoredSource(property) ||
            !IsComponent(property.ContainingType, componentBase, componentContract))
        {
            return;
        }

        var attribute = property.GetAttributes().FirstOrDefault(candidate =>
            (supplyParameterFromFormAttribute is not null &&
             SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, supplyParameterFromFormAttribute)) ||
            (persistentStateAttribute is not null &&
             SymbolEqualityComparer.Default.Equals(candidate.AttributeClass, persistentStateAttribute)));
        if (attribute is null)
            return;

        var attributeName = attribute.AttributeClass?.Name ?? "SSR state handoff";
        if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
            attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);

        var location = attribute.ApplicationSyntaxReference?
                           .GetSyntax(context.CancellationToken).GetLocation() ??
                       GetAuthoredLocation(property);
        context.ReportDiagnostic(Diagnostic.Create(
            SsrStateHandoffUnavailable,
            location,
            attributeName));
    }

    private static void AnalyzeRazorAuthoringFiles(
        CompilationAnalysisContext context,
        INamedTypeSymbol? dbContext,
        ImmutableArray<ServerOnlyService> serverOnlyServices,
        ImmutableArray<KnownService> browserAdapterServices,
        ImmutableArray<KnownService> browserUnavailableServices)
    {
        foreach (var file in context.Options.AdditionalFiles
                     .Where(static file => file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(static file => file.Path, StringComparer.OrdinalIgnoreCase))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var text = file.GetText(context.CancellationToken);
            if (text is null)
                continue;

            AnalyzeRazorInjectDirectives(
                context,
                file,
                text,
                dbContext,
                serverOnlyServices,
                browserAdapterServices,
                browserUnavailableServices);
            // Supported @page declarations are consumed by the generated route catalog and
            // remain ordinary Blazor authoring; do not emit the retired host-missing warning.
            // @page 由 route catalog 消费，页面作者不应看到过时的 JAZORVCA009。
            AnalyzeUnsupportedRazorComponents(context, file, text);
        }
    }

    // Kept as a small compatibility wrapper for callers/tests that used the original
    // helper name while the authoring scan grew beyond @inject directives.
    private static void AnalyzeRazorInjectDirectives(
        CompilationAnalysisContext context,
        INamedTypeSymbol? dbContext,
        ImmutableArray<ServerOnlyService> serverOnlyServices)
    {
        AnalyzeRazorAuthoringFiles(
            context,
            dbContext,
            serverOnlyServices,
            ImmutableArray<KnownService>.Empty,
            ImmutableArray<KnownService>.Empty);
    }

    private static void AnalyzeRazorInjectDirectives(
        CompilationAnalysisContext context,
        AdditionalText file,
        SourceText text,
        INamedTypeSymbol? dbContext,
        ImmutableArray<ServerOnlyService> serverOnlyServices,
        ImmutableArray<KnownService> browserAdapterServices,
        ImmutableArray<KnownService> browserUnavailableServices)
    {
        foreach (var directive in EnumerateInjectDirectives(text))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var serviceType = ResolveDirectiveType(
                context.Compilation,
                directive.TypeName,
                type => ClassifyInjectionType(
                    type,
                    dbContext,
                    serverOnlyServices,
                    browserAdapterServices,
                    browserUnavailableServices),
                context.CancellationToken);
            if (serviceType is not { } resolved || resolved.Kind == InjectionKind.BrowserAdapter)
                continue;

            var location = Location.Create(
                file.Path,
                directive.TypeSpan,
                text.Lines.GetLinePositionSpan(directive.TypeSpan));
            var descriptor = resolved.Kind switch
            {
                InjectionKind.DbContext => BrowserIneligibleDbContext,
                InjectionKind.BrowserUnavailable when string.Equals(
                    resolved.ContractName,
                    PersistentComponentStateMetadataName,
                    StringComparison.Ordinal) => SsrStateHandoffUnavailable,
                InjectionKind.BrowserUnavailable => BrowserAdapterServiceUnavailable,
                _ => BrowserIneligibleServerService
            };
            var arguments = descriptor == BrowserIneligibleDbContext || descriptor == SsrStateHandoffUnavailable
                ? new object[] { FormatTypeName(resolved.Type) }
                : [FormatTypeName(resolved.Type), resolved.ContractName];
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, arguments));
        }
    }

    private static void AnalyzeRazorPageDirectives(
        CompilationAnalysisContext context,
        AdditionalText file,
        SourceText text)
    {
        foreach (var directive in EnumeratePageDirectives(text))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var location = Location.Create(
                file.Path,
                directive.RouteSpan,
                text.Lines.GetLinePositionSpan(directive.RouteSpan));
            context.ReportDiagnostic(Diagnostic.Create(
                RouteDirectiveRequiresHostAdapter,
                location,
                directive.RouteText));
        }
    }

    private static void AnalyzeUnsupportedRazorComponents(
        CompilationAnalysisContext context,
        AdditionalText file,
        SourceText text)
    {
        foreach (var tag in EnumerateRazorComponentTags(text))
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var contract = UnsupportedRazorComponentContracts.FirstOrDefault(candidate =>
                string.Equals(candidate.TagName, tag.TagName, StringComparison.Ordinal));
            if (contract == default ||
                !HasComponentContract(context.Compilation, contract.MetadataName) ||
                HasAuthoredComponentShadow(context.Compilation, tag.TagName, context.CancellationToken))
            {
                continue;
            }

            var location = Location.Create(
                file.Path,
                tag.NameSpan,
                text.Lines.GetLinePositionSpan(tag.NameSpan));
            context.ReportDiagnostic(Diagnostic.Create(
                BlazorComponentAdapterUnavailable,
                location,
                tag.TagName));
        }
    }

    private static bool HasAuthoredComponentShadow(
        Compilation compilation,
        string tagName,
        CancellationToken cancellationToken)
    {
        var componentBase = compilation.GetTypeByMetadataName(ComponentBaseMetadataName);
        var componentContract = compilation.GetTypeByMetadataName(IComponentMetadataName);
        if (componentBase is null && componentContract is null)
            return false;

        return compilation
            .GetSymbolsWithName(tagName, SymbolFilter.Type, cancellationToken)
            .OfType<INamedTypeSymbol>()
            .Any(candidate =>
                IsAuthoredSource(candidate) &&
                IsComponent(candidate, componentBase, componentContract));
    }

    private static IEnumerable<InjectDirective> EnumerateInjectDirectives(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            var lineText = text.ToString(line.Span);
            var index = 0;
            while (index < lineText.Length && char.IsWhiteSpace(lineText[index]))
                index++;

            const string directive = "@inject";
            if (!lineText.AsSpan(index).StartsWith(directive, StringComparison.Ordinal) ||
                index + directive.Length >= lineText.Length ||
                !char.IsWhiteSpace(lineText[index + directive.Length]))
            {
                continue;
            }

            index += directive.Length;
            while (index < lineText.Length && char.IsWhiteSpace(lineText[index]))
                index++;

            var typeStart = index;
            while (index < lineText.Length && !char.IsWhiteSpace(lineText[index]))
                index++;

            if (typeStart == index)
                continue;

            var typeSpan = TextSpan.FromBounds(line.Start + typeStart, line.Start + index);
            yield return new InjectDirective(text.ToString(typeSpan), typeSpan);
        }
    }

    private static ResolvedInjectionType? ResolveDirectiveType(
        Compilation compilation,
        string typeName,
        Func<INamedTypeSymbol, InjectionClassification?> classify,
        CancellationToken cancellationToken)
    {
        var normalizedName = typeName.StartsWith("global::", StringComparison.Ordinal)
            ? typeName.Substring("global::".Length)
            : typeName;
        if (normalizedName.IndexOfAny(['<', '[', '?']) >= 0)
            return null;

        var direct = compilation.GetTypeByMetadataName(normalizedName);
        if (direct is not null && classify(direct) is { } directContract)
            return new ResolvedInjectionType(
                direct,
                directContract.Kind,
                directContract.ContractName);

        // Razor directives frequently use a local type imported through _Imports. Resolve a
        // simple name only when it identifies exactly one known server-only symbol; ambiguity
        // stays silent rather than guessing a service and producing a false diagnostic.
        // Razor 的简单类型名只在唯一匹配时诊断，避免 analyzer 猜测 using 后误报。
        if (normalizedName.IndexOf('.') >= 0)
            return null;

        var matches = compilation
            .GetSymbolsWithName(normalizedName, SymbolFilter.Type, cancellationToken)
            .OfType<INamedTypeSymbol>()
            .Select(candidate => (Candidate: candidate, Classification: classify(candidate)))
            .Where(static item => item.Classification is not null)
            .OrderBy(static item => item.Candidate.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal)
            .ToArray();
        return matches.Length == 1
            ? new ResolvedInjectionType(
                matches[0].Candidate,
                matches[0].Classification!.Value.Kind,
                matches[0].Classification!.Value.ContractName)
            : null;
    }

    private static InjectionClassification? ClassifyInjectionType(
        INamedTypeSymbol type,
        INamedTypeSymbol? dbContext,
        ImmutableArray<ServerOnlyService> serverOnlyServices,
        ImmutableArray<KnownService> browserAdapterServices,
        ImmutableArray<KnownService> browserUnavailableServices)
    {
        if (dbContext is not null && DerivesFrom(type, dbContext))
            return new InjectionClassification(InjectionKind.DbContext, DbContextMetadataName);

        if (FindServerOnlyService(type, serverOnlyServices) is { } serverOnly)
        {
            return new InjectionClassification(
                InjectionKind.ServerOnly,
                serverOnly.MetadataName);
        }

        // Prefer an explicit adapter over the unavailable list if a framework type is
        // represented by both contracts in a future SDK revision.
        if (FindKnownService(type, browserAdapterServices) is not null)
            return new InjectionClassification(InjectionKind.BrowserAdapter, string.Empty);

        if (FindKnownService(type, browserUnavailableServices) is { } unavailable)
        {
            return new InjectionClassification(
                InjectionKind.BrowserUnavailable,
                unavailable.MetadataName);
        }

        return null;
    }

    private static ServerOnlyService? FindServerOnlyService(
        ITypeSymbol type,
        ImmutableArray<ServerOnlyService> services)
    {
        foreach (var service in services)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, service.Symbol.OriginalDefinition) ||
                DerivesFrom(type, service.Symbol) ||
                type is INamedTypeSymbol named && Implements(named, service.Symbol))
            {
                return service;
            }
        }

        return null;
    }

    private static KnownService? FindKnownService(
        ITypeSymbol type,
        ImmutableArray<KnownService> services)
    {
        foreach (var service in services)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, service.Symbol.OriginalDefinition) ||
                DerivesFrom(type, service.Symbol) ||
                type is INamedTypeSymbol named && Implements(named, service.Symbol))
            {
                return service;
            }
        }

        return null;
    }

    private static bool HasComponentContract(Compilation compilation, string metadataName)
    {
        if (compilation.GetTypeByMetadataName(metadataName) is not null)
            return true;

        // Generic framework components are sometimes exposed by a reference assembly
        // through a constructed symbol. Match namespace, metadata name and arity rather
        // than relying on display text, which includes type-parameter names.
        var arityIndex = metadataName.LastIndexOf('`');
        if (arityIndex < 0)
            return false;

        var metadataTypeName = metadataName.Substring(metadataName.LastIndexOf('.') + 1);
        var sourceName = metadataTypeName.Substring(0, metadataTypeName.LastIndexOf('`'));
        var namespaceName = metadataName.Substring(0, metadataName.LastIndexOf('.'));
        return compilation.GetSymbolsWithName(
                sourceName,
                SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .Any(candidate =>
                string.Equals(candidate.MetadataName, metadataTypeName, StringComparison.Ordinal) &&
                string.Equals(candidate.ContainingNamespace?.ToDisplayString(), namespaceName, StringComparison.Ordinal));
    }

    private static IEnumerable<PageDirective> EnumeratePageDirectives(SourceText text)
    {
        foreach (var line in text.Lines)
        {
            var lineText = text.ToString(line.Span);
            var index = 0;
            while (index < lineText.Length && char.IsWhiteSpace(lineText[index]))
                index++;

            const string directive = "@page";
            if (!lineText.AsSpan(index).StartsWith(directive, StringComparison.Ordinal) ||
                index + directive.Length >= lineText.Length ||
                !char.IsWhiteSpace(lineText[index + directive.Length]))
            {
                continue;
            }

            index += directive.Length;
            while (index < lineText.Length && char.IsWhiteSpace(lineText[index]))
                index++;

            var routeStart = index;
            while (index < lineText.Length && !char.IsWhiteSpace(lineText[index]))
                index++;
            if (routeStart == index)
                continue;

            var routeSpan = TextSpan.FromBounds(line.Start + routeStart, line.Start + index);
            yield return new PageDirective(text.ToString(routeSpan), routeSpan);
        }
    }

    private static IEnumerable<RazorComponentTag> EnumerateRazorComponentTags(SourceText text)
    {
        var source = text.ToString();
        var index = 0;
        while (index < source.Length)
        {
            if (source.AsSpan(index).StartsWith("<!--", StringComparison.Ordinal))
            {
                var commentEnd = source.IndexOf("-->", index + 4, StringComparison.Ordinal);
                index = commentEnd < 0 ? source.Length : commentEnd + 3;
                continue;
            }

            if (source[index] == '@' && index + 1 < source.Length && source[index + 1] == '*')
            {
                var commentEnd = source.IndexOf("*@", index + 2, StringComparison.Ordinal);
                index = commentEnd < 0 ? source.Length : commentEnd + 2;
                continue;
            }

            if (source[index] == '@' && IsRazorCodeDirective(source, index, out var codeBrace))
            {
                index = SkipBalancedCode(source, codeBrace);
                continue;
            }

            if (source[index] != '<' ||
                index + 1 >= source.Length ||
                source[index + 1] is '/' or '!' or '?')
            {
                index++;
                continue;
            }

            var nameStart = index + 1;
            if (!IsTagNameStart(source[nameStart]))
            {
                index++;
                continue;
            }

            var nameEnd = nameStart + 1;
            while (nameEnd < source.Length && IsTagNamePart(source[nameEnd]))
                nameEnd++;
            var tagName = source.Substring(nameStart, nameEnd - nameStart);
            var simpleTagName = tagName;
            var lastDot = simpleTagName.LastIndexOf('.');
            if (lastDot >= 0)
                simpleTagName = simpleTagName.Substring(lastDot + 1);
            if (UnsupportedRazorComponentContracts.Any(candidate =>
                    string.Equals(candidate.TagName, simpleTagName, StringComparison.Ordinal)))
            {
                var nameSpan = TextSpan.FromBounds(nameStart, nameEnd);
                yield return new RazorComponentTag(simpleTagName, nameSpan);
            }

            // Skip the opening tag's quoted attributes so '<Component>' text inside an
            // attribute value is not mistaken for a Razor component. The next real '<'
            // after the closing '>' is scanned normally.
            index = SkipMarkupTag(source, nameEnd);
        }
    }

    private static int SkipMarkupTag(string source, int start)
    {
        var quote = '\0';
        for (var index = start; index < source.Length; index++)
        {
            var current = source[index];
            if (quote != '\0')
            {
                if (current == quote)
                    quote = '\0';
                else if (current == '\\')
                    index++;
                continue;
            }

            if (current is '"' or '\'')
            {
                quote = current;
                continue;
            }

            if (current == '>')
                return index + 1;
        }

        return source.Length;
    }

    private static bool IsRazorCodeDirective(string source, int atIndex, out int openingBrace)
    {
        openingBrace = -1;
        if (atIndex + 1 < source.Length && source[atIndex + 1] == '{')
        {
            openingBrace = atIndex + 1;
            return true;
        }

        var cursor = atIndex + 1;
        while (cursor < source.Length && char.IsLetter(source[cursor]))
            cursor++;

        var directive = source.Substring(atIndex + 1, cursor - atIndex - 1);
        if (!string.Equals(directive, "code", StringComparison.Ordinal) &&
            !string.Equals(directive, "functions", StringComparison.Ordinal))
        {
            return false;
        }

        while (cursor < source.Length && char.IsWhiteSpace(source[cursor]))
            cursor++;
        if (cursor >= source.Length || source[cursor] != '{')
            return false;

        openingBrace = cursor;
        return true;
    }

    private static int SkipBalancedCode(string source, int openingBrace)
    {
        var depth = 0;
        var inString = '\0';
        for (var index = openingBrace; index < source.Length; index++)
        {
            var current = source[index];
            if (inString != '\0')
            {
                if (current == '\\')
                {
                    index++;
                }
                else if (current == inString)
                {
                    inString = '\0';
                }

                continue;
            }

            if (current is '\"' or '\'' )
            {
                inString = current;
                continue;
            }

            if (current == '{')
                depth++;
            else if (current == '}' && --depth == 0)
                return index + 1;
        }

        return source.Length;
    }

    private static bool IsTagNameStart(char value)
        => char.IsLetter(value);

    private static bool IsTagNamePart(char value)
        => char.IsLetterOrDigit(value) || value == '_' || value == '.';

    private static bool IsComponent(
        INamedTypeSymbol? type,
        INamedTypeSymbol? componentBase,
        INamedTypeSymbol? componentContract)
        => type is not null &&
           ((componentBase is not null && DerivesFrom(type, componentBase)) ||
            (componentContract is not null && Implements(type, componentContract)));

    private static bool DerivesFrom(ITypeSymbol? type, INamedTypeSymbol baseType)
    {
        for (var current = type as INamedTypeSymbol;
             current is not null;
             current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType.OriginalDefinition))
                return true;
        }

        return false;
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol contract)
        => type.AllInterfaces.Any(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate.OriginalDefinition, contract.OriginalDefinition));

    private static bool IsAuthoredSource(ISymbol symbol)
        => symbol.DeclaringSyntaxReferences.Any(reference =>
            !reference.SyntaxTree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase));

    private static bool IsAuthoredSource(SyntaxNode syntax)
        => !syntax.SyntaxTree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);

    private static Location GetAuthoredLocation(ISymbol symbol)
        => symbol.Locations.FirstOrDefault(location =>
            location.IsInSource &&
            !location.SourceTree!.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)) ??
           Location.None;

    private static string FormatTypeName(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

    private static string HelpLink(string anchor)
        => "https://github.com/devhxj/Jazor/blob/main/docs/03-guides/razorvue-authoring.md#" + anchor;

    private readonly record struct InjectDirective(string TypeName, TextSpan TypeSpan);

    private readonly record struct ServerOnlyService(string MetadataName, INamedTypeSymbol Symbol);

    private readonly record struct KnownService(string MetadataName, INamedTypeSymbol Symbol);

    private enum InjectionKind
    {
        DbContext,
        ServerOnly,
        BrowserAdapter,
        BrowserUnavailable
    }

    private readonly record struct InjectionClassification(
        InjectionKind Kind,
        string ContractName);

    private readonly record struct ResolvedInjectionType(
        INamedTypeSymbol Type,
        InjectionKind Kind,
        string ContractName);

    private readonly record struct PageDirective(string RouteText, TextSpan RouteSpan);

    private readonly record struct RazorComponentTag(string TagName, TextSpan NameSpan);
}
