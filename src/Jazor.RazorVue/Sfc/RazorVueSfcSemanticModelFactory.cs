using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;

namespace Jazor.RazorVue.Sfc;

internal sealed class RazorVueSfcSemanticModelFactory
{
    public RazorVueSfcSemanticModel Create(RazorVueCanonicalHComponentModel canonicalModel)
    {
        if (canonicalModel is null)
            throw new ArgumentNullException(nameof(canonicalModel));

        ValidateTemplateEncodability(canonicalModel);

        var ownerRelativeSfcPath = ChangeExtensionToVue(canonicalModel.RelativeComponentPath);
        var componentImports = CollectComponentImports(canonicalModel, ownerRelativeSfcPath);
        var bindings = CollectLiftedBindings(canonicalModel.ComponentFullName, canonicalModel.Template);
        var requiresSlotsRuntime = bindings.Bindings.Any(static binding => RequiresSlotsRuntime(binding.ExpressionText));
        return new RazorVueSfcSemanticModel(
            ComponentName: canonicalModel.ComponentName,
            ComponentFullName: canonicalModel.ComponentFullName,
            RelativeSfcPath: ownerRelativeSfcPath,
            Descriptor: canonicalModel.Descriptor,
            Imports: CollectImports(componentImports, canonicalModel.CompilerImports),
            CompilerImports: canonicalModel.CompilerImports,
            ComponentImports: componentImports,
            Styles: canonicalModel.Styles,
            PluginRequirements: canonicalModel.PluginRequirements,
            Hints: canonicalModel.Hints,
            SourceOrigins: canonicalModel.SourceOrigins,
            TemplateBlock: new RazorVueSfcTemplateBlockModel(
                canonicalModel.Template,
                bindings.Sites.ToImmutable(),
                canonicalModel.Template.Children.SelectMany(static child => child.SourceOrigins).ToImmutableArray()),
            ScriptSetupBlock: new RazorVueSfcScriptSetupBlockModel(canonicalModel.Setup, bindings.Bindings.ToImmutable(), requiresSlotsRuntime, canonicalModel.SourceOrigins),
            StyleBlocks: ImmutableArray<RazorVueSfcStyleBlockModel>.Empty,
            CustomBlocks: ImmutableArray<RazorVueSfcCustomBlockModel>.Empty);
    }

    private static ImmutableArray<string> CollectImports(
        ImmutableArray<RazorVueSfcComponentImport> componentImports,
        ImmutableArray<RazorVueCompilerImportBinding> compilerImports)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        builder.Add("vue");
        builder.AddRange(RazorVueCompilerImportFormatter.CollectImportSources(compilerImports));

        foreach (var import in componentImports)
        {
            if (string.IsNullOrWhiteSpace(import.ImportSpecifier) ||
                string.Equals(import.ImportSpecifier, "vue", StringComparison.Ordinal))
            {
                continue;
            }

            builder.Add(import.ImportSpecifier!);
        }

        return [.. builder.Distinct(StringComparer.Ordinal)];
    }

    private static ImmutableArray<RazorVueSfcComponentImport> CollectComponentImports(RazorVueCanonicalHComponentModel canonicalModel, string ownerRelativeSfcPath)
    {
        var builder = ImmutableArray.CreateBuilder<RazorVueSfcComponentImport>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in EnumerateNodes(canonicalModel.Template))
        {
            if (node is not RazorVueCanonicalComponentNode component ||
                component.ResolvedDescriptor is null)
            {
                continue;
            }

            if (!seenKeys.Add(component.ComponentFullName))
                continue;

            var descriptor = component.ResolvedDescriptor;
            if (string.Equals(descriptor.ImportSpecifier, "vue", StringComparison.Ordinal))
            {
                builder.Add(new RazorVueSfcComponentImport(
                    ComponentKey: component.ComponentFullName,
                    TemplateTagName: component.ResolutionName,
                    LocalBindingName: descriptor.ExportName,
                    ImportSpecifier: null,
                    ExportName: descriptor.ExportName,
                    ImportKind: RazorVueSfcComponentImportKind.None));
                continue;
            }

            var importKind = descriptor.SourceKind == VueComponentSourceKind.LibraryComponent &&
                             !string.Equals(descriptor.ExportName, "default", StringComparison.Ordinal)
                ? RazorVueSfcComponentImportKind.Named
                : RazorVueSfcComponentImportKind.Default;

            builder.Add(new RazorVueSfcComponentImport(
                ComponentKey: component.ComponentFullName,
                TemplateTagName: CreateTemplateTagName(component, descriptor),
                LocalBindingName: CreateLocalBindingName(component, descriptor),
                ImportSpecifier: NormalizeSfcImportSpecifier(descriptor.ImportSpecifier, descriptor.SourceKind, ownerRelativeSfcPath),
                ExportName: descriptor.ExportName,
                ImportKind: importKind));
        }

        return builder.ToImmutable();
    }

    private static void ValidateTemplateEncodability(RazorVueCanonicalHComponentModel canonicalModel)
    {
        foreach (var node in EnumerateNodes(canonicalModel.Template))
        {
            if (node.TemplateEncodability == RazorVueTemplateEncodability.NotTemplateEncodable)
            {
                throw CreateUnsupportedTemplateEncodingException(
                    canonicalModel.ComponentFullName,
                    node.SourceOrigins,
                    $"RazorVue SFC template encoding does not support canonical node '{node.NodeKind}'.");
            }
        }
    }

    private static BindingCollection CollectLiftedBindings(string ownerComponentFullName, RazorVueCanonicalTemplateFragment fragment)
    {
        var bindings = new BindingCollection();
        CollectLiftedBindings(ownerComponentFullName, fragment, bindings, 0, "root");
        return bindings;
    }

    private static bool RequiresSlotsRuntime(string expressionText)
        => expressionText.Contains("slots.", StringComparison.Ordinal) ||
           expressionText.Contains("slots[", StringComparison.Ordinal);

    private static void CollectLiftedBindings(
        string ownerComponentFullName,
        RazorVueCanonicalTemplateFragment fragment,
        BindingCollection bindings,
        int templateScopeDepth,
        string pathPrefix)
    {
        for (var index = 0; index < fragment.Children.Length; index++)
            CollectLiftedBindings(ownerComponentFullName, fragment.Children[index], bindings, templateScopeDepth, pathPrefix + "/child[" + index + "]");
    }

    private static void CollectLiftedBindings(
        string ownerComponentFullName,
        RazorVueCanonicalTemplateNode node,
        BindingCollection bindings,
        int templateScopeDepth,
        string path)
    {
        switch (node)
        {
            case RazorVueCanonicalElementNode element:
                CollectLiftedBindings(ownerComponentFullName, element.Attributes, bindings, templateScopeDepth, path + "/attrs");
                CollectLiftedBindings(ownerComponentFullName, element.Children, bindings, templateScopeDepth, path);
                return;

            case RazorVueCanonicalComponentNode component:
                CollectLiftedBindings(ownerComponentFullName, component.Attributes, bindings, templateScopeDepth, path + "/attrs");
                CollectLiftedBindings(ownerComponentFullName, component.Slots, bindings, templateScopeDepth, path + "/slots");
                CollectLiftedBindings(ownerComponentFullName, component.Children, bindings, templateScopeDepth, path);
                return;

            case RazorVueCanonicalInterpolationNode interpolation
                when ShouldCreateBindingSite(interpolation.TemplateEncodability, interpolation.SideEffectClassification, templateScopeDepth):
                bindings.Add(
                    ownerComponentFullName,
                        path,
                        interpolation.ExpressionText,
                        interpolation.BindingKind,
                        interpolation.TemplateExpressionSafety,
                        interpolation.SideEffectClassification,
                        interpolation.SourceOrigins);
                return;

            case RazorVueCanonicalConditionalNode conditional:
                if (ShouldCreateBindingSite(conditional.TemplateEncodability, conditional.SideEffectClassification, templateScopeDepth))
                {
                    bindings.Add(
                        ownerComponentFullName,
                        path + "/if",
                        conditional.ConditionExpressionText,
                        conditional.BindingKind,
                        conditional.TemplateExpressionSafety,
                        conditional.SideEffectClassification,
                        conditional.SourceOrigins);
                }
                CollectLiftedBindings(ownerComponentFullName, conditional.WhenTrue, bindings, templateScopeDepth, path + "/whenTrue");
                CollectLiftedBindings(ownerComponentFullName, conditional.WhenFalse, bindings, templateScopeDepth, path + "/whenFalse");
                return;

            case RazorVueCanonicalForEachNode loop:
                if (ShouldCreateBindingSite(loop.TemplateEncodability, loop.SideEffectClassification, templateScopeDepth))
                {
                    bindings.Add(
                        ownerComponentFullName,
                        path + "/forEach",
                        loop.SourceExpressionText,
                        loop.BindingKind,
                        loop.TemplateExpressionSafety,
                        loop.SideEffectClassification,
                        loop.SourceOrigins);
                }
                CollectLiftedBindings(ownerComponentFullName, loop.Body, bindings, templateScopeDepth + 1, path + "/body");
                return;

            case RazorVueCanonicalForNode loop:
                if (ShouldCreateBindingSite(loop.TemplateEncodability, loop.SideEffectClassification, templateScopeDepth))
                {
                    bindings.Add(
                        ownerComponentFullName,
                        path + "/for/init",
                        loop.InitialValueExpressionText,
                        loop.InitialValueBindingKind,
                        loop.InitialValueTemplateExpressionSafety,
                        loop.InitialValueSideEffectClassification,
                        loop.SourceOrigins);
                    bindings.Add(
                        ownerComponentFullName,
                        path + "/for/limit",
                        loop.LimitValueExpressionText,
                        loop.LimitValueBindingKind,
                        loop.LimitValueTemplateExpressionSafety,
                        loop.LimitValueSideEffectClassification,
                        loop.SourceOrigins);
                    if (!string.IsNullOrWhiteSpace(loop.StepValueExpressionText))
                    {
                        bindings.Add(
                            ownerComponentFullName,
                            path + "/for/step",
                            loop.StepValueExpressionText!,
                            loop.StepValueBindingKind,
                            loop.StepValueTemplateExpressionSafety,
                            loop.StepValueSideEffectClassification,
                            loop.SourceOrigins);
                    }
                }
                CollectLiftedBindings(ownerComponentFullName, loop.Body, bindings, templateScopeDepth + 1, path + "/body");
                return;

            case RazorVueCanonicalSlotOutletNode slotOutlet when
                slotOutlet.ArgumentExpressionText is not null &&
                ShouldCreateBindingSite(slotOutlet.TemplateEncodability, slotOutlet.SideEffectClassification, templateScopeDepth):
                bindings.Add(
                    ownerComponentFullName,
                    path + "/slotArg",
                    slotOutlet.ArgumentExpressionText,
                    slotOutlet.BindingKind,
                    slotOutlet.TemplateExpressionSafety,
                    slotOutlet.SideEffectClassification,
                    slotOutlet.SourceOrigins);
                return;

            default:
                return;
        }
    }

    private static void CollectLiftedBindings(
        string ownerComponentFullName,
        ImmutableArray<RazorVueCanonicalAttributeEntry> attributes,
        BindingCollection bindings,
        int templateScopeDepth,
        string pathPrefix)
    {
        for (var index = 0; index < attributes.Length; index++)
        {
            switch (attributes[index])
            {
                case RazorVueCanonicalAttributeBinding attribute:
                    if (attribute.ExpressionText is null ||
                        !ShouldCreateBindingSite(attribute.TemplateEncodability, attribute.SideEffectClassification, templateScopeDepth))
                    {
                        continue;
                    }

                    bindings.Add(
                        ownerComponentFullName,
                        pathPrefix + "/attr[" + index + "]",
                        attribute.ExpressionText,
                        attribute.BindingKind,
                        attribute.TemplateExpressionSafety,
                        attribute.SideEffectClassification,
                        attribute.SourceOrigins);
                    break;
                case RazorVueCanonicalAttributeSpreadBinding spread:
                    if (!ShouldCreateBindingSite(spread.TemplateEncodability, spread.SideEffectClassification, templateScopeDepth))
                        continue;

                    bindings.Add(
                        ownerComponentFullName,
                        pathPrefix + "/attr[" + index + "]/spread",
                        spread.ExpressionText,
                        spread.BindingKind,
                        spread.TemplateExpressionSafety,
                        spread.SideEffectClassification,
                        spread.SourceOrigins);
                    break;
            }
        }
    }

    private static void CollectLiftedBindings(
        string ownerComponentFullName,
        ImmutableArray<RazorVueCanonicalSlotBinding> slots,
        BindingCollection bindings,
        int templateScopeDepth,
        string pathPrefix)
    {
        for (var index = 0; index < slots.Length; index++)
        {
            var slot = slots[index];
            var effectiveScopeDepth = string.IsNullOrWhiteSpace(slot.ParameterName)
                ? templateScopeDepth
                : templateScopeDepth + 1;

            if (slot.ValueKind != RazorVueCanonicalSlotValueKind.ValueExpression ||
                slot.ValueExpressionText is null)
            {
                CollectLiftedBindings(ownerComponentFullName, slot.Children, bindings, effectiveScopeDepth, pathPrefix + "/slot[" + index + "]");
                continue;
            }

            if (!ShouldCreateBindingSite(slot.TemplateEncodability, slot.SideEffectClassification, effectiveScopeDepth))
            {
                CollectLiftedBindings(ownerComponentFullName, slot.Children, bindings, effectiveScopeDepth, pathPrefix + "/slot[" + index + "]");
                continue;
            }

            bindings.Add(
                ownerComponentFullName,
                pathPrefix + "/slot[" + index + "]",
                slot.ValueExpressionText,
                slot.BindingKind,
                slot.TemplateExpressionSafety,
                slot.SideEffectClassification,
                slot.SourceOrigins);
            CollectLiftedBindings(ownerComponentFullName, slot.Children, bindings, effectiveScopeDepth, pathPrefix + "/slot[" + index + "]");
        }
    }

    private static bool ShouldCreateBindingSite(
        RazorVueTemplateEncodability templateEncodability,
        RazorVueSideEffectClassification sideEffectClassification,
        int templateScopeDepth)
        => templateScopeDepth == 0 &&
           (templateEncodability == RazorVueTemplateEncodability.TemplateViaSetupBinding ||
            sideEffectClassification != RazorVueSideEffectClassification.None);

    private static bool CanUseDirectTemplateExpression(
        RazorVueExpressionBindingKind bindingKind,
        RazorVueTemplateExpressionSafety templateExpressionSafety,
        RazorVueSideEffectClassification sideEffectClassification)
    {
        if (bindingKind == RazorVueExpressionBindingKind.LocalReference)
            return false;

        return templateExpressionSafety == RazorVueTemplateExpressionSafety.DirectTemplateSafe &&
               sideEffectClassification == RazorVueSideEffectClassification.None;
    }

    private static void EnsureLiftableBindingKind(
        string ownerComponentFullName,
        RazorVueExpressionBindingKind bindingKind,
        ImmutableArray<RazorVueSourceOrigin> origins,
        string expressionText)
    {
        if (bindingKind != RazorVueExpressionBindingKind.LocalReference)
            return;

        throw CreateUnsupportedTemplateEncodingException(
            ownerComponentFullName,
            origins,
            $"RazorVue SFC template lifting cannot hoist component-local expression '{expressionText}' into <script setup> safely.");
    }

    private static IEnumerable<RazorVueCanonicalTemplateNode> EnumerateNodes(RazorVueCanonicalTemplateFragment fragment)
    {
        foreach (var child in fragment.Children)
        {
            yield return child;

            switch (child)
            {
                case RazorVueCanonicalElementNode element:
                    foreach (var nested in EnumerateNodes(element.Children))
                        yield return nested;
                    break;
                case RazorVueCanonicalComponentNode component:
                    foreach (var slot in component.Slots)
                    {
                        foreach (var nested in EnumerateNodes(slot.Children))
                            yield return nested;
                    }
                    foreach (var nested in EnumerateNodes(component.Children))
                        yield return nested;
                    break;
                case RazorVueCanonicalConditionalNode conditional:
                    foreach (var nested in EnumerateNodes(conditional.WhenTrue))
                        yield return nested;
                    foreach (var nested in EnumerateNodes(conditional.WhenFalse))
                        yield return nested;
                    break;
                case RazorVueCanonicalForEachNode loop:
                    foreach (var nested in EnumerateNodes(loop.Body))
                        yield return nested;
                    break;
                case RazorVueCanonicalForNode loop:
                    foreach (var nested in EnumerateNodes(loop.Body))
                        yield return nested;
                    break;
            }
        }
    }

    private static string CreateLiftedBindingName(int index)
        => "__jazor$" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static string ChangeExtensionToVue(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new InvalidOperationException("RazorVue SFC relative path cannot be empty.");

        if (relativePath.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
            return relativePath.Substring(0, relativePath.Length - 4) + ".vue";
        if (relativePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            return relativePath.Substring(0, relativePath.Length - 3) + ".vue";

        return relativePath + ".vue";
    }

    private static string NormalizeSfcImportSpecifier(
        string importSpecifier,
        VueComponentSourceKind sourceKind,
        string ownerRelativeSfcPath)
    {
        if (string.IsNullOrWhiteSpace(importSpecifier))
            return importSpecifier;

        if (sourceKind == VueComponentSourceKind.LibraryComponent)
        {
            return importSpecifier.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)
                ? importSpecifier.Substring(0, importSpecifier.Length - 4) + ".vue"
                : importSpecifier.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    ? importSpecifier.Substring(0, importSpecifier.Length - 3) + ".vue"
                    : importSpecifier;
        }

        var normalizedTarget = ChangeExtensionToVue(NormalizeOutputRelativeModulePath(importSpecifier));
        return MakeRelativeImportPath(ownerRelativeSfcPath, normalizedTarget);
    }

    private static string MakeRelativeImportPath(string ownerRelativeSfcPath, string targetRootRelativePath)
    {
        var ownerDirectory = GetDirectoryPath(ownerRelativeSfcPath);
        var ownerSegments = SplitPathSegments(ownerDirectory);
        var targetSegments = SplitPathSegments(targetRootRelativePath);

        var sharedPrefixLength = 0;
        while (sharedPrefixLength < ownerSegments.Length &&
               sharedPrefixLength < targetSegments.Length &&
               string.Equals(ownerSegments[sharedPrefixLength], targetSegments[sharedPrefixLength], StringComparison.Ordinal))
        {
            sharedPrefixLength++;
        }

        var relativeSegments = Enumerable.Repeat("..", ownerSegments.Length - sharedPrefixLength)
            .Concat(targetSegments.Skip(sharedPrefixLength))
            .ToArray();
        var relativePath = string.Join("/", relativeSegments);
        return relativePath.StartsWith(".", StringComparison.Ordinal)
            ? relativePath
            : "./" + relativePath;
    }

    private static string GetDirectoryPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        var normalized = relativePath.Replace('\\', '/');
        var separatorIndex = normalized.LastIndexOf('/');
        return separatorIndex < 0
            ? string.Empty
            : normalized.Substring(0, separatorIndex);
    }

    private static string[] SplitPathSegments(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [];

        return path
            .Split('/')
            .Where(static segment => !string.IsNullOrEmpty(segment))
            .ToArray();
    }

    private static string NormalizeOutputRelativeModulePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        return normalized;
    }

    private static string CreateTemplateTagName(
        RazorVueCanonicalComponentNode component,
        VueComponentDescriptor descriptor)
        => descriptor.SourceKind == VueComponentSourceKind.LibraryComponent
            ? descriptor.ExportName
            : component.ComponentName + "Component";

    private static string CreateLocalBindingName(
        RazorVueCanonicalComponentNode component,
        VueComponentDescriptor descriptor)
        => descriptor.SourceKind == VueComponentSourceKind.LibraryComponent
            ? descriptor.ExportName
            : component.ComponentName + "Component";

    private static RazorVueCompilationIssueException CreateUnsupportedTemplateEncodingException(
        string ownerComponentFullName,
        ImmutableArray<RazorVueSourceOrigin> origins,
        string message)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.UnsupportedTemplateEncoding,
            RazorVueIssueSeverity.Error,
            message,
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(issue, ownerComponentFullName, origins.IsDefaultOrEmpty ? null : origins[0]);
    }

    private sealed class BindingCollection
    {
        public ImmutableArray<RazorVueSfcSetupBinding>.Builder Bindings { get; } = ImmutableArray.CreateBuilder<RazorVueSfcSetupBinding>();
        public ImmutableArray<RazorVueSfcTemplateBindingSite>.Builder Sites { get; } = ImmutableArray.CreateBuilder<RazorVueSfcTemplateBindingSite>();

        public void Add(
            string ownerComponentFullName,
            string sitePath,
            string expressionText,
            RazorVueExpressionBindingKind bindingKind,
            RazorVueTemplateExpressionSafety templateExpressionSafety,
            RazorVueSideEffectClassification sideEffectClassification,
            ImmutableArray<RazorVueSourceOrigin> sourceOrigins)
        {
            if (CanUseDirectTemplateExpression(bindingKind, templateExpressionSafety, sideEffectClassification))
            {
                Sites.Add(new RazorVueSfcTemplateBindingSite(sitePath, expressionText));
                return;
            }

            EnsureLiftableBindingKind(ownerComponentFullName, bindingKind, sourceOrigins, expressionText);
            var bindingName = CreateLiftedBindingName(Bindings.Count);
            Bindings.Add(new RazorVueSfcSetupBinding(
                Name: bindingName,
                ExpressionText: expressionText,
                BindingKind: RazorVueSfcSetupBindingKind.Computed,
                TemplateExpressionText: bindingName,
                SourceOrigins: sourceOrigins));
            Sites.Add(new RazorVueSfcTemplateBindingSite(sitePath, bindingName));
        }
    }
}
