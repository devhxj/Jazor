# Jazor VueHost VS Code Extension (MVP)

This extension provides a minimal process-level integration for `Jazor.VueHost`.

## Features

- `Jazor VueHost: Start LSP`
- `Jazor VueHost: Stop LSP`
- `Jazor VueHost: Restart LSP`
- Output channel logging (`Jazor VueHost`)

## Configuration

- `jazorVueHost.executable` (default: `Jazor.VueHost`)
- `jazorVueHost.arguments` (default: `["--lsp", "--stdio"]`)
- `jazorVueHost.autoStart` (default: `true`)

The extension appends `--dev-root=<workspaceRoot>` automatically when missing.

## Scope

This is an ecosystem bootstrap only. It does not yet include a full VS Code Language Client transport layer.
