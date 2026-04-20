"use strict";

const vscode = require("vscode");
const { LanguageClient } = require("vscode-languageclient/node");

let languageClient = null;
let outputChannel = null;
let dashboardPanel = null;

function activate(context) {
    outputChannel = vscode.window.createOutputChannel("Jolt");
    context.subscriptions.push(outputChannel);

    context.subscriptions.push(
        vscode.commands.registerCommand("jolt.start", () => startHost(context)),
        vscode.commands.registerCommand("jolt.stop", () => stopHost()),
        vscode.commands.registerCommand("jolt.restart", async () => {
            await stopHost();
            await startHost(context);
        }),
        vscode.commands.registerCommand("jolt.showExtensionDashboard", () => showExtensionDashboard(context))
    );

    const autoStart = vscode.workspace.getConfiguration("jolt").get("autoStart", true);
    if (autoStart) {
        void startHost(context);
    }
}

async function startHost(context) {
    if (languageClient) {
        outputChannel.appendLine("[info] Jolt is already running.");
        return;
    }

    const launch = resolveLaunchConfig();
    outputChannel.appendLine(`[start] ${launch.executable} ${launch.args.join(" ")}`);
    outputChannel.show(true);

    const serverOptions = {
        run: {
            command: launch.executable,
            args: launch.args,
            options: {
                cwd: launch.workspaceRoot,
                windowsHide: true
            }
        },
        debug: {
            command: launch.executable,
            args: launch.args,
            options: {
                cwd: launch.workspaceRoot,
                windowsHide: true
            }
        }
    };
    const clientOptions = {
        documentSelector: [
            { scheme: "file", language: "jazor" },
            { scheme: "file", language: "vue" }
        ],
        outputChannel,
        synchronize: {
            configurationSection: "jolt"
        }
    };

    const client = new LanguageClient(
        "jolt",
        "Jolt",
        serverOptions,
        clientOptions
    );
    context.subscriptions.push(client);

    try {
        await client.start();
        languageClient = client;
        outputChannel.appendLine("[ready] Jolt language client started.");
    } catch (error) {
        outputChannel.appendLine(`[error] failed to start language client: ${formatError(error)}`);
        void vscode.window.showErrorMessage("Jolt failed to start. See output channel for details.");
    }
}

async function stopHost() {
    if (!languageClient) {
        outputChannel.appendLine("[info] Jolt is not running.");
        return;
    }

    const client = languageClient;
    languageClient = null;
    outputChannel.appendLine("[stop] stopping Jolt language client.");
    try {
        await client.stop();
    } catch (error) {
        outputChannel.appendLine(`[warn] failed to stop language client cleanly: ${formatError(error)}`);
    }
}

async function showExtensionDashboard(context) {
    if (!languageClient) {
        await startHost(context);
    }

    if (!languageClient) {
        return;
    }

    try {
        const dashboard = await languageClient.sendRequest("jazor/extensionObservabilityDashboard");
        renderDashboard(dashboard);
    } catch (error) {
        outputChannel.appendLine(`[error] dashboard request failed: ${formatError(error)}`);
        void vscode.window.showErrorMessage("Failed to fetch Jolt extension dashboard.");
    }
}

function resolveLaunchConfig() {
    const config = vscode.workspace.getConfiguration("jolt");
    const executable = config.get("executable", "Jolt");
    const configuredArguments = config.get("arguments", ["--lsp", "--stdio"]);
    const args = Array.isArray(configuredArguments)
        ? configuredArguments
            .filter((item) => typeof item === "string")
            .map((item) => item.trim())
            .filter((item) => item.length > 0)
        : ["--lsp", "--stdio"];
    const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath ?? process.cwd();
    if (!args.some((arg) => arg.startsWith("--dev-root="))) {
        args.push(`--dev-root=${workspaceRoot}`);
    }

    return {
        executable,
        args,
        workspaceRoot
    };
}

function renderDashboard(dashboard) {
    const generatedAt = dashboard?.generatedAt ?? "unknown";
    const loadHealth = Array.isArray(dashboard?.loadHealth) ? dashboard.loadHealth : [];
    const providerHealth = Array.isArray(dashboard?.providerHealth) ? dashboard.providerHealth : [];
    const recentEvents = Array.isArray(dashboard?.recentLoadEvents) ? dashboard.recentLoadEvents : [];

    if (!dashboardPanel) {
        dashboardPanel = vscode.window.createWebviewPanel(
            "joltExtensionDashboard",
            "Jazor Extension Dashboard",
            vscode.ViewColumn.Beside,
            {
                enableFindWidget: true
            }
        );
        dashboardPanel.onDidDispose(() => {
            dashboardPanel = null;
        });
    }

    dashboardPanel.title = `Jazor Extension Dashboard (${loadHealth.length}/${providerHealth.length})`;
    dashboardPanel.webview.html = createDashboardHtml({
        generatedAt,
        loadHealth,
        providerHealth,
        recentEvents
    });
    dashboardPanel.reveal(vscode.ViewColumn.Beside, true);

    outputChannel.appendLine(
        `[dashboard] generatedAt=${generatedAt}, loadHealth=${loadHealth.length}, providerHealth=${providerHealth.length}, recentLoadEvents=${recentEvents.length}`
    );
    outputChannel.show(true);
}

function createDashboardHtml(model) {
    const rows = model.loadHealth
        .map((item) => `
            <tr>
                <td>${escapeHtml(item.source ?? "-")}</td>
                <td>${escapeHtml(item.extensionId ?? "-")}</td>
                <td>${Number(item.loadedCount ?? 0)}</td>
                <td>${Number(item.rejectedCount ?? 0)}</td>
                <td>${Number(item.failedCount ?? 0)}</td>
                <td>${escapeHtml(item.lastReason ?? "-")}</td>
            </tr>`)
        .join("");

    const providerRows = model.providerHealth
        .map((item) => `
            <tr>
                <td>${escapeHtml(item.capability ?? "-")}</td>
                <td>${escapeHtml(item.providerName ?? "-")}</td>
                <td>${Number(item.successCount ?? 0)}</td>
                <td>${Number(item.failureCount ?? 0)}</td>
                <td>${Number(item.timeoutCount ?? 0)}</td>
                <td>${Number(item.skippedCount ?? 0)}</td>
                <td>${escapeHtml(item.lastErrorMessage ?? "-")}</td>
            </tr>`)
        .join("");

    const recentRows = model.recentEvents
        .map((item) => `
            <tr>
                <td>${escapeHtml(item.timestamp ?? "-")}</td>
                <td>${escapeHtml(item.source ?? "-")}</td>
                <td>${escapeHtml(item.extensionId ?? "-")}</td>
                <td>${escapeHtml(item.status ?? "-")}</td>
                <td>${escapeHtml(item.reason ?? "-")}</td>
            </tr>`)
        .join("");

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Jazor Extension Dashboard</title>
  <style>
    body { font-family: var(--vscode-font-family); color: var(--vscode-foreground); background: var(--vscode-editor-background); margin: 16px; }
    h2 { margin: 20px 0 8px; font-size: 14px; }
    .meta { margin-bottom: 12px; opacity: 0.8; }
    table { width: 100%; border-collapse: collapse; font-size: 12px; }
    th, td { border: 1px solid var(--vscode-editorWidget-border); padding: 6px 8px; text-align: left; vertical-align: top; }
    th { background: var(--vscode-editorWidget-background); }
    .empty { opacity: 0.65; padding: 8px 0; }
  </style>
</head>
<body>
  <div class="meta">Generated at: ${escapeHtml(model.generatedAt)}</div>
  <h2>Extension Load Health</h2>
  ${model.loadHealth.length > 0
        ? `<table><thead><tr><th>Source</th><th>Extension</th><th>Loaded</th><th>Rejected</th><th>Failed</th><th>Last Reason</th></tr></thead><tbody>${rows}</tbody></table>`
        : '<div class="empty">No load health events.</div>'}

  <h2>Provider Health</h2>
  ${model.providerHealth.length > 0
        ? `<table><thead><tr><th>Capability</th><th>Provider</th><th>Success</th><th>Failure</th><th>Timeout</th><th>Skipped</th><th>Last Error</th></tr></thead><tbody>${providerRows}</tbody></table>`
        : '<div class="empty">No provider health events.</div>'}

  <h2>Recent Load Events</h2>
  ${model.recentEvents.length > 0
        ? `<table><thead><tr><th>Timestamp</th><th>Source</th><th>Extension</th><th>Status</th><th>Reason</th></tr></thead><tbody>${recentRows}</tbody></table>`
        : '<div class="empty">No recent extension load events.</div>'}
</body>
</html>`;
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function deactivate() {
    return stopHost();
}

function formatError(error) {
    if (!error) {
        return "unknown error";
    }

    if (error instanceof Error) {
        return error.message;
    }

    return String(error);
}

module.exports = {
    activate,
    deactivate
};
