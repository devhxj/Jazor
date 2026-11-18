const vscode = require('vscode');
const { LanguageClient } = require('vscode-languageclient/node');

let client;

async function activate(context) {
    const server = vscode.workspace.getConfiguration('jolt').get('serverPath');
    const installed = process.platform === 'win32'
        ? { command: process.env.ComSpec || 'cmd.exe', args: ['/d', '/c', 'jolt.cmd', 'lsp'] }
        : { command: 'jolt', args: ['lsp'] };
    const command = !server ? installed
        : server.endsWith('.dll') ? { command: 'dotnet', args: [server] } : { command: server };
    const watcher = vscode.workspace.createFileSystemWatcher('**/{*.jazor,package.json,pnpm-lock.yaml}');
    context.subscriptions.push(watcher);
    client = new LanguageClient('jolt', 'Jolt', command, {
        documentSelector: [{ scheme: 'file', language: 'jazor' }],
        synchronize: { fileEvents: watcher },
    });
    await client.start();
}

async function deactivate() {
    await client?.stop();
}

module.exports = { activate, deactivate };
