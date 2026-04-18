# Jazor VueHost VS Code Extension

This extension provides a VS Code Language Client integration for `Jazor.VueHost`.

## Features

- `Jazor VueHost: Start LSP`
- `Jazor VueHost: Stop LSP`
- `Jazor VueHost: Restart LSP`
- `Jazor VueHost: Show Extension Dashboard` (`jazor/extensionObservabilityDashboard`, Webview + output summary)
- Output channel logging (`Jazor VueHost`)

## Configuration

- `jazorVueHost.executable` (default: `Jazor.VueHost`)
- `jazorVueHost.arguments` (default: `["--lsp", "--stdio"]`)
- `jazorVueHost.autoStart` (default: `true`)

The extension appends `--dev-root=<workspaceRoot>` automatically when missing.

## Packaging and Publish

```bash
npm install
npm run package
# npm run publish
```
