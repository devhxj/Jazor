import { editor, MarkerSeverity } from 'monaco-editor/editor/editor.api.js';
import { MountEditor, ObserveContent, ReadMarker, UpdateEditor } from './App.jazor';

const host = document.getElementById('editor');
const transport = new Worker(new URL('./editor-worker.js', import.meta.url), { type: 'module' });
const terminate = transport.terminate.bind(transport);
// Monaco terminates the browser worker after its transport promise settles.
const terminated = new Promise(resolve => {
    transport.terminate = () => {
        terminate();
        resolve();
    };
});
let dispose;
const workerResult = new Promise((resolve, reject) => {
    transport.addEventListener('error', reject, { once: true });
    dispose = MountEditor(host, transport, resolve);
});
const [instance] = editor.getEditors();
const [model] = editor.getModels();
if (!instance || !model || instance.getModel() !== model || instance.getValue() !== 'one line')
    throw new Error('Expected the generated editor to mount the generated model');
const originalMarkers = editor.getModelMarkers({ resource: model.uri });
if (originalMarkers.length !== 2)
    throw new Error('Expected both owners before clearing');
if (originalMarkers.some(marker => marker.message !== 'typed warning' ||
    marker.severity !== MarkerSeverity.Warning || marker.endColumn !== 2))
    throw new Error('Expected the generated calls to write the warning and range');
if (ReadMarker() !== 'keep-me|typed warning|warning')
    throw new Error('Expected the generated call to read the typed marker');
UpdateEditor();
const markers = editor.getModelMarkers({ resource: model.uri });
if (markers.length !== 1 || markers[0].owner !== 'keep-me')
    throw new Error('Expected only the other owner to remain');
await new Promise(requestAnimationFrame);
const background = host.querySelector('.monaco-editor-background');
if (!background || getComputedStyle(background).backgroundColor !== 'rgb(18, 52, 86)')
    throw new Error('Expected the generated theme to change the Monaco background');
if (await workerResult !== 'one line')
    throw new Error('Expected the Monaco worker to read the synced model');
const versions = [];
const unsubscribe = ObserveContent(version => versions.push(version));
model.setValue('second version');
if (versions.length !== 1 || versions[0] !== model.getVersionId())
    throw new Error('Expected the generated subscription to receive the typed version');
unsubscribe();
model.setValue('third version');
if (versions.length !== 1)
    throw new Error('Expected the generated disposal to unsubscribe');
dispose();
if (host.querySelector('.monaco-editor') || editor.getEditors().includes(instance) || !model.isDisposed())
    throw new Error('Expected the generated cleanup to dispose the editor and model');
await terminated;
document.body.dataset.monacoVerified = 'true';
