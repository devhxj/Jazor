const assert = require('node:assert/strict');
const vscode = require('vscode');

async function run() {
    const config = vscode.workspace.getConfiguration('jolt');
    const extension = vscode.extensions.getExtension('jazor.jolt');
    const server = process.env.JOLT_TEST_SERVER ?? require('node:path').resolve(extension.extensionPath,
        '../../src/Jolt.LanguageServer/bin/Debug/net11.0/Jolt.LanguageServer.dll');
    const previous = config.inspect('serverPath').globalValue;
    await config.update('serverPath', server, vscode.ConfigurationTarget.Global);
    try {
        const project = vscode.workspace.workspaceFolders[0].uri;
        const document = await vscode.workspace.openTextDocument(vscode.Uri.joinPath(project, 'Counter.jazor'));
        await vscode.window.showTextDocument(document);
        await extension.activate();
        assert.equal(document.languageId, 'jazor');
        const result = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', document.uri, new vscode.Position(0, 8));
        assert.ok(result.items.some(item => (item.label.label ?? item.label) === 'Count'));
    } finally {
        await config.update('serverPath', previous, vscode.ConfigurationTarget.Global);
    }
}

module.exports = { run };
