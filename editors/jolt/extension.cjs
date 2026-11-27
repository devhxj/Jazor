const vscode = require('vscode');
const path = require('node:path');
const { LanguageClient } = require('vscode-languageclient/node');
const { serverCommand, projectDirectory } = require('./server-command.cjs');

const clients = new Map();
const settings = '{*.csproj,*.props,*.targets,project.inputs,project.assets.json,global.json}';

function pathKey(file) {
    return process.platform === 'win32' ? path.normalize(file).toLowerCase() : path.normalize(file);
}

function inside(directory, file) {
    const relative = path.relative(directory, file);
    return relative !== '..' && !relative.startsWith('..' + path.sep) && !path.isAbsolute(relative);
}

async function activate(context) {
    function openSource(source) {
        if (source.scheme !== 'file') return;
        const folder = vscode.workspace.getWorkspaceFolder(source);
        const directory = projectDirectory(path.dirname(source.fsPath), folder?.uri.fsPath);
        if (!clients.has(directory)) {
            const start = startClient(context, directory, source);
            clients.set(directory, start);
            start.catch(() => clients.delete(directory));
        }
        return clients.get(directory);
    }
    function openDocument(document) {
        if (document.languageId === 'jazor') return openSource(document.uri);
    }
    context.subscriptions.push(vscode.workspace.onDidOpenTextDocument(openDocument));
    const starts = vscode.workspace.textDocuments.map(openDocument);
    const prestarts = (vscode.workspace.workspaceFolders ?? []).filter(folder => folder.uri.scheme === 'file').map(async folder => {
        // Warm one project per folder. A document in any other project starts its own client on open.
        const [source] = await vscode.workspace.findFiles(new vscode.RelativePattern(folder, '**/*.jazor'),
            '**/{node_modules,bin,obj,.jazor,.git}/**', 1);
        if (source) await openSource(source);
    });
    await Promise.allSettled([...starts, ...prestarts]);
}

async function startClient(context, directory, source) {
    const uri = vscode.Uri.file(directory);
    const command = await serverCommand(directory, vscode.workspace.getConfiguration('jolt', source).get('serverPath'));
    const watcher = vscode.workspace.createFileSystemWatcher(new vscode.RelativePattern(uri,
        '**/{*.jazor,*.cs,*.vue,*.ts,*.tsx,*.mts,*.cts,*.js,*.jsx,*.mjs,tsconfig*.json,package.json,pnpm-lock.yaml,.npmrc}'));
    const declarations = vscode.workspace.createFileSystemWatcher(new vscode.RelativePattern(uri, '**/node_modules/**/*.{d.ts,d.mts,d.cts}'));
    const configuration = vscode.workspace.createFileSystemWatcher(new vscode.RelativePattern(uri, '**/' + settings));
    context.subscriptions.push(watcher, declarations, configuration);
    const client = new LanguageClient('jolt', 'Jolt', command, {
        workspaceFolder: { uri, name: path.basename(directory), index: 0 },
        documentSelector: [{ scheme: 'file', language: 'jazor', pattern: new vscode.RelativePattern(uri, '**/*.jazor') }],
        initializationOptions: { projectSources: true },
        synchronize: { fileEvents: [watcher, declarations, configuration] },
    });

    // Forward buffers without registering Jolt's authoring features for other languages.
    const linked = new Map();
    const forwarded = new Set();
    const sharedBuffer = document => {
        if (document.uri.scheme !== 'file' || !/\.(cs|vue|ts|tsx|mts|cts|js|jsx|mjs)$/i.test(document.fileName)) return false;
        if (linked.has(pathKey(document.fileName))) return true;
        return inside(directory, document.fileName) &&
            pathKey(projectDirectory(path.dirname(document.fileName), directory)) === pathKey(directory);
    };
    const identifier = document => ({ uri: document.uri.toString(), version: document.version });
    const open = document => {
        const key = document.uri.toString();
        if (!forwarded.has(key) && sharedBuffer(document)) {
            forwarded.add(key);
            return client.sendNotification('textDocument/didOpen', {
                textDocument: { ...identifier(document), languageId: document.languageId, text: document.getText() },
            });
        }
    };
    const close = document => {
        if (forwarded.delete(document.uri.toString()))
            return client.sendNotification('textDocument/didClose', { textDocument: identifier(document) });
    };
    const changed = (file, type) => client.sendNotification('workspace/didChangeWatchedFiles', {
        changes: [{ uri: file.toString(), type }],
    });
    const watch = (pattern, matches) => {
        const watcher = vscode.workspace.createFileSystemWatcher(pattern);
        watcher.onDidCreate(file => matches(file) && changed(file, 1));
        watcher.onDidChange(file => matches(file) && changed(file, 2));
        watcher.onDidDelete(file => matches(file) && changed(file, 3));
        return watcher;
    };
    client.onNotification('jolt/projectSources', async ({ project, sources }) => {
        if (pathKey(vscode.Uri.parse(project).fsPath) !== pathKey(directory)) return;
        const selected = new Map(sources.map(source => {
            const file = vscode.Uri.parse(source);
            return [pathKey(file.fsPath), file];
        }));
        for (const [key, watcher] of linked) {
            if (!selected.has(key)) {
                watcher?.dispose();
                linked.delete(key);
            }
        }
        for (const [key, file] of selected) {
            if (!linked.has(key)) {
                // The project watcher already sees linked paths that live inside this directory.
                const watcher = inside(directory, file.fsPath) ? null :
                    watch(new vscode.RelativePattern(path.dirname(file.fsPath), '*.cs'), candidate => pathKey(candidate.fsPath) === key);
                linked.set(key, watcher);
                if (watcher) context.subscriptions.push(watcher);
            }
        }
        await Promise.all(vscode.workspace.textDocuments.map(document => sharedBuffer(document) ? open(document) : close(document)));
    });
    await client.start();
    for (let parent = path.dirname(directory), child = directory; parent !== child; child = parent, parent = path.dirname(parent)) {
        const pattern = new vscode.RelativePattern(parent, '{Directory.Build.props,Directory.Build.targets,Directory.Packages.props,global.json}');
        context.subscriptions.push(watch(pattern, file => pathKey(path.dirname(file.fsPath)) === pathKey(parent)));
    }
    context.subscriptions.push(
        vscode.workspace.onDidOpenTextDocument(open),
        vscode.workspace.onDidChangeTextDocument(({ document, contentChanges }) => {
            if (forwarded.has(document.uri.toString()) && contentChanges.length > 0)
                return client.sendNotification('textDocument/didChange', {
                    textDocument: identifier(document), contentChanges: [{ text: document.getText() }],
                });
        }),
        vscode.workspace.onDidCloseTextDocument(close),
    );
    await Promise.all(vscode.workspace.textDocuments.map(open));
    return client;
}

async function deactivate() {
    const results = await Promise.allSettled(clients.values());
    await Promise.all(results.map(result => result.status === 'fulfilled' && result.value.stop()));
}

module.exports = { activate, deactivate };
