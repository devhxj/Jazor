// Literal imports keep Debug and Release on the same dependency graph. Loading the
// contribution before resolving ensures the first model already has C# support.
export async function loadEditor() {
    const [{ editor }] = await Promise.all([
        import("monaco-editor/editor/editor.api.js"),
        import("monaco-editor/languages/definitions/csharp/register.js")
    ]);
    return editor;
}

// Keep Monaco's identity-sensitive instances outside the reactive page state.
let session

export function create(api, containerId, value, changed) {
    if (session) return true
    const container = document.getElementById(containerId)
    if (!container) return false

    const editor = api.create(container, {
        value,
        language: 'csharp',
        theme: 'vs-dark',
        fontSize: 14,
        minimap: { enabled: true },
        wordWrap: 'off',
        lineNumbers: 'on',
        scrollBeyondLastLine: false,
        automaticLayout: true,
        tabSize: 4,
        insertSpaces: true,
    })
    // The editor owns the initial model created from value and language.
    const model = editor.getModel()
    const listener = model.onDidChangeContent(() => changed(model.getValue()))
    session = { editor, model, listener }
    return true
}

export function getValue(fallback) {
    return session?.model.getValue() ?? fallback
}

export function setValue(value) {
    const model = session?.model
    if (model && model.getValue() !== value) model.setValue(value)
}

export function layoutAndFocus() {
    session?.editor.layout()
    session?.editor.focus()
}

export function dispose() {
    const current = session
    session = undefined
    if (!current) return
    current.listener.dispose()
    current.editor.setModel(null)
    current.editor.dispose()
    if (!current.model.isDisposed()) current.model.dispose()
}

export function createMonacoActions() {
    return { loadEditor, create, getValue, setValue, layoutAndFocus, dispose }
}
