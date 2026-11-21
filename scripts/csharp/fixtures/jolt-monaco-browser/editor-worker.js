import { initialize } from 'monaco-editor/editor/editor.worker.js';

initialize(context => ({
    readModel() {
        return context.getMirrorModels().map(model => model.getValue()).join('\n');
    }
}));
