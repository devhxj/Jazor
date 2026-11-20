const { createRequire } = require('node:module');
const { dirname, join } = require('node:path');

exports.typeCheck = async function typeCheck(configFile) {
    const fromProject = createRequire(join(dirname(configFile), 'package.json'));
    const tscPath = fromProject.resolve('typescript/lib/tsc');
    const originalArgs = process.argv;
    const originalExit = process.exit;
    const originalWrite = process.stdout.write;
    const finished = Symbol('TypeScript exited');
    let diagnostics = '';
    let exitCode = 0;
    // vue-tsc launches the TypeScript CLI in this realm. Capture its synchronous
    // output and completion without letting tsc terminate the embedded runtime.
    process.argv = [process.execPath, tscPath, '--noEmit', '--pretty', 'false', '-p', configFile];
    process.exit = code => {
        exitCode = code;
        throw finished;
    };
    process.stdout.write = chunk => {
        diagnostics += chunk;
        return true;
    };
    try {
        fromProject('vue-tsc').run(tscPath);
    } catch (error) {
        if (error !== finished)
            throw error;
    } finally {
        process.argv = originalArgs;
        process.exit = originalExit;
        process.stdout.write = originalWrite;
    }
    return JSON.stringify({ exitCode, diagnostics });
};

exports.typeScriptDefinition = async function typeScriptDefinition(configFile, sourceFile, searchText, searchOffset) {
    const root = dirname(configFile);
    const ts = createRequire(join(root, 'package.json'))('typescript');
    const config = ts.readConfigFile(configFile, ts.sys.readFile);
    const project = ts.parseJsonConfigFileContent(config.config, ts.sys, root);
    const source = ts.sys.readFile(sourceFile);
    const occurrence = source.indexOf(searchText);
    if (occurrence < 0)
        throw new Error(`Missing definition probe ${searchText} in ${sourceFile}`);
    const service = ts.createLanguageService({
        ...ts.sys,
        getCompilationSettings: () => project.options,
        getCurrentDirectory: () => root,
        getDefaultLibFileName: options => ts.getDefaultLibFilePath(options),
        getScriptFileNames: () => project.fileNames,
        getScriptSnapshot(fileName) {
            const text = ts.sys.readFile(fileName);
            return text === undefined ? undefined : ts.ScriptSnapshot.fromString(text);
        },
        getScriptVersion: () => '0',
    });
    try {
        const definitions = service.getDefinitionAtPosition(sourceFile, occurrence + Number(searchOffset)) ?? [];
        return JSON.stringify(definitions.map(definition => {
            const location = { fileName: definition.fileName, pos: definition.textSpan.start };
            return service.getSourceMapper().tryGetSourcePosition(location) ?? location;
        }));
    } finally {
        service.dispose();
    }
};
