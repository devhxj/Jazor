using System;
using System.Collections.Generic;

namespace Wiki;

internal static class WikiCatalogGuard
{
    internal static void ValidateOrThrow()
    {
        EnsureCatalogLength(nameof(WikiHomeModule.PageGroups), WikiHomeModule.PageGroups.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageTitles), WikiHomeModule.PageTitles.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageSummaries), WikiHomeModule.PageSummaries.Length);
        EnsureCatalogLength(nameof(WikiHomeModule.PageStatuses), WikiHomeModule.PageStatuses.Length);
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
