using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jazor.Emit;

internal sealed class RazorVueConsumerEntryCompiler
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
    private static readonly Regex JavaScriptIdentifierPattern = new(
        @"^[$_\p{L}][$_\p{L}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\u200C\u200D]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RazorRouteParameterPattern = new(
        @"^[A-Za-z_][A-Za-z0-9_]*\??$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly HashSet<string> JavaScriptReservedIdentifiers = new(StringComparer.Ordinal)
    {
        "await",
        "arguments",
        "break",
        "case",
        "catch",
        "class",
        "const",
        "continue",
        "debugger",
        "default",
        "delete",
        "do",
        "else",
        "enum",
        "eval",
        "export",
        "extends",
        "false",
        "finally",
        "for",
        "function",
        "if",
        "implements",
        "import",
        "in",
        "instanceof",
        "interface",
        "let",
        "new",
        "null",
        "package",
        "private",
        "protected",
        "public",
        "return",
        "static",
        "super",
        "switch",
        "this",
        "throw",
        "true",
        "try",
        "typeof",
        "var",
        "void",
        "while",
        "with",
        "yield"
    };

    public async Task<RazorVueConsumerEntryResult> GenerateAsync(RazorVueConsumerEntryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var manifestPath = ResolveRequiredPath(
            options.ManifestPath,
            Path.Combine(options.HostJazorRoot, "jazor-manifest.json"));
        var hostRequirementsModulePath = ResolveRequiredPath(
            options.HostRequirementsModulePath,
            RazorVueModuleWriter.GetHostRequirementsModulePath(options.HostJazorRoot));
        var browserGeneratedRoot = ResolveRequiredPath(
            options.BrowserGeneratedRoot,
            Path.Combine(options.OutputDirectory, "generated-browser"));
        var ssrGeneratedRoot = ResolveRequiredPath(
            options.SsrGeneratedRoot,
            Path.Combine(options.OutputDirectory, "generated-ssr"));
        var clientEntryPath = ResolveRequiredPath(
            options.ClientEntryPath,
            Path.Combine(options.OutputDirectory, "client-entry.mjs"));
        var ssrEntryPath = ResolveRequiredPath(
            options.SsrEntryPath,
            Path.Combine(options.OutputDirectory, "ssr-entry.mjs"));
        var vueFeatureFlagsPath = ResolveRequiredPath(
            options.VueFeatureFlagsPath,
            Path.Combine(options.OutputDirectory, "vue-feature-flags.mjs"));
        var clientRuntimeExportName = string.IsNullOrWhiteSpace(options.ClientRuntimeExportName)
            ? "mountRazorVueConsumer"
            : options.ClientRuntimeExportName.Trim();
        var ssrRuntimeExportName = string.IsNullOrWhiteSpace(options.SsrRuntimeExportName)
            ? "runRazorVueConsumerSsr"
            : options.SsrRuntimeExportName.Trim();
        var ssrExecuteExportName = string.IsNullOrWhiteSpace(options.SsrExecuteExportName)
            ? "executeSsr"
            : options.SsrExecuteExportName.Trim();

        var needsBrowser = options.Mode is RazorVueConsumerEntryMode.Browser or RazorVueConsumerEntryMode.Both;
        var needsSsr = options.Mode is RazorVueConsumerEntryMode.Ssr or RazorVueConsumerEntryMode.Both;

        if (!Directory.Exists(options.HostJazorRoot))
            return RazorVueConsumerEntryResult.Fail(6, $"RazorVue host output root was not found: '{options.HostJazorRoot}'.");

        if (!File.Exists(manifestPath))
            return RazorVueConsumerEntryResult.Fail(7, $"RazorVue manifest was not found: '{manifestPath}'.");

        if (!File.Exists(hostRequirementsModulePath))
            return RazorVueConsumerEntryResult.Fail(8, $"RazorVue host requirements module was not found: '{hostRequirementsModulePath}'.");

        if (needsBrowser && string.IsNullOrWhiteSpace(options.ClientRuntimeModulePath))
            return RazorVueConsumerEntryResult.Fail(9, "Missing required browser runtime module path.");

        if (needsBrowser && !File.Exists(options.ClientRuntimeModulePath))
            return RazorVueConsumerEntryResult.Fail(9, $"RazorVue browser runtime module was not found: '{options.ClientRuntimeModulePath}'.");

        if (needsSsr && string.IsNullOrWhiteSpace(options.SsrRuntimeModulePath))
            return RazorVueConsumerEntryResult.Fail(10, "Missing required SSR runtime module path.");

        if (needsSsr && !File.Exists(options.SsrRuntimeModulePath))
            return RazorVueConsumerEntryResult.Fail(10, $"RazorVue SSR runtime module was not found: '{options.SsrRuntimeModulePath}'.");

        var manifest = RazorVueManifestSerializer.TryLoad(manifestPath);
        if (manifest is null)
            return RazorVueConsumerEntryResult.Fail(7, $"RazorVue manifest was not found or could not be read: '{manifestPath}'.");

        var validationError = ValidateJavaScriptIdentifier(clientRuntimeExportName, "--client-runtime-export");
        if (validationError is not null)
            return RazorVueConsumerEntryResult.Fail(12, validationError);

        validationError = ValidateJavaScriptIdentifier(ssrRuntimeExportName, "--ssr-runtime-export");
        if (validationError is not null)
            return RazorVueConsumerEntryResult.Fail(12, validationError);

        validationError = ValidateJavaScriptIdentifier(ssrExecuteExportName, "--ssr-execute-export");
        if (validationError is not null)
            return RazorVueConsumerEntryResult.Fail(12, validationError);

        var resolvedComponentsResult = ResolveComponents(manifest, options.Components);
        if (!resolvedComponentsResult.IsSuccess)
            return RazorVueConsumerEntryResult.Fail(13, resolvedComponentsResult.Error!);

        var routeResults = BuildRouteResults(resolvedComponentsResult.Components!);
        if (!routeResults.IsSuccess)
            return RazorVueConsumerEntryResult.Fail(15, routeResults.Error!);

        var selectedSfcComponents = resolvedComponentsResult.Components!
            .Where(static component => string.Equals(component.ManifestEntry.ComponentModel, ManifestComponentModel.Sfc, StringComparison.Ordinal))
            .ToArray();
        var selectedSfcEntryModulePaths = selectedSfcComponents
            .Select(static component => NormalizeRelativeModulePath(component.ManifestEntry.RelativeModulePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var component in resolvedComponentsResult.Components!)
        {
            if (!string.Equals(component.ManifestEntry.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal))
                continue;

            var hostComponentModulePath = Path.Combine(
                options.HostJazorRoot,
                component.ManifestEntry.RelativeModulePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(hostComponentModulePath))
            {
                return RazorVueConsumerEntryResult.Fail(
                    14,
                    $"RazorVue H component host module was not found: '{hostComponentModulePath}'.");
            }
        }

        var cleanSafetyError = ValidateCleanSafety(
            options,
            browserGeneratedRoot,
            ssrGeneratedRoot,
            clientEntryPath,
            ssrEntryPath,
            vueFeatureFlagsPath,
            hostRequirementsModulePath,
            needsBrowser,
            needsSsr);
        if (cleanSafetyError is not null)
            return RazorVueConsumerEntryResult.Fail(16, cleanSafetyError);

        try
        {
            if (options.Clean)
                EmptyDirectory(options.OutputDirectory);
            else
                Directory.CreateDirectory(options.OutputDirectory);

            var bridgeCompiler = new RazorVueSfcBridgeCompiler();
            BridgeResultDocument? browserBridge = null;
            BridgeResultDocument? ssrBridge = null;

            if (needsBrowser && selectedSfcComponents.Length > 0)
            {
                var result = await bridgeCompiler.CompileAsync(new RazorVueSfcBridgeOptions(
                    options.HostJazorRoot,
                    browserGeneratedRoot,
                    manifestPath,
                    RazorVueSfcBridgeMode.Browser,
                    options.Production,
                    options.Clean,
                    selectedSfcEntryModulePaths));
                if (!result.IsSuccess)
                    return RazorVueConsumerEntryResult.Fail(17, result.Error ?? "RazorVue browser SFC bridge failed.");

                browserBridge = await ReadBridgeResultAsync(result.ResultPath);
            }

            if (needsSsr && selectedSfcComponents.Length > 0)
            {
                var result = await bridgeCompiler.CompileAsync(new RazorVueSfcBridgeOptions(
                    options.HostJazorRoot,
                    ssrGeneratedRoot,
                    manifestPath,
                    RazorVueSfcBridgeMode.Ssr,
                    options.Production,
                    options.Clean,
                    selectedSfcEntryModulePaths));
                if (!result.IsSuccess)
                    return RazorVueConsumerEntryResult.Fail(18, result.Error ?? "RazorVue SSR SFC bridge failed.");

                ssrBridge = await ReadBridgeResultAsync(result.ResultPath);
            }

            var componentResults = BuildComponentResults(
                resolvedComponentsResult.Components!,
                browserBridge,
                ssrBridge);

            if (needsBrowser)
            {
                await WriteTextAsync(vueFeatureFlagsPath, BuildVueFeatureFlagsModule());
                await WriteTextAsync(
                    clientEntryPath,
                    BuildBrowserEntryModule(
                        options.HostJazorRoot,
                        clientEntryPath,
                        vueFeatureFlagsPath,
                        hostRequirementsModulePath,
                        options.ClientRuntimeModulePath!,
                        clientRuntimeExportName,
                        componentResults,
                        routeResults.Routes!));
            }

            if (needsSsr)
            {
                await WriteTextAsync(
                    ssrEntryPath,
                    BuildSsrEntryModule(
                        options.HostJazorRoot,
                        ssrEntryPath,
                        hostRequirementsModulePath,
                        options.SsrRuntimeModulePath!,
                        ssrRuntimeExportName,
                        ssrExecuteExportName,
                        componentResults,
                        routeResults.Routes!));
            }

            var resultPath = options.WriteResultPath is null
                ? null
                : Path.GetFullPath(options.WriteResultPath);
            if (!string.IsNullOrWhiteSpace(resultPath))
            {
                await WriteTextAsync(
                    resultPath,
                    JsonSerializer.Serialize(
                        new RazorVueConsumerEntryResultDocument(
                            manifestPath,
                            options.HostJazorRoot,
                            options.OutputDirectory,
                            GetModeValue(options.Mode),
                            options.Production,
                            needsBrowser ? clientEntryPath : null,
                            needsSsr ? ssrEntryPath : null,
                            needsBrowser ? vueFeatureFlagsPath : null,
                            needsBrowser ? browserGeneratedRoot : null,
                            needsSsr ? ssrGeneratedRoot : null,
                            componentResults,
                            routeResults.Routes!),
                        JsonOptions) + Environment.NewLine);
            }

            return RazorVueConsumerEntryResult.Success(resultPath, componentResults.Count);
        }
        catch (Exception ex)
        {
            return RazorVueConsumerEntryResult.Fail(19, ex.ToString());
        }
    }

    private static ResolveComponentsResult ResolveComponents(
        RazorVueManifestModel manifest,
        IReadOnlyList<RazorVueConsumerComponentSelection> selections)
    {
        if (selections.Count == 0)
            return ResolveComponentsResult.Fail("At least one RazorVue consumer component selection is required.");

        var aliases = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<ResolvedConsumerComponent>(selections.Count);

        foreach (var selection in selections)
        {
            var alias = selection.Alias.Trim();
            var selector = selection.Selector.Trim();
            var aliasError = ValidateJavaScriptIdentifier(alias, $"component alias '{alias}'");
            if (aliasError is not null)
                return ResolveComponentsResult.Fail(aliasError);

            if (!aliases.Add(alias))
                return ResolveComponentsResult.Fail($"Duplicate RazorVue consumer component alias '{alias}'.");

            var matches = FindComponentMatches(manifest, selector);
            if (matches.Count == 0)
            {
                return ResolveComponentsResult.Fail(
                    $"RazorVue manifest did not contain a component matching selector '{selector}'. Use 'id:', 'name:', or 'path:' to make the selector explicit.");
            }

            if (matches.Count > 1)
            {
                var candidates = string.Join(
                    ", ",
                    matches
                        .OrderBy(static module => module.ComponentId, StringComparer.Ordinal)
                        .Select(static module => $"'{module.ComponentId}' at '{module.RelativeModulePath}'"));
                return ResolveComponentsResult.Fail(
                    $"RazorVue consumer component selector '{selector}' matched multiple RazorVue components: {candidates}. Use 'id:', 'name:', or 'path:' to make the selector explicit.");
            }

            components.Add(new ResolvedConsumerComponent(alias, selection.Selector, matches[0]));
        }

        return ResolveComponentsResult.Success(components);
    }

    private static List<RazorVueManifestEntry> FindComponentMatches(RazorVueManifestModel manifest, string selector)
    {
        var normalizedSelector = selector.Trim();
        if (normalizedSelector.Length == 0)
            return [];

        var kindSeparatorIndex = normalizedSelector.IndexOf(':', StringComparison.Ordinal);
        if (kindSeparatorIndex > 0)
        {
            var kind = normalizedSelector[..kindSeparatorIndex];
            var value = normalizedSelector[(kindSeparatorIndex + 1)..].Trim();
            if (value.Length == 0)
                return [];

            return kind.ToLowerInvariant() switch
            {
                "id" => manifest.Modules
                    .Where(module => string.Equals(module.ComponentId, value, StringComparison.Ordinal))
                    .ToList(),
                "name" => manifest.Modules
                    .Where(module => string.Equals(module.ComponentName, value, StringComparison.Ordinal))
                    .ToList(),
                "path" => manifest.Modules
                    .Where(module => string.Equals(
                        NormalizeRelativeModulePath(module.RelativeModulePath),
                        NormalizeRelativeModulePath(value),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                _ => []
            };
        }

        var normalizedPathSelector = NormalizeRelativeModulePath(normalizedSelector);
        return manifest.Modules
            .Where(module =>
                string.Equals(module.ComponentId, normalizedSelector, StringComparison.Ordinal) ||
                string.Equals(module.ComponentName, normalizedSelector, StringComparison.Ordinal) ||
                string.Equals(
                    NormalizeRelativeModulePath(module.RelativeModulePath),
                    normalizedPathSelector,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyList<RazorVueConsumerComponentResult> BuildComponentResults(
        IReadOnlyList<ResolvedConsumerComponent> resolvedComponents,
        BridgeResultDocument? browserBridge,
        BridgeResultDocument? ssrBridge)
    {
        var browserByRelativePath = BuildBridgeLookup(browserBridge);
        var ssrByRelativePath = BuildBridgeLookup(ssrBridge);
        var components = new List<RazorVueConsumerComponentResult>(resolvedComponents.Count);

        foreach (var resolvedComponent in resolvedComponents)
        {
            var relativeModulePath = NormalizeRelativeModulePath(resolvedComponent.ManifestEntry.RelativeModulePath);
            browserByRelativePath.TryGetValue(relativeModulePath, out var browserModule);
            ssrByRelativePath.TryGetValue(relativeModulePath, out var ssrModule);

            components.Add(new RazorVueConsumerComponentResult(
                resolvedComponent.Alias,
                resolvedComponent.Selector,
                resolvedComponent.ManifestEntry.ComponentId,
                resolvedComponent.ManifestEntry.ComponentName,
                string.Equals(resolvedComponent.ManifestEntry.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal)
                    ? "default"
                    : browserModule?.ExportName ?? ssrModule?.ExportName ?? resolvedComponent.ManifestEntry.ComponentName,
                resolvedComponent.ManifestEntry.ComponentModel,
                relativeModulePath,
                string.Equals(resolvedComponent.ManifestEntry.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal)
                    ? relativeModulePath
                    : browserModule?.RelativeOutputPath,
                browserModule?.OutputPath,
                string.Equals(resolvedComponent.ManifestEntry.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal)
                    ? relativeModulePath
                    : ssrModule?.RelativeOutputPath,
                ssrModule?.OutputPath));
        }

        return components;
    }

    private static ResolveRoutesResult BuildRouteResults(IReadOnlyList<ResolvedConsumerComponent> resolvedComponents)
    {
        var routes = new List<RazorVueConsumerRouteResult>();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var component in resolvedComponents)
        {
            var routeTemplates = component.ManifestEntry.RouteTemplates;
            for (var index = 0; index < routeTemplates.Count; index++)
            {
                var routeTemplate = routeTemplates[index];
                if (!TryConvertRazorRouteTemplate(routeTemplate, out var routePath, out var parameterNames, out var error))
                {
                    return ResolveRoutesResult.Fail(
                        $"RazorVue consumer route template '{routeTemplate}' on component '{component.ManifestEntry.ComponentId}' is not currently supported: {error}");
                }

                if (!seenPaths.Add(routePath))
                {
                    return ResolveRoutesResult.Fail(
                        $"RazorVue consumer route path '{routePath}' was produced more than once. Route paths must be unique across the selected components.");
                }

                routes.Add(new RazorVueConsumerRouteResult(
                    component.Alias + "__" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    component.Alias,
                    component.ManifestEntry.ComponentId,
                    component.ManifestEntry.ComponentName,
                    component.ManifestEntry.ComponentModel,
                    routeTemplate,
                    routePath,
                    parameterNames));
            }
        }

        return ResolveRoutesResult.Success(routes);
    }

    private static IReadOnlyDictionary<string, BridgeModuleDocument> BuildBridgeLookup(BridgeResultDocument? bridge)
        => bridge?.Modules.ToDictionary(
            static module => NormalizeRelativeModulePath(module.RelativeModulePath),
            static module => module,
            StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, BridgeModuleDocument>(StringComparer.OrdinalIgnoreCase);

    private static string BuildBrowserEntryModule(
        string hostJazorRoot,
        string clientEntryPath,
        string vueFeatureFlagsPath,
        string hostRequirementsModulePath,
        string clientRuntimeModulePath,
        string clientRuntimeExportName,
        IReadOnlyList<RazorVueConsumerComponentResult> components,
        IReadOnlyList<RazorVueConsumerRouteResult> routes)
    {
        var lines = new List<string>
        {
            $"import {JsonSerializer.Serialize(ToModuleSpecifier(clientEntryPath, vueFeatureFlagsPath))};"
        };

        foreach (var component in components)
            lines.Add(BuildBrowserComponentImportLine(hostJazorRoot, clientEntryPath, component));

        lines.Add($"import {{ razorVueHostRequirements }} from {JsonSerializer.Serialize(ToModuleSpecifier(clientEntryPath, hostRequirementsModulePath))};");
        lines.Add($"import {{ {clientRuntimeExportName} }} from {JsonSerializer.Serialize(ToModuleSpecifier(clientEntryPath, clientRuntimeModulePath))};");
        lines.Add(string.Empty);
        lines.Add(BuildConsumerComponentsObject(components));
        lines.Add(BuildConsumerRoutesObject(routes));
        lines.Add("export { razorVueHostRequirements };");
        lines.Add($"{clientRuntimeExportName}(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);");
        lines.Add(string.Empty);
        return string.Join("\n", lines);
    }

    private static string BuildSsrEntryModule(
        string hostJazorRoot,
        string ssrEntryPath,
        string hostRequirementsModulePath,
        string ssrRuntimeModulePath,
        string ssrRuntimeExportName,
        string ssrExecuteExportName,
        IReadOnlyList<RazorVueConsumerComponentResult> components,
        IReadOnlyList<RazorVueConsumerRouteResult> routes)
    {
        var lines = new List<string>();
        foreach (var component in components)
            lines.Add(BuildSsrComponentImportLine(hostJazorRoot, ssrEntryPath, component));

        lines.Add($"import {{ razorVueHostRequirements }} from {JsonSerializer.Serialize(ToModuleSpecifier(ssrEntryPath, hostRequirementsModulePath))};");
        lines.Add($"import {{ {ssrRuntimeExportName} }} from {JsonSerializer.Serialize(ToModuleSpecifier(ssrEntryPath, ssrRuntimeModulePath))};");
        lines.Add(string.Empty);
        lines.Add(BuildConsumerComponentsObject(components));
        lines.Add(BuildConsumerRoutesObject(routes));
        lines.Add($"export {{ {ssrRuntimeExportName}, razorVueHostRequirements }};");
        lines.Add($"export async function {ssrExecuteExportName}() {{");
        lines.Add($"  return await {ssrRuntimeExportName}(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);");
        lines.Add("}");
        lines.Add(string.Empty);
        return string.Join("\n", lines);
    }

    private static string BuildConsumerComponentsObject(IReadOnlyList<RazorVueConsumerComponentResult> components)
    {
        var lines = new List<string>
        {
            "export const razorVueConsumerComponents = Object.freeze({"
        };
        foreach (var component in components)
            lines.Add($"  {component.Alias},");

        lines.Add("});");
        return string.Join("\n", lines);
    }

    private static string BuildConsumerRoutesObject(IReadOnlyList<RazorVueConsumerRouteResult> routes)
    {
        if (routes.Count == 0)
            return "export const razorVueConsumerRoutes = Object.freeze([]);";

        var lines = new List<string>
        {
            "export const razorVueConsumerRoutes = Object.freeze(["
        };

        foreach (var route in routes)
        {
            lines.Add("  Object.freeze({");
            lines.Add($"    name: {JsonSerializer.Serialize(route.Name)},");
            lines.Add($"    alias: {JsonSerializer.Serialize(route.Alias)},");
            lines.Add($"    componentId: {JsonSerializer.Serialize(route.ComponentId)},");
            lines.Add($"    componentName: {JsonSerializer.Serialize(route.ComponentName)},");
            lines.Add($"    componentModel: {JsonSerializer.Serialize(route.ComponentModel)},");
            lines.Add($"    routeTemplate: {JsonSerializer.Serialize(route.RouteTemplate)},");
            lines.Add($"    path: {JsonSerializer.Serialize(route.Path)},");
            lines.Add($"    parameterNames: Object.freeze({BuildStringArrayLiteral(route.ParameterNames)}),");
            lines.Add("  }),");
        }

        lines.Add("]);");
        return string.Join("\n", lines);
    }

    private static string BuildBrowserComponentImportLine(
        string hostJazorRoot,
        string clientEntryPath,
        RazorVueConsumerComponentResult component)
    {
        if (string.Equals(component.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal))
        {
            var hostModulePath = Path.Combine(
                hostJazorRoot,
                component.RelativeModulePath.Replace('/', Path.DirectorySeparatorChar));
            return $"import {component.Alias} from {JsonSerializer.Serialize(ToModuleSpecifier(clientEntryPath, hostModulePath))};";
        }

        if (component.BrowserOutputPath is null)
            throw new InvalidOperationException($"RazorVue browser bridge did not emit selected component '{component.ComponentId}'.");

        return $"import {{ {FormatNamedImport(component.ExportName, component.Alias)} }} from {JsonSerializer.Serialize(ToModuleSpecifier(clientEntryPath, component.BrowserOutputPath))};";
    }

    private static string BuildSsrComponentImportLine(
        string hostJazorRoot,
        string ssrEntryPath,
        RazorVueConsumerComponentResult component)
    {
        if (string.Equals(component.ComponentModel, ManifestComponentModel.H, StringComparison.Ordinal))
        {
            var hostModulePath = Path.Combine(
                hostJazorRoot,
                component.RelativeModulePath.Replace('/', Path.DirectorySeparatorChar));
            return $"import {component.Alias} from {JsonSerializer.Serialize(ToModuleSpecifier(ssrEntryPath, hostModulePath))};";
        }

        if (component.SsrOutputPath is null)
            throw new InvalidOperationException($"RazorVue SSR bridge did not emit selected component '{component.ComponentId}'.");

        return $"import {{ {FormatNamedImport(component.ExportName, component.Alias)} }} from {JsonSerializer.Serialize(ToModuleSpecifier(ssrEntryPath, component.SsrOutputPath))};";
    }

    private static string BuildVueFeatureFlagsModule()
        => """
        globalThis.__VUE_OPTIONS_API__ = true;
        globalThis.__VUE_PROD_DEVTOOLS__ = false;
        globalThis.__VUE_PROD_HYDRATION_MISMATCH_DETAILS__ = false;

        """.ReplaceLineEndings("\n");

    private static string? ValidateCleanSafety(
        RazorVueConsumerEntryOptions options,
        string browserGeneratedRoot,
        string ssrGeneratedRoot,
        string clientEntryPath,
        string ssrEntryPath,
        string vueFeatureFlagsPath,
        string hostRequirementsModulePath,
        bool needsBrowser,
        bool needsSsr)
    {
        if (!options.Clean)
            return null;

        var protectedPaths = new List<ProtectedPath>
        {
            new("host output root", options.HostJazorRoot),
            new("host requirements module", hostRequirementsModulePath)
        };

        if (needsBrowser)
            protectedPaths.Add(new("client runtime module", options.ClientRuntimeModulePath!));

        if (needsSsr)
            protectedPaths.Add(new("SSR runtime module", options.SsrRuntimeModulePath!));

        foreach (var cleanRoot in EnumerateCleanRoots(options, browserGeneratedRoot, ssrGeneratedRoot, needsBrowser, needsSsr))
        {
            foreach (var protectedPath in protectedPaths)
            {
                if (IsSameOrAncestorOf(cleanRoot, protectedPath.Path))
                {
                    return $"RazorVue consumer entry clean directory '{cleanRoot}' cannot be the same as or an ancestor of the {protectedPath.Description} '{protectedPath.Path}'.";
                }
            }
        }

        if (needsBrowser && needsSsr && PathsOverlap(browserGeneratedRoot, ssrGeneratedRoot))
        {
            return $"RazorVue consumer entry browser and SSR generated roots must not overlap when --clean is true: '{browserGeneratedRoot}' and '{ssrGeneratedRoot}'.";
        }

        var generatedRoots = new[]
        {
            needsBrowser ? browserGeneratedRoot : null,
            needsSsr ? ssrGeneratedRoot : null
        }.Where(static path => path is not null).Select(static path => path!).ToArray();
        var generatedFiles = new[]
        {
            clientEntryPath,
            ssrEntryPath,
            vueFeatureFlagsPath
        };
        foreach (var generatedRoot in generatedRoots)
        {
            foreach (var generatedFile in generatedFiles)
            {
                if (IsSameOrAncestorOf(generatedRoot, generatedFile))
                {
                    return $"RazorVue consumer entry generated root '{generatedRoot}' cannot be the same as or an ancestor of generated entry file '{generatedFile}' when --clean is true.";
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCleanRoots(
        RazorVueConsumerEntryOptions options,
        string browserGeneratedRoot,
        string ssrGeneratedRoot,
        bool needsBrowser,
        bool needsSsr)
    {
        yield return options.OutputDirectory;
        if (needsBrowser)
            yield return browserGeneratedRoot;

        if (needsSsr)
            yield return ssrGeneratedRoot;
    }

    private static bool PathsOverlap(string left, string right)
        => IsSameOrAncestorOf(left, right) || IsSameOrAncestorOf(right, left);

    private static string? ValidateJavaScriptIdentifier(string value, string description)
    {
        if (!JavaScriptIdentifierPattern.IsMatch(value))
            return $"RazorVue consumer {description} must be a JavaScript identifier: '{value}'.";

        return JavaScriptReservedIdentifiers.Contains(value)
            ? $"RazorVue consumer {description} cannot use reserved JavaScript identifier '{value}'."
            : null;
    }

    private static async Task<BridgeResultDocument> ReadBridgeResultAsync(string? resultPath)
    {
        if (string.IsNullOrWhiteSpace(resultPath) || !File.Exists(resultPath))
            throw new InvalidOperationException($"RazorVue SFC bridge result file was not written: '{resultPath}'.");

        var document = JsonSerializer.Deserialize<BridgeResultDocument>(await File.ReadAllTextAsync(resultPath));
        if (document is null)
            throw new InvalidOperationException($"RazorVue SFC bridge result file could not be read: '{resultPath}'.");

        return document;
    }

    private static async Task WriteTextAsync(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, content, Utf8WithoutBom);
    }

    private static void EmptyDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }

    private static string ResolveRequiredPath(string? value, string defaultValue)
        => string.IsNullOrWhiteSpace(value)
            ? Path.GetFullPath(defaultValue)
            : Path.GetFullPath(value);

    private static string FormatNamedImport(string exportName, string alias)
        => string.Equals(exportName, alias, StringComparison.Ordinal)
            ? exportName
            : $"{exportName} as {alias}";

    private static string ToModuleSpecifier(string fromPath, string toPath)
    {
        var fromDirectory = Path.GetDirectoryName(Path.GetFullPath(fromPath)) ?? Directory.GetCurrentDirectory();
        var relativePath = Path.GetRelativePath(fromDirectory, Path.GetFullPath(toPath)).Replace('\\', '/');
        if (!relativePath.StartsWith("./", StringComparison.Ordinal) &&
            !relativePath.StartsWith("../", StringComparison.Ordinal))
        {
            relativePath = "./" + relativePath;
        }

        return relativePath;
    }

    private static string BuildStringArrayLiteral(IReadOnlyList<string> values)
        => "[" + string.Join(", ", values.Select(static value => JsonSerializer.Serialize(value))) + "]";

    private static bool TryConvertRazorRouteTemplate(
        string routeTemplate,
        out string routePath,
        out string[] parameterNames,
        out string? error)
    {
        var normalizedTemplate = routeTemplate.Trim();
        if (normalizedTemplate.Length == 0)
        {
            routePath = string.Empty;
            parameterNames = [];
            error = "route template cannot be empty.";
            return false;
        }

        if (!normalizedTemplate.StartsWith("/", StringComparison.Ordinal))
            normalizedTemplate = "/" + normalizedTemplate;

        if (normalizedTemplate == "/")
        {
            routePath = "/";
            parameterNames = [];
            error = null;
            return true;
        }

        var trailingSlash = normalizedTemplate.EndsWith("/", StringComparison.Ordinal);
        var rawSegments = normalizedTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var convertedSegments = new List<string>(rawSegments.Length);
        var names = new List<string>();

        foreach (var rawSegment in rawSegments)
        {
            if (rawSegment.IndexOf('{') >= 0 || rawSegment.IndexOf('}') >= 0)
            {
                if (rawSegment.Length < 2 || rawSegment[0] != '{' || rawSegment[^1] != '}')
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = "composite route segments are not supported; use either a literal segment or a full '{parameter}' segment.";
                    return false;
                }

                var parameterToken = rawSegment[1..^1].Trim();
                if (parameterToken.Length == 0)
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = "route parameter name cannot be empty.";
                    return false;
                }

                if (parameterToken.Contains(':', StringComparison.Ordinal))
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = "route constraints are not supported yet.";
                    return false;
                }

                if (parameterToken.Contains('=', StringComparison.Ordinal))
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = "route default values are not supported yet.";
                    return false;
                }

                if (parameterToken[0] == '*')
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = "catch-all route parameters are not supported yet.";
                    return false;
                }

                if (!RazorRouteParameterPattern.IsMatch(parameterToken))
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = $"route parameter '{parameterToken}' is not a supported identifier.";
                    return false;
                }

                var isOptional = parameterToken.EndsWith("?", StringComparison.Ordinal);
                var parameterName = isOptional ? parameterToken[..^1] : parameterToken;
                if (names.Contains(parameterName, StringComparer.Ordinal))
                {
                    routePath = string.Empty;
                    parameterNames = [];
                    error = $"route parameter '{parameterName}' was declared more than once.";
                    return false;
                }

                names.Add(parameterName);
                convertedSegments.Add(":" + parameterName + (isOptional ? "?" : string.Empty));
            }
            else
            {
                convertedSegments.Add(rawSegment);
            }
        }

        routePath = "/" + string.Join("/", convertedSegments);
        if (trailingSlash)
            routePath += "/";

        parameterNames = names.ToArray();
        error = null;
        return true;
    }

    private static string NormalizeRelativeModulePath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        return string.Join("/", segments);
    }

    private static bool IsSameOrAncestorOf(string candidateDirectory, string targetPath)
    {
        var candidateFullPath = Path.GetFullPath(candidateDirectory);
        var targetFullPath = Path.GetFullPath(targetPath);
        if (string.Equals(
            TrimTrailingDirectorySeparators(candidateFullPath),
            TrimTrailingDirectorySeparators(targetFullPath),
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var candidate = EnsureTrailingDirectorySeparator(candidateFullPath);
        var target = targetFullPath;
        if (Directory.Exists(target))
            target = EnsureTrailingDirectorySeparator(target);

        return target.StartsWith(candidate, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetModeValue(RazorVueConsumerEntryMode mode)
        => mode switch
        {
            RazorVueConsumerEntryMode.Browser => "browser",
            RazorVueConsumerEntryMode.Ssr => "ssr",
            RazorVueConsumerEntryMode.Both => "both",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported RazorVue consumer entry mode.")
        };

    private static string TrimTrailingDirectorySeparators(string path)
        => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private sealed record ProtectedPath(string Description, string Path);

    private sealed record ResolveComponentsResult(
        bool IsSuccess,
        IReadOnlyList<ResolvedConsumerComponent>? Components,
        string? Error)
    {
        public static ResolveComponentsResult Success(IReadOnlyList<ResolvedConsumerComponent> components)
            => new(true, components, null);

        public static ResolveComponentsResult Fail(string error)
            => new(false, null, error);
    }

    private sealed record ResolveRoutesResult(
        bool IsSuccess,
        IReadOnlyList<RazorVueConsumerRouteResult>? Routes,
        string? Error)
    {
        public static ResolveRoutesResult Success(IReadOnlyList<RazorVueConsumerRouteResult> routes)
            => new(true, routes, null);

        public static ResolveRoutesResult Fail(string error)
            => new(false, null, error);
    }

    private sealed record ResolvedConsumerComponent(
        string Alias,
        string Selector,
        RazorVueManifestEntry ManifestEntry);

    private sealed record BridgeResultDocument(
        List<BridgeModuleDocument> Modules);

    private sealed record BridgeModuleDocument(
        string ComponentId,
        string ComponentName,
        string ExportName,
        string RelativeModulePath,
        string RelativeOutputPath,
        string OutputPath,
        string? CssOutputPath);
}

internal sealed record RazorVueConsumerEntryResult(
    bool IsSuccess,
    int ExitCode,
    string? Error,
    string? ResultPath,
    int ComponentCount)
{
    public static RazorVueConsumerEntryResult Success(string? resultPath, int componentCount)
        => new(true, 0, null, resultPath, componentCount);

    public static RazorVueConsumerEntryResult Fail(int exitCode, string error)
        => new(false, exitCode, error, null, 0);
}

internal sealed record RazorVueConsumerComponentResult(
    string Alias,
    string Selector,
    string ComponentId,
    string ComponentName,
    string ExportName,
    string ComponentModel,
    string RelativeModulePath,
    string? BrowserRelativeOutputPath,
    string? BrowserOutputPath,
    string? SsrRelativeOutputPath,
    string? SsrOutputPath);

internal sealed record RazorVueConsumerRouteResult(
    string Name,
    string Alias,
    string ComponentId,
    string ComponentName,
    string ComponentModel,
    string RouteTemplate,
    string Path,
    IReadOnlyList<string> ParameterNames);

internal sealed record RazorVueConsumerEntryResultDocument(
    string ManifestPath,
    string HostJazorRoot,
    string OutputDirectory,
    string Mode,
    bool Production,
    string? ClientEntryPath,
    string? SsrEntryPath,
    string? VueFeatureFlagsPath,
    string? BrowserGeneratedRoot,
    string? SsrGeneratedRoot,
    IReadOnlyList<RazorVueConsumerComponentResult> Components,
    IReadOnlyList<RazorVueConsumerRouteResult> Routes);
