const vscode = require('vscode');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const { LanguageClient } = require('vscode-languageclient/node');

let client;

async function activate(context) {
    const server = vscode.workspace.getConfiguration('jolt').get('serverPath');
    const tools = path.join(process.env.DOTNET_CLI_HOME || os.homedir(), '.dotnet', 'tools');
    const installed = process.platform === 'win32'
        ? { command: process.env.ComSpec || 'cmd.exe', args: ['/d', '/c', 'jolt.cmd', 'lsp'],
            ...(fs.existsSync(path.join(tools, 'jolt.cmd')) ? { options: { cwd: tools } } : {}) }
        : { command: 'jolt', args: ['lsp'] };
    const command = !server ? installed
        : server.endsWith('.dll') ? { command: 'dotnet', args: [server] } : { command: server, args: ['lsp'] };
    const watcher = vscode.workspace.createFileSystemWatcher('**/{*.jazor,package.json,pnpm-lock.yaml,.npmrc}');
    const declarations = vscode.workspace.createFileSystemWatcher('**/node_modules/**/*.{d.ts,d.mts,d.cts}');
    context.subscriptions.push(watcher, declarations);
    client = new LanguageClient('jolt', 'Jolt', command, {
        documentSelector: [{ scheme: 'file', language: 'jazor' }],
        synchronize: { fileEvents: [watcher, declarations] },
    });
    await client.start();
}

async function deactivate() {
    await client?.stop();
}

module.exports = { activate, deactivate };
