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
    const watcher = vscode.workspace.createFileSystemWatcher('**/{*.jazor,*.cs,*.vue,*.ts,*.tsx,*.mts,*.cts,*.js,*.jsx,*.mjs,tsconfig*.json,package.json,pnpm-lock.yaml,.npmrc}');
    const declarations = vscode.workspace.createFileSystemWatcher('**/node_modules/**/*.{d.ts,d.mts,d.cts}');
    context.subscriptions.push(watcher, declarations);
    client = new LanguageClient('jolt', 'Jolt', command, {
        documentSelector: [{ scheme: 'file', language: 'jazor' }],
        synchronize: { fileEvents: [watcher, declarations] },
    });
    await client.start();
    // Forward buffers without registering Jolt's authoring features for other languages.
    const sharedBuffer = document => document.uri.scheme === 'file' && /\.(cs|vue|ts|tsx|mts|cts|js|jsx|mjs)$/.test(document.fileName);
    const identifier = document => ({ uri: document.uri.toString(), version: document.version });
    const open = document => {
        if (sharedBuffer(document))
            return client.sendNotification('textDocument/didOpen', {
                textDocument: { ...identifier(document), languageId: document.languageId, text: document.getText() },
            });
    };
    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(open),
        vscode.workspace.onDidChangeTextDocument(({ document, contentChanges }) => {
            if (sharedBuffer(document) && contentChanges.length > 0)
                return client.sendNotification('textDocument/didChange', {
                    textDocument: identifier(document), contentChanges: [{ text: document.getText() }],
                });
        }),
        vscode.workspace.onDidCloseTextDocument(document => {
            if (sharedBuffer(document))
                return client.sendNotification('textDocument/didClose', { textDocument: identifier(document) });
        }),
    );
    await Promise.all(vscode.workspace.textDocuments.map(open));
}

async function deactivate() {
    await client?.stop();
}

module.exports = { activate, deactivate };
