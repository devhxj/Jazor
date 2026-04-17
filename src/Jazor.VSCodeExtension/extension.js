"use strict";

const vscode = require("vscode");
const childProcess = require("child_process");

let hostProcess = null;
let outputChannel = null;

function activate(context) {
    outputChannel = vscode.window.createOutputChannel("Jazor VueHost");
    context.subscriptions.push(outputChannel);

    context.subscriptions.push(
        vscode.commands.registerCommand("jazorVueHost.start", () => startHost()),
        vscode.commands.registerCommand("jazorVueHost.stop", () => stopHost()),
        vscode.commands.registerCommand("jazorVueHost.restart", async () => {
            stopHost();
            await startHost();
        })
    );

    const autoStart = vscode.workspace.getConfiguration("jazorVueHost").get("autoStart", true);
    if (autoStart) {
        void startHost();
    }
}

async function startHost() {
    if (hostProcess) {
        outputChannel.appendLine("[info] Jazor.VueHost is already running.");
        return;
    }

    const config = vscode.workspace.getConfiguration("jazorVueHost");
    const executable = config.get("executable", "Jazor.VueHost");
    const configuredArguments = config.get("arguments", ["--lsp", "--stdio"]);
    const args = Array.isArray(configuredArguments) ? [...configuredArguments] : ["--lsp", "--stdio"];
    const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath ?? process.cwd();
    if (!args.some((arg) => typeof arg === "string" && arg.startsWith("--dev-root="))) {
        args.push(`--dev-root=${workspaceRoot}`);
    }

    outputChannel.appendLine(`[start] ${executable} ${args.join(" ")}`);
    outputChannel.show(true);

    try {
        hostProcess = childProcess.spawn(executable, args, {
            cwd: workspaceRoot,
            windowsHide: true,
            stdio: ["pipe", "pipe", "pipe"]
        });
    } catch (error) {
        hostProcess = null;
        outputChannel.appendLine(`[error] failed to start process: ${formatError(error)}`);
        void vscode.window.showErrorMessage("Jazor VueHost failed to start. See output channel for details.");
        return;
    }

    hostProcess.stdout?.on("data", (chunk) => {
        outputChannel.append(chunk.toString());
    });
    hostProcess.stderr?.on("data", (chunk) => {
        outputChannel.append(chunk.toString());
    });

    hostProcess.on("error", (error) => {
        outputChannel.appendLine(`[error] process error: ${formatError(error)}`);
        hostProcess = null;
    });

    hostProcess.on("exit", (code, signal) => {
        outputChannel.appendLine(`[exit] code=${code ?? "null"} signal=${signal ?? "null"}`);
        hostProcess = null;
    });
}

function stopHost() {
    if (!hostProcess) {
        outputChannel.appendLine("[info] Jazor.VueHost is not running.");
        return;
    }

    outputChannel.appendLine("[stop] terminating Jazor.VueHost process.");
    hostProcess.kill();
    hostProcess = null;
}

function deactivate() {
    stopHost();
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
