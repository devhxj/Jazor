# Jolt VS Code Extension

This extension provides a VS Code Language Client integration for `Jolt`.

## Features

- `Jolt: Start LSP`
- `Jolt: Stop LSP`
- `Jolt: Restart LSP`
- `Jolt: Show Extension Dashboard` (`jazor/extensionObservabilityDashboard`, Webview + output summary)
- Output channel logging (`Jolt`)

## Configuration

- `jolt.executable` (default: `Jolt`)
- `jolt.arguments` (default: `["--lsp", "--stdio"]`)
- `jolt.autoStart` (default: `true`)

The extension appends `--dev-root=<workspaceRoot>` automatically when missing.

## Packaging and Publish

```bash
npm install
npm run package
# npm run publish
```
