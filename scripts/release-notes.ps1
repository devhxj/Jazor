param(
    [Parameter(Mandatory = $true)]
    [string]$Tag,
    [string]$PreviousTag = "",
    [string]$Repository = ""
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = & git @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }

    return $output
}

function Resolve-PreviousTag {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CurrentTag
    )

    $tags = Invoke-Git -Arguments @("tag", "--sort=-version:refname")
    return $tags |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and $_ -ne $CurrentTag } |
        Select-Object -First 1
}

function Parse-CommitSubject {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Subject
    )

    $trimmed = $Subject.Trim()
    $match = [regex]::Match(
        $trimmed,
        '^[^\p{L}\p{Nd}]*(?<type>feat|fix|refactor|test|docs|chore)(\((?<scope>[^)]+)\))?:\s*(?<desc>.+)$',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    if ($match.Success) {
        return [pscustomobject]@{
            Type = $match.Groups["type"].Value.ToLowerInvariant()
            Scope = $match.Groups["scope"].Value.Trim()
            Description = $match.Groups["desc"].Value.Trim()
            Subject = $trimmed
        }
    }

    return [pscustomobject]@{
        Type = "other"
        Scope = ""
        Description = $trimmed
        Subject = $trimmed
    }
}

function Get-SectionTitle {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Type
    )

    switch ($Type) {
        "feat" { return "Features" }
        "fix" { return "Fixes" }
        "refactor" { return "Refactors" }
        "test" { return "Tests" }
        "docs" { return "Documentation" }
        "chore" { return "Chores" }
        default { return "Other" }
    }
}

if ([string]::IsNullOrWhiteSpace($PreviousTag)) {
    $PreviousTag = Resolve-PreviousTag -CurrentTag $Tag
}

$range = if ([string]::IsNullOrWhiteSpace($PreviousTag)) { $Tag } else { "$PreviousTag..$Tag" }
$subjects = Invoke-Git -Arguments @("log", "--reverse", "--format=%s", $range)
$parsedCommits = @($subjects | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object { Parse-CommitSubject -Subject $_ })

$scopes = $parsedCommits |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_.Scope) } |
    ForEach-Object { $_.Scope.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries) } |
    ForEach-Object { $_.Trim() } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$topScopes = @($scopes |
    Group-Object |
    Sort-Object -Property Count, Name -Descending |
    Select-Object -First 6 |
    ForEach-Object { '`' + $_.Name + '`' })

$lineBuilder = [System.Collections.Generic.List[string]]::new()
$lineBuilder.Add("## Summary")
$lineBuilder.Add("")

if ([string]::IsNullOrWhiteSpace($PreviousTag)) {
    $lineBuilder.Add("- $($parsedCommits.Count) commits included in this release.")
} else {
    $lineBuilder.Add(("- {0} commits since `{1}`." -f $parsedCommits.Count, $PreviousTag))
}

if ($topScopes.Count -gt 0) {
    $lineBuilder.Add("- Primary scopes: $($topScopes -join ', ').")
}

if (-not [string]::IsNullOrWhiteSpace($Repository) -and -not [string]::IsNullOrWhiteSpace($PreviousTag)) {
    $lineBuilder.Add("- Compare: https://github.com/$Repository/compare/$PreviousTag...$Tag")
}

$sectionOrder = @("feat", "fix", "refactor", "test", "docs", "chore", "other")
foreach ($sectionType in $sectionOrder) {
    $entries = @($parsedCommits | Where-Object { $_.Type -eq $sectionType })
    if ($entries.Count -eq 0) {
        continue
    }

    $lineBuilder.Add("")
    $lineBuilder.Add("## $(Get-SectionTitle -Type $sectionType)")
    $lineBuilder.Add("")

    foreach ($entry in $entries) {
        if ([string]::IsNullOrWhiteSpace($entry.Scope)) {
            $lineBuilder.Add("- $($entry.Description)")
            continue
        }

        $lineBuilder.Add("- **$($entry.Scope)**: $($entry.Description)")
    }
}

if (-not [string]::IsNullOrWhiteSpace($Repository) -and -not [string]::IsNullOrWhiteSpace($PreviousTag)) {
    $lineBuilder.Add("")
    $lineBuilder.Add("## Full Changelog")
    $lineBuilder.Add("")
    $lineBuilder.Add("https://github.com/$Repository/compare/$PreviousTag...$Tag")
}

[string]::Join([Environment]::NewLine, $lineBuilder)
