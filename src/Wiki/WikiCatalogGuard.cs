using System;
using System.Collections.Generic;
using System.Globalization;

namespace Wiki;

internal static class WikiCatalogGuard
{
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
        EnsureCatalogLength(nameof(WikiHomeModule.PageBodies), WikiHomeModule.PageBodies.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSectionIdSets), WikiHomeModule.PageSectionIdSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSectionTitleSets), WikiHomeModule.PageSectionTitleSets.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageRelatedPathSets), WikiHomeModule.PageRelatedPathSets.Length);

        var knownPaths = new HashSet<string>(StringComparer.Ordinal);
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

            if (WikiHomeModule.PageBodies[pageIndex] == null)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog contains a null page body delegate at index " + pageIndex +
                    " for path " + path + ".");
            }

            var sectionIds = WikiHomeModule.PageSectionIdSets[pageIndex];
            var sectionTitles = WikiHomeModule.PageSectionTitleSets[pageIndex];
            if (sectionIds.Length != sectionTitles.Length)
            {
                throw new InvalidOperationException(
                    "Wiki route catalog section metadata mismatch for path " + path +
                    ": ids=" + sectionIds.Length + ", titles=" + sectionTitles.Length + ".");
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
    }

    private static void EnsureCatalogLength(string catalogName, int catalogLength)
    {
        if (catalogLength != WikiHomeModule.PagePaths.Length)
        {
            throw new InvalidOperationException(
                "Wiki route catalog length mismatch for " + catalogName +
                ": expected " + WikiHomeModule.PagePaths.Length + ", actual " + catalogLength + ".");
        }
    }

    private static void EnsureCatalogValue(string catalogName, string value, int pageIndex)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return;

        throw new InvalidOperationException(
            "Wiki route catalog contains an empty value in " + catalogName +
            " at page index " + pageIndex + ".");
    }
}
