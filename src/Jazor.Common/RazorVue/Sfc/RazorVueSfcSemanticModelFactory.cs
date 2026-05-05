using System.Collections.Immutable;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;

namespace Jazor.RazorVue.Sfc;

internal sealed class RazorVueSfcSemanticModelFactory
{
    public RazorVueSfcSemanticModel Create(RazorVueCanonicalHComponentModel canonicalModel)
    {
        if (canonicalModel is null)
            throw new ArgumentNullException(nameof(canonicalModel));

        ValidateTemplateEncodability(canonicalModel);

        var bindings = CollectLiftedBindings(canonicalModel.ComponentFullName, canonicalModel.Template);
        return new RazorVueSfcSemanticModel(
            ComponentName: canonicalModel.ComponentName,
            ComponentFullName: canonicalModel.ComponentFullName,
            RelativeSfcPath: ChangeExtensionToVue(canonicalModel.RelativeComponentPath),
            Descriptor: canonicalModel.Descriptor,
            Imports: CollectImports(canonicalModel),
            ComponentImports: CollectComponentImports(canonicalModel),
            Styles: canonicalModel.Styles,
            PluginRequirements: canonicalModel.PluginRequirements,
            Hints: canonicalModel.Hints,
            SourceOrigins: canonicalModel.SourceOrigins,
            TemplateBlock: new RazorVueSfcTemplateBlockModel(
                canonicalModel.Template,
                bindings.Sites.ToImmutable(),
                canonicalModel.Template.Children.SelectMany(static child => child.SourceOrigins).ToImmutableArray()),
            ScriptSetupBlock: new RazorVueSfcScriptSetupBlockModel(canonicalModel.Setup, bindings.Bindings.ToImmutable(), canonicalModel.SourceOrigins),
            StyleBlocks: canonicalModel.Styles
                .Select(static style => new RazorVueSfcStyleBlockModel(
                    Text: string.Empty,
                    IsScoped: false,
                    ModuleName: null,
                    Language: null,
                    SourceFilePath: style,
                    SourceOrigins: ImmutableArray<RazorVueSourceOrigin>.Empty))
                .ToImmutableArray(),
            CustomBlocks: ImmutableArray<RazorVueSfcCustomBlockModel>.Empty);
    }

    private static ImmutableArray<string> CollectImports(RazorVueCanonicalHComponentModel canonicalModel)
    {
        var builder = ImmutableArray.CreateBuilder<string>();
        builder.Add("vue");

        foreach (var import in canonicalModel.Imports)
        {
            if (string.Equals(import, "vue", StringComparison.Ordinal))
                continue;

            builder.Add(NormalizeSfcImportSpecifier(import));
        }

        return builder
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
    }

    private static ImmutableArray<RazorVueSfcComponentImport> CollectComponentImports(RazorVueCanonicalHComponentModel canonicalModel)
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
                ImportSpecifier: NormalizeSfcImportSpecifier(descriptor.ImportSpecifier),
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
                when ShouldLiftExpression(interpolation.TemplateEncodability, templateScopeDepth):
                EnsureLiftableBindingKind(ownerComponentFullName, interpolation.BindingKind, interpolation.SourceOrigins, interpolation.ExpressionText);
                bindings.Add(path, interpolation.ExpressionText, interpolation.SourceOrigins);
                return;

            case RazorVueCanonicalConditionalNode conditional:
                if (ShouldLiftExpression(conditional.TemplateEncodability, templateScopeDepth))
                {
                    EnsureLiftableBindingKind(ownerComponentFullName, conditional.BindingKind, conditional.SourceOrigins, conditional.ConditionExpressionText);
                    bindings.Add(path + "/if", conditional.ConditionExpressionText, conditional.SourceOrigins);
                }
                CollectLiftedBindings(ownerComponentFullName, conditional.WhenTrue, bindings, templateScopeDepth, path + "/whenTrue");
                CollectLiftedBindings(ownerComponentFullName, conditional.WhenFalse, bindings, templateScopeDepth, path + "/whenFalse");
                return;

            case RazorVueCanonicalForEachNode loop:
                if (ShouldLiftExpression(loop.TemplateEncodability, templateScopeDepth))
                {
                    EnsureLiftableBindingKind(ownerComponentFullName, loop.BindingKind, loop.SourceOrigins, loop.SourceExpressionText);
                    bindings.Add(path + "/forEach", loop.SourceExpressionText, loop.SourceOrigins);
                }
                CollectLiftedBindings(ownerComponentFullName, loop.Body, bindings, templateScopeDepth + 1, path + "/body");
                return;

            case RazorVueCanonicalSlotOutletNode slotOutlet when
                slotOutlet.ArgumentExpressionText is not null &&
                ShouldLiftExpression(slotOutlet.TemplateEncodability, templateScopeDepth):
                EnsureLiftableBindingKind(ownerComponentFullName, slotOutlet.BindingKind, slotOutlet.SourceOrigins, slotOutlet.ArgumentExpressionText);
                bindings.Add(path + "/slotArg", slotOutlet.ArgumentExpressionText, slotOutlet.SourceOrigins);
                return;

            default:
                return;
        }
    }

    private static void CollectLiftedBindings(
        string ownerComponentFullName,
        ImmutableArray<RazorVueCanonicalAttributeBinding> attributes,
        BindingCollection bindings,
        int templateScopeDepth,
        string pathPrefix)
    {
        for (var index = 0; index < attributes.Length; index++)
        {
            var attribute = attributes[index];
            if (attribute.ExpressionText is null ||
                !ShouldLiftExpression(attribute.TemplateEncodability, templateScopeDepth))
            {
                continue;
            }

            EnsureLiftableBindingKind(ownerComponentFullName, attribute.BindingKind, attribute.SourceOrigins, attribute.ExpressionText);
            bindings.Add(pathPrefix + "/attr[" + index + "]", attribute.ExpressionText, attribute.SourceOrigins);
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
            if (slot.ValueKind != RazorVueCanonicalSlotValueKind.ValueExpression ||
                slot.ValueExpressionText is null)
                continue;

            var effectiveScopeDepth = string.IsNullOrWhiteSpace(slot.ParameterName)
                ? templateScopeDepth
                : templateScopeDepth + 1;
            if (!ShouldLiftExpression(slot.TemplateEncodability, effectiveScopeDepth))
                continue;

            EnsureLiftableBindingKind(ownerComponentFullName, slot.BindingKind, slot.SourceOrigins, slot.ValueExpressionText);
            bindings.Add(pathPrefix + "/slot[" + index + "]", slot.ValueExpressionText, slot.SourceOrigins);
        }
    }

    private static bool ShouldLiftExpression(
        RazorVueTemplateEncodability templateEncodability,
        int templateScopeDepth)
        => templateEncodability == RazorVueTemplateEncodability.TemplateViaSetupBinding &&
           templateScopeDepth == 0;

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
            }
        }
    }

    private static string CreateLiftedBindingName(int index)
        => "__jazorVueSfcBinding" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

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

    private static string NormalizeSfcImportSpecifier(string importSpecifier)
    {
        if (string.IsNullOrWhiteSpace(importSpecifier))
            return importSpecifier;

        return importSpecifier.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)
            ? importSpecifier.Substring(0, importSpecifier.Length - 4) + ".vue"
            : importSpecifier.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                ? importSpecifier.Substring(0, importSpecifier.Length - 3) + ".vue"
                : importSpecifier;
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
            string sitePath,
            string expressionText,
            ImmutableArray<RazorVueSourceOrigin> sourceOrigins)
        {
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
