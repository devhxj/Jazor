// WikiCatalogGuard.cs - 路由目录验证守卫 / Route catalog validation guard
// 启动前验证所有页面元数据数组的长度一致性、路径唯一性和数据完整性
// Validates page metadata array length consistency, path uniqueness, and data integrity before startup

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Wiki;

internal static class WikiCatalogGuard
{
    // 主验证入口：检查所有元数据数组 / Main validation entry: check all metadata arrays
    internal static void ValidateOrThrow()
    {
        EnsureCatalogLength(nameof(WikiHomeModule.PageGroups), WikiHomeModule.PageGroups.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageTitles), WikiHomeModule.PageTitles.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSummaries), WikiHomeModule.PageSummaries.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageStatuses), WikiHomeModule.PageStatuses.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageOwners), WikiHomeModule.PageOwners.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageAudiences), WikiHomeModule.PageAudiences.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSourceFiles), WikiHomeModule.PageSourceFiles.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageLastUpdatedDates), WikiHomeModule.PageLastUpdatedDates.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageReadingMinutes), WikiHomeModule.PageReadingMinutes.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSearchBodies), WikiHomeModule.PageSearchBodies.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageTagSets), WikiHomeModule.PageTagSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageBlockSets), WikiHomeModule.PageBlockSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSectionIdSets), WikiHomeModule.PageSectionIdSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSectionTitleSets), WikiHomeModule.PageSectionTitleSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageRelatedPathSets), WikiHomeModule.PageRelatedPathSets.Length);

        var knownPaths = new HashSet<string>(StringComparer.Ordinal);
        // 逐页验证元数据完整性 / Per-page metadata integrity validation
        for (var pageIndex = 0; pageIndex < WikiHomeModule.PagePaths.Length; pageIndex++)
        {
            var path = WikiHomeModule.PagePaths[pageIndex];
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Wiki route catalog contains an empty page path at index " + pageIndex + ".");

            if (!path.StartsWith("/", StringComparison.Ordinal))
                throw new InvalidOperationException("Wiki route catalog path must start with '/': " + path);

            if (!knownPaths.Add(path))
                throw new InvalidOperationException("Wiki route catalog contains a duplicate page path: " + path);

            EnsureCatalogValue(nameof(WikiHomeModule.PageGroups), WikiHomeModule.PageGroups[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageTitles), WikiHomeModule.PageTitles[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageSummaries), WikiHomeModule.PageSummaries[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageStatuses), WikiHomeModule.PageStatuses[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageOwners), WikiHomeModule.PageOwners[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageAudiences), WikiHomeModule.PageAudiences[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageSourceFiles), WikiHomeModule.PageSourceFiles[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageLastUpdatedDates), WikiHomeModule.PageLastUpdatedDates[pageIndex], pageIndex);
            EnsureCatalogValue(nameof(WikiHomeModule.PageSearchBodies), WikiHomeModule.PageSearchBodies[pageIndex], pageIndex);

            if (!DateTime.TryParseExact(
                    WikiHomeModule.PageLastUpdatedDates[pageIndex],
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains an invalid yyyy-MM-dd date in " +
                    nameof(WikiHomeModule.PageLastUpdatedDates) + " for path " + path + ".");
            }
            if (WikiHomeModule.PageReadingMinutes[pageIndex] <= 0)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains a non-positive reading-time value for path " + path + ".");
            }

            // docs 内容页必须有正文块；搜索工具页为空数组 / Docs pages require content blocks; search is empty
            var blocks = WikiHomeModule.PageBlockSets[pageIndex];
            if (blocks == null)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains a null page block set at index " + pageIndex +
                    " for path " + path + ".");
            }

            if (path != "/search" && blocks.Length == 0)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains an empty docs page body for path " + path + ".");
            }

            var sectionIds = WikiHomeModule.PageSectionIdSets[pageIndex];
            var sectionTitles = WikiHomeModule.PageSectionTitleSets[pageIndex];
            if (sectionIds.Length != sectionTitles.Length)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog section metadata mismatch for path " + path +
                    ": ids=" + sectionIds.Length + ", titles=" + sectionTitles.Length + ".");
            }

            if (sectionIds.Length == 0)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains no section metadata for path " + path + ".");
            }

            var tags = WikiHomeModule.PageTagSets[pageIndex];
            if (tags == null || tags.Length == 0)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains an empty tag set for path " + path + ".");
            }

            var knownTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var tagIndex = 0; tagIndex < tags.Length; tagIndex++)
            {
                var tag = tags[tagIndex];
                if (string.IsNullOrWhiteSpace(tag))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains an empty tag at page index " + pageIndex +
                        ", tag index " + tagIndex + ".");
                }

                if (!knownTags.Add(tag))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains a duplicate tag '" + tag +
                        "' for path " + path + ".");
                }
            }

            var knownSectionIds = new HashSet<string>(StringComparer.Ordinal);
            for (var sectionIndex = 0; sectionIndex < sectionIds.Length; sectionIndex++)
            {
                var sectionId = sectionIds[sectionIndex];
                var sectionTitle = sectionTitles[sectionIndex];
                if (string.IsNullOrWhiteSpace(sectionId))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains an empty section id at page index " + pageIndex +
                        ", section index " + sectionIndex + ".");
                }

                if (string.IsNullOrWhiteSpace(sectionTitle))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains an empty section title at page index " + pageIndex +
                        ", section index " + sectionIndex + ".");
                }

                if (!knownSectionIds.Add(sectionId))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains a duplicate section id '" + sectionId +
                        "' for path " + path + ".");
                }
            }
        }

        // 验证相关页面引用的有效性 / Validate related page references
        for (var pageIndex = 0; pageIndex < WikiHomeModule.PageRelatedPathSets.Length; pageIndex++)
        {
            var currentPath = WikiHomeModule.PagePaths[pageIndex];
            var relatedPaths = WikiHomeModule.PageRelatedPathSets[pageIndex];
            var knownRelatedPaths = new HashSet<string>(StringComparer.Ordinal);
            for (var relatedIndex = 0; relatedIndex < relatedPaths.Length; relatedIndex++)
            {
                var relatedPath = relatedPaths[relatedIndex];
                if (!knownPaths.Contains(relatedPath))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains an unknown related path '" + relatedPath +
                        "' for page " + currentPath + ".");
                }

                if (relatedPath == currentPath)
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog related paths cannot point to the current page: " + currentPath);
                }

                if (!knownRelatedPaths.Add(relatedPath))
                {
                    throw new InvalidOperationException(
                        "Wiki route catalog contains a duplicate related path '" + relatedPath +
                        "' for page " + currentPath + ".");
                }
            }
        }

        // 分组级数组彼此等长、非空且指向已注册路由 / Group arrays align, non-empty, point to known routes
        if (WikiHomeModule.NavGroupIds.Length == 0)
            throw new InvalidOperationException("Wiki route catalog has no navigation groups.");
        if (WikiHomeModule.NavGroupLabels.Length != WikiHomeModule.NavGroupIds.Length ||
            WikiHomeModule.NavGroupLandingPaths.Length != WikiHomeModule.NavGroupIds.Length)
        {
            throw new InvalidOperationException(
                "Wiki route catalog navigation group arrays are misaligned: ids=" + WikiHomeModule.NavGroupIds.Length +
                ", labels=" + WikiHomeModule.NavGroupLabels.Length +
                ", landingPaths=" + WikiHomeModule.NavGroupLandingPaths.Length + ".");
        }

        var knownGroups = new HashSet<string>(StringComparer.Ordinal);
        var knownLandingPaths = new HashSet<string>(StringComparer.Ordinal);
        for (var groupIndex = 0; groupIndex < WikiHomeModule.NavGroupIds.Length; groupIndex++)
        {
            var groupId = WikiHomeModule.NavGroupIds[groupIndex];
            if (string.IsNullOrWhiteSpace(groupId) || !knownGroups.Add(groupId))
                throw new InvalidOperationException("Wiki route catalog has an invalid or duplicate navigation group: " + groupId);

            if (string.IsNullOrWhiteSpace(WikiHomeModule.NavGroupLabels[groupIndex]))
                throw new InvalidOperationException("Wiki route catalog has an empty group label for group " + groupId);

            var landingPath = WikiHomeModule.NavGroupLandingPaths[groupIndex];
            if (!knownPaths.Contains(landingPath) || !knownLandingPaths.Add(landingPath))
                throw new InvalidOperationException("Wiki route catalog has an unknown or duplicate group landing path '" + landingPath + "' for group " + groupId + ".");
        }
    }

    // 检查数组长度与 PagePaths 一致 / Check array length matches PagePaths
    private static void EnsureCatalogLength(string catalogName, int catalogLength)
    {
        if (catalogLength != WikiHomeModule.PagePaths.Length)
        {
            throw new InvalidOperationException(
                "Wiki route catalog length mismatch for " + catalogName +
                ": expected " + WikiHomeModule.PagePaths.Length + ", actual " + catalogLength + ".");
        }
    }

    // 检查字符串值非空 / Check string value is non-empty
    private static void EnsureCatalogValue(string catalogName, string value, int pageIndex)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return;

        throw new InvalidOperationException(
            "Wiki route catalog contains an empty value in " + catalogName +
            " at page index " + pageIndex + ".");
    }
}
