using System.Reflection;
using ECMAScript.ElementPlus;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class ElementPlusLiteralDomainContractTests
{
    [TestMethod]
    public void ElementPlus_ComponentLiteralDomains_UseSharedAndNamedContracts()
    {
        var expectations = new (Type DeclaringType, string PropertyName, Type ExpectedType)[]
        {
            (typeof(ElAffix), nameof(ElAffix.Position), typeof(ElementPlusTopBottomPlacement?)),
            (typeof(ElAvatar), nameof(ElAvatar.Shape), typeof(ElementPlusAvatarShape?)),
            (typeof(ElAvatarGroup), nameof(ElAvatarGroup.Shape), typeof(ElementPlusAvatarShape?)),
            (typeof(ElAvatarGroup), nameof(ElAvatarGroup.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElAvatarGroup), nameof(ElAvatarGroup.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElButton), nameof(ElButton.Type), typeof(ElementPlusButtonType?)),
            (typeof(ElButton), nameof(ElButton.NativeType), typeof(ElementPlusButtonNativeType?)),
            (typeof(ElButtonGroup), nameof(ElButtonGroup.Type), typeof(ElementPlusButtonType?)),
            (typeof(ElButtonGroup), nameof(ElButtonGroup.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElCalendar), nameof(ElCalendar.ControllerType), typeof(ElementPlusCalendarControllerType?)),
            (typeof(ElCarousel), nameof(ElCarousel.Type), typeof(ElementPlusCarouselType?)),
            (typeof(ElCarousel), nameof(ElCarousel.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElCollapse), nameof(ElCollapse.ExpandIconPosition), typeof(ElementPlusCollapseIconPosition?)),
            (typeof(ElContainer), nameof(ElContainer.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElDescriptions), nameof(ElDescriptions.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElDivider), nameof(ElDivider.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElDivider), nameof(ElDivider.ContentPosition), typeof(ElementPlusContentPosition?)),
            (typeof(ElDropdown), nameof(ElDropdown.Type), typeof(ElementPlusButtonType?)),
            (typeof(ElDropdown), nameof(ElDropdown.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElDropdown), nameof(ElDropdown.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElFormItem), nameof(ElFormItem.ValidateStatus), typeof(ElementPlusFormItemValidateStatus?)),
            (typeof(ElPopover), nameof(ElPopover.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElPopover), nameof(ElPopover.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElProgress), nameof(ElProgress.Type), typeof(ElementPlusProgressType?)),
            (typeof(ElProgress), nameof(ElProgress.Status), typeof(ElementPlusProgressStatus?)),
            (typeof(ElSegmented), nameof(ElSegmented.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElSelect), nameof(ElSelect.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElSelect), nameof(ElSelect.TagType), typeof(ElementPlusTagType?)),
            (typeof(ElSelect), nameof(ElSelect.TagEffect), typeof(ElementPlusTagEffect?)),
            (typeof(ElSelect), nameof(ElSelect.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElSpace), nameof(ElSpace.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElStep), nameof(ElStep.Status), typeof(ElementPlusStepStatus?)),
            (typeof(ElSteps), nameof(ElSteps.Direction), typeof(ElementPlusDirection?)),
            (typeof(ElSteps), nameof(ElSteps.FinishStatus), typeof(ElementPlusStepStatus?)),
            (typeof(ElSteps), nameof(ElSteps.ProcessStatus), typeof(ElementPlusStepStatus?)),
            (typeof(ElTabs), nameof(ElTabs.Type), typeof(ElementPlusTabsType?)),
            (typeof(ElTabs), nameof(ElTabs.TabPosition), typeof(ElementPlusPopperPlacementSide?)),
            (typeof(ElTag), nameof(ElTag.Type), typeof(ElementPlusTagType?)),
            (typeof(ElTag), nameof(ElTag.Effect), typeof(ElementPlusTagEffect?)),
            (typeof(ElText), nameof(ElText.Type), typeof(ElementPlusSemanticType?)),
            (typeof(ElTimeline), nameof(ElTimeline.Mode), typeof(ElementPlusTimelineMode?)),
            (typeof(ElTimelineItem), nameof(ElTimelineItem.Placement), typeof(ElementPlusTopBottomPlacement?)),
            (typeof(ElTimelineItem), nameof(ElTimelineItem.Type), typeof(ElementPlusSemanticType?)),
            (typeof(ElTooltip), nameof(ElTooltip.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.TagType), typeof(ElementPlusTagType?)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.TagEffect), typeof(ElementPlusTagEffect?)),
            (typeof(ElTreeSelect), nameof(ElTreeSelect.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.Effect), typeof(ElementPlusPopperEffect?)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.TagType), typeof(ElementPlusTagType?)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.TagEffect), typeof(ElementPlusTagEffect?)),
            (typeof(ElVirtualizedSelect), nameof(ElVirtualizedSelect.Placement), typeof(ElementPlusPopperPlacement?))
        };

        foreach (var (declaringType, propertyName, expectedType) in expectations)
        {
            AssertPropertyType(declaringType, propertyName, expectedType);
        }
    }

    [TestMethod]
    public void ElementPlus_SharedLiteralContracts_AreReusedByValueTypes()
    {
        var expectations = new (Type DeclaringType, string PropertyName, Type ExpectedType)[]
        {
            (typeof(ElementPlusButtonConfig), nameof(ElementPlusButtonConfig.Type), typeof(ElementPlusButtonType?)),
            (typeof(ElementPlusLinkConfig), nameof(ElementPlusLinkConfig.Type), typeof(ElementPlusLinkType?)),
            (typeof(ElementPlusTagTooltipProps), nameof(ElementPlusTagTooltipProps.Placement), typeof(ElementPlusPopperPlacement?)),
            (typeof(ElementPlusTagTooltipProps), nameof(ElementPlusTagTooltipProps.FallbackPlacements), typeof(ElementPlusPopperPlacement[])),
            (typeof(ElementPlusButtonProps), nameof(ElementPlusButtonProps.Type), typeof(ElementPlusButtonType?)),
            (typeof(ElementPlusButtonProps), nameof(ElementPlusButtonProps.NativeType), typeof(ElementPlusButtonNativeType?))
        };

        foreach (var (declaringType, propertyName, expectedType) in expectations)
        {
            AssertPropertyType(declaringType, propertyName, expectedType);
        }
    }

    private static void AssertPropertyType(Type declaringType, string propertyName, Type expectedType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"{declaringType.FullName}.{propertyName}");
        Assert.AreEqual(expectedType, property!.PropertyType, $"{declaringType.FullName}.{propertyName}");
    }
}
