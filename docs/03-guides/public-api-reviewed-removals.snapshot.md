# Reviewed API removals: Jazor 1.1.0

The NodeApi toolchain refactor retains C# compiler and component authoring semantics.
Vite/SSR now invoke APIs directly, so JazorViteServerOptions and JazorSsrOptions no
longer expose TaskName. Remove these settings when upgrading. The maintainer has
selected 1.1.0 for this infrastructure migration; other breaking changes continue
to follow the MAJOR rule. See CHANGELOG.md and nodejs-pnpm-migration-plan.md.

The comparison tool deduplicates signatures across types. These three entries
cover the property and its accessors on both options types. The 1.0.0 snapshot
remains unchanged; all removed signatures stay visible in generated reports.

  - System.String TaskName
  - System.String get_TaskName()
  - Void set_TaskName(System.String)

## Reviewed API removals for Jazor 2.7.0

The maintainer approved removing the following two attributes in this MINOR
release. See the 2.7.0 migration instructions in CHANGELOG.md. The frozen 1.0.0
snapshot predates these types. The allowlist records their exact type entries.
The constructors and `Index` signatures also occur on other public types.
The comparison tool deduplicates those signatures across types.

- type ECMAScript.ECMAScriptUndefinedArgumentAttribute
- type ECMAScript.ECMAScriptUndefinedDefaultAttribute
