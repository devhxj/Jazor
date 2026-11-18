const assert = require('node:assert/strict');
const vscode = require('vscode');

async function run() {
    const config = vscode.workspace.getConfiguration('jolt');
    const extension = vscode.extensions.getExtension('jazor.jolt');
    const server = process.env.JOLT_TEST_SERVER ?? require('node:path').resolve(extension.extensionPath,
        '../../src/Jolt.LanguageServer/bin/Debug/net11.0/Jolt.LanguageServer.dll');
    const previous = config.inspect('serverPath').globalValue;
    if (!process.env.JOLT_TEST_INSTALLED)
        await config.update('serverPath', server, vscode.ConfigurationTarget.Global);
    try {
        const project = vscode.workspace.workspaceFolders[0].uri;
        const document = await vscode.workspace.openTextDocument(vscode.Uri.joinPath(project, 'Counter.jazor'));
        await vscode.window.showTextDocument(document);
        await extension.activate();
        assert.equal(document.languageId, 'jazor');
        const result = await vscode.commands.executeCommand('vscode.executeCompletionItemProvider', document.uri, new vscode.Position(0, 8));
        assert.ok(result.items.some(item => (item.label.label ?? item.label) === 'Count'));
        const position = new vscode.Position(0, 6);
        const references = await vscode.commands.executeCommand('vscode.executeReferenceProvider', document.uri, position);
        assert.ok(references.some(reference => reference.range.start.line === 1));
        const rename = await vscode.commands.executeCommand('vscode.executeDocumentRenameProvider', document.uri, position, 'Total');
        assert.equal(rename.get(document.uri).length, 2);
        assert.ok(await vscode.workspace.applyEdit(rename));
        assert.match(document.getText(), /@Total/);
        await vscode.commands.executeCommand('undo');
        assert.match(document.getText(), /@Count/);
        const formatting = await vscode.commands.executeCommand('vscode.executeFormatDocumentProvider', document.uri, { tabSize: 4, insertSpaces: true });
        assert.ok(Array.isArray(formatting));
        await verifyComponent(project);
        console.log('Verified Jolt completion, references, rename, undo and formatting in VS Code.');
    } finally {
        if (!process.env.JOLT_TEST_INSTALLED)
            await config.update('serverPath', previous, vscode.ConfigurationTarget.Global);
    }
}

async function verifyComponent(project) {
    const scope = 'Widgets' + process.pid;
    const directory = vscode.Uri.joinPath(project, scope);
    const child = vscode.Uri.joinPath(directory, 'Panel.jazor');
    const caller = vscode.Uri.joinPath(project, 'Page' + process.pid + '.jazor');
    const write = (uri, text) => vscode.workspace.fs.writeFile(uri, Buffer.from(text));
    await vscode.workspace.fs.createDirectory(directory);
    await write(child, '<p>@Value<slot /></p>\n@code { [Parameter, EditorRequired] public int Value { get; set; } }');
    await write(caller, '<Panel Value="1">Text</Panel>');
    const page = await vscode.workspace.openTextDocument(caller);
    await vscode.window.showTextDocument(page);
    try {
        for (let attempt = 0; attempt < 50 && !vscode.languages.getDiagnostics(caller).some(item => item.code === 'JOLT1001'); attempt++)
            await new Promise(resolve => setTimeout(resolve, 100));
        const position = new vscode.Position(0, 2);
        const actions = await vscode.commands.executeCommand('vscode.executeCodeActionProvider',
            caller, new vscode.Range(position, position), 'quickfix');
        const action = actions.find(item => item.title === 'Add @using App.' + scope);
        assert.ok(action?.edit, JSON.stringify({ actions, diagnostics: vscode.languages.getDiagnostics(caller) }));
        assert.ok(await vscode.workspace.applyEdit(action.edit));
        assert.match(page.getText(), new RegExp('^@using App\\.' + scope));
        const rename = await vscode.commands.executeCommand('vscode.executeDocumentRenameProvider',
            caller, new vscode.Position(2, 3), 'Tile');
        assert.ok(await vscode.workspace.applyEdit(rename));
        await vscode.workspace.fs.stat(vscode.Uri.joinPath(directory, 'Tile.jazor'));
        assert.match(page.getText(), /<Tile Value="1">Text<\/Tile>/);
    } finally {
        await page.save();
        await vscode.workspace.fs.delete(caller);
        await vscode.workspace.fs.delete(directory, { recursive: true });
    }
}

module.exports = { run };
